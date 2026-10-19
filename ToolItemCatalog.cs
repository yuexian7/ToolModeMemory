using System;

namespace ToolModeMemory
{
	/// <summary>工具项状态来源：原版工具字段，或依赖某个第三方模组存在。</summary>
	public enum ItemSource
	{
		Vanilla = 0,
		/// <summary>Anarchy（Paradox ModId 74604）。</summary>
		AnarchyMod = 1,
		/// <summary>Extra Networks and Areas + ExtraLib（77175 / 75724）。
		/// 当前目录里没有项属于它：「左侧和右侧」「常规」一度被归到这里，
		/// 反编译 Anarchy.dll 后确认两块面板都来自 Anarchy（见 ToolMemoryBridge 的 Anarchy 段）。</summary>
		ExtraNetworksMod = 2,
		/// <summary>原版工具栏的筛选面板（数据包 / 地区主题）。</summary>
		Toolbar = 3
	}

	/// <summary>
	/// 工具项所属的大类（= 设置页的标签页）。三类的设置、共用范围与记忆键**互不相通**：
	/// 同名项（工具模式 / 对齐 / 笔刷 / 地下模式）在三页各有一份，各配一套开关与范围。
	///
	/// 判定在 <c>ToolMemoryBridge.DomainOf(tool, prefab)</c>，依据全部来自反编译 Game.dll：
	///   Asset    = 用工具栏里的**资产**干活的工具：NetToolSystem（道路/轨道/管线/电线）、
	///              ObjectToolSystem 选中的普通建筑/树木/摆件、UpgradeToolSystem。
	///   Function = 功能型工具：ZoneToolSystem / AreaToolSystem / RouteToolSystem /
	///              TerrainToolSystem / BulldozeToolSystem / SelectionToolSystem，
	///              外加 ObjectToolSystem 选中「标记与预制对象」（动物生成器、生成位置等）。
	///   Item     = 城市美化菜单下的 8 类道具（自然 / 公园 / 住宅 / 商业 / 工业 / 残骸 / 贴花 / 盆栽），
	///              同样是 ObjectToolSystem，但按**菜单 + 分类**判定。
	/// </summary>
	public enum ItemDomain
	{
		Asset = 0,
		Function = 1,
		Item = 2
	}

	/// <summary>
	/// 功能域内部的 6 个分类（owner 定的清单，用于「同类」这一档的归组）：
	///   Zones   4 个功能区（住宅/商业/工业/办公；**不含**专门产业，那属于建筑资产）
	///   Area    3 个区域工具（市辖区、专门产业区（垃圾填埋场划分走的就是它）、表面区域）
	///   Route   交通运输菜单栏里的所有路线工具
	///   Terrain 9 个地形改造工具（海拔调整 / 地面平整 / 斜坡生成 / 地形平滑 /
	///           纹理绘制 01–04 / 纹理橡皮擦）
	///   Space   空间与区域（公园区域、徒步区域、散步区域、清空区域等其余区域工具，
	///           以及推土机、选择工具这类没有专属分类的功能）
	///   Marker  所有标记与预制对象（动物生成工具、生成位置、电线标记…）
	/// 资产域与道具域不用它（各自的「同类」判据见 AssetClass.cs 与道具菜单分类）。
	/// </summary>
	public enum FuncKind
	{
		None = 0,
		Zones = 1,
		Area = 2,
		Route = 3,
		Terrain = 4,
		Space = 5,
		Marker = 6
	}

	/// <summary>可记忆的工具面板项。id 稳定，用于设置属性名与记忆文件键。</summary>
	public sealed class ToolItemDef
	{
		public readonly string Id;
		/// <summary>设置页序号（1 起，**按标签页各自编号**）。</summary>
		public readonly int Number;
		public readonly ItemSource Source;
		/// <summary>所属标签页。</summary>
		public readonly ItemDomain Domain;
		/// <summary>原版是否记忆：-1=无记忆，0..4 同 MemoryScope。</summary>
		public readonly int VanillaScope;
		/// <summary>推荐范围：-1 = 无推荐（跟原版走）。</summary>
		public readonly int RecommendedScope;
		/// <summary>出厂是否启用本项。推荐与原版一致的项不默认开（用户定的规则）。</summary>
		public readonly bool DefaultEnabled;
		/// <summary>
		/// 子字段 id 列表。null = 本项只有一个值，直接存在项 id 下。
		/// 一项覆盖多个游戏值时必须列出（例如「并列模式」同时有 parallelCount 与
		/// parallelOffset 两个游戏字段，一个记忆桶存不下），每个子字段在记忆文件里
		/// 各自一个桶、共用本项的范围与键。
		/// </summary>
		public readonly string[] Subs;

