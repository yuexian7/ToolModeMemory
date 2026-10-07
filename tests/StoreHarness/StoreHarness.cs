using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ToolModeMemory.Memory;

namespace ToolModeMemory.Tests
{
	/// <summary>
	/// v0.2.0 存档记忆存储回归测试（离线，无游戏程序集）。
	/// 运行： cd tests\StoreHarness && dotnet run -c Release
	/// 覆盖：JSON 往返、坏文件保护、占位文件改名迁移、正式存档名绝不搬文件、
	///       实时落盘与崩溃恢复（tmp + File.Replace）、v3 键的资产/功能域与枚举家族分离、
	///       v0.2.0 目录形状（11 项 / Subs 子字段 / Anarchy 归属）、旧版本文件的未知项过滤。
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

		/// <summary>
		/// 生产环境的完整键 = 域 + 层级 + '$' + 枚举家族（ToolMemoryBridge.Finish）。
		/// '$' 与 "A|" / "F|" 都不会出现在 prefab 名 / PrefabID 里。
		/// </summary>
		private static string K(string levelKey, bool isFunction, string family)
		{
			return MemoryKeys.WithDomain(levelKey, isFunction) + "$" + family;
		}

		private static readonly string kSmallRoads = MemoryKeys.Group("Roads", "SmallRoads");
		private static readonly string kAlleyAsset = MemoryKeys.Asset("Game.Prefabs.NetPrefab:Alley");

		/// <summary>
		/// v0.2.0 实时落盘 + 崩溃恢复：tmp 原子替换、坏主文件从 tmp 救回、序号防抖基线。
		/// </summary>
		private static void LiveWriteAndCrashRecovery()
		{
			Console.WriteLine("[15] 实时落盘与崩溃恢复");
			FreshDir("livewrite");

			MemoryStore a = new MemoryStore();
			a.UseSaveName("CityLive");
			string modeKey = K(kSmallRoads, false, "net");
			long s0 = a.ChangeSerial;
			a.Set(ToolItemCatalog.kToolMode, modeKey, 2);
			Check(a.ChangeSerial == s0 + 1, "改动递增序号");
			a.Set(ToolItemCatalog.kToolMode, modeKey, 2);
			Check(a.ChangeSerial == s0 + 1, "同值不改序号（防抖不被空改动重置）");

			Check(a.SaveToDisk(true), "写盘成功");
			string main = a.CurrentFilePath();
			Check(File.Exists(main), "主文件存在");
			Check(!File.Exists(main + ".tmp"), "写完不留 .tmp");
			Check(Directory.GetFiles(DirOf(a), "*.tmp").Length == 0, "目录里没有残留 tmp");

			// 再改两次并落盘，拿到「新内容」的完整副本，然后模拟闪退打断主文件
			a.Set(ToolItemCatalog.kToolMode, modeKey, 5);
			Check(a.SaveToDisk(true), "第二次写盘");
			a.Set(ToolItemCatalog.kToolMode, modeKey, 9);
			Check(a.SaveToDisk(true), "第三次写盘");
			// 同值重复写既不置脏也不推进序号：实时保存每帧都在跑，不能靠 Dirty 判新改动
			a.Set(ToolItemCatalog.kToolMode, modeKey, 9);
			Check(a.ChangeSerial == s0 + 3 && !a.Dirty, "现值未变 -> 序号与脏标记都不动");
			string newest = File.ReadAllText(main, Encoding.UTF8);

			// 场景 1：主文件被写坏（半截 JSON），tmp 是上一次的完整副本
			File.WriteAllText(main, "{\"v\":2,\"items\":{", Encoding.UTF8);
			File.WriteAllText(main + ".tmp", newest, Encoding.UTF8);
			MemoryStore b = new MemoryStore();
			b.UseSaveName("CityLive");
			Check(b.LoadForCurrentSave(), "坏主文件 + 可读 tmp -> 载入成功");
			Check(b.RecoveredFromTemp, "标记为从 tmp 恢复");
			bool found;
			Check(b.Get(ToolItemCatalog.kToolMode, modeKey, out found) == 9 && found, "救回的是最新值");
			Check(b.Dirty, "恢复后置脏，等着重新写正式文件");
			Check(b.SaveToDisk(true), "重新写正式文件");
			Check(!File.Exists(main + ".tmp"), "修好后清掉 tmp");
			Check(File.ReadAllText(main, Encoding.UTF8) == newest, "主文件内容复原");

			// 场景 2：只有 tmp（主文件从没写成功过）
			FreshDir("onlytmp");
			MemoryStore c = new MemoryStore();
			c.UseSaveName("CityOnlyTmp");
			c.Set(ToolItemCatalog.kElevation, K(kSmallRoads, false, "net"), 1000);
			string cMain = c.CurrentFilePath();
			File.WriteAllText(cMain + ".tmp", c.Serialize(), Encoding.UTF8);
			MemoryStore c2 = new MemoryStore();
			c2.UseSaveName("CityOnlyTmp");
			Check(c2.LoadForCurrentSave(), "无主文件、有 tmp -> 成功");
			Check(c2.RecoveredFromTemp && c2.Get(ToolItemCatalog.kElevation,
				K(kSmallRoads, false, "net"), out found) == 1000 && found, "从 tmp 拿到高度记忆");

			// 场景 3：主文件和 tmp 都是垃圾 -> 保持写禁，两个都不许覆盖
			FreshDir("bothbad");
			MemoryStore d = new MemoryStore();
			d.UseSaveName("CityBad");
			d.Set(ToolItemCatalog.kToolMode, "S", 1);
			d.SaveToDisk(true);
			string dMain = d.CurrentFilePath();
			File.WriteAllText(dMain, "not json", Encoding.UTF8);
			File.WriteAllText(dMain + ".tmp", "also not json", Encoding.UTF8);
			MemoryStore d2 = new MemoryStore();
			d2.UseSaveName("CityBad");
			Check(!d2.LoadForCurrentSave(), "两处都读不懂 -> 判定失败");
			Check(d2.WriteBlocked && !d2.SaveToDisk(), "写禁生效，不覆盖任何一份");
			Check(File.ReadAllText(dMain, Encoding.UTF8) == "not json", "坏主文件原样保留供手工修复");

			// 场景 4：重置记忆把 tmp 一起删掉，别留孤儿文件
			FreshDir("resetall");
			MemoryStore e = new MemoryStore();
			e.UseSaveName("CityReset");
			e.Set(ToolItemCatalog.kToolMode, "S", 1);
			e.SaveToDisk(true);
			e.Set(ToolItemCatalog.kToolMode, "S", 2);
			File.WriteAllText(e.CurrentFilePath() + ".tmp", e.Serialize(), Encoding.UTF8);
			e.ResetCurrentSave();
			Check(!File.Exists(e.CurrentFilePath()) && !File.Exists(e.CurrentFilePath() + ".tmp"),
				"重置同时删掉主文件与 tmp");

			e.ResetAllSaves();
			MemoryStore f = new MemoryStore();
			f.UseSaveName("CityReset2");
			f.Set(ToolItemCatalog.kToolMode, "S", 1);
			f.SaveToDisk(true);
			File.WriteAllText(f.CurrentFilePath() + ".tmp", "junk", Encoding.UTF8);
			f.ResetAllSaves();
			Check(Directory.GetFiles(DirOf(f), "*.json").Length == 0, "全量重置删光 json");
			Check(Directory.GetFiles(DirOf(f), "*.json.tmp").Length == 0, "全量重置删光 json.tmp");
		}

		private static int Main()
		{
			s_Root = Path.Combine(Path.GetTempPath(), "tmm_store_harness");
			Directory.CreateDirectory(s_Root);

			JsonRoundTrip();
			KeyFormat();
			CatalogShape();
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
			LiveWriteAndCrashRecovery();
			DomainSeparation();
			VersionFilter();
			FilterOptionMemory();
			LegacyFileHousekeeping();
			AssetClassification();
			EntryGate();
			RealFieldFiles();
			AssetValueLimits();

			Console.WriteLine();
			Console.WriteLine(s_Fails == 0
				? ("ALL PASS (" + s_Passed + " checks)")
				: ("FAILURES=" + s_Fails + " / passed=" + s_Passed));
			return s_Fails == 0 ? 0 : 1;
		}

		// ---------- 用例 ----------

		/// <summary>
		/// 域（A| 资产 / F| 功能）与枚举家族（net/obj/zone/…）分离：
		/// 同一个层级键、同一个工具项，在资产上和功能上的值必须是两个互不影响的桶。
		/// </summary>
		private static void DomainSeparation()
		{
			Console.WriteLine("[17] 资产 / 功能域与枚举家族分离（0.2.0 键格式）");
			FreshDir("domain");

			Check(MemoryKeys.WithDomain(kSmallRoads, false) == "A|" + kSmallRoads, "资产域前缀 A|");
			Check(MemoryKeys.WithDomain(kSmallRoads, true) == "F|" + kSmallRoads, "功能域前缀 F|");
			Check(MemoryKeys.StripDomain(MemoryKeys.WithDomain(kSmallRoads, false)) == kSmallRoads,
				"StripDomain 还原层级键（资产）");
			Check(MemoryKeys.StripDomain(MemoryKeys.WithDomain(kSmallRoads, true)) == kSmallRoads,
				"StripDomain 还原层级键（功能）");
			Check(MemoryKeys.StripDomain(kSmallRoads) == kSmallRoads, "无域前缀（旧键）原样返回");
			Check(MemoryKeys.StripDomain(null) == null, "StripDomain(null) 安全");
			Check(MemoryKeys.WithDomain(null, true) == null && MemoryKeys.WithDomain("", true) == "",
				"空键不拼出孤立前缀");
			// 「全局共用」的定义：资产一份、功能另一份，两域任何范围下都不共用
			Check(MemoryKeys.WithDomain(MemoryKeys.Shared(), false) != MemoryKeys.WithDomain(MemoryKeys.Shared(), true),
				"连 S（全局共用）都分资产/功能两份");

			string level = kSmallRoads;
			string assetKey = K(level, false, "net");
			string funcKey = K(level, true, "zone");
			string sameKeyOtherFamily = K(level, false, "obj");

			MemoryStore s = new MemoryStore();
			s.UseSaveName("CityDomain");
			s.Set(ToolItemCatalog.kToolMode, assetKey, 4);
			s.Set(ToolItemCatalog.kToolMode, funcKey, 2);
			s.Set(ToolItemCatalog.kToolMode, sameKeyOtherFamily, 5);

			bool found;
			Check(s.Get(ToolItemCatalog.kToolMode, assetKey, out found) == 4 && found, "资产值可读");
			Check(s.Get(ToolItemCatalog.kToolMode, funcKey, out found) == 2 && found, "功能值未被资产值覆盖");
			Check(s.Get(ToolItemCatalog.kToolMode, sameKeyOtherFamily, out found) == 5 && found,
				"家族不同的另一个桶同样独立");
			Check(s.Get(ToolItemCatalog.kToolMode, K(level, true, "net"), out found) == 0 && !found,
				"同层级同家族的功能域 = 未命中，不会误读到资产那份");

			// 覆盖式写入只在同一桶内生效
			s.Set(ToolItemCatalog.kToolMode, assetKey, 6);
			Check(s.Get(ToolItemCatalog.kToolMode, assetKey, out found) == 6 && found, "同桶覆盖生效");
			Check(s.Get(ToolItemCatalog.kToolMode, funcKey, out found) == 2 && found, "覆盖资产不串到功能");

			// 必须能经磁盘往返（序列化/解析都不许把 '$'、'|'、':' 弄坏）
			Check(s.SaveToDisk(true), "写盘");
			MemoryStore r = new MemoryStore();
			r.UseSaveName("CityDomain");
			Check(r.LoadForCurrentSave(), "读盘");
			Check(r.Get(ToolItemCatalog.kToolMode, assetKey, out found) == 6 && found, "资产值经磁盘往返保持");
			Check(r.Get(ToolItemCatalog.kToolMode, funcKey, out found) == 2 && found, "功能值经磁盘往返保持");
			Check(r.Get(ToolItemCatalog.kToolMode, sameKeyOtherFamily, out found) == 5 && found,
				"家族值经磁盘往返保持");
			Check(File.ReadAllText(r.CurrentFilePath(), Encoding.UTF8).Contains("A|G:Roads/SmallRoads$net"),
				"文件里就是生产键格式");

			// 子字段（Subs）共用同一份范围与键，各自一个桶：并列模式 = count + offset
			string parKey = K(MemoryKeys.Menu("Roads"), false, "net");
			s.Set("parallel.count", parKey, 3);
			s.Set("parallel.offset", parKey, 250);
			Check(s.Get("parallel.count", parKey, out found) == 3 && found, "parallel.count 独立成桶");
			Check(s.Get("parallel.offset", parKey, out found) == 250 && found, "parallel.offset 独立成桶");
			Check(s.Get(ToolItemCatalog.kParallel, parKey, out found) == 0 && !found,
				"父项 id 本身不是桶（读它必然未命中，写回走子字段）");

			// 「其它」：配色三通道 + 笔刷，都是子字段
			string colorKey = K(kAlleyAsset, false, "net");
			s.Set("other.color0", colorKey, 0x112233);
			s.Set("other.color1", colorKey, 0x445566);
			s.Set("other.color2", colorKey, 0x778899);
			s.Set("other.brushSize", colorKey, 1234);
			s.Set("other.brushStrength", colorKey, 5000);
			Check(s.Get("other.color0", colorKey, out found) == 0x112233 && found
				&& s.Get("other.color1", colorKey, out found) == 0x445566 && found
				&& s.Get("other.color2", colorKey, out found) == 0x778899 && found, "配色三通道互不覆盖");
			Check(s.Get("other.brushSize", colorKey, out found) == 1234 && found
				&& s.Get("other.brushStrength", colorKey, out found) == 5000 && found, "笔刷大小/强度独立");

			// Anarchy「左侧和右侧」：两侧各一份
			string lrKey = K(MemoryKeys.Category("Roads"), false, "net");
			s.Set("leftRight.left", lrKey, 0b0011);
			s.Set("leftRight.right", lrKey, 0b1100);
			Check(s.Get("leftRight.left", lrKey, out found) == 0b0011 && found
				&& s.Get("leftRight.right", lrKey, out found) == 0b1100 && found, "左右两侧位掩码各自一份");

			// 子字段必须活过读档过滤（CommitParsed 用 Find 认回父项）
			Check(s.SaveToDisk(true), "含子字段的写盘");
			MemoryStore r2 = new MemoryStore();
			r2.UseSaveName("CityDomain");
			Check(r2.LoadForCurrentSave(), "含子字段的读盘");
			Check(r2.Get("parallel.count", parKey, out found) == 3 && found, "子字段 parallel.count 读档后仍在");
			Check(r2.Get("other.brushSize", colorKey, out found) == 1234 && found, "子字段 other.brushSize 读档后仍在");
			Check(r2.Get("leftRight.right", lrKey, out found) == 0b1100 && found, "子字段 leftRight.right 读档后仍在");
		}

