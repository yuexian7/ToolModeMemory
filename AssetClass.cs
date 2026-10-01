using System;
using System.Text;

namespace ToolModeMemory
{
	/// <summary>
	/// 从资产 prefab 实体上读到的「这件资产是给谁用的」事实。
	/// 只用整数与字符串承载，不引用任何游戏类型，因此本文件会被 tests/StoreHarness
	/// （离线测试工程，不引用 Game.dll）直接编译进去做回归。
	///
	/// 数值全部来自反编译 Game.dll（研究产物见 research/class30/），
	/// 位值一旦改动就必须同步改 ToolMemoryBridge 里的采集代码：
	///   Game.Net.Layer        : Road=0x1 PowerlineLow=0x2 PowerlineHigh=0x4 WaterPipe=0x8 SewagePipe=0x10
	///                           StormwaterPipe=0x20 TrainTrack=0x40 Pathway=0x80 Waterway=0x100 Taxiway=0x200
	///                           TramTrack=0x400 SubwayTrack=0x800 Fence=0x1000 MarkerPathway=0x2000
	///                           MarkerTaxiway=0x4000 PublicTransportRoad=0x8000 LaneEditor=0x10000
	///                           ResourceLine=0x20000 NetFence=0x40000
	///   Game.Net.TrackTypes   : Train=1 Tram=2 Subway=4
	///   Game.Net.UtilityTypes : WaterPipe=1 SewagePipe=2 StormwaterPipe=4 LowVoltageLine=8 Fence=0x10
	///                           Catenary=0x20 HighVoltageLine=0x40 Resource=0x80
	///   Game.City.CityService : 0 Communications 1 Districts 2 Education 3 Electricity 4 FireAndRescue
	///                           5 GarbageManagement 6 HealthcareAndDeathcare 7 Landscaping 8 ParksAndRecreation
	///                           9 PoliceAndAdministration 10 Roads 11 Transportation 12 WaterAndSewage 13 Zones
	/// </summary>
	[Flags]
	public enum AssetKind : uint
	{
		None = 0u,
		Road = 1u << 0,
		RailTrain = 1u << 1,
		RailTram = 1u << 2,
		RailSubway = 1u << 3,
		Waterway = 1u << 4,
		Pathway = 1u << 5,
		Air = 1u << 6
	}

	/// <summary>一件资产的分类事实（全部由 ToolMemoryBridge 从实体组件上采集）。</summary>
	public struct AssetFacts
	{
		/// <summary>
		/// 该资产**自己声明**的通行对象：来自 prefab 实体上的 RoadData / TrackData / WaterwayData /
		/// PathwayData / AirplaneData / TaxiwayData，以及这些组件缺失时从车道子实体
		/// （CarLaneData / TrackLaneData / PedestrianLaneData）汇总出来的对象。
		/// </summary>
		public AssetKind Kind;

		/// <summary>NetData 的 Layer 位掩码（三个字段取并集）；没有 NetData 时为 0。</summary>
		public uint Layers;

		/// <summary>UtilityObjectData / UtilityLaneData 的 UtilityTypes 位掩码并集；没有时为 0。</summary>
		public uint Utility;

		/// <summary>是否挂了 ServiceObjectData 并解析到服务实体。false 时下面两个字段无意义。</summary>
		public bool HasService;

		/// <summary>Game.City.CityService 的序号（ServiceObjectData 指向的服务实体上的 ServiceData）。</summary>
		public int ServiceOrdinal;

		/// <summary>服务 prefab 实体的资产名，只用于序号落在官方枚举之外的自定义服务。</summary>
		public string ServiceName;

		/// <summary>挂了 TreeData（树木资产）。</summary>
		public bool Tree;

		/// <summary>
		/// 全空的事实。字段刻意设计成「默认值 = 什么都不知道」（服务用 HasService 标记而不是用
		/// 序号 -1 当哨兵），这样漏填字段的后果是退化成不分类，而不是退化成某个真的类。
		/// </summary>
		public static AssetFacts Empty()
		{
			return new AssetFacts();
		}
	}

