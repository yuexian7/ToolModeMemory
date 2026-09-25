using System;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Assets;
using Game.Modding;
using Game.SceneFlow;
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
		public const string kVersion = "0.1.1";

		public static ILog log = LogManager.GetLogger(nameof(ToolModeMemory)).SetShowsErrorsInUI(false);

		public static MemoryStore Store;

		private Setting m_Setting;
		private ToolMemorySystem m_System;
		private bool m_QuitHooked;

		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info("Tool Mode Memory v" + kVersion + " loading (local build, not published)...");

			Store = new MemoryStore();

			m_Setting = new Setting(this);
			Setting.Instance = m_Setting;

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
				AssetDatabase.global.LoadSettings(nameof(ToolModeMemory), m_Setting, new Setting(this));
			}
			catch (Exception ex)
			{
				log.Warn("LoadSettings failed, using defaults: " + ex.GetType().Name);
			}

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

			log.Info("Tool Mode Memory v" + kVersion + " loaded. Master switch default=OFF (vanilla).");
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

		private void OnGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
		{
			// 原版在这里 ResetToolPreferences；我们稍后在 LoadingComplete 恢复
		}

		private void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
		{
			try
			{
				if (mode == GameMode.Game)
				{
					string saveName = TryGetSaveName();
					if (m_System != null)
					{
						m_System.OnEnteredGame(saveName);
						if (m_Setting != null && m_Setting.Enabled)
						{
							m_System.RequestApply();
						}
					}
					else
					{
						if (Store != null)
						{
							if (!string.IsNullOrEmpty(saveName)) Store.SetSaveName(saveName);
							else Store.EnsureSessionName();
							Store.LoadForCurrentSave();
						}
					}
					log.Info("Entered game, saveName=" + (saveName ?? "(none)"));
				}
				else if (mode == GameMode.MainMenu)
				{
					if (m_System != null)
					{
						m_System.OnLeavingGame();
					}
					else if (Store != null && Store.Dirty)
					{
						Store.SaveToDisk();
					}
					log.Info("Back to main menu.");
				}
			}
			catch (Exception ex)
			{
				log.Warn("OnGameLoadingComplete: " + ex.GetType().Name);
			}
		}

		private void OnGameSaveLoad(string saveName, string previewUri, bool start, bool success)
		{
			try
			{
				if (start) return;
				if (!success) return;
				if (string.IsNullOrEmpty(saveName)) return;
				if (Store != null)
				{
					Store.SetSaveName(saveName);
				}
				if (m_System != null)
				{
					m_System.FlushIfDirty();
				}
				else if (Store != null)
				{
					Store.SaveToDisk();
				}
				log.Info("Save named '" + saveName + "', memory flushed.");
			}
			catch (Exception ex)
			{
				log.Warn("OnGameSaveLoad: " + ex.GetType().Name);
			}
		}

		private static string TryGetSaveName()
		{
			try
			{
				Game.Settings.UserState userState = GameManager.instance.settings.userState;
				if (userState != null && userState.lastSaveGameMetadata != null)
				{
					SaveGameMetadata meta = userState.lastSaveGameMetadata;
					if (meta.target != null && !string.IsNullOrEmpty(meta.target.displayName))
					{
						return meta.target.displayName;
					}
					if (!string.IsNullOrEmpty(meta.identifier)) return meta.identifier;
				}
			}
			catch { }
			return null;
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
			Setting.Instance = null;
			m_System = null;
			Store = null;
		}
	}
}
