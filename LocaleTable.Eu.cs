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
			d["scope.desc"] = "Wie weit der Wert dieses Punkts geteilt wird. Die Klammern in der Dropdown-Liste zeigen, was das Original tut, und markieren die vorgesehene Stufe.";
			d["scope.line.group"] = "Selbe Gruppe: {0}";
			d["scope.line.menu"] = "Selbes Menü: {0}";
			d["scope.line.category"] = "Selber Typ: gruppiert nach dem, was ein Werkzeug tatsächlich bedient, sodass Zahl der Spuren, Größe, Stufe und die Kategorie in der Werkzeugleiste keinen Unterschied machen (ein kleiner Parkplatz und ein großer Parkplatz sind derselbe Typ), aber Brücken, Kais und Kreuzungen bilden jeweils einen eigenen Typ, getrennt von den Straßen, also {0}";
			d["scope.line.shared"] = "Global geteilt: jedes Werkzeug dieser Seite, das diesen Punkt unterstützt, nutzt denselben Wert";
			d["scope.line.shared.function"] = "Global geteilt: jede Funktion dieser Seite, die diesen Punkt unterstützt, nutzt denselben Wert";
			d["scope.line.shared.item"] = "Global geteilt: jedes Requisit dieser Seite, das diesen Punkt unterstützt, nutzt denselben Wert";
			d["scope.line.unique"] = "Gar nicht geteilt: jedes Werkzeug dieser Seite, das diesen Punkt unterstützt, wird eigenständig eingestellt";
			d["scope.line.unique.function"] = "Gar nicht geteilt: jede Funktion dieser Seite, die diesen Punkt unterstützt, wird eigenständig eingestellt";
			d["scope.line.unique.item"] = "Gar nicht geteilt: jedes Requisit dieser Seite, das diesen Punkt unterstützt, wird eigenständig eingestellt";

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
			d["item.anarchy.desc"] = "Merkt sich den Schalter Anarchy, den der Anarchy-Mod hinzufügt; Anarchy ist im ganzen Spiel geteilt, dieser Punkt ist also global. Ohne den Anarchy-Mod wird dieser Punkt nicht angezeigt.";
			d["item.anarchy.ex.group"] = "zum Beispiel schaltest du Anarchy für eine zweispurige kleine Straße ein, andere kleine Straßen haben Anarchy ebenfalls an, eine große Straße dagegen nicht";
			d["item.anarchy.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, hat Anarchy an, im Menü Strom dagegen nicht";
			d["item.anarchy.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt Anarchy an, aber eine Brücke oder doppelte U-Bahn-Gleise gehen wieder aus und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			d["item.themes.label"] = "Thema";
			d["item.themes.desc"] = "Merkt, welche Einträge im Filter „Thema“ der Werkzeugleiste angehakt sind; ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die diese Themen unterstützen.\nDiese Zeile erscheint nur, wenn die aktuelle Kategorie wirklich Assets enthält, die Themen verwenden.";
			d["item.themes.ex.group"] = "zum Beispiel hakt du ein Thema nur unter Straßen/Kleine Straßen ein, eine große Straße filtert dann nicht danach";
			d["item.themes.ex.menu"] = "ein im Menü Straßen angehaktes Thema gilt im Menü Strom nicht und ist wieder da, wenn du zurückkehrst";
			d["item.themes.ex.category"] = "der Haken in dieser Zeile gehört zum Filter der Werkzeugleiste selbst, er wird also über das Menü und die Kategorie gemerkt, in denen du gerade bist, und nicht über den Asset-Typ";
			d["item.packs.label"] = "Paket";
			d["item.packs.desc"] = "Merkt, welche Einträge im Filter „Paket“ der Werkzeugleiste angehakt sind; ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die zu diesen Paketen gehören.\nDas Original leert die Auswahl jedes Mal, wenn du das Menü oder die Kategorie wechselst.";
			d["item.packs.ex.group"] = "zum Beispiel hakt du ein Paket unter Straßen/Kleine Straßen ein, das Original leert es bei einer großen Straße, und es ist wieder da, wenn du zurückkehrst";
			d["item.packs.ex.menu"] = "die im Menü Straßen angehakten Pakete und die im Menü Strom angehakten werden getrennt gemerkt";
			d["item.packs.ex.category"] = "der Haken in dieser Zeile gehört zum Filter der Werkzeugleiste selbst, er wird über das Menü und die Kategorie gemerkt, in denen du gerade bist; das Original leert ihn jedes Mal, wenn du die Kategorie wechselst";

			// ---------- Werkzeugmodus ----------
			d["item.toolMode.label"] = "Werkzeugmodus";
			d["item.toolMode.desc"] = "Merkt den Modus jedes Werkzeugs; die Optionen unterscheiden sich: Straßen, Gleise und Leitungen bieten Gerade, Einfache Kurve, Komplexe Kurve, Durchgehend, Raster, Ersetzen, Punkt.\nDie Zeile erscheint nur, wenn das gewählte Werkzeug mehr als einen Modus hat.";
			d["item.toolMode.ex.group"] = "zum Beispiel wählst du Einfache Kurve für eine zweispurige kleine Straße, andere kleine Straßen nutzen ebenfalls Einfache Kurve, eine große Straße dagegen nicht";
			d["item.toolMode.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, steht auf Einfache Kurve, im Menü Strom dagegen nicht";
			d["item.toolMode.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es Einfache Kurve, aber eine Brücke oder doppelte U-Bahn-Gleise fallen auf ihren Standardmodus zurück und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Höhe ----------
			d["item.elevation.label"] = "Höhe";
			d["item.elevation.desc"] = "Merkt die Höhe, mit der ein Werkzeug verlassen wurde, einschließlich des Ergebnisses von Erhöhen und Absenken.\nDer Höhenunterschied (wie weit dich ein einzelner Druck versetzt; Anarchy nennt diese Zeile Höhenschritt) wird zusammen mit der Höhe gemerkt und nutzt denselben gemeinsamen Bereich.";
			d["item.elevation.ex.group"] = "zum Beispiel hebst du eine zweispurige kleine Straße auf 10 m, andere kleine Straßen gehen ebenfalls auf 10 m, eine große Straße aber nicht auf 10 m";
			d["item.elevation.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, liegt auf 10 m, im Menü Strom dagegen nicht auf 10 m";
			d["item.elevation.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es bei 10 m, aber eine Brücke oder doppelte Bahngleise gehen zurück auf 0 m und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Parallelmodus ----------
			d["item.parallel.label"] = "Parallelmodus";
			d["item.parallel.desc"] = "Merkt die beiden Einträge im Panel Parallelstraße: den Schalter Parallelmodus und den Parallelabstand.\nIst er an, wird neben der gezogenen Linie eine zweite gebaut; einstellen lässt sich nur der Abstand. Standardmäßig aus, also eine Linie wie im Original.";
			d["item.parallel.ex.group"] = "zum Beispiel schaltest du den Parallelmodus für eine zweispurige kleine Straße ein und stellst den Parallelabstand auf 5 m, andere kleine Straßen kommen ebenfalls eingeschaltet mit denselben 5 m zurück, eine große Straße dagegen nicht";
			d["item.parallel.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, baut parallel mit 5 m Abstand, im Menü Strom dagegen nicht";
			d["item.parallel.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt er an und der Abstand bleibt bei 5 m, aber eine Brücke oder ein U-Bahn-Gleis geht aus und braucht eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Einrasten ----------
			d["item.snap.label"] = "Einrasten";
			d["item.snap.desc"] = "Merkt jeden Schalter der Zeile Einrasten, etwa an Straßenkanten, in der Mitte einer Straße, an Knoten, an der Oberfläche eines Objekts oder an der Küste.\nWelche Schalter erscheinen, hängt vom Werkzeug ab; der Sammlerschalter für die ganze Zeile wird selbst nicht gemerkt.";
			d["item.snap.ex.group"] = "zum Beispiel schaltest du „An Straßenkanten einrasten“ für eine zweispurige kleine Straße aus, andere kleine Straßen haben es ebenfalls aus, eine große Straße hat dagegen alles an";
			d["item.snap.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Einrast-Schaltern, im Menü Strom dagegen nicht";
			d["item.snap.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt dieselbe Reihe erhalten, aber eine Brücke oder ein Spazierweg geht auf die Standardwerte zurück und braucht eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Topographie ----------
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Merkt die Zeile Topographie, also ob Nur Höhenlinien anzeigen aktiv ist.";
			d["item.topography.ex.group"] = "zum Beispiel schaltest du die Topographie für eine zweispurige kleine Straße ein, andere kleine Straßen haben sie ebenfalls an, eine große Straße dagegen nicht";
			d["item.topography.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, zeigt Höhenlinien, im Menü Strom dagegen nicht";
			d["item.topography.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleiben die Höhenlinien sichtbar, aber bei einer Brücke oder einer Stromleitung ist die Zeile aus und sie braucht eine eigene Einstellung";


			// ---------- Links und Rechts (Anarchy) ----------
			d["item.leftRight.label"] = "Links und Rechts";
			d["item.leftRight.desc"] = "Merkt sich, welche Netzwerkerweiterungen auf den Zeilen „Links“ und „Rechts“ im Netzwerkpanel von Anarchy gewählt sind, zum Beispiel Fahrradweg, Rasen, Kai, Stützwand und Schallschutzmauer.\nBenötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod wird dieser Punkt nicht angezeigt.";
			d["item.leftRight.ex.group"] = "zum Beispiel wählst du auf der rechten Seite einer zweispurigen kleinen Straße den Fahrradweg, andere kleine Straßen haben rechts ebenfalls den Fahrradweg, eine große Straße dagegen nicht";
			d["item.leftRight.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dasselbe Paar Seitenauswahlen, im Menü Strom dagegen nicht";
			d["item.leftRight.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt rechts der Fahrradweg, aber bei einer Brücke oder doppelten Bahngleisen ist wieder nichts ausgewählt und sie brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Allgemein (Anarchy) ----------
			d["item.general.label"] = "Allgemein";
			d["item.general.desc"] = "Merkt sich die Schalter auf der Zeile „Allgemein“ im Netzwerkpanel von Anarchy: Boden, Erhöht, Tunnel, Konstante Neigung, Breiter Trennstreifen und Erweiterter Höhenbereich (setzt die Höhenbegrenzung außer Kraft).\nBenötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod wird dieser Punkt nicht angezeigt.";
			d["item.general.ex.group"] = "zum Beispiel wählst du Tunnel für eine zweispurige kleine Straße, andere kleine Straßen werden ebenfalls in Tunnel gezwungen, eine große Straße dagegen nicht";
			d["item.general.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Allgemein-Schaltern, im Menü Strom dagegen nicht";
			d["item.general.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es Tunnel, aber bei einer Brücke oder doppelten U-Bahn-Gleisen ist wieder nichts angehakt und sie brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Untergrundmodus ----------
			d["item.underground.label"] = "Untergrundmodus";
			d["item.underground.desc"] = "Merkt den Schalter Untergrundmodus: ob du etwas unter der Erde oder darüber platzierst.";
			d["item.underground.ex.group"] = "zum Beispiel schaltest du den Untergrundmodus für eine zweispurige kleine Straße ein, andere kleine Straßen gehen ebenfalls unter die Erde, eine große Straße dagegen nicht";
			d["item.underground.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, wird unter der Erde gebaut, im Menü Strom dagegen nicht";
			d["item.underground.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es unter der Erde, aber eine Brücke oder doppelte Bahngleise bleiben an der Oberfläche und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Sonstiges ----------
			d["item.other.label"] = "Sonstiges";
			d["item.other.desc"] = "Merkt die übrigen Zeilen, die ein Werkzeugpanel aufzeichnen kann: die Straßenfarben der Zeile Farbe.";
			d["item.other.ex.group"] = "zum Beispiel färbst du eine zweispurige kleine Straße blau, andere kleine Straßen werden ebenfalls blau, eine große Straße dagegen nicht";
			d["item.other.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Farbe, im Menü Strom dagegen nicht";
			d["item.other.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es blau, aber eine Brücke oder doppelte Bahngleise behalten ihre eigene Aufzeichnung und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（de-DE）====================
		d["tab.master"] = "Hauptschalter";
		d["tab.assets"] = "Assets";
		d["tab.functions"] = "Funktionen";
		d["tab.items"] = "Requisiten";
		d["group.domain"] = "Schalter pro Seite";
		d["enabled.label"] = "Mod aktivieren";
		d["enabled.desc"] = "Standardmäßig aus: frisch installiert tut der Mod nichts, alles läuft wie im Original.\nEingeschaltet wird jedes Werkzeugpanel aufgezeichnet und beim Laden des Spielstands zurückgegeben. Er schaltet sich von selbst ein, sobald einer der drei Seitenschalter unten an ist.";
		d["domain.asset.label"] = "Werkzeugpunkte der Seite „Assets“ merken";
		d["domain.asset.desc"] = "An: die Werkzeugeinstellungen dieser Seite werden laufend aufgezeichnet und kehren beim Laden des Spielstands zurück.\nAus: von dieser Seite wird nichts aufgezeichnet und nichts wiederhergestellt; die Seite wird ausgegraut und die Werkzeuge laufen wie im Original.";
		d["domain.function.label"] = "Werkzeugpunkte der Seite „Funktionen“ merken";
		d["domain.function.desc"] = "An: die Werkzeugeinstellungen dieser Seite werden laufend aufgezeichnet und kehren beim Laden des Spielstands zurück.\nAus: von dieser Seite wird nichts aufgezeichnet und nichts wiederhergestellt; die Seite wird ausgegraut und die Werkzeuge laufen wie im Original.";
		d["domain.item.label"] = "Werkzeugpunkte der Seite „Requisiten“ merken";
		d["domain.item.desc"] = "An: die Werkzeugeinstellungen dieser Seite werden laufend aufgezeichnet und kehren beim Laden des Spielstands zurück.\nAus: von dieser Seite wird nichts aufgezeichnet und nichts wiederhergestellt; die Seite wird ausgegraut und die Werkzeuge laufen wie im Original.";
		d["scope.category.function"] = "Selber Typ";
		d["scope.category.item"] = "Selbe Requisitenart";
		d["scope.line.sharedSingle"] = "Global geteilt: jedes Werkzeug auf dieser Seite, das diesen Punkt unterstützt, nutzt denselben Wert";
			d["scope.note"] = "Hinweis: Jede Stufe oben gilt nur innerhalb dieser Seite.";
		d["item.color.label"] = "Farbe";
		d["item.decoration.label"] = "Alter beibehalten";
		d["item.decoration.desc"] = "Merkt den Schalter „Alter beibehalten“: ist er an, wachsen die Bäume, die du anmalst oder setzt, nicht weiter und sterben auch nicht mehr an Altersschwäche.\nDie Zeile erscheint nur bei Assets mit Baumdaten, also beim Malen von Bäumen im Menü „Vegetation“.";
		d["item.decoration.ex.group"] = "zum Beispiel schaltest du „Alter beibehalten“ beim Malen von Bäumen ein, die anderen Bäume derselben Gruppe sind ebenfalls an, in einer anderen Gruppe dagegen nicht";
		d["item.decoration.ex.menu"] = "jeder Baum im Menü „Vegetation“ behält den Schalter an, in einem anderen Menü dagegen nicht";
		d["item.decoration.ex.category"] = "wechselst du zu einer anderen Baumart, bleibt er an, doch diese Zeile gibt es nur bei Assets mit Baumdaten – beim Malen von Markierungen fehlt sie ganz";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Pinseltyp";
		d["item.brushType.desc"] = "Merkt, welcher Pinsel in der Zeile „Pinseltyp“ gewählt ist: Kreis, Quadrat, weiche Kante und so weiter – das bestimmt die Form der Kante, die du malst.\nDie Zeile gibt es nur bei Werkzeugen, die malen können: bei den neun Terraforming-Werkzeugen und beim Objektwerkzeug, wenn das gewählte Asset malbar ist.";
		d["item.brushType.ex.group"] = "zum Beispiel wählst du beim Malen einen quadratischen Pinsel, die anderen malbaren Assets derselben Gruppe nutzen dann ebenfalls den quadratischen Pinsel, die einer anderen Gruppe dagegen nicht";
		d["item.brushType.ex.menu"] = "jedes malbare Asset im selben Menü nutzt denselben Pinsel, in einem anderen Menü dagegen nicht";
		d["item.brushType.ex.category"] = "wechselst du zu einer anderen Kategorie, gilt deren eigener Pinsel, auch wenn beide gemalt werden können";
		d["item.brushSize.label"] = "Pinselgröße";
		d["item.brushSize.desc"] = "Merkt die Zeile „Pinselgröße“: wie weit ein Strich reicht.\nDie Zeile gibt es nur bei Werkzeugen, die malen können: bei den neun Terraforming-Werkzeugen und beim Objektwerkzeug, wenn das gewählte Asset malbar ist.";
		d["item.brushSize.ex.group"] = "zum Beispiel stellst du einen Pinsel von 30 m ein, die anderen malbaren Assets derselben Gruppe sind dann ebenfalls 30 m, die einer anderen Gruppe dagegen nicht";
		d["item.brushSize.ex.menu"] = "jedes malbare Asset im selben Menü nutzt dieselbe Pinselgröße, in einem anderen Menü dagegen nicht";
		d["item.brushSize.ex.category"] = "wechselst du zu einer anderen Kategorie, gilt deren eigene Pinselgröße, auch wenn beide gemalt werden können";
		d["item.brushStrength.label"] = "Pinselstärke";
		d["item.brushStrength.desc"] = "Merkt die Zeile „Pinselstärke“: wie viel ein Strich verändert.\nDie Zeile gibt es nur bei Werkzeugen, die malen können: bei den neun Terraforming-Werkzeugen und beim Objektwerkzeug, wenn das gewählte Asset malbar ist.";
		d["item.brushStrength.ex.group"] = "zum Beispiel stellst du die Pinselstärke auf die Hälfte, die anderen malbaren Assets derselben Gruppe stehen dann ebenfalls auf der Hälfte, die einer anderen Gruppe dagegen nicht";
		d["item.brushStrength.ex.menu"] = "jedes malbare Asset im selben Menü nutzt dieselbe Pinselstärke, in einem anderen Menü dagegen nicht";
		d["item.brushStrength.ex.category"] = "wechselst du zu einer anderen Kategorie, gilt deren eigene Pinselstärke, auch wenn beide gemalt werden können";
		d["item.brushHeight.label"] = "Zielhöhe";
		d["item.brushHeight.desc"] = "Merkt den Wert der Zeile „Zielhöhe“: auf welcher Höhe das Gelände am Ende steht, nachdem ein Terraforming-Werkzeug es eingeebnet oder zu einer Böschung gedrückt hat.\nDie Zeile gibt es nur bei den beiden Terraforming-Werkzeugen „Terrain angleichen“ und „Terrainsteigung“.";
		d["item.brushHeight.ex.group"] = "zum Beispiel setzt du beim Einebnen die Zielhöhe auf 50 m, die anderen Terraforming-Werkzeuge derselben Gruppe nutzen dann ebenfalls 50 m, die einer anderen Gruppe dagegen nicht";
		d["item.brushHeight.ex.menu"] = "jedes Werkzeug der Terraforming-Gruppe nutzt dieselbe Zielhöhe, eine andere Funktionsgruppe dagegen nicht";
		d["item.brushHeight.ex.category"] = "wechselst du zu einer anderen Funktionsgruppe, gilt deren eigene Zielhöhe; die übrigen Terraforming-Werkzeuge haben die Zeile gar nicht";
		d["item.distance.label"] = "Distanz";
		d["item.distance.desc"] = "Merkt den Wert der Zeile „Distanz“: wie weit aufeinanderfolgende Objekte auseinanderliegen, wenn du sie entlang einer Linie oder einer Kurve setzt.\nDie Zeile erscheint nur im Linien- und im Kurvenmodus.";
		d["item.distance.ex.group"] = "zum Beispiel stellst du die Distanz auf 10 m, die anderen Requisiten derselben Gruppe liegen dann ebenfalls 10 m auseinander, die einer anderen Gruppe dagegen nicht";
		d["item.distance.ex.menu"] = "jedes Asset, das im selben Menü entlang einer Linie gesetzt werden kann, nutzt dieselbe Distanz, in einem anderen Menü dagegen nicht";
		d["item.distance.ex.category"] = "wechselst du zu einer anderen Kategorie, gilt deren eigene Distanz, auch wenn beide entlang einer Linie gesetzt werden können";
		d["item.ageMask.label"] = "Alter";
		d["item.ageMask.desc"] = "Merkt, welche Altersstufen in der Zeile „Alter“ angehakt sind: Setzling, Jung, Ausgewachsen und Alt, wobei mehrere gleichzeitig angehakt sein können.\nDie Zeile gibt es nur bei Baum-Assets, also beim Malen oder Setzen von Bäumen im Menü „Vegetation“.";
		d["item.ageMask.ex.group"] = "zum Beispiel haken Sie beim Malen von Bäumen nur Ausgewachsen an, die anderen Bäume derselben Gruppe erzeugen dann ebenfalls nur ausgewachsene, die einer anderen Gruppe dagegen nicht";
		d["item.ageMask.ex.menu"] = "jeder Baum im Menü „Vegetation“ nutzt dieselben Altersstufen, in einem anderen Menü dagegen nicht";
		d["item.ageMask.ex.category"] = "wechselst du zu einer anderen Baumart, bleiben diese Stufen erhalten, doch die Zeile gibt es nur bei Baum-Assets – beim Malen von Markierungen fehlt sie ganz";
		d["item.anarchyObjectElevation.label"] = "Objekthöhe";
		d["item.anarchyObjectElevation.desc"] = "Merkt die Reihe von Höhensteuerelementen, die Anarchy an das Panel des Objektwerkzeugs anhängt: Transformationsfixierung, Höhe, Höhenvarianz und Höhenschritt.\nAlle vier stehen in einer Reihe und zählen als ein Punkt.";
		d["item.anarchyObjectElevation.ex.group"] = "zum Beispiel stellst du die Höhe auf 15 m mit 2 m Varianz ein, die anderen Requisiten derselben Gruppe nutzen dieselben Werte, die einer anderen Gruppe dagegen nicht";
		d["item.anarchyObjectElevation.ex.menu"] = "jede Requisite im selben Menü nutzt dieselbe Höhe, in einem anderen Menü dagegen nicht";
		d["item.anarchyObjectElevation.ex.category"] = "wechselst du zu einer anderen Kategorie, gilt deren eigene Höhe";
		d["item.f.snap.desc"] = "Merkt jeden Schalter der Zeile „Einrasten“ auf Funktionspanels: bei 90-Grad-Winkeln, an Zellenraster, an Straßen, an der Seite des Besitzers, nur Höhenlinien anzeigen und so weiter.\nWelche Schalter erscheinen, hängt vom Werkzeug ab; der Sammlerschalter für die ganze Liste wird selbst nicht gemerkt.";
		d["item.f.snap.ex.group"] = "zum Beispiel schaltest du „An Zellenraster einrasten“ für die Wohnzone aus, Gewerbe-, Industrie- und Bürozone haben es dann ebenfalls aus, die Flächenwerkzeuge dagegen nicht";
		d["item.f.snap.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe nutzt dieselbe Reihe von Schaltern, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.snap.ex.category"] = "wechselst du zu einer anderen Funktionsgruppe, gelten deren eigene Standardschalter, auch wenn beide über dasselbe Menü der Werkzeugleiste erreichbar sind";
		d["item.f.toolMode.desc"] = "Merkt die Auswahl der Zeile „Werkzeugmodus“ auf Funktionspanels; die Optionen unterscheiden sich je Werkzeug: Zonentypen bieten Füllen, Auswahlrechteck und Farbe, Flächenwerkzeuge Bearbeiten und Kartenraster erstellen, jedes Terraforming-Werkzeug eigene Modi.";
		d["item.f.toolMode.ex.group"] = "zum Beispiel wählst du „Farbe“ für die Wohnzone, Gewerbe-, Industrie- und Bürozone stehen dann ebenfalls auf „Farbe“, die Flächenwerkzeuge dagegen nicht";
		d["item.f.toolMode.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe nutzt denselben Modus, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.toolMode.ex.category"] = "wechselst du zu einer anderen Funktionsgruppe, gilt wieder deren eigener Modus, auch wenn beide über dasselbe Menü der Werkzeugleiste erreichbar sind";
		d["item.f.topography.desc"] = "Merkt den Schalter der Zeile „Topographie“ auf Funktionspanels, also ob du beim Ziehen von Zonen die Höhenlinien des Geländes siehst.";
		d["item.f.topography.ex.group"] = "zum Beispiel haken Sie die Höhenlinien für die Wohnzone an, Gewerbe-, Industrie- und Bürozone sind dann ebenfalls angehakt";
		d["item.f.topography.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe bleibt angehakt, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.topography.ex.category"] = "wechselst du zu einer anderen Funktionsgruppe, gilt deren eigene Einstellung";
		d["item.f.underground.desc"] = "Merkt den Schalter der Zeile „Untergrundmodus“ auf Funktionspanels.";
		d["item.f.underground.ex.group"] = "zum Beispiel schalten Sie den Untergrundmodus für die Wohnzone ein, Gewerbe-, Industrie- und Bürozone liegen dann ebenfalls unter der Erde";
		d["item.f.underground.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe bleibt unter der Erde, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.underground.ex.category"] = "wechselst du zu einer anderen Funktionsgruppe, gilt deren eigene Einstellung";
		d["item.i.snap.desc"] = "Merkt jeden Schalter der Zeile „Einrasten“ auf Requisitenpanels: an der Oberfläche eines Objekts, an nahegelegener Geometrie, an Straßen, nur entsprechenden Typ entfernen und so weiter, je nach Requisite.\nWelche Schalter erscheinen, hängt von der Requisite ab.";
		d["item.i.snap.ex.group"] = "zum Beispiel schalten Sie „An der Oberfläche eines Objekts einrasten“ für eine Park-Requisite aus, die anderen Requisiten derselben Gruppe haben es dann ebenfalls aus, die einer anderen Gruppe nicht";
		d["item.i.snap.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ nutzt dieselbe Reihe von Schaltern, in einem anderen Menü dagegen nicht";
		d["item.i.snap.ex.category"] = "wechselst du zu einer anderen Requisitenkategorie, gelten deren eigene Standardschalter, obwohl beide im Menü „Landschaftsbau“ liegen";
		d["item.i.toolMode.desc"] = "Merkt die Auswahl der Zeile „Werkzeugmodus“ auf Requisitenpanels: Einzeln platzieren, Mehrfach platzieren, Linie, Kurve, Objektstempel-Werkzeug, Malen und so weiter, je nach Werkzeug.";
		d["item.i.toolMode.ex.group"] = "zum Beispiel wählen Sie „Mehrfach platzieren“ für eine Natur-Requisite, die anderen Requisiten derselben Gruppe nutzen dann ebenfalls „Mehrfach platzieren“, die einer anderen Gruppe nicht";
		d["item.i.toolMode.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ steht auf diesem Modus, in einem anderen Menü dagegen nicht";
		d["item.i.toolMode.ex.category"] = "wechselst du zu einer anderen Requisitenkategorie, etwa von Natur zu Markierungen, gilt wieder deren eigener Modus, obwohl beide im Menü „Landschaftsbau“ liegen";
		d["item.i.topography.desc"] = "Merkt den Schalter der Zeile „Topographie“ auf Requisitenpanels.";
		d["item.i.topography.ex.group"] = "zum Beispiel haken Sie die Höhenlinien beim Setzen von Requisiten an, die anderen Requisiten derselben Gruppe sind dann ebenfalls angehakt";
		d["item.i.topography.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ bleibt angehakt, in einem anderen Menü dagegen nicht";
		d["item.i.topography.ex.category"] = "wechselst du zu einer anderen Requisitenkategorie, gilt deren eigene Einstellung";
		d["item.i.underground.desc"] = "Merkt den Schalter der Zeile „Untergrundmodus“ auf Requisitenpanels, also ob du Requisiten unter- oder oberirdisch setzt.";
		d["item.i.underground.ex.group"] = "zum Beispiel schalten Sie den Untergrundmodus für eine Requisite ein, die anderen Requisiten derselben Gruppe gehen dann ebenfalls unter die Erde";
		d["item.i.underground.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ landet unter der Erde, in einem anderen Menü dagegen nicht";
		d["item.i.underground.ex.category"] = "wechselst du zu einer anderen Requisitenkategorie, gilt deren eigene Einstellung";
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
			d["scope.desc"] = "Hasta qué punto se comparte el valor de este elemento. Los paréntesis de la lista desplegable indican qué hace el juego original y marcan el nivel previsto.";
			d["scope.line.group"] = "Mismo grupo: {0}";
			d["scope.line.menu"] = "Mismo menú: {0}";
			d["scope.line.category"] = "Mismo tipo: se agrupa según a quién sirve de verdad la herramienta, así que el número de carriles, el tamaño, el nivel y la categoría de la barra de herramientas no suponen ninguna diferencia (un aparcamiento pequeño y uno grande son el mismo tipo), pero los puentes, los atracaderos y los intercambiadores forman cada uno su propio tipo, aparte de las carreteras, es decir {0}";
			d["scope.line.shared"] = "Global: cada herramienta de esta página que admite este elemento usa un mismo valor";
			d["scope.line.shared.function"] = "Global: cada función de esta página que admite este elemento usa un mismo valor";
			d["scope.line.shared.item"] = "Global: cada objeto de esta página que admite este elemento usa un mismo valor";
			d["scope.line.unique"] = "Sin compartir: cada herramienta de esta página que admite este elemento se ajusta por separado";
			d["scope.line.unique.function"] = "Sin compartir: cada función de esta página que admite este elemento se ajusta por separado";
			d["scope.line.unique.item"] = "Sin compartir: cada objeto de esta página que admite este elemento se ajusta por separado";

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
			d["item.anarchy.desc"] = "Recuerda el estado del interruptor Anarchy que añade el mod Anarchy; Anarchy se comparte en todo el juego, así que este elemento es global.\nSin el mod Anarchy este elemento no aparece.";
			d["item.anarchy.ex.group"] = "por ejemplo, activas Anarchy en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen activado, mientras que una carretera grande no";
			d["item.anarchy.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras tienen Anarchy activado, pero al pasar al menú de Electricidad ya no";
			d["item.anarchy.ex.category"] = "cambiar a una carretera de seis carriles sigue teniéndolo activado, pero un puente o unas vías de metro dobles lo dejan desactivado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			d["item.themes.label"] = "Temática";
			d["item.themes.desc"] = "Recuerda qué casillas están marcadas en el filtro «Temática» de la barra de herramientas; marcar una limita la lista de la barra a los assets que admiten esos temas.\nEsa fila solo aparece cuando la categoría actual contiene de verdad assets que usan temas.";
			d["item.themes.ex.group"] = "por ejemplo, marcas un tema solo en Carreteras/Carreteras pequeñas, de modo que una carretera grande ya no filtra por él";
			d["item.themes.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras siguen el tema marcado allí, pero al pasar al menú de Electricidad no";
			d["item.themes.ex.category"] = "lo marcado en esta fila es estado del propio filtro de la barra de herramientas, así que se recuerda por el menú y la categoría en los que estés, no por el tipo de activo";
			d["item.packs.label"] = "Paquete";
			d["item.packs.desc"] = "Recuerda qué casillas están marcadas en el filtro «Paquete» de la barra de herramientas; marcar una limita la lista de la barra a los assets de esos paquetes.\nEl juego original los vacía cada vez que cambias de menú o de categoría.";
			d["item.packs.ex.group"] = "por ejemplo, marcas un paquete en Carreteras/Carreteras pequeñas, el juego original lo borra en las carreteras grandes y vuelve al regresar";
			d["item.packs.ex.menu"] = "los paquetes marcados en el menú de Carreteras y en el menú de Electricidad se recuerdan por separado";
			d["item.packs.ex.category"] = "lo marcado en esta fila es estado del propio filtro de la barra de herramientas, que se recuerda por el menú y la categoría en los que estés; el juego original la vacía cada vez que cambias de categoría";

			// ---------- Herramientas ----------
			d["item.toolMode.label"] = "Herramientas";
			d["item.toolMode.desc"] = "Recuerda el modo elegido para cada herramienta; las opciones cambian según la herramienta: carreteras, vías y tuberías ofrecen Recta, Curva sencilla, Curva compleja, Continua, Cuadrícula, Reemplazar y Punto.\nLa fila solo aparece cuando la herramienta elegida tiene más de un modo.";
			d["item.toolMode.ex.group"] = "por ejemplo, eliges Curva sencilla en una carretera pequeña de dos carriles y las otras carreteras pequeñas también usan Curva sencilla, mientras que una carretera grande no";
			d["item.toolMode.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en Curva sencilla, pero al pasar al menú de Electricidad ya no";
			d["item.toolMode.ex.category"] = "cambiar a una carretera de seis carriles sigue dando Curva sencilla, pero un puente o unas vías de metro dobles vuelven a su modo predeterminado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Elevación ----------
			d["item.elevation.label"] = "Elevación";
			d["item.elevation.desc"] = "Recuerda la elevación con la que dejaste la herramienta, incluido el resultado de Aumentar la elevación y Disminuir la elevación.\nEl Escalón de elevación (lo que avanza con una sola pulsación; Anarchy llama a esa fila Paso de Elevación) se recuerda junto con ella y usa el mismo ámbito compartido.";
			d["item.elevation.ex.group"] = "por ejemplo, subes una carretera de dos carriles a 10 m y las otras carreteras pequeñas también quedan a 10 m, mientras que una carretera grande no está a 10 m";
			d["item.elevation.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están a 10 m, pero al pasar al menú de Electricidad no están a 10 m";
			d["item.elevation.ex.category"] = "cambiar a una carretera de seis carriles sigue dando 10 m, pero un puente o unas vías de tren dobles vuelven a 0 m y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Modo paralelo ----------
			d["item.parallel.label"] = "Modo paralelo";
			d["item.parallel.desc"] = "Recuerda las dos cosas del panel Calle paralela: el interruptor Modo paralelo y la separación de la Compensación paralela.\nAl activarlo se construye una segunda línea junto a la que trazas; solo puedes ajustar lo separadas que están. Desactivado por defecto, es decir una sola línea como el original.";
			d["item.parallel.ex.group"] = "por ejemplo, activas el Modo paralelo en una carretera pequeña de dos carriles y pones la Compensación paralela en 5 m, las otras carreteras pequeñas vuelven activadas con los mismos 5 m, mientras que una carretera grande no";
			d["item.parallel.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en paralelo con 5 m de compensación, pero al pasar al menú de Electricidad ya no";
			d["item.parallel.ex.category"] = "cambiar a una carretera de seis carriles sigue activado con los mismos 5 m de compensación, pero un puente o una vía de metro se desactiva y necesita su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Ajustar ----------
			d["item.snap.label"] = "Ajustar";
			d["item.snap.desc"] = "Recuerda cada interruptor de la fila Ajustar, por ejemplo a los lados de una carretera, a la mitad de la carretera, a los nodos, a la superficie de un objeto o a la orilla.\nQué interruptores aparecen depende de la herramienta; el interruptor múltiple de toda la fila no se recuerda.";
			d["item.snap.ex.group"] = "por ejemplo, desactivas «Ajustar a los lados de una carretera» en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen desactivado, mientras que una carretera grande lo tiene todo activado";
			d["item.snap.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de ajuste, pero al pasar al menú de Electricidad no";
			d["item.snap.ex.category"] = "cambiar a una carretera de seis carriles mantiene el mismo conjunto, pero un puente o un camino vuelve a los valores predeterminados y necesita su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Topografía ----------
			d["item.topography.label"] = "Topografía";
			d["item.topography.desc"] = "Recuerda la fila Topografía, es decir, si Muestra las líneas de contorno está activo.";
			d["item.topography.ex.group"] = "por ejemplo, activas Topografía en una carretera pequeña de dos carriles y las otras carreteras pequeñas también la tienen activada, mientras que una carretera grande no";
			d["item.topography.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras muestran las líneas de contorno, pero al pasar al menú de Electricidad no";
			d["item.topography.ex.category"] = "cambiar a una carretera de seis carriles sigue mostrándolas, pero un puente o un cable de baja tensión tienen la fila desactivada y necesitan su propio ajuste";


			// ---------- Izquierda y derecha (Anarchy) ----------
			d["item.leftRight.label"] = "Izquierda y derecha";
			d["item.leftRight.desc"] = "Recuerda las mejoras de red elegidas en las filas Izquierda y Derecha del panel de red de Anarchy, por ejemplo Vía ciclista, Césped, Muelle, Muro de Retención y Barrera acústica.\nNecesita Anarchy (mod 74604 de Paradox); sin él este elemento no aparece.";
			d["item.leftRight.ex.group"] = "por ejemplo, eliges Vía ciclista en el lado derecho de una carretera pequeña de dos carriles y las otras carreteras pequeñas también la reciben a la derecha, mientras que una carretera grande no";
			d["item.leftRight.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo par de selecciones laterales, pero al pasar al menú de Electricidad no";
			d["item.leftRight.ex.category"] = "cambiar a una carretera de seis carriles sigue teniendo Vía ciclista a la derecha, pero un puente o unas vías de tren dobles vuelven a no tener nada elegido y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- General (Anarchy) ----------
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Recuerda los interruptores de la fila General del panel de red de Anarchy: Terreno, Elevado, Túnel, Pendiente Constante, Mediana Amplia y Rango de Elevación Expandido (que quita el límite de altura).\nNecesita Anarchy (mod 74604 de Paradox); sin él este elemento no aparece.";
			d["item.general.ex.group"] = "por ejemplo, eliges Túnel en una carretera pequeña de dos carriles y las otras carreteras pequeñas también se obligan a ser túneles, mientras que una carretera grande no";
			d["item.general.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de General, pero al pasar al menú de Electricidad no";
			d["item.general.ex.category"] = "cambiar a una carretera de seis carriles sigue dando Túnel, pero un puente o unas vías de metro dobles vuelven a no tener nada marcado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Modo subterráneo ----------
			d["item.underground.label"] = "Modo subterráneo";
			d["item.underground.desc"] = "Recuerda el interruptor Modo subterráneo, que decide si lo que colocas va bajo tierra o encima.";
			d["item.underground.ex.group"] = "por ejemplo, activas el Modo subterráneo en una carretera pequeña de dos carriles y las otras carreteras pequeñas también van bajo tierra, mientras que una carretera grande no";
			d["item.underground.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras se construyen bajo tierra, pero al pasar al menú de Electricidad no";
			d["item.underground.ex.category"] = "cambiar a una carretera de seis carriles sigue yendo bajo tierra, pero un puente o unas vías de tren dobles se quedan en la superficie y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Otros ----------
			d["item.other.label"] = "Otros";
			d["item.other.desc"] = "Recuerda las demás filas que un panel de herramienta puede registrar: los colores de carretera de la fila Color.";
			d["item.other.ex.group"] = "por ejemplo, pones el color de una carretera pequeña de dos carriles en azul y las otras carreteras pequeñas también se vuelven azules, mientras que una carretera grande no";
			d["item.other.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo color, pero al pasar al menú de Electricidad no";
			d["item.other.ex.category"] = "cambiar a una carretera de seis carriles sigue siendo azul, pero un puente o unas vías de tren dobles guardan su propia copia y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（es-ES）====================
		d["enabled.label"] = "Activar el mod";
			d["enabled.desc"] = "Desactivado por defecto: recién instalado, el mod no hace nada y todo funciona como el juego original.\nAl activarlo se registran los paneles de herramienta y vuelven al cargar una partida. Se activa solo en cuanto uno de los tres interruptores por página de abajo está activado.";
		d["scope.line.sharedSingle"] = "Global: todas las herramientas de esta página que admiten este elemento usan un mismo valor";
			d["scope.note"] = "Nota: cada nivel de arriba solo se aplica dentro de esta página.";
		d["domain.asset.desc"] = "Activado: los ajustes de herramienta de esta página se registran en tiempo real y vuelven al cargar una partida.\nDesactivado: de esta página no se registra nada ni se restaura nada; la página queda en gris y las herramientas se comportan como en el juego original.";
		d["domain.asset.label"] = "Recordar los elementos de herramienta de los activos";
		d["domain.function.desc"] = "Activado: los ajustes de herramienta de esta página se registran en tiempo real y vuelven al cargar una partida.\nDesactivado: de esta página no se registra nada ni se restaura nada; la página queda en gris y las herramientas se comportan como en el juego original.";
		d["domain.function.label"] = "Recordar los elementos de herramienta de las funciones";
		d["domain.item.desc"] = "Activado: los ajustes de herramienta de esta página se registran en tiempo real y vuelven al cargar una partida.\nDesactivado: de esta página no se registra nada ni se restaura nada; la página queda en gris y las herramientas se comportan como en el juego original.";
		d["domain.item.label"] = "Recordar los elementos de herramienta de los objetos";
		d["group.domain"] = "Interruptores por página";
		d["item.color.label"] = "Color";
		d["item.decoration.desc"] = "Recuerda el interruptor «Preservar edad»: mientras esté activado, los árboles que pinta o coloca dejan de crecer y dejan de morir de viejos.\nLa fila solo aparece en los activos con datos de árbol, es decir al pintar árboles del menú Vegetación.";
		d["item.decoration.ex.category"] = "al cambiar a otro tipo de árbol sigue activado, pero la fila solo existe en los activos con datos de árbol, de modo que pintar una calcomanía no tiene esa fila en absoluto (necesita su propio ajuste)";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Tipo de pincel";
		d["item.brushType.desc"] = "Recuerda qué pincel está seleccionado en la fila «Tipo de pincel»: círculo, cuadrado, borde suave y demás, lo que define la forma del borde que pinta.\nLa fila solo existe en las herramientas que pueden pintar: las nueve herramientas de terreno y la herramienta de objetos cuando el activo seleccionado es pintable.";
		d["item.brushType.ex.group"] = "por ejemplo, si elige un pincel cuadrado al pintar, los demás activos pintables del mismo grupo también usan el pincel cuadrado, mientras que los de otro grupo no";
		d["item.brushType.ex.menu"] = "todos los activos pintables de un mismo menú usan este mismo pincel, pero al pasar a otro menú no";
		d["item.brushType.ex.category"] = "al cambiar a otra categoría se usa el pincel propio de esa categoría (necesita su propio ajuste), aunque ambos se puedan pintar";
		d["item.brushSize.label"] = "Tamaño de pincel";
		d["item.brushSize.desc"] = "Recuerda la fila «Tamaño de pincel»: hasta dónde llega una pasada.\nLa fila solo existe en las herramientas que pueden pintar: las nueve herramientas de terreno y la herramienta de objetos cuando el activo seleccionado es pintable.";
		d["item.brushSize.ex.group"] = "por ejemplo, si pone un pincel de 30 m, los demás activos pintables del mismo grupo también están a 30 m, mientras que los de otro grupo no";
		d["item.brushSize.ex.menu"] = "todos los activos pintables de un mismo menú usan este mismo tamaño de pincel, pero al pasar a otro menú no";
		d["item.brushSize.ex.category"] = "al cambiar a otra categoría se usa el tamaño de pincel propio de esa categoría (necesita su propio ajuste), aunque ambos se puedan pintar";
		d["item.brushStrength.label"] = "Fuerza de pincel";
		d["item.brushStrength.desc"] = "Recuerda la fila «Fuerza de pincel»: cuánto cambia cada pasada.\nLa fila solo existe en las herramientas que pueden pintar: las nueve herramientas de terreno y la herramienta de objetos cuando el activo seleccionado es pintable.";
		d["item.brushStrength.ex.group"] = "por ejemplo, si pone la fuerza de pincel a la mitad, los demás activos pintables del mismo grupo también están a la mitad, mientras que los de otro grupo no";
		d["item.brushStrength.ex.menu"] = "todos los activos pintables de un mismo menú usan esta misma fuerza de pincel, pero al pasar a otro menú no";
		d["item.brushStrength.ex.category"] = "al cambiar a otra categoría se usa la fuerza de pincel propia de esa categoría (necesita su propio ajuste), aunque ambos se puedan pintar";
		d["item.brushHeight.label"] = "Altura objetivo";
		d["item.brushHeight.desc"] = "Recuerda el valor de la fila «Altura objetivo»: a qué altura queda el suelo después de que una herramienta de terreno lo nivele o lo convierta en una pendiente.\nLa fila solo existe en las dos herramientas de terreno «Herramienta de nivelación de terreno» y «Herramienta de pendientes».";
		d["item.brushHeight.ex.group"] = "por ejemplo, si pone la altura objetivo en 50 m al nivelar, las demás herramientas de terreno del mismo grupo también usan 50 m, mientras que las de otro grupo no";
		d["item.brushHeight.ex.menu"] = "todas las herramientas del grupo de terreno usan esta misma altura objetivo, mientras que otro grupo de funciones no";
		d["item.brushHeight.ex.category"] = "al cambiar a otro grupo de funciones se usa la altura objetivo propia de ese grupo (necesita su propio ajuste); las demás herramientas de terreno no tienen la fila en absoluto";
		d["item.distance.label"] = "Distancia";
		d["item.distance.desc"] = "Recuerda el valor de la fila «Distancia»: a qué distancia quedan unos de otros los objetos consecutivos cuando los coloca a lo largo de una línea o de una curva.\nLa fila solo aparece en los modos Línea y Curva.";
		d["item.distance.ex.group"] = "por ejemplo, si pone la distancia en 10 m, los demás objetos del mismo grupo también quedan a 10 m unos de otros, mientras que los de otro grupo no";
		d["item.distance.ex.menu"] = "todos los activos que se pueden colocar a lo largo de una línea en un mismo menú usan esta misma distancia, pero al pasar a otro menú no";
		d["item.distance.ex.category"] = "al cambiar a otra categoría se usa la distancia propia de esa categoría (necesita su propio ajuste), aunque ambos se puedan colocar a lo largo de una línea";
		d["item.ageMask.label"] = "Edad";
		d["item.ageMask.desc"] = "Recuerda qué etapas de edad están marcadas en la fila «Edad»: brote, joven, maduro y anciano, y se pueden marcar varias a la vez.\nLa fila solo existe en los activos de árboles, es decir al pintar o colocar árboles del menú Vegetación.";
		d["item.ageMask.ex.group"] = "por ejemplo, si marca solo maduro al pintar árboles, los demás árboles del mismo grupo también generan solo ejemplares maduros, mientras que los de otro grupo no";
		d["item.ageMask.ex.menu"] = "todos los árboles del menú Vegetación usan estas mismas etapas de edad, pero al pasar a otro menú no";
		d["item.ageMask.ex.category"] = "al cambiar a otro tipo de árbol se mantienen estas etapas, pero la fila solo existe en los activos de árboles, así que pintar calcomanías y cosas parecidas no la tiene en absoluto (necesita su propio ajuste)";
		d["item.anarchyObjectElevation.label"] = "Elevación del objeto";
		d["item.anarchyObjectElevation.desc"] = "Recuerda la fila de controles de altura que Anarchy añade al panel de la herramienta de objetos: bloqueo de transformación, elevación, variación de elevación y el paso de subida y bajada.\nLos cuatro están en una sola fila y cuentan como un solo elemento.";
		d["item.anarchyObjectElevation.ex.group"] = "por ejemplo, si pone la altura en 15 m con una variación de 2 m, los demás objetos del mismo grupo usan los mismos valores, mientras que los de otro grupo no";
		d["item.anarchyObjectElevation.ex.menu"] = "todos los objetos de un mismo menú usan esta misma altura, pero al pasar a otro menú no";
		d["item.anarchyObjectElevation.ex.category"] = "al cambiar a otra categoría se usa la altura propia de esa categoría (necesita su propio ajuste)";
		d["item.decoration.ex.group"] = "por ejemplo, si activa «Preservar edad» mientras pinta árboles, los demás árboles del mismo grupo también la tienen activada, pero los de otro grupo no";
		d["item.decoration.ex.menu"] = "todos los árboles del menú Vegetación la mantienen activada, pero al pasar a otro menú no";
		d["item.decoration.label"] = "Preservar edad";
		d["item.f.snap.desc"] = "Recuerda cada interruptor de la fila «Ajustar» de los paneles de las herramientas de función: a ángulos de 90 grados, a la cuadrícula, a las carreteras, al lado del propietario, mostrar las líneas de contorno y demás.\nQué interruptores aparecen depende de la herramienta; el interruptor por lotes de toda la lista no se recuerda.";
		d["item.f.snap.ex.category"] = "al cambiar a otro grupo de funciones se vuelve a los valores predeterminados de ese grupo (necesita su propio ajuste), aunque los dos sean accesibles desde el mismo menú de la barra de herramientas";
		d["item.f.snap.ex.group"] = "por ejemplo, si desactiva el ajuste a la cuadrícula para la zona residencial, las zonas comercial, industrial y de oficinas también lo tienen desactivado, pero las herramientas de área no";
		d["item.f.snap.ex.menu"] = "todas las herramientas de un mismo grupo de funciones usan este mismo conjunto de interruptores, pero las de otro grupo de funciones no";
		d["item.f.toolMode.desc"] = "Recuerda el modo de herramienta elegido en los paneles de las herramientas de función; las opciones cambian según la herramienta: los tipos de zona ofrecen Rellenar, Seleccionar y Pintar, las herramientas de área Editar y Generar cuadrícula de mapa, cada herramienta de terreno sus propios modos.";
		d["item.f.toolMode.ex.category"] = "al cambiar a otro grupo de funciones se vuelve al modo propio de ese grupo (necesita su propio ajuste), aunque ambos sean accesibles desde el mismo menú de la barra de herramientas";
		d["item.f.toolMode.ex.group"] = "por ejemplo, si elige Pintar para la zona residencial, las zonas comercial, industrial y de oficinas también están en Pintar, pero las herramientas de área no";
		d["item.f.toolMode.ex.menu"] = "todas las herramientas de un mismo grupo de funciones usan este mismo modo, pero las de otro grupo de funciones no";
		d["item.f.topography.desc"] = "Recuerda el estado de la fila «Líneas de contorno» de los paneles de las herramientas de función, es decir, si se ven las líneas de contorno del terreno mientras dibuja zonas.";
		d["item.f.topography.ex.category"] = "al cambiar a otro grupo de funciones se usa el ajuste propio de ese grupo";
		d["item.f.topography.ex.group"] = "por ejemplo, si marca las líneas de contorno para la zona residencial, las zonas comercial, industrial y de oficinas también las tienen marcadas";
		d["item.f.topography.ex.menu"] = "todas las herramientas de un mismo grupo de funciones las tienen marcadas, pero las de otro grupo de funciones no";
		d["item.f.underground.desc"] = "Recuerda el estado de la fila «Modo subterráneo» de los paneles de las herramientas de función.";
		d["item.f.underground.ex.category"] = "al cambiar a otro grupo de funciones se usa el ajuste propio de ese grupo";
		d["item.f.underground.ex.group"] = "por ejemplo, si activa el modo subterráneo para la zona residencial, las zonas comercial, industrial y de oficinas también están bajo tierra";
		d["item.f.underground.ex.menu"] = "todas las herramientas de un mismo grupo de funciones están en modo subterráneo, pero las de otro grupo de funciones no";
		d["item.i.snap.desc"] = "Recuerda cada interruptor de la fila «Ajustar» de los paneles de los objetos: a la superficie del edificio, a las formas cercanas, a la carretera, eliminar solo el tipo coincidente y demás, dependiendo del objeto.\nQué interruptores aparecen depende del objeto.";
		d["item.i.snap.ex.category"] = "al cambiar a otra categoría de objetos se vuelve a los valores predeterminados de esa categoría (necesita su propio ajuste), aunque ambas estén en el menú Paisajismo";
		d["item.i.snap.ex.group"] = "por ejemplo, si desactiva el ajuste a la superficie del edificio para un objeto de Parque, los demás objetos del mismo grupo también lo tienen desactivado, mientras que los de otro grupo no";
		d["item.i.snap.ex.menu"] = "todos los objetos del menú Paisajismo usan este mismo conjunto de interruptores, pero al pasar a otro menú no";
		d["item.i.toolMode.desc"] = "Recuerda el modo de herramienta elegido en los paneles de los objetos: Colocar uno, Colocar varios, Línea, Curva, Herramienta de sello de objetos, Pintar y demás, dependiendo de la herramienta.";
		d["item.i.toolMode.ex.category"] = "al cambiar a otra categoría de objetos, por ejemplo de Naturaleza a Calcomanías, se vuelve al modo propio de esa categoría (necesita su propio ajuste), aunque ambas estén en el menú Paisajismo";
		d["item.i.toolMode.ex.group"] = "por ejemplo, si elige Colocar varios para un objeto de Naturaleza, los demás objetos del mismo grupo también usan Colocar varios, mientras que los de otro grupo no";
		d["item.i.toolMode.ex.menu"] = "todos los objetos del menú Paisajismo están en este modo, pero al pasar a otro menú no";
		d["item.i.topography.desc"] = "Recuerda el estado de la fila «Líneas de contorno» de los paneles de los objetos.";
		d["item.i.topography.ex.category"] = "al cambiar a otra categoría de objetos se usa el ajuste propio de esa categoría";
		d["item.i.topography.ex.group"] = "por ejemplo, si marca las líneas de contorno mientras coloca objetos, los demás objetos del mismo grupo también las tienen marcadas";
		d["item.i.topography.ex.menu"] = "todos los objetos del menú Paisajismo las tienen marcadas, pero al pasar a otro menú no";
		d["item.i.underground.desc"] = "Recuerda el estado de la fila «Modo subterráneo» de los paneles de los objetos, es decir, si los objetos se colocan bajo el suelo o encima de él.";
		d["item.i.underground.ex.category"] = "al cambiar a otra categoría de objetos se usa el ajuste propio de esa categoría";

		d["item.i.underground.ex.group"] = "por ejemplo, si activa el modo subterráneo para un objeto, los demás objetos del mismo grupo también pasan a estar bajo tierra";
		d["item.i.underground.ex.menu"] = "todos los objetos del menú Paisajismo están en modo subterráneo, pero al pasar a otro menú no";
		d["scope.category.function"] = "Mismo tipo";
		d["scope.category.item"] = "Mismo tipo de objeto";
		d["tab.assets"] = "Activos";
		d["tab.functions"] = "Funciones";
		d["tab.items"] = "Objetos";
		d["tab.master"] = "Interruptor general";
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
			d["scope.desc"] = "Dans quelle mesure la valeur de cet élément est partagée. Les parenthèses de la liste déroulante indiquent le comportement d'origine et marquent le niveau prévu.";
			d["scope.line.group"] = "Même groupe : {0}";
			d["scope.line.menu"] = "Même menu : {0}";
			d["scope.line.category"] = "Même type : classé d'après ce à quoi sert réellement l'outil, donc le nombre de voies, la taille, le niveau et la catégorie de la barre d'outils ne font aucune différence (un petit et un grand parc de stationnement sont du même type), mais les ponts, les quais et les croisements forment chacun leur propre type, à part les routes, c'est-à-dire {0}";
			d["scope.line.shared"] = "Global : tout outil de cette page compatible avec cet élément partage une seule valeur";
			d["scope.line.shared.function"] = "Global : toute fonction de cette page compatible avec cet élément partage une seule valeur";
			d["scope.line.shared.item"] = "Global : tout accessoire de cette page compatible avec cet élément partage une seule valeur";
			d["scope.line.unique"] = "Aucun partage : chaque outil de cette page compatible avec cet élément se règle indépendamment";
			d["scope.line.unique.function"] = "Aucun partage : chaque fonction de cette page compatible avec cet élément se règle indépendamment";
			d["scope.line.unique.item"] = "Aucun partage : chaque accessoire de cette page compatible avec cet élément se règle indépendamment";

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
			d["item.anarchy.desc"] = "Mémorise l'état du bouton Anarchy ajouté par le mod Anarchy ; Anarchy est partagé dans tout le jeu, cet élément est donc global.\nSans le mod Anarchy, cet élément n'apparaît pas.";
			d["item.anarchy.ex.group"] = "par exemple, activez Anarchy sur une petite route à deux voies, les autres petites routes l'ont activé aussi, mais une grande route non";
			d["item.anarchy.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes ont Anarchy activé, mais dans le menu Électricité ce n'est plus le cas";
			d["item.anarchy.ex.category"] = "passer à une route à six voies laisse Anarchy activé, mais un pont ou une double voie de métro repasse à désactivé et réclame son propre réglage, bien que tous soient dans le menu Routes";

			d["item.themes.label"] = "Thème";
			d["item.themes.desc"] = "Mémorise les entrées cochées dans le filtre « Thème » de la barre d'outils ; cocher un thème limite la liste de la barre aux assets qui prennent en charge ces thèmes.\nCette ligne n'apparaît que si la catégorie courante contient vraiment des assets utilisant des thèmes.";
			d["item.themes.ex.group"] = "par exemple, cochez un thème uniquement sous Routes/Petites routes, une grande route ne filtre donc plus selon lui";
			d["item.themes.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes suivent le thème coché dans ce menu, mais dans le menu Électricité ce n'est plus le cas";
			d["item.themes.ex.category"] = "cette ligne est l'état du filtre de la barre d'outils lui-même, elle est donc mémorisée d'après le menu et la catégorie où vous vous trouvez, et non d'après le type d'actif";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Mémorise les entrées cochées dans le filtre « Pack » de la barre d'outils ; cocher un pack limite la liste de la barre aux assets appartenant à ces packs.\nLe jeu d'origine les efface à chaque changement de menu ou de catégorie.";
			d["item.packs.ex.group"] = "par exemple, cochez un pack sous Routes/Petites routes, le jeu d'origine l'efface sur une grande route et il revient quand vous y retournez";
			d["item.packs.ex.menu"] = "les packs cochés dans le menu Routes et dans le menu Électricité sont mémorisés séparément";
			d["item.packs.ex.category"] = "cette ligne est l'état du filtre de la barre d'outils lui-même, mémorisée d'après le menu et la catégorie où vous vous trouvez ; le jeu d'origine l'efface à chaque changement de catégorie";

			// ---------- Mode Outil ----------
			d["item.toolMode.label"] = "Mode Outil";
			d["item.toolMode.desc"] = "Mémorise le mode choisi pour chaque outil ; les options varient : routes, rails et canalisations proposent Droit, Courbe simple, Courbe complexe, Continu, Grille, Remplacer et Point.\nCette ligne n'apparaît que lorsque l'outil choisi propose plus d'un mode.";
			d["item.toolMode.ex.group"] = "par exemple, choisissez Courbe simple sur une petite route à deux voies, les autres petites routes utilisent aussi Courbe simple, mais une grande route non";
			d["item.toolMode.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en Courbe simple, mais dans le menu Électricité ce n'est plus le cas";
			d["item.toolMode.ex.category"] = "passer à une route à six voies donne toujours Courbe simple, mais un pont ou une double voie de métro retombe sur son mode par défaut et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Élévation ----------
			d["item.elevation.label"] = "Élévation";
			d["item.elevation.desc"] = "Mémorise l'élévation laissée sur l'outil, y compris le résultat de Augmenter l'élévation et Diminuer l'élévation.\nL'Étape d'élévation (le déplacement d'une seule pression ; l'interface Anarchy nomme cette ligne Incrément d'élévation) est mémorisée avec elle et utilise la même portée de partage.";
			d["item.elevation.ex.group"] = "par exemple, montez une route à deux voies à 10 m, les autres petites routes passent aussi à 10 m, mais une grande route n'est pas à 10 m";
			d["item.elevation.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont à 10 m, mais dans le menu Électricité ils ne sont pas à 10 m";
			d["item.elevation.ex.category"] = "passer à une route à six voies donne toujours 10 m, mais un pont ou une double voie ferrée repart à 0 m et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Mode parallèle ----------
			d["item.parallel.label"] = "Mode parallèle";
			d["item.parallel.desc"] = "Mémorise les deux éléments du panneau Route parallèle : l'interrupteur Mode parallèle et l'espacement de la Compensation parallèle.\nActivé, une seconde ligne se construit le long de celle que vous tracez ; seule la distance entre les deux est réglable. Coupé par défaut, donc une seule ligne comme à l'origine.";
			d["item.parallel.ex.group"] = "par exemple, activez le Mode parallèle sur une petite route à deux voies et réglez la Compensation parallèle sur 5 m, les autres petites routes reviennent activées avec les mêmes 5 m, mais une grande route non";
			d["item.parallel.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en parallèle avec 5 m de compensation, mais dans le menu Électricité ce n'est plus le cas";
			d["item.parallel.ex.category"] = "passer à une route à six voies reste activé avec les mêmes 5 m de compensation, mais un pont ou une voie de métro se désactive et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Accrocher ----------
			d["item.snap.label"] = "Accrocher";
			d["item.snap.desc"] = "Mémorise chaque interrupteur de la ligne Accrocher, par exemple aux côtés d'une route, au milieu d'une route, aux nœuds, à la surface d'un objet ou au rivage.\nQuels interrupteurs apparaissent dépend de l'outil ; l'interrupteur groupé de toute la ligne n'est pas mémorisé.";
			d["item.snap.ex.group"] = "par exemple, désactivez « Accrochez aux côtés d'une route » sur une petite route à deux voies, les autres petites routes l'ont désactivé aussi, mais une grande route a tout activé";
			d["item.snap.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs d'accrochage, mais dans le menu Électricité ce n'est plus le cas";
			d["item.snap.ex.category"] = "passer à une route à six voies garde le même jeu d'interrupteurs, mais un pont ou un sentier revient aux valeurs par défaut et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Topographie ----------
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Mémorise la ligne Topographie, c'est-à-dire si Afficher les lignes de contour est actif.";
			d["item.topography.ex.group"] = "par exemple, activez la Topographie sur une petite route à deux voies, les autres petites routes l'ont activée aussi, mais une grande route non";
			d["item.topography.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes affichent les lignes de contour, mais dans le menu Électricité ce n'est plus le cas";
			d["item.topography.ex.category"] = "passer à une route à six voies affiche toujours les lignes de contour, mais un pont ou un câble électrique ont la ligne désactivée et réclament leur propre réglage";


			// ---------- Gauche et droite (Anarchy) ----------
			d["item.leftRight.label"] = "Gauche et droite";
			d["item.leftRight.desc"] = "Mémorise les améliorations de réseau choisies sur les lignes Gauche et Droite du panneau réseau d'Anarchy, par exemple Piste cyclable, Herbe, Quai, Mur de soutènement et Barrière acoustique.\nNécessite Anarchy (mod Paradox 74604) ; sans lui cet élément n'apparaît pas.";
			d["item.leftRight.ex.group"] = "par exemple, choisissez Piste cyclable à droite d'une petite route à deux voies, les autres petites routes l'ont aussi à droite, mais une grande route non";
			d["item.leftRight.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même choix des deux côtés, mais dans le menu Électricité ce n'est plus le cas";
			d["item.leftRight.ex.category"] = "passer à une route à six voies garde Piste cyclable à droite, mais un pont ou une double voie ferrée reviennent à rien de choisi et réclament leur propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Général (Anarchy) ----------
			d["item.general.label"] = "Général";
			d["item.general.desc"] = "Mémorise les interrupteurs de la ligne Général du panneau réseau d'Anarchy : Au sol, Élévation, Tunnel, Pente constante, Grande médiane et Gamme d'élévation élargie (qui supprime la limite de hauteur).\nNécessite Anarchy (mod Paradox 74604) ; sans lui cet élément n'apparaît pas.";
			d["item.general.ex.group"] = "par exemple, choisissez Tunnel sur une petite route à deux voies, les autres petites routes sont forcées en tunnel aussi, mais une grande route non";
			d["item.general.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs de Général, mais dans le menu Électricité ce n'est plus le cas";
			d["item.general.ex.category"] = "passer à une route à six voies donne toujours Tunnel, mais un pont ou une double voie de métro reviennent à rien de coché et réclament leur propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Mode souterrain ----------
			d["item.underground.label"] = "Mode souterrain";
			d["item.underground.desc"] = "Mémorise l'interrupteur Mode souterrain, qui décide si ce que vous placez passe sous le sol ou dessus.";
			d["item.underground.ex.group"] = "par exemple, activez le Mode souterrain sur une petite route à deux voies, les autres petites routes passent aussi sous terre, mais une grande route non";
			d["item.underground.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont construits sous terre, mais dans le menu Électricité ce n'est plus le cas";
			d["item.underground.ex.category"] = "passer à une route à six voies reste sous terre, mais un pont ou une double voie ferrée restent en surface et réclament leur propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Autres ----------
			d["item.other.label"] = "Autres";
			d["item.other.desc"] = "Mémorise les autres lignes qu'un panneau d'outil peut enregistrer : les couleurs de route de la ligne Couleur.";
			d["item.other.ex.group"] = "par exemple, mettez la couleur d'une petite route à deux voies en bleu, les autres petites routes deviennent bleues aussi, mais une grande route non";
			d["item.other.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même réglage de couleur, mais dans le menu Électricité ce n'est plus le cas";
			d["item.other.ex.category"] = "passer à une route à six voies reste en bleu, mais un pont ou une double voie ferrée gardent leur propre copie et réclament leur propre réglage, bien que tous soient dans le menu Routes";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（fr-FR）====================
		d["tab.master"] = "Interrupteur principal";
		d["tab.assets"] = "Assets";
		d["tab.functions"] = "Fonctions";
		d["tab.items"] = "Accessoires";
		d["group.domain"] = "Interrupteurs par page";
		d["domain.asset.label"] = "Mémoriser les éléments d'outil des assets";
		d["domain.asset.desc"] = "Activé : les réglages d'outil de cette page sont enregistrés en continu et reviennent au chargement d'une sauvegarde.\nCoupé : rien de cette page n'est enregistré ni restauré ; la page est grisée et chaque outil retrouve son comportement d'origine.";
		d["domain.function.label"] = "Mémoriser les éléments d'outil des fonctions";
		d["domain.function.desc"] = "Activé : les réglages d'outil de cette page sont enregistrés en continu et reviennent au chargement d'une sauvegarde.\nCoupé : rien de cette page n'est enregistré ni restauré ; la page est grisée et chaque outil retrouve son comportement d'origine.";
		d["domain.item.label"] = "Mémoriser les éléments d'outil des accessoires";
		d["domain.item.desc"] = "Activé : les réglages d'outil de cette page sont enregistrés en continu et reviennent au chargement d'une sauvegarde.\nCoupé : rien de cette page n'est enregistré ni restauré ; la page est grisée et chaque outil retrouve son comportement d'origine.";
		d["item.color.label"] = "Couleur";
		d["item.decoration.label"] = "Préserver l'âge";
		d["item.decoration.desc"] = "Mémorise l'interrupteur « Préserver l'âge » : une fois coché, les arbres que vous brossez cessent de grandir comme de vieillir et ne tombent plus.\nCette ligne n'apparaît que sur les assets qui portent des données d'arbres, c'est-à-dire en brossant des arbres depuis le menu Végétation.";
		d["item.decoration.ex.group"] = "par exemple, cochez « Préserver l'âge » en brossant des arbres et les autres arbres du même groupe l'ont activé eux aussi, mais un autre groupe ne l'est pas";
		d["item.decoration.ex.menu"] = "tous les arbres du menu Végétation gardent cette ligne activée, mais en changeant de menu ce n'est plus le cas";
		d["item.decoration.ex.category"] = "passer à une autre sorte d'arbre laisse aussi cette ligne activée, mais elle n'existe que sur les assets portant des données d'arbres, donc brosser des décalcomanies n'a pas du tout cette ligne (réglage séparé nécessaire)";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Type de pinceau";
		d["item.brushType.desc"] = "Mémorise le pinceau sélectionné dans la ligne « Type de pinceau » : cercle, carré, bord doux, etc., ce qui définit la forme du bord que vous peignez.\nCette ligne n'existe que sur les outils capables de peindre : les neuf outils de terraformation, et l'outil Objet lorsque l'asset sélectionné est brossable.";
		d["item.brushType.ex.group"] = "par exemple, choisissez un pinceau carré en peignant et les autres assets brossables du même groupe utilisent eux aussi le pinceau carré, mais un autre groupe non";
		d["item.brushType.ex.menu"] = "tous les assets brossables d'un même menu utilisent ce même pinceau, mais en changeant de menu ce n'est plus le cas";
		d["item.brushType.ex.category"] = "passer à une autre catégorie utilise le pinceau propre à cette catégorie (réglage séparé nécessaire), même quand les deux peuvent être peints";
		d["item.brushSize.label"] = "Taille du pinceau";
		d["item.brushSize.desc"] = "Mémorise la ligne « Taille du pinceau » : l'étendue couverte par une passe.\nCette ligne n'existe que sur les outils capables de peindre : les neuf outils de terraformation, et l'outil Objet lorsque l'asset sélectionné est brossable.";
		d["item.brushSize.ex.group"] = "par exemple, réglez un pinceau de 30 m et les autres assets brossables du même groupe sont aussi à 30 m, mais un autre groupe non";
		d["item.brushSize.ex.menu"] = "tous les assets brossables d'un même menu utilisent cette même taille de pinceau, mais en changeant de menu ce n'est plus le cas";
		d["item.brushSize.ex.category"] = "passer à une autre catégorie utilise la taille de pinceau propre à cette catégorie (réglage séparé nécessaire), même quand les deux peuvent être peints";
		d["item.brushStrength.label"] = "Force du pinceau";
		d["item.brushStrength.desc"] = "Mémorise la ligne « Force du pinceau » : ce que chaque passe modifie.\nCette ligne n'existe que sur les outils capables de peindre : les neuf outils de terraformation, et l'outil Objet lorsque l'asset sélectionné est brossable.";
		d["item.brushStrength.ex.group"] = "par exemple, réglez la force du pinceau sur la moitié et les autres assets brossables du même groupe sont eux aussi à la moitié, mais un autre groupe non";
		d["item.brushStrength.ex.menu"] = "tous les assets brossables d'un même menu utilisent cette même force de pinceau, mais en changeant de menu ce n'est plus le cas";
		d["item.brushStrength.ex.category"] = "passer à une autre catégorie utilise la force de pinceau propre à cette catégorie (réglage séparé nécessaire), même quand les deux peuvent être peints";
		d["item.brushHeight.label"] = "Hauteur cible";
		d["item.brushHeight.desc"] = "Mémorise la valeur de la ligne « Hauteur cible » : à quelle hauteur le sol se retrouve après qu'un outil de terraformation l'a aplani ou transformé en pente.\nCette ligne n'existe que sur les deux outils de terrain Outil de nivellement de terrain et Outil pour les terrains en pente.";
		d["item.brushHeight.ex.group"] = "par exemple, réglez la hauteur cible à 50 m en nivelant et les autres outils de terraformation du même groupe utilisent aussi 50 m, mais un autre groupe non";
		d["item.brushHeight.ex.menu"] = "tous les outils de la famille Terraformation utilisent cette même hauteur cible, mais une autre famille de fonctions non";
		d["item.brushHeight.ex.category"] = "passer à une autre famille de fonctions utilise la hauteur cible propre à cette famille (réglage séparé nécessaire) ; les autres outils de terraformation n'ont pas du tout cette ligne";
		d["item.distance.label"] = "Distance";
		d["item.distance.desc"] = "Mémorise la valeur de la ligne « Distance » : l'écart entre des objets consécutifs lorsque vous les placez le long d'une ligne ou d'une courbe.\nCette ligne n'apparaît que dans les modes Ligne et Courbe.";
		d["item.distance.ex.group"] = "par exemple, réglez la distance sur 10 m et les autres accessoires du même groupe sont eux aussi espacés de 10 m, mais un autre groupe non";
		d["item.distance.ex.menu"] = "tous les assets qui peuvent être placés le long d'une ligne dans un même menu utilisent cette même distance, mais en changeant de menu ce n'est plus le cas";
		d["item.distance.ex.category"] = "passer à une autre catégorie utilise la distance propre à cette catégorie (réglage séparé nécessaire), même quand les deux peuvent être placés le long d'une ligne";
		d["item.ageMask.label"] = "Âge";
		d["item.ageMask.desc"] = "Mémorise les stades d'âge cochés dans la ligne « Âge » : jeune arbre, jeune, adulte et âgé, plusieurs pouvant être cochés à la fois.\nCette ligne n'existe que sur les assets d'arbres, c'est-à-dire en brossant ou plaçant des arbres du menu Végétation.";
		d["item.ageMask.ex.group"] = "par exemple, ne cochez que adulte en brossant des arbres et les autres arbres du même groupe ne produisent eux aussi que des adultes, mais un autre groupe non";
		d["item.ageMask.ex.menu"] = "tous les arbres du menu Végétation utilisent ces mêmes stades d'âge, mais en changeant de menu ce n'est plus le cas";
		d["item.ageMask.ex.category"] = "passer à une autre sorte d'arbre garde ces stades, mais la ligne n'existe que sur les assets d'arbres, donc brosser des décalcomanies et autres n'a pas du tout cette ligne (réglage séparé nécessaire)";
		d["item.anarchyObjectElevation.label"] = "Élévation de l'objet";
		d["item.anarchyObjectElevation.desc"] = "Mémorise la rangée de contrôles de hauteur qu'Anarchy ajoute au panneau de l'outil Objet : verrouillage de transformation, élévation, variation d'altitude et pas d'augmentation et de diminution.\nCes quatre réglages tiennent sur une seule rangée et comptent donc comme un seul élément.";
		d["item.anarchyObjectElevation.ex.group"] = "par exemple, réglez la hauteur sur 15 m avec une variation de 2 m et les autres accessoires du même groupe utilisent les mêmes valeurs, mais un autre groupe non";
		d["item.anarchyObjectElevation.ex.menu"] = "tous les accessoires d'un même menu utilisent cette même hauteur, mais en changeant de menu ce n'est plus le cas";
		d["item.anarchyObjectElevation.ex.category"] = "passer à une autre catégorie utilise la hauteur propre à cette catégorie (réglage séparé nécessaire)";
		d["item.f.snap.desc"] = "Mémorise chaque interrupteur de la ligne « Accrocher » des panneaux de fonctions : aux angles à 90 degrés, à la grille de zone, aux routes, au côté du propriétaire, afficher les lignes de contour et d'autres encore.\nQuels interrupteurs apparaissent dépendent de l'outil ; l'interrupteur groupé de toute la liste n'est pas mémorisé.";
		d["item.f.snap.ex.group"] = "par exemple, désactivez « Accrochez à la grille de zone » pour la zone résidentielle et les zones commerciale, industrielle et de bureaux l'ont désactivé elles aussi, mais les outils de périmètre non";
		d["item.f.snap.ex.menu"] = "tous les outils d'une même famille de fonctions utilisent ce même jeu d'interrupteurs, mais une autre famille de fonctions ne le fait pas";
		d["item.f.snap.ex.category"] = "passer à une autre famille de fonctions rétablit les interrupteurs par défaut de cette famille (réglage séparé nécessaire), même quand les deux sont atteignables depuis le même menu de la barre d'outils";
		d["item.f.toolMode.desc"] = "Mémorise le choix de la ligne « Mode Outil » des panneaux de fonctions ; les options varient selon l'outil : les zones proposent Remplissage, Sélection et Peinture, les outils de périmètre Modifier et Générer une grille de carte, chaque outil de terraformation ses propres modes.";
		d["item.f.toolMode.ex.group"] = "par exemple, choisissez Peinture pour la zone résidentielle et les zones commerciale, industrielle et de bureaux sont en Peinture elles aussi, mais les outils de périmètre non";
		d["item.f.toolMode.ex.menu"] = "tous les outils d'une même famille de fonctions utilisent ce même mode, mais une autre famille de fonctions ne le fait pas";
		d["item.f.toolMode.ex.category"] = "passer à une autre famille de fonctions retombe sur le mode propre à cette famille (réglage séparé nécessaire), même quand les deux sont atteignables depuis le même menu de la barre d'outils";
		d["item.f.topography.desc"] = "Mémorise l'état de la ligne « Lignes de contour » des panneaux de fonctions, c'est-à-dire si vous voyez ou non les courbes de niveau du terrain en dessinant des zones.";
		d["item.f.topography.ex.group"] = "par exemple, cochez les lignes de contour pour la zone résidentielle et les zones commerciale, industrielle et de bureaux sont cochées elles aussi";
		d["item.f.topography.ex.menu"] = "tous les outils d'une même famille de fonctions les gardent cochées, mais une autre famille de fonctions ne le fait pas";
		d["item.f.topography.ex.category"] = "passer à une autre famille de fonctions utilise le réglage propre à cette famille";
		d["item.f.underground.desc"] = "Mémorise l'état de la ligne « Mode souterrain » des panneaux de fonctions.";
		d["item.f.underground.ex.group"] = "par exemple, activez le mode souterrain pour la zone résidentielle et les zones commerciale, industrielle et de bureaux passent elles aussi sous terre";
		d["item.f.underground.ex.menu"] = "tous les outils d'une même famille de fonctions restent en mode souterrain, mais une autre famille de fonctions ne le fait pas";
		d["item.f.underground.ex.category"] = "passer à une autre famille de fonctions utilise le réglage propre à cette famille";
		d["item.i.snap.desc"] = "Mémorise chaque interrupteur de la ligne « Accrocher » des panneaux d'accessoires : à la surface d'un bâtiment, à la géométrie à proximité, aux routes, supprimer uniquement le type correspondant et d'autres encore.\nQuels interrupteurs apparaissent dépendent de l'accessoire.";
		d["item.i.snap.ex.group"] = "par exemple, désactivez « Accrochez à la surface d'un bâtiment » pour un accessoire de la famille Parc et les autres accessoires du même groupe l'ont désactivé eux aussi, mais un autre groupe non";
		d["item.i.snap.ex.menu"] = "tous les accessoires du menu Aménagement paysager utilisent ce même jeu d'interrupteurs, mais en changeant de menu ce n'est plus le cas";
		d["item.i.snap.ex.category"] = "passer à une autre famille d'accessoires rétablit les interrupteurs par défaut de cette famille (réglage séparé nécessaire), même quand les deux sont sous le menu Aménagement paysager";
		d["item.i.toolMode.desc"] = "Mémorise le choix de la ligne « Mode Outil » des panneaux d'accessoires : En placer un, En placer plusieurs, Ligne, Courbe, Outil de marquage d'objet, Peinture et ainsi de suite selon l'outil.";
		d["item.i.toolMode.ex.group"] = "par exemple, choisissez En placer plusieurs pour un accessoire de la famille Nature et les autres accessoires du même groupe sont en En placer plusieurs eux aussi, mais un autre groupe ne l'est pas";
		d["item.i.toolMode.ex.menu"] = "tous les accessoires du menu Aménagement paysager sont sur ce mode, mais en changeant de menu ce n'est plus le cas";
		d["item.i.toolMode.ex.category"] = "passer à une autre famille d'accessoires, de Nature à Décalcomanies par exemple, retombe sur le mode propre à cette famille (réglage séparé nécessaire), même quand les deux sont sous le menu Aménagement paysager";
		d["item.i.topography.desc"] = "Mémorise l'état de la ligne « Lignes de contour » des panneaux d'accessoires.";
		d["item.i.topography.ex.group"] = "par exemple, cochez les lignes de contour en posant des accessoires et les autres accessoires du même groupe sont cochés eux aussi";
		d["item.i.topography.ex.menu"] = "tous les accessoires du menu Aménagement paysager les gardent cochées, mais en changeant de menu ce n'est plus le cas";
		d["item.i.topography.ex.category"] = "passer à une autre famille d'accessoires utilise le réglage propre à cette famille";
		d["item.i.underground.desc"] = "Mémorise l'état de la ligne « Mode souterrain » des panneaux d'accessoires, c'est-à-dire si vous posez les accessoires sous le sol ou par-dessus.";
		d["item.i.underground.ex.group"] = "par exemple, activez le mode souterrain pour un accessoire et les autres accessoires du même groupe passent eux aussi sous terre";
		d["item.i.underground.ex.menu"] = "tous les accessoires du menu Aménagement paysager passent sous terre, mais en changeant de menu ce n'est plus le cas";
		d["item.i.underground.ex.category"] = "passer à une autre famille d'accessoires utilise le réglage propre à cette famille";

		d["scope.category.function"] = "Même type";
		d["scope.category.item"] = "Même type d'accessoire";
		d["enabled.label"] = "Activer le mod";
		d["enabled.desc"] = "Coupé par défaut : fraîchement installé, le mod ne fait rien et tout se comporte comme dans le jeu d'origine.\nUne fois activé, les panneaux d'outil sont enregistrés et reviennent au chargement d'une sauvegarde. Il se réactive dès qu'un des trois interrupteurs par page ci-dessous est activé.";
		d["scope.line.sharedSingle"] = "Global : tous les outils de cette page qui prennent en charge cet élément partagent une seule valeur";
			d["scope.note"] = "Remarque : chaque niveau ci-dessus ne vaut qu'à l'intérieur de cette page.";
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
			d["scope.desc"] = "Quanto viene condiviso il valore di questa voce. Le parentesi nel menu a tendina mostrano cosa fa l'originale e segnano il livello previsto.";
			d["scope.line.group"] = "Stesso gruppo: {0}";
			d["scope.line.menu"] = "Stesso menu: {0}";
			d["scope.line.category"] = "Stesso tipo: raggruppati in base a ciò a cui lo strumento serve davvero, quindi numero di corsie, dimensione, livello e categoria della barra degli strumenti non fanno alcuna differenza (un parcheggio piccolo e uno grande sono lo stesso tipo), ma ponti, banchine e incroci formano ognuno un tipo a sé, separato dalle strade, cioè {0}";
			d["scope.line.shared"] = "Globale: ogni strumento di questa pagina che supporta questa voce usa un unico valore";
			d["scope.line.shared.function"] = "Globale: ogni funzione di questa pagina che supporta questa voce usa un unico valore";
			d["scope.line.shared.item"] = "Globale: ogni oggetto di questa pagina che supporta questa voce usa un unico valore";
			d["scope.line.unique"] = "Non condiviso: ogni strumento di questa pagina che supporta questa voce viene impostato in modo indipendente";
			d["scope.line.unique.function"] = "Non condiviso: ogni funzione di questa pagina che supporta questa voce viene impostata in modo indipendente";
			d["scope.line.unique.item"] = "Non condiviso: ogni oggetto di questa pagina che supporta questa voce viene impostato in modo indipendente";

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
			d["item.anarchy.desc"] = "Ricorda lo stato del pulsante Anarchy aggiunto dal mod Anarchy; Anarchy è condiviso in tutto il gioco, quindi questa voce è globale.\nSenza il mod Anarchy questa voce non compare.";
			d["item.anarchy.ex.group"] = "per esempio, attivi Anarchy su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attivo, mentre una strada grande no";
			d["item.anarchy.ex.menu"] = "tutti gli asset che clicchi nel menu Strade hanno Anarchy attivo, ma passando al menu Elettricità no";
			d["item.anarchy.ex.category"] = "passando a una strada a sei corsie Anarchy resta attivo, ma un ponte o un binario della metropolitana doppio tornano a spento e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Ricorda quali voci sono spuntate nel filtro «Tema» della barra degli strumenti; spuntarne uno limita l'elenco della barra agli asset che supportano quei temi.\nQuella riga compare solo quando la categoria corrente contiene davvero asset che usano i temi.";
			d["item.themes.ex.group"] = "per esempio, spunti un tema solo in Strade/Strade piccole, così una strada grande non filtra per quel tema";
			d["item.themes.ex.menu"] = "tutti gli asset che clicchi nel menu Strade seguono il tema spuntato lì, ma passando al menu Elettricità no";
			d["item.themes.ex.category"] = "la spunta di questa riga è lo stato del filtro della barra degli strumenti stesso, quindi viene ricordata per il menu e la categoria in cui ti trovi, e non per tipo di asset";
			d["item.packs.label"] = "Pacchetto";
			d["item.packs.desc"] = "Ricorda quali voci sono spuntate nel filtro «Pacchetto» della barra degli strumenti; spuntarne una limita l'elenco della barra agli asset che appartengono a quei pacchetti.\nL'originale lo azzera ogni volta che cambi menu o categoria.";
			d["item.packs.ex.group"] = "per esempio, spunti un pacchetto in Strade/Strade piccole, l'originale lo azzera sulle strade grandi e ritorna quando ci torni";
			d["item.packs.ex.menu"] = "i pacchetti spuntati nel menu Strade e nel menu Elettricità vengono ricordati separatamente";
			d["item.packs.ex.category"] = "la spunta di questa riga è lo stato del filtro della barra degli strumenti stesso, ricordata per il menu e la categoria in cui ti trovi; l'originale la azzera ogni volta che cambi categoria";

			// ---------- Modalità strumento ----------
			d["item.toolMode.label"] = "Modalità strumento";
			d["item.toolMode.desc"] = "Ricorda la modalità scelta per ogni strumento; le opzioni cambiano a seconda dello strumento: strade, binari e tubi offrono Rettilineo, Curva semplice, Curva complessa, Continua, Griglia, Sostituisci e Punto.\nLa riga compare solo quando lo strumento scelto ha più di una modalità.";
			d["item.toolMode.ex.group"] = "per esempio, scegli Curva semplice su una piccola strada a due corsie e anche le altre piccole strade usano Curva semplice, mentre una strada grande no";
			d["item.toolMode.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono su Curva semplice, ma passando al menu Elettricità no";
			d["item.toolMode.ex.category"] = "passando a una strada a sei corsie resta Curva semplice, ma un ponte o un binario della metropolitana doppio ripartono dalla loro modalità predefinita e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Elevazione ----------
			d["item.elevation.label"] = "Elevazione";
			d["item.elevation.desc"] = "Ricorda l'elevazione a cui lasci lo strumento, incluso il risultato di Aumenta elevazione e Diminuisci elevazione.\nIl Livello elevazione (quanto sposta una singola pressione; Anarchy chiama questa riga Step di elevazione) viene ricordato insieme ad essa e usa lo stesso ambito di condivisione.";
			d["item.elevation.ex.group"] = "per esempio, porti una strada a due corsie a 10 m e anche le altre piccole strade vanno a 10 m, mentre una strada grande non è a 10 m";
			d["item.elevation.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono a 10 m, ma passando al menu Elettricità non sono a 10 m";
			d["item.elevation.ex.category"] = "passando a una strada a sei corsie restano 10 m, ma un ponte o un binario ferroviario doppio ripartono da 0 m e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Modalità parallelismo ----------
			d["item.parallel.label"] = "Modalità parallelismo";
			d["item.parallel.desc"] = "Ricorda le due voci del pannello Strada parallela: l'interruttore Modalità parallelismo e la distanza della Compensazione parallelismo.\nAttiva, accanto alla linea che tracci ne viene costruita una seconda; puoi regolare solo quanto sono distanziate. Spenta per impostazione predefinita, quindi una sola linea come l'originale.";
			d["item.parallel.ex.group"] = "per esempio, attivi la Modalità parallelismo su una piccola strada a due corsie e imposti la Compensazione parallelismo a 5 m, e anche le altre piccole strade tornano attive con gli stessi 5 m, mentre una strada grande no";
			d["item.parallel.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono in parallelo con 5 m di compensazione, ma passando al menu Elettricità no";
			d["item.parallel.ex.category"] = "passando a una strada a sei corsie resta attivo con gli stessi 5 m di compensazione, ma un ponte o un binario della metropolitana tornano a spento e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Aggancio ----------
			d["item.snap.label"] = "Aggancio";
			d["item.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio, per esempio ai lati di una strada, al centro di una strada, ai nodi, alla superficie di un oggetto o alla costa.\nQuali interruttori compaiono dipende dallo strumento; l'interruttore multiplo di tutta la riga non viene ricordato.";
			d["item.snap.ex.group"] = "per esempio, spegni «Aggancia ai lati di una strada» su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno spento, mentre una strada grande ce li ha tutti accesi";
			d["item.snap.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di aggancio, ma passando al menu Elettricità no";
			d["item.snap.ex.category"] = "passando a una strada a sei corsie l'insieme resta lo stesso, ma un ponte o un sentiero ritornano ai valori predefiniti e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Topografia ----------
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Ricorda la riga Topografia, cioè se Mostra le linee di contorno è attivo.";
			d["item.topography.ex.group"] = "per esempio, attivi la Topografia su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attiva, mentre una strada grande no";
			d["item.topography.ex.menu"] = "tutti gli asset che clicchi nel menu Strade mostrano le linee di contorno, ma passando al menu Elettricità no";
			d["item.topography.ex.category"] = "passando a una strada a sei corsie le linee restano, ma un ponte o i cavi elettrici hanno la riga spenta e necessitano un'impostazione propria";


			// ---------- Sinistra e destra (Anarchy) ----------
			d["item.leftRight.label"] = "Sinistra e destra";
			d["item.leftRight.desc"] = "Ricorda quali potenziamenti di rete sono scelti sulle righe Sinistra e Destra del pannello di rete di Anarchy, per esempio Pista ciclabile, Erba, Banchina, Muro di contenimento e Barriera insonorizzante.\nRichiede Anarchy (mod Paradox 74604); senza, questa voce non compare.";
			d["item.leftRight.ex.group"] = "per esempio, scegli Pista ciclabile sul lato destro di una piccola strada a due corsie e anche le altre piccole strade ce l'hanno a destra, mentre una strada grande no";
			d["item.leftRight.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso paio di scelte laterali, ma passando al menu Elettricità no";
			d["item.leftRight.ex.category"] = "passando a una strada a sei corsie la pista ciclabile resta a destra, ma un ponte o un binario ferroviario doppio tornano a niente di scelto e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Generale (Anarchy) ----------
			d["item.general.label"] = "Generale";
			d["item.general.desc"] = "Ricorda gli interruttori della riga Generale del pannello di rete di Anarchy: Terreno, Elevato, Tunnel, Pendio costante, Mediana ampia e Intervallo di elevazione ampliato (che toglie il limite di altezza).\nRichiede Anarchy (mod Paradox 74604); senza, questa voce non compare.";
			d["item.general.ex.group"] = "per esempio, scegli Tunnel su una piccola strada a due corsie e anche le altre piccole strade vengono forzate in tunnel, mentre una strada grande no";
			d["item.general.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di Generale, ma passando al menu Elettricità no";
			d["item.general.ex.category"] = "passando a una strada a sei corsie resta Tunnel, ma un ponte o un binario della metropolitana doppio tornano a niente di spuntato e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Modalità sottosuolo ----------
			d["item.underground.label"] = "Modalità sottosuolo";
			d["item.underground.desc"] = "Ricorda l'interruttore Modalità sottosuolo, che decide se ciò che piazzi va sotto terra o sopra.";
			d["item.underground.ex.group"] = "per esempio, attivi la Modalità sottosuolo su una piccola strada a due corsie e anche le altre piccole strade vanno sottoterra, mentre una strada grande no";
			d["item.underground.ex.menu"] = "tutti gli asset che clicchi nel menu Strade vengono costruiti sottoterra, ma passando al menu Elettricità no";
			d["item.underground.ex.category"] = "passando a una strada a sei corsie si resta sottoterra, ma un ponte o un binario ferroviario doppio rimangono in superficie e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Altro ----------
			d["item.other.label"] = "Altro";
			d["item.other.desc"] = "Ricorda le altre righe che un pannello strumento può registrare: i colori delle strade nella riga Colore.";
			d["item.other.ex.group"] = "per esempio, imposti il colore di una piccola strada a due corsie su blu e anche le altre piccole strade diventano blu, mentre una strada grande no";
			d["item.other.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso colore, ma passando al menu Elettricità no";
			d["item.other.ex.category"] = "passando a una strada a sei corsie resta blu, ma un ponte o un binario ferroviario doppio tengono una copia tutta loro e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（it-IT）====================
		d["tab.master"] = "Interruttore generale";
		d["tab.assets"] = "Asset";
		d["tab.functions"] = "Funzioni";
		d["tab.items"] = "Oggetti";
		d["group.domain"] = "Interruttori per pagina";
		d["domain.asset.label"] = "Ricorda le voci degli strumenti nella pagina Asset";
		d["domain.asset.desc"] = "Attivo: le impostazioni degli strumenti di questa pagina vengono registrate in tempo reale e tornano al caricamento di un salvataggio.\nSpento: di questa pagina non viene registrato né ripristinato nulla; la pagina viene oscurata e gli strumenti tornano al comportamento originale.";
		d["domain.function.label"] = "Ricorda le voci degli strumenti nella pagina Funzioni";
		d["domain.function.desc"] = "Attivo: le impostazioni degli strumenti di questa pagina vengono registrate in tempo reale e tornano al caricamento di un salvataggio.\nSpento: di questa pagina non viene registrato né ripristinato nulla; la pagina viene oscurata e gli strumenti tornano al comportamento originale.";
		d["domain.item.label"] = "Ricorda le voci degli strumenti nella pagina Oggetti";
		d["domain.item.desc"] = "Attivo: le impostazioni degli strumenti di questa pagina vengono registrate in tempo reale e tornano al caricamento di un salvataggio.\nSpento: di questa pagina non viene registrato né ripristinato nulla; la pagina viene oscurata e gli strumenti tornano al comportamento originale.";
		d["enabled.label"] = "Abilita la mod";
		d["enabled.desc"] = "Spento per impostazione predefinita: appena installato, il mod non fa nulla e tutto funziona come nel gioco originale.\nUna volta acceso, i pannelli degli strumenti vengono registrati e tornano al caricamento di un salvataggio. Si accende da solo non appena uno dei tre interruttori per pagina qui sotto è attivo.";
		d["scope.line.sharedSingle"] = "Globale: ogni strumento di questa pagina che supporta questa voce usa un unico valore";
			d["scope.note"] = "Nota: ogni livello qui sopra vale solo all'interno di questa pagina.";
		d["scope.category.function"] = "Stesso tipo";
		d["scope.category.item"] = "Stesso tipo di oggetto";
		d["item.color.label"] = "Colore";
		d["item.decoration.label"] = "Mantieni età";
		d["item.decoration.desc"] = "Ricorda l'interruttore Mantieni età: quando è attivo, gli alberi pennellati o posizionati smettono di crescere e di morire di vecchiaia.\nLa riga compare solo sugli asset con dati sugli alberi, cioè quando si pennellano o posizionano alberi dal menu Vegetazione.";
		d["item.decoration.ex.group"] = "per esempio, attivando Mantieni età mentre si pennellano alberi, anche gli altri modi di pennellare la vegetazione dello stesso gruppo risultano attivi, mentre un altro gruppo no";
		d["item.decoration.ex.menu"] = "tutti gli asset del menu Vegetazione lo mantengono attivo, ma passando a un altro menu no";
		d["item.decoration.ex.category"] = "passando a un altro tipo di albero resta attivo, ma la riga esiste solo sugli asset con dati sugli alberi, quindi pennellando una decalcomania non c'è affatto (serve un'impostazione propria)";
		d["item.i.anarchy.label"] = "Anarchy";
		d["item.brushType.label"] = "Tipo di pennello";
		d["item.brushType.desc"] = "Ricorda quale pennello è selezionato nella riga Tipo di pennello: cerchio, quadrato, bordo morbido e così via, che determina la forma del bordo che dipingi.\nLa riga esiste solo sugli strumenti che possono dipingere: i nove strumenti di Terraformazione e lo strumento oggetti quando l'asset selezionato è pennellabile.";
		d["item.brushType.ex.group"] = "per esempio, scegliendo un pennello quadrato mentre dipingi, anche gli altri asset pennellabili dello stesso gruppo usano il pennello quadrato, mentre un altro gruppo no";
		d["item.brushType.ex.menu"] = "tutti gli asset pennellabili di uno stesso menu usano questo stesso pennello, ma passando a un altro menu no";
		d["item.brushType.ex.category"] = "passando a un'altra categoria si usa il pennello di quella categoria (serve un'impostazione propria), anche quando entrambe possono essere dipinte";
		d["item.brushSize.label"] = "Dimensione pennello";
		d["item.brushSize.desc"] = "Ricorda la riga Dimensione pennello: quanta area copre una passata.\nLa riga esiste solo sugli strumenti che possono dipingere: i nove strumenti di Terraformazione e lo strumento oggetti quando l'asset selezionato è pennellabile.";
		d["item.brushSize.ex.group"] = "per esempio, impostando un pennello da 30 m, anche gli altri asset pennellabili dello stesso gruppo sono a 30 m, mentre un altro gruppo no";
		d["item.brushSize.ex.menu"] = "tutti gli asset pennellabili di uno stesso menu usano questa stessa dimensione del pennello, ma passando a un altro menu no";
		d["item.brushSize.ex.category"] = "passando a un'altra categoria si usa la dimensione del pennello di quella categoria (serve un'impostazione propria), anche quando entrambe possono essere dipinte";
		d["item.brushStrength.label"] = "Robustezza pennello";
		d["item.brushStrength.desc"] = "Ricorda la riga Robustezza pennello: quanto cambia ogni passata.\nLa riga esiste solo sugli strumenti che possono dipingere: i nove strumenti di Terraformazione e lo strumento oggetti quando l'asset selezionato è pennellabile.";
		d["item.brushStrength.ex.group"] = "per esempio, impostando la robustezza del pennello a metà, anche gli altri asset pennellabili dello stesso gruppo sono a metà, mentre un altro gruppo no";
		d["item.brushStrength.ex.menu"] = "tutti gli asset pennellabili di uno stesso menu usano questa stessa robustezza del pennello, ma passando a un altro menu no";
		d["item.brushStrength.ex.category"] = "passando a un'altra categoria si usa la robustezza del pennello di quella categoria (serve un'impostazione propria), anche quando entrambe possono essere dipinte";
		d["item.brushHeight.label"] = "Altezza bersaglio";
		d["item.brushHeight.desc"] = "Ricorda il valore della riga Altezza bersaglio: a che altezza finisce il terreno dopo che uno strumento di Terraformazione lo livella o lo trasforma in un pendio.\nLa riga esiste solo sui due strumenti di Terraformazione Strumento livellamento terreno e Strumento inclinazione terreno.";
		d["item.brushHeight.ex.group"] = "per esempio, impostando l'altezza bersaglio a 50 m mentre livelli, anche gli altri strumenti di Terraformazione dello stesso gruppo usano 50 m, mentre un altro gruppo no";
		d["item.brushHeight.ex.menu"] = "tutti gli strumenti del gruppo Terraformazione usano questa stessa altezza bersaglio, mentre un altro gruppo di funzioni no";
		d["item.brushHeight.ex.category"] = "passando a un altro gruppo di funzioni si usa l'altezza bersaglio di quel gruppo (serve un'impostazione propria); gli altri strumenti di Terraformazione non hanno affatto la riga";
		d["item.distance.label"] = "Distanza";
		d["item.distance.desc"] = "Ricorda il valore della riga Distanza: quanto distano tra loro oggetti consecutivi quando li posizioni lungo una linea o una curva.\nLa riga compare solo nelle modalità Linea e Curva.";
		d["item.distance.ex.group"] = "per esempio, impostando la distanza a 10 m, anche gli altri oggetti dello stesso gruppo distano 10 m, mentre un altro gruppo no";
		d["item.distance.ex.menu"] = "tutti gli asset che possono essere posizionati lungo una linea in uno stesso menu usano questa stessa distanza, ma passando a un altro menu no";
		d["item.distance.ex.category"] = "passando a un'altra categoria si usa la distanza di quella categoria (serve un'impostazione propria), anche quando entrambe possono essere posizionate lungo una linea";
		d["item.ageMask.label"] = "Età";
		d["item.ageMask.desc"] = "Ricorda quali fasi di età sono spuntate nella riga Età: arbusto, giovane, adulto e anziano, e se ne possono spuntare più di una insieme.\nLa riga esiste solo sugli asset di alberi, cioè pennellando o posizionando alberi nel menu Vegetazione.";
		d["item.ageMask.ex.group"] = "per esempio, spuntando solo adulto mentre pennelli alberi, anche gli altri alberi dello stesso gruppo generano solo esemplari adulti, mentre un altro gruppo no";
		d["item.ageMask.ex.menu"] = "tutti gli alberi del menu Vegetazione usano queste stesse fasi di età, ma passando a un altro menu no";
		d["item.ageMask.ex.category"] = "passando a un altro tipo di albero queste fasi restano, ma la riga esiste solo sugli asset di alberi, quindi pennellando decalcomanie e simili non c'è affatto (serve un'impostazione propria)";
		d["item.anarchyObjectElevation.label"] = "Elevazione dell'oggetto";
		d["item.anarchyObjectElevation.desc"] = "Ricorda la riga di controlli dell'altezza che Anarchy aggiunge al pannello dello strumento oggetti: blocco della trasformazione, elevazione, variazione di elevazione e step di aumento e diminuzione.\nTutti e quattro stanno su una sola riga e contano quindi come una sola voce.";
		d["item.anarchyObjectElevation.ex.group"] = "per esempio, impostando l'altezza a 15 m con una variazione di 2 m, anche gli altri oggetti dello stesso gruppo usano gli stessi valori, mentre un altro gruppo no";
		d["item.anarchyObjectElevation.ex.menu"] = "tutti gli oggetti di uno stesso menu usano questa stessa altezza, ma passando a un altro menu no";
		d["item.anarchyObjectElevation.ex.category"] = "passando a un'altra categoria si usa l'altezza di quella categoria (serve un'impostazione propria)";
		d["item.f.toolMode.desc"] = "Ricorda la riga Modalità strumento nei pannelli delle funzioni; le opzioni cambiano da strumento a strumento: i tipi di zona offrono Riempi, Seleziona e Pittura, gli strumenti area Modifica e Genera griglia mappa, ogni strumento di Terraformazione le proprie modalità.";
		d["item.f.toolMode.ex.group"] = "per esempio, scegliendo Pittura per la zona residenziale, anche le zone commerciali, industriali e uffici sono su Pittura, mentre gli strumenti area no";
		d["item.f.toolMode.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni usano questa stessa modalità, mentre un altro gruppo di funzioni no";
		d["item.f.toolMode.ex.category"] = "passando a un altro gruppo di funzioni si torna alla modalità di quel gruppo (serve un'impostazione propria), anche quando entrambi sono raggiungibili dallo stesso menu della barra degli strumenti";
		d["item.f.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio nei pannelli delle funzioni: ad angoli di 90°, alla griglia della zona, alle strade, al lato del proprietario, mostra le linee di contorno e così via.\nQuali interruttori compaiono dipende dallo strumento; il pulsante collettivo di tutta la lista non viene ricordato.";
		d["item.f.snap.ex.group"] = "per esempio, disattivando Aggancia alla griglia della zona per la zona residenziale, anche le zone commerciali, industriali e uffici ce l'hanno disattivato, mentre gli strumenti area no";
		d["item.f.snap.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni usano questo stesso insieme di interruttori, mentre un altro gruppo di funzioni no";
		d["item.f.snap.ex.category"] = "passando a un altro gruppo di funzioni si torna ai valori predefiniti di quel gruppo (serve un'impostazione propria), anche quando entrambi sono raggiungibili dallo stesso menu della barra degli strumenti";
		d["item.f.topography.desc"] = "Ricorda l'attivazione della riga Topografia nei pannelli delle funzioni, cioè se le linee di contorno del terreno sono visibili mentre si disegnano le zone.";
		d["item.f.topography.ex.group"] = "per esempio, spuntando le linee di contorno per la zona residenziale, anche le zone commerciali, industriali e uffici risultano spuntate";
		d["item.f.topography.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni le mantengono spuntate, mentre un altro gruppo di funzioni no";
		d["item.f.topography.ex.category"] = "passando a un altro gruppo di funzioni si usa l'impostazione di quel gruppo";
		d["item.f.underground.desc"] = "Ricorda l'attivazione della riga Modalità sottosuolo nei pannelli delle funzioni.";
		d["item.f.underground.ex.group"] = "per esempio, attivando la modalità sottosuolo per la zona residenziale, anche le zone commerciali, industriali e uffici vanno sottoterra";
		d["item.f.underground.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni restano sottoterra, mentre un altro gruppo di funzioni no";
		d["item.f.underground.ex.category"] = "passando a un altro gruppo di funzioni si usa l'impostazione di quel gruppo";
		d["item.i.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio nei pannelli degli oggetti: alla superficie di un oggetto, alla geometria vicina, alle strade, rimuovi solo il tipo corrispondente e così via, a seconda dell'oggetto.\nQuali interruttori compaiono dipende dall'oggetto.";
		d["item.i.snap.ex.group"] = "per esempio, disattivando Aggancia alla superficie di un oggetto per un oggetto della categoria Parco, anche gli altri oggetti dello stesso gruppo ce l'hanno disattivato, mentre un altro gruppo no";
		d["item.i.snap.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio usano questo stesso insieme di interruttori, ma passando a un altro menu no";
		d["item.i.snap.ex.category"] = "passando a un'altra categoria di oggetti si torna ai valori predefiniti di quella categoria (serve un'impostazione propria), anche quando entrambe sono nel menu Architettura del paesaggio";
		d["item.i.toolMode.desc"] = "Ricorda la riga Modalità strumento nei pannelli degli oggetti: Posiziona uno, Posiziona multipli, Linea, Curva, Strumento timbro oggetti, Pittura e così via, a seconda dello strumento.";
		d["item.i.toolMode.ex.group"] = "per esempio, scegliendo Posiziona multipli per un oggetto della categoria Natura, anche gli altri oggetti dello stesso gruppo usano Posiziona multipli, mentre un altro gruppo no";
		d["item.i.toolMode.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio sono su questa modalità, ma passando a un altro menu no";
		d["item.i.toolMode.ex.category"] = "passando a un'altra categoria di oggetti, per esempio da Natura a Decalcomanie, si torna alla modalità di quella categoria (serve un'impostazione propria), anche quando entrambe sono nel menu Architettura del paesaggio";
		d["item.i.topography.desc"] = "Ricorda l'attivazione della riga Topografia nei pannelli degli oggetti.";
		d["item.i.topography.ex.group"] = "per esempio, spuntando le linee di contorno mentre si posizionano oggetti, anche gli altri oggetti dello stesso gruppo risultano spuntati";
		d["item.i.topography.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio le mantengono spuntate, ma passando a un altro menu no";
		d["item.i.topography.ex.category"] = "passando a un'altra categoria di oggetti si usa l'impostazione di quella categoria";
		d["item.i.underground.desc"] = "Ricorda l'attivazione della riga Modalità sottosuolo nei pannelli degli oggetti, cioè se gli oggetti vengono posizionati sotto o sopra il terreno.";
		d["item.i.underground.ex.group"] = "per esempio, attivando la modalità sottosuolo per un oggetto, anche gli altri oggetti dello stesso gruppo vanno sottoterra";
		d["item.i.underground.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio vanno sottoterra, ma passando a un altro menu no";
		d["item.i.underground.ex.category"] = "passando a un'altra categoria di oggetti si usa l'impostazione di quella categoria";

			return d;
		}
	}
}