	/// <summary>
	/// 「同类资产」判定：按**这件资产服务谁**归群，而不是按工具栏把它的图标摆在哪个分类下。
	///
	/// 归并规则（owner 2026-10-01 的原话：「按对象是否一致判断」）：
	///   给车走的算一类（小巷 / 两车道道路 / 六车道道路 / 高架 / 各类桥梁 / 隧道 全部同类，
	///     车道数、宽窄、级别都不算区别）；
	///   给人走的另算一类（步行道、人行天桥、自行车道 ≠ 车行路）；
	///   火车轨道 / 地铁轨道 / 有轨电车轨道 各算一类（对象分别是火车、地铁、电车）；
	///   航道不论宽窄都是船；电缆、高压线、接触网都算「输电」这一件事，
	///     与发电站（公共服务=电力）不同类；供水管 / 污水管 / 雨水管 各按传播的东西分；
	///   建筑按它提供的公共服务分类：小学、中学同为 Education 故同类，诊所与医院同为 Healthcare 故同类。
	///
	/// 判不出对象时**宁可不合并**（返回 null，调用方退化成按单个资产记忆），
	/// 绝不猜一个会把互不相干的资产绑到一起的类。
	///
	/// 本类是纯函数：只吃 AssetFacts，不碰游戏运行时，因此可离线回归。
	/// </summary>
	public static class AssetClass
	{
		// 分类令牌（写进记忆键，短小、纯 ASCII、不含 '$' 与 '/'）。
		private const ulong T_ROAD = 1UL << 0;
		private const ulong T_TRAIN = 1UL << 1;
		private const ulong T_TRAM = 1UL << 2;
		private const ulong T_SUBWAY = 1UL << 3;
		private const ulong T_WATERWAY = 1UL << 4;
		private const ulong T_PATHWAY = 1UL << 5;
		private const ulong T_AIR = 1UL << 6;
		private const ulong T_POWER = 1UL << 7;
		private const ulong T_WATER_PIPE = 1UL << 8;
		private const ulong T_SEWAGE_PIPE = 1UL << 9;
		private const ulong T_STORM_PIPE = 1UL << 10;
		private const ulong T_RESOURCE_LINE = 1UL << 11;
		private const ulong T_FENCE = 1UL << 12;
		private const ulong T_TREE = 1UL << 13;

		private static readonly string[] TOKEN_NAME =
		{
			"road", "rail_train", "rail_tram", "rail_subway", "waterway", "pathway", "air",
			"power", "water_pipe", "sewage_pipe", "stormwater_pipe", "resource_line", "fence", "tree"
		};

		/// <summary>
		/// 「主导对象」：出现其中任何一个时，人行道与围栏只是这条线上的附属物，不参与分类。
		/// 这一条是「双车道桥与四车道桥必须同类」的关键——带不带人行道的桥都只按车算。
		/// </summary>
		private const ulong PRIMARY =
			T_ROAD | T_TRAIN | T_TRAM | T_SUBWAY | T_WATERWAY | T_AIR |
			T_POWER | T_WATER_PIPE | T_SEWAGE_PIPE | T_STORM_PIPE | T_RESOURCE_LINE;

		/// <summary>Game.Net.Layer 的位值（反编译证据见 AssetFacts 注释）。</summary>
		private const uint L_ROAD = 0x1u;
		private const uint L_POWERLINE_LOW = 0x2u;
		private const uint L_POWERLINE_HIGH = 0x4u;
		private const uint L_WATER_PIPE = 0x8u;
		private const uint L_SEWAGE_PIPE = 0x10u;
		private const uint L_STORM_PIPE = 0x20u;
		private const uint L_TRAIN = 0x40u;
		private const uint L_PATHWAY = 0x80u;
		private const uint L_WATERWAY = 0x100u;
		private const uint L_TAXIWAY = 0x200u;
		private const uint L_TRAM = 0x400u;
		private const uint L_SUBWAY = 0x800u;
		private const uint L_FENCE = 0x1000u;
		private const uint L_MARKER_PATHWAY = 0x2000u;
		private const uint L_MARKER_TAXIWAY = 0x4000u;
		private const uint L_BUS_ROAD = 0x8000u;
		private const uint L_LANE_EDITOR = 0x10000u;   // 只是资产编辑器的占位层，不参与分类
		private const uint L_RESOURCE_LINE = 0x20000u;
		private const uint L_NET_FENCE = 0x40000u;

