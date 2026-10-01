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

# v0.2.2 目录顺序 = 设置页顺序，跨板块连续编号（官方 1..9，Anarchy 10..12）。
# 第三个元素只是「拿不到词条时的占位名」，真正显示的名字取 item.<id>.label。
OFFICIAL = [
    (1, "themes", "地区主题"), (2, "packs", "数据包"), (3, "toolMode", "工具模式"),
    (4, "elevation", "高度"), (5, "parallel", "并列模式"), (6, "snap", "对齐"),
    (7, "topography", "地形"), (8, "underground", "地下模式"), (9, "other", "其它"),
]
ANARCHY = [
    (10, "anarchy", "Anarchy"), (11, "leftRight", "左侧和右侧"), (12, "general", "常规"),
]
ITEMS = OFFICIAL + ANARCHY


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

    # 「全局共用」这一行按项不同：游戏里只有一份设置的那几项（第 8 个构造参数 true）
    # 走 scope.line.sharedSingle，预览必须跟着 ScopeDescription() 的逻辑走，否则校对的就是假文本。
    cat = io.open(os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                               "ToolItemCatalog.cs"), encoding="utf-8").read()
    # 常量名 kTopography -> 项 id topography，和 rows 里用的 id 对得上
    single = set(re.sub(r"^k(.?)", lambda m: m.group(1).lower(), name)
                 for name in re.findall(r"new ToolItemDef\((\w+),[^\n]*, (?:s_\w+|null), true\)", cat))

    def block(title_key, rows):
        out.append("")
        out.append("【板块】" + d[title_key])
        for num, item_id, placeholder in rows:
            label = "%d. %s" % (num, d.get("item.%s.label" % item_id) or placeholder)
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
    block("group.official", OFFICIAL)
    block("group.anarchy", ANARCHY)
    out.append("")
    out.append("【关于页按钮】%s / %s / %s" % (d["about.kofi"], d["about.forum"], d["about.rainbow"]))
    print("\n".join(out))
    io.open(os.path.join(lc.ROOT, "research", "l10n_preview_%s.txt" % locale), "w",
            encoding="utf-8", newline="\n").write("\n".join(out) + "\n")
    total = sum(len(x) for x in out)
    print("[总字数 %d]" % total)


if __name__ == "__main__":
    main()
