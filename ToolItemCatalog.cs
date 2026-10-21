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
	/// v0.8.0 改名：「资产」→「建筑与服务」（owner：道具本身也是资产，旧名有歧义）。
	/// 枚举名保留 Asset（记忆键前缀 A| 与老存档的键格式不能动），只有界面文案改了。
	///
	/// 判定在 <c>ToolMemoryBridge.DomainOf(tool, prefab)</c>，依据全部来自反编译 Game.dll：
	///   Asset    = 建筑与服务：NetToolSystem（道路/轨道/管线/电线）、UpgradeToolSystem、
	///              ObjectToolSystem 选中的普通建筑。
	///   Function = 功能型工具：ZoneToolSystem / AreaToolSystem / RouteToolSystem /
	///              TerrainToolSystem / BulldozeToolSystem / SelectionToolSystem，
	///              外加 ObjectToolSystem 选中「标记与预制对象」（动物生成器、生成位置等）。
	///   Item     = 城市美化菜单下的 8 类道具 + 植被（树）：v0.8.0 起树木也算道具页
	///              （owner 2026-10-20 定：树是用摆件工具刷的，与公园道具同一套面板行）。
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

		/// <summary>
		/// 本项这一行**实际会在哪几类工具/资产上出现**，用逗号分隔的稳定标记（owner 需求 2：
		/// 「工具项在哪几类功能里有都写清楚」）。标记取自 <see cref="PlaceTokens"/>：
		///   net/object/zone/area/route/terrain/bulldoze/water  = 原版 11 个工具（按 toolID）
		///   upgrade/selection/default                          = 面板为空或不存在，只作记录
		///   tree                                               = 带 TreeData 的资产（树）
		///   brushable/stampable                                = 摆件工具里带该 flag 的资产
		///   lineCurve                                          = 摆件工具的线/曲线模式
		///   editorOnly                                         = 仅在编辑器出现
		///   anarchy                                            = Anarchy 注入行
		/// 依据 = <c>research/round4/vanilla_rows.md</c> 第 5 节「行 × 工具」反查表 +
		/// <c>research/round4/anarchy_rows.md</c>，逐条带 文件:行号 证据。
		/// 设置页说明文本按同一份清单写（12 语言），tests/option_panel_check.py 校验两者一致。
		/// </summary>
		public readonly string Places;

		/// <summary>
		/// 共用范围说明里「同组 / 同菜单」两句举例用哪个工具类别（v0.9.1）。
		/// 空 = 用本项自己的 <c>item.&lt;id&gt;.ex.*</c>（只有建筑与服务页这么写）；
		/// 非空 = 到 <c>ex.&lt;family&gt;.&lt;slot&gt;</c> 取该类别的一句例，**不再回落到本项**。
		///
		/// 0.9.1 修的就是这一件事：0.9.0 之前功能页 / 道具页的项没有自己的例子时，
		/// ItemText() 会回落到「去掉 f./i. 前缀」的建筑与服务页文案 ——
		/// 于是功能页和道具页上出现了「道路 / 小型道路」这种根本不在这一页的东西。
		/// 例子改成按类别取之后，功能页最多出现功能区 / 路线 / 地形改造，
		/// 道具页最多出现城市美化菜单下的道具分类，跨页取词在结构上就不可能再发生。
		///
		/// 类别清单（与 <c>ex.&lt;family&gt;.*</c> 的键一一对应）：
		///   net    = 建筑与服务页的道路/轨道等（唯一保留「本项自己举例子」的页）
		///   zone   = 功能区（住宅区里的各种住宅类型算一组，商业区是另一组）
		///   route  = 路线工具（按交通工具分组）
		///   terrain= 地形改造（整个菜单只有一个分组 → 同组与同菜单是同一批工具）
		///   prop   = 道具（城市美化菜单下的 8 类 + 植被）
		///   tree   = 树（植被只有一个分组 → 同组与同菜单是同一批树）
		/// 判据与证据见 research/round4/vanilla_rows.md 第 3、6 节与本文件各项注释。
		/// </summary>
		public readonly string ExampleFamily;

		/// <summary>
		/// 本项只对哪一类工具生效（v0.9.2）。空 = 不限。
		/// 取值与 <c>ToolMemoryBridge.Family(tool)</c> 一致：net / obj / zone / area / terrain /
		/// route / bulldoze / water / selection / other。
		///
		/// 为什么要有它：同一个名字的行在不同工具上**可选值根本不同**（工具模式在道路上是
		/// 直线/简单曲线/网格，在建筑上是放置单个/升级/移动；对齐在道路上是路边/网格，
		/// 在建筑上是建筑表面），所以记忆键本来就按家族分开存。可设置界面上却只列了一行，
		/// 于是「全局共用」那句「所有显示这一项的工具共用设置」与实际不符 —— 道路上设的值
		/// 到建筑上读不到。v0.9.2 起这类项按工具类别拆成独立的两行/三行
		/// （工具模式（道路）/ 工具模式（建筑）…），每一行只负责自己那一类工具，
		/// 说明与例子也只写那一类。
		///
		/// 拆行**不改记忆桶名**：拆出来的每一项 Subs 仍指向原来的桶（toolMode / snap），
		/// 键里本来就带家族段，所以老存档里记下的值原样读得回。
		/// </summary>
		public readonly string Family;

		/// <summary>
		/// 文案基项 id（v0.9.2）：拆出来的行（toolMode.net 等）没有自己的 12 语言文案，
		/// label / desc 仍取基项（toolMode）那一份，界面行名再拼上工具类别后缀。
		/// 空 = 本事就是基项。
		/// </summary>
		public readonly string TextId;

		public ToolItemDef(string id, int number, ItemSource source, int vanillaScope,
			int recommendedScope, bool defaultEnabled, string[] subs = null, bool familyFreeScope = false,
			ItemDomain domain = ItemDomain.Asset, string localeAlias = null, string places = "",
			string exampleFamily = null, string family = null, string textId = null)
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
			Places = places ?? string.Empty;
			ExampleFamily = exampleFamily ?? string.Empty;
			Family = family ?? string.Empty;
			TextId = textId ?? string.Empty;
		}

		/// <summary>Places 允许的标记全集。门禁据此拒绝拼错的标记。</summary>
		public static readonly string[] PlaceTokens = new string[]
		{
			"net", "object", "zone", "area", "route", "terrain", "bulldoze", "water",
			"upgrade", "selection", "default",
			"tree", "brushable", "stampable", "lineCurve", "editorOnly", "anarchy"
		};

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
	///   建筑与服务页 1..13，Anarchy 板块 14..16；
	///   功能页 1..8；
	///   道具页 1..12。
	/// 板块归属由 Source 决定：Vanilla 与 Toolbar 进「官方工具项设置」，AnarchyMod 进 Anarchy 板块。
	/// 项名一律采用对应语言包里的官方行名：原版项取游戏语言包（Toolbar.* / ToolOptions.*），
	/// Anarchy 项取 Anarchy 自带语言包（Anarchy.SECTION_TITLE[...]），该语言缺失才自行翻译。
	///
	/// v0.8.0 变化（owner 需求 1~2：面板行要按游戏真实行拆开，不能再合并）：
	///  * 拆行：原「笔刷」一项含 brushSize + brushStrength 两个值，现拆成**笔刷粗细 / 笔刷强度**
	///    两项（owner：不要合一起，全部独立拆开设置共用范围）。桶名不变，各自沿用原桶。
	///  * 新增**笔刷类型**（ToolBaseSystem.brushType，Toolbar.BRUSH_TYPE）、
	///    **目标高度**（ToolBaseSystem.brushHeight，Toolbar.BRUSH_HEIGHT，仅地形 Level/Slope）、
	///    **年龄**（ObjectToolSystem.ageMask，Toolbar.VEGETATION_AGE_PANEL_TITLE，仅树木）、
	///    **距离**（ObjectToolSystem.distance，Toolbar.TOOL_MODE_DISTANCE，仅线/曲线模式）。
	///  * 行归属按 <c>research/round4/vanilla_rows.md</c> 的工具×行矩阵定，不再按印象拍：
	///    每一项的 <see cref="ToolItemDef.places"/> 写明「出现在哪几类功能里」（owner 需求 2）。
	///  * 配色（color）**只在路线工具**出现（Game.dll 里 IColored 唯一实现者是 RoutePrefab），
	///    路线工具属功能域 → 配色从建筑与服务页移到功能页。
	///
	/// v0.7.0 变化（owner 需求 1~7）：
	///  * 三域拆分：建筑与服务 / 功能 / 道具，各自一套同名工具项，记忆键靠域前缀隔开，互不共用；
	///  * 「其它」取消：它原来装的配色（placementColor 三个通道）独立成「配色」一项；
	///  * 「同类资产」统一改叫「同类」，功能域按 6 个功能分类归组、道具域按 8 个道具分类归组。
	/// </summary>
	public static class ToolItemCatalog
	{
		// ---- 资产域 ----
		public const string kThemes = "themes";
		public const string kPacks = "packs";
		/// <summary>工具模式的记忆桶名（v0.9.2 起按工具类别拆行，桶名不变以保住老存档）。</summary>
		public const string kToolMode = "toolMode";
		public const string kToolModeNet = "toolMode.net";
		public const string kToolModeObj = "toolMode.obj";
		public const string kElevation = "elevation";
		public const string kParallel = "parallel";
		/// <summary>对齐的记忆桶名（同上，拆行后桶名不变）。</summary>
		public const string kSnap = "snap";
		public const string kSnapNet = "snap.net";
		public const string kSnapObj = "snap.obj";
		public const string kTopography = "topography";
		public const string kUnderground = "underground";
		/// <summary>
		/// 笔刷粗细（ToolBaseSystem.brushSize，Toolbar.BRUSH_SIZE）。
		/// v0.8.0 从原「笔刷」一项拆出（owner：不要合一起，全部独立拆开）。
		/// 桶名沿用 <c>other.brushSize</c>，玩家已记下的值不受影响。
		/// </summary>
		public const string kBrushSize = "brushSize";
		/// <summary>笔刷强度（ToolBaseSystem.brushStrength，Toolbar.BRUSH_STRENGTH）。同上拆出，桶名 <c>other.brushStrength</c>。</summary>
		public const string kBrushStrength = "brushStrength";
		/// <summary>
		/// 笔刷类型（ToolBaseSystem.brushType，Toolbar.BRUSH_TYPE，zh 官方未译仍显示 "Brush Type"）。
		/// 出现条件：<c>AllowBrush()</c> 为真 —— 摆件工具（资产带 Brushable）与全部地形改造工具。
		/// 值是一个 <c>BrushPrefab</c> 实体 → 记忆为实体名（不是数字），见 ToolMemoryBridge。
		/// </summary>
		public const string kBrushType = "brushType";
		/// <summary>
		/// 目标高度（ToolBaseSystem.brushHeight，Toolbar.BRUSH_HEIGHT）。
		/// **只有地形工具的 Level（平整）/ Slope（斜坡）两种资产**出现这一行（ToolUISystem L184），
		/// 摆件刷涂没有它。写回时游戏内部存的是 height + SeaLevel（ToolUISystem L657-660）。
		/// </summary>
		public const string kBrushHeight = "brushHeight";
		/// <summary>
		/// 年龄（ObjectToolSystem.ageMask，Toolbar.VEGETATION_AGE_PANEL_TITLE）。
		/// 只在选中**带 TreeData 的资产**（树）时出现，且仅游戏模式（ObjectToolSystem L2418）。
		/// v0.8.0 起树木归道具页（owner 定），所以这一项在道具页。
		/// </summary>
		public const string kAgeMask = "ageMask";
		/// <summary>
		/// 距离（ObjectToolSystem.distance，Toolbar.TOOL_MODE_DISTANCE）。
		/// 是「工具模式」下的**子行**，只在摆件工具的 Line（5）/ Curve（6）模式出现（ToolUISystem L149-150）。
		/// 单独成项（owner：全部独立拆开），共用范围独立于工具模式。
		/// </summary>
		public const string kDistance = "distance";
		// v0.8.0：配色（原 kColor = "color"）与保留年代（原 kDecoration = "decoration"）
		// 都已不在本页 —— 配色归功能页（kFColor，只有路线工具有这一行），
		// 保留年代归道具页（kIDecoration，只有树有这一行）。两个旧常量已删除：
		// 留着它们只会让人以为「建筑与服务页还有 color / decoration 这一项」。

		// ---- Anarchy（资产页的第三方板块）----
		public const string kAnarchy = "anarchy";
		public const string kLeftRight = "leftRight";
		public const string kGeneral = "general";
		/// <summary>
		/// Anarchy 注入在摆件工具面板上的「物件高度」一排（v0.8.0 新增）。
		/// owner 2026-10-20 定：锁定变换 / 高度 / 高度变化范围 / 抬高降低幅度「这4项是同一项，
		/// 都在同一排显示，自然一起记忆和共用」；「重置高度为 0」是瞬时按钮，不列。
		/// 证据：research/round4/anarchy_rows.md A 块 ——
		///   LockElevation（AnarchyUISystem L497，初值 Settings.ElevationLock=false）
		///   ElevationValue（L491，0f）、ElevationStep（L492，1f）、ElevationVariance（L494，0f）。
		/// </summary>
		public const string kObjectElevation = "anarchyObjectElevation";

		// ---- 功能域（id 加 "f." 前缀，与资产域的同名项彻底隔开）----
		public const string kFThemes = "f.themes";
		public const string kFPacks = "f.packs";
		public const string kFToolMode = "f.toolMode";
		public const string kFToolModeZone = "f.toolMode.zone";
		public const string kFToolModeObj = "f.toolMode.obj";
		public const string kFSnap = "f.snap";
		public const string kFSnapZone = "f.snap.zone";
		public const string kFSnapArea = "f.snap.area";
		public const string kFSnapObj = "f.snap.obj";
		public const string kFTopography = "f.topography";
		public const string kFUnderground = "f.underground";
		public const string kFBrushType = "f.brushType";
		public const string kFBrushSize = "f.brushSize";
		public const string kFBrushStrength = "f.brushStrength";
		public const string kFBrushHeight = "f.brushHeight";
		public const string kFDistance = "f.distance";
		public const string kFColor = "f.color";

		// ---- 道具域（id 加 "i." 前缀）----
		// 老版的 kIToolMode / kISnap / kIBrush / kITopography / kIUnderground 保留原名
		// （都是原有项，只有「笔刷」按 owner 要求拆成三项）。
		public const string kIThemes = "i.themes";
		public const string kIPacks = "i.packs";
		public const string kIToolMode = "i.toolMode";
		public const string kIDistance = "i.distance";
		public const string kIBrushType = "i.brushType";
		public const string kIBrushSize = "i.brushSize";
		public const string kIBrushStrength = "i.brushStrength";
		public const string kIAgeMask = "i.ageMask";
		public const string kIDecoration = "i.decoration";
		public const string kISnap = "i.snap";
		public const string kITopography = "i.topography";
		public const string kIUnderground = "i.underground";
		public const string kIAnarchy = "i.anarchy";
		public const string kIObjectElevation = "i.anarchyObjectElevation";

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
		/// 笔刷粗细的桶名（ToolBaseSystem.brushSize）。
		///
		/// 桶名故意沿用 <c>other.</c> 前缀：<b>它就是记忆文件里的桶名</b>
		/// （0.1.2~0.7.0 笔刷一直挂在「其它」/「笔刷」这一项下）。v0.8.0 把原来的「笔刷」
		/// 拆成粗细 / 强度两项，项 id 变成 brushSize / brushStrength，**桶名一个都不改** ——
		/// 改桶名会让玩家已经记下的笔刷整桶变成不认识的数据。名字好不好看只是源码里的事，
		/// 抹掉玩家记过的值才是伤人的事。
		/// </summary>
		private static readonly string[] s_BrushSizeSubs = new string[] { "other.brushSize" };

		/// <summary>笔刷强度的桶名（ToolBaseSystem.brushStrength），理由同上。</summary>
		private static readonly string[] s_BrushStrengthSubs = new string[] { "other.brushStrength" };

		/// <summary>笔刷类型的桶名（ToolBaseSystem.brushType，值按 BrushPrefab 名记）。</summary>
		private static readonly string[] s_BrushTypeSubs = new string[] { "brushType" };

		/// <summary>目标高度的桶名（ToolBaseSystem.brushHeight）。</summary>
		private static readonly string[] s_BrushHeightSubs = new string[] { "brushHeight" };

		/// <summary>年龄的桶名（ObjectToolSystem.ageMask，AgeMask 位掩码）。</summary>
		private static readonly string[] s_AgeMaskSubs = new string[] { "ageMask" };

		/// <summary>距离的桶名（ObjectToolSystem.distance）。</summary>
		private static readonly string[] s_DistanceSubs = new string[] { "distance" };

		/// <summary>
		/// 「物件高度」一排的四个值（Anarchy 注入，见 kObjectElevation）。
		/// 桶名沿用 Anarchy 自己的 binding 名，便于排查：
		///   anarchyElevation         = ElevationValue（当前摆放高度，单位 m）
		///   anarchyElevationStep     = ElevationStep（每次抬高/降低的幅度，4 档离散：1/5/10/50）
		///   anarchyElevationVariance = ElevationVariance（高度变化范围，0 = 不随机）
		///   anarchyLockElevation     = LockElevation（锁定变换：锁定后拖动不改高度）
		/// 注意：Anarchy 的 ElevationStep 与**原版**道路 elevationStep（0.01~25 连续）语义不同，
		/// 绝不能合并（anarchy_rows.md Q5 明确提醒）。
		/// </summary>
		private static readonly string[] s_ObjectElevationSubs = new string[]
		{
			"anarchyElevation", "anarchyElevationStep", "anarchyElevationVariance", "anarchyLockElevation"
		};

		private static readonly string[] s_IObjectElevationSubs = new string[]
		{
			"anarchyElevation", "anarchyElevationStep", "anarchyElevationVariance", "anarchyLockElevation"
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
		private static readonly string[] s_ToolModeSubs = new string[] { "toolMode" };
		private static readonly string[] s_SnapSubs = new string[] { "snap" };
		private static readonly string[] s_FToolModeSubs = new string[] { "toolMode" };
		private static readonly string[] s_FSnapSubs = new string[] { "snap" };
		private static readonly string[] s_FTopographySubs = new string[] { "topography" };
		private static readonly string[] s_FUndergroundSubs = new string[] { "underground" };
		private static readonly string[] s_FBrushSizeSubs = new string[] { "other.brushSize" };
		private static readonly string[] s_FBrushStrengthSubs = new string[] { "other.brushStrength" };
		private static readonly string[] s_FBrushTypeSubs = new string[] { "brushType" };
		private static readonly string[] s_FBrushHeightSubs = new string[] { "brushHeight" };
		private static readonly string[] s_FDistanceSubs = new string[] { "distance" };
		private static readonly string[] s_FColorSubs = new string[]
		{
			"other.color0", "other.color1", "other.color2", "other.color3"
		};

		/// <summary>道具域各项的桶名，同上。</summary>
		private static readonly string[] s_IToolModeSubs = new string[] { "toolMode" };
		private static readonly string[] s_ISnapSubs = new string[] { "snap" };
		private static readonly string[] s_ITopographySubs = new string[] { "topography" };
		private static readonly string[] s_IUndergroundSubs = new string[] { "underground" };
		private static readonly string[] s_IBrushSizeSubs = new string[] { "other.brushSize" };
		private static readonly string[] s_IBrushStrengthSubs = new string[] { "other.brushStrength" };
		private static readonly string[] s_IBrushTypeSubs = new string[] { "brushType" };
		private static readonly string[] s_IAgeMaskSubs = new string[] { "ageMask" };
		private static readonly string[] s_IDecorationSubs = new string[] { "decoration" };
		private static readonly string[] s_IDistanceSubs = new string[] { "distance" };

		/// <summary>
		/// 资产域「保留年代」的桶名。游戏里叫 decorationMode（ToolOptions.DECORATION_MODE_TITLE），
		/// 是新出现的行，没有历史包袱，直接用可读的名字。
		/// </summary>
		private static readonly string[] s_DecorationSubs = new string[] { "decoration" };

		/// <summary>
		/// 建筑与服务页。这一份是「用工具栏资产干活的工具」的全部可记忆项：
		/// NetToolSystem（道路/轨道/管线/电线）、UpgradeToolSystem、ObjectToolSystem 选中普通建筑。
		///
		/// v0.8.0 按 research/round4/vanilla_rows.md 第 5 节反查表逐行核过；
		/// v0.9.0 按 owner 需求 4 收紧推荐：只有「原版没有同类记忆、而按同类分真有增益」的项
		/// 才留推荐，其余一律无推荐 + 出厂不开 = 完全原版行为。
		///  * 「高度」「并列模式」**只有道路工具**有（net 专属，ToolUISystem L511-543）；
		///  * 笔刷三行（种类/粗细/强度）**不在本页** —— AllowBrush 只认摆件工具的 Brushable 资产
		///    与地形工具，建筑不是可刷涂资产，放上来就是「设了没反应」；
		///  * 「配色」**不在本页** —— Game.dll 里 IColored 唯一实现者是 RoutePrefab，
		///    只有路线工具显示颜色行，已移到功能页；
		///  * 装饰物 / 树木 / 区域**不在本页** —— 已分别移到道具页与功能页（0.8.0~0.9.0），
		///    本页只剩道路/轨道/管线/电线/建筑这类「买下来盖下去」的资产，所以工具模式一行的
		///    可选值也就是直线、曲线这些，不会再出现装饰物与树木的选项。
		/// </summary>
		public static readonly ToolItemDef[] AssetItems = new ToolItemDef[]
		{
			// 原版「地区主题」是全局共用，「数据包」按组显示并切组自动重置 -> 都无推荐，出厂不开
			new ToolItemDef(kThemes, 1, ItemSource.Toolbar, (int)MemoryScope.GlobalShared, kNoRecommendation, false,
				null, false, ItemDomain.Asset, null, "net,object"),
			new ToolItemDef(kPacks, 2, ItemSource.Toolbar, (int)MemoryScope.Group, kNoRecommendation, false,
				null, false, ItemDomain.Asset, null, "net,object"),
			// v0.9.0 owner 需求 4：工具模式推荐改成「同类」。
			// 理由：原版按 m_Group（=分类）记，同一分类里换资产会串；而本页的工具模式可选值
			// 只有直线/曲线这类，按「同类」（资产服务对象）归群之后，
			// 「换到同为车行的四车道仍是直线」正是玩家想要的行为。
			// v0.9.2：按工具类别拆开 —— 道路工具的模式枚举是直线/简单曲线/网格…，
			// 建筑工具是放置单个/升级/移动…，可选值根本不同，共用一个下拉不成立。
			// 桶名仍是 toolMode（Subs），键里本来就带 $net/$obj，老存档照读。
			new ToolItemDef(kToolModeNet, 3, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true,
				s_ToolModeSubs, false, ItemDomain.Asset, null, "net", null, "net", kToolMode),
			new ToolItemDef(kToolModeObj, 4, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true,
				s_ToolModeSubs, false, ItemDomain.Asset, null, "object", "none", "obj", kToolMode),
			// 高度：原版整个会话一个值（跨资产共用），推荐「同组」= 每组分一份 -> 出厂开
			new ToolItemDef(kElevation, 5, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.Group, true,
				s_ElevationSubs, false, ItemDomain.Asset, null, "net"),
			// v0.9.0 owner 需求 4：并列模式推荐去掉、出厂关。
			// 理由：原版就按 m_Group 记，「同组」这一档与原版逐字一致，没有增益；
			// 而「全局共用 / 全局不共用」两档与原版差别也不大（并列数只在 Net 上出现，
			// 同一组内的资产共用已经覆盖绝大多数使用场景）。
			new ToolItemDef(kParallel, 6, ItemSource.Vanilla, (int)MemoryScope.Group, kNoRecommendation, false,
				s_ParallelSubs, false, ItemDomain.Asset, null, "net"),
			// 对齐：原版按 m_Group 记，本项是位掩码，跨工具混键会写非法组合 -> 推荐「同类」
			// 对齐同理（v0.9.2）：道路工具是路边/网格/节点…，建筑工具是建筑表面/网格…。
			new ToolItemDef(kSnapNet, 7, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true,
				s_SnapSubs, false, ItemDomain.Asset, null, "net", null, "net", kSnap),
			new ToolItemDef(kSnapObj, 8, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true,
				s_SnapSubs, false, ItemDomain.Asset, null, "object", "none", "obj", kSnap),
			new ToolItemDef(kTopography, 9, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.GlobalShared, false,
				null, true, ItemDomain.Asset, null, "net,object", "none"),
			// 地下模式：推荐同组 == 原版同组 -> 默认关（用户定的规则）
			new ToolItemDef(kUnderground, 10, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Group, false,
				null, true, ItemDomain.Asset, null, "net,object", "none"),

			// ---- Anarchy 板块（9..12：序号在同一标签页内连续，避免一页里出现两个「1.」）----
			// Anarchy 是第三方模组，原版完全没有对应物（VanillaScope = kVanillaNone），
			// 所以「原版档」对它无意义。v0.9.0 起本页 Anarchy 项一律无推荐、出厂关。
			new ToolItemDef(kAnarchy, 11, ItemSource.AnarchyMod, kVanillaNone, kNoRecommendation, false,
				null, true, ItemDomain.Asset, null, "anarchy", "none"),
			new ToolItemDef(kLeftRight, 12, ItemSource.AnarchyMod, kVanillaNone, kNoRecommendation, false,
				s_LeftRightSubs, true, ItemDomain.Asset, null, "net,anarchy", "none"),
			new ToolItemDef(kGeneral, 13, ItemSource.AnarchyMod, kVanillaNone, kNoRecommendation, false,
				null, true, ItemDomain.Asset, null, "net,anarchy", "none"),
			// 物件高度（v0.8.0 新增）：Anarchy 注入在摆件工具面板上的**一排**高度控件。
			// owner 2026-10-20：锁定变换 / 高度 / 高度变化范围 / 抬高降低幅度「这4项是同一项」，
			// 同一排显示 → 一个工具项、一起记忆、一起共用；「重置高度为0」是瞬时按钮不列。
			// 证据：anarchy_rows.md A 块（ElevationValue 0f / ElevationStep 1f /
			// ElevationVariance 0f / LockElevation=Settings.ElevationLock），
			// 门 = 摆件工具 actualMode∈{Create,Brush,Stamp,Line,Curve} 且非建筑且 ShowElevationToolOption。
			new ToolItemDef(kObjectElevation, 14, ItemSource.AnarchyMod, kVanillaNone, kNoRecommendation, false,
				s_ObjectElevationSubs, false, ItemDomain.Asset, null, "object,anarchy", "none"),
		};

		/// <summary>
		/// 功能页：4 个功能区、3 个区域工具、所有路线工具、9 个地形改造工具、
		/// 5 个空间与区域工具、所有标记与预制对象。
		///
		/// v0.8.0 按反查表补齐并拆行：老版只有 5 项，漏了笔刷类型 / 目标高度 / 距离 / 配色 / 主题 / 数据包。
		/// v0.9.0 owner 需求 4：**本页与道具页全部无推荐、出厂全关，一律按原版行为。**
		/// 理由（已核查 research/round4/vanilla_rows.md + NetToolPreferences）：
		///  * 原版的记忆实现只有 NetToolSystem 一处，且只按 m_Group（=分类）记；
		///  * 功能型工具（Zone/Area/Route/Terrain/Bulldoze/Marker）**原版一个都不记**，
		///    所以「原版档」对它们无意义，剩下的四档全是自定义行为；
		///  * 这些工具的模式枚举与行集合差别极大（见反查表），按「同类」归组共用
		///    等于把不同枚举的值塞进同一把键，会写下玩家设不出来的组合。
		/// 因此本页不提供推荐，玩家想改共用范围自己改，出厂保持原版。
		/// </summary>
		public static readonly ToolItemDef[] FunctionItems = new ToolItemDef[]
		{
			// 主题/数据包：功能区、区域工具、路线工具、地形工具面板上都有这两行（视资产库而定）
			new ToolItemDef(kFThemes, 1, ItemSource.Toolbar, (int)MemoryScope.GlobalShared, kNoRecommendation, false,
				null, false, ItemDomain.Function, null, "zone,area,route,terrain", "zone"),
			new ToolItemDef(kFPacks, 2, ItemSource.Toolbar, (int)MemoryScope.Group, kNoRecommendation, false,
				null, false, ItemDomain.Function, null, "zone,area,route,terrain", "zone"),
			// 功能型工具的模式枚举各不相同（Zone 填充/滚动/刷涂、Area 编辑/生成、Bulldoze 主要/附属/全部），
			// 原版一个都不记；v0.9.0 起本页无推荐，出厂关 = 原版行为。
			// v0.9.2：功能页的「工具模式」其实只有功能区工具显示这一行 —— 区域工具与拆除工具
			// 游戏内都只有一种模式（GetUIModes 恒返 1 个），行是藏起来的。所以行名标出「功能区」。
			new ToolItemDef(kFToolMode, 3, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FToolModeSubs, false, ItemDomain.Function, null, "zone", "zone", "zone"),
			// v0.9.2：对齐按工具类别拆三行 —— 功能区的对齐项是「对齐现有几何 / 直方向」，
			// 区域工具多出「道路中间 / 道路侧边 / 物体表面」，标记对象又是另一套。
			new ToolItemDef(kFSnapZone, 4, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FSnapSubs, false, ItemDomain.Function, null, "zone", "zone", "zone", kFSnap),
			new ToolItemDef(kFSnapArea, 5, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FSnapSubs, false, ItemDomain.Function, null, "area", "none", "area", kFSnap),
			new ToolItemDef(kFSnapObj, 6, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FSnapSubs, false, ItemDomain.Function, null, "object", "none", "obj", kFSnap),
			new ToolItemDef(kFUnderground, 7, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FUndergroundSubs, true, ItemDomain.Function, null, "area,route,bulldoze,object", "none"),
			// 等高线：游戏里全局只有一份值（UndergroundViewSystem.globalContourLinesOn）。
			// owner 2026-10-19 明确选「三页各一份，按当前工具所属页读写」，所以功能页也有一行。
			new ToolItemDef(kFTopography, 8, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, kNoRecommendation, false,
				s_FTopographySubs, true, ItemDomain.Function, null, "zone,terrain,water,object", "none"),
			// ---- v0.8.0 拆出来的笔刷家族：种类 / 粗细 / 强度 各自独立一项（owner：不要合一起）----
			// 值都住在 ToolBaseSystem 上，地形工具与摆件工具（Brushable 资产）共用这两行。
			new ToolItemDef(kFBrushType, 9, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FBrushTypeSubs, false, ItemDomain.Function, null, "terrain,object", "terrain"),
			new ToolItemDef(kFBrushSize, 10, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FBrushSizeSubs, false, ItemDomain.Function, null, "terrain,object", "terrain"),
			new ToolItemDef(kFBrushStrength, 11, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FBrushStrengthSubs, false, ItemDomain.Function, null, "terrain,object", "terrain"),
			// 目标高度：**只有地形工具的 Level（平整）/ Slope（斜坡）**有这一行（ToolUISystem L184），
			// 其余 7 个地形资产与摆件刷涂都没有。写回时游戏存的是 height + SeaLevel（:657-660）。
			new ToolItemDef(kFBrushHeight, 12, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FBrushHeightSubs, false, ItemDomain.Function, null, "terrain", "terrain"),
			// 距离：摆件工具的线/曲线模式下的子行（ToolUISystem L149-150）。功能页的标记工具也吃它。
			new ToolItemDef(kFDistance, 13, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FDistanceSubs, false, ItemDomain.Function, null, "object,lineCurve", "none"),
			// 配色（v0.8.0 从建筑与服务页移来）：IColored 唯一实现者是 RoutePrefab -> 只有路线工具有这行。
			new ToolItemDef(kFColor, 14, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_FColorSubs, false, ItemDomain.Function, null, "route", "route"),
		};

		/// <summary>
		/// 道具页：城市美化菜单下的 8 类道具（自然 / 公园 / 住宅 / 商业 / 工业 / 残骸 / 贴花 / 盆栽）
		/// **外加植被（树）** —— owner 2026-10-20 定：树也是用摆件工具刷出来的，与公园道具同一套面板行，
		/// 所以「年龄」「保留年代」两行归这一页（它们只在带 TreeData 的资产上出现）。
		///
		/// 面板行按反查表「Object Tool」一列列全：主题 / 数据包 / 工具模式 / 距离 / 笔刷三行 /
		/// 年龄 / 保留年代 / 对齐 / 地形 / 地下模式。**没有「高度」** —— 老版道具页看到的「高度」
		/// 与「高度变化范围」其实是 Anarchy 注入的那一排（见 Anarchy 板块的「物件高度」），
		/// 原版 ObjectToolSystem 根本没有高度行（vanilla_rows.md 第 5 节：高度只有 Net Tool）。
		///
		/// v0.9.0 owner 需求 4：**本页全部无推荐、出厂全关，一律按原版行为。**
		/// 理由与功能页相同：原版只给 NetToolSystem 记偏好（按 m_Group），
		/// 摆件工具**原版一个值都不记**（ObjectToolSystem.ResetToolPreferences 只重置、不落盘），
		/// 所以「原版档」对本页无意义；而道具的模式枚举（摆放/刷涂/印章/直线/曲线）
		/// 与功能页、资产页各不相同，按「同类」共用没有原版依据。
		/// </summary>
		public static readonly ToolItemDef[] ItemItems = new ToolItemDef[]
		{
			new ToolItemDef(kIThemes, 1, ItemSource.Toolbar, (int)MemoryScope.GlobalShared, kNoRecommendation, false,
				null, false, ItemDomain.Item, null, "object", "prop"),
			new ToolItemDef(kIPacks, 2, ItemSource.Toolbar, (int)MemoryScope.Group, kNoRecommendation, false,
				null, false, ItemDomain.Item, null, "object", "prop"),
			new ToolItemDef(kIToolMode, 3, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IToolModeSubs, false, ItemDomain.Item, null, "object", "prop"),
			// 距离：只有线/曲线模式的道具才有这一行（刷涂/印章模式没有）
			new ToolItemDef(kIDistance, 4, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IDistanceSubs, false, ItemDomain.Item, null, "object,lineCurve", "prop"),
			// 笔刷家族：只有可刷涂（Brushable）/ 可印章（Stampable）的道具才有，普通静态道具没有
			new ToolItemDef(kIBrushType, 5, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IBrushTypeSubs, false, ItemDomain.Item, null, "object,brushable", "prop"),
			// 道具刷涂的粗细/强度 = 物体密度与范围（与地形笔刷的位移量不是一回事，原版自己也是两份值）
			new ToolItemDef(kIBrushSize, 6, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IBrushSizeSubs, false, ItemDomain.Item, null, "object,brushable", "prop"),
			new ToolItemDef(kIBrushStrength, 7, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IBrushStrengthSubs, false, ItemDomain.Item, null, "object,brushable", "prop"),
			// 年龄 / 保留年代：只有树（带 TreeData 的资产）且游戏模式下才有这两行
			new ToolItemDef(kIAgeMask, 8, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IAgeMaskSubs, false, ItemDomain.Item, null, "object,tree", "tree"),
			new ToolItemDef(kIDecoration, 9, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IDecorationSubs, false, ItemDomain.Item, null, "object,tree", "tree"),
			new ToolItemDef(kISnap, 10, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_ISnapSubs, false, ItemDomain.Item, null, "object", "prop"),
			new ToolItemDef(kITopography, 11, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, kNoRecommendation, false,
				s_ITopographySubs, true, ItemDomain.Item, null, "object,stampable", "none"),
			new ToolItemDef(kIUnderground, 12, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false,
				s_IUndergroundSubs, true, ItemDomain.Item, null, "object", "none"),

			// ---- Anarchy 板块（13..14）----
			// Anarchy 主开关在摆件工具上出现（白名单含 "Object Tool"），道具正是摆件工具放的
			new ToolItemDef(kIAnarchy, 13, ItemSource.AnarchyMod, kVanillaNone, kNoRecommendation, false,
				null, true, ItemDomain.Item, null, "anarchy", "none"),
			// 物件高度：与建筑与服务页同一排控件，但owner 定的「三页互不共用」要求本页也有自己的一份
			// （独立开关 + 独立共用范围 + I| 前缀，玩家在道具页调的高度不会覆盖建筑页的）。
			new ToolItemDef(kIObjectElevation, 14, ItemSource.AnarchyMod, kVanillaNone, kNoRecommendation, false,
				s_IObjectElevationSubs, false, ItemDomain.Item, null, "object,anarchy", "prop"),
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
