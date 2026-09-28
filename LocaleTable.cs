using System;
using System.Collections.Generic;
using System.Reflection;
using Colossal;
using Game.UI;
using Game.UI.Widgets;

namespace ToolModeMemory
{
	/// <summary>
	/// 12 官方语言词条 + 范围下拉（带原版/推荐标注）。
	///
	/// v0.2.0 改动：
	///  * 弃掉旧的位置参数式 Base(...)，改成按键名填充的字典，任何一门语言漏键都能立刻看出来；
	///    运行时仍会向 en-US 回落一次，避免设置页出现空白行。
	///  * 所有游戏内名词一律取自官方语言包（Locale.cok）里对应语言的原文，
	///    官方缺失或未翻译的键回落该键的 en-US 原文（例如 Toolbar.BRUSH_TYPE）。
	///  * 条目清单换成 ToolItemCatalog 的 13 项；序号由本文件在注册时拼到标签前面。
	///  * 第 1 项（Anarchy）与第 10、11 项（Extra Networks and Areas 面板）不是原版内容，
	///    名字按用户给的暂定文案处理，等对应模组的官方文案确认后再补丁。
	///
	/// 每份字典各填各的语言，禁止读 activeLocale（AccessAnarchy 的教训）。
	/// </summary>
	internal static class LocaleTable
	{
		public const string kScopeGroup = "scope.group";
		public const string kScopeMenu = "scope.menu";
		public const string kScopeCategory = "scope.category";
		public const string kScopeGlobalShared = "scope.globalShared";
		public const string kScopeGlobalUnique = "scope.globalUnique";

		public const string kTagVanilla = "tag.vanilla";
		public const string kTagRecommended = "tag.recommended";
		public const string kTagRecVanilla = "tag.recVanilla";
		public const string kTagNone = "tag.none";

		private static readonly string[] kLocales =
		{
			"de-DE", "en-US", "es-ES", "fr-FR", "it-IT", "ja-JP",
			"ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-HANS", "zh-HANT"
		};

		public static string[] Locales { get { return kLocales; } }

		private static string _activeLocale = "en-US";

		/// <summary>语言表按语言缓存一次，避免每次刷新设置页都重建整表词条。</summary>
		private static readonly Dictionary<string, Dictionary<string, string>> kCache =
			new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

		public static void SetActiveLocale(string locale)
		{
			for (int i = 0; i < kLocales.Length; i++)
			{
				if (string.Equals(kLocales[i], locale, StringComparison.OrdinalIgnoreCase))
				{
					_activeLocale = kLocales[i];
					return;
				}
			}
			_activeLocale = "en-US";
		}

		public static string T(string key)
		{
			Dictionary<string, string> d = Table(_activeLocale);
			string value;
			if (d.TryGetValue(key, out value)) return value;
			return Table("en-US")[key];
		}

		/// <summary>范围下拉：短标签 + 按工具项标注（原版）（推荐）（推荐原版）。</summary>
		public static DropdownItem<int>[] BuildScopeDropdown(ToolItemDef def)
		{
			Dictionary<string, string> d = Table(_activeLocale);
			return new DropdownItem<int>[]
			{
				Item(0, kScopeGroup, def, d),
				Item(1, kScopeMenu, def, d),
				Item(2, kScopeCategory, def, d),
				Item(3, kScopeGlobalShared, def, d),
				Item(4, kScopeGlobalUnique, def, d),
			};
		}

		private static DropdownItem<int> Item(int value, string baseKey, ToolItemDef def, Dictionary<string, string> d)
		{
			string name = d[baseKey];
			int van = def != null ? def.VanillaScope : ToolItemCatalog.kVanillaNone;
			int rec = def != null ? def.RecommendedScope : 0;
			bool isVan = van == value;
			bool isRec = rec == value;
			string tag;
			if (isVan && isRec) tag = d[kTagRecVanilla];
			else if (isVan) tag = d[kTagVanilla];
			else if (isRec) tag = d[kTagRecommended];
			else tag = d[kTagNone];
			return new DropdownItem<int> { value = value, displayName = name + tag };
		}

		/// <summary>
		/// 设置页词条注册。框架键 = ModSetting 的 GetXxxLocaleID(...)，值来自对应语言的字典。
		/// </summary>
		public static Dictionary<string, string> BuildEntries(ToolModeMemorySettings setting, string locale)
		{
			Dictionary<string, string> d = Table(locale);
			Dictionary<string, string> o = new Dictionary<string, string>();

			o[setting.GetSettingsLocaleID()] = d["mod.name"];
			o[setting.GetOptionTabLocaleID(ToolModeMemorySettings.kTabMod)] = d["tab.mod"];
			o[setting.GetOptionTabLocaleID(ToolModeMemorySettings.kTabAbout)] = d["tab.about"];
			AddTabAndGroupTitles(setting, d, o);

			// 总开关 + 兼容开关 + 范围定义说明行
			o[setting.GetOptionLabelLocaleID("Enabled")] = d["enabled.label"];
			o[setting.GetOptionDescLocaleID("Enabled")] = d["enabled.desc"];
			o[setting.GetOptionLabelLocaleID("CompatOtherMods")] = d["compat.label"];
			o[setting.GetOptionDescLocaleID("CompatOtherMods")] = d["compat.desc"];
			o[setting.GetOptionLabelLocaleID("ScopeDefinitions")] = d["scope.defs"];

			AddItems(setting, d, o);

			// 记忆管理（关于页）
			o[setting.GetOptionLabelLocaleID("ResetMemory")] = d["reset.label"];
			o[setting.GetOptionDescLocaleID("ResetMemory")] = d["reset.desc"];
			o[setting.GetOptionWarningLocaleID("ResetMemory")] = d["reset.warn"];
			o["Options.WARNING[CONFIRM_RESET]"] = d["reset.confirm"];

			o[setting.GetOptionLabelLocaleID("ResetAllSettings")] = d["resetall.label"];
			o[setting.GetOptionDescLocaleID("ResetAllSettings")] = d["resetall.desc"];
			o[setting.GetOptionWarningLocaleID("ResetAllSettings")] = d["resetall.warn"];
			o["Options.WARNING[CONFIRM_RESET_ALL]"] = d["resetall.confirm"];

			o[setting.GetOptionLabelLocaleID("OpenMemoryFolder")] = d["folder.label"];
			o[setting.GetOptionDescLocaleID("OpenMemoryFolder")] = d["folder.desc"];

			// 信息与链接
			o[setting.GetOptionLabelLocaleID("ModVersion")] = d["about.version"];
			o[setting.GetOptionLabelLocaleID("ModAuthor")] = d["about.author"];
			o[setting.GetOptionLabelLocaleID("OpenKofi")] = d["about.kofi"];
			o[setting.GetOptionDescLocaleID("OpenKofi")] = d["about.kofi.desc"];
			o[setting.GetOptionLabelLocaleID("OpenForum")] = d["about.forum"];
			o[setting.GetOptionDescLocaleID("OpenForum")] = d["about.forum.desc"];
			o[setting.GetOptionLabelLocaleID("OpenRainbowSite")] = d["about.rainbow"];
			o[setting.GetOptionDescLocaleID("OpenRainbowSite")] = d["about.rainbow.desc"];
			return o;
		}

