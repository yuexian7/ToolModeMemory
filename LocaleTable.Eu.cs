using System;
using System.Collections.Generic;

namespace ToolModeMemory
{
	/// <summary>
	/// de-DE / es-ES / fr-FR / it-IT 文案。从 LocaleTable.cs 拆出，便于分工维护；结构与其它语言一致：Frames() 之后再补本语言独有的键。
	/// v0.2.1：页面底部的「共用范围说明」板块取消，五行定义并进每一个工具项的 scope 说明，
	/// 例子改由 item.&lt;id&gt;.ex.group / .menu / .category 三个槽位按该项自己的数值填入。
	/// 游戏内名词一律取官方语言包（research/locale/all_locales.json）与 Anarchy 自带语言包。
	/// </summary>
	internal static partial class LocaleTable
	{
		// ======================= de-DE =======================

		private static Dictionary<string, string> De()
		{
			Dictionary<string, string> d = Frames("de-DE");
			d["mod.name"] = "Werkzeugmodus-Gedächtnis";
			d["tab.mod"] = "Werkzeugmodus-Einstellungen";
			d["tab.about"] = "Über";
			d["group.master"] = "Werkzeugmodus-Gedächtnis";
			d["group.official"] = "Einstellungen der offiziellen Werkzeuge";
			d["group.anarchy"] = "Einstellungen der Anarchy-Werkzeuge";
			d["group.reset"] = "Speicherverwaltung";
			d["group.about"] = "Informationen und Links";

			d["scope.label"] = "Gemeinsamer Bereich";
			d["scope.desc"] = "Wie weit der Wert dieses Punkts geteilt wird. Die Klammern zeigen, was das Spiel von sich aus macht (Original).";
			d["scope.line.group"] = "Selbe Gruppe: Werkzeuge in derselben Gruppe der Menüleiste nutzen denselben Wert. {0}";
			d["scope.line.menu"] = "Selbes Menü: alle Werkzeuge eines Menüs nutzen denselben Wert. {0}";
			d["scope.line.category"] = "Selber Typ: Objekte werden danach gruppiert, wem sie dienen, und eine Gruppe nutzt denselben Wert. {0}";

			d[kScopeGroup] = "Selbe Gruppe";
			d[kScopeMenu] = "Selbes Menü";
			d[kScopeCategory] = "Selber Asset-Typ";
			d[kScopeGlobalShared] = "Global geteilt";
			d[kScopeGlobalUnique] = "Gar nicht geteilt";
			d[kTagVanilla] = " (Original)";
			d[kTagRecommended] = " (empfohlen)";
			d[kTagRecVanilla] = " (empfohlen, Original)";

			d["reset.label"] = "Speicher zurücksetzen";
			d["reset.desc"] = "Im Spiel: das Gedächtnis dieses Spielstands löschen und die Werkzeuge in den Originalzustand beim Laden versetzen. Im Hauptmenü oder Editor: das Gedächtnis aller Spielstände löschen.";
			d["reset.warn"] = "Das kann nicht rückgängig gemacht werden.";
			d["reset.confirm"] = "Alle gemerkten Werkzeugeinstellungen zurücksetzen?";
			d["resetall.label"] = "Alle Einstellungen zurücksetzen";
			d["resetall.desc"] = "Setzt jede Option dieses Mods auf den Standardwert zurück: der Hauptschalter wird wieder ausgeschaltet, die drei Seitenschalter wieder eingeschaltet, und Aktivierung und gemeinsamer Bereich jedes Punkts werden ebenfalls zurückgesetzt. Das bereits gespeicherte Gedächtnis der Spielstände bleibt unberührt.";
			d["resetall.warn"] = "Das kann nicht rückgängig gemacht werden.";
			d["resetall.confirm"] = "Wirklich alle Einstellungen zurücksetzen?";
			d["folder.label"] = "Gedächtnisdateien aller Spielstände verwalten";
			d["folder.desc"] = "Öffnet den lokalen Ordner mit den Gedächtnisdateien, eine pro Spielstand, und der Dateiname ist genau der Name des Spielstands, den du in der Liste „Spiel laden“ siehst. Das Gedächtnis wird immer nach dem Namen des Spielstands gelesen: eine Datei wird nur benutzt, wenn der Name des Spielstands und der Dateiname der json-Datei gleich sind, also benennst du die Datei um, um das Gedächtnis von einem Spielstand auf einen anderen zu kopieren. Wenn du eine automatische Speicherung lädst, wird ihr Gedächtnis trotzdem in die Datei des ursprünglichen Spielstands geschrieben und überschreibt sie, weil eine automatische Speicherung jedes Mal einen neuen Namen bekommt und kein Dateiname sein kann; nur bei einer automatischen Speicherung, deren ursprünglicher Spielstand nicht mehr feststellbar ist, wird das Gedächtnis vorläufig unter dem Stadtnamen geführt, und sobald du manuell speicherst, bekommt die Datei den richtigen Namen des Spielstands.";

			d["about.version"] = "Mod-Version";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Unterstütze den Autor auf Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Den Thread im Paradox-Forum öffnen.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Die Website der Rainbow-Reihe öffnen.";

			// ---------- Anarchy ----------
			d["item.anarchy.desc"] = "Merkt sich den Schalter des Anarchy-Mods.\nEr wird nur bei Werkzeugen gemerkt, die diesen Schalter wirklich haben.\nOhne den Anarchy-Mod wird dieser Punkt nicht angezeigt.";
			d["item.anarchy.ex.group"] = "zum Beispiel schaltest du Anarchy für eine zweispurige kleine Straße ein, andere kleine Straßen haben Anarchy ebenfalls an, eine große Straße dagegen nicht.";
			d["item.anarchy.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, hat Anarchy an, im Menü Strom dagegen nicht.";
			d["item.anarchy.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt Anarchy an, aber eine Brücke oder doppelte U-Bahn-Gleise gehen wieder aus und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			d["item.themes.label"] = "Thema";
			d["item.themes.desc"] = "Merkt, welche Einträge im Filter „Thema“ der Werkzeugleiste angehakt sind; ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die diese Themen unterstützen.\nDiese Zeile erscheint nur, wenn die aktuelle Kategorie wirklich Assets enthält, die Themen verwenden.";
			d["item.themes.ex.group"] = "zum Beispiel hakt du ein Thema nur unter Straßen/Kleine Straßen ein, eine große Straße filtert dann nicht danach.";
			d["item.themes.ex.menu"] = "ein im Menü Straßen angehaktes Thema gilt im Menü Strom nicht und ist wieder da, wenn du zurückkehrst.";
			d["item.themes.ex.category"] = "der Haken in dieser Zeile gehört zum Filter der Werkzeugleiste selbst, er wird also über das Menü und die Kategorie gemerkt, in denen du gerade bist, und nicht über den Asset-Typ.";
			d["item.packs.label"] = "Paket";
			d["item.packs.desc"] = "Merkt, welche Einträge im Filter „Paket“ der Werkzeugleiste angehakt sind; ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die zu diesen Paketen gehören.\nDas Original leert die Auswahl jedes Mal, wenn du das Menü oder die Kategorie wechselst.";
			d["item.packs.ex.group"] = "zum Beispiel hakt du ein Paket unter Straßen/Kleine Straßen ein, das Original leert es bei einer großen Straße, und es ist wieder da, wenn du zurückkehrst.";
			d["item.packs.ex.menu"] = "die im Menü Straßen angehakten Pakete und die im Menü Strom angehakten werden getrennt gemerkt.";
			d["item.packs.ex.category"] = "der Haken in dieser Zeile gehört zum Filter der Werkzeugleiste selbst, er wird über das Menü und die Kategorie gemerkt, in denen du gerade bist; das Original leert ihn jedes Mal, wenn du die Kategorie wechselst.";

			// ---------- Werkzeugmodus ----------
			d["item.toolMode.label"] = "Werkzeugmodus";
			d["item.toolMode.ex.group"] = "zum Beispiel wählst du Einfache Kurve für eine zweispurige kleine Straße, andere kleine Straßen nutzen ebenfalls Einfache Kurve, eine große Straße dagegen nicht.";
			d["item.toolMode.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, steht auf Einfache Kurve, im Menü Strom dagegen nicht.";
			d["item.toolMode.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es Einfache Kurve, aber eine Brücke oder doppelte U-Bahn-Gleise fallen auf ihren Standardmodus zurück und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Höhe ----------
			d["item.elevation.label"] = "Höhe";
			d["item.elevation.desc"] = "Merkt die Höhe, mit der ein Werkzeug verlassen wurde, einschließlich des Ergebnisses von Erhöhen und Absenken.\nDer Höhenunterschied (wie weit dich ein einzelner Druck versetzt; Anarchy nennt diese Zeile Höhenschritt) wird zusammen mit der Höhe gemerkt und nutzt denselben gemeinsamen Bereich.";
			d["item.elevation.ex.group"] = "zum Beispiel hebst du eine zweispurige kleine Straße auf 10 m, andere kleine Straßen gehen ebenfalls auf 10 m, eine große Straße aber nicht auf 10 m.";
			d["item.elevation.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, liegt auf 10 m, im Menü Strom dagegen nicht auf 10 m.";
			d["item.elevation.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es bei 10 m, aber eine Brücke oder doppelte Bahngleise gehen zurück auf 0 m und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Parallelmodus ----------
			d["item.parallel.label"] = "Parallelmodus";
			d["item.parallel.desc"] = "Merkt die beiden Einträge im Panel Parallelstraße: den Schalter Parallelmodus und den Parallelabstand.\nIst er an, wird neben der gezogenen Linie eine zweite gebaut; einstellen lässt sich nur der Abstand. Standardmäßig aus, also eine Linie wie im Original.";
			d["item.parallel.ex.group"] = "zum Beispiel schaltest du den Parallelmodus für eine zweispurige kleine Straße ein und stellst den Parallelabstand auf 5 m, andere kleine Straßen kommen ebenfalls eingeschaltet mit denselben 5 m zurück, eine große Straße dagegen nicht.";
			d["item.parallel.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, baut parallel mit 5 m Abstand, im Menü Strom dagegen nicht.";
			d["item.parallel.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt er an und der Abstand bleibt bei 5 m, aber eine Brücke oder ein U-Bahn-Gleis geht aus und braucht eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Einrasten ----------
			d["item.snap.label"] = "Einrasten";
			d["item.snap.desc"] = "Merkt jeden Schalter der Zeile Einrasten, etwa an Straßenkanten, in der Mitte einer Straße, an Knoten, an der Oberfläche eines Objekts oder an der Küste.\nWelche Schalter erscheinen, hängt vom Werkzeug ab; der Sammlerschalter für die ganze Zeile wird selbst nicht gemerkt.";
			d["item.snap.ex.group"] = "zum Beispiel schaltest du „An Straßenkanten einrasten“ für eine zweispurige kleine Straße aus, andere kleine Straßen haben es ebenfalls aus, eine große Straße hat dagegen alles an.";
			d["item.snap.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Einrast-Schaltern, im Menü Strom dagegen nicht.";
			d["item.snap.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt dieselbe Reihe erhalten, aber eine Brücke oder ein Spazierweg geht auf die Standardwerte zurück und braucht eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Topographie ----------
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Merkt die Zeile Topographie, also ob Nur Höhenlinien anzeigen aktiv ist.";
			d["item.topography.ex.group"] = "zum Beispiel schaltest du die Topographie für eine zweispurige kleine Straße ein, andere kleine Straßen haben sie ebenfalls an, eine große Straße dagegen nicht.";
			d["item.topography.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, zeigt Höhenlinien, im Menü Strom dagegen nicht.";
			d["item.topography.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleiben die Höhenlinien sichtbar, aber bei einer Brücke oder einer Stromleitung ist die Zeile aus und sie braucht eine eigene Einstellung.";


			// ---------- Links und Rechts (Anarchy) ----------
			d["item.leftRight.label"] = "Links und Rechts";
			d["item.leftRight.desc"] = "Merkt sich, welche Netzwerkerweiterungen auf den Zeilen „Links“ und „Rechts“ im Netzwerkpanel von Anarchy gewählt sind, zum Beispiel Fahrradweg, Rasen, Kai, Stützwand und Schallschutzmauer.\nBenötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod wird dieser Punkt nicht angezeigt.";
			d["item.leftRight.ex.group"] = "zum Beispiel wählst du auf der rechten Seite einer zweispurigen kleinen Straße den Fahrradweg, andere kleine Straßen haben rechts ebenfalls den Fahrradweg, eine große Straße dagegen nicht.";
			d["item.leftRight.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dasselbe Paar Seitenauswahlen, im Menü Strom dagegen nicht.";
			d["item.leftRight.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt rechts der Fahrradweg, aber bei einer Brücke oder doppelten Bahngleisen ist wieder nichts ausgewählt und sie brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Allgemein (Anarchy) ----------
			d["item.general.label"] = "Allgemein";
			d["item.general.desc"] = "Merkt sich die Schalter auf der Zeile „Allgemein“ im Netzwerkpanel von Anarchy: Boden, Erhöht, Tunnel, Konstante Neigung, Breiter Trennstreifen und Erweiterter Höhenbereich (setzt die Höhenbegrenzung außer Kraft).\nBenötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod wird dieser Punkt nicht angezeigt.";
			d["item.general.ex.group"] = "zum Beispiel wählst du Tunnel für eine zweispurige kleine Straße, andere kleine Straßen werden ebenfalls in Tunnel gezwungen, eine große Straße dagegen nicht.";
			d["item.general.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Allgemein-Schaltern, im Menü Strom dagegen nicht.";
			d["item.general.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es Tunnel, aber bei einer Brücke oder doppelten U-Bahn-Gleisen ist wieder nichts angehakt und sie brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Untergrundmodus ----------
			d["item.underground.label"] = "Untergrundmodus";
			d["item.underground.desc"] = "Merkt den Schalter Untergrundmodus: ob du etwas unter der Erde oder darüber platzierst.";
			d["item.underground.ex.group"] = "zum Beispiel schaltest du den Untergrundmodus für eine zweispurige kleine Straße ein, andere kleine Straßen gehen ebenfalls unter die Erde, eine große Straße dagegen nicht.";
			d["item.underground.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, wird unter der Erde gebaut, im Menü Strom dagegen nicht.";
			d["item.underground.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es unter der Erde, aber eine Brücke oder doppelte Bahngleise bleiben an der Oberfläche und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen.";

			// ---------- Sonstiges ----------

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（de-DE）====================
		d["tab.master"] = "Hauptschalter";
		d["tab.assets"] = "Assets";
		d["tab.functions"] = "Funktionen";
		d["tab.items"] = "Requisiten";
		d["group.domain"] = "Schalter pro Seite";
		d["enabled.label"] = "Mod aktivieren";
		d["enabled.desc"] = "Schaltet den ganzen Mod ein oder aus.\nIst er an, wirken die drei Schalter darunter: jede Werkzeugeinstellung wird beim Arbeiten gemerkt und beim Laden eines Spielstands zurückgegeben.\nIst er aus, hält sich der Mod ganz heraus und jedes Werkzeug verhält sich wie im Original.";
		d["domain.asset.label"] = "Werkzeugpunkte der Seite „Assets“ merken";
		d["domain.asset.desc"] = "Steuert, ob die Punkte dieser Seite gemerkt werden:\nAn - die Einstellungen werden beim Arbeiten aufgezeichnet und beim Laden eines Spielstands zurückgegeben;\nAus - nichts wird aufgezeichnet oder zurückgegeben, dann gibt es auch keinen gemeinsamen Bereich einzustellen.\nDiese Seite enthält Straßen, Bauwerke und städtische Dienste.";
		d["domain.function.label"] = "Werkzeugpunkte der Seite „Funktionen“ merken";
		d["domain.function.desc"] = "An: die Werkzeugeinstellungen dieser Seite werden laufend aufgezeichnet und kehren beim Laden des Spielstands zurück.\nAus: von dieser Seite wird nichts aufgezeichnet und nichts wiederhergestellt; die Seite wird ausgegraut und die Werkzeuge laufen wie im Original.";
		d["domain.item.label"] = "Werkzeugpunkte der Seite „Requisiten“ merken";
		d["domain.item.desc"] = "An: die Werkzeugeinstellungen dieser Seite werden laufend aufgezeichnet und kehren beim Laden des Spielstands zurück.\nAus: von dieser Seite wird nichts aufgezeichnet und nichts wiederhergestellt; die Seite wird ausgegraut und die Werkzeuge laufen wie im Original.";
		d["scope.category.function"] = "Selber Typ";
		d["scope.category.item"] = "Selbe Requisitenart";
		d["item.color.label"] = "Farbe";
		d["item.decoration.label"] = "Alter beibehalten";
		d["item.decoration.desc"] = "Merkt den Schalter „Alter beibehalten“: ist er an, wachsen die Bäume, die du anmalst, nicht weiter und sterben auch nicht mehr an Altersschwäche.\nDiese Zeile haben nur Bäume.";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Pinseltyp";
		d["item.brushType.desc"] = "Merkt, welcher Pinsel in der Zeile „Pinseltyp“ gewählt ist: Kreis, Quadrat, weiche Kante und so weiter – das bestimmt die Form der Kante, die du malst.\nDie Zeile gibt es nur bei Werkzeugen, die malen können: bei den neun Terraforming-Werkzeugen und beim Objektwerkzeug, wenn das gewählte Asset malbar ist.";
		d["item.brushSize.label"] = "Pinselgröße";
		d["item.brushSize.desc"] = "Merkt die Zeile „Pinselgröße“: wie weit ein Strich reicht.\nDie Zeile gibt es nur bei Werkzeugen, die malen können: bei den neun Terraforming-Werkzeugen und beim Objektwerkzeug, wenn das gewählte Asset malbar ist.";
		d["item.brushStrength.label"] = "Pinselstärke";
		d["item.brushStrength.desc"] = "Merkt die Zeile „Pinselstärke“: wie viel ein Strich verändert.\nDie Zeile gibt es nur bei Werkzeugen, die malen können: bei den neun Terraforming-Werkzeugen und beim Objektwerkzeug, wenn das gewählte Asset malbar ist.";
		d["item.brushHeight.label"] = "Zielhöhe";
		d["item.brushHeight.desc"] = "Merkt den Wert der Zeile „Zielhöhe“: auf welcher Höhe das Gelände am Ende steht, nachdem ein Terraforming-Werkzeug es eingeebnet oder zu einer Böschung gedrückt hat.\nDie Zeile gibt es nur bei den beiden Terraforming-Werkzeugen „Terrain angleichen“ und „Terrainsteigung“.";
		d["item.distance.label"] = "Distanz";
		d["item.distance.desc"] = "Merkt den Wert der Zeile „Distanz“: wie weit aufeinanderfolgende Objekte auseinanderliegen, wenn du sie entlang einer Linie oder einer Kurve setzt.\nDie Zeile erscheint nur im Linien- und im Kurvenmodus.";
		d["item.ageMask.label"] = "Alter";
		d["item.ageMask.desc"] = "Merkt, welche Altersstufen in der Zeile „Alter“ angehakt sind: Setzling, Jung, Ausgewachsen und Alt, auch mehrere gleichzeitig.\nDiese Zeile haben nur Bäume.";
		d["item.anarchyObjectElevation.label"] = "Objekthöhe";
		d["item.anarchyObjectElevation.desc"] = "Merkt die Reihe von Höhensteuerelementen, die Anarchy an das Panel des Objektwerkzeugs anhängt: Transformationsfixierung, Höhe, Höhenvarianz und Höhenschritt.\nAlle vier stehen in einer Reihe und zählen als ein Punkt.";
		d["item.f.snap.desc"] = "Merkt jeden Schalter der Zeile „Einrasten“ auf Funktionspanels: bei 90-Grad-Winkeln, an Zellenraster, an Straßen, an der Seite des Besitzers, nur Höhenlinien anzeigen und so weiter.\nWelche Schalter erscheinen, hängt vom Werkzeug ab; der Sammlerschalter für die ganze Liste wird selbst nicht gemerkt.";
		d["item.f.topography.desc"] = "Merkt den Schalter der Zeile „Topographie“ auf Funktionspanels, also ob du beim Ziehen von Zonen die Höhenlinien des Geländes siehst.";
		d["item.f.underground.desc"] = "Merkt den Schalter der Zeile „Untergrundmodus“ auf Funktionspanels.";
		d["item.i.snap.desc"] = "Merkt jeden Schalter der Zeile „Einrasten“ auf Requisitenpanels: an der Oberfläche eines Objekts, an nahegelegener Geometrie, an Straßen, nur entsprechenden Typ entfernen und so weiter, je nach Requisite.\nWelche Schalter erscheinen, hängt von der Requisite ab.";
		d["item.i.toolMode.desc"] = "Merkt die Auswahl der Zeile „Werkzeugmodus“ auf Requisitenpanels: Einzeln platzieren, Mehrfach platzieren, Linie, Kurve, Objektstempel-Werkzeug, Malen und so weiter, je nach Werkzeug.";
		d["item.i.topography.desc"] = "Merkt den Schalter der Zeile „Topographie“ auf Requisitenpanels.";
		d["item.i.underground.desc"] = "Merkt den Schalter der Zeile „Untergrundmodus“ auf Requisitenpanels, also ob du Requisiten unter- oder oberirdisch setzt.";
			d["ex.net.group"] = "Zum Beispiel nutzen alle Straßen der Gruppe Kleine Straßen denselben Wert; Große Straßen ist eine andere Gruppe.";
			d["ex.net.menu"] = "Zum Beispiel nutzen alle Straßen im Menü Straßen denselben Wert; im Strommenü gilt das nicht.";
			d["ex.net.category"] = "Zum Beispiel zählen Straßen mit beliebig vielen Spuren als ein Straßentyp; Brücken und Bahngleise sind eigene Typen.";
			d["ex.zone.group"] = "Zum Beispiel bilden Wohnungen niedriger, mittlerer und hoher Dichte in Wohnzonen eine Gruppe; Gewerbezonen ist eine andere Gruppe, obwohl beide im selben Menü liegen.";
			d["ex.zone.menu"] = "Zum Beispiel nutzen Wohnzonen, Gewerbezonen, Industriezonen und Bürozonen denselben Wert; ein anderes Menü nicht.";
			d["ex.route.group"] = "Zum Beispiel werden Linien nach Verkehrsmittel gruppiert (Straße, Straßenbahn, Zug, U-Bahn und so weiter); eine Linie eines anderen Verkehrsmittels ist eine andere Gruppe.";
			d["ex.route.menu"] = "Zum Beispiel nutzen alle Linien im Menü Transport denselben Wert; ein anderes Menü nicht.";
			d["ex.terrain.group"] = "Terraforming hat nur eine Gruppe, deshalb wählen „Selbe Gruppe“ und „Selbes Menü“ genau dieselben Werkzeuge.";
			d["ex.terrain.menu"] = "Terraforming hat nur eine Gruppe, deshalb wählen „Selbes Menü“ und „Selbe Gruppe“ genau dieselben Werkzeuge.";
			d["ex.prop.group"] = "Zum Beispiel teilt das Menü Landschaftsbau die Objekte in Natur, Park, Wohnen, Gewerbe, Industrie, Trümmer, Aufkleber und Pflanzgefäße; eine Kategorie nutzt denselben Wert, eine andere ist eine andere Gruppe.";
			d["ex.prop.menu"] = "Alle Objekte und Bäume im Menü Landschaftsbau nutzen denselben Wert.";
			d["ex.tree.group"] = "Alle Bäume liegen in der Gruppe Vegetation im Menü Landschaftsbau, deshalb wählen „Selbe Gruppe“ und „Selbes Menü“ dieselben Bäume.";
			d["ex.tree.menu"] = "Alle Bäume liegen in der Gruppe Vegetation im Menü Landschaftsbau, deshalb wählen „Selbes Menü“ und „Selbe Gruppe“ dieselben Bäume.";
			d["item.color.desc"] = "Merkt sich die Zeile „Farbe“ bei Linienwerkzeugen: die Farbe, in der eine Linie gezeichnet wird.";
			d["item.f.snap.desc"] = "Merkt sich alle Schalter der Zeile „Einrasten“ bei Funktionswerkzeugen; welche Schalter es gibt, hängt vom Werkzeug ab.\nDer Spieleigene Sammelschalter für alle Einrastpunkte wird nicht gemerkt.";
			d["item.f.topography.desc"] = "Merkt sich die Zeile „Topographie“ bei Zonen-, Terraforming- und anderen Funktionswerkzeugen: ob Geländelinien sichtbar sind.";
			d["item.f.underground.desc"] = "Merkt sich die Zeile „Untergrundmodus“ bei Gebiets-, Linien-, Abriss- und Objektwerkzeugen.";
			d["item.i.topography.desc"] = "Merkt sich die Zeile „Topographie“ bei Objektwerkzeugen.";
			d["item.anarchyObjectElevation.desc"] = "Merkt sich die Höhenregler, die Anarchy an die Werkzeugpanels hängt: Transformationssperre, Höhe, Höhenvarianz und die Schrittweite zum Anheben/Absenken.\nSie stehen in einer Reihe und werden daher gemeinsam gemerkt und geteilt.";
			d["scope.desc.function"] = "Wie weit der Wert dieses Punkts geteilt wird.";
			d["scope.desc.item"] = "Wie weit der Wert dieses Punkts geteilt wird.";
			d["scope.line.shared"] = "Global geteilt: alle Gebäude und Dienste, die diesen Punkt anzeigen, nutzen dieselbe Einstellung.";
			d["scope.line.shared.function"] = "Global geteilt: alle Funktionen, die diesen Punkt anzeigen, nutzen dieselbe Einstellung.";
			d["scope.line.shared.item"] = "Global geteilt: alle Requisiten, die diesen Punkt anzeigen, nutzen dieselbe Einstellung.";
			d["scope.line.unique"] = "Gar nicht geteilt: jedes Gebäude und jeder Dienst, der diesen Punkt anzeigt, wird einzeln eingestellt.";
			d["scope.line.unique.function"] = "Gar nicht geteilt: jede Funktion, die diesen Punkt anzeigt, wird einzeln eingestellt.";
			d["scope.line.unique.item"] = "Gar nicht geteilt: jedes Requisit, das diesen Punkt anzeigt, wird einzeln eingestellt.";
			d["scope.note"] = "Die Geltungsbereiche für Gebäude und Dienste, Funktionen und Requisiten sind voneinander unabhängig.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Diese Zeile gilt nur für {0}.";
			d["fam.net.desc"] = "Die Straßenfamilie: Straßen, Gleise, Leitungen und Stromleitungen zählen alle dazu.";
			d["fam.net"] = "Straßen";
			d["fam.assetObj"] = "Gebäude";
			d["fam.funcObj"] = "Marker";
			d["fam.zone"] = "Zonen";
			d["fam.area"] = "Gebietswerkzeuge";
			d["item.toolMode.desc"] = "Merkt sich den Modus, der bei diesen Werkzeugen gewählt ist.";
			d["item.f.toolMode.desc"] = "Merkt sich die Zeile Werkzeugmodus bei Zonenwerkzeugen: Füllen, Auswahlrahmen und Malen.";
			return d;
		}

		// ======================= es-ES =======================

		private static Dictionary<string, string> Es()
		{
			Dictionary<string, string> d = Frames("es-ES");
			d["mod.name"] = "Memoria de herramientas";
			d["tab.mod"] = "Ajustes del modo de herramienta";
			d["tab.about"] = "Acerca de";
			d["group.master"] = "Memoria de herramientas";
			d["group.official"] = "Ajustes de las herramientas oficiales";
			d["group.anarchy"] = "Ajustes de las herramientas de Anarchy";
			d["group.reset"] = "Gestión de la memoria";
			d["group.about"] = "Información y enlaces";

			d["scope.label"] = "Ámbito compartido";
			d["scope.desc"] = "Hasta qué punto se comparte el valor de este elemento. Los paréntesis indican qué hace el juego de forma predeterminada (original).";
			d["scope.line.group"] = "Mismo grupo: las herramientas del mismo grupo de la barra de menús comparten un valor. {0}";
			d["scope.line.menu"] = "Mismo menú: todas las herramientas de un menú comparten un valor. {0}";
			d["scope.line.category"] = "Mismo tipo: los objetos se agrupan por a quién sirven y cada grupo comparte un valor. {0}";

			d[kScopeGroup] = "Mismo grupo";
			d[kScopeMenu] = "Mismo menú";
			d[kScopeCategory] = "Mismo tipo de activo";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Sin compartir";
			d[kTagVanilla] = " (original)";
			d[kTagRecommended] = " (recomendado)";
			d[kTagRecVanilla] = " (recomendado, original)";

			d["reset.label"] = "Restablecer memoria";
			d["reset.desc"] = "En partida: borra la memoria de esta partida y devuelve las herramientas al estado original de al cargar. En el menú principal o el Editor: borra la memoria de todas las partidas.";
			d["reset.warn"] = "No se puede deshacer.";
			d["reset.confirm"] = "¿Restablecer los ajustes de herramienta recordados?";
			d["resetall.label"] = "Restablecer todos los ajustes";
			d["resetall.desc"] = "Devuelve cada opción de este mod a su valor por defecto: el interruptor general vuelve a desactivarse, los tres interruptores por página vuelven a activarse, y también se restablecen el estado de activación y el ámbito compartido de cada elemento. La memoria ya registrada de las partidas no se toca.";
			d["resetall.warn"] = "No se puede deshacer.";
			d["resetall.confirm"] = "¿Restablecer todos los ajustes?";
			d["folder.label"] = "Gestionar los archivos de memoria de todas las partidas";
			d["folder.desc"] = "Abre la carpeta local con los archivos de memoria, uno por partida, y el nombre del archivo es exactamente el nombre de la partida que ves en la lista «Cargar partida». La memoria se lee siempre por nombre de partida: un archivo solo se usa cuando el nombre de la partida y el nombre del archivo json coinciden, así que renombrar el archivo es la forma de copiar la memoria de una partida a otra. Al cargar un guardado automático su memoria sigue escribiéndose en el archivo de la partida de origen y lo sobrescribe, porque un guardado automático recibe un nombre nuevo cada vez y no puede servir de nombre de archivo; solo un guardado automático cuya partida de origen no se puede identificar se guarda de momento con el nombre de la ciudad, y pasa a llevar el nombre real de la partida en cuanto guardas manualmente.";

			d["about.version"] = "Versión del mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Apoya al autor en Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Abrir el hilo en el foro de Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Abrir el sitio de la serie Rainbow.";

			// ---------- Anarchy ----------
			d["item.anarchy.desc"] = "Recuerda el interruptor del mod Anarchy.\nSolo se recuerda en las herramientas que realmente tienen ese interruptor.\nSin el mod Anarchy este elemento no aparece.";
			d["item.anarchy.ex.group"] = "por ejemplo, activas Anarchy en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen activado, mientras que una carretera grande no.";
			d["item.anarchy.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras tienen Anarchy activado, pero al pasar al menú de Electricidad ya no.";
			d["item.anarchy.ex.category"] = "cambiar a una carretera de seis carriles sigue teniéndolo activado, pero un puente o unas vías de metro dobles lo dejan desactivado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras.";

			d["item.themes.label"] = "Temática";
			d["item.themes.desc"] = "Recuerda qué casillas están marcadas en el filtro «Temática» de la barra de herramientas; marcar una limita la lista de la barra a los assets que admiten esos temas.\nEsa fila solo aparece cuando la categoría actual contiene de verdad assets que usan temas.";
			d["item.themes.ex.group"] = "por ejemplo, marcas un tema solo en Carreteras/Carreteras pequeñas, de modo que una carretera grande ya no filtra por él.";
			d["item.themes.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras siguen el tema marcado allí, pero al pasar al menú de Electricidad no.";
			d["item.themes.ex.category"] = "lo marcado en esta fila es estado del propio filtro de la barra de herramientas, así que se recuerda por el menú y la categoría en los que estés, no por el tipo de activo.";
			d["item.packs.label"] = "Paquete";
			d["item.packs.desc"] = "Recuerda qué casillas están marcadas en el filtro «Paquete» de la barra de herramientas; marcar una limita la lista de la barra a los assets de esos paquetes.\nEl juego original los vacía cada vez que cambias de menú o de categoría.";
			d["item.packs.ex.group"] = "por ejemplo, marcas un paquete en Carreteras/Carreteras pequeñas, el juego original lo borra en las carreteras grandes y vuelve al regresar.";
			d["item.packs.ex.menu"] = "los paquetes marcados en el menú de Carreteras y en el menú de Electricidad se recuerdan por separado.";
			d["item.packs.ex.category"] = "lo marcado en esta fila es estado del propio filtro de la barra de herramientas, que se recuerda por el menú y la categoría en los que estés; el juego original la vacía cada vez que cambias de categoría.";

			// ---------- Herramientas ----------
			d["item.toolMode.label"] = "Herramientas";
			d["item.toolMode.ex.group"] = "por ejemplo, eliges Curva sencilla en una carretera pequeña de dos carriles y las otras carreteras pequeñas también usan Curva sencilla, mientras que una carretera grande no.";
			d["item.toolMode.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en Curva sencilla, pero al pasar al menú de Electricidad ya no.";
			d["item.toolMode.ex.category"] = "cambiar a una carretera de seis carriles sigue dando Curva sencilla, pero un puente o unas vías de metro dobles vuelven a su modo predeterminado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- Elevación ----------
			d["item.elevation.label"] = "Elevación";
			d["item.elevation.desc"] = "Recuerda la elevación con la que dejaste la herramienta, incluido el resultado de Aumentar la elevación y Disminuir la elevación.\nEl Escalón de elevación (lo que avanza con una sola pulsación; Anarchy llama a esa fila Paso de Elevación) se recuerda junto con ella y usa el mismo ámbito compartido.";
			d["item.elevation.ex.group"] = "por ejemplo, subes una carretera de dos carriles a 10 m y las otras carreteras pequeñas también quedan a 10 m, mientras que una carretera grande no está a 10 m.";
			d["item.elevation.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están a 10 m, pero al pasar al menú de Electricidad no están a 10 m.";
			d["item.elevation.ex.category"] = "cambiar a una carretera de seis carriles sigue dando 10 m, pero un puente o unas vías de tren dobles vuelven a 0 m y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- Modo paralelo ----------
			d["item.parallel.label"] = "Modo paralelo";
			d["item.parallel.desc"] = "Recuerda las dos cosas del panel Calle paralela: el interruptor Modo paralelo y la separación de la Compensación paralela.\nAl activarlo se construye una segunda línea junto a la que trazas; solo puedes ajustar lo separadas que están. Desactivado por defecto, es decir una sola línea como el original.";
			d["item.parallel.ex.group"] = "por ejemplo, activas el Modo paralelo en una carretera pequeña de dos carriles y pones la Compensación paralela en 5 m, las otras carreteras pequeñas vuelven activadas con los mismos 5 m, mientras que una carretera grande no.";
			d["item.parallel.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en paralelo con 5 m de compensación, pero al pasar al menú de Electricidad ya no.";
			d["item.parallel.ex.category"] = "cambiar a una carretera de seis carriles sigue activado con los mismos 5 m de compensación, pero un puente o una vía de metro se desactiva y necesita su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- Ajustar ----------
			d["item.snap.label"] = "Ajustar";
			d["item.snap.desc"] = "Recuerda cada interruptor de la fila Ajustar, por ejemplo a los lados de una carretera, a la mitad de la carretera, a los nodos, a la superficie de un objeto o a la orilla.\nQué interruptores aparecen depende de la herramienta; el interruptor múltiple de toda la fila no se recuerda.";
			d["item.snap.ex.group"] = "por ejemplo, desactivas «Ajustar a los lados de una carretera» en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen desactivado, mientras que una carretera grande lo tiene todo activado.";
			d["item.snap.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de ajuste, pero al pasar al menú de Electricidad no.";
			d["item.snap.ex.category"] = "cambiar a una carretera de seis carriles mantiene el mismo conjunto, pero un puente o un camino vuelve a los valores predeterminados y necesita su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- Topografía ----------
			d["item.topography.label"] = "Topografía";
			d["item.topography.desc"] = "Recuerda la fila Topografía, es decir, si Muestra las líneas de contorno está activo.";
			d["item.topography.ex.group"] = "por ejemplo, activas Topografía en una carretera pequeña de dos carriles y las otras carreteras pequeñas también la tienen activada, mientras que una carretera grande no.";
			d["item.topography.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras muestran las líneas de contorno, pero al pasar al menú de Electricidad no.";
			d["item.topography.ex.category"] = "cambiar a una carretera de seis carriles sigue mostrándolas, pero un puente o un cable de baja tensión tienen la fila desactivada y necesitan su propio ajuste.";


			// ---------- Izquierda y derecha (Anarchy) ----------
			d["item.leftRight.label"] = "Izquierda y derecha";
			d["item.leftRight.desc"] = "Recuerda las mejoras de red elegidas en las filas Izquierda y Derecha del panel de red de Anarchy, por ejemplo Vía ciclista, Césped, Muelle, Muro de Retención y Barrera acústica.\nNecesita Anarchy (mod 74604 de Paradox); sin él este elemento no aparece.";
			d["item.leftRight.ex.group"] = "por ejemplo, eliges Vía ciclista en el lado derecho de una carretera pequeña de dos carriles y las otras carreteras pequeñas también la reciben a la derecha, mientras que una carretera grande no.";
			d["item.leftRight.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo par de selecciones laterales, pero al pasar al menú de Electricidad no.";
			d["item.leftRight.ex.category"] = "cambiar a una carretera de seis carriles sigue teniendo Vía ciclista a la derecha, pero un puente o unas vías de tren dobles vuelven a no tener nada elegido y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- General (Anarchy) ----------
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Recuerda los interruptores de la fila General del panel de red de Anarchy: Terreno, Elevado, Túnel, Pendiente Constante, Mediana Amplia y Rango de Elevación Expandido (que quita el límite de altura).\nNecesita Anarchy (mod 74604 de Paradox); sin él este elemento no aparece.";
			d["item.general.ex.group"] = "por ejemplo, eliges Túnel en una carretera pequeña de dos carriles y las otras carreteras pequeñas también se obligan a ser túneles, mientras que una carretera grande no.";
			d["item.general.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de General, pero al pasar al menú de Electricidad no.";
			d["item.general.ex.category"] = "cambiar a una carretera de seis carriles sigue dando Túnel, pero un puente o unas vías de metro dobles vuelven a no tener nada marcado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- Modo subterráneo ----------
			d["item.underground.label"] = "Modo subterráneo";
			d["item.underground.desc"] = "Recuerda el interruptor Modo subterráneo, que decide si lo que colocas va bajo tierra o encima.";
			d["item.underground.ex.group"] = "por ejemplo, activas el Modo subterráneo en una carretera pequeña de dos carriles y las otras carreteras pequeñas también van bajo tierra, mientras que una carretera grande no.";
			d["item.underground.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras se construyen bajo tierra, pero al pasar al menú de Electricidad no.";
			d["item.underground.ex.category"] = "cambiar a una carretera de seis carriles sigue yendo bajo tierra, pero un puente o unas vías de tren dobles se quedan en la superficie y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras.";

			// ---------- Otros ----------

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（es-ES）====================
		d["enabled.label"] = "Activar el mod";
			d["enabled.desc"] = "Activa o desactiva todo el mod.\nActivado, los tres interruptores de abajo surten efecto: cada ajuste de herramienta se recuerda mientras trabajas y se repone al cargar una partida.\nDesactivado, el mod no interviene y cada herramienta se comporta como en el juego original.";
		d["domain.asset.desc"] = "Controla si se recuerdan los elementos de esta página:\nActivado: los ajustes se registran mientras trabajas y se reponen al cargar una partida;\nDesactivado: no se registra ni se repone nada, así que tampoco hay ámbito compartido que configurar.\nEsta página contiene carreteras, edificios y servicios urbanos.";
		d["domain.asset.label"] = "Recordar los elementos de herramienta de los activos";
		d["domain.function.desc"] = "Controla si se recuerdan los elementos de esta página:\nActivado: los ajustes se registran mientras trabajas y se reponen al cargar una partida;\nDesactivado: no se registra ni se repone nada, así que tampoco hay ámbito compartido que configurar.\nEsta página contiene zonas, áreas, rutas y herramientas de terreno.";
		d["domain.function.label"] = "Recordar los elementos de herramienta de las funciones";
		d["domain.item.desc"] = "Controla si se recuerdan los elementos de esta página:\nActivado: los ajustes se registran mientras trabajas y se reponen al cargar una partida;\nDesactivado: no se registra ni se repone nada, así que tampoco hay ámbito compartido que configurar.\nEsta página contiene los objetos del menú Paisajismo, árboles incluidos.";
		d["domain.item.label"] = "Recordar los elementos de herramienta de los objetos";
		d["group.domain"] = "Interruptores por página";
		d["item.color.label"] = "Color";
		d["item.decoration.desc"] = "Recuerda el interruptor «Preservar edad»: mientras esté activado, los árboles que pinta dejan de crecer y de morir de viejos.\nEsta fila solo la tienen los árboles.";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Tipo de pincel";
		d["item.brushType.desc"] = "Recuerda qué pincel está seleccionado en la fila «Tipo de pincel»: círculo, cuadrado, borde suave y demás, lo que define la forma del borde que pinta.\nLa fila solo existe en las herramientas que pueden pintar: las nueve herramientas de terreno y la herramienta de objetos cuando el activo seleccionado es pintable.";
		d["item.brushSize.label"] = "Tamaño de pincel";
		d["item.brushSize.desc"] = "Recuerda la fila «Tamaño de pincel»: hasta dónde llega una pasada.\nLa fila solo existe en las herramientas que pueden pintar: las nueve herramientas de terreno y la herramienta de objetos cuando el activo seleccionado es pintable.";
		d["item.brushStrength.label"] = "Fuerza de pincel";
		d["item.brushStrength.desc"] = "Recuerda la fila «Fuerza de pincel»: cuánto cambia cada pasada.\nLa fila solo existe en las herramientas que pueden pintar: las nueve herramientas de terreno y la herramienta de objetos cuando el activo seleccionado es pintable.";
		d["item.brushHeight.label"] = "Altura objetivo";
		d["item.brushHeight.desc"] = "Recuerda el valor de la fila «Altura objetivo»: a qué altura queda el suelo después de que una herramienta de terreno lo nivele o lo convierta en una pendiente.\nLa fila solo existe en las dos herramientas de terreno «Herramienta de nivelación de terreno» y «Herramienta de pendientes».";
		d["item.distance.label"] = "Distancia";
		d["item.distance.desc"] = "Recuerda el valor de la fila «Distancia»: a qué distancia quedan unos de otros los objetos consecutivos cuando los coloca a lo largo de una línea o de una curva.\nLa fila solo aparece en los modos Línea y Curva.";
		d["item.ageMask.label"] = "Edad";
		d["item.ageMask.desc"] = "Recuerda qué etapas de edad están marcadas en la fila «Edad»: brote, joven, maduro y anciano, y varias a la vez.\nEsta fila solo la tienen los árboles.";
		d["item.anarchyObjectElevation.label"] = "Elevación del objeto";
		d["item.anarchyObjectElevation.desc"] = "Recuerda la fila de controles de altura que Anarchy añade al panel de la herramienta de objetos: bloqueo de transformación, elevación, variación de elevación y el paso de subida y bajada.\nLos cuatro están en una sola fila y cuentan como un solo elemento.";
		d["item.decoration.label"] = "Preservar edad";
		d["item.f.snap.desc"] = "Recuerda cada interruptor de la fila «Ajustar» de los paneles de las herramientas de función: a ángulos de 90 grados, a la cuadrícula, a las carreteras, al lado del propietario, mostrar las líneas de contorno y demás.\nQué interruptores aparecen depende de la herramienta; el interruptor por lotes de toda la lista no se recuerda.";
		d["item.f.topography.desc"] = "Recuerda el estado de la fila «Líneas de contorno» de los paneles de las herramientas de función, es decir, si se ven las líneas de contorno del terreno mientras dibuja zonas.";
		d["item.f.underground.desc"] = "Recuerda el estado de la fila «Modo subterráneo» de los paneles de las herramientas de función.";
		d["item.i.snap.desc"] = "Recuerda cada interruptor de la fila «Ajustar» de los paneles de los objetos: a la superficie del edificio, a las formas cercanas, a la carretera, eliminar solo el tipo coincidente y demás, dependiendo del objeto.\nQué interruptores aparecen depende del objeto.";
		d["item.i.toolMode.desc"] = "Recuerda el modo de herramienta elegido en los paneles de los objetos: Colocar uno, Colocar varios, Línea, Curva, Herramienta de sello de objetos, Pintar y demás, dependiendo de la herramienta.";
		d["item.i.topography.desc"] = "Recuerda el estado de la fila «Líneas de contorno» de los paneles de los objetos.";
		d["item.i.underground.desc"] = "Recuerda el estado de la fila «Modo subterráneo» de los paneles de los objetos, es decir, si los objetos se colocan bajo el suelo o encima de él.";

		d["scope.category.function"] = "Mismo tipo";
		d["scope.category.item"] = "Mismo tipo de objeto";
		d["tab.assets"] = "Activos";
		d["tab.functions"] = "Funciones";
		d["tab.items"] = "Objetos";
		d["tab.master"] = "Interruptor general";
			d["ex.net.group"] = "Por ejemplo, todas las carreteras del grupo Carreteras pequeñas comparten un valor; Carreteras grandes es otro grupo.";
			d["ex.net.menu"] = "Por ejemplo, todas las carreteras del menú Carreteras comparten un valor; el menú Electricidad no.";
			d["ex.net.category"] = "Por ejemplo, las carreteras de cualquier número de carriles son un mismo tipo; los puentes y las vías de tren son tipos aparte.";
			d["ex.zone.group"] = "Por ejemplo, la vivienda de baja, media y alta densidad de Zonas residenciales forma un grupo; Zonas comerciales es otro grupo, aunque ambos estén en el mismo menú.";
			d["ex.zone.menu"] = "Por ejemplo, Zonas residenciales, Zonas comerciales, Zonas industriales y Zonas de oficina comparten un valor; otro menú no.";
			d["ex.route.group"] = "Por ejemplo, las líneas se agrupan por medio de transporte (carretera, tranvía, tren, metro, etc.); una línea de otro medio es otro grupo.";
			d["ex.route.menu"] = "Por ejemplo, todas las líneas del menú Transporte comparten un valor; otro menú no.";
			d["ex.terrain.group"] = "La terraformación solo tiene un grupo, así que Mismo grupo y Mismo menú eligen exactamente las mismas herramientas.";
			d["ex.terrain.menu"] = "La terraformación solo tiene un grupo, así que Mismo menú y Mismo grupo eligen exactamente las mismas herramientas.";
			d["ex.prop.group"] = "Por ejemplo, el menú Paisajismo divide los objetos en Naturaleza, Parque, residencial, comercial, industrial, escombros, calcomanías y jardineras; una categoría comparte un valor y otra es otro grupo.";
			d["ex.prop.menu"] = "Todos los objetos y árboles del menú Paisajismo comparten un valor.";
			d["ex.tree.group"] = "Todos los árboles están en el grupo Vegetación del menú Paisajismo, así que Mismo grupo y Mismo menú eligen los mismos árboles.";
			d["ex.tree.menu"] = "Todos los árboles están en el grupo Vegetación del menú Paisajismo, así que Mismo menú y Mismo grupo eligen los mismos árboles.";
			d["item.color.desc"] = "Recuerda la fila Color de las herramientas de línea: el color con el que se dibuja la línea.";
			d["item.f.snap.desc"] = "Recuerda los interruptores de la fila Ajustar en las herramientas de función; las opciones dependen de la herramienta.\nEl botón del juego para activar o desactivar todos los ajustes es un interruptor global y no se recuerda.";
			d["item.f.topography.desc"] = "Recuerda la fila Topografía en las herramientas de zonas, terraformación y otras: si se ven las curvas de nivel.";
			d["item.f.underground.desc"] = "Recuerda la fila Modo subterráneo en las herramientas de área, línea, demolición y objetos.";
			d["item.i.topography.desc"] = "Recuerda la fila Topografía en los paneles de objetos.";
			d["item.anarchyObjectElevation.desc"] = "Recuerda los controles de altura que Anarchy añade al panel: bloqueo de transformación, altura, variación de altura y el paso para subir o bajar.\nEstán en la misma fila, así que se recuerdan y comparten juntos.";
			d["scope.desc.function"] = "Hasta qué punto se comparte el valor de este elemento.";
			d["scope.desc.item"] = "Hasta qué punto se comparte el valor de este elemento.";
			d["scope.line.shared"] = "Global: todos los edificios y servicios que muestran este elemento comparten un mismo ajuste.";
			d["scope.line.shared.function"] = "Global: todas las funciones que muestran este elemento comparten un mismo ajuste.";
			d["scope.line.shared.item"] = "Global: todos los objetos que muestran este elemento comparten un mismo ajuste.";
			d["scope.line.unique"] = "Sin compartir: cada edificio y servicio que muestra este elemento se ajusta por separado.";
			d["scope.line.unique.function"] = "Sin compartir: cada función que muestra este elemento se ajusta por separado.";
			d["scope.line.unique.item"] = "Sin compartir: cada objeto que muestra este elemento se ajusta por separado.";
			d["scope.note"] = "Los ámbitos de edificios y servicios, funciones y objetos son independientes entre sí.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Esta fila solo se aplica a {0}.";
			d["fam.net.desc"] = "La familia de carreteras: carreteras, vías, tuberías y líneas eléctricas cuentan todas.";
			d["fam.net"] = "carreteras";
			d["fam.assetObj"] = "edificios";
			d["fam.funcObj"] = "marcadores";
			d["fam.zone"] = "zonas";
			d["fam.area"] = "herramientas de área";
			d["item.toolMode.desc"] = "Recuerda el modo elegido en estas herramientas.";
			d["item.f.toolMode.desc"] = "Recuerda la fila Modo de herramienta en las herramientas de zona: Relleno, Marco y Pintar.";
			return d;
		}

		// ======================= fr-FR =======================

		private static Dictionary<string, string> Fr()
		{
			Dictionary<string, string> d = Frames("fr-FR");
			d["mod.name"] = "Mémoire du mode d'outil";
			d["tab.mod"] = "Réglages du mode d'outil";
			d["tab.about"] = "À propos";
			d["group.master"] = "Mémoire du mode d'outil";
			d["group.official"] = "Réglages des outils officiels";
			d["group.anarchy"] = "Réglages des outils d'Anarchy";
			d["group.reset"] = "Gestion de la mémoire";
			d["group.about"] = "Informations et liens";

			d["scope.label"] = "Portée de partage";
			d["scope.desc"] = "Dans quelle mesure la valeur de cet élément est partagée. Les parenthèses indiquent ce que fait le jeu par défaut (d'origine).";
			d["scope.line.group"] = "Même groupe : les outils d'un même groupe de la barre de menus partagent une valeur. {0}";
			d["scope.line.menu"] = "Même menu : tous les outils d'un menu partagent une valeur. {0}";
			d["scope.line.category"] = "Même type : les objets sont classés selon ce à quoi ils servent, et un même type partage une valeur. {0}";

			d[kScopeGroup] = "Même groupe";
			d[kScopeMenu] = "Même menu";
			d[kScopeCategory] = "Même type d'actif";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Aucun partage";
			d[kTagVanilla] = " (d'origine)";
			d[kTagRecommended] = " (recommandé)";
			d[kTagRecVanilla] = " (recommandé, d'origine)";

			d["reset.label"] = "Réinitialiser la mémoire";
			d["reset.desc"] = "En partie : efface la mémoire de cette partie et rend les outils à l'état d'origine de la charge. Au menu principal ou dans l'Éditeur : efface la mémoire de toutes les parties.";
			d["reset.warn"] = "Irréversible.";
			d["reset.confirm"] = "Réinitialiser les réglages d'outil mémorisés ?";
			d["resetall.label"] = "Réinitialiser tous les réglages";
			d["resetall.desc"] = "Remet chaque option de ce mod à sa valeur par défaut : l'interrupteur général se coupe à nouveau, les trois interrupteurs par page se réactivent, et l'état comme la portée de partage de chaque élément sont rétablis. La mémoire déjà enregistrée pour les parties n'est pas touchée.";
			d["resetall.warn"] = "Irréversible.";
			d["resetall.confirm"] = "Réinitialiser tous les réglages ?";
			d["folder.label"] = "Gérer les fichiers mémoire de toutes les parties";
			d["folder.desc"] = "Ouvre le dossier local des fichiers mémoire, un par partie, et le nom du fichier est exactement le nom de la partie que vous voyez dans la liste « Charger la partie ». La mémoire se lit toujours d'après le nom de la partie : un fichier n'est utilisé que si le nom de la partie et celui du fichier json correspondent, donc renommer le fichier est le moyen de copier la mémoire d'une partie à une autre. Quand vous chargez une sauvegarde automatique, sa mémoire va quand même dans le fichier portant le nom de la partie d'origine et l'écrase, parce qu'une sauvegarde automatique reçoit un nom nouveau à chaque fois et ne peut pas servir de nom de fichier ; seule une sauvegarde automatique dont on ne retrouve pas la partie d'origine est provisoirement gardée sous le nom de la ville, et le fichier prend le vrai nom de la partie dès que vous enregistrez manuellement.";

			d["about.version"] = "Version du mod";
			d["about.author"] = "Auteur";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Soutenez l'auteur sur Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Ouvrir le fil sur le forum Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Ouvrir le site de la série Rainbow.";

			// ---------- Anarchy ----------
			d["item.anarchy.desc"] = "Mémorise le bouton du mod Anarchy.\nIl n'est mémorisé que sur les outils qui ont réellement ce bouton.\nSans le mod Anarchy, cet élément n'apparaît pas.";
			d["item.anarchy.ex.group"] = "par exemple, activez Anarchy sur une petite route à deux voies, les autres petites routes l'ont activé aussi, mais une grande route non.";
			d["item.anarchy.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes ont Anarchy activé, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.anarchy.ex.category"] = "passer à une route à six voies laisse Anarchy activé, mais un pont ou une double voie de métro repasse à désactivé et réclame son propre réglage, bien que tous soient dans le menu Routes.";

			d["item.themes.label"] = "Thème";
			d["item.themes.desc"] = "Mémorise les entrées cochées dans le filtre « Thème » de la barre d'outils ; cocher un thème limite la liste de la barre aux assets qui prennent en charge ces thèmes.\nCette ligne n'apparaît que si la catégorie courante contient vraiment des assets utilisant des thèmes.";
			d["item.themes.ex.group"] = "par exemple, cochez un thème uniquement sous Routes/Petites routes, une grande route ne filtre donc plus selon lui.";
			d["item.themes.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes suivent le thème coché dans ce menu, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.themes.ex.category"] = "cette ligne est l'état du filtre de la barre d'outils lui-même, elle est donc mémorisée d'après le menu et la catégorie où vous vous trouvez, et non d'après le type d'actif.";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Mémorise les entrées cochées dans le filtre « Pack » de la barre d'outils ; cocher un pack limite la liste de la barre aux assets appartenant à ces packs.\nLe jeu d'origine les efface à chaque changement de menu ou de catégorie.";
			d["item.packs.ex.group"] = "par exemple, cochez un pack sous Routes/Petites routes, le jeu d'origine l'efface sur une grande route et il revient quand vous y retournez.";
			d["item.packs.ex.menu"] = "les packs cochés dans le menu Routes et dans le menu Électricité sont mémorisés séparément.";
			d["item.packs.ex.category"] = "cette ligne est l'état du filtre de la barre d'outils lui-même, mémorisée d'après le menu et la catégorie où vous vous trouvez ; le jeu d'origine l'efface à chaque changement de catégorie.";

			// ---------- Mode Outil ----------
			d["item.toolMode.label"] = "Mode Outil";
			d["item.toolMode.ex.group"] = "par exemple, choisissez Courbe simple sur une petite route à deux voies, les autres petites routes utilisent aussi Courbe simple, mais une grande route non.";
			d["item.toolMode.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en Courbe simple, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.toolMode.ex.category"] = "passer à une route à six voies donne toujours Courbe simple, mais un pont ou une double voie de métro retombe sur son mode par défaut et réclame son propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Élévation ----------
			d["item.elevation.label"] = "Élévation";
			d["item.elevation.desc"] = "Mémorise l'élévation laissée sur l'outil, y compris le résultat de Augmenter l'élévation et Diminuer l'élévation.\nL'Étape d'élévation (le déplacement d'une seule pression ; l'interface Anarchy nomme cette ligne Incrément d'élévation) est mémorisée avec elle et utilise la même portée de partage.";
			d["item.elevation.ex.group"] = "par exemple, montez une route à deux voies à 10 m, les autres petites routes passent aussi à 10 m, mais une grande route n'est pas à 10 m.";
			d["item.elevation.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont à 10 m, mais dans le menu Électricité ils ne sont pas à 10 m.";
			d["item.elevation.ex.category"] = "passer à une route à six voies donne toujours 10 m, mais un pont ou une double voie ferrée repart à 0 m et réclame son propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Mode parallèle ----------
			d["item.parallel.label"] = "Mode parallèle";
			d["item.parallel.desc"] = "Mémorise les deux éléments du panneau Route parallèle : l'interrupteur Mode parallèle et l'espacement de la Compensation parallèle.\nActivé, une seconde ligne se construit le long de celle que vous tracez ; seule la distance entre les deux est réglable. Coupé par défaut, donc une seule ligne comme à l'origine.";
			d["item.parallel.ex.group"] = "par exemple, activez le Mode parallèle sur une petite route à deux voies et réglez la Compensation parallèle sur 5 m, les autres petites routes reviennent activées avec les mêmes 5 m, mais une grande route non.";
			d["item.parallel.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en parallèle avec 5 m de compensation, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.parallel.ex.category"] = "passer à une route à six voies reste activé avec les mêmes 5 m de compensation, mais un pont ou une voie de métro se désactive et réclame son propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Accrocher ----------
			d["item.snap.label"] = "Accrocher";
			d["item.snap.desc"] = "Mémorise chaque interrupteur de la ligne Accrocher, par exemple aux côtés d'une route, au milieu d'une route, aux nœuds, à la surface d'un objet ou au rivage.\nQuels interrupteurs apparaissent dépend de l'outil ; l'interrupteur groupé de toute la ligne n'est pas mémorisé.";
			d["item.snap.ex.group"] = "par exemple, désactivez « Accrochez aux côtés d'une route » sur une petite route à deux voies, les autres petites routes l'ont désactivé aussi, mais une grande route a tout activé.";
			d["item.snap.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs d'accrochage, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.snap.ex.category"] = "passer à une route à six voies garde le même jeu d'interrupteurs, mais un pont ou un sentier revient aux valeurs par défaut et réclame son propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Topographie ----------
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Mémorise la ligne Topographie, c'est-à-dire si Afficher les lignes de contour est actif.";
			d["item.topography.ex.group"] = "par exemple, activez la Topographie sur une petite route à deux voies, les autres petites routes l'ont activée aussi, mais une grande route non.";
			d["item.topography.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes affichent les lignes de contour, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.topography.ex.category"] = "passer à une route à six voies affiche toujours les lignes de contour, mais un pont ou un câble électrique ont la ligne désactivée et réclament leur propre réglage.";


			// ---------- Gauche et droite (Anarchy) ----------
			d["item.leftRight.label"] = "Gauche et droite";
			d["item.leftRight.desc"] = "Mémorise les améliorations de réseau choisies sur les lignes Gauche et Droite du panneau réseau d'Anarchy, par exemple Piste cyclable, Herbe, Quai, Mur de soutènement et Barrière acoustique.\nNécessite Anarchy (mod Paradox 74604) ; sans lui cet élément n'apparaît pas.";
			d["item.leftRight.ex.group"] = "par exemple, choisissez Piste cyclable à droite d'une petite route à deux voies, les autres petites routes l'ont aussi à droite, mais une grande route non.";
			d["item.leftRight.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même choix des deux côtés, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.leftRight.ex.category"] = "passer à une route à six voies garde Piste cyclable à droite, mais un pont ou une double voie ferrée reviennent à rien de choisi et réclament leur propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Général (Anarchy) ----------
			d["item.general.label"] = "Général";
			d["item.general.desc"] = "Mémorise les interrupteurs de la ligne Général du panneau réseau d'Anarchy : Au sol, Élévation, Tunnel, Pente constante, Grande médiane et Gamme d'élévation élargie (qui supprime la limite de hauteur).\nNécessite Anarchy (mod Paradox 74604) ; sans lui cet élément n'apparaît pas.";
			d["item.general.ex.group"] = "par exemple, choisissez Tunnel sur une petite route à deux voies, les autres petites routes sont forcées en tunnel aussi, mais une grande route non.";
			d["item.general.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs de Général, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.general.ex.category"] = "passer à une route à six voies donne toujours Tunnel, mais un pont ou une double voie de métro reviennent à rien de coché et réclament leur propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Mode souterrain ----------
			d["item.underground.label"] = "Mode souterrain";
			d["item.underground.desc"] = "Mémorise l'interrupteur Mode souterrain, qui décide si ce que vous placez passe sous le sol ou dessus.";
			d["item.underground.ex.group"] = "par exemple, activez le Mode souterrain sur une petite route à deux voies, les autres petites routes passent aussi sous terre, mais une grande route non.";
			d["item.underground.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont construits sous terre, mais dans le menu Électricité ce n'est plus le cas.";
			d["item.underground.ex.category"] = "passer à une route à six voies reste sous terre, mais un pont ou une double voie ferrée restent en surface et réclament leur propre réglage, bien que tous soient dans le menu Routes.";

			// ---------- Autres ----------

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（fr-FR）====================
		d["tab.master"] = "Interrupteur principal";
		d["tab.assets"] = "Assets";
		d["tab.functions"] = "Fonctions";
		d["tab.items"] = "Accessoires";
		d["group.domain"] = "Interrupteurs par page";
		d["domain.asset.label"] = "Mémoriser les éléments d'outil des assets";
		d["domain.asset.desc"] = "Détermine si les éléments de cette page sont mémorisés :\nActivé - les réglages sont mémorisés pendant que vous travaillez et remis au chargement d'une sauvegarde ;\nCoupé - rien n'est mémorisé ni remis, il n'y a donc aucune portée de partage à régler.\nCette page contient les routes, les bâtiments et les services urbains.";
		d["domain.function.label"] = "Mémoriser les éléments d'outil des fonctions";
		d["domain.function.desc"] = "Détermine si les éléments de cette page sont mémorisés :\nActivé - les réglages sont mémorisés pendant que vous travaillez et remis au chargement d'une sauvegarde ;\nCoupé - rien n'est mémorisé ni remis, il n'y a donc aucune portée de partage à régler.\nCette page contient les zones, les périmètres, les tracés et les outils de terrain.";
		d["domain.item.label"] = "Mémoriser les éléments d'outil des accessoires";
		d["domain.item.desc"] = "Détermine si les éléments de cette page sont mémorisés :\nActivé - les réglages sont mémorisés pendant que vous travaillez et remis au chargement d'une sauvegarde ;\nCoupé - rien n'est mémorisé ni remis, il n'y a donc aucune portée de partage à régler.\nCette page contient les accessoires du menu Aménagement paysager, arbres compris.";
		d["item.color.label"] = "Couleur";
		d["item.decoration.label"] = "Préserver l'âge";
		d["item.decoration.desc"] = "Mémorise l'interrupteur « Préserver l'âge » : une fois coché, les arbres que vous brossez cessent de grandir et de vieillir.\nSeuls les arbres ont cette ligne.";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Type de pinceau";
		d["item.brushType.desc"] = "Mémorise le pinceau sélectionné dans la ligne « Type de pinceau » : cercle, carré, bord doux, etc., ce qui définit la forme du bord que vous peignez.\nCette ligne n'existe que sur les outils capables de peindre : les neuf outils de terraformation, et l'outil Objet lorsque l'asset sélectionné est brossable.";
		d["item.brushSize.label"] = "Taille du pinceau";
		d["item.brushSize.desc"] = "Mémorise la ligne « Taille du pinceau » : l'étendue couverte par une passe.\nCette ligne n'existe que sur les outils capables de peindre : les neuf outils de terraformation, et l'outil Objet lorsque l'asset sélectionné est brossable.";
		d["item.brushStrength.label"] = "Force du pinceau";
		d["item.brushStrength.desc"] = "Mémorise la ligne « Force du pinceau » : ce que chaque passe modifie.\nCette ligne n'existe que sur les outils capables de peindre : les neuf outils de terraformation, et l'outil Objet lorsque l'asset sélectionné est brossable.";
		d["item.brushHeight.label"] = "Hauteur cible";
		d["item.brushHeight.desc"] = "Mémorise la valeur de la ligne « Hauteur cible » : à quelle hauteur le sol se retrouve après qu'un outil de terraformation l'a aplani ou transformé en pente.\nCette ligne n'existe que sur les deux outils de terrain Outil de nivellement de terrain et Outil pour les terrains en pente.";
		d["item.distance.label"] = "Distance";
		d["item.distance.desc"] = "Mémorise la valeur de la ligne « Distance » : l'écart entre des objets consécutifs lorsque vous les placez le long d'une ligne ou d'une courbe.\nCette ligne n'apparaît que dans les modes Ligne et Courbe.";
		d["item.ageMask.label"] = "Âge";
		d["item.ageMask.desc"] = "Mémorise les stades d'âge cochés dans la ligne « Âge » : jeune arbre, jeune, adulte et âgé, plusieurs à la fois si vous voulez.\nSeuls les arbres ont cette ligne.";
		d["item.anarchyObjectElevation.label"] = "Élévation de l'objet";
		d["item.anarchyObjectElevation.desc"] = "Mémorise la rangée de contrôles de hauteur qu'Anarchy ajoute au panneau de l'outil Objet : verrouillage de transformation, élévation, variation d'altitude et pas d'augmentation et de diminution.\nCes quatre réglages tiennent sur une seule rangée et comptent donc comme un seul élément.";
		d["item.f.snap.desc"] = "Mémorise chaque interrupteur de la ligne « Accrocher » des panneaux de fonctions : aux angles à 90 degrés, à la grille de zone, aux routes, au côté du propriétaire, afficher les lignes de contour et d'autres encore.\nQuels interrupteurs apparaissent dépendent de l'outil ; l'interrupteur groupé de toute la liste n'est pas mémorisé.";
		d["item.f.topography.desc"] = "Mémorise l'état de la ligne « Lignes de contour » des panneaux de fonctions, c'est-à-dire si vous voyez ou non les courbes de niveau du terrain en dessinant des zones.";
		d["item.f.underground.desc"] = "Mémorise l'état de la ligne « Mode souterrain » des panneaux de fonctions.";
		d["item.i.snap.desc"] = "Mémorise chaque interrupteur de la ligne « Accrocher » des panneaux d'accessoires : à la surface d'un bâtiment, à la géométrie à proximité, aux routes, supprimer uniquement le type correspondant et d'autres encore.\nQuels interrupteurs apparaissent dépendent de l'accessoire.";
		d["item.i.toolMode.desc"] = "Mémorise le choix de la ligne « Mode Outil » des panneaux d'accessoires : En placer un, En placer plusieurs, Ligne, Courbe, Outil de marquage d'objet, Peinture et ainsi de suite selon l'outil.";
		d["item.i.topography.desc"] = "Mémorise l'état de la ligne « Lignes de contour » des panneaux d'accessoires.";
		d["item.i.underground.desc"] = "Mémorise l'état de la ligne « Mode souterrain » des panneaux d'accessoires, c'est-à-dire si vous posez les accessoires sous le sol ou par-dessus.";

		d["scope.category.function"] = "Même type";
		d["scope.category.item"] = "Même type d'accessoire";
		d["enabled.label"] = "Activer le mod";
		d["enabled.desc"] = "Active ou coupe tout le mod.\nActivé, les trois interrupteurs ci-dessous prennent effet : chaque réglage d'outil est mémorisé pendant que vous travaillez et remis en place au chargement d'une sauvegarde.\nCoupé, le mod reste en retrait et chaque outil se comporte comme dans le jeu d'origine.";
			d["ex.net.group"] = "Par exemple, toutes les routes du groupe Petites routes partagent une valeur ; Grands axes routiers est un autre groupe.";
			d["ex.net.menu"] = "Par exemple, toutes les routes du menu Routes partagent une valeur ; le menu Électricité non.";
			d["ex.net.category"] = "Par exemple, les routes de tout nombre de voies forment un même type ; les ponts et les voies ferrées sont des types à part.";
			d["ex.zone.group"] = "Par exemple, les logements de faible, moyenne et forte densité de Zones résidentielles forment un groupe ; Zones commerciales est un autre groupe, bien que les deux soient dans le même menu.";
			d["ex.zone.menu"] = "Par exemple, Zones résidentielles, Zones commerciales, Zones industrielles et Zones de bureaux partagent une valeur ; un autre menu non.";
			d["ex.route.group"] = "Par exemple, les lignes sont groupées par mode de transport (route, tram, train, métro, etc.) ; une ligne d'un autre mode est un autre groupe.";
			d["ex.route.menu"] = "Par exemple, toutes les lignes du menu Transport partagent une valeur ; un autre menu non.";
			d["ex.terrain.group"] = "La terraformation n'a qu'un seul groupe : Même groupe et Même menu choisissent donc les mêmes outils.";
			d["ex.terrain.menu"] = "La terraformation n'a qu'un seul groupe : Même menu et Même groupe choisissent donc les mêmes outils.";
			d["ex.prop.group"] = "Par exemple, le menu Aménagement paysager divise les objets en Nature, Parc, résidentiel, commercial, industriel, débris, décalques et jardinières ; une catégorie partage une valeur, une autre est un autre groupe.";
			d["ex.prop.menu"] = "Tous les objets et arbres du menu Aménagement paysager partagent une valeur.";
			d["ex.tree.group"] = "Tous les arbres sont dans le groupe Végétation du menu Aménagement paysager : Même groupe et Même menu choisissent donc les mêmes arbres.";
			d["ex.tree.menu"] = "Tous les arbres sont dans le groupe Végétation du menu Aménagement paysager : Même menu et Même groupe choisissent donc les mêmes arbres.";
			d["item.color.desc"] = "Retient la ligne Couleur des outils de ligne : la couleur avec laquelle la ligne est dessinée.";
			d["item.f.snap.desc"] = "Retient les interrupteurs de la ligne Accrocher des outils de fonction ; les choix dépendent de l'outil.\nLe bouton du jeu pour tout accrocher ou non est un interrupteur global et n'est pas mémorisé.";
			d["item.f.topography.desc"] = "Retient la ligne Topographie des outils de zones, de terraformation et autres : si les courbes de niveau sont visibles.";
			d["item.f.underground.desc"] = "Retient la ligne Mode souterrain des outils de zone, de ligne, de démolition et d'objets.";
			d["item.i.topography.desc"] = "Retient la ligne Topographie des panneaux d'objets.";
			d["item.anarchyObjectElevation.desc"] = "Retient les contrôles de hauteur qu'Anarchy ajoute au panneau : verrouillage de transformation, hauteur, variation de hauteur et le pas de montée/descente.\nIls sont sur une même ligne, donc ils sont mémorisés et partagés ensemble.";
			d["scope.desc.function"] = "Dans quelle mesure la valeur de cet élément est partagée.";
			d["scope.desc.item"] = "Dans quelle mesure la valeur de cet élément est partagée.";
			d["scope.line.shared"] = "Global : tous les bâtiments et services qui affichent cet élément partagent un même réglage.";
			d["scope.line.shared.function"] = "Global : toutes les fonctions qui affichent cet élément partagent un même réglage.";
			d["scope.line.shared.item"] = "Global : tous les accessoires qui affichent cet élément partagent un même réglage.";
			d["scope.line.unique"] = "Aucun partage : chaque bâtiment et service qui affiche cet élément se règle séparément.";
			d["scope.line.unique.function"] = "Aucun partage : chaque fonction qui affiche cet élément se règle séparément.";
			d["scope.line.unique.item"] = "Aucun partage : chaque accessoire qui affiche cet élément se règle séparément.";
			d["scope.note"] = "Les portées des bâtiments et services, des fonctions et des accessoires sont indépendantes.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Cette ligne ne s'applique qu'à {0}.";
			d["fam.net.desc"] = "La famille des routes : routes, voies, canalisations et lignes électriques en font partie.";
			d["fam.net"] = "routes";
			d["fam.assetObj"] = "bâtiments";
			d["fam.funcObj"] = "marqueurs";
			d["fam.zone"] = "zones";
			d["fam.area"] = "outils de secteur";
			d["item.toolMode.desc"] = "Mémorise le mode choisi sur ces outils.";
			d["item.f.toolMode.desc"] = "Mémorise la ligne Mode d'outil des outils de zone : Remplissage, Sélection et Peinture.";
			return d;
		}

		// ======================= it-IT =======================

		private static Dictionary<string, string> It()
		{
			Dictionary<string, string> d = Frames("it-IT");
			d["mod.name"] = "Memoria modalità strumento";
			d["tab.mod"] = "Impostazioni modalità strumento";
			d["tab.about"] = "Informazioni";
			d["group.master"] = "Memoria modalità strumento";
			d["group.official"] = "Impostazioni degli strumenti ufficiali";
			d["group.anarchy"] = "Impostazioni degli strumenti di Anarchy";
			d["group.reset"] = "Gestione memoria";
			d["group.about"] = "Informazioni e collegamenti";

			d["scope.label"] = "Ambito di condivisione";
			d["scope.desc"] = "Quanto viene condiviso il valore di questa voce. Le parentesi mostrano cosa fa il gioco di suo (originale).";
			d["scope.line.group"] = "Stesso gruppo: gli strumenti dello stesso gruppo della barra dei menu condividono un valore. {0}";
			d["scope.line.menu"] = "Stesso menu: tutti gli strumenti di un menu condividono un valore. {0}";
			d["scope.line.category"] = "Stesso tipo: gli oggetti sono raggruppati in base a ciò a cui servono e ogni tipo condivide un valore. {0}";

			d[kScopeGroup] = "Stesso gruppo";
			d[kScopeMenu] = "Stesso menu";
			d[kScopeCategory] = "Stesso tipo di asset";
			d[kScopeGlobalShared] = "Globale";
			d[kScopeGlobalUnique] = "Non condiviso";
			d[kTagVanilla] = " (originale)";
			d[kTagRecommended] = " (consigliato)";
			d[kTagRecVanilla] = " (consigliato, originale)";

			d["reset.label"] = "Reimposta memoria";
			d["reset.desc"] = "In partita: cancella la memoria di questa partita e riporta gli strumenti allo stato originale del caricamento. Nel menu principale o nell'editor: cancella la memoria di tutte le partite.";
			d["reset.warn"] = "Non annullabile.";
			d["reset.confirm"] = "Reimpostare le impostazioni degli strumenti memorizzate?";
			d["resetall.label"] = "Reimposta tutte le impostazioni";
			d["resetall.desc"] = "Riporta ogni opzione di questo mod al valore predefinito: l'interruttore generale si rispegne, i tre interruttori per pagina si riattivano, e vengono ripristinati anche lo stato di attivazione e l'ambito di condivisione di ogni voce. La memoria già registrata delle partite non viene toccata.";
			d["resetall.warn"] = "Non annullabile.";
			d["resetall.confirm"] = "Reimpostare tutte le impostazioni?";
			d["folder.label"] = "Gestisci i file di memoria di tutte le partite";
			d["folder.desc"] = "Apre la cartella locale dei file di memoria, uno per partita, e il nome del file è esattamente il nome della partita che vedi nell'elenco «Carica partita». La memoria viene sempre letta in base al nome della partita: un file viene usato solo se il nome della partita e quello del file json corrispondono, quindi rinominare il file è il modo di copiare la memoria da una partita all'altra. Quando carichi un salvataggio automatico, la sua memoria finisce comunque nel file con il nome della partita di origine e lo sovrascrive, perché un salvataggio automatico riceve un nome nuovo ogni volta e non può fare da nome file; solo un salvataggio automatico di cui non si riesce a ritrovare la partita di origine viene salvato provvisoriamente con il nome della città, e prende il vero nome della partita non appena salvi manualmente.";

			d["about.version"] = "Versione mod";
			d["about.author"] = "Autore";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Supporta l'autore su Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Apri la discussione sul forum Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Apri il sito della serie Rainbow.";

			// ---------- Anarchy ----------
			d["item.anarchy.desc"] = "Ricorda l'interruttore del mod Anarchy.\nViene ricordato solo sugli strumenti che hanno davvero questo interruttore.\nSenza il mod Anarchy questa voce non compare.";
			d["item.anarchy.ex.group"] = "per esempio, attivi Anarchy su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attivo, mentre una strada grande no.";
			d["item.anarchy.ex.menu"] = "tutti gli asset che clicchi nel menu Strade hanno Anarchy attivo, ma passando al menu Elettricità no.";
			d["item.anarchy.ex.category"] = "passando a una strada a sei corsie Anarchy resta attivo, ma un ponte o un binario della metropolitana doppio tornano a spento e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Ricorda quali voci sono spuntate nel filtro «Tema» della barra degli strumenti; spuntarne uno limita l'elenco della barra agli asset che supportano quei temi.\nQuella riga compare solo quando la categoria corrente contiene davvero asset che usano i temi.";
			d["item.themes.ex.group"] = "per esempio, spunti un tema solo in Strade/Strade piccole, così una strada grande non filtra per quel tema.";
			d["item.themes.ex.menu"] = "tutti gli asset che clicchi nel menu Strade seguono il tema spuntato lì, ma passando al menu Elettricità no.";
			d["item.themes.ex.category"] = "la spunta di questa riga è lo stato del filtro della barra degli strumenti stesso, quindi viene ricordata per il menu e la categoria in cui ti trovi, e non per tipo di asset.";
			d["item.packs.label"] = "Pacchetto";
			d["item.packs.desc"] = "Ricorda quali voci sono spuntate nel filtro «Pacchetto» della barra degli strumenti; spuntarne una limita l'elenco della barra agli asset che appartengono a quei pacchetti.\nL'originale lo azzera ogni volta che cambi menu o categoria.";
			d["item.packs.ex.group"] = "per esempio, spunti un pacchetto in Strade/Strade piccole, l'originale lo azzera sulle strade grandi e ritorna quando ci torni.";
			d["item.packs.ex.menu"] = "i pacchetti spuntati nel menu Strade e nel menu Elettricità vengono ricordati separatamente.";
			d["item.packs.ex.category"] = "la spunta di questa riga è lo stato del filtro della barra degli strumenti stesso, ricordata per il menu e la categoria in cui ti trovi; l'originale la azzera ogni volta che cambi categoria.";

			// ---------- Modalità strumento ----------
			d["item.toolMode.label"] = "Modalità strumento";
			d["item.toolMode.ex.group"] = "per esempio, scegli Curva semplice su una piccola strada a due corsie e anche le altre piccole strade usano Curva semplice, mentre una strada grande no.";
			d["item.toolMode.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono su Curva semplice, ma passando al menu Elettricità no.";
			d["item.toolMode.ex.category"] = "passando a una strada a sei corsie resta Curva semplice, ma un ponte o un binario della metropolitana doppio ripartono dalla loro modalità predefinita e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Elevazione ----------
			d["item.elevation.label"] = "Elevazione";
			d["item.elevation.desc"] = "Ricorda l'elevazione a cui lasci lo strumento, incluso il risultato di Aumenta elevazione e Diminuisci elevazione.\nIl Livello elevazione (quanto sposta una singola pressione; Anarchy chiama questa riga Step di elevazione) viene ricordato insieme ad essa e usa lo stesso ambito di condivisione.";
			d["item.elevation.ex.group"] = "per esempio, porti una strada a due corsie a 10 m e anche le altre piccole strade vanno a 10 m, mentre una strada grande non è a 10 m.";
			d["item.elevation.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono a 10 m, ma passando al menu Elettricità non sono a 10 m.";
			d["item.elevation.ex.category"] = "passando a una strada a sei corsie restano 10 m, ma un ponte o un binario ferroviario doppio ripartono da 0 m e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Modalità parallelismo ----------
			d["item.parallel.label"] = "Modalità parallelismo";
			d["item.parallel.desc"] = "Ricorda le due voci del pannello Strada parallela: l'interruttore Modalità parallelismo e la distanza della Compensazione parallelismo.\nAttiva, accanto alla linea che tracci ne viene costruita una seconda; puoi regolare solo quanto sono distanziate. Spenta per impostazione predefinita, quindi una sola linea come l'originale.";
			d["item.parallel.ex.group"] = "per esempio, attivi la Modalità parallelismo su una piccola strada a due corsie e imposti la Compensazione parallelismo a 5 m, e anche le altre piccole strade tornano attive con gli stessi 5 m, mentre una strada grande no.";
			d["item.parallel.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono in parallelo con 5 m di compensazione, ma passando al menu Elettricità no.";
			d["item.parallel.ex.category"] = "passando a una strada a sei corsie resta attivo con gli stessi 5 m di compensazione, ma un ponte o un binario della metropolitana tornano a spento e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Aggancio ----------
			d["item.snap.label"] = "Aggancio";
			d["item.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio, per esempio ai lati di una strada, al centro di una strada, ai nodi, alla superficie di un oggetto o alla costa.\nQuali interruttori compaiono dipende dallo strumento; l'interruttore multiplo di tutta la riga non viene ricordato.";
			d["item.snap.ex.group"] = "per esempio, spegni «Aggancia ai lati di una strada» su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno spento, mentre una strada grande ce li ha tutti accesi.";
			d["item.snap.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di aggancio, ma passando al menu Elettricità no.";
			d["item.snap.ex.category"] = "passando a una strada a sei corsie l'insieme resta lo stesso, ma un ponte o un sentiero ritornano ai valori predefiniti e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Topografia ----------
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Ricorda la riga Topografia, cioè se Mostra le linee di contorno è attivo.";
			d["item.topography.ex.group"] = "per esempio, attivi la Topografia su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attiva, mentre una strada grande no.";
			d["item.topography.ex.menu"] = "tutti gli asset che clicchi nel menu Strade mostrano le linee di contorno, ma passando al menu Elettricità no.";
			d["item.topography.ex.category"] = "passando a una strada a sei corsie le linee restano, ma un ponte o i cavi elettrici hanno la riga spenta e necessitano un'impostazione propria.";


			// ---------- Sinistra e destra (Anarchy) ----------
			d["item.leftRight.label"] = "Sinistra e destra";
			d["item.leftRight.desc"] = "Ricorda quali potenziamenti di rete sono scelti sulle righe Sinistra e Destra del pannello di rete di Anarchy, per esempio Pista ciclabile, Erba, Banchina, Muro di contenimento e Barriera insonorizzante.\nRichiede Anarchy (mod Paradox 74604); senza, questa voce non compare.";
			d["item.leftRight.ex.group"] = "per esempio, scegli Pista ciclabile sul lato destro di una piccola strada a due corsie e anche le altre piccole strade ce l'hanno a destra, mentre una strada grande no.";
			d["item.leftRight.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso paio di scelte laterali, ma passando al menu Elettricità no.";
			d["item.leftRight.ex.category"] = "passando a una strada a sei corsie la pista ciclabile resta a destra, ma un ponte o un binario ferroviario doppio tornano a niente di scelto e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Generale (Anarchy) ----------
			d["item.general.label"] = "Generale";
			d["item.general.desc"] = "Ricorda gli interruttori della riga Generale del pannello di rete di Anarchy: Terreno, Elevato, Tunnel, Pendio costante, Mediana ampia e Intervallo di elevazione ampliato (che toglie il limite di altezza).\nRichiede Anarchy (mod Paradox 74604); senza, questa voce non compare.";
			d["item.general.ex.group"] = "per esempio, scegli Tunnel su una piccola strada a due corsie e anche le altre piccole strade vengono forzate in tunnel, mentre una strada grande no.";
			d["item.general.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di Generale, ma passando al menu Elettricità no.";
			d["item.general.ex.category"] = "passando a una strada a sei corsie resta Tunnel, ma un ponte o un binario della metropolitana doppio tornano a niente di spuntato e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Modalità sottosuolo ----------
			d["item.underground.label"] = "Modalità sottosuolo";
			d["item.underground.desc"] = "Ricorda l'interruttore Modalità sottosuolo, che decide se ciò che piazzi va sotto terra o sopra.";
			d["item.underground.ex.group"] = "per esempio, attivi la Modalità sottosuolo su una piccola strada a due corsie e anche le altre piccole strade vanno sottoterra, mentre una strada grande no.";
			d["item.underground.ex.menu"] = "tutti gli asset che clicchi nel menu Strade vengono costruiti sottoterra, ma passando al menu Elettricità no.";
			d["item.underground.ex.category"] = "passando a una strada a sei corsie si resta sottoterra, ma un ponte o un binario ferroviario doppio rimangono in superficie e necessitano un'impostazione propria, benché tutti siano nel menu Strade.";

			// ---------- Altro ----------

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（it-IT）====================
		d["tab.master"] = "Interruttore generale";
		d["tab.assets"] = "Asset";
		d["tab.functions"] = "Funzioni";
		d["tab.items"] = "Oggetti";
		d["group.domain"] = "Interruttori per pagina";
		d["domain.asset.label"] = "Ricorda le voci degli strumenti nella pagina Asset";
		d["domain.asset.desc"] = "Decide se le voci di questa pagina vengono ricordate:\nAttivo: le impostazioni vengono registrate mentre lavori e rimesse al caricamento di un salvataggio;\nSpento: non viene registrato né rimesso nulla, quindi non c'è alcun ambito di condivisione da impostare.\nQuesta pagina contiene strade, edifici e servizi cittadini.";
		d["domain.function.label"] = "Ricorda le voci degli strumenti nella pagina Funzioni";
		d["domain.function.desc"] = "Decide se le voci di questa pagina vengono ricordate:\nAttivo: le impostazioni vengono registrate mentre lavori e rimesse al caricamento di un salvataggio;\nSpento: non viene registrato né rimesso nulla, quindi non c'è alcun ambito di condivisione da impostare.\nQuesta pagina contiene zone, aree, percorsi e strumenti del terreno.";
		d["domain.item.label"] = "Ricorda le voci degli strumenti nella pagina Oggetti";
		d["domain.item.desc"] = "Decide se le voci di questa pagina vengono ricordate:\nAttivo: le impostazioni vengono registrate mentre lavori e rimesse al caricamento di un salvataggio;\nSpento: non viene registrato né rimesso nulla, quindi non c'è alcun ambito di condivisione da impostare.\nQuesta pagina contiene gli oggetti del menu Architettura del paesaggio, alberi compresi.";
		d["enabled.label"] = "Abilita la mod";
		d["enabled.desc"] = "Accende o spegne tutto il mod.\nAcceso, i tre interruttori qui sotto hanno effetto: ogni impostazione degli strumenti viene ricordata mentre lavori e rimessa al caricamento di un salvataggio.\nSpento, il mod resta in disparte e ogni strumento si comporta come nel gioco originale.";
		d["scope.category.function"] = "Stesso tipo";
		d["scope.category.item"] = "Stesso tipo di oggetto";
		d["item.color.label"] = "Colore";
		d["item.decoration.label"] = "Mantieni età";
		d["item.decoration.desc"] = "Ricorda l'interruttore Mantieni età: quando è attivo, gli alberi che pennelli smettono di crescere e di morire di vecchiaia.\nQuesta riga ce l'hanno solo gli alberi.";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Tipo di pennello";
		d["item.brushType.desc"] = "Ricorda quale pennello è selezionato nella riga Tipo di pennello: cerchio, quadrato, bordo morbido e così via, che determina la forma del bordo che dipingi.\nLa riga esiste solo sugli strumenti che possono dipingere: i nove strumenti di Terraformazione e lo strumento oggetti quando l'asset selezionato è pennellabile.";
		d["item.brushSize.label"] = "Dimensione pennello";
		d["item.brushSize.desc"] = "Ricorda la riga Dimensione pennello: quanta area copre una passata.\nLa riga esiste solo sugli strumenti che possono dipingere: i nove strumenti di Terraformazione e lo strumento oggetti quando l'asset selezionato è pennellabile.";
		d["item.brushStrength.label"] = "Robustezza pennello";
		d["item.brushStrength.desc"] = "Ricorda la riga Robustezza pennello: quanto cambia ogni passata.\nLa riga esiste solo sugli strumenti che possono dipingere: i nove strumenti di Terraformazione e lo strumento oggetti quando l'asset selezionato è pennellabile.";
		d["item.brushHeight.label"] = "Altezza bersaglio";
		d["item.brushHeight.desc"] = "Ricorda il valore della riga Altezza bersaglio: a che altezza finisce il terreno dopo che uno strumento di Terraformazione lo livella o lo trasforma in un pendio.\nLa riga esiste solo sui due strumenti di Terraformazione Strumento livellamento terreno e Strumento inclinazione terreno.";
		d["item.distance.label"] = "Distanza";
		d["item.distance.desc"] = "Ricorda il valore della riga Distanza: quanto distano tra loro oggetti consecutivi quando li posizioni lungo una linea o una curva.\nLa riga compare solo nelle modalità Linea e Curva.";
		d["item.ageMask.label"] = "Età";
		d["item.ageMask.desc"] = "Ricorda quali fasi di età sono spuntate nella riga Età: arbusto, giovane, adulto e anziano, anche più di una insieme.\nQuesta riga ce l'hanno solo gli alberi.";
		d["item.anarchyObjectElevation.label"] = "Elevazione dell'oggetto";
		d["item.anarchyObjectElevation.desc"] = "Ricorda la riga di controlli dell'altezza che Anarchy aggiunge al pannello dello strumento oggetti: blocco della trasformazione, elevazione, variazione di elevazione e step di aumento e diminuzione.\nTutti e quattro stanno su una sola riga e contano quindi come una sola voce.";
		d["item.f.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio nei pannelli delle funzioni: ad angoli di 90°, alla griglia della zona, alle strade, al lato del proprietario, mostra le linee di contorno e così via.\nQuali interruttori compaiono dipende dallo strumento; il pulsante collettivo di tutta la lista non viene ricordato.";
		d["item.f.topography.desc"] = "Ricorda l'attivazione della riga Topografia nei pannelli delle funzioni, cioè se le linee di contorno del terreno sono visibili mentre si disegnano le zone.";
		d["item.f.underground.desc"] = "Ricorda l'attivazione della riga Modalità sottosuolo nei pannelli delle funzioni.";
		d["item.i.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio nei pannelli degli oggetti: alla superficie di un oggetto, alla geometria vicina, alle strade, rimuovi solo il tipo corrispondente e così via, a seconda dell'oggetto.\nQuali interruttori compaiono dipende dall'oggetto.";
		d["item.i.toolMode.desc"] = "Ricorda la riga Modalità strumento nei pannelli degli oggetti: Posiziona uno, Posiziona multipli, Linea, Curva, Strumento timbro oggetti, Pittura e così via, a seconda dello strumento.";
		d["item.i.topography.desc"] = "Ricorda l'attivazione della riga Topografia nei pannelli degli oggetti.";
		d["item.i.underground.desc"] = "Ricorda l'attivazione della riga Modalità sottosuolo nei pannelli degli oggetti, cioè se gli oggetti vengono posizionati sotto o sopra il terreno.";

			d["ex.net.group"] = "Per esempio, tutte le strade del gruppo Strade piccole condividono un valore; Strade grandi è un altro gruppo.";
			d["ex.net.menu"] = "Per esempio, tutte le strade del menu Strade condividono un valore; il menu Elettricità no.";
			d["ex.net.category"] = "Per esempio, le strade con qualsiasi numero di corsie sono un unico tipo; i ponti e i binari sono tipi a sé.";
			d["ex.zone.group"] = "Per esempio, le abitazioni a bassa, media e alta densità di Zone residenziali formano un gruppo; Zone commerciali è un altro gruppo, anche se entrambe sono nello stesso menu.";
			d["ex.zone.menu"] = "Per esempio, Zone residenziali, Zone commerciali, Zone industriali e Zone uffici condividono un valore; un altro menu no.";
			d["ex.route.group"] = "Per esempio, le linee sono raggruppate per mezzo di trasporto (strada, tram, treno, metropolitana e così via); una linea di un altro mezzo è un altro gruppo.";
			d["ex.route.menu"] = "Per esempio, tutte le linee del menu Trasporti condividono un valore; un altro menu no.";
			d["ex.terrain.group"] = "La terraformazione ha un solo gruppo, quindi Stesso gruppo e Stesso menu scelgono esattamente gli stessi strumenti.";
			d["ex.terrain.menu"] = "La terraformazione ha un solo gruppo, quindi Stesso menu e Stesso gruppo scelgono esattamente gli stessi strumenti.";
			d["ex.prop.group"] = "Per esempio, il menu Architettura del paesaggio divide gli oggetti in Natura, Parco, residenziale, commerciale, industriale, detriti, decalcomanie e fioriere; una categoria condivide un valore e un'altra è un altro gruppo.";
			d["ex.prop.menu"] = "Tutti gli oggetti e gli alberi del menu Architettura del paesaggio condividono un valore.";
			d["ex.tree.group"] = "Tutti gli alberi sono nel gruppo Vegetazione del menu Architettura del paesaggio, quindi Stesso gruppo e Stesso menu scelgono gli stessi alberi.";
			d["ex.tree.menu"] = "Tutti gli alberi sono nel gruppo Vegetazione del menu Architettura del paesaggio, quindi Stesso menu e Stesso gruppo scelgono gli stessi alberi.";
			d["item.color.desc"] = "Ricorda la riga Colore degli strumenti linea: il colore con cui viene disegnata la linea.";
			d["item.f.snap.desc"] = "Ricorda gli interruttori della riga Aggancio degli strumenti funzione; le opzioni dipendono dallo strumento.\nIl pulsante del gioco per attivare o disattivare tutti gli agganci è un interruttore globale e non viene memorizzato.";
			d["item.f.topography.desc"] = "Ricorda la riga Topografia degli strumenti zona, terraformazione e altri: se sono visibili le curve di livello.";
			d["item.f.underground.desc"] = "Ricorda la riga Modalità sottosuolo degli strumenti area, linea, demolizione e oggetti.";
			d["item.i.topography.desc"] = "Ricorda la riga Topografia nei pannelli degli oggetti.";
			d["item.anarchyObjectElevation.desc"] = "Ricorda i controlli di altezza che Anarchy aggiunge al pannello: blocco trasformazione, altezza, variazione di altezza e il passo di alzata/abbassamento.\nSono sulla stessa riga, quindi vengono memorizzati e condivisi insieme.";
			d["scope.desc.function"] = "Quanto viene condiviso il valore di questa voce.";
			d["scope.desc.item"] = "Quanto viene condiviso il valore di questa voce.";
			d["scope.line.shared"] = "Globale: tutti gli edifici e i servizi che mostrano questa voce condividono un'unica impostazione.";
			d["scope.line.shared.function"] = "Globale: tutte le funzioni che mostrano questa voce condividono un'unica impostazione.";
			d["scope.line.shared.item"] = "Globale: tutti gli oggetti che mostrano questa voce condividono un'unica impostazione.";
			d["scope.line.unique"] = "Non condiviso: ogni edificio e servizio che mostra questa voce viene impostato separatamente.";
			d["scope.line.unique.function"] = "Non condiviso: ogni funzione che mostra questa voce viene impostata separatamente.";
			d["scope.line.unique.item"] = "Non condiviso: ogni oggetto che mostra questa voce viene impostato separatamente.";
			d["scope.note"] = "Gli ambiti di edifici e servizi, funzioni e oggetti sono indipendenti tra loro.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Questa riga vale solo per {0}.";
			d["fam.net.desc"] = "La famiglia delle strade: strade, binari, condutture e linee elettriche sono tutte incluse.";
			d["fam.net"] = "strade";
			d["fam.assetObj"] = "edifici";
			d["fam.funcObj"] = "marcatori";
			d["fam.zone"] = "zone";
			d["fam.area"] = "strumenti area";
			d["item.toolMode.desc"] = "Ricorda la modalità scelta su questi strumenti.";
			d["item.f.toolMode.desc"] = "Ricorda la riga Modalità strumento negli strumenti zona: Riempimento, Selezione e Pittura.";
			return d;
		}
	}
}
