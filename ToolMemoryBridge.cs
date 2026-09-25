using System;
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
	/// 键：G:group / M:menu / C:category / S / P:prefabIndex
	///
	/// 关键修复（v0.1.1）：NetToolSystem.CheckElevationRange 会执行
	///   elevation = Clamp(m_DesiredElevation, range)
	/// 只写公开 elevation 属性会在换 prefab / 高度范围变化时被 m_DesiredElevation 冲掉，
	/// 表现为「切路高度仍然共用」。写回时必须同步私有 m_DesiredElevation。
	/// </summary>
	public static class ToolMemoryBridge
	{
		private static readonly FieldInfo s_DesiredElevation =
			typeof(NetToolSystem).GetField("m_DesiredElevation", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly FieldInfo s_LastElevationRange =
			typeof(NetToolSystem).GetField("m_LastElevationRange", BindingFlags.Instance | BindingFlags.NonPublic);

		public static string ResolveKey(MemoryScope scope, PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			switch (scope)
			{
				case MemoryScope.GlobalShared:
					return "S";
				case MemoryScope.GlobalUnique:
					if (prefab == null) return "P:null";
					Entity pe = prefabSystem.GetEntity(prefab);
					return "P:" + pe.Index.ToString();
				case MemoryScope.Group:
					return "G:" + GetGroup(prefab, prefabSystem, em);
				case MemoryScope.Category:
					return "C:" + GetCategory(prefab, prefabSystem, em);
				case MemoryScope.Menu:
					return "M:" + GetMenu(prefab, prefabSystem, em);
				default:
					return "S";
			}
		}

		private static string GetGroup(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			if (prefab == null) return "null";
			Entity e = prefabSystem.GetEntity(prefab);
			UIObjectData data;
			if (em.TryGetComponent(e, out data))
			{
				return data.m_Group.Index.ToString();
			}
			return "null";
		}

		private static string GetCategory(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			if (prefab == null) return "null";
			Entity e = prefabSystem.GetEntity(prefab);
			UIObjectData data;
			if (em.TryGetComponent(e, out data))
			{
				return "c" + data.m_Group.Index.ToString();
			}
			return "null";
		}

		private static string GetMenu(PrefabBase prefab, PrefabSystem prefabSystem, EntityManager em)
		{
			if (prefab == null) return "null";
			Entity e = prefabSystem.GetEntity(prefab);
			UIObjectData data;
			if (em.TryGetComponent(e, out data))
			{
				return "m" + data.m_Group.Index.ToString();
			}
			return "null";
		}

		/// <summary>未命中记忆时的出厂默认值。</summary>
		public static int DefaultValue(string itemId)
		{
			switch (itemId)
			{
				case ToolItemCatalog.kNetElevation:
				case ToolItemCatalog.kNetUnderground:
				case ToolItemCatalog.kObjUnderground:
				case ToolItemCatalog.kNetParallel:
					return 0;
				case ToolItemCatalog.kNetDraw:
				case ToolItemCatalog.kObjPlace:
				case ToolItemCatalog.kZoneMode:
				case ToolItemCatalog.kAreaMode:
				case ToolItemCatalog.kWaterMode:
				case ToolItemCatalog.kTerrainMode:
				case ToolItemCatalog.kBulldozeMode:
				case ToolItemCatalog.kUpgradeMode:
					return 0;
				case ToolItemCatalog.kNetSnap:
				case ToolItemCatalog.kObjAlign:
					return 0;
				default:
					return 0;
			}
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
		/// 否则 CheckElevationRange 会在换路时用旧 desired 冲掉新高度。
		/// </summary>
		public static void ApplyElevation(NetToolSystem net, float meters)
		{
			if (net == null) return;
			net.elevation = meters;
			if (s_DesiredElevation != null)
			{
				s_DesiredElevation.SetValue(net, meters);
			}
			if (s_LastElevationRange != null)
			{
				// 置为不可能相等的默认值，迫使下次 CheckElevationRange 重新 clamp(desired)
				try
				{
					object current = s_LastElevationRange.GetValue(net);
					// Bounds1 是 struct；直接置 default 再改 desired 已足够，这里写回 default
					s_LastElevationRange.SetValue(net, current); // keep
					// 将 range 置为 default：用 Activator 创建默认 struct
					Type t = s_LastElevationRange.FieldType;
					s_LastElevationRange.SetValue(net, Activator.CreateInstance(t));
				}
				catch { }
			}
		}
	}
}
