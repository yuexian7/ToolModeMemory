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
			d["group.compat"] = "Kompatibilität";
			d["group.about"] = "Informationen und Links";

			d["enabled.label"] = "Werkzeugmodus-Gedächtnis aktivieren";
			d["enabled.desc"] = "Standardmäßig aktiviert. Solange dies aktiviert ist, wird jeder Wert fortlaufend aufgezeichnet; beim nächsten Laden eines Spielstands sind die Panels wieder so, wie du sie verlassen hast. Ein einzelner Punkt deaktiviert nur seine Wiederherstellung - die Werte werden weiterhin aufgezeichnet. Nur wenn du diesen Hauptschalter ausschaltest, endet jede Aufzeichnung und jedes Werkzeug verhält sich wieder wie im Original.";
			d["compat.label"] = "Kompatibel mit anderen Mods";
			d["compat.desc"] = "Standardmäßig aktiviert. Aktiviert: Das Gedächtnis nutzt die von anderen Mods (Asset UI Manager, ExtraLib und ähnlich) angepassten Menü- und Gruppennamen, ein verschobenes Asset folgt also seinem neuen Platz. Deaktiviert: Die zuerst gesehene Einordnung bleibt erhalten, was stabiler ist. Beachte: Ein benutzerdefiniertes Asset, das sein Autor nicht der richtigen Asset-Art zugeordnet hat, lässt sich nicht zusammen mit dieser Art anpassen. In beiden Fällen gehen bereits aufgezeichnete Werte nicht verloren.";
			d["scope.label"] = "Gemeinsamer Bereich";
			d["scope.desc"] = "Wie weit der Wert dieses Punkts geteilt wird. Die Klammern in der Dropdown-Liste zeigen, was das Original tut und was wir empfehlen.";
			d["scope.line.group"] = "Selbe Gruppe: {0}";
			d["scope.line.menu"] = "Selbes Menü: {0}";
			d["scope.line.category"] = "Selber Asset-Typ: nur Assets oder Funktionen derselben Unterkategorie teilen sich die Einstellung, also {0}";
			d["scope.line.shared"] = "Global geteilt: jedes Asset und jede Funktion, die diesen Punkt unterstützt, teilt sich den Wert - aber Assets und Funktionen teilen nicht untereinander";
			d["scope.line.sharedSingle"] = "Global geteilt: jedes Asset und jede Funktion, die diesen Punkt unterstützt, teilt sich denselben Wert - auch zwischen Assets und Funktionen";
			d["scope.line.unique"] = "Gar nicht geteilt: jedes Asset und jede Funktion, die diesen Punkt unterstützt, wird eigenständig eingestellt";
			d["scope.note"] = "Hinweis: Funktionen meint hier Zonen, Räume und Flächen, Terraforming, Markierungen und Objekt-Fertigteile, also die Werkzeuge, die keine Assets sind";

			d[kScopeGroup] = "Selbe Gruppe";
			d[kScopeMenu] = "Selbes Menü";
			d[kScopeCategory] = "Selber Asset-Typ";
			d[kScopeGlobalShared] = "Global geteilt";
			d[kScopeGlobalUnique] = "Gar nicht geteilt";
			d[kTagVanilla] = " (Original)";
			d[kTagRecommended] = " (empfohlen)";
			d[kTagRecVanilla] = " (empfohlen, Original)";

			d["reset.label"] = "Speicher zurücksetzen";
			d["reset.desc"] = "Im Spiel: das Gedächtnis dieses Spielstands löschen und die Werkzeuge in den Originalzustand beim Laden versetzen. Im Hauptmenü: das Gedächtnis aller Spielstände löschen.";
			d["reset.warn"] = "Das kann nicht rückgängig gemacht werden.";
			d["reset.confirm"] = "Alle gemerkten Werkzeugeinstellungen zurücksetzen?";
			d["resetall.label"] = "Alle Einstellungen zurücksetzen";
			d["resetall.desc"] = "Setzt jede Option dieses Mods auf den empfohlenen Standardwert zurück: Hauptschalter und Kompatibilitätsschalter werden wieder aktiviert, auch Aktivierung und gemeinsamer Bereich jedes Punkts werden wiederhergestellt. Das bereits gespeicherte Gedächtnis der Spielstände bleibt unberührt.";
			d["resetall.warn"] = "Das kann nicht rückgängig gemacht werden.";
			d["resetall.confirm"] = "Wirklich alle Einstellungen zurücksetzen?";
			d["folder.label"] = "Gedächtnisdateien aller Spielstände verwalten";
			d["folder.desc"] = "Öffnet den lokalen Ordner mit den Gedächtnisdateien, eine pro Spielstand, benannt wie der Spielstand. Das Gedächtnis wird immer nach dem Namen des Spielstands gelesen: eine Datei wird nur benutzt, wenn der Name des Spielstands und der Dateiname der json-Datei gleich sind, also benennst du die Datei um, um das Gedächtnis von einem Spielstand auf einen anderen zu kopieren. Beim Laden einer automatischen Speicherung wird ihr Gedächtnis unter dem Stadtnamen geführt, weil eine automatische Speicherung jedes Mal einen neuen Namen bekommt.";

			d["about.version"] = "Mod-Version";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Unterstütze den Autor auf Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Den Thread im Paradox-Forum öffnen.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Die Website der Rainbow-Reihe öffnen.";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "Merkt sich den Status des Schalters Anarchy, den der Anarchy-Mod hinzufügt. Anarchy wird im ganzen Spiel geteilt, dieser Punkt ist also global; ist er an, kannst du den gemeinsamen Bereich ändern. Ohne den Anarchy-Mod tut dieser Punkt nichts.";
			d["item.anarchy.ex.group"] = "zum Beispiel schaltest du Anarchy für eine zweispurige kleine Straße ein, andere kleine Straßen haben Anarchy ebenfalls an, eine große Straße dagegen nicht";
			d["item.anarchy.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, hat Anarchy an, im Menü Strom dagegen nicht";
			d["item.anarchy.ex.category"] = "wechselt man zu einer großen Straße, bleibt Anarchy an, aber eine Brücke geht wieder aus und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			d["item.themes.label"] = "Thema";
			d["item.themes.desc"] = "Merkt, welche Einträge im Filter „Thema“ der Werkzeugleiste angehakt sind. Diese Zeile erscheint nur, wenn die aktuelle Kategorie wirklich Assets enthält, die Themen verwenden, und ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die diese Themen unterstützen. Das Original setzt den Filter erst beim Laden eines Spielstands auf das Standardthema zurück, deshalb ist dieser Punkt ab Werk deaktiviert.";
			d["item.themes.ex.group"] = "zum Beispiel hakt du ein Thema nur unter Straßen/Kleine Straßen ein, eine große Straße filtert dann nicht danach";
			d["item.themes.ex.menu"] = "ein im Menü Straßen angehaktes Thema gilt im Menü Strom nicht und ist wieder da, wenn du zurückkehrst";
			d["item.themes.ex.category"] = "ebenso zählen Gassen und U-Bahn-Gleise als derselbe Asset-Typ, das angehakte Thema wird daher über die Menüs hinweg geteilt";
			d["item.packs.label"] = "Paket";
			d["item.packs.desc"] = "Merkt, welche Einträge im Filter „Paket“ der Werkzeugleiste angehakt sind; ein Haken beschränkt die Liste der Werkzeugleiste auf Assets, die zu diesen Paketen gehören. Das Original leert die Auswahl jedes Mal, wenn du das Menü oder die Kategorie wechselst, das getrennte Merken pro Menü und Kategorie passt also zu dem, was du siehst - auch dieser Punkt ist ab Werk deaktiviert.";
			d["item.packs.ex.group"] = "zum Beispiel hakt du ein Paket unter Straßen/Kleine Straßen ein, das Original leert es bei einer großen Straße, und es ist wieder da, wenn du zurückkehrst";
			d["item.packs.ex.menu"] = "die im Menü Straßen angehakten Pakete und die im Menü Strom angehakten werden getrennt gemerkt";
			d["item.packs.ex.category"] = "ebenso stehen Gassen sowohl im Menü Straßen als auch im Menü Bezirke unter demselben Namen, die angehakten Pakete sind daher eine gemeinsame Auswahl";

			// ---------- 2 Werkzeugmodus ----------
			d["item.toolMode.label"] = "Werkzeugmodus";
			d["item.toolMode.desc"] = "Merkt den Modus jedes Werkzeugs, und die Optionen unterscheiden sich: Straßen, Gleise und Leitungen bieten Gerade, Einfache Kurve, Komplexe Kurve, Durchgehend, Raster, Ersetzen, Punkt; Gebäude, Objekte und Bäume bieten Einzeln platzieren, Mehrfach platzieren, Linie, Kurve, Objektstempel-Werkzeug; Zonen bieten Füllen, Auswahlrechteck, Farbe; Flächen bieten Bearbeiten, Kartenraster erstellen. Modi von Assets und Modi von Funktionen werden getrennt gemerkt, weil ihre Optionen nicht dieselben sind.";
			d["item.toolMode.ex.group"] = "zum Beispiel wählst du Einfache Kurve für eine zweispurige kleine Straße, andere kleine Straßen nutzen ebenfalls Einfache Kurve, eine große Straße dagegen nicht";
			d["item.toolMode.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, steht auf Einfache Kurve, im Menü Strom dagegen nicht";
			d["item.toolMode.ex.category"] = "wechselt man zu einer großen Straße, bleibt es Einfache Kurve, aber eine Brücke fällt auf ihren Standardmodus zurück und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 3 Höhe ----------
			d["item.elevation.label"] = "Höhe";
			d["item.elevation.desc"] = "Merkt die Höhe, mit der ein Werkzeug verlassen wurde, einschließlich des Ergebnisses von Erhöhen und Absenken. Der Höhenunterschied (wie weit dich ein einzelner Druck versetzt; Anarchy nennt diese Zeile Höhenschritt) wird zusammen mit der Höhe gemerkt und nutzt denselben gemeinsamen Bereich. Die Höhe von Kreuzungen ist nicht enthalten.";
			d["item.elevation.ex.group"] = "zum Beispiel hebst du eine zweispurige kleine Straße auf 10 m, andere kleine Straßen gehen ebenfalls auf 10 m, eine große Straße aber nicht auf 10 m";
			d["item.elevation.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, liegt auf 10 m, im Menü Strom dagegen nicht auf 10 m";
			d["item.elevation.ex.category"] = "wechselt man zu einer großen Straße, bleibt es bei 10 m, aber eine Brücke geht zurück auf 0 m und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 4 Parallelmodus ----------
			d["item.parallel.label"] = "Parallelmodus";
			d["item.parallel.desc"] = "Merkt den Schalter Parallelmodus (Parallelmodus an-/abschalten) zusammen mit der Anzahl der Parallelstraße und dem Parallelabstand, den Abstand vergrößern und Abstand verringern ändern. Der Parallelmodus baut parallele Verkehrswege.";
			d["item.parallel.ex.group"] = "zum Beispiel schaltest du den Parallelmodus für eine zweispurige kleine Straße ein und stellst die Anzahl auf 3, andere kleine Straßen bauen ebenfalls 3, eine große Straße dagegen nicht";
			d["item.parallel.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, baut 3 parallele Linien, im Menü Strom dagegen nicht";
			d["item.parallel.ex.category"] = "wechselt man zu einer großen Straße, bleiben es 3 Linien, aber eine Brücke geht aus und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 5 Einrasten ----------
			d["item.snap.label"] = "Einrasten";
			d["item.snap.desc"] = "Merkt jeden Schalter der Zeile Einrasten: An existierender Geometrie einrasten, An Rasterzellenlänge einrasten, Bei 90-Grad-Winkeln einrasten, An Straßenkanten einrasten, An Straßen einrasten, An der Seite des Besitzers einrasten, An den Gebäudekanten einrasten, In der Mitte einer Straße einrasten, An der Küste einrasten, An nahegelegener Geometrie einrasten, An Hilfslinien einrasten, An Zellenraster einrasten, An Knoten einrasten, An der Oberfläche eines Objekts einrasten, Aufrecht einrasten, An Flächenraster einrasten, Bindet überlappende Objekte an ein Gebäude, Nur entsprechenden Typ entfernen, Nur Höhenlinien anzeigen, An Distanz einrasten. Die Zeile Einrasten für alle ein-/ausschalten ist ein Sammlerschalter und wird selbst nicht gemerkt.";
			d["item.snap.ex.group"] = "zum Beispiel schaltest du „An Straßenkanten einrasten“ für eine zweispurige kleine Straße aus, andere kleine Straßen haben es ebenfalls aus, eine große Straße hat dagegen alles an";
			d["item.snap.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Einrast-Schaltern, im Menü Strom dagegen nicht";
			d["item.snap.ex.category"] = "wechselt man zu einer großen Straße, bleibt dieselbe Reihe erhalten, aber eine Brücke geht auf die Standardwerte zurück und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 6 Topographie ----------
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Merkt die Zeile Topographie, also ob Nur Höhenlinien anzeigen aktiv ist.";
			d["item.topography.ex.group"] = "zum Beispiel schaltest du die Topographie für eine zweispurige kleine Straße ein, andere kleine Straßen haben sie ebenfalls an, eine große Straße dagegen nicht";
			d["item.topography.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, zeigt Höhenlinien, im Menü Strom dagegen nicht";
			d["item.topography.ex.category"] = "wechselt man zu einer großen Straße, bleiben die Höhenlinien sichtbar, aber bei einer Brücke ist die Zeile aus und sie braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";


			// ---------- 8 Links und Rechts (Anarchy) ----------
			d["item.leftRight.label"] = "Links und Rechts";
			d["item.leftRight.desc"] = "Merkt sich, welche Netzwerkerweiterungen auf den Zeilen „Links“ und „Rechts“ im Netzwerkpanel von Anarchy gewählt sind, zum Beispiel Fahrradweg, Bäume, Parken, Kai, Stützwand und Schallschutzmauer. Benötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod tut dieser Punkt nichts.";
			d["item.leftRight.ex.group"] = "zum Beispiel wählst du auf der rechten Seite einer zweispurigen kleinen Straße den Fahrradweg, andere kleine Straßen haben rechts ebenfalls den Fahrradweg, eine große Straße dagegen nicht";
			d["item.leftRight.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dasselbe Paar Seitenauswahlen, im Menü Strom dagegen nicht";
			d["item.leftRight.ex.category"] = "wechselt man zu einer großen Straße, bleibt rechts der Fahrradweg, aber bei einer Brücke ist wieder nichts ausgewählt und sie braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 9 Allgemein (Anarchy) ----------
			d["item.general.label"] = "Allgemein";
			d["item.general.desc"] = "Merkt sich die Schalter auf der Zeile „Allgemein“ im Netzwerkpanel von Anarchy: Boden, Erhöht, Tunnel, Konstante Neigung, Breiter Trennstreifen und Erweiterter Höhenbereich (setzt die Höhenbegrenzung außer Kraft). Benötigt Anarchy (Paradox-Mod 74604); ohne diesen Mod tut dieser Punkt nichts.";
			d["item.general.ex.group"] = "zum Beispiel wählst du Tunnel für eine zweispurige kleine Straße, andere kleine Straßen werden ebenfalls in Tunnel gezwungen, eine große Straße dagegen nicht";
			d["item.general.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Reihe von Allgemein-Schaltern, im Menü Strom dagegen nicht";
			d["item.general.ex.category"] = "wechselt man zu einer großen Straße, bleibt es Tunnel, aber bei einer Brücke ist wieder nichts angehakt und sie braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 10 Untergrundmodus ----------
			d["item.underground.label"] = "Untergrundmodus";
			d["item.underground.desc"] = "Merkt den Schalter Untergrundmodus (Untergrundmodus an-/abschalten): ob du etwas unter der Erde oder darüber platzierst.";
			d["item.underground.ex.group"] = "zum Beispiel schaltest du den Untergrundmodus für eine zweispurige kleine Straße ein, andere kleine Straßen gehen ebenfalls unter die Erde, eine große Straße dagegen nicht";
			d["item.underground.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, wird unter der Erde gebaut, im Menü Strom dagegen nicht";
			d["item.underground.ex.category"] = "wechselt man zu einer großen Straße, bleibt es unter der Erde, aber eine Brücke bleibt an der Oberfläche und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";

			// ---------- 11 Sonstiges ----------
			d["item.other.label"] = "Sonstiges";
			d["item.other.desc"] = "Merkt die übrigen Zeilen, die ein Werkzeugpanel aufzeichnen kann: die Straßenfarben der Zeile Farbe sowie Pinselgröße und Pinselstärke.";
			d["item.other.ex.group"] = "zum Beispiel färbst du eine zweispurige kleine Straße blau, andere kleine Straßen werden ebenfalls blau, eine große Straße dagegen nicht";
			d["item.other.ex.menu"] = "jedes Asset, das du im Menü Straßen anklickst, nutzt dieselbe Farb- und Pinseleinstellung, im Menü Strom dagegen nicht";
			d["item.other.ex.category"] = "wechselt man zu einer großen Straße, bleibt es blau, aber eine Brücke geht auf die Standardfarbe zurück und braucht eine eigene Einstellung, obwohl beide im Menü Straßen liegen";
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
			d["group.compat"] = "Compatibilidad";
			d["group.about"] = "Información y enlaces";

			d["enabled.label"] = "Activar Memoria de modo de herramienta";
			d["enabled.desc"] = "Activado por defecto. Mientras esté activado, todos los valores se registran sin parar, así que al volver a una partida los paneles quedan como los dejaste. Desactivar un solo elemento solo impide restaurarlo a él; sus valores siguen registrándose. Solo al desactivar este interruptor general se deja de registrar y cada herramienta vuelve a comportarse como en el juego original.";
			d["compat.label"] = "Compatible con otros mods";
			d["compat.desc"] = "Activado por defecto. Activado: la memoria se clasifica según los nombres de menú y grupo tal como los ajustan otros mods (Asset UI Manager, ExtraLib y similares), de modo que un activo que moviste sigue su nueva ubicación. Desactivado: se mantiene la clasificación que este mod vio primero, que es más estable. Ten en cuenta que un activo personalizado cuyo autor no clasificó en el tipo de activo correcto no se puede ajustar junto con ese tipo. Ninguna de las dos opciones borra los valores ya registrados.";
			d["scope.label"] = "Ámbito compartido";
			d["scope.desc"] = "Hasta qué punto se comparte el valor de este elemento. Los paréntesis de la lista desplegable indican qué hace el juego original y qué recomendamos.";
			d["scope.line.group"] = "Mismo grupo: {0}";
			d["scope.line.menu"] = "Mismo menú: {0}";
			d["scope.line.category"] = "Mismo tipo de activo: solo lo comparten los activos o funciones de la misma subcategoría, es decir {0}";
			d["scope.line.shared"] = "Global: cada activo y cada función que admitan este elemento comparten el valor, pero los activos y las funciones no comparten entre ellos";
			d["scope.line.sharedSingle"] = "Global: cada activo y cada función que admitan este elemento comparten el valor, y los activos y las funciones también comparten entre ellos";
			d["scope.line.unique"] = "Sin compartir: cada activo y cada función que admita este elemento se ajusta por separado";
			d["scope.note"] = "Nota: funciones significa Zonas, Espacios y áreas, Terraformación, Marcadores y prefabricados de objeto, es decir, las herramientas que no son activos";

			d[kScopeGroup] = "Mismo grupo";
			d[kScopeMenu] = "Mismo menú";
			d[kScopeCategory] = "Mismo tipo de activo";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Sin compartir";
			d[kTagVanilla] = " (original)";
			d[kTagRecommended] = " (recomendado)";
			d[kTagRecVanilla] = " (recomendado, original)";

			d["reset.label"] = "Restablecer memoria";
			d["reset.desc"] = "En partida: borra la memoria de esta partida y devuelve las herramientas al estado original de al cargar. En el menú principal: borra la memoria de todas las partidas.";
			d["reset.warn"] = "No se puede deshacer.";
			d["reset.confirm"] = "¿Restablecer los ajustes de herramienta recordados?";
			d["resetall.label"] = "Restablecer todos los ajustes";
			d["resetall.desc"] = "Devuelve cada opción de este mod a su valor recomendado: el interruptor general y el de compatibilidad vuelven a activarse, y también se restauran el estado de activación y el ámbito compartido de cada elemento. La memoria ya registrada de las partidas no se toca.";
			d["resetall.warn"] = "No se puede deshacer.";
			d["resetall.confirm"] = "¿Restablecer todos los ajustes?";
			d["folder.label"] = "Gestionar los archivos de memoria de todas las partidas";
			d["folder.desc"] = "Abre la carpeta local con los archivos de memoria, uno por partida y con el nombre de la partida. La memoria se lee siempre por nombre de partida: un archivo solo se usa cuando el nombre de la partida y el nombre del archivo json coinciden, así que renombrar el archivo es la forma de copiar la memoria de una partida a otra. Al cargar un guardado automático su memoria se guarda con el nombre de la ciudad, porque un guardado automático recibe un nombre nuevo cada vez.";

			d["about.version"] = "Versión del mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Apoya al autor en Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Abrir el hilo en el foro de Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Abrir el sitio de la serie Rainbow.";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "Recuerda el estado del interruptor Anarchy que añade el mod Anarchy. Anarchy se comparte en todo el juego, así que este elemento es global; una vez activado puedes cambiar el ámbito compartido. Sin el mod Anarchy este elemento no hace nada.";
			d["item.anarchy.ex.group"] = "por ejemplo, activas Anarchy en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen activado, mientras que una carretera grande no";
			d["item.anarchy.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras tienen Anarchy activado, pero al pasar al menú de Electricidad ya no";
			d["item.anarchy.ex.category"] = "cambiar a una carretera grande sigue teniéndolo activado, pero un puente vuelve a desactivado y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			d["item.themes.label"] = "Temática";
			d["item.themes.desc"] = "Recuerda qué casillas están marcadas en el filtro «Temática» de la barra de herramientas. Esa fila solo aparece cuando la categoría actual contiene de verdad assets que usan temas, y marcar una limita la lista de la barra a los assets que admiten esos temas. El juego original solo la restablece al tema predeterminado al cargar una partida, así que este elemento viene desactivado de serie.";
			d["item.themes.ex.group"] = "por ejemplo, marcas un tema solo en Carreteras/Carreteras pequeñas, de modo que una carretera grande ya no filtra por él";
			d["item.themes.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras siguen el tema marcado allí, pero al pasar al menú de Electricidad no";
			d["item.themes.ex.category"] = "las callejas y las vías de metro cuentan como el mismo tipo de activo, así que el tema marcado se comparte entre menús";
			d["item.packs.label"] = "Paquete";
			d["item.packs.desc"] = "Recuerda qué casillas están marcadas en el filtro «Paquete» de la barra de herramientas; marcar una limita la lista de la barra a los assets de esos paquetes. El juego original los vacía cada vez que cambias de menú o de categoría, así que recordarlos por menú y categoría coincide con lo que ves, y este elemento también viene desactivado de serie.";
			d["item.packs.ex.group"] = "por ejemplo, marcas un paquete en Carreteras/Carreteras pequeñas, el juego original lo borra en las carreteras grandes y vuelve al regresar";
			d["item.packs.ex.menu"] = "los paquetes marcados en el menú de Carreteras y en el menú de Electricidad se recuerdan por separado";
			d["item.packs.ex.category"] = "las callejas están en los menús de Carreteras y Distritos con el mismo nombre, así que los paquetes marcados forman un único conjunto compartido";

			// ---------- 2 Herramientas ----------
			d["item.toolMode.label"] = "Herramientas";
			d["item.toolMode.desc"] = "Recuerda el modo elegido para cada herramienta, y las opciones cambian según la herramienta: carreteras, vías y tuberías ofrecen Recta, Curva sencilla, Curva compleja, Continua, Cuadrícula, Reemplazar y Punto; edificios, accesorios y árboles ofrecen Colocar uno, Colocar varios, Línea, Curva y Herramienta de sello de objetos; las zonas ofrecen Rellenar, Seleccionar y Pintar; las áreas ofrecen Editar y Generar cuadrícula de mapa. Los modos de activos y los de funciones se guardan por separado porque sus opciones no son las mismas.";
			d["item.toolMode.ex.group"] = "por ejemplo, eliges Curva sencilla en una carretera pequeña de dos carriles y las otras carreteras pequeñas también usan Curva sencilla, mientras que una carretera grande no";
			d["item.toolMode.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están en Curva sencilla, pero al pasar al menú de Electricidad ya no";
			d["item.toolMode.ex.category"] = "cambiar a una carretera grande sigue dando Curva sencilla, pero un puente vuelve a su modo predeterminado y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 3 Elevación ----------
			d["item.elevation.label"] = "Elevación";
			d["item.elevation.desc"] = "Recuerda la elevación con la que dejaste la herramienta, incluido el resultado de Aumentar la elevación y Disminuir la elevación. El Escalón de elevación (lo que avanza con una sola pulsación; Anarchy llama a esa fila Paso de Elevación) se recuerda junto con ella y usa el mismo ámbito compartido. No cubre la elevación de los intercambiadores.";
			d["item.elevation.ex.group"] = "por ejemplo, subes una carretera de dos carriles a 10 m y las otras carreteras pequeñas también quedan a 10 m, mientras que una carretera grande no está a 10 m";
			d["item.elevation.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras están a 10 m, pero al pasar al menú de Electricidad no están a 10 m";
			d["item.elevation.ex.category"] = "cambiar a una carretera grande sigue dando 10 m, pero un puente vuelve a 0 m y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 4 Modo paralelo ----------
			d["item.parallel.label"] = "Modo paralelo";
			d["item.parallel.desc"] = "Recuerda el interruptor Modo paralelo (Alternar modo paralelo) junto con la cantidad de Calle paralela y la Compensación paralela que cambian Aumentar la compensación y Disminuir la compensación. El modo paralelo construye redes paralelas.";
			d["item.parallel.ex.group"] = "por ejemplo, activas el Modo paralelo en una carretera pequeña de dos carriles y pones la cantidad en 3, las otras carreteras pequeñas también construyen 3, mientras que una carretera grande no";
			d["item.parallel.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras construyen 3 líneas paralelas, pero al pasar al menú de Electricidad no";
			d["item.parallel.ex.category"] = "cambiar a una carretera grande sigue construyendo 3, pero un puente vuelve a desactivado y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 5 Ajustar ----------
			d["item.snap.label"] = "Ajustar";
			d["item.snap.desc"] = "Recuerda cada interruptor de la fila Ajustar: Ajustar a una geometría establecida, Ajustar al largo de la celda de zonificación, Ajustar a ángulos de 90 grados, Ajustar a los lados de una carretera, Ajustar a las carreteras, Ajustar al lado del propietario, Ajustar a los lados del edificio, Ajustar a la mitad de la carretera, Ajustar a la orilla, Ajustar a una geometría cercana, Ajustar a las líneas de guía, Ajustar a la cuadrícula de la zona, Ajustar a los nodos, Ajustar a la superficie de un objeto, Ajustar en vertical, Ajustar a la cuadrícula del solar, Une los objetos superpuestos a un edificio, Elimina solo el tipo coincidente, Muestra las líneas de contorno y Ajustar a la distancia. La fila Activar/desactivar todos los ajustes del juego es un interruptor múltiple y no se recuerda.";
			d["item.snap.ex.group"] = "por ejemplo, desactivas «Ajustar a los lados de una carretera» en una carretera pequeña de dos carriles y las otras carreteras pequeñas también lo tienen desactivado, mientras que una carretera grande lo tiene todo activado";
			d["item.snap.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de ajuste, pero al pasar al menú de Electricidad no";
			d["item.snap.ex.category"] = "cambiar a una carretera grande mantiene el mismo conjunto, pero un puente vuelve a los valores predeterminados y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 6 Topografía ----------
			d["item.topography.label"] = "Topografía";
			d["item.topography.desc"] = "Recuerda la fila Topografía, es decir, si Muestra las líneas de contorno está activo.";
			d["item.topography.ex.group"] = "por ejemplo, activas Topografía en una carretera pequeña de dos carriles y las otras carreteras pequeñas también la tienen activada, mientras que una carretera grande no";
			d["item.topography.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras muestran las líneas de contorno, pero al pasar al menú de Electricidad no";
			d["item.topography.ex.category"] = "cambiar a una carretera grande sigue mostrándolas, pero un puente tiene la fila desactivada y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";


			// ---------- 8 Izquierda y derecha (Anarchy) ----------
			d["item.leftRight.label"] = "Izquierda y derecha";
			d["item.leftRight.desc"] = "Recuerda las mejoras de red elegidas en las filas Izquierda y Derecha del panel de red de Anarchy, por ejemplo Vía ciclista, Árboles, Aparcamiento, Muelle, Muro de Retención y Barrera acústica. Necesita Anarchy (mod 74604 de Paradox); sin él este elemento no hace nada.";
			d["item.leftRight.ex.group"] = "por ejemplo, eliges Vía ciclista en el lado derecho de una carretera pequeña de dos carriles y las otras carreteras pequeñas también la reciben a la derecha, mientras que una carretera grande no";
			d["item.leftRight.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo par de selecciones laterales, pero al pasar al menú de Electricidad no";
			d["item.leftRight.ex.category"] = "cambiar a una carretera grande sigue teniendo Vía ciclista a la derecha, pero un puente vuelve a no tener nada elegido y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 9 General (Anarchy) ----------
			d["item.general.label"] = "General";
			d["item.general.desc"] = "Recuerda los interruptores de la fila General del panel de red de Anarchy: Terreno, Elevado, Túnel, Pendiente Constante, Mediana Amplia y Rango de Elevación Expandido (que quita el límite de altura). Necesita Anarchy (mod 74604 de Paradox); sin él este elemento no hace nada.";
			d["item.general.ex.group"] = "por ejemplo, eliges Túnel en una carretera pequeña de dos carriles y las otras carreteras pequeñas también se obligan a ser túneles, mientras que una carretera grande no";
			d["item.general.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo conjunto de interruptores de General, pero al pasar al menú de Electricidad no";
			d["item.general.ex.category"] = "cambiar a una carretera grande sigue dando Túnel, pero un puente vuelve a no tener nada marcado y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 10 Modo subterráneo ----------
			d["item.underground.label"] = "Modo subterráneo";
			d["item.underground.desc"] = "Recuerda el interruptor Modo subterráneo (Alternar modo subterráneo), que decide si lo que colocas va bajo tierra o encima.";
			d["item.underground.ex.group"] = "por ejemplo, activas el Modo subterráneo en una carretera pequeña de dos carriles y las otras carreteras pequeñas también van bajo tierra, mientras que una carretera grande no";
			d["item.underground.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras se construyen bajo tierra, pero al pasar al menú de Electricidad no";
			d["item.underground.ex.category"] = "cambiar a una carretera grande sigue yendo bajo tierra, pero un puente se queda en la superficie y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";

			// ---------- 11 Otros ----------
			d["item.other.label"] = "Otros";
			d["item.other.desc"] = "Recuerda las demás filas que un panel de herramienta puede registrar: los colores de carretera de la fila Color, más Tamaño de pincel y Fuerza de pincel.";
			d["item.other.ex.group"] = "por ejemplo, pones el color de una carretera pequeña de dos carriles en azul y las otras carreteras pequeñas también se vuelven azules, mientras que una carretera grande no";
			d["item.other.ex.menu"] = "todos los activos que pulsas en el menú de Carreteras usan este mismo color y ajuste de pincel, pero al pasar al menú de Electricidad no";
			d["item.other.ex.category"] = "cambiar a una carretera grande sigue siendo azul, pero un puente vuelve al color predeterminado y necesita su propio ajuste, aunque los dos estén en el menú de Carreteras";
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
			d["group.compat"] = "Compatibilité";
			d["group.about"] = "Informations et liens";

			d["enabled.label"] = "Activer la mémoire du mode d'outil";
			d["enabled.desc"] = "Cette option est activée par défaut. Tant qu'elle est active, chaque valeur est enregistrée en continu, et le panneau retrouve l'état où vous l'avez laissé en revenant dans la partie. Désactiver un seul élément arrête seulement sa restauration ; ses valeurs continuent d'être enregistrées. Seul cet interrupteur général, une fois coupé, arrête tout l'enregistrement et chaque outil retrouve son comportement d'origine.";
			d["compat.label"] = "Compatible avec d'autres mods";
			d["compat.desc"] = "Activé par défaut. Activé : la mémoire se classe selon les noms de menu et de groupe tels que d'autres mods (Asset UI Manager, ExtraLib et similaires) les ont ajustés ; un asset déplacé suit donc son nouvel emplacement. Désactivé : la classification vue pour la première fois est conservée, ce qui est plus stable. Notez qu'un asset personnalisé que son auteur n'a pas rangé dans le bon type d'asset ne peut pas être réglé avec ce type. Dans les deux cas, les valeurs déjà enregistrées ne sont pas perdues.";
			d["scope.label"] = "Portée de partage";
			d["scope.desc"] = "Dans quelle mesure la valeur de cet élément est partagée. Les parenthèses de la liste déroulante indiquent le comportement d'origine et notre recommandation.";
			d["scope.line.group"] = "Même groupe : {0}";
			d["scope.line.menu"] = "Même menu : {0}";
			d["scope.line.category"] = "Même type d'actif : seuls les assets ou les fonctions de la même sous-catégorie partagent le réglage, c'est-à-dire {0}";
			d["scope.line.shared"] = "Global : tout asset et toute fonction compatibles avec cet élément partagent la valeur, mais les assets et les fonctions ne partagent jamais entre eux";
			d["scope.line.sharedSingle"] = "Global : tout asset et toute fonction compatibles avec cet élément partagent la valeur, et les assets et les fonctions partagent aussi entre eux";
			d["scope.line.unique"] = "Aucun partage : chaque asset et chaque fonction compatibles avec cet élément se règlent indépendamment";
			d["scope.note"] = "Remarque : par fonctions, on entend Zones, Espaces et aires, Terraformation, Marqueur et objets préfabriqués, c'est-à-dire les outils qui ne sont pas des assets";

			d[kScopeGroup] = "Même groupe";
			d[kScopeMenu] = "Même menu";
			d[kScopeCategory] = "Même type d'actif";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Aucun partage";
			d[kTagVanilla] = " (d'origine)";
			d[kTagRecommended] = " (recommandé)";
			d[kTagRecVanilla] = " (recommandé, d'origine)";

			d["reset.label"] = "Réinitialiser la mémoire";
			d["reset.desc"] = "En partie : efface la mémoire de cette partie et rend les outils à l'état d'origine de la charge. Au menu principal : efface la mémoire de toutes les parties.";
			d["reset.warn"] = "Irréversible.";
			d["reset.confirm"] = "Réinitialiser les réglages d'outil mémorisés ?";
			d["resetall.label"] = "Réinitialiser tous les réglages";
			d["resetall.desc"] = "Remet chaque option de ce mod à son réglage recommandé : l'interrupteur général et celui de compatibilité se réactivent, et l'état comme la portée de partage de chaque élément sont rétablis. La mémoire déjà enregistrée pour les parties n'est pas touchée.";
			d["resetall.warn"] = "Irréversible.";
			d["resetall.confirm"] = "Réinitialiser tous les réglages ?";
			d["folder.label"] = "Gérer les fichiers mémoire de toutes les parties";
			d["folder.desc"] = "Ouvre le dossier local des fichiers mémoire, un par partie, nommé d'après la partie. La mémoire se lit toujours d'après le nom de la partie : un fichier n'est utilisé que si le nom de la partie et celui du fichier json correspondent, donc renommer le fichier est le moyen de copier la mémoire d'une partie à une autre. Quand vous chargez une sauvegarde automatique, sa mémoire est gardée sous le nom de la ville, parce qu'une sauvegarde automatique reçoit un nom nouveau à chaque fois.";

			d["about.version"] = "Version du mod";
			d["about.author"] = "Auteur";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Soutenez l'auteur sur Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Ouvrir le fil sur le forum Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Ouvrir le site de la série Rainbow.";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "Mémorise l'état du bouton Anarchy ajouté par le mod Anarchy. Anarchy est partagé dans tout le jeu, cet élément est donc global ; une fois activé, vous pouvez modifier la portée de partage. Sans le mod Anarchy, cet élément ne fait rien.";
			d["item.anarchy.ex.group"] = "par exemple, activez Anarchy sur une petite route à deux voies, les autres petites routes l'ont activé aussi, mais une grande route non";
			d["item.anarchy.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes ont Anarchy activé, mais dans le menu Électricité ce n'est plus le cas";
			d["item.anarchy.ex.category"] = "en passant à une grande route Anarchy reste activé, mais un pont revient à désactivé et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			d["item.themes.label"] = "Thème";
			d["item.themes.desc"] = "Mémorise les entrées cochées dans le filtre « Thème » de la barre d'outils. Cette ligne n'apparaît que si la catégorie courante contient vraiment des assets utilisant des thèmes, et cocher un thème limite la liste de la barre aux assets qui prennent en charge ces thèmes. Le jeu d'origine ne remet ce filtre sur le thème par défaut qu'au chargement d'une sauvegarde, cet élément est donc désactivé par défaut.";
			d["item.themes.ex.group"] = "par exemple, cochez un thème uniquement sous Routes/Petites routes, une grande route ne filtre donc plus selon lui";
			d["item.themes.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes suivent le thème coché dans ce menu, mais dans le menu Électricité ce n'est plus le cas";
			d["item.themes.ex.category"] = "les allées et les voies de métro comptent comme le même type d'actif, le thème coché est donc partagé d'un menu à l'autre";
			d["item.packs.label"] = "Pack";
			d["item.packs.desc"] = "Mémorise les entrées cochées dans le filtre « Pack » de la barre d'outils ; cocher un pack limite la liste de la barre aux assets appartenant à ces packs. Le jeu d'origine les efface à chaque changement de menu ou de catégorie, les mémoriser par menu et catégorie correspond donc à ce que vous voyez, et cet élément est lui aussi désactivé par défaut.";
			d["item.packs.ex.group"] = "par exemple, cochez un pack sous Routes/Petites routes, le jeu d'origine l'efface sur une grande route et il revient quand vous y retournez";
			d["item.packs.ex.menu"] = "les packs cochés dans le menu Routes et dans le menu Électricité sont mémorisés séparément";
			d["item.packs.ex.category"] = "les allées figurent sous le même nom dans les menus Routes et Quartiers, les packs cochés forment donc un seul jeu partagé";

			// ---------- 2 Mode Outil ----------
			d["item.toolMode.label"] = "Mode Outil";
			d["item.toolMode.desc"] = "Mémorise le mode choisi pour chaque outil, et les options varient : routes, rails et canalisations proposent Droit, Courbe simple, Courbe complexe, Continu, Grille, Remplacer et Point ; bâtiments, accessoires et arbres proposent En placer un, En placer plusieurs, Ligne, Courbe et Outil de marquage d'objet ; les zones proposent Remplissage, Sélection et Peinture ; les périmètres proposent Modifier et Générer une grille de carte. Les modes des assets et ceux des fonctions sont mémorisés séparément, car leurs options diffèrent.";
			d["item.toolMode.ex.group"] = "par exemple, choisissez Courbe simple sur une petite route à deux voies, les autres petites routes utilisent aussi Courbe simple, mais une grande route non";
			d["item.toolMode.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont en Courbe simple, mais dans le menu Électricité ce n'est plus le cas";
			d["item.toolMode.ex.category"] = "passer à une grande route donne toujours Courbe simple, mais un pont retombe sur son mode par défaut et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 3 Élévation ----------
			d["item.elevation.label"] = "Élévation";
			d["item.elevation.desc"] = "Mémorise l'élévation laissée sur l'outil, y compris le résultat de Augmenter l'élévation et Diminuer l'élévation. L'Étape d'élévation (le déplacement d'une seule pression ; l'interface Anarchy nomme cette ligne Incrément d'élévation) est mémorisée avec elle et utilise la même portée de partage. L'élévation des croisements n'est pas concernée.";
			d["item.elevation.ex.group"] = "par exemple, montez une route à deux voies à 10 m, les autres petites routes passent aussi à 10 m, mais une grande route n'est pas à 10 m";
			d["item.elevation.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont à 10 m, mais dans le menu Électricité ils ne sont pas à 10 m";
			d["item.elevation.ex.category"] = "passer à une grande route donne toujours 10 m, mais un pont repart à 0 m et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 4 Mode parallèle ----------
			d["item.parallel.label"] = "Mode parallèle";
			d["item.parallel.desc"] = "Mémorise l'interrupteur Mode parallèle (Changer le mode parallèle) avec le nombre de Route parallèle et la Compensation parallèle qu'Augmenter la compensation et Diminuer la compensation modifient. Permet de construire des réseaux parallèles.";
			d["item.parallel.ex.group"] = "par exemple, activez le Mode parallèle sur une petite route à deux voies et mettez la quantité à 3, les autres petites routes construisent 3 lignes aussi, mais une grande route non";
			d["item.parallel.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes construisent 3 lignes parallèles, mais dans le menu Électricité ce n'est plus le cas";
			d["item.parallel.ex.category"] = "en passant à une grande route on construit toujours 3 lignes, mais un pont se désactive et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 5 Accrocher ----------
			d["item.snap.label"] = "Accrocher";
			d["item.snap.desc"] = "Mémorise chaque interrupteur de la ligne Accrocher : Accrochez à la géométrie existante, Accrochez à la longueur de la cellule de zonage, Accrochez aux angles à 90 degrés, Accrochez aux côtés d'une route, Accrochez aux routes, Accrochez au côté du propriétaire, Accrochez aux côtés d'un bâtiment, Accrochez au milieu d'une route, Accrochez au rivage, Accrochez à la géométrie à proximité, Accrochez aux lignes directrices, Accrochez à la grille de zone, Accrochez aux nœuds, Accrochez à la surface d'un objet, Accrochez verticalement, Accrochez à la grille des emplacements, Lie les éléments qui se chevauchent à un bâtiment, Supprimer uniquement le type correspondant, Afficher les lignes de contour et Accrochez à distance. La ligne Activer/désactiver tous les accrochages du jeu est un interrupteur groupé et n'est pas mémorisée.";
			d["item.snap.ex.group"] = "par exemple, désactivez « Accrochez aux côtés d'une route » sur une petite route à deux voies, les autres petites routes l'ont désactivé aussi, mais une grande route a tout activé";
			d["item.snap.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs d'accrochage, mais dans le menu Électricité ce n'est plus le cas";
			d["item.snap.ex.category"] = "en passant à une grande route on garde le même jeu d'interrupteurs, mais un pont revient aux valeurs par défaut et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 6 Topographie ----------
			d["item.topography.label"] = "Topographie";
			d["item.topography.desc"] = "Mémorise la ligne Topographie, c'est-à-dire si Afficher les lignes de contour est actif.";
			d["item.topography.ex.group"] = "par exemple, activez la Topographie sur une petite route à deux voies, les autres petites routes l'ont activée aussi, mais une grande route non";
			d["item.topography.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes affichent les lignes de contour, mais dans le menu Électricité ce n'est plus le cas";
			d["item.topography.ex.category"] = "en passant à une grande route les lignes de contour restent visibles, mais un pont a la ligne désactivée et réclame son propre réglage, bien que les deux soient dans le menu Routes";


			// ---------- 8 Gauche et droite (Anarchy) ----------
			d["item.leftRight.label"] = "Gauche et droite";
			d["item.leftRight.desc"] = "Mémorise les améliorations de réseau choisies sur les lignes Gauche et Droite du panneau réseau d'Anarchy, par exemple Piste cyclable, Arbres, Stationnement, Quai, Mur de soutènement et Barrière acoustique. Nécessite Anarchy (mod Paradox 74604) ; sans lui cet élément ne fait rien.";
			d["item.leftRight.ex.group"] = "par exemple, choisissez Piste cyclable à droite d'une petite route à deux voies, les autres petites routes l'ont aussi à droite, mais une grande route non";
			d["item.leftRight.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même choix des deux côtés, mais dans le menu Électricité ce n'est plus le cas";
			d["item.leftRight.ex.category"] = "en passant à une grande route la Piste cyclable reste à droite, mais un pont revient à rien de choisi et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 9 Général (Anarchy) ----------
			d["item.general.label"] = "Général";
			d["item.general.desc"] = "Mémorise les interrupteurs de la ligne Général du panneau réseau d'Anarchy : Au sol, Élévation, Tunnel, Pente constante, Grande médiane et Gamme d'élévation élargie (qui supprime la limite de hauteur). Nécessite Anarchy (mod Paradox 74604) ; sans lui cet élément ne fait rien.";
			d["item.general.ex.group"] = "par exemple, choisissez Tunnel sur une petite route à deux voies, les autres petites routes sont forcées en tunnel aussi, mais une grande route non";
			d["item.general.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même jeu d'interrupteurs de Général, mais dans le menu Électricité ce n'est plus le cas";
			d["item.general.ex.category"] = "passer à une grande route donne toujours Tunnel, mais un pont revient à rien de coché et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 10 Mode souterrain ----------
			d["item.underground.label"] = "Mode souterrain";
			d["item.underground.desc"] = "Mémorise l'interrupteur Mode souterrain (Changer le mode souterrain), qui décide si ce que vous placez passe sous le sol ou dessus.";
			d["item.underground.ex.group"] = "par exemple, activez le Mode souterrain sur une petite route à deux voies, les autres petites routes passent aussi sous terre, mais une grande route non";
			d["item.underground.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes sont construits sous terre, mais dans le menu Électricité ce n'est plus le cas";
			d["item.underground.ex.category"] = "passer à une grande route reste sous terre, mais un pont reste en surface et réclame son propre réglage, bien que les deux soient dans le menu Routes";

			// ---------- 11 Autres ----------
			d["item.other.label"] = "Autres";
			d["item.other.desc"] = "Mémorise les autres lignes qu'un panneau d'outil peut enregistrer : les couleurs de route de la ligne Couleur, plus Taille du pinceau et Force du pinceau.";
			d["item.other.ex.group"] = "par exemple, mettez la couleur d'une petite route à deux voies en bleu, les autres petites routes deviennent bleues aussi, mais une grande route non";
			d["item.other.ex.menu"] = "tous les assets sur lesquels vous cliquez dans le menu Routes utilisent ce même réglage de couleur et de pinceau, mais dans le menu Électricité ce n'est plus le cas";
			d["item.other.ex.category"] = "passer à une grande route reste en bleu, mais un pont revient à sa couleur par défaut et réclame son propre réglage, bien que les deux soient dans le menu Routes";
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
			d["group.compat"] = "Compatibilità";
			d["group.about"] = "Informazioni e collegamenti";

			d["enabled.label"] = "Abilita Memoria modalità strumento";
			d["enabled.desc"] = "Attivo per impostazione predefinita. Finché è attivo, ogni valore viene registrato di continuo e al ritorno in partita i pannelli sono esattamente come li hai lasciati. Disattivare una singola voce ne blocca solo il ripristino; i valori continuano a essere registrati. Solo spegnendo questo interruttore generale si interrompe ogni registrazione e ogni strumento torna al comportamento originale.";
			d["compat.label"] = "Compatibile con altri mod";
			d["compat.desc"] = "Attivo per impostazione predefinita. Attivo: la memoria usa i nomi di menu e gruppo come adattati da altri mod (Asset UI Manager, ExtraLib e simili), quindi un asset spostato segue la sua nuova posizione. Disattivo: viene mantenuta la classificazione che questo mod ha visto per prima, più stabile. Nota che un asset personalizzato che l'autore non ha classificato nel tipo di asset corretto non può essere regolato insieme a quel tipo. In entrambi i casi i valori già registrati non vengono persi.";
			d["scope.label"] = "Ambito di condivisione";
			d["scope.desc"] = "Quanto viene condiviso il valore di questa voce. Le parentesi nel menu a tendina mostrano cosa fa l'originale e cosa consigliamo.";
			d["scope.line.group"] = "Stesso gruppo: {0}";
			d["scope.line.menu"] = "Stesso menu: {0}";
			d["scope.line.category"] = "Stesso tipo di asset: solo gli asset o le funzioni della stessa sottocategoria condividono l'impostazione, cioè {0}";
			d["scope.line.shared"] = "Globale: ogni asset e ogni funzione che supporta questa voce condivide il valore, ma asset e funzioni non condividono mai tra loro";
			d["scope.line.sharedSingle"] = "Globale: ogni asset e ogni funzione che supporta questa voce condivide il valore, e anche asset e funzioni condividono tra loro";
			d["scope.line.unique"] = "Non condiviso: ogni asset e ogni funzione che supporta questa voce viene impostato in modo indipendente";
			d["scope.note"] = "Nota: per funzioni si intendono Zone, Aree e spazi, Terraformazione, Indicatore e oggetti prefabbricati, cioè gli strumenti che non sono asset";

			d[kScopeGroup] = "Stesso gruppo";
			d[kScopeMenu] = "Stesso menu";
			d[kScopeCategory] = "Stesso tipo di asset";
			d[kScopeGlobalShared] = "Globale";
			d[kScopeGlobalUnique] = "Non condiviso";
			d[kTagVanilla] = " (originale)";
			d[kTagRecommended] = " (consigliato)";
			d[kTagRecVanilla] = " (consigliato, originale)";

			d["reset.label"] = "Reimposta memoria";
			d["reset.desc"] = "In partita: cancella la memoria di questa partita e riporta gli strumenti allo stato originale del caricamento. Nel menu principale: cancella la memoria di tutte le partite.";
			d["reset.warn"] = "Non annullabile.";
			d["reset.confirm"] = "Reimpostare le impostazioni degli strumenti memorizzate?";
			d["resetall.label"] = "Reimposta tutte le impostazioni";
			d["resetall.desc"] = "Riporta ogni opzione di questo mod al suo valore consigliato predefinito: l'interruttore generale e quello di compatibilità si riattivano, e vengono ripristinati anche lo stato di attivazione e l'ambito di condivisione di ogni voce. La memoria già registrata delle partite non viene toccata.";
			d["resetall.warn"] = "Non annullabile.";
			d["resetall.confirm"] = "Reimpostare tutte le impostazioni?";
			d["folder.label"] = "Gestisci i file di memoria di tutte le partite";
			d["folder.desc"] = "Apre la cartella locale dei file di memoria, uno per partita, con il nome della partita. La memoria viene sempre letta in base al nome della partita: un file viene usato solo se il nome della partita e quello del file json corrispondono, quindi rinominare il file è il modo di copiare la memoria da una partita all'altra. Quando carichi un salvataggio automatico, la sua memoria viene salvata con il nome della città, perché un salvataggio automatico riceve un nome nuovo ogni volta.";

			d["about.version"] = "Versione mod";
			d["about.author"] = "Autore";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Supporta l'autore su Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Apri la discussione sul forum Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Apri il sito della serie Rainbow.";

			// ---------- 1 Anarchy ----------
			d["item.anarchy.desc"] = "Ricorda lo stato del pulsante Anarchy aggiunto dal mod Anarchy. Anarchy è condiviso in tutto il gioco, quindi questa voce è globale; una volta attivo puoi cambiare l'ambito di condivisione. Senza il mod Anarchy questa voce non fa nulla.";
			d["item.anarchy.ex.group"] = "per esempio, attivi Anarchy su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attivo, mentre una strada grande no";
			d["item.anarchy.ex.menu"] = "tutti gli asset che clicchi nel menu Strade hanno Anarchy attivo, ma passando al menu Elettricità no";
			d["item.anarchy.ex.category"] = "passando a una strada grande Anarchy resta attivo, ma un ponte torna a spento e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Ricorda quali voci sono spuntate nel filtro «Tema» della barra degli strumenti. Quella riga compare solo quando la categoria corrente contiene davvero asset che usano i temi, e spuntarne uno limita l'elenco della barra agli asset che supportano quei temi. L'originale reimposta il filtro sul tema predefinito solo al caricamento di un salvataggio, quindi questa voce è disattivata per impostazione predefinita.";
			d["item.themes.ex.group"] = "per esempio, spunti un tema solo in Strade/Strade piccole, così una strada grande non filtra per quel tema";
			d["item.themes.ex.menu"] = "tutti gli asset che clicchi nel menu Strade seguono il tema spuntato lì, ma passando al menu Elettricità no";
			d["item.themes.ex.category"] = "i vicoli e i binari della metropolitana contano come lo stesso tipo di asset, quindi il tema spuntato è condiviso tra i menu";
			d["item.packs.label"] = "Pacchetto";
			d["item.packs.desc"] = "Ricorda quali voci sono spuntate nel filtro «Pacchetto» della barra degli strumenti; spuntarne una limita l'elenco della barra agli asset che appartengono a quei pacchetti. L'originale lo azzera ogni volta che cambi menu o categoria, quindi ricordarlo per menu e categoria corrisponde a ciò che vedi, e anche questa voce è disattivata per impostazione predefinita.";
			d["item.packs.ex.group"] = "per esempio, spunti un pacchetto in Strade/Strade piccole, l'originale lo azzera sulle strade grandi e ritorna quando ci torni";
			d["item.packs.ex.menu"] = "i pacchetti spuntati nel menu Strade e nel menu Elettricità vengono ricordati separatamente";
			d["item.packs.ex.category"] = "i vicoli compaiono con lo stesso nome sia nel menu Strade che nel menu Quartieri, quindi i pacchetti spuntati sono un unico insieme condiviso";

			// ---------- 2 Modalità strumento ----------
			d["item.toolMode.label"] = "Modalità strumento";
			d["item.toolMode.desc"] = "Ricorda la modalità scelta per ogni strumento e le opzioni cambiano a seconda dello strumento: strade, binari e tubi offrono Rettilineo, Curva semplice, Curva complessa, Continua, Griglia, Sostituisci, Punto; edifici, oggetti e alberi offrono Posiziona uno, Posiziona multipli, Linea, Curva, Strumento timbro oggetti; le zone offrono Riempi, Seleziona, Pittura; le aree offrono Modifica, Genera griglia mappa. Le modalità degli asset e quelle delle funzioni sono ricordate separatamente perché le opzioni non sono le stesse.";
			d["item.toolMode.ex.group"] = "per esempio, scegli Curva semplice su una piccola strada a due corsie e anche le altre piccole strade usano Curva semplice, mentre una strada grande no";
			d["item.toolMode.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono su Curva semplice, ma passando al menu Elettricità no";
			d["item.toolMode.ex.category"] = "passando a una strada grande resta Curva semplice, ma un ponte riparte dalla sua modalità predefinita e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 3 Elevazione ----------
			d["item.elevation.label"] = "Elevazione";
			d["item.elevation.desc"] = "Ricorda l'elevazione a cui lasci lo strumento, incluso il risultato di Aumenta elevazione e Diminuisci elevazione. Il Livello elevazione (quanto sposta una singola pressione; Anarchy chiama questa riga Step di elevazione) viene ricordato insieme ad essa e usa lo stesso ambito di condivisione. L'elevazione degli incroci non è inclusa.";
			d["item.elevation.ex.group"] = "per esempio, porti una strada a due corsie a 10 m e anche le altre piccole strade vanno a 10 m, mentre una strada grande non è a 10 m";
			d["item.elevation.ex.menu"] = "tutti gli asset che clicchi nel menu Strade sono a 10 m, ma passando al menu Elettricità non sono a 10 m";
			d["item.elevation.ex.category"] = "passando a una strada grande restano 10 m, ma un ponte riparte da 0 m e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 4 Modalità parallelismo ----------
			d["item.parallel.label"] = "Modalità parallelismo";
			d["item.parallel.desc"] = "Ricorda l'interruttore Modalità parallelismo (Attiva/disattiva modalità parallelismo) con il numero di Strada parallela e la Compensazione parallelismo che Aumenta compensazione e Diminuisci compensazione modificano. Permette di costruire reti parallele.";
			d["item.parallel.ex.group"] = "per esempio, attivi la Modalità parallelismo su una piccola strada a due corsie e imposti il numero a 3, e anche le altre piccole strade ne costruiscono 3, mentre una strada grande no";
			d["item.parallel.ex.menu"] = "tutti gli asset che clicchi nel menu Strade costruiscono 3 linee parallele, ma passando al menu Elettricità no";
			d["item.parallel.ex.category"] = "passando a una strada grande si costruiscono ancora 3 linee, ma un ponte torna a spento e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 5 Aggancio ----------
			d["item.snap.label"] = "Aggancio";
			d["item.snap.desc"] = "Ricorda ogni interruttore della riga Aggancio: Aggancia alla geometria esistente, Aggancia alla lunghezza della cella di zonizzazione, Aggancia ad angoli di 90°, Aggancia ai lati di una strada, Aggancia alle strade, Aggancia al lato del proprietario, Aggancia ai lati di un edificio, Aggancia al centro di una strada, Aggancia alla costa, Aggancia alla geometria vicina, Aggancia alle linee guida, Aggancia alla griglia della zona, Aggancia ai nodi, Aggancia alla superficie di un oggetto, Aggancia in verticale, Aggancia alla griglia del lotto, Collega elementi sovrapposti a un edificio, Rimuovi solo il tipo corrispondente, Mostra le linee di contorno, Aggancia a distanza. La riga Attiva/disattiva tutti gli agganci del gioco è un interruttore multiplo e non viene ricordata.";
			d["item.snap.ex.group"] = "per esempio, spegni «Aggancia ai lati di una strada» su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno spento, mentre una strada grande ce li ha tutti accesi";
			d["item.snap.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di aggancio, ma passando al menu Elettricità no";
			d["item.snap.ex.category"] = "passando a una strada grande l'insieme resta lo stesso, ma un ponte ritorna ai valori predefiniti e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 6 Topografia ----------
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Ricorda la riga Topografia, cioè se Mostra le linee di contorno è attivo.";
			d["item.topography.ex.group"] = "per esempio, attivi la Topografia su una piccola strada a due corsie e anche le altre piccole strade ce l'hanno attiva, mentre una strada grande no";
			d["item.topography.ex.menu"] = "tutti gli asset che clicchi nel menu Strade mostrano le linee di contorno, ma passando al menu Elettricità no";
			d["item.topography.ex.category"] = "passando a una strada grande le linee restano, ma un ponte ha la riga spenta e necessita un'impostazione propria, benché entrambi siano nel menu Strade";


			// ---------- 8 Sinistra e destra (Anarchy) ----------
			d["item.leftRight.label"] = "Sinistra e destra";
			d["item.leftRight.desc"] = "Ricorda quali potenziamenti di rete sono scelti sulle righe Sinistra e Destra del pannello di rete di Anarchy, per esempio Pista ciclabile, Alberi, Parcheggio, Banchina, Muro di contenimento e Barriera insonorizzante. Richiede Anarchy (mod Paradox 74604); senza, questa voce non fa nulla.";
			d["item.leftRight.ex.group"] = "per esempio, scegli Pista ciclabile sul lato destro di una piccola strada a due corsie e anche le altre piccole strade ce l'hanno a destra, mentre una strada grande no";
			d["item.leftRight.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso paio di scelte laterali, ma passando al menu Elettricità no";
			d["item.leftRight.ex.category"] = "passando a una strada grande la pista ciclabile resta a destra, ma un ponte torna a niente di scelto e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 9 Generale (Anarchy) ----------
			d["item.general.label"] = "Generale";
			d["item.general.desc"] = "Ricorda gli interruttori della riga Generale del pannello di rete di Anarchy: Terreno, Elevato, Tunnel, Pendio costante, Mediana ampia e Intervallo di elevazione ampliato (che toglie il limite di altezza). Richiede Anarchy (mod Paradox 74604); senza, questa voce non fa nulla.";
			d["item.general.ex.group"] = "per esempio, scegli Tunnel su una piccola strada a due corsie e anche le altre piccole strade vengono forzate in tunnel, mentre una strada grande no";
			d["item.general.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso insieme di interruttori di Generale, ma passando al menu Elettricità no";
			d["item.general.ex.category"] = "passando a una strada grande resta Tunnel, ma un ponte torna a niente di spuntato e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 10 Modalità sottosuolo ----------
			d["item.underground.label"] = "Modalità sottosuolo";
			d["item.underground.desc"] = "Ricorda l'interruttore Modalità sottosuolo (Attiva/disattiva modalità sottosuolo), che decide se ciò che piazzi va sotto terra o sopra.";
			d["item.underground.ex.group"] = "per esempio, attivi la Modalità sottosuolo su una piccola strada a due corsie e anche le altre piccole strade vanno sottoterra, mentre una strada grande no";
			d["item.underground.ex.menu"] = "tutti gli asset che clicchi nel menu Strade vengono costruiti sottoterra, ma passando al menu Elettricità no";
			d["item.underground.ex.category"] = "passando a una strada grande si resta sottoterra, ma un ponte rimane in superficie e necessita un'impostazione propria, benché entrambi siano nel menu Strade";

			// ---------- 11 Altro ----------
			d["item.other.label"] = "Altro";
			d["item.other.desc"] = "Ricorda le altre righe che un pannello strumento può registrare: i colori delle strade nella riga Colore, più Dimensione pennello e Robustezza pennello.";
			d["item.other.ex.group"] = "per esempio, imposti il colore di una piccola strada a due corsie su blu e anche le altre piccole strade diventano blu, mentre una strada grande no";
			d["item.other.ex.menu"] = "tutti gli asset che clicchi nel menu Strade usano questo stesso colore e impostazione del pennello, ma passando al menu Elettricità no";
			d["item.other.ex.category"] = "passando a una strada grande resta blu, ma un ponte ritorna al colore predefinito e necessita un'impostazione propria, benché entrambi siano nel menu Strade";
			return d;
		}
	}
}
