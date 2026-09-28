using System;
using System.Collections.Generic;
using System.Reflection;
using Colossal.Entities;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Game.UI.InGame;
using Unity.Entities;
using ToolModeMemory.Memory;

namespace ToolModeMemory
{
	/// <summary>
	/// 范围键解析 + 工具面板项的读取 / 写回（本模组碰游戏状态的唯一出口）。
	///
	/// ── 真实层级（反编译 Game.dll 证据，见 research/dll）────────────────────
	///   资产 prefab --UIObjectData.m_Group--> 分类 --UIAssetCategoryData.m_Menu--> 菜单
	///   UIObjectData 只有 m_Group / m_Priority 两个字段，不存在第三层「组」。
	///   名称取 PrefabSystem.GetPrefabName(Entity)，资产身份取 PrefabBase.GetPrefabID()，
	///   一律不用 Entity.Index（索引会随 DLC / 创意工坊加载顺序漂移，导致串记忆）。
	///
	/// ── 完整记忆键 = 域 + 层级 + 家族 ──────────────────────────────────────
	///   key = MemoryKeys.WithDomain(层级键, isFunction) + "$" + 家族
	///   例： "A|G:道路/小型道路$net"   资产域、同组、道路工具
	///        "F|S$zone"                功能区、全局共用、区域工具
	///   · 域（A| / F|）：资产与功能的可选项集合完全不同（功能区只有填充/框选/刷涂，
	///     道路有直线/简单曲线/复合曲线/连续/替换…），所以任何范围下都不许跨域共用，
	///     「全局共用」的真实含义是「所有支持该项的**资产**一份、**功能**另一份」。
	///     判据（见 IsFunction 的注释）：区划/区域/水体/地形/推土/选择这类功能型工具恒为功能；
	///     其余工具看选中的 prefab 实体是否挂了 UIObjectData（= 它在工具栏菜单/分类层级里），
	///     挂了就是资产，没挂或没选中资产就是功能。
	///   · 家族（net/obj/zone/area/water/other）：域只分「资产/功能」两类，但
	///     NetToolSystem.Mode、ObjectToolSystem.Mode、ZoneToolSystem.Mode 是**不同的枚举类型**，
	///     同一个整数在不同枚举里含义完全不同，所以再按工具家族切一层，
	///     保证选项集合永远不会跨枚举串用。
	///   记忆文件是 v3（MemoryStore.kVersion），v1/v2 的旧键在读档时按「不认识的工具项」
	///   整桶丢弃，所以这里不需要做任何键迁移。
	///
	/// ── 高程（v0.1.1 结论保留）────────────────────────────────────────────
	///   NetToolSystem.CheckElevationRange 执行 elevation = Clamp(m_DesiredElevation, range)，
	///   而 elevation 的 setter 不写 m_DesiredElevation，所以写回必须同时写私有字段
	///   m_DesiredElevation，并把 m_LastElevationRange 置成 default(Bounds1) 作废缓存，
	///   否则换资产的下一帧会用旧 desired 把新高度冲掉。
	///
	/// ── Anarchy（74604）的三项：anarchy / leftRight / general ──────────────────
	///   值住在 Anarchy 的两个 UI 世界系统里（不在 ToolBaseSystem 上），而本项目**不引用**
	///   Anarchy.dll（玩家可能没装），所以按类型全名反射解析并把 Type / FieldInfo /
	///   PropertyInfo 缓存成静态字段，实例走 World.GetOrCreateSystemManaged（见下面的
	///   Anarchy 反射段）。任何一环缺失都退化成 no-op：不外抛异常、日志最多一条。
	/// </summary>
	public static class ToolMemoryBridge
	{
		// ---- 字段 id：与 ToolItemCatalog 里 Subs 数组的字符串逐字一致 ----
		// 这里重复写字面量而不是让目录引本类的常量：ToolItemCatalog.cs 还要被离线测试工程
		// （tests/StoreHarness）单独编译，不能反向依赖带游戏类型的本类。
		private const string F_TOOL_MODE = "toolMode";
		private const string F_ELEVATION = "elevation";
		private const string F_PARALLEL_COUNT = "parallel.count";
		private const string F_PARALLEL_OFFSET = "parallel.offset";
		private const string F_SNAP = "snap";
		private const string F_TOPOGRAPHY = "topography";
		private const string F_ELEVATION_STEP = "elevationStep";
		private const string F_UNDERGROUND = "underground";
		private const string F_COLOR0 = "other.color0";
		private const string F_COLOR1 = "other.color1";
		private const string F_COLOR2 = "other.color2";
		private const string F_COLOR3 = "other.color3";
		private const string F_BRUSH_SIZE = "other.brushSize";
		private const string F_BRUSH_STRENGTH = "other.brushStrength";
		private const string F_ANARCHY = "anarchy";
		private const string F_LR_LEFT = "leftRight.left";
		private const string F_LR_RIGHT = "leftRight.right";
		private const string F_GENERAL = "general";

		private const string FAM_NET = "net";
		private const string FAM_OBJ = "obj";
		private const string FAM_ZONE = "zone";
		private const string FAM_AREA = "area";
		private const string FAM_WATER = "water";
		private const string FAM_OTHER = "other";

		/// <summary>键里域与家族的分隔符（'$' 不会出现在 prefab 名 / PrefabID 里）。</summary>
		private const char kFamilySep = '$';

		/// <summary>浮点值统一按 ×100 存成整数（两位小数，1m 高度的话精度到 1cm）。</summary>
		private const float kScale = 100f;

		// ================= 反射句柄（静态一次解析，热路径零反射查找） =================