		/// <summary>
		/// 该项在「全局共用」下是否只用层级键当记忆键（不套资产/功能域、不套枚举家族）。
		///
		/// 适用于「游戏里本来就只有一份值、而且这个值对所有工具都是同一个意思」的项：
		/// 等高线（UndergroundViewSystem.globalContourLinesOn，整个会话一个 bool）、
		/// 地下模式（每个工具一个 bool，但 on/off 的含义到处都一样）、
		/// Anarchy 的三份（都住在 NetworkAnarchyUISystem 这一个实例上）。
		/// 这类项如果照旧套上域/家族，「全局共用」就会碎成 A|S$net / A|S$obj / F|S$area…
		/// 每把一份，玩家换到另一个工具组时读到的是没记过的默认值，
		/// 看上去就是「设了全局共用还是每组各记各的」（0.3.0 修的这个缺陷）。
		/// 工具模式 / 对齐这类**枚举与位掩码**不适用：同一个数字在道路工具和区域工具里
		/// 根本是两回事，混成一把键会写进非法组合。
		/// </summary>
		public readonly bool FamilyFreeScope;

		/// <summary>
		/// 词条别名：这把 id 的说明与例子沿用另一项已写好的文案（12 种语言都有）。
		/// 只有「配色」用到：它在 0.6.2 之前就叫「其它」，而那 12 份表里
		/// item.other.desc / .ex.* 讲的全部就是道路颜色，行名换成官方的「颜色」即可，
		/// 其余句子一字不用重写（重写一遍 12 种语言只会引入前后不一致）。
		/// </summary>
		public readonly string LocaleAlias;

		public ToolItemDef(string id, int number, ItemSource source, int vanillaScope,
			int recommendedScope, bool defaultEnabled, string[] subs = null, bool familyFreeScope = false,
			ItemDomain domain = ItemDomain.Asset, string localeAlias = null)
		{
			Id = id;
			Number = number;
			Source = source;
			VanillaScope = vanillaScope;
			RecommendedScope = recommendedScope;
			DefaultEnabled = defaultEnabled;
			Subs = subs;
			FamilyFreeScope = familyFreeScope;
			Domain = domain;
			LocaleAlias = localeAlias;
		}

		/// <summary>实际用作该项默认的范围。</summary>
		public int EffectiveRecommendedScope()
		{
			return RecommendedScope >= 0 ? RecommendedScope : (VanillaScope >= 0 ? VanillaScope : (int)MemoryScope.Group);
		}

		/// <summary>设置页属性名用的 PascalCase（themes -> Themes，f.toolMode -> FToolMode）。</summary>
		public string PascalId()
		{
			string s = Id.Replace(".", " ");
			string[] parts = s.Split(' ');
			System.Text.StringBuilder sb = new System.Text.StringBuilder(Id.Length);
			for (int i = 0; i < parts.Length; i++)
			{
				string p = parts[i];
				if (p.Length == 0) continue;
				sb.Append(char.ToUpperInvariant(p[0]));
				if (p.Length > 1) sb.Append(p.Substring(1));
			}
			return sb.ToString();
		}
	}

