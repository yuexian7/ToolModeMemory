#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
版本字面量一致性检查（离线，~30 行判定，不需要游戏 DLL）。

第三轮审查的 P0-2：本模组的版本号只有两处字面量（`ToolModeMemoryMod.kVersion` 与
`PublishConfiguration.xml` 的 `<ModVersion>`），外加 `<ChangeLog>` 首行。0.6.1 那次就是
只改了 xml 没改代码，日志横幅停在旧版本号上，实机判据（「横幅版本==本版」）当场失效。
所以这里一次钉死三处，并且顺手钉两条发布红线：

  1. 三处版本号必须逐字相同，且形如 x.y.z；
  2. `<ModId>` 必须还在（丢了会在商店页重复上架，它是发布身份，不是草稿）；
  3. ChangeLog 只允许写当前版本（旧版本正文留在文件里 = 线上会把历史一起重推）；
  4. 已构建产物存在时：DLL 里必须搜得到本版本的 UTF-16LE 字面量，
     并且产物比所有源码新（否则「验的版本号」验的是上一次构建的陈货）。

运行： python tests\\version_check.py
"""
import io
import os
import re
import struct
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MOD = os.path.join(ROOT, "ToolModeMemoryMod.cs")
XML = os.path.join(ROOT, "Properties", "PublishConfiguration.xml")
DLL = os.path.join(ROOT, "bin", "Release", "net48", "ToolModeMemory.dll")

fails = []


def check(cond, msg):
    if cond:
        print("OK   " + msg)
    else:
        print("FAIL " + msg)
        fails.append(msg)


def read(path):
    return io.open(path, encoding="utf-8-sig").read()


def main():
    src = read(MOD)
    # 文件头部那段历史注释里真的写着 `<ModVersion>` / `<ChangeLog>` 这些字面量，
    # 不先剥掉注释就会把注释正文当成元素内容（第一版就在这里假报警）。
    xml = re.sub(r"<!--[\s\S]*?-->", "", read(XML))

    # ---- 1. 三处版本字面量 ----
    m_code = re.search(r'public const string kVersion = "([^"]+)"', src)
    m_xml = re.search(r'<ModVersion Value="([^"]+)"', xml)
    m_log = re.search(r"<ChangeLog>\s*([^<\s][^\n]*)", xml)
    check(m_code is not None, "ToolModeMemoryMod.cs 里有 kVersion 字面量")
    check(m_xml is not None, "PublishConfiguration.xml 里有 <ModVersion>")
    check(m_log is not None, "PublishConfiguration.xml 里有 <ChangeLog> 正文")
    if not (m_code and m_xml and m_log):
        print("\nFAILURES=3")
        return 1

    code_v = m_code.group(1)
    xml_v = m_xml.group(1)
    log_v = m_log.group(1).strip()
    check(re.match(r"^\d+\.\d+\.\d+$", code_v) is not None,
          "kVersion 形如 x.y.z（当前 %s）" % code_v)
    check(code_v == xml_v, "kVersion == <ModVersion>（%s / %s）" % (code_v, xml_v))
    check(log_v == "v" + code_v, "<ChangeLog> 首行 == v<kVersion>（%r）" % log_v)

    # ---- 2. 发布红线：ModId 是身份，不是草稿 ----
    m_id = re.search(r'<ModId Value="(\d+)"', xml)
    check(m_id is not None, "<ModId> 仍在（丢了会重复上架，绝不能删）")

    # ---- 3. ChangeLog 只写当前版本 ----
    body = xml[xml.find("<ChangeLog>"):xml.find("</ChangeLog>")]
    heads = re.findall(r"(?m)^v(\d+\.\d+\.\d+)\b", body)
    check(heads == [code_v], "ChangeLog 只含当前版本一节（读到小节头：%s）" % (heads or "无"))
    check("中文" in body, "ChangeLog 有中英两段（玩家可见文案按双语写）")

    # ---- 4. 产物侧：字面量真的编进去了，且产物不是陈货 ----
    if os.path.exists(DLL):
        data = open(DLL, "rb").read()
        # #US 堆里的字符串是 UTF-16LE；grep -a "0.6.2" 搜不到它（0.6.2 之前的假命中是正则通配）
        needle = code_v.encode("utf-16-le")
        check(needle in data, "部署产物里搜得到 v%s 的 UTF-16LE 字面量" % code_v)
        newest = max(os.path.getmtime(os.path.join(dirpath, f))
                     for dirpath, _dirs, files in os.walk(ROOT)
                     for f in files
                     if f.endswith(".cs") and os.sep + "research" + os.sep not in
                        os.path.join(dirpath, f))
        check(os.path.getmtime(DLL) >= newest,
              "产物不早于任何源码（本次判定用的是当前构建，不是上一次的陈货）")
    else:
        print("SKIP 没有 bin/Release/net48/ToolModeMemory.dll，先跑 dotnet build -c Release 再验字面量")

    print()
    print("ALL PASS" if not fails else "FAILURES=%d" % len(fails))
    return 0 if not fails else 1


if __name__ == "__main__":
    sys.exit(main())
