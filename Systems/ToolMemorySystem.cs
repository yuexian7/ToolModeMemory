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
	/// 每帧捕获（防切资产丢状态）+ 事件与换资产当帧写回 + 未命中写默认值。
	/// 总开关关闭时 Enabled=false：不进 OnUpdate、不订阅工具事件、零分配。
	/// </summary>
	public partial class ToolMemorySystem : GameSystemBase
	{
		private ToolSystem m_ToolSystem;
		private PrefabSystem m_PrefabSystem;
		private int m_PendingApplyFrames;
		private bool m_EventsHooked;
		private Entity m_LastPrefabEntity;
		private bool m_WarnedBlocked;

		// 实时落盘防抖：改动停止 kFlushDelay 秒后写盘。崩溃也就丢这 1 秒内的改动，
		// 而且写的是 tmp+替换，不会留下半截文件。
		private const float kFlushDelay = 1f;
		private long m_SeenSerial;
		private float m_FlushAt;

		// 一次 Capture/Apply 之内最多解析 5 个范围键（11 个工具项共用），跨调用必须重开
		private int m_KeyPass;
		private readonly string[] m_Keys = new string[5];
		private readonly int[] m_KeyPasses = new int[5];

		private static ILog log = ToolModeMemoryMod.log;

		protected override void OnCreate()
		{
			base.OnCreate();
			m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			// 由 ToolModeMemorySettings 决定；默认关 = 完全不参与
			base.Enabled = false;
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
			// 必须**插到最前面**：NetToolSystem.prefab setter 里先 LoadToolPreferences()
			// 再广播 EventPrefabChanged，而 ToolUISystem.OnPrefabChanged 只是重推一次绑定值，
			// GetterValueBinding 对新旧相同的值直接去重不发事件（Colossal.UI.Binding）。
			// 我们若排在它后面写回，UI 已经按「原版默认全选」画完，这一帧之后就不再重画
			// —— 正是「切资产第一次显示不对、第二次才对」的成因（需求 7）。
			m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(
				new Action<ToolBaseSystem>(OnToolChanged), m_ToolSystem.EventToolChanged);
			m_ToolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(
				new Action<PrefabBase>(OnPrefabChanged), m_ToolSystem.EventPrefabChanged);
			m_EventsHooked = true;
		}

		private void UnhookEvents()
		{
			if (!m_EventsHooked || m_ToolSystem == null) return;
			try
			{
				m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Remove(
					m_ToolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
				m_ToolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Remove(
					m_ToolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
			}
			catch { }
			m_EventsHooked = false;
		}

		private void OnToolChanged(ToolBaseSystem tool)
		{
			// ToolSystem.activeTool 的 setter 是「先赋值后广播」，这里已经看不到旧工具了，
			// 所以不需要（也无法）在这里补捕旧工具的状态——上一帧的每帧捕获已经记下了它。
			m_LastPrefabEntity = Entity.Null;
			ApplyNow();
			m_PendingApplyFrames = 1;
		}

		private void OnPrefabChanged(PrefabBase prefab)
		{
			// 原版 LoadToolPreferences 已跑完（先加载后广播）。立刻写回一次让 UI 画对；
			// 但此刻 NetToolSystem.GetAvailableSnapMask 读到的还是上一个 prefab 的数据
			// （m_Prefab 要到 InitializeRaycast 才更新），所以下一帧 ToolUpdate 再用新掩码
			// 校正一次——两趟缺一不可。
			ApplyNow();
			m_PendingApplyFrames = 1;
		}

		protected override void OnUpdate()
		{
			ToolModeMemorySettings setting = ToolModeMemorySettings.Instance;
			if (setting == null || !setting.Enabled) return;

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
			}

			if (m_PendingApplyFrames > 0 || prefabChanged)
			{
				if (m_PendingApplyFrames > 0) m_PendingApplyFrames--;
				ApplyNow();
			}

			// 每帧捕获：切资产前最后一帧的状态已入库
			CaptureNow();

			FlushDebounced();
		}

		/// <summary>
		/// 实时保存：有新改动就把落盘时间往后推，改动停下来 1 秒后落盘。
		/// 只有 Dirty 且到点才真正写，稳态下每帧只读两个字段，零分配。
		/// </summary>
		private void FlushDebounced()
		{
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store == null) return;
			float now = UnityEngine.Time.unscaledTime;
			long serial = store.ChangeSerial;
			if (serial != m_SeenSerial)
			{
				m_SeenSerial = serial;
				m_FlushAt = now + kFlushDelay;
				return;
			}
			if (now < m_FlushAt) return;
			if (!store.Dirty) return;
			store.SaveToDisk(true);
		}

		public void RequestApply()
		{
			m_PendingApplyFrames = 1;
		}

		/// <summary>
		/// 总开关。关闭时真的 Enabled=false：不进 OnUpdate、不订阅工具事件、零分配。
		/// 必须在主线程调用（ToolModeMemorySettings.Sync 由选项页触发，满足）。
		/// </summary>
		public void SetMasterEnabled(bool on)
		{
			if (base.Enabled == on) return;
			base.Enabled = on;
			if (!on)
			{
				m_PendingApplyFrames = 0;
				return;
			}
			m_LastPrefabEntity = Entity.Null;
			m_PendingApplyFrames = 1;
		}

		public bool MasterEnabled { get { return base.Enabled; } }

		private string KeyFor(ToolModeMemorySettings setting, PrefabBase prefab, ToolBaseSystem tool, MemoryScope scope)
		{
			int slot = (int)scope;
			if (slot < 0 || slot >= m_Keys.Length) slot = 0;
			if (m_Keys[slot] != null && m_KeyPasses[slot] == m_KeyPass) return m_Keys[slot];
			string key = ToolMemoryBridge.ResolveKey(scope, prefab, tool, m_PrefabSystem, base.EntityManager);
			m_Keys[slot] = key;
			m_KeyPasses[slot] = m_KeyPass;
			return key;
		}

		public void CaptureNow()
		{
			m_KeyPass++;
			ToolModeMemorySettings setting = ToolModeMemorySettings.Instance;
			MemoryStore store = ToolModeMemoryMod.Store;
			if (setting == null || !setting.Enabled || store == null) return;
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			PrefabBase prefab = m_ToolSystem.activePrefab;

			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				// 只受总开关限制：工具项「是否恢复」的开关不拦记录，
				// 否则关掉一项就等于丢掉它的历史，再打开时是空的。
				// 一项可能覆盖多个游戏字段（Subs），每个字段一个独立记忆桶，
				// 共用同一范围与同一个键（键每趟最多解析一次，所以放懒求值）。
				string[] subs = def.Subs;
				int count = subs == null ? 1 : subs.Length;
				string key = null;
				for (int j = 0; j < count; j++)
				{
					string fieldId = subs == null ? def.Id : subs[j];
					int value;
					if (!ToolMemoryBridge.TryCapture(fieldId, tool, out value)) continue;
					if (key == null) key = KeyFor(setting, prefab, tool, setting.GetItemScope(def.Id));
					store.Set(fieldId, key, value);
				}
			}
		}

		public void ApplyNow()
		{
			m_KeyPass++;
			ToolModeMemorySettings setting = ToolModeMemorySettings.Instance;
			MemoryStore store = ToolModeMemoryMod.Store;
			if (setting == null || !setting.Enabled || store == null) return;
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			PrefabBase prefab = m_ToolSystem.activePrefab;

			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				if (!setting.IsItemEnabled(def.Id)) continue;
				MemoryScope scope = setting.GetItemScope(def.Id);
				string key = KeyFor(setting, prefab, tool, scope);

				string[] subs = def.Subs;
				int count = subs == null ? 1 : subs.Length;
				for (int j = 0; j < count; j++)
				{
					string fieldId = subs == null ? def.Id : subs[j];
					bool found;
					int value = store.Get(fieldId, key, out found);
					if (!found)
					{
						// 未命中：单资产档必须写默认，避免上一资产状态泄漏；
						// elevation 也写默认（原版根本不重置高度）。
						bool needDefault = scope == MemoryScope.GlobalUnique
							|| def.Id == ToolItemCatalog.kElevation;
						if (!needDefault) continue;
						value = ToolMemoryBridge.DefaultValue(fieldId);
					}
					try
					{
						ToolMemoryBridge.TryApply(fieldId, tool, value);
					}
					catch (Exception ex)
					{
						log.Warn("Apply " + fieldId + " failed: " + ex.GetType().Name + " " + ex.Message);
					}
				}
			}
		}

		/// <summary>重置记忆后把当前工具面板恢复到出厂状态。</summary>
		public void ResetActiveTool()
		{
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				string[] subs = items[i].Subs;
				int count = subs == null ? 1 : subs.Length;
				for (int j = 0; j < count; j++)
				{
					string fieldId = subs == null ? items[i].Id : subs[j];
					try { ToolMemoryBridge.TryApply(fieldId, tool, ToolMemoryBridge.DefaultValue(fieldId)); }
					catch { }
				}
			}
		}

		/// <summary>
		/// 进入存档/编辑器。返回 false = 记忆文件读不懂，本轮不再覆盖写盘。
		/// </summary>
		public bool OnEnteredGame()
		{
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store == null) return true;
			bool ok = store.LoadForCurrentSave();
			ToolMemoryBridge.ForgetNames();
			m_LastPrefabEntity = Entity.Null;
			m_PendingApplyFrames = 2;
			if (!ok)
			{
				if (!m_WarnedBlocked)
				{
					m_WarnedBlocked = true;
					log.Warn("Memory file unreadable, writing disabled for this save: " + store.CurrentFilePath());
				}
				return false;
			}
			m_WarnedBlocked = false;
			// 换档后 ChangeSerial 基线重置，避免拿上一个档的序号判断「有没有新改动」
			m_SeenSerial = store.ChangeSerial;
			m_FlushAt = UnityEngine.Time.unscaledTime + kFlushDelay;
			if (store.RecoveredFromTemp)
			{
				log.Warn("Memory file was damaged, restored from the last live write: " + store.CurrentFilePath());
			}
			log.Info("Memory loaded for '" + store.SaveName + "'" + (store.IsPlaceholder ? " (placeholder)" : ""));
			return true;
		}

		public void OnLeavingGame()
		{
			CaptureNow();
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store != null && store.Dirty && store.SaveToDisk())
			{
				log.Info("Memory saved: " + store.CurrentFilePath());
			}
		}

		public void FlushIfDirty()
		{
			CaptureNow();
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store != null && store.Dirty) store.SaveToDisk();
		}
	}
}
