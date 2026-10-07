#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
12 语言文案完整性检查（离线，纯源码解析，不需要游戏 DLL）。

检查内容：
  1. 每个语言函数（ZhHans/ZhHant/En/De/Es/Fr/It/Ja/Ko/Pl/Pt/Ru）都补齐了必需键；
  2. v0.2.1 起共用范围定义改成「每行模板 + 每项例子」：
     scope.line.* 与 scope.note 必须存在，三条带例子的模板必须含 {0}，
     11 个工具项 × 3 个例子（group/menu/category）必须齐全；
  3. 旧键不得复活：scope.defs（单列说明板块）、item.packs.* / item.themes.*（不在目录里）；
  4. 文案不得为空，也不得再出现已被证伪的归属（Extra Networks）。

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

# v0.2.2 的 12 项（顺序 = 设置页跨板块连续编号）；elevationStep 已并入 elevation 的 Subs
ITEM_IDS = ["themes", "packs", "toolMode", "elevation", "parallel", "snap", "topography",
            "underground", "other", "anarchy", "leftRight", "general"]
# 已取消的独立项：它们的键再出现就是漏删
RETIRED = ["elevationStep"]

SCALAR_KEYS = [
    "mod.name", "tab.mod", "tab.about",
    "group.master", "group.official", "group.anarchy",
    "group.reset", "group.about",
    "enabled.label", "enabled.desc",
    "scope.label", "scope.desc",
    "scope.line.group", "scope.line.menu", "scope.line.category",
    "scope.line.shared", "scope.line.sharedSingle", "scope.line.unique", "scope.note",
    "scope.group", "scope.menu", "scope.category", "scope.globalShared", "scope.globalUnique",
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

TEMPLATE_KEYS = ["scope.line.group", "scope.line.menu", "scope.line.category"]

ASSIGN = re.compile(r'^\s*d\[(?P<key>"[^"]*"|[A-Za-z_][A-Za-z0-9_]*)\]\s*=\s*"(?P<val>.*)"\s*;\s*$')
CONST = re.compile(r'const string\s+(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*"(?P<val>[^"]*)"')
FUNC = re.compile(r'private static Dictionary<string, string> (?P<name>\w+)\(\)')


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
                key = am.group("key")
                if key.startswith('"'):
                    key = key[1:-1]
                else:
                    key = const_map.get(key, key)
                    if key is None:
                        print("!! %s: 未知常量键 %s" % (fname, am.group("key")))
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


def main():
    text = load()
    if not text:
        print("找不到 LocaleTable*.cs")
        return 1
    const_map = consts(text)
    const_map = {k: v for k, v in const_map.items() if k.startswith("k") or k.startswith("KEY")}
    # 直接把 public const string kScopeGroup = "scope.group"; 这类映射建全
    all_consts = {}
    for lines in text.values():
        for line in lines:
            m = CONST.search(line)
            if m:
                all_consts[m.group("name")] = m.group("val")
    tables = parse(text, all_consts)

    fails = 0
    print("解析到语言：%s" % ", ".join(sorted(tables)))
    required = list(SCALAR_KEYS)
    for item in ITEM_IDS:
        required.append("item.%s.label" % item)
        required.append("item.%s.desc" % item)
        for slot in ("group", "menu", "category"):
            required.append("item.%s.ex.%s" % (item, slot))

    for lang in sorted(LANGS.values()):
        d = tables.get(lang)
        if d is None:
            print("FAIL %s：没有解析到语言函数" % lang)
            fails += 1
            continue
        missing = [k for k in required if k not in d and k not in FRAMES_KEYS]
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
