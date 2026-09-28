#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
选项页结构与默认值的静态检查（离线，纯源码解析）。

v0.2.1 的两条硬要求：
  1. 共用范围的五行定义不再单列板块，而是并进每一个工具项的「共用范围」说明；
  2. 总开关与兼容开关出厂默认都是打开。
这两条光看运行结果很费时间，所以直接对着源码形状断言。

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

    # ---- 2. 模组设置页只有一个板块，总开关也在里面 ----
    order = re.search(r"\[SettingsUIGroupOrder\(([^\)]*)\)\]", s)
    show = re.search(r"\[SettingsUIShowGroupName\(([^\)]*)\)\]", s)
    check(order is not None and "kGroupItems" in order.group(1),
          "GroupOrder 里只剩一个设置页板块 kGroupItems")
    check(show is not None and "kGroupItems" in show.group(1),
          "ShowGroupName 里有 kGroupItems")
    enabled_sec = re.search(r"\[SettingsUISection\(kTabMod, kGroupItems\)\]\s*(?:\[^\]]*\]\s*)*public bool Enabled", s)
    check(enabled_sec is not None, "总开关 Enabled 就在「工具记忆模式设置」板块里")

    rows = len(re.findall(r"\[SettingsUISection\(kTabMod, kGroupItems\)\]", s))
    items = len(re.findall(r"new ToolItemDef\(", cat))
    check(rows == items * 2 + 1,
          "设置页行数 = 总开关 1 + 每项 2（共 %d 行，目录 %d 项）" % (rows, items))

    # ---- 3. 出厂默认：两个开关都是打开 ----
    defaults = body_of(s, "public override void SetDefaults()")
    check(defaults is not None, "SetDefaults() 存在")
    if defaults:
        check("m_Enabled = true;" in defaults, "总开关出厂默认 = 开")
        check("m_CompatOtherMods = true;" in defaults, "兼容开关出厂默认 = 开")
        check("InitItemDefaults();" in defaults, "每一项的出厂值走目录里的推荐值")

    ctor = body_of(s, "public ToolModeMemorySettings(IMod mod) : base(mod)")
    check(ctor is not None and "SetDefaults();" in ctor,
          "构造函数调用 SetDefaults()（框架不会替模组设置调用它）")

    field = re.search(r"private bool m_Enabled\s*=\s*(\w+);", s)
    check(field is not None and field.group(1) == "true", "m_Enabled 字段初值也是 true")
    field2 = re.search(r"private bool m_CompatOtherMods\s*=\s*(\w+);", s)
    check(field2 is not None and field2.group(1) == "true", "m_CompatOtherMods 字段初值也是 true")
    bridge = io.open(os.path.join(ROOT, "ToolMemoryBridge.cs"), encoding="utf-8-sig").read()
    check("s_LiveHierarchy = true" in bridge, "ToolMemoryBridge 的实时层级默认 true")
    check("m_Setting.AfterLoaded();" in io.open(os.path.join(ROOT, "ToolModeMemoryMod.cs"),
                                                encoding="utf-8-sig").read(),
          "读盘完成后调用 AfterLoaded()，把文件里的兼容开关推给 Bridge")

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
