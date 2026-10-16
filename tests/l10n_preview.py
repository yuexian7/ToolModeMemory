#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
把设置页真正会显示的文本拼出来看（模拟 LocaleTable.AddItems / ScopeDescription）。
用法： python tests\\l10n_preview.py zh-HANS
"""
import io
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import l10n_check as lc  # noqa: E402

# 目录顺序 = 设置页顺序，编号跨板块连续。这里**解析 ToolItemCatalog.cs**，不手抄：
# 手抄一份就会和源码漂（v0.6.2 加「笔刷」时它就漏了，预览里 Anarchy 还显示成第 10 项）。
CATALOG = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                       "ToolItemCatalog.cs")


def load_items():
    """[(number, item_id, source)] —— source ∈ {Toolbar, Vanilla, AnarchyMod}。"""
    cat = io.open(CATALOG, encoding="utf-8-sig").read()
    consts = dict(re.findall(r'const string\s+(\w+)\s*=\s*"([^"]*)"', cat))
    out = []
    for m in re.finditer(r"new ToolItemDef\((\w+),\s*(\d+),\s*ItemSource\.(\w+)", cat):
        out.append((int(m.group(2)), consts.get(m.group(1), m.group(1)), m.group(3)))
    return sorted(out)


def main():
    locale = sys.argv[1] if len(sys.argv) > 1 else "zh-HANS"
    text = lc.load()
    consts = {}
    for lines in text.values():
        for line in lines:
            m = lc.CONST.search(line)
            if m:
                consts[m.group("name")] = m.group("val")
    tables = lc.parse(text, consts)
    d = tables[locale]
    # Frames() 里的键 12 份共用，lc.parse 不收（它只解析语言函数），这里单独抽出来当兜底，
    # 否则 anarchy 这一项在预览里显示成裸 id。只取 Frames() 那一段，不混进任何语言函数。
    frames = {}
    for lines in text.values():
        body = None
        for line in lines:
            if re.match(r"\s*private static Dictionary<string, string> Frames\(", line):
                body = []
                continue
            if body is None:
                continue
            m = re.match(r'^\s*d\["([^"]+)"\]\s*=\s*"([^"]*)"\s*;\s*$', line)
            if m:
                frames[m.group(1)] = m.group(2)
            elif line.strip() == "}":
                body = None
    d = dict(frames, **d)

    def fill(tpl, ex):
        if not tpl:
            return ""
        return tpl.replace("{0}", ex)

    items = load_items()
    if not items:
        print("FAIL 没从 ToolItemCatalog.cs 解析到任何项")
        return 1
    nums = [n for n, _i, _s in items]
    if nums != list(range(1, len(items) + 1)):
        print("FAIL 编号不是 1..%d 连续：%s" % (len(items), nums))
        return 1
    official = [(n, i) for n, i, s in items if s in ("Vanilla", "Toolbar")]
    anarchy = [(n, i) for n, i, s in items if s == "AnarchyMod"]
    if len(official) + len(anarchy) != len(items):
        print("FAIL 有项的 ItemSource 不在 Vanilla/Toolbar/AnarchyMod 里：%s"
              % sorted({s for _n, _i, s in items}))
        return 1

    # 「全局共用」这一行按项不同：游戏里只有一份设置的那几项（第 8 个构造参数 true）
    # 走 scope.line.sharedSingle，预览必须跟着 ScopeDescription() 的逻辑走，否则校对的就是假文本。
    cat = io.open(CATALOG, encoding="utf-8-sig").read()
    # 常量名 kTopography -> 项 id topography，和 rows 里用的 id 对得上
    single = set(re.sub(r"^k(.?)", lambda m: m.group(1).lower(), name)
                 for name in re.findall(r"new ToolItemDef\((\w+),[^\n]*, (?:s_\w+|null), true\)", cat))

    def block(title_key, rows):
        out.append("")
        out.append("【板块】" + d[title_key])
        for num, item_id in rows:
            label = "%d. %s" % (num, d.get("item.%s.label" % item_id) or item_id)
            out.append("")
            out.append("── %s" % label)
            out.append("   说明: " + d["item.%s.desc" % item_id])
            shared_key = "scope.line.sharedSingle" if item_id in single else "scope.line.shared"
            scope = [d["scope.desc"],
                     fill(d["scope.line.group"], d["item.%s.ex.group" % item_id]),
                     fill(d["scope.line.menu"], d["item.%s.ex.menu" % item_id]),
                     fill(d["scope.line.category"], d["item.%s.ex.category" % item_id]),
                     d[shared_key],
                     d["scope.line.unique"],
                     d["scope.note"]]
            out.append("   共用范围（%s）:" % d["scope.label"])
            for i, line in enumerate(scope):
                out.append("     %d) %s" % (i + 1, line))

    out = []
    out.append("=========== %s ===========" % locale)
    # 总开关单独一块（设置页里这块不显示标题，这里仍把标题打出来便于校对）
    out.append("【板块】" + d["group.master"] + "（设置页不显示标题）")
    out.append("%s = %s" % (d["enabled.label"], d["enabled.desc"]))
    block("group.official", official)
    block("group.anarchy", anarchy)
    out.append("")
    out.append("【关于页按钮】%s / %s / %s" % (d["about.kofi"], d["about.forum"], d["about.rainbow"]))
    print("\n".join(out))
    io.open(os.path.join(lc.ROOT, "research", "l10n_preview_%s.txt" % locale), "w",
            encoding="utf-8", newline="\n").write("\n".join(out) + "\n")
    total = sum(len(x) for x in out)
    print("[总字数 %d]" % total)


if __name__ == "__main__":
    main()
