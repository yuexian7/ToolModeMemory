using System.Collections.Generic;
using Colossal;
using Game.UI;
using Game.UI.Widgets;

namespace ToolModeMemory
{
	/// <summary>
	/// 12 官方语言词条 + 范围下拉（带原版/推荐标注）。
	/// 每份字典各填各的语言（禁止读 activeLocale，见 AccessAnarchy 教训）。
	/// </summary>
	internal static class LocaleTable
	{
		public const string kScopeGroup = "scope.group";
		public const string kScopeMenu = "scope.menu";
		public const string kScopeCategory = "scope.category";
		public const string kScopeGlobalShared = "scope.globalShared";
		public const string kScopeGlobalUnique = "scope.globalUnique";

		public const string kTagVanilla = "tag.vanilla";
		public const string kTagRecommended = "tag.recommended";
		public const string kTagRecVanilla = "tag.recVanilla";
		public const string kTagNone = "tag.none";

		private static readonly string[] kLocales =
		{
			"de-DE", "en-US", "es-ES", "fr-FR", "it-IT", "ja-JP",
			"ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-HANS", "zh-HANT"
		};

		public static string[] Locales { get { return kLocales; } }

		private static string _activeLocale = "en-US";

		public static void SetActiveLocale(string locale)
		{
			for (int i = 0; i < kLocales.Length; i++)
			{
				if (string.Equals(kLocales[i], locale, System.StringComparison.OrdinalIgnoreCase))
				{
					_activeLocale = kLocales[i];
					return;
				}
			}
			_activeLocale = "en-US";
		}

		public static string T(string key)
		{
			Dictionary<string, string> table = Build(_activeLocale);
			string value;
			if (table.TryGetValue(key, out value)) return value;
			return Build("en-US")[key];
		}

		/// <summary>范围下拉：按工具项标注（原版）（推荐）（推荐原版）。</summary>
		public static DropdownItem<int>[] BuildScopeDropdown(ToolItemDef def)
		{
			Dictionary<string, string> d = Build(_activeLocale);
			return new DropdownItem<int>[]
			{
				Item(0, kScopeGroup, def, d),
				Item(1, kScopeMenu, def, d),
				Item(2, kScopeCategory, def, d),
				Item(3, kScopeGlobalShared, def, d),
				Item(4, kScopeGlobalUnique, def, d),
			};
		}

		private static DropdownItem<int> Item(int value, string baseKey, ToolItemDef def, Dictionary<string, string> d)
		{
			string name = d[baseKey];
			int van = def.VanillaScope;
			int rec = def.RecommendedScope;
			bool isVan = van == value;
			bool isRec = rec == value;
			string tag;
			if (isVan && isRec) tag = d[kTagRecVanilla];
			else if (isVan) tag = d[kTagVanilla];
			else if (isRec) tag = d[kTagRecommended];
			else tag = d[kTagNone];
			return new DropdownItem<int> { value = value, displayName = name + tag };
		}

		public static Dictionary<string, string> BuildEntries(Setting setting, string locale)
		{
			Dictionary<string, string> d = Build(locale);
			PatchAbout(d, locale);
			Dictionary<string, string> o = new Dictionary<string, string>();
			o[setting.GetSettingsLocaleID()] = d["mod.name"];
			o[setting.GetOptionTabLocaleID(Setting.kTabMod)] = d["tab.mod"];
			o[setting.GetOptionTabLocaleID(Setting.kTabAbout)] = d["tab.about"];
			o[setting.GetOptionGroupLocaleID(Setting.kGroupMain)] = d["group.main"];
			o[setting.GetOptionGroupLocaleID(Setting.kGroupNet)] = d["group.net"];
			o[setting.GetOptionGroupLocaleID(Setting.kGroupObj)] = d["group.obj"];
			o[setting.GetOptionGroupLocaleID(Setting.kGroupZone)] = d["group.zone"];
			o[setting.GetOptionGroupLocaleID(Setting.kGroupReset)] = d["group.reset"];
			o[setting.GetOptionLabelLocaleID("Enabled")] = d["enabled.label"];
			o[setting.GetOptionDescLocaleID("Enabled")] = d["enabled.desc"];

			o[setting.GetOptionLabelLocaleID("ModVersion")] = d["about.version"];
			o[setting.GetOptionLabelLocaleID("ModAuthor")] = d["about.author"];
			o[setting.GetOptionLabelLocaleID("OpenKofi")] = d["about.kofi"];
			o[setting.GetOptionDescLocaleID("OpenKofi")] = d["about.kofi.desc"];
			o[setting.GetOptionLabelLocaleID("OpenForum")] = d["about.forum"];
			o[setting.GetOptionDescLocaleID("OpenForum")] = d["about.forum.desc"];
			o[setting.GetOptionLabelLocaleID("OpenRainbowSite")] = d["about.rainbow"];
			o[setting.GetOptionDescLocaleID("OpenRainbowSite")] = d["about.rainbow.desc"];

			AddItem(o, setting, "NetDrawEnabled", "NetDrawScope", d, "item.net.draw");
			AddItem(o, setting, "NetSnapEnabled", "NetSnapScope", d, "item.net.snap");
			AddItem(o, setting, "NetParallelEnabled", "NetParallelScope", d, "item.net.parallel");
			AddItem(o, setting, "NetUndergroundEnabled", "NetUndergroundScope", d, "item.net.underground");
			AddItem(o, setting, "NetElevationEnabled", "NetElevationScope", d, "item.net.elevation");
			AddItem(o, setting, "ObjPlaceEnabled", "ObjPlaceScope", d, "item.obj.place");
			AddItem(o, setting, "ObjAlignEnabled", "ObjAlignScope", d, "item.obj.align");
			AddItem(o, setting, "ObjUndergroundEnabled", "ObjUndergroundScope", d, "item.obj.underground");
			AddItem(o, setting, "ZoneModeEnabled", "ZoneModeScope", d, "item.zone.mode");
			AddItem(o, setting, "AreaModeEnabled", "AreaModeScope", d, "item.area.mode");
			AddItem(o, setting, "WaterModeEnabled", "WaterModeScope", d, "item.water.mode");
			AddItem(o, setting, "TerrainModeEnabled", "TerrainModeScope", d, "item.terrain.mode");
			AddItem(o, setting, "BulldozeModeEnabled", "BulldozeModeScope", d, "item.bulldoze.mode");
			AddItem(o, setting, "UpgradeModeEnabled", "UpgradeModeScope", d, "item.upgrade.mode");

			o[setting.GetOptionLabelLocaleID("ResetMemory")] = d["reset.label"];
			o[setting.GetOptionDescLocaleID("ResetMemory")] = d["reset.desc"];
			o[setting.GetOptionWarningLocaleID("ResetMemory")] = d["reset.warn"];
			o["Options.WARNING[CONFIRM_RESET]"] = d["reset.confirm"];
			o[setting.GetOptionLabelLocaleID("OpenMemoryFolder")] = d["folder.label"];
			o[setting.GetOptionDescLocaleID("OpenMemoryFolder")] = d["folder.desc"];
			return o;
		}

