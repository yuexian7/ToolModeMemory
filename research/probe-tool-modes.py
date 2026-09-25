import zipfile
p = r"F:\SteamLibrary\steamapps\common\Cities Skylines II\Cities2_Data\Content\Game\Locale.cok"
z = zipfile.ZipFile(p)
text = z.read("zh-HANS.loc").decode("utf-8", errors="replace")
# extract Tools.TOOL_MODE entries and nearby
idx = 0
key = "Tools.TOOL_MODE"
while True:
    i = text.find(key, idx)
    if i < 0:
        break
    print(text[i:i + 80].replace("\n", "\\n"))
    idx = i + 1
    if idx > 5000000:
        break
print("--- GLOSSARY tools ---")
for k in ["Glossary.SECTION_TITLE[Tools]", "Glossary.SECTION_TITLE[Bulldoze", "Glossary.SECTION_TITLE[Roads]", "Glossary.SECTION_TITLE[Snap", "Glossary.SECTION_TITLE[Elevat", "Glossary.SECTION_TITLE[Net", "Glossary.SECTION_TITLE[Upgrade"]:
    i = text.find(k)
    if i >= 0:
        print(text[i:i + 120].replace("\n", "\\n"))
print("--- sample mode keys ---")
for k in ["Straight", "SimpleCurve", "ComplexCurve", "Continuous", "Grid", "Replace", "Point", "Create", "Brush", "Stamp", "Line", "Curve", "MainElements", "SubElements", "Everything", "Edit", "Generate", "AddSource"]:
    s = f"Tools.TOOL_MODE[{k}]"
    i = text.find(s)
    if i >= 0:
        print(text[i:i + 60].replace("\n", "\\n"))
    else:
        print(s, "NOT FOUND")
