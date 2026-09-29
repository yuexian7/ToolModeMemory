using System;
using System.Text;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Assets;
using Game.Modding;
using Game.SceneFlow;
using Game.Serialization;
using ToolModeMemory.Memory;
using ToolModeMemory.Systems;
using Unity.Entities;
using UnityEngine;

namespace ToolModeMemory
{
	/// <summary>
	/// Tool Mode Memory 入口。零 Harmony：只挂公开事件 + 轻量系统。
	/// 退出清理走 Application.quitting / ProcessExit（Playbook §3.1）。
	/// </summary>
	public class ToolModeMemoryMod : IMod
	{
		public const string kVersion = "0.2.3";

		public static ILog log = LogManager.GetLogger(nameof(ToolModeMemory)).SetShowsErrorsInUI(false);

		public static MemoryStore Store;

		private static ToolModeMemoryMod s_Instance;

		private ToolModeMemorySettings m_Setting;
		private ToolMemorySystem m_System;
		private bool m_QuitHooked;
		private SaveIndex m_Index = new SaveIndex();

		/// <summary>当前存档的 guid 文本（进档时解析出来就留着，用于索引）。</summary>
		private string m_CurrentGuid;

		/// <summary>当前是否真的在「存档」里（编辑器 / 主菜单不算）。</summary>
		private bool m_InSave;

		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info("Tool Mode Memory v" + kVersion + " loading...");
			s_Instance = this;

			Store = new MemoryStore();

			m_Setting = new ToolModeMemorySettings(this);
			ToolModeMemorySettings.Instance = m_Setting;

			try
			{
				string[] locales = LocaleTable.Locales;
				for (int i = 0; i < locales.Length; i++)
				{
					GameManager.instance.localizationManager.AddSource(locales[i], new LocaleSource(m_Setting, locales[i]));
				}
				log.Info("Locale sources registered: " + locales.Length);
			}
			catch (Exception ex)
			{
				log.Warn("Locale register failed: " + ex.GetType().Name);
			}

			try
			{
				m_Setting.RegisterInOptionsUI();
			}
			catch (Exception ex)
			{
				log.Warn("RegisterInOptionsUI failed: " + ex.GetType().Name);
			}

			// LoadSettings（无键位，但仍按标准顺序）
			try
			{
				AssetDatabase.global.LoadSettings(nameof(ToolModeMemory), m_Setting, new ToolModeMemorySettings(this));
			}
			catch (Exception ex)
			{
				log.Warn("LoadSettings failed, using defaults: " + ex.GetType().Name);
			}
			// 反序列化是用属性 setter 写值的，读盘完成前必须屏蔽 Sync/Persist
			ToolModeMemorySettings.Ready = true;
			// 补一次 Sync：让文件里的兼容开关真正传给 ToolMemoryBridge
			m_Setting.AfterLoaded();

			// 生命周期事件
			try
			{
				GameManager gm = GameManager.instance;
				gm.onGameLoadingComplete += OnGameLoadingComplete;
				gm.onGameSaveLoad += OnGameSaveLoad;
				gm.onGamePreload += OnGamePreload;
			}
			catch (Exception ex)
			{
				log.Warn("Hook GameManager events failed: " + ex.GetType().Name);
			}

			HookQuit();

			try
			{
				updateSystem.UpdateAt<ToolMemorySystem>(SystemUpdatePhase.ToolUpdate);
				m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld != null
					? Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ToolMemorySystem>()
					: null;
				log.Info("ToolMemorySystem registered at ToolUpdate.");
			}
			catch (Exception ex)
			{
				log.Error("ToolMemorySystem register failed: " + ex.GetType().Name + " " + ex.Message);
			}

			RefreshActive();
			log.Info("Tool Mode Memory v" + kVersion + " loaded. Master switch default=ON.");
		}

		/// <summary>
		/// 让系统 Enabled 严格等于「总开关 且 在存档内」。
		/// 关 = 不订阅工具事件、不进 OnUpdate、零分配。
		/// </summary>
		public static void RefreshActive()
		{
			ToolModeMemoryMod inst = s_Instance;
			if (inst == null) return;
			// 每次从 world 重取：缓存的实例若被重建，会出现「开关开着、日志正常、功能全无」
			if (inst.m_System == null && Unity.Entities.World.DefaultGameObjectInjectionWorld != null)
			{
				inst.m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld
					.GetExistingSystemManaged<ToolMemorySystem>();
			}
			if (inst.m_System == null) return;
			bool on = inst.m_InSave && inst.m_Setting != null && inst.m_Setting.Enabled;
			try { inst.m_System.SetMasterEnabled(on); } catch { }
		}

		private void HookQuit()
		{
			if (m_QuitHooked) return;
			try
			{
				Application.quitting += OnApplicationQuitting;
				AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
				m_QuitHooked = true;
			}
			catch (Exception ex)
			{
				log.Warn("Hook quit failed: " + ex.GetType().Name);
			}
		}

