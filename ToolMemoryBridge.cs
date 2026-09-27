using System;
using System.Collections.Generic;
using System.Reflection;
using Colossal.Entities;
using Colossal.Logging;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using Game.UI.InGame;
using Unity.Entities;
using ToolModeMemory.Memory;

namespace ToolModeMemory
{
	/// <summary>
	/// 范围键解析 + 工具状态快照/写回。
	///
	/// 真实层级（反编译 Game.dll 证据）：
	///   资产 prefab --UIObjectData.m_Group--> 分类 --UIAssetCategoryData.m_Menu--> 菜单(UIAssetMenuData)
	///   UIObjectData 只有 m_Group / m_Priority 两个字段，不存在第三层「组」。
	/// 键：
	///   Menu     = M:{菜单名}
	///   Group    = G:{菜单名}/{分类名}      （= 原版 NetToolSystem 粒度）
	///   Category = C:{分类名}              （跨菜单同名分类）
	///   GlobalShared = S
	///   GlobalUnique = P:{PrefabID}
	/// 名称一律取 PrefabSystem.GetPrefabName / PrefabBase.GetPrefabID，不用 Entity.Index
	/// （索引会随 DLC / 创意工坊资产加载顺序变化而漂移，导致串记忆）。
	///
	/// 高程（v0.1.1 结论保留）：NetToolSystem.CheckElevationRange 执行
	///   elevation = Clamp(m_DesiredElevation, range)
	/// 而 elevation setter 不写 m_DesiredElevation，所以写回必须同步私有字段。
	/// </summary>
	public static class ToolMemoryBridge
	{
		private static readonly FieldInfo s_DesiredElevation =
			typeof(NetToolSystem).GetField("m_DesiredElevation", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly FieldInfo s_LastElevationRange =
			typeof(NetToolSystem).GetField("m_LastElevationRange", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly Dictionary<Entity, string> s_NameCache = new Dictionary<Entity, string>();

		/// <summary>清掉实体名缓存（退出存档 / 资产重载时调用）。</summary>
		public static void ForgetNames()
		{
			s_NameCache.Clear();
		}

		public static string ResolveKey(MemoryScope scope, PrefabBase prefab, ToolBaseSystem tool,
			PrefabSystem prefabSystem, EntityManager em)
		{
			string key;
			switch (scope)
			{
				case MemoryScope.GlobalShared:
					return MemoryKeys.Shared();
				case MemoryScope.GlobalUnique:
					key = MemoryKeys.Asset(AssetName(prefab, tool, prefabSystem));
					break;
				case MemoryScope.Menu:
					key = MemoryKeys.Menu(MenuOf(prefab, prefabSystem, em));
					break;
				case MemoryScope.Category:
					key = MemoryKeys.Category(CategoryOf(prefab, prefabSystem, em));
					break;
				case MemoryScope.Group:
					key = MemoryKeys.Group(MenuOf(prefab, prefabSystem, em), CategoryOf(prefab, prefabSystem, em));
					break;
				default:
					return MemoryKeys.Shared();
			}
			// 该层级解析不出来（资产不在工具栏层级里 / 没有选中资产）：退回按工具隔离，
			// 绝不能让多个互不相干的工具共用一个桶。
			return key ?? ToolIdentity(tool);
		}

		// ---- 键拼装规则见 Memory/MemoryKeys.cs（纯函数，离线可测）----

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

		/// <summary>资产所属分类名；无 UIObjectData / 未分组时返回 null。</summary>
		private static string CategoryOf(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			Entity group = GroupOf(prefab, prefabSystem, em);
			if (group == Entity.Null) return null;
			return NameOf(group, prefabSystem, em);
		}

		/// <summary>资产所属分类所属菜单名。</summary>
		private static string MenuOf(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			Entity group = GroupOf(prefab, prefabSystem, em);
			if (group == Entity.Null) return null;
			try
			{
				UIAssetCategoryData catData;
				if (!em.TryGetComponent(group, out catData)) return null;
				Entity menu = catData.m_Menu;
				if (menu == Entity.Null) return null;
				return NameOf(menu, prefabSystem, em);
			}
			catch { return null; }
		}

		private static Entity GroupOf(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			if (prefab == null || prefabSystem == null) return Entity.Null;
			try
			{
				Entity e = prefabSystem.GetEntity(prefab);
				if (e == Entity.Null || !em.Exists(e)) return Entity.Null;
				UIObjectData data;
				if (!em.TryGetComponent(e, out data)) return Entity.Null;
				return data.m_Group;
			}
			catch { return Entity.Null; }
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
			if (s_NameCache.Count < 4096) s_NameCache[e] = name;
			return name;
		}

		/// <summary>未命中记忆时的出厂默认值：各枚举的第 0 项 / 高度 0 / 平行 0。</summary>
		public static int DefaultValue(string itemId)
		{
			return 0;
		}

		public static bool TryCapture(string itemId, ToolBaseSystem tool, out int value)
		{
			value = 0;
			if (tool == null) return false;
			NetToolSystem net = tool as NetToolSystem;
			if (net != null)
			{
				switch (itemId)
				{
					case ToolItemCatalog.kNetDraw:
						value = (int)net.mode;
						return true;
					case ToolItemCatalog.kNetSnap:
						value = (int)net.selectedSnap;
						return true;
					case ToolItemCatalog.kNetParallel:
						value = net.parallelCount;
						return true;
					case ToolItemCatalog.kNetUnderground:
						value = net.underground ? 1 : 0;
						return true;
					case ToolItemCatalog.kNetElevation:
						// 优先读 m_DesiredElevation（与 UI 一致），失败则读公开属性
						if (s_DesiredElevation != null)
						{
							object raw = s_DesiredElevation.GetValue(net);
							if (raw is float f) value = (int)Math.Round(f * 100f);
							else value = (int)Math.Round(net.elevation * 100f);
						}
						else
						{
							value = (int)Math.Round(net.elevation * 100f);
						}
						return true;
				}
			}
			ObjectToolSystem obj = tool as ObjectToolSystem;
			if (obj != null)
			{
				switch (itemId)
				{
					case ToolItemCatalog.kObjPlace:
						value = (int)obj.mode;
						return true;
					case ToolItemCatalog.kObjAlign:
						value = (int)obj.selectedSnap;
						return true;
					case ToolItemCatalog.kObjUnderground:
						value = obj.underground ? 1 : 0;
						return true;
				}
			}
			ZoneToolSystem zone = tool as ZoneToolSystem;
			if (zone != null && itemId == ToolItemCatalog.kZoneMode)
			{
				value = (int)zone.mode;
				return true;
			}
			AreaToolSystem area = tool as AreaToolSystem;
			if (area != null)
			{
				if (itemId == ToolItemCatalog.kAreaMode)
				{
					value = (int)area.mode;
					return true;
				}
				if (itemId == ToolItemCatalog.kObjUnderground)
				{
					value = area.underground ? 1 : 0;
					return true;
				}
			}
			WaterToolSystem water = tool as WaterToolSystem;
			if (water != null && itemId == ToolItemCatalog.kWaterMode)
			{
				value = (int)water.mode;
				return true;
			}
			BulldozeToolSystem bulldoze = tool as BulldozeToolSystem;
			if (bulldoze != null)
			{
				if (itemId == ToolItemCatalog.kBulldozeMode)
				{
					value = (int)bulldoze.mode;
					return true;
				}
				if (itemId == ToolItemCatalog.kObjUnderground)
				{
					value = bulldoze.underground ? 1 : 0;
					return true;
				}
			}
			return false;
		}

		public static bool TryApply(string itemId, ToolBaseSystem tool, int value)
		{
			if (tool == null) return false;
			NetToolSystem net = tool as NetToolSystem;
			if (net != null)
			{
				switch (itemId)
				{
					case ToolItemCatalog.kNetDraw:
						net.mode = (NetToolSystem.Mode)value;
						return true;
					case ToolItemCatalog.kNetSnap:
						net.selectedSnap = (Snap)value;
						return true;
					case ToolItemCatalog.kNetParallel:
						net.parallelCount = value;
						return true;
					case ToolItemCatalog.kNetUnderground:
						net.underground = value != 0;
						return true;
					case ToolItemCatalog.kNetElevation:
						ApplyElevation(net, value / 100f);
						return true;
				}
			}
			ObjectToolSystem obj = tool as ObjectToolSystem;
			if (obj != null)
			{
				switch (itemId)
				{
					case ToolItemCatalog.kObjPlace:
						obj.mode = (ObjectToolSystem.Mode)value;
						return true;
					case ToolItemCatalog.kObjAlign:
						obj.selectedSnap = (Snap)value;
						return true;
					case ToolItemCatalog.kObjUnderground:
						obj.underground = value != 0;
						return true;
				}
			}
			ZoneToolSystem zone = tool as ZoneToolSystem;
			if (zone != null && itemId == ToolItemCatalog.kZoneMode)
			{
				zone.mode = (ZoneToolSystem.Mode)value;
				return true;
			}
			AreaToolSystem area = tool as AreaToolSystem;
			if (area != null)
			{
				if (itemId == ToolItemCatalog.kAreaMode)
				{
					area.mode = (AreaToolSystem.Mode)value;
					return true;
				}
				if (itemId == ToolItemCatalog.kObjUnderground)
				{
					area.underground = value != 0;
					return true;
				}
			}
			WaterToolSystem water = tool as WaterToolSystem;
			if (water != null && itemId == ToolItemCatalog.kWaterMode)
			{
				water.mode = (WaterToolSystem.Mode)value;
				return true;
			}
			BulldozeToolSystem bulldoze = tool as BulldozeToolSystem;
			if (bulldoze != null)
			{
				if (itemId == ToolItemCatalog.kBulldozeMode)
				{
					bulldoze.mode = (BulldozeToolSystem.Mode)value;
					return true;
				}
				if (itemId == ToolItemCatalog.kObjUnderground)
				{
					bulldoze.underground = value != 0;
					return true;
				}
			}
			return false;
		}

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
	}
}