		private static void AddItem(Dictionary<string, string> o, Setting s, string en, string sc, Dictionary<string, string> d, string key)
		{
			o[s.GetOptionLabelLocaleID(en)] = d[key + ".label"];
			o[s.GetOptionDescLocaleID(en)] = d[key + ".desc"];
			o[s.GetOptionLabelLocaleID(sc)] = d["scope.label"];
			o[s.GetOptionDescLocaleID(sc)] = d["scope.desc"];
		}

		// 兼容 Build(Setting,locale) 入口
		public static Dictionary<string, string> BuildForSource(Setting setting, string locale)
		{
			return BuildEntries(setting, locale);
		}

		/// <summary>About 页专属文案（版本/作者/链接）。</summary>
		private static void PatchAbout(Dictionary<string, string> d, string locale)
		{
			switch (locale)
			{
				case "zh-HANS":
					d["tab.about"] = "关于";
					d["about.version"] = "模组版本";
					d["about.author"] = "作者";
					d["about.kofi"] = "请我喝杯咖啡";
					d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
					d["about.forum"] = "论坛页面";
					d["about.forum.desc"] = "打开 Paradox 论坛帖子。";
					d["about.rainbow"] = "RAINBOW官网";
					d["about.rainbow.desc"] = "打开 RAINBOW 系列官网。";
					break;
				case "zh-HANT":
					d["tab.about"] = "關於";
					d["about.version"] = "模組版本";
					d["about.author"] = "作者";
					d["about.kofi"] = "請我喝杯咖啡";
					d["about.kofi.desc"] = "在 Ko-fi 上支持作者。";
					d["about.forum"] = "論壇頁面";
					d["about.forum.desc"] = "開啟 Paradox 論壇貼文。";
					d["about.rainbow"] = "RAINBOW官網";
					d["about.rainbow.desc"] = "開啟 RAINBOW 系列官網。";
					break;
				case "de-DE":
					d["tab.about"] = "Über";
					d["about.version"] = "Mod-Version";
					d["about.author"] = "Autor";
					d["about.kofi"] = "Kauf mir einen Kaffee";
					d["about.forum"] = "Forenseite";
					d["about.rainbow"] = "RAINBOW-Website";
					break;
				case "ja-JP":
					d["tab.about"] = "情報";
					d["about.version"] = "Mod バージョン";
					d["about.author"] = "作者";
					d["about.kofi"] = "コーヒーをおごる";
					d["about.forum"] = "フォーラムページ";
					d["about.rainbow"] = "RAINBOW 公式サイト";
					break;
				case "ko-KR":
					d["tab.about"] = "정보";
					d["about.version"] = "Mod 버전";
					d["about.author"] = "저자";
					d["about.kofi"] = "커피 한 잔 사주기";
					d["about.forum"] = "포럼 페이지";
					d["about.rainbow"] = "RAINBOW 웹사이트";
					break;
				default:
					d["tab.about"] = "About";
					d["about.version"] = "Mod version";
					d["about.author"] = "Author";
					d["about.kofi"] = "Buy me a coffee";
					d["about.forum"] = "Forum page";
					d["about.rainbow"] = "RAINBOW website";
					break;
			}
		}

