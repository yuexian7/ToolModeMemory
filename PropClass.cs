using System;

namespace ToolModeMemory
{
	/// <summary>
	/// 道具域「同类」的纯判定。owner 2026-10-19 定案：道具 = 城市美化菜单栏下的 8 类道具。
	///
	/// 8 类即工具栏里 Landscaping 菜单下的 8 个道具分类（UIAssetCategoryPrefab 名，
	/// 与 SubServices.NAME 的 key 一一对应）：
	///   PropsNature        自然
	///   PropsPark          公园
	///   PropsResidential   住宅道具
	///   PropsCommercial    商业道具
	///   PropsIndustrial    工业道具
	///   PropsDebris        残骸
	///   PropsDecals        贴花
	///   PropsPlantpots     盆栽
	/// 认不出（不在 Landscaping 菜单下的道具）返回 null，调用方退化成按单个道具记忆。
	/// 第三方道具若能识别出分类名仍照常归组；归不进 8 类的就退化，
	/// 绝不把残骸和盆栽偷并成一群（道具分类在游戏里就是 8 档，多一档少一档都不是原版语义）。
	/// </summary>
	public static class PropClass
	{
		/// <summary>记忆键里道具分类段的固定前缀（与资产域 "K:" / 功能域 "F:" 互不撞）。</summary>
		public const string KEY_PREFIX = "I:";

		private const string NAME_MENU_LANDSCAPING = "Landscaping";

		/// <summary>8 类道具的官方英文分类名（camelCase PrefabID 部分），与游戏语言无关。</summary>
		private static readonly string[] CATEGORY_NAMES =
		{
			"PropsNature", "PropsPark", "PropsResidential", "PropsCommercial",
			"PropsIndustrial", "PropsDebris", "PropsDecals", "PropsPlantpots"
		};

		/// <summary>分类名 → 令牌（8 类各一个）；认不出返回 null。</summary>
		public static string CategoryToken(string menuName, string categoryName)
		{
			if (!IsPropMenu(menuName)) return null;
			return CategoryToken(categoryName);
		}

		/// <summary>是否在道具菜单（城市美化）下。第三方菜单一律不算。</summary>
		public static bool IsPropMenu(string menuName)
		{
			return menuName != null && menuName == NAME_MENU_LANDSCAPING;
		}

		/// <summary>分类名 → 令牌；认不出（含第三方新加的道具分类）返回 null。</summary>
		public static string CategoryToken(string categoryName)
		{
			if (categoryName == null) return null;
			for (int i = 0; i < CATEGORY_NAMES.Length; i++)
			{
				if (categoryName == CATEGORY_NAMES[i]) return KEY_PREFIX + categoryName;
			}
			return null;
		}
	}
}
