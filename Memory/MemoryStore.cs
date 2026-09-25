using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 按存档持久化的工具记忆。文件名 = 存档名（净化后），每档一份，退出覆盖写。
	/// 格式为可手工编辑的 JSON：{"v":1,"save":"...","items":{"net.draw":{"G:1":2}}}
	/// </summary>
	public sealed class MemoryStore
	{
		public const int kVersion = 1;

		private readonly Dictionary<string, Dictionary<string, int>> m_Items =
			new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);

		private string m_SaveName;
		private string m_SessionFallback;
		private bool m_Dirty;

		public string SaveName { get { return m_SaveName; } }
		public bool Dirty { get { return m_Dirty; } }

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

		public void SetSaveName(string saveName)
		{
			if (string.IsNullOrEmpty(saveName)) return;
			string next = SanitizeFileName(saveName);
			if (next == m_SaveName) return;
			string oldPath = CurrentFilePath();
			m_SaveName = next;
			m_Dirty = true;
			// 首次命名：若存在会话占位文件则迁移
			try
			{
				if (!string.IsNullOrEmpty(oldPath) && File.Exists(oldPath) && !File.Exists(CurrentFilePath()))
				{
					File.Move(oldPath, CurrentFilePath());
				}
			}
			catch { }
		}

		public void EnsureSessionName()
		{
			if (!string.IsNullOrEmpty(m_SaveName)) return;
			if (string.IsNullOrEmpty(m_SessionFallback))
			{
				m_SessionFallback = "_unsaved_" + Guid.NewGuid().ToString("N").Substring(0, 8);
			}
			m_SaveName = m_SessionFallback;
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

		public void LoadForCurrentSave()
		{
			m_Items.Clear();
			string path = CurrentFilePath();
			if (path == null || !File.Exists(path))
			{
				m_Dirty = false;
				return;
			}
			try
			{
				string text = File.ReadAllText(path, Encoding.UTF8);
				Parse(text, m_Items);
				m_Dirty = false;
			}
			catch
			{
				m_Items.Clear();
			}
		}

		public bool SaveToDisk()
		{
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

		/// <summary>删除当前存档记忆文件并清空运行时。</summary>
		public void ResetCurrentSave()
		{
			ClearRuntime();
			string path = CurrentFilePath();
			try
			{
				if (path != null && File.Exists(path)) File.Delete(path);
			}
			catch { }
			m_Dirty = false;
		}

		/// <summary>主菜单：删除全部存档记忆。</summary>
		public void ResetAllSaves()
		{
			ClearRuntime();
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
			m_Dirty = false;
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

		public static void Parse(string text, Dictionary<string, Dictionary<string, int>> into)
		{
			int i = 0;
			SkipWs(text, ref i);
			if (i >= text.Length || text[i] != '{') return;
			i++;
			while (i < text.Length)
			{
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == '}') break;
				string key = ReadJsonString(text, ref i);
				if (key == null) break;
				SkipWs(text, ref i);
				if (i >= text.Length || text[i] != ':') break;
				i++;
				SkipWs(text, ref i);
				if (key == "items" && i < text.Length && text[i] == '{')
				{
					ParseItems(text, ref i, into);
				}
				else
				{
					SkipValue(text, ref i);
				}
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == ',') i++;
			}
		}

		private static void ParseItems(string text, ref int i, Dictionary<string, Dictionary<string, int>> into)
		{
			i++; // {
			while (i < text.Length)
			{
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == '}') { i++; return; }
				string itemId = ReadJsonString(text, ref i);
				if (itemId == null) return;
				SkipWs(text, ref i);
				if (i >= text.Length || text[i] != ':') return;
				i++;
				SkipWs(text, ref i);
				if (i >= text.Length || text[i] != '{') { SkipValue(text, ref i); continue; }
				i++;
				Dictionary<string, int> bag = new Dictionary<string, int>(StringComparer.Ordinal);
				while (i < text.Length)
				{
					SkipWs(text, ref i);
					if (i < text.Length && text[i] == '}') { i++; break; }
					string k = ReadJsonString(text, ref i);
					if (k == null) break;
					SkipWs(text, ref i);
					if (i >= text.Length || text[i] != ':') break;
					i++;
					SkipWs(text, ref i);
					int val = ReadJsonInt(text, ref i);
					bag[k] = val;
					SkipWs(text, ref i);
					if (i < text.Length && text[i] == ',') i++;
				}
				into[itemId] = bag;
				SkipWs(text, ref i);
				if (i < text.Length && text[i] == ',') i++;
			}
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
			return sb.ToString();
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

		private static void SkipValue(string s, ref int i)
		{
			SkipWs(s, ref i);
			if (i >= s.Length) return;
			char c = s[i];
			if (c == '{')
			{
				int depth = 0;
				do
				{
					if (s[i] == '{') depth++;
					else if (s[i] == '}') depth--;
					i++;
				} while (i < s.Length && depth > 0);
			}
			else if (c == '[')
			{
				int depth = 0;
				do
				{
					if (s[i] == '[') depth++;
					else if (s[i] == ']') depth--;
					i++;
				} while (i < s.Length && depth > 0);
			}
			else if (c == '"')
			{
				ReadJsonString(s, ref i);
			}
			else
			{
				while (i < s.Length && s[i] != ',' && s[i] != '}' && s[i] != ']') i++;
			}
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