		private static Dictionary<string, string> Build(string locale)
		{
			switch (locale)
			{
				case "de-DE": return De();
				case "es-ES": return Es();
				case "fr-FR": return Fr();
				case "it-IT": return It();
				case "ja-JP": return Ja();
				case "ko-KR": return Ko();
				case "pl-PL": return Pl();
				case "pt-BR": return Pt();
				case "ru-RU": return Ru();
				case "zh-HANS": return ZhHans();
				case "zh-HANT": return ZhHant();
				default: return En();
			}
		}

		private static Dictionary<string, string> Base(
			string modName, string tabMod, string tabManage,
			string gMain, string gNet, string gObj, string gZone, string gReset,
			string enabledLabel, string enabledDesc,
			string scopeLabel, string scopeDesc,
			string g, string m, string c, string gs, string gu,
			string tVan, string tRec, string tRecVan, string tNone,
			string resetL, string resetD, string resetW, string resetC,
			string folderL, string folderD,
			params string[] itemsLabelDesc) // 28 strings = 14*2
		{
			Dictionary<string, string> d = new Dictionary<string, string>();
			d["mod.name"] = modName;
			d["tab.mod"] = tabMod;
			d["tab.about"] = tabManage;
			d["group.main"] = gMain;
			d["group.net"] = gNet;
			d["group.obj"] = gObj;
			d["group.zone"] = gZone;
			d["group.reset"] = gReset;
			d["enabled.label"] = enabledLabel;
			d["enabled.desc"] = enabledDesc;
			d["scope.label"] = scopeLabel;
			d["scope.desc"] = scopeDesc;
			d[kScopeGroup] = g;
			d[kScopeMenu] = m;
			d[kScopeCategory] = c;
			d[kScopeGlobalShared] = gs;
			d[kScopeGlobalUnique] = gu;
			d[kTagVanilla] = tVan;
			d[kTagRecommended] = tRec;
			d[kTagRecVanilla] = tRecVan;
			d[kTagNone] = tNone;
			d["reset.label"] = resetL;
			d["reset.desc"] = resetD;
			d["reset.warn"] = resetW;
			d["reset.confirm"] = resetC;
			d["folder.label"] = folderL;
			d["folder.desc"] = folderD;
			// About 页文案（各语言 Base 调用末尾由 patchAbout 注入）
			d["about.version"] = "Mod version";
			d["about.author"] = "Author";
			d["about.kofi"] = "Buy me a coffee";
			d["about.kofi.desc"] = "Support the author on Ko-fi.";
			d["about.forum"] = "Forum page";
			d["about.forum.desc"] = "Open the Paradox forum thread.";
			d["about.rainbow"] = "RAINBOW website";
			d["about.rainbow.desc"] = "Open the Rainbow Series site.";
			string[] keys =
			{
				"item.net.draw","item.net.snap","item.net.parallel","item.net.underground","item.net.elevation",
				"item.obj.place","item.obj.align","item.obj.underground",
				"item.zone.mode","item.area.mode","item.water.mode","item.terrain.mode","item.bulldoze.mode",
				"item.upgrade.mode"
			};
			for (int i = 0; i < keys.Length; i++)
			{
				d[keys[i] + ".label"] = itemsLabelDesc[i * 2];
				d[keys[i] + ".desc"] = itemsLabelDesc[i * 2 + 1];
			}
			return d;
		}

		private static Dictionary<string, string> En()
		{
			return Base(
				"Tool Mode Memory", "Mod settings", "Memory management",
				"General", "Roads, tracks, pipes", "Buildings, props, trees", "Zones, water, terrain", "Memory files",
				"Enable Tool Mode Memory",
				"Remember tool panel settings per savegame and restore them when you return. Turn off to use 100% vanilla behaviour (settings reset every time you enter a save).",
				"Sharing scope", "How widely this item's setting is shared among assets. Parentheses show what vanilla does and what we recommend.",
				"Same group (e.g. small roads)", "Same menu (roads, education...)", "Same asset type (alley, metro track...)", "Shared globally", "Per asset (not shared)",
				" (vanilla)", " (recommended)", " (recommended, vanilla)", "",
				"Reset memory",
				"In a save: wipe this save's memory and restore vanilla tool state. In the main menu: wipe memory for every save.",
				"This cannot be undone.",
				"Reset all remembered tool settings?",
				"Open memory folder",
				"Open the local folder where per-save memory files are stored (one JSON per save, named after the save).",
				"Draw mode", "Straight / curves / continuous / grid / replace for roads, tracks and pipes.",
				"Snapping", "Snap options (geometry, cell length, guidelines...).",
				"Parallel roads", "Parallel offset count and spacing when placing multiple roads at once.",
				"Underground / elevated", "Tunnel vs elevated placement for nets that support it.",
				"Height", "Road / track height when placing. Change height to build tunnels or elevated roads. Vanilla keeps one value for the whole session.",
				"Place mode", "Create / upgrade / move / brush / stamp / line / curve for buildings, props and trees.",
				"Align & snap", "Alignment and snapping behaviour for objects (this is the align tool row).",
				"Underground place", "Place pipes and underground objects below ground.",
				"Zone draw mode", "Flood fill / marquee / paint for districts and industry areas.",
				"Surface area mode", "Edit / generate for surface areas such as landfills.",
				"Water tool mode", "Add source / edit modes of the water tool.",
				"Terrain tool mode", "Brush modes of the terrain tool.",
				"Bulldoze mode", "Main elements / sub elements / everything.",
				"Upgrade mode", "Road upgrade / rebuild behaviour.");
		}

