using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ToolModeMemory.Memory;

namespace ToolModeMemory.Tests
{
	/// <summary>
	/// v0.1.2 存档记忆存储回归测试（离线，无游戏程序集）。
	/// 运行： dotnet run -c Release --project tests\StoreHarness
	/// 覆盖：JSON 往返、坏文件保护、占位文件改名迁移、正式存档名之间绝不搬文件。
	/// </summary>
	internal static class StoreHarness
	{
		private static int s_Fails;
		private static int s_Passed;
		private static string s_Root;

		private static void Check(bool cond, string msg)
		{
			if (cond) { s_Passed++; Console.WriteLine("  OK   " + msg); }
			else { s_Fails++; Console.WriteLine("  FAIL " + msg); }
		}

		private static string FreshDir(string caseName)
		{
			string dir = Path.Combine(s_Root, caseName);
			if (Directory.Exists(dir)) Directory.Delete(dir, true);
			UnityEngine.Application.TestDir = dir;
			Directory.CreateDirectory(MemoryStore.DataDirectory);
			return dir;
		}

		private static string DirOf(MemoryStore s)
		{
			return MemoryStore.DataDirectory;
		}

		private static int Main()
		{
			s_Root = Path.Combine(Path.GetTempPath(), "tmm_store_harness");
			Directory.CreateDirectory(s_Root);

			JsonRoundTrip();
			KeyFormat();
			Identity();
			IndexFile();
			SessionHandover();
			JsonRobustness();
			BadFileIsNotOverwritten();
			FutureVersionIsNotOverwritten();
			PlaceholderMigratesToSaveName();
			NamedSaveNeverMovesPreviousFile();
			AdoptKeepsExistingTargetFile();
			OnlyDirtyWrites();
			BeginMainMenuClears();
			FileNameSanitizer();

			Console.WriteLine();
			Console.WriteLine(s_Fails == 0
				? ("ALL PASS (" + s_Passed + " checks)")
				: ("FAILURES=" + s_Fails + " / passed=" + s_Passed));
			return s_Fails == 0 ? 0 : 1;
		}

		// ---------- 用例 ----------

		private static void SessionHandover()
		{
			Console.WriteLine("[14] game→game 直接切换与只读保护（残留的沿用旧名路径）");
			UnityEngine.Application.TestDir = FreshDir("handover");

			MemoryStore a = new MemoryStore();
			a.UseSaveName("CityA");
			a.Set("net.draw", "G:Roads/Small", 6);
			Check(a.SaveToDisk(true), "CityA.json 建立");
			string aPath = a.CurrentFilePath();

			// 不经过 BeginMainMenu，直接进一个身份未确证的档（新建城市 / game->game 切换）
			a.StartUnnamedSession();
			Check(a.IsPlaceholder && a.SaveName != "CityA", "强制换成新占位名");
			a.Set("net.snap", "M:Roads", 3);
			Check(a.SaveToDisk(true), "写到自己的占位文件");
			Check(a.CurrentFilePath() != aPath, "没有沿用 CityA 的文件");
			Check(File.ReadAllText(aPath).Contains("G:Roads/Small"), "CityA 内容未被改写");

			// EnsureSessionName 在已有名字时确实空转（所以必须用 StartUnnamedSession）
			MemoryStore b = new MemoryStore();
			b.UseSaveName("CityB");
			b.EnsureSessionName();
			Check(b.SaveName == "CityB" && !b.IsPlaceholder, "EnsureSessionName 不覆盖已有正式名");

			// 只读保护不许被改名绕过
			MemoryStore c = new MemoryStore();
			c.UseSaveName("CityC");
			c.Set("net.draw", "G:x", 1);
			c.SaveToDisk(true);
			string cPath = c.CurrentFilePath();
			File.WriteAllText(cPath, "{ broken", Encoding.UTF8);
			Check(!c.LoadForCurrentSave(), "读坏 -> 只读保护");
			c.AdoptSaveName("CityD");
			Check(c.SaveName == "CityC", "保护期间不许改名");
			Check(File.Exists(cPath) && File.ReadAllText(cPath).Contains("broken"), "坏文件原样保留");
			Check(!c.SaveToDisk(), "保护期间不许写");
		}

