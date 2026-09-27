using System;

namespace ToolModeMemory
{
	/// <summary>可记忆的工具面板项。id 稳定，用于设置属性名与记忆文件键。</summary>
	public sealed class ToolItemDef
	{
		public readonly string Id;
		public readonly string GroupKey;
		/// <summary>原版是否记忆：-1=无记忆，0..4 同 MemoryScope。</summary>
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

	/// <summary>
	/// 记忆共用范围。游戏工具栏只有三层：菜单 → 分类 → 资产，加上两档全局。
	/// 层级对应反编译证据：
	///   资产 prefab --UIObjectData.m_Group--> 分类 --UIAssetCategoryData.m_Menu--> 菜单(UIAssetMenuData)
	/// </summary>
	public enum MemoryScope
	{
		/// <summary>同一「菜单 + 分类」内共用（= 原版 NetToolSystem 粒度）。</summary>
		Group = 0,
		/// <summary>同一菜单（道路 / 教育 …）内共用。</summary>
		Menu = 1,
		/// <summary>同名分类跨菜单共用（如道路与管道下都有的「隧道」）。</summary>
		Category = 2,
		/// <summary>所有资产共用一份。</summary>
		GlobalShared = 3,
		/// <summary>每个资产各记一份。</summary>
		GlobalUnique = 4
	}

	/// <summary>
	/// 工具项目录。顺序 = 设置页顺序。
	/// v0.1.2：删除 terrain.mode / upgrade.mode——反编译确认 TerrainToolSystem 与
	/// UpgradeToolSystem 都没有 mode 属性，其「模式」其实是选中的资产本身，无法作为工具状态记忆。
	/// </summary>
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
		public const string kBulldozeMode = "bulldoze.mode";

		public const int kVanillaNone = -1;

		public static readonly ToolItemDef[] Items = new ToolItemDef[]
		{
			new ToolItemDef(kNetDraw, "g.net", (int)MemoryScope.Group, (int)MemoryScope.Group),
			new ToolItemDef(kNetSnap, "g.net", (int)MemoryScope.Group, (int)MemoryScope.Group),
			new ToolItemDef(kNetParallel, "g.net", (int)MemoryScope.Group, (int)MemoryScope.Group),
			new ToolItemDef(kNetUnderground, "g.net", (int)MemoryScope.Group, (int)MemoryScope.Group),
			// 原版 elevation 不进 NetToolPreferences，整个会话所有资产共用一个值 = 最宽档
			new ToolItemDef(kNetElevation, "g.net", (int)MemoryScope.GlobalShared, (int)MemoryScope.Group),
			new ToolItemDef(kObjPlace, "g.obj", kVanillaNone, (int)MemoryScope.Category),
			new ToolItemDef(kObjAlign, "g.obj", kVanillaNone, (int)MemoryScope.Group),
			new ToolItemDef(kObjUnderground, "g.obj", kVanillaNone, (int)MemoryScope.Group),
			new ToolItemDef(kZoneMode, "g.zone", kVanillaNone, (int)MemoryScope.Group),
			new ToolItemDef(kAreaMode, "g.zone", kVanillaNone, (int)MemoryScope.Group),
			new ToolItemDef(kWaterMode, "g.zone", kVanillaNone, (int)MemoryScope.Group),
			new ToolItemDef(kBulldozeMode, "g.zone", kVanillaNone, (int)MemoryScope.GlobalShared),
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
