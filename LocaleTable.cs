using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Colossal;
using Game.UI;
using Game.UI.Widgets;

namespace ToolModeMemory
{
	/// <summary>
	/// 12 官方语言词条 + 范围下拉（短名）+ 每个工具项自己的共用范围说明。
	///
	/// v0.2.1 改动：
	///  * 共用范围的五行定义不再单列在页面底部的说明板块，而是并进**每一个**工具项
	///    「共用范围」那一行的说明文本里（用户要求）；例子按该项自己的数值来写，
	///    不再是统一的「道路 10m」。
	///  * 简体中文文案以用户给的原话为准，不经过英文回译。
	///  * 第 1、8、9 项（Anarchy 开关 / 左侧和右侧 / 常规）都属于 Anarchy（74604），
	///    行名沿用 Anarchy 自带语言包的 Anarchy.SECTION_TITLE[Left]/[Right]/[General]；
	///    旧文案把它们写成 Extra Networks and Areas，是错误归属，已改正。
	///
	/// 每份字典各填各的语言，禁止读 activeLocale（AccessAnarchy 的教训）。
	/// </summary>
	internal static partial class LocaleTable
	{
		public const string kScopeGroup = "scope.group";
		public const string kScopeMenu = "scope.menu";
		public const string kScopeCategory = "scope.category";
		public const string kScopeGlobalShared = "scope.globalShared";
		public const string kScopeGlobalUnique = "scope.globalUnique";

		// 共用范围说明的五行模板 + 尾注。{0} 由该工具项自己的例子（item.<id>.ex.*）填入。
		public const string kScopeLineGroup = "scope.line.group";
		public const string kScopeLineMenu = "scope.line.menu";
		public const string kScopeLineCategory = "scope.line.category";
		public const string kScopeLineShared = "scope.line.shared";
		/// <summary>
		/// 「游戏里只有一份设置」的那几项（地形 / 地下模式 / Anarchy 三项）专用：
		/// 它们的「全局共用」连资产与功能都不分，用上面那句会把行为说错。
		/// </summary>
		public const string kScopeLineSharedSingle = "scope.line.sharedSingle";
		public const string kScopeLineUnique = "scope.line.unique";
		public const string kScopeNote = "scope.note";

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
			return Val(Table("en-US"), key);
		}

		/// <summary>
		/// 取词条，且**绝不抛 KeyNotFoundException**。
		/// 一个缺键就足够把整页拖成原始 key：BuildEntries 是在注册语言包时跑的，
		/// 中途抛出会让框架一条都没注册上，于是每一项都只能显示键名（0.2.2 开发中
		/// 先加了 themes/packs 两行、文案还没写，就是这个现象）。
		/// 顺序：本语言 -> en-US（FillGaps 之外的双保险）-> 原样返回键名。
		/// 存在的键即使值是空串也照原样返回（tag.none 就是空后缀，不能当成缺键）。
		/// </summary>
		private static string Val(Dictionary<string, string> d, string key)
		{
			if (string.IsNullOrEmpty(key)) return "";
			string value;
			if (d != null && d.TryGetValue(key, out value)) return value;
			Dictionary<string, string> en = Table("en-US");
			if (!ReferenceEquals(d, en) && en.TryGetValue(key, out value)) return value;
			return key;
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
			string name = Val(d, baseKey);
			int van = def != null ? def.VanillaScope : ToolItemCatalog.kVanillaNone;
			int rec = def != null ? def.RecommendedScope : 0;
			bool isVan = van == value;
			bool isRec = rec == value;
			string tag;
			if (isVan && isRec) tag = Val(d, kTagRecVanilla);
			else if (isVan) tag = Val(d, kTagVanilla);
			else if (isRec) tag = Val(d, kTagRecommended);
			else tag = Val(d, kTagNone);
			return new DropdownItem<int> { value = value, displayName = name + tag };
		}

		/// <summary>
		/// 设置页词条注册。框架键 = ModSetting 的 GetXxxLocaleID(...)，值来自对应语言的字典。
		/// </summary>
		public static Dictionary<string, string> BuildEntries(ToolModeMemorySettings setting, string locale)
		{
			Dictionary<string, string> d = Table(locale);
			Dictionary<string, string> o = new Dictionary<string, string>();

			o[setting.GetSettingsLocaleID()] = Val(d, "mod.name");
			o[setting.GetOptionTabLocaleID(ToolModeMemorySettings.kTabMod)] = Val(d, "tab.mod");
			o[setting.GetOptionTabLocaleID(ToolModeMemorySettings.kTabAbout)] = Val(d, "tab.about");
			AddTabAndGroupTitles(setting, d, o);

			// 总开关 + 兼容开关（共用范围定义已并入每一项的 scope 说明，见 AddItems）
			o[setting.GetOptionLabelLocaleID("Enabled")] = Val(d, "enabled.label");
			o[setting.GetOptionDescLocaleID("Enabled")] = Val(d, "enabled.desc");
			o[setting.GetOptionLabelLocaleID("CompatOtherMods")] = Val(d, "compat.label");
			o[setting.GetOptionDescLocaleID("CompatOtherMods")] = Val(d, "compat.desc");

			AddItems(setting, d, o);

			// 记忆管理（关于页）
			o[setting.GetOptionLabelLocaleID("ResetMemory")] = Val(d, "reset.label");
			o[setting.GetOptionDescLocaleID("ResetMemory")] = Val(d, "reset.desc");
			o[setting.GetOptionWarningLocaleID("ResetMemory")] = Val(d, "reset.warn");
			o["Options.WARNING[CONFIRM_RESET]"] = Val(d, "reset.confirm");

			o[setting.GetOptionLabelLocaleID("ResetAllSettings")] = Val(d, "resetall.label");
			o[setting.GetOptionDescLocaleID("ResetAllSettings")] = Val(d, "resetall.desc");
			o[setting.GetOptionWarningLocaleID("ResetAllSettings")] = Val(d, "resetall.warn");
			o["Options.WARNING[CONFIRM_RESET_ALL]"] = Val(d, "resetall.confirm");

			o[setting.GetOptionLabelLocaleID("OpenMemoryFolder")] = Val(d, "folder.label");
			o[setting.GetOptionDescLocaleID("OpenMemoryFolder")] = Val(d, "folder.desc");

			// 信息与链接
			o[setting.GetOptionLabelLocaleID("ModVersion")] = Val(d, "about.version");
			o[setting.GetOptionLabelLocaleID("ModAuthor")] = Val(d, "about.author");
			o[setting.GetOptionLabelLocaleID("OpenKofi")] = Val(d, "about.kofi");
			o[setting.GetOptionDescLocaleID("OpenKofi")] = Val(d, "about.kofi.desc");
			o[setting.GetOptionLabelLocaleID("OpenForum")] = Val(d, "about.forum");
			o[setting.GetOptionDescLocaleID("OpenForum")] = Val(d, "about.forum.desc");
			o[setting.GetOptionLabelLocaleID("OpenRainbowSite")] = Val(d, "about.rainbow");
			o[setting.GetOptionDescLocaleID("OpenRainbowSite")] = Val(d, "about.rainbow.desc");
			return o;
		}

