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
				Item(2, CategoryLabelKey(def), def, d),
				Item(3, kScopeGlobalShared, def, d),
				Item(4, kScopeGlobalUnique, def, d),
			};
		}

		/// <summary>
		/// 「同类」这一档的显示名。owner 需求 2：把「同类资产」统一改叫「同类」，
		/// **但资产与道具两页保留带宾语的写法** —— 那两页说的确实是「同一类资产」
		/// 「同一类道具」，只写「同类」反而看不出同类的是什么。
		/// 所以：资产页沿用已有的 scope.category（同类资产），功能页查 scope.category.function
		/// （同类），道具页查 scope.category.item（同类道具）；缺句由 FillGaps 回落 en-US。
		/// </summary>
		private static string CategoryLabelKey(ToolItemDef def)
		{
			if (def == null) return kScopeCategory;
			switch (def.Domain)
			{
				case ItemDomain.Function: return kScopeCategory + ".function";
				case ItemDomain.Item: return kScopeCategory + ".item";
				default: return kScopeCategory;
			}
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
		/// 需求 4：五种共用范围的定义写进**本项**共用范围行的说明文本，例子按本项的数值来举。
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

		/// <summary>例子同理走 ItemText 的三级回退；找不到返回空串（宁可少一句例子）。</summary>
		private static string Example(Dictionary<string, string> d, ToolItemDef def, string slot)
		{
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
		/// 例子（{0}）仍按每一项自己的数值来举，见 item.&lt;id&gt;.ex.*。
		///
		/// 说明里的「五行」在三个页上都用**同一句**定义：三页只是各自挑了面板上哪几行来记，
		/// 五档共用范围讲的是「在多大范围内共用」，与在哪一页无关。
		/// 只有例子必须按页现写：道路的对齐、功能区的对齐、道具的对齐可选项不一样。
		/// </summary>
		private static string ScopeDescription(ToolItemDef def, Dictionary<string, string> d)
		{
			string categoryKey = def.Domain == ItemDomain.Asset
				? kScopeLineCategory
				: kScopeLineCategory + "." + DomainSuffix(def.Domain);
			StringBuilder sb = new StringBuilder();
			sb.Append(Val(d, "scope.desc"));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineGroup, Example(d, def, "group")));
			sb.Append('\n');
			sb.Append(Line(d, kScopeLineMenu, Example(d, def, "menu")));
			sb.Append('\n');
			sb.Append(Line(d, categoryKey, Example(d, def, "category")));
			sb.Append('\n');
			sb.Append(Val(d, def.FamilyFreeScope ? kScopeLineSharedSingle : kScopeLineShared));
			sb.Append('\n');
			sb.Append(Val(d, kScopeLineUnique));
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
			if (string.IsNullOrEmpty(example)) return template;
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
		// 权威语言：五行共用范围定义沿用用户给的原话，例子按每一项自己的数值改写，
		// 不经过英文回译。其它语言由这份定稿翻译。

		private static Dictionary<string, string> ZhHans()
		{
			Dictionary<string, string> d = Frames("zh-HANS");
			d["mod.name"] = "工具模式区分记忆";
			// v0.7.0 需求 3：五个标签页（总开关 / 资产 / 功能 / 道具 / 关于）。
			d["tab.mod"] = "工具模式记忆设置";
			d["tab.master"] = "总开关";
			d["tab.assets"] = "资产";
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
			d["enabled.label"] = "启用模组";
			d["enabled.desc"] = "默认打开。开着时下面所有设置都生效，所有工具项的数值都会实时记录，回到存档时就是你上次离开时的样子；单独关掉某一项只是不再恢复那一项，它的数值仍在记录。关掉这个大开关则一切回到原版：停止记录、不再恢复，资产、功能、道具三页里的所有设置同时失效（灰掉），每个工具都按原版自己的规矩走。下面三个分域开关全关时这里自动跟着关；三个里任何一个打开，这里自动打开。";
			d["domain.asset.label"] = "启用资产的工具项记忆";
			d["domain.asset.desc"] = "管「资产」页里所有工具项：记录与读档恢复一起开关。关掉之后资产页的设置全部灰掉不生效，工具回到原版行为，已记录的数据仍保留在文件里，重新打开照旧恢复。";
			d["domain.function.label"] = "启用功能的工具项记忆";
			d["domain.function.desc"] = "管「功能」页里所有工具项：功能区、区域工具、路线工具、地形改造工具、空间与区域工具、标记与预制对象这六类。关掉之后功能页的设置全部灰掉不生效，工具回到原版行为，已记录的数据仍保留在文件里，重新打开照旧恢复。";
			d["domain.item.label"] = "启用道具的工具项记忆";
			d["domain.item.desc"] = "管「道具」页里所有工具项：城市美化菜单里的八类道具（自然、公园、住宅道具、商业道具、工业道具、残骸、贴花、盆栽）。关掉之后道具页的设置全部灰掉不生效，工具回到原版行为，已记录的数据仍保留在文件里，重新打开照旧恢复。";


			d["scope.label"] = "共用范围";
			d["scope.desc"] = "这一项的数值在多大范围内共用，下拉框括号里标出原版行为与推荐选择。";
			d[kScopeLineGroup] = "同组：{0}";
			d[kScopeLineMenu] = "同菜单：{0}";
			d[kScopeLineCategory] = "同类资产：按资产实际服务谁归类；车道数、宽窄、级别、大小与工具栏里的分类都不算区别（小型停车场与大型停车场就同类），而桥梁、埠头、交叉路口各自单独成类：{0}";
			// 需求 2：功能页与道具页的「同类资产」改叫「同类」/「同类道具」，归类口径也跟着换。
			d[kScopeLineCategory + ".function"] = "同类：按功能所属的六个分类归群——功能区（住宅、商业、工业、办公）、区域工具（市辖区创建工具、专门产业区工具、表面区域规划工具）、路线工具（交通运输菜单里的全部路线）、地形改造工具（海拔调整、地形平滑、地面平整、斜坡生成、纹理绘制 01～04、纹理橡皮擦共九种）、空间与区域工具（公园区域、徒步区域、散步区域、清空区域、剪裁地面）、标记与预制对象（动物生成工具等）：{0}";
			d[kScopeLineCategory + ".item"] = "同类道具：按城市美化菜单的八类道具归群（自然、公园、住宅道具、商业道具、工业道具、残骸、贴花、盆栽）；同一类里模型不同、大小不同都不算区别：{0}";
			d[kScopeLineShared] = "全局共用：任何支持本项工具的资产/功能全部共用，但资产和功能之间不共用";
			d[kScopeLineSharedSingle] = "全局共用：所有支持本项的工具共用一份";
			d[kScopeLineUnique] = "全部不共用：所有支持本项工具的资产/功能完全独立设置";
			d[kScopeNote] = "注意，资产、功能、道具三页各记各的，即使同名也互不相通；全局共用与全部不共用同样只在当前这一页内成立。";

			d[kScopeGroup] = "同组";
			d[kScopeMenu] = "同菜单";
			d[kScopeCategory] = "同类资产";
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
			d["item.anarchy.desc"] = "记忆 Anarchy 模组的开关，Anarchy 模组是全局共用，开启后可以修改共用范围。没安装 Anarchy 模组时这一项不会出现。";
			d["item.anarchy.ex.group"] = "比如给两车道小型道路打开 Anarchy，切换到其它的小型道路，也是打开的，但切换到大型道路就不是了";
			d["item.anarchy.ex.menu"] = "点击道路菜单里面的所有资产，Anarchy 都是打开的，但切换到电力菜单就不是了";
			d["item.anarchy.ex.category"] = "切换到六车道道路也是打开的，但切换到桥梁或双向地铁轨道就变成关闭（需单独设置），即使它们都在道路菜单下";

			// 地区主题 / 数据包：工具栏筛选面板那两行，行名照原版语言包
			// （Toolbar.THEME_PANEL_TITLE / Toolbar.ASSET_PACKS_PANEL_TITLE）。
			d["item.themes.label"] = "地区主题";
			d["item.themes.desc"] = "记忆工具栏筛选面板里「地区主题」勾选了哪些。只有当前分类里确实有资产用到主题时才会出现这一行，勾上之后工具栏只列出支持这些主题的资产。原版只在读档时把它重置回默认主题，所以这一项不推荐开。";
			d["item.themes.ex.group"] = "比如只在道路/小型道路里勾了某个主题，切换到大型道路就不按它筛选了";
			d["item.themes.ex.menu"] = "在道路菜单里勾的主题，切换到电力菜单就不生效，回到道路菜单又还是那一套";
			d["item.themes.ex.category"] = "这一行的勾选属于工具栏面板本身，按当前所在的菜单+分类记忆，不按资产类别共用";
			d["item.packs.label"] = "数据包";
			d["item.packs.desc"] = "记忆工具栏筛选面板里「数据包」勾选了哪些，勾上之后工具栏只列出属于这些数据包的资产。原版每次切换菜单或分类都会把它清空，所以按「同一菜单+分类」来记最贴近你看到的规律，这一项也不推荐开。";
			d["item.packs.ex.group"] = "比如在道路/小型道路勾了某个数据包，切到大型道路会被原版清空，回到小型道路又自动恢复";
			d["item.packs.ex.menu"] = "在道路菜单勾的数据包，和电力菜单勾的各记各的";
			d["item.packs.ex.category"] = "这一行的勾选属于工具栏面板本身，按当前所在的菜单+分类记忆；原版每次换分类都会把它清空";

			// ---------- 工具模式 ----------
			d["item.toolMode.label"] = "工具模式";
			d["item.toolMode.desc"] = "记忆每个工具所选的模式，可选项按工具不同：道路、轨道、管道为直线、简单曲线、复杂曲线、连续曲线、网格、替换、节点；建筑、装饰物、树木为放置单个、放置多个、直线、曲线、目标印戳工具；功能区为填充、滚动、刷涂；区域为编辑、生成地图网格。资产的工具模式与功能的工具模式分开记忆，因为它们的选项并不相同。";
			d["item.toolMode.ex.group"] = "比如给两车道小型道路选了简单曲线，切换到其它的小型道路，也是简单曲线，但切换到大型道路就不是了";
			d["item.toolMode.ex.menu"] = "点击道路菜单里面的所有资产，都是简单曲线，但切换到电力菜单就不是了";
			d["item.toolMode.ex.category"] = "切换到六车道道路也是简单曲线，但切换到桥梁或双向地铁轨道就回到默认模式（需单独设置），即使它们都在道路菜单下";

			// ---------- 高度（用户原话的例子） ----------
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "记忆工具离开时的高度值，也包括提升高度、降低高度得到的结果；「高度阶段」（一次抬高或降低多少米，Anarchy 面板里那一行叫高度调整幅度）跟高度一起记忆、用同一个共用范围。交叉路口的高度不在记忆范围内。部分资产自己限定了可用的高度范围：共用数值超出所选资产允许的范围时，那件资产保持它自己允许的高度，已记忆的共用数值不变。";
			d["item.elevation.ex.group"] = "比如点击两车道道路将高度提高到10m，那么切换其它的小型道路，高度也是10m，但切换到大型道路就不是10m了";
			d["item.elevation.ex.menu"] = "点击所有道路菜单里面的资产，高度都是10m，但切换到电力菜单就不是10m了";
			d["item.elevation.ex.category"] = "切换到六车道道路高度也是10m，但切换到桥梁或双线铁路就变成0m（需单独设置），即使它们都在道路菜单下";

			// ---------- 并列模式 ----------
			d["item.parallel.label"] = "并列模式";
			d["item.parallel.desc"] = "记忆「并列道路」面板里的两样东西：并列模式开关（切换并列模式）与「并列偏移」的间距。原版一次并列就是两条线：打开后沿主线旁边同时画出第二条，能调的是这两条之间隔多远，比如 1 米、2 米；关掉就只画一条。";
			d["item.parallel.ex.group"] = "比如给两车道小型道路打开并列模式并把并列偏移调到5米，切换到其它的小型道路，同样是打开、偏移也是5米，但切换到大型道路就不是了";
			d["item.parallel.ex.menu"] = "点击道路菜单里面的所有资产，并列都是打开、偏移都是5米，但切换到电力菜单就不是了";
			d["item.parallel.ex.category"] = "切换到六车道道路同样是打开、偏移也是5米，但切换到桥梁或地铁轨道就变成关闭（需单独设置），即使它们都在道路菜单下";

			// ---------- 对齐 ----------
			d["item.snap.label"] = "对齐";
			d["item.snap.desc"] = "记忆「对齐」里的每一个开关：快速对齐当前形状、快速对齐功能区单元格长度、快速对齐直角、快速对齐路边、快速对齐道路、快速对齐拥有者一侧、快速对齐建筑两侧、快速对齐道路中央、快速对齐海岸线、快速对齐附近形状、快速对齐导线、快速对齐功能区网格、快速对齐节点、快速对齐物体表面、快速直立对齐、快速对齐空地网格、将重叠物体绑定至建筑、仅移除相符类型、显示轮廓线条、快速对齐距离。游戏自带的「开启／关闭所有对齐功能」是批量开关，本身不会被记忆。";
			d["item.snap.ex.group"] = "比如给两车道小型道路关掉快速对齐路边，切换到其它的小型道路，也是关着的，但切换到大型道路就是全开的默认状态";
			d["item.snap.ex.menu"] = "点击道路菜单里面的所有资产，对齐开关都是这一套，但切换到电力菜单就不是了";
			d["item.snap.ex.category"] = "切换到六车道道路也保持这一套开关，但切换到桥梁或步行道就恢复默认（需单独设置），即使它们都在道路菜单下";

			// ---------- 地形 ----------
			d["item.topography.label"] = "地形";
			d["item.topography.desc"] = "记忆「地形」这一行，也就是显示轮廓线条的开与关。";
			d["item.topography.ex.group"] = "比如给两车道小型道路勾上地形，切换到其它的小型道路，同样是勾着的，但切换到大型道路就不是了";
			d["item.topography.ex.menu"] = "点击道路菜单里面的所有资产，地形都是勾着的，但切换到电力菜单就不是了";
			d["item.topography.ex.category"] = "切换到六车道道路也是勾着的，但切换到桥梁或电缆就变成不勾（需单独设置）";


			// ---------- 左侧和右侧（Anarchy） ----------
			d["item.leftRight.label"] = "左侧和右侧";
			d["item.leftRight.desc"] = "记忆 Anarchy 网络面板里「左侧」「右侧」两行所选的网络升级，例如自行车车道、树木、草坪、堤岸、挡土墙、隔音屏障。需要安装 Anarchy（Paradox 模组 74604），没装时这一项不会出现。";
			d["item.leftRight.ex.group"] = "比如给两车道小型道路的右侧选上自行车车道，切换到其它的小型道路，右侧同样是自行车车道，但切换到大型道路就不是了";
			d["item.leftRight.ex.menu"] = "点击道路菜单里面的所有资产，左右两侧都是这一套升级，但切换到电力菜单就不是了";
			d["item.leftRight.ex.category"] = "切换到六车道道路右侧也是自行车车道，但切换到桥梁或双线铁路就变成未选择（需单独设置），即使它们都在道路菜单下";

			// ---------- 常规（Anarchy） ----------
			d["item.general.label"] = "常规";
			d["item.general.desc"] = "记忆 Anarchy 网络面板里「常规」一行的开关：强制地面、强制高架、强制隧道、平直斜坡、宽分隔带、扩展高度范围（取消高度上限）等。需要安装 Anarchy（Paradox 模组 74604），没装时这一项不会出现。";
			d["item.general.ex.group"] = "比如给两车道小型道路勾上强制隧道，切换到其它的小型道路，同样是强制隧道，但切换到大型道路就不是了";
			d["item.general.ex.menu"] = "点击道路菜单里面的所有资产，常规选项都是这一套，但切换到电力菜单就不是了";
			d["item.general.ex.category"] = "切换到六车道道路也是强制隧道，但切换到桥梁或双向地铁轨道就全部回到未勾选（需单独设置），即使它们都在道路菜单下";

			// ---------- 地下模式 ----------
			d["item.underground.label"] = "地下模式";
			d["item.underground.desc"] = "记忆「地下模式」开关（切换地下模式），也就是你放的东西在地面以下还是地面之上。";
			d["item.underground.ex.group"] = "比如给两车道小型道路打开地下模式，切换到其它的小型道路，同样在地下，但切换到大型道路就不是了";
			d["item.underground.ex.menu"] = "点击道路菜单里面的所有资产，都在地下模式，但切换到电力菜单就不是了";
			d["item.underground.ex.category"] = "切换到六车道道路也在地下，但切换到桥梁或双线铁路就回到地面（需单独设置），即使它们都在道路菜单下";

			// ---------- 其它 ----------
			d["item.other.label"] = "其它";
			d["item.other.desc"] = "记忆工具面板上其余可以记录的行：「颜色」一行的道路配色。";
			d["item.other.ex.group"] = "比如把两车道小型道路的颜色改成蓝色，切换到其它的小型道路，同样是蓝色，但切换到大型道路就不是了";
			d["item.other.ex.menu"] = "点击道路菜单里面的所有资产，颜色都是这一套，但切换到电力菜单就不是了";
			d["item.other.ex.category"] = "切换到六车道道路也是蓝色，但切换到桥梁或双线铁路就按它自己那一类另记一份（需单独设置），即使它们都在道路菜单下";

			// ---------- 笔刷（v0.6.2 从「其它」里独立出来：地形工具玩家的痛点） ----------
			// 行名取官方语言包：Toolbar.BRUSH_SIZE = 笔刷粗细，Toolbar.BRUSH_STRENGTH = 笔刷强度。
			d["item.brush.label"] = "笔刷";
			d["item.brush.desc"] = "记忆「笔刷粗细」和「笔刷强度」两行：笔刷粗细决定一次刷过多大范围，笔刷强度决定每一下改变多少。本项只管资产页这边的刷涂，也就是植被菜单里可以刷着摆放的树木等资产；地形改造工具的笔刷归功能页那一项，八类道具的笔刷归道具页那一项，三页各记各的。面板上没有这两行的工具既不记录也不会被改动。";
			d["item.brush.ex.group"] = "比如把刷树的笔刷粗细调到30米，同一组里的其它可刷资产也是30米，但换到另一组就不是了";
			d["item.brush.ex.menu"] = "点击植被菜单里面的所有资产，笔刷粗细和强度都是这一套，但切换到其它菜单就不是了";
			d["item.brush.ex.category"] = "切换到另一种树也是这一套，但这一档按资产服务的对象归群，地形改造工具和道具各在另两页另记一份（需单独设置）";

			// ---------- 颜色（v0.7.0 从「其它」独立；面板行名 ToolOptions.COLOR_TITLE = 颜色） ----------
			// 说明与例子沿用旧「其它」那一份文案（内容本来就全是讲道路颜色），这里只把行名写实。
			d["item.color.label"] = "颜色";

			// ---------- 保留年代（面板行名 ToolOptions.DECORATION_MODE_TITLE = 保留年代） ----------
			d["item.decoration.label"] = "保留年代";
			d["item.decoration.desc"] = "记忆「保留年代」开关：勾上之后被刷到的树不再成长、也不再老化倒下，等于把这片树的年龄冻在当下。这一行只出现在带树木数据的资产面板上（植被菜单里刷树的时候），其它资产面板上没有这一行，也就不记录、不改动。";
			d["item.decoration.ex.group"] = "比如刷树时打开保留年代，同一组里的其它植被刷法同样是打开的，但换到另一组就不是了";
			d["item.decoration.ex.menu"] = "点击植被菜单里面的所有资产，保留年代都是打开的，但切换到其它菜单就不是了";
			d["item.decoration.ex.category"] = "切换到另一种树也是打开的，但这一行只在带树木数据的资产上有，刷贴花之类根本没有这一行（需单独设置）";

			// ---------- 功能页五项（f.*）：行名与资产页同名项完全一致，主语换成六类功能 ----------
			// 需求 4：工具模式、对齐在三个页上各一套，互相独立；等高线/地下模式同样三页各一份，
			// 按当前工具所在页读写；笔刷只出现在地形改造工具与刷涂类功能上。
			d["item.f.toolMode.desc"] = "记忆功能面板上「工具模式」一行的选择：功能区为填充、框选、刷涂；区域工具（市辖区创建工具、专门产业区划分工具、表面区域规划工具）为编辑、生成地图网格；地形改造工具按工具各有模式；拆除工具为仅拆除主要元素、连同子元素、拆除一切。资产页与道具页的工具模式各记各的，互不影响。";
			d["item.f.toolMode.ex.group"] = "比如给住宅功能区选了刷涂，切换到商业、工业、办公功能区同样是刷涂，但换到区域工具就不是了";
			d["item.f.toolMode.ex.menu"] = "同一类功能里的工具都是这一套模式，换到另一类功能就不是了";
			d["item.f.toolMode.ex.category"] = "切换到另一类功能就回到它自己那份模式（需单独设置），即使它们都在同一个菜单栏里";
			d["item.f.snap.desc"] = "记忆功能区面板上「对齐」里的每一个开关：快速对齐直角、快速对齐网格、快速对齐道路、快速对齐拥有者一侧、显示轮廓线条等，可选项随工具不同而不同。游戏自带的「开启／关闭所有对齐功能」是批量开关，本身不会被记忆。资产页与道具页的对齐各记各的，互不影响。";
			d["item.f.snap.ex.group"] = "比如给住宅功能区关掉快速对齐网格，切换到商业、工业、办公功能区同样是关着的，但换到区域工具就不是了";
			d["item.f.snap.ex.menu"] = "同一类功能里的工具都是这一套开关，换到另一类功能就不是了";
			d["item.f.snap.ex.category"] = "切换到另一类功能就恢复它自己的默认开关（需单独设置），即使它们都在同一个菜单栏里";
			d["item.f.topography.desc"] = "记忆功能区面板上「等高线」一行的开与关，也就是画区域时看不看得到地形等高线。这一行按当前工具所在的那一页读写：功能页记的是功能区这一份，资产页另有资产那一份，两边不会互相顶掉。";
			d["item.f.topography.ex.group"] = "比如给住宅功能区勾上等高线，切换到商业、工业、办公功能区同样是勾着的";
			d["item.f.topography.ex.menu"] = "同一类功能里的工具都是勾着的，换到另一类功能就不是了";
			d["item.f.topography.ex.category"] = "切换到另一类功能就按它自己那份（需单独设置）；资产页的「地形」是另一份，不受这里影响";
			d["item.f.underground.desc"] = "记忆功能区面板上「地下模式」一行的开与关。这一行按当前工具所在的那一页读写：功能页记的是功能区这一份，资产页与道具页各有自己那一份，互不共用。";
			d["item.f.underground.ex.group"] = "比如给住宅功能区打开地下模式，切换到商业、工业、办公功能区同样在地下";
			d["item.f.underground.ex.menu"] = "同一类功能里的工具都在地下模式，换到另一类功能就不是了";
			d["item.f.underground.ex.category"] = "切换到另一类功能就按它自己那份（需单独设置）；资产页的「地下模式」是另一份，不受这里影响";
			d["item.f.brush.desc"] = "记忆功能面板上「笔刷粗细」和「笔刷强度」两行：笔刷粗细决定一次刷过多大范围，笔刷强度决定每一下改变多少。面板上有这两行的功能才起作用，也就是九种地形改造工具（海拔调整工具、地形平滑工具、地面平整工具、斜坡生成工具、纹理绘制工具 01～04、纹理橡皮擦）与功能区刷涂；没有这两行的功能既不记录也不会被改动。";
			d["item.f.brush.ex.group"] = "比如把抬升的笔刷粗细调到30米，同一组里的其它地形改造工具也是30米，但换到另一组就不是了";
			d["item.f.brush.ex.menu"] = "地形改造这一类里的所有工具，笔刷粗细和强度都是这一套，换到另一类功能就不是了";
			d["item.f.brush.ex.category"] = "切换到另一类功能就按它自己那份笔刷（需单独设置），即使面板上都有这两行";

			// ---------- 道具页五项（i.*）：城市美化八类道具 ----------
			d["item.i.toolMode.desc"] = "记忆道具面板上「工具模式」一行的选择：放置单个、放置多个、直线、曲线、目标印戳工具、刷涂等（按工具不同）。资产页与功能页的工具模式各记各的，互不影响。";
			d["item.i.toolMode.ex.group"] = "比如给某个自然类道具选了放置多个，切换到同一组的其它道具同样是放置多个，但换到另一组就不是了";
			d["item.i.toolMode.ex.menu"] = "城市美化菜单里的所有道具都是这一套模式，换到别的菜单就不是了";
			d["item.i.toolMode.ex.category"] = "切换到另一类道具（比如从自然换到贴花）就回到它自己那份模式（需单独设置），即使都在城市美化菜单下";
			d["item.i.snap.desc"] = "记忆道具面板上「对齐」里的每一个开关：快速对齐建筑表面、快速对齐附近形状、快速对齐道路、仅移除相符类型等，可选项随道具不同而不同。资产页与功能页的对齐各记各的，互不影响。";
			d["item.i.snap.ex.group"] = "比如给某个公园类道具关掉快速对齐建筑表面，切换到同一组的其它道具同样是关着的，但换到另一组就不是了";
			d["item.i.snap.ex.menu"] = "城市美化菜单里的所有道具都是这一套开关，换到别的菜单就不是了";
			d["item.i.snap.ex.category"] = "切换到另一类道具就恢复它自己的默认开关（需单独设置），即使都在城市美化菜单下";
			d["item.i.brush.desc"] = "记忆道具面板上「笔刷粗细」和「笔刷强度」两行，也就是刷涂摆放道具时一次刷多大、每一下摆多少。面板上有这两行的道具才起作用；没有这两行的道具既不记录也不会被改动。";
			d["item.i.brush.ex.group"] = "比如把刷涂的笔刷粗细调到20米，同一组里其它能刷的道具也是20米，但换到另一组就不是了";
			d["item.i.brush.ex.menu"] = "城市美化菜单里所有能刷涂的道具都是这一套，换到别的菜单就不是了";
			d["item.i.brush.ex.category"] = "切换到另一类道具就按它自己那份笔刷（需单独设置），即使都在城市美化菜单下";
			d["item.i.topography.desc"] = "记忆道具面板上「等高线」一行的开与关。这一行按当前工具所在的那一页读写：道具页记的是道具这一份，资产页与功能页各有自己那一份，互不共用。";
			d["item.i.topography.ex.group"] = "比如刷道具时勾上等高线，同一组里其它道具同样是勾着的";
			d["item.i.topography.ex.menu"] = "城市美化菜单里的所有道具都是勾着的，换到别的菜单就不是了";
			d["item.i.topography.ex.category"] = "切换到另一类道具就按它自己那份（需单独设置）；资产页、功能页的「等高线」是另外的份，不受这里影响";
			d["item.i.underground.desc"] = "记忆道具面板上「地下模式」一行的开与关，也就是摆在地面以下还是地面之上。这一行按当前工具所在的那一页读写：道具页记的是道具这一份，资产页与功能页各有自己那一份，互不共用。";
			d["item.i.underground.ex.group"] = "比如给某个道具打开地下模式，同一组里其它道具同样在地下";
			d["item.i.underground.ex.menu"] = "城市美化菜单里的所有道具都在地下模式，换到别的菜单就不是了";
			d["item.i.underground.ex.category"] = "切换到另一类道具就按它自己那份（需单独设置）；资产页、功能页的「地下模式」是另外的份，不受这里影响";
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
			d["enabled.desc"] = "預設開啟。開啟時下面所有設定都生效，所有工具項的數值都會即時記錄，回到存檔時就是你上次離開時的樣子；單獨關掉某一項只是不再恢復那一項，它的數值仍在記錄。關掉這個大開關則一切回到原版：停止記錄、不再恢復，資產、功能、道具三頁裡的所有設定同時失效（灰掉），每個工具都按原版自己的規矩走。下面三個分域開關全關時這裡自動跟著關；三個裡任何一個開啟，這裡自動開啟。";
			d["domain.asset.label"] = "啟用資產的工具項記憶";
			d["domain.asset.desc"] = "管「資產」頁裡所有工具項：記錄與讀檔恢復一起開關。關掉之後資產頁的設定全部灰掉不生效，工具回到原版行為，已記錄的資料仍保留在檔案裡，重新開啟照舊恢復。";
			d["domain.function.label"] = "啟用功能的工具項記憶";
			d["domain.function.desc"] = "管「功能」頁裡所有工具項：功能區、區域工具、路線工具、地形改造工具、空間與區域工具、標記與預製物件這六類。關掉之後功能頁的設定全部灰掉不生效，工具回到原版行為，已記錄的資料仍保留在檔案裡，重新開啟照舊恢復。";
			d["domain.item.label"] = "啟用道具的工具項記憶";
			d["domain.item.desc"] = "管「道具」頁裡所有工具項：景觀美化選單裡的八類物品（自然、公園、住宅物品、商業物品、工業物品、殘骸、貼花、盆栽）。關掉之後道具頁的設定全部灰掉不生效，工具回到原版行為，已記錄的資料仍保留在檔案裡，重新開啟照舊恢復。";
			d["tab.mod"] = "工具模式記憶設定";
			d["tab.master"] = "總開關";
			d["tab.assets"] = "資產";
			d["tab.functions"] = "功能";
			d["tab.items"] = "道具";
			d["group.domain"] = "分域開關";
			d["scope.label"] = "共用範圍";
			d["scope.desc"] = "這一項的數值在多大範圍內共用，下拉框括號裡標出原版行為與推薦選擇。";
			d[kScopeLineGroup] = "同組：{0}";
			d[kScopeLineMenu] = "同選單：{0}";
			d[kScopeLineCategory] = "同類資產：按資產實際服務誰歸類；車道數、寬窄、級別、大小與工具欄裡的分類都不算區別（小型停車場與大型停車場就同類），而橋樑、碼頭、交叉路口各自單獨成類：{0}";
			d[kScopeLineCategory + ".function"] = "同類：按功能所屬的六個分類歸群——功能區（住宅、商業、工業、辦公）、區域工具（行政區創建工具、特殊工業功能區域工具、地面區域劃分工具）、路線工具（交通運輸選單裡的全部路線）、地形改造工具（地形變更、軟化地形、水平地形、斜坡地形、紋理繪製 01～04、紋理橡皮擦共九種）、空間與區域工具（公園區域、徒步區域、閒蕩區域、清空區域、裁剪地面）、標記與預製物件（動物生成工具等）：{0}";
			d[kScopeLineCategory + ".item"] = "同類道具：按景觀美化選單的八類物品歸群（自然、公園、住宅物品、商業物品、工業物品、殘骸、貼花、盆栽）；同一類裡模型不同、大小不同都不算區別：{0}";
			d[kScopeLineShared] = "全域共用：任何支援本項工具的資產/功能全部共用，但資產和功能之間不共用";
			d[kScopeLineSharedSingle] = "全域共用：所有支援本項的工具共用一份";
			d[kScopeLineUnique] = "全部不共用：所有支援本項工具的資產/功能完全獨立設定";
			d[kScopeNote] = "注意，資產、功能、道具三頁各記各的，即使同名也互不相通；全域共用與全部不共用同樣只在本頁內成立。";

			d[kScopeGroup] = "同組";
			d[kScopeMenu] = "同選單";
			d[kScopeCategory] = "同類資產";
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
			d["item.anarchy.desc"] = "記憶 Anarchy 模組的開關，Anarchy 模組是全域共用，開啟後可以修改共用範圍。沒安裝 Anarchy 模組時這一項不會出現。";
			d["item.anarchy.ex.group"] = "例如給兩車道小型道路開啟 Anarchy，切換到其他小型道路也是開啟的，但切換到大型道路就不是了";
			d["item.anarchy.ex.menu"] = "點選道路選單裡面的所有資產，Anarchy 都是開啟的，但切換到電力選單就不是了";
			d["item.anarchy.ex.category"] = "切換到六車道道路同樣是開啟的，但切換到橋樑或雙線地鐵軌道就變成關閉（需單獨設定），即使它們都在道路選單下";

			// 地區主題 / 資料包：行名照原版語言包（Toolbar.THEME_PANEL_TITLE = 主題、
			// Toolbar.ASSET_PACKS_PANEL_TITLE = 安裝包）。
			d["item.themes.label"] = "主題";
			d["item.themes.desc"] = "記憶工具列篩選面板裡「主題」勾選了哪些。只有目前分類確實有資產使用主題時才會出現這一列，勾選後工具列只列出支援這些主題的資產。原版只在讀檔時把它重置回預設主題，所以這一項不建議開。";
			d["item.themes.ex.group"] = "例如只在道路/小型道路勾了某個主題，切換到大型道路就不按它篩選了";
			d["item.themes.ex.menu"] = "在道路選單勾的主題，切換到電力選單就不生效，回到道路選單又還是那一組";
			d["item.themes.ex.category"] = "這一行的勾選屬於工具欄面板本身，按當前所在的選單+分類記憶，不按資產類別共用";
			d["item.packs.label"] = "安裝包";
			d["item.packs.desc"] = "記憶工具列篩選面板裡「安裝包」勾選了哪些，勾選後工具列只列出屬於這些安裝包的資產。原版每次切換選單或分類都會把它清空，所以按「同一選單+分類」來記最貼近你看到的規律，這一項也不建議開。";
			d["item.packs.ex.group"] = "例如在道路/小型道路勾了某個安裝包，切到大型道路會被原版清空，回到小型道路又自動恢復";
			d["item.packs.ex.menu"] = "在道路選單勾的安裝包，和電力選單勾的各記各的";
			d["item.packs.ex.category"] = "這一行的勾選屬於工具欄面板本身，按當前所在的選單+分類記憶；原版每次換分類都會把它清空";

			// ---------- 工具模式 ----------
			d["item.toolMode.label"] = "工具模式";
			d["item.toolMode.desc"] = "記憶每個工具所選的模式，可選項按工具不同：道路、軌道、管道為直線、簡單曲線、複數曲線、連續曲線、網格、替換、連接點；建築、裝飾物、樹木為設置一個、設置多個、直線、曲線、物件印章工具；功能區為填充、選取方格、塗上；區域為編輯、生成地圖方格。資產的工具模式與功能的工具模式分開記憶，因為它們的選項並不相同。";
			d["item.toolMode.ex.group"] = "例如給兩車道小型道路選了簡單曲線，切換到其他小型道路也是簡單曲線，但切換到大型道路就不是了";
			d["item.toolMode.ex.menu"] = "點選道路選單裡面的所有資產，都是簡單曲線，但切換到電力選單就不是了";
			d["item.toolMode.ex.category"] = "切換到六車道道路也是簡單曲線，但切換到橋樑或雙線地鐵軌道就回到預設模式（需單獨設定），即使它們都在道路選單下";

			// ---------- 高度 ----------
			d["item.elevation.label"] = "高度";
			d["item.elevation.desc"] = "記憶工具離開時的高度值，也包括提升高度、降低高度得到的結果；「高度階段」（一次抬高或降低多少公尺，Anarchy 面板裡那一行叫高度間距）跟高度一起記憶、用同一個共用範圍。交叉路口的高度不在記憶範圍內。部分資產自己限定了可用的高度範圍：共用數值超出所選資產允許的範圍時，該資產保持它自己允許的高度，已記憶的共用數值不變。";
			d["item.elevation.ex.group"] = "例如點選兩車道道路把高度提高到10m，那麼切換其他小型道路，高度也是10m，但切換到大型道路就不是10m了";
			d["item.elevation.ex.menu"] = "點選所有道路選單裡面的資產，高度都是10m，但切換到電力選單就不是10m了";
			d["item.elevation.ex.category"] = "切換到六車道道路高度也是10m，但切換到橋樑或雙軌鐵路就變成0m（需單獨設定），即使它們都在道路選單下";

			// ---------- 並行模式 ----------
			d["item.parallel.label"] = "並行模式";
			d["item.parallel.desc"] = "記憶「並行道路」面板裡的兩樣東西：並行模式開關（切換並行模式）與「並行偏移」的間距。原版一次並行就是兩條線：開啟後沿主線旁邊同時畫出第二條，能調的是這兩條之間隔多遠，例如 1 米、2 米；關掉就只畫一條。";
			d["item.parallel.ex.group"] = "例如給兩車道小型道路開啟並行模式並把並行偏移調到5米，切換到其他小型道路同樣是開啟、偏移也是5米，但切換到大型道路就不是了";
			d["item.parallel.ex.menu"] = "點選道路選單裡面的所有資產，並行都是開啟、偏移都是5米，但切換到電力選單就不是了";
			d["item.parallel.ex.category"] = "切換到六車道道路同樣是開啟、偏移也是5米，但切換到橋樑或地鐵軌道就變成關閉（需單獨設定），即使它們都在道路選單下";

			// ---------- 吸附 ----------
			d["item.snap.label"] = "吸附";
			d["item.snap.desc"] = "記憶「吸附」裡的每一個開關：快速對齊已有幾何線、快速對齊功能區單位格長度、快速對齊90度角、快速對齊路邊、快速對齊道路、快速對齊擁有者一側、快速對齊建築兩側、快速對齊道路中央、快速對齊水陸交界線、快速對齊附近幾何線、快速對齊導線、快速對齊功能區網格、快速對齊節點、快速對齊物件表面、快速直立對齊、快速對齊空地網格、將重疊對象綁定至建築、只移除符合的類型、顯示輪廓、對齊距離。遊戲自帶的「開啟／關閉所有對齊功能」是批次開關，本身不會被記憶。";
			d["item.snap.ex.group"] = "例如給兩車道小型道路關掉快速對齊路邊，切換到其他小型道路同樣是關著的，但切換到大型道路就是全開的預設狀態";
			d["item.snap.ex.menu"] = "點選道路選單裡面的所有資產，吸附開關都是這一組，但切換到電力選單就不是了";
			d["item.snap.ex.category"] = "切換到六車道道路仍保持這一組開關，但切換到橋樑或步行道就恢復預設（需單獨設定），即使它們都在道路選單下";

			// ---------- 地形圖 ----------
			d["item.topography.label"] = "地形圖";
			d["item.topography.desc"] = "記憶「地形圖」這一行的開與關，也就是是否顯示輪廓。";
			d["item.topography.ex.group"] = "例如給兩車道小型道路勾選地形圖，切換到其他小型道路同樣是勾選的，但切換到大型道路就不是了";
			d["item.topography.ex.menu"] = "點選道路選單裡面的所有資產，地形圖都是勾選的，但切換到電力選單就不是了";
			d["item.topography.ex.category"] = "切換到六車道道路也是勾選的，但切換到橋樑或電纜就變成不勾（需單獨設定）";


			// ---------- 左側和右側（Anarchy） ----------
			d["item.leftRight.label"] = "左側和右側";
			d["item.leftRight.desc"] = "記憶 Anarchy 網路面板「左」「右」兩行所選的網路升級，例如自行車道、樹木、草坪、堤岸、擋土牆、音障。需要安裝 Anarchy（Paradox 模組 74604），沒裝時這一項不會出現。";
			d["item.leftRight.ex.group"] = "例如給兩車道小型道路的右側選上自行車道，切換到其他小型道路右側同樣是自行車道，但切換到大型道路就不是了";
			d["item.leftRight.ex.menu"] = "點選道路選單裡面的所有資產，左右兩側都是這一組升級，但切換到電力選單就不是了";
			d["item.leftRight.ex.category"] = "切換到六車道道路右側也是自行車道，但切換到橋樑或雙軌鐵路就變成未選擇（需單獨設定），即使它們都在道路選單下";

			// ---------- 一般（Anarchy） ----------
			d["item.general.label"] = "一般";
			d["item.general.desc"] = "記憶 Anarchy 網路面板「一般」一行的開關：地面、高架、隧道、固定坡度、寬分隔島、擴展高度範圍（取消高度上限）等。需要安裝 Anarchy（Paradox 模組 74604），沒裝時這一項不會出現。";
			d["item.general.ex.group"] = "例如給兩車道小型道路勾選隧道，切換到其他小型道路同樣是隧道，但切換到大型道路就不是了";
			d["item.general.ex.menu"] = "點選道路選單裡面的所有資產，一般選項都是這一組，但切換到電力選單就不是了";
			d["item.general.ex.category"] = "切換到六車道道路也是隧道，但切換到橋樑或雙線地鐵軌道就全部回到未勾選（需單獨設定），即使它們都在道路選單下";

			// ---------- 地下模式 ----------
			d["item.underground.label"] = "地下模式";
			d["item.underground.desc"] = "記憶「地下模式」開關（切換地下模式），也就是你放的東西在地面以下還是地面之上。";
			d["item.underground.ex.group"] = "例如給兩車道小型道路開啟地下模式，切換到其他小型道路同樣在地下，但切換到大型道路就不是了";
			d["item.underground.ex.menu"] = "點選道路選單裡面的所有資產，都在地下模式，但切換到電力選單就不是了";
			d["item.underground.ex.category"] = "切換到六車道道路也在地下，但切換到橋樑或雙軌鐵路就回到地面（需單獨設定），即使它們都在道路選單下";

			// ---------- 其他 ----------
			d["item.other.label"] = "其他";
			d["item.other.desc"] = "記憶工具面板上其餘可以記錄的行：「顏色」一行的道路配色。";
			d["item.other.ex.group"] = "例如把兩車道小型道路的顏色改成藍色，切換到其他小型道路同樣是藍色，但切換到大型道路就不是了";
			d["item.other.ex.menu"] = "點選道路選單裡面的所有資產，顏色都是這一組設定，但切換到電力選單就不是了";
			d["item.other.ex.category"] = "切換到六車道道路也是藍色，但切換到橋樑或雙軌鐵路就按它自己那一類另記一份（需單獨設定），即使它們都在道路選單下";

			// 行名取官方語言包：Toolbar.BRUSH_SIZE = 筆刷大小，Toolbar.BRUSH_STRENGTH = 筆刷硬度
			d["item.brush.label"] = "筆刷";
			d["item.brush.desc"] = "記憶「筆刷大小」和「筆刷硬度」兩行：筆刷大小決定一次刷過多大範圍，筆刷硬度決定每一下改變多少。本項只管資產頁這邊的刷塗，也就是植被選單裡可以刷著擺放的樹木等資產；地形改造工具的筆刷歸功能頁那一項，八類物品的筆刷歸道具頁那一項，三頁各記各的。面板上沒有這兩行的工具既不記錄也不會被改動。";
			d["item.brush.ex.group"] = "例如把刷樹的筆刷大小調成30公尺，同一組裡面的其他可刷資產也是30公尺，但換到另一組就不是了";
			d["item.brush.ex.menu"] = "點選植被選單裡面的所有資產，筆刷大小和硬度都是這一組設定，但切換到其他選單就不是了";
			d["item.brush.ex.category"] = "切換到另一種樹也是這一組，但這一檔按資產服務的對象歸群，地形改造工具和物品各在另兩頁另記一份（需單獨設定）";

			// ---------- 顏色（v0.7.0 從「其他」獨立；面板行名 ToolOptions.COLOR_TITLE = 顏色） ----------
			d["item.color.label"] = "顏色";

			// ---------- 保留年代（面板行名 ToolOptions.DECORATION_MODE_TITLE = 保留年代） ----------
			d["item.decoration.label"] = "保留年代";
			d["item.decoration.desc"] = "記憶「保留年代」開關：勾上之後被刷到的樹不再成長、也不再老化倒下，等於把這片樹的年齡凍在當下。這一行只出現在帶樹木資料的資產面板上（植被選單裡刷樹的時候），其他資產面板上沒有這一行，也就不記錄、不改動。";
			d["item.decoration.ex.group"] = "例如刷樹時打開保留年代，同一組裡面的其他植被刷法同样是打開的，但換到另一組就不是了";
			d["item.decoration.ex.menu"] = "點選植被選單裡面的所有資產，保留年代都是打開的，但切換到其他選單就不是了";
			d["item.decoration.ex.category"] = "切換到另一種樹也是打開的，但這一行只在帶樹木資料的資產上有，刷貼花之類根本没有這一行（需單獨設定）";

			// ---------- 功能頁五項（f.*）----------
			d["item.f.toolMode.desc"] = "記憶功能面板上「工具模式」一行的選擇：功能區為填充、框選、刷塗；區域工具（行政區創建工具、特殊工業功能區域工具、地面區域劃分工具）為編輯、產生地圖網格；地形改造工具按工具各有模式；拆除工具為僅拆除主要元素、連同子元素、拆除一切。資產頁與道具頁的工具模式各記各的，互不影響。";
			d["item.f.toolMode.ex.group"] = "例如給住宅功能區選了刷塗，切換到商業、工業、辦公功能區同样是刷塗，但換到區域工具就不是了";
			d["item.f.toolMode.ex.menu"] = "同一類功能裡面的工具都是這一組模式，換到另一類功能就不是了";
			d["item.f.toolMode.ex.category"] = "切換到另一類功能就回到它自己那組的標準（需單獨設定），即使它們都可以在同一個工具欄選單進入";
			d["item.f.snap.desc"] = "記憶功能面板上「對齊」裡面的每一個開關：快速對齊直角、快速對齊格線、快速對齊道路、快速對齊擁有者一側、顯示輪廓線條等，可選項隨工具而不同。遊戲自帶的「開啟／關閉所有對齊功能」是批次開關，本身不會被記憶。資產頁與道具頁的對齊各記各的，互不影響。";
			d["item.f.snap.ex.group"] = "例如給住宅功能區关掉快速對齊格線，切換到商業、工業、辦公功能區同样是關掉的，但換到區域工具就不是了";
			d["item.f.snap.ex.menu"] = "同一類功能裡面的工具都是這一組開關，換到另一類功能就不是了";
			d["item.f.snap.ex.category"] = "切換到另一類功能就恢復它自己的預設開關（需單獨設定），即使它們都可以在同一個工具欄選單進入";
			d["item.f.topography.desc"] = "記憶功能面板上「等高線」一行的開與關，也就是畫區域時看不看得到地形等高線。這一行按目前工具所在的那一頁讀寫：功能頁記的是功能區那一份，資產頁另有資產那一份，兩邊不會互相頂掉。";
			d["item.f.topography.ex.group"] = "例如給住宅功能區勾上等高線，切換到商業、工業、辦公功能區同样是勾著的";
			d["item.f.topography.ex.menu"] = "同一類功能裡面的工具都是勾著的，換到另一類功能就不是了";
			d["item.f.topography.ex.category"] = "切換到另一類功能就按它自己那一份（需單獨設定）；資產頁的「地形」是另一份，不受這裡影響";
			d["item.f.underground.desc"] = "記憶功能面板上「地下模式」一行的開與關。這一行按目前工具所在的那一頁讀寫：功能頁記的是功能區那一份，資產頁與道具頁各有自己那一份，互不共用。";
			d["item.f.underground.ex.group"] = "例如給住宅功能區打開地下模式，切換到商業、工業、辦公功能區同样在地下";
			d["item.f.underground.ex.menu"] = "同一類功能裡面的工具都在地下模式，換到另一類功能就不是了";
			d["item.f.underground.ex.category"] = "切換到另一類功能就按它自己那一份（需單獨設定）；資產頁的「地下模式」是另一份，不受這裡影響";
			d["item.f.brush.desc"] = "記憶功能面板上「筆刷大小」和「筆刷硬度」兩行：筆刷大小決定一次刷過多大範圍，筆刷硬度決定每一下改變多少。面板上有這兩行的功能才起作用，也就是九種地形改造工具（地形變更工具、軟化地形工具、水平地形工具、斜坡地形工具、紋理繪製工具 01～04、紋理橡皮擦）與功能區刷塗；沒有這兩行的功能既不記錄也不會被改動。";
			d["item.f.brush.ex.group"] = "例如把地形變更的筆刷大小調成30公尺，同一組裡面的其他地形改造工具也是30公尺，但換到另一組就不是了";
			d["item.f.brush.ex.menu"] = "地形改造這一類裡面的所有工具，筆刷大小和硬度都是這一組設定，換到另一類功能就不是了";
			d["item.f.brush.ex.category"] = "切換到另一類功能就按它自己那一份筆刷（需單獨設定），即使面板上都有這兩行";

			// ---------- 道具頁五項（i.*）：景觀美化選單八類物品 ----------
			d["item.i.toolMode.desc"] = "記憶道具面板上「工具模式」一行的選擇：放置單一、放置多個、直線、曲線、目標圖戳工具、刷塗等（按工具而不同）。資產頁與功能頁的工具模式各記各的，互不影響。";
			d["item.i.toolMode.ex.group"] = "例如給某個自然類道具選了放置多個，切換到同一組的其他道具同样是放置多個，但換到另一組就不是了";
			d["item.i.toolMode.ex.menu"] = "景觀美化選單裡面的所有道具都是這一組模式，換到別的選單就不是了";
			d["item.i.toolMode.ex.category"] = "切換到另一類道具（例如從自然換到貼花）就回到它那一份模式（需單獨設定），即使都在景觀美化選單下";
			d["item.i.snap.desc"] = "記憶道具面板上「對齊」裡面的每一個開關：快速對齊建築表面、快速對齊附近形狀、快速對齊道路、僅移除相符類型等，可選項隨道具而不同。資產頁與功能頁的對齊各記各的，互不影響。";
			d["item.i.snap.ex.group"] = "例如給某個公園類道具关掉快速對齊建築表面，切換到同一組的其他道具同样是關掉的，但換到另一組就不是了";
			d["item.i.snap.ex.menu"] = "景觀美化選單裡面的所有道具都是這一組開關，換到別的選單就不是了";
			d["item.i.snap.ex.category"] = "切換到另一類道具就恢復它自己的預設開關（需單獨設定），即使都在景觀美化選單下";
			d["item.i.brush.desc"] = "記憶道具面板上「筆刷大小」和「筆刷硬度」兩行，也就是刷擺道具時一次刷多大、每一下擺多少。面板上有這兩行的道具才起作用；沒有這兩行的道具既不記錄也不會被改動。";
			d["item.i.brush.ex.group"] = "例如把刷塗的筆刷大小調成20公尺，同一組裡其他可刷的道具也是20公尺，但換到另一組就不是了";
			d["item.i.brush.ex.menu"] = "景觀美化選單裡所有能刷塗的道具都是這一組，換到別的選單就不是了";
			d["item.i.brush.ex.category"] = "切換到另一類道具就按它自己那一份筆刷（需單獨設定），即使都在景觀美化選單下";
			d["item.i.topography.desc"] = "記憶道具面板上「等高線」一行的開與關。這一行按目前工具所在的那一頁讀寫：道具頁記的是道具那一份，資產頁與功能頁各有自己那一份，互不共用。";
			d["item.i.topography.ex.group"] = "例如刷道具時勾上等高線，同一組裡其他道具同样是勾著的";
			d["item.i.topography.ex.menu"] = "景觀美化選單裡面的所有道具都是勾著的，換到別的選單就不是了";
			d["item.i.topography.ex.category"] = "切換到另一類道具就按它自己那一份（需單獨設定）；資產頁、功能頁的「等高線」是另外的份，不受這裡影響";
			d["item.i.underground.desc"] = "記憶道具面板上「地下模式」一行的開與關，也就是擺在地面以下還是地面之上。這一行按目前工具所在的那一頁讀寫：道具頁記的是道具那一份，資產頁與功能頁各有自己那一份，互不共用。";
			d["item.i.underground.ex.group"] = "例如給某個道具打開地下模式，同一組裡其他道具同样在地下";
			d["item.i.underground.ex.menu"] = "景觀美化選單裡面的所有道具都在地下模式，換到別的選單就不是了";
			d["item.i.underground.ex.category"] = "切換到另一類道具就按它自己那一份（需單獨設定）；資產頁、功能頁的「地下模式」是另外的份，不受這裡影響";
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
			d["tab.assets"] = "Assets";
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
			d["enabled.desc"] = "On by default. While it is on every setting below takes effect and every item's value is recorded as you work, so returning to a save puts the panels back exactly as you left them; turning one item off only stops that item from being restored - its values are still recorded. Switching this master switch off returns everything to vanilla: recording stops, nothing is restored, and all settings on the Assets, Functions and Items pages are greyed out at once, so every tool behaves the way the game does on its own. It turns itself off when all three per-page switches below are off, and back on as soon as any one of them is on.";
			d["domain.asset.label"] = "Remember asset tool items";
			d["domain.asset.desc"] = "Covers every item on the Assets page: recording and restoring on load switch together. While it is off the whole Assets page is greyed out, tools behave like vanilla, and values already recorded stay in the file untouched, so switching it back on restores them.";
			d["domain.function.label"] = "Remember function tool items";
			d["domain.function.desc"] = "Covers every item on the Functions page - the six function groups: zone types, the area tools, all route tools, the terrain tools, spaces and areas, and markers and object prefabs. While it is off the whole Functions page is greyed out, tools behave like vanilla, and values already recorded stay in the file untouched, so switching it back on restores them.";
			d["domain.item.label"] = "Remember item tool items";
			d["domain.item.desc"] = "Covers every item on the Items page - the eight prop categories of the Landscaping menu: Nature, Park, Residential Props, Commercial Props, Industrial Props, Debris, Decals and Potted plants. While it is off the whole Items page is greyed out, tools behave like vanilla, and values already recorded stay in the file untouched, so switching it back on restores them.";
			d["scope.label"] = "Sharing scope";
			d["scope.desc"] = "How widely this item's value is shared. The brackets in the drop-down show what vanilla does and what we recommend.";
			d[kScopeLineGroup] = "Same group: {0}";
			d[kScopeLineMenu] = "Same menu: {0}";
			d[kScopeLineCategory] = "Same asset type: grouped by what the asset actually serves, so lane count, size, level and the toolbar category make no difference (a small parking lot and a large one are the same type), while bridges, quays and intersections each form their own type: {0}";
			// Requirement 2: on the Functions and Items pages "same asset type" is not about assets,
			// so it reads "Same group type" / "Same prop type" and groups by the page's own taxonomy.
			d[kScopeLineCategory + ".function"] = "Same type: grouped by which of the six function groups the tool belongs to - zone types (residential, commercial, industrial, office), the area tools (District Creation Tool, Specialized Industry tool, Surface Area tool), the route tools (every route in the Transportation menu), the nine terrain tools (Raise Terrain, Smooth Terrain, Level Terrain, Slope Terrain, Paint Texture 01-04, Erase Texture), spaces and areas (Park Area, Walking Area, Hangaround Area, Clear Area, Clip Surface) and markers and object prefabs (the animal spawn tool and its like): {0}";
			d[kScopeLineCategory + ".item"] = "Same prop type: grouped by which of the eight prop categories of the Landscaping menu the item is in (Nature, Park, Residential Props, Commercial Props, Industrial Props, Debris, Decals, Potted Plants); a different model or a different size inside one category is not a different type: {0}";
			d[kScopeLineShared] = "Shared globally: every asset or function that supports this item shares it, but assets and functions do not share with each other";
			d[kScopeLineSharedSingle] = "Shared globally: every tool on this page that supports this item uses one value";
			d[kScopeLineUnique] = "Not shared at all: every asset and function that supports this item is set independently";
			d[kScopeNote] = "Note: the Assets, Functions and Items pages each keep their own memory even when a row has the same name on several pages; Shared globally and Not shared at all only reach inside the page you are looking at.";

			d[kScopeGroup] = "Same group";
			d[kScopeMenu] = "Same menu";
			d[kScopeCategory] = "Same asset type";
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
			d["item.anarchy.desc"] = "Remembers the Anarchy mod's switch. Anarchy is shared across the whole game, so this one is global; once it is on you can change the sharing scope. Without the Anarchy mod installed this item does not appear.";
			d["item.anarchy.ex.group"] = "for example, turn Anarchy on for a two-lane small road and the other small roads keep it on, while a large road does not";
			d["item.anarchy.ex.menu"] = "every asset you click in the Roads menu has Anarchy on, but moving to the Electricity menu it is not";
			d["item.anarchy.ex.category"] = "switching to a six-lane road keeps it on, but a bridge or a double subway track turns it off and needs its own setting, even though they are all in the Roads menu";

			// Row names follow the game's own language pack:
			// Toolbar.THEME_PANEL_TITLE = "Theme", Toolbar.ASSET_PACKS_PANEL_TITLE = "Pack".
			d["item.themes.label"] = "Theme";
			d["item.themes.desc"] = "Remembers which entries are ticked in the toolbar's Theme filter. That row only appears when the current category really contains assets that use themes, and ticking one limits the toolbar list to assets supporting those themes. Vanilla only resets it to the default theme when a save is loaded, so we recommend leaving this item off.";
			d["item.themes.ex.group"] = "for example you tick a theme only under Roads/Small Roads, so large roads don't filter by it";
			d["item.themes.ex.menu"] = "a theme ticked in the Roads menu stops applying in the Electricity menu and is back when you return";
			d["item.themes.ex.category"] = "this row is the filter panel's own state, so it is filed under the menu and group you are in rather than by asset type";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Remembers which entries are ticked in the toolbar's Pack filter; ticking one limits the toolbar list to assets belonging to those packs. Vanilla clears it every time you change menu or category, so remembering it per menu + category matches what you see; we recommend leaving this item off too.";
			d["item.packs.ex.group"] = "for example you tick a pack under Roads/Small Roads, vanilla clears it on large roads, and it comes back when you return";
			d["item.packs.ex.menu"] = "packs ticked in the Roads menu and in the Electricity menu are remembered separately";
			d["item.packs.ex.category"] = "this row is the filter panel's own state, filed under the menu and group you are in; vanilla clears it every time the group changes";

			// ---------- Tool Mode ----------
			d["item.toolMode.label"] = "Tool Mode";
			d["item.toolMode.desc"] = "Remembers the mode picked for each tool, and the options differ by tool: roads, tracks and pipes offer Straight, Simple Curve, Complex Curve, Continuous, Grid, Replace and Point; buildings, props and trees offer Place one, Place multiple, Line, Curve and Object stamp tool; zones offer Fill, Marquee and Paint; areas offer Edit and Generate Map Grid. Asset tool modes and function tool modes are remembered separately because their option sets differ.";
			d["item.toolMode.ex.group"] = "for example, pick Simple Curve for a two-lane small road and the other small roads use Simple Curve too, while a large road does not";
			d["item.toolMode.ex.menu"] = "every asset you click in the Roads menu is on Simple Curve, but moving to the Electricity menu it is not";
			d["item.toolMode.ex.category"] = "switching to a six-lane road still gives Simple Curve, but a bridge or a double subway track falls back to its default mode and needs its own setting, even though they are all in the Roads menu";

			// ---------- Elevation ----------
			d["item.elevation.label"] = "Elevation";
			d["item.elevation.desc"] = "Remembers the elevation a tool was left at, including where Increase elevation and Decrease elevation took it. The Elevation step (how far one press moves you — the row Anarchy adds for the same value carries that very name) is remembered together with it and shares its scope. The elevation of intersections is not covered. Some assets have their own elevation limits: if the shared value is outside what the selected asset allows, that asset keeps an elevation it can actually use, and the shared value itself stays as it was.";
			d["item.elevation.ex.group"] = "for example, raise a two-lane road to 10 m and the other small roads go to 10 m too, while a large road is not at 10 m";
			d["item.elevation.ex.menu"] = "every asset you click in the Roads menu is at 10 m, but moving to the Electricity menu it is not at 10 m";
			d["item.elevation.ex.category"] = "switching to a six-lane road is still 10 m, but a bridge or a double train track goes back to 0 m and needs its own setting, even though they are all in the Roads menu";

			// ---------- Parallel Mode ----------
			d["item.parallel.label"] = "Parallel Mode";
			d["item.parallel.desc"] = "Remembers the two things in the Parallel Road panel: the Parallel Mode switch (Toggle parallel mode) and the Parallel Offset spacing. Parallel mode is always two lines: switch it on and a second line is built alongside the one you draw, and what you can tune is how far apart those two sit, say 1 m or 2 m. Switched off you just get the single line.";
			d["item.parallel.ex.group"] = "for example, turn on Parallel Mode for a two-lane small road and set the offset to 5 m, the other small roads come back switched on with the same 5 m, while a large road does not";
			d["item.parallel.ex.menu"] = "every asset you click in the Roads menu is parallel with a 5 m offset, but moving to the Electricity menu it is not";
			d["item.parallel.ex.category"] = "switching to a six-lane road is still on with the same 5 m offset, but a bridge or a subway track switches it off and needs its own setting, even though they are all in the Roads menu";

			// ---------- Snapping ----------
			d["item.snap.label"] = "Snapping";
			d["item.snap.desc"] = "Remembers every switch on the Snapping row: Snap to existing geometry, Snap to zoning cell length, Snap to 90 degree angles, Snap to the sides of a road, Snap to roads, Snap to the side of the owner, Snap to the sides of a building, Snap to the middle of a road, Snap to shoreline, Snap to nearby geometry, Snap to guide lines, Snap to zone grid, Snap to nodes, Snap to the surface of an object, Snap upright, Snap to lot grid, Binds overlapping items to a building, Remove only matching type, Show contour lines and Snap to distance. The game's own Toggle all snapping on/off row is a batch switch and is not remembered.";
			d["item.snap.ex.group"] = "for example, switch off Snap to the sides of a road for a two-lane small road and the other small roads have it off too, while a large road still has everything on";
			d["item.snap.ex.menu"] = "every asset you click in the Roads menu uses this same set of snapping switches, but moving to the Electricity menu it does not";
			d["item.snap.ex.category"] = "switching to a six-lane road keeps the same set, but a bridge or a pathway returns to the defaults and needs its own setting, even though they are all in the Roads menu";

			// ---------- Topography ----------
			d["item.topography.label"] = "Topography";
			d["item.topography.desc"] = "Remembers the Topography row, that is whether Show contour lines is on.";
			d["item.topography.ex.group"] = "for example, turn Topography on for a two-lane small road and the other small roads have it on too, while a large road does not";
			d["item.topography.ex.menu"] = "every asset you click in the Roads menu shows contour lines, but moving to the Electricity menu it does not";
			d["item.topography.ex.category"] = "switching to a six-lane road still shows them, but a bridge or an electric cable has the row off and needs its own setting";


			// ---------- Left / Right (Anarchy) ----------
			d["item.leftRight.label"] = "Left and Right";
			d["item.leftRight.desc"] = "Remembers which network upgrades are picked on the Left and Right rows of the Anarchy network panel, for example Bike Lane, Trees, Grass, Quay, Retaining Wall and Sound Barrier. Needs Anarchy (Paradox mod 74604); without it this item does not appear.";
			d["item.leftRight.ex.group"] = "for example, pick Bike Lane on the right side of a two-lane small road and the other small roads get Bike Lane there too, while a large road does not";
			d["item.leftRight.ex.menu"] = "every asset you click in the Roads menu uses this same pair of side selections, but moving to the Electricity menu it does not";
			d["item.leftRight.ex.category"] = "switching to a six-lane road still has Bike Lane on the right, but a bridge or a double train track goes back to nothing picked and needs its own setting, even though they are all in the Roads menu";

			// ---------- General (Anarchy) ----------
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Remembers the switches on the General row of the Anarchy network panel: Ground, Elevated, Tunnel, Constant Slope, Wide Median and Expanded Elevation Range (which lifts the height limit). Needs Anarchy (Paradox mod 74604); without it this item does not appear.";
			d["item.general.ex.group"] = "for example, pick Tunnel for a two-lane small road and the other small roads are forced into tunnels too, while a large road is not";
			d["item.general.ex.menu"] = "every asset you click in the Roads menu uses this same set of General switches, but moving to the Electricity menu it does not";
			d["item.general.ex.category"] = "switching to a six-lane road still gives Tunnel, but a bridge or a double subway track goes back to nothing ticked and needs its own setting, even though they are all in the Roads menu";

			// ---------- Underground Mode ----------
			d["item.underground.label"] = "Underground Mode";
			d["item.underground.desc"] = "Remembers the Underground Mode switch (Toggle underground mode), which decides whether what you place goes below the ground or on top of it.";
			d["item.underground.ex.group"] = "for example, turn on Underground Mode for a two-lane small road and the other small roads go underground too, while a large road does not";
			d["item.underground.ex.menu"] = "every asset you click in the Roads menu is built underground, but moving to the Electricity menu it is not";
			d["item.underground.ex.category"] = "switching to a six-lane road still goes underground, but a bridge or a double train track stays on the surface and needs its own setting, even though they are all in the Roads menu";

			// ---------- Other ----------
			d["item.other.label"] = "Other";
			d["item.other.desc"] = "Remembers the remaining rows a tool panel can record: the road colors on the Color row.";
			d["item.other.ex.group"] = "for example, set the color of a two-lane small road to blue and the other small roads turn blue too, while a large road does not";
			d["item.other.ex.menu"] = "every asset you click in the Roads menu uses this same color, but moving to the Electricity menu it does not";
			d["item.other.ex.category"] = "switching to a six-lane road is still blue, but a bridge or a double train track keeps its own copy and needs its own setting, even though they are all in the Roads menu";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行。
			// v0.7.0：地形改造工具归到功能页，笔刷于是分三页各一项，本项只管资产页的刷涂。
			d["item.brush.label"] = "Brush";
			d["item.brush.desc"] = "Remembers the Brush Size and Brush Strength rows: Brush Size sets how wide one stroke reaches, Brush Strength sets how much each stroke changes. This one only covers brushing on the Assets page, that is placing assets such as trees from the Vegetation menu with the Paint mode; the nine terrain tools have their own brush item on the Functions page and the prop categories have theirs on the Items page, and the three pages never share. Tools whose panel has no such rows are neither recorded nor changed.";
			d["item.brush.ex.group"] = "for example, set a 30 m brush while brushing trees and the other brushable assets in the same group are 30 m too, while a tool in another group is not";
			d["item.brush.ex.menu"] = "every asset you brush in the Vegetation menu uses this same brush size and strength, but moving to another menu it does not";
			d["item.brush.ex.category"] = "switching to another kind of tree keeps this same brush, but the terrain tools and the props each keep their own copy on another page (needs its own setting)";

			// ---------- Colour (v0.7.0: split out of "Other"; panel row = ToolOptions.COLOR_TITLE) ----------
			d["item.color.label"] = "Color";

			// ---------- Preserve Age (panel row = ToolOptions.DECORATION_MODE_TITLE) ----------
			d["item.decoration.label"] = "Preserve Age";
			d["item.decoration.desc"] = "Remembers the Preserve Age switch: while it is on, trees you brush or place stop growing and stop dying of old age, which freezes the age of that stand of trees where it is. The row only shows up on assets that carry tree data (brushing trees from the Vegetation menu); assets whose panel has no such row are neither recorded nor changed.";
			d["item.decoration.ex.group"] = "for example, switch Preserve Age on while brushing trees and the other ways of brushing vegetation in the same group are on too, while another group is not";
			d["item.decoration.ex.menu"] = "every asset in the Vegetation menu keeps it on, but moving to another menu it does not";
			d["item.decoration.ex.category"] = "switching to another kind of tree keeps it on, but the row only exists on assets with tree data, so brushing a decal has no such row at all (needs its own setting)";

			// ---------- Functions page (f.*): same panel rows, but grouped by the six function types ----------
			d["item.f.toolMode.desc"] = "Remembers the Tool Mode row on function panels. The options differ per tool: zone types offer Fill, Marquee and Paint; the area tools (District Creation Tool, Specialized Industry Area Tool, Surface area tool) offer Edit and Generate Map Grid; each terrain tool has its own modes; the bulldozer offers Main Elements, Sub Elements and Everything. The tool mode on the Assets page and on the Items page is remembered separately and none of them affects the others.";
			d["item.f.toolMode.ex.group"] = "for example, pick Paint for the residential zone and commercial, industrial and office zones are on Paint too, but the area tools are not";
			d["item.f.toolMode.ex.menu"] = "every tool inside one function group uses this same mode, while another function group does not";
			d["item.f.toolMode.ex.category"] = "switching to another function group falls back to that group's own mode (needs its own setting), even when both are reachable from the same toolbar menu";
			d["item.f.snap.desc"] = "Remembers every switch in the Snap row on function panels: quick align to right angle, quick align to grid, quick align to road, quick align to owner side, show contour lines and so on - which switches appear depends on the tool. The game's own Enable/Disable all snapping is a batch button and is not remembered. The snap row on the Assets page and on the Items page is remembered separately and none of them affects the others.";
			d["item.f.snap.ex.group"] = "for example, turn quick align to grid off for the residential zone and commercial, industrial and office zones have it off too, but the area tools do not";
			d["item.f.snap.ex.menu"] = "every tool inside one function group uses this same set of switches, while another function group does not";
			d["item.f.snap.ex.category"] = "switching to another function group goes back to that group's defaults (needs its own setting), even when both are reachable from the same toolbar menu";
			d["item.f.topography.desc"] = "Remembers the Contour Lines row on function panels, that is whether you see the terrain contours while drawing zones. This row is read and written per page: the Functions page keeps the zone copy, the Assets page keeps its own, and one never overwrites the other.";
			d["item.f.topography.ex.group"] = "for example, tick contour lines for the residential zone and commercial, industrial and office zones are ticked too";
			d["item.f.topography.ex.menu"] = "every tool inside one function group keeps them ticked, while another function group does not";
			d["item.f.topography.ex.category"] = "switching to another function group uses that group's own copy (needs its own setting); the Contour Lines row on the Assets page is a separate copy and is not affected";
			d["item.f.underground.desc"] = "Remembers the Underground Mode row on function panels. This row is read and written per page: the Functions page keeps the zone copy, the Assets and Items pages each keep their own, and none of them is shared.";
			d["item.f.underground.ex.group"] = "for example, switch underground mode on for the residential zone and commercial, industrial and office zones are underground too";
			d["item.f.underground.ex.menu"] = "every tool inside one function group stays underground, while another function group does not";
			d["item.f.underground.ex.category"] = "switching to another function group uses that group's own copy (needs its own setting); the Underground Mode row on the Assets page is a separate copy and is not affected";
			d["item.f.brush.desc"] = "Remembers the Brush Size and Brush Strength rows on function panels: Brush Size sets how wide one stroke reaches, Brush Strength sets how much each stroke changes. Only functions whose panel really has these rows are affected, that is the nine terrain tools (Shift Terrain, Soften Terrain, Level Terrain, Slope Terrain, Texture Paint 01-04 and Texture Eraser) and painting zones; functions without those rows are neither recorded nor changed.";
			d["item.f.brush.ex.group"] = "for example, set a 30 m brush on one terrain tool and the other terrain tools in the same group are 30 m too, while another group is not";
			d["item.f.brush.ex.menu"] = "every tool in the terrain group uses this same brush size and strength, while another function group does not";
			d["item.f.brush.ex.category"] = "switching to another function group uses that group's own brush (needs its own setting), even when both panels have these two rows";

			// ---------- Items page (i.*): the eight prop categories of the Landscaping menu ----------
			d["item.i.toolMode.desc"] = "Remembers the Tool Mode row on prop panels: Place single, Place multiple, Line, Curve, Object stamp tool, Paint and so on, depending on the tool. The tool mode on the Assets page and on the Functions page is remembered separately and none of them affects the others.";
			d["item.i.toolMode.ex.group"] = "for example, pick Place multiple for a Nature prop and the other props in the same group use Place multiple too, while another group does not";
			d["item.i.toolMode.ex.menu"] = "every prop in the Landscaping menu is on this mode, but moving to another menu it is not";
			d["item.i.toolMode.ex.category"] = "switching to another prop category, say from Nature to Decals, falls back to that category's own mode (needs its own setting), even though both are in the Landscaping menu";
			d["item.i.snap.desc"] = "Remembers every switch in the Snap row on prop panels: quick align to building surface, quick align to nearby shapes, quick align to road, remove matching types only and so on, depending on the prop. The snap row on the Assets page and on the Functions page is remembered separately and none of them affects the others.";
			d["item.i.snap.ex.group"] = "for example, turn quick align to building surface off for a Park prop and the other props in the same group have it off too, while another group does not";
			d["item.i.snap.ex.menu"] = "every prop in the Landscaping menu uses this same set of switches, but moving to another menu it does not";
			d["item.i.snap.ex.category"] = "switching to another prop category goes back to that category's defaults (needs its own setting), even though both are in the Landscaping menu";
			d["item.i.brush.desc"] = "Remembers the Brush Size and Brush Strength rows on prop panels, that is how wide a brush stroke reaches and how many props one stroke drops. Only props whose panel really has these rows are affected; props without them are neither recorded nor changed.";
			d["item.i.brush.ex.group"] = "for example, set a 20 m brush and the other brushable props in the same group are 20 m too, while another group is not";
			d["item.i.brush.ex.menu"] = "every brushable prop in the Landscaping menu uses this same brush, but moving to another menu it does not";
			d["item.i.brush.ex.category"] = "switching to another prop category uses that category's own brush (needs its own setting), even though both are in the Landscaping menu";
			d["item.i.topography.desc"] = "Remembers the Contour Lines row on prop panels. This row is read and written per page: the Items page keeps the prop copy, the Assets and Functions pages each keep their own, and none of them is shared.";
			d["item.i.topography.ex.group"] = "for example, tick contour lines while placing props and the other props in the same group are ticked too";
			d["item.i.topography.ex.menu"] = "every prop in the Landscaping menu keeps them ticked, but moving to another menu it does not";
			d["item.i.topography.ex.category"] = "switching to another prop category uses that category's own copy (needs its own setting); the Contour Lines rows on the Assets and Functions pages are separate copies and are not affected";
			d["item.i.underground.desc"] = "Remembers the Underground Mode row on prop panels, that is whether you place props below or above the ground. This row is read and written per page: the Items page keeps the prop copy, the Assets and Functions pages each keep their own, and none of them is shared.";
			d["item.i.underground.ex.group"] = "for example, switch underground mode on for a prop and the other props in the same group go underground too";
			d["item.i.underground.ex.menu"] = "every prop in the Landscaping menu goes underground, but moving to another menu it does not";
			d["item.i.underground.ex.category"] = "switching to another prop category uses that category's own copy (needs its own setting); the Underground Mode rows on the Assets and Functions pages are separate copies and are not affected";
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
