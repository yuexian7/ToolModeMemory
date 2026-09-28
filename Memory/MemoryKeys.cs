using System;

namespace ToolModeMemory.Memory
{
	/// <summary>
	/// 记忆键的纯拼装规则（不依赖游戏类型，可离线测试）。
	///
	/// 层级来自反编译 Game.dll 的事实：
	///   资产 prefab --UIObjectData.m_Group--> 分类 --UIAssetCategoryData.m_Menu--> 菜单(UIAssetMenuData)
	///   UIObjectData 只有 m_Group / m_Priority 两个字段，不存在第三层「组」。
	///
	/// 键一律使用名称/稳定 ID，不使用 Entity.Index
	/// （索引会随 DLC、创意工坊资产的加载顺序变化而漂移，导致旧键读不到、串到别的资产上）。
	/// </summary>
	public static class MemoryKeys
	{
		public const string PREFIX_GROUP = "G:";
		public const string PREFIX_MENU = "M:";
		public const string PREFIX_CATEGORY = "C:";
		public const string PREFIX_ASSET = "P:";
		public const string SHARED = "S";
		public const string PREFIX_TOOL = "T:";

		public static string Shared()
		{
			return SHARED;
		}

		public static string Menu(string menuName)
		{
			return menuName == null ? null : PREFIX_MENU + menuName;
		}

		public static string Category(string categoryName)
		{
			return categoryName == null ? null : PREFIX_CATEGORY + categoryName;
		}

		/// <summary>
		/// 同组 = 菜单限定的分类（= 原版 NetToolSystem 的粒度）。
		/// 菜单未知时退化为裸分类名，绝不拼出 "/null"。
		/// </summary>
		public static string Group(string menuName, string categoryName)
		{
			if (categoryName == null) return null;
			return menuName == null ? PREFIX_GROUP + categoryName : PREFIX_GROUP + menuName + "/" + categoryName;
		}

		/// <summary>单资产：PrefabID 文本（Type:Name，跨启动稳定）。</summary>
		public static string Asset(string prefabIdentity)
		{
			return prefabIdentity == null ? null : PREFIX_ASSET + prefabIdentity;
		}

		/// <summary>该层级解析不出来时的兜底：按工具隔离。</summary>
		public static string Tool(string toolID)
		{
			return PREFIX_TOOL + (string.IsNullOrEmpty(toolID) ? "null" : toolID);
		}

		/// <summary>
		/// 资产 / 功能分隔前缀。二者的可选项集合完全不同（功能区只有填充/滚动/刷涂，
		/// 道路有直线/曲线/网格/替换…），所以任何范围下都不允许跨这一界共用，
		/// 「全局共用」的定义也因此是「所有支持该项的**资产**共用一份、**功能**共用另一份」。
		/// </summary>
		public const string DOMAIN_ASSET = "A|";
		public const string DOMAIN_FUNCTION = "F|";

		/// <summary>给任何键加资产/功能域；null 键原样返回。</summary>
		public static string WithDomain(string key, bool isFunction)
		{
			if (string.IsNullOrEmpty(key)) return key;
			return (isFunction ? DOMAIN_FUNCTION : DOMAIN_ASSET) + key;
		}

		/// <summary>从完整键里剥掉域前缀，得到层级部分（日志/测试/旧数据判用）。</summary>
		public static string StripDomain(string key)
		{
			if (key == null) return null;
			if (key.StartsWith(DOMAIN_ASSET, StringComparison.Ordinal)) return key.Substring(DOMAIN_ASSET.Length);
			if (key.StartsWith(DOMAIN_FUNCTION, StringComparison.Ordinal)) return key.Substring(DOMAIN_FUNCTION.Length);
			return key;
		}

		/// <summary>把范围翻成键前缀，供日志与测试用。</summary>
		public static string PrefixFor(MemoryScope scope)
		{
			switch (scope)
			{
				case MemoryScope.Group: return PREFIX_GROUP;
				case MemoryScope.Menu: return PREFIX_MENU;
				case MemoryScope.Category: return PREFIX_CATEGORY;
				case MemoryScope.GlobalUnique: return PREFIX_ASSET;
				default: return SHARED;
			}
		}
	}
}
