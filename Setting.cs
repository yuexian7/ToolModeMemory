using System;
using System.Collections.Generic;
using System.IO;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using ToolModeMemory.Memory;
using UnityEngine;

namespace ToolModeMemory
{
	/// <summary>
	/// Tool Mode Memory 选项页：模组设置 / 记忆管理 两个标签页。
	/// 全部改动实时生效（setter 内 Sync）。
	/// </summary>
	[FileLocation(nameof(ToolModeMemory))]
	[SettingsUITabOrder(kTabMod, kTabAbout)]
	[SettingsUIGroupOrder(kGroupMain, kGroupNet, kGroupObj, kGroupZone, kGroupReset, kGroupAbout)]
	[SettingsUIShowGroupName(kGroupMain, kGroupNet, kGroupObj, kGroupZone, kGroupReset)]
	public class Setting : ModSetting
	{
		public const string kTabMod = "ModSettings";
		public const string kTabAbout = "About";

		public const string kGroupMain = "Main";
		public const string kGroupNet = "NetItems";
		public const string kGroupObj = "ObjItems";
		public const string kGroupZone = "ZoneItems";
		public const string kGroupReset = "MemoryReset";
		/// <summary>关于页底部信息板块（不显示板块名）。</summary>
		public const string kGroupAbout = "AboutInfo";

		public const int kScopeGroup = 0;
		public const int kScopeMenu = 1;
		public const int kScopeCategory = 2;
		public const int kScopeGlobalShared = 3;
		public const int kScopeGlobalUnique = 4;

		public static Setting Instance;

		private bool m_Enabled;
		private readonly Dictionary<string, bool> m_ItemEnabled = new Dictionary<string, bool>(StringComparer.Ordinal);
		private readonly Dictionary<string, int> m_ItemScope = new Dictionary<string, int>(StringComparer.Ordinal);

		public Setting(IMod mod) : base(mod)
		{
			InitItemDefaults();
		}