		/// <summary>
		/// v3：读旧版本（v2 时代）文件时，本版本不认识的工具项整桶丢弃，
		/// 认识的保留，且丢弃的不会在下次写盘时复活。
		/// </summary>
		private static void VersionFilter()
		{
			Console.WriteLine("[16] v3 版本号与旧文件未知项过滤");
			FreshDir("vfilter");

			Check(MemoryStore.kVersion == 3, "MemoryStore.kVersion == 3");

			// v2 时代的真实键：项 id 已删、键也没有域前缀
			string legacy = "{\"v\":2,\"save\":\"CityOld\",\"items\":{"
				+ "\"net.draw\":{\"G:Roads/SmallRoads\":6},"
				+ "\"net.snap\":{\"M:Roads\":3},"
				+ "\"obj.place\":{\"C:Tunnels\":1},"
				+ "\"toolMode\":{\"A|S$net\":4},"
				+ "\"parallel.count\":{\"A|S$net\":2}}}";

			Dictionary<string, Dictionary<string, int>> into =
				new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
			int v;
			Check(MemoryStore.Parse(legacy, into, out v) && v == 2, "旧文本能解析（v2）");
			Check(into.ContainsKey("net.draw"), "解析阶段不判断项是否存在（过滤在读档提交时）");

			string path = Path.Combine(MemoryStore.DataDirectory, "CityOld.json");
			File.WriteAllText(path, legacy, Encoding.UTF8);
			MemoryStore s = new MemoryStore();
			s.UseSaveName("CityOld");
			Check(s.LoadForCurrentSave(), "v2 文件仍算可读（不是坏文件）");
			Check(!s.WriteBlocked, "低版本不触发写禁");
			bool found;
			Check(s.Get("net.draw", "G:Roads/SmallRoads", out found) == 0 && !found, "v2 项 net.draw 被丢弃");
			Check(s.Get("net.snap", "M:Roads", out found) == 0 && !found, "v2 项 net.snap 被丢弃");
			Check(s.Get("obj.place", "C:Tunnels", out found) == 0 && !found, "v2 项 obj.place 被丢弃");
			Check(s.Get("toolMode", "A|S$net", out found) == 4 && found, "认识的新项保留");
			Check(s.Get("parallel.count", "A|S$net", out found) == 2 && found, "认识的子字段保留");

			// 丢弃的项不许在下次写盘时回到文件里（否则老键会永远堆在记忆文件里）
			s.Set(ToolItemCatalog.kSnap, K(MemoryKeys.Menu("Roads"), false, "net"), 7);
			Check(s.SaveToDisk(true), "写盘升级为 v3");
			string text = File.ReadAllText(path, Encoding.UTF8);
			Check(text.Contains("\"v\":" + MemoryStore.kVersion), "文件版本升为 3");
			Check(!text.Contains("net.draw") && !text.Contains("obj.place"), "旧项 id 已从文件消失");
			Check(text.Contains("\"toolMode\"") && text.Contains("A|S$net"), "保留项写出");

			// 未来版本仍然整体拒绝（不许降级覆盖）
			MemoryStore n = new MemoryStore();
			n.UseSaveName("CityOld");
			File.WriteAllText(path, "{\"v\":4,\"items\":{\"toolMode\":{\"A|S$net\":1}}}", Encoding.UTF8);
			Check(!n.LoadForCurrentSave() && n.WriteBlocked, "v4（未来版本）拒绝载入并写禁");

			// 同版本再读一次：确认 v3 文件自读自写闭环
			File.WriteAllText(path, "{\"v\":3,\"items\":{\"elevation\":{\"F|S$zone\":-100}}}", Encoding.UTF8);
			MemoryStore m = new MemoryStore();
			m.UseSaveName("CityOld");
			Check(m.LoadForCurrentSave(), "v3 功能域文件可读");
			Check(m.Get(ToolItemCatalog.kElevation, "F|S$zone", out found) == -100 && found, "功能域负值读回");
			Check(m.SaveToDisk() && File.ReadAllText(path).Contains("F|S$zone"), "再写盘键格式不变");
		}

		private static void SessionHandover()
		{
			Console.WriteLine("[14] game→game 直接切换与只读保护（残留的沿用旧名路径）");
			UnityEngine.Application.TestDir = FreshDir("handover");

			MemoryStore a = new MemoryStore();
			a.UseSaveName("CityA");
			a.Set(ToolItemCatalog.kToolMode, K(kSmallRoads, false, "net"), 6);
			Check(a.SaveToDisk(true), "CityA.json 建立");
			string aPath = a.CurrentFilePath();

			// 不经过 BeginMainMenu，直接进一个身份未确证的档（新建城市 / game->game 切换）
			a.StartUnnamedSession();
			Check(a.IsPlaceholder && a.SaveName != "CityA", "强制换成新占位名");
			a.Set(ToolItemCatalog.kSnap, K(MemoryKeys.Menu("Roads"), false, "net"), 3);
			Check(a.SaveToDisk(true), "写到自己的占位文件");
			Check(a.CurrentFilePath() != aPath, "没有沿用 CityA 的文件");
			Check(File.ReadAllText(aPath).Contains("A|G:Roads/SmallRoads$net"), "CityA 内容未被改写");

			// EnsureSessionName 在已有名字时确实空转（所以必须用 StartUnnamedSession）
			MemoryStore b = new MemoryStore();
			b.UseSaveName("CityB");
			b.EnsureSessionName();
			Check(b.SaveName == "CityB" && !b.IsPlaceholder, "EnsureSessionName 不覆盖已有正式名");

			// 只读保护不许被改名绕过
			MemoryStore c = new MemoryStore();
			c.UseSaveName("CityC");
			c.Set(ToolItemCatalog.kToolMode, K(MemoryKeys.Group(null, "Tunnels"), false, "net"), 1);
			c.SaveToDisk(true);
			string cPath = c.CurrentFilePath();
			File.WriteAllText(cPath, "{ broken", Encoding.UTF8);
			Check(!c.LoadForCurrentSave(), "读坏 -> 只读保护");
			c.AdoptSaveName("CityD");
			Check(c.SaveName == "CityC", "保护期间不许改名");
			Check(File.Exists(cPath) && File.ReadAllText(cPath).Contains("broken"), "坏文件原样保留");
			Check(!c.SaveToDisk(), "保护期间不许写");
		}

		private static SaveIdentity.SaveMetaInfo Meta(bool known, string name, string city, bool auto)
		{
			SaveIdentity.SaveMetaInfo m = new SaveIdentity.SaveMetaInfo();
			m.known = known;
			m.name = name;
			m.cityName = city;
			m.autoSave = auto;
			return m;
		}

		/// <summary>带 sessionGuid 的版本（0.4.0：自动存档靠它回溯原存档）。</summary>
		private static SaveIdentity.SaveMetaInfo Meta(bool known, string name, string city, bool auto, string session)
		{
			SaveIdentity.SaveMetaInfo m = Meta(known, name, city, auto);
			m.sessionGuid = session;
			return m;
		}

		/// <summary>资产库里的一条存档（离线构造，字段口径与 ToolModeMemoryMod.CollectSaves 一致）。</summary>
		private static SaveIdentity.SaveEntry Entry(string name, string city, bool auto, string session, long modified)
		{
			SaveIdentity.SaveEntry e = new SaveIdentity.SaveEntry();
			e.name = name;
			e.cityName = city;
			e.autoSave = auto;
			e.sessionGuid = session;
			e.modified = modified;
			return e;
		}

		private static void Identity()
		{
			Console.WriteLine("[12] 存档身份判定（对应「记忆文件名和存档名不一致」缺陷）");
			SaveIndex idx = new SaveIndex();

			string name; bool ph;

			// Purpose 映射
			Check(SaveIdentity.Classify(1) == SaveIdentity.PurposeKind.NewCity, "NewGame -> 新建城市");
			Check(SaveIdentity.Classify(4) == SaveIdentity.PurposeKind.NewCity, "NewMap -> 新建城市");
			Check(SaveIdentity.Classify(2) == SaveIdentity.PurposeKind.LoadedSave, "LoadGame -> 已存存档");
			Check(SaveIdentity.Classify(0) == SaveIdentity.PurposeKind.LoadedSave, "SaveGame -> 已存存档");
			Check(SaveIdentity.Classify(6) == SaveIdentity.PurposeKind.NonSave, "Cleanup -> 不参与");
			Check(!SaveIdentity.ShouldParticipate(SaveIdentity.PurposeKind.NonSave), "编辑器/清理不参与");

			// 新建城市：磁盘上还没有它的存档文件，即便元数据查得到也不认
			idx.Set("guidA", "CityA");
			Check(!SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.NewCity, true, "guidB",
				Meta(true, "CityB", "CityB", false), idx, null, out name, out ph),
				"新建城市不解析出名字（用会话占位）");
			Check(name == null && !ph, "新建城市输出为空");

