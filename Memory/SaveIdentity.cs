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
	///     每跳一次自动存档就换一个，所以不能当记忆文件名；
	///   - 但自动存档认得出自己出自哪个存档：GameManager.Save:958 把当前会话的
	///     Telemetry sessionGuid 写进元数据，而 GameManager.Load:1196 + GetSessionGuid:1153
	///     在载入时把这同一个 sessionGuid 恢复成本局会话（只有 NewGame/NewMap 才发新的），
	///     所以「手动存档 + 它之后产生的所有自动存档」共用一个 sessionGuid。
	///     于是自动存档的记忆一律写在**原存档名**的文件里（玩家需求：
	///     「我打开的是自动保存的存档，就按原存档的名称覆盖记忆文件」）。
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
			/// <summary>城市名：判不出自动存档出自哪个存档时，用它当记忆文件名。</summary>
			public string cityName;
			/// <summary>target.autoSave。</summary>
			public bool autoSave;
			/// <summary>
			/// target.sessionGuid（"N" 格式，Guid.Empty 时留 null）。
			/// 依据：GameManager.Save:958 写 `meta.sessionGuid = Telemetry.GetCurrentSession()`，
			/// 而 GameManager.Load:1196 + GetSessionGuid:1153 表明**载入存档会把它的 sessionGuid
			/// 恢复成本局会话的 sessionGuid**（只有 NewGame/NewMap 才发新的）。
			/// 所以「同一个城市链」上的手动存档和它产生的自动存档，sessionGuid 相同 ——
			/// 这就是自动存档回溯原存档名的唯一可靠线索。
			/// </summary>
			public string sessionGuid;
		}

		/// <summary>
		/// 资产库里扫到的一条存档（只取判定要用的字段，纯数据，离线可造）。
		/// </summary>
		public struct SaveEntry
		{
			/// <summary>存档列表里显示的名字。</summary>
			public string name;
			public string cityName;
			public bool autoSave;
			/// <summary>sessionGuid 的 "N" 文本；未知为 null。</summary>
			public string sessionGuid;
			/// <summary>最后修改时刻（可比大小即可，用来在同一条链的多个手动存档里挑最新那份）。</summary>
			public long modified;
		}

		/// <summary>索引里「会话链 -&gt; 存档名」条目的键前缀（save 文件的 guid 是纯十六进制，不会撞）。</summary>
		public const string SESSION_PREFIX = "@";

		/// <summary>会话链键。</summary>
		public static string SessionKey(string sessionGuid)
		{
			return string.IsNullOrEmpty(sessionGuid) ? null : SESSION_PREFIX + sessionGuid;
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
		/// siblings 允许为 null（调用方只在自动存档时才扫资产库）。
		/// </summary>
		public static bool TryResolveName(PurposeKind kind, bool haveGuid, string guidText,
			SaveMetaInfo meta, SaveIndex index, SaveEntry[] siblings,
			out string fileName, out bool placeholder)
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

			if (meta.known && meta.autoSave)
			{
				// 自动存档：它的名字是时间戳（每跳一次就换一个），绝不能当记忆文件名。
				// 玩家的要求是「按原存档的名称覆盖记忆文件」，所以先顺着 sessionGuid
				// 找回出身的那份手动存档，找到就直接用它的名字（不是占位名：
				// 之后玩家手动存盘时用的就是同一个名字，文件原地覆盖，不搬家）。
				string parent = FindAutosaveParent(meta, siblings, index);
				if (IsUsableName(parent))
				{
					fileName = parent;
					placeholder = false;
					return true;
				}

				if (IsUsableName(meta.cityName))
				{
					// 认不出出身（原存档被删了、或这城市从来没手动存过盘）：退回城市名，
					// 并记成占位名，等玩家哪天手动存盘时文件会自动改成真正的存档名。
					fileName = meta.cityName;
					placeholder = true;
					return true;
				}
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
		/// 顺着 sessionGuid 找自动存档的「原存档名」。
		/// 判定次序（从严到宽，宁缺毋滥）：
		///   1. 同一条会话链上的手动存档，且城市名对得上 —— 取最后修改的那份；
		///   2. 只对齐会话链（玩家在存盘前改过城市名时仍认得出）；
		///   3. 本模组索引里记过这条会话链的名字（原存档后来被删了也还认得）；
		///   4. 都没有 -&gt; null，由调用方退回城市名占位。
		/// 同一条链上有多个手动存档（一局里「另存为」过两次）时无法绝对确定，
		/// 选最新修改的那份：玩家刚另存出来的那个，之后自动存档多半就是它的。
		/// </summary>
		public static string FindAutosaveParent(SaveMetaInfo meta, SaveEntry[] siblings, SaveIndex index)
		{
			if (string.IsNullOrEmpty(meta.sessionGuid)) return null;

			string best = null;
			long bestModified = long.MinValue;
			string loose = null;
			long looseModified = long.MinValue;

			if (siblings != null)
			{
				for (int i = 0; i < siblings.Length; i++)
				{
					SaveEntry e = siblings[i];
					if (e.autoSave) continue;
					if (!IsUsableName(e.name)) continue;
					if (!string.Equals(e.sessionGuid, meta.sessionGuid, StringComparison.OrdinalIgnoreCase)) continue;

					bool sameCity = !string.IsNullOrEmpty(meta.cityName)
						&& string.Equals(e.cityName, meta.cityName, StringComparison.Ordinal);
					if (e.modified >= looseModified)
					{
						loose = e.name;
						looseModified = e.modified;
					}
					if (sameCity && e.modified >= bestModified)
					{
						best = e.name;
						bestModified = e.modified;
					}
				}
			}

			if (IsUsableName(best)) return best;
			if (IsUsableName(loose)) return loose;

			if (index != null)
			{
				string remembered = index.LookupName(SessionKey(meta.sessionGuid));
				if (IsUsableName(remembered)) return remembered;
			}
			return null;
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
	/// 两类键共存：
	///   &lt;存档文件 guid&gt; -&gt; 名字：资产库偶尔查不到（存档正在改名、云端只读副本）时的备份通道；
	///   <see cref="SaveIdentity.SESSION_PREFIX"/>+&lt;sessionGuid&gt; -&gt; 名字：这条会话链的原存档名，
	///   玩家把那份手动存档删了也还认得它的自动存档。
	/// 索引只增不换会越写越大，所以条目数封顶 kMaxEntries（超了就忽略新键，
	/// 已有的键仍然有效 —— 记忆文件名主要靠实时解析，索引只是兜底）。
	/// </summary>
	public sealed class SaveIndex
	{
		public const char SEP = '|';

		/// <summary>索引条目上限：按一位玩家几十条会话链的量级放足够宽，只为防爆。</summary>
		public const int kMaxEntries = 512;

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
			if (old == null && m_ByGuid.Count >= kMaxEntries) return false;   // 封顶：只丢新键
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
