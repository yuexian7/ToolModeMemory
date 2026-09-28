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

		public ToolItemDef(string id, int number, ItemSource source, int vanillaScope,
			int recommendedScope, bool defaultEnabled, string[] subs = null)
		{
			Id = id;
			Number = number;
			Source = source;
			VanillaScope = vanillaScope;
			RecommendedScope = recommendedScope;
			DefaultEnabled = defaultEnabled;
			Subs = subs;
		}

		/// <summary>实际用作该项默认的范围。</summary>
		public int EffectiveRecommendedScope()
		{
			return RecommendedScope >= 0 ? RecommendedScope : (VanillaScope >= 0 ? VanillaScope : (int)MemoryScope.Group);
		}
	}

	/// <summary>
	/// 记忆共用范围。游戏工具栏只有三层：菜单 → 分类 → 资产，加上两档全局。
	/// v0.2.0 定义（与设置页说明文本一一对应）：
	///   同组   = 同一「菜单 + 分类」，如所有小型道路
	///   同菜单 = 整个菜单（道路 / 教育 …）；同一资产出现在两个菜单时分别记忆
	///   同类资产 = 同名分类跨菜单共用（小巷、地铁轨道…）；功能区/空间/区域/地形改造/
	///              标记/预制对象这类「功能」按其功能名归类
	///   全局共用 = 所有支持该项的资产/功能共用，但资产与功能之间不共用
	///   全局不共用 = 每个资产/功能各一份
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
	/// 工具项目录。顺序 = 设置页顺序（1..11），文案见 LocaleTable。
	/// 项名一律采用游戏官方语言包里的行名：工具模式 / 对齐 / 并列模式 / 高度 /
	/// 高度阶段 / 地下模式 / 地形(Topography) / 颜色等归入「其它」。
	/// 1、8、9 三项（Anarchy 开关、左侧和右侧、常规）来自 Anarchy 模组（74604），
	/// 只在该模组存在时生效（见 ToolMemoryBridge 的 Anarchy 反射段）。
	/// v0.2.0 起「数据包 / 地区主题」两项（工具栏筛选面板）推迟到 0.2.1：
	/// 它们是 ToolbarUISystem 的私有 UI 状态（m_SelectedAssetPacks / m_SelectedThemes），
	/// 不属于工具面板选项，没有可公开读写的成员。
	/// </summary>
	public static class ToolItemCatalog
	{
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
			// 原版每个工具模式按分类记忆（NetToolPreferences），故推荐全局共用才有增益
			new ToolItemDef(kAnarchy, 1, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Category, false),
			new ToolItemDef(kToolMode, 2, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.GlobalShared, true),
			// 高度：原版整个会话一个值（= 全局共用），推荐同组
			new ToolItemDef(kElevation, 3, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.Group, true),
			new ToolItemDef(kParallel, 4, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.GlobalUnique, true, s_ParallelSubs),
			new ToolItemDef(kSnap, 5, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Category, true),
			new ToolItemDef(kTopography, 6, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.GlobalShared, false),
			new ToolItemDef(kElevationStep, 7, ItemSource.Vanilla, (int)MemoryScope.GlobalShared, (int)MemoryScope.Category, true),
			new ToolItemDef(kLeftRight, 8, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Group, true, s_LeftRightSubs),
			new ToolItemDef(kGeneral, 9, ItemSource.AnarchyMod, kVanillaNone, (int)MemoryScope.Category, true),
			// 地下模式：推荐同组 == 原版同组 -> 默认关（用户定的规则）
			new ToolItemDef(kUnderground, 10, ItemSource.Vanilla, (int)MemoryScope.Group, (int)MemoryScope.Group, false),
			new ToolItemDef(kOther, 11, ItemSource.Vanilla, kVanillaNone, kNoRecommendation, false, s_OtherSubs),
		};

		public static ToolItemDef Find(string id)
		{
			for (int i = 0; i < Items.Length; i++)
			{
				if (Items[i].Id == id) return Items[i];
			}
			// 子字段 id（"parallel.count" / "other.color0" …）认回父项：
			// MemoryStore 读档时用 Find() 过滤「这一项还认识吗」，而每个子字段在记忆里
			// 是一个独立桶（键 = 子字段 id），这里不认回来的话子字段每次进档都会被丢掉。
			if (id != null)
			{
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