	/// <summary>
	/// 记忆共用范围。游戏工具栏只有三层：菜单 → 分类 → 资产，加上两档全局。
	/// 五档定义（与设置页说明文本一一对应）：
	///   同组   = 同一「菜单 + 分类」，如所有小型道路（菜单/分类取工具栏当前的实时层级，也就是其它模组调整完的那份）
	///   同菜单 = 整个菜单（道路 / 教育 …）；同一资产出现在两个菜单时分别记忆
	///   同类   = 按「这一项自己服务的对象」归群：
	///              资产域看资产声明的通行对象/服务（判定见 AssetClass.cs）：小巷 / 各车道数的道路 /
	///              桥梁都是车行道路，步行道、地铁轨道、火车轨道、电缆各算一类，小学与中学同为教育设施；
	///              功能域看 6 个功能分类（功能区 / 区域工具 / 路线工具 / 地形改造 / 空间与区域 / 标记与预制对象）；
	///              道具域看城市美化下的 8 个道具分类（自然 / 公园 / 住宅 / 商业 / 工业 / 残骸 / 贴花 / 盆栽）。
	///              判不出类别的资产退化成按单个资产记忆
	///   全局共用 = 所有支持该项的资产/功能/道具共用一份，但三个域之间不共用
	///   全局不共用 = 每个资产/功能/道具各一份
	/// 例外：地区主题 / 数据包的「同类」仍按工具栏的（菜单+分类）发键，因为那两行的值是
	///   筛选面板的状态而不是资产的属性（理由见 ToolMemoryBridge.ResolveLevelKey 的 Category 分支）。
	/// </summary>
	public enum MemoryScope
	{
		Group = 0,
		Menu = 1,
		Category = 2,
		GlobalShared = 3,
		GlobalUnique = 4
	}

	/// <summary>
	/// 工具项目录。顺序 = 各标签页内的设置页顺序，**每个标签页各自从 1 开始编号**：
	///   资产页 1..10（地区主题 / 数据包 / 工具模式 / 高度 / 并列模式 / 对齐 / 等高线 /
	///               地下模式 / 笔刷 / 配色），Anarchy 板块 11..13；
	///   功能页 1..4（工具模式 / 对齐 / 地下模式 / 笔刷）；
	///   道具页 1..5（工具模式 / 对齐 / 笔刷 / 保留年代 / 地下模式）。
	/// 板块归属由 Source 决定：Vanilla 与 Toolbar 进「官方工具项设置」，AnarchyMod 进 Anarchy 板块。
	/// 项名一律采用对应语言包里的官方行名：原版项取游戏语言包（Toolbar.* / ToolOptions.*），
	/// Anarchy 项取 Anarchy 自带语言包（Anarchy.SECTION_TITLE[...]），该语言缺失才自行翻译。
	///
	/// v0.7.0 变化（owner 需求 1~7）：
	///  * 三域拆分：资产 / 功能 / 道具，各自一套同名工具项，记忆键靠域前缀隔开，互不共用；
	///  * 「其它」取消：它原来装的配色（placementColor 三个通道）独立成「配色」一项；
	///  * 「同类资产」统一改叫「同类」，功能域按 6 个功能分类归组、道具域按 8 个道具分类归组；
	///  * 功能域新增「工具模式 / 对齐 / 地下模式 / 笔刷」，道具域新增「保留年代」。
	/// </summary>
	public static class ToolItemCatalog
	{
		// ---- 资产域 ----
		public const string kThemes = "themes";
		public const string kPacks = "packs";
		public const string kToolMode = "toolMode";
		public const string kElevation = "elevation";
		public const string kParallel = "parallel";
		public const string kSnap = "snap";
		public const string kTopography = "topography";
		public const string kUnderground = "underground";
		public const string kBrush = "brush";
		/// <summary>道路配色。v0.7.0 从「其它」独立出来（owner：取消「其它」，全部列出来）。</summary>
		public const string kColor = "color";
		/// <summary>
		/// 保留年代（ObjectToolSystem.decorationMode，ToolOptions.DECORATION_MODE_TITLE）。
		/// 原版只在选中**带 TreeData 的资产**时才出现这一行（ObjectToolSystem L2418
		/// <c>allowAge = actionMode.IsGame() &amp;&amp; HasComponent&lt;TreeData&gt;(m_SelectedPrefab)</c>），
		/// 也就是只有树木类资产面板上有它。而树木在工具栏里属于城市美化/植被，
		/// **不是** owner 说的 8 类道具之一 → 这一项归资产域，道具页不放这一行
		/// （道具面板上根本没有这一行，放上去就是「设了没反应」）。
		/// </summary>
		public const string kDecoration = "decoration";