		private static readonly FieldInfo s_DesiredElevation =
			typeof(NetToolSystem).GetField("m_DesiredElevation", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly FieldInfo s_LastElevationRange =
			typeof(NetToolSystem).GetField("m_LastElevationRange", BindingFlags.Instance | BindingFlags.NonPublic);

		/// <summary>
		/// NetToolSystem.m_HasPlacementColor：选中 prefab 是否带 NetCustomColorData，
		/// 只有为 true 时面板上才有「配色」这一行（原版在 prefab setter 里算，见 decompiled
		/// Game.Tools/NetToolSystem.cs L5326）。没有配色选项时我们绝不写 placementColor。
		/// </summary>
		private static readonly FieldInfo s_HasPlacementColor =
			typeof(NetToolSystem).GetField("m_HasPlacementColor", BindingFlags.Instance | BindingFlags.NonPublic);

		// ---------- Anarchy（Paradox ModId 74604，程序集名 Anarchy）----------
		// 本项目**不引用** Anarchy.dll（玩家可能没装），所以这三项的成员全部按类型全名
		// 在已加载程序集里找，解析一次就存进这些静态字段，热路径只剩 GetValue/SetValue；
		// 没装模组时全部为 null，anarchy / leftRight / general 三项自然退化成 no-op。
		// 事实来源：反编译 Anarchy.dll，见 research/extra/anarchy/
		//   Anarchy.Systems.Common.AnarchyUISystem : UISystemBase
		//     public bool AnarchyEnabled => m_AnarchyEnabled                     L179（字段 L85）
		//     OnUpdate: if (m_AnarchyEnabled != m_AnarchyBinding.Value) 同步绑定  L703-706
		//   Anarchy.Systems.NetworkAnarchy.NetworkAnarchyUISystem : UISystemBase
		//     public SideUpgrades LeftUpgrade  => m_LeftUpgrade.Value  与 m_LeftShowUpgrade.Value 相与   L111
		//     public SideUpgrades RightUpgrade => m_RightUpgrade.Value 与 m_RightShowUpgrade.Value 相与  L113
		//     public Composition  NetworkComposition => m_Composition.Value 与 m_ShowComposition.Value 相与 L115
		//   三个后端字段类型都是 Anarchy.Extensions.ValueBindingHelper（泛型），它的 public Value
		//   setter 就是 Binding.Update(value)（ValueBindingHelper.cs L18-21）：写它顺带把面板画对，
		//   所以不需要（也不该）自己去找 Anarchy 的 binding。
		//   SideUpgrades（位掩码）Quay=1 RetainingWall=2 Trees=4 GrassStrip=8 WideSidewalk=16
		//                     SoundBarrier=32 BikeLane=64 BikeRestriction=128
		//   Composition（位掩码） Ground=1 Elevated=2 Tunnel=4 ConstantSlope=8 WideMedian=16
		//                     Trees=32 GrassStrip=64 Lighting=128 ExpandedElevationRange=256

		private const string ANARCHY_UI_TYPE_NAME = "Anarchy.Systems.Common.AnarchyUISystem";
		private const string NETWORK_UI_TYPE_NAME = "Anarchy.Systems.NetworkAnarchy.NetworkAnarchyUISystem";

		/// <summary>Composition 的「地面 / 高架 / 隧道」三位，同一时刻只允许留一个。</summary>
		private const int kCompositionLevelMask = 7;

		private static readonly Type s_AnarchyUiType = FindLoadedType(ANARCHY_UI_TYPE_NAME);
		private static readonly PropertyInfo s_AnarchyEnabledProp = FindProperty(s_AnarchyUiType, "AnarchyEnabled");
		private static readonly FieldInfo s_AnarchyEnabledField = FindField(s_AnarchyUiType, "m_AnarchyEnabled");

		private static readonly Type s_NetworkUiType = FindLoadedType(NETWORK_UI_TYPE_NAME);
		private static readonly PropertyInfo s_LeftUpgradeProp = FindProperty(s_NetworkUiType, "LeftUpgrade");
		private static readonly PropertyInfo s_RightUpgradeProp = FindProperty(s_NetworkUiType, "RightUpgrade");
		private static readonly PropertyInfo s_CompositionProp = FindProperty(s_NetworkUiType, "NetworkComposition");
		private static readonly FieldInfo s_LeftUpgradeField = FindField(s_NetworkUiType, "m_LeftUpgrade");
		private static readonly FieldInfo s_RightUpgradeField = FindField(s_NetworkUiType, "m_RightUpgrade");
		private static readonly FieldInfo s_CompositionField = FindField(s_NetworkUiType, "m_Composition");

		/// <summary>每个 ValueBindingHelper 闭合类型自己的 public Value 属性（写回走它才带 UI 更新）。</summary>
		private static readonly PropertyInfo s_LeftHelperValue = FindValueProperty(s_LeftUpgradeField);
		private static readonly PropertyInfo s_RightHelperValue = FindValueProperty(s_RightUpgradeField);
		private static readonly PropertyInfo s_CompositionHelperValue = FindValueProperty(s_CompositionField);

		/// <summary>两个位掩码枚举类型（取自公开属性的返回类型），用来把整数造回枚举值。</summary>
		private static readonly Type s_SideUpgradesType = FindEnumType(s_LeftUpgradeProp);
		private static readonly Type s_CompositionType = FindEnumType(s_CompositionProp);

		/// <summary>World 的 GetOrCreateSystemManaged 泛型方法定义（类型参数只有运行时才知道）。</summary>
		private static readonly MethodInfo s_SystemFactory = FindSystemFactory();

		/// <summary>AnarchyUISystem 实例，按当前 World 缓存（换档由 ForgetNames 重开）。</summary>
		private static object s_AnarchyUi;
		private static bool s_AnarchyUiResolved;

		/// <summary>NetworkAnarchyUISystem 实例，同上。</summary>
		private static object s_NetworkUi;
		private static bool s_NetworkUiResolved;

		private static bool s_WarnedNoAnarchy;
		private static bool s_WarnedSystemFail;

		// ================= 缓存（每个存档清一次，见 ForgetNames） =================

		/// <summary>实体 -> prefab 名（菜单名 / 分类名）。</summary>
		private static readonly Dictionary<Entity, string> s_NameCache = new Dictionary<Entity, string>();

		/// <summary>兼容模式关闭时冻结的层级位置：[0]=菜单 [1]=分类。</summary>
		private static readonly Dictionary<PrefabBase, string[]> s_FrozenPos =
			new Dictionary<PrefabBase, string[]>();

		/// <summary>资产 / 功能判定结果，按 prefab 引用记忆。</summary>
		private static readonly Dictionary<PrefabBase, bool> s_FnCache = new Dictionary<PrefabBase, bool>();

		/// <summary>工具家族，按具体类型记忆（类型数量有限，天然有界）。</summary>
		private static readonly Dictionary<Type, string> s_FamilyCache = new Dictionary<Type, string>();

		// ResolveKey 每帧都会带进系统引用，缓存下来供只有 (tool, prefab) 的公开判据复用
		private static PrefabSystem s_CachedPrefabSystem;
		private static EntityManager s_Em;
		private static bool s_HaveSystems;

		/// <summary>地形(等高线)开关的后端系统，本档内缓存一次。</summary>
		private static UndergroundViewSystem s_ViewSystem;

		private const int kCacheCap = 4096;

		/// <summary>
		/// 「与其他模组兼容」开关（设置项 CompatOtherMods 映射过来）。
		/// true（默认）：每次都实时解析菜单/分类名——Asset UI Manager、ExtraLib 这类模组会
		/// 直接改工具栏层级数据，资产被挪到别的菜单后记忆跟着新位置走。
		/// false：本档内第一次解析到的 (菜单, 分类) 就被冻结在该 prefab 上并一直复用，
		/// 别的模组中途重排菜单不会把已有键改掉（键稳定优先于位置正确）。
		/// 只冻结解析成功（至少拿到分类名）的结果，工具栏还没建好时的空结果不冻结。
		/// </summary>
		public static bool LiveHierarchy
		{
			get { return s_LiveHierarchy; }
			set { s_LiveHierarchy = value; }
		}
		private static bool s_LiveHierarchy = true;

		/// <summary>清掉所有按存档/资产重载失效的缓存（进存档、退出存档时调用）。</summary>
		public static void ForgetNames()
		{
			s_NameCache.Clear();
			s_FrozenPos.Clear();
			s_FnCache.Clear();
			s_FamilyCache.Clear();
			s_ViewSystem = null;
			s_HaveSystems = false;
			s_CachedPrefabSystem = null;
			// Anarchy 的系统实例属于当前 World，换档必须重新取；
			// 类型与 MemberInfo 是进程级的，解析一次就够，不跟着清。
			s_AnarchyUi = null;
			s_AnarchyUiResolved = false;
			s_NetworkUi = null;
			s_NetworkUiResolved = false;
		}

		// ============================ 键解析 ============================

		/// <summary>
		/// 把一个共用范围翻成完整记忆键（域 + 层级 + 家族）。永不返回 null。
		/// </summary>
		public static string ResolveKey(MemoryScope scope, PrefabBase prefab, ToolBaseSystem tool,
			PrefabSystem prefabSystem, EntityManager em)
		{
			string levelKey;
			try
			{
				if (prefabSystem != null) s_CachedPrefabSystem = prefabSystem;
				s_Em = em;
				s_HaveSystems = true;

				switch (scope)
				{
					case MemoryScope.GlobalShared:
						return Finish(MemoryKeys.Shared(), prefab, tool);
					case MemoryScope.GlobalUnique:
						levelKey = MemoryKeys.Asset(AssetName(prefab, tool, prefabSystem));
						break;
					case MemoryScope.Menu:
					{
						string menu;
						string category;
						ResolveHierarchy(prefab, prefabSystem, em, out menu, out category);
						levelKey = MemoryKeys.Menu(menu);
						break;
					}
					case MemoryScope.Category:
					{
						string menu;
						string category;
						ResolveHierarchy(prefab, prefabSystem, em, out menu, out category);
						levelKey = MemoryKeys.Category(category);
						break;
					}
					case MemoryScope.Group:
					{
						string menu;
						string category;
						ResolveHierarchy(prefab, prefabSystem, em, out menu, out category);
						levelKey = MemoryKeys.Group(menu, category);
						break;
					}
					default:
						return Finish(MemoryKeys.Shared(), prefab, tool);
				}
			}
			catch
			{
				// 任何一步炸了都不能把异常抛给游戏主循环：退化成按工具隔离
				return Finish(ToolIdentity(tool), prefab, tool);
			}

			// 该层级解析不出来（资产不在工具栏层级里 / 没有选中资产）：退回按工具隔离，
			// 绝不能让多个互不相干的工具共用一个桶。
			if (levelKey == null) levelKey = ToolIdentity(tool);
			return Finish(levelKey, prefab, tool);
		}

		/// <summary>层级键 -> 完整键（加资产/功能域，再加家族）。</summary>
		private static string Finish(string levelKey, PrefabBase prefab, ToolBaseSystem tool)
		{
			bool isFunction = IsFunction(tool, prefab);
			string family = Family(tool);
			return MemoryKeys.WithDomain(levelKey, isFunction) + kFamilySep + family;
		}

		/// <summary>
		/// 是否「功能区」面板。两条判据（取并集）：
		/// 1) 功能型工具（区划 / 区域 / 水体 / 地形 / 推土 / 选择）永远是功能——它们的
		///    选项集合（填充-框选-刷涂、生成/编辑…）和工具栏资产完全不同，即使区划与
		///    区域样式在工具栏里也占一个菜单位置（prefab 同样挂着 UIObjectData），
		///    也绝不与建筑/道路这类资产共用同一个桶；
		/// 2) 其余工具（含第三方资产工具）看数据：选中的 prefab 挂了 UIObjectData
		///    （= 它在菜单/分类层级里）就是资产，否则就是功能；没选中资产也算功能。
		/// 结果按 prefab 引用记忆，热路径每帧最多一次组件查询。
		/// </summary>
		public static bool IsFunction(ToolBaseSystem tool, PrefabBase prefab)
		{
			if (prefab == null) return true;
			if (FunctionByToolType(tool)) return true;
			bool cached;
			if (s_FnCache.TryGetValue(prefab, out cached)) return cached;
			bool r = ComputeIsFunction(prefab);
			if (s_FnCache.Count < kCacheCap) s_FnCache[prefab] = r;
			return r;
		}

		private static bool ComputeIsFunction(PrefabBase prefab)
		{
			try
			{
				PrefabSystem ps = s_CachedPrefabSystem;
				if (ps == null || !s_HaveSystems) return true;
				Entity e = ps.GetEntity(prefab);
				if (e == Entity.Null || !s_Em.Exists(e)) return true;
				UIObjectData data;
				// 没挂 UIObjectData = 不在菜单/分类层级里 = 功能区面板
				return !s_Em.TryGetComponent(e, out data);
			}
			catch
			{
				return true;
			}
		}

		/// <summary>判据 1：按工具类型直接定为功能的面板。</summary>
		private static bool FunctionByToolType(ToolBaseSystem tool)
		{
			if (tool == null) return true;
			if (tool is ZoneToolSystem) return true;
			if (tool is AreaToolSystem) return true;
			if (tool is WaterToolSystem) return true;
			if (tool is TerrainToolSystem) return true;
			if (tool is BulldozeToolSystem) return true;
			if (tool is SelectionToolSystem) return true;
			return false;
		}

		/// <summary>
		/// 工具家族标签：只用来隔开**不同枚举类型**的选项集合（各工具的 mode 都是独立枚举）。
		/// 按类型记忆，热路径不产生字符串分配。
		/// </summary>
		public static string Family(ToolBaseSystem tool)
		{
			if (tool == null) return FAM_OTHER;
			Type t = tool.GetType();
			string fam;
			if (s_FamilyCache.TryGetValue(t, out fam)) return fam;
			fam = ComputeFamily(tool);
			if (s_FamilyCache.Count < kCacheCap) s_FamilyCache[t] = fam;
			return fam;
		}

		private static string ComputeFamily(ToolBaseSystem tool)
		{
			// ObjectToolBaseSystem 有两个子类：ObjectToolSystem 与 UpgradeToolSystem，
			// 所以这里必须精确判 ObjectToolSystem（升级工具归 other）。
			if (tool is NetToolSystem) return FAM_NET;
			if (tool is ObjectToolSystem) return FAM_OBJ;
			if (tool is ZoneToolSystem) return FAM_ZONE;
			if (tool is AreaToolSystem) return FAM_AREA;
			if (tool is WaterToolSystem) return FAM_WATER;
			return FAM_OTHER;
		}

		private static string AssetName(PrefabBase prefab, ToolBaseSystem tool, PrefabSystem prefabSystem)
		{
			if (prefab == null) return null;
			try
			{
				PrefabID id = prefab.GetPrefabID();
				string s = id.ToString();
				if (!string.IsNullOrEmpty(s)) return s;
				if (!string.IsNullOrEmpty(prefab.name)) return prefab.name;
			}
			catch { }
			return null;
		}

		private static string ToolIdentity(ToolBaseSystem tool)
		{
			return MemoryKeys.Tool(tool != null ? tool.toolID : null);
		}

		/// <summary>
		/// 解析（菜单, 分类）名。LiveHierarchy=false 时第一次成功解析的结果会冻结在该 prefab 上。
		/// 名称仍走 s_NameCache（实体 -> 名），每帧只有一次 GetEntity + 两次组件查询。
		/// </summary>
		private static void ResolveHierarchy(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em,
			out string menu, out string category)
		{
			menu = null;
			category = null;
			if (prefab == null) return;

			if (!s_LiveHierarchy)
			{
				string[] frozen;
				if (s_FrozenPos.TryGetValue(prefab, out frozen))
				{
					menu = frozen[0];
					category = frozen[1];
					return;
				}
			}

			try
			{
				Entity group = Entity.Null;
				if (prefabSystem != null)
				{
					Entity prefabEntity = prefabSystem.GetEntity(prefab);
					if (prefabEntity != Entity.Null && em.Exists(prefabEntity))
					{
						UIObjectData data;
						if (em.TryGetComponent(prefabEntity, out data)) group = data.m_Group;
					}
				}
				if (group != Entity.Null && em.Exists(group))
				{
					category = NameOf(group, prefabSystem, em);
					UIAssetCategoryData catData;
					if (em.TryGetComponent(group, out catData))
					{
						Entity menuEntity = catData.m_Menu;
						if (menuEntity != Entity.Null) menu = NameOf(menuEntity, prefabSystem, em);
					}
				}
			}
			catch
			{
				menu = null;
				category = null;
			}

			// 只冻结拿到分类名的结果：工具栏数据还没建好时（两者皆空）冻结下去
			// 会让这个资产整个存档期都退化成 T:{toolID}，反而不如实时解析。
			if (!s_LiveHierarchy && category != null && s_FrozenPos.Count < kCacheCap)
			{
				s_FrozenPos[prefab] = new string[] { menu, category };
			}
		}

		private static string NameOf(Entity e, PrefabSystem prefabSystem, EntityManager em)
		{
			if (e == Entity.Null) return null;
			string cached;
			if (s_NameCache.TryGetValue(e, out cached)) return cached;
			string name = null;
			try
			{
				if (prefabSystem != null && em.Exists(e)) name = prefabSystem.GetPrefabName(e);
			}
			catch { name = null; }
			if (string.IsNullOrEmpty(name)) name = null;
			if (s_NameCache.Count < kCacheCap) s_NameCache[e] = name;
			return name;
		}

		// ============================ 出厂默认值 ============================

		/// <summary>
		/// 未命中记忆时写回的出厂值（= 原版自己的初值，来源见每条注释）。
		/// 返回 0 且 TryApply 拒绝写 0 的字段表示「本项没有全局默认值」，
		/// 让调用方保持游戏当前值，绝不写一个会把工具搞坏的值。
		/// </summary>
		public static int DefaultValue(string fieldId)
		{
			switch (fieldId)
			{
				// 各 mode 枚举的第 0 项就是原版默认：NetToolSystem.Mode.Straight、
				// ObjectToolSystem.Mode.Create、ZoneToolSystem.Mode.FloodFill、
				// AreaToolSystem.Mode.Edit、WaterToolSystem.Mode.EditSource。
				case F_TOOL_MODE: return 0;
				// NetToolSystem.ResetToolPreferences(): elevation = 0
				case F_ELEVATION: return 0;
				// NetToolSystem.OnCreate(): parallelCount 未被显式设置（= 0，关闭并列）
				case F_PARALLEL_COUNT: return 0;
				// NetToolSystem.OnCreate(): parallelOffset = 8f
				case F_PARALLEL_OFFSET: return 800;
				// Snap.All（位掩码全 1）。写回时经 GetActualSnap 夹到该资产的 onMask，
				// 结果正好是「该资产提供的选项全开」= 原版默认，绝不会全关。
				case F_SNAP: return -1;
				// UndergroundViewSystem.OnCreate(): globalContourLinesOn = false
				case F_TOPOGRAPHY: return 0;
				// NetToolSystem.OnCreate(): elevationStep = 10f（写 0 会让 ElevationUp/Down 除零）
				case F_ELEVATION_STEP: return 1000;
				case F_UNDERGROUND: return 0;
				// 配色没有全局出厂值（每个资产的默认色在自己的 NetCustomColorData 里），
				// 0 是「无默认」哨兵，ApplyColor 见 0 直接不写。
				case F_COLOR0:
				case F_COLOR1:
				case F_COLOR2: return 0;
				// 笔刷出厂值按地形类型各存一份（TerrainToolSystem 私有 TerraformingBrushValues 列表），
				// 全局没有默认值；0 = 无默认，TryApply 见 0 直接不写，否则会把笔刷废掉。
				case F_BRUSH_SIZE:
				case F_BRUSH_STRENGTH: return 0;
				// Anarchy 三项的 0 是**真出厂值**（不是「无默认」哨兵）：AnarchyUISystem 的
				// m_AnarchyEnabled 初值 false，NetworkAnarchyUISystem 的三个绑定初值
				// SideUpgrades.None / Composition.None（OnCreate L171-173），
				// 所以写 0 就是 Anarchy 自己开局的状态，ResetActiveTool 也靠它把面板清干净。
				case F_ANARCHY: return 0;
				case F_LR_LEFT: return 0;
				case F_LR_RIGHT: return 0;
				case F_GENERAL: return 0;
				default: return 0;
			}
		}

		// ============================ 捕获 ============================

		/// <summary>
		/// 读一个字段（fieldId = 工具项 id 或其子字段 id）。返回 false = 当前工具没有这一项。
		/// 每帧对每个字段调用一次，内部不抛异常、不额外分配。
		/// </summary>
		public static bool TryCapture(string fieldId, ToolBaseSystem tool, out int value)
		{
			value = 0;
			if (tool == null || fieldId == null) return false;
			try
			{
				switch (fieldId)
				{
					case F_TOOL_MODE: return CaptureToolMode(tool, out value);
					case F_ELEVATION: return CaptureElevation(tool, out value);
					case F_ELEVATION_STEP: return CaptureElevationStep(tool, out value);
					case F_PARALLEL_COUNT: return CaptureParallelCount(tool, out value);
					case F_PARALLEL_OFFSET: return CaptureParallelOffset(tool, out value);
					case F_SNAP: return CaptureSnap(tool, out value);
					case F_TOPOGRAPHY: return CaptureTopography(out value);
					case F_UNDERGROUND: return CaptureUnderground(tool, out value);
					case F_COLOR0: return CaptureColor(tool, 0, out value);
					case F_COLOR1: return CaptureColor(tool, 1, out value);
					case F_COLOR2: return CaptureColor(tool, 2, out value);
					case F_COLOR3: return CaptureColor(tool, 3, out value);
					case F_BRUSH_SIZE: return CaptureBrush(tool, true, out value);
					case F_BRUSH_STRENGTH: return CaptureBrush(tool, false, out value);
					case F_ANARCHY: return CaptureAnarchyEnabled(out value);
					case F_LR_LEFT: return CaptureAnarchyFlags(tool, s_LeftUpgradeProp, out value);
					case F_LR_RIGHT: return CaptureAnarchyFlags(tool, s_RightUpgradeProp, out value);
					case F_GENERAL: return CaptureAnarchyFlags(tool, s_CompositionProp, out value);
					default:
						return false;
				}
			}
			catch
			{
				value = 0;
				return false;
			}
		}

		/// <summary>
		/// 工具模式：各家族各自的枚举。推土工具按需求不参与记忆；
		/// TerrainToolSystem / UpgradeToolSystem 没有 mode 属性（反编译确认，
		/// 它们的「模式」就是选中的资产本身），所以这两个工具自然返回 false。
		/// </summary>
		private static bool CaptureToolMode(ToolBaseSystem tool, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net != null) { value = (int)net.mode; return true; }
			ObjectToolSystem obj = tool as ObjectToolSystem;
			if (obj != null) { value = (int)obj.mode; return true; }
			ZoneToolSystem zone = tool as ZoneToolSystem;
			if (zone != null) { value = (int)zone.mode; return true; }
			AreaToolSystem area = tool as AreaToolSystem;
			if (area != null) { value = (int)area.mode; return true; }
			WaterToolSystem water = tool as WaterToolSystem;
			if (water != null) { value = (int)water.mode; return true; }
			return false;
		}

