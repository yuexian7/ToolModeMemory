using System;
using System.Collections.Generic;
using System.Text;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 「这次进的是哪个存档」的判定，纯函数（不碰游戏类型，离线可测）。
	///
	/// 事实依据（反编译 Game.dll）：
	///   - userState.lastSaveGameMetadata 只在 GameManager.Save() 成功分支赋值，载入时不更新；
	///   - 自动存档也走 GameManager.Save(名字 = "dd-MMMM-HH-mm-ss")，同样会覆盖它；
	///   - 载入时 LoadGameSystem.context.instigatorGuid == 被载入存档的 SaveGameMetadata.id。
	/// 所以只有「存档元数据的 id == 本次载入 guid」或「guid 已在索引里」时，存档名才算确证。
	/// </summary>
	public static class SaveIdentity
	{
		public const string AUTO_PREFIX = "_auto_";
		public const string UNSAVED_PREFIX = "_unsaved_";

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
		/// </summary>
		public static bool TryResolveName(PurposeKind kind, bool haveGuid, string guidText,
			string lastMetaIdText, string lastMetaName, bool lastMetaIsAutoSave,
			SaveIndex index, out string fileName, out bool placeholder)
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

			string indexed = index != null ? index.LookupName(guidText) : null;
			if (!string.IsNullOrEmpty(indexed))
			{
				fileName = indexed;
				return true;
			}

			if (!lastMetaIsAutoSave && !string.IsNullOrEmpty(lastMetaName)
				&& string.Equals(lastMetaIdText, guidText, StringComparison.Ordinal))
			{
				fileName = lastMetaName;
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