		// ---- Anarchy（资产页的第三方板块）----
		public const string kAnarchy = "anarchy";
		public const string kLeftRight = "leftRight";
		public const string kGeneral = "general";

		// ---- 功能域（id 加 "f." 前缀，与资产域的同名项彻底隔开）----
		public const string kFToolMode = "f.toolMode";
		public const string kFSnap = "f.snap";
		public const string kFTopography = "f.topography";
		public const string kFUnderground = "f.underground";
		public const string kFBrush = "f.brush";

		// ---- 道具域（id 加 "i." 前缀）----
		// 只有四行：道具面板上真有的、且不属于资产域语义的行。
		// 「保留年代」不在这里 —— 它只在带 TreeData 的资产上出现，而树木属于
		// 城市美化/植被（不是 owner 点的 8 类道具），归资产域第 11 项。
		public const string kIToolMode = "i.toolMode";
		public const string kISnap = "i.snap";
		public const string kIBrush = "i.brush";
		public const string kITopography = "i.topography";
		public const string kIUnderground = "i.underground";

		/// <summary>已退役的项 id：v0.7.0 取消「其它」（配色独立成 kColor）。</summary>
		public const string kRetiredOther = "other";

		public const int kVanillaNone = -1;
		public const int kNoRecommendation = -1;

		/// <summary>「高度」覆盖的两个游戏字段：elevation 与 elevationStep（高度阶段/调整幅度）。</summary>
		private static readonly string[] s_ElevationSubs = new string[]
		{
			"elevation", "elevationStep"
		};

		/// <summary>「并列模式」的两个游戏字段：parallelCount / parallelOffset。</summary>
		private static readonly string[] s_ParallelSubs = new string[]
		{
			"parallel.count", "parallel.offset"
		};

		/// <summary>
		/// 「配色」的游戏字段：道路配色（NetToolSystem.placementColor，ColorSet 共 3 个通道，
		/// color3 故意留着当占位，读写都返回 false）。
		/// 字段 id 沿用 <c>other.</c> 前缀：<b>它就是记忆文件里的桶名</b>
		/// （0.1.2~0.6.2 配色一直挂在「其它」这一项下）。项 id 改叫 color 只影响设置页，
		/// 桶名不变，玩家已经记下的配色不会丢。
		/// </summary>
		private static readonly string[] s_ColorSubs = new string[]
		{
			"other.color0", "other.color1", "other.color2", "other.color3"
		};

		/// <summary>
		/// 资产域「笔刷」的两个游戏字段（ToolBaseSystem.brushSize / brushStrength，只有
		/// <c>brushing</c> 为真的工具用得到）。
		///
		/// 子字段 id 故意沿用 <c>other.</c> 前缀：<b>它就是记忆文件里的桶名</b>
		/// （0.1.2~0.6.1 笔刷一直挂在「其它」这一项下）。改名叫 "brush.size" 看着更顺眼，
		/// 但玩家已经记下的笔刷粗细/强度会整桶变成不认识的数据 —— 名字不好看只是源码里的事，
		/// 抹掉玩家记过的值才是伤人的事。
		/// </summary>
		private static readonly string[] s_BrushSubs = new string[]
		{
			"other.brushSize", "other.brushStrength"
		};

		/// <summary>
		/// 「左侧和右侧」的两个游戏字段：Anarchy 的 NetworkAnarchyUISystem
		/// m_LeftUpgrade / m_RightUpgrade（两侧各一份 SideUpgrades 位掩码）。
		/// </summary>
		private static readonly string[] s_LeftRightSubs = new string[]
		{
			"leftRight.left", "leftRight.right"
		};

