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
	/// Tool Mode Memory 选项页。
	/// v0.2.2 结构：模组设置 = 三个板块 —— 顶部总开关（**不显示板块名**）、
	/// 「官方工具项设置」（原版 9 项）、「Anarchy工具项设置」（该模组的 3 项）；
	/// 序号跨板块连续 1..12；每项 = 是否恢复 + 共用范围，五种共用范围的定义写在
	/// 每一项自己的共用范围说明里。关于 = 记忆管理 + 兼容性 + 版本/作者/链接按钮。
	/// 总开关关闭时，下面所有工具项仍然变灰不可点（DisableByCondition 仍挂在每一项上）。
	/// 全部改动实时生效（setter 内 Sync + Persist）。
	/// </summary>
	[FileLocation(nameof(ToolModeMemory))]
	[SettingsUITabOrder(kTabMod, kTabAbout)]
	[SettingsUIGroupOrder(kGroupMaster, kGroupOfficial, kGroupAnarchy, kGroupMemory, kGroupCompat, kGroupInfo)]
	[SettingsUIShowGroupName(kGroupOfficial, kGroupAnarchy, kGroupMemory, kGroupCompat)]
	public class ToolModeMemorySettings : ModSetting
	{
		public const string kTabMod = "ModSettings";
		public const string kTabAbout = "About";

		/// <summary>总开关板块：单独一块，按用户要求不显示板块名（不在 ShowGroupName 里）。</summary>
		public const string kGroupMaster = "Master";
		/// <summary>原版工具项板块。</summary>
		public const string kGroupOfficial = "OfficialItems";
		/// <summary>第三方模组板块：一个模组一块，板块名 = 模组名 + 「工具项设置」。</summary>
		public const string kGroupAnarchy = "AnarchyItems";
		public const string kGroupMemory = "MemoryFiles";
		/// <summary>需求 9：关于页中间的兼容性板块。</summary>
		public const string kGroupCompat = "Compatibility";
		/// <summary>关于页底部信息板块（不显示板块名）。</summary>
		public const string kGroupInfo = "AboutInfo";

		public static ToolModeMemorySettings Instance;

		/// <summary>
		/// LoadSettings 是用属性 setter 反序列化的；在配置读入完成前必须屏蔽
		/// Sync/Persist，否则开局就会触发十几次设置落盘任务。
		/// </summary>
		public static bool Ready;

		private bool m_Enabled = true;
		private bool m_CompatOtherMods = true;
		private readonly Dictionary<string, bool> m_ItemEnabled = new Dictionary<string, bool>(StringComparer.Ordinal);
		private readonly Dictionary<string, int> m_ItemScope = new Dictionary<string, int>(StringComparer.Ordinal);

		/// <summary>
		/// 模组设置不走 SharedSettings.Reset()：框架只把「当前实例」交给
		/// AssetDatabase.LoadSettings(name, obj, defaultObj)（AssetDatabase.cs:611），
		/// 没有存档文件时没人会调用 SetDefaults()。所以出厂默认必须在构造函数里就落好，
		/// 这也让「总开关默认打开」真的成立（原版 GameplaySettings 的写法一致）。
		/// </summary>
		public ToolModeMemorySettings(IMod mod) : base(mod)
		{
			SetDefaults();
		}

		private void InitItemDefaults()
		{
			ToolItemDef[] items = ToolItemCatalog.Items;
			for (int i = 0; i < items.Length; i++)
			{
				m_ItemEnabled[items[i].Id] = items[i].DefaultEnabled;
				m_ItemScope[items[i].Id] = items[i].EffectiveRecommendedScope();
			}
		}

		/// <summary>总开关。关闭 = 停止记忆 + 完全原版行为。默认打开。</summary>
		[SettingsUISection(kTabMod, kGroupMaster)]
		public bool Enabled
		{
			get { return m_Enabled; }
			set
			{
				m_Enabled = value;
				Sync();
				Persist();
			}
		}

		public bool IsMasterOff()
		{
			return !m_Enabled;
		}

		// ---------- 官方工具项设置（1..9）：是否恢复 + 共用范围 ----------

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool ThemesEnabled { get { return GetEnabled(ToolItemCatalog.kThemes); } set { SetEnabled(ToolItemCatalog.kThemes, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetThemesScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsThemesScopeDisabled))]
		public int ThemesScope { get { return GetScope(ToolItemCatalog.kThemes); } set { SetScope(ToolItemCatalog.kThemes, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool PacksEnabled { get { return GetEnabled(ToolItemCatalog.kPacks); } set { SetEnabled(ToolItemCatalog.kPacks, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetPacksScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsPacksScopeDisabled))]
		public int PacksScope { get { return GetScope(ToolItemCatalog.kPacks); } set { SetScope(ToolItemCatalog.kPacks, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool ToolModeEnabled { get { return GetEnabled(ToolItemCatalog.kToolMode); } set { SetEnabled(ToolItemCatalog.kToolMode, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetToolModeScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsToolModeScopeDisabled))]
		public int ToolModeScope { get { return GetScope(ToolItemCatalog.kToolMode); } set { SetScope(ToolItemCatalog.kToolMode, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool ElevationEnabled { get { return GetEnabled(ToolItemCatalog.kElevation); } set { SetEnabled(ToolItemCatalog.kElevation, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetElevationScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsElevationScopeDisabled))]
		public int ElevationScope { get { return GetScope(ToolItemCatalog.kElevation); } set { SetScope(ToolItemCatalog.kElevation, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool ParallelEnabled { get { return GetEnabled(ToolItemCatalog.kParallel); } set { SetEnabled(ToolItemCatalog.kParallel, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetParallelScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsParallelScopeDisabled))]
		public int ParallelScope { get { return GetScope(ToolItemCatalog.kParallel); } set { SetScope(ToolItemCatalog.kParallel, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool SnapEnabled { get { return GetEnabled(ToolItemCatalog.kSnap); } set { SetEnabled(ToolItemCatalog.kSnap, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetSnapScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsSnapScopeDisabled))]
		public int SnapScope { get { return GetScope(ToolItemCatalog.kSnap); } set { SetScope(ToolItemCatalog.kSnap, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool TopographyEnabled { get { return GetEnabled(ToolItemCatalog.kTopography); } set { SetEnabled(ToolItemCatalog.kTopography, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetTopographyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsTopographyScopeDisabled))]
		public int TopographyScope { get { return GetScope(ToolItemCatalog.kTopography); } set { SetScope(ToolItemCatalog.kTopography, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool UndergroundEnabled { get { return GetEnabled(ToolItemCatalog.kUnderground); } set { SetEnabled(ToolItemCatalog.kUnderground, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetUndergroundScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsUndergroundScopeDisabled))]
		public int UndergroundScope { get { return GetScope(ToolItemCatalog.kUnderground); } set { SetScope(ToolItemCatalog.kUnderground, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool OtherEnabled { get { return GetEnabled(ToolItemCatalog.kOther); } set { SetEnabled(ToolItemCatalog.kOther, value); } }

		[SettingsUISection(kTabMod, kGroupOfficial)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetOtherScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsOtherScopeDisabled))]
		public int OtherScope { get { return GetScope(ToolItemCatalog.kOther); } set { SetScope(ToolItemCatalog.kOther, value); } }

		// ---------- Anarchy工具项设置（10..12）：只在装了 Anarchy 时有实际作用 ----------

		[SettingsUISection(kTabMod, kGroupAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool AnarchyEnabled { get { return GetEnabled(ToolItemCatalog.kAnarchy); } set { SetEnabled(ToolItemCatalog.kAnarchy, value); } }

		[SettingsUISection(kTabMod, kGroupAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetAnarchyScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsAnarchyScopeDisabled))]
		public int AnarchyScope { get { return GetScope(ToolItemCatalog.kAnarchy); } set { SetScope(ToolItemCatalog.kAnarchy, value); } }

		[SettingsUISection(kTabMod, kGroupAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool LeftRightEnabled { get { return GetEnabled(ToolItemCatalog.kLeftRight); } set { SetEnabled(ToolItemCatalog.kLeftRight, value); } }

		[SettingsUISection(kTabMod, kGroupAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetLeftRightScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsLeftRightScopeDisabled))]
		public int LeftRightScope { get { return GetScope(ToolItemCatalog.kLeftRight); } set { SetScope(ToolItemCatalog.kLeftRight, value); } }

		[SettingsUISection(kTabMod, kGroupAnarchy)]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsMasterOff))]
		public bool GeneralEnabled { get { return GetEnabled(ToolItemCatalog.kGeneral); } set { SetEnabled(ToolItemCatalog.kGeneral, value); } }

		[SettingsUISection(kTabMod, kGroupAnarchy)]
		[SettingsUIDropdown(typeof(ToolModeMemorySettings), nameof(GetGeneralScopeItems))]
		[SettingsUIValueVersion(typeof(ToolModeMemorySettings), nameof(GetScopeItemsVersion))]
		[SettingsUIDisableByCondition(typeof(ToolModeMemorySettings), nameof(IsGeneralScopeDisabled))]
		public int GeneralScope { get { return GetScope(ToolItemCatalog.kGeneral); } set { SetScope(ToolItemCatalog.kGeneral, value); } }

		// ---------- 每项的共用范围行是否可点：总开关关闭 或 本项没开 ----------

		public bool IsThemesScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kThemes); }

		public bool IsPacksScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kPacks); }

		public bool IsToolModeScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kToolMode); }

		public bool IsElevationScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kElevation); }

		public bool IsParallelScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kParallel); }

		public bool IsSnapScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kSnap); }

		public bool IsTopographyScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kTopography); }

		public bool IsUndergroundScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kUnderground); }

		public bool IsOtherScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kOther); }

		public bool IsAnarchyScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kAnarchy); }

		public bool IsLeftRightScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kLeftRight); }

		public bool IsGeneralScopeDisabled() { return IsMasterOff() || !GetEnabled(ToolItemCatalog.kGeneral); }

		// ---------- 关于页：记忆管理 ----------

		[SettingsUISection(kTabAbout, kGroupMemory)]
		[SettingsUIButton]
		[SettingsUIConfirmation("CONFIRM_RESET", null)]
		public bool ResetMemory
		{
			set { DoResetMemory(); }
		}

		/// <summary>需求 1：按钮文本改为「管理所有存档的记忆文件」，行为不变（打开记忆目录）。</summary>
		[SettingsUISection(kTabAbout, kGroupMemory)]
		[SettingsUIButton]
		public bool OpenMemoryFolder
		{
			set { DoOpenMemoryFolder(); }
		}

		/// <summary>需求 1：新增「重置所有设置项」，把本模组的所有设置退回推荐值。</summary>
		[SettingsUISection(kTabAbout, kGroupMemory)]
		[SettingsUIButton]
		[SettingsUIConfirmation("CONFIRM_RESET_SETTINGS", null)]
		public bool ResetAllSettings
		{
			set { DoResetAllSettings(); }
		}

		// ---------- 关于页：兼容性（需求 9） ----------

		/// <summary>
		/// 开：按其它模组调整后的菜单/分类名去记忆（Asset UI Manager、ExtraLib 等会直接改
		/// 工具栏层级数据，资产被移到别的菜单后记忆会跟着新位置）。
		/// 关：本档内第一次解析到的菜单/分类名会一直沿用，不受中途重排影响。
		/// </summary>
		[SettingsUISection(kTabAbout, kGroupCompat)]
		public bool CompatOtherMods
		{
			get { return m_CompatOtherMods; }
			set
			{
				m_CompatOtherMods = value;
				Sync();
				Persist();
			}
		}

		public bool CompatEnabled { get { return m_CompatOtherMods; } }

		/// <summary>
		/// 读盘完成（Ready 已置真）后调用一次：LoadSettings 是用属性 setter 反序列化的，
		/// 期间 Sync 被屏蔽，所以这里补一次，让存进文件的兼容开关真的生效。
		/// </summary>
		public void AfterLoaded()
		{
			Sync();
		}

		// ---------- 关于页：版本 / 作者 / 链接 ----------

		// 注意：这里不能加 [SettingsUIMultilineText]。AutomaticSettings 给 MultilineText
		// 建的 widget 只有 label，没有 accessor（Game.UI.Menu.AutomaticSettings.cs:17-28），
		// 值永远不显示；去掉该属性才会走 StringField 读到值。
		[SettingsUISection(kTabAbout, kGroupInfo)]
		public string ModVersion
		{
			get { return ToolModeMemoryMod.kVersion; }
		}

		[SettingsUISection(kTabAbout, kGroupInfo)]
		public string ModAuthor
		{
			get { return "yuexian"; }
		}

		// 需求 1：三个链接按钮排成一行（框架支持：SettingsUIButtonGroup -> ButtonRow 横向）。
		[SettingsUISection(kTabAbout, kGroupInfo)]
		[SettingsUIButton]
		[SettingsUIButtonGroup("aboutLinks")]
		public bool OpenKofi
		{
			set { OpenUrl("https://ko-fi.com/yuexian7"); }
		}

		[SettingsUISection(kTabAbout, kGroupInfo)]
		[SettingsUIButton]
		[SettingsUIButtonGroup("aboutLinks")]
		public bool OpenForum
		{
			// 这里曾经是模板（AccessAnarchy）的帖子地址，v0.1.3 起指向本模组的帖子。
			set { OpenUrl("https://forum.paradoxplaza.com/forum/threads/tool-mode-memory.1942676/"); }
		}

		[SettingsUISection(kTabAbout, kGroupInfo)]
		[SettingsUIButton]
		[SettingsUIButtonGroup("aboutLinks")]
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

		// ---------- 范围下拉（只显示短名；五种范围的定义写在每一项自己的说明里） ----------

		// ---------- 范围下拉（只显示短名；五种范围的定义写在每一项自己的说明里） ----------
		public DropdownItem<int>[] GetThemesScopeItems() { return BuildScopeItems(ToolItemCatalog.kThemes); }
		public DropdownItem<int>[] GetPacksScopeItems() { return BuildScopeItems(ToolItemCatalog.kPacks); }
		public DropdownItem<int>[] GetToolModeScopeItems() { return BuildScopeItems(ToolItemCatalog.kToolMode); }
		public DropdownItem<int>[] GetElevationScopeItems() { return BuildScopeItems(ToolItemCatalog.kElevation); }
		public DropdownItem<int>[] GetParallelScopeItems() { return BuildScopeItems(ToolItemCatalog.kParallel); }
		public DropdownItem<int>[] GetSnapScopeItems() { return BuildScopeItems(ToolItemCatalog.kSnap); }
		public DropdownItem<int>[] GetTopographyScopeItems() { return BuildScopeItems(ToolItemCatalog.kTopography); }
		public DropdownItem<int>[] GetUndergroundScopeItems() { return BuildScopeItems(ToolItemCatalog.kUnderground); }
		public DropdownItem<int>[] GetOtherScopeItems() { return BuildScopeItems(ToolItemCatalog.kOther); }
		public DropdownItem<int>[] GetAnarchyScopeItems() { return BuildScopeItems(ToolItemCatalog.kAnarchy); }
		public DropdownItem<int>[] GetLeftRightScopeItems() { return BuildScopeItems(ToolItemCatalog.kLeftRight); }
		public DropdownItem<int>[] GetGeneralScopeItems() { return BuildScopeItems(ToolItemCatalog.kGeneral); }

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

		/// <summary>该项是否参与「恢复」。注意记录不受这个开关限制（需求 10）。</summary>
		public bool IsItemEnabled(string id)
		{
			if (!m_Enabled) return false;
			bool v;
			return m_ItemEnabled.TryGetValue(id, out v) && v;
		}

		/// <summary>总开关（记录的唯一闸门）。</summary>
		public bool MasterOn { get { return m_Enabled; } }

		public MemoryScope GetItemScope(string id)
		{
			ToolItemDef def = ToolItemCatalog.Find(id);
			int fallback = def != null ? def.EffectiveRecommendedScope() : (int)MemoryScope.Group;
			int v;
			if (!m_ItemScope.TryGetValue(id, out v)) return (MemoryScope)fallback;
			if (v < 0 || v > (int)MemoryScope.GlobalUnique) return (MemoryScope)fallback;
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
			Persist();
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
			Persist();
		}

		/// <summary>
		/// 落盘走基类 ApplyAndSave()：框架的 GetTargetSetting 是按
		/// fragment.source.GetType().Name 匹配的（AssetDatabase.cs:830），传注册名反而找不到目标。
		/// 类名必须全局唯一，所以本类叫 ToolModeMemorySettings 而不是 Setting。
		/// </summary>
		private void Persist()
		{
			if (!Ready) return;
			try { ApplyAndSave(); } catch { }
		}

		/// <summary>设置变更后立刻作用到运行中的工具；总开关为关时绝不写回。</summary>
		public void Sync()
		{
			if (!Ready) return;
			try
			{
				// 兼容开关的唯一落地位置：setter、重置按钮、读盘完成都经过这里
				ToolMemoryBridge.LiveHierarchy = m_CompatOtherMods;
				ToolModeMemoryMod.RefreshActive();
				if (!m_Enabled) return;
				Systems.ToolMemorySystem sys = null;
				if (Unity.Entities.World.DefaultGameObjectInjectionWorld != null)
				{
					sys = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<Systems.ToolMemorySystem>();
				}
				if (sys != null && sys.MasterEnabled) sys.RequestApply();
			}
			catch { }
		}

		/// <summary>需求 1：退回出厂推荐值（含总开关与兼容开关，两者默认都是开）。</summary>
		private void DoResetAllSettings()
		{
			try
			{
				SetDefaults();
				Sync();
				if (Ready)
				{
					try { ApplyAndSave(); } catch { }
				}
				ToolModeMemoryMod.log.Info("All settings restored to recommended defaults.");
			}
			catch (Exception ex)
			{
				ToolModeMemoryMod.log.Warn("ResetAllSettings failed: " + ex.GetType().Name);
			}
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
					// 只删文件不够：把当前面板先退回出厂值，否则下一帧捕获又把现值记回去
					Systems.ToolMemorySystem sys = Unity.Entities.World.DefaultGameObjectInjectionWorld != null
						? Unity.Entities.World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<Systems.ToolMemorySystem>()
						: null;
					if (sys != null) sys.ResetActiveTool();
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

		/// <summary>
		/// 出厂默认：总开关开、兼容开关开、每一项按 ToolItemCatalog 的推荐值。
		/// 「重置所有设置项」按钮也走这里（需求 1 + 本轮「默认打开」）。
		/// 这里不碰 ToolMemoryBridge：构造期（LoadSettings 之前）不该触发它的静态反射初始化，
		/// 由 Sync() 统一把开关推给它。
		/// </summary>
		public override void SetDefaults()
		{
			m_Enabled = true;
			m_CompatOtherMods = true;
			InitItemDefaults();
		}
	}
}
