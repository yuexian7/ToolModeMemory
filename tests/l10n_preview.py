#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
把设置页真正会显示的文本拼出来看（模拟 LocaleTable.AddItems / ScopeDescription）。
用法： python tests\\l10n_preview.py zh-HANS
"""
import io
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import l10n_check as lc  # noqa: E402

ITEMS = [
    (1, "anarchy", "Anarchy"), (2, "toolMode", "工具模式"), (3, "elevation", "高度"),
    (4, "parallel", "并列模式"), (5, "snap", "对齐"), (6, "topography", "地形"),
    (7, "elevationStep", "高度阶段"), (8, "leftRight", "左侧和右侧"), (9, "general", "常规"),
    (10, "underground", "地下模式"), (11, "other", "其它"),
]


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

    def fill(tpl, ex):
        if not tpl:
            return ""
        return tpl.replace("{0}", ex)

    out = []
    out.append("=========== %s ===========" % locale)
    out.append("[板块] " + d["group.items"])
    out.append("%s = %s" % (d["enabled.label"], d["enabled.desc"]))
    for num, item_id, _ in ITEMS:
        label = "%d. %s" % (num, d.get("item.%s.label" % item_id, item_id))
        out.append("")
        out.append("── %s" % label)
        out.append("   说明: " + d["item.%s.desc" % item_id])
        scope = [d["scope.desc"],
                 fill(d["scope.line.group"], d["item.%s.ex.group" % item_id]),
                 fill(d["scope.line.menu"], d["item.%s.ex.menu" % item_id]),
                 fill(d["scope.line.category"], d["item.%s.ex.category" % item_id]),
                 d["scope.line.shared"],
                 d["scope.line.unique"],
                 d["scope.note"]]
        out.append("   共用范围（%s）:" % d["scope.label"])
        for i, line in enumerate(scope):
            out.append("     %d) %s" % (i + 1, line))
    print("\n".join(out))
    io.open(os.path.join(lc.ROOT, "research", "l10n_preview_%s.txt" % locale), "w",
            encoding="utf-8", newline="\n").write("\n".join(out) + "\n")
    total = sum(len(x) for x in out)
    print("[总字数 %d]" % total)


if __name__ == "__main__":
    main()