		/// <summary>
		/// 功能域 / 道具域的每一项**沿用资产域同名的桶名**（"toolMode" / "snap" / "topography" /
		/// "underground" / "other.brushSize" …，也就是上面那几组 s_*Subs 数组本身）。
		///
		/// 这是刻意的：三域互不共用靠的是**记忆键里的域前缀**（A| / F| / I|，见 MemoryKeys），
		/// 桶名（fieldId）只决定「这是面板上哪一行」。如果给功能域另起一套桶名
		/// （"f.toolMode"…），0.6.2 之前玩家在资产域记下的地形笔刷（other.brushSize）
		/// 就整桶变成不认识的数据 —— 名字好不好看不影响玩家，抹掉记过的值才影响。
		/// 项 id 才带 f. / i. 前缀（决定设置页属性名与恢复开关），桶名不带。
		/// </summary>
		private static readonly string[] s_FToolModeSubs = new string[] { "toolMode" };
		private static readonly string[] s_FSnapSubs = new string[] { "snap" };
		private static readonly string[] s_FTopographySubs = new string[] { "topography" };
		private static readonly string[] s_FUndergroundSubs = new string[] { "underground" };

		/// <summary>道具域各项的桶名，同上。</summary>
		private static readonly string[] s_IToolModeSubs = new string[] { "toolMode" };
		private static readonly string[] s_ISnapSubs = new string[] { "snap" };
		private static readonly string[] s_ITopographySubs = new string[] { "topography" };
		private static readonly string[] s_IUndergroundSubs = new string[] { "underground" };

		/// <summary>
		/// 资产域「保留年代」的桶名。游戏里叫 decorationMode（ToolOptions.DECORATION_MODE_TITLE），
		/// 是新出现的行，没有历史包袱，直接用可读的名字。
		/// </summary>
		private static readonly string[] s_DecorationSubs = new string[] { "decoration" };

		/// <summary>
		/// 资产域（= 资产标签页）。这一份是「用工具栏资产干活的工具」的全部可记忆项。
		/// </summary>
		public static readonly ToolItemDef[] AssetItems = new ToolItemDef[]
		{
			// 原版「地区主题」是全局共用，「数据包」按组显示并切组自动重置 -> 都无推荐，出厂不开
			new ToolItemDef(kThemes, 1, ItemSource.Toolbar, (int)MemoryScope.GlobalShared, kNoRecommendation, false,
				null, false, ItemDomain.Asset),
			new ToolItemDef(kPacks, 2, ItemSource.Toolbar, (int)MemoryScope.Group, kNoRecommendation, false,
				null, false, ItemDomain.Asset),
			// 原版每个工具模式按分类记忆（NetToolPreferences），故推荐全局共用才有增益
			new ToolItemDef(kToolMode, 3, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.GlobalShared, true,
				null, false, ItemDomain.Asset),
			new ToolItemDef(kElevation, 4, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.Group, true,
				s_ElevationSubs, false, ItemDomain.Asset),
			new ToolItemDef(kParallel, 5, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.GlobalUnique, true,
				s_ParallelSubs, false, ItemDomain.Asset),
			new ToolItemDef(kSnap, 6, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true,
				null, false, ItemDomain.Asset),
			new ToolItemDef(kTopography, 7, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.GlobalShared, false,
				null, true, ItemDomain.Asset),
			// 地下模式：推荐同组 == 原版同组 -> 默认关（用户定的规则）
			new ToolItemDef(kUnderground, 8, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Group, false,
				null, true, ItemDomain.Asset),
			// 笔刷：原版只在**本次会话**里按地形类型各存一份
			// （TerrainToolSystem 私有 TerraformingBrushValues[4]，读档/新会话回默认 100 m / 0.5），
			// 所以 VanillaScope = 无记忆；推荐「全局共用」= 各类地形工具共用一把，
			// 换抬地/降地/软化/刷涂不回到出厂粗细。
			// 故意**不套 FamilyFreeScope**（与 0.6.2 审查建议不同，理由有反编译凭据）：
			// family-free 会把「全局共用」压成裸键 S，于是地形笔刷与摆件刷涂（ObjectToolSystem 的
			// Brush 模式，brushing 同样为真）共用同一个数 —— 可这两个数的含义不一样
			// （地形 = 每一下的位移量，摆件 = 物体密度），原版自己也是两份独立值。
			new ToolItemDef(kBrush, 9, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.GlobalShared, false,
				s_BrushSubs, false, ItemDomain.Asset),
			// 配色（v0.7.0 从「其它」独立）：原版每次换资产都按资产自带的默认色重设，无记忆。
			// 行名换成官方语言包的「颜色」（ToolOptions.COLOR_TITLE），
			// 说明与例子沿用旧「其它」那一份 12 语言文案（内容本来就全是讲道路颜色）。
			new ToolItemDef(kColor, 10, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_ColorSubs, false, ItemDomain.Asset, kRetiredOther),
			// 保留年代：只有带 TreeData 的资产（树木，工具栏在城市美化/植被下）面板上才有这一行。
			new ToolItemDef(kDecoration, 11, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_DecorationSubs, false, ItemDomain.Asset),

			// ---- Anarchy 工具项（资产页的第三方板块，12..14：序号在同一标签页内连续，
			//      与 0.6.2 的规矩一致，避免一页里出现两个「1.」）----
			new ToolItemDef(kAnarchy, 12, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Category, false,
				null, true, ItemDomain.Asset),
			new ToolItemDef(kLeftRight, 13, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Group, true,
				s_LeftRightSubs, true, ItemDomain.Asset),
			new ToolItemDef(kGeneral, 14, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Category, true,
				null, true, ItemDomain.Asset),
		};