		private static void Identity()
		{
			Console.WriteLine("[12] 存档身份判定（对应「读错档 / 写坏别人记忆」缺陷）");
			SaveIndex idx = new SaveIndex();

			string name; bool ph;

			// Purpose 映射
			Check(SaveIdentity.Classify(1) == SaveIdentity.PurposeKind.NewCity, "NewGame -> 新建城市");
			Check(SaveIdentity.Classify(4) == SaveIdentity.PurposeKind.NewCity, "NewMap -> 新建城市");
			Check(SaveIdentity.Classify(2) == SaveIdentity.PurposeKind.LoadedSave, "LoadGame -> 已存存档");
			Check(SaveIdentity.Classify(0) == SaveIdentity.PurposeKind.LoadedSave, "SaveGame -> 已存存档");
			Check(SaveIdentity.Classify(6) == SaveIdentity.PurposeKind.NonSave, "Cleanup -> 不参与");
			Check(!SaveIdentity.ShouldParticipate(SaveIdentity.PurposeKind.NonSave), "编辑器/清理不参与");

			// 新建城市：即便 lastSaveGameMetadata 还指向上一个档，也绝不能沿用
			idx.Set("guidA", "CityA");
			Check(!SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.NewCity, true, "guidB",
				"guidA", "CityA", false, idx, out name, out ph),
				"新建城市不解析出名字（用会话占位）");
			Check(name == null && !ph, "新建城市输出为空");

