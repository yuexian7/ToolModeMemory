using System;

namespace ToolModeMemory
{
	/// <summary>
	/// 一件「功能型工具选中的资产」的分类事实。只用整数与 bool 承载，不引用任何游戏类型，
	/// 所以本文件会被离线测试工程（tests/StoreHarness）直接编译进去做回归。
	///
	/// 字段来源全部是反编译 Game.dll 的组件判定（采集代码在 ToolMemoryBridge.CollectFuncFacts）：
	///   ZoneAreaType       Game.Zones.AreaType（ZoneData.m_AreaType，byte）
	///                      None=0 Residential=1 Commercial=2 Industrial=3
	///   ZoneOffice         ZoneFlags.Office（ZoneData.m_ZoneFlags &amp; 8）—— 办公区在官方枚举里
	///                      不是第四种 AreaType，而是住宅区 + Office 标志位，必须单独判
	///   AreaKind           Game.Areas.AreaType（AreaGeometryData.m_Type，int）
	///                      None=-1 Lot=0 District=1 MapTile=2 Space=3 Surface=4
	///   Extractor          ExtractorAreaData 存在（Game.Prefabs.ExtractorArea：
	///                      GetPrefabComponents → ExtractorAreaData）→ 专门产业区
	///   ParkArea           ParkData 存在（公园 / 徒步区域那类可自动生成的区域）
	///   TerraformType      Game.Prefabs.TerraformingType（TerraformingData.m_Type）
	///                      Shift=0 Level=1 Soften=2 Slope=3 Count=4
	///   TerraformTarget    Game.Prefabs.TerraformingTarget：None=-1 Height=0 Ore=1 Oil=2
	///                      FertileLand=3 GroundWater=4 Material=5 → 纹理绘制/橡皮擦一族
	///   TransportType      Game.Prefabs.TransportType（TransportLinePrefab.m_TransportType）
	///                      None=-1 Bus=0 Train=1 Taxi=2 Tram=3 Ship=4 Post=5 Helicopter=6
	///                      Airplane=7 Subway=8 Rocket=9 Work=10 Ferry=11 Bicycle=12 Car=13
	///   RouteLine          RouteData 存在（路线工具选中的就是它，TransportType 读不到时的兜底）
	///   Marker             MarkerObjectPrefab 家族：Game.Creatures.CreatureSpawner /
	///                      Game.Prefabs.Marker 等组件存在 → 标记与预制对象
	///   TerrainTool        工具本身就是 TerrainToolSystem（没选中 TerraformingPrefab 时的兜底）
	///   SpecializedIndustry ZoneFlags 之外的专门产业标记（ExtractorArea 挂在 LotPrefab 上，
	///                      专门产业区与垃圾填埋场划分走同一条路，owner 2026-10-19 明确）
	/// 判不出一律留默认值（=「什么都不知道」），后果是退化成按单个工具记忆，绝不会串到别的类。
	/// </summary>
	public struct FuncFacts
	{
		/// <summary>ZoneData.m_AreaType；-1 = 不是区划资产。</summary>
		public int ZoneAreaType;

		/// <summary>ZoneData.m_ZoneFlags &amp; ZoneFlags.Office。</summary>
		public bool ZoneOffice;

		/// <summary>AreaGeometryData.m_Type；-1 = 不是区域资产。</summary>
		public int AreaKind;

		/// <summary>ExtractorAreaData：专门产业区 / 垃圾填埋场划分。</summary>
		public bool Extractor;

		/// <summary>ParkData：公园区域 / 徒步区域（可自动生成那一族）。</summary>
		public bool ParkArea;

		/// <summary>TerraformingData.m_Type；-1 = 不是地形资产。</summary>
		public int TerraformType;

		/// <summary>TerraformingData.m_Target；-2 = 不是地形资产。</summary>
		public int TerraformTarget;

		/// <summary>TransportType；-1 = 读不到。</summary>
		public int TransportType;

		/// <summary>RouteData 存在。</summary>
		public bool RouteLine;

		/// <summary>MarkerObjectPrefab 家族。</summary>
		public bool Marker;

		/// <summary>当前工具是地形工具。</summary>
		public bool TerrainTool;

		/// <summary>当前工具是区划工具。</summary>
		public bool ZoneTool;

		/// <summary>当前工具是区域工具。</summary>
		public bool AreaTool;

		/// <summary>当前工具是路线工具。</summary>
		public bool RouteTool;

		/// <summary>当前工具是选择工具（市辖区绘制那一套）。</summary>
		public bool SelectionTool;

		/// <summary>当前工具是推土机。</summary>
		public bool BulldozeTool;

		public static FuncFacts Empty()
		{
			FuncFacts f = new FuncFacts();
			f.ZoneAreaType = -1;
			f.AreaKind = -1;
			f.TerraformType = -1;
			f.TerraformTarget = -2;
			f.TransportType = -1;
			return f;
		}
	}