		/// <summary>
		/// 高度：优先读私有 m_DesiredElevation（UI 上显示的就是它，elevation 只是被
		/// CheckElevationRange 夹过一次的当前值），读不到才退回公开属性。
		/// </summary>
		private static bool CaptureElevation(ToolBaseSystem tool, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			float meters = net.elevation;
			if (s_DesiredElevation != null)
			{
				object raw = s_DesiredElevation.GetValue(net);
				if (raw is float f) meters = f;
			}
			value = (int)Math.Round(meters * kScale);
			return true;
		}

		/// <summary>高度阶段：只有道路工具有（NetToolSystem.elevationStep，UI 走 SetElevationStep）。</summary>
		private static bool CaptureElevationStep(ToolBaseSystem tool, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			value = (int)Math.Round(net.elevationStep * kScale);
			return true;
		}

		/// <summary>
		/// 并列条数。allowParallel 是**从选中 prefab 的 PlacementFlags.AllowParallel 派生**的
		/// （NetToolSystem.prefab setter），所以只读不写 parallelCount 以外的东西，
		/// 更不可能把它当状态存下来。
		/// </summary>
		private static bool CaptureParallelCount(ToolBaseSystem tool, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			value = net.parallelCount;
			return true;
		}

		private static bool CaptureParallelOffset(ToolBaseSystem tool, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			value = (int)Math.Round(net.parallelOffset * kScale);
			return true;
		}

