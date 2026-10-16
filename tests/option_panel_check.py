#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
选项页结构与默认值的静态检查（离线，纯源码解析）。

对着源码形状断言的硬要求：
  1. 共用范围的五行定义不单列板块，而是并进每一个工具项的「共用范围」说明；
  2. 总开关出厂默认打开（兼容开关 v0.6.0 已取消，不允许复活）；
  3. 板块结构：总开关一块（不显示标题）/ 官方一块 / Anarchy 一块，行数 = 目录项数×2，
     行数与变灰条件条数一律从目录条数推，不写死常数；
  4. v0.6.2：Anarchy 三项「没装就整块隐藏」用 HideByCondition ——
     DisableByCondition 只在建页那一刻求值一次（Widget.UpdateBase），会把装了的人冻灰；
     hidden 由 OptionsUISystem.OnUpdate 每帧重算，只有它能跟随安装状态。
  5. v0.6.2：笔刷（brush）独立成行，官方板块因此多两行、目录多一项。

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

    # ---- 2. v0.2.2 板块结构：总开关单独一块且不显示标题；官方一块；每个模组一块 ----
    order = re.search(r"\[SettingsUIGroupOrder\(([^\)]*)\)\]", s)
    show = re.search(r"\[SettingsUIShowGroupName\(([^\)]*)\)\]", s)
    check(order is not None and "kGroupMaster" in order.group(1) and "kGroupOfficial" in order.group(1)
          and "kGroupAnarchy" in order.group(1),
          "GroupOrder 依次是 总开关 / 官方工具项 / Anarchy工具项 / 关于页各块")
    check(show is not None and "kGroupOfficial" in show.group(1) and "kGroupAnarchy" in show.group(1),
          "显示标题的板块包含官方与 Anarchy 两块")
    check(show is not None and "kGroupMaster" not in show.group(1),
          "总开关板块不显示标题（用户要求）")
    check(re.search(r"\[SettingsUISection\(kTabMod, kGroupMaster\)\]\s*public bool Enabled", s) is not None,
          "总开关自己在一块里")

    off_rows = len(re.findall(r"\[SettingsUISection\(kTabMod, kGroupOfficial\)\]", s))
    an_rows = len(re.findall(r"\[SettingsUISection\(kTabMod, kGroupAnarchy\)\]", s))
    items = re.findall(r"new ToolItemDef\((\w+),", cat)
    src_official = len(re.findall(r"ItemSource\.(Vanilla|Toolbar)", cat))
    src_anarchy = len(re.findall(r"ItemSource\.AnarchyMod", cat))
    check(off_rows == src_official * 2,
          "官方板块行数 = 官方项数×2（%d 行 / %d 项）" % (off_rows, src_official))
    check(an_rows == src_anarchy * 2,
          "Anarchy 板块行数 = Anarchy 项数×2（%d 行 / %d 项）" % (an_rows, src_anarchy))
    check(len(items) == 13, "目录 13 项（地区主题/数据包/工具模式/高度/并列/对齐/地形/地下/笔刷/其它 + Anarchy 三项）")
    # 行数断言一律从目录条数推，不手写常数：0.6.2 加「笔刷」时第三轮审查点名「设置页 12 项」
    # 这类硬编码会跟着漂，而漂了的数字比没有数字更坏（它看着像通过）。
    n_items = len(items)
    # 关闭总开关后，下面每一项都要变灰：每个 *Enabled 行都必须挂 IsMasterOff。
    # 按行扫（属性三行一组：Section / DisableByCondition / 属性声明）。
    slines = s.split("\n")
    enabled_rows = []
    for i, ln in enumerate(slines):
        m = re.search(r"public bool (\w+)Enabled", ln)
        if not m:
            continue
        window = "\n".join(slines[max(0, i - 6):i])
        if "kGroupOfficial" in window or "kGroupAnarchy" in window:
            # 属性行的顺序不作承诺（Anarchy 那六行在 Disable 之后还挂了 Hide），
            # 所以往上找最近的一条 SettingsUIDisableByCondition，而不是要求紧邻。
            cond = ""
            for wl in reversed(slines[max(0, i - 6):i]):
                dm = re.search(r"\[SettingsUIDisableByCondition\(typeof\(ToolModeMemorySettings\), "
                               r"nameof\((\w+)\)\)\]", wl)
                if dm:
                    cond = dm.group(1)
                    break
            enabled_rows.append((m.group(1), cond))
    check(len(enabled_rows) == n_items and all(g[1] == "IsMasterOff" for g in enabled_rows),
          "全部 %d 项的是否恢复开关都挂 IsMasterOff（总开关关闭即整块变灰）：%s"
          % (n_items, [(n, c) for n, c in enabled_rows if c != "IsMasterOff"] or "无遗漏"))
    scope_rows = re.findall(r"nameof\((Is\w+ScopeDisabled)\)\)\]", s)
    check(len(scope_rows) == n_items, "%d 个共用范围行都有各自的变灰条件" % n_items)
    helpers = re.findall(r"public bool (Is\w+ScopeDisabled)\(\) \{ return IsMasterOff\(\) \|\| !GetEnabled", s)
    check(len(helpers) == n_items, "%d 个变灰判定都是「总开关关 或 本项没开」" % n_items)

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
    # 官方板块的 20 行不得被这条判据牵连：非 Anarchy 行不许出现隐藏属性
    off_block = re.findall(r"\[SettingsUISection\(kTabMod, kGroupOfficial\)\](?:(?!\[SettingsUISection)[\s\S])*", s)
    check(not any("SettingsUIHideByCondition" in b for b in off_block),
          "官方板块没有任何行被隐藏（只有 Anarchy 三项会消失）")

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