		/// <summary>
		/// 功能域（= 功能标签页）：4 个功能区、3 个区域工具、所有路线工具、9 个地形改造工具、
		/// 5 个空间与区域工具、所有标记与预制对象。它们共用同一批面板行（工具模式 / 对齐 /
		/// 地下模式 / 笔刷），差别只在「同类」这一档怎么归组 —— 归组由 FuncKind 决定。
		/// </summary>
		public static readonly ToolItemDef[] FunctionItems = new ToolItemDef[]
		{
			// 功能型工具的模式枚举各不相同（Zone 填充/滚动/刷涂、Area 编辑/生成、Bulldoze 主要/附属/全部），
			// 家族键把它们隔开，所以「同组」是安全默认；推荐「同类」= 6 大类归组（owner 定案）。
			new ToolItemDef(kFToolMode, 1, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_FToolModeSubs, false, ItemDomain.Function),
			new ToolItemDef(kFSnap, 2, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_FSnapSubs, false, ItemDomain.Function),
			// 等高线：游戏里全局只有一份值（UndergroundViewSystem.globalContourLinesOn）。
			// owner 2026-10-19 明确选「三页各一份，按当前工具所属页读写」，所以功能页也有一行。
			new ToolItemDef(kFTopography, 3, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.GlobalShared, false,
				s_FTopographySubs, true, ItemDomain.Function),
			new ToolItemDef(kFUnderground, 4, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Group, false,
				s_FUndergroundSubs, true, ItemDomain.Function),
			// 地形工具的笔刷（9 个地形改造工具都吃这一项）：原版按地形类型各存一份、不落盘。
			// 桶名与资产域笔刷相同（other.brushSize / other.brushStrength）是安全的：
			// 捕获与写回都按**当前工具所属域**过滤（见 ToolMemorySystem.CaptureNow），
			// 一帧里只有一个域会写，而完整键又带着 F| 前缀，所以两域的笔刷不会互相覆盖。
			new ToolItemDef(kFBrush, 5, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_BrushSubs, false, ItemDomain.Function),
		};

