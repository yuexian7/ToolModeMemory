#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
把设置页真正会显示的文本拼出来看（模拟 LocaleTable.AddItems / ScopeDescription）。
v0.7.0：五个标签页，每页内编号从 1 数，「同类」这一行按页取键
（资产页 scope.line.category / 功能页 +.function / 道具页 +.item），
「全局共用」按页取 sharedSingle（v0.7.0 起这是「本页共用一份」）。

用法： python tests\\l10n_preview.py zh-HANS
"""
import io
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import l10n_check as lc  # noqa: E402

# 目录顺序 = 设置页顺序，编号每页内从 1 数。这里**解析 ToolItemCatalog.cs**，不手抄：
# 手抄一份就会和源码漂（v0.6.2 加「笔刷」时它就漏了，预览里 Anarchy 还显示成第 10 项）。
CATALOG = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                       "ToolItemCatalog.cs")


def load_pages():
    """{domain: [(number, id, source, family)]}，按源码顺序取三段数组。

    v0.9.1：例子按 ToolItemDef.ExampleFamily 取（ex.<family>.*），
    预览必须和 LocaleTable.Example() 完全一致，否则预览里看到的还是旧的跨页例子。
    """
    cat = io.open(CATALOG, encoding="utf-8-sig").read()
    consts = dict(re.findall(r'const string\s+(\w+)\s*=\s*"([^"]*)"', cat))
    # 三段数组的名字与顺序（ToolItemCatalog.cs）
    arrays = [("__ASSET__", lc.ASSET_ITEMS if hasattr(lc, "ASSET_ITEMS") else "AssetItems"),
              ("__FN__", "FunctionItems"), ("__ITEM__", "ItemItems")]
    # 更稳妥：直接按数组名字定位
    names = ["AssetItems", "FunctionItems", "ItemItems"]
    pages = {}
    for name in names:
        start = cat.find("new ToolItemDef[]")
        # 找到声明处
        decl = re.search(r"%s\s*=\s*new ToolItemDef\[\]" % name, cat)
        if not decl:
            decl = re.search(r"public static readonly ToolItemDef\[\]\s*%s" % name, cat)
        seg = cat[decl.end():cat.find(";", decl.end())]
        rows = []
        for m in re.finditer(r"new ToolItemDef\((\w+),\s*(\d+),\s*ItemSource\.(\w+)", seg):
            rows.append((int(m.group(2)), consts.get(m.group(1), m.group(1)), m.group(3)))
        # 每条构造调用末尾的 exampleFamily（第 12 个形参，只有写了引号字面量才取）
        fams = []
        for call in re.finditer(r"new ToolItemDef\((.*?)\)\s*,", seg, re.S):
            pass
        for m in re.finditer(r"new ToolItemDef\((.*?)\),", seg, re.S):
            args = m.group(1)
            tail = re.findall(r'"([^"]*)"', args)
            fam = ""
            # 倒数第二个字符串是 places，最后一个是 exampleFamily（有就取）
            if len(tail) >= 2:
                fam = tail[-1]
            fams.append(fam)
        rows = [(n, i, s2, fams[k] if k < len(fams) else "") for k, (n, i, s2) in enumerate(rows)]
        pages[name] = rows
    return pages


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
        if not ex:
            return tpl.replace("{0}", "").rstrip()
        return tpl.replace("{0}", ex)

    def nl(text):
        """源码里写成 \\n 的换行是给游戏渲染用的；预览里还原成真换行，
        否则预览会显示成字面量「\\n」，看不出实际排版。"""
        return text.replace("\\n", "\n") if text else text

    ALIAS = {"color": "other"}

    def txt(item_id, slot, domain, family=""):
        """ItemText 三级回退：本项键 -> LocaleAlias 键 -> 去掉 f./i. 的同名键。"""
        base = item_id[2:] if item_id[:2] in ("f.", "i.") else item_id
        cands = [item_id]
        if item_id in ALIAS:
            cands.append(ALIAS[item_id])
        cands.append(base)
        for cid in cands:
            k = "item.%s.%s" % (cid, slot)
            if k in d and d[k]:
                return d[k]
        return d.get("item.%s.%s" % (base, slot), "")

    def example(item_id, slot, family):
        """与 LocaleTable.Example() 一致：有 family 就只取 ex.<family>.<slot>，不再回落到本项。"""
        if family:
            if family == "none":
                return ""
            return d.get("ex.%s.%s" % (family, slot), "")
        return txt(item_id, "ex." + slot, "", family)

    pages = load_pages()
    total_items = sum(len(v) for v in pages.values())
    if not total_items:
        print("FAIL 没从 ToolItemCatalog.cs 解析到任何项")
        return 1
    # 每页编号必须各自 1..n（v0.7.0 起不再跨页连续）
    for name, rows in pages.items():
        nums = [n for n, _i, _s, _f in rows]
        if nums != list(range(1, len(rows) + 1)):
            print("FAIL %s 编号不是 1..%d 连续：%s" % (name, len(rows), nums))
            return 1
        for _n, i, s, _f in rows:
            if s not in ("Vanilla", "Toolbar", "AnarchyMod"):
                print("FAIL %s 里 %s 的 ItemSource 不认识：%s" % (name, i, s))
                return 1

    # 「全局共用」按页不同：游戏里只有一份设置的那几项（构造参数 FamilyFreeScope=true）
    # 走 scope.line.sharedSingle（「本页所有支持这项的工具共用一份」）。
    cat = io.open(CATALOG, encoding="utf-8-sig").read()
    single = set()
    for m in re.finditer(r"new ToolItemDef\((\w+),\s*\d+,\s*ItemSource\.\w+,[^\n]*?, (?:s_\w+|null), true\)", cat):
        single.add(m.group(1))
    consts2 = dict(re.findall(r'const string\s+(\w+)\s*=\s*"([^"]*)"', cat))
    single = {consts2.get(k, k).lower() for k in single}

    DOMAIN_OF = {"AssetItems": ("asset", "group.official", "scope.line.category"),
                 "FunctionItems": ("function", "group.official", None),
                 "ItemItems": ("item", "group.official", None)}

    out = []
    out.append("=========== %s ===========" % locale)
    # ---- 总开关页 ----
    out.append("")
    out.append("【标签页】%s" % d["tab.master"])
    out.append("%s = %s" % (d["enabled.label"], d["enabled.desc"]))
    out.append("")
    out.append("【板块】" + d["group.domain"])
    for key, dom in (("domain.asset", "asset"), ("domain.function", "function"), ("domain.item", "item")):
        if key + ".label" in d:
            out.append("  %s： %s / %s" % (d[key + ".label"], d.get(key + ".desc", ""), ""))
    # ---- 三页 ----
    for name, rows in pages.items():
        suffix, group_key, _ = DOMAIN_OF[name]
        official = [(n, i, f) for n, i, s, f in rows if s in ("Vanilla", "Toolbar")]
        anarchy = [(n, i, f) for n, i, s, f in rows if s == "AnarchyMod"]
        cat_key = "scope.category" if suffix == "asset" else "scope.category." + suffix
        line_cat = "scope.line.category" if suffix == "asset" else "scope.line.category." + suffix
        tab_key = {"asset": "tab.assets", "function": "tab.functions", "item": "tab.items"}[suffix]
        out.append("")
        out.append("【标签页】%s" % d[tab_key])

        def emit(title_key, page_rows):
            out.append("")
            out.append("【板块】" + d[title_key])
            for num, item_id, family in page_rows:
                label = "%d. %s" % (num, txt(item_id, "label", suffix) or item_id)
                out.append("")
                out.append("── %s" % label)
                out.append("   说明: " + nl(txt(item_id, "desc", suffix)))
                # v0.8.0：与 LocaleTable.ScopeDescription 完全对齐 ——
                # 「同类 / 全局共用 / 全部不共用」三句都按当前项的域取专用键
                # （scope.line.X.<asset|function|item>），本语言没写专用键时剥掉最后一段
                # 退回通用句。资产页用的就是通用句，所以只有功能页 / 道具页真的会切。
                # 这里的 suffix 就是当前页的域名（asset / function / item）。
                def by_domain(base_key):
                    """有专用键用专用键，否则退回不带后缀的通用句（同 C# 的 Line()）。"""
                    if suffix == "asset":
                        return d.get(base_key, base_key)
                    k = base_key + "." + suffix
                    return d.get(k, d.get(base_key, k))

                shared_key = "scope.line.sharedSingle" if item_id in single else "scope.line.shared"
                if item_id in single:
                    shared_line = d[shared_key]          # 本页共用一份，三页通用
                else:
                    shared_line = by_domain("scope.line.shared")
                # v0.9.0 需求 4：功能页与道具页不再提供「同类」这一档
                # （LocaleTable.BuildScopeDropdown 只给建筑与服务页列它），
                # 所以预览也要少一行，否则看到的下拉说明比游戏里多一项。
                scope = [d.get("scope.desc." + suffix, d["scope.desc"]),
                         fill(d["scope.line.group"], example(item_id, "group", family)),
                         fill(d["scope.line.menu"], example(item_id, "menu", family))]
                if suffix == "asset":
                    scope.append(fill(d["scope.line.category"],
                                      example(item_id, "category", family)))
                scope.append(shared_line)
                scope.append(by_domain("scope.line.unique"))
                scope.append(d["scope.note"])
                out.append("   共用范围（%s）:" % d["scope.label"])
                for i, line in enumerate(scope):
                    out.append("     %d) %s" % (i + 1, nl(line)))

        emit(group_key, official)
        if anarchy:
            emit("group.anarchy", anarchy)
    out.append("")
    out.append("【关于页按钮】%s / %s / %s" % (d["about.kofi"], d["about.forum"], d["about.rainbow"]))
    print("\n".join(out))
    io.open(os.path.join(lc.ROOT, "research", "l10n_preview_%s.txt" % locale), "w",
            encoding="utf-8", newline="\n").write("\n".join(out) + "\n")
    total = sum(len(x) for x in out)
    print("[总字数 %d]" % total)


if __name__ == "__main__":
    main()
