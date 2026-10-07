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

	/// <summary>可记忆的工具面板项。id 稳定，用于设置属性名与记忆文件键。</summary>
	public sealed class ToolItemDef
	{
		public readonly string Id;
		/// <summary>设置页序号（1 起）。总开关不编号。</summary>
		public readonly int Number;
		public readonly ItemSource Source;
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

		public ToolItemDef(string id, int number, ItemSource source, int vanillaScope,
			int recommendedScope, bool defaultEnabled, string[] subs = null, bool familyFreeScope = false)
		{
			Id = id;
			Number = number;
			Source = source;
			VanillaScope = vanillaScope;
			RecommendedScope = recommendedScope;
			DefaultEnabled = defaultEnabled;
			Subs = subs;
			FamilyFreeScope = familyFreeScope;
		}

		/// <summary>实际用作该项默认的范围。</summary>
		public int EffectiveRecommendedScope()
		{
			return RecommendedScope >= 0 ? RecommendedScope : (VanillaScope >= 0 ? VanillaScope : (int)MemoryScope.Group);
		}
	}

	/// <summary>
	/// 记忆共用范围。游戏工具栏只有三层：菜单 → 分类 → 资产，加上两档全局。
	/// v0.3.1 定义（与设置页说明文本一一对应）：
	///   同组   = 同一「菜单 + 分类」，如所有小型道路（菜单/分类取工具栏当前的实时层级，也就是其它模组调整完的那份）
	///   同菜单 = 整个菜单（道路 / 教育 …）；同一资产出现在两个菜单时分别记忆
	///   同类资产 = v0.3.1 起 **按资产自己服务的对象**归群（判定见 AssetClass.cs）：小巷 / 各车道数的
	///              道路 / 桥梁都是车行道路，步行道、地铁轨道、火车轨道、电缆各算一类，小学与中学
	///              同为教育设施；车道数、宽窄、级别都不算区别，工具栏把它摆在哪个分类下也不算
	///              （这一档不看工具栏层级，只读资产自己的组件，所以别的模组怎么重排分组都不影响它）；
	///              判不出类别的资产退化成按单个资产记忆
	///   全局共用 = 所有支持该项的资产/功能共用，但资产与功能之间不共用
	///   全局不共用 = 每个资产/功能各一份
	/// 例外：地区主题 / 数据包的「同类资产」仍按工具栏的（菜单+分类）发键，因为那两行的值是
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
	/// 工具项目录。顺序 = 设置页顺序，**跨板块连续编号**：
	/// 「官方工具项设置」1..9（地区主题 / 数据包 / 工具模式 / 高度 / 并列模式 / 对齐 /
	/// 地形 / 地下模式 / 其它），「Anarchy工具项设置」10..12（Anarchy / 左侧和右侧 / 常规）。
	/// 板块归属由 Source 决定：Vanilla 与 Toolbar 进官方板块，AnarchyMod 进 Anarchy 板块。
	/// 项名一律采用对应语言包里的官方行名：原版项取游戏语言包（Toolbar.* / ToolOptions.*），
	/// Anarchy 项取 Anarchy 自带语言包（Anarchy.SECTION_TITLE[...]），该语言缺失才自行翻译。
	///
	/// v0.2.2 变化：
	///  * 新增 themes / packs（工具栏的「地区主题」「数据包」筛选行，原版是
	///    ToolbarUISystem 的 UI 状态，之前版本没有实现）；
	///  * 删掉独立的 elevationStep 项：它和「高度」是同一个面板上的连体设置，
	///    用户要求「和高度要一起记忆」，所以 elevationStep 现在是 kElevation 的 Sub，
	///    共用高度的开关与范围。字段 id 保持 "elevation" / "elevationStep" 不变，
	///    所以旧记忆文件里的键照旧能读回来。
	///  * 10、11、12 三项来自 Anarchy（74604），只在该模组存在时生效。
	/// </summary>
	public static class ToolItemCatalog
	{
		public const string kThemes = "themes";
		public const string kPacks = "packs";
		public const string kAnarchy = "anarchy";
		public const string kToolMode = "toolMode";
		public const string kElevation = "elevation";
		public const string kParallel = "parallel";
		public const string kSnap = "snap";
		public const string kTopography = "topography";
		public const string kElevationStep = "elevationStep";
		public const string kLeftRight = "leftRight";
		public const string kGeneral = "general";
		public const string kUnderground = "underground";
		public const string kOther = "other";

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
		/// 「其它」的游戏字段：道路配色（NetToolSystem.placementColor，ColorSet 共 3 个通道，
		/// color3 故意留着当占位，读写都返回 false）+ 笔刷大小 / 强度（ToolBaseSystem.brushSize /
		/// brushStrength，只有 brushing 的工具真正使用）。
		/// </summary>
		private static readonly string[] s_OtherSubs = new string[]
		{
			"other.color0", "other.color1", "other.color2", "other.color3",
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

		public static readonly ToolItemDef[] Items = new ToolItemDef[]
		{
			// ---- 官方工具项设置（1..9）----
			// 原版「地区主题」是全局共用，「数据包」按组显示并切组自动重置 -> 都无推荐，出厂不开
			new ToolItemDef(kThemes, 1, ItemSource.Toolbar, (int)MemoryScope.GlobalShared, kNoRecommendation, false),
			new ToolItemDef(kPacks, 2, ItemSource.Toolbar, (int)MemoryScope.Group, kNoRecommendation, false),
			// 原版每个工具模式按分类记忆（NetToolPreferences），故推荐全局共用才有增益
			new ToolItemDef(kToolMode, 3, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.GlobalShared, true),
			new ToolItemDef(kElevation, 4, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.Group, true, s_ElevationSubs),
			new ToolItemDef(kParallel, 5, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.GlobalUnique, true, s_ParallelSubs),
			new ToolItemDef(kSnap, 6, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true),
			new ToolItemDef(kTopography, 7, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.GlobalShared, false, null, true),
			// 地下模式：推荐同组 == 原版同组 -> 默认关（用户定的规则）
			new ToolItemDef(kUnderground, 8, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Group, false, null, true),
			new ToolItemDef(kOther, 9, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false, s_OtherSubs),

			// ---- Anarchy 工具项设置（10..12）----
			new ToolItemDef(kAnarchy, 10, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Category, false, null, true),
			new ToolItemDef(kLeftRight, 11, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Group, true, s_LeftRightSubs, true),
			new ToolItemDef(kGeneral, 12, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Category, true, null, true),
		};

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
	}
}
