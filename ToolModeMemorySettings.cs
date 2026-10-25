using System;
using System.Collections.Generic;
using System.IO;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using ToolModeMemory.Memory;
using UnityEngine;

namespace ToolModeMemory
{
	/// <summary>
	/// Tool Mode Memory 选项页。
	///
	/// v0.7.0 结构（owner 需求 3 + 5）：**五个标签页**
	///   总开关  「启用模组」大开关 + 三块「启用资产/功能/道具的工具项记忆」
	///   资产    官方工具项设置（1..10）→ Anarchy工具项设置（1..3）
	///   功能    官方工具项设置（1..5）
	///   道具    官方工具项设置（1..6）
	///   关于    记忆管理 + 版本/作者/链接按钮（v0.6.0 起「兼容性」那一块已去掉：
	///           工具栏层级只有被其它模组改写过的这一份数据，那个开关关掉也一样按对方的分组记，
	///           留着只会误导）
	///
	/// 每一项 = 一个「是否恢复」开关 + 一个「共用范围」下拉，五种范围的定义写在
	/// 每一项自己的说明里；序号在每个标签页内各自从 1 开始（三页同名项是三条独立设置）。
	///
	/// 三层闸门（都是「恢复」闸门，记录只受大开关限制 —— 需求 10）：
	///   启用模组 m_Enabled         关 = 停止记录 + 完全原版行为
	///   资产/功能/道具三块开关       关 = 该域所有项不再恢复（仍在记录）
	///   每一项自己的开关            关 = 这一项不再恢复（仍在记录）
	/// 大开关的「自动反映」：打开任意一块域开关 → 大开关自动开；三块全关 → 大开关自动关
	/// （owner 需求 3）。这个反映只在玩家手动改动时生效（Ready 之后），
	/// 读盘期间不反映，否则「大开关=关 + 资产=开」这种玩家手动组合会被反序列化顺序改写。
	///
	/// 变灰用 DisableByCondition（框架只在构建该页那一刻求值一次，见 research/GameDump
	/// Game.UI.Menu/AutomaticSettings.cs 的 BuildTab + Widget.UpdateBase），
	/// Anarchy 那三行额外挂 HideByCondition（每帧重算，没装 Anarchy 就整块不出现，理由见下面的注释）。
	/// 全部改动实时生效（setter 内 Sync + Persist）。
	/// </summary>
	[FileLocation(nameof(ToolModeMemory))]
	[SettingsUITabOrder(kTabMaster, kTabAssets, kTabFunctions, kTabItems, kTabAbout)]
	[SettingsUIGroupOrder(kGroupMaster, kGroupDomain, kGroupAssetOfficial, kGroupAssetAnarchy,
		kGroupFunctionOfficial, kGroupFunctionAnarchy, kGroupItemOfficial, kGroupItemAnarchy,
		kGroupMemory, kGroupInfo)]
	[SettingsUIShowGroupName(kGroupDomain, kGroupAssetOfficial, kGroupAssetAnarchy,
		kGroupFunctionOfficial, kGroupFunctionAnarchy, kGroupItemOfficial, kGroupItemAnarchy, kGroupMemory)]
	public class ToolModeMemorySettings : ModSetting
	{
		public const string kTabMaster = "Master";
		public const string kTabAssets = "Assets";
		public const string kTabFunctions = "Functions";
		public const string kTabItems = "Items";
		public const string kTabAbout = "About";

		/// <summary>大开关板块：按用户要求不显示板块名（不在 ShowGroupName 里）。</summary>
		public const string kGroupMaster = "MasterSwitch";
		/// <summary>三块「启用 X 的工具项记忆」：显示板块名，让 owner 要的小标题成行。</summary>
		public const string kGroupDomain = "DomainSwitches";
		/// <summary>资产页：官方工具项板块。</summary>
		public const string kGroupAssetOfficial = "AssetOfficialItems";
		/// <summary>资产页：第三方板块，一个模组一块，板块名 = 模组名 + 「工具项设置」。</summary>
		public const string kGroupAssetAnarchy = "AssetAnarchyItems";
		public const string kGroupFunctionOfficial = "FunctionOfficialItems";
		/// <summary>
		/// 功能页：第三方板块（v0.10.0 新增）。owner：「功能页怎么没有 anarchy 板块？
		/// 像地形改造这种功能还是能设置 anarchy 的」。
		/// 板块名里带 Anarchy ⇒ 组标题走 locale 的 group.anarchy（「Anarchy工具项设置」），不必新加键。
		/// </summary>
		public const string kGroupFunctionAnarchy = "FunctionAnarchyItems";
		public const string kGroupItemOfficial = "ItemOfficialItems";
		/// <summary>道具页：第三方板块（v0.8.0 新增，Anarchy 的物件高度与主开关在摆件工具上出现）。</summary>
		public const string kGroupItemAnarchy = "ItemAnarchyItems";
		public const string kGroupMemory = "MemoryFiles";
		// v0.6.0 删掉了「与其他模组兼容」那一整块（原 kGroupCompat）：工具栏的菜单/分组在运行期内
		// 只有被其它模组就地改写过的这一份数据，没有「原版那份」可回退，所以那个开关关掉也一样
		// 按对方调整后的分组记（owner 实机反馈 + UIObjectData 只有一份的事实）。
		// 整行去掉，永远跟随当前 UI 层级。
		/// <summary>关于页底部信息板块（不显示板块名）。</summary>
		public const string kGroupInfo = "AboutInfo";

		public static ToolModeMemorySettings Instance;

		/// <summary>
		/// LoadSettings 是用属性 setter 反序列化的；在配置读入完成前必须屏蔽
		/// Sync/Persist，也必须屏蔽「大开关自动反映」，否则开局就会触发几十次设置落盘任务，
		/// 而且反序列化顺序会改写玩家手动留下的「大开关关 + 某域开」组合。
		/// </summary>
		public static bool Ready;

		// v0.9.0 owner 需求 1：总开关默认关、三个分开关默认开。
		// 这个字段值必须与 SetDefaults() 一致 —— 没有存档文件时没人调用 SetDefaults()，
		// 出厂默认靠的就是这里的字段初始值（见下面构造函数注释）。
		private bool m_Enabled = false;
		/// <summary>三块域开关（= 三个标签页的总闸）。默认都开。</summary>
		private readonly bool[] m_DomainEnabled = new bool[] { true, true, true };
		private readonly Dictionary<string, bool> m_ItemEnabled = new Dictionary<string, bool>(StringComparer.Ordinal);
		private readonly Dictionary<string, int> m_ItemScope = new Dictionary<string, int>(StringComparer.Ordinal);

		/// <summary>
		/// 模组设置不走 SharedSettings.Reset()：框架只把「当前实例」交给
		/// AssetDatabase.LoadSettings(name, obj, defaultObj)（AssetDatabase.cs:611），
		/// 没有存档文件时没人会调用 SetDefaults()。所以出厂默认必须在构造函数里就落好，
		/// 这也让「总开关默认关、分开关默认开」真的成立（原版 GameplaySettings 的写法一致）。
		/// </summary>
		public ToolModeMemorySettings(IMod mod) : base(mod)
		{
			SetDefaults();
		}