		/// <summary>
		/// 道具域（= 道具标签页）：城市美化菜单下的 8 类道具（自然 / 公园 / 住宅 / 商业 /
		/// 工业 / 残骸 / 贴花 / 盆栽）。道具都是 ObjectToolSystem 放的，所以行与资产页相近，
		/// 但「同类」按道具自己的 8 个分类归组。
		/// </summary>
		public static readonly ToolItemDef[] ItemItems = new ToolItemDef[]
		{
			new ToolItemDef(kIToolMode, 1, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_IToolModeSubs, false, ItemDomain.Item),
			new ToolItemDef(kISnap, 2, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_ISnapSubs, false, ItemDomain.Item),
			// 道具刷涂的强度 = 物体密度（与地形笔刷的位移量不是一回事，原版自己也是两份值）
			new ToolItemDef(kIBrush, 3, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Category, true,
				s_BrushSubs, false, ItemDomain.Item),
			new ToolItemDef(kITopography, 4, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.GlobalShared, false,
				s_ITopographySubs, true, ItemDomain.Item),
			new ToolItemDef(kIUnderground, 5, ItemSource.Vanilla, kVanillaNone, (int)MemoryScope.Group, false,
				s_IUndergroundSubs, true, ItemDomain.Item),
		};

		/// <summary>全表（三域拼接）。设置页、捕获/写回、回归测试都按域过滤后使用。</summary>
		public static readonly ToolItemDef[] Items = Concat(AssetItems, FunctionItems, ItemItems);

		private static ToolItemDef[] Concat(params ToolItemDef[][] groups)
		{
			int n = 0;
			for (int i = 0; i < groups.Length; i++) n += groups[i].Length;
			ToolItemDef[] all = new ToolItemDef[n];
			int k = 0;
			for (int i = 0; i < groups.Length; i++)
			{
				for (int j = 0; j < groups[i].Length; j++) all[k++] = groups[i][j];
			}
			return all;
		}

		/// <summary>某个标签页的全部项（顺序 = 设置页顺序）。</summary>
		public static ToolItemDef[] ForDomain(ItemDomain domain)
		{
			switch (domain)
			{
				case ItemDomain.Function: return FunctionItems;
				case ItemDomain.Item: return ItemItems;
				default: return AssetItems;
			}
		}

		/// <summary>
		/// 「全局共用」这一档是否要用不带域/家族的裸层级键（理由见 ToolItemDef.FamilyFreeScope）。
		/// 其余四档照旧按层级 + 域 + 家族存，保证同一把键不会被两类互不相干的面板抢用。
		/// </summary>
		public static bool UsesFamilyFreeKey(ToolItemDef def, MemoryScope scope)
		{
			if (def == null) return false;
			return def.FamilyFreeScope && scope == MemoryScope.GlobalShared;
		}

		public static ToolItemDef Find(string id)
		{
			for (int i = 0; i < Items.Length; i++)
			{
				if (Items[i].Id == id) return Items[i];
			}
			// 子字段 id 认回父项。两种形态都要认：
			//   带点的（"parallel.count" / "other.color0" …）按前缀认；
			//   不带点的（"elevationStep" —— 它自己本来就是个合法项名）在 Subs 里精确匹配。
			// MemoryStore 读档时用 Find() 过滤「这一项还认识吗」，而每个子字段在记忆里
			// 是一个独立桶（键 = 子字段 id），这里不认回来的话子字段每次进档都会被丢掉。
			if (id != null)
			{
				for (int i = 0; i < Items.Length; i++)
				{
					string[] subs = Items[i].Subs;
					if (subs == null) continue;
					for (int j = 0; j < subs.Length; j++)
					{
						if (subs[j] == id) return Items[i];
					}
				}
				int dot = id.IndexOf('.');
				if (dot > 0)
				{
					string root = id.Substring(0, dot);
					for (int i = 0; i < Items.Length; i++)
					{
						if (Items[i].Id == root) return Items[i];
					}
				}
			}
			return null;
		}

		public static int Count { get { return Items.Length; } }

		/// <summary>该域有几个官方（非第三方模组）项 —— 设置页「官方工具项设置」板块的行数基数。</summary>
		public static int OfficialCount(ItemDomain domain)
		{
			ToolItemDef[] items = ForDomain(domain);
			int n = 0;
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].Source != ItemSource.AnarchyMod) n++;
			}
			return n;
		}

		/// <summary>该域有几个 Anarchy 项 —— 第三方板块的行数基数。</summary>
		public static int AnarchyCount(ItemDomain domain)
		{
			ToolItemDef[] items = ForDomain(domain);
			int n = 0;
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].Source == ItemSource.AnarchyMod) n++;
			}
			return n;
		}
	}
}