		/// <summary>
		/// 对齐/吸附：整个 Snap 位掩码存一个 int（Snap 是 [Flags] enum : uint，
		/// 全开 = Snap.All = -1）。ToolBaseSystem.selectedSnap 是所有工具通用的虚属性，
		/// 所以不必按工具家族分支。基类 GetAvailableSnapMask 返回 (None, None) 就等于
		/// 「这个工具面板上没有对齐这一行」，此时不记录也不写回，免得把无意义的全 1
		/// 掩码灌进共用桶里。
		/// </summary>
		private static bool CaptureSnap(ToolBaseSystem tool, out int value)
		{
			value = 0;
			Snap on;
			Snap off;
			tool.GetAvailableSnapMask(out on, out off);
			if (on == Snap.None && off == Snap.None) return false;
			value = (int)tool.selectedSnap;
			return true;
		}

		/// <summary>
		/// 地形(等高线)：面板上那一行是 ToolUISystem 的 contourMode 绑定，
		/// 后端成员已核实为 Game.Rendering.UndergroundViewSystem.globalContourLinesOn
		/// （public bool，ToolUISystem.GetContourMode / SetContourMode 直接读写它，
		/// 见 decompiled Game.UI.InGame/ToolUISystem.cs L555-567、Game.Rendering/UndergroundViewSystem.cs L67）。
		/// 它是**会话全局**状态而不是工具状态（原版自己也只有一份），
		/// 所以本项的推荐范围就是「全局共用」。
		/// </summary>
		private static bool CaptureTopography(out int value)
		{
			value = 0;
			UndergroundViewSystem view = ViewSystem();
			if (view == null) return false;
			value = view.globalContourLinesOn ? 1 : 0;
			return true;
		}