	/// <summary>
	/// 功能域「同类」的纯判定。owner 2026-10-19 定案：**按 6 大类归组**，
	/// 也就是这一档只有 6 个桶，不再细分到每类内部的成员：
	///   zones    4 个功能区（住宅 / 商业 / 工业 / 办公）
	///   area     3 个区域工具（市辖区 / 专门产业区（垃圾填埋场划分走的就是它）/ 表面区域）
	///   route    交通运输菜单栏里的所有路线工具
	///   terrain  9 个地形改造工具
	///   space    5 个空间与区域工具
	///   marker   所有标记与预制对象
	/// 判不出（None）时返回 null，调用方退化成按单个工具记忆。
	///
	/// 专门产业（农业 / 林业 / 采油…）本身是**建筑资产**，不是功能区（owner 明确），
	/// 但「专门产业区放置后自动进入的那个区域工具」属于 3 个区域工具之一。
	/// </summary>
	public static class FuncClass
	{
		/// <summary>记忆键里功能分类段的固定前缀（与资产域 "K:" / 道具域 "P:" 互不撞）。</summary>
		public const string KEY_PREFIX = "F:";

		public const string TOKEN_ZONES = "zones";
		public const string TOKEN_AREA = "area";
		public const string TOKEN_ROUTE = "route";
		public const string TOKEN_TERRAIN = "terrain";
		public const string TOKEN_SPACE = "space";
		public const string TOKEN_MARKER = "marker";

		/// <summary>6 大类之一；None = 归不进去。</summary>
		public static FuncKind Kind(FuncFacts f)
		{
			// 顺序就是优先级：越具体的判据越先看。

			// 标记与预制对象（动物生成工具、生成位置、车辆生成器…）：
			// 这类资产由摆件工具放置，但玩家看到的是一条「标记」线或一只动物，
			// 与道具（有造型、可以刷一片）不是一回事。
			if (f.Marker) return FuncKind.Marker;

			// 9 个地形改造工具（TerraformingPrefab 带的 TerraformingData）。
			if (f.TerraformType >= 0) return FuncKind.Terrain;
			if (f.TerrainTool) return FuncKind.Terrain;

			// 路线工具：TransportLinePrefab / RouteData。
			if (f.RouteLine || f.RouteTool) return FuncKind.Route;

			// 4 个功能区。ZoneFlags.Office 优先判：办公区在官方枚举里
			// 是 AreaType.Residential + Office 标志（反编译 ZoneData.IsOffice 就是这么写的），
			// 不先看标志位的话办公区会被并进住宅区那一档。
			if (f.ZoneAreaType >= 0) return FuncKind.Zones;
			if (f.ZoneTool) return FuncKind.Zones;

			// 区域工具：AreaGeometryData.m_Type 分得很清
			//   District(1)  = 市辖区
			//   Surface(4)   = 表面区域
			//   Lot(0)       = 地块区域（专门产业区、垃圾填埋场划分都是 Lot，
			//                  判据是同一实体上还挂着 ExtractorAreaData）
			//   Space(3)     = 空间区域（公园 / 徒步 / 散步 / 清空那一族）
			if (f.Extractor) return FuncKind.Area;
			switch (f.AreaKind)
			{
				case 1: return FuncKind.Area;   // District
				case 4: return FuncKind.Area;   // Surface
				case 0: return f.ParkArea ? FuncKind.Space : FuncKind.Area; // Lot
				case 3: return FuncKind.Space;  // Space
				case 2: return FuncKind.Space;  // MapTile（选择工具的地块模式）
				default: break;
			}
			if (f.ParkArea) return FuncKind.Space;
			if (f.SelectionTool) return FuncKind.Space;
			if (f.AreaTool) return FuncKind.Space;

			// 推土机不是一组工具，它的面板只有「主要 / 附属 / 全部」，没有专属分类，
			// 归进「空间与区域」这一桶（它与清空区域做的事最接近）。
			if (f.BulldozeTool) return FuncKind.Space;

			return FuncKind.None;
		}

		/// <summary>6 大类 → 键片段。None 返回 null。</summary>
		public static string Token(FuncKind kind)
		{
			switch (kind)
			{
				case FuncKind.Zones: return TOKEN_ZONES;
				case FuncKind.Area: return TOKEN_AREA;
				case FuncKind.Route: return TOKEN_ROUTE;
				case FuncKind.Terrain: return TOKEN_TERRAIN;
				case FuncKind.Marker: return TOKEN_MARKER;
				case FuncKind.Space: return TOKEN_SPACE;
				default: return null;
			}
		}

		/// <summary>
		/// 「同类」的记忆键片段（形如 "F:terrain"）。判不出返回 null。
		/// 永不抛异常：分类失败绝不能把异常传到游戏主循环。
		/// </summary>
		public static string Key(FuncFacts f)
		{
			try
			{
				string tok = Token(Kind(f));
				return tok != null ? KEY_PREFIX + tok : null;
			}
			catch
			{
				return null;
			}
		}
	}
}
