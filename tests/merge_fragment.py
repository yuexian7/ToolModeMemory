#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
把翻译子代理交回的片段（research/l10n_fragment_<locale>.cs）并回对应的语言函数体。

为什么用脚本而不是手工粘：一份片段 69 行 × 10 个语言，手工粘必错行；
而且其中 8 把键（enabled.label / enabled.desc / scope.note / scope.line.sharedSingle /
item.brush.*）在函数体里**已经有旧说法**，必须删掉旧行再插新行 ——
只追加的话两条同键行靠"后写覆盖"碰巧也对，但文件里留着两套说法，
下次谁改前面的那条就是白改（0.6.x 的教训：留着旧行 = 留着旧说法的来源）。

用法： python tests\\merge_fragment.py de-DE es-ES ...
幂等：重复运行同一片段只保留一份（先删同键旧行再插）。
"""
import io
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 语言函数在哪个文件（与 LocaleTable.Build() 的分发一致）
FUNC = {
    "zh-HANS": ("LocaleTable.cs", "ZhHans"),
    "zh-HANT": ("LocaleTable.cs", "ZhHant"),
    "en-US": ("LocaleTable.cs", "En"),
    "de-DE": ("LocaleTable.Eu.cs", "De"),
    "es-ES": ("LocaleTable.Eu.cs", "Es"),
    "fr-FR": ("LocaleTable.Eu.cs", "Fr"),
    "it-IT": ("LocaleTable.Eu.cs", "It"),
    "pl-PL": ("LocaleTable.Eu2.cs", "Pl"),
    "pt-BR": ("LocaleTable.Eu2.cs", "Pt"),
    "ru-RU": ("LocaleTable.Eu2.cs", "Ru"),
    "ja-JP": ("LocaleTable.Asia.cs", "Ja"),
    "ko-KR": ("LocaleTable.Asia.cs", "Ko"),
}

LINE = re.compile(r'^\s*d\[(?P<key>"[^"]+"|[A-Za-z_][A-Za-z0-9_]*(?:\s*\+\s*"[^"]+")*)\]'
                  r'\s*=\s*"(?P<val>.*)"\s*;\s*$')
CONST = re.compile(r'const string\s+(\w+)\s*=\s*"([^"]*)"')
ASSIGN_KEY = re.compile(r'^\s*d\["([^"]+)"\]\s*=')


def frag_lines(locale):
    path = os.path.join(ROOT, "research", "l10n_fragment_%s.cs" % locale)
    out = []
    for raw in io.open(path, encoding="utf-8-sig").read().split("\n"):
        s = raw.strip()
        if not s:
            continue
        if not LINE.match(s):
            raise SystemExit("FAIL 片段里有不合规的行：%s -> %s" % (path, s[:80]))
        out.append(s)
    return out


def body_span(lines, func):
    """返回 (start, end)：函数体内第一行到 return d; 之前的行号区间。"""
    head = re.compile(r"private static Dictionary<string, string> %s\(\)" % func)
    start = None
    for i, l in enumerate(lines):
        if head.search(l):
            start = i
            break
    if start is None:
        raise SystemExit("FAIL 找不到函数 %s" % func)
    depth = 0
    seen_open = False
    for j in range(start, len(lines)):
        depth += lines[j].count("{") - lines[j].count("}")
        if lines[j].count("{") > 0:
            seen_open = True
        if seen_open and depth == 0:
            ret = None
            for k in range(j - 1, start, -1):
                if lines[k].strip() == "return d;":
                    ret = k
                    break
            if ret is None:
                raise SystemExit("FAIL %s 里没找到 return d;" % func)
            return start, ret
    raise SystemExit("FAIL %s 花括号没配平" % func)


def merge(locale):
    fname, func = FUNC[locale]
    path = os.path.join(ROOT, fname)
    text = io.open(path, encoding="utf-8-sig").read()
    nl = "\r\n" if "\r\n" in text else "\n"
    lines = text.replace("\r\n", "\n").split("\n")
    consts = dict(CONST.findall(text))

    def key_of(s):
        m = ASSIGN_KEY.match(s)
        if m:
            return m.group(1)
        lm = LINE.match(s)
        if not lm:
            return None
        k = lm.group("key").strip()
        if k.startswith('"'):
            return k[1:-1]
        # d[kScopeCategory + ".function"] 这种拼出来的键：能解析就登记
        parts = re.findall(r'"([^"]*)"|([A-Za-z_][A-Za-z0-9_]*)', k)
        out = ""
        for lit, ident in parts:
            if lit:
                out += lit
            else:
                if ident not in consts:
                    return None
                out += consts[ident]
        return out

    frag = frag_lines(locale)
    frag_keys = [key_of(s) for s in frag]
    for s, kk in zip(frag, frag_keys):
        if kk is None:
            raise SystemExit("FAIL 片段键解析不了：%s" % s[:80])

    start, ret = body_span(lines, func)
    # 1) 删掉函数体内与片段同键的旧行（片段本身不在文件里，所以只删现有的）
    kept = []
    removed = 0
    for i in range(0, len(lines)):
        if start < i < ret:
            kk = key_of(lines[i])
            if kk in frag_keys:
                removed += 1
                continue
        kept.append(lines[i])
    # 删完之后原来的区间失效，重新定位一次
    lines = kept
    start, ret = body_span(lines, func)
    # 2) 在 return d; 之前插入片段（带一行注释，注明版本与来源，方便回查）
    block = ["", "\t\t// ==================== v0.7.0 新增 / 重写（%s）====================" % locale]
    block += ["\t\t" + s for s in frag]
    new = lines[:ret] + block + lines[ret:]
    io.open(path, "w", encoding="utf-8", newline=nl).write("\n".join(new))
    print("OK %s -> %s::%s  删旧 %d 行，插新 %d 行" % (locale, fname, func, removed, len(frag)))
    return len(frag)


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 1
    total = 0
    for loc in sys.argv[1:]:
        if loc not in FUNC:
            print("FAIL 未知语言 %s" % loc)
            return 1
        total += merge(loc)
    print("合计插入 %d 行" % total)
    return 0


if __name__ == "__main__":
    sys.exit(main())
