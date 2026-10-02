using System;
using System.Collections.Generic;
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
		public const string kVersion = "0.4.0";

		public static ILog log = LogManager.GetLogger(nameof(ToolModeMemory)).SetShowsErrorsInUI(false);

		public static MemoryStore Store;

		private static ToolModeMemoryMod s_Instance;

		private ToolModeMemorySettings m_Setting;
		private ToolMemorySystem m_System;
		private bool m_QuitHooked;
		private SaveIndex m_Index = new SaveIndex();

		/// <summary>
		/// 我们自己注册进本地化系统的 12 个词条源（卸载模组时必须一个个摘掉：
		/// 本地化管理器是游戏对象，活得比模组久，留着就是对已卸载程序集的悬挂引用）。
		/// </summary>
		private string[] m_LocaleLocales;
		private LocaleSource[] m_LocaleSources;

		/// <summary>当前存档的 guid 文本（进档时解析出来就留着，用于索引）。</summary>
		private string m_CurrentGuid;

		/// <summary>当前是否真的在「存档」里（编辑器 / 主菜单不算）。</summary>
		private bool m_InSave;

		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info("Tool Mode Memory v" + kVersion + " loading...");
			s_Instance = this;

			Store = new MemoryStore();

			// 开机先做一次清理：随机会话名 (_unsaved_xxxx) 永远不可能再被读到，
			// 旧版本认不出存档名留下的 _auto_<guid> 现在能改名成真正的存档名。
			// 资产库此时若还没缓存好，改名会整体空转，不会误删任何东西。
			try
			{
				int junk = MemoryStore.CleanSessionPlaceholders(null);
				if (junk > 0) log.Info("Removed " + junk + " unreachable session file(s).");
			}
			catch { }
			RecoverPlaceholderFiles();

			m_Setting = new ToolModeMemorySettings(this);
			ToolModeMemorySettings.Instance = m_Setting;

			try
			{
				string[] locales = LocaleTable.Locales;
				m_LocaleSources = new LocaleSource[locales.Length];
				for (int i = 0; i < locales.Length; i++)
				{
					m_LocaleSources[i] = new LocaleSource(m_Setting, locales[i]);
					GameManager.instance.localizationManager.AddSource(locales[i], m_LocaleSources[i]);
				}
				m_LocaleLocales = locales;
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
					RecoverPlaceholderFiles();
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

				// 只有「载入已有存档」才认这个 guid：新建城市时 instigatorGuid 是地图的 id，
				// 拿它建索引会让同一张地图上的两座新城共用记忆文件。
				SaveIdentity.PurposeKind kind = SaveIdentity.Classify((int)purpose);
				Colossal.Hash128 loadGuid;
				bool haveGuid = TryGetInstigatorGuid(out loadGuid);
				string guidText = (kind == SaveIdentity.PurposeKind.LoadedSave && haveGuid)
					? loadGuid.ToString() : null;
				m_CurrentGuid = guidText;

				SaveIdentity.SaveMetaInfo metaInfo = guidText != null
					? ReadSaveMeta(loadGuid) : new SaveIdentity.SaveMetaInfo();

				// 只有真的进了自动存档才扫一次资产库（几条 metadata JSON，不读存档本体）。
				// 扫它只为一件事：顺着 sessionGuid 找这份自动存档出自哪个手动存档。
				SaveIdentity.SaveEntry[] siblings = (metaInfo.known && metaInfo.autoSave)
					? CollectSaves(true) : null;

				string resolved;
				bool isPlaceholder;
				bool named = SaveIdentity.TryResolveName(kind, guidText != null, guidText,
					metaInfo, m_Index, siblings, out resolved, out isPlaceholder);

				if (named)
				{
					if (isPlaceholder) Store.UsePlaceholderName(resolved);
					else Store.UseSaveName(resolved);

					// 0.2.3 及以前认不出存档名，把记忆写在 _auto_<guid>.json 里。
					// 现在名字确证了，先搬家再读，玩家升级后第一次进档就接得上。
					if (Store.MigrateLegacyFile(SaveIdentity.LegacyAutoName(guidText)))
					{
						log.Info("Moved legacy memory file '" + SaveIdentity.LegacyAutoName(guidText)
							+ "' to '" + Store.SaveName + "'.");
					}

					// 记下 guid -> 存档名：资产库偶尔查不到（存档正在改名、云端只读副本）时还能认回来
					if (!isPlaceholder)
					{
						bool changed = m_Index.Set(guidText, Store.SaveName);
						// 顺带记「这条会话链属于哪个存档名」：原存档被删了也还认得它的自动存档
						if (!string.IsNullOrEmpty(metaInfo.sessionGuid))
						{
							changed |= m_Index.Set(SaveIdentity.SessionKey(metaInfo.sessionGuid), Store.SaveName);
						}
						if (changed) SaveIndexToDisk();
					}
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
		/// 按本次载入的 guid 反查存档元数据，取游戏里显示的那个存档名。
		///
		/// 依据（反编译 Game.dll）：
		///   - GameManager.Load:1196 把 saveGameMetadata.id 交给序列化上下文 ⇒ instigatorGuid
		///     就是这条存档元数据的 guid；
		///   - GameManager.Load:1219 游戏自己按 guid 载入存档用的就是
		///     AssetDatabase.global.TryGetAsset(guid, out asset)，反查通道可靠；
		///   - AssetData.name => database.GetName(id) => FileSystemDataSource.GetName:719
		///     返回 Unescape(文件名)，即玩家在存档列表里看到的名字（= 存档包名，不是城市名）。
		///
		/// 载入完成后游戏会 Dispose 那份元数据，但那只是 Unload（m_Target 置空），
		/// 资产仍注册在库里，name 照读、target 会按需重新 Load。
		/// 只在主线程的进档回调里调用：AssetDatabase.m_Databases 是普通 HashSet，不是线程安全的。
		/// </summary>
		private static SaveIdentity.SaveMetaInfo ReadSaveMeta(Colossal.Hash128 guid)
		{
			SaveIdentity.SaveMetaInfo info = new SaveIdentity.SaveMetaInfo();
			try
			{
				SaveGameMetadata meta;
				if (!AssetDatabase.global.TryGetAsset<SaveGameMetadata>(guid, out meta) || meta == null)
				{
					return info;
				}
				info.name = meta.name;
				SaveInfo target = null;
				try { target = meta.target; } catch { }
				if (target == null)
				{
					// 元数据读不开：没有 autoSave 标志就无法排除时间戳名，按「查不到」处理
					return info;
				}
				info.cityName = target.cityName;
				info.autoSave = target.autoSave;
				info.sessionGuid = FormatSessionGuid(target.sessionGuid);
				info.known = SaveIdentity.IsUsableName(info.name);
			}
			catch (Exception ex)
			{
				log.Warn("ReadSaveMeta: " + ex.GetType().Name + " " + ex.Message);
			}
			return info;
		}

		/// <summary>
		/// 存档元数据里的 sessionGuid（"N" 格式）。Guid.Empty = 这条链无从判断
		/// （1.6 之前存的档、或根本没写过这个字段的档），一律当未知。
		/// </summary>
		private static string FormatSessionGuid(Guid g)
		{
			if (g == Guid.Empty) return null;
			return g.ToString("N");
		}

		/// <summary>扫描条数上限：正常玩家几十条存档，封顶只为防极端情况卡住进档。</summary>
		private const int kMaxSaveScan = 300;

		private static SaveIdentity.SaveEntry[] s_SaveScan;

		/// <summary>
		/// 扫资产库里所有存档的元数据，交给 SaveIdentity 判「这份自动存档出自哪个手动存档」。
		/// reset=false 时复用上一次的结果（主菜单批量搬旧占位文件时不必每搬一条重扫一遍）。
		///
		/// 只在两个地方调用：进档（且进的是自动存档）与主菜单的一次性清理。
		/// Metadata&lt;T&gt;.target 是懒读文件的（每条几 KB 的 JSON），一次扫描 = 几十次小文件读，
		/// 亚毫秒级；绝不放进每帧路径（性能纪律见 开发笔记「不进每帧」一节）。
		/// </summary>
		private static SaveIdentity.SaveEntry[] CollectSaves(bool reset)
		{
			if (!reset && s_SaveScan != null) return s_SaveScan;
			List<SaveIdentity.SaveEntry> list = new List<SaveIdentity.SaveEntry>(32);
			try
			{
				IEnumerable<SaveGameMetadata> all = AssetDatabase.global.GetAssets<SaveGameMetadata>();
				if (all != null)
				{
					foreach (SaveGameMetadata m in all)
					{
						if (m == null) continue;
						if (list.Count >= kMaxSaveScan) break;
						SaveInfo t;
						try { t = m.target; } catch { continue; }
						if (t == null) continue;
						if (!SaveIdentity.IsUsableName(m.name)) continue;
						SaveIdentity.SaveEntry e = new SaveIdentity.SaveEntry();
						e.name = m.name;
						e.cityName = t.cityName;
						e.autoSave = t.autoSave;
						e.sessionGuid = FormatSessionGuid(t.sessionGuid);
						e.modified = t.lastModified.Ticks;
						list.Add(e);
					}
				}
			}
			catch (Exception ex)
			{
				log.Warn("CollectSaves: " + ex.GetType().Name + " " + ex.Message);
			}
			s_SaveScan = list.ToArray();
			return s_SaveScan;
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
				string sessionGuid = null;
				bool auto = true;
				try
				{
					SaveGameMetadata meta = GameManager.instance.settings.userState.lastSaveGameMetadata;
					if (meta != null)
					{
						// 口径必须和 instigatorGuid 一致：Identifier.ToString() 会带 "[uri]"
						Colossal.Hash128 metaGuid = meta.id;
						if (metaGuid.isValid) metaId = metaGuid.ToString();
						if (meta.target != null)
						{
							auto = meta.target.autoSave;
							sessionGuid = FormatSessionGuid(meta.target.sessionGuid);
						}
					}
				}
				catch { }

				// 自动存档的名字是 "dd-MMMM-HH-mm-ss"，认了就会把记忆搬进时间戳文件
				if (!SaveIdentity.ShouldAdoptOnSave(success, auto, saveName))
				{
					if (success && !auto) log.Info("Save event without usable name, ignored.");
					return;
				}

				// 占位名（新建城市第一次存盘）→ 把占位文件改名接管；
				// 已经有正式名字了但玩家「另存为」成别的名字 → 复制一份给新档，本局之后按新名字写。
				if (Store.IsPlaceholder) Store.AdoptSaveName(saveName);
				else Store.CarryOverTo(saveName);
				bool changed = m_Index.Set(m_CurrentGuid ?? metaId, saveName);
				// 玩家手动存盘 = 这条会话链现在有了正式名字，记下来，之后它的自动存档就认这个名字
				if (!string.IsNullOrEmpty(sessionGuid))
				{
					changed |= m_Index.Set(SaveIdentity.SessionKey(sessionGuid), saveName);
				}
				if (changed) SaveIndexToDisk();
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

		/// <summary>
		/// 把旧版本留下的 _auto_&lt;guid&gt;.json 改成真正的存档名。
		/// 只在主菜单里跑（不在存档内、文件没人用），而且只做「目标不存在才搬」的重命名，
		/// 绝不删任何对得上号的文件：认不出名字的一律原地留着。
		/// </summary>
		private static void RecoverPlaceholderFiles()
		{
			try
			{
				string dir = MemoryStore.DataDirectory;
				if (!System.IO.Directory.Exists(dir)) return;
				string[] files = System.IO.Directory.GetFiles(dir, SaveIdentity.AUTO_PREFIX + "*.json");
				if (files.Length == 0) return;
				CollectSaves(true);   // 一次性扫库，下面每条旧文件都复用这份结果
				int moved = 0;
				for (int i = 0; i < files.Length; i++)
				{
					string stem = System.IO.Path.GetFileNameWithoutExtension(files[i]);
					if (stem.Length <= SaveIdentity.AUTO_PREFIX.Length + 8) continue;
					string real = ResolveNameForGuid(stem.Substring(SaveIdentity.AUTO_PREFIX.Length));
					if (string.IsNullOrEmpty(real) || real == stem) continue;
					string target = System.IO.Path.Combine(dir, MemoryStore.SanitizeFileName(real) + ".json");
					if (System.IO.File.Exists(target)) continue;
					try
					{
						System.IO.File.Move(files[i], target);
						moved++;
					}
					catch { }
				}
				if (moved > 0) log.Info("Renamed " + moved + " legacy memory file(s) to their save names.");
			}
			catch (Exception ex)
			{
				log.Warn("RecoverPlaceholderFiles: " + ex.GetType().Name);
			}
		}

		/// <summary>给一个存档 guid 求「游戏里显示的名字」；认不出来返回 null。</summary>
		private static string ResolveNameForGuid(string guidText)
		{
			Colossal.Hash128 g;
			if (string.IsNullOrEmpty(guidText)) return null;
			if (!Colossal.Hash128.TryParse(guidText, out g) || !g.isValid) return null;
			bool placeholder;
			string name;
			SaveIdentity.SaveMetaInfo meta = ReadSaveMeta(g);
			bool ok = SaveIdentity.TryResolveName(SaveIdentity.PurposeKind.LoadedSave, true, guidText,
				meta, s_Instance != null ? s_Instance.m_Index : new SaveIndex(),
				(meta.known && meta.autoSave) ? CollectSaves(false) : null,
				out name, out placeholder);
			if (!ok || placeholder) return null;   // 认不准就不搬：留下的 _auto_ 文件比搬错强
			return name;
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

		/// <summary>
		/// 把 OnLoad 注册的 12 个词条源逐个摘掉。本地化管理器是游戏的对象，会活得比我们久：
		/// 留着 = 卸载后每次重载语言都要回调进一个已消失的程序集。
		/// 注册到一半就失败的情况也覆盖（数组里允许出现 null）。
		/// </summary>
		private void RemoveLocaleSources()
		{
			if (m_LocaleSources == null || m_LocaleLocales == null) return;
			Colossal.Localization.LocalizationManager lm = null;
			try { lm = GameManager.instance != null ? GameManager.instance.localizationManager : null; } catch { }
			if (lm == null) return;
			int removed = 0;
			for (int i = 0; i < m_LocaleLocales.Length && i < m_LocaleSources.Length; i++)
			{
				if (m_LocaleSources[i] == null) continue;
				try
				{
					lm.RemoveSource(m_LocaleLocales[i], m_LocaleSources[i]);
					removed++;
				}
				catch { }
			}
			m_LocaleSources = null;
			m_LocaleLocales = null;
			log.Info("Locale sources removed: " + removed);
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

			// 卸载模组 = 本程序集随时可能消失，凡是「挂在游戏对象上的东西」都必须当场摘干净，
			// 否则玩家下次进存档就会看到因本模组卸载而引起的报错（需求 8）。
			try
			{
				if (m_System != null) m_System.Detach();
			}
			catch (Exception ex)
			{
				log.Warn("System detach: " + ex.GetType().Name);
			}

			try
			{
				RemoveLocaleSources();
			}
			catch (Exception ex)
			{
				log.Warn("RemoveLocaleSources: " + ex.GetType().Name);
			}

			// 桥接类里缓存的都是当前 World 的对象（系统实例、prefabSystem、EntityManager、
			// 工具栏 UI 系统、Anarchy 实例）：世界随时可能被销毁，退出前一律丢掉。
			try
			{
				ToolMemoryBridge.ForgetNames();
			}
			catch { }

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
