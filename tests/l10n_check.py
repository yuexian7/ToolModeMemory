#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
12 语言文案完整性检查（离线，纯源码解析，不需要游戏 DLL）。

检查内容：
  1. 每个语言函数（ZhHans/ZhHant/En/De/Es/Fr/It/Ja/Ko/Pl/Pt/Ru）都补齐了必需键；
  2. v0.2.1 起共用范围定义改成「每行模板 + 每项例子」：
     scope.line.* 与 scope.note 必须存在，三条带例子的模板必须含 {0}，
     13 个工具项 × 3 个例子（group/menu/category）必须齐全；
  3. 旧键不得复活：scope.defs（单列说明板块）与 RETIRED 里已取消独立项的 item.<x>.* 键
     （v0.2.2 起 elevationStep 并入 elevation 的 Subs，它自己的文案键就是漏删）；
  4. 文案不得为空，也不得再出现已被证伪的归属（Extra Networks）；
  5. ITEM_IDS 与 ToolItemCatalog 的成员和顺序完全一致（「其它」永远官方板块最后，v0.6.2 需求）。

运行： python tests\\l10n_check.py
"""
import io
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FILES = ["LocaleTable.cs", "LocaleTable.Eu.cs", "LocaleTable.Asia.cs", "LocaleTable.Eu2.cs"]

LANGS = {
    "ZhHans": "zh-HANS", "ZhHant": "zh-HANT", "En": "en-US", "De": "de-DE",
    "Es": "es-ES", "Fr": "fr-FR", "It": "it-IT", "Ja": "ja-JP", "Ko": "ko-KR",
    "Pl": "pl-PL", "Pt": "pt-BR", "Ru": "ru-RU",
}

# Frames() 里 12 份共用、不需要各语言重复的键
FRAMES_KEYS = {"frame.locale", "item.anarchy.label", "scope.group.none"}

# 设置页跨板块连续编号的 13 项（v0.6.2 起「笔刷」独立成行，编号 9；「其它」永远官方板块最后，
# v0.7.0：设置页拆成三个标签页，同名行按域各一套，项 id 带域前缀 f. / i.。
# 顺序 = 目录顺序 = 设置页顺序（资产页 1..14，功能页 1..5，道具页 1..5；编号在每个页内各自从 1 数）。
# 这份表**只是文档**：真正的一致性由下面对着 ToolItemCatalog.cs 解析出来的目录核对，
# 手抄错了会被 "目录与 ITEM_IDS 不一致" 直接拦下。
ITEM_IDS = ["themes", "packs", "toolMode", "elevation", "parallel", "snap", "topography",
            "underground", "brush", "color", "decoration", "anarchy", "leftRight", "general",
            "f.toolMode", "f.snap", "f.topography", "f.underground", "f.brush",
            "i.toolMode", "i.snap", "i.brush", "i.topography", "i.underground"]
# 已取消的独立项：它们的键再出现就是漏删。
# 已取消的独立项：它们的键再出现就是漏删。
#   elevationStep：0.2.2 并入 elevation 的 Subs。
# 注意 0.7.0 的「其它」不列在这里：它作为**项**已经取消（目录里没有了，上面那条
# 「目录与 ITEM_IDS 一致」就是钉子），但 item.other.desc / .ex.* 三句**必须留着** ——
# 「颜色」这一项换了名字，12 种语言的文案靠 ToolItemDef.LocaleAlias 沿用旧的那份，
# 删掉就等于把 12 种语言已经写对的句子清空。
RETIRED = ["elevationStep"]

# 换了名字的项：id → 文案沿用哪一份旧 id（对应 ToolItemDef.LocaleAlias）。
# 「颜色」就是 0.6.2 的「其它」，12 种语言的 desc/ex.* 内容本来就全是讲道路颜色。
TEXT_ALIAS = {"color": "other"}

# 三个标签页与每项归属（v0.7.0）：不手抄，全部从 ToolItemCatalog.cs 里解析，见 parse_catalog。
DOMAIN_LABEL = {"Asset": "资产", "Function": "功能", "Item": "道具"}

SCALAR_KEYS = [
    "mod.name",
    # v0.7.0 五个标签页 + 分域开关板块
    "tab.mod", "tab.master", "tab.assets", "tab.functions", "tab.items", "tab.about",
    "group.master", "group.domain", "group.official", "group.anarchy",
    "group.reset", "group.about",
    "enabled.label", "enabled.desc",
    "domain.asset.label", "domain.asset.desc",
    "domain.function.label", "domain.function.desc",
    "domain.item.label", "domain.item.desc",
    "scope.label", "scope.desc",
    "scope.line.group", "scope.line.menu", "scope.line.category",
    "scope.line.category.function", "scope.line.category.item",
    "scope.line.shared", "scope.line.sharedSingle", "scope.line.unique", "scope.note",
    "scope.group", "scope.menu", "scope.category",
    "scope.category.function", "scope.category.item",
    "scope.globalShared", "scope.globalUnique",
    "tag.vanilla", "tag.recommended", "tag.recVanilla",
    "reset.label", "reset.desc", "reset.warn", "reset.confirm",
    "resetall.label", "resetall.desc", "resetall.warn", "resetall.confirm",
    "folder.label", "folder.desc",
    "about.version", "about.author",
    "about.kofi", "about.kofi.desc", "about.forum", "about.forum.desc",
    "about.rainbow", "about.rainbow.desc",
]

FORBIDDEN = ["scope.defs"]

# 关于页三个按钮：owner 定的名字。中文两份用他写的中文，其余 10 种语言一律用英文原文。
# 0.2.2 曾把 "RAINBOW官网" 当成品牌串照抄进非中文语言，被要求改回英文，所以这里钉死。
BUTTON_KEYS = {"about.kofi": "Buy me a Coffee",
               "about.forum": "Forum Page",
               "about.rainbow": "RAINBOW Site"}
ZH_LANGS = ("zh-HANS", "zh-HANT")
# 拉丁/西里尔语言里出现中日韩字就是漏翻（日语本身就写汉字，不参与这条检查）
NO_CJK_LANGS = ("en-US", "de-DE", "es-ES", "fr-FR", "it-IT", "pl-PL", "pt-BR", "ru-RU")
CJK = re.compile(u'[ぁ-ヿ㐀-䶿一-鿿가-힣豈-﫿]')

TEMPLATE_KEYS = ["scope.line.group", "scope.line.menu", "scope.line.category",
                 "scope.line.category.function", "scope.line.category.item"]

# 赋值行：d[<键表达式>] = "值";
# 键表达式允许三种写法（v0.7.0 起了第三种）："字面量" / 常量名 / 常量名 + "后缀"
# —— 例如 d[kScopeCategory + ".function"]，三页各一份的标签键就是这么写的。
ASSIGN = re.compile(r'^\s*d\[(?P<key>[^\]]+)\]\s*=\s*"(?P<val>.*)"\s*;\s*$')
KEY_PART = re.compile(r'"[^"]*"|[A-Za-z_][A-Za-z0-9_.]*')
CONST = re.compile(r'const string\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*"(?P<val>[^"]*)"')
FUNC = re.compile(r'private static Dictionary<string, string> (?P<name>\w+)\(\)')


def resolve_key(expr, const_map):
    """`kScopeCategory + ".item"` → "scope.category.item"；解析不了返回 None。"""
    parts = KEY_PART.findall(expr)
    if not parts or "".join(parts) != expr.replace(" ", "").replace("+", ""):
        return None
    out = []
    for p in parts:
        if p.startswith('"'):
            out.append(p[1:-1])
            continue
        if p not in const_map:
            return None
        out.append(const_map[p])
    return "".join(out)


def load():
    text = {}
    for f in FILES:
        p = os.path.join(ROOT, f)
        if os.path.exists(p):
            text[f] = io.open(p, encoding="utf-8-sig").read().split("\n")
    return text


def consts(text):
    out = {}
    for lines in text.values():
        for line in lines:
            m = CONST.search(line)
            if m:
                out[m.group("name")] = m.group("val")
    return out


def parse(text, const_map):
    """{lang: {key: value}}"""
    tables = {}
    for fname, lines in text.items():
        cur = None
        depth = 0
        started = False
        for line in lines:
            fm = FUNC.search(line)
            if fm and fm.group("name") in LANGS:
                cur = LANGS[fm.group("name")]
                tables.setdefault(cur, {})
                started = False
                depth = 0
            if cur is None:
                continue
            if not started:
                if line.count("{") > 0:
                    started = True
                    depth += line.count("{") - line.count("}")
                continue
            depth += line.count("{") - line.count("}")
            am = ASSIGN.match(line)
            if am:
                key = resolve_key(am.group("key").strip(), const_map)
                if key is None:
                    print("!! %s: 无法解析键 %s" % (fname, am.group("key")))
                    continue
                tables[cur][key] = am.group("val")
            if started and depth <= 0:
                cur = None
                started = False
    return tables


def check_shape(text):
    """语言函数体内只允许：空行、注释、d["k"] = "v"; 、return d;。多出来的行说明结构被改坏了。"""
    bad = []
    ok_line = re.compile(r'^\s*(//|///|/\*|\*)')
    for fname, lines in text.items():
        inside = False
        lang = None
        for i, line in enumerate(lines):
            fm = FUNC.search(line)
            if fm and fm.group("name") in LANGS:
                inside = True
                lang = LANGS[fm.group("name")]
                continue
            if not inside:
                continue
            s = line.strip()
            if s in ("{", "}", "") or s == "return d;":
                if s == "}":
                    inside = False
                    lang = None
                continue
            if ok_line.match(line) or ASSIGN.match(line):
                continue
            if s.startswith("Dictionary<string, string> d = Frames("):
                continue
            bad.append("%s:%d [%s] %s" % (fname, i + 1, lang, s[:70]))
    return bad


def split_args(s):
    """按顶层逗号切参数（忽略字符串与括号内部）。"""
    out, depth, cur, instr = [], 0, [], False
    i = 0
    while i < len(s):
        c = s[i]
        if instr:
            cur.append(c)
            if c == "\\":
                if i + 1 < len(s):
                    cur.append(s[i + 1])
                i += 2
                continue
            if c == '"':
                instr = False
            i += 1
            continue
        if c == '"':
            instr = True
            cur.append(c)
        elif c in "([{":
            depth += 1
            cur.append(c)
        elif c in ")]}":
            depth -= 1
            cur.append(c)
        elif c == "," and depth == 0:
            out.append("".join(cur).strip())
            cur = []
        else:
            cur.append(c)
        i += 1
    if "".join(cur).strip():
        out.append("".join(cur).strip())
    return out


def catalog_calls(src):
    """所有 new ToolItemDef(...) 的参数列表（允许跨行，靠括号配平取）。"""
    calls = []
    token = "new ToolItemDef("
    pos = 0
    while True:
        k = src.find(token, pos)
        if k < 0:
            return calls
        depth, j, instr = 0, k + len(token) - 1, False
        while j < len(src):
            c = src[j]
            if instr:
                if c == "\\":
                    j += 2
                    continue
                if c == '"':
                    instr = False
            elif c == '"':
                instr = True
            elif c == "(":
                depth += 1
            elif c == ")":
                depth -= 1
                if depth == 0:
                    break
            j += 1
        calls.append(split_args(src[k + len(token):j]))
        pos = j + 1


def parse_catalog():
    """从 ToolItemCatalog.cs 解析出 [(number, item_id, domain, source, locale_alias)]。

    不手抄：v0.6.2 加「笔刷」时手抄的表漏了一项，设置页出现「有这行、说明是空的」，
    所以目录本身就是唯一事实来源，这里只做形状校验（编号、分域、别名）。
    构造函数签名（见 ToolItemDef..ctor）：
      id, number, source, vanillaScope, recommendedScope, defaultEnabled,
      subs = null, familyFreeScope = false, domain = Asset, localeAlias = null
    """
    src = io.open(os.path.join(ROOT, "ToolItemCatalog.cs"), encoding="utf-8-sig").read()
    consts = dict(re.findall(r'const string\s+(\w+)\s*=\s*"([^"]*)"', src))
    rows = []
    for args in catalog_calls(src):
        if len(args) < 3:
            continue
        item_id = consts.get(args[0], args[0].strip('"'))
        number = int(args[1])
        source = args[2].split(".")[-1]
        domain = "Asset"
        alias = None
        for idx, a in enumerate(args[3:], start=3):
            m = re.match(r"^ItemDomain\.(\w+)$", a)
            if m:
                domain = m.group(1)
            elif idx == 9 and re.match(r"^k\w+$", a):
                # 第 10 个形参才是 localeAlias（见 ToolItemDef..ctor），别的位置上是
                # MemoryScope / s_*Subs 之类，不能当别名
                alias = consts.get(a, a)
        rows.append((number, item_id, domain, source, alias))
    return rows


def check_catalog_shape(rows):
    """目录形状：每个标签页内编号 1..n 连续；Anarchy 项在资产页最后；别名指向存在的旧 id。"""
    bad = []
    by_domain = {}
    for row in rows:
        by_domain.setdefault(row[2], []).append(row)
    for domain, items in sorted(by_domain.items()):
        nums = [n for n, _i, _d, _s, _a in items]
        want = sorted(nums)
        if nums != want or want != list(range(1, len(items) + 1)):
            bad.append("%s 页编号不是 1..%d 连续：%s" % (DOMAIN_LABEL.get(domain, domain), len(items), nums))
        seq = [s for _n, _i, _d, s, _a in items]
        if "AnarchyMod" in seq and seq.index("AnarchyMod") != len(seq) - seq.count("AnarchyMod"):
            bad.append("%s 页的第三方（Anarchy）项没有排在最后：%s"
                       % (DOMAIN_LABEL.get(domain, domain), seq))
    for num, item_id, domain, source, alias in rows:
        # 别名指向的旧 id 本来就不在目录里（它是被改名/合并掉的项），
        # 所以这里对的是「检查表里登记的别名」，两边不一致说明有一边漏改。
        if alias and TEXT_ALIAS.get(item_id) != alias:
            bad.append("%d.%s 目录里的 LocaleAlias=%r 与检查表 TEXT_ALIAS=%r 不一致"
                       % (num, item_id, alias, TEXT_ALIAS.get(item_id)))
        if alias is None and item_id in TEXT_ALIAS:
            bad.append("%d.%s 检查表登记了文案别名 %r，但目录里没有 LocaleAlias（运行时会查错键）"
                       % (num, item_id, TEXT_ALIAS[item_id]))
    return bad


def main():
    text = load()
    if not text:
        print("找不到 LocaleTable*.cs")
        return 1
    all_consts = {}
    for lines in text.values():
        for line in lines:
            m = CONST.search(line)
            if m:
                all_consts[m.group("name")] = m.group("val")
    tables = parse(text, all_consts)

    rows = parse_catalog()
    if not rows:
        print("FAIL 没从 ToolItemCatalog.cs 解析到任何项（构造调用格式变了，检查器要跟着改）")
        return 1
    catalog_ids = [i for _n, i, _d, _s, _a in rows]
    stale = sorted(set(ITEM_IDS) - set(catalog_ids))
    new = sorted(set(catalog_ids) - set(ITEM_IDS))
    if stale or new:
        print("FAIL 目录与 ITEM_IDS 不一致：文案未覆盖 %s / 已不存在的项 %s"
              % (new or "无", stale or "无"))
        return 1
    bad_shape = check_catalog_shape(rows)
    if bad_shape:
        for msg in bad_shape:
            print("FAIL 目录形状 %s" % msg)
        return 1
    print("OK   目录 %d 项：%s（每页内编号连续、Anarchy 在资产页最后）"
          % (len(rows), " / ".join("%s%d项" % (DOMAIN_LABEL[d], sum(1 for r in rows if r[2] == d))
                                   for d in ("Asset", "Function", "Item"))))

    fails = 0
    print("解析到语言：%s" % ", ".join(sorted(tables)))
    required = list(SCALAR_KEYS)
    # 每一项需要哪些键，必须和 LocaleTable.ItemText() 的三级回退**完全一致**，
    # 否则要么误报（回退本来就合法），要么漏报（设置页出现空说明）。
    #   1) item.<id>.<slot>                 —— 本项自己写过
    #   2) item.<LocaleAlias>.<slot>        —— 改名的项（color ← other）
    #   3) item.<去掉 f./i. 前缀>.<slot>    —— 功能页 / 道具页的同名行沿用资产页那份
    # 有 2)/3) 可依赖的键组成「可选组」：本项键与回退键至少有一个存在即可，
    # 两个都没有才算缺（漏写的表现正是设置页上那一项说明空白）。
    groups = []
    for item in ITEM_IDS:
        alias = TEXT_ALIAS.get(item)
        base = item.split(".", 1)[1] if item.startswith(("f.", "i.")) else None
        for sfx in ("label", "desc", "ex.group", "ex.menu", "ex.category"):
            own = "item.%s.%s" % (item, sfx)
            fb = ("item.%s.%s" % (alias, sfx)) if alias else (
                ("item.%s.%s" % (base, sfx)) if base else None)
            if fb is None:
                required.append(own)
            else:
                groups.append((own, fb))
    for lang in sorted(LANGS.values()):
        d = tables.get(lang)
        if d is None:
            print("FAIL %s：没有解析到语言函数" % lang)
            fails += 1
            continue
        missing = [k for k in required if k not in d and k not in FRAMES_KEYS]
        # 可选组：本项键与回退键都没有才算缺
        for own, fb in groups:
            if own not in d and fb not in d:
                missing.append("%s（也找不到回退键 %s）" % (own, fb))
        banned = [k for k in FORBIDDEN if k in d]
        empty = [k for k, v in d.items() if not v.strip()]
        no_slot = [k for k in TEMPLATE_KEYS if "{0}" not in d.get(k, "")]
        stale = [k for k, v in d.items() if "Extra Networks" in v]
        retired = [k for k in d for r in RETIRED if k.startswith("item.%s." % r)]
        # 三个按钮名：非中文语言必须是 owner 定的英文原文，一个字都不许多/换
        wrong_btn = ["%s=%r" % (k, d.get(k, "")) for k, v in BUTTON_KEYS.items()
                     if lang not in ZH_LANGS and d.get(k, "") != v]
        # 拉丁/西里尔语言里混进中日韩字 = 漏翻（0.2.2 的 "RAINBOW官网" 就是这么漏出去的）
        cjk = ["%s=%s" % (k, v[:28]) for k, v in d.items()
               if lang in NO_CJK_LANGS and CJK.search(v)]
        bad = missing + banned + empty + no_slot + stale + retired + wrong_btn + cjk
        if bad:
            fails += 1
            print("FAIL %s (%d 个键)" % (lang, len(d)))
            for k in missing:
                print("   缺键   %s" % k)
            for k in banned:
                print("   旧键   %s" % k)
            for k in empty:
                print("   空值   %s" % k)
            for k in no_slot:
                print("   缺{{0}} %s" % k)
            for k in stale:
                print("   错误归属 %s" % k)
            for k in retired:
                print("   退役键 %s（高度阶段已并入高度）" % k)
            for k in wrong_btn:
                print("   按钮名应为英文 %s" % k)
            for k in cjk:
                print("   非中文语言含中日韩字 %s" % k)
        else:
            print("OK   %s：%d 键，%d 项例子齐全" % (lang, len(d), len(ITEM_IDS) * 3))

    # 结构检查：语言函数体里不该出现别的语句（改坏了编译就靠这条先发现）
    for msg in check_shape(text):
        fails += 1
        print("FAIL 结构 %s" % msg)

    # 语言之间键集合必须一致（漏翻译会被发现，en-US 回落不算通过）
    if "en-US" in tables:
        en = set(k for k in tables["en-US"])
        for lang, d in sorted(tables.items()):
            extra = set(d) - en
            if extra:
                fails += 1
                print("FAIL %s 有 en-US 没有的键：%s" % (lang, ", ".join(sorted(extra))))

    print()
    print("ALL PASS" if fails == 0 else "FAILURES=%d" % fails)
    return 0 if fails == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
