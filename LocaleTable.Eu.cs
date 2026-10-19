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
			d["scope.desc"] = "Wie weit der Wert dieses Punkts geteilt wird. Die Klammern in der Dropdown-Liste zeigen, was das Original tut und was wir empfehlen.";
			d["scope.line.group"] = "Selbe Gruppe: {0}";
			d["scope.line.menu"] = "Selbes Menü: {0}";
			d["scope.line.category"] = "Selber Asset-Typ: einsortiert nach dem, was ein Asset tatsächlich versorgt, sodass Zahl der Spuren, Größe, Stufe und die Kategorie in der Werkzeugleiste keinen Unterschied machen (ein kleiner Parkplatz und ein großer Parkplatz sind derselbe Typ), aber Brücken, Kais und Kreuzungen bilden jeweils einen eigenen Typ, getrennt von den Straßen, also {0}";
			d["scope.line.shared"] = "Global geteilt: jedes Asset und jede Funktion, die diesen Punkt unterstützt, teilt sich den Wert - aber Assets und Funktionen teilen nicht untereinander";
			d["scope.line.unique"] = "Gar nicht geteilt: jedes Asset und jede Funktion, die diesen Punkt unterstützt, wird eigenständig eingestellt";

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
			d["resetall.desc"] = "Setzt jede Option dieses Mods auf den empfohlenen Standardwert zurück: der Hauptschalter wird wieder aktiviert, auch Aktivierung und gemeinsamer Bereich jedes Punkts werden wiederhergestellt. Das bereits gespeicherte Gedächtnis der Spielstände bleibt unberührt.";
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
			d["item.anarchy.desc"] = "Merkt sich den Status des Schalters Anarchy, den der Anarchy-Mod hinzufügt. Anarchy wird im ganzen Spiel geteilt, dieser Punkt ist also global; ist er an, kannst du den gemeinsamen Bereich ändern. Ohne den Anarchy-Mod wird dieser Punkt nicht angezeigt.";
			d["item.anarchy.ex.group"] = "zum Beispiel schaltest du Anarchy für eine zweispurige kleine Straße ein, andere kleine Straßen haben Anarchy ebenfalls an, eine große Straße dagegen nicht";
			d["item.anarchy.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, hat Anarchy an, im Menü Strom dagegen nicht";
			d["item.anarchy.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt Anarchy an, aber eine Brücke oder doppelte U-Bahn-Gleise gehen wieder aus und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			d["item.themes.label"] = "Thema";
			d["item.themes.desc"] = "Merkt, welche Einträge im Filter „Thema“ der Werkzeugleiste angehakt sind. Diese Zeile erscheint nur, wenn die aktuelle Kategorie wirklich Assets enthält, die Themen verwenden, und ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die diese Themen unterstützen. Das Original setzt den Filter erst beim Laden eines Spielstands auf das Standardthema zurück, deshalb empfehlen wir, diesen Punkt ausgeschaltet zu lassen.";
			d["item.themes.ex.group"] = "zum Beispiel hakt du ein Thema nur unter Straßen/Kleine Straßen ein, eine große Straße filtert dann nicht danach";
			d["item.themes.ex.menu"] = "ein im Menü Straßen angehaktes Thema gilt im Menü Strom nicht und ist wieder da, wenn du zurückkehrst";
			d["item.themes.ex.category"] = "der Haken in dieser Zeile gehört zum Filter der Werkzeugleiste selbst, er wird also über das Menü und die Kategorie gemerkt, in denen du gerade bist, und nicht über den Asset-Typ";
			d["item.packs.label"] = "Paket";
			d["item.packs.desc"] = "Merkt, welche Einträge im Filter „Paket“ der Werkzeugleiste angehakt sind; ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die zu diesen Paketen gehören. Das Original leert die Auswahl jedes Mal, wenn du das Menü oder die Kategorie wechselst, das getrennte Merken pro Menü und Kategorie passt also zu dem, was du siehst, deshalb empfehlen wir, auch diesen Punkt ausgeschaltet zu lassen.";
			d["item.packs.ex.group"] = "zum Beispiel hakt du ein Paket unter Straßen/Kleine Straßen ein, das Original leert es bei einer großen Straße, und es ist wieder da, wenn du zurückkehrst";
			d["item.packs.ex.menu"] = "die im Menü Straßen angehakten Pakete und die im Menü Strom angehakten werden getrennt gemerkt";
			d["item.packs.ex.category"] = "der Haken in dieser Zeile gehört zum Filter der Werkzeugleiste selbst, er wird über das Menü und die Kategorie gemerkt, in denen du gerade bist; das Original leert ihn jedes Mal, wenn du die Kategorie wechselst";

			// ---------- Werkzeugmodus ----------
			d["item.toolMode.label"] = "Werkzeugmodus";
			d["item.toolMode.desc"] = "Merkt den Modus jedes Werkzeugs, und die Optionen unterscheiden sich: Straßen, Gleise und Leitungen bieten Gerade, Einfache Kurve, Komplexe Kurve, Durchgehend, Raster, Ersetzen, Punkt; Gebäude, Objekte und Bäume bieten Einzeln platzieren, Mehrfach platzieren, Linie, Kurve, Objektstempel-Werkzeug; Zonen bieten Füllen, Auswahlrechteck, Farbe; Flächen bieten Bearbeiten, Kartenraster erstellen. Modi von Assets und Modi von Funktionen werden getrennt gemerkt, weil ihre Optionen nicht dieselben sind.";
			d["item.toolMode.ex.group"] = "zum Beispiel wählst du Einfache Kurve für eine zweispurige kleine Straße, andere kleine Straßen nutzen ebenfalls Einfache Kurve, eine große Straße dagegen nicht";
			d["item.toolMode.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, steht auf Einfache Kurve, im Menü Strom dagegen nicht";
			d["item.toolMode.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es Einfache Kurve, aber eine Brücke oder doppelte U-Bahn-Gleise fallen auf ihren Standardmodus zurück und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Höhe ----------
			d["item.elevation.label"] = "Höhe";
			d["item.elevation.desc"] = "Merkt die Höhe, mit der ein Werkzeug verlassen wurde, einschließlich des Ergebnisses von Erhöhen und Absenken. Der Höhenunterschied (wie weit dich ein einzelner Druck versetzt; Anarchy nennt diese Zeile Höhenschritt) wird zusammen mit der Höhe gemerkt und nutzt denselben gemeinsamen Bereich. Die Höhe von Kreuzungen ist nicht enthalten. Einige Assets haben eine eigene zulässige Höhe: Liegt der geteilte Wert außerhalb dessen, was das gewählte Asset darf, behält dieses Asset eine für es zulässige Höhe, und der gespeicherte geteilte Wert bleibt unverändert.";
			d["item.elevation.ex.group"] = "zum Beispiel hebst du eine zweispurige kleine Straße auf 10 m, andere kleine Straßen gehen ebenfalls auf 10 m, eine große Straße aber nicht auf 10 m";
			d["item.elevation.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, liegt auf 10 m, im Menü Strom dagegen nicht auf 10 m";
			d["item.elevation.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es bei 10 m, aber eine Brücke oder doppelte Bahngleise gehen zurück auf 0 m und brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Parallelmodus ----------
			d["item.parallel.label"] = "Parallelmodus";
			d["item.parallel.desc"] = "Merkt die beiden Einträge im Panel Parallelstraße: den Schalter Parallelmodus (Parallelmodus an-/abschalten) und den Parallelabstand. Beim Parallelmodus sind es immer zwei Linien: schaltest du ihn ein, wird neben die von dir gezogene Linie eine zweite gebaut, und einstellen kannst du nur, wie weit die beiden auseinander liegen, etwa 1 m oder 2 m. Ist er ausgeschaltet, bleibt es bei einer einzigen Linie.";
			d["item.parallel.ex.group"] = "zum Beispiel schaltest du den Parallelmodus für eine zweispurige kleine Straße ein und stellst den Parallelabstand auf 5 m, andere kleine Straßen kommen ebenfalls eingeschaltet mit denselben 5 m zurück, eine große Straße dagegen nicht";
			d["item.parallel.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, baut parallel mit 5 m Abstand, im Menü Strom dagegen nicht";
			d["item.parallel.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt er an und der Abstand bleibt bei 5 m, aber eine Brücke oder ein U-Bahn-Gleis geht aus und braucht eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Einrasten ----------
			d["item.snap.label"] = "Einrasten";
			d["item.snap.desc"] = "Merkt jeden Schalter der Zeile Einrasten: An existierender Geometrie einrasten, An Rasterzellenlänge einrasten, Bei 90-Grad-Winkeln einrasten, An Straßenkanten einrasten, An Straßen einrasten, An der Seite des Besitzers einrasten, An den Gebäudekanten einrasten, In der Mitte einer Straße einrasten, An der Küste einrasten, An nahegelegener Geometrie einrasten, An Hilfslinien einrasten, An Zellenraster einrasten, An Knoten einrasten, An der Oberfläche eines Objekts einrasten, Aufrecht einrasten, An Flächenraster einrasten, Bindet überlappende Objekte an ein Gebäude, Nur entsprechenden Typ entfernen, Nur Höhenlinien anzeigen, An Distanz einrasten. Die Zeile Einrasten für alle ein-/ausschalten ist ein Sammlerschalter und wird selbst nicht gemerkt.";
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
			d["item.leftRight.desc"] = "Merkt sich, welche Netzwerkerweiterungen auf den Zeilen „Links“ und „Rechts“ im Netzwerkpanel von Anarchy gewählt sind, zum Beispiel Fahrradweg, Bäume, Rasen, Kai, Stützwand und Schallschutzmauer. Benötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod wird dieser Punkt nicht angezeigt.";
			d["item.leftRight.ex.group"] = "zum Beispiel wählst du auf der rechten Seite einer zweispurigen kleinen Straße den Fahrradweg, andere kleine Straßen haben rechts ebenfalls den Fahrradweg, eine große Straße dagegen nicht";
			d["item.leftRight.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dasselbe Paar Seitenauswahlen, im Menü Strom dagegen nicht";
			d["item.leftRight.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt rechts der Fahrradweg, aber bei einer Brücke oder doppelten Bahngleisen ist wieder nichts ausgewählt und sie brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Allgemein (Anarchy) ----------
			d["item.general.label"] = "Allgemein";
			d["item.general.desc"] = "Merkt sich die Schalter auf der Zeile „Allgemein“ im Netzwerkpanel von Anarchy: Boden, Erhöht, Tunnel, Konstante Neigung, Breiter Trennstreifen und Erweiterter Höhenbereich (setzt die Höhenbegrenzung außer Kraft). Benötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod wird dieser Punkt nicht angezeigt.";
			d["item.general.ex.group"] = "zum Beispiel wählst du Tunnel für eine zweispurige kleine Straße, andere kleine Straßen werden ebenfalls in Tunnel gezwungen, eine große Straße dagegen nicht";
			d["item.general.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Allgemein-Schaltern, im Menü Strom dagegen nicht";
			d["item.general.ex.category"] = "wechselt man zu einer sechsspurigen Straße, bleibt es Tunnel, aber bei einer Brücke oder doppelten U-Bahn-Gleisen ist wieder nichts angehakt und sie brauchen eine eigene Einstellung, obwohl alle im Menü Straßen liegen";

			// ---------- Untergrundmodus ----------
			d["item.underground.label"] = "Untergrundmodus";
			d["item.underground.desc"] = "Merkt den Schalter Untergrundmodus (Untergrundmodus an-/abschalten): ob du etwas unter der Erde oder darüber platzierst.";
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
			d["item.brush.label"] = "Pinsel";

		// ==================== v0.7.0 新增 / 重写（de-DE）====================
		d["tab.master"] = "Hauptschalter";
		d["tab.assets"] = "Assets";
		d["tab.functions"] = "Funktionen";
		d["tab.items"] = "Requisiten";
		d["group.domain"] = "Schalter pro Seite";
		d["enabled.label"] = "Mod aktivieren";
		d["enabled.desc"] = "Standardmäßig an. Solange er an ist, wirkt jede Einstellung darunter, und jeder Wert jedes Punkts wird beim Arbeiten fortlaufend aufgezeichnet – kehren Sie zu einem Spielstand zurück, stehen die Panels wieder genau so, wie Sie sie verlassen haben; schalten Sie nur einen einzelnen Punkt aus, wird allein dessen Wiederherstellung beendet, seine Werte werden weiterhin aufgezeichnet. Schalten Sie diesen Hauptschalter aus, kehrt alles zum Original zurück: Die Aufzeichnung endet, nichts wird wiederhergestellt, und alle Einstellungen der Seiten „Assets“, „Funktionen“ und „Requisiten“ sind auf einmal ausgegraut, sodass jedes Werkzeug wieder so arbeitet, wie das Spiel es von sich aus tut. Er schaltet sich selbst aus, wenn die drei Seitenschalter darunter alle aus sind, und wieder ein, sobald auch nur einer von ihnen an ist.";
		d["domain.asset.label"] = "Werkzeugpunkte der Seite „Assets“ merken";
		d["domain.asset.desc"] = "Gilt für jeden Punkt der Seite „Assets“: Aufzeichnen und Wiederherstellen beim Laden werden zusammen geschaltet. Ist der Schalter aus, ist die ganze Seite „Assets“ ausgegraut und ohne Wirkung, die Werkzeuge verhalten sich wie im Original, und die bereits aufgezeichneten Werte bleiben unberührt in der Datei – schalten Sie ihn wieder ein, werden sie wie bisher wiederhergestellt.";
		d["domain.function.label"] = "Werkzeugpunkte der Seite „Funktionen“ merken";
		d["domain.function.desc"] = "Gilt für jeden Punkt der Seite „Funktionen“ – die sechs Funktionsgruppen: Zonentypen, die Flächenwerkzeuge, alle Routenwerkzeuge, die Terraforming-Werkzeuge, Räume und Flächen sowie Markierungen und Objekt-Fertigteile. Ist der Schalter aus, ist die ganze Seite „Funktionen“ ausgegraut und ohne Wirkung, die Werkzeuge verhalten sich wie im Original, und die bereits aufgezeichneten Werte bleiben unberührt in der Datei – schalten Sie ihn wieder ein, werden sie wie bisher wiederhergestellt.";
		d["domain.item.label"] = "Werkzeugpunkte der Seite „Requisiten“ merken";
		d["domain.item.desc"] = "Gilt für jeden Punkt der Seite „Requisiten“ – die acht Requisitenkategorien im Menü „Landschaftsbau“: Natur, Park, Wohnrequisiten, Gewerberequisiten, Industrierequisiten, Trümmer, Markierungen und Topfpflanzen. Ist der Schalter aus, ist die ganze Seite „Requisiten“ ausgegraut und ohne Wirkung, die Werkzeuge verhalten sich wie im Original, und die bereits aufgezeichneten Werte bleiben unberührt in der Datei – schalten Sie ihn wieder ein, werden sie wie bisher wiederhergestellt.";
		d["scope.category.function"] = "Selber Typ";
		d["scope.category.item"] = "Selbe Requisitenart";
		d["scope.line.category.function"] = "Selber Typ: einsortiert nach einer der sechs Funktionsgruppen – Zonentypen (Wohnzone, Gewerbezone, Industriezone, Bürozone), Flächenwerkzeuge (Bezirkserstellungswerkzeug, Industriespezialisierungsgebiet-Werkzeug, Oberflächenwerkzeug), Routenwerkzeuge (jede Route im Menü „Transport“), die neun Terraforming-Werkzeuge (Terrainhöhe anpassen, Terrain ebnen, Terrain angleichen, Terrainsteigung, Textur-Pinselwerkzeug 01-04, Textur-Radiergummi), Räume und Flächen (Parkfläche, Gehbereich, Aufenthaltsfläche, Gebiet bereinigen, Transparente Oberfläche) sowie Markierungen und Objekt-Fertigteile (etwa die Tier-Spawner): {0}";
		d["scope.line.category.item"] = "Selbe Requisitenart: einsortiert nach einer der acht Requisitenkategorien im Menü „Landschaftsbau“ (Natur, Park, Wohnrequisiten, Gewerberequisiten, Industrierequisiten, Trümmer, Markierungen, Topfpflanzen); ein anderes Modell oder eine andere Größe innerhalb derselben Kategorie ist kein anderer Typ: {0}";
		d["scope.line.sharedSingle"] = "Global geteilt: jedes Werkzeug auf dieser Seite, das diesen Punkt unterstützt, nutzt denselben Wert";
		d["scope.note"] = "Hinweis: Die Seiten „Assets“, „Funktionen“ und „Requisiten“ führen jeweils ihre eigene Aufzeichnung, auch wenn eine Zeile auf mehreren Seiten gleich heißt; „Global geteilt“ und „Gar nicht geteilt“ gelten ebenfalls nur innerhalb der Seite, die Sie gerade ansehen.";
		d["item.color.label"] = "Farbe";
		d["item.decoration.label"] = "Alter beibehalten";
		d["item.decoration.desc"] = "Merkt sich den Schalter „Alter beibehalten“: Ist er an, wachsen die Bäume, die Sie anmalen oder setzen, nicht weiter und sterben auch nicht mehr an Altersschwäche – das Alter dieses Baumbestands bleibt damit genau auf dem Stand von jetzt stehen. Die Zeile erscheint nur bei Assets, die Baumdaten tragen (also beim Malen von Bäumen im Menü „Vegetation“); Assets, deren Panel diese Zeile nicht hat, werden weder aufgezeichnet noch verändert.";
		d["item.decoration.ex.group"] = "zum Beispiel schalten Sie „Alter beibehalten“ beim Malen von Bäumen ein, die anderen Arten, Vegetation zu malen, sind in derselben Gruppe ebenfalls an, in einer anderen Gruppe dagegen nicht";
		d["item.decoration.ex.menu"] = "jedes Asset im Menü „Vegetation“ behält den Schalter an, in einem anderen Menü dagegen nicht";
		d["item.decoration.ex.category"] = "wechseln Sie zu einer anderen Baumart, bleibt er an, doch diese Zeile gibt es nur bei Assets mit Baumdaten – beim Malen von Markierungen und Ähnlichem fehlt sie ganz (braucht eine eigene Einstellung)";
		d["item.f.brush.desc"] = "Merkt sich die Zeilen „Pinselgröße“ und „Pinselstärke“ auf Funktionspanels: Pinselgröße bestimmt, wie viel Fläche ein Strich erfasst, Pinselstärke, wie viel er verändert. Wirkt nur bei Funktionen, deren Panel diese beiden Zeilen wirklich hat, also bei den neun Terraforming-Werkzeugen (Terrainhöhe anpassen, Terrain ebnen, Terrain angleichen, Terrainsteigung, Textur-Pinselwerkzeug 01-04 und Textur-Radiergummi) und beim Malen von Zonen; Funktionen ohne diese Zeilen werden weder aufgezeichnet noch verändert.";
		d["item.f.brush.ex.group"] = "zum Beispiel stellen Sie bei Terrainhöhe anpassen die Pinselgröße auf 30 m, die übrigen Terraforming-Werkzeuge derselben Gruppe stehen dann ebenfalls auf 30 m, die einer anderen Gruppe nicht";
		d["item.f.brush.ex.menu"] = "jedes Werkzeug der Terraforming-Gruppe nutzt dieselbe Pinselgröße und -stärke, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.brush.ex.category"] = "wechseln Sie zu einer anderen Funktionsgruppe, gilt deren eigener Pinsel (braucht eine eigene Einstellung), auch wenn beide Panels diese beiden Zeilen haben";
		d["item.f.snap.desc"] = "Merkt sich jeden Schalter der Zeile „Einrasten“ auf Funktionspanels: bei 90-Grad-Winkeln einrasten, an Zellenraster einrasten, an Straßen einrasten, an der Seite des Besitzers einrasten, nur Höhenlinien anzeigen und so weiter – welche Schalter erscheinen, hängt vom Werkzeug ab. Der Sammlerschalter des Spiels „Einrasten aktivieren/deaktivieren“ ist ein Knopf für die ganze Liste und wird selbst nicht gemerkt. Die Einrastzeile der Seite „Assets“ und die der Seite „Requisiten“ werden getrennt gemerkt und beeinflussen einander nicht.";
		d["item.f.snap.ex.group"] = "zum Beispiel schalten Sie „An Zellenraster einrasten“ für die Wohnzone aus, Gewerbe-, Industrie- und Bürozone haben es dann ebenfalls aus, die Flächenwerkzeuge dagegen nicht";
		d["item.f.snap.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe nutzt dieselbe Reihe von Schaltern, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.snap.ex.category"] = "wechseln Sie zu einer anderen Funktionsgruppe, gelten deren eigene Standardschalter (braucht eine eigene Einstellung), auch wenn beide über dasselbe Menü der Werkzeugleiste erreichbar sind";
		d["item.f.toolMode.desc"] = "Merkt sich die Auswahl der Zeile „Werkzeugmodus“ auf Funktionspanels, und die Optionen unterscheiden sich je Werkzeug: Zonentypen bieten Füllen, Auswahlrechteck und Farbe; die Flächenwerkzeuge (Bezirkserstellungswerkzeug, Industriespezialisierungsgebiet-Werkzeug, Oberflächenwerkzeug) bieten Bearbeiten und Kartenraster erstellen; jedes Terraforming-Werkzeug hat eigene Modi; die Planierraupe bietet Hauptelement, Unterelement und Alles. Der Werkzeugmodus der Seite „Assets“ und der der Seite „Requisiten“ werden getrennt gemerkt, und keiner beeinflusst die anderen.";
		d["item.f.toolMode.ex.group"] = "zum Beispiel wählen Sie „Farbe“ für die Wohnzone, Gewerbe-, Industrie- und Bürozone stehen dann ebenfalls auf „Farbe“, die Flächenwerkzeuge dagegen nicht";
		d["item.f.toolMode.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe nutzt denselben Modus, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.toolMode.ex.category"] = "wechseln Sie zu einer anderen Funktionsgruppe, gilt wieder deren eigener Modus (braucht eine eigene Einstellung), auch wenn beide über dasselbe Menü der Werkzeugleiste erreichbar sind";
		d["item.f.topography.desc"] = "Merkt sich den Schalter der Zeile „Topographie“ auf Funktionspanels, also ob Sie beim Ziehen von Zonen die Höhenlinien des Geländes sehen. Diese Zeile wird seitenweise gelesen und geschrieben: die Seite „Funktionen“ führt die Aufzeichnung der Zonen, die Seite „Assets“ ihre eigene, und keine überschreibt die andere.";
		d["item.f.topography.ex.group"] = "zum Beispiel haken Sie die Höhenlinien für die Wohnzone an, Gewerbe-, Industrie- und Bürozone sind dann ebenfalls angehakt";
		d["item.f.topography.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe bleibt angehakt, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.topography.ex.category"] = "wechseln Sie zu einer anderen Funktionsgruppe, gilt deren eigene Aufzeichnung (braucht eine eigene Einstellung); die Zeile „Topographie“ der Seite „Assets“ ist eine separate Aufzeichnung und bleibt davon unberührt";
		d["item.f.underground.desc"] = "Merkt sich den Schalter der Zeile „Untergrundmodus“ auf Funktionspanels. Diese Zeile wird seitenweise gelesen und geschrieben: die Seite „Funktionen“ führt die Aufzeichnung der Zonen, die Seiten „Assets“ und „Requisiten“ führen jeweils ihre eigene, geteilt wird dabei nichts.";
		d["item.f.underground.ex.group"] = "zum Beispiel schalten Sie den Untergrundmodus für die Wohnzone ein, Gewerbe-, Industrie- und Bürozone liegen dann ebenfalls unter der Erde";
		d["item.f.underground.ex.menu"] = "jedes Werkzeug innerhalb derselben Funktionsgruppe bleibt unter der Erde, eine andere Funktionsgruppe dagegen nicht";
		d["item.f.underground.ex.category"] = "wechseln Sie zu einer anderen Funktionsgruppe, gilt deren eigene Aufzeichnung (braucht eine eigene Einstellung); der „Untergrundmodus“ der Seite „Assets“ ist eine separate Aufzeichnung und bleibt davon unberührt";
		d["item.i.brush.desc"] = "Merkt sich die Zeilen „Pinselgröße“ und „Pinselstärke“ auf Requisitenpanels, also wie weit ein Pinselstrich reicht und wie viele Requisiten er pro Strich setzt. Wirkt nur bei Requisiten, deren Panel diese beiden Zeilen wirklich hat; Requisiten ohne diese Zeilen werden weder aufgezeichnet noch verändert.";
		d["item.i.brush.ex.group"] = "zum Beispiel stellen Sie beim Malen von Requisiten die Pinselgröße auf 20 m, die anderen malbaren Requisiten derselben Gruppe stehen dann ebenfalls auf 20 m, die einer anderen Gruppe nicht";
		d["item.i.brush.ex.menu"] = "jede malbare Requisite im Menü „Landschaftsbau“ nutzt denselben Pinsel, in einem anderen Menü dagegen nicht";
		d["item.i.brush.ex.category"] = "wechseln Sie zu einer anderen Requisitenkategorie, gilt deren eigener Pinsel (braucht eine eigene Einstellung), obwohl beide im Menü „Landschaftsbau“ liegen";
		d["item.i.snap.desc"] = "Merkt sich jeden Schalter der Zeile „Einrasten“ auf Requisitenpanels: an der Oberfläche eines Objekts einrasten, an nahegelegener Geometrie einrasten, an Straßen einrasten, nur entsprechenden Typ entfernen und so weiter, je nach Requisite. Die Einrastzeile der Seite „Assets“ und die der Seite „Funktionen“ werden getrennt gemerkt, und keine beeinflusst die anderen.";
		d["item.i.snap.ex.group"] = "zum Beispiel schalten Sie „An der Oberfläche eines Objekts einrasten“ für eine Park-Requisite aus, die anderen Requisiten derselben Gruppe haben es dann ebenfalls aus, die einer anderen Gruppe nicht";
		d["item.i.snap.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ nutzt dieselbe Reihe von Schaltern, in einem anderen Menü dagegen nicht";
		d["item.i.snap.ex.category"] = "wechseln Sie zu einer anderen Requisitenkategorie, gelten deren eigene Standardschalter (braucht eine eigene Einstellung), obwohl beide im Menü „Landschaftsbau“ liegen";
		d["item.i.toolMode.desc"] = "Merkt sich die Auswahl der Zeile „Werkzeugmodus“ auf Requisitenpanels: Einzeln platzieren, Mehrfach platzieren, Linie, Kurve, Objektstempel-Werkzeug, Farbe und so weiter, je nach Werkzeug. Der Werkzeugmodus der Seite „Assets“ und der der Seite „Funktionen“ werden getrennt gemerkt, und keiner beeinflusst die anderen.";
		d["item.i.toolMode.ex.group"] = "zum Beispiel wählen Sie „Mehrfach platzieren“ für eine Natur-Requisite, die anderen Requisiten derselben Gruppe nutzen dann ebenfalls „Mehrfach platzieren“, die einer anderen Gruppe nicht";
		d["item.i.toolMode.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ steht auf diesem Modus, in einem anderen Menü dagegen nicht";
		d["item.i.toolMode.ex.category"] = "wechseln Sie zu einer anderen Requisitenkategorie, etwa von Natur zu Markierungen, gilt wieder deren eigener Modus (braucht eine eigene Einstellung), obwohl beide im Menü „Landschaftsbau“ liegen";
		d["item.i.topography.desc"] = "Merkt sich den Schalter der Zeile „Topographie“ auf Requisitenpanels. Diese Zeile wird seitenweise gelesen und geschrieben: die Seite „Requisiten“ führt die Aufzeichnung der Requisiten, die Seiten „Assets“ und „Funktionen“ führen jeweils ihre eigene, geteilt wird dabei nichts.";
		d["item.i.topography.ex.group"] = "zum Beispiel haken Sie die Höhenlinien beim Setzen von Requisiten an, die anderen Requisiten derselben Gruppe sind dann ebenfalls angehakt";
		d["item.i.topography.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ bleibt angehakt, in einem anderen Menü dagegen nicht";
		d["item.i.topography.ex.category"] = "wechseln Sie zu einer anderen Requisitenkategorie, gilt deren eigene Aufzeichnung (braucht eine eigene Einstellung); die Zeilen „Topographie“ der Seiten „Assets“ und „Funktionen“ sind separate Aufzeichnungen und bleiben davon unberührt";
		d["item.i.underground.desc"] = "Merkt sich den Schalter der Zeile „Untergrundmodus“ auf Requisitenpanels, also ob Sie Requisiten unter- oder oberirdisch setzen. Diese Zeile wird seitenweise gelesen und geschrieben: die Seite „Requisiten“ führt die Aufzeichnung der Requisiten, die Seiten „Assets“ und „Funktionen“ führen jeweils ihre eigene, geteilt wird dabei nichts.";
		d["item.i.underground.ex.group"] = "zum Beispiel schalten Sie den Untergrundmodus für eine Requisite ein, die anderen Requisiten derselben Gruppe gehen dann ebenfalls unter die Erde";
		d["item.i.underground.ex.menu"] = "jede Requisite im Menü „Landschaftsbau“ landet unter der Erde, in einem anderen Menü dagegen nicht";
		d["item.i.underground.ex.category"] = "wechseln Sie zu einer anderen Requisitenkategorie, gilt deren eigene Aufzeichnung (braucht eine eigene Einstellung); der „Untergrundmodus“ der Seiten „Assets“ und „Funktionen“ ist eine separate Aufzeichnung und bleibt davon unberührt";
		d["item.brush.desc"] = "Merkt sich die Zeilen „Pinselgröße“ und „Pinselstärke“: Pinselgröße bestimmt, wie viel Fläche ein Strich erfasst, Pinselstärke, wie viel er verändert. Dieser Punkt deckt nur das Malen auf der Seite „Assets“ ab, also das Setzen von Assets wie Bäumen aus dem Menü „Vegetation“ im Modus „Farbe“; die neun Terraforming-Werkzeuge haben ihren eigenen Pinselpunkt auf der Seite „Funktionen“, die acht Requisitenkategorien den ihren auf der Seite „Requisiten“, und die drei Seiten teilen nichts untereinander. Werkzeuge, deren Panel diese beiden Zeilen nicht hat, werden weder aufgezeichnet noch verändert.";
		d["item.brush.ex.group"] = "zum Beispiel stellen Sie beim Malen von Bäumen die Pinselgröße auf 30 m, die anderen malbaren Assets derselben Gruppe stehen dann ebenfalls auf 30 m, ein Werkzeug einer anderen Gruppe nicht";
		d["item.brush.ex.menu"] = "jedes Asset, das Sie im Menü „Vegetation“ malen, nutzt dieselbe Pinselgröße und -stärke, in einem anderen Menü dagegen nicht";
		d["item.brush.ex.category"] = "wechseln Sie zu einer anderen Baumart, bleibt derselbe Pinsel erhalten, doch die Terraforming-Werkzeuge und die Requisiten führen je eine eigene Aufzeichnung auf einer anderen Seite (braucht eine eigene Einstellung)";
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
			d["scope.desc"] = "Hasta qué punto se comparte el valor de este elemento. Los paréntesis de la lista desplegable indican qué hace el juego original y qué recomendamos.";
			d["scope.line.group"] = "Mismo grupo: {0}";
			d["scope.line.menu"] = "Mismo menú: {0}";
			d["scope.line.category"] = "Mismo tipo de activo: se agrupa según a quién sirve de verdad el activo, así que el número de carriles, el tamaño, el nivel y la categoría de la barra de herramientas no suponen ninguna diferencia (un aparcamiento pequeño y uno grande son el mismo tipo), pero los puentes, los atracaderos y los intercambiadores forman cada uno su propio tipo, aparte de las carreteras, es decir {0}";
			d["scope.line.shared"] = "Global: cada activo y cada función que admitan este elemento comparten el valor, pero los activos y las funciones no comparten entre ellos";
			d["scope.line.unique"] = "Sin compartir: cada activo y cada función que admita este elemento se ajusta por separado";

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
			d["resetall.desc"] = "Devuelve cada opción de este mod a su valor recomendado: el interruptor general vuelve a activarse, y también se restauran el estado de activación y el ámbito compartido de cada elemento. La memoria ya registrada de las partidas no se toca.";
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
			d["item.anarchy.desc"] = "Recuerda el estado del interruptor Anarchy que añade el mod Anarchy. Anarchy se comparte en todo el juego, así que este elemento es global; una vez activado puedes cambiar el ámbito compartido. Sin el mod Anarchy este elemento no aparece.";
			d["item.anarchy.ex.group"] = "por ejemplo, activas Anarchy en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen activado, mientras que una carretera grande no";
			d["item.anarchy.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras tienen Anarchy activado, pero al pasar al menú de Electricidad ya no";
			d["item.anarchy.ex.category"] = "cambiar a una carretera de seis carriles sigue teniéndolo activado, pero un puente o unas vías de metro dobles lo dejan desactivado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			d["item.themes.label"] = "Temática";
			d["item.themes.desc"] = "Recuerda qué casillas están marcadas en el filtro «Temática» de la barra de herramientas. Esa fila solo aparece cuando la categoría actual contiene de verdad assets que usan temas, y marcar una limita la lista de la barra a los assets que admiten esos temas. El juego original solo la restablece al tema predeterminado al cargar una partida, así que te recomendamos dejar este elemento desactivado.";
			d["item.themes.ex.group"] = "por ejemplo, marcas un tema solo en Carreteras/Carreteras pequeñas, de modo que una carretera grande ya no filtra por él";
			d["item.themes.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras siguen el tema marcado allí, pero al pasar al menú de Electricidad no";
			d["item.themes.ex.category"] = "lo marcado en esta fila es estado del propio filtro de la barra de herramientas, así que se recuerda por el menú y la categoría en los que estés, no por el tipo de activo";
			d["item.packs.label"] = "Paquete";
			d["item.packs.desc"] = "Recuerda qué casillas están marcadas en el filtro «Paquete» de la barra de herramientas; marcar una limita la lista de la barra a los assets de esos paquetes. El juego original los vacía cada vez que cambias de menú o de categoría, así que recordarlos por menú y categoría coincide con lo que ves, y también te recomendamos dejar este elemento desactivado.";
			d["item.packs.ex.group"] = "por ejemplo, marcas un paquete en Carreteras/Carreteras pequeñas, el juego original lo borra en las carreteras grandes y vuelve al regresar";
			d["item.packs.ex.menu"] = "los paquetes marcados en el menú de Carreteras y en el menú de Electricidad se recuerdan por separado";
			d["item.packs.ex.category"] = "lo marcado en esta fila es estado del propio filtro de la barra de herramientas, que se recuerda por el menú y la categoría en los que estés; el juego original la vacía cada vez que cambias de categoría";

			// ---------- Herramientas ----------
			d["item.toolMode.label"] = "Herramientas";
			d["item.toolMode.desc"] = "Recuerda el modo elegido para cada herramienta, y las opciones cambian según la herramienta: carreteras, vías y tuberías ofrecen Recta, Curva sencilla, Curva compleja, Continua, Cuadrícula, Reemplazar y Punto; edificios, accesorios y árboles ofrecen Colocar uno, Colocar varios, Línea, Curva y Herramienta de sello de objetos; las zonas ofrecen Rellenar, Seleccionar y Pintar; las áreas ofrecen Editar y Generar cuadrícula de mapa. Los modos de activos y los de funciones se guardan por separado porque sus opciones no son las mismas.";
			d["item.toolMode.ex.group"] = "por ejemplo, eliges Curva sencilla en una carretera pequeña de dos carriles y las otras carreteras pequeñas también usan Curva sencilla, mientras que una carretera grande no";
			d["item.toolMode.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en Curva sencilla, pero al pasar al menú de Electricidad ya no";
			d["item.toolMode.ex.category"] = "cambiar a una carretera de seis carriles sigue dando Curva sencilla, pero un puente o unas vías de metro dobles vuelven a su modo predeterminado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Elevación ----------
			d["item.elevation.label"] = "Elevación";
			d["item.elevation.desc"] = "Recuerda la elevación con la que dejaste la herramienta, incluido el resultado de Aumentar la elevación y Disminuir la elevación. El Escalón de elevación (lo que avanza con una sola pulsación; Anarchy llama a esa fila Paso de Elevación) se recuerda junto con ella y usa el mismo ámbito compartido. No cubre la elevación de las intersecciones. Algunos activos tienen su propia franja de elevación permitida: si el valor compartido queda fuera de lo que permite el activo seleccionado, este conserva una elevación que le sea válida y el valor compartido guardado no cambia.";
			d["item.elevation.ex.group"] = "por ejemplo, subes una carretera de dos carriles a 10 m y las otras carreteras pequeñas también quedan a 10 m, mientras que una carretera grande no está a 10 m";
			d["item.elevation.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están a 10 m, pero al pasar al menú de Electricidad no están a 10 m";
			d["item.elevation.ex.category"] = "cambiar a una carretera de seis carriles sigue dando 10 m, pero un puente o unas vías de tren dobles vuelven a 0 m y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Modo paralelo ----------
			d["item.parallel.label"] = "Modo paralelo";
			d["item.parallel.desc"] = "Recuerda las dos cosas del panel Calle paralela: el interruptor Modo paralelo (Alternar modo paralelo) y la separación de la Compensación paralela. Con el modo paralelo siempre son dos líneas: al activarlo se construye una segunda línea junto a la que trazas, y lo que puedes ajustar es lo separadas que están esas dos, por ejemplo 1 m o 2 m. Al desactivarlo te quedas solo con la línea única.";
			d["item.parallel.ex.group"] = "por ejemplo, activas el Modo paralelo en una carretera pequeña de dos carriles y pones la Compensación paralela en 5 m, las otras carreteras pequeñas vuelven activadas con los mismos 5 m, mientras que una carretera grande no";
			d["item.parallel.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en paralelo con 5 m de compensación, pero al pasar al menú de Electricidad ya no";
			d["item.parallel.ex.category"] = "cambiar a una carretera de seis carriles sigue activado con los mismos 5 m de compensación, pero un puente o una vía de metro se desactiva y necesita su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Ajustar ----------
			d["item.snap.label"] = "Ajustar";
			d["item.snap.desc"] = "Recuerda cada interruptor de la fila Ajustar: Ajustar a una geometría establecida, Ajustar al largo de la celda de zonificación, Ajustar a ángulos de 90 grados, Ajustar a los lados de una carretera, Ajustar a las carreteras, Ajustar al lado del propietario, Ajustar a los lados del edificio, Ajustar a la mitad de la carretera, Ajustar a la orilla, Ajustar a una geometría cercana, Ajustar a las líneas de guía, Ajustar a la cuadrícula de la zona, Ajustar a los nodos, Ajustar a la superficie de un objeto, Ajustar en vertical, Ajustar a la cuadrícula del solar, Une los objetos superpuestos a un edificio, Elimina solo el tipo coincidente, Muestra las líneas de contorno y Ajustar a la distancia. La fila Activar/desactivar todos los ajustes del juego es un interruptor múltiple y no se recuerda.";
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
			d["item.leftRight.desc"] = "Recuerda las mejoras de red elegidas en las filas Izquierda y Derecha del panel de red de Anarchy, por ejemplo Vía ciclista, Árboles, Césped, Muelle, Muro de Retención y Barrera acústica. Necesita Anarchy (mod 74604 de Paradox); sin él este elemento no aparece.";
			d["item.leftRight.ex.group"] = "por ejemplo, eliges Vía ciclista en el lado derecho de una carretera pequeña de dos carriles y las otras carreteras pequeñas también la reciben a la derecha, mientras que una carretera grande no";
			d["item.leftRight.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo par de selecciones laterales, pero al pasar al menú de Electricidad no";
			d["item.leftRight.ex.category"] = "cambiar a una carretera de seis carriles sigue teniendo Vía ciclista a la derecha, pero un puente o unas vías de tren dobles vuelven a no tener nada elegido y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- General (Anarchy) ----------
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Recuerda los interruptores de la fila General del panel de red de Anarchy: Terreno, Elevado, Túnel, Pendiente Constante, Mediana Amplia y Rango de Elevación Expandido (que quita el límite de altura). Necesita Anarchy (mod 74604 de Paradox); sin él este elemento no aparece.";
			d["item.general.ex.group"] = "por ejemplo, eliges Túnel en una carretera pequeña de dos carriles y las otras carreteras pequeñas también se obligan a ser túneles, mientras que una carretera grande no";
			d["item.general.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de General, pero al pasar al menú de Electricidad no";
			d["item.general.ex.category"] = "cambiar a una carretera de seis carriles sigue dando Túnel, pero un puente o unas vías de metro dobles vuelven a no tener nada marcado y necesitan su propio ajuste, aunque todos estén en el menú de Carreteras";

			// ---------- Modo subterráneo ----------
			d["item.underground.label"] = "Modo subterráneo";
			d["item.underground.desc"] = "Recuerda el interruptor Modo subterráneo (Alternar modo subterráneo), que decide si lo que colocas va bajo tierra o encima.";
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
			d["item.brush.label"] = "Pincel";

		// ==================== v0.7.0 新增 / 重写（es-ES）====================
		d["enabled.label"] = "Activar el mod";
		d["enabled.desc"] = "Activado por defecto. Mientras esté activado, todos los ajustes de abajo surten efecto y el valor de cada elemento se registra mientras trabaja, de modo que al volver a una partida los paneles quedan exactamente como los dejó; desactivar un solo elemento solo impide que ese elemento se restaure: sus valores siguen registrándose. Al desactivar este interruptor general todo vuelve al juego original: se deja de registrar, no se restaura nada y todos los ajustes de las páginas de Activos, Funciones y Objetos se desactivan a la vez (se muestran en gris), de forma que cada herramienta se comporta como lo hace el juego por sí mismo. Se desactiva solo cuando los tres interruptores por página de abajo están desactivados, y se vuelve a activar en cuanto cualquiera de ellos está activado.";
		d["scope.line.sharedSingle"] = "Global: todas las herramientas de esta página que admiten este elemento usan un mismo valor";
		d["scope.note"] = "Nota: las páginas de Activos, Funciones y Objetos guardan cada una su propia memoria incluso cuando una fila tiene el mismo nombre en varias páginas; «Global» y «Sin compartir» solo se aplican dentro de la página que está viendo.";
		d["item.brush.desc"] = "Recuerda las filas «Tamaño de pincel» y «Fuerza de pincel»: el tamaño de pincel define hasta dónde llega una pasada y la fuerza de pincel define cuánto cambia cada pasada. Este elemento solo cubre el pintado de la página de Activos, es decir, la colocación de activos como árboles del menú Vegetación con el modo Pintar; las nueve herramientas de terreno tienen su propio elemento de pincel en la página de Funciones y las categorías de objetos tienen el suyo en la página de Objetos, y las tres páginas nunca comparten. Las herramientas cuyo panel no tiene esas filas no se registran ni se modifican.";
		d["item.brush.ex.group"] = "por ejemplo, si pone el tamaño de pincel en 30 m mientras pinta árboles, los demás activos pintables del mismo grupo también usan 30 m, mientras que los de otro grupo no";
		d["item.brush.ex.menu"] = "todos los activos que pinta en el menú Vegetación usan este mismo tamaño y fuerza de pincel, pero al pasar a otro menú no";
		d["item.brush.ex.category"] = "al cambiar a otro tipo de árbol se mantiene este mismo pincel, pero este nivel agrupa los activos según a qué sirven, de modo que las herramientas de terreno y los objetos guardan cada uno su propia copia en otra página (necesita su propio ajuste)";
		d["domain.asset.desc"] = "Cubre todos los elementos de la página de Activos: el registro y la restauración al cargar se activan juntos. Mientras esté desactivada, toda la página de Activos se muestra en gris, las herramientas se comportan como en el juego original y los valores ya registrados permanecen intactos en el archivo, de modo que al volver a activarla se restauran.";
		d["domain.asset.label"] = "Recordar los elementos de herramienta de los activos";
		d["domain.function.desc"] = "Cubre todos los elementos de la página de Funciones, los seis grupos de funciones: los tipos de zona, las herramientas de área, todas las herramientas de ruta, las herramientas de terreno, los espacios y las áreas, y los marcadores y los prefabricados de objeto. Mientras esté desactivada, toda la página de Funciones se muestra en gris, las herramientas se comportan como en el juego original y los valores ya registrados permanecen intactos en el archivo, de modo que al volver a activarla se restauran.";
		d["domain.function.label"] = "Recordar los elementos de herramienta de las funciones";
		d["domain.item.desc"] = "Cubre todos los elementos de la página de Objetos, las ocho categorías de objetos del menú Paisajismo: Naturaleza, Parque, Objetos residenciales, Objetos comerciales, Objetos industriales, Escombros, Calcomanías y Plantas en maceta. Mientras esté desactivada, toda la página de Objetos se muestra en gris, las herramientas se comportan como en el juego original y los valores ya registrados permanecen intactos en el archivo, de modo que al volver a activarla se restauran.";
		d["domain.item.label"] = "Recordar los elementos de herramienta de los objetos";
		d["group.domain"] = "Interruptores por página";
		d["item.color.label"] = "Color";
		d["item.decoration.desc"] = "Recuerda el interruptor «Preservar edad»: mientras esté activado, los árboles que pinta o coloca dejan de crecer y dejan de morir de viejos, lo que congela la edad de ese grupo de árboles tal como está. La fila solo aparece en los activos que contienen datos de árbol (al pintar árboles del menú Vegetación); los activos cuyo panel no tiene esa fila no se registran ni se modifican.";
		d["item.decoration.ex.category"] = "al cambiar a otro tipo de árbol sigue activado, pero la fila solo existe en los activos con datos de árbol, de modo que pintar una calcomanía no tiene esa fila en absoluto (necesita su propio ajuste)";
		d["item.decoration.ex.group"] = "por ejemplo, si activa «Preservar edad» mientras pinta árboles, las demás formas de pintar vegetación del mismo grupo también la tienen activada, pero las de otro grupo no";
		d["item.decoration.ex.menu"] = "todos los activos del menú Vegetación la mantienen activada, pero al pasar a otro menú no";
		d["item.decoration.label"] = "Preservar edad";
		d["item.f.brush.desc"] = "Recuerda las filas «Tamaño de pincel» y «Fuerza de pincel» de los paneles de las herramientas de función: el tamaño de pincel define hasta dónde llega una pasada y la fuerza de pincel define cuánto cambia cada pasada. Solo se ven afectadas las funciones cuyo panel tiene realmente estas dos filas, es decir, las nueve herramientas de terreno (Herramienta de altura de terreno, Herramienta de suavización de terreno, Herramienta de nivelación de terreno, Herramienta de pendientes, Herramienta de pintura de textura 01-04 y Borrador de textura) y el pintado de zonas; las funciones sin esas filas no se registran ni se modifican.";
		d["item.f.brush.ex.category"] = "al cambiar a otro grupo de funciones se usa el pincel propio de ese grupo (necesita su propio ajuste), aunque ambos paneles tengan estas dos filas";
		d["item.f.brush.ex.group"] = "por ejemplo, si pone el tamaño de pincel de una herramienta de terreno en 30 m, las demás herramientas de terreno del mismo grupo también usan 30 m, mientras que las de otro grupo no";
		d["item.f.brush.ex.menu"] = "todas las herramientas del grupo de Terraformación usan este mismo tamaño y fuerza de pincel, pero las de otro grupo de funciones no";
		d["item.f.snap.desc"] = "Recuerda cada interruptor de la fila «Ajustar» de los paneles de las herramientas de función: ajustar a ángulos de 90 grados, ajustar a la cuadrícula, ajustar a las carreteras, ajustar al lado del propietario, mostrar las líneas de contorno y demás; los interruptores que aparecen dependen de la herramienta. El propio «Activar/desactivar todos los ajustes» del juego es un interruptor por lotes y no se recuerda. La fila «Ajustar» de la página de Activos y la de la página de Objetos se recuerdan por separado y ninguna afecta a la otra.";
		d["item.f.snap.ex.category"] = "al cambiar a otro grupo de funciones se vuelve a los valores predeterminados de ese grupo (necesita su propio ajuste), aunque los dos sean accesibles desde el mismo menú de la barra de herramientas";
		d["item.f.snap.ex.group"] = "por ejemplo, si desactiva el ajuste a la cuadrícula para la zona residencial, las zonas comercial, industrial y de oficinas también lo tienen desactivado, pero las herramientas de área no";
		d["item.f.snap.ex.menu"] = "todas las herramientas de un mismo grupo de funciones usan este mismo conjunto de interruptores, pero las de otro grupo de funciones no";
		d["item.f.toolMode.desc"] = "Recuerda el modo de herramienta elegido en los paneles de las herramientas de función. Las opciones cambian según la herramienta: los tipos de zona ofrecen Rellenar, Seleccionar y Pintar; las herramientas de área (Herramienta de creación de distritos, Herramienta de área de especialización industrial y Herramienta de zona de superficie) ofrecen Editar y Generar cuadrícula de mapa; cada herramienta de terreno tiene sus propios modos; la herramienta de demolición ofrece Elementos principales, Subelementos y Todo. El modo de herramienta de la página de Activos y el de la página de Objetos se recuerdan por separado y ninguno afecta al otro.";
		d["item.f.toolMode.ex.category"] = "al cambiar a otro grupo de funciones se vuelve al modo propio de ese grupo (necesita su propio ajuste), aunque ambos sean accesibles desde el mismo menú de la barra de herramientas";
		d["item.f.toolMode.ex.group"] = "por ejemplo, si elige Pintar para la zona residencial, las zonas comercial, industrial y de oficinas también están en Pintar, pero las herramientas de área no";
		d["item.f.toolMode.ex.menu"] = "todas las herramientas de un mismo grupo de funciones usan este mismo modo, pero las de otro grupo de funciones no";
		d["item.f.topography.desc"] = "Recuerda el estado de la fila «Líneas de contorno» de los paneles de las herramientas de función, es decir, si se ven las líneas de contorno del terreno mientras dibuja zonas. Esta fila se lee y se escribe por página: la página de Funciones guarda la copia de las zonas y la página de Activos guarda la suya, y ninguna sobrescribe a la otra.";
		d["item.f.topography.ex.category"] = "al cambiar a otro grupo de funciones se usa la copia propia de ese grupo (necesita su propio ajuste); la fila «Líneas de contorno» de la página de Activos es una copia aparte y no se ve afectada";
		d["item.f.topography.ex.group"] = "por ejemplo, si marca las líneas de contorno para la zona residencial, las zonas comercial, industrial y de oficinas también las tienen marcadas";
		d["item.f.topography.ex.menu"] = "todas las herramientas de un mismo grupo de funciones las tienen marcadas, pero las de otro grupo de funciones no";
		d["item.f.underground.desc"] = "Recuerda el estado de la fila «Modo subterráneo» de los paneles de las herramientas de función. Esta fila se lee y se escribe por página: la página de Funciones guarda la copia de las zonas, y las páginas de Activos y de Objetos guardan cada una la suya, y ninguna se comparte.";
		d["item.f.underground.ex.category"] = "al cambiar a otro grupo de funciones se usa la copia propia de ese grupo (necesita su propio ajuste); la fila «Modo subterráneo» de la página de Activos es una copia aparte y no se ve afectada";
		d["item.f.underground.ex.group"] = "por ejemplo, si activa el modo subterráneo para la zona residencial, las zonas comercial, industrial y de oficinas también están bajo tierra";
		d["item.f.underground.ex.menu"] = "todas las herramientas de un mismo grupo de funciones están en modo subterráneo, pero las de otro grupo de funciones no";
		d["item.i.brush.desc"] = "Recuerda las filas «Tamaño de pincel» y «Fuerza de pincel» de los paneles de los objetos, es decir, hasta dónde llega una pasada del pincel y cuántos objetos coloca una pasada. Solo se ven afectados los objetos cuyo panel tiene realmente estas dos filas; los objetos que no las tienen no se registran ni se modifican.";
		d["item.i.brush.ex.category"] = "al cambiar a otra categoría de objetos se usa el pincel propio de esa categoría (necesita su propio ajuste), aunque ambas estén en el menú Paisajismo";
		d["item.i.brush.ex.group"] = "por ejemplo, si pone el tamaño de pincel en 20 m, los demás objetos pintables del mismo grupo también usan 20 m, mientras que los de otro grupo no";
		d["item.i.brush.ex.menu"] = "todos los objetos pintables del menú Paisajismo usan este mismo pincel, pero al pasar a otro menú no";
		d["item.i.snap.desc"] = "Recuerda cada interruptor de la fila «Ajustar» de los paneles de los objetos: ajustar a la superficie del edificio, ajustar a las formas cercanas, ajustar a la carretera, eliminar solo el tipo coincidente y demás, dependiendo del objeto. La fila «Ajustar» de la página de Activos y la de la página de Funciones se recuerdan por separado y ninguna afecta a la otra.";
		d["item.i.snap.ex.category"] = "al cambiar a otra categoría de objetos se vuelve a los valores predeterminados de esa categoría (necesita su propio ajuste), aunque ambas estén en el menú Paisajismo";
		d["item.i.snap.ex.group"] = "por ejemplo, si desactiva el ajuste a la superficie del edificio para un objeto de Parque, los demás objetos del mismo grupo también lo tienen desactivado, mientras que los de otro grupo no";
		d["item.i.snap.ex.menu"] = "todos los objetos del menú Paisajismo usan este mismo conjunto de interruptores, pero al pasar a otro menú no";
		d["item.i.toolMode.desc"] = "Recuerda el modo de herramienta elegido en los paneles de los objetos: Colocar uno, Colocar varios, Línea, Curva, Herramienta de sello de objetos, Pintar y demás, dependiendo de la herramienta. El modo de herramienta de la página de Activos y el de la página de Funciones se recuerdan por separado y ninguno afecta al otro.";
		d["item.i.toolMode.ex.category"] = "al cambiar a otra categoría de objetos, por ejemplo de Naturaleza a Calcomanías, se vuelve al modo propio de esa categoría (necesita su propio ajuste), aunque ambas estén en el menú Paisajismo";
		d["item.i.toolMode.ex.group"] = "por ejemplo, si elige Colocar varios para un objeto de Naturaleza, los demás objetos del mismo grupo también usan Colocar varios, mientras que los de otro grupo no";
		d["item.i.toolMode.ex.menu"] = "todos los objetos del menú Paisajismo están en este modo, pero al pasar a otro menú no";
		d["item.i.topography.desc"] = "Recuerda el estado de la fila «Líneas de contorno» de los paneles de los objetos. Esta fila se lee y se escribe por página: la página de Objetos guarda la copia de los objetos, y las páginas de Activos y de Funciones guardan cada una la suya, y ninguna se comparte.";
		d["item.i.topography.ex.category"] = "al cambiar a otra categoría de objetos se usa la copia propia de esa categoría (necesita su propio ajuste); las filas «Líneas de contorno» de las páginas de Activos y de Funciones son copias aparte y no se ven afectadas";
		d["item.i.topography.ex.group"] = "por ejemplo, si marca las líneas de contorno mientras coloca objetos, los demás objetos del mismo grupo también las tienen marcadas";
		d["item.i.topography.ex.menu"] = "todos los objetos del menú Paisajismo las tienen marcadas, pero al pasar a otro menú no";
		d["item.i.underground.desc"] = "Recuerda el estado de la fila «Modo subterráneo» de los paneles de los objetos, es decir, si los objetos se colocan bajo el suelo o encima de él. Esta fila se lee y se escribe por página: la página de Objetos guarda la copia de los objetos, y las páginas de Activos y de Funciones guardan cada una la suya, y ninguna se comparte.";
		d["item.i.underground.ex.category"] = "al cambiar a otra categoría de objetos se usa la copia propia de esa categoría (necesita su propio ajuste); las filas «Modo subterráneo» de las páginas de Activos y de Funciones son copias aparte y no se ven afectadas";
		d["item.i.underground.ex.group"] = "por ejemplo, si activa el modo subterráneo para un objeto, los demás objetos del mismo grupo también pasan a estar bajo tierra";
		d["item.i.underground.ex.menu"] = "todos los objetos del menú Paisajismo están en modo subterráneo, pero al pasar a otro menú no";
		d["scope.category.function"] = "Mismo tipo";
		d["scope.category.item"] = "Mismo tipo de objeto";
		d["scope.line.category.function"] = "Mismo tipo: se agrupa según a cuál de los seis grupos de funciones pertenece la herramienta: los tipos de zona (residencial, comercial, industrial y de oficinas), las herramientas de área (Herramienta de creación de distritos, Herramienta de área de especialización industrial y Herramienta de zona de superficie), las herramientas de ruta (todas las rutas del menú de Transporte), las nueve herramientas de terreno (Herramienta de altura de terreno, Herramienta de suavización de terreno, Herramienta de nivelación de terreno, Herramienta de pendientes, Herramienta de pintura de textura 01-04 y Borrador de textura), los espacios y las áreas (Zona de parque, Zona transitable, Zona de espera, Eliminar zona y Superficie de clip) y los marcadores y los prefabricados de objeto (la herramienta de aparición de animales y similares): {0}";
		d["scope.line.category.item"] = "Mismo tipo de objeto: se agrupa según a cuál de las ocho categorías de objetos del menú Paisajismo pertenece el elemento (Naturaleza, Parque, Objetos residenciales, Objetos comerciales, Objetos industriales, Escombros, Calcomanías y Plantas en maceta); un modelo distinto o un tamaño distinto dentro de una misma categoría no es un tipo distinto: {0}";
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
			d["scope.desc"] = "Dans quelle mesure la valeur de cet élément est partagée. Les parenthèses de la liste déroulante indiquent le comportement d'origine et notre recommandation.";
			d["scope.line.group"] = "Même groupe : {0}";
			d["scope.line.menu"] = "Même menu : {0}";
			d["scope.line.category"] = "Même type d'actif : classé d'après ce à quoi sert réellement l'asset, donc le nombre de voies, la taille, le niveau et la catégorie de la barre d'outils ne font aucune différence (un petit et un grand parc de stationnement sont du même type), mais les ponts, les quais et les croisements forment chacun leur propre type, à part les routes, c'est-à-dire {0}";
			d["scope.line.shared"] = "Global : tout asset et toute fonction compatibles avec cet élément partagent la valeur, mais les assets et les fonctions ne partagent jamais entre eux";
			d["scope.line.unique"] = "Aucun partage : chaque asset et chaque fonction compatibles avec cet élément se règlent indépendamment";

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
			d["resetall.desc"] = "Remet chaque option de ce mod à son réglage recommandé : l'interrupteur général se réactive, et l'état comme la portée de partage de chaque élément sont rétablis. La mémoire déjà enregistrée pour les parties n'est pas touchée.";
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
			d["item.anarchy.desc"] = "Mémorise l'état du bouton Anarchy ajouté par le mod Anarchy. Anarchy est partagé dans tout le jeu, cet élément est donc global ; une fois activé, vous pouvez modifier la portée de partage. Sans le mod Anarchy, cet élément n'apparaît pas.";
			d["item.anarchy.ex.group"] = "par exemple, activez Anarchy sur une petite route à deux voies, les autres petites routes l'ont activé aussi, mais une grande route non";
			d["item.anarchy.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes ont Anarchy activé, mais dans le menu Électricité ce n'est plus le cas";
			d["item.anarchy.ex.category"] = "passer à une route à six voies laisse Anarchy activé, mais un pont ou une double voie de métro repasse à désactivé et réclame son propre réglage, bien que tous soient dans le menu Routes";

			d["item.themes.label"] = "Thème";
			d["item.themes.desc"] = "Mémorise les entrées cochées dans le filtre « Thème » de la barre d'outils. Cette ligne n'apparaît que si la catégorie courante contient vraiment des assets utilisant des thèmes, et cocher un thème limite la liste de la barre aux assets qui prennent en charge ces thèmes. Le jeu d'origine ne remet ce filtre sur le thème par défaut qu'au chargement d'une sauvegarde, nous vous recommandons donc de laisser cet élément désactivé.";
			d["item.themes.ex.group"] = "par exemple, cochez un thème uniquement sous Routes/Petites routes, une grande route ne filtre donc plus selon lui";
			d["item.themes.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes suivent le thème coché dans ce menu, mais dans le menu Électricité ce n'est plus le cas";
			d["item.themes.ex.category"] = "cette ligne est l'état du filtre de la barre d'outils lui-même, elle est donc mémorisée d'après le menu et la catégorie où vous vous trouvez, et non d'après le type d'actif";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Mémorise les entrées cochées dans le filtre « Pack » de la barre d'outils ; cocher un pack limite la liste de la barre aux assets appartenant à ces packs. Le jeu d'origine les efface à chaque changement de menu ou de catégorie, les mémoriser par menu et catégorie correspond donc à ce que vous voyez, et nous vous recommandons aussi de laisser cet élément désactivé.";
			d["item.packs.ex.group"] = "par exemple, cochez un pack sous Routes/Petites routes, le jeu d'origine l'efface sur une grande route et il revient quand vous y retournez";
			d["item.packs.ex.menu"] = "les packs cochés dans le menu Routes et dans le menu Électricité sont mémorisés séparément";
			d["item.packs.ex.category"] = "cette ligne est l'état du filtre de la barre d'outils lui-même, mémorisée d'après le menu et la catégorie où vous vous trouvez ; le jeu d'origine l'efface à chaque changement de catégorie";

			// ---------- Mode Outil ----------
			d["item.toolMode.label"] = "Mode Outil";
			d["item.toolMode.desc"] = "Mémorise le mode choisi pour chaque outil, et les options varient : routes, rails et canalisations proposent Droit, Courbe simple, Courbe complexe, Continu, Grille, Remplacer et Point ; bâtiments, accessoires et arbres proposent En placer un, En placer plusieurs, Ligne, Courbe et Outil de marquage d'objet ; les zones proposent Remplissage, Sélection et Peinture ; les périmètres proposent Modifier et Générer une grille de carte. Les modes des assets et ceux des fonctions sont mémorisés séparément, car leurs options diffèrent.";
			d["item.toolMode.ex.group"] = "par exemple, choisissez Courbe simple sur une petite route à deux voies, les autres petites routes utilisent aussi Courbe simple, mais une grande route non";
			d["item.toolMode.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en Courbe simple, mais dans le menu Électricité ce n'est plus le cas";
			d["item.toolMode.ex.category"] = "passer à une route à six voies donne toujours Courbe simple, mais un pont ou une double voie de métro retombe sur son mode par défaut et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Élévation ----------
			d["item.elevation.label"] = "Élévation";
			d["item.elevation.desc"] = "Mémorise l'élévation laissée sur l'outil, y compris le résultat de Augmenter l'élévation et Diminuer l'élévation. L'Étape d'élévation (le déplacement d'une seule pression ; l'interface Anarchy nomme cette ligne Incrément d'élévation) est mémorisée avec elle et utilise la même portée de partage. L'élévation des croisements n'est pas concernée. Certains assets ont leur propre plage d'élévation : si la valeur partagée sort de ce que permet l'asset sélectionné, celui-ci conserve une élévation qui lui est possible et la valeur partagée mémorisée reste inchangée.";
			d["item.elevation.ex.group"] = "par exemple, montez une route à deux voies à 10 m, les autres petites routes passent aussi à 10 m, mais une grande route n'est pas à 10 m";
			d["item.elevation.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont à 10 m, mais dans le menu Électricité ils ne sont pas à 10 m";
			d["item.elevation.ex.category"] = "passer à une route à six voies donne toujours 10 m, mais un pont ou une double voie ferrée repart à 0 m et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Mode parallèle ----------
			d["item.parallel.label"] = "Mode parallèle";
			d["item.parallel.desc"] = "Mémorise les deux éléments du panneau Route parallèle : l'interrupteur Mode parallèle (Changer le mode parallèle) et l'espacement de la Compensation parallèle. En mode parallèle, ce sont toujours deux lignes : une fois activé, une seconde ligne se construit le long de celle que vous tracez, et ce que vous pouvez régler est la distance qui sépare ces deux lignes, par exemple 1 m ou 2 m. Désactivé, vous n'avez que la ligne simple.";
			d["item.parallel.ex.group"] = "par exemple, activez le Mode parallèle sur une petite route à deux voies et réglez la Compensation parallèle sur 5 m, les autres petites routes reviennent activées avec les mêmes 5 m, mais une grande route non";
			d["item.parallel.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en parallèle avec 5 m de compensation, mais dans le menu Électricité ce n'est plus le cas";
			d["item.parallel.ex.category"] = "passer à une route à six voies reste activé avec les mêmes 5 m de compensation, mais un pont ou une voie de métro se désactive et réclame son propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Accrocher ----------
			d["item.snap.label"] = "Accrocher";
			d["item.snap.desc"] = "Mémorise chaque interrupteur de la ligne Accrocher : Accrochez à la géométrie existante, Accrochez à la longueur de la cellule de zonage, Accrochez aux angles à 90 degrés, Accrochez aux côtés d'une route, Accrochez aux routes, Accrochez au côté du propriétaire, Accrochez aux côtés d'un bâtiment, Accrochez au milieu d'une route, Accrochez au rivage, Accrochez à la géométrie à proximité, Accrochez aux lignes directrices, Accrochez à la grille de zone, Accrochez aux nœuds, Accrochez à la surface d'un objet, Accrochez verticalement, Accrochez à la grille des emplacements, Lie les éléments qui se chevauchent à un bâtiment, Supprimer uniquement le type correspondant, Afficher les lignes de contour et Accrochez à distance. La ligne Activer/désactiver tous les accrochages du jeu est un interrupteur groupé et n'est pas mémorisée.";
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
			d["item.leftRight.desc"] = "Mémorise les améliorations de réseau choisies sur les lignes Gauche et Droite du panneau réseau d'Anarchy, par exemple Piste cyclable, Arbres, Herbe, Quai, Mur de soutènement et Barrière acoustique. Nécessite Anarchy (mod Paradox 74604) ; sans lui cet élément n'apparaît pas.";
			d["item.leftRight.ex.group"] = "par exemple, choisissez Piste cyclable à droite d'une petite route à deux voies, les autres petites routes l'ont aussi à droite, mais une grande route non";
			d["item.leftRight.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même choix des deux côtés, mais dans le menu Électricité ce n'est plus le cas";
			d["item.leftRight.ex.category"] = "passer à une route à six voies garde Piste cyclable à droite, mais un pont ou une double voie ferrée reviennent à rien de choisi et réclament leur propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Général (Anarchy) ----------
			d["item.general.label"] = "Général";
			d["item.general.desc"] = "Mémorise les interrupteurs de la ligne Général du panneau réseau d'Anarchy : Au sol, Élévation, Tunnel, Pente constante, Grande médiane et Gamme d'élévation élargie (qui supprime la limite de hauteur). Nécessite Anarchy (mod Paradox 74604) ; sans lui cet élément n'apparaît pas.";
			d["item.general.ex.group"] = "par exemple, choisissez Tunnel sur une petite route à deux voies, les autres petites routes sont forcées en tunnel aussi, mais une grande route non";
			d["item.general.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs de Général, mais dans le menu Électricité ce n'est plus le cas";
			d["item.general.ex.category"] = "passer à une route à six voies donne toujours Tunnel, mais un pont ou une double voie de métro reviennent à rien de coché et réclament leur propre réglage, bien que tous soient dans le menu Routes";

			// ---------- Mode souterrain ----------
			d["item.underground.label"] = "Mode souterrain";
			d["item.underground.desc"] = "Mémorise l'interrupteur Mode souterrain (Changer le mode souterrain), qui décide si ce que vous placez passe sous le sol ou dessus.";
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
			d["item.brush.label"] = "Pinceau";

		// ==================== v0.7.0 新增 / 重写（fr-FR）====================
		d["tab.master"] = "Interrupteur principal";
		d["tab.assets"] = "Assets";
		d["tab.functions"] = "Fonctions";
		d["tab.items"] = "Accessoires";
		d["group.domain"] = "Interrupteurs par page";
		d["domain.asset.label"] = "Mémoriser les éléments d'outil des assets";
		d["domain.asset.desc"] = "Couvre tous les éléments de la page Assets : l'enregistrement et la restauration au chargement s'activent et se coupent ensemble. Quand cet interrupteur est coupé, toute la page Assets est grisée et inopérante, chaque outil retrouve son comportement d'origine, et les valeurs déjà enregistrées restent conservées dans le fichier, donc le réactiver les restaure comme avant.";
		d["domain.function.label"] = "Mémoriser les éléments d'outil des fonctions";
		d["domain.function.desc"] = "Couvre tous les éléments de la page Fonctions : les six familles que sont les zones, les outils de périmètre, tous les outils de tracé, les outils de terraformation, les espaces et aires, et les marqueurs et objets préfabriqués. Quand cet interrupteur est coupé, toute la page Fonctions est grisée et inopérante, chaque outil retrouve son comportement d'origine, et les valeurs déjà enregistrées restent conservées dans le fichier, donc le réactiver les restaure comme avant.";
		d["domain.item.label"] = "Mémoriser les éléments d'outil des accessoires";
		d["domain.item.desc"] = "Couvre tous les éléments de la page Accessoires : les huit familles d'accessoires du menu Aménagement paysager, à savoir Nature, Parc, Accessoires résidentiels, Accessoires commerciaux, Accessoires industriels, Débris, Décalcomanies et Plantes en pot. Quand cet interrupteur est coupé, toute la page Accessoires est grisée et inopérante, chaque outil retrouve son comportement d'origine, et les valeurs déjà enregistrées restent conservées dans le fichier, donc le réactiver les restaure comme avant.";
		d["item.color.label"] = "Couleur";
		d["item.decoration.label"] = "Préserver l'âge";
		d["item.decoration.desc"] = "Mémorise l'interrupteur « Préserver l'âge » : une fois coché, les arbres que vous brossez cessent de grandir comme de vieillir et ne tombent plus, ce qui fige l'âge de ce peuplement d'arbres à son état du moment. Cette ligne n'apparaît que sur les assets qui portent des données d'arbres (le brossage d'arbres depuis le menu Végétation) ; les assets dont le panneau n'a pas cette ligne ne sont ni enregistrés ni modifiés.";
		d["item.decoration.ex.group"] = "par exemple, cochez « Préserver l'âge » en brossant des arbres et les autres façons de brosser la végétation du même groupe sont activées elles aussi, mais un autre groupe ne l'est pas";
		d["item.decoration.ex.menu"] = "tous les assets du menu Végétation gardent cette ligne activée, mais en changeant de menu ce n'est plus le cas";
		d["item.decoration.ex.category"] = "passer à une autre sorte d'arbre laisse aussi cette ligne activée, mais elle n'existe que sur les assets portant des données d'arbres, donc brosser des décalcomanies n'a pas du tout cette ligne (réglage séparé nécessaire)";
		d["item.f.brush.desc"] = "Mémorise les lignes « Taille du pinceau » et « Force du pinceau » des panneaux de fonctions : la taille décide de l'étendue couverte par une passe et la force décide de ce que chaque passe modifie. Seules les fonctions dont le panneau affiche vraiment ces deux lignes sont concernées, à savoir les neuf outils de terraformation (Outil de déplacement de terrain, Outil d'atténuation du terrain, Outil de nivellement de terrain, Outil pour les terrains en pente, Outil de peinture de texture 01 à 04 et Gomme de texture) et la peinture des zones ; les fonctions sans ces deux lignes ne sont ni enregistrées ni modifiées.";
		d["item.f.brush.ex.group"] = "par exemple, réglez la taille du pinceau de l'Outil de déplacement de terrain sur 30 m et les autres outils de terraformation du même groupe passent aussi à 30 m, mais un autre groupe ne l'est pas";
		d["item.f.brush.ex.menu"] = "tous les outils de la famille Terraformation utilisent cette même taille et la même force de pinceau, mais une autre famille de fonctions ne le fait pas";
		d["item.f.brush.ex.category"] = "passer à une autre famille de fonctions utilise le pinceau propre à cette famille (réglage séparé nécessaire), même quand les deux panneaux affichent ces deux lignes";
		d["item.f.snap.desc"] = "Mémorise chaque interrupteur de la ligne « Accrocher » des panneaux de fonctions : « Accrochez aux angles à 90 degrés », « Accrochez à la grille de zone », « Accrochez aux routes », « Accrochez au côté du propriétaire », « Afficher les lignes de contour » et d'autres encore, les options variant selon l'outil. La ligne « Activer/désactiver tous les accrochages » du jeu elle-même est un interrupteur groupé et n'est pas mémorisée. La ligne « Accrocher » de la page Assets et celle de la page Accessoires sont mémorisées chacune pour leur compte et aucune n'influence les autres.";
		d["item.f.snap.ex.group"] = "par exemple, désactivez « Accrochez à la grille de zone » pour la zone résidentielle et les zones commerciale, industrielle et de bureaux l'ont désactivé elles aussi, mais les outils de périmètre non";
		d["item.f.snap.ex.menu"] = "tous les outils d'une même famille de fonctions utilisent ce même jeu d'interrupteurs, mais une autre famille de fonctions ne le fait pas";
		d["item.f.snap.ex.category"] = "passer à une autre famille de fonctions rétablit les interrupteurs par défaut de cette famille (réglage séparé nécessaire), même quand les deux sont atteignables depuis le même menu de la barre d'outils";
		d["item.f.toolMode.desc"] = "Mémorise le choix de la ligne « Mode Outil » des panneaux de fonctions, et les options varient selon l'outil : les zones proposent Remplissage, Sélection et Peinture ; les outils de périmètre (Outil de création de quartier, Outil pour zone de spécialisation industrielle, Outil de surface) proposent Modifier et Générer une grille de carte ; chaque outil de terraformation a ses propres modes ; l'outil de démolition propose Éléments principaux uniquement, Avec les sous-éléments et Tout. Le « Mode Outil » de la page Assets et celui de la page Accessoires sont mémorisés chacun pour leur compte et aucun n'influence les autres.";
		d["item.f.toolMode.ex.group"] = "par exemple, choisissez Peinture pour la zone résidentielle et les zones commerciale, industrielle et de bureaux sont en Peinture elles aussi, mais les outils de périmètre non";
		d["item.f.toolMode.ex.menu"] = "tous les outils d'une même famille de fonctions utilisent ce même mode, mais une autre famille de fonctions ne le fait pas";
		d["item.f.toolMode.ex.category"] = "passer à une autre famille de fonctions retombe sur le mode propre à cette famille (réglage séparé nécessaire), même quand les deux sont atteignables depuis le même menu de la barre d'outils";
		d["item.f.topography.desc"] = "Mémorise l'état de la ligne « Lignes de contour » des panneaux de fonctions, c'est-à-dire si vous voyez ou non les courbes de niveau du terrain en dessinant des zones. Cette ligne se lit et s'écrit page par page : la page Fonctions garde la copie des zones, la page Assets garde la sienne, et l'une n'écrase jamais l'autre.";
		d["item.f.topography.ex.group"] = "par exemple, cochez les lignes de contour pour la zone résidentielle et les zones commerciale, industrielle et de bureaux sont cochées elles aussi";
		d["item.f.topography.ex.menu"] = "tous les outils d'une même famille de fonctions les gardent cochées, mais une autre famille de fonctions ne le fait pas";
		d["item.f.topography.ex.category"] = "passer à une autre famille de fonctions utilise la copie propre à cette famille (réglage séparé nécessaire) ; la ligne « Lignes de contour » de la page Assets est une copie à part et n'est pas touchée";
		d["item.f.underground.desc"] = "Mémorise l'état de la ligne « Mode souterrain » des panneaux de fonctions. Cette ligne se lit et s'écrit page par page : la page Fonctions garde la copie des zones, les pages Assets et Accessoires gardent chacune la leur, et aucune n'est partagée.";
		d["item.f.underground.ex.group"] = "par exemple, activez le mode souterrain pour la zone résidentielle et les zones commerciale, industrielle et de bureaux passent elles aussi sous terre";
		d["item.f.underground.ex.menu"] = "tous les outils d'une même famille de fonctions restent en mode souterrain, mais une autre famille de fonctions ne le fait pas";
		d["item.f.underground.ex.category"] = "passer à une autre famille de fonctions utilise la copie propre à cette famille (réglage séparé nécessaire) ; la ligne « Mode souterrain » de la page Assets est une copie à part et n'est pas touchée";
		d["item.i.brush.desc"] = "Mémorise les lignes « Taille du pinceau » et « Force du pinceau » des panneaux d'accessoires, c'est-à-dire l'étendue couverte par une passe de brossage et le nombre d'accessoires posés par passe. Seuls les accessoires dont le panneau affiche vraiment ces deux lignes sont concernés ; les accessoires sans ces lignes ne sont ni enregistrés ni modifiés.";
		d["item.i.brush.ex.group"] = "par exemple, réglez la taille du pinceau du mode Pinceau sur 20 m et les autres accessoires brossables du même groupe sont aussi à 20 m, mais un autre groupe ne l'est pas";
		d["item.i.brush.ex.menu"] = "tous les accessoires brossables du menu Aménagement paysager utilisent ce même pinceau, mais en changeant de menu ce n'est plus le cas";
		d["item.i.brush.ex.category"] = "passer à une autre famille d'accessoires utilise le pinceau propre à cette famille (réglage séparé nécessaire), même quand les deux sont sous le menu Aménagement paysager";
		d["item.i.snap.desc"] = "Mémorise chaque interrupteur de la ligne « Accrocher » des panneaux d'accessoires : « Accrochez à la surface d'un bâtiment », « Accrochez à la géométrie à proximité », « Accrochez aux routes », « Supprimer uniquement le type correspondant » et d'autres encore, les options variant selon l'accessoire. La ligne « Accrocher » de la page Assets et celle de la page Fonctions sont mémorisées chacune pour leur compte et aucune n'influence les autres.";
		d["item.i.snap.ex.group"] = "par exemple, désactivez « Accrochez à la surface d'un bâtiment » pour un accessoire de la famille Parc et les autres accessoires du même groupe l'ont désactivé eux aussi, mais un autre groupe non";
		d["item.i.snap.ex.menu"] = "tous les accessoires du menu Aménagement paysager utilisent ce même jeu d'interrupteurs, mais en changeant de menu ce n'est plus le cas";
		d["item.i.snap.ex.category"] = "passer à une autre famille d'accessoires rétablit les interrupteurs par défaut de cette famille (réglage séparé nécessaire), même quand les deux sont sous le menu Aménagement paysager";
		d["item.i.toolMode.desc"] = "Mémorise le choix de la ligne « Mode Outil » des panneaux d'accessoires : En placer un, En placer plusieurs, Ligne, Courbe, Outil de marquage d'objet, Peinture et ainsi de suite selon l'outil. Le « Mode Outil » de la page Assets et celui de la page Fonctions sont mémorisés chacun pour leur compte et aucun n'influence les autres.";
		d["item.i.toolMode.ex.group"] = "par exemple, choisissez En placer plusieurs pour un accessoire de la famille Nature et les autres accessoires du même groupe sont en En placer plusieurs eux aussi, mais un autre groupe ne l'est pas";
		d["item.i.toolMode.ex.menu"] = "tous les accessoires du menu Aménagement paysager sont sur ce mode, mais en changeant de menu ce n'est plus le cas";
		d["item.i.toolMode.ex.category"] = "passer à une autre famille d'accessoires, de Nature à Décalcomanies par exemple, retombe sur le mode propre à cette famille (réglage séparé nécessaire), même quand les deux sont sous le menu Aménagement paysager";
		d["item.i.topography.desc"] = "Mémorise l'état de la ligne « Lignes de contour » des panneaux d'accessoires. Cette ligne se lit et s'écrit page par page : la page Accessoires garde la copie des accessoires, les pages Assets et Fonctions gardent chacune la leur, et aucune n'est partagée.";
		d["item.i.topography.ex.group"] = "par exemple, cochez les lignes de contour en posant des accessoires et les autres accessoires du même groupe sont cochés eux aussi";
		d["item.i.topography.ex.menu"] = "tous les accessoires du menu Aménagement paysager les gardent cochées, mais en changeant de menu ce n'est plus le cas";
		d["item.i.topography.ex.category"] = "passer à une autre famille d'accessoires utilise la copie propre à cette famille (réglage séparé nécessaire) ; les lignes « Lignes de contour » des pages Assets et Fonctions sont des copies à part et ne sont pas touchées";
		d["item.i.underground.desc"] = "Mémorise l'état de la ligne « Mode souterrain » des panneaux d'accessoires, c'est-à-dire si vous posez les accessoires sous le sol ou par-dessus. Cette ligne se lit et s'écrit page par page : la page Accessoires garde la copie des accessoires, les pages Assets et Fonctions gardent chacune la leur, et aucune n'est partagée.";
		d["item.i.underground.ex.group"] = "par exemple, activez le mode souterrain pour un accessoire et les autres accessoires du même groupe passent eux aussi sous terre";
		d["item.i.underground.ex.menu"] = "tous les accessoires du menu Aménagement paysager passent sous terre, mais en changeant de menu ce n'est plus le cas";
		d["item.i.underground.ex.category"] = "passer à une autre famille d'accessoires utilise la copie propre à cette famille (réglage séparé nécessaire) ; les lignes « Mode souterrain » des pages Assets et Fonctions sont des copies à part et ne sont pas touchées";
		d["scope.category.function"] = "Même type";
		d["scope.category.item"] = "Même type d'accessoire";
		d["scope.line.category.function"] = "Même type : regroupement d'après la famille de fonctions à laquelle appartient l'outil, au nombre de six : les zones (résidentielle, commerciale, industrielle et de bureaux), les outils de périmètre (Outil de création de quartier, Outil pour zone de spécialisation industrielle, Outil de surface), les outils de tracé (toutes les routes du menu Transports), les neuf outils de terraformation (Outil de déplacement de terrain, Outil d'atténuation du terrain, Outil de nivellement de terrain, Outil pour les terrains en pente, Outil de peinture de texture 01 à 04 et Gomme de texture), les espaces et aires (Zone de stationnement, Zone de promenade, Zone d'attente, Zone claire et Surface de l'attache), et les marqueurs et objets préfabriqués (l'outil d'apparition des animaux et ses semblables) : {0}";
		d["scope.line.category.item"] = "Même type d'accessoire : regroupement d'après la famille d'accessoires du menu Aménagement paysager où se trouve l'accessoire, au nombre de huit (Nature, Parc, Accessoires résidentiels, Accessoires commerciaux, Accessoires industriels, Débris, Décalcomanies et Plantes en pot) ; un modèle différent ou une taille différente au sein d'une même famille ne change pas le type : {0}";
		d["enabled.label"] = "Activer le mod";
		d["enabled.desc"] = "Cette option est activée par défaut. Tant qu'elle est active, tous les réglages ci-dessous prennent effet et la valeur de chaque élément d'outil est enregistrée en continu, si bien qu'en revenant dans une sauvegarde les panneaux retrouvent exactement l'état où vous les avez laissés ; désactiver un seul élément arrête seulement sa restauration, ses valeurs continuent d'être enregistrées. Couper cet interrupteur général rend tout au jeu d'origine : l'enregistrement s'arrête, plus rien n'est restauré et tous les réglages des pages Assets, Fonctions et Accessoires sont grisés d'un coup, chaque outil suivant alors ses propres règles d'origine. Il se coupe de lui-même quand les trois interrupteurs par page ci-dessous sont tous coupés, et se réactive dès que l'un des trois est activé.";
		d["scope.line.sharedSingle"] = "Global : tous les outils de cette page qui prennent en charge cet élément partagent une seule valeur";
		d["scope.note"] = "Remarque : les pages Assets, Fonctions et Accessoires gardent chacune leur propre mémoire, même quand une ligne porte le même nom sur plusieurs pages ; « Global » et « Aucun partage » ne valent qu'à l'intérieur de la page où vous vous trouvez.";
		d["item.brush.desc"] = "Mémorise les lignes « Taille du pinceau » et « Force du pinceau » : la taille décide de l'étendue couverte par une passe et la force décide de ce que chaque passe modifie. Cet élément ne couvre que le brossage de la page Assets, c'est-à-dire la pose d'assets tels que les arbres depuis le menu Végétation en mode Pinceau ; les neuf outils de terraformation ont leur propre élément Pinceau sur la page Fonctions et les familles d'accessoires ont le leur sur la page Accessoires, et les trois pages ne partagent jamais rien. Les outils dont le panneau n'affiche pas ces deux lignes ne sont ni enregistrés ni modifiés.";
		d["item.brush.ex.group"] = "par exemple, réglez la taille du pinceau du brossage d'arbres sur 30 m et les autres assets brossables du même groupe sont aussi à 30 m, mais un outil d'un autre groupe ne l'est pas";
		d["item.brush.ex.menu"] = "tous les assets que vous brossez dans le menu Végétation utilisent cette même taille et la même force de pinceau, mais en changeant de menu ce n'est plus le cas";
		d["item.brush.ex.category"] = "passer à une autre sorte d'arbre garde ce même pinceau, mais ce niveau regroupe les assets d'après ce qu'ils desservent, et les outils de terraformation comme les accessoires gardent chacun leur propre copie sur une autre page (réglage séparé nécessaire)";
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
			d["scope.desc"] = "Quanto viene condiviso il valore di questa voce. Le parentesi nel menu a tendina mostrano cosa fa l'originale e cosa consigliamo.";
			d["scope.line.group"] = "Stesso gruppo: {0}";
			d["scope.line.menu"] = "Stesso menu: {0}";
			d["scope.line.category"] = "Stesso tipo di asset: raggruppati in base a ciò a cui l'asset serve davvero, quindi numero di corsie, dimensione, livello e categoria della barra degli strumenti non fanno alcuna differenza (un parcheggio piccolo e uno grande sono lo stesso tipo), ma ponti, banchine e incroci formano ognuno un tipo a sé, separato dalle strade, cioè {0}";
			d["scope.line.shared"] = "Globale: ogni asset e ogni funzione che supporta questa voce condivide il valore, ma asset e funzioni non condividono mai tra loro";
			d["scope.line.unique"] = "Non condiviso: ogni asset e ogni funzione che supporta questa voce viene impostato in modo indipendente";

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
			d["resetall.desc"] = "Riporta ogni opzione di questo mod al suo valore consigliato predefinito: l'interruttore generale si riattiva, e vengono ripristinati anche lo stato di attivazione e l'ambito di condivisione di ogni voce. La memoria già registrata delle partite non viene toccata.";
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
			d["item.anarchy.desc"] = "Ricorda lo stato del pulsante Anarchy aggiunto dal mod Anarchy. Anarchy è condiviso in tutto il gioco, quindi questa voce è globale; una volta attivo puoi cambiare l'ambito di condivisione. Senza il mod Anarchy questa voce non compare.";
			d["item.anarchy.ex.group"] = "per esempio, attivi Anarchy su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attivo, mentre una strada grande no";
			d["item.anarchy.ex.menu"] = "tutti gli asset che clicchi nel menu Strade hanno Anarchy attivo, ma passando al menu Elettricità no";
			d["item.anarchy.ex.category"] = "passando a una strada a sei corsie Anarchy resta attivo, ma un ponte o un binario della metropolitana doppio tornano a spento e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Ricorda quali voci sono spuntate nel filtro «Tema» della barra degli strumenti. Quella riga compare solo quando la categoria corrente contiene davvero asset che usano i temi, e spuntarne uno limita l'elenco della barra agli asset che supportano quei temi. L'originale reimposta il filtro sul tema predefinito solo al caricamento di un salvataggio, quindi ti consigliamo di lasciare questa voce disattivata.";
			d["item.themes.ex.group"] = "per esempio, spunti un tema solo in Strade/Strade piccole, così una strada grande non filtra per quel tema";
			d["item.themes.ex.menu"] = "tutti gli asset che clicchi nel menu Strade seguono il tema spuntato lì, ma passando al menu Elettricità no";
			d["item.themes.ex.category"] = "la spunta di questa riga è lo stato del filtro della barra degli strumenti stesso, quindi viene ricordata per il menu e la categoria in cui ti trovi, e non per tipo di asset";
			d["item.packs.label"] = "Pacchetto";
			d["item.packs.desc"] = "Ricorda quali voci sono spuntate nel filtro «Pacchetto» della barra degli strumenti; spuntarne una limita l'elenco della barra agli asset che appartengono a quei pacchetti. L'originale lo azzera ogni volta che cambi menu o categoria, quindi ricordarlo per menu e categoria corrisponde a ciò che vedi, e anche per questa voce ti consigliamo di lasciarla disattivata.";
			d["item.packs.ex.group"] = "per esempio, spunti un pacchetto in Strade/Strade piccole, l'originale lo azzera sulle strade grandi e ritorna quando ci torni";
			d["item.packs.ex.menu"] = "i pacchetti spuntati nel menu Strade e nel menu Elettricità vengono ricordati separatamente";
			d["item.packs.ex.category"] = "la spunta di questa riga è lo stato del filtro della barra degli strumenti stesso, ricordata per il menu e la categoria in cui ti trovi; l'originale la azzera ogni volta che cambi categoria";

			// ---------- Modalità strumento ----------
			d["item.toolMode.label"] = "Modalità strumento";
			d["item.toolMode.desc"] = "Ricorda la modalità scelta per ogni strumento e le opzioni cambiano a seconda dello strumento: strade, binari e tubi offrono Rettilineo, Curva semplice, Curva complessa, Continua, Griglia, Sostituisci, Punto; edifici, oggetti e alberi offrono Posiziona uno, Posiziona multipli, Linea, Curva, Strumento timbro oggetti; le zone offrono Riempi, Seleziona, Pittura; le aree offrono Modifica, Genera griglia mappa. Le modalità degli asset e quelle delle funzioni sono ricordate separatamente perché le opzioni non sono le stesse.";
			d["item.toolMode.ex.group"] = "per esempio, scegli Curva semplice su una piccola strada a due corsie e anche le altre piccole strade usano Curva semplice, mentre una strada grande no";
			d["item.toolMode.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono su Curva semplice, ma passando al menu Elettricità no";
			d["item.toolMode.ex.category"] = "passando a una strada a sei corsie resta Curva semplice, ma un ponte o un binario della metropolitana doppio ripartono dalla loro modalità predefinita e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Elevazione ----------
			d["item.elevation.label"] = "Elevazione";
			d["item.elevation.desc"] = "Ricorda l'elevazione a cui lasci lo strumento, incluso il risultato di Aumenta elevazione e Diminuisci elevazione. Il Livello elevazione (quanto sposta una singola pressione; Anarchy chiama questa riga Step di elevazione) viene ricordato insieme ad essa e usa lo stesso ambito di condivisione. L'elevazione degli incroci non è inclusa. Alcuni asset hanno un proprio intervallo di elevazione consentito: se il valore condiviso esce da ciò che l'asset selezionato ammette, questo conserva un'elevazione a esso possibile e il valore condiviso memorizzato resta invariato.";
			d["item.elevation.ex.group"] = "per esempio, porti una strada a due corsie a 10 m e anche le altre piccole strade vanno a 10 m, mentre una strada grande non è a 10 m";
			d["item.elevation.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono a 10 m, ma passando al menu Elettricità non sono a 10 m";
			d["item.elevation.ex.category"] = "passando a una strada a sei corsie restano 10 m, ma un ponte o un binario ferroviario doppio ripartono da 0 m e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Modalità parallelismo ----------
			d["item.parallel.label"] = "Modalità parallelismo";
			d["item.parallel.desc"] = "Ricorda le due voci del pannello Strada parallela: l'interruttore Modalità parallelismo (Attiva/disattiva modalità parallelismo) e la distanza della Compensazione parallelismo. Con la modalità parallelismo sono sempre due linee: una volta attiva, accanto alla linea che tracci ne viene costruita una seconda, e ciò che puoi regolare è quanto sono distanziate le due, per esempio 1 m o 2 m. Se la spegni ti resta solo la linea singola.";
			d["item.parallel.ex.group"] = "per esempio, attivi la Modalità parallelismo su una piccola strada a due corsie e imposti la Compensazione parallelismo a 5 m, e anche le altre piccole strade tornano attive con gli stessi 5 m, mentre una strada grande no";
			d["item.parallel.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono in parallelo con 5 m di compensazione, ma passando al menu Elettricità no";
			d["item.parallel.ex.category"] = "passando a una strada a sei corsie resta attivo con gli stessi 5 m di compensazione, ma un ponte o un binario della metropolitana tornano a spento e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Aggancio ----------
			d["item.snap.label"] = "Aggancio";
			d["item.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio: Aggancia alla geometria esistente, Aggancia alla lunghezza della cella di zonizzazione, Aggancia ad angoli di 90°, Aggancia ai lati di una strada, Aggancia alle strade, Aggancia al lato del proprietario, Aggancia ai lati di un edificio, Aggancia al centro di una strada, Aggancia alla costa, Aggancia alla geometria vicina, Aggancia alle linee guida, Aggancia alla griglia della zona, Aggancia ai nodi, Aggancia alla superficie di un oggetto, Aggancia in verticale, Aggancia alla griglia del lotto, Collega elementi sovrapposti a un edificio, Rimuovi solo il tipo corrispondente, Mostra le linee di contorno, Aggancia a distanza. La riga Attiva/disattiva tutti gli agganci del gioco è un interruttore multiplo e non viene ricordata.";
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
			d["item.leftRight.desc"] = "Ricorda quali potenziamenti di rete sono scelti sulle righe Sinistra e Destra del pannello di rete di Anarchy, per esempio Pista ciclabile, Alberi, Erba, Banchina, Muro di contenimento e Barriera insonorizzante. Richiede Anarchy (mod Paradox 74604); senza, questa voce non compare.";
			d["item.leftRight.ex.group"] = "per esempio, scegli Pista ciclabile sul lato destro di una piccola strada a due corsie e anche le altre piccole strade ce l'hanno a destra, mentre una strada grande no";
			d["item.leftRight.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso paio di scelte laterali, ma passando al menu Elettricità no";
			d["item.leftRight.ex.category"] = "passando a una strada a sei corsie la pista ciclabile resta a destra, ma un ponte o un binario ferroviario doppio tornano a niente di scelto e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Generale (Anarchy) ----------
			d["item.general.label"] = "Generale";
			d["item.general.desc"] = "Ricorda gli interruttori della riga Generale del pannello di rete di Anarchy: Terreno, Elevato, Tunnel, Pendio costante, Mediana ampia e Intervallo di elevazione ampliato (che toglie il limite di altezza). Richiede Anarchy (mod Paradox 74604); senza, questa voce non compare.";
			d["item.general.ex.group"] = "per esempio, scegli Tunnel su una piccola strada a due corsie e anche le altre piccole strade vengono forzate in tunnel, mentre una strada grande no";
			d["item.general.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di Generale, ma passando al menu Elettricità no";
			d["item.general.ex.category"] = "passando a una strada a sei corsie resta Tunnel, ma un ponte o un binario della metropolitana doppio tornano a niente di spuntato e necessitano un'impostazione propria, benché tutti siano nel menu Strade";

			// ---------- Modalità sottosuolo ----------
			d["item.underground.label"] = "Modalità sottosuolo";
			d["item.underground.desc"] = "Ricorda l'interruttore Modalità sottosuolo (Attiva/disattiva modalità sottosuolo), che decide se ciò che piazzi va sotto terra o sopra.";
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
			d["item.brush.label"] = "Pennello";

		// ==================== v0.7.0 新增 / 重写（it-IT）====================
		d["tab.master"] = "Interruttore generale";
		d["tab.assets"] = "Asset";
		d["tab.functions"] = "Funzioni";
		d["tab.items"] = "Oggetti";
		d["group.domain"] = "Interruttori per pagina";
		d["domain.asset.label"] = "Ricorda le voci degli strumenti nella pagina Asset";
		d["domain.asset.desc"] = "Copre tutte le voci della pagina Asset: la registrazione e il ripristino al caricamento si attivano e disattivano insieme. Quando è disattivata, l'intera pagina Asset viene oscurata, gli strumenti tornano al comportamento originale e i valori già registrati restano nel file intatti, così riattivandola vengono ripristinati.";
		d["domain.function.label"] = "Ricorda le voci degli strumenti nella pagina Funzioni";
		d["domain.function.desc"] = "Copre tutte le voci della pagina Funzioni, cioè i sei gruppi di funzioni: tipi di zona, strumenti area, strumenti di percorso, strumenti di Terraformazione, spazi e aree, indicatori e oggetti prefabbricati. Quando è disattivata, l'intera pagina Funzioni viene oscurata, gli strumenti tornano al comportamento originale e i valori già registrati restano nel file intatti, così riattivandola vengono ripristinati.";
		d["domain.item.label"] = "Ricorda le voci degli strumenti nella pagina Oggetti";
		d["domain.item.desc"] = "Copre tutte le voci della pagina Oggetti, cioè le otto categorie di oggetti del menu Architettura del paesaggio: Natura, Parco, Oggetti residenziali, Oggetti commerciali, Oggetti industriali, Detriti, Decalcomanie e Piante in vaso. Quando è disattivata, l'intera pagina Oggetti viene oscurata, gli strumenti tornano al comportamento originale e i valori già registrati restano nel file intatti, così riattivandola vengono ripristinati.";
		d["enabled.label"] = "Abilita la mod";
		d["enabled.desc"] = "Attivo per impostazione predefinita. Finché è attivo, ogni impostazione qui sotto ha effetto e il valore di ogni voce viene registrato mentre si lavora, così tornando a un salvataggio i pannelli sono esattamente come li si è lasciati; disattivare una singola voce ne blocca solo il ripristino, mentre i suoi valori continuano a essere registrati. Spegnendo questo interruttore generale tutto torna all'originale: la registrazione si ferma, nulla viene ripristinato e tutte le impostazioni delle pagine Asset, Funzioni e Oggetti vengono oscurate in una volta, così ogni strumento si comporta come farebbe da solo. Si spegne da solo quando tutti e tre gli interruttori per pagina qui sotto sono spenti, e si riaccende non appena uno di essi è acceso.";
		d["scope.line.sharedSingle"] = "Globale: ogni strumento di questa pagina che supporta questa voce usa un unico valore";
		d["scope.note"] = "Nota: le pagine Asset, Funzioni e Oggetti tengono ciascuna la propria memoria anche quando una riga ha lo stesso nome su più pagine; Globale e Non condiviso valgono solo all'interno della pagina che si sta guardando.";
		d["scope.category.function"] = "Stesso tipo";
		d["scope.category.item"] = "Stesso tipo di oggetto";
		d["scope.line.category.function"] = "Stesso tipo: raggruppati in base a uno dei sei gruppi di funzioni a cui appartiene lo strumento - tipi di zona (residenziali, commerciali, industriali, uffici), strumenti area (Strumento creazione quartiere, Strumento area di specializzazione industriale, Strumento aree con superficie), strumenti di percorso (tutti i percorsi del menu Trasporti), i nove strumenti di Terraformazione (Strumento sposta terreno, Strumento addolcimento terreno, Strumento livellamento terreno, Strumento inclinazione terreno, Strumento pennello texture 01-04, Gomma texture), spazi e aree (Area di parcheggio, Area pedonale, Area di ritrovo, Area libera, Superficie sagomata) e indicatori e oggetti prefabbricati (lo strumento di generazione animali e simili): {0}";
		d["scope.line.category.item"] = "Stesso tipo di oggetto: raggruppati in base a una delle otto categorie di oggetti del menu Architettura del paesaggio in cui si trova l'oggetto (Natura, Parco, Oggetti residenziali, Oggetti commerciali, Oggetti industriali, Detriti, Decalcomanie, Piante in vaso); un modello diverso o una dimensione diversa all'interno di una categoria non sono un tipo diverso: {0}";
		d["item.color.label"] = "Colore";
		d["item.decoration.label"] = "Mantieni età";
		d["item.decoration.desc"] = "Ricorda l'interruttore Mantieni età: quando è attivo, gli alberi pennellati o posizionati smettono di crescere e di morire di vecchiaia, il che congela l'età di quel gruppo di alberi al valore attuale. La riga compare solo sugli asset che contengono dati sugli alberi (quando si pennellano alberi dal menu Vegetazione); gli asset il cui pannello non ha questa riga non vengono né registrati né modificati.";
		d["item.decoration.ex.group"] = "per esempio, attivando Mantieni età mentre si pennellano alberi, anche gli altri modi di pennellare la vegetazione dello stesso gruppo risultano attivi, mentre un altro gruppo no";
		d["item.decoration.ex.menu"] = "tutti gli asset del menu Vegetazione lo mantengono attivo, ma passando a un altro menu no";
		d["item.decoration.ex.category"] = "passando a un altro tipo di albero resta attivo, ma la riga esiste solo sugli asset con dati sugli alberi, quindi pennellando una decalcomania non c'è affatto (serve un'impostazione propria)";
		d["item.f.brush.desc"] = "Ricorda le righe Dimensione pennello e Robustezza pennello nei pannelli delle funzioni: la dimensione stabilisce quanta area copre una passata, la robustezza quanto cambia ogni passata. Riguarda solo le funzioni il cui pannello ha davvero queste due righe, cioè i nove strumenti di Terraformazione (Strumento sposta terreno, Strumento addolcimento terreno, Strumento livellamento terreno, Strumento inclinazione terreno, Strumento pennello texture 01-04 e Gomma texture) e la pittura delle zone; le funzioni senza queste righe non vengono né registrate né modificate.";
		d["item.f.brush.ex.group"] = "per esempio, impostando una dimensione pennello di 30 m su uno strumento di Terraformazione, anche gli altri strumenti di Terraformazione dello stesso gruppo sono a 30 m, mentre un altro gruppo no";
		d["item.f.brush.ex.menu"] = "tutti gli strumenti del gruppo Terraformazione usano questa stessa dimensione e robustezza del pennello, mentre un altro gruppo di funzioni no";
		d["item.f.brush.ex.category"] = "passando a un altro gruppo di funzioni si usa il pennello di quel gruppo (serve un'impostazione propria), anche quando entrambi i pannelli hanno queste due righe";
		d["item.f.toolMode.desc"] = "Ricorda la riga Modalità strumento nei pannelli delle funzioni. Le opzioni cambiano da strumento a strumento: i tipi di zona offrono Riempi, Seleziona e Pittura; gli strumenti area (Strumento creazione quartiere, Strumento area di specializzazione industriale, Strumento aree con superficie) offrono Modifica e Genera griglia mappa; ogni strumento di Terraformazione ha le proprie modalità; lo strumento Demolizione offre Elemento principale, Elemento secondario e Tutto. La modalità strumento della pagina Asset e quella della pagina Oggetti sono ricordate separatamente e nessuna influisce sulle altre.";
		d["item.f.toolMode.ex.group"] = "per esempio, scegliendo Pittura per la zona residenziale, anche le zone commerciali, industriali e uffici sono su Pittura, mentre gli strumenti area no";
		d["item.f.toolMode.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni usano questa stessa modalità, mentre un altro gruppo di funzioni no";
		d["item.f.toolMode.ex.category"] = "passando a un altro gruppo di funzioni si torna alla modalità di quel gruppo (serve un'impostazione propria), anche quando entrambi sono raggiungibili dallo stesso menu della barra degli strumenti";
		d["item.f.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio nei pannelli delle funzioni: Aggancia ad angoli di 90°, Aggancia alla griglia della zona, Aggancia alle strade, Aggancia al lato del proprietario, Mostra le linee di contorno e così via; quali interruttori compaiono dipende dallo strumento. La riga Attiva/disattiva tutti gli agganci del gioco è un pulsante collettivo e non viene ricordata. La riga Aggancio della pagina Asset e quella della pagina Oggetti sono ricordate separatamente e nessuna influisce sulle altre.";
		d["item.f.snap.ex.group"] = "per esempio, disattivando Aggancia alla griglia della zona per la zona residenziale, anche le zone commerciali, industriali e uffici ce l'hanno disattivato, mentre gli strumenti area no";
		d["item.f.snap.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni usano questo stesso insieme di interruttori, mentre un altro gruppo di funzioni no";
		d["item.f.snap.ex.category"] = "passando a un altro gruppo di funzioni si torna ai valori predefiniti di quel gruppo (serve un'impostazione propria), anche quando entrambi sono raggiungibili dallo stesso menu della barra degli strumenti";
		d["item.f.topography.desc"] = "Ricorda l'attivazione della riga Topografia nei pannelli delle funzioni, cioè se le linee di contorno del terreno sono visibili mentre si disegnano le zone. Questa riga viene letta e scritta per pagina: la pagina Funzioni conserva la copia delle zone, la pagina Asset ha la propria, e una non sovrascrive mai l'altra.";
		d["item.f.topography.ex.group"] = "per esempio, spuntando le linee di contorno per la zona residenziale, anche le zone commerciali, industriali e uffici risultano spuntate";
		d["item.f.topography.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni le mantengono spuntate, mentre un altro gruppo di funzioni no";
		d["item.f.topography.ex.category"] = "passando a un altro gruppo di funzioni si usa la copia di quel gruppo (serve un'impostazione propria); la riga Topografia della pagina Asset è una copia separata e non ne è influenzata";
		d["item.f.underground.desc"] = "Ricorda l'attivazione della riga Modalità sottosuolo nei pannelli delle funzioni. Questa riga viene letta e scritta per pagina: la pagina Funzioni conserva la copia delle zone, le pagine Asset e Oggetti hanno ciascuna la propria e nessuna viene condivisa.";
		d["item.f.underground.ex.group"] = "per esempio, attivando la modalità sottosuolo per la zona residenziale, anche le zone commerciali, industriali e uffici vanno sottoterra";
		d["item.f.underground.ex.menu"] = "tutti gli strumenti di uno stesso gruppo di funzioni restano sottoterra, mentre un altro gruppo di funzioni no";
		d["item.f.underground.ex.category"] = "passando a un altro gruppo di funzioni si usa la copia di quel gruppo (serve un'impostazione propria); la riga Modalità sottosuolo della pagina Asset è una copia separata e non ne è influenzata";
		d["item.i.brush.desc"] = "Ricorda le righe Dimensione pennello e Robustezza pennello nei pannelli degli oggetti, cioè quanto è ampia una passata del pennello e quanti oggetti deposita ogni passata. Riguarda solo gli oggetti il cui pannello ha davvero queste righe; gli oggetti che non le hanno non vengono né registrati né modificati.";
		d["item.i.brush.ex.group"] = "per esempio, impostando una dimensione pennello di 20 m, anche gli altri oggetti pennellabili dello stesso gruppo sono a 20 m, mentre un altro gruppo no";
		d["item.i.brush.ex.menu"] = "tutti gli oggetti pennellabili del menu Architettura del paesaggio usano questo stesso pennello, ma passando a un altro menu no";
		d["item.i.brush.ex.category"] = "passando a un'altra categoria di oggetti si usa il pennello di quella categoria (serve un'impostazione propria), anche quando entrambe sono nel menu Architettura del paesaggio";
		d["item.i.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio nei pannelli degli oggetti: Aggancia alla superficie di un oggetto, Aggancia alla geometria vicina, Aggancia alle strade, Rimuovi solo il tipo corrispondente e così via, a seconda dell'oggetto. La riga Aggancio della pagina Asset e quella della pagina Funzioni sono ricordate separatamente e nessuna influisce sulle altre.";
		d["item.i.snap.ex.group"] = "per esempio, disattivando Aggancia alla superficie di un oggetto per un oggetto della categoria Parco, anche gli altri oggetti dello stesso gruppo ce l'hanno disattivato, mentre un altro gruppo no";
		d["item.i.snap.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio usano questo stesso insieme di interruttori, ma passando a un altro menu no";
		d["item.i.snap.ex.category"] = "passando a un'altra categoria di oggetti si torna ai valori predefiniti di quella categoria (serve un'impostazione propria), anche quando entrambe sono nel menu Architettura del paesaggio";
		d["item.i.toolMode.desc"] = "Ricorda la riga Modalità strumento nei pannelli degli oggetti: Posiziona uno, Posiziona multipli, Linea, Curva, Strumento timbro oggetti, Pittura e così via, a seconda dello strumento. La modalità strumento della pagina Asset e quella della pagina Funzioni sono ricordate separatamente e nessuna influisce sulle altre.";
		d["item.i.toolMode.ex.group"] = "per esempio, scegliendo Posiziona multipli per un oggetto della categoria Natura, anche gli altri oggetti dello stesso gruppo usano Posiziona multipli, mentre un altro gruppo no";
		d["item.i.toolMode.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio sono su questa modalità, ma passando a un altro menu no";
		d["item.i.toolMode.ex.category"] = "passando a un'altra categoria di oggetti, per esempio da Natura a Decalcomanie, si torna alla modalità di quella categoria (serve un'impostazione propria), anche quando entrambe sono nel menu Architettura del paesaggio";
		d["item.i.topography.desc"] = "Ricorda l'attivazione della riga Topografia nei pannelli degli oggetti. Questa riga viene letta e scritta per pagina: la pagina Oggetti conserva la copia degli oggetti, le pagine Asset e Funzioni hanno ciascuna la propria e nessuna viene condivisa.";
		d["item.i.topography.ex.group"] = "per esempio, spuntando le linee di contorno mentre si posizionano oggetti, anche gli altri oggetti dello stesso gruppo risultano spuntati";
		d["item.i.topography.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio le mantengono spuntate, ma passando a un altro menu no";
		d["item.i.topography.ex.category"] = "passando a un'altra categoria di oggetti si usa la copia di quella categoria (serve un'impostazione propria); le righe Topografia delle pagine Asset e Funzioni sono copie separate e non ne sono influenzate";
		d["item.i.underground.desc"] = "Ricorda l'attivazione della riga Modalità sottosuolo nei pannelli degli oggetti, cioè se gli oggetti vengono posizionati sotto o sopra il terreno. Questa riga viene letta e scritta per pagina: la pagina Oggetti conserva la copia degli oggetti, le pagine Asset e Funzioni hanno ciascuna la propria e nessuna viene condivisa.";
		d["item.i.underground.ex.group"] = "per esempio, attivando la modalità sottosuolo per un oggetto, anche gli altri oggetti dello stesso gruppo vanno sottoterra";
		d["item.i.underground.ex.menu"] = "tutti gli oggetti del menu Architettura del paesaggio vanno sottoterra, ma passando a un altro menu no";
		d["item.i.underground.ex.category"] = "passando a un'altra categoria di oggetti si usa la copia di quella categoria (serve un'impostazione propria); le righe Modalità sottosuolo delle pagine Asset e Funzioni sono copie separate e non ne sono influenzate";
		d["item.brush.desc"] = "Ricorda le righe Dimensione pennello e Robustezza pennello: la dimensione stabilisce quanta area copre una passata, la robustezza quanto cambia ogni passata. Questa voce copre solo la pittura nella pagina Asset, cioè il posizionamento a pennello di asset come gli alberi dal menu Vegetazione in modalità Pittura; i nove strumenti di Terraformazione hanno la propria voce pennello nella pagina Funzioni e le categorie di oggetti l'hanno nella pagina Oggetti, e le tre pagine non condividono mai. Gli strumenti il cui pannello non ha queste righe non vengono né registrati né modificati.";
		d["item.brush.ex.group"] = "per esempio, impostando una dimensione pennello di 30 m mentre si pennellano alberi, anche gli altri asset pennellabili dello stesso gruppo sono a 30 m, mentre uno strumento di un altro gruppo no";
		d["item.brush.ex.menu"] = "tutti gli asset pennellabili nel menu Vegetazione usano questa stessa dimensione e robustezza del pennello, ma passando a un altro menu no";
		d["item.brush.ex.category"] = "passando a un altro tipo di albero si mantiene questo stesso pennello, ma gli strumenti di Terraformazione e gli oggetti tengono ciascuno la propria copia in un'altra pagina (serve un'impostazione propria)";
			return d;
		}
	}
}
