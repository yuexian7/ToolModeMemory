#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
选项页结构与默认值的静态检查（离线，纯源码解析）。

对着源码形状断言的硬要求（v0.7.0）：
  1. 五个标签页：总开关 / 资产 / 功能 / 道具 / 关于（owner 需求 3）；
  2. 总开关叫「启用模组」，管住**所有**共用设置：大开关关闭时三页里的每一项都变灰；
  3. 总开关页有三块分域开关（启用资产/功能/道具的工具项记忆），
     大开关的自动反映规则：三块全关 → 大开关关；任一打开 → 大开关开；
  4. 每一页：官方工具项设置在前，第三方模组（目前只有 Anarchy）在后，一块一模组；
  5. 行数 = 目录项数×2（每项一个开关 + 一个共用范围），行数与变灰条件条数
     一律从目录条数推，不写死常数；
  6. Anarchy 三项「没装就整块隐藏」用 HideByCondition —— DisableByCondition 只在建页
     那一刻求值一次（Widget.UpdateBase），会把装了的人冻灰；hidden 由
     OptionsUISystem.OnUpdate 每帧重算，只有它能跟随安装状态；
  7. 属性声明顺序 == 目录顺序（设置页行序 = 属性声明顺序），且每页内编号从 1 连续。

运行： python tests\\option_panel_check.py
"""
import io
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SETTINGS = os.path.join(ROOT, "ToolModeMemorySettings.cs")
LOCALES = os.path.join(ROOT, "LocaleTable.cs")
CATALOG = os.path.join(ROOT, "ToolItemCatalog.cs")

fails = []


def check(cond, msg):
    if cond:
        print("OK   " + msg)
    else:
        print("FAIL " + msg)
        fails.append(msg)


def body_of(text, signature):
    """从 signature 之后取配平花括号的函数体。"""
    i = text.find(signature)
    if i < 0:
        return None
    j = text.find("{", i)
    if j < 0:
        return None
    depth = 0
    for k in range(j, len(text)):
        if text[k] == "{":
            depth += 1
        elif text[k] == "}":
            depth -= 1
            if depth == 0:
                return text[j:k + 1]
    return None


def main():
    s = io.open(SETTINGS, encoding="utf-8-sig").read()
    loc = io.open(LOCALES, encoding="utf-8-sig").read()
    cat = io.open(CATALOG, encoding="utf-8-sig").read()

    # ---- 1. 旧的单列说明板块必须彻底没有 ----
    for dead in ["ScopeDefinitions", "kGroupScopeInfo", "kGroupMain"]:
        check(dead not in s, "设置类里没有残留「%s」" % dead)
        check(dead not in loc, "语言表里没有残留「%s」" % dead)
    # [SettingsUIMultilineText] 作为属性真的挂上去 = 又单开了一行说明板块；
    # 注释里提到它是历史教训（MultilineText 只渲染 label，不渲染值），不算违反。
    check(re.search(r"(?m)^\s*\[SettingsUIMultilineText\]\s*$", s) is None,
          "没有任何属性真的挂上 [SettingsUIMultilineText]")

    # ---- 2. v0.7.0 板块结构：五个标签页、八个板块 ----
    tabs = re.search(r"\[SettingsUITabOrder\(([^\)]*)\)\]", s)
    order = re.search(r"\[SettingsUIGroupOrder\(([^\)]*)\)\]", s)
    show = re.search(r"\[SettingsUIShowGroupName\(([^\)]*)\)\]", s)
    check(tabs is not None and [t.strip() for t in tabs.group(1).split(",")] ==
          ["kTabMaster", "kTabAssets", "kTabFunctions", "kTabItems", "kTabAbout"],
          "标签页依次是 总开关 / 资产 / 功能 / 道具 / 关于（需求 3）")
    check(order is not None and "kGroupDomain" in order.group(1),
          "GroupOrder 里有分域开关板块（总开关页三块「启用 X 的工具项记忆」）")
    check(show is not None and "kGroupDomain" in show.group(1),
          "分域开关板块显示标题")
    check(show is not None and "kGroupMaster" not in show.group(1),
          "大开关板块不显示标题（用户要求）")
    check(re.search(r"\[SettingsUISection\(kTabMaster, kGroupMaster\)\]\s*public bool Enabled", s)
          is not None, "「启用模组」大开关在总开关页第一块里")

    # 每个标签页的官方 / 第三方板块都必须真的挂在该页上（三页各自一套，不许串页）
    for tab, grp in (("kTabAssets", "kGroupAssetOfficial"), ("kTabAssets", "kGroupAssetAnarchy"),
                     ("kTabFunctions", "kGroupFunctionOfficial"), ("kTabItems", "kGroupItemOfficial")):
        n = len(re.findall(r"\[SettingsUISection\(%s, %s\)\]" % (tab, grp), s))
        check(n > 0, "%s 页有 %s 板块（%d 行）" % (tab, grp, n))

    items = re.findall(r"new ToolItemDef\((\w+),", cat)
    n_items = len(items)
    # 数「目录里定义的项」必须只数构造调用，不能数到 OfficialCount/AnarchyCount 里
    # 那些 `ItemSource.AnarchyMod` 比较表达式（v0.7.0 加了这两个辅助方法，
    # 老写法 `re.findall(r"ItemSource\.(Vanilla|Toolbar)", cat)` 会把它们算成项）。
    id_consts = dict(re.findall(r'const string (\w+)\s*=\s*"([^"]*)"', cat))
    cat_items = [(id_consts.get(m, m), int(num), src) for m, num, src in
                 re.findall(r"new ToolItemDef\((\w+),\s*(\d+),\s*ItemSource\.(\w+)", cat)]
    src_official = len([1 for _i, _n, s in cat_items if s in ("Vanilla", "Toolbar")])
    src_anarchy = len([1 for _i, _n, s in cat_items if s == "AnarchyMod"])
    check(n_items == src_official + src_anarchy,
          "目录 %d 项 = 官方 %d + 第三方 %d" % (n_items, src_official, src_anarchy))
    # 三页行数之和 = 项数×2（每项一个开关 + 一个共用范围），全部从目录推
    per_tab = {}
    for grp in ("kGroupAssetOfficial", "kGroupAssetAnarchy", "kGroupFunctionOfficial",
                "kGroupItemOfficial"):
        per_tab[grp] = len(re.findall(r"\[SettingsUISection\(\w+, %s\)\]" % grp, s))
    check(sum(per_tab.values()) == n_items * 2,
          "三页板块行数合计 = 项数×2（%d 行 / %d 项）" % (sum(per_tab.values()), n_items))
    check(per_tab["kGroupAssetAnarchy"] == src_anarchy * 2,
          "Anarchy 板块行数 = Anarchy 项数×2（%d 行 / %d 项）"
          % (per_tab["kGroupAssetAnarchy"], src_anarchy))
    # 关闭大开关后，三页里的每一项都要变灰：每个 *Enabled 行都必须挂本页的变灰条件。
    # 按行扫，**往前找最近的一条 [SettingsUISection( 前缀定义专属**，
    # 因为页与页交界处前 6 行的窗口会带进上一页的板块名（v0.7.0 拆页后必须这样才不串页）。
    slines = s.split("\n")
    page_groups = ("kGroupAssetOfficial", "kGroupAssetAnarchy",
                   "kGroupFunctionOfficial", "kGroupItemOfficial")
    cur_section = None
    enabled_rows = []
    for i, ln in enumerate(slines):
        sm = re.search(r"\[SettingsUISection\((\w+), (\w+)\)\]", ln)
        if sm:
            cur_section = (sm.group(1), sm.group(2))
            continue
        em = re.search(r"public bool (\w+)Enabled\s*\{", ln)
        if not em or cur_section is None:
            continue
        tab, grp = cur_section
        if grp not in page_groups:
            continue
        cond = ""
        for wl in reversed(slines[max(0, i - 4):i]):
            dm = re.search(r"\[SettingsUIDisableByCondition\(typeof\(ToolModeMemorySettings\), "
                           r"nameof\((\w+)\)\)\]", wl)
            if dm:
                cond = dm.group(1)
                break
        enabled_rows.append((em.group(1), cond, grp))
    check(len(enabled_rows) == n_items, "全部 %d 项的是否恢复开关都已变成属性行" % n_items)
    # 变灰条件：大开关 或 该页域开关。**一个条件就够**，因为三个 IsXxxSectionOff()
    # 本身已经写成 `IsMasterOff() || !GetDomain(...)`（见下面 2a 的形状断言），
    # 再要求每条都挂 IsMasterOff 是重复求值，写两条只会让修的时候改漏一处。
    sec_off = ("IsAssetSectionOff", "IsFunctionSectionOff", "IsItemSectionOff")
    check(all(g[1] in sec_off for g in enabled_rows),
          "全部 %d 项的是否恢复开关都挂 IsAssetSectionOff / IsFunctionSectionOff / IsItemSectionOff"
          "（大开关或域开关关闭即整页变灰）：%s"
          % (n_items, [(n, c) for n, c, g in enabled_rows if c not in sec_off] or "无遗漏"))

    # ---- 2a. 三个 IsXxxSectionOff 必须真的把大开关与域开关都算进去 ----
    # 需求 3：大开关管住所有共用设置；域开关管住这一页所有行。少任何一半，
    # 表现就是「关了大开关、资产页还能恢复」这种半死不活的状态。
    sec = body_of(s, "public bool IsAssetSectionOff()")
    check(sec is not None and "IsMasterOff()" in sec and "GetDomain(ItemDomain.Asset)" in sec,
          "IsAssetSectionOff = 大开关关 或 资产域开关关")
    for name, dom in (("IsFunctionSectionOff", "ItemDomain.Function"),
                      ("IsItemSectionOff", "ItemDomain.Item")):
        b = body_of(s, "public bool %s()" % name)
        check(b is not None and "IsMasterOff()" in b and "GetDomain(%s)" % dom in b,
              "%s = 大开关关 或 %s 域开关关" % (name, dom))
    # 大开关的自动反映：Ready 之下，SetDomain 调 ReflectMaster
    check(re.search(r"SetDomain\(ItemDomain\.\w+, value\)", s) is not None,
          "域开关的 setter 走 SetDomain()（自动反映只允许一处实现）")
    check(re.search(r"if \(Ready\)\s*\{?\s*m_Enabled = ReflectMaster\(\)", s) is not None,
          "域开关的 setter 会同步大开关（Ready 之后自动反映，需求 3）")
    scope_rows = re.findall(r"nameof\((Is\w+ScopeDisabled)\)\)\]", s)
    check(len(scope_rows) == n_items, "%d 个共用范围行都有各自的变灰条件" % n_items)
    helpers = re.findall(r"public bool (Is\w+ScopeDisabled)\(\) \{ return Is(?:Asset|Function|Item)"
                         r"SectionOff\(\) \|\| !GetEnabled", s)
    check(len(helpers) == n_items,
          "%d 个变灰判定都是「本域/本页没开 或 本项没开」" % n_items)
    # 需求 3：域开关关掉时，该页所有行也要变灰 —— IsItemEnabled 必须查 GetDomain
    check(re.search(r"GetDomain\(def\.Domain\)", s) is not None,
          "IsItemEnabled 里查了 GetDomain（域开关关闭时该页整页变灰）")
    check(re.search(r"bool IsDomainEnabled\(ItemDomain", s) is not None,
          "IsDomainEnabled(ItemDomain) 判定方法存在（记录与恢复共用同一判据）")

    # ---- 2b. Anarchy 三项：没装该**整块隐藏**，不是变灰 ----
    # 为什么不能用 DisableByCondition（0.6.2 反编译 AutomaticSettings/Widget 得到的结论）：
    # disabled 只在 RegisterSetting→BuildTab 建页那一瞬间求值一次（Widget.UpdateBase），
    # 而 Anarchy 常常在选项页建好之后才进 AppDomain，灰条会永远停在「没装」的旧结论上；
    # hidden 由 OptionsUISystem.OnUpdate→UpdateVisibility 每帧重算（只在停在选项页时），
    # 所以只有隐藏能跟随安装状态。
    hide = re.findall(r"\[SettingsUIHideByCondition\(typeof\(ToolModeMemorySettings\), "
                      r"nameof\((\w+)\)\)\]", s)
    check(len(hide) == src_anarchy * 2 and set(hide) == {"IsAnarchyMissing"},
          "Anarchy 板块 %d 行全部挂 IsAnarchyMissing（没装就整块隐藏）：%s" % (src_anarchy * 2, hide))
    check(re.search(r"public bool IsAnarchyMissing\(\)", s) is not None,
          "IsAnarchyMissing() 判定方法存在")
    check("AnarchyConfirmedAbsent()" in s,
          "隐藏判据复用 ToolMemoryBridge.AnarchyConfirmedAbsent()（判据只允许一处）")
    check(not re.search(r"DisableByCondition[^=\n]*IsAnarchyMissing", s),
          "没装 Anarchy 不走变灰（DisableByCondition 建页时冻结，装了也会一直灰着）")
    # 官方板块的行不得被这条判据牵连：非 Anarchy 行不许出现隐藏属性
    for grp in ("kGroupAssetOfficial", "kGroupFunctionOfficial", "kGroupItemOfficial"):
        blocks = re.findall(r"\[SettingsUISection\(\w+, %s\)\](?:(?!\[SettingsUISection)[\s\S])*" % grp, s)
        check(not any("SettingsUIHideByCondition" in b for b in blocks),
              "%s 板块没有任何行被隐藏（只有 Anarchy 项会消失）" % grp)

    # ---- 2c. 属性声明顺序与目录顺序锁死（每页各一段）----
    # 设置页行序 = 属性声明顺序（反编译 AutomaticSettings.BuildTab：按声明顺序逐个建 widget），
    # 「哪些项 / 什么编号 / 进哪个板块」由 ToolItemCatalog 定义。两边一旦不同步，玩家看到的是
    # 「第 9 项印着别的行」这类错位，编译和其余检查都发现不了。
    id_consts = dict(re.findall(r'const string (\w+)\s*=\s*"([^"]*)"', cat))
    cat_items = [(id_consts.get(m, m), int(num), src) for m, num, src in
                 re.findall(r"new ToolItemDef\((\w+),\s*(\d+),\s*ItemSource\.(\w+)", cat)]
    cat_official_all = [i for i, num, src in cat_items if src in ("Vanilla", "Toolbar")]
    anarchy_ids = [i for i, num, src in cat_items if src == "AnarchyMod"]

    def pascal_to_id(name):
        """属性名 → 目录 id。v0.7.0 的 FToolMode / IBrush 要还原成 f.toolMode / i.brush。"""
        if len(name) > 1 and name[0] in "FI" and name[1].isupper():
            return name[0].lower() + "." + name[1].lower() + name[2:]
        return name[0].lower() + name[1:]

    page_asset_official = [pascal_to_id(n) for n, c, g in enabled_rows if g == "kGroupAssetOfficial"]
    page_asset_anarchy = [pascal_to_id(n) for n, c, g in enabled_rows if g == "kGroupAssetAnarchy"]
    page_function = [pascal_to_id(n) for n, c, g in enabled_rows if g == "kGroupFunctionOfficial"]
    page_item = [pascal_to_id(n) for n, c, g in enabled_rows if g == "kGroupItemOfficial"]
    # 目录里各项归属哪个板块，不靠「顺序猜」：资产页官方 = 不带 f. / i. 前缀的官方项
    # （11 项），功能页 / 道具页各 5 项。f. / i. 前缀是 v0.7.0 的域约定。
    official_ids = [i for i in cat_official_all if not i.startswith(("f.", "i."))]
    func_ids = [i for i in cat_official_all if i.startswith("f.")]
    item_ids = [i for i in cat_official_all if i.startswith("i.")]
    check(page_asset_official == official_ids,
          "资产页官方板块属性顺序 == 目录顺序（%s）" % "->".join(official_ids))
    check(page_asset_anarchy == anarchy_ids,
          "资产页 Anarchy 板块属性顺序 == 目录顺序（%s）" % "->".join(anarchy_ids))
    check(page_function == func_ids,
          "功能页属性顺序 == 目录里的 f.* 顺序（%s）" % "->".join(func_ids))
    check(page_item == item_ids,
          "道具页属性顺序 == 目录里的 i.* 顺序（%s）" % "->".join(item_ids))
    check("other" not in official_ids,
          "「其它」已从目录里彻底删除（需求 4：去掉其它、列出全部）")

    # ---- 3. 出厂默认：总开关打开，兼容开关（v0.6.0 已取消）不允许再出现 ----
    defaults = body_of(s, "public override void SetDefaults()")
    check(defaults is not None, "SetDefaults() 存在")
    if defaults:
        check("m_Enabled = true;" in defaults, "总开关出厂默认 = 开")
        check("m_CompatOtherMods" not in defaults, "SetDefaults() 里已经没有兼容开关（该设置项已取消）")
        check("InitItemDefaults();" in defaults, "每一项的出厂值走目录里的推荐值")

    ctor = body_of(s, "public ToolModeMemorySettings(IMod mod) : base(mod)")
    check(ctor is not None and "SetDefaults();" in ctor,
          "构造函数调用 SetDefaults()（框架不会替模组设置调用它）")

    field = re.search(r"private bool m_Enabled\s*=\s*(\w+);", s)
    check(field is not None and field.group(1) == "true", "m_Enabled 字段初值也是 true")
    check("CompatOtherMods" not in s and "m_CompatOtherMods" not in s,
          "设置页里没有 CompatOtherMods 这一项（属性 / 字段 / Section 特性都已删干净）")
    bridge = io.open(os.path.join(ROOT, "ToolMemoryBridge.cs"), encoding="utf-8-sig").read()
    check("s_LiveHierarchy" not in bridge and "LiveHierarchy" not in bridge,
          "ToolMemoryBridge 不再有实时层级开关：层级恒为实时（其它模组调整完的那份）")
    check("s_FrozenPos" not in re.sub(r"^\s*///.*$", "", bridge, flags=re.M),
          "ToolMemoryBridge 不再冻结第一次解析到的层级（代码里没有 s_FrozenPos，只剩注释提到它）")
    check("m_Setting.AfterLoaded();" in io.open(os.path.join(ROOT, "ToolModeMemoryMod.cs"),
                                                encoding="utf-8-sig").read(),
          "读盘完成后调用 AfterLoaded()，把文件里的设置补推一次")

    # ---- 4. 「重置所有设置项」= SetDefaults，不再自己抄一份 ----
    reset = body_of(s, "private void DoResetAllSettings()")
    check(reset is not None and "SetDefaults();" in reset,
          "重置所有设置项直接调用 SetDefaults()（默认值只有一处定义）")

    # ---- 5. 共用范围说明改成「每行模板 + 每项例子」 ----
    for key in ['"scope.line.group"', '"scope.line.menu"', '"scope.line.category"',
                '"scope.line.shared"', '"scope.line.unique"', '"scope.note"']:
        check(key in loc or key.replace('"scope.', 'kScope') in loc or
              re.search(r"const string\s+\w+\s*=\s*" + re.escape(key[1:-1]) + ";", loc) is not None,
              "语言表里有模板键 %s" % key)
    check("ScopeDescription(def, d)" in loc,
          "每一项的 scope 行说明由 ScopeDescription() 按项拼装")
    check('GetOptionDescLocaleID(pascal + "Scope")' in loc,
          "scope 行说明按项注册（不再 11 项共用同一句）")

    print()
    print("ALL PASS" if not fails else "FAILURES=%d" % len(fails))
    return 0 if not fails else 1


if __name__ == "__main__":
    sys.exit(main())
