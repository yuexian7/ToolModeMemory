using System;
using Colossal.Logging;
using Game;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Unity.Entities;
using ToolModeMemory.Memory;

namespace ToolModeMemory.Systems
{
	/// <summary>
	/// 捕获 / 恢复控制器。
	/// 每帧捕获（防切资产丢状态）+ 键变化立即写回 + 未命中写默认值。
	/// </summary>
	public partial class ToolMemorySystem : GameSystemBase
	{
		private ToolSystem m_ToolSystem;
		private PrefabSystem m_PrefabSystem;
		private int m_Frame;
		private int m_PendingApplyFrames;
		private bool m_EventsHooked;
		private Entity m_LastPrefabEntity;
		private string m_LastKey;
		private int m_LogCountdown;

		private static ILog log = ToolModeMemoryMod.log;

		protected override void OnCreate()
		{
			base.OnCreate();
			m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();
			HookEvents();
		}

		protected override void OnStopRunning()
		{
			UnhookEvents();
			base.OnStopRunning();
		}

		protected override void OnDestroy()
		{
			UnhookEvents();
			base.OnDestroy();
		}

		private void HookEvents()
		{
			if (m_EventsHooked || m_ToolSystem == null) return;
			m_ToolSystem.EventToolChanged += OnToolChanged;
			m_ToolSystem.EventPrefabChanged += OnPrefabChanged;
			m_EventsHooked = true;
		}

		private void UnhookEvents()
		{
			if (!m_EventsHooked || m_ToolSystem == null) return;
			try
			{
				m_ToolSystem.EventToolChanged -= OnToolChanged;
				m_ToolSystem.EventPrefabChanged -= OnPrefabChanged;
			}
			catch { }
			m_EventsHooked = false;
		}

		private void OnToolChanged(ToolBaseSystem tool)
		{
			CaptureNow();
			m_PendingApplyFrames = 1;
			m_LastPrefabEntity = Entity.Null;
			m_LastKey = null;
		}

		private void OnPrefabChanged(PrefabBase prefab)
		{
			// 原版 LoadToolPreferences 已跑完；立刻写回我们的范围键（含 elevation 双字段）
			m_PendingApplyFrames = 1;
			m_LastKey = null;
		}

		protected override void OnUpdate()
		{
			Setting setting = Setting.Instance;
			if (setting == null || !setting.Enabled)
			{
				return;
			}

			m_Frame++;

			Entity prefabEntity = Entity.Null;
			PrefabBase prefab = m_ToolSystem != null ? m_ToolSystem.activePrefab : null;
			if (prefab != null && m_PrefabSystem != null)
			{
				prefabEntity = m_PrefabSystem.GetEntity(prefab);
			}

			bool prefabChanged = prefabEntity != m_LastPrefabEntity;
			if (prefabChanged)
			{
				m_LastPrefabEntity = prefabEntity;
				m_LastKey = null;
			}

			if (m_PendingApplyFrames > 0 || prefabChanged)
			{
				if (m_PendingApplyFrames > 0) m_PendingApplyFrames--;
				ApplyNow();
			}

			// 每帧捕获：切资产前最后一帧的状态已入库
			CaptureNow();

			if (m_LogCountdown > 0)
			{
				m_LogCountdown--;
				if (m_LogCountdown == 0)
				{
					log.Info("ToolMemory tick: prefab=" + (prefab != null ? prefab.name : "null")
						+ " key=" + (m_LastKey ?? "-")
						+ " frame=" + m_Frame);
				}
			}
		}

		public void RequestApply()
		{
			m_PendingApplyFrames = 1;
			m_LastKey = null;
		}

		public void RequestDiagnostics()
		{
			m_LogCountdown = 30;
		}

		private string CurrentKey(Setting setting, PrefabBase prefab, ToolItemDef def)
		{
			MemoryScope scope = setting.GetItemScope(def.Id);
			return ToolMemoryBridge.ResolveKey(scope, prefab, m_PrefabSystem, base.EntityManager);
		}

		public void CaptureNow()
		{
			Setting setting = Setting.Instance;
			MemoryStore store = ToolModeMemoryMod.Store;
			if (setting == null || !setting.Enabled || store == null) return;
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			PrefabBase prefab = m_ToolSystem.activePrefab;
			if (prefab == null) return;

			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				if (!setting.IsItemEnabled(def.Id)) continue;
				int value;
				if (!ToolMemoryBridge.TryCapture(def.Id, tool, out value)) continue;
				string key = CurrentKey(setting, prefab, def);
				store.Set(def.Id, key, value);
				if (def.Id == ToolItemCatalog.kNetElevation)
				{
					m_LastKey = key;
				}
			}
		}

		public void ApplyNow()
		{
			Setting setting = Setting.Instance;
			MemoryStore store = ToolModeMemoryMod.Store;
			if (setting == null || !setting.Enabled || store == null) return;
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			PrefabBase prefab = m_ToolSystem.activePrefab;
			if (prefab == null) return;

			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				if (!setting.IsItemEnabled(def.Id)) continue;
				MemoryScope scope = setting.GetItemScope(def.Id);
				string key = CurrentKey(setting, prefab, def);
				bool found;
				int value = store.Get(def.Id, key, out found);
				if (!found)
				{
					// 未命中：GlobalUnique 必须写默认，避免上一资产状态泄漏；
					// elevation 也写默认（原版根本不重置高度）。
					bool needDefault = scope == MemoryScope.GlobalUnique
						|| def.Id == ToolItemCatalog.kNetElevation;
					if (!needDefault) continue;
					value = ToolMemoryBridge.DefaultValue(def.Id);
				}
				try
				{
					ToolMemoryBridge.TryApply(def.Id, tool, value);
				}
				catch (Exception ex)
				{
					log.Warn("Apply " + def.Id + " failed: " + ex.GetType().Name + " " + ex.Message);
				}
			}
		}

		public void OnEnteredGame(string saveName)
		{
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store == null) return;
			if (!string.IsNullOrEmpty(saveName))
			{
				store.SetSaveName(saveName);
			}
			else
			{
				store.EnsureSessionName();
			}
			store.LoadForCurrentSave();
			m_LastPrefabEntity = Entity.Null;
			m_LastKey = null;
			m_PendingApplyFrames = 2;
			log.Info("Memory loaded for save '" + store.SaveName + "'");
		}

		public void OnLeavingGame()
		{
			CaptureNow();
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store != null)
			{
				if (store.SaveToDisk())
				{
					log.Info("Memory saved: " + store.CurrentFilePath());
				}
			}
		}

		public void FlushIfDirty()
		{
			CaptureNow();
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store != null && store.Dirty)
			{
				store.SaveToDisk();
			}
		}
	}
}
