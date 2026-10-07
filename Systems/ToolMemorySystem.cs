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
		private bool m_WarnedWriteFail;

		// 实时落盘防抖：玩家**停止改动** kFlushDelay 秒后才写盘。频繁切换工具的玩家
		// 一次游戏能产生上百次改动，逐次写盘既伤硬盘又没必要（记忆只在下次进档时才用）。
		// 10 秒的代价：闪退最多丢掉这 10 秒内的改动，而且写的是 tmp+替换，不会留下半截文件；
		// 回主菜单、退出游戏、存档成功这三处都会立刻强制落盘，正常流程一点都不丢。
		private const float kFlushDelay = 10f;

		// 写盘失败时的退避：0.4.0 性能审查发现的第二个坑 —— Dirty 在写成功前不会清，
		// 而防抖的判断是「到点 且 Dirty 就写」。目标文件被占用 / 目录只读 / 磁盘满时，
		// 这等于**每帧**做一次「建目录 + 写临时文件 + 替换」并抛一次异常，
		// 玩家看到的就是一路掉帧。失败后按 10→20→40→…→300 秒退避，成功一次立刻回到 10 秒。
		private const float kWriteRetryMax = 300f;
		private float m_WriteRetry = kFlushDelay;
		private long m_SeenSerial;
		private float m_FlushAt;

		// 一次 Capture/Apply 之内，完整键（域+层级+家族）与裸层级键各最多解析一遍
		// （五档共用范围，12 个工具项共用同一份缓存：同一趟里 prefab/tool 是同一个）。
		// 0.4.0 性能审查：以前这两个缓存在**每次** CaptureNow/ApplyNow 顶端作废，
		// 而捕获是每帧跑的，等于每帧重算五档键 —— 每帧二三十次字符串分配 + 十几次数值查找，
		// 稳态下全是白给（长时间游玩的周期性卡顿来源之一）。现在改成「m_KeyEpoch 有效期内复用」：
		// 只有换了工具或换了资产、或过了 kKeyRefreshSeconds 才重算一次。
		// （键的内容只可能因为「工具/资产/工具栏层级」而变；层级被别的模组中途重排是分钟级事件，
		//  两秒的重算窗口足够跟上，而 s_NameCache 本来就是按实体缓存名字。）
		private int m_KeyEpoch;
		private ToolBaseSystem m_KeyTool;
		private PrefabBase m_KeyPrefab;
		private float m_KeyAt;
		private const float kKeyRefreshSeconds = 2f;
		private readonly string[] m_Keys = new string[5];
		private readonly int[] m_KeyPasses = new int[5];

		// 工具栏筛选项（地区主题 / 数据包）：一组勾选，键规则和指纹见 CaptureFilters
		private const int kFilterUnknown = int.MinValue;
		private readonly int[] m_FilterSigs = new int[] { kFilterUnknown, kFilterUnknown };
		private readonly string[] m_FilterKeys = new string[2];
		private readonly List<string> m_FilterNames = new List<string>(32);
		// 裸层级键缓存：槽 = 范围 × 2 +「同类资产是否按工具栏层级」，5 档范围两种口径共 10 槽
		// 失效条件与 m_Keys 完全同步（同一个 m_KeyEpoch），两批必须看到同一个 prefab/tool。
		private readonly string[] m_LevelKeys = new string[10];
		private readonly int[] m_LevelKeyPasses = new int[10];

		/// <summary>
		/// 「在不在存档里 / 进档稳定期过没过」这两条闸门（规则与理由见 SaveSessionGate）。
		/// 玩家日志里能直接看到不关闸的后果：2026-10-07 16:41:46 开机进主菜单，
		/// <c>OnLeavingGame()</c> 照样跑了一遍捕获并写出 <c>_unsaved_9d40cc53.json</c>
		/// —— 主菜单的面板状态被当成记忆存了下来。
		/// </summary>
		private readonly SaveSessionGate m_Gate = new SaveSessionGate();

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

		/// <summary>
		/// 模组卸载（IMod.OnDispose）时用的彻底脱离：先停用自己（Enabled=false，
		/// 不再进 OnUpdate），再把挂在原版 ToolSystem 事件字段上的两个委托摘掉。
		/// 光靠 OnDestroy 不够：本系统是用 GetOrCreateSystemManaged 建在游戏的 World 里的，
		/// World 不重建就一直存在， OnDestroy 也就不会来 —— 那时若还挂着事件，
		/// 玩家每次切工具/切资产都会调用进一个已经卸载的程序集里，正是「卸载模组后疯狂报错」的形态。
		/// </summary>
		public void Detach()
		{
			base.Enabled = false;
			m_PendingApplyFrames = 0;
			m_Gate.Close();
			UnhookEvents();
			ForgetKeysAndCache();
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

			// 进档稳定期：每帧写回、不捕获（为什么必须两头都占，见 SaveSessionGate）。
			// 原版在载入过程中会把自己的工具偏好重置成出厂值，而工具成型比 onGameLoadingComplete
			// 还晚几帧；这段时间里捕获一次就会把「原版默认值」记成玩家的选择，
			// 下一次进档读到的就是这份被污染的记忆。
			if (m_Gate.Settling(UnityEngine.Time.unscaledTime))
			{
				m_PendingApplyFrames = 0;
				ApplyNow();
			}
			else if (m_PendingApplyFrames > 0 || prefabChanged)
			{
				if (m_PendingApplyFrames > 0) m_PendingApplyFrames--;
				ApplyNow();
			}

			// 每帧捕获：切资产前最后一帧的状态已入库（不在存档里 / 稳定期内会被闸门挡掉）
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
			// 不在存档里就不落盘：脏标记可能是上一个存档留下的，而主菜单里那个文件名
			// 已经不属于任何本局身份（BeginMainMenu 会清掉，但顺序上不保证总是先到）。
			if (!m_Gate.InSave) return;
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
			if (!store.Dirty)
			{
				// 写成功：退回正常防抖节奏
				m_WriteRetry = kFlushDelay;
				m_FlushAt = now + kFlushDelay;
				return;
			}
			// 还是脏的 = 这次写盘失败了（被占用 / 没权限 / 磁盘满）。
			// 绝不能保持「到点就写」，否则从现在起每帧都要重跑一遍写文件并抛异常 ——
			// 那才是真正会把模拟拖慢的地方。指数退避，最多五分钟试一次。
			m_WriteRetry = m_WriteRetry * 2f > kWriteRetryMax ? kWriteRetryMax : m_WriteRetry * 2f;
			m_FlushAt = now + m_WriteRetry;
			if (!m_WarnedWriteFail)
			{
				m_WarnedWriteFail = true;
				log.Warn("Could not write memory file, retrying every " + (int)m_WriteRetry
					+ "s: " + store.CurrentFilePath());
			}
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
			// 关着的这段时间里玩家可能换了资产、别的模组重排了工具栏：
			// 重开时键缓存与筛选项基线一律作废，不能拿关着时的旧状态当基线。
			ForgetKeysAndCache();
			ResetFilterBaseline();
			if (!on)
			{
				m_PendingApplyFrames = 0;
				return;
			}
			m_LastPrefabEntity = Entity.Null;
			m_PendingApplyFrames = 1;
			// 刚打开时同样要先写回、后捕获：关着的这段时间面板归玩家（和原版）做主，
			// 立刻捕获会把「没开记忆时」的状态当成这一局的选择记下来。
			if (m_Gate.InSave) m_Gate.Open(UnityEngine.Time.unscaledTime);
		}

		public bool MasterEnabled { get { return base.Enabled; } }

		private string KeyFor(ToolItemDef def, PrefabBase prefab, ToolBaseSystem tool, MemoryScope scope)
		{
			// 「游戏里只有一份值」的项在「全局共用」下必须只有一把键，否则这一档会碎成
			// A|S$net / A|S$obj / F|S$area… 每组各一份，玩家看到的正是「设了全局共用
			// 还是各工具组单独记忆」。详见 ToolItemCatalog.UsesFamilyFreeKey。
			if (ToolItemCatalog.UsesFamilyFreeKey(def, scope)) return LevelKeyCached(scope, false, prefab, tool);

			int slot = (int)scope;
			if (slot < 0 || slot >= m_Keys.Length) slot = 0;
			if (m_Keys[slot] != null && m_KeyPasses[slot] == m_KeyEpoch) return m_Keys[slot];
			string key = ToolMemoryBridge.ResolveKey(scope, prefab, tool, m_PrefabSystem, base.EntityManager);
			m_Keys[slot] = key;
			m_KeyPasses[slot] = m_KeyEpoch;
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

		/// <summary>
		/// 裸层级键（不含域/家族）：只给工具栏筛选项用，所以「同类资产」这一档
		/// 按工具栏的（菜单+分类）发键——那两行的勾选是面板状态，不是资产的属性。
		/// 详见 ToolMemoryBridge.ResolveLevelKey 的 categoryByUiHierarchy。
		/// </summary>
		private string LevelKeyFor(ToolModeMemorySettings setting, ToolItemDef def, PrefabBase prefab, ToolBaseSystem tool)
		{
			return LevelKeyCached(setting.GetItemScope(def.Id), true, prefab, tool);
		}

		/// <summary>
		/// 裸层级键（不套资产/功能域、不套枚举家族）：工具栏筛选项与「全局共用 + 游戏里
		/// 只有一份值」的工具项共用这条通道。失效条件与 m_Keys 同一次（m_KeyEpoch）。
		/// 缓存槽按「范围 × 是否按 UI 层级分同类」编号，两种口径不会互相顶掉。
		/// </summary>
		private string LevelKeyCached(MemoryScope scope, bool categoryByUiHierarchy, PrefabBase prefab,
			ToolBaseSystem tool)
		{
			int slot = (int)scope * 2 + (categoryByUiHierarchy ? 1 : 0);
			if (slot < 0 || slot >= m_LevelKeys.Length) slot = 0;
			if (m_LevelKeys[slot] != null && m_LevelKeyPasses[slot] == m_KeyEpoch) return m_LevelKeys[slot];
			string key = ToolMemoryBridge.ResolveLevelKey(scope, prefab, tool, m_PrefabSystem,
				base.EntityManager, categoryByUiHierarchy);
			m_LevelKeys[slot] = key;
			m_LevelKeyPasses[slot] = m_KeyEpoch;
			return key;
		}

		/// <summary>
		/// 键缓存的统一失效判断：工具换了、资产换了、或者隔了 kKeyRefreshSeconds
		/// （给「别的模组中途重排工具栏」留的活口）。同一帧里 Capture 与 Apply 互相复用。
		/// </summary>
		private void RefreshKeyCache(ToolBaseSystem tool, PrefabBase prefab)
		{
			float now = UnityEngine.Time.unscaledTime;
			if (ReferenceEquals(tool, m_KeyTool) && ReferenceEquals(prefab, m_KeyPrefab)
				&& now - m_KeyAt < kKeyRefreshSeconds)
			{
				return;
			}
			m_KeyTool = tool;
			m_KeyPrefab = prefab;
			m_KeyAt = now;
			m_KeyEpoch++;
		}

		/// <summary>强制重算所有键（换档、总开关重开、兼容口径改动）。</summary>
		public void ForgetKeysAndCache()
		{
			m_KeyTool = null;
			m_KeyPrefab = null;
			m_KeyAt = 0f;
			m_KeyEpoch++;
		}

		public void CaptureNow()
		{
			CaptureNow(false);
		}

		/// <param name="final">退出存档前那一次强制捕获：稳定期也要记，但不在存档里时照样不记。</param>
		private void CaptureNow(bool final)
		{
			ToolModeMemorySettings setting = ToolModeMemorySettings.Instance;
			MemoryStore store = ToolModeMemoryMod.Store;
			if (setting == null || !setting.Enabled || store == null) return;
			// 闸门：主菜单 / 编辑器 / 进档稳定期里一律不捕获（理由见 SaveSessionGate）。
			float now = UnityEngine.Time.unscaledTime;
			if (final ? !m_Gate.AllowFinalCapture() : !m_Gate.AllowCapture(now)) return;
			// 记忆文件读不懂的时候已经禁止写盘了，继续捕获只会白改内存、涨改动序号，
			// 让防抖去试一次注定写不进去的盘。
			if (store.WriteBlocked) return;
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			PrefabBase prefab = m_ToolSystem.activePrefab;
			RefreshKeyCache(tool, prefab);

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
					// 资产自带取值限制的项（现在只有高度）要先问一句：这个值是本资产够得着的吗？
					// 从道路切到水管时工具里还留着道路的 0m，而水管上限 -10m —— 那是上一件资产的值，
					// 记下去要么污染共用桶（把道路一起拽走），要么变成水管永远用不了的死值。
					if (ToolMemoryBridge.HasValueLimit(fieldId)
						&& !ToolMemoryBridge.AcceptsCapture(fieldId, tool, value)) continue;
					store.Set(fieldId, key, value);
				}
			}

			// 工具栏筛选项（地区主题 / 数据包）不在这条循环里：它们不住在 ToolBaseSystem 上，
			//  TryCapture 对这两个 id 恒返回 false，值由下面这趟按「一组勾选」单独处理。
			CaptureFilters(setting, store, prefab, tool);
		}

		public void ApplyNow()
		{
			ToolModeMemorySettings setting = ToolModeMemorySettings.Instance;
			MemoryStore store = ToolModeMemoryMod.Store;
			if (setting == null || !setting.Enabled || store == null) return;
			// 闸门：这一局的记忆没读进来（进档回调半路失败）时什么都不许写回 ——
			// 此刻 store 里可能是上一个存档的值，写回工具面板就是把别人的设置搬进这一局。
			if (!m_Gate.InSave) return;
			ToolBaseSystem tool = m_ToolSystem != null ? m_ToolSystem.activeTool : null;
			if (tool == null) return;
			PrefabBase prefab = m_ToolSystem.activePrefab;
			RefreshKeyCache(tool, prefab);

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
		/// 这个方法**自己不许抛**：调用方（ToolModeMemoryMod 的进档流程）靠它把
		/// 「读记忆」这一步做完，读不成也要让系统照常启用、面板照常写回。
		/// </summary>
		public bool OnEnteredGame()
		{
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store == null) return true;
			bool ok;
			// 读文件这一步单独兜住：它失败了后面三件事（清跨档缓存、开闸、排写回）
			// 照样必须做完 —— 空记忆比「整局不工作」好得多，也绝不能拿上一个档的缓存继续跑。
			try
			{
				ok = store.LoadForCurrentSave();
			}
			catch (Exception ex)
			{
				log.Warn("Could not read the memory file: " + ex.GetType().Name + " " + ex.Message);
				ok = false;
			}
			ToolMemoryBridge.ForgetNames();
			ForgetKeysAndCache();
			m_LastPrefabEntity = Entity.Null;
			// 写盘退避是按「这台机器此刻写不写得进去」累积出来的，换档（甚至换台机器）不该带着走
			m_WriteRetry = kFlushDelay;
			m_WarnedWriteFail = false;
			// 筛选项的指纹与层级键都是按档记录的：换档（World 重建、记忆重读）必须作废，
			// 否则会拿上一档的基线去比这一档的工具栏状态。
			ResetFilterBaseline();
			m_PendingApplyFrames = 2;
			// 开闸：从这一刻起才算「在存档里」，并进入进档稳定期。
			m_Gate.Open(UnityEngine.Time.unscaledTime);
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
			log.Info("Memory loaded for '" + store.SaveName + "' from " + store.CurrentFilePath()
				+ (store.IsPlaceholder ? " (placeholder)" : "")
				+ (store.IsEmpty ? " (no memory yet)" : " (" + store.EntryCount + " values)"));
			return true;
		}

		/// <summary>
		/// 离开存档（回主菜单 / 退出游戏 / 卸载模组）：先把最后一帧的状态记下来并落盘，再关闸。
		/// 关闸之后主菜单里的任何回调都不许再碰记忆。
		/// </summary>
		public void OnLeavingGame()
		{
			if (!m_Gate.InSave) return;
			CaptureNow(true);
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store != null && store.Dirty && store.SaveToDisk())
			{
				log.Info("Memory saved: " + store.CurrentFilePath());
			}
			m_Gate.Close();
		}

		/// <summary>存档内主动落盘（玩家存盘时调用）。稳定期没过就只落已读到的记忆。</summary>
		public void FlushIfDirty()
		{
			CaptureNow();
			MemoryStore store = ToolModeMemoryMod.Store;
			if (store != null && store.Dirty) store.SaveToDisk();
		}
	}
}