			// 从存档列表载入 B，而 lastSaveGameMetadata 还指向 A -> 不许用 A 的名字
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidB",
				"guidA", "CityA", false, idx, out name, out ph)
				&& name == "_auto_guidB" && ph, "id 不符 -> 用 guid 占位，不冒名");

			// Continue 载入 A：id 与 guid 一致 -> 用真实存档名
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidA",
				"guidA", "CityA", false, idx, out name, out ph)
				&& name == "CityA" && !ph, "id 相符 -> 用存档名");

			// 自动存档顶掉了 metadata -> 索引里认得就用索引里的名字
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidA",
				"guidAuto", "27-September-13-45-02", true, idx, out name, out ph)
				&& name == "CityA" && !ph, "自动存档污染 -> 索引接管");

			// 拿不到 guid -> 拒绝猜测
			Check(!SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, false, null,
				"guidA", "CityA", false, idx, out name, out ph), "无 guid 不猜名字");

			// 无索引无 metadata -> 稳定占位（重启后仍能找回同一份记忆）
			SaveIndex empty = new SaveIndex();
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidZ",
				null, null, true, empty, out name, out ph) && name == "_auto_guidZ" && ph,
				"首次遇到 -> 确定性 _auto_ 占位");

			// 存档事件：自动存档必须忽略
			Check(!SaveIdentity.ShouldAdoptOnSave(true, true, "27-September-13-45-02"), "忽略自动存档事件");
			Check(!SaveIdentity.ShouldAdoptOnSave(false, false, "CityA"), "存档失败不接管");
			Check(!SaveIdentity.ShouldAdoptOnSave(true, false, ""), "空名字不接管");
			Check(SaveIdentity.ShouldAdoptOnSave(true, false, "CityA"), "手动存档才接管");
		}

		private static void IndexFile()
		{
			Console.WriteLine("[13] guid|存档名 索引文件");
			SaveIndex idx = new SaveIndex();
			Check(idx.Set("g1", "我的城市"), "写入变化");
			Check(!idx.Set("g1", "我的城市"), "重复写入不算变化");
			Check(idx.Set("g2", "Other/Weird|Name"), "值里的分隔符");
			Check(idx.LookupName("g2") == "Other/Weird|Name", "按 guid 查名");
			Check(idx.LookupGuid("我的城市") == "g1", "按名查 guid");
			Check(idx.LookupName("nope") == null, "查不到返回 null");
			Check(!idx.Set(null, "x") && !idx.Set("g3", null), "空键值拒绝");

			string text = idx.Serialize();
			SaveIndex back = SaveIndex.Parse(text);
			Check(back.Count == 2, "往返条数");
			Check(back.LookupName("g1") == "我的城市", "往返内容");
			Check(SaveIndex.Parse("").Count == 0 && SaveIndex.Parse(null).Count == 0, "空文本安全");
			Check(SaveIndex.Parse("garbage\n\n# comment\n||||\ng4|CityFour").LookupName("g4") == "CityFour",
				"脏行跳过，好行保留");

			// 索引文件与记忆文件同目录，且 ResetAllSaves(*.json) 不会误删它
			UnityEngine.Application.TestDir = FreshDir("index");
			Check(MemoryStore.IndexPath.EndsWith("_index.txt"), "索引文件名不是 .json");
			Check(MemoryStore.IndexPath.StartsWith(MemoryStore.DataDirectory), "索引与记忆同目录");
		}

		private static void KeyFormat()
		{
			Console.WriteLine("[11] 范围键格式：五档互不相同，且解析不出来时不拼 \"null\"");
			string shared = MemoryKeys.Shared();
			string menu = MemoryKeys.Menu("Roads");
			string group = MemoryKeys.Group("Roads", "SmallRoads");
			string cat = MemoryKeys.Category("SmallRoads");
			string asset = MemoryKeys.Asset("Game.Prefabs.NetPrefab:Alley");
			Check(shared == "S", "全局共用 = S");
			Check(menu == "M:Roads", "同菜单 = 菜单名");
			Check(group == "G:Roads/SmallRoads", "同组 = 菜单/分类（原版粒度）");
			Check(cat == "C:SmallRoads", "同类资产 = 裸分类名（跨菜单）");
			Check(asset == "P:Game.Prefabs.NetPrefab:Alley", "单资产 = PrefabID");
			Check(shared != menu && menu != group && group != cat && cat != asset && asset != shared, "五档键互不相同");

			Check(MemoryKeys.Group(null, "Tunnels") == "G:Tunnels", "菜单未知 -> 不出现 /null");
			Check(MemoryKeys.Group("Roads", null) == null, "分类未知 -> 返回 null 交给兜底");
			Check(MemoryKeys.Menu(null) == null, "Menu(null) = null");
			Check(MemoryKeys.Category(null) == null, "Category(null) = null");
			Check(MemoryKeys.Asset(null) == null, "Asset(null) = null");
			Check(MemoryKeys.Tool("Water Tool") == "T:Water Tool", "兜底按工具隔离");
			Check(MemoryKeys.Tool(null) == "T:null", "兜底无工具");
			Check(MemoryKeys.PrefixFor(MemoryScope.Group) == "G:" && MemoryKeys.PrefixFor(MemoryScope.Menu) == "M:"
				&& MemoryKeys.PrefixFor(MemoryScope.Category) == "C:"
				&& MemoryKeys.PrefixFor(MemoryScope.GlobalUnique) == "P:"
				&& MemoryKeys.PrefixFor(MemoryScope.GlobalShared) == "S", "每档前缀唯一");

			ToolItemDef[] items = ToolItemCatalog.Items;
			Check(items.Length == 12, "v0.1.2 = 12 个工具项（terrain.mode / upgrade.mode 已删）");
			bool ok = true;
			for (int i = 0; i < items.Length; i++)
			{
				int v = items[i].RecommendedScope;
				if (v < 0 || v > (int)MemoryScope.GlobalUnique) ok = false;
				if (string.IsNullOrEmpty(items[i].Id)) ok = false;
				if (ToolItemCatalog.Find(items[i].Id) == null) ok = false;
				if (items[i].VanillaScope != ToolItemCatalog.kVanillaNone
					&& (items[i].VanillaScope < 0 || items[i].VanillaScope > (int)MemoryScope.GlobalUnique)) ok = false;
			}
			Check(ok, "推荐/原版档位都在枚举范围内");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kNetElevation).VanillaScope == (int)MemoryScope.GlobalShared,
				"高程原版档 = 全局共用（整个会话只有一个值）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kObjPlace).VanillaScope == ToolItemCatalog.kVanillaNone,
				"放置模式原版不记忆");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kNetDraw).VanillaScope == (int)MemoryScope.Group,
				"绘制模式原版档 = 同组（NetToolPreferences 按 m_Group）");
		}

		private static void JsonRoundTrip()
		{
			Console.WriteLine("[1] JSON 往返（新键格式）");
			UnityEngine.Application.TestDir = FreshDir("json");
			MemoryStore a = new MemoryStore();
			a.UseSaveName("My City");
			a.Set("net.draw", "G:Roads/SmallRoads", 2);
			a.Set("net.snap", "M:Roads", 15);
			a.Set("net.elevation", "P:Game.Prefabs.NetPrefab:Alley", -320);
			a.Set("obj.place", "C:Tunnels", 1);
			string json = a.Serialize();
			Check(json.Contains("\"v\":" + MemoryStore.kVersion), "写入当前版本");
			Check(json.Contains("G:Roads/SmallRoads"), "Group 键含菜单+分类");

			Dictionary<string, Dictionary<string, int>> into = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
			int v;
			bool ok = MemoryStore.Parse(json, into, out v);
			Check(ok, "解析成功");
			Check(v == MemoryStore.kVersion, "版本号读回");
			Check(into["net.draw"]["G:Roads/SmallRoads"] == 2, "net.draw 值回读");
			Check(into["net.snap"]["M:Roads"] == 15, "net.snap 值回读");
			Check(into["net.elevation"]["P:Game.Prefabs.NetPrefab:Alley"] == -320, "负高程回读");
			Check(into["obj.place"]["C:Tunnels"] == 1, "Category 键回读");

			// 磁盘往返
			Check(a.SaveToDisk(true), "写盘");
			MemoryStore b = new MemoryStore();
			b.UseSaveName("My City");
			Check(b.LoadForCurrentSave(), "读盘不报错");
			bool found;
			Check(b.Get("net.draw", "G:Roads/SmallRoads", out found) == 2 && found, "跨实例读回");
			Check(!b.Dirty, "读回后不脏");
			Check(!b.IsPlaceholder, "正式存档名不算占位");
		}

		private static void JsonRobustness()
		{
			Console.WriteLine("[2] 异常输入不抛");
			UnityEngine.Application.TestDir = FreshDir("robust");
			Dictionary<string, Dictionary<string, int>> into = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
			string[] bad =
			{
				"", "{", "not json", "{}", "{\"v\":2}", "{\"items\":{", "{\"v\":2,\"items\":{\"a\":{\"K:1\"}}}",
				"{\"v\":2,\"items\":{\"a\":{\"K:1\":}}}","{\"v\":2,\"items\":{\"a\":{\"K:1\":99999999999999}}}",
			};
			bool threw = false;
			try
			{
				foreach (string s in bad)
				{
					into.Clear();
					MemoryStore.Parse(s, into, out int _);
				}
			}
			catch (Exception ex) { threw = true; Console.WriteLine("       ex=" + ex.GetType().Name); }
			Check(!threw, "全部坏输入不抛异常");

			into.Clear();
			Check(!MemoryStore.Parse("not json", into, out int _), "非 JSON 判为不可用");
			into.Clear();
			Check(MemoryStore.Parse("{\"v\":2,\"other\":{\"x\":1},\"items\":{\"a\":{\"K:1\":3}}}", into, out int v2)
				&& v2 == 2 && into["a"]["K:1"] == 3, "未知顶层字段被跳过且不影响 items");
		}

		private static void BadFileIsNotOverwritten()
		{
			Console.WriteLine("[3] 坏文件禁止覆盖（用户可手工修复）");
			UnityEngine.Application.TestDir = FreshDir("bad");
			string path = Path.Combine(MemoryStore.DataDirectory, "Broken.json");
			File.WriteAllText(path, "{ this is not valid json", Encoding.UTF8);

			MemoryStore s = new MemoryStore();
			s.UseSaveName("Broken");
			Check(!s.LoadForCurrentSave(), "载入报告失败");
			Check(s.WriteBlocked, "进入只读保护");
			s.Set("net.draw", "G:Roads/Small", 4);
			Check(!s.SaveToDisk(), "拒绝覆盖");
			Check(File.ReadAllText(path).Contains("not valid"), "磁盘原文未被改写");
		}

		private static void FutureVersionIsNotOverwritten()
		{
			Console.WriteLine("[4] 更高版本文件不被降级覆盖");
			UnityEngine.Application.TestDir = FreshDir("future");
			string path = Path.Combine(MemoryStore.DataDirectory, "New.json");
			File.WriteAllText(path, "{\"v\":99,\"save\":\"New\",\"items\":{\"net.draw\":{\"G:x\":1}}}", Encoding.UTF8);
			MemoryStore s = new MemoryStore();
			s.UseSaveName("New");
			Check(!s.LoadForCurrentSave(), "v99 判为不可用");
			Check(s.WriteBlocked, "只读保护");
			s.Set("net.draw", "G:x", 7);
			Check(!s.SaveToDisk(), "不写盘");
			Check(File.ReadAllText(path).Contains("\"v\":99"), "原文保留");
		}

		private static void PlaceholderMigratesToSaveName()
		{
			Console.WriteLine("[5] 新建城市：占位文件在首次存档时改名接管");
			UnityEngine.Application.TestDir = FreshDir("migrate");
			MemoryStore s = new MemoryStore();
			s.EnsureSessionName();
			Check(s.IsPlaceholder, "占位标记");
			s.Set("net.draw", "G:Roads/Small", 3);
			Check(s.SaveToDisk(true), "会话内落盘到占位文件");
			string placeholder = s.CurrentFilePath();
			Check(File.Exists(placeholder), "占位文件存在");

			s.AdoptSaveName("First City");
			Check(!s.IsPlaceholder, "改名后不再是占位");
			Check(!File.Exists(placeholder), "占位文件已搬走");
			Check(File.Exists(s.CurrentFilePath()), "新文件存在");
			Check(s.Get("net.draw", "G:Roads/Small", out bool f) == 3 && f, "数据随文件保留");
		}

		private static void NamedSaveNeverMovesPreviousFile()
		{
			Console.WriteLine("[6] 关键回归：载入另一存档时，绝不允许改名搬走上一个存档的记忆");
			UnityEngine.Application.TestDir = FreshDir("crosssave");
			MemoryStore a = new MemoryStore();
			a.UseSaveName("CityA");
			a.Set("net.draw", "G:Roads/Small", 9);
			Check(a.SaveToDisk(true), "CityA.json 写出");
			string aPath = a.CurrentFilePath();

			// 模拟：回主菜单后载入没有记忆文件的 CityB
			MemoryStore live = new MemoryStore();
			live.UseSaveName("CityA");
			live.LoadForCurrentSave();
			live.UseSaveName("CityB");
			Check(File.Exists(aPath), "CityA.json 仍然存在（未被改名）");
			Check(!File.Exists(Path.Combine(DirOf(live), "CityB.json")), "CityB 未被伪造");
			live.Set("net.snap", "M:Roads", 2);
			Check(live.SaveToDisk(true), "CityB 写自己的文件");
			Check(File.ReadAllText(aPath).Contains("\"v\":" + MemoryStore.kVersion), "CityA 内容未变");
			Check(File.ReadAllText(aPath).Contains("G:Roads/Small"), "CityA 键未丢");
		}

		private static void AdoptKeepsExistingTargetFile()
		{
			Console.WriteLine("[7] 占位改名遇到同名正式文件：删占位，不产生搬移事故");
			UnityEngine.Application.TestDir = FreshDir("adopt");
			MemoryStore s = new MemoryStore();
			s.EnsureSessionName();
			s.Set("net.draw", "G:Roads/Small", 5);
			s.SaveToDisk(true);
			string placeholder = s.CurrentFilePath();

			// 目标已存在（该名字 previously 玩过）
			MemoryStore other = new MemoryStore();
			other.UseSaveName("Renamed");
			other.Set("net.snap", "M:Roads", 11);
			other.SaveToDisk(true);
			string target = other.CurrentFilePath();

			s.AdoptSaveName("Renamed");
			Check(!File.Exists(placeholder), "占位文件被清理");
			Check(File.Exists(target), "正式文件保留");
			Check(s.CurrentFilePath() == target, "后续写盘指向正式名");
		}

		private static void OnlyDirtyWrites()
		{
			Console.WriteLine("[8] 无改动不产生文件");
			UnityEngine.Application.TestDir = FreshDir("dirty");
			MemoryStore s = new MemoryStore();
			Check(!s.Dirty, "新建不脏");
			Check(!s.SaveToDisk(true), "onlyIfDirty 不写");
			Check(Directory.GetFiles(MemoryStore.DataDirectory, "*.json").Length == 0, "目录仍为空");
			s.Set("net.snap", "S", 1);
			Check(s.Dirty, "写入后置脏");
			Check(s.Get("net.snap", "S", out bool f) == 1 && f, "读回");
			s.Set("net.snap", "S", 1);
			Check(s.Dirty, "同值不重复置脏（仍为之前的脏）");
			Check(s.SaveToDisk(true), "写盘");
			Check(!s.Dirty, "写盘后干净");
			Check(s.SaveToDisk(true) == false, "干净时不再写");
		}

		private static void BeginMainMenuClears()
		{
			Console.WriteLine("[9] 回主菜单清场 + 重置记忆");
			UnityEngine.Application.TestDir = FreshDir("menu");
			MemoryStore s = new MemoryStore();
			s.UseSaveName("City");
			s.Set("net.draw", "G:a", 1);
			s.SaveToDisk(true);
			s.BeginMainMenu();
			Check(s.SaveName == null, "名字清空");
			Check(!s.Dirty, "不脏 -> 主菜单退出不生成文件");
			Check(!s.SaveToDisk(true), "主菜单不写盘");

			s.UseSaveName("City");
			s.Set("net.draw", "G:a", 1);
			s.ResetCurrentSave();
			Check(!File.Exists(s.CurrentFilePath()), "文件被删");
			Check(!s.Dirty, "重置后不脏");
			Check(!s.SaveToDisk(true), "重置后不会立刻复活文件");
		}

		private static void FileNameSanitizer()
		{
			Console.WriteLine("[10] 存档名净化");
			Check(MemoryStore.SanitizeFileName("a/b:c*?") == "a_b_c__", "非法字符替换");
			Check(MemoryStore.SanitizeFileName("   ") == "_unnamed", "空白名");
			Check(MemoryStore.SanitizeFileName(null) == "_unnamed", "null 名");
			Check(MemoryStore.SanitizeFileName(new string('x', 200)).Length == 80, "超长截断");
			Check(MemoryStore.SanitizeFileName("城市 名字-1_2") == "城市 名字-1_2", "合法中文名保留");
		}
	}
}