		/// <summary>Game.Net.UtilityTypes 的位值。</summary>
		private const uint U_WATER_PIPE = 0x1u;
		private const uint U_SEWAGE_PIPE = 0x2u;
		private const uint U_STORM_PIPE = 0x4u;
		private const uint U_POWER_LOW = 0x8u;
		private const uint U_FENCE = 0x10u;
		private const uint U_CATENARY = 0x20u;
		private const uint U_POWER_HIGH = 0x40u;
		private const uint U_RESOURCE = 0x80u;

		/// <summary>Game.Net.TrackTypes 的位值。</summary>
		private const uint TT_TRAIN = 0x1u;
		private const uint TT_TRAM = 0x2u;
		private const uint TT_SUBWAY = 0x4u;

		/// <summary>Game.City.CityService 的官方序号 → 稳定令牌。顺序变了也只会让该项退化成按名/按资产，不会串类。</summary>
		private static readonly string[] SERVICE_TOKEN =
		{
			"communications", "districts", "education", "electricity", "fire_rescue",
			"garbage", "healthcare", "landscaping", "parks", "police",
			"roads", "transportation", "water_sewage", "zones"
		};

		/// <summary>记忆键里分类段的固定前缀（与旧的 UI 分类名区分开，旧键自然失效，不会串到别的资产上）。</summary>
		public const string KEY_PREFIX = "K:";

