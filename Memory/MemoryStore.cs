using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 按存档持久化的工具记忆。文件名 = 存档名（净化后），每档一份，退出覆盖写。
	/// 格式为可手工编辑的 JSON：{"v":2,"save":"...","items":{"net.draw":{"G:Roads/SmallRoads":2}}}
	///
	/// v0.1.2 关键约束：
	/// - 只有「占位名」（_unsaved_ / _auto_）才允许改名迁移，正式存档名绝不搬文件（否则毁掉上一个存档的记忆）。
	/// - 目标文件存在但读不懂时禁止覆盖，保护用户可手工修复的文件。
	/// - 只有 Dirty 才写盘，避免在主菜单生成空文件。
	/// </summary>
	public sealed class MemoryStore
	{
		public const int kVersion = 2;

		private readonly Dictionary<string, Dictionary<string, int>> m_Items =
			new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);

		private string m_SaveName;
		private string m_SessionFallback;
		private bool m_Placeholder;
		private bool m_Dirty;
		private bool m_WriteBlocked;

		public string SaveName { get { return m_SaveName; } }
		public bool Dirty { get { return m_Dirty; } }

		/// <summary>当前是否使用占位文件名（未命名新档，或存档名无法确证）。</summary>
		public bool IsPlaceholder { get { return m_Placeholder; } }

		/// <summary>目标记忆文件读失败，本轮不再覆盖写盘。</summary>
		public bool WriteBlocked { get { return m_WriteBlocked; } }

		public static string DataDirectory
		{
			get
			{
				// Unity persistentDataPath = LocalLow\Colossal Order\Cities Skylines II
				string root = UnityEngine.Application.persistentDataPath;
				if (string.IsNullOrEmpty(root))
				{
					root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				}
				return Path.Combine(root, "ModsData", "ToolModeMemory");
			}
		}

		/// <summary>guid -&gt; 存档名 索引文件（见 SaveIndex）。</summary>
		public static string IndexPath
		{
			get { return Path.Combine(DataDirectory, "_index.txt"); }
		}

		/// <summary>确证的存档名（与 GameManager.onGameSaveLoad 的 saveName 同源）。不再搬动已有文件。</summary>
		public void UseSaveName(string saveName)
		{
			if (string.IsNullOrEmpty(saveName)) return;
			string next = SanitizeFileName(saveName);
			m_Placeholder = false;
			if (next == m_SaveName) return;
			m_SaveName = next;
		}

		/// <summary>占位名：可被后续真实存档名接管（仅此类文件允许 File.Move）。</summary>
		public void UsePlaceholderName(string name)
		{
			if (string.IsNullOrEmpty(name)) return;
			string next = SanitizeFileName(name);
			m_Placeholder = true;
			if (next == m_SaveName) return;
			m_SaveName = next;
		}

		/// <summary>
		/// 首次命名：把本次会话的占位文件迁移到真实存档名，仅此一种情况允许改名。
		/// 必须在数据尚未被目标存档覆盖时调用。
		/// </summary>
		public void AdoptSaveName(string saveName)
		{
			if (string.IsNullOrEmpty(saveName)) return;
			// 读坏了就什么都别动：不许改名、不许删、不许覆盖
			if (m_WriteBlocked) return;
			string next = SanitizeFileName(saveName);
			if (next == m_SaveName)
			{
				m_Placeholder = false;
				return;
			}
			string oldPath = CurrentFilePath();
			bool canMove = m_Placeholder && !string.IsNullOrEmpty(oldPath) && File.Exists(oldPath);
			m_SaveName = next;
			m_Placeholder = false;
			if (!canMove) return;
			try
			{
				if (!File.Exists(CurrentFilePath()))
				{
					Directory.CreateDirectory(DataDirectory);
					File.Move(oldPath, CurrentFilePath());
				}
				else
				{
					File.Delete(oldPath);
				}
			}
			catch { }
		}

		/// <summary>未命名新档：本次进程内的会话占位名。</summary>
		public void EnsureSessionName()
		{
			if (!string.IsNullOrEmpty(m_SaveName)) return;
			NewSessionName();
		}

		/// <summary>
		/// 进档但身份无法确证时**必须**用这个而不是 EnsureSessionName：
		/// game→game 直接切换不经过主菜单，m_SaveName 还留着上一个存档的名字，
		/// 空转就等于沿用别人的文件名。
		/// </summary>
		public void StartUnnamedSession()
		{
			m_Items.Clear();
			m_Dirty = false;
			m_WriteBlocked = false;
			NewSessionName();
		}

		private void NewSessionName()
		{
			m_SessionFallback = "_unsaved_" + Guid.NewGuid().ToString("N").Substring(0, 8);
			UsePlaceholderName(m_SessionFallback);
		}

		/// <summary>回主菜单 / 进编辑器：彻底清场，避免在主菜单生成空文件。</summary>
		public void BeginMainMenu()
		{
			m_Items.Clear();
			m_SaveName = null;
			m_Placeholder = false;
			m_Dirty = false;
			m_WriteBlocked = false;
			m_SessionFallback = null;
		}

		public void MarkDirty()
		{
			m_Dirty = true;
		}

		public int Get(string itemId, string key, out bool found)
		{
			found = false;
			Dictionary<string, int> bag;
			if (!m_Items.TryGetValue(itemId, out bag)) return 0;
			int v;
			if (bag.TryGetValue(key, out v))
			{
				found = true;
				return v;
			}
			return 0;
		}

		public void Set(string itemId, string key, int value)
		{
			if (string.IsNullOrEmpty(key)) return;
			Dictionary<string, int> bag;
			if (!m_Items.TryGetValue(itemId, out bag))
			{
				bag = new Dictionary<string, int>(StringComparer.Ordinal);
				m_Items[itemId] = bag;
			}
			int old;
			if (bag.TryGetValue(key, out old) && old == value) return;
			bag[key] = value;
			m_Dirty = true;
		}

		public void ClearRuntime()
		{
			m_Items.Clear();
			m_Dirty = true;
		}

		public string CurrentFilePath()
		{
			if (string.IsNullOrEmpty(m_SaveName)) return null;
			return Path.Combine(DataDirectory, m_SaveName + ".json");
		}

		/// <summary>
		/// 载入当前名字对应的记忆。返回 false = 文件存在但读不懂（调用方应停止写盘并提示）。
		/// </summary>
		public bool LoadForCurrentSave()
		{
			m_Items.Clear();
			m_WriteBlocked = false;
			m_Dirty = false;
			string path = CurrentFilePath();
			if (path == null) return true;
			if (!File.Exists(path)) return true;
			Dictionary<string, Dictionary<string, int>> parsed =
				new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
			try
			{
				string text = File.ReadAllText(path, Encoding.UTF8);
				int version;
				if (!Parse(text, parsed, out version))
				{
					m_WriteBlocked = true;
					return false;
				}
				if (version > kVersion)
				{
					// 未来版本文件：不认识就不动，避免降级覆盖
					m_WriteBlocked = true;
					return false;
				}
				foreach (KeyValuePair<string, Dictionary<string, int>> kv in parsed)
				{
					m_Items[kv.Key] = kv.Value;
				}
			}
			catch
			{
				m_Items.Clear();
				m_WriteBlocked = true;
				return false;
			}
			return true;
		}

		/// <summary>写盘。onlyIfDirty 时保持「无改动不产生文件」。</summary>
		public bool SaveToDisk(bool onlyIfDirty = false)
		{
			if (onlyIfDirty && !m_Dirty) return false;
			if (m_WriteBlocked) return false;
			EnsureSessionName();
			string path = CurrentFilePath();
			if (path == null) return false;
			try
			{
				Directory.CreateDirectory(DataDirectory);
				File.WriteAllText(path, Serialize(), new UTF8Encoding(false));
				m_Dirty = false;
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 删除当前存档记忆文件并清空运行时。
		/// 主菜单里用（ResetAllSaves 之后）不会复活；游戏内用需要配合
		/// ToolMemorySystem.ResetActiveTool() 把面板先退回出厂值，否则下一帧捕获会把现值重新记进来。
		/// </summary>
		public void ResetCurrentSave()
		{
			m_Items.Clear();
			m_Dirty = false;
			m_WriteBlocked = false;
			string path = CurrentFilePath();
			try
			{
				if (path != null && File.Exists(path)) File.Delete(path);
			}
			catch { }
		}

		/// <summary>主菜单：删除全部存档记忆。</summary>
		public void ResetAllSaves()
		{
			m_Items.Clear();
			m_Dirty = false;
			m_WriteBlocked = false;
			try
			{
				string dir = DataDirectory;
				if (Directory.Exists(dir))
				{
					string[] files = Directory.GetFiles(dir, "*.json");
					for (int i = 0; i < files.Length; i++)
					{
						try { File.Delete(files[i]); } catch { }
					}
				}
			}
			catch { }
		}

		public static string SanitizeFileName(string name)
		{
			if (string.IsNullOrEmpty(name)) return "_unnamed";
			char[] invalid = Path.GetInvalidFileNameChars();
			StringBuilder sb = new StringBuilder(name.Length);
			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				bool bad = false;
				for (int j = 0; j < invalid.Length; j++)
				{
					if (c == invalid[j]) { bad = true; break; }
				}
				sb.Append(bad ? '_' : c);
			}
			string s = sb.ToString().Trim();
			if (s.Length == 0) return "_unnamed";
			if (s.Length > 80) s = s.Substring(0, 80);
			return s;
		}

		public string Serialize()
		{
			StringBuilder sb = new StringBuilder(1024);
			sb.Append("{\"v\":").Append(kVersion);
			sb.Append(",\"save\":");
			WriteJsonString(sb, m_SaveName ?? "");
			sb.Append(",\"items\":{");
			bool firstItem = true;
			foreach (KeyValuePair<string, Dictionary<string, int>> item in m_Items)
			{
				if (item.Value == null || item.Value.Count == 0) continue;
				if (!firstItem) sb.Append(',');
				firstItem = false;
				WriteJsonString(sb, item.Key);
				sb.Append(":{");
				bool firstKey = true;
				foreach (KeyValuePair<string, int> pair in item.Value)
				{
					if (!firstKey) sb.Append(',');
					firstKey = false;
					WriteJsonString(sb, pair.Key);
					sb.Append(':').Append(pair.Value.ToString(CultureInfo.InvariantCulture));
				}
				sb.Append('}');
			}
			sb.Append("}}");
			return sb.ToString();
		}

		/// <summary>
		/// 严格解析：结构不合规返回 false（调用方据此拒绝覆盖写盘）。
		/// </summary>
		public static bool Parse(string text, Dictionary<string, Dictionary<string, int>> into, out int version)
		{
			version = 0;
			if (string.IsNullOrEmpty(text)) return false;
			int i = 0;
			SkipWs(text, ref i);
			if (i >= text.Length || text[i] != '{') return false;
			i++;
			bool itemsSeen = false;
			while (i < text.Length)
			{
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == '}') { i++; return true; }
				string key = ReadJsonString(text, ref i);
				if (key == null) return false;
				SkipWs(text, ref i);
				if (i >= text.Length || text[i] != ':') return false;
				i++;
				SkipWs(text, ref i);
				if (key == "v")
				{
					version = ReadJsonInt(text, ref i);
				}
				else if (key == "items" && i < text.Length && text[i] == '{')
				{
					if (!ParseItems(text, ref i, into)) return itemsSeen ? false : false;
					itemsSeen = true;
				}
				else
				{
					if (!SkipValue(text, ref i)) return false;
				}
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == ',') i++;
			}
			return itemsSeen;
		}

		public static bool Parse(string text, Dictionary<string, Dictionary<string, int>> into)
		{
			int v;
			return Parse(text, into, out v);
		}

		private static bool ParseItems(string text, ref int i, Dictionary<string, Dictionary<string, int>> into)
		{
			i++; // {
			while (i < text.Length)
			{
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == '}') { i++; return true; }
				string itemId = ReadJsonString(text, ref i);
				if (itemId == null) return false;
				SkipWs(text, ref i);
				if (i >= text.Length || text[i] != ':') return false;
				i++;
				SkipWs(text, ref i);
				if (i >= text.Length || text[i] != '{') return false;
				i++;
				Dictionary<string, int> bag = new Dictionary<string, int>(StringComparer.Ordinal);
				bool closed = false;
				while (i < text.Length)
				{
					SkipWs(text, ref i);
					if (i < text.Length && text[i] == '}') { i++; closed = true; break; }
					string k = ReadJsonString(text, ref i);
					if (k == null) return false;
					SkipWs(text, ref i);
					if (i >= text.Length || text[i] != ':') return false;
					i++;
					SkipWs(text, ref i);
					bag[k] = ReadJsonInt(text, ref i);
					SkipWs(text, ref i);
					if (i < text.Length && text[i] == ',') i++;
				}
				if (!closed) return false;
				into[itemId] = bag;
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == ',') i++;
			}
			return false;
		}

		private static void SkipWs(string s, ref int i)
		{
			while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n')) i++;
		}

		private static string ReadJsonString(string s, ref int i)
		{
			SkipWs(s, ref i);
			if (i >= s.Length || s[i] != '"') return null;
			i++;
			StringBuilder sb = new StringBuilder();
			while (i < s.Length)
			{
				char c = s[i];
				if (c == '"') { i++; return sb.ToString(); }
				if (c == '\\' && i + 1 < s.Length)
				{
					i++;
					char e = s[i];
					if (e == 'n') sb.Append('\n');
					else if (e == 't') sb.Append('\t');
					else if (e == 'r') sb.Append('\r');
					else sb.Append(e);
					i++;
					continue;
				}
				sb.Append(c);
				i++;
			}
			return null;
		}

		private static int ReadJsonInt(string s, ref int i)
		{
			SkipWs(s, ref i);
			int start = i;
			if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
			while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
			if (i == start) return 0;
			int v;
			int.TryParse(s.Substring(start, i - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
			return v;
		}

		private static bool SkipValue(string s, ref int i)
		{
			SkipWs(s, ref i);
			if (i >= s.Length) return false;
			char c = s[i];
			if (c == '{' || c == '[')
			{
				char open = c;
				char close = (c == '{') ? '}' : ']';
				int depth = 0;
				do
				{
					if (s[i] == open) depth++;
					else if (s[i] == close) depth--;
					i++;
				} while (i < s.Length && depth > 0);
				return depth == 0;
			}
			if (c == '"')
			{
				return ReadJsonString(s, ref i) != null;
			}
			int start = i;
			while (i < s.Length && s[i] != ',' && s[i] != '}' && s[i] != ']') i++;
			return i > start;
		}

		private static void WriteJsonString(StringBuilder sb, string value)
		{
			sb.Append('"');
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (c == '"' || c == '\\') { sb.Append('\\').Append(c); }
				else if (c == '\n') sb.Append("\\n");
				else if (c == '\r') sb.Append("\\r");
				else if (c == '\t') sb.Append("\\t");
				else sb.Append(c);
			}
			sb.Append('"');
		}
	}
}