		private static Dictionary<string, string> ZhHans()
		{
			return Base(
				"Tool Mode Memory", "模组设置", "关于",
				"常规", "道路、轨道、管道", "建筑、装饰、树木", "功能区、水源、地形", "记忆文件",
				"启用工具模式记忆",
				"记住各工具选项中的设置，并在下次进入该存档时自动恢复。关闭后完全保持原版行为（每次进档设置都会清空）。",
				"共用范围", "这项设置在多大范围内共享。括号内标明哪一项是原版、哪一项是推荐。",
				"同组（例如小型道路）", "同菜单（道路、教育…）", "同类资产（小巷、地铁轨道…）", "全局共用", "全局不共用（每资产独立）",
				"（原版）", "（推荐）", "（推荐原版）", "",
				"重置记忆",
				"游戏中：清除当前存档记忆，工具恢复刚进档时的原版状态。主菜单：清除所有存档的记忆。",
				"此操作无法撤销。",
				"确定要重置已记忆的工具设置吗？",
				"打开记忆文件夹",
				"打开保存记忆的本地文件夹（每个存档一份 JSON，文件名即存档名）。",
				"绘制模式", "道路/轨道/管道的直线、简单曲线、复合曲线、连续、网格、替换。",
				"吸附", "吸附选项（已有几何、街区长度、参考线等）。",
				"平行道路", "一次建造多条平行道路时的数量与间距。",
				"地下/高架", "改变高度时建造隧道或高架道路的切换。",
				"高度", "建造道路/轨道时的高度。改变高度即可建造隧道或高架道路。原版整局共用一个值；本模组可按范围记忆。",
				"放置模式", "建筑/装饰/树木的创建、升级、移动、笔刷、图章、直线、曲线。",
				"吸附/对齐", "物体的吸附与对齐选项（工具选项中的吸附行）。",
				"地下放置", "将管道等放在地面以下。",
				"功能区模式", "市辖区/专门产业区的填充、滚动、刷涂。",
				"表面区域模式", "表面区域规划工具的编辑/生成。",
				"水源工具模式", "水源工具的添加/编辑模式。",
				"地形工具模式", "地形笔刷（笔刷粗细/强度）模式。",
				"推土机模式", "推土机：主物件/子物件/全部。",
				"升级/重建", "道路升级与重建行为。");
		}

		private static Dictionary<string, string> ZhHant()
		{
			return Base(
				"Tool Mode Memory", "模組設定", "關於",
				"常規", "道路、軌道、管道", "建築、裝飾、樹木", "功能區、水源、地形", "記憶檔案",
				"啟用工具模式記憶",
				"記住各工具選項中的設定，並在下次進入該存檔時自動恢復。關閉後完全保持原版行為（每次進檔設定都會清空）。",
				"共用範圍", "這項設定在多大範圍內共享。括號內標明哪一項是原版、哪一項是推薦。",
				"同組（例如小型道路）", "同選單（道路、教育…）", "同類資產（小巷、地鐵軌道…）", "全域共用", "全域不共用（每資產獨立）",
				"（原版）", "（推薦）", "（推薦原版）", "",
				"重置記憶",
				"遊戲中：清除目前存檔記憶，工具恢復剛進檔時的原版狀態。主選單：清除所有存檔的記憶。",
				"此操作無法復原。",
				"確定要重置已記憶的工具設定嗎？",
				"開啟記憶資料夾",
				"開啟保存記憶的本機資料夾（每個存檔一份 JSON，檔名即存檔名）。",
				"繪製模式", "道路/軌道/管道的直線、簡單曲線、複合曲線、連續、網格、替換。",
				"吸附", "吸附選項（既有幾何、街區長度、參考線等）。",
				"平行道路", "一次建造多條平行道路時的數量與間距。",
				"地下/高架", "改變高度時建造隧道或高架道路的切換。",
				"高度", "建造道路/軌道時的高度。改變高度即可建造隧道或高架道路。原版整局共用一個值；本模組可按範圍記憶。",
				"放置模式", "建築/裝飾/樹木的建立、升級、移動、筆刷、圖章、直線、曲線。",
				"吸附/對齊", "物體的吸附與對齊選項（工具選項中的吸附列）。",
				"地下放置", "將管道等放在地面以下。",
				"功能區模式", "行政區/特殊工業功能區域的填充、滾動、刷塗。",
				"表面區域模式", "地面區域劃分工具的編輯/生成。",
				"水源工具模式", "水源工具的新增/編輯模式。",
				"地形工具模式", "地形筆刷模式。",
				"推土機模式", "推土機：主物件/子物件/全部。",
				"升級/重建", "道路升級與重建行為。");
		}

