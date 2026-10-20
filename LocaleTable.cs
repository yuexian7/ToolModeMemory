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
	///    「共用范围」那一行的说明文本里（用户要求）；例子按**工具类别**来写（v0.9.1 起），
	///    不再是统一的「道路 10m」。
	///  * 简体中文文案以用户给的原话为准，不经过英文回译。
	///  * Anarchy 开关 / 左侧和右侧 / 常规 这 3 项都属于 Anarchy（74604），
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

		// 共用范围说明的五行模板 + 尾注。{0} 由例子填入（v0.9.1 起按工具类别取 ex.<family>.*，
		// 只有建筑与服务页的项还用 item.<id>.ex.*）；没有例子时 Line() 会把 {0} 去掉。
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

		/// <summary>
		/// 范围下拉：短标签 + 按工具项标注（原版）（推荐）（推荐原版）。
		///
		/// v0.9.0 需求 4：**功能页与道具页不提供「同类」这一档** —— 原版只给 NetToolSystem
		/// 记偏好（按 m_Group = 资产分类，即我们的「同组」），功能型与摆件工具原版一个值都不记，
		/// 「原版档」对它们无意义；而它们彼此的模式枚举差别极大，按「同类」归组共用
		/// 等于把不同枚举的值塞进同一把键，会写下玩家设不出来的组合。
		/// 所以那两页只列 同组 / 同菜单 / 全局共用 / 全部不共用 四项，
		/// 「同类」只在建筑与服务页出现（那里的归类 = 资产服务谁，与 m_Group 之外多一档才有增益）。
		/// </summary>
		public static DropdownItem<int>[] BuildScopeDropdown(ToolItemDef def)
		{
			Dictionary<string, string> d = Table(_activeLocale);
			bool hasCategory = def == null || def.Domain == ItemDomain.Asset;
			var list = new List<DropdownItem<int>>
			{
				Item(0, kScopeGroup, def, d),
				Item(1, kScopeMenu, def, d),
			};
			if (hasCategory) list.Add(Item(2, kScopeCategory, def, d));
			list.Add(Item(3, kScopeGlobalShared, def, d));
			list.Add(Item(4, kScopeGlobalUnique, def, d));
			return list.ToArray();
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
			// 五个标签页的标题由 AddTabAndGroupTitles 反射 kTab* 常量统一注册（v0.7.0 起
			// 不再是「模组设置 + 关于」两页），这里只留关于页那一行以防反射漏掉。
			o[setting.GetOptionTabLocaleID(ToolModeMemorySettings.kTabAbout)] = Val(d, "tab.about");
			AddTabAndGroupTitles(setting, d, o);

			// 大开关 + 三块域开关（共用范围定义已并入每一项的 scope 说明，见 AddItems）
			// v0.6.0：CompatOtherMods 那一行已经整行取消，所以这里不再为它注册 label/desc。
			o[setting.GetOptionLabelLocaleID("Enabled")] = Val(d, "enabled.label");
			o[setting.GetOptionDescLocaleID("Enabled")] = Val(d, "enabled.desc");
			o[setting.GetOptionLabelLocaleID("AssetMemoryEnabled")] = Val(d, "domain.asset.label");
			o[setting.GetOptionDescLocaleID("AssetMemoryEnabled")] = Val(d, "domain.asset.desc");
			o[setting.GetOptionLabelLocaleID("FunctionMemoryEnabled")] = Val(d, "domain.function.label");
			o[setting.GetOptionDescLocaleID("FunctionMemoryEnabled")] = Val(d, "domain.function.desc");
			o[setting.GetOptionLabelLocaleID("ItemMemoryEnabled")] = Val(d, "domain.item.label");
			o[setting.GetOptionDescLocaleID("ItemMemoryEnabled")] = Val(d, "domain.item.desc");

			AddItems(setting, d, o);

			// 记忆管理（关于页）
			o[setting.GetOptionLabelLocaleID("ResetMemory")] = Val(d, "reset.label");
			o[setting.GetOptionDescLocaleID("ResetMemory")] = Val(d, "reset.desc");
			o[setting.GetOptionWarningLocaleID("ResetMemory")] = Val(d, "reset.warn");
			o["Options.WARNING[CONFIRM_RESET]"] = Val(d, "reset.confirm");

			o[setting.GetOptionLabelLocaleID("ResetAllSettings")] = Val(d, "resetall.label");
			o[setting.GetOptionDescLocaleID("ResetAllSettings")] = Val(d, "resetall.desc");
			o[setting.GetOptionWarningLocaleID("ResetAllSettings")] = Val(d, "resetall.warn");
			// 确认弹窗的键必须是 "Options.WARNING[" + SettingsUIConfirmation 的第一个参数 + "]"
			// （AutomaticSettings.GetConfirmationMessage L1153），设置类里写的是
			// CONFIRM_RESET_SETTINGS。0.6.2 之前这里注册的是 CONFIRM_RESET_ALL，
			// 于是那条文案永远查不到、弹窗只显示一个空白确认框 —— 顺手修掉。
			o["Options.WARNING[CONFIRM_RESET_SETTINGS]"] = Val(d, "resetall.confirm");
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
		/// 目录里的每一项：启用开关 + 共用范围下拉，两行都按 <see cref="ToolItemDef.Number"/>
		/// 与 <see cref="ToolItemDef.Source"/> 自动出现，新增一项不需要改这里。
		/// 标签在这里补上目录里的序号（序号在**每个标签页内**各自从 1 数，owner 需求 5）。
		///
		/// v0.7.0 三个标签页里有同名项（工具模式 / 对齐 / 等高线 / 地下模式 / 笔刷），
		/// 行名必须是同一个词，所以词条按键回落到资产域那一份（见 <see cref="Val2"/>）：
		///   item.f.toolMode.label 缺 → item.toolMode.label
		/// 只有**含义真的不同**的地方才另写一条（例如功能域「笔刷」讲的是 9 个地形改造工具）。
		/// 需求 4：五种共用范围的定义写进**本项**共用范围行的说明文本。
		/// v0.9.1：例子改成按工具类别取（ToolItemDef.ExampleFamily），不再从建筑与服务页借词。
		/// </summary>
		private static void AddItems(ToolModeMemorySettings setting, Dictionary<string, string> d, Dictionary<string, string> o)
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				string pascal = PascalCase(def.Id);
				o[setting.GetOptionLabelLocaleID(pascal + "Enabled")] = def.Number.ToString() + ". " + ItemText(d, def, "label");
				o[setting.GetOptionDescLocaleID(pascal + "Enabled")] = ItemText(d, def, "desc");
				o[setting.GetOptionLabelLocaleID(pascal + "Scope")] = Val(d, "scope.label");
				o[setting.GetOptionDescLocaleID(pascal + "Scope")] = ScopeDescription(def, d);
			}
		}

		/// <summary>
		/// 某一项的一条词条（label / desc / ex.group …），按三级回退找：
		///   1) item.&lt;本项 id&gt;.&lt;slot&gt;      —— 本项自己写过（功能页/道具页想改主语时用这条）
		///   2) item.&lt;LocaleAlias&gt;.&lt;slot&gt;  —— 换了名字的旧项（「配色」沿用「其它」的 12 语言文案）
		///   3) item.&lt;去掉域前缀&gt;.&lt;slot&gt;    —— 功能页 / 道具页的同名行沿用资产页那一份
		/// 三级都没有才原样返回键名（便于在实机上一眼看出漏写）。
		/// </summary>
		private static string ItemText(Dictionary<string, string> d, ToolItemDef def, string slot)
		{
			string key = "item." + def.Id + "." + slot;
			string value;
			if (d.TryGetValue(key, out value)) return value;
			if (!string.IsNullOrEmpty(def.LocaleAlias))
			{
				if (d.TryGetValue("item." + def.LocaleAlias + "." + slot, out value)) return value;
			}
			string baseId = BaseId(def.Id);
			if (baseId != def.Id && d.TryGetValue("item." + baseId + "." + slot, out value)) return value;
			Dictionary<string, string> en = Table("en-US");
			if (!ReferenceEquals(d, en))
			{
				if (en.TryGetValue(key, out value)) return value;
				if (!string.IsNullOrEmpty(def.LocaleAlias)
					&& en.TryGetValue("item." + def.LocaleAlias + "." + slot, out value)) return value;
				if (baseId != def.Id && en.TryGetValue("item." + baseId + "." + slot, out value)) return value;
			}
			return key;
		}

		/// <summary>
		/// 取「同组 / 同菜单 / 同类」那一行的例子。
		///
		/// v0.9.1：例子改为**按工具类别**取（<see cref="ToolItemDef.ExampleFamily"/>），
		/// 只有建筑与服务页（family 为空）才用本项自己的 <c>item.&lt;id&gt;.ex.*</c>。
		/// 0.9.0 及以前这里走的是 ItemText 的三级回退，功能页 / 道具页的项没有自己的例子时
		/// 会掉到「去掉 f./i. 前缀」的建筑与服务页文案上 ——
		/// 于是功能页和道具页上出现了道路 / 小型道路（owner 0.9.1 需求 1 点名的那些句子）。
		/// 现在跨页取例子在结构上就不可能发生：功能页只会取到 zone / route / terrain 三类，
		/// 道具页只会取到 prop / tree 两类。
		/// family = "none" 表示这一项不适合举例（比如距离只在标记对象上出现，
		/// 而标记对象全在一个分组里，同组与同菜单本来就是同一批工具），
		/// 这时只保留模板里的定义句，{0} 由 Line() 去掉。
		/// 找不到键也返回空串（宁可少一句例子，也不显示键名）。
		/// </summary>
		private static string Example(Dictionary<string, string> d, ToolItemDef def, string slot)
		{
			string family = def != null ? def.ExampleFamily : null;
			if (!string.IsNullOrEmpty(family))
			{
				if (family == "none") return string.Empty;
				string familyKey = "ex." + family + "." + slot;
				string familyValue;
				return d.TryGetValue(familyKey, out familyValue) ? familyValue : string.Empty;
			}
			string value = ItemText(d, def, "ex." + slot);
			return value.StartsWith("item.", StringComparison.Ordinal) ? "" : value;
		}

		/// <summary>
		/// 功能域 / 道具域的项 id 形如 "f.toolMode" / "i.brush"，去掉域前缀就是它在资产域的
		/// 同名项（三页的同名行是同一个游戏面板行，行名与说明的主体当然要用同一个词）。
		/// </summary>
		private static string BaseId(string id)
		{
			if (string.IsNullOrEmpty(id)) return id;
			int dot = id.IndexOf('.');
			return dot > 0 ? id.Substring(dot + 1) : id;
		}

		/// <summary>先查本键，没有再查回退键；两个都没有才原样返回**本键**（便于发现漏写）。</summary>
		private static string Val2(Dictionary<string, string> d, string key, string fallbackKey)
		{
			string value;
			if (d != null && d.TryGetValue(key, out value)) return value;
			if (d != null && !string.IsNullOrEmpty(fallbackKey) && d.TryGetValue(fallbackKey, out value)) return value;
			Dictionary<string, string> en = Table("en-US");
			if (!ReferenceEquals(d, en))
			{
				if (en.TryGetValue(key, out value)) return value;
				if (!string.IsNullOrEmpty(fallbackKey) && en.TryGetValue(fallbackKey, out value)) return value;
			}
			return key;
		}

		/// <summary>
		/// 某一工具项的「共用范围」说明文本：一句引导 + 五行定义 + 一句三域互不相通的界定。
		/// 缺键由 FillGaps 回落 en-US，不会拼出空行。
		///
		/// v0.7.0：只有「同类」这一行的定义真的分三个标签页（资产看这件资产服务谁、
		/// 功能看 6 个功能分类、道具看城市美化的 8 类道具），所以功能页与道具页各查一把
		/// 专用键（scope.line.category.function / .item）；**资产页仍用不带后缀的通用键**，
		/// 12 份表里已经写好的那句照用，不必再抄一份。缺专用句时 Line() 自动退回通用句。
		/// 其余四行（同组 / 同菜单 / 全局共用 / 全部不共用）三个页说的是同一件事，
		/// 各写一句就够 —— 抄三遍只会带来「改一处漏两处」的不一致。
		/// 例子（{0}）按工具类别取：功能页只会出现功能区 / 路线 / 地形改造，
		/// 道具页只会出现城市美化菜单下的道具分类（见 ToolItemDef.ExampleFamily）。
		///
		/// 说明里的「五行」在三个页上都用**同一句**定义：三页只是各自挑了面板上哪几行来记，
		/// 五档共用范围讲的是「在多大范围内共用」，与在哪一页无关。
		/// 只有例子必须按页现写：道路的对齐、功能区的对齐、道具的对齐可选项不一样。
		/// </summary>
		private static string ScopeDescription(ToolItemDef def, Dictionary<string, string> d)
		{
			// v0.8.0 需求 4：三页彻底分开后，「同类 / 全局共用 / 全部不共用」这三句都要说当前这一页
			// 自己的名词（建筑与服务 / 功能 / 道具），不能再提别的页。三句都按域查专用键
			// （scope.line.X.<域后缀>），本语言没写过专用键时 Line() 自动退回不带后缀的通用句
			// （资产页用的就是通用句，所以只有功能页/道具页需要额外的两把键）。
			string suffix = "." + DomainSuffix(def.Domain);
			// v0.9.0 需求 4：功能页与道具页不再提供「同类」这一档
			// （见 BuildScopeDropdown 的说明），所以这两页的说明里也不该出现那一行。
			bool hasCategory = def.Domain == ItemDomain.Asset;
			// 「游戏里只有一份设置」的项（等高线 / 地下模式 / Anarchy 三项）走 sharedSingle：
			// 那句说的是「本页所有支持这项的工具共用一份」，三页通用，不用按域换名词。
			string sharedKey = def.FamilyFreeScope
				? kScopeLineSharedSingle
				: (def.Domain == ItemDomain.Asset ? kScopeLineShared : kScopeLineShared + suffix);
			string uniqueKey = def.Domain == ItemDomain.Asset
				? kScopeLineUnique
				: kScopeLineUnique + suffix;
			StringBuilder sb = new StringBuilder();
			// v0.9.1：功能页 / 道具页的下拉里没有（原版）标签，所以那句「括号里标出游戏原本的做法」
			// 在这两页上是错的 —— 各页各用一句只说范围的话。
			sb.Append(Val2(d, "scope.desc" + suffix, "scope.desc"));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineGroup, Example(d, def, "group")));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineMenu, Example(d, def, "menu")));
			if (hasCategory)
			{
				sb.Append('\n');
				sb.Append(Line(d, kScopeLineCategory, Example(d, def, "category")));
			}
			sb.Append('\n');
			// 这两句没有例子可填，但仍要走按域回退：Line(d, key, "") 有域专用键就用它，
			// 没有就剥掉最后的域后缀用通用句（与 category 行同一套机制）。
			sb.Append(Line(d, sharedKey, null));
			sb.Append('\n');
			sb.Append(Line(d, uniqueKey, null));
			sb.Append('\n');
			sb.Append(Val(d, kScopeNote));
			return sb.ToString();
		}

		/// <summary>域 → 键后缀（asset / function / item）。</summary>
		private static string DomainSuffix(ItemDomain domain)
		{
			switch (domain)
			{
				case ItemDomain.Function: return "function";
				case ItemDomain.Item: return "item";
				default: return "asset";
			}
		}

		/// <summary>
		/// 取模板并把 {0} 换成这一项的例子。本语言没有这把精确键时退回**去掉最后一段**的通用键
		/// （scope.line.category.function → scope.line.category），两个都没有才让这一行整行消失：
		/// 少一句说明远好过在设置页上糊一串 "scope.line.category.function" 这种键名。
		/// 回退必须先看本语言的通用键再看 en-US —— Table() 里的 FillGaps 已经把 en-US 的键
		/// 并进本语言字典，所以直接在 d 里查两次就同时覆盖了「本语言的通用句」与「英文的通用句」，
		/// 而且本语言写过通用句时不会被英文顶掉。
		/// </summary>
		private static string Line(Dictionary<string, string> d, string templateKey, string example)
		{
			string template = Raw(d, templateKey);
			if (template == null)
			{
				int dot = templateKey.LastIndexOf('.');
				if (dot <= 0) return "";
				template = Raw(d, templateKey.Substring(0, dot));
				if (template == null) return "";
			}
			// v0.9.1：没有例子时只留定义句，绝不能把 {0} 这个占位符露给玩家
			if (string.IsNullOrEmpty(example)) return template.Replace("{0}", string.Empty).TrimEnd();
			return template.Replace("{0}", example);
		}

		/// <summary>本语言有就有，没有返回 null（不走 en-US 回落，理由见 <see cref="Line"/>）。</summary>
		private static string Raw(Dictionary<string, string> d, string key)
		{
			string value;
			if (d != null && d.TryGetValue(key, out value)) return value;
			return null;
		}

		/// <summary>
		/// id → 设置类里的属性名主干。"toolMode"→ToolMode、"leftRight"→LeftRight、
		/// "f.toolMode"→FToolMode、"i.brush"→IBrush（v0.7.0 的功能域 / 道具域项 id 带域前缀，
		/// 属性名是 FToolModeEnabled / IBrushScope 这种）。
		/// 与 <c>ToolItemDef.PascalId()</c> 必须是同一套规则，否则注册出来的键没人查。
		/// </summary>
		private static string PascalCase(string id)
		{
			if (string.IsNullOrEmpty(id)) return id;
			string[] parts = id.Replace(".", " ").Split(' ');
			StringBuilder sb = new StringBuilder(id.Length);
			for (int i = 0; i < parts.Length; i++)
			{
				string p = parts[i];
				if (p.Length == 0) continue;
				sb.Append(char.ToUpperInvariant(p[0]));
				if (p.Length > 1) sb.Append(p.Substring(1));
			}
			return sb.ToString();
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
					o[setting.GetOptionTabLocaleID(value)] = TabTitle(name.Substring(4), d);
				}
				else if (name.StartsWith("kGroup", StringComparison.Ordinal))
				{
					o[setting.GetOptionGroupLocaleID(value)] = GroupTitle(name, value, d);
				}
			}
		}

		/// <summary>
		/// 标签页标题：v0.7.0 起有五个页（总开关 / 资产 / 功能 / 道具 / 关于），
		/// 按 kTab 后缀查 tab.&lt;后缀小写&gt;。查不到时退回旧的两个键（tab.mod / tab.about），
		/// 保证某个语言漏写时页签仍然有名字，而不是显示英文键名。
		/// </summary>
		private static string TabTitle(string suffix, Dictionary<string, string> d)
		{
			string value;
			if (d.TryGetValue("tab." + suffix.ToLowerInvariant(), out value) && !string.IsNullOrEmpty(value))
			{
				return value;
			}
			if (suffix.IndexOf("About", StringComparison.OrdinalIgnoreCase) >= 0) return Val(d, "tab.about");
			return Val(d, "tab.mod");
		}

		/// <summary>
		/// 板块语义判定。v0.7.0 的板块结构：
		///   kGroupMaster       大开关（不显示标题，注册一个占位串就行）
		///   kGroupDomain       「启用 X 的工具项记忆」三块
		///   kGroup*Official    官方工具项设置（资产 / 功能 / 道具三页共用一句标题）
		///   kGroup*Anarchy     Anarchy工具项设置（模组名 + 「工具项设置」）
		///   kGroupMemory / kGroupInfo 关于页两块（v0.6.0 去掉了「兼容性」那一块）
		/// 设置类里的 kGroup* 常量会随重构改名，所以这里反射读取常量本身，按语义给标题。
		/// </summary>
		private static string GroupTitle(string field, string value, Dictionary<string, string> d)
		{
			string f = field.ToLowerInvariant();
			if (f.EndsWith("master", StringComparison.Ordinal)) return Val(d, "group.master");
			if (f.EndsWith("domain", StringComparison.Ordinal)) return Val(d, "group.domain");
			if (f.IndexOf("anarchy", StringComparison.Ordinal) >= 0) return Val(d, "group.anarchy");
			if (f.EndsWith("official", StringComparison.Ordinal)) return Val(d, "group.official");
			if (f.EndsWith("items", StringComparison.Ordinal) || f.EndsWith("main", StringComparison.Ordinal)
				|| f.EndsWith("settings", StringComparison.Ordinal) || f.EndsWith("mod", StringComparison.Ordinal))
			{
				return Val(d, "group.official");
			}
			string s = f + "|" + value.ToLowerInvariant();
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
		// 权威语言：五行共用范围定义沿用用户给的原话，例子按工具类别改写（v0.9.1），
		// 不经过英文回译。其它语言由这份定稿翻译。

		private static Dictionary<string, string> ZhHans()
		{
			Dictionary<string, string> d = Frames("zh-HANS");
			d["mod.name"] = "工具模式区分记忆";
			// v0.7.0 需求 3：五个标签页（总开关 / 资产 / 功能 / 道具 / 关于）。
			d["tab.mod"] = "工具模式记忆设置";
			d["tab.master"] = "总开关";
			d["tab.assets"] = "建筑与服务";
			d["tab.functions"] = "功能";
			d["tab.items"] = "道具";
			d["tab.about"] = "关于";
			d["group.master"] = "工具模式记忆";
			d["group.domain"] = "分域开关";
			d["group.official"] = "官方工具项设置";
			d["group.anarchy"] = "Anarchy工具项设置";
			d["group.reset"] = "记忆管理";
			d["group.about"] = "信息与链接";

			// 大开关：需求 3 要求改叫「启用模组」，它管住下面所有共用设置与读档恢复。
			// v0.8.0 需求 3：「资产」页改名「建筑与服务」（道具本身也是资产，旧名有歧义）。
			d["enabled.label"] = "启用模组";
			// v0.9.0 文案
			d["enabled.desc"] = "本模组的全部功能的开关。\n打开后下面三个分域开关才会生效，每个工具面板的设置都会实时记住，回到存档时自动恢复。\n关掉后模组完全不参与，所有工具都是游戏原本的样子。";
			d["domain.asset.label"] = "启用建筑与服务的工具项记忆";
			// v0.8.0 需求 5：分类口径的说明写在这里（不在共用范围的下拉说明里重复一遍）。
			// v0.9.0 文案
			d["domain.asset.desc"] = "管这一页所有工具项是否记忆：\n打开时，你的设置会实时记住，回到存档时自动恢复；\n关掉时不记忆、也不会从存档恢复，共用范围自然也就无从设置。\n这一页收录道路、建筑、服务设施等可以建造的东西。";
			d["domain.function.label"] = "启用功能的工具项记忆";
			// v0.9.0 文案
			d["domain.function.desc"] = "管这一页所有工具项是否记忆：\n打开时，你的设置会实时记住，回到存档时自动恢复；\n关掉时不记忆、也不会从存档恢复，共用范围自然也就无从设置。\n这一页收录功能区、区域、路线、地形改造等非资产类工具。";
			d["domain.item.label"] = "启用道具的工具项记忆";
			// v0.9.0 文案
			d["domain.item.desc"] = "管这一页所有工具项是否记忆：\n打开时，你的设置会实时记住，回到存档时自动恢复；\n关掉时不记忆、也不会从存档恢复，共用范围自然也就无从设置。\n这一页收录城市美化菜单里的道具，树木也在这一页。";


			d["scope.label"] = "共用范围";
			// v0.9.0 需求 4：功能页与道具页不再有推荐，所以这里只说「标出原版行为」。
			d["scope.desc"] = "这一项的数值在多大范围内共用。括号里标出游戏原本的做法（原版）。";
			// v0.9.1：三行都先给定义，再给例子 —— 定义句与工具项无关，永远成立；
			// 例子按「工具类别」取（ex.<family>.*），不再从别的页回落。
			d[kScopeLineGroup] = "同组：同一个菜单栏分组里的工具共用一份。{0}";
			d[kScopeLineMenu] = "同菜单：整个菜单里的工具共用一份。{0}";
			d[kScopeLineCategory] = "同类：按资产服务的对象归成群组，同一群共用一份。{0}";
			// ---- 共用范围的例子（v0.9.1）----
			// 只描述「哪些工具算一组 / 一个菜单」，不描述某一行的数值，
			// 所以同一类工具里的每一项都能共用这一句，也不会再出现「换到另一组」而实际没有另一组的情况。
			d["ex.net.group"] = "比如小型道路这一组里的各种道路共用一份；换到大型道路就是另一组了。";
			d["ex.net.menu"] = "比如道路菜单里的所有道路共用一份；换到电力菜单就不是了。";
			d["ex.net.category"] = "比如各车道数的道路都算车行道路，共用一份；桥梁、双线铁路各算一类。";
			d["ex.zone.group"] = "比如住宅区里的低密度、中密度、高密度住宅算一组，共用一份；换到商业区就是另一组（两个区都在同一个菜单下）。";
			d["ex.zone.menu"] = "比如住宅区、商业区、工业区、办公区都在功能区菜单下，共用一份；换到别的菜单就不是了。";
			d["ex.route.group"] = "比如线路按交通工具分组（道路、有轨电车、火车、地铁等），同一组的线路共用一份；换到另一种交通工具的线路就是另一组。";
			d["ex.route.menu"] = "比如交通运输菜单里的所有线路共用一份；换到别的菜单就不是了。";
			// 地形改造只有一个分组：同组与同菜单在这个菜单里本来就是同一批工具（owner 0.9.1 需求 2），
			// 所以这两句不再假装有「另一组」。
			d["ex.terrain.group"] = "地形改造只有一个分组，所以同组和同菜单选出来的是同一批工具。";
			d["ex.terrain.menu"] = "地形改造只有一个分组，所以同菜单和同组选出来的是同一批工具。";
			d["ex.prop.group"] = "比如城市美化菜单下分成自然、公园、住宅道具、商业道具、工业道具、残骸、贴花、盆栽几类，同一类的道具共用一份；换到另一类就是另一组。";
			d["ex.prop.menu"] = "城市美化菜单下的所有道具和植被共用一份。";
			d["ex.tree.group"] = "树都在城市美化菜单的植被分组里，所以同组和同菜单选出来的是同一批树。";
			d["ex.tree.menu"] = "树都在城市美化菜单的植被分组里，所以同菜单和同组选出来的是同一批树。";
			d[kScopeLineShared] = "全局共用：本页内所有支持这一项的建筑与服务都共用一份";
			d[kScopeLineSharedSingle] = "全局共用：本页内所有支持这一项的工具都共用一份";
			// v0.8.0 需求 4：三页完全分开，范围说明只说当前这一页的名词，不做跨页对比。
			// 功能页/道具页各查一把专用键，本语言没写时 Line() 自动退回上面那句通用句。
			d[kScopeLineShared + ".function"] = "全局共用：本页内所有支持这一项的功能工具都共用一份";
			d[kScopeLineShared + ".item"] = "全局共用：本页内所有支持这一项的道具都共用一份";
			d[kScopeLineUnique] = "全部不共用：本页内每一种建筑与服务都各自独立设置";
			d[kScopeLineUnique + ".function"] = "全部不共用：本页内每一个功能工具都各自独立设置";
			d[kScopeLineUnique + ".item"] = "全部不共用：本页内每一种道具都各自独立设置";
			d[kScopeNote] = "上面每一档都只在当前这一页里生效。";

			d[kScopeGroup] = "同组";
			d[kScopeMenu] = "同菜单";
			d[kScopeCategory] = "同类";
			d[kScopeCategory + ".function"] = "同类";
			d[kScopeCategory + ".item"] = "同类道具";
			d[kScopeGlobalShared] = "全局共用";
			d[kScopeGlobalUnique] = "全部不共用";
			d[kTagVanilla] = "（原版）";
			d[kTagRecommended] = "（推荐）";
			d[kTagRecVanilla] = "（推荐原版）";

			d["reset.label"] = "重置记忆";
			d["reset.desc"] = "游戏中：清除当前存档的记忆，工具恢复刚进档时的原版状态。主菜单或编辑器：清除所有存档的记忆。";
			d["reset.warn"] = "此操作无法撤销。";
			d["reset.confirm"] = "确定要重置已记忆的工具设置吗？";
			d["resetall.label"] = "重置所有设置项";
			d["resetall.desc"] = "把本模组的所有选项回到推荐默认值：总开关重新打开，每一项的启用状态和共用范围也一并恢复。已记录的存档记忆不受影响。";
			d["resetall.warn"] = "此操作无法撤销。";
			d["resetall.confirm"] = "确定要重置所有设置项吗？";
			d["folder.label"] = "管理所有存档的记忆文件";
			d["folder.desc"] = "打开保存记忆的本地文件夹，每个存档一份文件，文件名就是「载入游戏」里看到的存档名。模组读取记忆都是按存档名，存档名称和 json 文件名一致才会读取，所以可以通过修改文件名的方式实现复制。打开自动存档时，记忆仍然写进它原存档名的那份文件并覆盖它（自动存档的名字每次都不同，不能当文件名）；只有认不出原存档的自动存档才暂按城市名保存，等你手动存盘后自动改成真正的存档名。";

			d["about.version"] = "模组版本";
			d["about.author"] = "作者";
			d["about.kofi"] = "请我喝杯咖啡";
			d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
			d["about.forum"] = "论坛页面";
			d["about.forum.desc"] = "打开 Paradox 论坛帖子。";
			d["about.rainbow"] = "RAINBOW官网";
			d["about.rainbow.desc"] = "打开 RAINBOW 系列官网。";

			// ---------- Anarchy ----------
			// v0.9.0 文案
			d["item.anarchy.desc"] = "记住 Anarchy 模组的开关。\n没安装 Anarchy 时这一项不会出现。";
			d["item.anarchy.ex.group"] = "比如给两车道小型道路打开 Anarchy，切换到其它的小型道路，也是打开的，但切换到大型道路就不是了。";
			d["item.anarchy.ex.menu"] = "点击道路菜单里面的所有资产，Anarchy 都是打开的，但切换到电力菜单就不是了。";
			d["item.anarchy.ex.category"] = "切换到六车道道路也是打开的，但切换到桥梁或双向地铁轨道就变成关闭（需单独设置），即使它们都在道路菜单下。";

			// 地区主题 / 数据包：工具栏筛选面板那两行，行名照原版语言包
			// （Toolbar.THEME_PANEL_TITLE / Toolbar.ASSET_PACKS_PANEL_TITLE）。
			d["item.themes.label"] = "地区主题";
			// v0.9.0 文案
			d["item.themes.desc"] = "记住工具栏筛选面板里「地区主题」勾选了哪些。\n只有当前分类里确实有资产用到主题时才有这一行。";
			d["item.themes.ex.group"] = "比如只在道路/小型道路里勾了某个主题，切换到大型道路就不按它筛选了。";
			d["item.themes.ex.menu"] = "在道路菜单里勾的主题，切换到电力菜单就不生效，回到道路菜单又还是那一套。";
			d["item.themes.ex.category"] = "这一行的勾选属于工具栏面板本身，按当前所在的菜单+分类记忆，不按资产类别共用。";
			d["item.packs.label"] = "数据包";
			// v0.9.0 文案
			d["item.packs.desc"] = "记住工具栏筛选面板里「数据包」勾选了哪些。\n勾上后工具栏只列出属于这些数据包的资产。";
			d["item.packs.ex.group"] = "比如在道路/小型道路勾了某个数据包，切到大型道路会被原版清空，回到小型道路又自动恢复。";
			d["item.packs.ex.menu"] = "在道路菜单勾的数据包，和电力菜单勾的各记各的。";
			d["item.packs.ex.category"] = "这一行的勾选属于工具栏面板本身，按当前所在的菜单+分类记忆；原版每次换分类都会把它清空。";

			// ---------- 工具模式 ----------
			d["item.toolMode.label"] = "工具模式";
			// v0.9.0 文案
			d["item.toolMode.desc"] = "记住每个工具所选的模式。\n这一页的可选值就是直线、简单曲线、复杂曲线、连续曲线、网格、替换、节点，以及建筑的放置方式。";
			d["item.toolMode.ex.group"] = "比如给两车道小型道路选了简单曲线，切换到其它的小型道路，也是简单曲线，但切换到大型道路就不是了。";
			d["item.toolMode.ex.menu"] = "点击道路菜单里面的所有资产，都是简单曲线，但切换到电力菜单就不是了。";
			d["item.toolMode.ex.category"] = "切换到六车道道路也是简单曲线，但切换到桥梁或双向地铁轨道就回到默认模式（需单独设置），即使它们都在道路菜单下。";

			// ---------- 高度（用户原话的例子） ----------
			d["item.elevation.label"] = "高度";
			// v0.9.0 文案
			d["item.elevation.desc"] = "记住道路离开时的高度，包括抬高、降低之后的结果。\n「高度阶段」（一次抬高或降低多少米）跟着高度一起记，共用同一个范围。";
			d["item.elevation.ex.group"] = "比如点击两车道道路将高度提高到10m，那么切换其它的小型道路，高度也是10m，但切换到大型道路就不是10m了。";
			d["item.elevation.ex.menu"] = "点击所有道路菜单里面的资产，高度都是10m，但切换到电力菜单就不是10m了。";
			d["item.elevation.ex.category"] = "切换到六车道道路高度也是10m，但切换到桥梁或双线铁路就变成0m（需单独设置），即使它们都在道路菜单下。";

			// ---------- 并列模式 ----------
			d["item.parallel.label"] = "并列模式";
			// v0.9.0 文案
			d["item.parallel.desc"] = "记住并列道路的开关与两条线之间的间距。\n原版一次并列就是两条；关掉只画一条。";
			d["item.parallel.ex.group"] = "比如给两车道小型道路打开并列模式并把并列偏移调到5米，切换到其它的小型道路，同样是打开、偏移也是5米，但切换到大型道路就不是了。";
			d["item.parallel.ex.menu"] = "点击道路菜单里面的所有资产，并列都是打开、偏移都是5米，但切换到电力菜单就不是了。";
			d["item.parallel.ex.category"] = "切换到六车道道路同样是打开、偏移也是5米，但切换到桥梁或地铁轨道就变成关闭（需单独设置），即使它们都在道路菜单下。";

			// ---------- 对齐 ----------
			d["item.snap.label"] = "对齐";
			// v0.9.0 文案
			d["item.snap.desc"] = "记住「对齐」里各个开关的勾选状态。\n游戏自带的「开启／关闭所有对齐」是批量开关，本身不记忆。";
			d["item.snap.ex.group"] = "比如给两车道小型道路关掉快速对齐路边，切换到其它的小型道路，也是关着的，但切换到大型道路就是全开的默认状态。";
			d["item.snap.ex.menu"] = "点击道路菜单里面的所有资产，对齐开关都是这一套，但切换到电力菜单就不是了。";
			d["item.snap.ex.category"] = "切换到六车道道路也保持这一套开关，但切换到桥梁或步行道就恢复默认（需单独设置），即使它们都在道路菜单下。";

			// ---------- 地形 ----------
			d["item.topography.label"] = "地形";
			// v0.9.0 文案
			d["item.topography.desc"] = "记住「地形」这一行，也就是显示轮廓线条的开与关。";
			d["item.topography.ex.group"] = "比如给两车道小型道路勾上地形，切换到其它的小型道路，同样是勾着的，但切换到大型道路就不是了。";
			d["item.topography.ex.menu"] = "点击道路菜单里面的所有资产，地形都是勾着的，但切换到电力菜单就不是了。";
			d["item.topography.ex.category"] = "切换到六车道道路也是勾着的，但切换到桥梁或电缆就变成不勾（需单独设置）。";


			// ---------- 左侧和右侧（Anarchy） ----------
			d["item.leftRight.label"] = "左侧和右侧";
			// v0.9.0 文案
			d["item.leftRight.desc"] = "记住 Anarchy 网络面板里「左侧」「右侧」两行所选的网络升级。\n没安装 Anarchy 时这一项不会出现。";
			d["item.leftRight.ex.group"] = "比如给两车道小型道路的右侧选上自行车车道，切换到其它的小型道路，右侧同样是自行车车道，但切换到大型道路就不是了。";
			d["item.leftRight.ex.menu"] = "点击道路菜单里面的所有资产，左右两侧都是这一套升级，但切换到电力菜单就不是了。";
			d["item.leftRight.ex.category"] = "切换到六车道道路右侧也是自行车车道，但切换到桥梁或双线铁路就变成未选择（需单独设置），即使它们都在道路菜单下。";

			// ---------- 常规（Anarchy） ----------
			d["item.general.label"] = "常规";
			// v0.9.0 文案
			d["item.general.desc"] = "记住 Anarchy 网络面板里「常规」一行的开关。\n没安装 Anarchy 时这一项不会出现。";
			d["item.general.ex.group"] = "比如给两车道小型道路勾上强制隧道，切换到其它的小型道路，同样是强制隧道，但切换到大型道路就不是了。";
			d["item.general.ex.menu"] = "点击道路菜单里面的所有资产，常规选项都是这一套，但切换到电力菜单就不是了。";
			d["item.general.ex.category"] = "切换到六车道道路也是强制隧道，但切换到桥梁或双向地铁轨道就全部回到未勾选（需单独设置），即使它们都在道路菜单下。";
			// ---------- 物件高度（v0.8.0：Anarchy 注入在摆件工具面板上的一整排） ----------
			// owner 2026-10-20 定：锁定变换 / 高度 / 高度变化范围 / 抬高降低的幅度 是同一排显示的，
			// 算同一项、一起记忆一起共用；「重置高度为 0」是瞬时按钮，不列。
			d["item.anarchyObjectElevation.label"] = "物件高度";
			// v0.9.0 文案
			d["item.anarchyObjectElevation.desc"] = "记住 Anarchy 在工具面板上加的那一排高度控件：锁定变换、高度、高度变化范围、抬高降低的幅度。\n这四项在同一排显示，所以一起记忆、一起共用。";

			// ---------- 地下模式 ----------
			d["item.underground.label"] = "地下模式";
			// v0.9.0 文案
			d["item.underground.desc"] = "记住「地下模式」开关，也就是你放的东西在地面以下还是地面之上。";
			d["item.underground.ex.group"] = "比如给两车道小型道路打开地下模式，切换到其它的小型道路，同样在地下，但切换到大型道路就不是了。";
			d["item.underground.ex.menu"] = "点击道路菜单里面的所有资产，都在地下模式，但切换到电力菜单就不是了。";
			d["item.underground.ex.category"] = "切换到六车道道路也在地下，但切换到桥梁或双线铁路就回到地面（需单独设置），即使它们都在道路菜单下。";

			// ---------- 其它 ----------

			// ---------- 笔刷家族（v0.8.0 按 owner 要求全部拆开：种类 / 粗细 / 强度 各自独立一项） ----------
			// 行名取官方语言包：Toolbar.BRUSH_TYPE（官方未译，仍显示 Brush Type）、
			// Toolbar.BRUSH_SIZE = 笔刷粗细、Toolbar.BRUSH_STRENGTH = 笔刷强度。
			// 本页（建筑与服务）没有这三行 —— 只有摆件工具的可刷涂资产与地形改造工具才有。
			d["item.brushType.label"] = "笔刷种类";
			// v0.9.0 文案
			d["item.brushType.desc"] = "记住「笔刷种类」这一行选的刷子：圆形、方形、柔边等。\n只有能刷涂的工具才有这一行。";
			d["item.brushSize.label"] = "笔刷粗细";
			// v0.9.0 文案
			d["item.brushSize.desc"] = "记住「笔刷粗细」这一行：一次刷过多大范围。\n只有能刷涂的工具才有这一行。";
			d["item.brushStrength.label"] = "笔刷强度";
			// v0.9.0 文案
			d["item.brushStrength.desc"] = "记住「笔刷强度」这一行：每一下刷动改变多少。\n只有能刷涂的工具才有这一行。";

			// ---------- 颜色（v0.7.0 从「其它」独立；面板行名 ToolOptions.COLOR_TITLE = 颜色） ----------
			// 说明与例子沿用旧「其它」那一份文案（内容本来就全是讲道路颜色），这里只把行名写实。
			d["item.color.label"] = "颜色";
			// v0.9.1：「颜色」只出现在路线工具上（IColored 的唯一实现者是路线资产），
			// 旧文案沿用的「其它」说的是道路配色，属于另一个页。
			d["item.color.desc"] = "记住路线工具面板上「颜色」一行的线路颜色。";

			// ---------- 保留年代（面板行名 ToolOptions.DECORATION_MODE_TITLE = 保留年代） ----------
			d["item.decoration.label"] = "保留年代";
			// v0.9.0 文案
			d["item.decoration.desc"] = "记住「保留年代」开关：勾上后刷到的树不再成长、也不再老化倒下。\n这一行只在树上有。";
			d["item.i.anarchy.label"] = "Anarchy";
			d["item.brushHeight.label"] = "目标高度";
			// v0.9.0 文案
			d["item.brushHeight.desc"] = "记住「目标高度」这一行：地形要把地面削平或压成斜坡，最终停在多高。\n只有地面平整与斜坡这两种地形工具有这一行。";
			d["item.distance.label"] = "距离";
			// v0.9.0 文案
			d["item.distance.desc"] = "记住「距离」这一行：用直线或曲线摆放时，相邻两个之间隔多远。\n只有直线、曲线两种模式下才有这一行。";
			d["item.ageMask.label"] = "年龄";
			// v0.9.0 文案
			d["item.ageMask.desc"] = "记住「年龄」这一行勾了哪几档：幼苗、幼树、成树、老树，可以同时勾多档。\n只有树上有这一行。";

			// ---------- 功能页五项（f.*）：行名与资产页同名项完全一致，主语换成六类功能 ----------
			// 需求 4：工具模式、对齐在三个页上各一套，互相独立；等高线/地下模式同样三页各一份，
			// 按当前工具所在页读写；笔刷只出现在地形改造工具与刷涂类功能上。
			// v0.9.0 文案
			d["item.f.toolMode.desc"] = "记住功能工具面板上「工具模式」一行的选择：功能区为填充、滚动、刷涂；区域工具为编辑；拆除工具为主要部分、附属部分、全部。";
			// v0.9.0 文案
			d["item.f.snap.desc"] = "记住功能工具面板上「对齐」里各个开关的勾选状态，可选项随工具不同而不同。\n游戏自带的「开启／关闭所有对齐」是批量开关，本身不记忆。";
			// v0.9.0 文案
			d["item.f.topography.desc"] = "记住功能区、地形改造等工具面板上「地形」一行的开与关，也就是看不看得到地形轮廓线。";
			// v0.9.0 文案
			d["item.f.underground.desc"] = "记住区域、路线、拆除等工具面板上「地下模式」一行的开与关。";
			// v0.9.0 文案
			d["item.i.toolMode.desc"] = "记住道具面板上「工具模式」一行的选择：放置单个、放置多个、直线、曲线、目标印戳工具、刷涂等。";
			// v0.9.0 文案
			d["item.i.snap.desc"] = "记住道具面板上「对齐」里各个开关的勾选状态，可选项随道具不同而不同。";
			// v0.9.0 文案
			d["item.i.topography.desc"] = "记住道具面板上「地形」一行的开与关。";
			// v0.9.0 文案
			d["item.i.underground.desc"] = "记住道具面板上「地下模式」一行的开与关，也就是摆在地面以下还是地面之上。";
			d["scope.line.group"] = "同组：同一个菜单栏分组里的工具共用一份。{0}";
			d["scope.line.menu"] = "同菜单：整个菜单里的工具共用一份。{0}";
			d["scope.line.category"] = "同类：按资产服务的对象归成群组，同一群共用一份。{0}";
			d["scope.desc.function"] = "这一项的数值在多大范围内共用。";
			d["scope.desc.item"] = "这一项的数值在多大范围内共用。";
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
			d["group.about"] = "資訊與連結";

			d["enabled.label"] = "啟用模組";
			// v0.9.0 文案
			d["enabled.desc"] = "本模组的全部功能的开关。\n打开后下面三个分域开关才会生效，每个工具面板的设置都会实时记住，回到存档时自动恢复。\n关掉后模组完全不参与，所有工具都是游戏原本的样子。";
			d["domain.asset.label"] = "啟用建築與服務的工具項記憶";
			// v0.9.0 文案
			d["domain.asset.desc"] = "管这一页所有工具项是否记忆：\n打开时，你的设置会实时记住，回到存档时自动恢复；\n关掉时不记忆、也不会从存档恢复，共用范围自然也就无从设置。\n这一页收录道路、建筑、服务设施等可以建造的东西。";
			d["domain.function.label"] = "啟用功能的工具項記憶";
			// v0.9.0 文案
			d["domain.function.desc"] = "管这一页所有工具项是否记忆：\n打开时，你的设置会实时记住，回到存档时自动恢复；\n关掉时不记忆、也不会从存档恢复，共用范围自然也就无从设置。\n这一页收录功能区、区域、路线、地形改造等非资产类工具。";
			d["domain.item.label"] = "啟用道具的工具項記憶";
			// v0.9.0 文案
			d["domain.item.desc"] = "管这一页所有工具项是否记忆：\n打开时，你的设置会实时记住，回到存档时自动恢复；\n关掉时不记忆、也不会从存档恢复，共用范围自然也就无从设置。\n这一页收录城市美化菜单里的道具，树木也在这一页。";
			d["tab.mod"] = "工具模式記憶設定";
			d["tab.master"] = "總開關";
			d["tab.assets"] = "建築與服務";
			d["tab.functions"] = "功能";
			d["tab.items"] = "道具";
			d["group.domain"] = "分域開關";
			d["scope.label"] = "共用範圍";
			d["scope.desc"] = "這一項的數值在多大範圍內共用。括號裡標出遊戲原本的做法（原版）。";
			d[kScopeLineGroup] = "同組：{0}";
			d[kScopeLineMenu] = "同選單：{0}";
			d[kScopeLineCategory] = "同類：按本頁工具服務的對象歸成群組，同一群共用一份：{0}";
			d[kScopeLineShared] = "全域共用：本頁內所有支援這一項的建築與服務都共用一份";
			d[kScopeLineSharedSingle] = "全域共用：本頁內所有支援這一項的工具都共用一份";
			d[kScopeLineShared + ".function"] = "全域共用：本頁內所有支援這一項的功能工具都共用一份";
			d[kScopeLineShared + ".item"] = "全域共用：本頁內所有支援這一項的道具都共用一份";
			d[kScopeLineUnique] = "全部不共用：本頁內每一種建築與服務都各自獨立設定";
			d[kScopeLineUnique + ".function"] = "全部不共用：本頁內每一個功能工具都各自獨立設定";
			d[kScopeLineUnique + ".item"] = "全部不共用：本頁內每一種道具都各自獨立設定";
			d[kScopeNote] = "上面每一檔都只在當前這一頁裡生效。";

			d[kScopeGroup] = "同組";
			d[kScopeMenu] = "同選單";
			d[kScopeCategory] = "同類";
			d[kScopeCategory + ".function"] = "同類";
			d[kScopeCategory + ".item"] = "同類道具";
			d[kScopeGlobalShared] = "全域共用";
			d[kScopeGlobalUnique] = "全部不共用";
			d[kTagVanilla] = "（原版）";
			d[kTagRecommended] = "（推薦）";
			d[kTagRecVanilla] = "（推薦原版）";

			d["reset.label"] = "重設記憶";
			d["reset.desc"] = "遊戲中：清除目前存檔的記憶，工具恢復剛進檔時的原版狀態。主選單或編輯器：清除所有存檔的記憶。";
			d["reset.warn"] = "此操作無法復原。";
			d["reset.confirm"] = "確定要重設已記憶的工具設定嗎？";
			d["resetall.label"] = "重設所有設定項";
			d["resetall.desc"] = "把本模組的所有選項回到推薦預設值：總開關重新開啟，每一項的啟用狀態和共用範圍也一併恢復。已記錄的存檔記憶不受影響。";
			d["resetall.warn"] = "此操作無法復原。";
			d["resetall.confirm"] = "確定要重設所有設定項嗎？";
			d["folder.label"] = "管理所有存檔的記憶檔案";
			d["folder.desc"] = "開啟保存記憶的本機資料夾，每個存檔一份檔案，檔名就是「載入遊戲」裡看到的存檔名。模組讀取記憶都是按存檔名，存檔名稱和 json 檔名一致才會讀取，所以可以透過修改檔名的方式實現複製。開啟自動存檔時，記憶仍然寫進它原存檔名的那份檔案並覆蓋它（自動存檔的名稱每次都不一樣，不能當檔名）；只有認不出原存檔的自動存檔才暫按城市名保存，等你手動存檔後自動改成真正的存檔名。";

			d["about.version"] = "模組版本";
			d["about.author"] = "作者";
			d["about.kofi"] = "請我喝杯咖啡";
			d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
			d["about.forum"] = "論壇頁面";
			d["about.forum.desc"] = "開啟 Paradox 論壇貼文。";
			d["about.rainbow"] = "RAINBOW官網";
			d["about.rainbow.desc"] = "開啟 RAINBOW 系列官網。";

			// ---------- Anarchy ----------
			// v0.9.0 文案
			d["item.anarchy.desc"] = "记住 Anarchy 模组的开关。\n没安装 Anarchy 时这一项不会出现。";
			d["item.anarchy.ex.group"] = "例如給兩車道小型道路開啟 Anarchy，切換到其他小型道路也是開啟的，但切換到大型道路就不是了。";
			d["item.anarchy.ex.menu"] = "點選道路選單裡面的所有資產，Anarchy 都是開啟的，但切換到電力選單就不是了。";
			d["item.anarchy.ex.category"] = "切換到六車道道路同樣是開啟的，但切換到橋樑或雙線地鐵軌道就變成關閉（需單獨設定），即使它們都在道路選單下。";

			// 地區主題 / 資料包：行名照原版語言包（Toolbar.THEME_PANEL_TITLE = 主題、
			// Toolbar.ASSET_PACKS_PANEL_TITLE = 安裝包）。
			d["item.themes.label"] = "主題";
			// v0.9.0 文案
			d["item.themes.desc"] = "记住工具栏筛选面板里「地区主题」勾选了哪些。\n只有当前分类里确实有资产用到主题时才有这一行。";
			d["item.themes.ex.group"] = "例如只在道路/小型道路勾了某個主題，切換到大型道路就不按它篩選了。";
			d["item.themes.ex.menu"] = "在道路選單勾的主題，切換到電力選單就不生效，回到道路選單又還是那一組。";
			d["item.themes.ex.category"] = "這一行的勾選屬於工具欄面板本身，按當前所在的選單+分類記憶，不按資產類別共用。";
			d["item.packs.label"] = "安裝包";
			// v0.9.0 文案
			d["item.packs.desc"] = "记住工具栏筛选面板里「数据包」勾选了哪些。\n勾上后工具栏只列出属于这些数据包的资产。";
			d["item.packs.ex.group"] = "例如在道路/小型道路勾了某個安裝包，切到大型道路會被原版清空，回到小型道路又自動恢復。";
			d["item.packs.ex.menu"] = "在道路選單勾的安裝包，和電力選單勾的各記各的。";
			d["item.packs.ex.category"] = "這一行的勾選屬於工具欄面板本身，按當前所在的選單+分類記憶；原版每次換分類都會把它清空。";

			// ---------- 工具模式 ----------
			d["item.toolMode.label"] = "工具模式";
			// v0.9.0 文案
			d["item.toolMode.desc"] = "记住每个工具所选的模式。\n这一页的可选值就是直线、简单曲线、复杂曲线、连续曲线、网格、替换、节点，以及建筑的放置方式。";
			d["item.toolMode.ex.group"] = "例如給兩車道小型道路選了簡單曲線，切換到其他小型道路也是簡單曲線，但切換到大型道路就不是了。";
			d["item.toolMode.ex.menu"] = "點選道路選單裡面的所有資產，都是簡單曲線，但切換到電力選單就不是了。";
			d["item.toolMode.ex.category"] = "切換到六車道道路也是簡單曲線，但切換到橋樑或雙線地鐵軌道就回到預設模式（需單獨設定），即使它們都在道路選單下。";

			// ---------- 高度 ----------
			d["item.elevation.label"] = "高度";
			// v0.9.0 文案
			d["item.elevation.desc"] = "记住道路离开时的高度，包括抬高、降低之后的结果。\n「高度阶段」（一次抬高或降低多少米）跟着高度一起记，共用同一个范围。";
			d["item.elevation.ex.group"] = "例如點選兩車道道路把高度提高到10m，那麼切換其他小型道路，高度也是10m，但切換到大型道路就不是10m了。";
			d["item.elevation.ex.menu"] = "點選所有道路選單裡面的資產，高度都是10m，但切換到電力選單就不是10m了。";
			d["item.elevation.ex.category"] = "切換到六車道道路高度也是10m，但切換到橋樑或雙軌鐵路就變成0m（需單獨設定），即使它們都在道路選單下。";

			// ---------- 並行模式 ----------
			d["item.parallel.label"] = "並行模式";
			// v0.9.0 文案
			d["item.parallel.desc"] = "记住并列道路的开关与两条线之间的间距。\n原版一次并列就是两条；关掉只画一条。";
			d["item.parallel.ex.group"] = "例如給兩車道小型道路開啟並行模式並把並行偏移調到5米，切換到其他小型道路同樣是開啟、偏移也是5米，但切換到大型道路就不是了。";
			d["item.parallel.ex.menu"] = "點選道路選單裡面的所有資產，並行都是開啟、偏移都是5米，但切換到電力選單就不是了。";
			d["item.parallel.ex.category"] = "切換到六車道道路同樣是開啟、偏移也是5米，但切換到橋樑或地鐵軌道就變成關閉（需單獨設定），即使它們都在道路選單下。";

			// ---------- 吸附 ----------
			d["item.snap.label"] = "吸附";
			// v0.9.0 文案
			d["item.snap.desc"] = "记住「对齐」里各个开关的勾选状态。\n游戏自带的「开启／关闭所有对齐」是批量开关，本身不记忆。";
			d["item.snap.ex.group"] = "例如給兩車道小型道路關掉快速對齊路邊，切換到其他小型道路同樣是關著的，但切換到大型道路就是全開的預設狀態。";
			d["item.snap.ex.menu"] = "點選道路選單裡面的所有資產，吸附開關都是這一組，但切換到電力選單就不是了。";
			d["item.snap.ex.category"] = "切換到六車道道路仍保持這一組開關，但切換到橋樑或步行道就恢復預設（需單獨設定），即使它們都在道路選單下。";

			// ---------- 地形圖 ----------
			d["item.topography.label"] = "地形圖";
			// v0.9.0 文案
			d["item.topography.desc"] = "记住「地形」这一行，也就是显示轮廓线条的开与关。";
			d["item.topography.ex.group"] = "例如給兩車道小型道路勾選地形圖，切換到其他小型道路同樣是勾選的，但切換到大型道路就不是了。";
			d["item.topography.ex.menu"] = "點選道路選單裡面的所有資產，地形圖都是勾選的，但切換到電力選單就不是了。";
			d["item.topography.ex.category"] = "切換到六車道道路也是勾選的，但切換到橋樑或電纜就變成不勾（需單獨設定）。";


			// ---------- 左側和右側（Anarchy） ----------
			d["item.leftRight.label"] = "左側和右側";
			// v0.9.0 文案
			d["item.leftRight.desc"] = "记住 Anarchy 网络面板里「左侧」「右侧」两行所选的网络升级。\n没安装 Anarchy 时这一项不会出现。";
			d["item.leftRight.ex.group"] = "例如給兩車道小型道路的右側選上自行車道，切換到其他小型道路右側同樣是自行車道，但切換到大型道路就不是了。";
			d["item.leftRight.ex.menu"] = "點選道路選單裡面的所有資產，左右兩側都是這一組升級，但切換到電力選單就不是了。";
			d["item.leftRight.ex.category"] = "切換到六車道道路右側也是自行車道，但切換到橋樑或雙軌鐵路就變成未選擇（需單獨設定），即使它們都在道路選單下。";

			// ---------- 一般（Anarchy） ----------
			d["item.general.label"] = "一般";
			// v0.9.0 文案
			d["item.general.desc"] = "记住 Anarchy 网络面板里「常规」一行的开关。\n没安装 Anarchy 时这一项不会出现。";
			d["item.general.ex.group"] = "例如給兩車道小型道路勾選隧道，切換到其他小型道路同樣是隧道，但切換到大型道路就不是了。";
			d["item.general.ex.menu"] = "點選道路選單裡面的所有資產，一般選項都是這一組，但切換到電力選單就不是了。";
			d["item.general.ex.category"] = "切換到六車道道路也是隧道，但切換到橋樑或雙線地鐵軌道就全部回到未勾選（需單獨設定），即使它們都在道路選單下。";
			// ---------- 物件高度（v0.8.0：Anarchy 注入在擺件工具面板上的一整排） ----------
			// owner 2026-10-20 定：鎖定變換 / 高度 / 高度變化範圍 / 抬高降低的幅度 是同一排顯示的，
			// 算同一項、一起記憶一起共用；「重設高度為 0」是瞬時按鈕，不列。
			d["item.anarchyObjectElevation.label"] = "物件高度";
			// v0.9.0 文案
			d["item.anarchyObjectElevation.desc"] = "記住 Anarchy 在工具面板上加的那一排高度控制項：鎖定變換、高度、高度變化範圍、抬高降低的幅度。\n這四項在同一排顯示，所以一起記憶、一起共用。";

			// ---------- 地下模式 ----------
			d["item.underground.label"] = "地下模式";
			// v0.9.0 文案
			d["item.underground.desc"] = "记住「地下模式」开关，也就是你放的东西在地面以下还是地面之上。";
			d["item.underground.ex.group"] = "例如給兩車道小型道路開啟地下模式，切換到其他小型道路同樣在地下，但切換到大型道路就不是了。";
			d["item.underground.ex.menu"] = "點選道路選單裡面的所有資產，都在地下模式，但切換到電力選單就不是了。";
			d["item.underground.ex.category"] = "切換到六車道道路也在地下，但切換到橋樑或雙軌鐵路就回到地面（需單獨設定），即使它們都在道路選單下。";

			// ---------- 其他 ----------


			// ---------- 顏色（v0.7.0 從「其他」獨立；面板行名 ToolOptions.COLOR_TITLE = 顏色） ----------
			d["item.color.label"] = "顏色";

			// ---------- 保留年代（面板行名 ToolOptions.DECORATION_MODE_TITLE = 保留年代） ----------
			d["item.decoration.label"] = "保留年代";
			// v0.9.0 文案
			d["item.decoration.desc"] = "记住「保留年代」开关：勾上后刷到的树不再成长、也不再老化倒下。\n这一行只在树上有。";
			d["item.i.anarchy.label"] = "Anarchy";
			d["item.brushType.label"] = "筆刷種類";
			// v0.9.0 文案
			d["item.brushType.desc"] = "记住「笔刷种类」这一行选的刷子：圆形、方形、柔边等。\n只有能刷涂的工具才有这一行。";
			d["item.brushSize.label"] = "筆刷大小";
			// v0.9.0 文案
			d["item.brushSize.desc"] = "记住「笔刷粗细」这一行：一次刷过多大范围。\n只有能刷涂的工具才有这一行。";
			d["item.brushStrength.label"] = "筆刷硬度";
			// v0.9.0 文案
			d["item.brushStrength.desc"] = "记住「笔刷强度」这一行：每一下刷动改变多少。\n只有能刷涂的工具才有这一行。";
			d["item.brushHeight.label"] = "目標高度";
			// v0.9.0 文案
			d["item.brushHeight.desc"] = "记住「目标高度」这一行：地形要把地面削平或压成斜坡，最终停在多高。\n只有地面平整与斜坡这两种地形工具有这一行。";
			d["item.distance.label"] = "距離";
			// v0.9.0 文案
			d["item.distance.desc"] = "记住「距离」这一行：用直线或曲线摆放时，相邻两个之间隔多远。\n只有直线、曲线两种模式下才有这一行。";
			d["item.ageMask.label"] = "年齡";
			// v0.9.0 文案
			d["item.ageMask.desc"] = "记住「年龄」这一行勾了哪几档：幼苗、幼树、成树、老树，可以同时勾多档。\n只有树上有这一行。";
			d["item.anarchyObjectElevation.label"] = "物件高度";
			// v0.9.0 文案
			d["item.anarchyObjectElevation.desc"] = "记住 Anarchy 在工具面板上加的那一排高度控件：锁定变换、高度、高度变化范围、抬高降低的幅度。\n这四项在同一排显示，所以一起记忆、一起共用。";

			// ---------- 功能頁五項（f.*）----------
			// v0.9.0 文案
			d["item.f.toolMode.desc"] = "記住功能工具面板上「工具模式」一行的選擇：功能區為填充、選取方格、塗上；區域工具為編輯；拆除工具為主元素、子元素、全部。";
			// v0.9.0 文案
			d["item.f.snap.desc"] = "記住功能工具面板上「吸附」裡各個開關的勾選狀態，可選項隨工具不同而不同。\n遊戲自帶的「開啟／關閉所有吸附」是批次開關，本身不記憶。";
			// v0.9.0 文案
			d["item.f.topography.desc"] = "記住功能區、地形改造等工具面板上「地形圖」一行的開與關，也就是看不看得到地形輪廓線。";
			// v0.9.0 文案
			d["item.f.underground.desc"] = "記住區域、路線、拆除等工具面板上「地下模式」一行的開與關。";
			// v0.9.0 文案
			d["item.i.toolMode.desc"] = "Remembers the mode chosen on prop panels: place single, place multiple, line, curve, object stamp tool, paint and so on.";
			// v0.9.0 文案
			d["item.i.snap.desc"] = "Remembers which snapping toggles are ticked on prop panels; the choices differ from prop to prop.";
			// v0.9.0 文案
			d["item.i.topography.desc"] = "記住道具面板上「地形圖」一行的開與關。";
			// v0.9.0 文案
			d["item.i.underground.desc"] = "Remembers the Underground Mode toggle on prop panels: whether props are placed below or above ground.";
			d["scope.line.group"] = "同組：同一個選單列分組裡的工具共用一份。{0}";
			d["scope.line.menu"] = "同選單：整個選單裡的工具共用一份。{0}";
			d["scope.line.category"] = "同類：按資產服務的對象歸成群組，同一群共用一份。{0}";
			d["ex.net.group"] = "比如小路這一組裡的各種道路共用一份；換到大路就是另一組了。";
			d["ex.net.menu"] = "比如道路選單裡的所有道路共用一份；換到電力選單就不是了。";
			d["ex.net.category"] = "比如各車道數的道路都算車行道路，共用一份；橋樑、雙線鐵路各算一類。";
			d["ex.zone.group"] = "比如住宅功能區裡的低密度、中密度、高密度住宅算一組，共用一份；換到商業功能區就是另一組（兩個區都在同一個選單下）。";
			d["ex.zone.menu"] = "比如住宅功能區、商業功能區、工業功能區、辦公功能區都在功能區選單下，共用一份；換到別的選單就不是了。";
			d["ex.route.group"] = "比如路線按交通工具分組（道路、有軌電車、火車、捷運等），同一組的路線共用一份；換到另一種交通工具的路線就是另一組。";
			d["ex.route.menu"] = "比如運輸選單裡的所有路線共用一份；換到別的選單就不是了。";
			d["ex.terrain.group"] = "地形改造只有一個分組，所以同組和同選單選出來的是同一批工具。";
			d["ex.terrain.menu"] = "地形改造只有一個分組，所以同選單和同組選出來的是同一批工具。";
			d["ex.prop.group"] = "比如景觀美化選單下分成自然、公園、住宅道具、商業道具、工業道具、殘骸、貼花、盆栽幾類，同一類的道具共用一份；換到另一類就是另一組。";
			d["ex.prop.menu"] = "景觀美化選單下的所有道具和植被共用一份。";
			d["ex.tree.group"] = "樹都在景觀美化選單的植被分組裡，所以同組和同選單選出來的是同一批樹。";
			d["ex.tree.menu"] = "樹都在景觀美化選單的植被分組裡，所以同選單和同組選出來的是同一批樹。";
			d["item.color.desc"] = "記住路線工具面板上「顏色」一行的路線顏色。";
			d["scope.desc.function"] = "這一項的數值在多大範圍內共用。";
			d["scope.desc.item"] = "這一項的數值在多大範圍內共用。";
			return d;
		}

		// ======================= en-US =======================

		private static Dictionary<string, string> En()
		{
			Dictionary<string, string> d = Frames("en-US");
			d["mod.name"] = "TOOL MODE MEMORY";
			// v0.7.0 requirement 3: five tabs (master switch / assets / functions / items / about).
			d["tab.mod"] = "Tool Mode Memory Settings";
			d["tab.master"] = "Master Switch";
			d["tab.assets"] = "Buildings & Services";
			d["tab.functions"] = "Functions";
			d["tab.items"] = "Items";
			d["tab.about"] = "About";
			d["group.master"] = "Tool Mode Memory";
			d["group.domain"] = "Per-page switches";
			d["group.official"] = "Official Tool Settings";
			d["group.anarchy"] = "Anarchy Tool Settings";
			d["group.reset"] = "Memory management";
			d["group.about"] = "Information and links";

			d["enabled.label"] = "Enable Mod";
			// v0.9.0 文案
			d["enabled.desc"] = "Switches the whole mod on or off.\nWhile it is on, the three switches below take effect, every tool panel setting is remembered as you work and put back when you load a save.\nWhile it is off the mod stays out of the way and every tool behaves as it normally does.";
			// v0.8.0 requirement 3: "Assets" renamed to "Buildings & Services" (props are assets too,
			// so the old page name was ambiguous). Requirement 5: the taxonomy now lives in the
			// per-page switch description, not in the sharing-scope lines below.
			d["domain.asset.label"] = "Remember Buildings & Services tool items";
			// v0.9.0 文案
			d["domain.asset.desc"] = "Controls whether the items on this page are remembered:\non - your settings are recorded as you work and restored when you load a save;\noff - nothing is recorded or restored, so there is no sharing scope to configure.\nThis page holds roads, buildings and city services.";
			d["domain.function.label"] = "Remember Function tool items";
			// v0.9.0 文案
			d["domain.function.desc"] = "Controls whether the items on this page are remembered:\non - your settings are recorded as you work and restored when you load a save;\noff - nothing is recorded or restored, so there is no sharing scope to configure.\nThis page holds zones, areas, routes and terrain tools.";
			d["domain.item.label"] = "Remember Prop tool items";
			// v0.9.0 文案
			d["domain.item.desc"] = "Controls whether the items on this page are remembered:\non - your settings are recorded as you work and restored when you load a save;\noff - nothing is recorded or restored, so there is no sharing scope to configure.\nThis page holds the props in the Landscaping menu, trees included.";
			d["scope.label"] = "Sharing scope";
			d["scope.desc"] = "How widely this item's value is shared. The brackets show what the game does by default (vanilla).";
			d[kScopeLineGroup] = "Same group: {0}";
			d[kScopeLineMenu] = "Same menu: {0}";
			d[kScopeLineCategory] = "Same type: the tools on this page are put into groups by what each one actually serves, and one group shares one value: {0}";
			d[kScopeLineShared] = "Shared globally: every building or service on this page that supports this item uses one value";
			d[kScopeLineSharedSingle] = "Shared globally: every tool on this page that supports this item uses one value";
			// v0.8.0 requirement 4: the sharing lines name the page you are on and nothing else.
			// Function/Item pages look up their own key; missing ones fall back to the line above.
			d[kScopeLineShared + ".function"] = "Shared globally: every function tool on this page that supports this item uses one value";
			d[kScopeLineShared + ".item"] = "Shared globally: every prop on this page that supports this item uses one value";
			d[kScopeLineUnique] = "Not shared at all: every building or service on this page is set independently";
			d[kScopeLineUnique + ".function"] = "Not shared at all: every function tool on this page is set independently";
			d[kScopeLineUnique + ".item"] = "Not shared at all: every prop on this page is set independently";
			d[kScopeNote] = "Note: every level above only applies inside this page.";

			d[kScopeGroup] = "Same group";
			d[kScopeMenu] = "Same menu";
			d[kScopeCategory] = "Same type";
			d[kScopeCategory + ".function"] = "Same type";
			d[kScopeCategory + ".item"] = "Same prop type";
			d[kScopeGlobalShared] = "Shared globally";
			d[kScopeGlobalUnique] = "Not shared at all";
			d[kTagVanilla] = " (vanilla)";
			d[kTagRecommended] = " (recommended)";
			d[kTagRecVanilla] = " (recommended, vanilla)";

			d["reset.label"] = "Reset memory";
			d["reset.desc"] = "In a save: wipe this save's memory and put the tools back to the vanilla state they had on load. In the main menu or the editor: wipe the memory of every save.";
			d["reset.warn"] = "This cannot be undone.";
			d["reset.confirm"] = "Reset all remembered tool settings?";
			d["resetall.label"] = "Reset all settings";
			d["resetall.desc"] = "Puts every option of this mod back to its recommended default: the master switch goes back on, and each item's on/off state and sharing scope are restored. Memory already recorded for saves is left alone.";
			d["resetall.warn"] = "This cannot be undone.";
			d["resetall.confirm"] = "Reset every setting of this mod to its recommended default?";
			d["folder.label"] = "Manage memory files for all saves";
			d["folder.desc"] = "Opens the local folder that holds the memory files, one per save, named exactly like the save shown in the Load Game list. Memory is always read by save name: a file is only used when the save name and the json file name match, so renaming the file is how you copy memory from one save to another. When you load an auto save its memory still goes to the original save's file and overwrites it, because an auto save gets a new name every time; only an auto save whose original save cannot be traced falls back to the city name, and it is renamed as soon as you save manually.";

			d["about.version"] = "Mod version";
			d["about.author"] = "Author";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Support the author on Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Open the Paradox forum thread.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Open the Rainbow Series site.";

			// ---------- Anarchy ----------
			// v0.9.0 文案
			d["item.anarchy.desc"] = "Remembers the Anarchy mod's toggle.\nThis item does not appear unless Anarchy is installed.";
			d["item.anarchy.ex.group"] = "for example, turn Anarchy on for a two-lane small road and the other small roads keep it on, while a large road does not.";
			d["item.anarchy.ex.menu"] = "every asset you click in the Roads menu has Anarchy on, but moving to the Electricity menu it is not.";
			d["item.anarchy.ex.category"] = "switching to a six-lane road keeps it on, but a bridge or a double subway track turns it off and needs its own setting, even though they are all in the Roads menu.";

			// Row names follow the game's own language pack:
			// Toolbar.THEME_PANEL_TITLE = "Theme", Toolbar.ASSET_PACKS_PANEL_TITLE = "Pack".
			d["item.themes.label"] = "Theme";
			// v0.9.0 文案
			d["item.themes.desc"] = "Remembers which entries are ticked in the Theme filter.\nThat row only appears when the current category really has assets that use themes.";
			d["item.themes.ex.group"] = "for example you tick a theme only under Roads/Small Roads, so large roads don't filter by it.";
			d["item.themes.ex.menu"] = "a theme ticked in the Roads menu stops applying in the Electricity menu and is back when you return.";
			d["item.themes.ex.category"] = "this row is the filter panel's own state, so it is filed under the menu and group you are in rather than by asset type.";
			d["item.packs.label"] = "Pack";
			// v0.9.0 文案
			d["item.packs.desc"] = "Remembers which entries are ticked in the Pack filter.\nTicking one limits the toolbar list to assets in those packs.";
			d["item.packs.ex.group"] = "for example you tick a pack under Roads/Small Roads, vanilla clears it on large roads, and it comes back when you return.";
			d["item.packs.ex.menu"] = "packs ticked in the Roads menu and in the Electricity menu are remembered separately.";
			d["item.packs.ex.category"] = "this row is the filter panel's own state, filed under the menu and group you are in; vanilla clears it every time the group changes.";

			// ---------- Tool Mode ----------
			d["item.toolMode.label"] = "Tool Mode";
			// v0.9.0 文案
			d["item.toolMode.desc"] = "Remembers the mode each tool is set to.\nOn this page that means straight, simple curve, complex curve, continuous curve, grid, replace and node, plus how buildings are placed.";
			d["item.toolMode.ex.group"] = "for example, pick Simple Curve for a two-lane small road and the other small roads use Simple Curve too, while a large road does not.";
			d["item.toolMode.ex.menu"] = "every asset you click in the Roads menu is on Simple Curve, but moving to the Electricity menu it is not.";
			d["item.toolMode.ex.category"] = "switching to a six-lane road still gives Simple Curve, but a bridge or a double subway track falls back to its default mode and needs its own setting, even though they are all in the Roads menu.";

			// ---------- Elevation ----------
			d["item.elevation.label"] = "Elevation";
			// v0.9.0 文案
			d["item.elevation.desc"] = "Remembers the height the road leaves at, including the result of raising or lowering it.\nThe height step (how many metres one raise or lower moves) is remembered with it and shares the same scope.";
			d["item.elevation.ex.group"] = "for example, raise a two-lane road to 10 m and the other small roads go to 10 m too, while a large road is not at 10 m.";
			d["item.elevation.ex.menu"] = "every asset you click in the Roads menu is at 10 m, but moving to the Electricity menu it is not at 10 m.";
			d["item.elevation.ex.category"] = "switching to a six-lane road is still 10 m, but a bridge or a double train track goes back to 0 m and needs its own setting, even though they are all in the Roads menu.";

			// ---------- Parallel Mode ----------
			d["item.parallel.label"] = "Parallel Mode";
			// v0.9.0 文案
			d["item.parallel.desc"] = "Remembers the parallel road toggle and the gap between the two lines.\nVanilla always draws two lines side by side; turn it off and you get one.";
			d["item.parallel.ex.group"] = "for example, turn on Parallel Mode for a two-lane small road and set the offset to 5 m, the other small roads come back switched on with the same 5 m, while a large road does not.";
			d["item.parallel.ex.menu"] = "every asset you click in the Roads menu is parallel with a 5 m offset, but moving to the Electricity menu it is not.";
			d["item.parallel.ex.category"] = "switching to a six-lane road is still on with the same 5 m offset, but a bridge or a subway track switches it off and needs its own setting, even though they are all in the Roads menu.";

			// ---------- Snapping ----------
			d["item.snap.label"] = "Snapping";
			// v0.9.0 文案
			d["item.snap.desc"] = "Remembers which snapping toggles are ticked.\nThe game's own enable/disable-all-snapping button is a batch toggle and is not remembered itself.";
			d["item.snap.ex.group"] = "for example, switch off Snap to the sides of a road for a two-lane small road and the other small roads have it off too, while a large road still has everything on.";
			d["item.snap.ex.menu"] = "every asset you click in the Roads menu uses this same set of snapping switches, but moving to the Electricity menu it does not.";
			d["item.snap.ex.category"] = "switching to a six-lane road keeps the same set, but a bridge or a pathway returns to the defaults and needs its own setting, even though they are all in the Roads menu.";

			// ---------- Topography ----------
			d["item.topography.label"] = "Topography";
			// v0.9.0 文案
			d["item.topography.desc"] = "Remembers the Contour Lines row: whether contour lines are shown.";
			d["item.topography.ex.group"] = "for example, turn Topography on for a two-lane small road and the other small roads have it on too, while a large road does not.";
			d["item.topography.ex.menu"] = "every asset you click in the Roads menu shows contour lines, but moving to the Electricity menu it does not.";
			d["item.topography.ex.category"] = "switching to a six-lane road still shows them, but a bridge or an electric cable has the row off and needs its own setting.";


			// ---------- Left / Right (Anarchy) ----------
			d["item.leftRight.label"] = "Left and Right";
			// v0.9.0 文案
			d["item.leftRight.desc"] = "Remembers the upgrades chosen in Anarchy's Left and Right rows.\nThis item does not appear unless Anarchy is installed.";
			d["item.leftRight.ex.group"] = "for example, pick Bike Lane on the right side of a two-lane small road and the other small roads get Bike Lane there too, while a large road does not.";
			d["item.leftRight.ex.menu"] = "every asset you click in the Roads menu uses this same pair of side selections, but moving to the Electricity menu it does not.";
			d["item.leftRight.ex.category"] = "switching to a six-lane road still has Bike Lane on the right, but a bridge or a double train track goes back to nothing picked and needs its own setting, even though they are all in the Roads menu.";

			// ---------- General (Anarchy) ----------
			d["item.general.label"] = "General";
			// v0.9.0 文案
			d["item.general.desc"] = "Remembers the toggles in Anarchy's General row.\nThis item does not appear unless Anarchy is installed.";
			d["item.general.ex.group"] = "for example, pick Tunnel for a two-lane small road and the other small roads are forced into tunnels too, while a large road is not.";
			d["item.general.ex.menu"] = "every asset you click in the Roads menu uses this same set of General switches, but moving to the Electricity menu it does not.";
			d["item.general.ex.category"] = "switching to a six-lane road still gives Tunnel, but a bridge or a double subway track goes back to nothing ticked and needs its own setting, even though they are all in the Roads menu.";
			// ---------- Object elevation (v0.8.0: the row Anarchy injects into the object tool panel) ----------
			// The owner decided on 2026-10-20 that transform lock / elevation / elevation variance /
			// raise-lower step are one row on screen, so they are one item, remembered and shared together.
			// "Reset elevation to 0" is an instantaneous button, so it is not listed.
			d["item.anarchyObjectElevation.label"] = "Object Elevation";
			// v0.9.0 文案
			d["item.anarchyObjectElevation.desc"] = "Remembers the row of height controls Anarchy adds to tool panels: transform lock, elevation, elevation variance and the raise/lower step.\nThey sit on one row, so they are remembered and shared together.";

			// ---------- Underground Mode ----------
			d["item.underground.label"] = "Underground Mode";
			// v0.9.0 文案
			d["item.underground.desc"] = "Remembers the Underground Mode toggle: whether what you place goes below or above ground.";
			d["item.underground.ex.group"] = "for example, turn on Underground Mode for a two-lane small road and the other small roads go underground too, while a large road does not.";
			d["item.underground.ex.menu"] = "every asset you click in the Roads menu is built underground, but moving to the Electricity menu it is not.";
			d["item.underground.ex.category"] = "switching to a six-lane road still goes underground, but a bridge or a double train track stays on the surface and needs its own setting, even though they are all in the Roads menu.";

			// ---------- Other ----------


			// ---------- Colour (v0.7.0: split out of "Other"; panel row = ToolOptions.COLOR_TITLE) ----------
			d["item.color.label"] = "Color";

			// ---------- Preserve Age (panel row = ToolOptions.DECORATION_MODE_TITLE) ----------
			d["item.decoration.label"] = "Preserve Age";
			// v0.9.0 文案
			d["item.decoration.desc"] = "Remembers the Preserve Age toggle: trees you paint stop growing and no longer die of old age.\nOnly trees have this row.";
			d["item.i.anarchy.label"] = "Anarchy";
			d["item.brushType.label"] = "Brush Type";
			// v0.9.0 文案
			d["item.brushType.desc"] = "Remembers the brush chosen in the Brush Type row: circle, square, soft edge and so on.\nOnly tools that can paint have this row.";
			d["item.brushSize.label"] = "Brush Size";
			// v0.9.0 文案
			d["item.brushSize.desc"] = "Remembers the Brush Size row: how wide an area one stroke covers.\nOnly tools that can paint have this row.";
			d["item.brushStrength.label"] = "Brush Strength";
			// v0.9.0 文案
			d["item.brushStrength.desc"] = "Remembers the Brush Strength row: how much one stroke changes.\nOnly tools that can paint have this row.";
			d["item.brushHeight.label"] = "Target Height";
			// v0.9.0 文案
			d["item.brushHeight.desc"] = "Remembers the Target Height row: how high the ground ends up after levelling or sloping it.\nOnly the level and slope terrain tools have this row.";
			d["item.distance.label"] = "Distance";
			// v0.9.0 文案
			d["item.distance.desc"] = "Remembers the Distance row: how far apart consecutive objects are when you place them along a line or a curve.\nOnly the line and curve modes have this row.";
			d["item.ageMask.label"] = "Age";
			// v0.9.0 文案
			d["item.ageMask.desc"] = "Remembers which age stages are ticked: sapling, young, mature and old, several at once if you like.\nOnly trees have this row.";
			d["item.anarchyObjectElevation.label"] = "Object Elevation";
			// v0.9.0 文案
			d["item.anarchyObjectElevation.desc"] = "Remembers the row of height controls Anarchy adds to the object tool panel: transform lock, elevation, elevation variance and the raise/lower step.\nThey sit on one row, so they are remembered and shared together.";

			// ---------- Functions page (f.*): same panel rows, but grouped by the six function types ----------
			// v0.9.0 文案
			d["item.f.toolMode.desc"] = "Remembers the Tool Mode row on function tools: Fill, Marquee and Paint for zone tools; Edit for area tools; Main Element, Sub Element and Everything for the bulldoze tool.";
			d["item.f.snap.desc"] = "Remembers every switch in the Snapping row on function tools; the available switches depend on the tool.\nThe game's own enable/disable all snapping button is a batch switch and is not remembered.";
			d["item.f.topography.desc"] = "Remembers the Topography row on zone, terraforming and other function tools: whether terrain contour lines are visible.";
			d["item.f.underground.desc"] = "Remembers the Underground Mode row on area, route, bulldoze and object tools.";
			d["item.i.toolMode.desc"] = "Remembers the Tool Mode row on prop panels: Place single, Place multiple, Line, Curve, Object stamp tool, Paint and so on, depending on the tool. The tool mode on the Assets page and on the Functions page is remembered separately and none of them affects the others.";
			d["item.i.snap.desc"] = "Remembers every switch in the Snap row on prop panels: quick align to building surface, quick align to nearby shapes, quick align to road, remove matching types only and so on, depending on the prop. The snap row on the Assets page and on the Functions page is remembered separately and none of them affects the others.";
			d["item.i.topography.desc"] = "Remembers the Topography row on prop panels.";
			d["item.i.underground.desc"] = "Remembers the Underground Mode row on prop panels, that is whether you place props below or above the ground. This row is read and written per page: the Items page keeps the prop copy, the Assets and Functions pages each keep their own, and none of them is shared.";
			d["scope.line.group"] = "Same group: tools in one group of the menu bar share one value. {0}";
			d["scope.line.menu"] = "Same menu: every tool in one menu shares one value. {0}";
			d["scope.line.category"] = "Same type: assets are grouped by what they serve, and one group shares one value. {0}";
			d["ex.net.group"] = "For example, all roads in the Small Roads group share one value; Large Roads is another group.";
			d["ex.net.menu"] = "For example, every road in the Roads menu shares one value; the Electricity menu does not.";
			d["ex.net.category"] = "For example, roads of any lane count are one road type; bridges and railway tracks are types of their own.";
			d["ex.zone.group"] = "For example, low-, medium- and high-density housing in Residential Zones form one group; Commercial Zones is another group, although both are in the same menu.";
			d["ex.zone.menu"] = "For example, Residential Zones, Commercial Zones, Industrial Zones and Office Zones all share one value; another menu does not.";
			d["ex.route.group"] = "For example, lines are grouped by mode of transport (road, tram, train, subway and so on); a line of another mode is another group.";
			d["ex.route.menu"] = "For example, every line in the Transportation menu shares one value; another menu does not.";
			d["ex.terrain.group"] = "Terraforming has only one group, so Same group and Same menu pick exactly the same tools.";
			d["ex.terrain.menu"] = "Terraforming has only one group, so Same menu and Same group pick exactly the same tools.";
			d["ex.prop.group"] = "For example, the Landscaping menu splits props into Nature, Park, residential, commercial, industrial, debris, decals and planter categories; one category shares one value, another category is another group.";
			d["ex.prop.menu"] = "Every prop and tree in the Landscaping menu shares one value.";
			d["ex.tree.group"] = "All trees sit in the Vegetation group of the Landscaping menu, so Same group and Same menu pick exactly the same trees.";
			d["ex.tree.menu"] = "All trees sit in the Vegetation group of the Landscaping menu, so Same menu and Same group pick exactly the same trees.";
			d["item.color.desc"] = "Remembers the Color row on route tools: the color a line is drawn in.";
			d["scope.desc.function"] = "How widely this item's value is shared.";
			d["scope.desc.item"] = "How widely this item's value is shared.";
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
