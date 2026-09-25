using System;

namespace ToolModeMemory
{
	/// <summary>可记忆的工具面板项。id 稳定，用于设置属性名与记忆文件键。</summary>
	public sealed class ToolItemDef
	{
		public readonly string Id;
		public readonly string GroupKey;
		/// <summary>原版粒度：-1=无记忆，0..4 同 Scope 枚举，5=会话全局（高程）。</summary>
		public readonly int VanillaScope;
		public readonly int RecommendedScope;

		public ToolItemDef(string id, string groupKey, int vanillaScope, int recommendedScope)
		{
			Id = id;
			GroupKey = groupKey;
			VanillaScope = vanillaScope;
			RecommendedScope = recommendedScope;
		}
	}

	/// <summary>记忆共用范围。</summary>
	public enum MemoryScope
	{
		Group = 0,
		Menu = 1,
		Category = 2,
		GlobalShared = 3,
		GlobalUnique = 4
	}

	/// <summary>工具项目录。顺序 = 设置页顺序。</summary>
	public static class ToolItemCatalog
	{
		public const string kNetDraw = "net.draw";
		public const string kNetSnap = "net.snap";
		public const string kNetParallel = "net.parallel";
		public const string kNetUnderground = "net.underground";
		public const string kNetElevation = "net.elevation";
		public const string kObjPlace = "obj.place";
		public const string kObjAlign = "obj.align";
		public const string kObjUnderground = "obj.underground";
		public const string kZoneMode = "zone.mode";
		public const string kAreaMode = "area.mode";
		public const string kWaterMode = "water.mode";
		public const string kTerrainMode = "terrain.mode";
		public const string kBulldozeMode = "bulldoze.mode";
		public const string kUpgradeMode = "upgrade.mode";

		public const int kVanillaNone = -1;
		public const int kVanillaSessionGlobal = 5;

		public static readonly ToolItemDef[] Items = new ToolItemDef[]
		{
			new ToolItemDef(kNetDraw, "g.net", 0, 0),
			new ToolItemDef(kNetSnap, "g.net", 0, 0),
			new ToolItemDef(kNetParallel, "g.net", 0, 0),
			new ToolItemDef(kNetUnderground, "g.net", 0, 0),
			new ToolItemDef(kNetElevation, "g.net", kVanillaSessionGlobal, 0),
			new ToolItemDef(kObjPlace, "g.obj", kVanillaNone, 2),
			new ToolItemDef(kObjAlign, "g.obj", kVanillaNone, 0),
			new ToolItemDef(kObjUnderground, "g.obj", kVanillaNone, 0),
			new ToolItemDef(kZoneMode, "g.zone", kVanillaNone, 0),
			new ToolItemDef(kAreaMode, "g.zone", kVanillaNone, 0),
			new ToolItemDef(kWaterMode, "g.zone", kVanillaNone, 0),
			new ToolItemDef(kTerrainMode, "g.zone", kVanillaNone, 3),
			new ToolItemDef(kBulldozeMode, "g.zone", kVanillaNone, 3),
			new ToolItemDef(kUpgradeMode, "g.obj", kVanillaNone, 2),
		};

		public static ToolItemDef Find(string id)
		{
			for (int i = 0; i < Items.Length; i++)
			{
				if (Items[i].Id == id) return Items[i];
			}
			return null;
		}
	}
}