		private void InitItemDefaults()
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				// v0.9.2：按工具类别拆出来的行（toolMode.net …）**不预填**。
				// 预填会让它们带着出厂值进字典，老存档里玩家给「工具模式」设过的开关与范围
				// 就再也传不下来（GetEnabled / GetScope 的继承分支永远查不到「没记录」）。
				// 留空之后，没记录时由那两个方法先沿用基项、再退回推荐档，两边都对。
				if (!string.IsNullOrEmpty(items[i].TextId)) continue;
				m_ItemEnabled[items[i].Id] = items[i].DefaultEnabled;
				m_ItemScope[items[i].Id] = items[i].EffectiveRecommendedScope();
			}
		}

		// ==================== 总开关页 ====================

		/// <summary>
		/// 「启用模组」。关闭 = 停止记录 + 停止任何写回 = 完全原版行为，
		/// 且下面所有标签页的每一行都变灰不可点。默认打开。
		/// </summary>
		[SettingsUISection(kTabMaster, kGroupMaster)]
		public bool Enabled
		{
			get { return m_Enabled; }
			set
			{
				m_Enabled = value;
				Sync();
				Persist();
			}
		}

		/// <summary>三块域开关的索引（= ItemDomain 的枚举值）。</summary>
		private static int DomainIndex(ItemDomain domain)
		{
			switch (domain)
			{
				case ItemDomain.Function: return 1;
				case ItemDomain.Item: return 2;
				default: return 0;
			}
		}

		private bool GetDomain(ItemDomain domain)
		{
			return m_DomainEnabled[DomainIndex(domain)];
		}

		/// <summary>
		/// 域开关被玩家改动：先记下，再「自动反映」大开关（owner 需求 3：
		/// 三块全关 → 大开关自动关；打开任意一块 → 大开关自动开）。
		/// 读盘期间（Ready==false）不反映，理由见 <see cref="Ready"/>。
		/// </summary>
		private void SetDomain(ItemDomain domain, bool value)
		{
			m_DomainEnabled[DomainIndex(domain)] = value;
			if (Ready)
			{
				m_Enabled = ReflectMaster();
			}
			Sync();
			Persist();
		}

		/// <summary>三块里是否还有开着的：三块全关 → 大开关关；任何一块开着 → 大开关开。</summary>
		private bool ReflectMaster()
		{
			return m_DomainEnabled[0] || m_DomainEnabled[1] || m_DomainEnabled[2];
		}

		[SettingsUISection(kTabMaster, kGroupDomain)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsDomainDisabled))]
		public bool AssetMemoryEnabled { get { return GetDomain(ItemDomain.Asset); } set { SetDomain(ItemDomain.Asset, value); } }

		[SettingsUISection(kTabMaster, kGroupDomain)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsDomainDisabled))]
		public bool FunctionMemoryEnabled { get { return GetDomain(ItemDomain.Function); } set { SetDomain(ItemDomain.Function, value); } }

		[SettingsUISection(kTabMaster, kGroupDomain)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsDomainDisabled))]
		public bool ItemMemoryEnabled { get { return GetDomain(ItemDomain.Item); } set { SetDomain(ItemDomain.Item, value); } }

		/// <summary>
		/// 三块域开关自己什么时候变灰：大开关关着的时候。
		/// 大开关开着时它们永远可点（否则「三块全关」之后再也打不开就死锁了）。
		/// </summary>
		public bool IsDomainDisabled()
		{
			return !m_Enabled;
		}

		public bool IsMasterOff()
		{
			return !m_Enabled;
		}

		/// <summary>某一页的官方板块是否整体变灰：大开关关 或 该域开关关。</summary>
		public bool IsAssetSectionOff() { return IsMasterOff() || !GetDomain(ItemDomain.Asset); }
		public bool IsFunctionSectionOff() { return IsMasterOff() || !GetDomain(ItemDomain.Function); }
		public bool IsItemSectionOff() { return IsMasterOff() || !GetDomain(ItemDomain.Item); }

		// ==================== 资产页：官方工具项（1..10） ====================
		//
		// 每一项两行：是否恢复 + 共用范围。属性名 = ToolItemDef.PascalId() + Enabled / Scope，
		// 语言表按同一规则注册词条（LocaleTable.AddItems），所以目录里加一项不用改这里以外的地方。
		// 域变灰判据 IsAssetSectionOff 之后还要叠「本项自己没开」，见下面 IsXxxScopeDisabled。

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ThemesEnabled { get { return GetEnabled(ToolItemCatalog.kThemes); } set { SetEnabled(ToolItemCatalog.kThemes, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetThemesScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsThemesScopeDisabled))]
		public int ThemesScope { get { return GetScope(ToolItemCatalog.kThemes); } set { SetScope(ToolItemCatalog.kThemes, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool PacksEnabled { get { return GetEnabled(ToolItemCatalog.kPacks); } set { SetEnabled(ToolItemCatalog.kPacks, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetPacksScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsPacksScopeDisabled))]
		public int PacksScope { get { return GetScope(ToolItemCatalog.kPacks); } set { SetScope(ToolItemCatalog.kPacks, value); } }

		// v0.9.2：工具模式 / 对齐按工具类别拆行（道路与建筑的可选值不同，不能共用一个下拉）。
		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ToolModeNetEnabled { get { return GetEnabled(ToolItemCatalog.kToolModeNet); } set { SetEnabled(ToolItemCatalog.kToolModeNet, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetToolModeNetScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsToolModeNetScopeDisabled))]
		public int ToolModeNetScope { get { return GetScope(ToolItemCatalog.kToolModeNet); } set { SetScope(ToolItemCatalog.kToolModeNet, value); } }
		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ToolModeObjEnabled { get { return GetEnabled(ToolItemCatalog.kToolModeObj); } set { SetEnabled(ToolItemCatalog.kToolModeObj, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetToolModeObjScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsToolModeObjScopeDisabled))]
		public int ToolModeObjScope { get { return GetScope(ToolItemCatalog.kToolModeObj); } set { SetScope(ToolItemCatalog.kToolModeObj, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ElevationEnabled { get { return GetEnabled(ToolItemCatalog.kElevation); } set { SetEnabled(ToolItemCatalog.kElevation, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetElevationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsElevationScopeDisabled))]
		public int ElevationScope { get { return GetScope(ToolItemCatalog.kElevation); } set { SetScope(ToolItemCatalog.kElevation, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ParallelEnabled { get { return GetEnabled(ToolItemCatalog.kParallel); } set { SetEnabled(ToolItemCatalog.kParallel, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetParallelScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsParallelScopeDisabled))]
		public int ParallelScope { get { return GetScope(ToolItemCatalog.kParallel); } set { SetScope(ToolItemCatalog.kParallel, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool SnapNetEnabled { get { return GetEnabled(ToolItemCatalog.kSnapNet); } set { SetEnabled(ToolItemCatalog.kSnapNet, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetSnapNetScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsSnapNetScopeDisabled))]
		public int SnapNetScope { get { return GetScope(ToolItemCatalog.kSnapNet); } set { SetScope(ToolItemCatalog.kSnapNet, value); } }
		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool SnapObjEnabled { get { return GetEnabled(ToolItemCatalog.kSnapObj); } set { SetEnabled(ToolItemCatalog.kSnapObj, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetSnapObjScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsSnapObjScopeDisabled))]
		public int SnapObjScope { get { return GetScope(ToolItemCatalog.kSnapObj); } set { SetScope(ToolItemCatalog.kSnapObj, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool TopographyEnabled { get { return GetEnabled(ToolItemCatalog.kTopography); } set { SetEnabled(ToolItemCatalog.kTopography, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetTopographyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsTopographyScopeDisabled))]
		public int TopographyScope { get { return GetScope(ToolItemCatalog.kTopography); } set { SetScope(ToolItemCatalog.kTopography, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool UndergroundEnabled { get { return GetEnabled(ToolItemCatalog.kUnderground); } set { SetEnabled(ToolItemCatalog.kUnderground, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetUndergroundScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsUndergroundScopeDisabled))]
		public int UndergroundScope { get { return GetScope(ToolItemCatalog.kUnderground); } set { SetScope(ToolItemCatalog.kUnderground, value); } }

		// v0.8.0：原「笔刷」一项已拆成粗细/强度两项并移到功能页与道具页
		// （原版只有地形工具与摆件刷涂有这两行，建筑与服务页没有 → 放这里就是「设了没反应」）。
		// v0.8.0：配色（color）已移到功能页 —— 只有路线工具显示这一行。

		// ==================== 建筑与服务页：Anarchy 工具项 ====================
		//
		// 这三行挂的是 **HideByCondition**，不是 DisableByCondition。原因（反编译 Game.dll 核实）：
		// 变灰那条路只在「设置页构建的那一刻」求值一次 —— AutomaticSettings.BuildTab 里
		// 逐个 widget.Update()，而 Widget.UpdateBase() 才读 disabled()；游戏每帧只做
		// Page.UpdateVisibility / UpdateNameAndDescription / UpdateWarning
		// （OptionsUISystem.OnUpdate，且只有停在 Options 画面才跑），没有第二次 disabled()。
		// 本模组的 RegisterInOptionsUI 又发生在 OnLoad，比 Anarchy 的程序集进 AppDomain
		// 早约 2 秒（玩家实测日志），拿「没装 Anarchy」当禁用条件就会把这三行**永久冻灰**，
		// 连装了 Anarchy 的玩家也点不动 —— 那是比现状（能点、静默 no-op）更糟的回归。
		// hidden() 走的 UpdateVisibility() 是每帧重算的，所以隐藏判据会随 Anarchy 判定落地
		// 当场生效：没装 = 整块消失，装了 = 自动回来。判定未落地前一律不隐藏（见 ToolMemoryBridge）。
		//
		// Anarchy 板块的归属（v0.8.0 按 research/round4/anarchy_rows.md 重建）：
		//  * 主开关 / 左侧右侧 / 常规 —— 只有**道路工具**有（NetworkAnarchyUISystem 订的就是 NetToolSystem）；
		//  * 物件高度 —— 只有**摆件工具**有（AnarchyUISystem 注入在 Object Tool 面板上），
		//    所以这一项在建筑与服务页、道具页各有一份（owner 定的三页互不共用）。

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool AnarchyEnabled { get { return GetEnabled(ToolItemCatalog.kAnarchy); } set { SetEnabled(ToolItemCatalog.kAnarchy, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetAnarchyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int AnarchyScope { get { return GetScope(ToolItemCatalog.kAnarchy); } set { SetScope(ToolItemCatalog.kAnarchy, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool LeftRightEnabled { get { return GetEnabled(ToolItemCatalog.kLeftRight); } set { SetEnabled(ToolItemCatalog.kLeftRight, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetLeftRightScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsLeftRightScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int LeftRightScope { get { return GetScope(ToolItemCatalog.kLeftRight); } set { SetScope(ToolItemCatalog.kLeftRight, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool GeneralEnabled { get { return GetEnabled(ToolItemCatalog.kGeneral); } set { SetEnabled(ToolItemCatalog.kGeneral, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetGeneralScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsGeneralScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int GeneralScope { get { return GetScope(ToolItemCatalog.kGeneral); } set { SetScope(ToolItemCatalog.kGeneral, value); } }

		/// <summary>
		/// 物件高度（Anarchy 注入在摆件工具面板上的那一排：锁定变换 / 高度 / 高度变化范围 /
		/// 抬高降低的幅度）。owner 2026-10-20：这 4 项同一排显示，算同一项、一起记忆和共用；
		/// 「重置高度为 0」是瞬时按钮，不列。
		/// 判据用 IsAnarchyMissing：没装 Anarchy 时整块隐藏（Hide 每帧重算，见上面的长注释）。
		/// </summary>
		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool AnarchyObjectElevationEnabled { get { return GetEnabled(ToolItemCatalog.kObjectElevation); } set { SetEnabled(ToolItemCatalog.kObjectElevation, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetAnarchyObjectElevationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyObjectElevationScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int AnarchyObjectElevationScope { get { return GetScope(ToolItemCatalog.kObjectElevation); } set { SetScope(ToolItemCatalog.kObjectElevation, value); } }

		// ==================== 功能页：官方工具项（1..12） ====================

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FThemesEnabled { get { return GetEnabled(ToolItemCatalog.kFThemes); } set { SetEnabled(ToolItemCatalog.kFThemes, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFThemesScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFThemesScopeDisabled))]
		public int FThemesScope { get { return GetScope(ToolItemCatalog.kFThemes); } set { SetScope(ToolItemCatalog.kFThemes, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FPacksEnabled { get { return GetEnabled(ToolItemCatalog.kFPacks); } set { SetEnabled(ToolItemCatalog.kFPacks, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFPacksScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFPacksScopeDisabled))]
		public int FPacksScope { get { return GetScope(ToolItemCatalog.kFPacks); } set { SetScope(ToolItemCatalog.kFPacks, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FToolModeEnabled { get { return GetEnabled(ToolItemCatalog.kFToolMode); } set { SetEnabled(ToolItemCatalog.kFToolMode, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFToolModeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFToolModeScopeDisabled))]
		public int FToolModeScope { get { return GetScope(ToolItemCatalog.kFToolMode); } set { SetScope(ToolItemCatalog.kFToolMode, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FSnapZoneEnabled { get { return GetEnabled(ToolItemCatalog.kFSnapZone); } set { SetEnabled(ToolItemCatalog.kFSnapZone, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFSnapZoneScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFSnapZoneScopeDisabled))]
		public int FSnapZoneScope { get { return GetScope(ToolItemCatalog.kFSnapZone); } set { SetScope(ToolItemCatalog.kFSnapZone, value); } }
		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FSnapAreaEnabled { get { return GetEnabled(ToolItemCatalog.kFSnapArea); } set { SetEnabled(ToolItemCatalog.kFSnapArea, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFSnapAreaScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFSnapAreaScopeDisabled))]
		public int FSnapAreaScope { get { return GetScope(ToolItemCatalog.kFSnapArea); } set { SetScope(ToolItemCatalog.kFSnapArea, value); } }
		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FSnapObjEnabled { get { return GetEnabled(ToolItemCatalog.kFSnapObj); } set { SetEnabled(ToolItemCatalog.kFSnapObj, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFSnapObjScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFSnapObjScopeDisabled))]
		public int FSnapObjScope { get { return GetScope(ToolItemCatalog.kFSnapObj); } set { SetScope(ToolItemCatalog.kFSnapObj, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FUndergroundEnabled { get { return GetEnabled(ToolItemCatalog.kFUnderground); } set { SetEnabled(ToolItemCatalog.kFUnderground, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFUndergroundScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFUndergroundScopeDisabled))]
		public int FUndergroundScope { get { return GetScope(ToolItemCatalog.kFUnderground); } set { SetScope(ToolItemCatalog.kFUnderground, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FTopographyEnabled { get { return GetEnabled(ToolItemCatalog.kFTopography); } set { SetEnabled(ToolItemCatalog.kFTopography, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFTopographyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFTopographyScopeDisabled))]
		public int FTopographyScope { get { return GetScope(ToolItemCatalog.kFTopography); } set { SetScope(ToolItemCatalog.kFTopography, value); } }

		// v0.8.0 拆行：笔刷类型 / 笔刷粗细 / 笔刷强度 / 目标高度 各自一项、各配一套开关与范围。
		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FBrushTypeEnabled { get { return GetEnabled(ToolItemCatalog.kFBrushType); } set { SetEnabled(ToolItemCatalog.kFBrushType, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFBrushTypeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFBrushTypeScopeDisabled))]
		public int FBrushTypeScope { get { return GetScope(ToolItemCatalog.kFBrushType); } set { SetScope(ToolItemCatalog.kFBrushType, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FBrushSizeEnabled { get { return GetEnabled(ToolItemCatalog.kFBrushSize); } set { SetEnabled(ToolItemCatalog.kFBrushSize, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFBrushSizeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFBrushSizeScopeDisabled))]
		public int FBrushSizeScope { get { return GetScope(ToolItemCatalog.kFBrushSize); } set { SetScope(ToolItemCatalog.kFBrushSize, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FBrushStrengthEnabled { get { return GetEnabled(ToolItemCatalog.kFBrushStrength); } set { SetEnabled(ToolItemCatalog.kFBrushStrength, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFBrushStrengthScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFBrushStrengthScopeDisabled))]
		public int FBrushStrengthScope { get { return GetScope(ToolItemCatalog.kFBrushStrength); } set { SetScope(ToolItemCatalog.kFBrushStrength, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FBrushHeightEnabled { get { return GetEnabled(ToolItemCatalog.kFBrushHeight); } set { SetEnabled(ToolItemCatalog.kFBrushHeight, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFBrushHeightScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFBrushHeightScopeDisabled))]
		public int FBrushHeightScope { get { return GetScope(ToolItemCatalog.kFBrushHeight); } set { SetScope(ToolItemCatalog.kFBrushHeight, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FDistanceEnabled { get { return GetEnabled(ToolItemCatalog.kFDistance); } set { SetEnabled(ToolItemCatalog.kFDistance, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFDistanceScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFDistanceScopeDisabled))]
		public int FDistanceScope { get { return GetScope(ToolItemCatalog.kFDistance); } set { SetScope(ToolItemCatalog.kFDistance, value); } }

		/// <summary>道路配色：只有路线工具显示这一行（IColored 唯一实现者是 RoutePrefab）。</summary>
		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FColorEnabled { get { return GetEnabled(ToolItemCatalog.kFColor); } set { SetEnabled(ToolItemCatalog.kFColor, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFColorScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFColorScopeDisabled))]
		public int FColorScope { get { return GetScope(ToolItemCatalog.kFColor); } set { SetScope(ToolItemCatalog.kFColor, value); } }

		// ==================== 功能页：第三方工具项（15..16，v0.10.0） ====================
		//
		// owner：「功能页怎么没有 anarchy 板块？像地形改造这种功能还是能设置 anarchy 的。」
		// 证据（research/round4/anarchy_rows.md 门 B）：Anarchy 主开关只对 m_ToolIDs 白名单
		// {Object, Net, Area, Bulldoze, Terrain, Upgrade, Line} 显示。落在本页的是
		// 地形改造 / 区域工具 / 拆除 / 标记与预制对象；功能区、路线、水体没有这一段，
		// 所以本行对它们不生效（运行期门槛见 ToolMemoryBridge.AnarchyToggleVisible）。
		// 文案复用建筑与服务页那一份（TextId = kAnarchy / kObjectElevation），12 语言不需要新键。

		[SettingsUISection(kTabFunctions, kGroupFunctionAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool FAnarchyEnabled { get { return GetEnabled(ToolItemCatalog.kFAnarchy); } set { SetEnabled(ToolItemCatalog.kFAnarchy, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFAnarchyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFAnarchyScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int FAnarchyScope { get { return GetScope(ToolItemCatalog.kFAnarchy); } set { SetScope(ToolItemCatalog.kFAnarchy, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool FAnarchyObjectElevationEnabled { get { return GetEnabled(ToolItemCatalog.kFAnarchyObjectElevation); } set { SetEnabled(ToolItemCatalog.kFAnarchyObjectElevation, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFAnarchyObjectElevationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFAnarchyObjectElevationScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int FAnarchyObjectElevationScope { get { return GetScope(ToolItemCatalog.kFAnarchyObjectElevation); } set { SetScope(ToolItemCatalog.kFAnarchyObjectElevation, value); } }

		// ==================== 道具页：官方工具项（1..12） ====================
		//
		// v0.8.0 按 research/round4/vanilla_rows.md「Object Tool」一列把面板行列全：
		// 主题 / 数据包 / 工具模式 / 距离 / 笔刷种类 / 笔刷粗细 / 笔刷强度 / 年龄 / 保留年代 /
		// 对齐 / 地形 / 地下模式。**没有「高度」** —— 玩家在道具页看到的高度与高度变化范围
		// 其实是 Anarchy 注入的那一排，见本页 Anarchy 板块的「物件高度」。

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IThemesEnabled { get { return GetEnabled(ToolItemCatalog.kIThemes); } set { SetEnabled(ToolItemCatalog.kIThemes, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIThemesScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIThemesScopeDisabled))]
		public int IThemesScope { get { return GetScope(ToolItemCatalog.kIThemes); } set { SetScope(ToolItemCatalog.kIThemes, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IPacksEnabled { get { return GetEnabled(ToolItemCatalog.kIPacks); } set { SetEnabled(ToolItemCatalog.kIPacks, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIPacksScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIPacksScopeDisabled))]
		public int IPacksScope { get { return GetScope(ToolItemCatalog.kIPacks); } set { SetScope(ToolItemCatalog.kIPacks, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IToolModeEnabled { get { return GetEnabled(ToolItemCatalog.kIToolMode); } set { SetEnabled(ToolItemCatalog.kIToolMode, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIToolModeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIToolModeScopeDisabled))]
		public int IToolModeScope { get { return GetScope(ToolItemCatalog.kIToolMode); } set { SetScope(ToolItemCatalog.kIToolMode, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IDistanceEnabled { get { return GetEnabled(ToolItemCatalog.kIDistance); } set { SetEnabled(ToolItemCatalog.kIDistance, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIDistanceScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIDistanceScopeDisabled))]
		public int IDistanceScope { get { return GetScope(ToolItemCatalog.kIDistance); } set { SetScope(ToolItemCatalog.kIDistance, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IBrushTypeEnabled { get { return GetEnabled(ToolItemCatalog.kIBrushType); } set { SetEnabled(ToolItemCatalog.kIBrushType, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIBrushTypeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIBrushTypeScopeDisabled))]
		public int IBrushTypeScope { get { return GetScope(ToolItemCatalog.kIBrushType); } set { SetScope(ToolItemCatalog.kIBrushType, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IBrushSizeEnabled { get { return GetEnabled(ToolItemCatalog.kIBrushSize); } set { SetEnabled(ToolItemCatalog.kIBrushSize, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIBrushSizeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIBrushSizeScopeDisabled))]
		public int IBrushSizeScope { get { return GetScope(ToolItemCatalog.kIBrushSize); } set { SetScope(ToolItemCatalog.kIBrushSize, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IBrushStrengthEnabled { get { return GetEnabled(ToolItemCatalog.kIBrushStrength); } set { SetEnabled(ToolItemCatalog.kIBrushStrength, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIBrushStrengthScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIBrushStrengthScopeDisabled))]
		public int IBrushStrengthScope { get { return GetScope(ToolItemCatalog.kIBrushStrength); } set { SetScope(ToolItemCatalog.kIBrushStrength, value); } }

		/// <summary>年龄：只有树（带 TreeData 的资产）在游戏模式下才有这一行。</summary>
		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IAgeMaskEnabled { get { return GetEnabled(ToolItemCatalog.kIAgeMask); } set { SetEnabled(ToolItemCatalog.kIAgeMask, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIAgeMaskScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIAgeMaskScopeDisabled))]
		public int IAgeMaskScope { get { return GetScope(ToolItemCatalog.kIAgeMask); } set { SetScope(ToolItemCatalog.kIAgeMask, value); } }

		/// <summary>保留年代：与年龄同门，只有树才有这一行。</summary>
		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IDecorationEnabled { get { return GetEnabled(ToolItemCatalog.kIDecoration); } set { SetEnabled(ToolItemCatalog.kIDecoration, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIDecorationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIDecorationScopeDisabled))]
		public int IDecorationScope { get { return GetScope(ToolItemCatalog.kIDecoration); } set { SetScope(ToolItemCatalog.kIDecoration, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool ISnapEnabled { get { return GetEnabled(ToolItemCatalog.kISnap); } set { SetEnabled(ToolItemCatalog.kISnap, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetISnapScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsISnapScopeDisabled))]
		public int ISnapScope { get { return GetScope(ToolItemCatalog.kISnap); } set { SetScope(ToolItemCatalog.kISnap, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool ITopographyEnabled { get { return GetEnabled(ToolItemCatalog.kITopography); } set { SetEnabled(ToolItemCatalog.kITopography, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetITopographyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsITopographyScopeDisabled))]
		public int ITopographyScope { get { return GetScope(ToolItemCatalog.kITopography); } set { SetScope(ToolItemCatalog.kITopography, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IUndergroundEnabled { get { return GetEnabled(ToolItemCatalog.kIUnderground); } set { SetEnabled(ToolItemCatalog.kIUnderground, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIUndergroundScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIUndergroundScopeDisabled))]
		public int IUndergroundScope { get { return GetScope(ToolItemCatalog.kIUnderground); } set { SetScope(ToolItemCatalog.kIUnderground, value); } }

		// ==================== 道具页：Anarchy 工具项（13..14） ====================

		[SettingsUISection(kTabItems, kGroupItemAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool IAnarchyEnabled { get { return GetEnabled(ToolItemCatalog.kIAnarchy); } set { SetEnabled(ToolItemCatalog.kIAnarchy, value); } }

		[SettingsUISection(kTabItems, kGroupItemAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIAnarchyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIAnarchyScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int IAnarchyScope { get { return GetScope(ToolItemCatalog.kIAnarchy); } set { SetScope(ToolItemCatalog.kIAnarchy, value); } }

		/// <summary>
		/// 物件高度（道具页的那一份）：与建筑与服务页同一排 Anarchy 控件，但按 owner 定的
		/// 「三页互不共用」独立记忆 —— 在道具页调的高度不会覆盖建筑与服务页的。
		/// </summary>
		[SettingsUISection(kTabItems, kGroupItemAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public bool IAnarchyObjectElevationEnabled { get { return GetEnabled(ToolItemCatalog.kIObjectElevation); } set { SetEnabled(ToolItemCatalog.kIObjectElevation, value); } }

		[SettingsUISection(kTabItems, kGroupItemAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIAnarchyObjectElevationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIAnarchyObjectElevationScopeDisabled))]
		[SettingsUIHideByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyMissing))]
		public int IAnarchyObjectElevationScope { get { return GetScope(ToolItemCatalog.kIObjectElevation); } set { SetScope(ToolItemCatalog.kIObjectElevation, value); } }

		// ---------- 每项的共用范围行是否可点：本域没开 或 本项没开 ----------

		public bool IsThemesScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kThemes); }

		public bool IsPacksScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kPacks); }

		public bool IsToolModeNetScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kToolModeNet); }
		public bool IsToolModeObjScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kToolModeObj); }

		public bool IsElevationScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kElevation); }

		public bool IsParallelScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kParallel); }

		public bool IsSnapNetScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kSnapNet); }
		public bool IsSnapObjScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kSnapObj); }

		public bool IsTopographyScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kTopography); }

		public bool IsUndergroundScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kUnderground); }

		public bool IsAnarchyScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kAnarchy); }

		public bool IsLeftRightScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kLeftRight); }

		public bool IsGeneralScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kGeneral); }

		public bool IsAnarchyObjectElevationScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kObjectElevation); }

		public bool IsFThemesScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFThemes); }

		public bool IsFPacksScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFPacks); }

		public bool IsFToolModeScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFToolMode); }

		public bool IsFSnapZoneScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFSnapZone); }
		public bool IsFSnapAreaScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFSnapArea); }
		public bool IsFSnapObjScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFSnapObj); }

		public bool IsFUndergroundScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFUnderground); }

		public bool IsFTopographyScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFTopography); }

		public bool IsFBrushTypeScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFBrushType); }

		public bool IsFBrushSizeScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFBrushSize); }

		public bool IsFBrushStrengthScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFBrushStrength); }

		public bool IsFBrushHeightScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFBrushHeight); }

		public bool IsFDistanceScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFDistance); }

		public bool IsFColorScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFColor); }
		public bool IsFAnarchyScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFAnarchy); }
		public bool IsFAnarchyObjectElevationScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFAnarchyObjectElevation); }

		public bool IsIThemesScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIThemes); }

		public bool IsIPacksScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIPacks); }

		public bool IsIToolModeScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIToolMode); }

		public bool IsIDistanceScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIDistance); }

		public bool IsIBrushTypeScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIBrushType); }

		public bool IsIBrushSizeScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIBrushSize); }

		public bool IsIBrushStrengthScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIBrushStrength); }

		public bool IsIAgeMaskScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIAgeMask); }

		public bool IsIDecorationScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIDecoration); }

		public bool IsISnapScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kISnap); }

		public bool IsITopographyScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kITopography); }

		public bool IsIUndergroundScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIUnderground); }

		public bool IsIAnarchyScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIAnarchy); }

		public bool IsIAnarchyObjectElevationScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIObjectElevation); }

		/// <summary>
		/// Anarchy 那三行（共 6 行）的隐藏判据：玩家没装 / 没启用 Anarchy 时整块不出现。
		/// 判据本身在 <see cref="ToolMemoryBridge.AnarchyConfirmedAbsent"/>：
		/// 只有「扫过已加载的程序集、确实找不到 Anarchy 的两个 UI 类型，并且启动宽限期也过了」
		/// 才算没装；拿不准（Anarchy 还在加载、世界没建好、扫描抛异常）一律返回 false = 照旧显示。
		/// 这条判据被选项页每帧调用（只在停在 Options 画面时），所以它自己必须既不抛也不做重活。
		/// </summary>
		public bool IsAnarchyMissing()
		{
			try { return ToolMemoryBridge.AnarchyConfirmedAbsent(); }
			catch { return false; }
		}

		// ==================== 关于页：记忆管理（v0.7.0 未改动） ====================

		[SettingsUISection(kTabAbout, kGroupMemory)]
		[SettingsUIButton]
		[SettingsUIConfirmation("CONFIRM_RESET", null)]
		public bool ResetMemory
		{
			set { DoResetMemory(); }
		}

		/// <summary>需求 1：按钮文本改为「管理所有存档的记忆文件」，行为不变（打开记忆目录）。</summary>
		[SettingsUISection(kTabAbout, kGroupMemory)]
		[SettingsUIButton]
		public bool OpenMemoryFolder
		{
			set { DoOpenMemoryFolder(); }
		}

		/// <summary>需求 1：新增「重置所有设置项」，把本模组的所有设置退回推荐值。</summary>
		[SettingsUISection(kTabAbout, kGroupMemory)]
		[SettingsUIButton]
		[SettingsUIConfirmation("CONFIRM_RESET_SETTINGS", null)]
		public bool ResetAllSettings
		{
			set { DoResetAllSettings(); }
		}

		/// <summary>
		/// 读盘完成（Ready 已置真）后调用一次：LoadSettings 是用属性 setter 反序列化的，
		/// 期间 Sync 被屏蔽，所以这里补一次，让文件里的设置真的作用到运行中的工具上。
		/// </summary>
		public void AfterLoaded()
		{
			Sync();
		}

		// ---------- 关于页：版本 / 作者 / 链接 ----------

		// 注意：这里不能加 [SettingsUIMultilineText]。AutomaticSettings 给 MultilineText
		// 建的 widget 只有 label，没有 accessor（Game.UI.Menu.AutomaticSettings.cs:17-28），
		// 值永远不显示；去掉该属性才会走 StringField 读到值。
		[SettingsUISection(kTabAbout, kGroupInfo)]
		public string ModVersion
		{
			get { return ToolModeMemoryMod.kVersion; }
		}

		[SettingsUISection(kTabAbout, kGroupInfo)]
		public string ModAuthor
		{
			get { return "yuexian"; }
		}

		// 需求 1：三个链接按钮排成一行（框架支持：SettingsUIButtonGroup -> ButtonRow 横向）。
		[SettingsUISection(kTabAbout, kGroupInfo)]
		[SettingsUIButton]
		[SettingsUIButtonGroup("aboutLinks")]
		public bool OpenKofi
		{
			set { OpenUrl("https://ko-fi.com/yuexian7"); }
		}

		[SettingsUISection(kTabAbout, kGroupInfo)]
		[SettingsUIButton]
		[SettingsUIButtonGroup("aboutLinks")]
		public bool OpenForum
		{
			// 这里曾经是模板（AccessAnarchy）的帖子地址，v0.1.3 起指向本模组的帖子。
			set { OpenUrl("https://forum.paradoxplaza.com/forum/threads/tool-mode-memory.1942676/"); }
		}

		[SettingsUISection(kTabAbout, kGroupInfo)]
		[SettingsUIButton]
		[SettingsUIButtonGroup("aboutLinks")]
		public bool OpenRainbowSite
		{
			set { OpenUrl("https://rainbow-series-hvpma89wi25.qoder.zone/#top"); }
		}

		private static void OpenUrl(string url)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("OpenUrl failed: " + ex.GetType().Name);
			}
		}

		// ---------- 范围下拉（只显示短名；五种范围的定义写在每一项自己的说明里） ----------
		public DropdownItem<int>[] GetThemesScopeItems() { return BuildScopeItems(ToolItemCatalog.kThemes); }
		public DropdownItem<int>[] GetPacksScopeItems() { return BuildScopeItems(ToolItemCatalog.kPacks); }
		public DropdownItem<int>[] GetToolModeNetScopeItems() { return BuildScopeItems(ToolItemCatalog.kToolModeNet); }
		public DropdownItem<int>[] GetToolModeObjScopeItems() { return BuildScopeItems(ToolItemCatalog.kToolModeObj); }
		public DropdownItem<int>[] GetElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kElevation); }
		public DropdownItem<int>[] GetParallelScopeItems() { return BuildScopeItems(ToolItemCatalog.kParallel); }
		public DropdownItem<int>[] GetSnapNetScopeItems() { return BuildScopeItems(ToolItemCatalog.kSnapNet); }
		public DropdownItem<int>[] GetSnapObjScopeItems() { return BuildScopeItems(ToolItemCatalog.kSnapObj); }
		public DropdownItem<int>[] GetTopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kTopography); }
		public DropdownItem<int>[] GetUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kUnderground); }
		public DropdownItem<int>[] GetAnarchyScopeItems() { return BuildScopeItems(ToolItemCatalog.kAnarchy); }
		public DropdownItem<int>[] GetLeftRightScopeItems() { return BuildScopeItems(ToolItemCatalog.kLeftRight); }
		public DropdownItem<int>[] GetGeneralScopeItems() { return BuildScopeItems(ToolItemCatalog.kGeneral); }
		public DropdownItem<int>[] GetAnarchyObjectElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kObjectElevation); }
		public DropdownItem<int>[] GetFThemesScopeItems() { return BuildScopeItems(ToolItemCatalog.kFThemes); }
		public DropdownItem<int>[] GetFPacksScopeItems() { return BuildScopeItems(ToolItemCatalog.kFPacks); }
		public DropdownItem<int>[] GetFToolModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kFToolMode); }
		public DropdownItem<int>[] GetFSnapZoneScopeItems() { return BuildScopeItems(ToolItemCatalog.kFSnapZone); }
		public DropdownItem<int>[] GetFSnapAreaScopeItems() { return BuildScopeItems(ToolItemCatalog.kFSnapArea); }
		public DropdownItem<int>[] GetFSnapObjScopeItems() { return BuildScopeItems(ToolItemCatalog.kFSnapObj); }
		public DropdownItem<int>[] GetFUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kFUnderground); }
		public DropdownItem<int>[] GetFTopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kFTopography); }
		public DropdownItem<int>[] GetFBrushTypeScopeItems() { return BuildScopeItems(ToolItemCatalog.kFBrushType); }
		public DropdownItem<int>[] GetFBrushSizeScopeItems() { return BuildScopeItems(ToolItemCatalog.kFBrushSize); }
		public DropdownItem<int>[] GetFBrushStrengthScopeItems() { return BuildScopeItems(ToolItemCatalog.kFBrushStrength); }
		public DropdownItem<int>[] GetFBrushHeightScopeItems() { return BuildScopeItems(ToolItemCatalog.kFBrushHeight); }
		public DropdownItem<int>[] GetFDistanceScopeItems() { return BuildScopeItems(ToolItemCatalog.kFDistance); }
		public DropdownItem<int>[] GetFColorScopeItems() { return BuildScopeItems(ToolItemCatalog.kFColor); }
		public DropdownItem<int>[] GetFAnarchyScopeItems() { return BuildScopeItems(ToolItemCatalog.kFAnarchy); }
		public DropdownItem<int>[] GetFAnarchyObjectElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kFAnarchyObjectElevation); }
		public DropdownItem<int>[] GetIThemesScopeItems() { return BuildScopeItems(ToolItemCatalog.kIThemes); }
		public DropdownItem<int>[] GetIPacksScopeItems() { return BuildScopeItems(ToolItemCatalog.kIPacks); }
		public DropdownItem<int>[] GetIToolModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kIToolMode); }
		public DropdownItem<int>[] GetIDistanceScopeItems() { return BuildScopeItems(ToolItemCatalog.kIDistance); }
		public DropdownItem<int>[] GetIBrushTypeScopeItems() { return BuildScopeItems(ToolItemCatalog.kIBrushType); }
		public DropdownItem<int>[] GetIBrushSizeScopeItems() { return BuildScopeItems(ToolItemCatalog.kIBrushSize); }
		public DropdownItem<int>[] GetIBrushStrengthScopeItems() { return BuildScopeItems(ToolItemCatalog.kIBrushStrength); }
		public DropdownItem<int>[] GetIAgeMaskScopeItems() { return BuildScopeItems(ToolItemCatalog.kIAgeMask); }
		public DropdownItem<int>[] GetIDecorationScopeItems() { return BuildScopeItems(ToolItemCatalog.kIDecoration); }
		public DropdownItem<int>[] GetISnapScopeItems() { return BuildScopeItems(ToolItemCatalog.kISnap); }
		public DropdownItem<int>[] GetITopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kITopography); }
		public DropdownItem<int>[] GetIUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kIUnderground); }
		public DropdownItem<int>[] GetIAnarchyScopeItems() { return BuildScopeItems(ToolItemCatalog.kIAnarchy); }
		public DropdownItem<int>[] GetIAnarchyObjectElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kIObjectElevation); }

		private DropdownItem<int>[] BuildScopeItems(string itemId)
		{
			// 这一段跑在选项页框架里，而 GameManager.instance 不保证已经有值：设置页可能在
			// 主菜单之前被打开，也可能正在卸载。原来的写法直接解引用，一崩就把整页选项拖坏。
			// 取不到就沿用上一次的语言（初始 = 英文），下拉照样画得出来。
			SetActiveLocaleSafe();
			ToolItemDef def = ToolItemCatalog.Find(itemId);
			return LocaleTable.BuildScopeDropdown(def);
		}

		public int GetScopeItemsVersion()
		{
			// 这个版本号只用来让框架知道「语言换了，下拉要重画」。取不到语言 id 时返回 0，
			// 框架最多多重画一次，不会出错。
			try
			{
				GameManager gm = GameManager.instance;
				if (gm == null || gm.localizationManager == null) return 0;
				return gm.localizationManager.activeLocaleId.GetHashCode();
			}
			catch { return 0; }
		}

		/// <summary>把当前语言告诉 LocaleTable；拿不到就安静地留着上一次的（初始英文）。</summary>
		private static void SetActiveLocaleSafe()
		{
			try
			{
				GameManager gm = GameManager.instance;
				if (gm == null || gm.localizationManager == null) return;
				LocaleTable.SetActiveLocale(gm.localizationManager.activeLocaleId);
			}
			catch { }
		}

		// ---------- 读写小工具 ----------

		/// <summary>
		/// 该项是否参与「恢复」。三层闸门：大开关 → 本域开关 → 本项开关。
		/// 注意记录只受大开关限制，不受后两层限制（需求 10：关掉一项只是不再恢复，
		/// 它的数值仍在记录，否则关掉一项就等于丢掉它的历史）。
		/// </summary>
		public bool IsItemEnabled(string id)
		{
			if (!m_Enabled) return false;
			ToolItemDef def = ToolItemCatalog.Find(id);
			if (def != null && !GetDomain(def.Domain)) return false;
			bool v;
			return m_ItemEnabled.TryGetValue(id, out v) && v;
		}

		/// <summary>大开关（记录的唯一闸门）。</summary>
		public bool MasterOn { get { return m_Enabled; } }

		/// <summary>
		/// 某个域现在到底该不该工作：大开关开 且 该域开关开。
		/// 系统侧（ToolMemorySystem）按当前工具所属域问这一句，决定要不要恢复。
		/// </summary>
		public bool IsDomainEnabled(ItemDomain domain)
		{
			return m_Enabled && GetDomain(domain);
		}

		public MemoryScope GetItemScope(string id)
		{
			ToolItemDef def = ToolItemCatalog.Find(id);
			int fallback = def != null ? def.EffectiveRecommendedScope() : (int)MemoryScope.Group;
			int v;
			if (!m_ItemScope.TryGetValue(id, out v)) return (MemoryScope)fallback;
			if (v < 0 || v > (int)MemoryScope.GlobalUnique) return (MemoryScope)fallback;
			return (MemoryScope)v;
		}

		private bool GetEnabled(string id)
		{
			bool v;
			if (m_ItemEnabled.TryGetValue(id, out v)) return v;
			// v0.9.2：按工具类别拆出来的新行（toolMode.net …）在老存档的设置里不存在。
			// 直接返回 false 会让老玩家升级后发现「原来开着的工具模式被关了」，
			// 所以拆行项在**自己没有记录**时先沿用基项（toolMode）那一份设置；
			// 玩家一旦单独改过其中一行，自己的值就覆盖基项。
			ToolItemDef def = ToolItemCatalog.Find(id);
			if (def != null && !string.IsNullOrEmpty(def.TextId)
				&& m_ItemEnabled.TryGetValue(def.TextId, out v)) return v;
			return false;
		}

		private void SetEnabled(string id, bool value)
		{
			m_ItemEnabled[id] = value;
			// 拆行项第一次被改动时，把基项也写成同一个值：设置页从此有一份显式记录，
			// 下次读盘不再走继承；同时另一条同名行不会停在老值上（玩家看到两行一致）。
			ToolItemDef def = ToolItemCatalog.Find(id);
			if (def != null && !string.IsNullOrEmpty(def.TextId)) m_ItemEnabled[def.TextId] = value;
			Sync();
			Persist();
		}

		private int GetScope(string id)
		{
			int v;
			if (m_ItemScope.TryGetValue(id, out v)) return v;
			ToolItemDef def = ToolItemCatalog.Find(id);
			// 同上：拆行项没记录时沿用基项的共用范围（老玩家升级后的第一眼与升级前一致）
			if (def != null && !string.IsNullOrEmpty(def.TextId)
				&& m_ItemScope.TryGetValue(def.TextId, out v)) return v;
			// 设置在**设置页打开之前**就会被读（面板每帧取值），所以没记录时要回落到
			// 这一项自己的推荐档，不能一律 0（0 = 同组）—— 那会把「同类」等的推荐抹掉。
			return def != null ? def.EffectiveRecommendedScope() : 0;
		}

		private void SetScope(string id, int value)
		{
			// v0.10.0：改范围要「搬家」。owner 的规则 ——
			// 从大范围改小（如全局改同组）：改完所有组显示的还是全局那一份（靠读的时候沿宽档回落）；
			// 从小范围改大（如同组改全局）：以最后一次设置的为准（把当前值写进新档的键）。
			// 这里只记待办：真正搬要在「有当前工具/资产」的时候才知道往哪个键里写。
			int old = GetScope(id);
			m_ItemScope[id] = value;
			ToolItemDef def = ToolItemCatalog.Find(id);
			if (def != null && !string.IsNullOrEmpty(def.TextId)) m_ItemScope[def.TextId] = value;
			Sync();
			Persist();
			if (old != value) RequestScopeMigration(id, old, value);
		}

		/// <summary>把「范围变了」告诉运行中的系统（Sync 刚从同一个 World 里取过它）。</summary>
		private static void RequestScopeMigration(string id, int from, int to)
		{
			try
			{
				if (Unity.Entities.World.DefaultGameObjectInjectionWorld == null) return;
				Systems.ToolMemorySystem sys =
					Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<Systems.ToolMemorySystem>();
				if (sys != null) sys.RequestScopeMigration(id, from, to);
			}
			catch { }
		}

		/// <summary>
		/// 落盘走基类 ApplyAndSave()：框架的 GetTargetSetting 是按
		/// fragment.source.GetType().Name 匹配的（AssetDatabase.cs:830），传注册名反而找不到目标。
		/// 类名必须全局唯一，所以本类叫 ToolModeMemorySettings 而不是 Setting。
		/// </summary>
		private void Persist()
		{
			if (!Ready) return;
			try { ApplyAndSave(); } catch { }
		}

		/// <summary>设置变更后立刻作用到运行中的工具；大开关为关时绝不写回。</summary>
		public void Sync()
		{
			if (!Ready) return;
			try
			{
				ToolModeMemoryMod.RefreshActive();
				if (!m_Enabled) return;
				Systems.ToolMemorySystem sys = null;
				if (Unity.Entities.World.DefaultGameObjectInjectionWorld != null)
				{
					sys = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<Systems.ToolMemorySystem>();
				}
				if (sys == null) return;
				// 玩家刚改的可能是某一项的共用范围或「是否恢复」，这些都会改变键的算法；
				// 系统的键缓存是按工具/资产复用的（性能），所以这里必须主动作废一次。
				sys.ForgetKeysAndCache();
				if (sys.MasterEnabled) sys.RequestApply();
			}
			catch { }
		}

		/// <summary>需求 1：退回出厂推荐值（含大开关与三块域开关；每一项的是否恢复与共用范围也一起退回）。</summary>
		private void DoResetAllSettings()
		{
			try
			{
				SetDefaults();
				Sync();
				if (Ready)
				{
					try { ApplyAndSave(); } catch { }
				}
				ToolModeMemoryMod.log.Info("All settings restored to recommended defaults.");
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("ResetAllSettings failed: " + ex.GetType().Name);
			}
		}

		private void DoResetMemory()
		{
			try
			{
				bool inGame = GameManager.instance != null && GameManager.instance.gameMode.IsGame();
				MemoryStore store = ToolModeMemoryMod.Store;
				if (store == null) return;
				if (inGame)
				{
					store.ResetCurrentSave();
					// 只删文件不够：把当前面板先退回出厂值，否则下一帧捕获又把现值记回去
					Systems.ToolMemorySystem sys = Unity.Entities.World.DefaultGameObjectInjectionWorld != null
						? Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<Systems.ToolMemorySystem>()
						: null;
					if (sys != null) sys.ResetActiveTool();
					ToolModeMemoryMod.log.Info("Reset memory for current save.");
				}
				else
				{
					store.ResetAllSaves();
					ToolModeMemoryMod.log.Info("Reset memory for ALL saves.");
				}
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("ResetMemory failed: " + ex.GetType().Name);
			}
		}

		private void DoOpenMemoryFolder()
		{
			try
			{
				string dir = MemoryStore.DataDirectory;
				Directory.CreateDirectory(dir);
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = dir,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("OpenMemoryFolder failed: " + ex.GetType().Name);
			}
		}

		/// <summary>
		/// 出厂默认：大开关开、三块域开关开、每一项按 ToolItemCatalog 的推荐值。
		/// 「重置所有设置项」按钮也走这里（需求 1 + 「默认打开」）。
		/// 这里不碰 ToolMemoryBridge：构造期（LoadSettings 之前）不该触发它的静态反射初始化，
		/// 运行态一律由 Sync() 推。
		/// </summary>
		public override void SetDefaults()
		{
			// v0.9.0 owner 需求 1：总开关默认**关**，三个分开关默认**开**。
			// 语义（需求 2）：总开关管全部；分开关管「本域是否记忆 + 是否随存档恢复」。
			// 出厂总开关关 = 新玩家装上是彻底的原版行为，不会被任何默认记忆打扰；
			// 玩家打开总开关那一刻，三块分开关已经是开的，立刻就能用，不需要再逐块点开。
			m_Enabled = false;
			m_DomainEnabled[0] = true;
			m_DomainEnabled[1] = true;
			m_DomainEnabled[2] = true;
			InitItemDefaults();
		}
	}
}