		private void OnGamePreload(Purpose purpose, GameMode mode)
		{
			// 原版在这里 ResetToolPreferences；我们在 LoadingComplete 之后恢复
		}

		private void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			try
			{
				bool inGame = mode.IsGame();
				bool inEditor = mode.IsEditor();

				if (!inGame && !inEditor)
				{
					// 回主菜单：落盘后清场，主菜单里不再捕获，也不会生成空文件
					if (m_System != null) m_System.OnLeavingGame();
					else if (Store != null && Store.Dirty) Store.SaveToDisk();
					m_InSave = false;
					RefreshActive();
					if (Store != null) Store.BeginMainMenu();
					log.Info("Back to main menu.");
					return;
				}

				if (Store == null) Store = new MemoryStore();

				if (inEditor)
				{
					// 资产/地图编辑器没有「存档」概念：完全不参与，避免污染上一次游玩的记忆
					m_InSave = false;
					RefreshActive();
					Store.BeginMainMenu();
					log.Info("Editor mode: tool memory inactive.");
					return;
				}

				m_InSave = true;

				LoadIndex();

				string resolved;
				bool isPlaceholder;
				ResolveSaveName(purpose, m_Index, out resolved, out isPlaceholder);
				// 只有「载入已有存档」才留载入 guid：新建城市时 instigatorGuid 是地图的 id，
				// 拿它建索引会让同一张地图上的两座新城共用记忆文件。
				m_CurrentGuid = (SaveIdentity.Classify((int)purpose) == SaveIdentity.PurposeKind.LoadedSave
					&& TryGetInstigatorGuid(out Colossal.Hash128 g)) ? g.ToString() : null;

				if (!string.IsNullOrEmpty(resolved))
				{
					if (isPlaceholder) Store.UsePlaceholderName(resolved);
					else Store.UseSaveName(resolved);
				}
				else
				{
					// 新建城市 / 身份未确证：强制换成本次会话的占位名。
					// 不能用 EnsureSessionName —— game→game 直接切换不经过主菜单时
					// 名字还留着上一个存档的，空转就等于沿用别人的文件。
					Store.StartUnnamedSession();
				}

				if (m_System != null)
				{
					m_System.OnEnteredGame();
				}
				RefreshActive();
				if (m_Setting != null && m_Setting.Enabled && m_System != null)
				{
					m_System.RequestApply();
				}
				log.Info("Entered game as '" + Store.SaveName + "'"
					+ (Store.IsPlaceholder ? " (placeholder)" : "") + " purpose=" + purpose);
			}
			catch (Exception ex)
			{
				log.Warn("OnGameLoadingComplete: " + ex.GetType().Name + " " + ex.Message);
			}
		}

		/// <summary>
		/// 存档身份判定。
		/// userState.lastSaveGameMetadata 只在 GameManager.Save() 成功路径里赋值
		/// （Game.SceneFlow/GameManager.cs:995），载入时不更新，所以：
		///   新建城市 / 从存档列表直接载入 -> 它可能指向上一个「存过」的档；
		/// 只有当它确实等于本次反序列化上下文的 instigatorGuid 时才可信。
		/// 不可信时使用确定性占位名 _auto_&lt;guid&gt;：同一存档再次载入仍能找回记忆，
		/// 且绝不会写坏别的存档的记忆文件。
		/// </summary>
		private static void ResolveSaveName(Purpose purpose, SaveIndex index,
			out string fileName, out bool placeholder)
		{
			fileName = null;
			placeholder = false;

			SaveIdentity.PurposeKind kind = SaveIdentity.Classify((int)purpose);
			if (!SaveIdentity.ShouldParticipate(kind)) return;

			Colossal.Hash128 loadGuid = default(Colossal.Hash128);
			bool haveGuid = TryGetInstigatorGuid(out loadGuid);
			string guidText = haveGuid ? loadGuid.ToString() : null;

			string metaIdText = null;
			string metaName = null;
			bool metaIsAuto = true;
			try
			{
				SaveGameMetadata meta = GameManager.instance.settings.userState.lastSaveGameMetadata;
				if (meta != null)
				{
					// meta.id 是 Identifier（struct{guid, uri}），ToString() 会带 "[uri]"，
					// 必须显式取 guid 才能和 instigatorGuid 的文本比。
					Colossal.Hash128 metaGuid = meta.id;
					if (metaGuid.isValid) metaIdText = metaGuid.ToString();
					metaIsAuto = meta.target == null ? true : meta.target.autoSave;
					// 注意：Metadata.identifier == "{database}/{guid}"，根本不是存档名，
					// 用它当文件名会和 onGameSaveLoad 给的 saveName 永远对不上（一个档两份文件）。
					// 存档名只能取 target.displayName（= 存档包名）。
					if (meta.target != null && !string.IsNullOrEmpty(meta.target.displayName))
					{
						metaName = meta.target.displayName;
					}
				}
			}
			catch { }

			bool ok = SaveIdentity.TryResolveName(kind, haveGuid, guidText, metaIdText, metaName,
				metaIsAuto, index, out fileName, out placeholder);
			if (!ok)
			{
				fileName = null;
				placeholder = false;
			}
		}

		private static bool TryGetInstigatorGuid(out Colossal.Hash128 guid)
		{
			guid = default(Colossal.Hash128);
			try
			{
				World w = Unity.Entities.World.DefaultGameObjectInjectionWorld;
				if (w == null) return false;
				LoadGameSystem lgs = w.GetExistingSystemManaged<LoadGameSystem>();
				if (lgs == null) return false;
				Context ctx = lgs.context;
				if (!ctx.instigatorGuid.isValid) return false;
				guid = ctx.instigatorGuid;
				return true;
			}
			catch { return false; }
		}

		private void OnGameSaveLoad(string saveName, string previewUri, bool start, bool success)
		{
			try
			{
				if (start) return;
				if (!m_InSave) return;
				if (Store == null) return;

				string metaId = null;
				bool auto = true;
				try
				{
					SaveGameMetadata meta = GameManager.instance.settings.userState.lastSaveGameMetadata;
					if (meta != null)
					{
						// 口径必须和 instigatorGuid 一致：Identifier.ToString() 会带 "[uri]"
						Colossal.Hash128 metaGuid = meta.id;
						if (metaGuid.isValid) metaId = metaGuid.ToString();
						auto = meta.target == null ? true : meta.target.autoSave;
					}
				}
				catch { }

				// 自动存档的名字是 "dd-MMMM-HH-mm-ss"，认了就会把记忆搬进时间戳文件
				if (!SaveIdentity.ShouldAdoptOnSave(success, auto, saveName))
				{
					if (success && !auto) log.Info("Save event without usable name, ignored.");
					return;
				}

				// 只允许把「本次会话的占位文件」改名接管；正式存档名之间不搬文件
				Store.AdoptSaveName(saveName);
				if (m_Index.Set(m_CurrentGuid ?? metaId, saveName)) SaveIndexToDisk();
				if (m_System != null) m_System.FlushIfDirty();
				else if (Store.Dirty) Store.SaveToDisk();
				log.Info("Save named '" + saveName + "', memory flushed.");
			}
			catch (Exception ex)
			{
				log.Warn("OnGameSaveLoad: " + ex.GetType().Name);
			}
		}

		private void LoadIndex()
		{
			try
			{
				string path = MemoryStore.IndexPath;
				if (System.IO.File.Exists(path))
				{
					m_Index = SaveIndex.Parse(System.IO.File.ReadAllText(path, Encoding.UTF8));
				}
			}
			catch (Exception ex)
			{
				log.Warn("LoadIndex failed: " + ex.GetType().Name);
			}
		}

		private void SaveIndexToDisk()
		{
			try
			{
				System.IO.Directory.CreateDirectory(MemoryStore.DataDirectory);
				System.IO.File.WriteAllText(MemoryStore.IndexPath, m_Index.Serialize(), new UTF8Encoding(false));
			}
			catch (Exception ex)
			{
				log.Warn("SaveIndex failed: " + ex.GetType().Name);
			}
		}

		private void OnApplicationQuitting()
		{
			SafeFlush("quitting");
		}

		private void OnProcessExit(object sender, EventArgs e)
		{
			SafeFlush("processExit");
		}

		private void SafeFlush(string reason)
		{
			try
			{
				if (m_System != null)
				{
					m_System.OnLeavingGame();
				}
				else if (Store != null && Store.Dirty)
				{
					Store.SaveToDisk();
				}
				log.Info("Flush on " + reason + " done.");
			}
			catch (Exception ex)
			{
				log.Warn("Flush on " + reason + " failed: " + ex.GetType().Name);
			}
		}

		public void OnDispose()
		{
			log.Info("Tool Mode Memory OnDispose");
			try
			{
				GameManager gm = GameManager.instance;
				if (gm != null)
				{
					gm.onGameLoadingComplete -= OnGameLoadingComplete;
					gm.onGameSaveLoad -= OnGameSaveLoad;
					gm.onGamePreload -= OnGamePreload;
				}
			}
			catch { }

			try
			{
				if (m_QuitHooked)
				{
					Application.quitting -= OnApplicationQuitting;
					AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
					m_QuitHooked = false;
				}
			}
			catch { }

			SafeFlush("dispose");

			try
			{
				if (m_Setting != null)
				{
					m_Setting.UnregisterInOptionsUI();
					m_Setting = null;
				}
			}
			catch (Exception ex)
			{
				log.Warn("UnregisterInOptionsUI: " + ex.GetType().Name);
			}
			ToolModeMemorySettings.Instance = null;
			ToolModeMemorySettings.Ready = false;
			m_System = null;
			Store = null;
			if (s_Instance == this) s_Instance = null;
		}
	}
}