		/// <summary>
		/// 11 个工具项：启用开关 + 共用范围下拉。标签在这里补上目录里的序号，
		/// 语言表里的 item.*.label 一律不带数字。
		/// 需求 4（v0.2.1 修正）：五种共用范围的定义写进**本项**共用范围行的说明文本，
		/// 例子按本项的数值来举。
		/// </summary>
		private static void AddItems(ToolModeMemorySettings setting, Dictionary<string, string> d, Dictionary<string, string> o)
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				string pascal = PascalCase(def.Id);
				o[setting.GetOptionLabelLocaleID(pascal + "Enabled")] = def.Number.ToString() + ". " + Val(d, "item." + def.Id + ".label");
				o[setting.GetOptionDescLocaleID(pascal + "Enabled")] = Val(d, "item." + def.Id + ".desc");
				o[setting.GetOptionLabelLocaleID(pascal + "Scope")] = Val(d, "scope.label");
				o[setting.GetOptionDescLocaleID(pascal + "Scope")] = ScopeDescription(def, d);
			}
		}

		/// <summary>
		/// 某一工具项的「共用范围」说明文本：一句引导 + 五行定义（同组/同菜单/同类资产用
		/// 本项自己的例子）+ 一句「功能」的界定。缺键由 FillGaps 回落 en-US，不会拼出空行。
		/// </summary>
		private static string ScopeDescription(ToolItemDef def, Dictionary<string, string> d)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Val(d, "scope.desc"));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineGroup, Example(d, def.Id, "group")));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineMenu, Example(d, def.Id, "menu")));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineCategory, Example(d, def.Id, "category")));
			sb.Append('\n');
			sb.Append(Val(d, def.FamilyFreeScope ? kScopeLineSharedSingle : kScopeLineShared));
			sb.Append('\n');
			sb.Append(Val(d, kScopeLineUnique));
			sb.Append('\n');
			sb.Append(Val(d, kScopeNote));
			return sb.ToString();
		}

		private static string Line(Dictionary<string, string> d, string templateKey, string example)
		{
			string template;
			if (!d.TryGetValue(templateKey, out template)) return "";
			if (string.IsNullOrEmpty(example)) return template;
			return template.Replace("{0}", example);
		}

		private static string Example(Dictionary<string, string> d, string itemId, string slot)
		{
			string value;
			return d.TryGetValue("item." + itemId + ".ex." + slot, out value) ? value : "";
		}

		/// <summary>id 形如 toolMode / leftRight，设置类里的属性名是 ToolMode / LeftRight。</summary>
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
					o[setting.GetOptionTabLocaleID(value)] = aboutTab ? Val(d, "tab.about") : Val(d, "tab.mod");
				}
				else if (name.StartsWith("kGroup", StringComparison.Ordinal))
				{
					o[setting.GetOptionGroupLocaleID(value)] = GroupTitle(name, value, d);
				}
			}
		}

		/// <summary>
		/// 分组语义判定。v0.2.2 的板块结构：
		///   kGroupMaster  总开关（不显示标题，注册一个占位串就行）
		///   kGroupOfficial 官方工具项设置
		///   kGroupAnarchy  Anarchy工具项设置（模组名 + 「工具项设置」）
		///   kGroupMemory / kGroupCompat / kGroupInfo 关于页三块
		/// 设置类里的 kGroup* 常量会随重构改名，所以这里反射读取常量本身，按语义给标题。
		/// </summary>
		private static string GroupTitle(string field, string value, Dictionary<string, string> d)
		{
			string f = field.ToLowerInvariant();
			if (f.EndsWith("master", StringComparison.Ordinal)) return Val(d, "group.master");
			if (f.EndsWith("official", StringComparison.Ordinal)) return Val(d, "group.official");
			if (f.IndexOf("anarchy", StringComparison.Ordinal) >= 0) return Val(d, "group.anarchy");
			if (f.EndsWith("items", StringComparison.Ordinal) || f.EndsWith("main", StringComparison.Ordinal)
				|| f.EndsWith("settings", StringComparison.Ordinal) || f.EndsWith("mod", StringComparison.Ordinal))
			{
				return Val(d, "group.official");
			}
			string s = f + "|" + value.ToLowerInvariant();
			if (s.IndexOf("compat", StringComparison.Ordinal) >= 0) return Val(d, "group.compat");
			if (s.IndexOf("about", StringComparison.Ordinal) >= 0 || s.IndexOf("info", StringComparison.Ordinal) >= 0
				|| s.IndexOf("link", StringComparison.Ordinal) >= 0)
			{
				return Val(d, "group.about");
			}
			if (s.IndexOf("reset", StringComparison.Ordinal) >= 0 || s.IndexOf("memory", StringComparison.Ordinal) >= 0
				|| s.IndexOf("folder", StringComparison.Ordinal) >= 0)
			{
				return Val(d, "group.reset");
			}
			return Val(d, "group.official");
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
		// 权威语言：五行共用范围定义沿用用户给的原话，例子按每一项自己的数值改写，
		// 不经过英文回译。其它语言由这份定稿翻译。

		private static Dictionary<string, string> ZhHans()
		{
			Dictionary<string, string> d = Frames("zh-HANS");
			d["mod.name"] = "工具模式区分记忆";
			d["tab.mod"] = "工具模式记忆设置";
			d["tab.about"] = "关于";
			d["group.master"] = "工具模式记忆";
			d["group.official"] = "官方工具项设置";
			d["group.anarchy"] = "Anarchy工具项设置";
			d["group.reset"] = "记忆管理";
			d["group.compat"] = "兼容性";
			d["group.about"] = "信息与链接";

			d["enabled.label"] = "启用工具模式记忆";
			d["enabled.desc"] = "默认打开。开着期间，所有工具项的数值都会实时记录，回到存档时就是你上次离开时的样子；单独关掉某一项只是不再恢复那一项，它的数值仍在记录。只有关掉这个总开关才会停止记录，并把所有工具恢复成原版行为。";

			d["compat.label"] = "是否兼容其它模组";
			d["compat.desc"] = "默认打开。打开后按其它模组调整过的菜单、分组名称来记忆：像 ASSET UI MANAGER 这类模组会把部分资产挪到别的菜单或新的组，被挪过的资产就跟着它的新位置走。关掉后沿用本模组第一次看到这个资产时的归类，不受中途重排影响，更稳定。这个开关只影响「同组」「同菜单」两档，以及地区主题 / 数据包按哪个分组记；「同类资产」是按资产实际服务谁判断的，开不开都一样。注意：自定义资产如果作者没把它的车道或服务设施设对，本模组认不出它的用途，就只能按单个资产单独记忆。两种选择都不会丢掉已经记录的数值。";

			d["scope.label"] = "共用范围";
			d["scope.desc"] = "这一项的数值在多大范围内共用，下拉框括号里标出原版行为与推荐选择。";
			d[kScopeLineGroup] = "同组：{0}";
			d[kScopeLineMenu] = "同菜单：{0}";
			d[kScopeLineCategory] = "同类资产：按资产实际服务谁归类，车道数、宽窄、级别与工具栏里的分类都不算区别，即{0}";
			d[kScopeLineShared] = "全局共用：任何支持本项工具的资产/功能全部共用，但资产和功能之间不共用";
			d[kScopeLineSharedSingle] = "全局共用：所有支持本项的工具共用一份，资产和功能之间也共用";
			d[kScopeLineUnique] = "全部不共用：所有支持本项工具的资产/功能完全独立设置";
			d[kScopeNote] = "注意，功能指的是功能区、空间和区域、地形改造、标记和预制对象等非资产";

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
			d["resetall.label"] = "重置所有设置项";
			d["resetall.desc"] = "把本模组的所有选项回到推荐默认值：总开关、兼容开关都重新打开，每一项的启用状态和共用范围也一并恢复。已记录的存档记忆不受影响。";
			d["resetall.warn"] = "此操作无法撤销。";
			d["resetall.confirm"] = "确定要重置所有设置项吗？";
			d["folder.label"] = "管理所有存档的记忆文件";
			d["folder.desc"] = "打开保存记忆的本地文件夹，每个存档一份文件，文件名就是存档名。模组读取记忆都是按存档名，存档名称和 json 文件名一致才会读取。所以可以通过修改文件名的方式实现复制。读自动存档时按城市名保存这一份记忆，因为自动存档的存档名每次都不一样。";

			d["about.version"] = "模组版本";
			d["about.author"] = "作者";
			d["about.kofi"] = "请我喝杯咖啡";
			d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
			d["about.forum"] = "论坛页面";
			d["about.forum.desc"] = "打开 Paradox 论坛帖子。";
			d["about.rainbow"] = "RAINBOW官网";
			d["about.rainbow.desc"] = "打开 RAINBOW 系列官网。";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "记忆 Anarchy 模组的开关，Anarchy 模组是全局共用，开启后可以修改共用范围。没安装 Anarchy 模组时这一项不起作用。";
			d["item.anarchy.ex.group"] = "比如给两车道小型道路打开 Anarchy，切换到其它的小型道路，也是打开的，但切换到大型道路就不是了";
			d["item.anarchy.ex.menu"] = "点击道路菜单里面的所有资产，Anarchy 都是打开的，但切换到电力菜单就不是了";
			d["item.anarchy.ex.category"] = "切换到六车道道路或桥梁也是打开的，但切换到双向地铁轨道就变成关闭（需单独设置），即使它们都在道路菜单下";

			// 地区主题 / 数据包：工具栏筛选面板那两行，行名照原版语言包
			// （Toolbar.THEME_PANEL_TITLE / Toolbar.ASSET_PACKS_PANEL_TITLE）。
			d["item.themes.label"] = "地区主题";
			d["item.themes.desc"] = "记忆工具栏筛选面板里「地区主题」勾选了哪些。只有当前分类里确实有资产用到主题时才会出现这一行，勾上之后工具栏只列出支持这些主题的资产。原版只在读档时把它重置回默认主题，所以这一项默认不开。";
			d["item.themes.ex.group"] = "比如只在道路/小型道路里勾了某个主题，切换到大型道路就不按它筛选了";
			d["item.themes.ex.menu"] = "在道路菜单里勾的主题，切换到电力菜单就不生效，回到道路菜单又还是那一套";
			d["item.themes.ex.category"] = "这一行的勾选属于工具栏面板本身，按当前所在的菜单+分类记忆，不按资产类别共用";
			d["item.packs.label"] = "数据包";
			d["item.packs.desc"] = "记忆工具栏筛选面板里「数据包」勾选了哪些，勾上之后工具栏只列出属于这些数据包的资产。原版每次切换菜单或分类都会把它清空，所以按「同一菜单+分类」来记最贴近你看到的规律，这一项默认也不开。";
			d["item.packs.ex.group"] = "比如在道路/小型道路勾了某个数据包，切到大型道路会被原版清空，回到小型道路又自动恢复";
			d["item.packs.ex.menu"] = "在道路菜单勾的数据包，和电力菜单勾的各记各的";
			d["item.packs.ex.category"] = "这一行的勾选属于工具栏面板本身，按当前所在的菜单+分类记忆；原版每次换分类都会把它清空";

			// ---------- 2 工具模式 ----------
			d["item.toolMode.label"] = "工具模式";
			d["item.toolMode.desc"] = "记忆每个工具所选的模式，可选项按工具不同：道路、轨道、管道为直线、简单曲线、复杂曲线、连续曲线、网格、替换、节点；建筑、装饰物、树木为放置单个、放置多个、直线、曲线、目标印戳工具；功能区为填充、滚动、刷涂；区域为编辑、生成地图网格。资产的工具模式与功能的工具模式分开记忆，因为它们的选项并不相同。";
			d["item.toolMode.ex.group"] = "比如给两车道小型道路选了简单曲线，切换到其它的小型道路，也是简单曲线，但切换到大型道路就不是了";
			d["item.toolMode.ex.menu"] = "点击道路菜单里面的所有资产，都是简单曲线，但切换到电力菜单就不是了";
			d["item.toolMode.ex.category"] = "切换到六车道道路或桥梁也是简单曲线，但切换到双向地铁轨道就回到默认模式（需单独设置），即使它们都在道路菜单下";

			// ---------- 3 高度（用户原话的例子） ----------
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "记忆工具离开时的高度值，也包括提升高度、降低高度得到的结果；「高度阶段」（一次抬高或降低多少米，Anarchy 面板里那一行叫高度调整幅度）跟高度一起记忆、用同一个共用范围。交叉路口的高度不在记忆范围内。";
			d["item.elevation.ex.group"] = "比如点击两车道道路将高度提高到10m，那么切换其它的小型道路，高度也是10m，但切换到大型道路就不是10m了";
			d["item.elevation.ex.menu"] = "点击所有道路菜单里面的资产，高度都是10m，但切换到电力菜单就不是10m了";
			d["item.elevation.ex.category"] = "切换到六车道道路或桥梁高度也是10m，但切换到双线铁路就变成0m（需单独设置），即使它们都在道路菜单下";

			// ---------- 4 并列模式 ----------
			d["item.parallel.label"] = "并列模式";
			d["item.parallel.desc"] = "记忆「并列模式」开关（切换并列模式），连同「并列道路」的数量和「并列偏移」的间距（增加偏移、减少偏移）。并列模式用来建造并列网络。";
			d["item.parallel.ex.group"] = "比如给两车道小型道路打开并列模式并把数量调到3，切换到其它的小型道路，同样是3条，但切换到大型道路就不是了";
			d["item.parallel.ex.menu"] = "点击道路菜单里面的所有资产，并列数量都是3，但切换到电力菜单就不是了";
			d["item.parallel.ex.category"] = "切换到六车道道路或桥梁同样是3条，但切换到有轨电车轨道就变成关闭（需单独设置），即使它们都在道路菜单下";

			// ---------- 5 对齐 ----------
			d["item.snap.label"] = "对齐";
			d["item.snap.desc"] = "记忆「对齐」里的每一个开关：快速对齐当前形状、快速对齐功能区单元格长度、快速对齐直角、快速对齐路边、快速对齐道路、快速对齐拥有者一侧、快速对齐建筑两侧、快速对齐道路中央、快速对齐海岸线、快速对齐附近形状、快速对齐导线、快速对齐功能区网格、快速对齐节点、快速对齐物体表面、快速直立对齐、快速对齐空地网格、将重叠物体绑定至建筑、仅移除相符类型、显示轮廓线条、快速对齐距离。游戏自带的「开启／关闭所有对齐功能」是批量开关，本身不会被记忆。";
			d["item.snap.ex.group"] = "比如给两车道小型道路关掉快速对齐路边，切换到其它的小型道路，也是关着的，但切换到大型道路就是全开的默认状态";
			d["item.snap.ex.menu"] = "点击道路菜单里面的所有资产，对齐开关都是这一套，但切换到电力菜单就不是了";
			d["item.snap.ex.category"] = "切换到六车道道路或桥梁也保持这一套开关，但切换到步行道就恢复默认（需单独设置），即使它们都在道路菜单下";

			// ---------- 6 地形 ----------
			d["item.topography.label"] = "地形";
			d["item.topography.desc"] = "记忆「地形」这一行，也就是显示轮廓线条的开与关。";
			d["item.topography.ex.group"] = "比如给两车道小型道路勾上地形，切换到其它的小型道路，同样是勾着的，但切换到大型道路就不是了";
			d["item.topography.ex.menu"] = "点击道路菜单里面的所有资产，地形都是勾着的，但切换到电力菜单就不是了";
			d["item.topography.ex.category"] = "切换到六车道道路或桥梁也是勾着的，但切换到电缆就变成不勾（需单独设置）";


			// ---------- 8 左侧和右侧（Anarchy） ----------
			d["item.leftRight.label"] = "左侧和右侧";
			d["item.leftRight.desc"] = "记忆 Anarchy 网络面板里「左侧」「右侧」两行所选的网络升级，例如自行车车道、树木、停车位、堤岸、挡土墙、隔音屏障。需要安装 Anarchy（Paradox 模组 74604），没装时这一项不起作用。";
			d["item.leftRight.ex.group"] = "比如给两车道小型道路的右侧选上自行车车道，切换到其它的小型道路，右侧同样是自行车车道，但切换到大型道路就不是了";
			d["item.leftRight.ex.menu"] = "点击道路菜单里面的所有资产，左右两侧都是这一套升级，但切换到电力菜单就不是了";
			d["item.leftRight.ex.category"] = "切换到六车道道路右侧也是自行车车道，但切换到双线铁路就变成未选择（需单独设置），即使它们都在道路菜单下";

			// ---------- 9 常规（Anarchy） ----------
			d["item.general.label"] = "常规";
			d["item.general.desc"] = "记忆 Anarchy 网络面板里「常规」一行的开关：强制地面、强制高架、强制隧道、平直斜坡、宽分隔带、扩展高度范围（取消高度上限）等。需要安装 Anarchy（Paradox 模组 74604），没装时这一项不起作用。";
			d["item.general.ex.group"] = "比如给两车道小型道路勾上强制隧道，切换到其它的小型道路，同样是强制隧道，但切换到大型道路就不是了";
			d["item.general.ex.menu"] = "点击道路菜单里面的所有资产，常规选项都是这一套，但切换到电力菜单就不是了";
			d["item.general.ex.category"] = "切换到六车道道路或桥梁也是强制隧道，但切换到双向地铁轨道就全部回到未勾选（需单独设置），即使它们都在道路菜单下";

			// ---------- 10 地下模式 ----------
			d["item.underground.label"] = "地下模式";
			d["item.underground.desc"] = "记忆「地下模式」开关（切换地下模式），也就是你放的东西在地面以下还是地面之上。";
			d["item.underground.ex.group"] = "比如给两车道小型道路打开地下模式，切换到其它的小型道路，同样在地下，但切换到大型道路就不是了";
			d["item.underground.ex.menu"] = "点击道路菜单里面的所有资产，都在地下模式，但切换到电力菜单就不是了";
			d["item.underground.ex.category"] = "切换到六车道道路或桥梁也在地下，但切换到双线铁路就回到地面（需单独设置），即使它们都在道路菜单下";

			// ---------- 11 其它 ----------
			d["item.other.label"] = "其它";
			d["item.other.desc"] = "记忆工具面板上其余可以记录的行：「颜色」一行的道路配色，以及「笔刷粗细」「笔刷强度」。";
			d["item.other.ex.group"] = "比如把两车道小型道路的颜色改成蓝色，切换到其它的小型道路，同样是蓝色，但切换到大型道路就不是了";
			d["item.other.ex.menu"] = "点击道路菜单里面的所有资产，颜色和笔刷都是这一套，但切换到电力菜单就不是了";
			d["item.other.ex.category"] = "切换到六车道道路或桥梁也是蓝色，但切换到双线铁路就按它自己那一类另记一份（需单独设置），即使它们都在道路菜单下";
			return d;
		}

		// ======================= zh-HANT =======================

		private static Dictionary<string, string> ZhHant()
		{
			Dictionary<string, string> d = Frames("zh-HANT");
			d["mod.name"] = "工具模式區分記憶";
			d["tab.mod"] = "工具模式記憶設定";
			d["tab.about"] = "關於";
			d["group.master"] = "工具模式記憶";
			d["group.official"] = "官方工具項設定";
			d["group.anarchy"] = "Anarchy工具項設定";
			d["group.reset"] = "記憶管理";
			d["group.compat"] = "相容性";
			d["group.about"] = "資訊與連結";

			d["enabled.label"] = "啟用工具模式記憶";
			d["enabled.desc"] = "預設開啟。開啟期間，所有工具項的數值都會即時記錄，回到存檔時就是你上次離開時的樣子；單獨關掉某一項只是不再恢復那一項，它的數值仍在記錄。只有關掉這個總開關才會停止記錄，並把所有工具還原成原版行為。";
			d["compat.label"] = "是否相容其他模組";
			d["compat.desc"] = "預設開啟。開啟後按照其他模組調整過的選單、分組名稱來記憶：像 ASSET UI MANAGER 這類模組會把部分資產挪到其他選單或新的分組，被挪過的資產就跟著它的新位置走。關掉後沿用本模組第一次看到這個資產時的歸類，不受中途重排影響，更穩定。這個開關只影響「同組」「同選單」兩檔，以及地區主題 / 資料包按哪個分組記；「同類資產」是按資產實際服務誰判斷的，開不開都一樣。注意：自訂資產如果作者沒把它的車道或服務設施設對，本模組認不出它的用途，就只能按單個資產單獨記憶。兩種選擇都不會丟掉已記錄的數值。";
			d["scope.label"] = "共用範圍";
			d["scope.desc"] = "這一項的數值在多大範圍內共用，下拉框括號裡標出原版行為與推薦選擇。";
			d[kScopeLineGroup] = "同組：{0}";
			d[kScopeLineMenu] = "同選單：{0}";
			d[kScopeLineCategory] = "同類資產：按資產實際服務誰歸類，車道數、寬窄、級別與工具欄裡的分類都不算區別，即{0}";
			d[kScopeLineShared] = "全域共用：任何支援本項工具的資產/功能全部共用，但資產和功能之間不共用";
			d[kScopeLineSharedSingle] = "全域共用：所有支援本項的工具共用一份，資產和功能之間也共用";
			d[kScopeLineUnique] = "全部不共用：所有支援本項工具的資產/功能完全獨立設定";
			d[kScopeNote] = "注意，功能指的是功能區、空間與區域、地形改造、標記和預製物件等非資產";

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
			d["resetall.label"] = "重設所有設定項";
			d["resetall.desc"] = "把本模組的所有選項回到推薦預設值：總開關、相容開關都重新開啟，每一項的啟用狀態和共用範圍也一併恢復。已記錄的存檔記憶不受影響。";
			d["resetall.warn"] = "此操作無法復原。";
			d["resetall.confirm"] = "確定要重設所有設定項嗎？";
			d["folder.label"] = "管理所有存檔的記憶檔案";
			d["folder.desc"] = "開啟保存記憶的本機資料夾，每個存檔一份檔案，檔名就是存檔名。模組讀取記憶都是按存檔名，存檔名稱和 json 檔名一致才會讀取。所以可以透過修改檔名的方式實現複製。讀取自動存檔時按城市名保存這份記憶，因為自動存檔的存檔名每次都不一樣。";

			d["about.version"] = "模組版本";
			d["about.author"] = "作者";
			d["about.kofi"] = "請我喝杯咖啡";
			d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
			d["about.forum"] = "論壇頁面";
			d["about.forum.desc"] = "開啟 Paradox 論壇貼文。";
			d["about.rainbow"] = "RAINBOW官網";
			d["about.rainbow.desc"] = "開啟 RAINBOW 系列官網。";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "記憶 Anarchy 模組的開關，Anarchy 模組是全域共用，開啟後可以修改共用範圍。沒安裝 Anarchy 模組時這一項不起作用。";
			d["item.anarchy.ex.group"] = "例如給兩車道小型道路開啟 Anarchy，切換到其他小型道路也是開啟的，但切換到大型道路就不是了";
			d["item.anarchy.ex.menu"] = "點選道路選單裡面的所有資產，Anarchy 都是開啟的，但切換到電力選單就不是了";
			d["item.anarchy.ex.category"] = "切換到六車道道路或橋樑同樣是開啟的，但切換到雙線地鐵軌道就變成關閉（需單獨設定），即使它們都在道路選單下";

			// 地區主題 / 資料包：行名照原版語言包（Toolbar.THEME_PANEL_TITLE = 主題、
			// Toolbar.ASSET_PACKS_PANEL_TITLE = 安裝包）。
			d["item.themes.label"] = "主題";
			d["item.themes.desc"] = "記憶工具列篩選面板裡「主題」勾選了哪些。只有目前分類確實有資產使用主題時才會出現這一列，勾選後工具列只列出支援這些主題的資產。原版只在讀檔時把它重置回預設主題，所以這一項預設不開。";
			d["item.themes.ex.group"] = "例如只在道路/小型道路勾了某個主題，切換到大型道路就不按它篩選了";
			d["item.themes.ex.menu"] = "在道路選單勾的主題，切換到電力選單就不生效，回到道路選單又還是那一組";
			d["item.themes.ex.category"] = "這一行的勾選屬於工具欄面板本身，按當前所在的選單+分類記憶，不按資產類別共用";
			d["item.packs.label"] = "安裝包";
			d["item.packs.desc"] = "記憶工具列篩選面板裡「安裝包」勾選了哪些，勾選後工具列只列出屬於這些安裝包的資產。原版每次切換選單或分類都會把它清空，所以按「同一選單+分類」來記最貼近你看到的規律，這一項預設也不開。";
			d["item.packs.ex.group"] = "例如在道路/小型道路勾了某個安裝包，切到大型道路會被原版清空，回到小型道路又自動恢復";
			d["item.packs.ex.menu"] = "在道路選單勾的安裝包，和電力選單勾的各記各的";
			d["item.packs.ex.category"] = "這一行的勾選屬於工具欄面板本身，按當前所在的選單+分類記憶；原版每次換分類都會把它清空";

			// ---------- 2 工具模式 ----------
			d["item.toolMode.label"] = "工具模式";
			d["item.toolMode.desc"] = "記憶每個工具所選的模式，可選項按工具不同：道路、軌道、管道為直線、簡單曲線、複數曲線、連續曲線、網格、替換、連接點；建築、裝飾物、樹木為設置一個、設置多個、直線、曲線、物件印章工具；功能區為填充、選取方格、塗上；區域為編輯、生成地圖方格。資產的工具模式與功能的工具模式分開記憶，因為它們的選項並不相同。";
			d["item.toolMode.ex.group"] = "例如給兩車道小型道路選了簡單曲線，切換到其他小型道路也是簡單曲線，但切換到大型道路就不是了";
			d["item.toolMode.ex.menu"] = "點選道路選單裡面的所有資產，都是簡單曲線，但切換到電力選單就不是了";
			d["item.toolMode.ex.category"] = "切換到六車道道路或橋樑也是簡單曲線，但切換到雙線地鐵軌道就回到預設模式（需單獨設定），即使它們都在道路選單下";

			// ---------- 3 高度 ----------
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "記憶工具離開時的高度值，也包括提升高度、降低高度得到的結果；「高度階段」（一次抬高或降低多少公尺，Anarchy 面板裡那一行叫高度間距）跟高度一起記憶、用同一個共用範圍。交叉路口的高度不在記憶範圍內。";
			d["item.elevation.ex.group"] = "例如點選兩車道道路把高度提高到10m，那麼切換其他小型道路，高度也是10m，但切換到大型道路就不是10m了";
			d["item.elevation.ex.menu"] = "點選所有道路選單裡面的資產，高度都是10m，但切換到電力選單就不是10m了";
			d["item.elevation.ex.category"] = "切換到六車道道路或橋樑高度也是10m，但切換到雙軌鐵路就變成0m（需單獨設定），即使它們都在道路選單下";

			// ---------- 4 並行模式 ----------
			d["item.parallel.label"] = "並行模式";
			d["item.parallel.desc"] = "記憶「並行模式」開關（切換並行模式），連同「並行道路」的數量和「並行偏移」的間距（增加偏移、減少偏移）。並行模式用來建造並行網狀系統。";
			d["item.parallel.ex.group"] = "例如給兩車道小型道路開啟並行模式並把數量調到3，切換到其他小型道路同樣是3條，但切換到大型道路就不是了";
			d["item.parallel.ex.menu"] = "點選道路選單裡面的所有資產，並行數量都是3，但切換到電力選單就不是了";
			d["item.parallel.ex.category"] = "切換到六車道道路或橋樑同樣是3條，但切換到電車軌道就變成關閉（需單獨設定），即使它們都在道路選單下";

			// ---------- 5 吸附 ----------
			d["item.snap.label"] = "吸附";
			d["item.snap.desc"] = "記憶「吸附」裡的每一個開關：快速對齊已有幾何線、快速對齊功能區單位格長度、快速對齊90度角、快速對齊路邊、快速對齊道路、快速對齊擁有者一側、快速對齊建築兩側、快速對齊道路中央、快速對齊水陸交界線、快速對齊附近幾何線、快速對齊導線、快速對齊功能區網格、快速對齊節點、快速對齊物件表面、快速直立對齊、快速對齊空地網格、將重疊對象綁定至建築、只移除符合的類型、顯示輪廓、對齊距離。遊戲自帶的「開啟／關閉所有對齊功能」是批次開關，本身不會被記憶。";
			d["item.snap.ex.group"] = "例如給兩車道小型道路關掉快速對齊路邊，切換到其他小型道路同樣是關著的，但切換到大型道路就是全開的預設狀態";
			d["item.snap.ex.menu"] = "點選道路選單裡面的所有資產，吸附開關都是這一組，但切換到電力選單就不是了";
			d["item.snap.ex.category"] = "切換到六車道道路或橋樑仍保持這一組開關，但切換到步行道就恢復預設（需單獨設定），即使它們都在道路選單下";

			// ---------- 6 地形圖 ----------
			d["item.topography.label"] = "地形圖";
			d["item.topography.desc"] = "記憶「地形圖」這一行的開與關，也就是是否顯示輪廓。";
			d["item.topography.ex.group"] = "例如給兩車道小型道路勾選地形圖，切換到其他小型道路同樣是勾選的，但切換到大型道路就不是了";
			d["item.topography.ex.menu"] = "點選道路選單裡面的所有資產，地形圖都是勾選的，但切換到電力選單就不是了";
			d["item.topography.ex.category"] = "切換到六車道道路或橋樑也是勾選的，但切換到電纜就變成不勾（需單獨設定）";


			// ---------- 8 左側和右側（Anarchy） ----------
			d["item.leftRight.label"] = "左側和右側";
			d["item.leftRight.desc"] = "記憶 Anarchy 網路面板「左」「右」兩行所選的網路升級，例如自行車道、樹木、停車、堤岸、擋土牆、音障。需要安裝 Anarchy（Paradox 模組 74604），沒裝時這一項不起作用。";
			d["item.leftRight.ex.group"] = "例如給兩車道小型道路的右側選上自行車道，切換到其他小型道路右側同樣是自行車道，但切換到大型道路就不是了";
			d["item.leftRight.ex.menu"] = "點選道路選單裡面的所有資產，左右兩側都是這一組升級，但切換到電力選單就不是了";
			d["item.leftRight.ex.category"] = "切換到六車道道路右側也是自行車道，但切換到雙軌鐵路就變成未選擇（需單獨設定），即使它們都在道路選單下";

			// ---------- 9 一般（Anarchy） ----------
			d["item.general.label"] = "一般";
			d["item.general.desc"] = "記憶 Anarchy 網路面板「一般」一行的開關：地面、高架、隧道、固定坡度、寬分隔島、擴展高度範圍（取消高度上限）等。需要安裝 Anarchy（Paradox 模組 74604），沒裝時這一項不起作用。";
			d["item.general.ex.group"] = "例如給兩車道小型道路勾選隧道，切換到其他小型道路同樣是隧道，但切換到大型道路就不是了";
			d["item.general.ex.menu"] = "點選道路選單裡面的所有資產，一般選項都是這一組，但切換到電力選單就不是了";
			d["item.general.ex.category"] = "切換到六車道道路或橋樑也是隧道，但切換到雙線地鐵軌道就全部回到未勾選（需單獨設定），即使它們都在道路選單下";

			// ---------- 10 地下模式 ----------
			d["item.underground.label"] = "地下模式";
			d["item.underground.desc"] = "記憶「地下模式」開關（切換地下模式），也就是你放的東西在地面以下還是地面之上。";
			d["item.underground.ex.group"] = "例如給兩車道小型道路開啟地下模式，切換到其他小型道路同樣在地下，但切換到大型道路就不是了";
			d["item.underground.ex.menu"] = "點選道路選單裡面的所有資產，都在地下模式，但切換到電力選單就不是了";
			d["item.underground.ex.category"] = "切換到六車道道路或橋樑也在地下，但切換到雙軌鐵路就回到地面（需單獨設定），即使它們都在道路選單下";

			// ---------- 11 其他 ----------
			d["item.other.label"] = "其他";
			d["item.other.desc"] = "記憶工具面板上其餘可以記錄的行：「顏色」一行的道路配色，以及「筆刷大小」「筆刷硬度」。";
			d["item.other.ex.group"] = "例如把兩車道小型道路的顏色改成藍色，切換到其他小型道路同樣是藍色，但切換到大型道路就不是了";
			d["item.other.ex.menu"] = "點選道路選單裡面的所有資產，顏色和筆刷都是這一組設定，但切換到電力選單就不是了";
			d["item.other.ex.category"] = "切換到六車道道路或橋樑也是藍色，但切換到雙軌鐵路就按它自己那一類另記一份（需單獨設定），即使它們都在道路選單下";
			return d;
		}

		// ======================= en-US =======================

		private static Dictionary<string, string> En()
		{
			Dictionary<string, string> d = Frames("en-US");
			d["mod.name"] = "TOOL MODE MEMORY";
			d["tab.mod"] = "Tool Mode Memory Settings";
			d["tab.about"] = "About";
			d["group.master"] = "Tool Mode Memory";
			d["group.official"] = "Official Tool Settings";
			d["group.anarchy"] = "Anarchy Tool Settings";
			d["group.reset"] = "Memory management";
			d["group.compat"] = "Compatibility";
			d["group.about"] = "Information and links";

			d["enabled.label"] = "Enable Tool Mode Memory";
			d["enabled.desc"] = "On by default. While it is on, every item's value is recorded as you work, so returning to a save puts the panels back exactly as you left them. Turning one item off only stops that item from being restored - its values are still recorded. Only switching this master switch off stops all recording and returns every tool to vanilla behaviour.";
			d["compat.label"] = "Compatible with other mods";
			d["compat.desc"] = "On by default. On: memory is filed under the menu and group names as adjusted by other mods - Asset UI Manager and the like move assets to another menu or a new group, and a moved asset follows its new location. Off: the classification this mod saw first is kept, which is steadier when the layout changes mid-session. This switch only covers Same group and Same menu, plus which group the Theme and Pack rows follow; Same asset type is decided by what the asset actually serves and is the same either way. Note that a custom asset whose author did not set its lanes or served facility correctly cannot be recognised, so it falls back to being remembered on its own. Neither choice throws away values that are already recorded.";
			d["scope.label"] = "Sharing scope";
			d["scope.desc"] = "How widely this item's value is shared. The brackets in the drop-down show what vanilla does and what we recommend.";
			d[kScopeLineGroup] = "Same group: {0}";
			d[kScopeLineMenu] = "Same menu: {0}";
			d[kScopeLineCategory] = "Same asset type: grouped by what the asset actually serves, so lane count, size, level and the toolbar category make no difference, that is {0}";
			d[kScopeLineShared] = "Shared globally: every asset or function that supports this item shares it, but assets and functions do not share with each other";
			d[kScopeLineSharedSingle] = "Shared globally: every tool that supports this item uses one value, assets and functions included";
			d[kScopeLineUnique] = "Not shared at all: every asset and function that supports this item is set independently";
			d[kScopeNote] = "Note: the word functions here means Zones, Spaces and Areas, Terraforming, Marker and Object Prefabs, that is the tools that are not assets";

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
			d["resetall.label"] = "Reset all settings";
			d["resetall.desc"] = "Puts every option of this mod back to its recommended default: the master switch and the compatibility switch go back on, and each item's on/off state and sharing scope are restored. Memory already recorded for saves is left alone.";
			d["resetall.warn"] = "This cannot be undone.";
			d["resetall.confirm"] = "Reset every setting of this mod to its recommended default?";
			d["folder.label"] = "Manage memory files for all saves";
			d["folder.desc"] = "Opens the local folder that holds the memory files, one per save, named after the save. Memory is always read by save name: a file is only used when the save name and the json file name match, so renaming the file is how you copy memory from one save to another. When you load an auto save its memory is kept under the city name, because an auto save gets a new name every time.";

			d["about.version"] = "Mod version";
			d["about.author"] = "Author";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Support the author on Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Open the Paradox forum thread.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Open the Rainbow Series site.";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "Remembers the Anarchy mod's switch. Anarchy is shared across the whole game, so this one is global; once it is on you can change the sharing scope. Without the Anarchy mod installed this item does nothing.";
			d["item.anarchy.ex.group"] = "for example, turn Anarchy on for a two-lane small road and the other small roads keep it on, while a large road does not";
			d["item.anarchy.ex.menu"] = "every asset you click in the Roads menu has Anarchy on, but moving to the Electricity menu it is not";
			d["item.anarchy.ex.category"] = "switching to a six-lane road or a bridge keeps it on, but a double subway track turns it off and needs its own setting, even though they are all in the Roads menu";

			// Row names follow the game's own language pack:
			// Toolbar.THEME_PANEL_TITLE = "Theme", Toolbar.ASSET_PACKS_PANEL_TITLE = "Pack".
			d["item.themes.label"] = "Theme";
			d["item.themes.desc"] = "Remembers which entries are ticked in the toolbar's Theme filter. That row only appears when the current category really contains assets that use themes, and ticking one limits the toolbar list to assets supporting those themes. Vanilla only resets it to the default theme when a save is loaded, so this item ships switched off.";
			d["item.themes.ex.group"] = "for example you tick a theme only under Roads/Small Roads, so large roads don't filter by it";
			d["item.themes.ex.menu"] = "a theme ticked in the Roads menu stops applying in the Electricity menu and is back when you return";
			d["item.themes.ex.category"] = "this row is the filter panel's own state, so it is filed under the menu and group you are in rather than by asset type";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Remembers which entries are ticked in the toolbar's Pack filter; ticking one limits the toolbar list to assets belonging to those packs. Vanilla clears it every time you change menu or category, so remembering it per menu + category matches what you see, and this item also ships switched off.";
			d["item.packs.ex.group"] = "for example you tick a pack under Roads/Small Roads, vanilla clears it on large roads, and it comes back when you return";
			d["item.packs.ex.menu"] = "packs ticked in the Roads menu and in the Electricity menu are remembered separately";
			d["item.packs.ex.category"] = "this row is the filter panel's own state, filed under the menu and group you are in; vanilla clears it every time the group changes";

			// ---------- 2 Tool Mode ----------
			d["item.toolMode.label"] = "Tool Mode";
			d["item.toolMode.desc"] = "Remembers the mode picked for each tool, and the options differ by tool: roads, tracks and pipes offer Straight, Simple Curve, Complex Curve, Continuous, Grid, Replace and Point; buildings, props and trees offer Place one, Place multiple, Line, Curve and Object stamp tool; zones offer Fill, Marquee and Paint; areas offer Edit and Generate Map Grid. Asset tool modes and function tool modes are remembered separately because their option sets differ.";
			d["item.toolMode.ex.group"] = "for example, pick Simple Curve for a two-lane small road and the other small roads use Simple Curve too, while a large road does not";
			d["item.toolMode.ex.menu"] = "every asset you click in the Roads menu is on Simple Curve, but moving to the Electricity menu it is not";
			d["item.toolMode.ex.category"] = "switching to a six-lane road or a bridge still gives Simple Curve, but a double subway track falls back to its default mode and needs its own setting, even though they are all in the Roads menu";

			// ---------- 3 Elevation ----------
			d["item.elevation.label"] = "Elevation";
			d["item.elevation.desc"] = "Remembers the elevation a tool was left at, including where Increase elevation and Decrease elevation took it. The Elevation step (how far one press moves you — the row Anarchy adds for the same value carries that very name) is remembered together with it and shares its scope. The elevation of intersections is not covered.";
			d["item.elevation.ex.group"] = "for example, raise a two-lane road to 10 m and the other small roads go to 10 m too, while a large road is not at 10 m";
			d["item.elevation.ex.menu"] = "every asset you click in the Roads menu is at 10 m, but moving to the Electricity menu it is not at 10 m";
			d["item.elevation.ex.category"] = "switching to a six-lane road or a bridge is still 10 m, but a double train track goes back to 0 m and needs its own setting, even though they are all in the Roads menu";

			// ---------- 4 Parallel Mode ----------
			d["item.parallel.label"] = "Parallel Mode";
			d["item.parallel.desc"] = "Remembers the Parallel Mode switch (Toggle parallel mode) together with the Parallel Road count and the Parallel Offset spacing that Increase offset and Decrease offset change. Parallel Mode builds parallel networks.";
			d["item.parallel.ex.group"] = "for example, turn on Parallel Mode for a two-lane small road and set the count to 3, the other small roads also build 3, while a large road does not";
			d["item.parallel.ex.menu"] = "every asset you click in the Roads menu builds 3 parallel lines, but moving to the Electricity menu it does not";
			d["item.parallel.ex.category"] = "switching to a six-lane road or a bridge still builds 3, but a tram track switches it off and needs its own setting, even though they are all in the Roads menu";

			// ---------- 5 Snapping ----------
			d["item.snap.label"] = "Snapping";
			d["item.snap.desc"] = "Remembers every switch on the Snapping row: Snap to existing geometry, Snap to zoning cell length, Snap to 90 degree angles, Snap to the sides of a road, Snap to roads, Snap to the side of the owner, Snap to the sides of a building, Snap to the middle of a road, Snap to shoreline, Snap to nearby geometry, Snap to guide lines, Snap to zone grid, Snap to nodes, Snap to the surface of an object, Snap upright, Snap to lot grid, Binds overlapping items to a building, Remove only matching type, Show contour lines and Snap to distance. The game's own Toggle all snapping on/off row is a batch switch and is not remembered.";
			d["item.snap.ex.group"] = "for example, switch off Snap to the sides of a road for a two-lane small road and the other small roads have it off too, while a large road still has everything on";
			d["item.snap.ex.menu"] = "every asset you click in the Roads menu uses this same set of snapping switches, but moving to the Electricity menu it does not";
			d["item.snap.ex.category"] = "switching to a six-lane road or a bridge keeps the same set, but a pathway returns to the defaults and needs its own setting, even though they are all in the Roads menu";

			// ---------- 6 Topography ----------
			d["item.topography.label"] = "Topography";
			d["item.topography.desc"] = "Remembers the Topography row, that is whether Show contour lines is on.";
			d["item.topography.ex.group"] = "for example, turn Topography on for a two-lane small road and the other small roads have it on too, while a large road does not";
			d["item.topography.ex.menu"] = "every asset you click in the Roads menu shows contour lines, but moving to the Electricity menu it does not";
			d["item.topography.ex.category"] = "switching to a six-lane road or a bridge still shows them, but an electric cable has the row off and needs its own setting";


			// ---------- 8 Left / Right (Anarchy) ----------
			d["item.leftRight.label"] = "Left and Right";
			d["item.leftRight.desc"] = "Remembers which network upgrades are picked on the Left and Right rows of the Anarchy network panel, for example Bike Lane, Trees, Parking, Quay, Retaining Wall and Sound Barrier. Needs Anarchy (Paradox mod 74604); without it this item does nothing.";
			d["item.leftRight.ex.group"] = "for example, pick Bike Lane on the right side of a two-lane small road and the other small roads get Bike Lane there too, while a large road does not";
			d["item.leftRight.ex.menu"] = "every asset you click in the Roads menu uses this same pair of side selections, but moving to the Electricity menu it does not";
			d["item.leftRight.ex.category"] = "switching to a six-lane road still has Bike Lane on the right, but a double train track goes back to nothing picked and needs its own setting, even though they are all in the Roads menu";

			// ---------- 9 General (Anarchy) ----------
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Remembers the switches on the General row of the Anarchy network panel: Ground, Elevated, Tunnel, Constant Slope, Wide Median and Expanded Elevation Range (which lifts the height limit). Needs Anarchy (Paradox mod 74604); without it this item does nothing.";
			d["item.general.ex.group"] = "for example, pick Tunnel for a two-lane small road and the other small roads are forced into tunnels too, while a large road is not";
			d["item.general.ex.menu"] = "every asset you click in the Roads menu uses this same set of General switches, but moving to the Electricity menu it does not";
			d["item.general.ex.category"] = "switching to a six-lane road or a bridge still gives Tunnel, but a double subway track goes back to nothing ticked and needs its own setting, even though they are all in the Roads menu";

			// ---------- 10 Underground Mode ----------
			d["item.underground.label"] = "Underground Mode";
			d["item.underground.desc"] = "Remembers the Underground Mode switch (Toggle underground mode), which decides whether what you place goes below the ground or on top of it.";
			d["item.underground.ex.group"] = "for example, turn on Underground Mode for a two-lane small road and the other small roads go underground too, while a large road does not";
			d["item.underground.ex.menu"] = "every asset you click in the Roads menu is built underground, but moving to the Electricity menu it is not";
			d["item.underground.ex.category"] = "switching to a six-lane road or a bridge still goes underground, but a double train track stays on the surface and needs its own setting, even though they are all in the Roads menu";

			// ---------- 11 Other ----------
			d["item.other.label"] = "Other";
			d["item.other.desc"] = "Remembers the remaining rows a tool panel can record: the road colours on the Color row, plus Brush Size and Brush Strength.";
			d["item.other.ex.group"] = "for example, set the colour of a two-lane small road to blue and the other small roads turn blue too, while a large road does not";
			d["item.other.ex.menu"] = "every asset you click in the Roads menu uses this same colour and brush setting, but moving to the Electricity menu it does not";
			d["item.other.ex.category"] = "switching to a six-lane road or a bridge is still blue, but a double train track keeps its own copy and needs its own setting, even though they are all in the Roads menu";
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