		/// <summary>
		/// 地下模式：有公开 underground 属性的工具逐个取（Zone/Water/Terrain 三件反编译
		/// 确认没有这个状态，返回 false = 本项对它们不存在）。
		/// </summary>
		private static bool CaptureUnderground(ToolBaseSystem tool, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net != null) { value = net.underground ? 1 : 0; return true; }
			ObjectToolSystem obj = tool as ObjectToolSystem;
			if (obj != null) { value = obj.underground ? 1 : 0; return true; }
			AreaToolSystem area = tool as AreaToolSystem;
			if (area != null) { value = area.underground ? 1 : 0; return true; }
			BulldozeToolSystem bulldoze = tool as BulldozeToolSystem;
			if (bulldoze != null) { value = bulldoze.underground ? 1 : 0; return true; }
			RouteToolSystem route = tool as RouteToolSystem;
			if (route != null) { value = route.underground ? 1 : 0; return true; }
			return false;
		}

		private static bool CaptureColor(ToolBaseSystem tool, int channel, out int value)
		{
			value = 0;
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			if (!HasPlacementColor(net)) return false;
			// Game.Rendering.ColorSet 实测只有 m_Channel0..2 三个 UnityEngine.Color 通道
			if (channel < 0 || channel > 2) return false;
			ColorSet set = net.placementColor;
			value = PackColor(set[channel]);
			return true;
		}

		/// <summary>笔刷：只有 brushing 的工具真正使用（ToolBaseSystem.brushSize / brushStrength）。</summary>
		private static bool CaptureBrush(ToolBaseSystem tool, bool size, out int value)
		{
			value = 0;
			if (!tool.brushing) return false;
			float f = size ? tool.brushSize : tool.brushStrength;
			value = (int)Math.Round(f * kScale);
			return true;
		}

		/// <summary>
		/// Anarchy 总开关：读公开属性 AnarchyEnabled（就是私有 m_AnarchyEnabled）。
		/// 它是 Anarchy 的**会话全局**状态、不属于任何工具，所以不按工具设门槛——
		/// anarchy 这一项对 Anarchy 支持的所有工具都成立（道路、建筑、区划…）。
		/// </summary>
		private static bool CaptureAnarchyEnabled(out int value)
		{
			value = 0;
			object ui = AnarchyUi();
			if (ui == null) return false;
			object raw = null;
			if (s_AnarchyEnabledProp != null) raw = s_AnarchyEnabledProp.GetValue(ui, null);
			if (raw == null && s_AnarchyEnabledField != null) raw = s_AnarchyEnabledField.GetValue(ui);
			if (!(raw is bool)) return false;
			value = (bool)raw ? 1 : 0;
			return true;
		}

		/// <summary>
		/// 读 Anarchy 网络面板的一个位掩码（左侧 / 右侧 / 常规）。
		/// 公开 getter 已经与 m_LeftShowUpgrade / m_RightShowUpgrade / m_ShowComposition 相与，
		/// 也就是天然只含「当前这个资产面板上真有的那些选项」，正适合当记忆值。
		/// 这三项只在道路工具上存在（Anarchy 的网络面板只对 NetToolSystem 打开，
		/// 见 NetworkAnarchyUISystem.OnCreate 里它对 EventToolChanged / EventPrefabChanged 的订阅），
		/// 其他工具返回 false = 本项不存在，既不记录也不写回。
		/// </summary>
		private static bool CaptureAnarchyFlags(ToolBaseSystem tool, PropertyInfo prop, out int value)
		{
			value = 0;
			if (!(tool is NetToolSystem)) return false;
			if (prop == null) return false;
			object ui = NetworkUi();
			if (ui == null) return false;
			IConvertible flags = prop.GetValue(ui, null) as IConvertible;
			if (flags == null) return false;
			value = flags.ToInt32(null);
			return true;
		}