		/// <summary>
		/// 13 个工具项：启用开关 + 共用范围下拉。标签在这里补上目录里的序号，
		/// 语言表里的 item.*.label 一律不带数字。
		/// </summary>
		private static void AddItems(ToolModeMemorySettings setting, Dictionary<string, string> d, Dictionary<string, string> o)
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				string pascal = PascalCase(def.Id);
				o[setting.GetOptionLabelLocaleID(pascal + "Enabled")] = def.Number.ToString() + ". " + d["item." + def.Id + ".label"];
				o[setting.GetOptionDescLocaleID(pascal + "Enabled")] = d["item." + def.Id + ".desc"];
				o[setting.GetOptionLabelLocaleID(pascal + "Scope")] = d["scope.label"];
				o[setting.GetOptionDescLocaleID(pascal + "Scope")] = d["scope.desc"];
			}
		}

		/// <summary>id 形如 toolMode / elevationStep，设置类里的属性名是 ToolMode / ElevationStep。</summary>
		private static string PascalCase(string id)
		{
			if (string.IsNullOrEmpty(id)) return id;
			return char.ToUpperInvariant(id[0]) + id.Substring(1);
		}

		/// <summary>
		/// 板块名。设置类里的 kGroup* 常量会随重构改名（v0.2.0 把四个分组并成一个），
		/// 所以这里反射读取常量本身，按语义给标题，不写死分组名。
		/// </summary>
		private static void AddTabAndGroupTitles(ToolModeMemorySettings setting, Dictionary<string, string> d, Dictionary<string, string> o)
		{
			FieldInfo[] fields = typeof(ToolModeMemorySettings).GetFields(
				BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo f = fields[i];
				if (f.FieldType != typeof(string) || !f.IsLiteral) continue;
				string name = f.Name;
				string value = f.GetValue(null) as string;
				if (string.IsNullOrEmpty(value)) continue;
				if (name.StartsWith("kTab", StringComparison.Ordinal))
				{
					bool aboutTab = name.IndexOf("About", StringComparison.OrdinalIgnoreCase) >= 0;
					o[setting.GetOptionTabLocaleID(value)] = aboutTab ? d["tab.about"] : d["tab.mod"];
				}
				else if (name.StartsWith("kGroup", StringComparison.Ordinal))
				{
					o[setting.GetOptionGroupLocaleID(value)] = GroupTitle(name, value, d);
				}
			}
		}

		/// <summary>
		/// 分组语义判定：字段名优先（kGroupMain / kGroupItems 都是全部工具项那一个板块），
		/// 再看字段值里的关键词，最后默认归到工具项板块。
		/// </summary>
		private static string GroupTitle(string field, string value, Dictionary<string, string> d)
		{
			string f = field.ToLowerInvariant();
			if (f.EndsWith("items", StringComparison.Ordinal) || f.EndsWith("main", StringComparison.Ordinal)
				|| f.EndsWith("settings", StringComparison.Ordinal) || f.EndsWith("mod", StringComparison.Ordinal))
			{
				return d["group.items"];
			}
			string s = f + "|" + value.ToLowerInvariant();
			if (s.IndexOf("compat", StringComparison.Ordinal) >= 0) return d["group.compat"];
			if (s.IndexOf("about", StringComparison.Ordinal) >= 0 || s.IndexOf("info", StringComparison.Ordinal) >= 0
				|| s.IndexOf("link", StringComparison.Ordinal) >= 0)
			{
				return d["group.about"];
			}
			if (s.IndexOf("reset", StringComparison.Ordinal) >= 0 || s.IndexOf("memory", StringComparison.Ordinal) >= 0
				|| s.IndexOf("folder", StringComparison.Ordinal) >= 0)
			{
				return d["group.reset"];
			}
			return d["group.items"];
		}

		/// <summary>兼容旧的 Build(ToolModeMemorySettings,locale) 入口：等价于 BuildEntries。</summary>
		public static Dictionary<string, string> BuildForSource(ToolModeMemorySettings setting, string locale)
		{
			return BuildEntries(setting, locale);
		}

		private static Dictionary<string, string> Table(string locale)
		{
			Dictionary<string, string> d;
			if (kCache.TryGetValue(locale, out d)) return d;
			d = Build(locale);
			if (!string.Equals(locale, "en-US", StringComparison.Ordinal)) FillGaps(d, Table("en-US"));
			kCache[locale] = d;
			return d;
		}

		/// <summary>漏键回落到 en-US 同键原文（官方语言包本身也有未翻译键，规则一致）。</summary>
		private static void FillGaps(Dictionary<string, string> d, Dictionary<string, string> en)
		{
			foreach (KeyValuePair<string, string> kv in en)
			{
				if (!d.ContainsKey(kv.Key)) d[kv.Key] = kv.Value;
			}
		}

		private static Dictionary<string, string> Build(string locale)
		{
			switch (locale)
			{
				case "de-DE": return De();
				case "es-ES": return Es();
				case "fr-FR": return Fr();
				case "it-IT": return It();
				case "ja-JP": return Ja();
				case "ko-KR": return Ko();
				case "pl-PL": return Pl();
				case "pt-BR": return Pt();
				case "ru-RU": return Ru();
				case "zh-HANS": return ZhHans();
				case "zh-HANT": return ZhHant();
				default: return En();
			}
		}

		/// <summary>
		/// 框架键：与语言无关、12 份共用的部分。各语言方法在此基础上按键补齐自己的文案。
		/// </summary>
		private static Dictionary<string, string> Frames(string locale)
		{
			Dictionary<string, string> d = new Dictionary<string, string>(StringComparer.Ordinal);
			d["frame.locale"] = locale;
			// Anarchy 是第三方模组自带的按钮名，12 种语言一律保持原样（用户要求）。
			d["item.anarchy.label"] = "Anarchy";
			// 无标注时后缀为空串，四个标签键都存在以免下拉里出现空引用。
			d[kTagNone] = "";
			return d;
		}

		// ======================= zh-HANS =======================

		private static Dictionary<string, string> ZhHans()
		{
			Dictionary<string, string> d = Frames("zh-HANS");
			d["mod.name"] = "工具模式区分记忆";
			d["tab.mod"] = "工具模式记忆设置";
			d["tab.about"] = "关于";
			d["group.items"] = "工具记忆模式设置";
			d["group.reset"] = "记忆管理";
			d["group.compat"] = "兼容性";
			d["group.about"] = "信息与链接";

			d["enabled.label"] = "启用工具模式记忆";
			d["enabled.desc"] = "开启期间，每一项设置的数值都会被持续记录，回到存档时就是你上次离开时的样子。单独关掉某一项，只是不再恢复那一项，它的数值仍然在记录。只有关掉这个总开关才会停止记录，所有工具恢复原版行为。";
			d["compat.label"] = "兼容其他模组";
			d["compat.desc"] = "开启后，记忆按其他模组（Asset UI Manager、ExtraLib 等）调整后的菜单与分组名称归类，被挪过位置的资产会跟着它的新位置走。关闭时沿用本模组第一次看到该资产时的分类，更加稳定。两种做法都不会丢掉已经记录的数值。";
			d["scope.label"] = "共用范围";
			d["scope.desc"] = "这项数值在多大范围内共享。括号里标出原版行为和推荐选择。";
			d["scope.defs"] = "共用范围说明\n同组：给一条双向两车道的小型道路把高度调到 10 米，其他小型道路也会是 10 米，大型道路不受影响。\n同菜单：道路菜单里的任何资产都共用这一项，换到电力菜单就不共用。\n同类资产：只有同一子类的资产或功能才共用。换成另一条大型道路仍然共用 10 米，但桥梁会退回 0 米、需要自己单独设置，尽管两者都在道路菜单下。这里的功能指的是功能区、空间与区域、地形改造、标记、预制对象这些非资产工具。\n全局共用：所有支持该项的资产和功能共用一份数值，但资产与功能之间不互相共用。\n全部不共用：每个资产、每个功能各存一份，完全独立。";

			d[kScopeGroup] = "同组";
			d[kScopeMenu] = "同菜单";
			d[kScopeCategory] = "同类资产";
			d[kScopeGlobalShared] = "全局共用";
			d[kScopeGlobalUnique] = "全部不共用";
			d[kTagVanilla] = "（原版）";
			d[kTagRecommended] = "（推荐）";
			d[kTagRecVanilla] = "（推荐原版）";

			d["reset.label"] = "重置记忆";
			d["reset.desc"] = "游戏中：清除当前存档的记忆，工具恢复刚进档时的原版状态。主菜单：清除所有存档的记忆。";
			d["reset.warn"] = "此操作无法撤销。";
			d["reset.confirm"] = "确定要重置已记忆的工具设置吗？";
			d["resetall.label"] = "恢复所有推荐默认设置";
			d["resetall.desc"] = "把本模组的所有选项回到推荐默认值：总开关、兼容开关，以及每一项的启用状态和共用范围。已记录的存档记忆不受影响。";
			d["resetall.warn"] = "此操作无法撤销。";
			d["resetall.confirm"] = "确定要恢复所有推荐默认设置吗？";
			d["folder.label"] = "管理所有存档的记忆文件";
			d["folder.desc"] = "打开保存记忆的本地文件夹，每个存档一份文件，文件名就是存档名。";

			d["about.version"] = "模组版本";
			d["about.author"] = "作者";
			d["about.kofi"] = "请我喝杯咖啡";
			d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
			d["about.forum"] = "论坛页面";
			d["about.forum.desc"] = "打开 Paradox 论坛帖子。";
			d["about.rainbow"] = "RAINBOW官网";
			d["about.rainbow.desc"] = "打开 RAINBOW 系列官网。";

			d["item.anarchy.desc"] = "记忆 Anarchy 模组（Paradox 模组 74604）在当前工具上的那颗 Anarchy 开关。取消高度上限这类选项属于该模组本身，不是原版内容；没装 Anarchy 时这一项不起作用。";
			d["item.packs.label"] = "数据包";
			d["item.packs.desc"] = "记忆工具选项里「数据包」一行的勾选：开启/关闭所有数据包、基础版游戏、资产模组。";
			d["item.themes.label"] = "地区主题";
			d["item.themes.desc"] = "记忆「地区主题」一行的选择。可选项随资产类别而变，例如建筑类里是欧洲、北美、现代建筑、都市步行道。";
			d["item.toolMode.label"] = "工具模式";
			d["item.toolMode.desc"] = "记忆每个工具所选的模式，可选项按工具不同：道路、轨道、管道为直线、简单曲线、复杂曲线、连续曲线、网格、替换、节点；建筑、装饰物、树木为放置单个、放置多个、直线、曲线、目标印戳工具；功能区为填充、滚动、刷涂；区域为编辑、生成地图网格。资产的工具模式与功能的工具模式分开记忆，因为它们的选项并不相同。";
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "记忆工具离开时的高度值，也包括提升高度、降低高度得到的结果。交叉路口与立交桥的高度不在记忆范围内。";
			d["item.parallel.label"] = "并列模式";
			d["item.parallel.desc"] = "记忆「并列模式」开关（切换并列模式），连同「并列道路」的数量和「并列偏移」的间距（增加偏移、减少偏移）。并列模式用来建造并列网络。";
			d["item.snap.label"] = "对齐";
			d["item.snap.desc"] = "记忆「对齐」里的每一个开关：快速对齐当前形状、快速对齐功能区单元格长度、快速对齐直角、快速对齐路边、快速对齐道路、快速对齐拥有者一侧、快速对齐建筑两侧、快速对齐道路中央、快速对齐海岸线、快速对齐附近形状、快速对齐导线、快速对齐功能区网格、快速对齐节点、快速对齐物体表面、快速直立对齐、快速对齐空地网格、将重叠物体绑定至建筑、仅移除相符类型、显示轮廓线条、快速对齐距离。游戏自带的「开启／关闭所有对齐功能」是批量开关，本身不会被记忆。";
			d["item.topography.label"] = "地形";
			d["item.topography.desc"] = "记忆「地形」这一行，也就是显示轮廓线条的开与关。";
			d["item.elevationStep.label"] = "高度阶段";
			d["item.elevationStep.desc"] = "记忆「高度阶段」的数值，也就是一次提升高度或降低高度所移动的量。";
			d["item.leftRight.label"] = "左侧和右侧";
			d["item.leftRight.desc"] = "记忆 Extra Networks and Areas 面板里左侧、右侧两行所选的附加网络，例如自行车车道、树木、停车场、堤岸、挡土墙、隔音屏障。需要安装该模组，没装时这一项不起作用。";
			d["item.general.label"] = "常规";
			d["item.general.desc"] = "记忆同一模组「常规」面板里的开关：强制地面模式、隧道模式、高架模式、取消高度上限。需要安装该模组。";
			d["item.underground.label"] = "地下模式";
			d["item.underground.desc"] = "记忆「地下模式」开关（切换地下模式），也就是你放的东西在地面以下还是地面之上。";
			d["item.other.label"] = "杂项";
			d["item.other.desc"] = "记忆工具面板上其余可以记录的行：笔刷粗细、笔刷强度、笔刷角度、目标高度、距离、颜色、年龄（孩童、青年、成年、老年）、保留年代。";
			return d;
		}

		// ======================= zh-HANT =======================

		private static Dictionary<string, string> ZhHant()
		{
			Dictionary<string, string> d = Frames("zh-HANT");
			d["mod.name"] = "工具模式區分記憶";
			d["tab.mod"] = "工具模式記憶設定";
			d["tab.about"] = "關於";
			d["group.items"] = "工具記憶模式設定";
			d["group.reset"] = "記憶管理";
			d["group.compat"] = "相容性";
			d["group.about"] = "資訊與連結";

			d["enabled.label"] = "啟用工具模式記憶";
			d["enabled.desc"] = "開啟期間，每一項設定的數值都會被持續記錄，回到存檔時就是你上次離開時的樣子。單獨關掉某一項，只是不再恢復那一項，它的數值仍然在記錄。只有關掉這個總開關才會停止記錄，所有工具恢復原版行為。";
			d["compat.label"] = "相容其他模組";
			d["compat.desc"] = "開啟後，記憶按其他模組（Asset UI Manager、ExtraLib 等）調整後的選單與分組名稱歸類，被挪過位置的資產會跟著它的新位置走。關閉時沿用本模組第一次看到該資產時的分類，更加穩定。兩種做法都不會丟掉已記錄的數值。";
			d["scope.label"] = "共用範圍";
			d["scope.desc"] = "這項數值在多大範圍內共用。括號裡標出原版行為與推薦選擇。";
			d["scope.defs"] = "共用範圍說明\n同組：給一雙向兩車道的小型道路把高度調到 10 公尺，其他小型道路也會是 10 公尺，大型道路不受影響。\n同選單：道路選單裡的任何資產都共用這一項，換到電力選單就不共用。\n同類資產：只有同一子類的資產或功能才共用。換成另一條大型道路仍然共用 10 公尺，但橋樑會退回 0 公尺、需要自己單獨設定，儘管兩者都在道路選單下。這裡的功能指的是功能區、空間與區域、地形改造、標記、預製物件這些非資產工具。\n全域共用：所有支援該項的資產和功能共用一份數值，但資產與功能之間不互相共用。\n全部不共用：每個資產、每個功能各存一份，完全獨立。";

			d[kScopeGroup] = "同組";
			d[kScopeMenu] = "同選單";
			d[kScopeCategory] = "同類資產";
			d[kScopeGlobalShared] = "全域共用";
			d[kScopeGlobalUnique] = "全部不共用";
			d[kTagVanilla] = "（原版）";
			d[kTagRecommended] = "（推薦）";
			d[kTagRecVanilla] = "（推薦原版）";

			d["reset.label"] = "重設記憶";
			d["reset.desc"] = "遊戲中：清除目前存檔的記憶，工具恢復剛進檔時的原版狀態。主選單：清除所有存檔的記憶。";
			d["reset.warn"] = "此操作無法復原。";
			d["reset.confirm"] = "確定要重設已記憶的工具設定嗎？";
			d["resetall.label"] = "恢復所有推薦預設設定";
			d["resetall.desc"] = "把本模組的所有選項回到推薦預設值：總開關、相容開關，以及每一項的啟用狀態和共用範圍。已記錄的存檔記憶不受影響。";
			d["resetall.warn"] = "此操作無法復原。";
			d["resetall.confirm"] = "確定要恢復所有推薦預設設定嗎？";
			d["folder.label"] = "管理所有存檔的記憶檔案";
			d["folder.desc"] = "開啟保存記憶的本機資料夾，每個存檔一份檔案，檔名就是存檔名。";

			d["about.version"] = "模組版本";
			d["about.author"] = "作者";
			d["about.kofi"] = "請我喝杯咖啡";
			d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
			d["about.forum"] = "論壇頁面";
			d["about.forum.desc"] = "開啟 Paradox 論壇貼文。";
			d["about.rainbow"] = "RAINBOW官網";
			d["about.rainbow.desc"] = "開啟 RAINBOW 系列官網。";

			d["item.anarchy.desc"] = "記憶 Anarchy 模組（Paradox 模組 74604）在目前工具上的那顆 Anarchy 開關。取消高度上限這類選項屬於該模組本身，不是原版內容；沒裝 Anarchy 時這一項不起作用。";
			d["item.packs.label"] = "安裝包";
			d["item.packs.desc"] = "記憶工具選項裡「安裝包」一行的勾選：開啟／關閉所有安裝包、基礎版遊戲、資產模組。";
			d["item.themes.label"] = "主題";
			d["item.themes.desc"] = "記憶「主題」一行的選擇。可選項随資產類別而變，例如建築類裡是歐式、北美、現代建築、都會步道。";
			d["item.toolMode.label"] = "工具模式";
			d["item.toolMode.desc"] = "記憶每個工具所選的模式，可選項按工具不同：道路、軌道、管道為直線、簡單曲線、複數曲線、連續曲線、網格、替換、連接點；建築、裝飾物、樹木為設置一個、設置多個、直線、曲線、物件印章工具；功能區為填充、選取方格、塗上；區域為編輯、生成地圖方格。資產的工具模式與功能的工具模式分開記憶，因為它們的選項並不相同。";
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "記憶工具離開時的高度值，也包括提升高度、降低高度得到的結果。交叉路口與交流道的高度不在記憶範圍內。";
			d["item.parallel.label"] = "並行模式";
			d["item.parallel.desc"] = "記憶「並行模式」開關（切換並行模式），連同「並行道路」的數量和「並行偏移」的間距（增加偏移、減少偏移）。並行模式用來建造並行網狀系統。";
			d["item.snap.label"] = "吸附";
			d["item.snap.desc"] = "記憶「吸附」裡的每一個開關：快速對齊已有幾何線、快速對齊功能區單位格長度、快速對齊90度角、快速對齊路邊、快速對齊道路、快速對齊擁有者一側、快速對齊建築兩側、快速對齊道路中央、快速對齊水陸交界線、快速對齊附近幾何線、快速對齊導線、快速對齊功能區網格、快速對齊節點、快速對齊物件表面、快速直立對齊、快速對齊空地網格、將重疊對象綁定至建築、只移除符合的類型、顯示輪廓、對齊距離。遊戲自帶的「開啟／關閉所有對齊功能」是批次開關，本身不會被記憶。";
			d["item.topography.label"] = "地形圖";
			d["item.topography.desc"] = "記憶「地形圖」這一行的開與關，也就是是否顯示輪廓。";
			d["item.elevationStep.label"] = "高度階段";
			d["item.elevationStep.desc"] = "記憶「高度階段」的數值，也就是一次提升高度或降低高度所移動的量。";
			d["item.leftRight.label"] = "左側和右側";
			d["item.leftRight.desc"] = "記憶 Extra Networks and Areas 面板裡左側、右側兩行所選的附加網路，例如單車專用道、樹木、停車、堤岸、擋土牆、音障。需要安裝該模組，沒裝時這一項不起作用。";
			d["item.general.label"] = "常規";
			d["item.general.desc"] = "記憶同一模組「常規」面板裡的開關：強制地面模式、隧道模式、高架模式、取消高度上限。需要安裝該模組。";
			d["item.underground.label"] = "地下模式";
			d["item.underground.desc"] = "記憶「地下模式」開關（切換地下模式），也就是你放的東西在地面以下還是地面之上。";
			d["item.other.label"] = "綜合";
			d["item.other.desc"] = "記憶工具面板上其餘可以記錄的行：筆刷大小、筆刷硬度、筆刷角度、目標高度、距離、顏色、年齡（樹苗、年輕、成熟、老年）、保留年代。";
			return d;
		}

		// ======================= en-US =======================

		private static Dictionary<string, string> En()
		{
			Dictionary<string, string> d = Frames("en-US");
			d["mod.name"] = "TOOL MODE MEMORY";
			d["tab.mod"] = "Tool Mode Memory Settings";
			d["tab.about"] = "About";
			d["group.items"] = "Tool Memory Settings";
			d["group.reset"] = "Memory management";
			d["group.compat"] = "Compatibility";
			d["group.about"] = "Information and links";

			d["enabled.label"] = "Enable Tool Mode Memory";
			d["enabled.desc"] = "While this is on, every value keeps being recorded, so returning to a save puts the panels back exactly as you left them. Turning one item off only stops that item from being restored - its values are still recorded. Only switching this master switch off stops all recording and returns every tool to vanilla behaviour.";
			d["compat.label"] = "Compatible with other mods";
			d["compat.desc"] = "On: memory is filed under the menu and group names as adjusted by other mods (Asset UI Manager, ExtraLib and the like), so an asset you moved follows its new location. Off: the classification this mod saw first is kept, which is steadier. Neither choice throws away values that are already recorded.";
			d["scope.label"] = "Sharing scope";
			d["scope.desc"] = "How widely this value is shared between assets. The brackets show what vanilla does and what we recommend.";
			d["scope.defs"] = "Sharing scopes\nSame group: raise the elevation of a two-lane small road to 10 m and the other small roads go to 10 m too, while large roads stay untouched.\nSame menu: any asset in the Roads menu shares it, but moving to the Electricity menu does not.\nSame asset type: only assets or functions of the same sub-category share it. Switching to another large road still shares the 10 m, but a bridge resets to 0 m and needs its own setting, even though both live under the Roads menu. Functions here means Zones, Spaces and Areas, Terraforming, Markers and Object Prefabs, that is the tools that are not assets.\nShared globally: every asset and function that supports the option shares one value, but assets and functions never share with each other.\nNot shared at all: every asset and function keeps its own value.";

			d[kScopeGroup] = "Same group";
			d[kScopeMenu] = "Same menu";
			d[kScopeCategory] = "Same asset type";
			d[kScopeGlobalShared] = "Shared globally";
			d[kScopeGlobalUnique] = "Not shared at all";
			d[kTagVanilla] = " (vanilla)";
			d[kTagRecommended] = " (recommended)";
			d[kTagRecVanilla] = " (recommended, vanilla)";

			d["reset.label"] = "Reset memory";
			d["reset.desc"] = "In a save: wipe this save's memory and put the tools back to the vanilla state they had on load. In the main menu: wipe the memory of every save.";
			d["reset.warn"] = "This cannot be undone.";
			d["reset.confirm"] = "Reset all remembered tool settings?";
			d["resetall.label"] = "Restore all recommended defaults";
			d["resetall.desc"] = "Puts every option of this mod back to its recommended default: the master switch, the compatibility switch, and the on/off state and sharing scope of each item. Memory already recorded for saves is left alone.";
			d["resetall.warn"] = "This cannot be undone.";
			d["resetall.confirm"] = "Restore all settings to their recommended defaults?";
			d["folder.label"] = "Manage memory files for all saves";
			d["folder.desc"] = "Opens the local folder that holds the memory files, one per save, named after the save.";

			d["about.version"] = "Mod version";
			d["about.author"] = "Author";
			d["about.kofi"] = "Buy me a coffee";
			d["about.kofi.desc"] = "Support the author on Ko-fi.";
			d["about.forum"] = "Forum page";
			d["about.forum.desc"] = "Open the Paradox forum thread.";
			d["about.rainbow"] = "RAINBOW website";
			d["about.rainbow.desc"] = "Open the Rainbow Series site.";

			d["item.anarchy.desc"] = "Remembers the Anarchy mod's own on/off button (Paradox mod 74604) for the tool you are using. Options such as removing the height limit belong to that mod, not to the game. Without Anarchy installed this item does nothing.";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Remembers the ticks on the Pack row of Tool Options: Toggle all packs on/off, Base Game and Asset Mods.";
			d["item.themes.label"] = "Theme";
			d["item.themes.desc"] = "Remembers the Theme row. What is on offer depends on the asset category - buildings, for example, list European, North American, Modern Architecture and Urban Promenades.";
			d["item.toolMode.label"] = "Tool Mode";
			d["item.toolMode.desc"] = "Remembers the mode picked for each tool, and the options differ by tool: roads, tracks and pipes offer Straight, Simple Curve, Complex Curve, Continuous, Grid, Replace and Point; buildings, props and trees offer Place one, Place multiple, Line, Curve and Object stamp tool; zones offer Fill, Marquee and Paint; areas offer Edit and Generate Map Grid. Asset tool modes and function tool modes are remembered separately because their option sets differ.";
			d["item.elevation.label"] = "Elevation";
			d["item.elevation.desc"] = "Remembers the elevation a tool was left at, including where Increase elevation and Decrease elevation took it. The elevation of intersections and crossings is not covered.";
			d["item.parallel.label"] = "Parallel Mode";
			d["item.parallel.desc"] = "Remembers the Parallel Mode switch (Toggle parallel mode) together with the Parallel Road count and the Parallel Offset spacing that Increase offset and Decrease offset change. Parallel Mode builds parallel networks.";
			d["item.snap.label"] = "Snapping";
			d["item.snap.desc"] = "Remembers every switch on the Snapping row: Snap to existing geometry, Snap to zoning cell length, Snap to 90 degree angles, Snap to the sides of a road, Snap to roads, Snap to the side of the owner, Snap to the sides of a building, Snap to the middle of a road, Snap to shoreline, Snap to nearby geometry, Snap to guide lines, Snap to zone grid, Snap to nodes, Snap to the surface of an object, Snap upright, Snap to lot grid, Binds overlapping items to a building, Remove only matching type, Show contour lines and Snap to distance. The game's own Toggle all snapping on/off row is a batch switch and is not remembered.";
			d["item.topography.label"] = "Topography";
			d["item.topography.desc"] = "Remembers the Topography row, that is whether Show contour lines is on.";
			d["item.elevationStep.label"] = "Elevation step";
			d["item.elevationStep.desc"] = "Remembers the Elevation step value, the amount one press of Increase elevation or Decrease elevation moves you.";
			d["item.leftRight.label"] = "Left & Right Sides";
			d["item.leftRight.desc"] = "Remembers which extra side networks are picked on the Left and Right rows of the Extra Networks and Areas panel, for example Bicycle Lane, Trees, Parking, Quay, Retaining Wall and Sound Barrier. Needs that mod; without it this item does nothing.";
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Remembers the switches on the General panel of that same mod: force ground mode, tunnel mode, elevated mode and the removed height limit. Needs that mod.";
			d["item.underground.label"] = "Underground Mode";
			d["item.underground.desc"] = "Remembers the Underground Mode switch (Toggle underground mode), which decides whether what you place goes below the ground or on top of it.";
			d["item.other.label"] = "Misc";
			d["item.other.desc"] = "Remembers the other rows a tool panel can offer: Brush Size, Brush Strength, Brush Angle, Target Height, Distance, Color, Age (Sapling, Young, Mature, Elderly) and Preserve Age.";
			return d;
		}

		// ======================= de-DE =======================

		private static Dictionary<string, string> De()
		{
			Dictionary<string, string> d = Frames("de-DE");
			d["mod.name"] = "Werkzeugmodus-Gedächtnis";
			d["tab.mod"] = "Werkzeugmodus-Einstellungen";
			d["tab.about"] = "Über";
			d["group.items"] = "Werkzeuggedächtnis-Einstellungen";
			d["group.reset"] = "Speicherverwaltung";
			d["group.compat"] = "Kompatibilität";
			d["group.about"] = "Informationen und Links";

			d["enabled.label"] = "Werkzeugmodus-Gedächtnis aktivieren";
			d["enabled.desc"] = "Solange dies aktiviert ist, wird jeder Wert fortlaufend aufgezeichnet; beim nächsten Laden eines Spielstands sind die Panels wieder so, wie du sie verlassen hast. Ein einzelner Punkt deaktiviert nur seine Wiederherstellung - die Werte werden weiterhin aufgezeichnet. Nur wenn du diesen Hauptschalter ausschaltst, endet jede Aufzeichnung und jedes Werkzeug verhält sich wieder wie im Original.";
			d["compat.label"] = "Kompatibel mit anderen Mods";
			d["compat.desc"] = "Aktiviert: Das Gedächtnis nutzt die von anderen Mods (Asset UI Manager, ExtraLib und ähnlich) angepassten Menü- und Gruppennamen, ein verschobenes Asset folgt also seinem neuen Platz. Deaktiviert: Die zuerst gesehene Einordnung bleibt erhalten, was stabiler ist. In beiden Fällen gehen bereits aufgezeichnete Werte nicht verloren.";
			d["scope.label"] = "Gemeinsamer Bereich";
			d["scope.desc"] = "Wie weit dieser Wert geteilt wird. Die Klammer zeigt Original und Empfehlung.";
			d["scope.defs"] = "Gemeinsame Bereiche\nSelbe Gruppe: Hebst du die Höhe einer zweispurigen kleinen Straße auf 10 m, gehen die anderen kleinen Straßen ebenfalls auf 10 m, große Straßen bleiben unberührt.\nSelbes Menü: Jedes Asset im Straßenmenü teilt den Wert; im Strommenü nicht.\nSelber Asset-Typ: Nur Assets oder Funktionen derselben Unterkategorie teilen ihn. Eine andere große Straße teilt weiterhin die 10 m, aber eine Brücke springt auf 0 m und braucht eine eigene Einstellung, obwohl beide im Straßenmenü liegen. Funktionen meint hier Zonen, Räume und Flächen, Terraforming, Markierung und Objekt-Prefabs, also die Werkzeuge, die keine Assets sind.\nGlobal geteilt: Jedes Asset und jede Funktion, die die Eingabe unterstützt, teilt einen Wert - aber Assets und Funktionen nie untereinander.\nGar nicht geteilt: Jedes Asset und jede Funktion behält den eigenen Wert.";

			d[kScopeGroup] = "Selbe Gruppe";
			d[kScopeMenu] = "Selbes Menü";
			d[kScopeCategory] = "Selber Asset-Typ";
			d[kScopeGlobalShared] = "Global geteilt";
			d[kScopeGlobalUnique] = "Gar nicht geteilt";
			d[kTagVanilla] = " (Original)";
			d[kTagRecommended] = " (empfohlen)";
			d[kTagRecVanilla] = " (empfohlen, Original)";

			d["reset.label"] = "Speicher zurücksetzen";
			d["reset.desc"] = "Im Spiel: das Gedächtnis dieses Spielstands löschen und die Werkzeuge in den Originalzustand beim Laden versetzen. Im Hauptmenü: das Gedächtnis aller Spielstände löschen.";
			d["reset.warn"] = "Das kann nicht rückgängig gemacht werden.";
			d["reset.confirm"] = "Alle gemerkten Werkzeugeinstellungen zurücksetzen?";
			d["resetall.label"] = "Alle empfohlenen Standardwerte wiederherstellen";
			d["resetall.desc"] = "Setzt jede Option dieses Mods auf den empfohlenen Standard zurück: Hauptschalter, Kompatibilität sowie Aktivierung und gemeinsamer Bereich jedes Punkts. Das bereits gespeicherte Gedächtnis der Spielstände bleibt unberührt.";
			d["resetall.warn"] = "Das kann nicht rückgängig gemacht werden.";
			d["resetall.confirm"] = "Alle empfohlenen Standardwerte wiederherstellen?";
			d["folder.label"] = "Gedächtnisdateien aller Spielstände verwalten";
			d["folder.desc"] = "Öffnet den lokalen Ordner mit den Gedächtnisdateien, eine pro Spielstand, benannt wie der Spielstand.";

			d["about.version"] = "Mod-Version";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Kauf mir einen Kaffee";
			d["about.kofi.desc"] = "Unterstütze den Autor auf Ko-fi.";
			d["about.forum"] = "Forenseite";
			d["about.forum.desc"] = "Den Thread im Paradox-Forum öffnen.";
			d["about.rainbow"] = "RAINBOW-Website";
			d["about.rainbow.desc"] = "Die Website der Rainbow-Reihe öffnen.";

			d["item.anarchy.desc"] = "Merkt den eigenen An-/Ausschalter des Anarchy-Mods (Paradox-Mod 74604) für das gerade genutzte Werkzeug. Optionen wie die aufgehobene Höhenbegrenzung gehören zu diesem Mod, nicht zum Spiel. Ohne Anarchy passiert hier nichts.";
			d["item.packs.label"] = "Paket";
			d["item.packs.desc"] = "Merkt die Häkchen der Zeile Paket in den Werkzeugoptionen: Alle Pakete ein-/ausschalten, Basisspiel, Asset-Mods.";
			d["item.themes.label"] = "Thema";
			d["item.themes.desc"] = "Merkt die Auswahl der Zeile Thema. Das Angebot hängt von der Asset-Kategorie ab - bei Gebäuden zum Beispiel Europäisch, Nordamerikanisch, Moderne Architektur, Städtische Promenaden.";
			d["item.toolMode.label"] = "Werkzeugmodus";
			d["item.toolMode.desc"] = "Merkt den Modus jedes Werkzeugs, und die Optionen unterscheiden sich: Straßen, Gleise und Leitungen bieten Gerade, Einfache Kurve, Komplexe Kurve, Durchgehend, Raster, Ersetzen, Punkt; Gebäude, Objekte und Bäume bieten Einzeln platzieren, Mehrfach platzieren, Linie, Kurve, Objektstempel-Werkzeug; Zonen bieten Füllen, Auswahlrechteck, Farbe; Flächen bieten Bearbeiten, Kartenraster erstellen. Modi von Assets und Modi von Funktionen werden getrennt gemerkt, weil ihre Optionen nicht dieselben sind.";
			d["item.elevation.label"] = "Höhe";
			d["item.elevation.desc"] = "Merkt die Höhe, mit der ein Werkzeug verlassen wurde, einschließlich des Ergebnisses von Erhöhen und Absenken. Die Höhe von Kreuzungen und Verzweigungen ist nicht enthalten.";
			d["item.parallel.label"] = "Parallelmodus";
			d["item.parallel.desc"] = "Merkt den Schalter Parallelmodus (Parallelmodus an-/abschalten) zusammen mit der Anzahl der Parallelstraße und dem Parallelabstand, den Abstand vergrößern und Abstand verringern ändern. Baue parallele Verkehrswege.";
			d["item.snap.label"] = "Einrasten";
			d["item.snap.desc"] = "Merkt jeden Schalter der Zeile Einrasten: An existierender Geometrie einrasten, An Rasterzellenlänge einrasten, Bei 90-Grad-Winkeln einrasten, An Straßenkanten einrasten, An Straßen einrasten, An der Seite des Besitzers einrasten, An den Gebäudekanten einrasten, In der Mitte einer Straße einrasten, An der Küste einrasten, An nahegelegener Geometrie einrasten, An Hilfslinien einrasten, An Zellenraster einrasten, An Knoten einrasten, An der Oberfläche eines Objekts einrasten, Aufrecht einrasten, An Flächenraster einrasten, Bindet überlappende Objekte an ein Gebäude, Nur entsprechenden Typ entfernen, Nur Höhenlinien anzeigen, An Distanz einrasten. Die Zeile Einrasten für alle ein-/ausschalten ist ein Sammlerschalter und wird selbst nicht gemerkt.";
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Merkt die Zeile Topographie, also ob Nur Höhenlinien anzeigen aktiv ist.";
			d["item.elevationStep.label"] = "Höhenunterschied";
			d["item.elevationStep.desc"] = "Merkt den Wert von Höhenunterschied: wie viel ein einzelnes Erhöhen oder Absenken verschiebt.";
			d["item.leftRight.label"] = "Links & Rechts";
			d["item.leftRight.desc"] = "Merkt die zusätzlichen Netzwerkzeilen, die im Panel Extra Networks and Areas für Links und Rechts gewählt sind, zum Beispiel Fahrradweg, Bäume, Parken, Kai, Stützwand, Schallschutzmauer. Braucht diesen Mod; ohne ihn tut dieser Punkt nichts.";
			d["item.general.label"] = "Allgemein";
			d["item.general.desc"] = "Merkt die Schalter im Panel Allgemein desselben Mods: Boden, Tunnel, Erhöht, Erweiterter Höhenbereich. Braucht diesen Mod.";
			d["item.underground.label"] = "Untergrundmodus";
			d["item.underground.desc"] = "Merkt den Schalter Untergrundmodus (Untergrundmodus an-/abschalten): ob du etwas unter der Erde oder darüber platzierst.";
			d["item.other.label"] = "Versch.";
			d["item.other.desc"] = "Merkt die restlichen Zeilen, die ein Werkzeugpanel bieten kann: Pinselgröße, Pinselstärke, Pinsel-Winkel, Zielhöhe, Distanz, Farbe, Alter (Setzling, Jung, Ausgewachsen, Alt), Alter beibehalten.";
			return d;
		}

		// ======================= es-ES =======================

		private static Dictionary<string, string> Es()
		{
			Dictionary<string, string> d = Frames("es-ES");
			d["mod.name"] = "Memoria de herramientas";
			d["tab.mod"] = "Ajustes del modo de herramienta";
			d["tab.about"] = "Acerca de";
			d["group.items"] = "Ajustes de la memoria de herramientas";
			d["group.reset"] = "Gestión de la memoria";
			d["group.compat"] = "Compatibilidad";
			d["group.about"] = "Información y enlaces";

			d["enabled.label"] = "Activar Memoria de modo de herramienta";
			d["enabled.desc"] = "Mientras esté activado, todos los valores se registran sin parar, así que al volver a una partida los paneles quedan como los dejaste. Desactivar un solo elemento solo impide restaurarlo a él; sus valores siguen registrándose. Solo al desactivar este interruptor general se deja de registrar y cada herramienta vuelve a comportarse como en el juego original.";
			d["compat.label"] = "Compatible con otros mods";
			d["compat.desc"] = "Activado: la memoria se clasifica según los nombres de menú y grupo tal como los ajustan otros mods (Asset UI Manager, ExtraLib y similares), de modo que un activo que moviste sigue su nueva ubicación. Desactivado: se mantiene la clasificación que este mod vio primero, que es más estable. Ninguna de las dos opciones borra los valores ya registrados.";
			d["scope.label"] = "Ámbito compartido";
			d["scope.desc"] = "Hasta dónde se comparte este valor. Los paréntesis indican qué hace el juego original y qué recomendamos.";
			d["scope.defs"] = "Ámbitos de uso\nMismo grupo: sube la elevación de una calle pequeña de dos carriles a 10 m y las otras calles pequeñas también quedan a 10 m, mientras las grandes no cambian.\nMismo menú: cualquier activo del menú de carreteras la comparte; al pasar al menú de electricidad ya no.\nMismo tipo de activo: solo la comparten activos o funciones de la misma subcategoría. Cambiar a otra calle grande sigue compartiendo los 10 m, pero un puente vuelve a 0 m y necesita su propio ajuste, aunque los dos estén en el menú de carreteras. Funciones significa Zonas, Espacios y áreas, Terraformación, Marcadores y prefabricados de objeto, es decir, las herramientas que no son activos.\nGlobal: cada activo y función que admita la opción comparte un valor, pero los activos y las funciones nunca comparten entre sí.\nSin compartir: cada activo y función guarda su propio valor.";

			d[kScopeGroup] = "Mismo grupo";
			d[kScopeMenu] = "Mismo menú";
			d[kScopeCategory] = "Mismo tipo de activo";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Sin compartir";
			d[kTagVanilla] = " (original)";
			d[kTagRecommended] = " (recomendado)";
			d[kTagRecVanilla] = " (recomendado, original)";

			d["reset.label"] = "Restablecer memoria";
			d["reset.desc"] = "En partida: borra la memoria de esta partida y devuelve las herramientas al estado original de al cargar. En el menú principal: borra la memoria de todas las partidas.";
			d["reset.warn"] = "No se puede deshacer.";
			d["reset.confirm"] = "¿Restablecer los ajustes de herramienta recordados?";
			d["resetall.label"] = "Restaurar todos los valores recomendados";
			d["resetall.desc"] = "Devuelve cada opción de este mod a su valor recomendado: el interruptor general, el de compatibilidad y el estado y ámbito compartido de cada elemento. La memoria ya registrada de las partidas no se toca.";
			d["resetall.warn"] = "No se puede deshacer.";
			d["resetall.confirm"] = "¿Restaurar todos los ajustes a sus valores recomendados?";
			d["folder.label"] = "Gestionar los archivos de memoria de todas las partidas";
			d["folder.desc"] = "Abre la carpeta local con los archivos de memoria, uno por partida y con el nombre de la partida.";

			d["about.version"] = "Versión del mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Invítame a un café";
			d["about.kofi.desc"] = "Apoya al autor en Ko-fi.";
			d["about.forum"] = "Página del foro";
			d["about.forum.desc"] = "Abrir el hilo en el foro de Paradox.";
			d["about.rainbow"] = "Sitio de RAINBOW";
			d["about.rainbow.desc"] = "Abrir el sitio de la serie Rainbow.";

			d["item.anarchy.desc"] = "Recuerda el interruptor propio del mod Anarchy (mod 74604 de Paradox) para la herramienta en uso. Opciones como quitar el límite de altura pertenecen a ese mod, no al juego. Sin Anarchy instalado esto no hace nada.";
			d["item.packs.label"] = "Paquete";
			d["item.packs.desc"] = "Recuerda las casillas de la fila Paquete de Opciones de herramientas: Activar/desactivar todos los paquetes, Juego base y Mods de activos.";
			d["item.themes.label"] = "Temática";
			d["item.themes.desc"] = "Recuerda la fila Temática. Lo que se ofrece depende de la categoría del activo; los edificios, por ejemplo, muestran Europeo, Norteamericano, Arquitectura moderna y Paseos urbanos.";
			d["item.toolMode.label"] = "Herramientas";
			d["item.toolMode.desc"] = "Recuerda el modo elegido para cada herramienta, y las opciones cambian según la herramienta: carreteras, vías y tuberías ofrecen Recta, Curva sencilla, Curva compleja, Continua, Cuadrícula, Reemplazar y Punto; edificios, accesorios y árboles ofrecen Colocar uno, Colocar varios, Línea, Curva y Herramienta de sello de objetos; las zonas ofrecen Rellenar, Seleccionar y Pintar; las áreas ofrecen Editar y Generar cuadrícula de mapa. Los modos de activos y los de funciones se guardan por separado porque sus opciones no son las mismas.";
			d["item.elevation.label"] = "Elevación";
			d["item.elevation.desc"] = "Recuerda la elevación con la que dejaste la herramienta, incluido el resultado de Aumentar la elevación y Disminuir la elevación. No cubre la elevación de intersecciones y cruces.";
			d["item.parallel.label"] = "Modo paralelo";
			d["item.parallel.desc"] = "Recuerda el interruptor Modo paralelo (Alternar modo paralelo) junto con la cantidad de Calle paralela y la Compensación paralela que cambian Aumentar la compensación y Disminuir la compensación. Permite Construye redes paralelas.";
			d["item.snap.label"] = "Ajustar";
			d["item.snap.desc"] = "Recuerda cada interruptor de la fila Ajustar: Ajustar a una geometría establecida, Ajustar al largo de la celda de zonificación, Ajustar a ángulos de 90 grados, Ajustar a los lados de una carretera, Ajustar a las carreteras, Ajustar al lado del propietario, Ajustar a los lados del edificio, Ajustar a la mitad de la carretera, Ajustar a la orilla, Ajustar a una geometría cercana, Ajustar a las líneas de guía, Ajustar a la cuadrícula de la zona, Ajustar a los nodos, Ajustar a la superficie de un objeto, Ajustar en vertical, Ajustar a la cuadrícula del solar, Une los objetos superpuestos a un edificio, Elimina solo el tipo coincidente, Muestra las líneas de contorno y Ajustar a la distancia. La fila Activar/desactivar todos los ajustes del juego es un interruptor múltiple y no se recuerda.";
			d["item.topography.label"] = "Topografía";
			d["item.topography.desc"] = "Recuerda la fila Topografía, es decir, si Muestra las líneas de contorno está activo.";
			d["item.elevationStep.label"] = "Escalón de elevación";
			d["item.elevationStep.desc"] = "Recuerda el valor de Escalón de elevación, la distancia que recorres con un pulsar de Aumentar la elevación o Disminuir la elevación.";
			d["item.leftRight.label"] = "Izquierda y derecha";
			d["item.leftRight.desc"] = "Recuerda las redes laterales elegidas en las filas Izquierda y Derecha del panel Extra Networks and Areas, por ejemplo Vía ciclista, Árboles, Aparcamiento, Muelle, Muro de Retención y Barrera acústica. Necesita ese mod; sin él esto no hace nada.";
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Recuerda los interruptores del panel General de ese mismo mod: Terreno, Túnel, Elevado y Rango de Elevación Expandido. Necesita ese mod.";
			d["item.underground.label"] = "Modo subterráneo";
			d["item.underground.desc"] = "Recuerda el interruptor Modo subterráneo (Alternar modo subterráneo), que decide si lo que colocas va bajo tierra o encima.";
			d["item.other.label"] = "Varios";
			d["item.other.desc"] = "Recuerda las demás filas que un panel de herramienta puede ofrecer: Tamaño de pincel, Fuerza de pincel, Ángulo del pincel, Altura objetivo, Distancia, Color, Edad (Brote, Joven, Maduro, Anciano) y Preservar edad.";
			return d;
		}

		// ======================= fr-FR =======================

		private static Dictionary<string, string> Fr()
		{
			Dictionary<string, string> d = Frames("fr-FR");
			d["mod.name"] = "Mémoire du mode d'outil";
			d["tab.mod"] = "Réglages du mode d'outil";
			d["tab.about"] = "À propos";
			d["group.items"] = "Réglages de la mémoire d'outil";
			d["group.reset"] = "Gestion de la mémoire";
			d["group.compat"] = "Compatibilité";
			d["group.about"] = "Informations et liens";

			d["enabled.label"] = "Activer la mémoire du mode d'outil";
			d["enabled.desc"] = "Tant que c'est activé, chaque valeur est enregistrée en continu, et le panneau retrouve l'état où vous l'avez laissé en revenant dans la partie. Désactiver un seul élément arrête seulement sa restauration ; ses valeurs continuent d'être enregistrées. Seule la coupure de cet interrupteur général stoppe l'enregistrement et rend chaque outil à son comportement d'origine.";
			d["compat.label"] = "Compatible avec d'autres mods";
			d["compat.desc"] = "Activé : la mémoire se classe selon les noms de menu et de groupe tels que d'autres mods (Asset UI Manager, ExtraLib et similaires) les ont ajustés ; un asset déplacé suit donc son nouvel emplacement. Désactivé : la classification vue pour la première fois est conservée, ce qui est plus stable. Dans les deux cas, les valeurs déjà enregistrées ne sont pas perdues.";
			d["scope.label"] = "Portée de partage";
			d["scope.desc"] = "Jusqu'où cette valeur est partagée entre les assets. Les parenthèses indiquent le comportement d'origine et notre recommandation.";
			d["scope.defs"] = "Portées de partage\nMême groupe : montez l'élévation d'une petite rue à deux voies à 10 m, les autres petites rues passent aussi à 10 m, les grandes rues ne changent pas.\nMême menu : tout asset du menu Routes la partage ; passez au menu Électricité et ce n'est plus le cas.\nMême type d'actif : seuls les assets ou fonctions de la même sous-catégorie la partagent. Passer à une autre grande rue partage toujours les 10 m, mais un pont repart à 0 m et réclame son propre réglage, bien que les deux soient dans le menu Routes. Par fonctions, on entend Zones, Espaces et aires, Terraformation, Marqueur et objets préfabriqués, c'est-à-dire les outils qui ne sont pas des assets.\nGlobal : tout asset et toute fonction compatibles partagent une seule valeur, mais les assets et les fonctions ne partagent jamais entre eux.\nAucun partage : chaque asset et chaque fonction garde sa propre valeur.";

			d[kScopeGroup] = "Même groupe";
			d[kScopeMenu] = "Même menu";
			d[kScopeCategory] = "Même type d'actif";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Aucun partage";
			d[kTagVanilla] = " (d'origine)";
			d[kTagRecommended] = " (recommandé)";
			d[kTagRecVanilla] = " (recommandé, d'origine)";

			d["reset.label"] = "Réinitialiser la mémoire";
			d["reset.desc"] = "En partie : efface la mémoire de cette partie et rend les outils à l'état d'origine de la charge. Au menu principal : efface la mémoire de toutes les parties.";
			d["reset.warn"] = "Irréversible.";
			d["reset.confirm"] = "Réinitialiser les réglages d'outil mémorisés ?";
			d["resetall.label"] = "Rétablir tous les réglages recommandés";
			d["resetall.desc"] = "Remet chaque option de ce mod à son réglage recommandé : l'interrupteur général, celui de compatibilité, et l'état comme la portée de partage de chaque élément. La mémoire déjà enregistrée pour les parties n'est pas touchée.";
			d["resetall.warn"] = "Irréversible.";
			d["resetall.confirm"] = "Rétablir tous les réglages à leur valeur recommandée ?";
			d["folder.label"] = "Gérer les fichiers mémoire de toutes les parties";
			d["folder.desc"] = "Ouvre le dossier local des fichiers mémoire, un par partie, nommé d'après la partie.";

			d["about.version"] = "Version du mod";
			d["about.author"] = "Auteur";
			d["about.kofi"] = "Offrez-moi un café";
			d["about.kofi.desc"] = "Soutenez l'auteur sur Ko-fi.";
			d["about.forum"] = "Page du forum";
			d["about.forum.desc"] = "Ouvrir le fil sur le forum Paradox.";
			d["about.rainbow"] = "Site RAINBOW";
			d["about.rainbow.desc"] = "Ouvrir le site de la série Rainbow.";

			d["item.anarchy.desc"] = "Mémorise le bouton activé/désactivé propre au mod Anarchy (mod Paradox 74604) pour l'outil en cours. Des options telles que la suppression de la limite de hauteur appartiennent à ce mod, pas au jeu. Sans Anarchy installé, cette règle ne fait rien.";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Mémorise les cases de la ligne Pack des Options outil : Activation/désactivation de tous les packs, Jeu de base et Mods d'assets.";
			d["item.themes.label"] = "Thème";
			d["item.themes.desc"] = "Mémorise la ligne Thème. Ce qui est proposé dépend de la catégorie d'asset ; les bâtiments listent par exemple Européen, Nord-américain, Architecture moderne et Promenades urbaines.";
			d["item.toolMode.label"] = "Mode Outil";
			d["item.toolMode.desc"] = "Mémorise le mode choisi pour chaque outil, et les options varient : routes, rails et canalisations proposent Droit, Courbe simple, Courbe complexe, Continu, Grille, Remplacer et Point ; bâtiments, accessoires et arbres proposent En placer un, En placer plusieurs, Ligne, Courbe et Outil de marquage d'objet ; les zones proposent Remplissage, Sélection et Peinture ; les périmètres proposent Modifier et Générer une grille de carte. Les modes des assets et ceux des fonctions sont mémorisés séparément, car leurs options diffèrent.";
			d["item.elevation.label"] = "Élévation";
			d["item.elevation.desc"] = "Mémorise l'élévation laissée sur l'outil, y compris le résultat de Augmenter l'élévation et Diminuer l'élévation. L'élévation des intersections et des croisements n'est pas concernée.";
			d["item.parallel.label"] = "Mode parallèle";
			d["item.parallel.desc"] = "Mémorise l'interrupteur Mode parallèle (Changer le mode parallèle) avec le nombre de Route parallèle et la Compensation parallèle qu'Augmenter la compensation et Diminuer la compensation modifient. Permet de construire des réseaux parallèles.";
			d["item.snap.label"] = "Accrocher";
			d["item.snap.desc"] = "Mémorise chaque interrupteur de la ligne Accrocher : Accrochez à la géométrie existante, Accrochez à la longueur de la cellule de zonage, Accrochez aux angles à 90 degrés, Accrochez aux côtés d'une route, Accrochez aux routes, Accrochez au côté du propriétaire, Accrochez aux côtés d'un bâtiment, Accrochez au milieu d'une route, Accrochez au rivage, Accrochez à la géométrie à proximité, Accrochez aux lignes directrices, Accrochez à la grille de zone, Accrochez aux nœuds, Accrochez à la surface d'un objet, Accrochez verticalement, Accrochez à la grille des emplacements, Lie les éléments qui se chevauchent à un bâtiment, Supprimer uniquement le type correspondant, Afficher les lignes de contour et Accrochez à distance. La ligne Activer/désactiver tous les accrochages du jeu est un interrupteur groupé et n'est pas mémorisée.";
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Mémorise la ligne Topographie, c'est-à-dire si Afficher les lignes de contour est actif.";
			d["item.elevationStep.label"] = "Étape d'élévation";
			d["item.elevationStep.desc"] = "Mémorise la valeur Étape d'élévation, c'est-à-dire de combien bouge une seule pression de Augmenter l'élévation ou Diminuer l'élévation.";
			d["item.leftRight.label"] = "Gauche et droite";
			d["item.leftRight.desc"] = "Mémorise les réseaux latéraux choisis sur les lignes Gauche et Droite du panneau Extra Networks and Areas, par exemple Piste cyclable, Arbres, Stationnement, Quai, Mur de soutènement et Barrière acoustique. Nécessite ce mod ; sinon cette règle ne fait rien.";
			d["item.general.label"] = "Général";
			d["item.general.desc"] = "Mémorise les interrupteurs du panneau Général du même mod : Au sol, Tunnel, Élévation et Gamme d'élévation élargie. Nécessite ce mod.";
			d["item.underground.label"] = "Mode souterrain";
			d["item.underground.desc"] = "Mémorise l'interrupteur Mode souterrain (Changer le mode souterrain), qui décide si ce que vous placez passe sous le sol ou dessus.";
			d["item.other.label"] = "Divers";
			d["item.other.desc"] = "Mémorise les autres lignes qu'un panneau d'outil peut proposer : Taille du pinceau, Force du pinceau, Angle du pinceau, Hauteur cible, Distance, Couleur, Âge (Jeune arbre, Jeune, Adulte, Âgé) et Préserver l'âge.";
			return d;
		}

		// ======================= it-IT =======================

		private static Dictionary<string, string> It()
		{
			Dictionary<string, string> d = Frames("it-IT");
			d["mod.name"] = "Memoria modalità strumento";
			d["tab.mod"] = "Impostazioni modalità strumento";
			d["tab.about"] = "Informazioni";
			d["group.items"] = "Impostazioni memoria strumenti";
			d["group.reset"] = "Gestione memoria";
			d["group.compat"] = "Compatibilità";
			d["group.about"] = "Informazioni e collegamenti";

			d["enabled.label"] = "Abilita Memoria modalità strumento";
			d["enabled.desc"] = "Finché è attivo, ogni valore viene registrato di continuo e al ritorno in partita i pannelli sono esattamente come li hai lasciati. Disattivare una singola voce ne blocca solo il ripristino; i valori continuano a essere registrati. Solo spegnendo questo interruttore generale si interrompe ogni registrazione e ogni strumento torna al comportamento originale.";
			d["compat.label"] = "Compatibile con altri mod";
			d["compat.desc"] = "Attivo: la memoria usa i nomi di menu e gruppo come adattati da altri mod (Asset UI Manager, ExtraLib e simili), quindi un asset spostato segue la sua nuova posizione. Disattivo: viene mantenuta la classificazione che questo mod ha visto per prima, più stabile. In entrambi i casi i valori già registrati non vengono persi.";
			d["scope.label"] = "Ambito di condivisione";
			d["scope.desc"] = "Quanto è condiviso questo valore tra gli asset. Le parentesi mostrano cosa fa l'originale e cosa consigliamo.";
			d["scope.defs"] = "Ambiti di condivisione\nStesso gruppo: porta la quota di una piccola strada a due corsie a 10 m e anche le altre piccole strade vanno a 10 m, mentre le strade grandi restano invariate.\nStesso menu: qualsiasi asset nel menu Strade la condivide; passando al menu Elettricità no.\nStesso tipo di asset: solo asset o funzioni della stessa sottocategoria la condividono. Passare a un'altra strada grande condivide ancora i 10 m, ma un ponte riparte da 0 m e necessita un'impostazione propria, benché entrambi siano nel menu Strade. Per funzioni si intendono Zone, Aree e spazi, Terraformazione, Indicatore e oggetti prefabbricati, cioè gli strumenti che non sono asset.\nGlobale: ogni asset e funzione che supporta l'opzione condivide un solo valore, ma asset e funzioni non condividono mai tra loro.\nNon condiviso: ogni asset e ogni funzione mantiene il proprio valore.";

			d[kScopeGroup] = "Stesso gruppo";
			d[kScopeMenu] = "Stesso menu";
			d[kScopeCategory] = "Stesso tipo di asset";
			d[kScopeGlobalShared] = "Globale";
			d[kScopeGlobalUnique] = "Non condiviso";
			d[kTagVanilla] = " (originale)";
			d[kTagRecommended] = " (consigliato)";
			d[kTagRecVanilla] = " (consigliato, originale)";

			d["reset.label"] = "Reimposta memoria";
			d["reset.desc"] = "In partita: cancella la memoria di questa partita e riporta gli strumenti allo stato originale del caricamento. Nel menu principale: cancella la memoria di tutte le partite.";
			d["reset.warn"] = "Non annullabile.";
			d["reset.confirm"] = "Reimpostare le impostazioni degli strumenti memorizzate?";
			d["resetall.label"] = "Ripristina tutte le impostazioni consigliate";
			d["resetall.desc"] = "Riporta ogni opzione di questo mod al valore consigliato: interruttore generale, interruttore di compatibilità, stato e ambito di condivisione di ogni voce. La memoria già registrata delle partite non viene toccata.";
			d["resetall.warn"] = "Non annullabile.";
			d["resetall.confirm"] = "Ripristinare tutte le impostazioni al valore consigliato?";
			d["folder.label"] = "Gestisci i file di memoria di tutte le partite";
			d["folder.desc"] = "Apre la cartella locale dei file di memoria, uno per partita, con il nome della partita.";

			d["about.version"] = "Versione mod";
			d["about.author"] = "Autore";
			d["about.kofi"] = "Offrimi un caffè";
			d["about.kofi.desc"] = "Supporta l'autore su Ko-fi.";
			d["about.forum"] = "Pagina del forum";
			d["about.forum.desc"] = "Apri la discussione sul forum Paradox.";
			d["about.rainbow"] = "Sito RAINBOW";
			d["about.rainbow.desc"] = "Apri il sito della serie Rainbow.";

			d["item.anarchy.desc"] = "Ricorda il pulsante on/off proprio del mod Anarchy (mod Paradox 74604) per lo strumento in uso. Opzioni come la rimozione del limite di altezza appartengono a quel mod, non al gioco. Senza Anarchy installato questa voce non fa nulla.";
			d["item.packs.label"] = "Pacchetto";
			d["item.packs.desc"] = "Ricorda le spunte della riga Pacchetto in Opzioni strumento: Attiva/disattiva tutti i pacchetti, Gioco base, Mod di risorse.";
			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Ricorda la riga Tema. Cosa è disponibile dipende dalla categoria di asset; gli edifici, per esempio, elencano Europeo, Nordamericano, Architettura moderna e Passeggiate urbane.";
			d["item.toolMode.label"] = "Modalità strumento";
			d["item.toolMode.desc"] = "Ricorda la modalità scelta per ogni strumento e le opzioni cambiano a seconda dello strumento: strade, binari e tubi offrono Rettilineo, Curva semplice, Curva complessa, Continua, Griglia, Sostituisci, Punto; edifici, oggetti e alberi offrono Posiziona uno, Posiziona multipli, Linea, Curva, Strumento timbro oggetti; le zone offrono Riempi, Seleziona, Pittura; le aree offrono Modifica, Genera griglia mappa. Le modalità degli asset e quelle delle funzioni sono ricordate separatamente perché le opzioni non sono le stesse.";
			d["item.elevation.label"] = "Elevazione";
			d["item.elevation.desc"] = "Ricorda l'elevazione a cui lasci lo strumento, incluso il risultato di Aumenta elevazione e Diminuisci elevazione. L'elevazione di incroci e interscambi non è inclusa.";
			d["item.parallel.label"] = "Modalità parallelismo";
			d["item.parallel.desc"] = "Ricorda l'interruttore Modalità parallelismo (Attiva/disattiva modalità parallelismo) con il numero di Strada parallela e la Compensazione parallelismo che Aumenta compensazione e Diminuisci compensazione modificano. Permette di costruire reti parallele.";
			d["item.snap.label"] = "Aggancio";
			d["item.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio: Aggancia alla geometria esistente, Aggancia alla lunghezza della cella di zonizzazione, Aggancia ad angoli di 90°, Aggancia ai lati di una strada, Aggancia alle strade, Aggancia al lato del proprietario, Aggancia ai lati di un edificio, Aggancia al centro di una strada, Aggancia alla costa, Aggancia alla geometria vicina, Aggancia alle linee guida, Aggancia alla griglia della zona, Aggancia ai nodi, Aggancia alla superficie di un oggetto, Aggancia in verticale, Aggancia alla griglia del lotto, Collega elementi sovrapposti a un edificio, Rimuovi solo il tipo corrispondente, Mostra le linee di contorno, Aggancia a distanza. La riga Attiva/disattiva tutti gli agganci del gioco è un interruttore multiplo e non viene ricordata.";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Ricorda la riga Topografia, cioè se Mostra le linee di contorno è attivo.";
			d["item.elevationStep.label"] = "Livello elevazione";
			d["item.elevationStep.desc"] = "Ricorda il valore Livello elevazione, quanto sposta una singola pressione di Aumenta elevazione o Diminuisci elevazione.";
			d["item.leftRight.label"] = "Sinistra e destra";
			d["item.leftRight.desc"] = "Ricorda le reti laterali scelte sulle righe Sinistra e Destra del pannello Extra Networks and Areas, per esempio Pista ciclabile, Alberi, Parcheggio, Banchina, Muro di contenimento, Barriera insonorizzante. Richiede quel mod; senza, questa voce non fa nulla.";
			d["item.general.label"] = "Generale";
			d["item.general.desc"] = "Ricorda gli interruttori del pannello Generale dello stesso mod: Terreno, Tunnel, Elevato, Intervallo di elevazione ampliato. Richiede quel mod.";
			d["item.underground.label"] = "Modalità sottosuolo";
			d["item.underground.desc"] = "Ricorda l'interruttore Modalità sottosuolo (Attiva/disattiva modalità sottosuolo), che decide se ciò che piazzi va sotto terra o sopra.";
			d["item.other.label"] = "Varie";
			d["item.other.desc"] = "Ricorda le altre righe che un pannello strumento può offrire: Dimensione pennello, Robustezza pennello, Angolo pennello, Altezza bersaglio, Distanza, Colore, Età (Arbusto, Giovane, Adulto, Anziano), Mantieni età.";
			return d;
		}

		// ======================= ja-JP =======================

		private static Dictionary<string, string> Ja()
		{
			Dictionary<string, string> d = Frames("ja-JP");
			d["mod.name"] = "ツールモード記憶";
			d["tab.mod"] = "ツールモード設定";
			d["tab.about"] = "情報";
			d["group.items"] = "ツール記憶の設定";
			d["group.reset"] = "メモリ管理";
			d["group.compat"] = "互換性";
			d["group.about"] = "情報とリンク";

			d["enabled.label"] = "ツールモード記憶を有効化";
			d["enabled.desc"] = "有効な間はすべての値が継続的に記録されるので、セーブに戻ると離れた時そのままの状態で復元します。個別の項目をオフにしても、その項目が復元されなくなるだけで、値の記録は続きます。このマスタースイッチをオフにした時だけ記録が止まり、すべてのツールが標準の動作に戻ります。";
			d["compat.label"] = "他の Mod と互換";
			d["compat.desc"] = "オンにすると、他の Mod（Asset UI Manager、ExtraLib など）が調整後のメニューとグループ名で記憶します。移動させたアセットはその新しい場所に追従します。オフにすると、この Mod が見た最初の分類が使われ、より安定します。どちらでも既に記録された値は失われません。";
			d["scope.label"] = "共有範囲";
			d["scope.desc"] = "この値をアセット間でどこまで共有するか。括弧内は標準の動作と推奨を示します。";
			d["scope.defs"] = "共有範囲の説明\n同じグループ: 車道 2 本の小型道路の高さを 10 m にすると、他の小型道路も 10 m になり、大型道路はそのままです。\n同じメニュー: 道路メニューのどのアセットも共有しますが、電力メニューに切り替えると共有しません。\n同じアセット種別: 同じ下位分類のアセットまたは機能だけを共有します。別の大型道路に替えても 10 m は共有されますが、橋は 0 m に戻るため専用の設定が必要です。どちらも道路メニューに属していても同じです。ここでの機能とは、区画、スペースとエリア、テラフォーミング、マーカー、オブジェクト プレハブなど、アセットではないツールを指します。\n全体で共有: その項目に対応するすべてのアセットと機能が一つの値を共有しますが、アセットと機能の間では共有しません。\nまったく共有しない: 各アセット、各機能がそれぞれ専用の値を持ちます。";

			d[kScopeGroup] = "同じグループ";
			d[kScopeMenu] = "同じメニュー";
			d[kScopeCategory] = "同じアセット種別";
			d[kScopeGlobalShared] = "全体で共有";
			d[kScopeGlobalUnique] = "まったく共有しない";
			d[kTagVanilla] = "（標準）";
			d[kTagRecommended] = "（推奨）";
			d[kTagRecVanilla] = "（推奨・標準）";

			d["reset.label"] = "メモリをリセット";
			d["reset.desc"] = "ゲーム中: このセーブの記憶を消し、ツールを読み込み時の標準状態に戻します。メインメニュー: すべてのセーブの記憶を消します。";
			d["reset.warn"] = "元に戻せません。";
			d["reset.confirm"] = "記憶したツール設定をリセットしますか？";
			d["resetall.label"] = "すべての推奨既定値に戻す";
			d["resetall.desc"] = "この Mod のすべてのオプションを推奨の既定値に戻します。マスタースイッチ、互換スイッチ、各項目の有効無効と共有範囲が含まれます。セーブに既に記録された記憶はそのままです。";
			d["resetall.warn"] = "元に戻せません。";
			d["resetall.confirm"] = "すべての設定を推奨の既定値に戻しますか？";
			d["folder.label"] = "すべてのセーブのメモリファイルを管理";
			d["folder.desc"] = "記憶を保存しているローカルフォルダーを開きます。セーブごとに 1 ファイル、名前はセーブ名です。";

			d["about.version"] = "Mod バージョン";
			d["about.author"] = "作者";
			d["about.kofi"] = "コーヒーをおごる";
			d["about.kofi.desc"] = "Ko-fi で作者を支援する。";
			d["about.forum"] = "フォーラムページ";
			d["about.forum.desc"] = "Paradox フォーラムのスレッドを開く。";
			d["about.rainbow"] = "RAINBOW 公式サイト";
			d["about.rainbow.desc"] = "Rainbow Series の公式サイトを開く。";

			d["item.anarchy.desc"] = "使用中のツールに対する Anarchy Mod（Paradox Mod 74604）自身のオン/オフボタンを記憶します。高さ制限の解除などのオプションはその Mod のもので、本体の機能ではありません。Anarchy がない場合この項目は動作しません。";
			d["item.packs.label"] = "パック";
			d["item.packs.desc"] = "ツールオプションの「パック」行のチェックを記憶します: 全パックの有効・無効を切り替え、ベースゲーム、アセットMod。";
			d["item.themes.label"] = "テーマ";
			d["item.themes.desc"] = "「テーマ」行の選択を記憶します。選択肢はアセットのカテゴリーによって変わります。建物ではたとえば ヨーロッパ風、北アメリカ風、現代建築家、都会のプロムナード です。";
			d["item.toolMode.label"] = "ツールモード";
			d["item.toolMode.desc"] = "各ツールで選んだモードを記憶します。選択肢はツールにより異なります。道路・軌道・パイプは 直線、1カーブ、2カーブ、連続カーブ、グリッド、既存のものを他のタイプのものに建て替える、ポイント。建築物・小物・樹木は 1つ配置、複数配置、直線、カーブ、オブジェクトスタンプツール。区画は 塗りつぶし、選択、ブラシ。エリアは 編集、マップグリッドを生成。アセットのツールモードと機能のツールモードは選択肢が違うため分けて記憶します。";
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "ツールを離れた時の高度を記憶します。高度を上げる、高度を下げる の結果も含まれます。交差点や跨線橋の高度は対象外です。";
			d["item.parallel.label"] = "平行モード";
			d["item.parallel.desc"] = "平行モードのスイッチ（平行モードを切り替え）を、平行道路の本数と平行間隔（間隔を延ばす、間隔を縮める）と一緒に記憶します。平行なネットワークを建設します。";
			d["item.snap.label"] = "スナップ";
			d["item.snap.desc"] = "「スナップ」行のすべてのスイッチを記憶します: 既存の配置にスナップ、区画セルの長さにスナップ、90°にスナップ、道路の脇にスナップ、道路にスナップ、オーナーの面にスナップ、建物の壁面にスナップ、道路の中心にスナップ、海岸線にスナップ、最寄りの配置にスナップ、ガイドラインにスナップ、区画グリッドにスナップ、ノードにスナップ、オブジェクトの表面にスナップ、直立にスナップ、区画グリッドにスナップ、重なったアイテムを建物にまとめる、一致する種類のみを削除します、輪郭線を表示します、距離に応じてスナップ。本体の「全スナップを切り替え」は一括スイッチであり、記憶されません。";
			d["item.topography.label"] = "地形";
			d["item.topography.desc"] = "「地形」行、つまり輪郭線を表示します のオンオフを記憶します。";
			d["item.elevationStep.label"] = "高度ステップ";
			d["item.elevationStep.desc"] = "「高度ステップ」の値、つまり高度を上げる / 高度を下げる を 1 回押すといくつ動くかを記憶します。";
			d["item.leftRight.label"] = "左側と右側";
			d["item.leftRight.desc"] = "Extra Networks and Areas のパネルで左側・右側の各行に選んだ追加ネットワークを記憶します。たとえば 自転車レーン、街路樹、駐車場、岸壁、擁壁、防音壁。この Mod が必要で、ない場合この項目は動作しません。";
			d["item.general.label"] = "General";
			d["item.general.desc"] = "同じ Mod の General パネルのスイッチを記憶します: Ground、Tunnel、Elevated、Expanded Elevation Range。この Mod が必要です。";
			d["item.underground.label"] = "地下モード";
			d["item.underground.desc"] = "地下モードのスイッチ（地下モードを切り替え）を記憶します。配置するものが地下か地上かを決定します。";
			d["item.other.label"] = "その他";
			d["item.other.desc"] = "ツールパネルに出るその他の行を記憶します: ブラシサイズ、ブラシの強さ、ブラシの角度、目標の高さ、距離、色、年齢（苗木、若木、成木、老木）、時期の保持。";
			return d;
		}

		// ======================= ko-KR =======================

		private static Dictionary<string, string> Ko()
		{
			Dictionary<string, string> d = Frames("ko-KR");
			d["mod.name"] = "도구 모드 기억";
			d["tab.mod"] = "도구 모드 설정";
			d["tab.about"] = "정보";
			d["group.items"] = "도구 기억 설정";
			d["group.reset"] = "메모리 관리";
			d["group.compat"] = "호환성";
			d["group.about"] = "정보 및 링크";

			d["enabled.label"] = "도구 모드 기억 사용";
			d["enabled.desc"] = "켜 두는 동안 모든 값이 계속 기록되므로, 세이브로 돌아오면 종료할 때의 상태로 그대로 복원됩니다. 항목 하나만 끄면 그 항목만 복원되지 않고 값은 계속 기록됩니다. 이 마스터 스위치만 꺼야 기록이 멈추고 모든 도구가 원본 동작으로 돌아갑니다.";
			d["compat.label"] = "다른 모드와 호환";
			d["compat.desc"] = "켜면 다른 모드(Asset UI Manager, ExtraLib 등)가 조정한 메뉴와 그룹 이름으로 기억합니다. 이동한 에셋은 새 위치를 따릅니다. 끄면 이 모드가 처음 본 분류를 그대로 써서 더 안정적입니다. 두 경우 모두 이미 기록된 값은 사라지지 않습니다.";
			d["scope.label"] = "공유 범위";
			d["scope.desc"] = "이 값을 에셋 사이에서 얼마나 넓게 공유하는지. 괄호는 원본 동작과 권장 표시입니다.";
			d["scope.defs"] = "공유 범위 설명\n같은 그룹: 차선 2개 소형 도로의 고도를 10m로 올리면 다른 소형 도로도 10m가 되고, 대형 도로는 그대로입니다.\n같은 메뉴: 도로 메뉴의 모든 에셋이 공유하지만, 전기 메뉴로 바꾸면 공유하지 않습니다.\n같은 에셋 유형: 같은 하위 유형의 에셋 또는 기능만 공유합니다. 다른 대형 도로로 바뀌어도 10m는 계속 공유하지만, 교량은 0m로 돌아와 따로 설정해야 합니다. 둘 다 도로 메뉴에 속해도 마찬가지입니다. 여기서 기능이란 구역, 지구, 테라포밍, 마커, 객체 프리팹처럼 에셋이 아닌 도구를 말합니다.\n전역 공유: 해당 항목을 지원하는 모든 에셋과 기능이 하나의 값을 공유하지만, 에셋과 기능 사이에서는 공유하지 않습니다.\n전부 별도: 각 에셋, 각 기능이 각각의 값을 갖습니다.";

			d[kScopeGroup] = "같은 그룹";
			d[kScopeMenu] = "같은 메뉴";
			d[kScopeCategory] = "같은 에셋 유형";
			d[kScopeGlobalShared] = "전역 공유";
			d[kScopeGlobalUnique] = "전부 별도";
			d[kTagVanilla] = " (원작)";
			d[kTagRecommended] = " (권장)";
			d[kTagRecVanilla] = " (권장, 원작)";

			d["reset.label"] = "메모리 초기화";
			d["reset.desc"] = "게임 중: 이 세이브의 기억을 지우고 도구를 불러올 때의 원작 상태로 되돌립니다. 메인 메뉴: 모든 세이브의 기억을 지웁니다.";
			d["reset.warn"] = "되돌릴 수 없습니다.";
			d["reset.confirm"] = "기억된 도구 설정을 초기화할까요?";
			d["resetall.label"] = "모든 권장 기본값으로 복원";
			d["resetall.desc"] = "이 모드의 모든 옵션을 권장 기본값으로 되돌립니다. 마스터 스위치, 호환 스위치, 각 항목의 사용 여부와 공유 범위가 포함됩니다. 세이브에 이미 기록된 기억은 그대로 남습니다.";
			d["resetall.warn"] = "되돌릴 수 없습니다.";
			d["resetall.confirm"] = "모든 설정을 권장 기본값으로 복원할까요?";
			d["folder.label"] = "모든 세이브의 메모리 파일 관리";
			d["folder.desc"] = "기억을 저장하는 로컬 폴더를 엽니다. 세이브마다 파일 하나, 파일명은 세이브 이름입니다.";

			d["about.version"] = "Mod 버전";
			d["about.author"] = "저자";
			d["about.kofi"] = "커피 한 잔 사주기";
			d["about.kofi.desc"] = "Ko-fi에서 저자를 지원합니다.";
			d["about.forum"] = "포럼 페이지";
			d["about.forum.desc"] = "Paradox 포럼 스레드를 엽니다.";
			d["about.rainbow"] = "RAINBOW 웹사이트";
			d["about.rainbow.desc"] = "Rainbow Series 웹사이트를 엽니다.";

			d["item.anarchy.desc"] = "사용 중인 도구에 대한 Anarchy 모드(Paradox 모드 74604) 자체의 켜기/끄기 버튼을 기억합니다. 높이 제한 해제 같은 항목은 그 모드의 것이며 게임 기본 기능이 아닙니다. Anarchy가 없으면 이 항목은 동작하지 않습니다.";
			d["item.packs.label"] = "팩";
			d["item.packs.desc"] = "도구 옵션의 팩 행 체크 상태를 기억합니다: 모든 팩 설정 켬/끔, 기본 게임, 에셋 모드.";
			d["item.themes.label"] = "테마";
			d["item.themes.desc"] = "테마 행의 선택을 기억합니다. 선택지는 에셋 분류에 따라 달라집니다. 예를 들어 건물에서는 유럽, 북미, 현대 건축물, 도시형 산책로가 표시됩니다.";
			d["item.toolMode.label"] = "도구 모드";
			d["item.toolMode.desc"] = "각 도구에서 고른 모드를 기억합니다. 선택지는 도구마다 다릅니다. 도로, 철도, 배관은 직선, 단곡선, 복잡한 곡선, 연속, 그리드, 교체, 점. 건물, 소품, 나무는 단일 배치, 복수 배치, 선, 곡선, 객체 스탬프 도구. 구역은 채우기, 범위 지정, 칠하기. 지구는 편집, 지도 그리드 생성. 에셋의 도구 모드와 기능의 도구 모드는 선택지가 다르므로 따로 기억합니다.";
			d["item.elevation.label"] = "고도";
			d["item.elevation.desc"] = "도구를 종료할 때의 고도를 기억합니다. 고도 높이기, 고도 낮추기의 결과도 포함됩니다. 교차로와 입체 교차로의 고도는 대상이 아닙니다.";
			d["item.parallel.label"] = "평행 모드";
			d["item.parallel.desc"] = "평행 모드 스위치(평행 모드 켬/끔)를 평행 도로 수와 평행 오프셋 간격(오프셋 증가, 오프셋 감소)과 함께 기억합니다. 평행 네트워크를 건설합니다.";
			d["item.snap.label"] = "맞춤";
			d["item.snap.desc"] = "맞춤 행의 모든 스위치를 기억합니다: 기존 지오메트리에 맞춤, 구역 칸 길이에 맞춤, 90도 각에 맞춤, 도로 편에 맞춤, 도로에 맞춤, 소유주 편에 맞춤, 건물 편에 맞춤, 도로 중간에 맞춤, 해안선에 맞춤, 주변 지오메트리에 맞춤, 기준선에 맞춤, 구역망에 맞춤, 노드에 맞춤, 사물의 표면에 맞춤, 똑바로 보기, 부지 그리드에 맞춤, 아이템 오버랩을 건물과 통합합니다, 일치하는 유형만 제거합니다, 등고선을 표시합니다, 거리에 맞춤. 게임 자체의 모든 맞춤 설정 켬/끔 행은 일괄 스위치라서 기억되지 않습니다.";
			d["item.topography.label"] = "지형도";
			d["item.topography.desc"] = "지형도 행, 즉 등고선을 표시합니다 의 켬/꺼짐을 기억합니다.";
			d["item.elevationStep.label"] = "고도 단계";
			d["item.elevationStep.desc"] = "고도 단계 값을 기억합니다. 고도 높이기 / 고도 낮추기를 한 번 누를 때 움직이는 양입니다.";
			d["item.leftRight.label"] = "좌측과 우측";
			d["item.leftRight.desc"] = "Extra Networks and Areas 패널에서 좌측, 우측 행에 고른 추가 네트워크를 기억합니다. 예를 들어 자전거 도로, 나무, 주차, 축대, 옹벽, 방음벽. 해당 모드가 필요하며 없으면 이 항목은 동작하지 않습니다.";
			d["item.general.label"] = "일반";
			d["item.general.desc"] = "같은 모드의 일반 패널 스위치를 기억합니다: 지면, 터널, 고가 모드, 확장된 고도 범위. 해당 모드가 필요합니다.";
			d["item.underground.label"] = "지하 모드";
			d["item.underground.desc"] = "지하 모드 스위치(지하 모드 켜기/끄기)를 기억합니다. 배치하는 것이 지하인지 지상인지 결정합니다.";
			d["item.other.label"] = "기타";
			d["item.other.desc"] = "도구 패널에 나오는 나머지 행을 기억합니다: 브러시 크기, 브러시 강도, 브러시 각도, 목표물 높이, 거리, 색상, 나이(샘플링, 청년, 어른, 노인), 성장 동결.";
			return d;
		}

		// ======================= pl-PL =======================

		private static Dictionary<string, string> Pl()
		{
			Dictionary<string, string> d = Frames("pl-PL");
			d["mod.name"] = "Pamięć narzędzi";
			d["tab.mod"] = "Ustawienia trybu narzędzia";
			d["tab.about"] = "Informacje";
			d["group.items"] = "Ustawienia pamięci narzędzi";
			d["group.reset"] = "Zarządzanie pamięcią";
			d["group.compat"] = "Zgodność";
			d["group.about"] = "Informacje i linki";

			d["enabled.label"] = "Włącz pamięć trybu narzędzia";
			d["enabled.desc"] = "Dopóki to włączone, każda wartość jest zapisywana na bieżąco, więc po powrocie do zapisu panele wyglądają tak, jak je zostawiłeś. Wyłączenie pojedynczej pozycji tylko ją wyłącza z przywracania - jej wartości nadal są zapisywane. Dopiero wyłączenie tego głównego przełącznika zatrzymuje zapisowanie i każde narzędzie wraca do zachowania z gry.";
			d["compat.label"] = "Zgodność z innymi modami";
			d["compat.desc"] = "Włączone: pamięć używa nazw menu i grup w postaci, w jakiej dostosowały je inne mody (Asset UI Manager, ExtraLib i podobne), więc przeniesiony zasób podąża za nowym miejscem. Wyłączone: zachowywana jest klasyfikacja widziana po raz pierwszy, co jest stabilniejsze. W obu przypadkach już zapisane wartości nie giną.";
			d["scope.label"] = "Zakres współdzielenia";
			d["scope.desc"] = "Jak szeroko ta wartość jest współdzielona między zasobami. Nawiasy pokazują oryginał i zalecenie.";
			d["scope.defs"] = "Zakresy współdzielenia\nTa sama grupa: podnieś wysokość małej dwupasmowej drogi do 10 m, a inne małe drogi też będą mieć 10 m, duże drogi pozostaną bez zmian.\nTo samo menu: każdy zasób z menu Drogowe ją współdzieli, ale po przejściu do menu Elektryczność już nie.\nTen sam typ zasobu: współdzielą ją tylko zasoby lub funkcje z tej samej podkategorii. Inna duża droga nadal współdzieli 10 m, ale most wraca do 0 m i wymaga własnego ustawienia, mimo że obie są w menu Drogowe. Funkcje oznaczają tu Strefy, Tereny i obszary, Terraformowanie, Znacznik i prefaby obiektów, czyli narzędzia, które nie są zasobami.\nGlobalnie: każdy zasób i funkcja obsługujące tę opcję współdzielą jedną wartość, ale zasoby i funkcje nigdy między sobą.\nBez współdzielenia: każdy zasób i każda funkcja mają własną wartość.";

			d[kScopeGroup] = "Ta sama grupa";
			d[kScopeMenu] = "To samo menu";
			d[kScopeCategory] = "Ten sam typ zasobu";
			d[kScopeGlobalShared] = "Globalnie";
			d[kScopeGlobalUnique] = "Bez współdzielenia";
			d[kTagVanilla] = " (oryginał)";
			d[kTagRecommended] = " (zalecane)";
			d[kTagRecVanilla] = " (zalecane, oryginał)";

			d["reset.label"] = "Resetuj pamięć";
			d["reset.desc"] = "W grze: kasuje pamięć tego zapisu i przywraca narzędzia do stanu oryginalnego z wczytania. W menu głównym: kasuje pamięć wszystkich zapisów.";
			d["reset.warn"] = "Nie można cofnąć.";
			d["reset.confirm"] = "Zresetować zapamiętane ustawienia narzędzi?";
			d["resetall.label"] = "Przywróć wszystkie ustawienia zalecane";
			d["resetall.desc"] = "Przywraca każdą opcję tego moda do wartości zalecanej: przełącznik główny, zgodność oraz stan i zakres każdej pozycji. Pamięć już zapisana dla zapisów gry pozostaje nienaruszona.";
			d["resetall.warn"] = "Nie można cofnąć.";
			d["resetall.confirm"] = "Przywrócić wszystkie ustawienia do wartości zalecanych?";
			d["folder.label"] = "Zarządzaj plikami pamięci wszystkich zapisów";
			d["folder.desc"] = "Otwiera lokalny folder z plikami pamięci, jednym na zapis, nazwanym jak zapis.";

			d["about.version"] = "Wersja moda";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Postaw mi kawę";
			d["about.kofi.desc"] = "Wesprzyj autora na Ko-fi.";
			d["about.forum"] = "Strona na forum";
			d["about.forum.desc"] = "Otwórz wątek na forum Paradox.";
			d["about.rainbow"] = "Strona RAINBOW";
			d["about.rainbow.desc"] = "Otwórz stronę serii Rainbow.";

			d["item.anarchy.desc"] = "Zapamiętuje własny przełącznik moda Anarchy (mod Paradox 74604) dla używanego narzędzia. Takie opcje jak usunięcie limitu wysokości należą do tego moda, nie do gry. Bez Anarchy ta pozycja nic nie robi.";
			d["item.packs.label"] = "Pakiet";
			d["item.packs.desc"] = "Zapamiętuje zaznaczenia w wierszu Pakiet w Opcjach narzędzi: Wł./wył. wszystkie pakiety, Gra podstawowa, Mody zasobów.";
			d["item.themes.label"] = "Motyw";
			d["item.themes.desc"] = "Zapamiętuje wiersz Motyw. Co jest dostępne zależy od kategorii zasobu; budynki pokazują na przykład Styl europejski, Styl amerykański, Nowoczesna architektura, Miejskie deptaki.";
			d["item.toolMode.label"] = "Narzędzia";
			d["item.toolMode.desc"] = "Zapamiętuje tryb wybrany dla każdego narzędzia, a opcje różnią się między nimi: drogi, tory i rury oferują Prosta, Jeden zakręt, Dwa zakręty, Ciągła, Siatka, Zastąp, Punkt; budynki, obiekty i drzewa oferują Umieść jeden, Umieść wiele, Linia, Krzywa, Stempel; strefy oferują Wypełnienie, Zaznaczanie obszarowe, Pędzel; obszary oferują Edytuj, Generuj siatkę mapy. Tryby zasobów i tryby funkcji zapamiętywane są osobno, bo ich opcje nie są takie same.";
			d["item.elevation.label"] = "Wzniesienie";
			d["item.elevation.desc"] = "Zapamiętuje wysokość, na jakiej zostawiono narzędzie, wraz z wynikiem Zwiększ wzniesienie i Zmniejsz wzniesienie. Wysokość skrzyżowań i węzłów nie jest objęta.";
			d["item.parallel.label"] = "Tryb równoległy";
			d["item.parallel.desc"] = "Zapamiętuje przełącznik Tryb równoległy (Włącz/wyłącz tryb równoległy) razem z liczbą Równoległa droga i Równoległa kompensacja, które zmienia Zwiększ kompensację i Zmniejsz kompensację. Pozwala budować równoległe sieci.";
			d["item.snap.label"] = "Przyciąganie";
			d["item.snap.desc"] = "Zapamiętuje każdy przełącznik wiersza Przyciąganie: Przyciągaj do istniejących kształtów, Przyciągaj do wielkości pól w strefach, Przyciągaj do kątów prostych, Przyciągaj do stron drogi, Przyciągaj do dróg, Przyciągaj do strony właściciela, Przyciągaj do boków budynku, Przyciągaj do środka drogi, Przyciągaj do linii brzegowej, Przyciągaj do pobliskich kształtów, Przyciągaj do linii pomocniczych, Przyciągaj do siatki w strefie, Przyciągaj do węzłów, Przyciągaj do powierzchni obiektu, Przyciągaj do góry, Przyciągaj do siatki wysypiska, Wiąże nakładające się obiekty z budynkiem, Usuń tylko pasujący typ, Pokaż linie konturów, Przyciągaj do odległości. Wiersz Wł./wył. przyciąganie z gry to przełącznik zbiorczy i nie jest zapamiętywany.";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Zapamiętuje wiersz Topografia, czyli czy Pokaż linie konturów jest włączone.";
			d["item.elevationStep.label"] = "Stopień wzniesienia";
			d["item.elevationStep.desc"] = "Zapamiętuje wartość Stopień wzniesienia, czyli o ile przesuwa jedno naciśnięcie Zwiększ wzniesienie albo Zmniejsz wzniesienie.";
			d["item.leftRight.label"] = "Lewa i prawa strona";
			d["item.leftRight.desc"] = "Zapamiętuje dodatkowe sieci wybrane w wierszach Po lewej i Po prawej panelu Extra Networks and Areas, na przykład Pas dla rowerów, Drzewa, Parking, Nabrzeże, Ściana oporowa, Bariera akustyczna. Wymaga tego moda; bez niego ta pozycja nic nie robi.";
			d["item.general.label"] = "Ogólnie";
			d["item.general.desc"] = "Zapamiętuje przełączniki w panelu Ogólnie tego samego moda: Podłoże, Tunel, Podwyższenie, Rozszerzony zasięg wysokości. Wymaga tego moda.";
			d["item.underground.label"] = "Tryb podziemny";
			d["item.underground.desc"] = "Zapamiętuje przełącznik Tryb podziemny (Włącz/wyłącz tryb podziemny), który decyduje, czy stawiasz coś pod ziemią, czy nad nią.";
			d["item.other.label"] = "Różne";
			d["item.other.desc"] = "Zapamiętuje pozostałe wiersze, które może pokazać panel narzędzia: Rozmiar pędzla, Nacisk pędzla, Kąt pędzla, Docelowa wysokość, Odległość, Kolor, Wiek (Sadzonka, Młode, Dojrzałe, Stare), Zachowaj wiek.";
			return d;
		}

		// ======================= pt-BR =======================

		private static Dictionary<string, string> Pt()
		{
			Dictionary<string, string> d = Frames("pt-BR");
			d["mod.name"] = "Memória das ferramentas";
			d["tab.mod"] = "Configurações do modo de ferramenta";
			d["tab.about"] = "Sobre";
			d["group.items"] = "Configurações da memória das ferramentas";
			d["group.reset"] = "Gestão de memória";
			d["group.compat"] = "Compatibilidade";
			d["group.about"] = "Informações e links";

			d["enabled.label"] = "Ativar Memória do modo de ferramenta";
			d["enabled.desc"] = "Enquanto estiver ligado, todos os valores ficam sendo gravados sem parar, então ao voltar a um jogo os painéis estão exatamente como você deixou. Desligar um item só impede que ele seja restaurado; os valores dele continuam sendo gravados. Só desligando este interruptor geral a gravação para e cada ferramenta volta ao comportamento original.";
			d["compat.label"] = "Compatível com outros mods";
			d["compat.desc"] = "Ligado: a memória usa os nomes de menu e grupo conforme ajustados por outros mods (Asset UI Manager, ExtraLib e semelhantes), então um ativo que você moveu segue o novo lugar. Desligado: mantém a classificação que este mod viu primeiro, o que é mais estável. Nenhuma das duas escolhas apaga valores já gravados.";
			d["scope.label"] = "Âmbito de partilha";
			d["scope.desc"] = "Até que ponto este valor é partilhado entre ativos. Os parênteses mostram o original e a recomendação.";
			d["scope.defs"] = "Âmbitos de partilha\nMesmo grupo: suba a elevação de uma rua pequena de duas faixas para 10 m e as outras ruas pequenas vão a 10 m também, enquanto as grandes ficam iguais.\nMesmo menu: qualquer ativo do menu Estradas partilha; ao mudar para o menu Eletricidade já não.\nMesmo tipo de ativo: só ativos ou funções da mesma subcategoria partilham. Mudar para outra rua grande continua partilhando os 10 m, mas uma ponte volta a 0 m e precisa do próprio ajuste, embora os dois estejam no menu Estradas. Por funções entende-se Zonas, Espaços e Áreas, Terraformação, Marcador e objetos pré-fabricados, isto é, as ferramentas que não são ativos.\nGlobal: cada ativo e função que apoie a opção partilha um valor, mas ativos e funções nunca partilham entre si.\nSem partilhar: cada ativo e cada função guarda o próprio valor.";

			d[kScopeGroup] = "Mesmo grupo";
			d[kScopeMenu] = "Mesmo menu";
			d[kScopeCategory] = "Mesmo tipo de ativo";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Sem partilhar";
			d[kTagVanilla] = " (original)";
			d[kTagRecommended] = " (recomendado)";
			d[kTagRecVanilla] = " (recomendado, original)";

			d["reset.label"] = "Repor memória";
			d["reset.desc"] = "No jogo: apaga a memória deste jogo e devolve as ferramentas ao estado original do carregamento. No menu principal: apaga a memória de todos os jogos.";
			d["reset.warn"] = "Não pode ser desfeito.";
			d["reset.confirm"] = "Repor as definições de ferramenta memorizadas?";
			d["resetall.label"] = "Restaurar todos os padrões recomendados";
			d["resetall.desc"] = "Devolve cada opção deste mod ao valor recomendado: o interruptor geral, o de compatibilidade e o estado e âmbito de cada item. A memória já gravada dos jogos não é afetada.";
			d["resetall.warn"] = "Não pode ser desfeito.";
			d["resetall.confirm"] = "Restaurar todas as definições para os valores recomendados?";
			d["folder.label"] = "Gerir os arquivos de memória de todos os jogos";
			d["folder.desc"] = "Abre a pasta local com os arquivos de memória, um por jogo, com o nome do jogo.";

			d["about.version"] = "Versão do mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Me oferece um café";
			d["about.kofi.desc"] = "Apoie o autor no Ko-fi.";
			d["about.forum"] = "Página do fórum";
			d["about.forum.desc"] = "Abrir a conversa no fórum da Paradox.";
			d["about.rainbow"] = "Site da RAINBOW";
			d["about.rainbow.desc"] = "Abrir o site da série Rainbow.";

			d["item.anarchy.desc"] = "Lembra o botão ligar/desligar do próprio mod Anarchy (mod 74604 da Paradox) para a ferramenta em uso. Opções como remover o limite de altura pertencem a esse mod, não ao jogo. Sem o Anarchy instalado isto não faz nada.";
			d["item.packs.label"] = "Pacote";
			d["item.packs.desc"] = "Lembra as caixas da linha Pacote nas Opções de ferramentas: Ativar/desativar todos os pacotes, Jogo-base, Mods de ativos.";
			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Lembra a linha Tema. O que aparece depende da categoria do ativo; os prédios, por exemplo, listam Europeu, Norte-americano, Arquitetura moderna, Calçadão urbano.";
			d["item.toolMode.label"] = "Ferramentas";
			d["item.toolMode.desc"] = "Lembra o modo escolhido para cada ferramenta, e as opções mudam conforme a ferramenta: estradas, trilhos e canos oferecem Reta, Curva simples, Curva complexa, Contínuo, Grade, Substituir, Ponto; prédios, acessórios e árvores oferecem Colocar um, Colocar vários, Linha, Curva, Ferramenta de carimbo de objetos; as zonas oferecem Preencher, Marcar, Pintar; as áreas oferecem Edição, Gerar grade de mapa. Os modos de ativos e os de funções são lembrados à parte porque as opções não são as mesmas.";
			d["item.elevation.label"] = "Elevação";
			d["item.elevation.desc"] = "Lembra a elevação em que a ferramenta ficou, incluindo o resultado de Aumentar elevação e Reduzir elevação. A elevação de cruzamentos e passagens não é coberta.";
			d["item.parallel.label"] = "Modo Paralelo";
			d["item.parallel.desc"] = "Lembra o interruptor Modo Paralelo (Alternar modo paralelo) junto com a quantidade de Via paralela e o Contraste paralelo que Aumentar contraste e Reduzir contraste alteram. Permite construir redes paralelas.";
			d["item.snap.label"] = "Aderir";
			d["item.snap.desc"] = "Lembra cada interruptor da linha Aderir: Aderir à geometria existente, Aderir ao tamanho da célula da zona, Aderir a ângulos de 90 graus, Aderir aos lados de uma via, Aderir a vias, Aderir ao lado do proprietário, Aderir aos lados de uma estrutura, Aderir ao meio de uma via, Aderir à costa, Aderir a geometria próxima, Aderir a linhas guia, Aderir à grade da zona, Aderir aos nodos, Aderir à superfície de um objeto, Aderir verticalmente, Aderir à grade do lote, Vincula itens sobrepostos a uma estrutura, Remover apenas o tipo correspondente, Exibir linhas de contorno, Aderir à distância. A linha Alternar modo Aderir do jogo é um interruptor em lote e não é lembrada.";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Lembra a linha Topografia, ou seja, se Exibir linhas de contorno está ligado.";
			d["item.elevationStep.label"] = "Passo de elevação";
			d["item.elevationStep.desc"] = "Lembra o valor Passo de elevação, quanto se anda com um toque em Aumentar elevação ou Reduzir elevação.";
			d["item.leftRight.label"] = "Esquerda e direita";
			d["item.leftRight.desc"] = "Lembra as redes laterais escolhidas nas linhas Esquerda e Direita do painel Extra Networks and Areas, por exemplo Ciclovia, Árvores, Estacionamento, Cais, Muro de contenção, Barreira de som. Requer esse mod; sem ele isto não faz nada.";
			d["item.general.label"] = "Geral";
			d["item.general.desc"] = "Lembra os interruptores do painel Geral do mesmo mod: Solo, Tunnel, Elevated, Expanded Elevation Range (esse mod ainda não tem tradução para essas três linhas). Requer esse mod.";
			d["item.underground.label"] = "Modo Subterrâneo";
			d["item.underground.desc"] = "Lembra o interruptor Modo Subterrâneo (Alternar modo subterrâneo), que decide se o que você coloca fica abaixo ou acima do solo.";
			d["item.other.label"] = "Diversos";
			d["item.other.desc"] = "Lembra as demais linhas que um painel de ferramenta pode trazer: Tamanho do pincel, Força do pincel, Ângulo do pincel, Altura do alvo, Distância, Cor, Idade (Muda, Jovem, Maduro, Ancião), Preservar idade.";
			return d;
		}

		// ======================= ru-RU =======================

		private static Dictionary<string, string> Ru()
		{
			Dictionary<string, string> d = Frames("ru-RU");
			d["mod.name"] = "Память режима инструмента";
			d["tab.mod"] = "Настройки режима инструмента";
			d["tab.about"] = "О моде";
			d["group.items"] = "Настройки памяти инструментов";
			d["group.reset"] = "Управление памятью";
			d["group.compat"] = "Совместимость";
			d["group.about"] = "Сведения и ссылки";

			d["enabled.label"] = "Включить память режима инструмента";
			d["enabled.desc"] = "Пока это включено, каждое значение записывается постоянно, и при возврате к сохранению панели будут такими, какими вы их оставили. Отключение одного пункта лишь убирает его восстановление, а значения всё равно записываются. Запись прекращается только при выключении этого главного переключателя, и каждый инструмент возвращается к обычному поведению.";
			d["compat.label"] = "Совместимость с другими модами";
			d["compat.desc"] = "Включено: память раскладывается по названиям меню и групп в том виде, как их изменили другие моды (Asset UI Manager, ExtraLib и подобные), поэтому перемещённый объект следует за новым местом. Выключено: сохраняется классификация, которую мод увидел первой, это устойчивее. В обоих случаях уже записанные значения не теряются.";
			d["scope.label"] = "Область общего доступа";
			d["scope.desc"] = "Насколько широко это значение разделяется между объектами. В скобках указано поведение в оригинале и наша рекомендация.";
			d["scope.defs"] = "Области общего доступа\nТа же группа: поднимите высоту небольшой двухполосной дороги до 10 м, и другие небольшие дороги тоже станут 10 м, а большие дороги не изменятся.\nТо же меню: любой объект из меню дорог разделяет её, но в меню электроснабжения уже нет.\nТот же тип объекта: разделяют только объекты или функции одной подкатегории. Другая большая дорога по-прежнему делит 10 м, но мост возвращается к 0 м и требует собственной настройки, хотя оба находятся в меню дорог. Под функциями имеются в виду Зоны, Пространства и области, Терраформирование, Маркер и префабы объектов, то есть инструменты, которые не являются объектами.\nГлобально: каждый объект и каждая функция, поддерживающие пункт, делят одно значение, но объекты и функции между собой не делят.\nНикакого разделения: каждый объект и каждая функция хранят своё значение.";

			d[kScopeGroup] = "Та же группа";
			d[kScopeMenu] = "То же меню";
			d[kScopeCategory] = "Тот же тип объекта";
			d[kScopeGlobalShared] = "Глобально";
			d[kScopeGlobalUnique] = "Без разделения";
			d[kTagVanilla] = " (оригинал)";
			d[kTagRecommended] = " (рекомендуется)";
			d[kTagRecVanilla] = " (рекомендуется, оригинал)";

			d["reset.label"] = "Сбросить память";
			d["reset.desc"] = "В игре: стирает память этого сохранения и возвращает инструменты к исходному состоянию при загрузке. В главном меню: стирает память всех сохранений.";
			d["reset.warn"] = "Отменить нельзя.";
			d["reset.confirm"] = "Сбросить запомненные настройки инструментов?";
			d["resetall.label"] = "Вернуть все рекомендуемые значения";
			d["resetall.desc"] = "Возвращает каждую настройку этого мода к рекомендуемому значению: главный переключатель, переключатель совместимости, а также состояние и область каждого пункта. Уже записанная память сохранений не затрагивается.";
			d["resetall.warn"] = "Отменить нельзя.";
			d["resetall.confirm"] = "Вернуть все настройки к рекомендуемым значениям?";
			d["folder.label"] = "Управление файлами памяти всех сохранений";
			d["folder.desc"] = "Открывает локальную папку с файлами памяти, по одному на сохранение, названным по имени сохранения.";

			d["about.version"] = "Версия мода";
			d["about.author"] = "Автор";
			d["about.kofi"] = "Купить мне кофе";
			d["about.kofi.desc"] = "Поддержать автора на Ko-fi.";
			d["about.forum"] = "Страница на форуме";
			d["about.forum.desc"] = "Открыть тему на форуме Paradox.";
			d["about.rainbow"] = "Сайт RAINBOW";
			d["about.rainbow.desc"] = "Открыть сайт серии Rainbow.";

			d["item.anarchy.desc"] = "Запоминает собственный переключатель мода Anarchy (мод Paradox 74604) для текущего инструмента. Такие пункты, как снятие ограничения высоты, относятся к тому моду, а не к игре. Без Anarchy этот пункт ничего не делает.";
			d["item.packs.label"] = "Набор";
			d["item.packs.desc"] = "Запоминает отметки в строке Набор панели «Параметры инструментов»: Вкл/выкл все наборы, Базовая игра, Модификации ресурсов.";
			d["item.themes.label"] = "Тема";
			d["item.themes.desc"] = "Запоминает строку Тема. Что предлагается, зависит от категории объекта; например, для зданий это Европейский стиль, Североамериканский стиль, Современная архитектура, Городские променады.";
			d["item.toolMode.label"] = "Режим инструмента";
			d["item.toolMode.desc"] = "Запоминает режим, выбранный для каждого инструмента, а параметры различаются: для дорог, путей и труб это Прямая, Простая кривая, Сложная кривая, Непрерывная, Сетка, Заменить, Точка; для зданий, объектов и деревьев — Разместить один, Разместить несколько, Линия, Кривая, Инструмент копирования объекта; для зон — Заливка, Рамка, Кисть; для областей — Изменить, Создать сетку карты. Режимы объектов и режимы функций запоминаются отдельно, потому что их наборы различаются.";
			d["item.elevation.label"] = "Эстакада";
			d["item.elevation.desc"] = "Запоминает высоту, на которой оставлен инструмент, включая результат от Увеличить подъем и Уменьшить подъем. Высота перекрёстков и развязок не запоминается.";
			d["item.parallel.label"] = "Режим параллели";
			d["item.parallel.desc"] = "Запоминает переключатель Режим параллели (Переключить режим параллели) вместе с количеством Параллельная дорога и Смещение параллели, которые меняют Увеличить смещение и Уменьшить смещение. Позволяет строить параллельные дороги.";
			d["item.snap.label"] = "Привязка";
			d["item.snap.desc"] = "Запоминает каждый переключатель в строке Привязка: Привязка к форме, Привязка к длине зоны, Привязка к прямым углам, Привязка к обочине дороги, Привязка к дорогам, Привязка к зданию-владельцу, Привязка к стенам здания, Привязка к середине дороги, Привязка к берегу, Привязка к ландшафту, Привязка к направляющим, Привязка к сетке зоны, Привязка к точкам, Привязка к поверхности объекта, Привязка по вертикали, Привязка к сетке участка, Привязывает перекрывающиеся элементы к зданию, Удалить только определенный тип, Показать контурные линии, Привязка к расстоянию. Строка Включить/выключить привязку — это пакетный переключатель, и он не запоминается.";
			d["item.topography.label"] = "Топография";
			d["item.topography.desc"] = "Запоминает строку Топография, то есть включено ли Показать контурные линии.";
			d["item.elevationStep.label"] = "Шаг подъема";
			d["item.elevationStep.desc"] = "Запоминает значение Шаг подъема: насколько сдвигает одно нажатие Увеличить подъем или Уменьшить подъем.";
			d["item.leftRight.label"] = "Лево и право";
			d["item.leftRight.desc"] = "Запоминает дополнительные сети, выбранные в строках Лево и Право панели Extra Networks and Areas, например Велосипедная полоса, Деревья, Парковка, Набережная, Подпорные стены, Шумозащитный экран. Требует этот мод; без него пункт ничего не делает.";
			d["item.general.label"] = "Общее";
			d["item.general.desc"] = "Запоминает переключатели панели Общее того же мода: Земля, Туннель, Эстакада, Расширенный диапазон высот. Требует этот мод.";
			d["item.underground.label"] = "Режим тоннеля";
			d["item.underground.desc"] = "Запоминает переключатель Режим тоннеля (Переключить режим тоннеля), который решает, окажется ли размещаемое под землёй или над ней.";
			d["item.other.label"] = "Разное";
			d["item.other.desc"] = "Запоминает прочие строки, которые может показывать панель инструмента: Размер кисти, Нажим кисти, Угол кисти, Целевая высота, Расстояние, Цвет, Возраст (Саженец, Молодое, Зрелое, Старое), Заморозить возраст.";
			return d;
		}
	}

	internal class LocaleSource : IDictionarySource
	{
		private readonly ToolModeMemorySettings m_Setting;
		private readonly Dictionary<string, string> m_Entries;

		public LocaleSource(ToolModeMemorySettings setting, string locale)
		{
			m_Setting = setting;
			m_Entries = LocaleTable.BuildEntries(setting, locale);
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return m_Entries;
		}

		public void Unload()
		{
		}
	}
}