		private void InitItemDefaults()
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				m_ItemEnabled[items[i].Id] = true;
				m_ItemScope[items[i].Id] = items[i].RecommendedScope;
			}
		}

		/// <summary>总开关。关闭 = 完全原版行为。</summary>
		[SettingsUISection(kTabMod, kGroupMain)]
		public bool Enabled
		{
			get { return m_Enabled; }
			set
			{
				m_Enabled = value;
				Sync();
			}
		}

		public bool IsMasterOff()
		{
			return !m_Enabled;
		}

		// ---------- 工具项：启用 + 范围（由宏展开式手写，保证框架反射可见） ----------

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool NetDrawEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kNetDraw); }
			set { SetEnabled(ToolItemCatalog.kNetDraw, value); }
		}

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetNetDrawScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsNetDrawScopeDisabled))]
		public int NetDrawScope
		{
			get { return GetScope(ToolItemCatalog.kNetDraw); }
			set { SetScope(ToolItemCatalog.kNetDraw, value); }
		}

		public bool IsNetDrawScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kNetDraw); }

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool NetSnapEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kNetSnap); }
			set { SetEnabled(ToolItemCatalog.kNetSnap, value); }
		}

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetNetSnapScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsNetSnapScopeDisabled))]
		public int NetSnapScope
		{
			get { return GetScope(ToolItemCatalog.kNetSnap); }
			set { SetScope(ToolItemCatalog.kNetSnap, value); }
		}

		public bool IsNetSnapScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kNetSnap); }

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool NetParallelEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kNetParallel); }
			set { SetEnabled(ToolItemCatalog.kNetParallel, value); }
		}

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetNetParallelScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsNetParallelScopeDisabled))]
		public int NetParallelScope
		{
			get { return GetScope(ToolItemCatalog.kNetParallel); }
			set { SetScope(ToolItemCatalog.kNetParallel, value); }
		}

		public bool IsNetParallelScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kNetParallel); }

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool NetUndergroundEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kNetUnderground); }
			set { SetEnabled(ToolItemCatalog.kNetUnderground, value); }
		}

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetNetUndergroundScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsNetUndergroundScopeDisabled))]
		public int NetUndergroundScope
		{
			get { return GetScope(ToolItemCatalog.kNetUnderground); }
			set { SetScope(ToolItemCatalog.kNetUnderground, value); }
		}

		public bool IsNetUndergroundScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kNetUnderground); }

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool NetElevationEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kNetElevation); }
			set { SetEnabled(ToolItemCatalog.kNetElevation, value); }
		}

		[SettingsUISection(kTabMod, kGroupNet)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetNetElevationScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsNetElevationScopeDisabled))]
		public int NetElevationScope
		{
			get { return GetScope(ToolItemCatalog.kNetElevation); }
			set { SetScope(ToolItemCatalog.kNetElevation, value); }
		}

		public bool IsNetElevationScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kNetElevation); }

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool ObjPlaceEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kObjPlace); }
			set { SetEnabled(ToolItemCatalog.kObjPlace, value); }
		}

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetObjPlaceScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsObjPlaceScopeDisabled))]
		public int ObjPlaceScope
		{
			get { return GetScope(ToolItemCatalog.kObjPlace); }
			set { SetScope(ToolItemCatalog.kObjPlace, value); }
		}

		public bool IsObjPlaceScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kObjPlace); }

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool ObjAlignEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kObjAlign); }
			set { SetEnabled(ToolItemCatalog.kObjAlign, value); }
		}

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetObjAlignScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsObjAlignScopeDisabled))]
		public int ObjAlignScope
		{
			get { return GetScope(ToolItemCatalog.kObjAlign); }
			set { SetScope(ToolItemCatalog.kObjAlign, value); }
		}

		public bool IsObjAlignScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kObjAlign); }

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool ObjUndergroundEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kObjUnderground); }
			set { SetEnabled(ToolItemCatalog.kObjUnderground, value); }
		}

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetObjUndergroundScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsObjUndergroundScopeDisabled))]
		public int ObjUndergroundScope
		{
			get { return GetScope(ToolItemCatalog.kObjUnderground); }
			set { SetScope(ToolItemCatalog.kObjUnderground, value); }
		}

		public bool IsObjUndergroundScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kObjUnderground); }

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool UpgradeModeEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kUpgradeMode); }
			set { SetEnabled(ToolItemCatalog.kUpgradeMode, value); }
		}

		[SettingsUISection(kTabMod, kGroupObj)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetUpgradeModeScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsUpgradeModeScopeDisabled))]
		public int UpgradeModeScope
		{
			get { return GetScope(ToolItemCatalog.kUpgradeMode); }
			set { SetScope(ToolItemCatalog.kUpgradeMode, value); }
		}

		public bool IsUpgradeModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kUpgradeMode); }

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool ZoneModeEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kZoneMode); }
			set { SetEnabled(ToolItemCatalog.kZoneMode, value); }
		}

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetZoneModeScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsZoneModeScopeDisabled))]
		public int ZoneModeScope
		{
			get { return GetScope(ToolItemCatalog.kZoneMode); }
			set { SetScope(ToolItemCatalog.kZoneMode, value); }
		}

		public bool IsZoneModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kZoneMode); }

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool AreaModeEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kAreaMode); }
			set { SetEnabled(ToolItemCatalog.kAreaMode, value); }
		}

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetAreaModeScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsAreaModeScopeDisabled))]
		public int AreaModeScope
		{
			get { return GetScope(ToolItemCatalog.kAreaMode); }
			set { SetScope(ToolItemCatalog.kAreaMode, value); }
		}

		public bool IsAreaModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kAreaMode); }

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool WaterModeEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kWaterMode); }
			set { SetEnabled(ToolItemCatalog.kWaterMode, value); }
		}

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetWaterModeScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsWaterModeScopeDisabled))]
		public int WaterModeScope
		{
			get { return GetScope(ToolItemCatalog.kWaterMode); }
			set { SetScope(ToolItemCatalog.kWaterMode, value); }
		}

		public bool IsWaterModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kWaterMode); }

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool TerrainModeEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kTerrainMode); }
			set { SetEnabled(ToolItemCatalog.kTerrainMode, value); }
		}

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetTerrainModeScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsTerrainModeScopeDisabled))]
		public int TerrainModeScope
		{
			get { return GetScope(ToolItemCatalog.kTerrainMode); }
			set { SetScope(ToolItemCatalog.kTerrainMode, value); }
		}

		public bool IsTerrainModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kTerrainMode); }

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsMasterOff))]
		public bool BulldozeModeEnabled
		{
			get { return GetEnabled(ToolItemCatalog.kBulldozeMode); }
			set { SetEnabled(ToolItemCatalog.kBulldozeMode, value); }
		}

		[SettingsUISection(kTabMod, kGroupZone)]
		[SettingsUIDropdown(typeof(Setting), nameof(GetBulldozeModeScopeItems))]
		[SettingsUIValueVersion(typeof(Setting), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(Setting), nameof(IsBulldozeModeScopeDisabled))]
		public int BulldozeModeScope
		{
			get { return GetScope(ToolItemCatalog.kBulldozeMode); }
			set { SetScope(ToolItemCatalog.kBulldozeMode, value); }
		}

		public bool IsBulldozeModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kBulldozeMode); }

		// ---------- 记忆管理 + 关于（About 标签页） ----------

		[SettingsUISection(kTabAbout, kGroupReset)]
		[SettingsUIButton]
		[SettingsUIConfirmation(null, "CONFIRM_RESET")]
		public bool ResetMemory
		{
			set { DoResetMemory(); }
		}

		[SettingsUISection(kTabAbout, kGroupReset)]
		[SettingsUIButton]
		public bool OpenMemoryFolder
		{
			set { DoOpenMemoryFolder(); }
		}

		[SettingsUISection(kTabAbout, kGroupAbout)]
		[SettingsUIMultilineText]
		public string ModVersion
		{
			get { return ToolModeMemoryMod.kVersion; }
			set { }
		}

		[SettingsUISection(kTabAbout, kGroupAbout)]
		[SettingsUIMultilineText]
		public string ModAuthor
		{
			get { return "yuexian"; }
			set { }
		}

		[SettingsUISection(kTabAbout, kGroupAbout)]
		[SettingsUIButton]
		public bool OpenKofi
		{
			set { OpenUrl("https://ko-fi.com/yuexian7"); }
		}

		[SettingsUISection(kTabAbout, kGroupAbout)]
		[SettingsUIButton]
		public bool OpenForum
		{
			set { OpenUrl("https://forum.paradoxplaza.com/forum/threads/access-anarchy.1941285/latest"); }
		}

		[SettingsUISection(kTabAbout, kGroupAbout)]
		[SettingsUIButton]
		public bool OpenRainbowSite
		{
			set { OpenUrl("https://rainbow-series-hvpma89wi25.qoder.zone/#top"); }
		}

		private static void OpenUrl(string url)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("OpenUrl failed: " + ex.GetType().Name);
			}
		}

		// ---------- 范围下拉（每项独立 getter，标注原版/推荐） ----------

		public DropdownItem<int>[] GetNetDrawScopeItems() { return BuildScopeItems(ToolItemCatalog.kNetDraw); }
		public DropdownItem<int>[] GetNetSnapScopeItems() { return BuildScopeItems(ToolItemCatalog.kNetSnap); }
		public DropdownItem<int>[] GetNetParallelScopeItems() { return BuildScopeItems(ToolItemCatalog.kNetParallel); }
		public DropdownItem<int>[] GetNetUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kNetUnderground); }
		public DropdownItem<int>[] GetNetElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kNetElevation); }
		public DropdownItem<int>[] GetObjPlaceScopeItems() { return BuildScopeItems(ToolItemCatalog.kObjPlace); }
		public DropdownItem<int>[] GetObjAlignScopeItems() { return BuildScopeItems(ToolItemCatalog.kObjAlign); }
		public DropdownItem<int>[] GetObjUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kObjUnderground); }
		public DropdownItem<int>[] GetZoneModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kZoneMode); }
		public DropdownItem<int>[] GetAreaModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kAreaMode); }
		public DropdownItem<int>[] GetWaterModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kWaterMode); }
		public DropdownItem<int>[] GetTerrainModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kTerrainMode); }
		public DropdownItem<int>[] GetBulldozeModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kBulldozeMode); }
		public DropdownItem<int>[] GetUpgradeModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kUpgradeMode); }

		private DropdownItem<int>[] BuildScopeItems(string itemId)
		{
			LocaleTable.SetActiveLocale(GameManager.instance.localizationManager.activeLocaleId);
			ToolItemDef def = ToolItemCatalog.Find(itemId);
			return LocaleTable.BuildScopeDropdown(def);
		}

		public int GetScopeItemsVersion()
		{
			return GameManager.instance.localizationManager.activeLocaleId.GetHashCode();
		}

		// ---------- 读写小工具 ----------

		public bool IsItemEnabled(string id)
		{
			if (!m_Enabled) return false;
			bool v;
			return m_ItemEnabled.TryGetValue(id, out v) && v;
		}

		public MemoryScope GetItemScope(string id)
		{
			int v;
			if (!m_ItemScope.TryGetValue(id, out v)) return MemoryScope.Group;
			if (v < 0 || v > 4) return MemoryScope.Group;
			return (MemoryScope)v;
		}

		private bool GetEnabled(string id)
		{
			bool v;
			return m_ItemEnabled.TryGetValue(id, out v) && v;
		}

		private void SetEnabled(string id, bool value)
		{
			m_ItemEnabled[id] = value;
			Sync();
		}

		private int GetScope(string id)
		{
			int v;
			return m_ItemScope.TryGetValue(id, out v) ? v : 0;
		}

		private void SetScope(string id, int value)
		{
			m_ItemScope[id] = value;
			Sync();
		}

		/// <summary>设置变更后立刻作用到运行中的工具。</summary>
		public void Sync()
		{
			try
			{
				Systems.ToolMemorySystem sys = null;
				if (Unity.Entities.World.DefaultGameObjectInjectionWorld != null)
				{
					sys = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<Systems.ToolMemorySystem>();
				}
				if (sys != null)
				{
					sys.RequestApply();
					sys.RequestDiagnostics();
				}
			}
			catch { }
		}

		private void DoResetMemory()
		{
			try
			{
				bool inGame = GameManager.instance != null && GameManager.instance.gameMode.IsGame();
				MemoryStore store = ToolModeMemoryMod.Store;
				if (store == null) return;
				if (inGame)
				{
					store.ResetCurrentSave();
					ToolModeMemoryMod.log.Info("Reset memory for current save.");
				}
				else
				{
					store.ResetAllSaves();
					ToolModeMemoryMod.log.Info("Reset memory for ALL saves.");
				}
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("ResetMemory failed: " + ex.GetType().Name);
			}
		}

		private void DoOpenMemoryFolder()
		{
			try
			{
				string dir = MemoryStore.DataDirectory;
				Directory.CreateDirectory(dir);
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = dir,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("OpenMemoryFolder failed: " + ex.GetType().Name);
			}
		}

		public override void SetDefaults()
		{
			m_Enabled = false;
			InitItemDefaults();
		}
	}
}
