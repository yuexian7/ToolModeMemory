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
		kGroupFunctionOfficial, kGroupItemOfficial, kGroupMemory, kGroupInfo)]
	[SettingsUIShowGroupName(kGroupDomain, kGroupAssetOfficial, kGroupAssetAnarchy,
		kGroupFunctionOfficial, kGroupItemOfficial, kGroupMemory)]
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
		public const string kGroupItemOfficial = "ItemOfficialItems";
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

		private bool m_Enabled = true;
		/// <summary>三块域开关（= 三个标签页的总闸）。</summary>
		private readonly bool[] m_DomainEnabled = new bool[] { true, true, true };
		private readonly Dictionary<string, bool> m_ItemEnabled = new Dictionary<string, bool>(StringComparer.Ordinal);
		private readonly Dictionary<string, int> m_ItemScope = new Dictionary<string, int>(StringComparer.Ordinal);

		/// <summary>
		/// 模组设置不走 SharedSettings.Reset()：框架只把「当前实例」交给
		/// AssetDatabase.LoadSettings(name, obj, defaultObj)（AssetDatabase.cs:611），
		/// 没有存档文件时没人会调用 SetDefaults()。所以出厂默认必须在构造函数里就落好，
		/// 这也让「总开关默认打开」真的成立（原版 GameplaySettings 的写法一致）。
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

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ToolModeEnabled { get { return GetEnabled(ToolItemCatalog.kToolMode); } set { SetEnabled(ToolItemCatalog.kToolMode, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetToolModeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsToolModeScopeDisabled))]
		public int ToolModeScope { get { return GetScope(ToolItemCatalog.kToolMode); } set { SetScope(ToolItemCatalog.kToolMode, value); } }

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
		public bool SnapEnabled { get { return GetEnabled(ToolItemCatalog.kSnap); } set { SetEnabled(ToolItemCatalog.kSnap, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetSnapScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsSnapScopeDisabled))]
		public int SnapScope { get { return GetScope(ToolItemCatalog.kSnap); } set { SetScope(ToolItemCatalog.kSnap, value); } }

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

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool BrushEnabled { get { return GetEnabled(ToolItemCatalog.kBrush); } set { SetEnabled(ToolItemCatalog.kBrush, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetBrushScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsBrushScopeDisabled))]
		public int BrushScope { get { return GetScope(ToolItemCatalog.kBrush); } set { SetScope(ToolItemCatalog.kBrush, value); } }

		/// <summary>道路配色：v0.7.0 从「其它」独立成一项（owner 需求 4：去掉「其它」，全部列出来）。</summary>
		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool ColorEnabled { get { return GetEnabled(ToolItemCatalog.kColor); } set { SetEnabled(ToolItemCatalog.kColor, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetColorScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsColorScopeDisabled))]
		public int ColorScope { get { return GetScope(ToolItemCatalog.kColor); } set { SetScope(ToolItemCatalog.kColor, value); } }

		/// <summary>
		/// 保留年代（ObjectToolSystem.decorationMode）。只有带 TreeData 的资产（树木）面板上才有这一行，
		/// 树木在工具栏里属于城市美化/植被、不属于 owner 点的 8 类道具，所以列在资产页。
		/// </summary>
		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAssetSectionOff))]
		public bool DecorationEnabled { get { return GetEnabled(ToolItemCatalog.kDecoration); } set { SetEnabled(ToolItemCatalog.kDecoration, value); } }

		[SettingsUISection(kTabAssets, kGroupAssetOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetDecorationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsDecorationScopeDisabled))]
		public int DecorationScope { get { return GetScope(ToolItemCatalog.kDecoration); } set { SetScope(ToolItemCatalog.kDecoration, value); } }

		// ==================== 资产页：Anarchy 工具项（1..3） ====================
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
		// 为什么只有资产页有 Anarchy 板块：Anarchy 的「左侧/右侧/常规」两块面板只挂在道路工具上
		// （NetworkAnarchyUISystem 订阅的就是 EventToolChanged 里的 NetToolSystem，
		// 见 research/extra/anarchy 反编译），功能区/道具面板上没有这三行，
		// 所以那两个标签页没有可记的东西，硬加一行只会让玩家以为设了却什么都没发生。

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

		// ==================== 功能页：官方工具项（1..5） ====================

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
		public bool FSnapEnabled { get { return GetEnabled(ToolItemCatalog.kFSnap); } set { SetEnabled(ToolItemCatalog.kFSnap, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFSnapScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFSnapScopeDisabled))]
		public int FSnapScope { get { return GetScope(ToolItemCatalog.kFSnap); } set { SetScope(ToolItemCatalog.kFSnap, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFunctionSectionOff))]
		public bool FTopographyEnabled { get { return GetEnabled(ToolItemCatalog.kFTopography); } set { SetEnabled(ToolItemCatalog.kFTopography, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFTopographyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFTopographyScopeDisabled))]
		public int FTopographyScope { get { return GetScope(ToolItemCatalog.kFTopography); } set { SetScope(ToolItemCatalog.kFTopography, value); } }

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
		public bool FBrushEnabled { get { return GetEnabled(ToolItemCatalog.kFBrush); } set { SetEnabled(ToolItemCatalog.kFBrush, value); } }

		[SettingsUISection(kTabFunctions, kGroupFunctionOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetFBrushScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsFBrushScopeDisabled))]
		public int FBrushScope { get { return GetScope(ToolItemCatalog.kFBrush); } set { SetScope(ToolItemCatalog.kFBrush, value); } }

		// ==================== 道具页：官方工具项（1..6） ====================

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
		public bool ISnapEnabled { get { return GetEnabled(ToolItemCatalog.kISnap); } set { SetEnabled(ToolItemCatalog.kISnap, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetISnapScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsISnapScopeDisabled))]
		public int ISnapScope { get { return GetScope(ToolItemCatalog.kISnap); } set { SetScope(ToolItemCatalog.kISnap, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsItemSectionOff))]
		public bool IBrushEnabled { get { return GetEnabled(ToolItemCatalog.kIBrush); } set { SetEnabled(ToolItemCatalog.kIBrush, value); } }

		[SettingsUISection(kTabItems, kGroupItemOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetIBrushScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsIBrushScopeDisabled))]
		public int IBrushScope { get { return GetScope(ToolItemCatalog.kIBrush); } set { SetScope(ToolItemCatalog.kIBrush, value); } }

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

		// ---------- 每项的共用范围行是否可点：本域没开 或 本项没开 ----------

		public bool IsThemesScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kThemes); }

		public bool IsPacksScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kPacks); }

		public bool IsToolModeScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kToolMode); }

		public bool IsElevationScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kElevation); }

		public bool IsParallelScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kParallel); }

		public bool IsSnapScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kSnap); }

		public bool IsTopographyScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kTopography); }

		public bool IsUndergroundScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kUnderground); }

		public bool IsColorScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kColor); }

		public bool IsDecorationScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kDecoration); }

		public bool IsBrushScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kBrush); }

		public bool IsAnarchyScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kAnarchy); }

		public bool IsLeftRightScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kLeftRight); }

		public bool IsGeneralScopeDisabled() { return IsAssetSectionOff() || !GetEnabled(ToolItemCatalog.kGeneral); }

		public bool IsFToolModeScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFToolMode); }

		public bool IsFSnapScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFSnap); }

		public bool IsFTopographyScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFTopography); }

		public bool IsFUndergroundScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFUnderground); }

		public bool IsFBrushScopeDisabled() { return IsFunctionSectionOff() || !GetEnabled(ToolItemCatalog.kFBrush); }

		public bool IsIToolModeScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIToolMode); }

		public bool IsISnapScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kISnap); }

		public bool IsIBrushScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIBrush); }

		public bool IsITopographyScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kITopography); }

		public bool IsIUndergroundScopeDisabled() { return IsItemSectionOff() || !GetEnabled(ToolItemCatalog.kIUnderground); }

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
		public DropdownItem<int>[] GetToolModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kToolMode); }
		public DropdownItem<int>[] GetElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kElevation); }
		public DropdownItem<int>[] GetParallelScopeItems() { return BuildScopeItems(ToolItemCatalog.kParallel); }
		public DropdownItem<int>[] GetSnapScopeItems() { return BuildScopeItems(ToolItemCatalog.kSnap); }
		public DropdownItem<int>[] GetTopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kTopography); }
		public DropdownItem<int>[] GetUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kUnderground); }
		public DropdownItem<int>[] GetColorScopeItems() { return BuildScopeItems(ToolItemCatalog.kColor); }
		public DropdownItem<int>[] GetDecorationScopeItems() { return BuildScopeItems(ToolItemCatalog.kDecoration); }
		public DropdownItem<int>[] GetBrushScopeItems() { return BuildScopeItems(ToolItemCatalog.kBrush); }
		public DropdownItem<int>[] GetAnarchyScopeItems() { return BuildScopeItems(ToolItemCatalog.kAnarchy); }
		public DropdownItem<int>[] GetLeftRightScopeItems() { return BuildScopeItems(ToolItemCatalog.kLeftRight); }
		public DropdownItem<int>[] GetGeneralScopeItems() { return BuildScopeItems(ToolItemCatalog.kGeneral); }
		public DropdownItem<int>[] GetFToolModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kFToolMode); }
		public DropdownItem<int>[] GetFSnapScopeItems() { return BuildScopeItems(ToolItemCatalog.kFSnap); }
		public DropdownItem<int>[] GetFTopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kFTopography); }
		public DropdownItem<int>[] GetFUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kFUnderground); }
		public DropdownItem<int>[] GetFBrushScopeItems() { return BuildScopeItems(ToolItemCatalog.kFBrush); }
		public DropdownItem<int>[] GetIToolModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kIToolMode); }
		public DropdownItem<int>[] GetISnapScopeItems() { return BuildScopeItems(ToolItemCatalog.kISnap); }
		public DropdownItem<int>[] GetIBrushScopeItems() { return BuildScopeItems(ToolItemCatalog.kIBrush); }
		public DropdownItem<int>[] GetITopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kITopography); }
		public DropdownItem<int>[] GetIUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kIUnderground); }

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
			return m_ItemEnabled.TryGetValue(id, out v) && v;
		}

		private void SetEnabled(string id, bool value)
		{
			m_ItemEnabled[id] = value;
			Sync();
			Persist();
		}

		private int GetScope(string id)
		{
			int v;
			return m_ItemScope.TryGetValue(id, out v) ? v : 0;
		}

		private void SetScope(string id, int value)
		{
			m_ItemScope[id] = value;
			Sync();
			Persist();
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
			m_Enabled = true;
			m_DomainEnabled[0] = true;
			m_DomainEnabled[1] = true;
			m_DomainEnabled[2] = true;
			InitItemDefaults();
		}
	}
}
