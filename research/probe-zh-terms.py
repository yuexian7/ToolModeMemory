import zipfile
p = r"F:\SteamLibrary\steamapps\common\Cities Skylines II\Cities2_Data\Content\Game\Locale.cok"
z = zipfile.ZipFile(p)
text = z.read("zh-HANS.loc").decode("utf-8", errors="replace")
terms = [
    "高程", "高度", "对齐", "吸附", "平行", "曲线", "连续", "网格", "替换", "升级",
    "推土", "拆除", "笔刷", "隧道", "高架", "道路", "轨道", "管道", "摆件", "装饰",
    "物件", "树木", "建筑", "工具选项", "创建", "放置", "模式", "直线", "地下",
    "重建", "拆毁", "道路工具", "铁路", "地铁", "车道", "简单", "复杂",
]
for t in terms:
    print(f"{t}={text.count(t)}")
for key in ["高程", "高度", "对齐", "吸附", "替换", "升级", "推土", "拆除", "笔刷", "曲线", "网格", "平行", "隧道", "高架", "摆件", "物件"]:
    print("== samples", key)
    n = 0
    for line in text.splitlines():
        if key in line and n < 5:
            print(line[:180])
            n += 1
