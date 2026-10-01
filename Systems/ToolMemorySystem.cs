using System;
using System.Collections.Generic;
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

		// 实时落盘防抖：玩家**停止改动** kFlushDelay 秒后才写盘。频繁切换工具的玩家
		// 一次游戏能产生上百次改动，逐次写盘既伤硬盘又没必要（记忆只在下次进档时才用）。
		// 10 秒的代价：闪退最多丢掉这 10 秒内的改动，而且写的是 tmp+替换，不会留下半截文件；
		// 回主菜单、退出游戏、存档成功这三处都会立刻强制落盘，正常流程一点都不丢。
		private const float kFlushDelay = 10f;
		private long m_SeenSerial;
		private float m_FlushAt;

		// 一次 Capture/Apply 之内，完整键（域+层级+家族）与裸层级键各最多解析 5 次
		// （五档共用范围，12 个工具项共用同一份缓存：同一趟里 prefab/tool 是同一个），
		// 跨调用必须重开。m_KeyPass 与 m_LevelKeyPass 在 CaptureNow/ApplyNow 顶端各自 +1。
		private int m_KeyPass;
		private readonly string[] m_Keys = new string[5];
		private readonly int[] m_KeyPasses = new int[5];

		// 工具栏筛选项（地区主题 / 数据包）：一组勾选，键规则和指纹见 CaptureFilters
		private const int kFilterUnknown = int.MinValue;
		private readonly int[] m_FilterSigs = new int[] { kFilterUnknown, kFilterUnknown };
		private readonly string[] m_FilterKeys = new string[2];
		private readonly List<string> m_FilterNames = new List<string>(32);
		private int m_LevelKeyPass;
		private readonly string[] m_LevelKeys = new string[5];
		private readonly int[] m_LevelKeyPasses = new int[5];

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
		/// 实时保存：有新改动就把落盘时间往后推，玩家停下来 kFlushDelay（10 秒）后才落盘。
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
			// 关掉期间玩家动过筛选，再打开时不能拿关着的基线比现在：重开就作废
			ResetFilterBaseline();
			if (!on)
			{
				m_PendingApplyFrames = 0;
				return;
			}
			m_LastPrefabEntity = Entity.Null;
			ResetFilterBaseline();
			m_PendingApplyFrames = 1;
		}

		public bool MasterEnabled { get { return base.Enabled; } }

		private string KeyFor(ToolItemDef def, PrefabBase prefab, ToolBaseSystem tool, MemoryScope scope)
		{
			// 「游戏里只有一份值」的项在「全局共用」下必须只有一把键，否则这一档会碎成
			// A|S$net / A|S$obj / F|S$area… 每组各一份，玩家看到的正是「设了全局共用
			// 还是各工具组单独记忆」。详见 ToolItemCatalog.UsesFamilyFreeKey。
			if (ToolItemCatalog.UsesFamilyFreeKey(def, scope)) return LevelKeyCached(scope, prefab, tool);

			int slot = (int)scope;
			if (slot < 0 || slot >= m_Keys.Length) slot = 0;
			if (m_Keys[slot] != null && m_KeyPasses[slot] == m_KeyPass) return m_Keys[slot];
			string key = ToolMemoryBridge.ResolveKey(scope, prefab, tool, m_PrefabSystem, base.EntityManager);
			m_Keys[slot] = key;
			m_KeyPasses[slot] = m_KeyPass;
			return key;
		}

		/// <summary>
		/// 工具栏筛选项（地区主题 / 数据包）的每帧捕获。
		///
		/// 与普通工具项不一样：那份值住在 ToolBaseSystem 上、是单个整数，走 Subs + TryCapture
		/// 那条循环；这两份是 ToolbarUISystem 里的一组勾选，所以按「一个可选项一把键 +
		/// 一把计数哨兵」存（键规则见 MemoryKeys.FilterOption，证据与理由见 ToolMemoryBridge
		/// 的筛选段注释）。键只用层级、不套资产/功能域和枚举家族：那份勾选整个工具栏共用一份，
		/// 只有读档才重置（T:624-631），套上域/家族就把「全局共用」偷成了「每个家族各一份」。
		///
		/// 每帧只算一次整数指纹，指纹不变就零分配。
		/// </summary>
		private void CaptureFilters(ToolModeMemorySettings setting, MemoryStore store,
			PrefabBase prefab, ToolBaseSystem tool)
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				if (def.Source != ItemSource.Toolbar) continue;
				int slot = FilterSlot(def.Id);
				int sig;
				if (!ToolMemoryBridge.ToolbarSelectionSignature(def.Id, out sig)) continue;
				string key = LevelKeyFor(setting, def, prefab, tool);
				if (string.IsNullOrEmpty(key)) continue;

				if (m_FilterSigs[slot] == kFilterUnknown)
				{
					// 本档第一次看到：只做基线，然后补一次恢复。读档后原版把主题重置成
					// 默认主题、数据包本来就是空的，不补这一下就要等玩家下次切菜单才生效。
					m_FilterSigs[slot] = sig;
					m_FilterKeys[slot] = key;
					RestoreFilter(setting, store, def, slot, key);
					continue;
				}
				if (m_FilterKeys[slot] != key)
				{
					// 层级键变了 = 玩家在切菜单/分类。原版正是在这一刻把数据包清空(T:1147/1180)，
					// 那个空表不是玩家的选择，把它记下来就会抹掉玩家在这个组里原本的勾选，
					// 所以这里只重设基线并恢复这个组自己的记忆，不写记忆。
					m_FilterKeys[slot] = key;
					m_FilterSigs[slot] = sig;
					RestoreFilter(setting, store, def, slot, key);
					continue;
				}
				if (m_FilterSigs[slot] == sig) continue;
				m_FilterSigs[slot] = sig;

				// 待在同一个组里、勾选真的变了：这才是玩家的选择，记下来。
				// 先把这一层级键下的旧勾选清干净，否则上次勾过、这次取消的项会留在文件里，
				// 恢复时又被勾回去。
				if (!ToolMemoryBridge.CaptureToolbarSelection(def.Id, m_FilterNames)) continue;
				store.RemoveOptionKeys(def.Id, key);
				for (int j = 0; j < m_FilterNames.Count; j++)
				{
					store.Set(def.Id, MemoryKeys.FilterOption(key, m_FilterNames[j]), MemoryKeys.FilterValue(true));
				}
				// 计数哨兵：0 也要写，「记过但一个都没勾」和「从没记过」是两回事
				store.Set(def.Id, key, m_FilterNames.Count);
			}
		}

		/// <summary>换资产 / 换工具时，筛选项也按当前层级键补一次恢复。</summary>
		private void RestoreFilters(ToolModeMemorySettings setting, MemoryStore store,
			PrefabBase prefab, ToolBaseSystem tool)
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				ToolItemDef def = items[i];
				if (def.Source != ItemSource.Toolbar) continue;
				string key = LevelKeyFor(setting, def, prefab, tool);
				if (string.IsNullOrEmpty(key)) continue;
				RestoreFilter(setting, store, def, FilterSlot(def.Id), key);
			}
		}

		/// <summary>
		/// 恢复一个筛选项。只在「这一层键下记过」时动手；当前分类里不存在的可选项一律忽略，
		/// 免得把工具栏筛成空列表（记忆里就是一个都没勾，那照做——那是玩家自己的选择）。
		/// </summary>
		private void RestoreFilter(ToolModeMemorySettings setting, MemoryStore store,
			ToolItemDef def, int slot, string key)
		{
			if (!setting.IsItemEnabled(def.Id)) return;
			bool found;
			int remembered = store.Get(def.Id, key, out found);
			if (!found) return;
			m_FilterNames.Clear();
			if (remembered > 0)
			{
				// 记过至少一个：必须拿得到当前分类的可选项列表才能挑出该勾哪些。
				// 拿不到就什么都不做 —— 带着空列表往下走等于把玩家的筛选清掉。
				if (!ToolMemoryBridge.ToolbarOptionNames(def.Id, m_FilterNames)) return;
				for (int j = m_FilterNames.Count - 1; j >= 0; j--)
				{
					bool hit;
					int v = store.Get(def.Id, MemoryKeys.FilterOption(key, m_FilterNames[j]), out hit);
					if (!hit || v != MemoryKeys.FilterValue(true)) m_FilterNames.RemoveAt(j);
				}
			}
			if (ToolMemoryBridge.ApplyToolbarSelection(def.Id, m_FilterNames))
			{
				// 把自己写回去的结果当新基线：否则同一帧的捕获会把这次恢复又记一遍（白白脏一次）
				int sig;
				if (ToolMemoryBridge.ToolbarSelectionSignature(def.Id, out sig)) m_FilterSigs[slot] = sig;
			}
		}

		/// <summary>两项固定占两个槽：0 = 地区主题，1 = 数据包。</summary>
		private static int FilterSlot(string id)
		{
			return id == ToolItemCatalog.kThemes ? 0 : 1;
		}

		/// <summary>作废筛选项的基线（换档、总开关重开）：下一帧重新认一次。</summary>
		private void ResetFilterBaseline()
		{
			m_FilterSigs[0] = kFilterUnknown;
			m_FilterSigs[1] = kFilterUnknown;
			m_FilterKeys[0] = null;
			m_FilterKeys[1] = null;
		}

		/// <summary>裸层级键（不含域/家族）：筛选项与「全局共用 + 游戏里只有一份值」的工具项共用。</summary>
		private string LevelKeyFor(ToolModeMemorySettings setting, ToolItemDef def, PrefabBase prefab, ToolBaseSystem tool)
		{
			return LevelKeyCached(setting.GetItemScope(def.Id), prefab, tool);
		}

		/// <summary>
		/// 裸层级键（不套资产/功能域、不套枚举家族）：工具栏筛选项与「全局共用 + 游戏里
		/// 只有一份值」的工具项共用这条通道。每趟 Capture/Apply 由 m_LevelKeyPass 作废缓存。
		/// </summary>
		private string LevelKeyCached(MemoryScope scope, PrefabBase prefab, ToolBaseSystem tool)
		{
			int slot = (int)scope;
			if (slot < 0 || slot >= m_LevelKeys.Length) slot = 0;
			if (m_LevelKeys[slot] != null && m_LevelKeyPasses[slot] == m_LevelKeyPass) return m_LevelKeys[slot];
			string key = ToolMemoryBridge.ResolveLevelKey(scope, prefab, tool, m_PrefabSystem, base.EntityManager);
			m_LevelKeys[slot] = key;
			m_LevelKeyPasses[slot] = m_LevelKeyPass;
			return key;
		}

		public void CaptureNow()
		{
			m_KeyPass++;
			// 裸层级键的缓存同样一趟一作废：CaptureNow 里既有用到完整键的工具项，
			// 也有只用层级键的筛选项，两批必须看到同一个 prefab/tool。
			m_LevelKeyPass++;
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
					if (key == null) key = KeyFor(def, prefab, tool, setting.GetItemScope(def.Id));
					store.Set(fieldId, key, value);
				}
			}

			// 工具栏筛选项（地区主题 / 数据包）不在这条循环里：它们不住在 ToolBaseSystem 上，
			//  TryCapture 对这两个 id 恒返回 false，值由下面这趟按「一组勾选」单独处理。
			CaptureFilters(setting, store, prefab, tool);
		}

		public void ApplyNow()
		{
			m_KeyPass++;
			m_LevelKeyPass++;
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
				string key = KeyFor(def, prefab, tool, scope);

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

			// 换工具 / 换资产时把工具栏筛选也补回来（这两项的记忆键是层级键，不带域和家族）
			RestoreFilters(setting, store, prefab, tool);
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
			// 筛选项的指纹与层级键都是按档记录的：换档（World 重建、记忆重读）必须作废，
			// 否则会拿上一档的基线去比这一档的工具栏状态。
			ResetFilterBaseline();
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