		private static Dictionary<string, string> De()
		{
			return Base(
				"Tool Mode Memory", "Mod-Einstellungen", "Speicherverwaltung",
				"Allgemein", "Straßen, Gleise, Leitungen", "Gebäude, Objekte, Bäume", "Zonen, Wasser, Gelände", "Speicherdateien",
				"Tool Mode Memory aktivieren",
				"Merkt sich Werkzeugeinstellungen pro Spielstand und stellt sie beim nächsten Laden wieder her. Ausschalten = 100 % Originalverhalten.",
					"Gemeinsamkeitsbereich", "Wie breit diese Eingabe geteilt wird. Klammern zeigen Original und Empfehlung.",
				"Selbe Gruppe (z. B. kleine Straßen)", "Selbes Menü (Straßen, Bildung ...)", "Selber Asset-Typ (Gasse, U-Bahn-Gleis ...)", "Global geteilt", "Pro Asset (nicht geteilt)",
				" (Original)", " (empfohlen)", " (empfohlen, Original)", "",
				"Speicher zurücksetzen",
				"Im Spiel: Speicher dieses Spielstands löschen. Im Hauptmenü: Speicher aller Spielstände löschen.",
				"Kann nicht rückgängig gemacht werden.",
				"Gemerkte Werkzeugeinstellungen wirklich zurücksetzen?",
				"Speicherordner öffnen",
				"Öffnet den lokalen Ordner mit den JSON-Dateien (eine pro Spielstand).",
				"Zeichenmodus", "Gerade / Kurven / fortlaufend / Raster / Ersetzen für Straßen, Gleise und Leitungen.",
				"Einrasten", "Einrastoptionen (Geometrie, Zellenlänge, Hilfslinien ...).",
				"Parallelstraßen", "Anzahl und Abstand paralleler Straßen.",
				"Unterirdisch / erhöht", "Tunnel oder erhöhte Platzierung.",
				"Höhe", "Höhe beim Platzieren. Original teilt einen Wert pro Sitzung.",
				"Platziermodus", "Erstellen / Upgrade / Verschieben / Pinsel / Stempel / Linie / Kurve.",
				"Ausrichten & Einrasten", "Ausrichtung und Einrasten von Objekten (Ausrichten-Zeile).",
				"Unterirdisch platzieren", "Leitungen und Objekte unter der Erde platzieren.",
				"Zonenzeichnen", "Füllen / Auswahl / Pinsel für Bezirke und Industrie.",
				"Flächenmodus", "Bearbeiten / Erzeugen für Oberflächen (z. B. Deponie).",
				"Wasserwerkzeug", "Quelle hinzufügen / Bearbeiten.",
				"Geländewerkzeug", "Pinselmodi des Geländewerkzeugs.",
				"Abreißen", "Hauptelemente / Teilelemente / alles.",
				"Upgrade-Modus", "Straßen-Upgrade und Wiederaufbau.");
		}

		private static Dictionary<string, string> Es()
		{
			return Base(
				"Tool Mode Memory", "Ajustes del mod", "Gestión de memoria",
				"General", "Carriles, vías, tuberías", "Edificios, accesorios, árboles", "Zonas, agua, terreno", "Archivos de memoria",
				"Activar Tool Mode Memory",
				"Recuerda los ajustes del panel de herramientas por partida y los restaura al volver. Desactiva para comportamiento 100 % original.",
				"Ámbito compartido", "Cuánto se comparte este ajuste. Los paréntesis indican original y recomendado.",
				"Mismo grupo (p. ej. calles pequeñas)", "Mismo menú (carreteras, educación...)", "Mismo tipo de activo (callejón, metro...)", "Global", "Por activo (sin compartir)",
				" (original)", " (recomendado)", " (recomendado, original)", "",
				"Restablecer memoria",
				"En partida: borra la memoria de esta partida. En el menú principal: borra la de todas.",
				"No se puede deshacer.",
				"¿Restablecer los ajustes recordados?",
				"Abrir carpeta de memoria",
				"Abre la carpeta local con los JSON (uno por partida, con el nombre de la partida).",
				"Modo de dibujo", "Recto / curvas / continuo / cuadrícula / reemplazar.",
				"Ajuste", "Opciones de ajuste (geometría, longitud de celda, guías...).",
				"Carriles paralelos", "Cantidad y separación de vías paralelas.",
				"Subterráneo / elevado", "Túnel o elevado al colocar redes.",
				"Elevación", "Altura al colocar. En original un valor por sesión.",
				"Modo de colocación", "Crear / mejorar / mover / pincel / sello / línea / curva.",
				"Alineación y ajuste", "Alineación y ajuste de objetos (fila de alinear).",
				"Colocar bajo tierra", "Colocar tuberías y objetos subterráneos.",
				"Modo de zona", "Relleno / marquesina / pintar para distritos e industria.",
				"Modo de superficie", "Editar / generar superficies (vertederos).",
				"Modo de agua", "Añadir fuente / editar.",
				"Modo de terreno", "Modos de pincel del terreno.",
				"Modo de demolición", "Elementos principales / subelementos / todo.",
				"Modo de mejora", "Mejora y reconstrucción de carreteras.");
		}