		// ============================ 写回 ============================

		/// <summary>写一个字段。返回 false = 当前工具没有这一项 / 该值不合法（不写）。</summary>
		public static bool TryApply(string fieldId, ToolBaseSystem tool, int value)
		{
			if (tool == null || fieldId == null) return false;
			try
			{
				switch (fieldId)
				{
					case F_TOOL_MODE: return ApplyToolMode(tool, value);
					case F_ELEVATION: return ApplyElevationField(tool, value);
					case F_ELEVATION_STEP: return ApplyElevationStep(tool, value);
					case F_PARALLEL_COUNT: return ApplyParallelCount(tool, value);
					case F_PARALLEL_OFFSET: return ApplyParallelOffset(tool, value);
					case F_SNAP: return ApplySnap(tool, value);
					case F_TOPOGRAPHY: return ApplyTopography(value);
					case F_UNDERGROUND: return ApplyUnderground(tool, value != 0);
					case F_COLOR0: return ApplyColor(tool, 0, value);
					case F_COLOR1: return ApplyColor(tool, 1, value);
					case F_COLOR2: return ApplyColor(tool, 2, value);
					case F_COLOR3: return ApplyColor(tool, 3, value);
					case F_BRUSH_SIZE: return ApplyBrush(tool, true, value);
					case F_BRUSH_STRENGTH: return ApplyBrush(tool, false, value);
					case F_ANARCHY: return ApplyAnarchyEnabled(value);
					case F_LR_LEFT: return ApplySideUpgrade(tool, s_LeftUpgradeField, s_LeftHelperValue, value);
					case F_LR_RIGHT: return ApplySideUpgrade(tool, s_RightUpgradeField, s_RightHelperValue, value);
					case F_GENERAL: return ApplyComposition(tool, value);
					default:
						return false;
				}
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 工具模式：越界一律不写（记忆文件是可手工编辑的 JSON，写进枚举里没有的值
		/// 会让原版的 switch 走空，比保持原值更糟）。上界取自各枚举最后一项。
		/// </summary>
		private static bool ApplyToolMode(ToolBaseSystem tool, int value)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net != null)
			{
				// enum Mode { Straight, SimpleCurve, ComplexCurve, Continuous, Grid, Replace, Point }
				if (value < 0 || value > 6) return false;
				net.mode = (NetToolSystem.Mode)value;
				return true;
			}
			ObjectToolSystem obj = tool as ObjectToolSystem;
			if (obj != null)
			{
				// enum Mode { Create, Upgrade, Move, Brush, Stamp, Line, Curve }
				if (value < 0 || value > 6) return false;
				obj.mode = (ObjectToolSystem.Mode)value;
				return true;
			}
			ZoneToolSystem zone = tool as ZoneToolSystem;
			if (zone != null)
			{
				// enum Mode { FloodFill, Marquee, Paint }
				if (value < 0 || value > 2) return false;
				zone.mode = (ZoneToolSystem.Mode)value;
				return true;
			}
			AreaToolSystem area = tool as AreaToolSystem;
			if (area != null)
			{
				// enum Mode { Edit, Generate }
				if (value < 0 || value > 1) return false;
				area.mode = (AreaToolSystem.Mode)value;
				return true;
			}
			WaterToolSystem water = tool as WaterToolSystem;
			if (water != null)
			{
				// enum Mode { EditSource, AddSource }
				if (value < 0 || value > 1) return false;
				water.mode = (WaterToolSystem.Mode)value;
				return true;
			}
			return false;
		}