		/// <summary>
		/// 算出「同类资产」的键片段；判不出来返回 null（调用方退化成按单个资产记忆）。
		/// 永不抛异常：分类失败绝不能把异常传到游戏主循环。
		/// </summary>
		public static string Key(AssetFacts f)
		{
			try
			{
				ulong set = Tokens(f);
				if (set == 0UL)
				{
					if (f.Tree) return KEY_PREFIX + "tree";
					string svc = ServiceToken(f);
					return svc != null ? KEY_PREFIX + svc : null;
				}
				if ((set & ~T_FENCE) != 0UL) set &= ~T_FENCE;      // 围栏永远是附属物
				if ((set & PRIMARY) != 0UL) set &= ~T_PATHWAY;    // 有车 / 轨 / 船 / 电就只看那个对象

				StringBuilder sb = new StringBuilder(KEY_PREFIX, 40);
				bool first = true;
				for (int i = 0; i < TOKEN_NAME.Length; i++)
				{
					if ((set & (1UL << i)) == 0UL) continue;
					if (!first) sb.Append('+');
					first = false;
					sb.Append(TOKEN_NAME[i]);
				}
				return first ? null : sb.ToString();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// 汇总分类令牌。优先级：资产自己声明的对象 &gt; NetData 的 Layer &gt; 公用设施类型。
		/// 上一层拿到东西就不再往下看，避免「车行路的人行道层」把同类拆成两截。
		/// </summary>
		private static ulong Tokens(AssetFacts f)
		{
			ulong set = KindTokens(f.Kind);
			if (set != 0UL) return set;
			set = LayerTokens(f.Layers);
			if (set != 0UL) return set;
			return UtilityTokens(f.Utility);
		}

		/// <summary>AssetKind 位 → 令牌（车道子实体汇总上来的也走这里，语义相同）。</summary>
		private static ulong KindTokens(AssetKind kind)
		{
			ulong set = 0UL;
			if ((kind & AssetKind.Road) != 0) set |= T_ROAD;
			if ((kind & AssetKind.RailTrain) != 0) set |= T_TRAIN;
			if ((kind & AssetKind.RailTram) != 0) set |= T_TRAM;
			if ((kind & AssetKind.RailSubway) != 0) set |= T_SUBWAY;
			if ((kind & AssetKind.Waterway) != 0) set |= T_WATERWAY;
			if ((kind & AssetKind.Pathway) != 0) set |= T_PATHWAY;
			if ((kind & AssetKind.Air) != 0) set |= T_AIR;
			return set;
		}

		/// <summary>Game.Net.Layer 位掩码 → 令牌。</summary>
		private static ulong LayerTokens(uint layers)
		{
			ulong set = 0UL;
			if ((layers & (L_ROAD | L_BUS_ROAD)) != 0UL) set |= T_ROAD;
			if ((layers & L_TRAIN) != 0UL) set |= T_TRAIN;
			if ((layers & L_TRAM) != 0UL) set |= T_TRAM;
			if ((layers & L_SUBWAY) != 0UL) set |= T_SUBWAY;
			if ((layers & L_WATERWAY) != 0UL) set |= T_WATERWAY;
			if ((layers & (L_PATHWAY | L_MARKER_PATHWAY)) != 0UL) set |= T_PATHWAY;
			if ((layers & (L_TAXIWAY | L_MARKER_TAXIWAY)) != 0UL) set |= T_AIR;
			if ((layers & (L_POWERLINE_LOW | L_POWERLINE_HIGH)) != 0UL) set |= T_POWER;
			if ((layers & L_WATER_PIPE) != 0UL) set |= T_WATER_PIPE;
			if ((layers & L_SEWAGE_PIPE) != 0UL) set |= T_SEWAGE_PIPE;
			if ((layers & L_STORM_PIPE) != 0UL) set |= T_STORM_PIPE;
			if ((layers & (L_RESOURCE_LINE)) != 0UL) set |= T_RESOURCE_LINE;
			if ((layers & (L_FENCE | L_NET_FENCE)) != 0UL) set |= T_FENCE;
			return set;
		}

		/// <summary>
		/// Game.Net.UtilityTypes 位掩码 → 令牌。高压线、低压电缆、铁路接触网在玩家眼里
		/// 都是「输电」这一件事（owner 的原话：电缆的对象是传播电），所以合成一个 power；
		/// 供水 / 污水 / 雨水是三种不同的东西，保持分开的对象。
		/// </summary>
		private static ulong UtilityTokens(uint utility)
		{
			ulong set = 0UL;
			if ((utility & (U_POWER_LOW | U_POWER_HIGH | U_CATENARY)) != 0UL) set |= T_POWER;
			if ((utility & U_WATER_PIPE) != 0UL) set |= T_WATER_PIPE;
			if ((utility & U_SEWAGE_PIPE) != 0UL) set |= T_SEWAGE_PIPE;
			if ((utility & U_STORM_PIPE) != 0UL) set |= T_STORM_PIPE;
			if ((utility & U_RESOURCE) != 0UL) set |= T_RESOURCE_LINE;
			if ((utility & U_FENCE) != 0UL) set |= T_FENCE;
			return set;
		}

		/// <summary>
		/// 服务令牌：优先用官方 CityService 序号（同一服务的不同等级建筑必然同序号，
		/// 小学 / 中学就是这样合并的）；序号在官方枚举之外才退回服务资产名。
		/// </summary>
		private static string ServiceToken(AssetFacts f)
		{
			if (f.HasService && f.ServiceOrdinal >= 0 && f.ServiceOrdinal < SERVICE_TOKEN.Length)
			{
				return "svc-" + SERVICE_TOKEN[f.ServiceOrdinal];
			}
			if (!f.HasService) return null;
			string custom = Sanitize(f.ServiceName);
			return custom != null ? "svc-" + custom : null;
		}

		/// <summary>把自定义服务名压成可当键用的片段：只留字母数字与 _ -，并限长。</summary>
		private static string Sanitize(string raw)
		{
			if (string.IsNullOrEmpty(raw)) return null;
			StringBuilder sb = new StringBuilder(raw.Length);
			for (int i = 0; i < raw.Length && sb.Length < 48; i++)
			{
				char c = raw[i];
				if (c >= 'a' && c <= 'z') sb.Append(c);
				else if (c >= 'A' && c <= 'Z') sb.Append(char.ToLowerInvariant(c));
				else if (c >= '0' && c <= '9') sb.Append(c);
				else if (c == '_' || c == '-' || c == ' ') sb.Append('-');
			}
			return sb.Length == 0 ? null : sb.ToString();
		}
	}
}
