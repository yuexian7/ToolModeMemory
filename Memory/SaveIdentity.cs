using System;
using System.Collections.Generic;
using System.Text;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 「这次进的是哪个存档」的判定，纯函数（不碰游戏类型，离线可测）。
	///
	/// 事实依据（反编译 Game.dll）：
	///   - 载入时 LoadGameSystem.context.instigatorGuid == 被载入存档的 SaveGameMetadata.id
	///     （GameManager.Load:1196 把 saveGameMetadata.id 原样交给 Context）；
	///   - AssetDatabase.global.TryGetAsset(guid, out asset) 就是这个 guid 的反查通道，
	///     游戏自己的 GameManager.Load(GameMode,Purpose,Hash128):1219 也用它；
	///   - 查到的元数据里 name == 存档列表显示的名字（FileSystemDataSource.GetName 把文件名
	///     去转义后返回），target.autoSave 标明它是不是自动存档，target.cityName 是城市名；
	///   - 自动存档的名字是 $"{DateTime.Now:dd-MMMM-HH-mm-ss}"（AutoSaveSystem:184），
	///     每跳一次自动存档就换一个，所以不能当记忆文件名。
	///
	/// 0.2.3 及以前只信 userState.lastSaveGameMetadata，而它只在 GameManager.Save() 成功分支
	/// 赋值（GameManager:995），单纯载入时它还是上一个存过盘的东西 → 对不上号 → 只能落进
	/// _auto_&lt;guid&gt; 占位名，于是玩家看到「记忆文件名和存档名完全不一致」。
	/// 现在直接按 guid 反查，名字与游戏内一致。
	/// </summary>
	public static class SaveIdentity
	{
		public const string AUTO_PREFIX = "_auto_";
		public const string UNSAVED_PREFIX = "_unsaved_";

		/// <summary>AssetDatabase 查不到条目时 GetName 的返回值（FileSystemDataSource:724）。</summary>
		public const string TRANSIENT_NAME = "Transient asset";

		/// <summary>
		/// 本次载入存档的元数据（由调用方从 Game.dll 读好后传进来，保持本类可离线测试）。
		/// </summary>
		public struct SaveMetaInfo
		{
			/// <summary>在资产库里查到了这条存档的元数据。</summary>
			public bool known;
			/// <summary>存档列表里显示的名字（存档包名；自动存档是时间戳）。</summary>
			public string name;
			/// <summary>城市名：自动存档用它当记忆文件名，否则每 10 分钟换一份。</summary>
			public string cityName;
			/// <summary>target.autoSave。</summary>
			public bool autoSave;
		}

		/// <summary>名字能不能直接当文件名用（排除空值与资产库的占位返回值）。</summary>
		public static bool IsUsableName(string name)
		{
			if (string.IsNullOrEmpty(name)) return false;
			if (string.Equals(name, TRANSIENT_NAME, StringComparison.Ordinal)) return false;
			return true;
		}

		/// <summary>旧版本（0.2.3 及以前）为认不出的存档生成的占位名，用于一次性搬家。</summary>
		public static string LegacyAutoName(string guidText)
		{
			if (string.IsNullOrEmpty(guidText)) return null;
			return AUTO_PREFIX + guidText;
		}

		/// <summary>本次进入是否需要参与（新建城市要，主菜单/编辑器不要）。</summary>
		public static bool ShouldParticipate(PurposeKind kind)
		{
			return kind == PurposeKind.LoadedSave || kind == PurposeKind.NewCity;
		}

		/// <summary>GameManager.Purpose 里我们关心的三类；其余（编辑器/清理）一律不参与。</summary>
		public enum PurposeKind
		{
			NewCity,
			LoadedSave,
			NonSave
		}

		public static PurposeKind Classify(int purposeValue)
		{
			// Colossal.Serialization.Entities.Purpose:
			// SaveGame=0 NewGame=1 LoadGame=2 SaveMap=3 NewMap=4 LoadMap=5 Cleanup=6
			switch (purposeValue)
			{
				case 1:
				case 4:
				case 3:
					return PurposeKind.NewCity;
				case 0:
				case 2:
				case 5:
					return PurposeKind.LoadedSave;
				default:
					return PurposeKind.NonSave;
			}
		}

		/// <summary>
		/// 决定记忆文件名。返回 false = 无法确证，调用方应使用随机会话占位名。
		/// placeholder=true 表示这名字只是暂时的，之后玩家手动存盘时要把文件搬过去。
		/// </summary>
		public static bool TryResolveName(PurposeKind kind, bool haveGuid, string guidText,
			SaveMetaInfo meta, SaveIndex index, out string fileName, out bool placeholder)
		{
			fileName = null;
			placeholder = false;

			if (kind == PurposeKind.NonSave) return false;

			// 新建城市：此刻磁盘上还没有它的存档文件，绝不复用任何旧名字
			if (kind == PurposeKind.NewCity) return false;

			if (!haveGuid)
			{
				// 拿不到本次载入的 guid 就无法证明身份：宁可用会话占位，也不猜名字
				return false;
			}

			if (meta.known && !meta.autoSave && IsUsableName(meta.name))
			{
				// 确证：这次载入的就是名叫 meta.name 的存档（玩家在游戏里看到的同一个名字）。
				// 优先级高于索引：存档被改名后文件名要跟着改，否则「按名字复制记忆」就失效了。
				fileName = meta.name;
				placeholder = false;
				return true;
			}

			if (meta.known && meta.autoSave && IsUsableName(meta.cityName))
			{
				// 自动存档：它的名字是时间戳（10 分钟换一个），拿城市名当本份记忆的键。
				// 记成占位名，玩家之后手动存盘时文件会自动改成真正的存档名。
				fileName = meta.cityName;
				placeholder = true;
				return true;
			}

			string indexed = index != null ? index.LookupName(guidText) : null;
			if (!string.IsNullOrEmpty(indexed))
			{
				fileName = indexed;
				placeholder = false;
				return true;
			}

			fileName = AUTO_PREFIX + guidText;
			placeholder = true;
			return true;
		}

		/// <summary>
		/// 存档事件到达时是否应当接管/记录名字。自动存档必须忽略：
		/// 它的名字是时间戳，认了就会把记忆搬进 27-September-13-45-02.json。
		/// </summary>
		public static bool ShouldAdoptOnSave(bool saveSucceeded, bool isAutoSave, string saveName)
		{
			if (!saveSucceeded || isAutoSave) return false;
			return !string.IsNullOrEmpty(saveName);
		}
	}

	/// <summary>
	/// guid -&gt; 存档名 索引（纯文本，一行一条，可手工修）。
	/// 有了它，即使自动存档把 lastSaveGameMetadata 顶掉，重启后仍能认出「这是我上次存过的 MyCity」。
	/// </summary>
	public sealed class SaveIndex
	{
		public const char SEP = '|';

		private readonly Dictionary<string, string> m_ByGuid =
			new Dictionary<string, string>(StringComparer.Ordinal);

		public int Count { get { return m_ByGuid.Count; } }

		public string LookupName(string guidText)
		{
			if (string.IsNullOrEmpty(guidText)) return null;
			string v;
			return m_ByGuid.TryGetValue(guidText, out v) ? v : null;
		}

		public string LookupGuid(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			foreach (KeyValuePair<string, string> kv in m_ByGuid)
			{
				if (string.Equals(kv.Value, name, StringComparison.Ordinal)) return kv.Key;
			}
			return null;
		}

		/// <summary>返回是否发生变化。</summary>
		public bool Set(string guidText, string name)
		{
			if (string.IsNullOrEmpty(guidText) || string.IsNullOrEmpty(name)) return false;
			string old;
			if (m_ByGuid.TryGetValue(guidText, out old) && old == name) return false;
			m_ByGuid[guidText] = name;
			return true;
		}

		public bool RemoveGuid(string guidText)
		{
			if (string.IsNullOrEmpty(guidText)) return false;
			return m_ByGuid.Remove(guidText);
		}

		public void Clear()
		{
			m_ByGuid.Clear();
		}

		public string Serialize()
		{
			StringBuilder sb = new StringBuilder(256);
			sb.Append("# Tool Mode Memory guid|save name\n");
			foreach (KeyValuePair<string, string> kv in m_ByGuid)
			{
				sb.Append(kv.Key).Append(SEP).Append(kv.Value).Append('\n');
			}
			return sb.ToString();
		}

		public static SaveIndex Parse(string text)
		{
			SaveIndex idx = new SaveIndex();
			if (string.IsNullOrEmpty(text)) return idx;
			string[] lines = text.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i].Trim('\r', ' ', '\t');
				if (line.Length == 0 || line[0] == '#') continue;
				int at = line.IndexOf(SEP);
				if (at <= 0 || at >= line.Length - 1) continue;
				string guid = line.Substring(0, at).Trim();
				string name = line.Substring(at + 1).Trim();
				if (guid.Length == 0 || name.Length == 0) continue;
				idx.m_ByGuid[guid] = name;
			}
			return idx;
		}
	}
}