		private static Dictionary<string, string> Fr()
		{
			return Base(
				"Tool Mode Memory", "Réglages du mod", "Gestion de la mémoire",
				"Général", "Routes, rails, canalisations", "Bâtiments, accessoires, arbres", "Zones, eau, terrain", "Fichiers mémoire",
				"Activer Tool Mode Memory",
				"Memorise les réglages du panneau d'outils par partie et les restaure au retour. Désactivez pour le comportement 100 % d'origine.",
				"Portée de partage", "À quel point ce réglage est partagé. Parenthèses = d'origine / recommandé.",
				"Même groupe (ex. petites routes)", "Même menu (routes, éducation...)", "Même type d'actif (ruelle, métro...)", "Global", "Par actif (non partagé)",
				" (d'origine)", " (recommandé)", " (recommandé, d'origine)", "",
				"Réinitialiser la mémoire",
				"En jeu : efface la mémoire de cette partie. Au menu principal : efface toutes les parties.",
				"Irréversible.",
				"Réinitialiser les réglages mémorisés ?",
				"Ouvrir le dossier mémoire",
				"Ouvre le dossier local des JSON (un par partie, nommé comme la partie).",
				"Mode de tracé", "Droit / courbes / continu / grille / remplacer.",
				"Accrochage", "Options d'accrochage (géométrie, longueur de cellule, guides...).",
				"Routes parallèles", "Nombre et espacement des routes parallèles.",
				"Souterrain / élevé", "Tunnel ou élevé pour les réseaux concernés.",
				"Altitude", "Hauteur au placement. En original une valeur par session.",
				"Mode de placement", "Créer / améliorer / déplacer / pinceau / tampon / ligne / courbe.",
				"Alignement et accrochage", "Alignement et accrochage des objets (ligne d'alignement).",
				"Placement souterrain", "Placer canalisations et objets sous terre.",
				"Mode de zone", "Remplissage / sélection / peinture pour districts et industrie.",
				"Mode de surface", "Éditer / générer des surfaces (décharges).",
				"Mode eau", "Ajouter source / éditer.",
				"Mode terrain", "Modes du pinceau de terrain.",
				"Mode démolition", "Éléments principaux / sous-éléments / tout.",
				"Mode amélioration", "Amélioration et reconstruction des routes.");
		}

		private static Dictionary<string, string> It()
		{
			return Base(
				"Tool Mode Memory", "Impostazioni mod", "Gestione memoria",
				"Generale", "Strade, binari, tubi", "Edifici, oggetti, alberi", "Zone, acqua, terreno", "File di memoria",
				"Abilita Tool Mode Memory",
				"Ricorda le impostazioni del pannello strumenti per partita e le ripristina al ritorno. Disattiva per il comportamento 100% originale.",
				"Condivisione", "Quanto è condivisa questa impostazione. Parentesi = originale / consigliato.",
				"Stesso gruppo (es. piccole strade)", "Stesso menu (strade, istruzione...)", "Stesso tipo di asset (vicolo, metropolitana...)", "Globale", "Per asset (non condiviso)",
				" (originale)", " (consigliato)", " (consigliato, originale)", "",
				"Reimposta memoria",
				"In partita: cancella la memoria di questa partita. Nel menu principale: cancella tutte le partite.",
				"Non annullabile.",
				"Reimpostare le impostazioni memorizzate?",
				"Apri cartella memoria",
				"Apre la cartella locale dei JSON (uno per partita, con il nome della partita).",
				"Modalità disegno", "Retto / curve / continuo / griglia / sostituisci.",
				"Aggancio", "Opzioni di aggancio (geometria, lunghezza cella, guide...).",
				"Strade parallele", "Numero e spaziatura di strade parallele.",
				"Sotterraneo / sopraelevato", "Galleria o sopraelevata al piazzamento.",
				"Quota", "Altezza al piazzamento. In originale un valore per sessione.",
				"Modalità piazzamento", "Crea / potenzia / sposta / pennello / timbro / linea / curva.",
				"Allinea e aggancia", "Allineamento e aggancio degli oggetti (riga allinea).",
				"Piazzamento sotterraneo", "Piazza tubi e oggetti sottoterra.",
				"Modalità zona", "Riempi / seleziona / pinta per distretti e industria.",
				"Modalità superficie", "Modifica / genera superfici (discariche).",
				"Modalità acqua", "Aggiungi fonte / modifica.",
				"Modalità terreno", "Modalità pennello del terreno.",
				"Modalità demolizione", "Elementi principali / secondari / tutto.",
				"Modalità potenziamento", "Potenziamento e ricostruzione strade.");
		}

		private static Dictionary<string, string> Ja()
		{
			return Base(
				"Tool Mode Memory", "Mod 設定", "メモリ管理",
				"一般", "道路・軌道・パイプ", "建築物・小物・樹木", "ゾーン・水・地形", "メモリファイル",
				"Tool Mode Memory を有効化",
				"ツールパネルの設定をセーブごとに記憶し、次回ロード時に復元します。オフで完全に標準動作（毎回リセット）。",
				"共有範囲", "この設定の共有範囲。括弧内は標準／推奨を示します。",
				"同じグループ（例: 小型道路）", "同じメニュー（道路、教育…）", "同じアセット種別（路地、地下鉄…）", "全体で共有", "アセットごと（共有しない）",
				"（標準）", "（推奨）", "（推奨・標準）", "",
				"メモリをリセット",
				"ゲーム中: このセーブの記憶を消去。メインメニュー: 全セーブの記憶を消去。",
				"元に戻せません。",
				"記憶したツール設定をリセットしますか？",
				"メモリフォルダを開く",
				"セーブごとの JSON を保存するローカルフォルダを開きます。",
				"描画モード", "道路・軌道・パイプの直線/曲線/連続/グリッド/置換。",
				"スナップ", "スナップオプション（形状、セル長、ガイド線など）。",
				"平行道路", "平行道路の本数と間隔。",
				"地下/高架", "トンネルまたは高架の配置切替。",
				"高さ", "配置時の高さ。標準はセッション中共通。",
				"配置モード", "作成/アップグレード/移動/ブラシ/スタンプ/直線/曲線。",
				"整列とスナップ", "オブジェクトの整列とスナップ（整列ツール行）。",
				"地下に配置", "パイプや地下オブジェクトを地下に配置。",
				"ゾーン描画モード", "地区・工業地域の塗り/選択/ペン。",
				"サーフェスモード", "処分場などのサーフェス編集/生成。",
				"水ツール", "水源の追加/編集。",
				"地形ツール", "地形ブラシのモード。",
				"解体モード", "主要/サブ/すべて。",
				"アップグレードモード", "道路のアップグレードと再建。");
		}