		/// <summary>道路高度：一次写全 m_Elevation / m_DesiredElevation / m_LastElevationRange。</summary>
		private static bool ApplyElevationField(ToolBaseSystem tool, int value)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			ApplyElevation(net, value / kScale);
			return true;
		}

		/// <summary>高度阶段：0 会让 ElevationUp/Down 的 floor(e/step) 除零，绝不写。</summary>
		private static bool ApplyElevationStep(ToolBaseSystem tool, int value)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			float step = value / kScale;
			if (step <= 0f) return false;
			net.elevationStep = step;
			return true;
		}

		private static bool ApplyParallelCount(ToolBaseSystem tool, int value)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			if (value < 0) return false;
			// 资产不支持并列时原版自己会把 actualParallelCount 归 0，这里照样写下去：
			// 换到支持的资产时状态就回来了，而 allowParallel 本身是派生值，绝不写。
			net.parallelCount = value;
			return true;
		}

		private static bool ApplyParallelOffset(ToolBaseSystem tool, int value)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			net.parallelOffset = value / kScale;
			return true;
		}

		/// <summary>
		/// 吸附/对齐：写回前必须过一遍原版夹取
		///   selectedSnap = GetActualSnap((Snap)value, onMask, offMask) == (value | ~off) & on
		/// 这正是原版行为：资产 A 只给 {a,b}、资产 B 给 {a,b,c} 时，
		/// 存下来的「c 关闭」不会把 A 的 a/b 弄没（on 之外的位一律丢掉），
		/// 而 off 掩码里「本资产强制开/关」的位保持原版结论；
		/// 关掉一个双方共有的位（如等高线）才会对 A、B 同时生效。
		/// 直接赋裸掩码就会让面板显示与工具实际行为不一致（多出该资产根本没有的位）。
		/// 两个掩码都是 None 的工具（选择工具等）面板上没有这一行，直接不写。
		/// </summary>
		private static bool ApplySnap(ToolBaseSystem tool, int value)
		{
			Snap on;
			Snap off;
			tool.GetAvailableSnapMask(out on, out off);
			if (on == Snap.None && off == Snap.None) return false;
			tool.selectedSnap = ToolBaseSystem.GetActualSnap((Snap)value, on, off);
			return true;
		}

		private static bool ApplyTopography(int value)
		{
			UndergroundViewSystem view = ViewSystem();
			if (view == null) return false;
			view.globalContourLinesOn = value != 0;
			return true;
		}

		/// <summary>
		/// 地下模式。除道路外都走 ToolBaseSystem.SetUnderground（与原版 UI 的
		/// setUndergroundMode 同一条路）；道路要直接写属性：NetToolSystem 的 override 是
		/// `if (actualMode == Mode.Replace) this.underground = underground;`
		/// （decompiled Game.Tools/NetToolSystem.cs L5710-5715），普通画法下会被静默吞掉。
		/// 未知类型（第三方工具）不猜，返回 false。
		/// </summary>
		private static bool ApplyUnderground(ToolBaseSystem tool, bool on)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net != null) { net.underground = on; return true; }
			ObjectToolSystem obj = tool as ObjectToolSystem;
			if (obj != null) { obj.SetUnderground(on); return true; }
			AreaToolSystem area = tool as AreaToolSystem;
			if (area != null) { area.SetUnderground(on); return true; }
			BulldozeToolSystem bulldoze = tool as BulldozeToolSystem;
			if (bulldoze != null) { bulldoze.SetUnderground(on); return true; }
			RouteToolSystem route = tool as RouteToolSystem;
			if (route != null) { route.SetUnderground(on); return true; }
			return false;
		}

		private static bool ApplyColor(ToolBaseSystem tool, int channel, int value)
		{
			NetToolSystem net = tool as NetToolSystem;
			if (net == null) return false;
			if (!HasPlacementColor(net)) return false;
			if (channel < 0 || channel > 2) return false;
			// 0 = 「无默认值」哨兵（DefaultValue 对配色返回 0）：alpha 为 0 的颜色本身没有意义，
			// 真去写反而会把该资产 NetCustomColorData 里的出厂配色刷成全透明黑。
			if (value == 0) return false;
			// 只换指定通道，其余通道保持现值（ColorSet 有 indexer，见 Game.Rendering/ColorSet.cs）
			ColorSet set = net.placementColor;
			set[channel] = UnpackColor(value);
			net.placementColor = set;
			return true;
		}

		/// <summary>
		/// 笔刷尺寸/强度。0 当「无默认」哨兵处理：笔刷出厂值按地形类型各存一份，
		/// 全局没有可比初值，写 0 会把笔刷废掉，所以宁可不写。
		/// 上限按原版自己的最大值夹：TerrainToolSystem.kMaxBrushSizeInGame = 1000f
		/// （编辑器里是 5000f），面板滑块也超不出这个范围。
		/// </summary>
		private static bool ApplyBrush(ToolBaseSystem tool, bool size, int value)
		{
			if (value == 0) return false;
			if (!tool.brushing) return false;
			float f = value / kScale;
			if (size)
			{
				if (f <= 0f) return false;
				if (f > TerrainToolSystem.kMaxBrushSizeInGame && tool is TerrainToolSystem) f = TerrainToolSystem.kMaxBrushSizeInGame;
				tool.brushSize = f;
			}
			else
			{
				// 强度可正可负（InvertBrushesJob 会把 m_Strength 取反），只挡 NaN
				if (float.IsNaN(f) || float.IsInfinity(f)) return false;
				tool.brushStrength = f;
			}
			return true;
		}

		/// <summary>
		/// Anarchy 总开关写回：只写私有字段 m_AnarchyEnabled，不碰它的 binding。
		/// Anarchy 自己的 OnUpdate 里有
		///   if (m_AnarchyEnabled != m_AnarchyBinding.Value) m_AnarchyBinding.Value = m_AnarchyEnabled;
		/// （decompiled Anarchy.Systems.Common/AnarchyUISystem.cs L703-706），
		/// 面板下一帧自己重画；我们若去写它的绑定就是重复劳动 + 抢它自己的状态机。
		/// 也不走它的私有 AnarchyToggled()：那个除了翻转还会顺手清 m_DisableAnarchyWhenCompleted
		/// 并打开 ResetNetCompositionDataSystem，那是「玩家按下开关」的语义，不是「恢复状态」。
		/// </summary>
		private static bool ApplyAnarchyEnabled(int value)
		{
			object ui = AnarchyUi();
			if (ui == null || s_AnarchyEnabledField == null) return false;
			s_AnarchyEnabledField.SetValue(ui, value != 0);
			return true;
		}

		/// <summary>
		/// 左侧 / 右侧的 SideUpgrades 位掩码。写的是 ValueBindingHelper.Value（它的 setter
		/// 就是 Binding.Update，所以状态与面板一次到位）。门槛与捕获一致：只有道路工具才有这一行。
		/// </summary>
		private static bool ApplySideUpgrade(ToolBaseSystem tool, FieldInfo helperField, PropertyInfo helperValue, int value)
		{
			if (!(tool is NetToolSystem)) return false;
			object ui = NetworkUi();
			return WriteAnarchyFlags(ui, helperField, helperValue, s_SideUpgradesType, value, false);
		}

		/// <summary>
		/// 「常规」的 Composition 位掩码，写回前按原版语义夹过一遍（见 SanitizeComposition）。
		/// ExpandedElevationRange 会经 Anarchy 自己的 ToolUISystemGetElevationRangePatch 改掉
		/// 道路的高度范围，我们只负责把这个位放回去，绝不直接碰 ECS 的
		/// PlaceableNetData.m_ElevationRange / HeightRangeRecord。
		/// </summary>
		private static bool ApplyComposition(ToolBaseSystem tool, int value)
		{
			if (!(tool is NetToolSystem)) return false;
			object ui = NetworkUi();
			return WriteAnarchyFlags(ui, s_CompositionField, s_CompositionHelperValue, s_CompositionType, value, true);
		}

		/// <summary>
		/// 写 Anarchy 某个 ValueBindingHelper 字段：取私有 helper，再用它 public 的 Value
		/// 属性写入按整数造出来的枚举值。任一环节缺失（没装模组 / 版本改名）都返回 false no-op。
		/// </summary>
		private static bool WriteAnarchyFlags(object ui, FieldInfo helperField, PropertyInfo helperValue,
			Type enumType, int value, bool singleLevel)
		{
			if (ui == null || helperField == null || helperValue == null || enumType == null) return false;
			object helper = helperField.GetValue(ui);
			if (helper == null) return false;
			int v = singleLevel ? SanitizeComposition(value) : value;
			helperValue.SetValue(helper, Enum.ToObject(enumType, v), null);
			return true;
		}

		/// <summary>
		/// 地面 / 高架 / 隧道 三个位里最多留一个：Anarchy 自己的 Composition 点击处理是
		/// 「点的是这三档之一就先把 m_Composition 的 1-2-4 清掉，再或上新档」
		/// （decompiled NetworkAnarchyUISystem.cs L288-292 + L323-330），直接写 helper 会绕过它，
		/// 所以这里自己夹：保留最低的一位（Ground 1 &lt; Elevated 2 &lt; Tunnel 4），其余位原样带走。
		/// </summary>
		private static int SanitizeComposition(int value)
		{
			int level = value & kCompositionLevelMask;
			if (level == 0) return value;
			int lowest = level & (-level);
			return (value & ~kCompositionLevelMask) | lowest;
		}

		// ============================ 小工具 ============================

		/// <summary>
		/// 同时写 m_Elevation 与 m_DesiredElevation，并作废 m_LastElevationRange，
		/// 否则 CheckElevationRange 会在换资产时用旧 desired 冲掉新高度。
		/// </summary>
		public static void ApplyElevation(NetToolSystem net, float meters)
		{
			if (net == null) return;
			net.elevation = meters;
			if (s_DesiredElevation != null)
			{
				try { s_DesiredElevation.SetValue(net, meters); } catch { }
			}
			if (s_LastElevationRange != null)
			{
				// 置为 default(Bounds1)，迫使下次 CheckElevationRange 用新 range 重新 clamp(desired)
				try { s_LastElevationRange.SetValue(net, Activator.CreateInstance(s_LastElevationRange.FieldType)); }
				catch { }
			}
		}

		/// <summary>当前选中道路是否带自定义配色（私有 m_HasPlacementColor，取不到就当没有）。</summary>
		private static bool HasPlacementColor(NetToolSystem net)
		{
			if (s_HasPlacementColor == null) return false;
			try
			{
				object raw = s_HasPlacementColor.GetValue(net);
				return raw is bool && (bool)raw;
			}
			catch { return false; }
		}

		/// <summary>取渲染系统（本档缓存一次；UnityEngine.Object 的 == 能识别已销毁）。</summary>
		private static UndergroundViewSystem ViewSystem()
		{
			if (s_ViewSystem != null) return s_ViewSystem;
			try
			{
				World world = World.DefaultGameObjectInjectionWorld;
				if (world == null) return null;
				s_ViewSystem = world.GetExistingSystemManaged<UndergroundViewSystem>();
			}
			catch
			{
				s_ViewSystem = null;
			}
			return s_ViewSystem;
		}

		// ================= Anarchy（74604）反射解析 =================
		// 三项的值都住在 Anarchy 的 UI 系统里（世界系统），不在 ToolBaseSystem 上，
		// 所以这里和 ViewSystem() 一样从 World.DefaultGameObjectInjectionWorld 懒取实例。
		//
		// 时序：Anarchy 也订阅了 ToolSystem 的事件，若玩家在他的设置里开了
		// 「ResetNetworkToolOptionsWhenChangingPrefab」（默认关），它会在换资产时把这三项清零
		// （decompiled NetworkAnarchyUISystem.cs L157-162）。我们的 ToolMemorySystem 在每个
		// 事件之后都会于下一帧 ToolUpdate 再写回一次（OnPrefabChanged / OnToolChanged 里的
		// m_PendingApplyFrames = 1），这就是缓解——不需要也不该去调它的私有 UpdateButtonDisplay。

		/// <summary>
		/// 按类型全名在已加载程序集里找 Anarchy 的类型（本项目不引用它的 dll，玩家也可能没装）。
		/// 静态字段初始化就会调用它，所以任何一步都不许抛——一旦抛出会变成
		/// TypeInitializationException，把整个桥接类（连原版那些项）一起废掉。
		/// </summary>
		private static Type FindLoadedType(string fullName)
		{
			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					Type t = assemblies[i].GetType(fullName, false);
					if (t != null) return t;
				}
			}
			catch { }
			return null;
		}

		private static PropertyInfo FindProperty(Type type, string name)
		{
			if (type == null) return null;
			try { return type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public); }
			catch { return null; }
		}

		private static FieldInfo FindField(Type type, string name)
		{
			if (type == null) return null;
			try { return type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); }
			catch { return null; }
		}

		/// <summary>ValueBindingHelper 字段背后的 public Value 属性（每个闭合类型各取一次）。</summary>
		private static PropertyInfo FindValueProperty(FieldInfo helperField)
		{
			if (helperField == null) return null;
			return FindProperty(helperField.FieldType, "Value");
		}

		private static Type FindEnumType(PropertyInfo prop)
		{
			if (prop == null) return null;
			Type t = prop.PropertyType;
			if (t == null) return null;
			return t.IsEnum ? t : null;
		}

		/// <summary>
		/// 取 World 的 GetOrCreateSystemManaged 泛型方法：自己遍历挑「1 个类型参数 + 0 个实参」
		/// 那个重载，因为同名非泛型重载存在时 GetMethod(名字) 会抛 AmbiguousMatchException。
		/// </summary>
		private static MethodInfo FindSystemFactory()
		{
			try
			{
				MethodInfo[] methods = typeof(World).GetMethods(BindingFlags.Public | BindingFlags.Instance);
				for (int i = 0; i < methods.Length; i++)
				{
					MethodInfo m = methods[i];
					if (m.Name != "GetOrCreateSystemManaged") continue;
					if (!m.IsGenericMethodDefinition) continue;
					if (m.GetGenericArguments().Length != 1) continue;
					if (m.GetParameters().Length != 0) continue;
					return m;
				}
			}
			catch { }
			return null;
		}

		/// <summary>AnarchyUISystem 实例（懒取；没装模组 = null = 三项 no-op）。</summary>
		private static object AnarchyUi()
		{
			if (s_AnarchyUiType == null)
			{
				WarnNoAnarchy();
				return null;
			}
			return ResolveAnarchySystem(s_AnarchyUiType, ref s_AnarchyUi, ref s_AnarchyUiResolved);
		}

		/// <summary>NetworkAnarchyUISystem 实例，同上。</summary>
		private static object NetworkUi()
		{
			if (s_NetworkUiType == null)
			{
				WarnNoAnarchy();
				return null;
			}
			return ResolveAnarchySystem(s_NetworkUiType, ref s_NetworkUi, ref s_NetworkUiResolved);
		}

		/// <summary>
		/// 用 Anarchy 自己的方式拿实例（World.GetOrCreateSystemManaged，见 Anarchy.Bridge/
		/// AnarchyBridge.cs L15）：绝不 new —— UI 系统要在 World 里注册 binding。
		/// World 还没建好时不置 resolved（下一帧再试一次）；真取失败（类型不满足泛型约束等）
		/// 就 latch 成 no-op 到本档结束，换档 ForgetNames 会重开。
		/// </summary>
		private static object ResolveAnarchySystem(Type systemType, ref object cache, ref bool resolved)
		{
			if (resolved) return cache;
			if (systemType == null || s_SystemFactory == null) return null;
			try
			{
				World world = World.DefaultGameObjectInjectionWorld;
				if (world == null) return null;
				cache = s_SystemFactory.MakeGenericMethod(systemType).Invoke(world, null);
				resolved = true;
			}
			catch (Exception ex)
			{
				cache = null;
				resolved = true;
				WarnSystemFail(systemType, ex);
			}
			return cache;
		}

		/// <summary>Anarchy 不在：最多提示一次，之后静默 no-op（没装模组是常态，不该刷日志）。</summary>
		private static void WarnNoAnarchy()
		{
			if (s_WarnedNoAnarchy) return;
			s_WarnedNoAnarchy = true;
			try
			{
				ToolModeMemoryMod.log.Info("Anarchy (ModId 74604) not loaded: anarchy / leftRight / general stay inactive.");
			}
			catch { }
		}

		private static void WarnSystemFail(Type systemType, Exception ex)
		{
			if (s_WarnedSystemFail) return;
			s_WarnedSystemFail = true;
			try
			{
				ToolModeMemoryMod.log.Warn("Anarchy system " + systemType.Name + " unavailable: "
					+ ex.GetType().Name + " " + ex.Message);
			}
			catch { }
		}

		/// <summary>Color -> RGBA32 -> int（记忆文件里是一个可手工读的整数）。</summary>
		private static int PackColor(UnityEngine.Color c)
		{
			UnityEngine.Color32 b = c;
			return unchecked((int)((uint)b.r | ((uint)b.g << 8) | ((uint)b.b << 16) | ((uint)b.a << 24)));
		}

		private static UnityEngine.Color UnpackColor(int packed)
		{
			uint u = unchecked((uint)packed);
			return new UnityEngine.Color32(
				(byte)(u & 0xFFu),
				(byte)((u >> 8) & 0xFFu),
				(byte)((u >> 16) & 0xFFu),
				(byte)((u >> 24) & 0xFFu));
		}
	}
}