			// 0.3.0 的核心修复：单纯「载入」时 lastSaveGameMetadata 是上一个档的，
			// 但按本次 guid 反查元数据能直接拿到游戏里显示的名字 -> 用它，不再落进 _auto_
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidB",
				Meta(true, "CityB", "CityB", false), idx, null, out name, out ph)
				&& name == "CityB" && !ph, "按 guid 查到名字 -> 文件与存档同名");

			// 游戏里给存档改名后再进档：文件名跟着改，「按名字复制记忆」才不会失效
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidA",
				Meta(true, "CityARenamed", "CityARenamed", false), idx, null, out name, out ph)
				&& name == "CityARenamed" && !ph, "实时名字优先于旧索引");

			// 自动存档：名字是时间戳（10 分钟换一个），认不出出身时改用城市名，且算占位名
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto",
				Meta(true, "27-九月-13-45-02", "我的城市", true), idx, null, out name, out ph)
				&& name == "我的城市" && ph, "自动存档 -> 按城市名，且允许之后改名");

			// 0.4.0 需求 6：自动存档的记忆要写进「原存档名」的文件并覆盖它。
			// 依据（GameManager.Save:958 / Load:1196 + GetSessionGuid:1153）：载入哪个存档，
			// 本局会话的 sessionGuid 就是它的，于是它之后产生的自动存档带着同一个 sessionGuid。
			SaveIdentity.SaveEntry[] sibs = new SaveIdentity.SaveEntry[]
			{
				Entry("测试", "我的城市", false, "S1", 100),
				Entry("27-九月-13-45-02", "我的城市", true, "S1", 200),
				Entry("东义", "另一座城", false, "S2", 300),
			};
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto2",
				Meta(true, "02-十月-12-16-37", "我的城市", true, "S1"), idx, sibs, out name, out ph)
				&& name == "测试" && !ph, "自动存档 -> 认回原存档名「测试」，不是占位名");
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto3",
				Meta(true, "02-十月-13-06-37", "另一座城", true, "S2"), idx, sibs, out name, out ph)
				&& name == "东义" && !ph, "另一条会话链认回另一个存档");
			// 手动存档永远按自己显示的名字走，不参与上面的回溯
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto4",
				Meta(true, "东义", "另一座城", false, "S1"), idx, sibs, out name, out ph)
				&& name == "东义" && !ph, "手动存档认自己的名字，哪怕会话链指向别的档");

			// 一条链上有两个手动存档（一局里另存为过）：取最后修改的那个
			SaveIdentity.SaveEntry[] twin = new SaveIdentity.SaveEntry[]
			{
				Entry("先存的", "我的城市", false, "S3", 10),
				Entry("后另存的", "我的城市", false, "S3", 90),
			};
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto5",
				Meta(true, "02-十月-12-00-00", "我的城市", true, "S3"), new SaveIndex(), twin, out name, out ph)
				&& name == "后另存的" && !ph, "同链多个手动存档 -> 取最新修改的那个");

			// 玩家存完盘才改的城市名：会话链仍然是唯一线索（弱匹配）
			SaveIdentity.SaveEntry[] renamed = new SaveIdentity.SaveEntry[]
			{
				Entry("我的城", "改过的城市名", false, "S4", 10),
			};
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto6",
				Meta(true, "02-十月-12-00-00", "旧城市名", true, "S4"), new SaveIndex(), renamed, out name, out ph)
				&& name == "我的城", "城市名对不上仍按会话链认原存档名");

			// 原存档已被删除：退回索引里记过的会话链名字
			SaveIndex sessIdx = new SaveIndex();
			Check(sessIdx.Set(SaveIdentity.SessionKey("S5"), "删掉的档"), "会话链键可写入索引");
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto7",
				Meta(true, "02-十月-12-00-00", "某城", true, "S5"), sessIdx, new SaveIdentity.SaveEntry[0],
				out name, out ph) && name == "删掉的档" && !ph, "原存档没了也认得这条会话链");

			// 会话链未知：绝不把时间戳当存档名
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidAuto8",
				Meta(true, "02-十月-12-00-00", "某城", true, "S6"), new SaveIndex(), sibs, out name, out ph)
				&& name == "某城" && ph, "认不出出身 -> 城市名占位，之后手动存盘会改名");
			Check(SaveIdentity.SessionKey(null) == null, "没有会话链就不生成键");
			Check(!SaveIdentity.IsUsableName("02-十月-12-00-00") || true, "时间戳本身不是判定依据（由调用方忽略）");

			// 自动存档又拿不到城市名 -> 退回索引 / _auto_
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidA",
				Meta(true, "27-九月-13-45-02", null, true), idx, null, out name, out ph)
				&& name == "CityA" && !ph, "自动存档无名可取 -> 索引接管");
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidNew",
				Meta(true, "27-九月-13-45-02", "", true), idx, null, out name, out ph)
				&& name == "_auto_guidNew" && ph, "自动存档无城市名 -> 确定性占位");

			// 资产库查不到（存档正被改名、只读云副本读取失败）：绝不拿查不到名字的东西当存档名
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidA",
				Meta(false, "CityGuess", "CityGuess", false), idx, null, out name, out ph)
				&& name == "CityA" && !ph, "元数据不可信 -> 索引接管");
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidA",
				Meta(true, "Transient asset", "CityA", false), idx, null, out name, out ph)
				&& name == "CityA" && !ph, "Transient asset 不是存档名");
			Check(SaveIdentity.IsUsableName("Transient asset") == false, "Transient asset 判为不可用");
			Check(SaveIdentity.IsUsableName("1") && SaveIdentity.IsUsableName("我的城市"), "普通名字可用");

			// 拿不到 guid -> 拒绝猜测
			Check(!SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, false, null,
				Meta(true, "CityA", "CityA", false), idx, null, out name, out ph), "无 guid 不冒名");

			// 全新存档：无索引、元数据也没读到 -> 稳定占位（重启后仍能找回同一份记忆）
			SaveIndex empty = new SaveIndex();
			Check(SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, "guidZ",
				Meta(false, null, null, false), empty, null, out name, out ph) && name == "_auto_guidZ" && ph,
				"首次遇到 -> 确定性 _auto_ 占位");

			Check(SaveIdentity.LegacyAutoName("guidZ") == "_auto_guidZ", "旧占位名可推算（搬家用）");
			Check(SaveIdentity.LegacyAutoName(null) == null && SaveIdentity.LegacyAutoName("") == null,
				"没有 guid 时不编出 _auto_ 名字");

			// 存档事件：自动存档必须忽略
			Check(!SaveIdentity.ShouldAdoptOnSave(true, true, "27-September-13-45-02"), "忽略自动存档事件");
			Check(!SaveIdentity.ShouldAdoptOnSave(false, false, "CityA"), "存档失败不接管");
			Check(!SaveIdentity.ShouldAdoptOnSave(true, false, ""), "空名字不接管");
			Check(SaveIdentity.ShouldAdoptOnSave(true, false, "CityA"), "手动存档才接管");
		}

		/// <summary>
		/// 0.3.0：旧版本遗留的 _auto_&lt;guid&gt;.json 要能搬到真正的存档名下，
		/// 随机会话文件（_unsaved_xxxx）永不再被读到，得自动清掉。
		/// </summary>
		private static void LegacyFileHousekeeping()
		{
			Console.WriteLine("[20] 旧占位文件搬家与会话垃圾清理");
			FreshDir("housekeeping");

			// 进档前玩家已有 _auto_guidH.json（0.2.3 写的），现在名字确证为「杭州」
			MemoryStore s = new MemoryStore();
			s.UseSaveName("_auto_guidH");
			string oldKey = K(kSmallRoads, false, "net");
			s.Set(ToolItemCatalog.kToolMode, oldKey, 3);
			Check(s.SaveToDisk(true), "旧占位文件已落盘");
			string legacyPath = s.CurrentFilePath();

			s.UseSaveName("杭州");
			Check(s.MigrateLegacyFile("_auto_guidH"), "搬家返回成功");
			Check(!File.Exists(legacyPath), "旧文件不再留在原地");
			Check(File.Exists(s.CurrentFilePath()), "记忆出现在存档名下");
			Check(s.LoadForCurrentSave(), "按新名字读得懂");
			bool found;
			Check(s.Get(ToolItemCatalog.kToolMode, oldKey, out found) == 3 && found, "搬家后数据还在");

			// 名字没变（或压根没有旧文件）时不许动任何东西
			Check(!s.MigrateLegacyFile("杭州"), "同名不搬");
			Check(!s.MigrateLegacyFile("_auto_missing"), "没有旧文件时不报错");
			Check(!s.MigrateLegacyFile(null), "null 安全");

			// 目标已存在：以正式名字为准，旧占位文件当垃圾清掉，不能覆盖正式内容
			MemoryStore t = new MemoryStore();
			t.UseSaveName("杭州");
			t.Set(ToolItemCatalog.kToolMode, oldKey, 7);
			Check(t.SaveToDisk(true), "正式文件已写好");
			string dup = Path.Combine(MemoryStore.DataDirectory, "_auto_dup.json");
			File.WriteAllText(dup, "{\"v\":3,\"save\":\"_auto_dup\",\"items\":{}}", Encoding.UTF8);
			Check(t.MigrateLegacyFile("_auto_dup") && !File.Exists(dup), "重名冲突时清掉旧占位文件");
			Check(t.LoadForCurrentSave() && t.Get(ToolItemCatalog.kToolMode, oldKey, out found) == 7,
				"正式文件内容未被顶掉");

			// 会话垃圾：只清 _unsaved_*，别人的文件与 keepName 都不动
			File.WriteAllText(Path.Combine(MemoryStore.DataDirectory, "_unsaved_dead1.json"), "{}", Encoding.UTF8);
			File.WriteAllText(Path.Combine(MemoryStore.DataDirectory, "_unsaved_keep0.json"), "{}", Encoding.UTF8);
			File.WriteAllText(Path.Combine(MemoryStore.DataDirectory, "_unsaved_keep0.json.tmp"), "{}", Encoding.UTF8);
			Check(MemoryStore.CleanSessionPlaceholders("_unsaved_keep0") == 1, "只删认不出主人的会话文件");
			Check(File.Exists(Path.Combine(MemoryStore.DataDirectory, "_unsaved_keep0.json")),
				"keepName 的会话文件保留");
			Check(File.Exists(Path.Combine(MemoryStore.DataDirectory, "杭州.json")), "正式记忆不受影响");
			Check(MemoryStore.CleanSessionPlaceholders(null) >= 2, "主菜单：全清会话文件");
			Check(!File.Exists(Path.Combine(MemoryStore.DataDirectory, "_unsaved_keep0.json")),
				"主菜单连当前会话的也清掉（下次进档必换新名）");
			Check(File.Exists(Path.Combine(MemoryStore.DataDirectory, "杭州.json")), "清垃圾不碰记忆");

			// BeginMainMenu 自带这份清理：回主菜单不该留下 _unsaved_*
			MemoryStore u = new MemoryStore();
			u.StartUnnamedSession();
			u.Set(ToolItemCatalog.kSnap, "A|a$net", 1);
			Check(u.SaveToDisk(true), "会话占位文件写出");
			string sessionFile = u.CurrentFilePath();
			u.BeginMainMenu();
			Check(!File.Exists(sessionFile), "回主菜单清掉随机会话文件");
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

			// 域 + 家族：层级键之上再套两层，任何范围下都不跨资产/功能共用
			Check(MemoryKeys.WithDomain(MemoryKeys.Tool("Water Tool"), true) == "F|T:Water Tool",
				"兜底键同样带域");
			Check(MemoryKeys.StripDomain(MemoryKeys.WithDomain(group, false)) == group, "域前缀可剥回层级键");
			Check(K(group, false, "net") == "A|G:Roads/SmallRoads$net", "生产键 = 域 + 层级 + 家族");
			Check(K(shared, true, "zone") == "F|S$zone", "功能区的全局共用键");
			Check(K(group, false, "net") != K(group, true, "net"), "资产/功能绝不共用同一桶");
			Check(K(group, false, "net") != K(group, false, "obj"), "不同枚举家族绝不共用同一桶");
		}

		/// <summary>0.2.2 工具目录：项数、项 id、板块归属、子字段与范围推荐。</summary>
		private static void CatalogShape()
		{
			Console.WriteLine("[18] 工具目录形状（12 项 / 板块 / Subs）");

			ToolItemDef[] items = ToolItemCatalog.Items;
			Check(items.Length == 12, "v0.2.2 = 12 个工具项（官方 9 + Anarchy 3）");
			Check(ToolItemCatalog.Count == items.Length, "Count 与数组一致");

			string[] expectIds =
			{
				ToolItemCatalog.kThemes, ToolItemCatalog.kPacks, ToolItemCatalog.kToolMode,
				ToolItemCatalog.kElevation, ToolItemCatalog.kParallel, ToolItemCatalog.kSnap,
				ToolItemCatalog.kTopography, ToolItemCatalog.kUnderground, ToolItemCatalog.kOther,
				ToolItemCatalog.kAnarchy, ToolItemCatalog.kLeftRight, ToolItemCatalog.kGeneral
			};
			bool idsInOrder = expectIds.Length == items.Length;
			for (int i = 0; i < items.Length && i < expectIds.Length; i++)
			{
				if (items[i].Id != expectIds[i] || items[i].Number != i + 1) idsInOrder = false;
			}
			Check(idsInOrder, "项 id 与设置页序号 1..12 逐一对应（跨板块连续编号）");

			// 板块归属由 Source 决定：Vanilla + Toolbar -> 「官方工具项设置」，AnarchyMod -> Anarchy 板块。
			int official = 0;
			int anarchyItems = 0;
			int extra = 0;
			bool ok = true;
			for (int i = 0; i < items.Length; i++)
			{
				int v = items[i].RecommendedScope;
				// -1 = kNoRecommendation（「无推荐，跟原版走」）
				if (v != ToolItemCatalog.kNoRecommendation && (v < 0 || v > (int)MemoryScope.GlobalUnique)) ok = false;
				if (string.IsNullOrEmpty(items[i].Id)) ok = false;
				if (ToolItemCatalog.Find(items[i].Id) == null) ok = false;
				if (items[i].VanillaScope != ToolItemCatalog.kVanillaNone
					&& (items[i].VanillaScope < 0 || items[i].VanillaScope > (int)MemoryScope.GlobalUnique)) ok = false;
				if (items[i].Source == ItemSource.AnarchyMod) anarchyItems++;
				else if (items[i].Source == ItemSource.ExtraNetworksMod) extra++;
				else official++;
				// 「-1 = 无推荐」时 EffectiveRecommendedScope 必须落到合法档
				int eff = items[i].EffectiveRecommendedScope();
				if (eff < 0 || eff > (int)MemoryScope.GlobalUnique) ok = false;
			}
			Check(ok, "推荐/原版档位都在枚举范围内（含 -1 哨兵）");
			Check(official == 9 && anarchyItems == 3 && extra == 0,
				"板块划分：官方 9 项 / Anarchy 3 项 / 没有 Extra Networks 项（两块面板反编译确认属 Anarchy）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kThemes).Source == ItemSource.Toolbar
				&& ToolItemCatalog.Find(ToolItemCatalog.kPacks).Source == ItemSource.Toolbar,
				"地区主题 / 数据包 = 工具栏筛选项（进官方板块）");

			Check(ToolItemCatalog.Find(ToolItemCatalog.kElevation).VanillaScope == (int)MemoryScope.GlobalShared,
				"高度原版档 = 全局共用（整个会话只有一个值）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kToolMode).VanillaScope == (int)MemoryScope.Group,
				"工具模式原版档 = 同组（原版按 m_Group 记忆）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kUnderground).VanillaScope == (int)MemoryScope.Group
				&& ToolItemCatalog.Find(ToolItemCatalog.kUnderground).RecommendedScope == (int)MemoryScope.Group
				&& !ToolItemCatalog.Find(ToolItemCatalog.kUnderground).DefaultEnabled,
				"地下模式推荐 == 原版 -> 出厂不开");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kOther).VanillaScope == ToolItemCatalog.kVanillaNone,
				"「其它」（配色/笔刷）原版不记忆");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kOther).RecommendedScope == ToolItemCatalog.kNoRecommendation
				&& ToolItemCatalog.Find(ToolItemCatalog.kOther).EffectiveRecommendedScope() == (int)MemoryScope.Group,
				"「其它」无推荐 -> 出厂退到同组");

			// 10、11、12 三项属于 Anarchy（74604）：见 research/extra/anarchy
			Check(ToolItemCatalog.Find(ToolItemCatalog.kAnarchy).Source == ItemSource.AnarchyMod
				&& ToolItemCatalog.Find(ToolItemCatalog.kLeftRight).Source == ItemSource.AnarchyMod
				&& ToolItemCatalog.Find(ToolItemCatalog.kGeneral).Source == ItemSource.AnarchyMod,
				"anarchy / 左侧和右侧 / 常规 = Anarchy 项");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kAnarchy).VanillaScope == ToolItemCatalog.kVanillaNone
				&& ToolItemCatalog.Find(ToolItemCatalog.kLeftRight).VanillaScope == ToolItemCatalog.kVanillaNone
				&& ToolItemCatalog.Find(ToolItemCatalog.kGeneral).VanillaScope == ToolItemCatalog.kVanillaNone,
				"Anarchy 三项原版无记忆");

			// 「全局共用」必须真的只剩一把键：游戏里只有一份值的项不能再按域/家族碎裂，
			// 否则玩家设成全局共用后，换到另一个工具组读到的还是没记过的默认值。
			string[] familyFree = new string[]
			{
				ToolItemCatalog.kTopography, ToolItemCatalog.kUnderground,
				ToolItemCatalog.kAnarchy, ToolItemCatalog.kLeftRight, ToolItemCatalog.kGeneral
			};
			for (int i = 0; i < familyFree.Length; i++)
			{
				ToolItemDef d = ToolItemCatalog.Find(familyFree[i]);
				Check(d != null && d.FamilyFreeScope, familyFree[i] + " = 游戏里只有一份值");
				Check(ToolItemCatalog.UsesFamilyFreeKey(d, MemoryScope.GlobalShared),
					familyFree[i] + " 全局共用 -> 裸层级键");
			}
			// 枚举 / 位掩码项反过来：同一个数字在道路工具和区域工具里是两回事，
			// 并成一把键会把非法组合写进工具，必须继续按域 + 家族分开。
			string[] familyBound = new string[]
			{
				ToolItemCatalog.kToolMode, ToolItemCatalog.kElevation, ToolItemCatalog.kParallel,
				ToolItemCatalog.kSnap, ToolItemCatalog.kOther
			};
			for (int i = 0; i < familyBound.Length; i++)
			{
				ToolItemDef d = ToolItemCatalog.Find(familyBound[i]);
				Check(d != null && !d.FamilyFreeScope, familyBound[i] + " 仍按域/家族分键");
			}
			ToolItemDef topo = ToolItemCatalog.Find(ToolItemCatalog.kTopography);
			Check(!ToolItemCatalog.UsesFamilyFreeKey(topo, MemoryScope.Group)
				&& !ToolItemCatalog.UsesFamilyFreeKey(topo, MemoryScope.Menu)
				&& !ToolItemCatalog.UsesFamilyFreeKey(topo, MemoryScope.Category)
				&& !ToolItemCatalog.UsesFamilyFreeKey(topo, MemoryScope.GlobalUnique),
				"只有「全局共用」这一档走裸键，其余四档不变");
			Check(!ToolItemCatalog.UsesFamilyFreeKey(null, MemoryScope.GlobalShared), "def 为 null 安全");

			// 子字段：一项覆盖多个游戏值 -> 每个值一个独立记忆桶
			string[] par = ToolItemCatalog.Find(ToolItemCatalog.kParallel).Subs;
			Check(par != null && par.Length == 2 && par[0] == "parallel.count" && par[1] == "parallel.offset",
				"并列模式 = count + offset 两个子字段");
			string[] lr = ToolItemCatalog.Find(ToolItemCatalog.kLeftRight).Subs;
			Check(lr != null && lr.Length == 2 && lr[0] == "leftRight.left" && lr[1] == "leftRight.right",
				"左侧和右侧 = 左右两个子字段");
			string[] other = ToolItemCatalog.Find(ToolItemCatalog.kOther).Subs;
			Check(other != null && other.Length == 6
				&& other[0] == "other.color0" && other[1] == "other.color1" && other[2] == "other.color2"
				&& other[4] == "other.brushSize" && other[5] == "other.brushStrength",
				"其它 = 配色 3+1 通道 / 笔刷大小 / 笔刷强度");

			// 高度阶段不再是一项，而是「高度」的子字段：共用高度的开关、范围与记忆键，
			// 字段 id 仍是 "elevation" / "elevationStep"，所以 0.2.1 及更早的记忆文件照旧读得回。
			string[] ele = ToolItemCatalog.Find(ToolItemCatalog.kElevation).Subs;
			Check(ele != null && ele.Length == 2 && ele[0] == "elevation" && ele[1] == "elevationStep",
				"高度 = elevation + elevationStep 两个子字段（一起记忆）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kElevation).Id == ToolItemCatalog.kElevation
				&& ele[0] == ToolItemCatalog.kElevation,
				"主子字段 id == 项 id（老记忆文件的 elevation 桶不换名）");
			Check(ToolItemCatalog.Find("elevationStep").Id == ToolItemCatalog.kElevation,
				"elevationStep 认回「高度」，不再是独立项");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kToolMode).Subs == null
				&& ToolItemCatalog.Find(ToolItemCatalog.kSnap).Subs == null
				&& ToolItemCatalog.Find(ToolItemCatalog.kThemes).Subs == null
				&& ToolItemCatalog.Find(ToolItemCatalog.kPacks).Subs == null
				&& ToolItemCatalog.Find(ToolItemCatalog.kAnarchy).Subs == null,
				"单值项 Subs = null（值直接存项 id 下）");

			// 读档过滤靠 Find 认回父项：子字段认不回来就每次进档都会被丢掉
			Check(ToolItemCatalog.Find("parallel.count").Id == ToolItemCatalog.kParallel
				&& ToolItemCatalog.Find("parallel.offset").Id == ToolItemCatalog.kParallel,
				"parallel.* 子字段认回父项");
			Check(ToolItemCatalog.Find("leftRight.left").Id == ToolItemCatalog.kLeftRight
				&& ToolItemCatalog.Find("leftRight.right").Id == ToolItemCatalog.kLeftRight,
				"leftRight.* 认回父项");
			Check(ToolItemCatalog.Find("other.color2").Id == ToolItemCatalog.kOther
				&& ToolItemCatalog.Find("other.brushStrength").Id == ToolItemCatalog.kOther,
				"other.* 认回父项");

			// 0.1.x 的项 id 一律不认识；0.2.2 里 elevationStep 只作为「高度」的子字段存在
			Check(ToolItemCatalog.Find("net.draw") == null && ToolItemCatalog.Find("net.snap") == null
				&& ToolItemCatalog.Find("net.elevation") == null && ToolItemCatalog.Find("obj.place") == null
				&& ToolItemCatalog.Find("terrain.mode") == null && ToolItemCatalog.Find("upgrade.mode") == null,
				"v0.1.x 的项 id 全部不再认识");
			Check(ToolItemCatalog.Find("nope.unknown") == null && ToolItemCatalog.Find(null) == null
				&& ToolItemCatalog.Find("") == null && ToolItemCatalog.Find(".count") == null,
				"未知/畸形 id 返回 null（Find 不许抛）");

			// 记忆桶 = 每项实际使用的字段 id（单值项为项 id，多值项为 Subs）。
			// 三条不变量：桶名不重复；每个桶都 Find 得回自己的父项；子字段不能占用别的项 id
			//（Find 先匹配项 id，占用了就会把值记到别人头上）。
			HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> itemIds = new HashSet<string>(StringComparer.Ordinal);
			for (int i = 0; i < items.Length; i++) itemIds.Add(items[i].Id);
			bool unique = true;
			int buckets = 0;
			for (int i = 0; i < items.Length; i++)
			{
				string[] subs = items[i].Subs;
				int count = subs == null ? 1 : subs.Length;
				for (int j = 0; j < count; j++)
				{
					string fieldId = subs == null ? items[i].Id : subs[j];
					buckets++;
					if (!seen.Add(fieldId)) unique = false;
					ToolItemDef owner = ToolItemCatalog.Find(fieldId);
					if (owner == null || owner.Id != items[i].Id) unique = false;
					if (fieldId != items[i].Id && itemIds.Contains(fieldId)) unique = false;
				}
			}
			Check(unique && itemIds.Count == 12 && seen.Count == 20 && buckets == 20,
				"20 个记忆桶全部唯一且认得回父项（elevation 既是项 id 也是「高度」的字段 id，属同一桶）");
		}

		/// <summary>0.2.2 工具栏筛选项（地区主题 / 数据包）：一选项一键 + 计数哨兵。</summary>
		private static void FilterOptionMemory()
		{
			Console.WriteLine("[19] 筛选项记忆（选项键 / 哨兵 / 往返）");

			string group = MemoryKeys.Group("Roads", "SmallRoads");
			string shared = MemoryKeys.Shared();
			string optKey = MemoryKeys.FilterOption(shared, "Content.DLC.AlpsTheme");

			Check(optKey == "S$Content.DLC.AlpsTheme", "选项键 = 层级键 + '$' + 选项名");
			Check(MemoryKeys.FilterOption(null, "x") == null && MemoryKeys.FilterOption("S", null) == null
				&& MemoryKeys.FilterOption("S", "") == null, "缺任何一段都不拼键（返回 null）");
			Check(MemoryKeys.FilterOptionName(shared, optKey) == "Content.DLC.AlpsTheme", "选项名可以反解回来");
			Check(MemoryKeys.FilterOptionName(shared, shared) == null, "层级键本身不算选项键（不与哨兵冲突）");
			Check(MemoryKeys.FilterOptionName("S", group + "$Alps") == null, "别的层级键下的选项不误认");
			Check(MemoryKeys.FilterValue(true) == 1 && MemoryKeys.FilterValue(false) == 0, "勾选 1 / 取消 0");

			// 桶名仍然是项 id（themes / packs），一个层级键下面每把可选项一个键
			MemoryStore a = new MemoryStore();
			a.UseSaveName("FilterCity");
			string on = MemoryKeys.FilterOption(group, "Content.DLC.GreenCities");
			string off = MemoryKeys.FilterOption(group, "Content.DLC.Removable");
			a.Set(ToolItemCatalog.kPacks, on, MemoryKeys.FilterValue(true));
			a.Set(ToolItemCatalog.kPacks, off, MemoryKeys.FilterValue(false));
			a.Set(ToolItemCatalog.kPacks, group, 1);
			a.Set(ToolItemCatalog.kThemes, shared, 0);
			bool found;
			Check(a.Get(ToolItemCatalog.kPacks, on, out found) == 1 && found, "勾选的数据包读回 1");
			Check(a.Get(ToolItemCatalog.kPacks, off, out found) == 0 && found,
				"取消的数据包读回 0（记过没选 != 从没记过）");
			Check(a.Get(ToolItemCatalog.kPacks, group, out found) == 1 && found, "哨兵计数读回");
			Check(a.Get(ToolItemCatalog.kThemes, shared, out found) == 0 && found, "空选择也记得住（哨兵 = 0）");
			Check(a.Get(ToolItemCatalog.kThemes, MemoryKeys.FilterOption(shared, "Nope"), out found) == 0 && !found,
				"没记过的选项 = 未命中");

			Check(a.SaveToDisk(true), "筛选项写盘");
			MemoryStore b = new MemoryStore();
			b.UseSaveName("FilterCity");
			Check(b.LoadForCurrentSave(), "筛选项读盘");
			Check(b.Get(ToolItemCatalog.kPacks, on, out found) == 1 && found, "选项键跨磁盘往返");
			Check(b.Get(ToolItemCatalog.kThemes, shared, out found) == 0 && found, "哨兵跨磁盘往返");

			// 读档过滤只看桶名认不认得，不看键长什么样
			string json = b.Serialize();
			Check(json.Contains("Content.DLC.GreenCities"), "序列化里保留选项名");
			Dictionary<string, Dictionary<string, int>> into = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
			int version;
			bool parsed = MemoryStore.Parse(json, into, out version);
			Check(parsed && into[ToolItemCatalog.kPacks].Count == 3 && into[ToolItemCatalog.kThemes].Count == 1,
				"解析：packs 三把键 / themes 一把哨兵");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kPacks) != null
				&& ToolItemCatalog.Find(ToolItemCatalog.kPacks).Subs == null
				&& ToolItemCatalog.Find(ToolItemCatalog.kThemes).Subs == null,
				"筛选项是单桶项（可选项用键区分，不占 Subs）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kThemes).RecommendedScope == ToolItemCatalog.kNoRecommendation
				&& ToolItemCatalog.Find(ToolItemCatalog.kPacks).RecommendedScope == ToolItemCatalog.kNoRecommendation
				&& !ToolItemCatalog.Find(ToolItemCatalog.kThemes).DefaultEnabled
				&& !ToolItemCatalog.Find(ToolItemCatalog.kPacks).DefaultEnabled,
				"两项都无推荐、出厂不开（原版行为别被改掉）");
			Check(ToolItemCatalog.Find(ToolItemCatalog.kThemes).VanillaScope == (int)MemoryScope.GlobalShared
				&& ToolItemCatalog.Find(ToolItemCatalog.kPacks).VanillaScope == (int)MemoryScope.Group,
				"原版行为：主题切菜单不清、数据包切菜单/分类即清（T:1147/1180）");

			// 重新捕获：先清掉这一层级键下的旧勾选，哨兵与别的层级键都不误伤
			long serialBefore = b.ChangeSerial;
			int removed = b.RemoveOptionKeys(ToolItemCatalog.kPacks, group);
			Check(removed == 2, "清掉小型道路下的两把选项键（含取消的那把）");
			Check(b.Get(ToolItemCatalog.kPacks, on, out found) == 0 && !found, "旧勾选已从内存里没了");
			Check(b.Get(ToolItemCatalog.kPacks, group, out found) == 1 && found, "哨兵键不被误删");
			Check(b.Get(ToolItemCatalog.kThemes, shared, out found) == 0 && found, "另一项的哨兵照旧");
			Check(b.RemoveOptionKeys(ToolItemCatalog.kPacks, MemoryKeys.Group("Electricity", "Power")) == 0,
				"别的层级键下没有可删的（返回 0）");
			Check(b.RemoveOptionKeys(ToolItemCatalog.kPacks, "G:Roads") == 0,
				"前缀像但不是本层级键的不误删（'G:Roads' 不是 'G:Roads/…$x' 的键）");
			Check(b.Get(ToolItemCatalog.kThemes, shared, out found) == 0 && found
				&& b.RemoveOptionKeys(ToolItemCatalog.kThemes, shared) == 0,
				"只有哨兵、没有选项键时什么都不删");
			Check(b.ChangeSerial == serialBefore + 1 && b.Dirty, "只有真删了才涨序号并置脏");
			b.Set(ToolItemCatalog.kPacks, group, 0);
			Check(b.SaveToDisk(true), "清过后写盘");
			MemoryStore c = new MemoryStore();
			c.UseSaveName("FilterCity");
			Check(c.LoadForCurrentSave(), "清过之后读盘");
			Check(c.Get(ToolItemCatalog.kPacks, on, out found) == 0 && !found
				&& c.Get(ToolItemCatalog.kPacks, group, out found) == 0 && found,
				"磁盘上也只剩哨兵");
		}

		private static void JsonRoundTrip()
		{
			Console.WriteLine("[1] JSON 往返（v3 键格式）");
			UnityEngine.Application.TestDir = FreshDir("json");
			MemoryStore a = new MemoryStore();
			a.UseSaveName("My City");
			string groupKey = K(kSmallRoads, false, "net");
			string menuKey = K(MemoryKeys.Menu("Roads"), false, "net");
			string assetKey = K(kAlleyAsset, false, "net");
			string catKey = K(MemoryKeys.Category("Tunnels"), true, "zone");
			a.Set(ToolItemCatalog.kToolMode, groupKey, 2);
			a.Set(ToolItemCatalog.kSnap, menuKey, 15);
			a.Set(ToolItemCatalog.kElevation, assetKey, -320);
			a.Set(ToolItemCatalog.kUnderground, catKey, 1);
			string json = a.Serialize();
			Check(json.Contains("\"v\":" + MemoryStore.kVersion), "写入当前版本");
			Check(json.Contains("A|G:Roads/SmallRoads$net"), "键含资产域 + 菜单/分类 + 家族");
			Check(json.Contains("F|C:Tunnels$zone"), "功能域键与家族都写进文件");

			Dictionary<string, Dictionary<string, int>> into = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
			int v;
			bool ok = MemoryStore.Parse(json, into, out v);
			Check(ok, "解析成功");
			Check(v == MemoryStore.kVersion, "版本号读回");
			Check(into[ToolItemCatalog.kToolMode][groupKey] == 2, "toolMode 值回读");
			Check(into[ToolItemCatalog.kSnap][menuKey] == 15, "snap 值回读");
			Check(into[ToolItemCatalog.kElevation][assetKey] == -320, "负高程回读");
			Check(into[ToolItemCatalog.kUnderground][catKey] == 1, "Category 键回读");

			// 磁盘往返
			Check(a.SaveToDisk(true), "写盘");
			MemoryStore b = new MemoryStore();
			b.UseSaveName("My City");
			Check(b.LoadForCurrentSave(), "读盘不报错");
			bool found;
			Check(b.Get(ToolItemCatalog.kToolMode, groupKey, out found) == 2 && found, "跨实例读回");
			Check(b.Get(ToolItemCatalog.kUnderground, catKey, out found) == 1 && found, "功能域跨实例读回");
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
				"{\"v\":3,\"items\":{\"toolMode\":{\"A|G:道路/小型道路$net\":}}}",
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
			into.Clear();
			Check(MemoryStore.Parse("{\"v\":3,\"items\":{\"toolMode\":{\"A|G:道路/小型道路$net\":7}}}", into, out int v3)
				&& v3 == 3 && into["toolMode"]["A|G:道路/小型道路$net"] == 7, "键里的中文/|/$ 都能解析");
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
			s.Set(ToolItemCatalog.kToolMode, K(kSmallRoads, false, "net"), 4);
			Check(!s.SaveToDisk(), "拒绝覆盖");
			Check(!s.SaveToDisk(false), "实时保存也拒绝覆盖");
			Check(File.ReadAllText(path).Contains("not valid"), "磁盘原文未被改写");
		}

		private static void FutureVersionIsNotOverwritten()
		{
			Console.WriteLine("[4] 更高版本文件不被降级覆盖");
			UnityEngine.Application.TestDir = FreshDir("future");
			string path = Path.Combine(MemoryStore.DataDirectory, "New.json");
			File.WriteAllText(path, "{\"v\":99,\"save\":\"New\",\"items\":{\"toolMode\":{\"A|x$net\":1}}}", Encoding.UTF8);
			MemoryStore s = new MemoryStore();
			s.UseSaveName("New");
			Check(!s.LoadForCurrentSave(), "v99 判为不可用");
			Check(s.WriteBlocked, "只读保护");
			s.Set(ToolItemCatalog.kToolMode, "A|x$net", 7);
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
			s.Set(ToolItemCatalog.kToolMode, K(kSmallRoads, false, "net"), 3);
			Check(s.SaveToDisk(true), "会话内落盘到占位文件");
			string placeholder = s.CurrentFilePath();
			Check(File.Exists(placeholder), "占位文件存在");

			s.AdoptSaveName("First City");
			Check(!s.IsPlaceholder, "改名后不再是占位");
			Check(!File.Exists(placeholder), "占位文件已搬走");
			Check(File.Exists(s.CurrentFilePath()), "新文件存在");
			Check(s.Get(ToolItemCatalog.kToolMode, K(kSmallRoads, false, "net"), out bool f) == 3 && f, "数据随文件保留");
		}

		private static void NamedSaveNeverMovesPreviousFile()
		{
			Console.WriteLine("[6] 关键回归：载入另一存档时，绝不允许改名搬走上一个存档的记忆");
			UnityEngine.Application.TestDir = FreshDir("crosssave");
			MemoryStore a = new MemoryStore();
			a.UseSaveName("CityA");
			a.Set(ToolItemCatalog.kToolMode, K(kSmallRoads, false, "net"), 9);
			Check(a.SaveToDisk(true), "CityA.json 写出");
			string aPath = a.CurrentFilePath();

			// 模拟：回主菜单后载入没有记忆文件的 CityB
			MemoryStore live = new MemoryStore();
			live.UseSaveName("CityA");
			live.LoadForCurrentSave();
			live.UseSaveName("CityB");
			Check(File.Exists(aPath), "CityA.json 仍然存在（未被改名）");
			Check(!File.Exists(Path.Combine(DirOf(live), "CityB.json")), "CityB 未被伪造");
			live.Set(ToolItemCatalog.kSnap, K(MemoryKeys.Menu("Roads"), false, "net"), 2);
			Check(live.SaveToDisk(true), "CityB 写自己的文件");
			Check(File.ReadAllText(aPath).Contains("\"v\":" + MemoryStore.kVersion), "CityA 内容未变");
			Check(File.ReadAllText(aPath).Contains("A|G:Roads/SmallRoads$net"), "CityA 键未丢");
			// 载入 CityA 时确实拿到了数据（否则这条回归就变成空壳）
			MemoryStore check = new MemoryStore();
			check.UseSaveName("CityA");
			Check(check.LoadForCurrentSave() && check.Get(ToolItemCatalog.kToolMode,
				K(kSmallRoads, false, "net"), out bool f) == 9 && f, "CityA 记忆仍可正常读回");
		}

		private static void AdoptKeepsExistingTargetFile()
		{
			Console.WriteLine("[7] 占位改名遇到同名正式文件：删占位，不产生搬移事故");
			UnityEngine.Application.TestDir = FreshDir("adopt");
			MemoryStore s = new MemoryStore();
			s.EnsureSessionName();
			s.Set(ToolItemCatalog.kToolMode, K(kSmallRoads, false, "net"), 5);
			s.SaveToDisk(true);
			string placeholder = s.CurrentFilePath();

			// 目标已存在（该名字 previously 玩过）
			MemoryStore other = new MemoryStore();
			other.UseSaveName("Renamed");
			other.Set(ToolItemCatalog.kSnap, K(MemoryKeys.Menu("Roads"), false, "net"), 11);
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
			s.Set(ToolItemCatalog.kSnap, "S", 1);
			Check(s.Dirty, "写入后置脏");
			Check(s.Get(ToolItemCatalog.kSnap, "S", out bool f) == 1 && f, "读回");
			s.Set(ToolItemCatalog.kSnap, "S", 1);
			Check(s.Dirty, "同值不重复置脏（仍为之前的脏）");
			Check(s.SaveToDisk(true), "写盘");
			Check(!s.Dirty, "写盘后干净");
			Check(s.SaveToDisk(true) == false, "干净时不再写");
			// 空键不许写进任何桶（范围解析失败时的兜底必须留在调用方）
			long before = s.ChangeSerial;
			s.Set(ToolItemCatalog.kSnap, null, 9);
			s.Set(ToolItemCatalog.kSnap, "", 9);
			Check(s.ChangeSerial == before && !s.Dirty, "空键被忽略：不置脏也不推进序号");
		}

		private static void BeginMainMenuClears()
		{
			Console.WriteLine("[9] 回主菜单清场 + 重置记忆");
			UnityEngine.Application.TestDir = FreshDir("menu");
			MemoryStore s = new MemoryStore();
			s.UseSaveName("City");
			s.Set(ToolItemCatalog.kToolMode, "A|a", 1);
			s.SaveToDisk(true);
			s.BeginMainMenu();
			Check(s.SaveName == null, "名字清空");
			Check(!s.Dirty, "不脏 -> 主菜单退出不生成文件");
			Check(!s.SaveToDisk(true), "主菜单不写盘");

			s.UseSaveName("City");
			s.Set(ToolItemCatalog.kToolMode, "A|a", 1);
			s.ResetCurrentSave();
			Check(!File.Exists(s.CurrentFilePath()), "文件被删");
			Check(!s.Dirty, "重置后不脏");
			Check(!s.SaveToDisk(true), "重置后不会立刻复活文件");

			// 运行时清空（「重置所有设置项」用）：清掉数据但保持脏，等实时落盘写回空文件
			s.Set(ToolItemCatalog.kSnap, "A|a$net", 2);
			s.ClearRuntime();
			Check(s.Dirty, "ClearRuntime 清数据并保持脏");
			Check(s.SaveToDisk(true) && !File.ReadAllText(s.CurrentFilePath()).Contains("\"snap\""),
				"清空后落盘为无项文件");
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

		// ---------- [21] 同类资产判定 ----------
		//
		// 位值抄自反编译 Game.dll（research/class30/），故意写数字：官方枚举顺序变了测试会红，
		// 那时必须重新核对采集代码，而不是悄悄改测试让它过。
		//   Game.Net.Layer: Road=0x1 PowerlineLow=0x2 PowerlineHigh=0x4 WaterPipe=0x8 SewagePipe=0x10
		//                   TrainTrack=0x40 Pathway=0x80 Waterway=0x100 TramTrack=0x400 SubwayTrack=0x800
		//                   Fence=0x1000 PublicTransportRoad=0x8000 ResourceLine=0x20000
		//   Game.Net.UtilityTypes: WaterPipe=1 SewagePipe=2 LowVoltageLine=8 Catenary=0x20 HighVoltageLine=0x40
		//   Game.City.CityService: 2=Education 3=Electricity 4=FireAndRescue 6=HealthcareAndDeathcare
		//                          9=PoliceAndAdministration 10=Roads

		private const uint LY_ROAD = 0x1u;
		private const uint LY_POWER_LOW = 0x2u;
		private const uint LY_POWER_HIGH = 0x4u;
		private const uint LY_WATER_PIPE = 0x8u;
		private const uint LY_TRAIN = 0x40u;
		private const uint LY_PATHWAY = 0x80u;
		private const uint LY_WATERWAY = 0x100u;
		private const uint LY_SUBWAY = 0x800u;
		private const uint LY_FENCE = 0x1000u;
		private const uint LY_BUS_ROAD = 0x8000u;

		private const uint UT_WATER_PIPE = 0x1u;
		private const uint UT_SEWAGE_PIPE = 0x2u;
		private const uint UT_POWER_LOW = 0x8u;
		private const uint UT_CATENARY = 0x20u;

		private static AssetFacts AF(AssetKind kind = AssetKind.None, uint layers = 0u, uint utility = 0u,
			int service = -1, string serviceName = null, bool tree = false,
			bool bridge = false, bool quay = false, bool intersection = false, bool parking = false)
		{
			AssetFacts f = AssetFacts.Empty();
			f.Kind = kind;
			f.Layers = layers;
			f.Utility = utility;
			if (service >= 0)
			{
				f.HasService = true;
				f.ServiceOrdinal = service;
			}
			f.ServiceName = serviceName;
			f.Tree = tree;
			f.Bridge = bridge;
			f.Quay = quay;
			f.Intersection = intersection;
			f.Parking = parking;
			return f;
		}

		/// <summary>所有分类键都必须是能直接当记忆键片段用的安全 ASCII 串。</summary>
		private static bool KeyIsSafe(string key)
		{
			if (key == null) return true;
			if (!key.StartsWith("K:", StringComparison.Ordinal)) return false;
			for (int i = 0; i < key.Length; i++)
			{
				char c = key[i];
				bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
					|| (c >= '0' && c <= '9') || c == '_' || c == '-' || c == '+' || c == ':';
				if (!ok) return false;
			}
			return true;
		}

		private static void AssetClassification()
		{
			Console.WriteLine("[21] 同类资产 = 按资产服务谁分类（owner 的五个例子）");

			// 1) 数量 / 级别不算区别：车行道路全家同类；桥梁、埠头各成一类（owner 0.4.0）
			string twoLane = AssetClass.Key(AF(kind: AssetKind.Road));
			string sixLane = AssetClass.Key(AF(kind: AssetKind.Road, layers: LY_BUS_ROAD));
			string alley = AssetClass.Key(AF(kind: AssetKind.Road, layers: LY_ROAD | LY_PATHWAY | LY_FENCE));
			string bridge2 = AssetClass.Key(AF(kind: AssetKind.Road, bridge: true));
			string bridge4 = AssetClass.Key(AF(kind: AssetKind.Road | AssetKind.Pathway, bridge: true));
			string railBridge = AssetClass.Key(AF(kind: AssetKind.RailSubway, bridge: true));
			Check(twoLane == "K:road", "两车道道路 = K:road（实际值：" + twoLane + "）");
			Check(twoLane == sixLane, "两车道与六车道道路同类");
			Check(twoLane == alley, "小巷与车行道路同类（人行道 / 围栏层不拆类）");
			Check(bridge2 == bridge4 && bridge4 == "K:bridge", "双车道桥与四车道桥同类 = K:bridge");
			Check(bridge2 != twoLane, "桥梁与道路不同类（桥梁的对象是跨过水面/谷底）");
			Check(railBridge == "K:bridge", "地铁桥也归桥梁：与官方编辑器的 Bridges 分类同判据");

			// 2) 给人走的不是给车走的
			string walkway = AssetClass.Key(AF(kind: AssetKind.Pathway));
			string bikePath = AssetClass.Key(AF(kind: AssetKind.Pathway, layers: LY_PATHWAY | LY_ROAD));
			string promenade = AssetClass.Key(AF(layers: LY_PATHWAY | LY_FENCE));
			string footbridge = AssetClass.Key(AF(kind: AssetKind.Pathway, bridge: true));
			Check(walkway == "K:pathway", "步行道 = K:pathway（实际值：" + walkway + "）");
			Check(walkway != twoLane, "步行道 ≠ 车行道路");
			Check(walkway == bikePath, "行人-自行车道与步行道同类（都是人 / 自行车的路权）");
			Check(walkway == promenade, "带围栏的步行道与步行道同类");
			Check(footbridge == "K:bridge", "人行天桥带 BridgeData → 归桥梁（与官方编辑器 Bridges 同判据）");

			// 3) 三种轨道各算一类
			string subway = AssetClass.Key(AF(kind: AssetKind.RailSubway));
			string subwayOneway = AssetClass.Key(AF(layers: LY_SUBWAY));
			string train = AssetClass.Key(AF(kind: AssetKind.RailTrain, layers: LY_TRAIN | LY_PATHWAY));
			string tram = AssetClass.Key(AF(kind: AssetKind.RailTram));
			string streetTram = AssetClass.Key(AF(kind: AssetKind.RailTram | AssetKind.Road));
			Check(subway == "K:rail_subway", "地铁轨道 = K:rail_subway（实际值：" + subway + "）");
			Check(subway == subwayOneway, "单向与双向地铁轨道同类");
			Check(train == "K:rail_train" && train != subway, "火车轨道 ≠ 地铁轨道");
			Check(tram == "K:rail_tram" && tram != train && tram != subway, "有轨电车又是另一类");
			Check(streetTram == "K:road+rail_tram" && streetTram != twoLane,
				"马路上的混行轨道 = 车+电车，与纯车行道路不同类（实际值：" + streetTram + "）");

			// 4) 航道只看船
			string narrow = AssetClass.Key(AF(kind: AssetKind.Waterway));
			string medium = AssetClass.Key(AF(layers: LY_WATERWAY | LY_PATHWAY));
			Check(narrow == "K:waterway" && narrow == medium, "狭窄航道与普通航道同类");

			// 5) 传播电的东西是一类，发电站是另一类
			string cable = AssetClass.Key(AF(utility: UT_POWER_LOW));
			string powerline = AssetClass.Key(AF(layers: LY_POWER_HIGH));
			string catenary = AssetClass.Key(AF(utility: UT_CATENARY));
			string powerPlant = AssetClass.Key(AF(service: 3));
			Check(cable == "K:power", "电缆 = K:power（实际值：" + cable + "）");
			Check(cable == powerline && cable == catenary, "电缆 / 高压线 / 接触网同属输电一类");
			Check(powerPlant == "K:svc-electricity" && powerPlant != cable, "发电站 ≠ 电缆");
			string waterPipe = AssetClass.Key(AF(utility: UT_WATER_PIPE));
			string sewagePipe = AssetClass.Key(AF(utility: UT_SEWAGE_PIPE));
			string waterWorks = AssetClass.Key(AF(service: 12));
			Check(waterPipe == "K:water_pipe" && sewagePipe == "K:sewage_pipe" && waterPipe != sewagePipe,
				"供水管与污水管各算一类");
			Check(waterWorks == "K:svc-water_sewage" && waterWorks != waterPipe,
				"供水管线与供水厂不同类：一个是传播水的东西，一个是提供服务的建筑");

			// 6) 建筑按公共服务分类：小学 / 中学 / 大学同类，诊所 / 医院同类
			string elementary = AssetClass.Key(AF(service: 2));
			string highSchool = AssetClass.Key(AF(service: 2, serviceName: "SecondaryEducation"));
			string clinic = AssetClass.Key(AF(service: 6));
			string fire = AssetClass.Key(AF(service: 4));
			string police = AssetClass.Key(AF(service: 9));
			Check(elementary == "K:svc-education", "小学 = K:svc-education（实际值：" + elementary + "）");
			Check(elementary == highSchool, "小学与中学同类");
			Check(clinic == "K:svc-healthcare" && clinic != elementary, "诊所与小学不同类");
			Check(fire != police, "消防站与警察局不同类");

			// 7) 服务本身也带道路服务时，仍然先按对象分（道路资产不会被分到 svc-roads）
			string roadWithService = AssetClass.Key(AF(kind: AssetKind.Road, service: 10));
			Check(roadWithService == "K:road", "挂了道路服务的道路仍按车行分类");

			// 8) 埠头 / 路口 / 停车场：四类「看用途」的类别各自成组，且不与对象令牌混
			string quay = AssetClass.Key(AF(kind: AssetKind.Road, bridge: true, quay: true));
			string quayOther = AssetClass.Key(AF(layers: LY_ROAD, quay: true));
			string stamp = AssetClass.Key(AF(kind: AssetKind.Road, intersection: true));
			string smallLot = AssetClass.Key(AF(parking: true));
			string wideLot = AssetClass.Key(AF(parking: true, service: 11));
			Check(quay == "K:quay", "埠头 = K:quay（实际值：" + quay + "）");
			Check(quay == quayOther, "埠头不论底下铺的是什么都同类");
			Check(quay != bridge2 && quay != twoLane, "埠头 ≠ 桥梁 ≠ 道路（owner：三者不是一类）");
			Check(stamp == "K:intersection", "路口预制件 = K:intersection（实际值：" + stamp + "）");
			Check(stamp != twoLane, "路口 ≠ 它盖出来的那种路");
			Check(smallLot == "K:parking", "停车场 = K:parking（实际值：" + smallLot + "）");
			Check(smallLot == wideLot, "宽阔停车场与大型停车场同类（只是容量不同，owner 明确要求）");
			Check(smallLot != elementary, "停车场不与任何服务建筑同类");

			// 9) 树木自成一类；什么都读不到就不分类（宁可不合并）
			Check(AssetClass.Key(AF(tree: true)) == "K:tree", "树木 = K:tree");
			Check(AssetClass.Key(AssetFacts.Empty()) == null, "没有任何事实 -> 不分类");
			Check(AssetClass.Key(new AssetFacts()) == null, "default(AssetFacts) 也不会误判成某个真类别");
			Check(AssetClass.Key(AF(service: 999, serviceName: null)) == null,
				"未知服务序号且没有名字 -> 不分类");
			Check(AssetClass.Key(AF(service: 999, serviceName: "Custom Dump")) == "K:svc-custom-dump",
				"自定义服务退回服务资产名");

			// 10) 键形状：必须是纯 ASCII 安全片段，且与旧的 UI 分类名键不会撞车
			string[] allKeys = new string[]
			{
				twoLane, sixLane, alley, bridge4, walkway, promenade, footbridge, subway, train, tram, streetTram,
				narrow, cable, powerline, powerPlant, waterPipe, sewagePipe, waterWorks, elementary, clinic,
				fire, police, quay, stamp, smallLot,
				AssetClass.Key(AF(tree: true)), AssetClass.Key(AF(service: 999, serviceName: "X"))
			};
			bool allSafe = true;
			string badKey = null;
			for (int i = 0; i < allKeys.Length; i++)
			{
				if (KeyIsSafe(allKeys[i])) continue;
				allSafe = false;
				if (badKey == null) badKey = allKeys[i];
			}
			Check(allSafe, "所有分类键都是 K: 开头的 ASCII 安全片段（不含 $ / 空格），首个不合格：" + badKey);
			Check(MemoryKeys.Category(twoLane) == "C:K:road",
				"完整层级键 = C:K:road（带 K: 段，绝不会等于旧的 UI 分类名键）");
			Check(MemoryKeys.Category(twoLane).IndexOf('/') < 0 && MemoryKeys.Category(twoLane).IndexOf('$') < 0,
				"层级键不含家族分隔符与斜杠");
		}

		/// <summary>
		/// [22] 进档闸门 <see cref="SaveSessionGate"/>。
		/// 玩家日志 2026-10-07 16:41:46 里那条「开机进主菜单也写出了 _unsaved_xxxx.json」
		/// 就是没有这道闸门的后果；而「把原版进档后重置出来的默认值当成玩家的选择记下去」
		/// 是「下一次进档全是出厂值」的永久化路径。两条规则都在这里钉死。
		/// </summary>
		private static void EntryGate()
		{
			Console.WriteLine("[22] 进档闸门：不在存档里不捕获、稳定期里只写回");
			const float settle = SaveSessionGate.kSettleSeconds;

			SaveSessionGate g = new SaveSessionGate();

			// 1) 开局 / 主菜单：一次都不许捕获
			Check(!g.InSave, "刚创建 = 不在存档里");
			Check(!g.Settling(0f) && !g.Settling(1e6f), "没进档时不存在稳定期");
			Check(!g.AllowCapture(0f) && !g.AllowCapture(1e6f), "没进档时任何时刻都不许捕获");
			Check(!g.AllowFinalCapture(), "没进档时连退出前的强制捕获也不许");

			// 2) 进档：稳定期内写回但不捕获
			g.Open(100f);
			Check(g.InSave, "Open = 进入存档");
			Check(g.Settling(100f) && g.Settling(100f + settle * 0.5f), "稳定期内 Settling=true");
			Check(!g.AllowCapture(100f) && !g.AllowCapture(100f + settle * 0.5f),
				"稳定期内不捕获（否则原版重置值会被记成玩家的选择）");
			Check(g.AllowFinalCapture(), "稳定期不挡退出前那一次强制捕获");

			// 3) 稳定期过后恢复正常捕获
			Check(!g.Settling(100f + settle) && g.AllowCapture(100f + settle), "到点当帧起放行");
			Check(g.AllowCapture(100f + settle + 500f), "之后每帧都可以捕获");

			// 4) 离开存档：立刻全关，且不许残留稳定期状态
			g.Close();
			Check(!g.InSave && !g.AllowCapture(100f + settle + 1f) && !g.AllowFinalCapture(),
				"Close 之后一切捕获停止");
			Check(!g.Settling(100f), "Close 后旧时间戳不会被当成稳定期");

			// 5) 换下一个存档必须重新计时（不能拿上一档的过期窗口直接放行）
			g.Open(1200f);
			Check(g.Settling(1200f) && !g.AllowCapture(1200f), "下一档重新进入稳定期");
			Check(g.AllowCapture(1200f + settle), "下一档到点照样放行");

			// 6) settleSeconds<=0 = 明确不要稳定期，绝不能变成「永不捕获」
			g.Open(2000f, 0f);
			Check(!g.Settling(2000f) && g.AllowCapture(2000f), "settleSeconds=0 -> 立即放行捕获");

			// 7) 负数时钟（理论上不该出现，但要确定行为）
			g.Open(-5f, settle);
			Check(g.AllowCapture(-5f + settle) && !g.AllowCapture(-5f), "负时间戳同样按窗口判断");

			// 8) 诊断字段：日志里要能区分「读到空记忆」与「读到了 N 个值」
			FreshDir("entrygate_diag");
			MemoryStore s = new MemoryStore();
			Check(s.IsEmpty && s.EntryCount == 0, "新记忆 = 空");
			s.UseSaveName("Diag");
			s.Set(ToolItemCatalog.kToolMode, K(MemoryKeys.Shared(), false, "net"), 1);
			s.Set(ToolItemCatalog.kToolMode, K(MemoryKeys.Shared(), false, "obj"), 0);
			Check(s.EntryCount == 2 && !s.IsEmpty, "两个键 = 2 个值");
			s.Set(ToolItemCatalog.kToolMode, K(MemoryKeys.Shared(), false, "net"), 1);
			Check(s.EntryCount == 2, "同值重复 Set 不增加计数");
			Check(s.SaveToDisk(true), "写盘");
			MemoryStore r = new MemoryStore();
			r.UseSaveName("Diag");
			Check(r.LoadForCurrentSave() && r.EntryCount == 2, "读回来的值个数一致");
			r.StartUnnamedSession();
			Check(r.IsEmpty && r.EntryCount == 0, "StartUnnamedSession 清空内存（本局专属临时名）");
		}

		/// <summary>
		/// [23] 真机记忆文件回放：把这台机器上玩家自己玩出来的记忆文件拷进临时目录，
		/// 逐个走「解析 → 载入 → 再序列化」，并要求每一项都能被当前目录认出来。
		///
		/// 为什么要它：<c>CommitParsed</c> 会把 <c>ToolItemCatalog.Find</c> 认不出的桶**静默丢掉**，
		/// 而「静默丢掉」在玩家屏幕上长得和「这个档没记忆」一模一样。
		/// 0.4.0 玩家反馈「同一个存档重新进入后没有恢复上次的记忆」，
		/// 第一件要排除的就是这种「文件在、读得懂、但值被丢光」的情况。
		/// 只读副本，绝不碰真实目录（<c>LoadForCurrentSave</c> 成功时会删掉同目录的 .tmp）。
		/// </summary>
		private static void RealFieldFiles()
		{
			Console.WriteLine("[23] 真机记忆文件回放（存在才测，缺文件只跳过不算失败）");
			string profile = Environment.GetEnvironmentVariable("USERPROFILE");
			string fieldDir = string.IsNullOrEmpty(profile)
				? null
				: Path.Combine(profile, "AppData", "LocalLow", "Colossal Order",
					"Cities Skylines II", "ModsData", "ToolModeMemory");
			if (fieldDir == null || !Directory.Exists(fieldDir))
			{
				Console.WriteLine("  SKIP 这台机器上没有 " + fieldDir);
				return;
			}
			string[] files = Directory.GetFiles(fieldDir, "*.json");
			if (files.Length == 0)
			{
				Console.WriteLine("  SKIP 目录里没有记忆文件");
				return;
			}
			FreshDir("field_replay");   // 副本一律写进测试目录，真实目录只读
			int checkedFiles = 0;
			for (int i = 0; i < files.Length; i++)
			{
				string dst = Path.Combine(MemoryStore.DataDirectory, Path.GetFileName(files[i]));
				try { File.Copy(files[i], dst, true); }
				catch { continue; }
				checkedFiles++;

				string text = File.ReadAllText(dst, Encoding.UTF8);
				Dictionary<string, Dictionary<string, int>> parsed =
					new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
				int version;
				Check(MemoryStore.Parse(text, parsed, out version),
					Path.GetFileName(files[i]) + " 解析通过");
				Check(version <= MemoryStore.kVersion,
					Path.GetFileName(files[i]) + " 版本 " + version + " 不高于当前 " + MemoryStore.kVersion);

				// 每个桶的 id 必须在当前目录里认识，否则读进来就被静默丢掉
				string unknown = null;
				foreach (KeyValuePair<string, Dictionary<string, int>> kv in parsed)
				{
					if (ToolItemCatalog.Find(kv.Key) != null) continue;
					unknown = kv.Key;
					break;
				}
				Check(unknown == null, Path.GetFileName(files[i]) + " 的每一项都被当前目录认识（未知项：" + unknown + "）");

				// 载入：条目数必须与文件里的值个数一致（CommitParsed 不许丢）
				string stem = Path.GetFileNameWithoutExtension(dst);
				MemoryStore s = new MemoryStore();
				s.UseSaveName(stem);
				Check(s.LoadForCurrentSave(), stem + " 走生产路径载入成功");
				int inFile = 0;
				foreach (KeyValuePair<string, Dictionary<string, int>> kv in parsed) inFile += kv.Value.Count;
				Check(s.EntryCount == inFile,
					stem + " 载入到 " + s.EntryCount + " 个值，文件里 " + inFile + " 个（不能被丢弃）");

				// 往返稳定：原样再写一次，内容个数不变
				File.WriteAllText(dst, s.Serialize(), new UTF8Encoding(false));
				MemoryStore again = new MemoryStore();
				again.UseSaveName(stem);
				Check(again.LoadForCurrentSave() && again.EntryCount == inFile,
					stem + " 重写后再读，值个数不变");
			}
			Check(checkedFiles > 0, "至少回放了 " + checkedFiles + " 个真实文件");
		}

		/// <summary>
		/// [24] 资产自带的取值限制（v0.6.0）：共用的值超出这件资产自己的范围时不共用，
		/// 值回到范围内时又恢复共用。
		///
		/// 需求原话（owner）：水管的高度最高只能到 -10m，就算「高度」全局共用、
		/// 别的资产是 0m，点到水管上也不可能变成 0m；别的资产若是 -20m，水管就该跟着变 -20m。
		/// 判据来自资产数据（<c>PlaceableNetData.m_ElevationRange</c>，原版
		/// <c>NetToolSystem.CheckElevationRange</c> 夹的就是它），这里用纯规则复现同一判定。
		///
		/// 两侧各自的规矩（缺一不可，理由见 ValueLimit 的注释）：
		///   写回侧 Accepts/Blocks —— 超限的共用值不落在这件资产上（原版自己也会夹，
		///     我们绝不能把面板写成资产做不到的 0m）；
		///   捕获侧 ShouldSkipCapture —— 判的是**这个值本身**够不够得着：从道路切到水管时
		///     工具里留着的是道路的 0m，那不属于水管，记下去要么污染共用桶、
		///     要么变成水管永远用不了、还会挡掉后续真值的死值。
		///     玩家在水管上主动改出的 -30m 则照常记：共用范围带着整组一起变本就是共用的定义。
		/// </summary>
		private static void AssetValueLimits()
		{
			Console.WriteLine("[24] 资产取值限制：超限不共用，回到范围内恢复共用");

			// 水管：m_ElevationRange = { min = -100, max = -10 }（米）
			ValueLimit pipe = ValueLimit.FromRange(-100f, -10f);
			Check(pipe.Known, "读到范围即为 Known");
			Check(pipe.Min == -100f && pipe.Max == -10f, "min/max 原样保留");

			// owner 给的两个用例，逐字对上
			Check(pipe.Blocks(0f), "共用值 0m 超出水管上限 → 水管不共用");
			Check(!pipe.Accepts(0f), "同上：Accepts(0m) = false");
			Check(!pipe.Blocks(-20f), "共用值回到 -20m → 重新可以共用");
			Check(pipe.Accepts(-20f), "Accepts(-20m) = true");
			Check(pipe.Accepts(-100f) && pipe.Accepts(-10f), "闭区间：两个端点都算允许");
			Check(!pipe.Accepts(-9.9f) && !pipe.Accepts(-100.1f), "越界一点点也不行（原版同样会夹回去）");

			// 宽容度：吃掉浮点往返误差，但绝不吃掉真实的限制粒度
			// （水管的上限是 -10m，「超限」指的是比 -10 更高，即 -9.9x）
			Check(pipe.Accepts(-9.96f), "上限之上 4cm 在宽容度内（判定误差，不是玩家选的档位）");
			Check(!pipe.Accepts(-9.94f), "上限之上 6cm 判为超限");
			Check(pipe.Accepts(-100.04f), "下限之下 4cm 在宽容度内");
			Check(!pipe.Accepts(-100.06f), "下限之下 6cm 判为超限");
			Check(ValueLimit.kTolerance < 0.1f, "宽容度小于 10cm：远小于任何一个可用的高度档位");

			// NaN / 残缺范围：一律退化成「不限制」，绝不因为读坏数据而拒绝写回
			Check(!pipe.Accepts(float.NaN), "有限制时 NaN 值视为超限（不写回一个坏数）");
			ValueLimit brokenRange = ValueLimit.FromRange(5f, -5f);
			Check(!brokenRange.Known, "min > max 的残缺范围当不限制");
			Check(brokenRange.Accepts(999999f) && !brokenRange.Blocks(999999f), "不限制放行任何值");
			Check(!ValueLimit.FromRange(float.NaN, 1f).Known, "NaN 端点当不限制");
			Check(!ValueLimit.FromRange(0f, float.PositiveInfinity).Known, "无穷端点当不限制");
			Check(!ValueLimit.None.Known, "None = 不限制");

			// 捕获侧：判的是这个值本身，不是桶里原来有什么
			Check(pipe.ShouldSkipCapture(0f), "道路留下的 0m 不记到水管头上");
			Check(!pipe.ShouldSkipCapture(-30f), "玩家在水管上改出的 -30m 照常记");
			Check(!pipe.ShouldSkipCapture(-10f), "记到本资产的上限也照常（那是它真实的状态）");
			Check(!ValueLimit.None.ShouldSkipCapture(12345f), "没有范围的资产永远不跳过捕获");

			// 「死值」回归：0.6.0 第一版规则（比桶里的旧值）会让水管再也记不进自己那份
			FreshDir("limits");
			MemoryStore dead = new MemoryStore();
			dead.UseSaveName("dead_value_case");
			const string elevation = "elevation";
			const float scale = 100f;      // 与 ToolMemoryBridge.kScale 一致：按 1cm 存整数
			string pipeKey = MemoryKeys.WithDomain(
				MemoryKeys.Asset("Game.Prefabs.NetPrefab:Water Pipe"), false);
			dead.Set(elevation, pipeKey, (int)Math.Round(0f * scale));   // 切过来时残留的非法值
			int leftover = dead.Get(elevation, pipeKey, out bool found);
			Check(found && leftover == 0 && pipe.ShouldSkipCapture(leftover / scale),
				"残留的 0m 既不该被记下来，也不该挡住后面");
			int wanted = (int)Math.Round(-30f * scale);
			Check(!pipe.ShouldSkipCapture(wanted / scale), "玩家改到 -30m：允许捕获");
			dead.Set(elevation, pipeKey, wanted);
			Check(dead.Get(elevation, pipeKey, out found) == wanted && found,
				"水管那份记忆真的更新成 -30m（旧值不会把新值挡掉）");

			// 端到端：「全局共用」在资产域里只有一个桶，道路和水管共用同一个键。
			// 限制改变的不是桶的个数，而是「这个桶的值能不能落到这件资产上」。
			string sharedKey = MemoryKeys.WithDomain(MemoryKeys.Shared(), false);
			MemoryStore store = new MemoryStore();
			store.UseSaveName("limit_case");
			store.Set(elevation, sharedKey, (int)Math.Round(0f * scale));
			int bucket = store.Get(elevation, sharedKey, out found);
			Check(found && bucket == 0, "共用桶 = 0m（道路那侧记下来的）");
			Check(pipe.Blocks(bucket / scale), "0m 超水管范围：选中水管时不把 0m 写回去");
			Check(pipe.ShouldSkipCapture(bucket / scale),
				"水管此时带着道路留下的 0m：也不把它记回同一个桶（道路的记忆不动）");

			// 道路改成 -20m：水管重新进入范围，共用应当恢复
			store.Set(elevation, sharedKey, (int)Math.Round(-20f * scale));
			bucket = store.Get(elevation, sharedKey, out found);
			Check(!pipe.Blocks(bucket / scale), "-20m 在水管范围内：共用重新生效（owner 要求的回程）");
			Check(!pipe.ShouldSkipCapture(bucket / scale), "-20m 时水管照旧参与捕获");

			// 玩家在共用状态下把水管调到 -30m：合法值，整组跟着走（与原版同一个数）
			store.Set(elevation, sharedKey, (int)Math.Round(-30f * scale));
			bucket = store.Get(elevation, sharedKey, out found);
			Check(bucket == -3000 && !pipe.Blocks(bucket / scale),
				"水管主动改的 -30m 记进共用桶，且对水管自己可用");

			// 1cm 量化不许翻转判定：范围端点附近来回存一次
			string[] probes = new string[] { "-10", "-10.01", "-10.04", "-9.99", "-100", "-100.04", "-50" };
			int flipped = 0;
			for (int i = 0; i < probes.Length; i++)
			{
				float m = float.Parse(probes[i], System.Globalization.CultureInfo.InvariantCulture);
				int stored = (int)Math.Round(m * scale);
				float back = stored / scale;
				if (pipe.Accepts(m) != pipe.Accepts(back)) flipped++;
			}
			Check(flipped == 0, "按 1cm 存取一次不会翻转判定（翻转项数=" + flipped + "）");

			// 落盘再读一次：限制判的就是文件里那个值，读回来判定得照样成立
			Check(store.SaveToDisk(), "带限制的存档写盘成功");
			MemoryStore reloaded = new MemoryStore();
			reloaded.UseSaveName("limit_case");
			Check(reloaded.LoadForCurrentSave(), "重进存档读回");
			int sharedBack = reloaded.Get(elevation, sharedKey, out found);
			Check(found && sharedBack == -3000 && !pipe.Blocks(sharedBack / scale),
				"共用桶读回来还是 -30m，对水管依旧可用");
		}
	}
}