		private static Dictionary<string, string> Ko()
		{
			return Base(
				"Tool Mode Memory", "모드 설정", "메모리 관리",
				"일반", "도로, 철도, 배관", "건물, 소품, 나무", "지역, 물, 지형", "메모리 파일",
				"Tool Mode Memory 사용",
				"도구 패널 설정을 세이브마다 기억하고 다음에 불러올 때 복원합니다. 끄면 100% 원작 동작.",
				"공유 범위", "이 설정의 공유 폭. 괄호는 원작/권장 표시입니다.",
				"같은 그룹(예: 소형 도로)", "같은 메뉴(도로, 교육…)", "같은 자산 유형(골목, 지하철…)", "전역 공유", "자산별(공유 안 함)",
				" (원작)", " (권장)", " (권장, 원작)", "",
				"메모리 초기화",
				"게임 중: 현재 세이브 기억 삭제. 메인 메뉴: 모든 세이브 기억 삭제.",
				"되돌릴 수 없습니다.",
				"기억된 도구 설정을 초기화할까요?",
				"메모리 폴더 열기",
				"세이브별 JSON이 있는 로컬 폴더를 엽니다.",
				"그리기 모드", "직선/곡선/연속/격자/바꾸기.",
				"스냅", "스냅 옵션(지오메트리, 셀 길이, 가이드선 등).",
				"평행 도로", "평행 도로 개수와 간격.",
				"지하/고가", "터널 또는 고가 배치.",
				"높이", "배치 높이. 원작은 세션 공통 값.",
				"배치 모드", "생성/업그레이드/이동/브러시/스탬프/직선/곡선.",
				"정렬과 스냅", "객체 정렬 및 스냅(정렬 도구 행).",
				"지하 배치", "배관과 지하 객체를 땅 아래에 배치.",
				"지역 그리기 모드", "구역/산업 지역 채우기·선택·그리기.",
				"표면 모드", "매립장 등 표면 편집/생성.",
				"물 도구", "수원 추가/편집.",
				"지형 도구", "지형 브러시 모드.",
				"철거 모드", "주 요소/하위 요소/전체.",
				"업그레이드 모드", "도로 업그레이드와 재건.");
		}

		private static Dictionary<string, string> Pl()
		{
			return Base(
				"Tool Mode Memory", "Ustawienia modu", "Zarządzanie pamięcią",
				"Ogólne", "Drogi, tory, rury", "Budynki, obiekty, drzewa", "Strefy, woda, teren", "Pliki pamięci",
				"Włącz Tool Mode Memory",
				"Zapamiętuje ustawienia panelu narzędzi per zapis i przywraca je przy powrocie. Wyłącz = 100% zachowania gry.",
				"Zakres współdzielenia", "Jak szeroko to ustawienie jest współdzielone. W nawiasach: oryginał / zalecane.",
				"Ta sama grupa (np. małe drogi)", "To samo menu (drogi, edukacja...)", "Ten sam typ zasobu (alejka, metro...)", "Globalnie", "Per zasób (bez współdzielenia)",
				" (oryginał)", " (zalecane)", " (zalecane, oryginał)", "",
				"Resetuj pamięć",
				"W grze: kasuje pamięć tego zapisu. W menu głównym: kasuje pamięć wszystkich zapisów.",
				"Nie można cofnąć.",
				"Zresetować zapamiętane ustawienia narzędzi?",
				"Otwórz folder pamięci",
				"Otwiera lokalny folder z plikami JSON (jeden na zapis).",
				"Tryb rysowania", "Proste / krzywe / ciągłe / siatka / zamiana.",
				"Przyciąganie", "Opcje przyciągania (geometria, długość komórki, linie...).",
				"Równoległe drogi", "Liczba i odstęp równoległych dróg.",
				"Pod ziemią / estakada", "Tunel lub estakada przy stawianiu.",
				"Wysokość", "Wysokość przy stawianiu. W oryginale jedna wartość na sesję.",
				"Tryb stawiania", "Twórz / ulepsz / przenieś / pędzel / stempel / linia / krzywa.",
				"Wyrównanie i przyciąganie", "Wyrównanie i przyciąganie obiektów (wiersz wyrównania).",
				"Stawianie pod ziemią", "Rury i obiekty podziemne pod ziemią.",
				"Tryb stref", "Wypełnienie / zaznaczenie / malowanie dzielnic i przemysłu.",
				"Tryb powierzchni", "Edytuj / generuj powierzchnie (wysypiska).",
				"Tryb wody", "Dodaj źródło / edytuj.",
				"Tryb terenu", "Tryby pędzla terenu.",
				"Tryb burzenia", "Elementy główne / podelementy / wszystko.",
				"Tryb ulepszania", "Ulepszanie i przebudowa dróg.");
		}

		private static Dictionary<string, string> Pt()
		{
			return Base(
				"Tool Mode Memory", "Configurações do mod", "Gestão de memória",
				"Geral", "Estradas, trilhos, canos", "Prédios, acessórios, árvores", "Zonas, água, terreno", "Arquivos de memória",
				"Activar Tool Mode Memory",
				"Lembra as definições do painel de ferramentas por jogo e restaura ao voltar. Desligue para 100% original.",
				"Âmbito de partilha", "Quão amplo é o partilhamento. Parênteses = original / recomendado.",
				"Mesmo grupo (ex. ruas pequenas)", "Mesmo menu (estradas, educação...)", "Mesmo tipo de ativo (beco, metro...)", "Global", "Por ativo (não partilhado)",
				" (original)", " (recomendado)", " (recomendado, original)", "",
				"Repor memória",
				"No jogo: apaga a memória deste jogo. No menu principal: apaga todos os jogos.",
				"Não pode ser desfeito.",
				"Repor as definições memorizadas?",
				"Abrir pasta de memória",
				"Abre a pasta local com os JSON (um por jogo, com o nome do jogo).",
				"Modo de desenho", "Reto / curvas / contínuo / grelha / substituir.",
				"Encaixe", "Opções de encaixe (geometria, comprimento da célula, guias...).",
				"Estradas paralelas", "Número e espaçamento de estradas paralelas.",
				"Subterrâneo / elevado", "Túnel ou elevado ao colocar.",
				"Elevação", "Altura ao colocar. No original um valor por sessão.",
				"Modo de colocação", "Criar / melhorar / mover / pincel / carimbo / linha / curva.",
				"Alinhar e encaixar", "Alinhamento e encaixe de objetos (linha alinhar).",
				"Colocação subterrânea", "Colocar canos e objetos subterrâneos.",
				"Modo de zona", "Preencher / seleção / pintar para distritos e indústria.",
				"Modo de superfície", "Editar / gerar superfícies (aterros).",
				"Modo de água", "Adicionar fonte / editar.",
				"Modo de terreno", "Modos do pincel de terreno.",
				"Modo de demolição", "Elementos principais / subelementos / tudo.",
				"Modo de melhoria", "Melhoria e reconstrução de estradas.");
		}

		private static Dictionary<string, string> Ru()
		{
			return Base(
				"Tool Mode Memory", "Настройки мода", "Управление памятью",
				"Общие", "Дороги, рельсы, трубы", "Здания, объекты, деревья", "Зоны, вода, рельеф", "Файлы памяти",
				"Включить Tool Mode Memory",
				"Запоминает настройки панели инструментов для каждого сохранения и восстанавливает их. Выкл — поведение на 100% как в игре.",
				"Область общего доступа", "Насколько широко разделяется настройка. В скобках: оригинал / рекомендация.",
				"Та же группа (напр. малые дороги)", "То же меню (дороги, образование...)", "Тот же тип объекта (переулок, метро...)", "Глобально", "На каждый объект",
				" (оригинал)", " (рекомендуется)", " (рекомендуется, оригинал)", "",
				"Сбросить память",
				"В игре: стереть память этого сохранения. В главном меню: стереть все сохранения.",
				"Отменить нельзя.",
				"Сбросить запомненные настройки?",
				"Открыть папку памяти",
				"Открывает локальную папку с JSON (один файл на сохранение).",
				"Режим рисования", "Прямая / кривые / непрерывно / сетка / замена.",
				"Привязка", "Параметры привязки (геометрия, длина ячейки, направляющие...).",
				"Параллельные дороги", "Число и шаг параллельных дорог.",
				"Под землёй / эстакада", "Тоннель или эстакада при размещении.",
				"Высота", "Высота при размещении. В оригинале одно значение на сессию.",
				"Режим размещения", "Создать / улучшить / переместить / кисть / штамп / линия / кривая.",
				"Выравнивание и привязка", "Выравнивание и привязка объектов (строка выравнивания).",
				"Подземное размещение", "Трубы и подземные объекты под землёй.",
				"Режим зон", "Заливка / рамка / кисть для районов и промышленности.",
				"Режим поверхности", "Правка / генерация поверхностей (свалки).",
				"Режим воды", "Добавить источник / правка.",
				"Режим рельефа", "Режимы кисти рельефа.",
				"Режим сноса", "Основные / подэлементы / всё.",
				"Режим улучшения", "Улучшение и перестройка дорог.");
		}
	}

	internal class LocaleSource : IDictionarySource
	{
		private readonly Setting m_Setting;
		private readonly Dictionary<string, string> m_Entries;

		public LocaleSource(Setting setting, string locale)
		{
			m_Setting = setting;
			m_Entries = LocaleTable.BuildEntries(setting, locale);
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return m_Entries;
		}

		public void Unload()
		{
		}
	}
}
