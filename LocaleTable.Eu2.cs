using System;
using System.Collections.Generic;

namespace ToolModeMemory
{
	/// <summary>
	/// pl-PL / pt-BR / ru-RU 文案。从 LocaleTable.cs 拆出，便于分工维护；结构与其它语言一致：Frames() 之后再补本语言独有的键。
	/// v0.2.1：页面底部的「共用范围说明」板块取消，五行定义并进每一个工具项的 scope 说明，
	/// 例子改由 item.&lt;id&gt;.ex.group / .menu / .category 三个槽位按该项自己的数值填入。
	/// 游戏内名词一律取官方语言包（research/locale/all_locales.json）与 Anarchy 自带语言包；
	/// 左侧和右侧、常规两项属于 Anarchy（74604），旧文案写成 Extra Networks and Areas 是错误归属，已改正。
	/// </summary>
	internal static partial class LocaleTable
	{
		// ======================= pl-PL =======================

		private static Dictionary<string, string> Pl()
		{
			Dictionary<string, string> d = Frames("pl-PL");
			d["mod.name"] = "Pamięć narzędzi";
			d["tab.mod"] = "Ustawienia trybu narzędzia";
			d["tab.about"] = "Informacje";
			d["group.master"] = "Pamięć narzędzi";
			d["group.official"] = "Ustawienia oficjalnych narzędzi";
			d["group.anarchy"] = "Ustawienia narzędzi Anarchy";
			d["group.reset"] = "Zarządzanie pamięcią";
			d["group.about"] = "Informacje i linki";

			d["scope.label"] = "Zakres współdzielenia";
			d["scope.desc"] = "Jak szeroko współdzielona jest wartość tej pozycji; nawiasy pokazują, co robi gra sama.\nZakres działa tylko przy włączonym przełączniku tej pozycji — wyłączona zachowuje się jak w grze.";
			d["scope.line.group"] = "Ta sama grupa: narzędzia w tej samej grupie paska menu używają jednej wartości. {0}";
			d["scope.line.menu"] = "To samo menu: wszystkie narzędzia w menu używają jednej wartości. {0}";
			d[kScopeLineCategory] = "Ten sam typ zasobu: zasoby grupujemy według tego, komu służą, np. mały i duży parking to jeden typ, a most to typ osobny: {0}";

			d[kScopeGroup] = "Ta sama grupa";
			d[kScopeMenu] = "To samo menu";
			d[kScopeCategory] = "Ten sam typ zasobu";
			d[kScopeGlobalShared] = "Globalnie";
			d[kScopeGlobalUnique] = "Bez współdzielenia";
			d[kTagVanilla] = " (oryginał)";
			d[kTagRecommended] = " (zalecane)";
			d[kTagRecVanilla] = " (zalecane, oryginał)";

			d["reset.label"] = "Resetuj pamięć";
			d["reset.desc"] = "W grze: kasuje pamięć tego zapisu i przywraca narzędzia do stanu oryginalnego z wczytania. W menu głównym lub edytorze: kasuje pamięć wszystkich zapisów.";
			d["reset.warn"] = "Nie można cofnąć.";
			d["reset.confirm"] = "Zresetować zapamiętane ustawienia narzędzi?";
			d["resetall.label"] = "Zresetuj wszystkie ustawienia";
			d["resetall.desc"] = "Przywraca wszystkie ustawienia tego moda do wartości fabrycznych: przełącznik główny wyłącza się, przełączniki stron włączają, a każda pozycja wraca do stanu fabrycznego.\nZapisana pamięć zapisów gry zostaje nienaruszona.";
			d["resetall.warn"] = "Nie można cofnąć.";
			d["resetall.confirm"] = "Zresetować wszystkie ustawienia tego moda do wartości fabrycznych?";
			d["folder.label"] = "Zarządzaj plikami pamięci wszystkich zapisów";
			d["folder.desc"] = "Otwiera folder z plikami pamięci, jednym na zapis, o nazwie takiej jak zapis na liście «Wczytaj grę».\nPlik jest używany tylko wtedy, gdy nazwa zapisu i nazwa pliku są takie same, więc zmiana nazwy pliku kopiuje pamięć między zapisami.";

			d["about.version"] = "Wersja moda";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Wesprzyj autora na Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Otwórz wątek na forum Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Otwórz stronę serii Rainbow.";

			d["item.anarchy.desc"] = "Zapamiętuje przełącznik moda Anarchy.\nJest zapamiętywany tylko na narzędziach, które naprawdę mają ten przełącznik.\nBez moda Anarchy ta pozycja się nie pojawia.";
			d["item.anarchy.ex.group"] = "na przykład włączysz Anarchy dla małej dwupasmowej drogi i inne małe drogi też będą mieć Anarchy włączone, a duża droga nie.";
			d["item.anarchy.ex.menu"] = "każdy zasób kliknięty w menu Drogi ma włączone Anarchy, ale po przejściu do menu Elektryczność już nie.";
			d["item.anarchy.ex.category"] = "przełączenie na drogę sześciopasmową zostawia go włączonym, ale most i metro potrzebują własnego ustawienia.";
			d["item.themes.label"] = "Motyw";
			d["item.themes.desc"] = "Zapamiętuje zaznaczone motywy w filtrze Motyw na pasku narzędzi, co ogranicza listę do zasobów obsługujących te motywy.\nWiersz pojawia się tylko w kategorii, która naprawdę ma motywy.";
			d["item.themes.ex.group"] = "na przykład zaznaczysz motyw tylko w Drogi/Małe drogi, więc duże drogi już według niego nie filtrują.";
			d["item.themes.ex.menu"] = "motyw zaznaczony w menu Drogi przestaje działać w menu Elektryczność i wraca, gdy tam wrócisz.";
			d["item.themes.ex.category"] = "ten wiersz to stan samego panelu filtrowania, więc jest zapamiętywany pod aktualnym menu i grupą, a nie według typu zasobu.";
			d["item.packs.label"] = "Pakiet";
			d["item.packs.desc"] = "Zapamiętuje zaznaczone pakiety w filtrze Pakiet na pasku narzędzi, co ogranicza listę do zasobów z tych pakietów.\nWiersz pojawia się tylko w kategorii, która ma takie pakiety.";
			d["item.packs.ex.group"] = "na przykład zaznaczysz pakiet w Drogi/Małe drogi, oryginał wyczyści go przy przejściu na duże drogi, a po powrocie znów jest zaznaczony.";
			d["item.packs.ex.menu"] = "pakiety zaznaczone w menu Drogi i pakiety zaznaczone w menu Elektryczność są zapamiętywane osobno.";
			d["item.packs.ex.category"] = "ten wiersz to stan samego panelu filtrowania, zapamiętywany pod aktualnym menu i grupą; oryginał czyści go przy każdej zmianie grupy.";
			d["item.toolMode.label"] = "Narzędzia";
			d["item.toolMode.ex.group"] = "na przykład ustawisz Jeden zakręt dla małej dwupasmowej drogi i inne małe drogi też będą mieć Jeden zakręt, a duża droga nie.";
			d["item.toolMode.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest na trybie Jeden zakręt, ale po przejściu do menu Elektryczność już nie.";
			d["item.toolMode.ex.category"] = "np. przejście na drogę sześciopasmową nadal daje Jeden zakręt, ale most albo tory metra wracają do swojego trybu domyślnego.";
			d["item.elevation.label"] = "Wzniesienie";
			d["item.elevation.desc"] = "Zapamiętuje wysokość, na jakiej zostawiono narzędzie, wraz ze stopniem wzniesienia, czyli o ile przesuwa jedno naciśnięcie.\nWysokość skrzyżowań nie jest pamiętana; zasób, dla którego wartość jest niedozwolona, zachowuje własną.";
			d["item.elevation.ex.group"] = "na przykład podniesiesz małą dwupasmową drogę do 10 m i inne małe drogi też pójdą na 10 m, a duża droga nie będzie na 10 m.";
			d["item.elevation.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest na 10 m, ale po przejściu do menu Elektryczność nie jest na 10 m.";
			d["item.elevation.ex.category"] = "droga sześciopasmowa też ma 10 m, ale most i tory kolejowe wracają do 0 m.";
			d["item.parallel.label"] = "Tryb równoległy";
			d["item.parallel.desc"] = "Zapamiętuje przełącznik Tryb równoległy i odstęp z panelu Równoległa droga: druga linia powstaje obok rysowanej.\nFabrycznie wyłączony, więc do czasu włączenia gra działa po swojemu.";
			d["item.parallel.ex.group"] = "włączysz Tryb równoległy z odstępem 5 m dla małej drogi i inne małe drogi wrócą tak samo, a duża droga nie.";
			d["item.parallel.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest równoległy z kompensacją 5 m, ale po przejściu do menu Elektryczność już nie.";
			d["item.parallel.ex.category"] = "droga sześciopasmowa też ma 5 m, ale most i metro wyłączają ten tryb.";
			d["item.snap.label"] = "Przyciąganie";
			d["item.snap.desc"] = "Zapamiętuje przełączniki wiersza Przyciąganie: do istniejących kształtów, do kątów prostych, do stron drogi, do linii pomocniczych i pozostałe.\nZestaw zależy od narzędzia; zbiorczy wiersz Wł./wył. przyciąganie z gry nie jest pamiętany.";
			d["item.snap.ex.group"] = "wyłączysz «Przyciągaj do stron drogi» dla małej drogi i inne małe drogi też to mają wyłączone.";
			d["item.snap.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego zestawu przełączników przyciągania, ale po przejściu do menu Elektryczność nie.";
			d["item.snap.ex.category"] = "droga sześciopasmowa zachowuje zestaw, ale most i ścieżka wracają do domyślnych.";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Zapamiętuje wiersz Topografia, czyli czy pokazywane są linie konturów terenu.";
			d["item.topography.ex.group"] = "na przykład włączysz Topografię dla małej dwupasmowej drogi i inne małe drogi też będą ją mieć włączoną, a duża droga nie.";
			d["item.topography.ex.menu"] = "każdy zasób kliknięty w menu Drogi pokazuje linie konturów, ale po przejściu do menu Elektryczność nie.";
			d["item.topography.ex.category"] = "droga sześciopasmowa nadal je pokazuje, ale most i przewód mają ten wiersz wyłączony.";
			d["item.leftRight.label"] = "Po lewej i Po prawej";
			d["item.leftRight.desc"] = "Zapamiętuje ulepszenia sieci wybrane w wierszach «Po lewej» i «Po prawej» panelu Anarchy, np. Pas dla rowerów, Trawnik, Nabrzeże czy Bariera akustyczna.\nWymaga Anarchy (mod Paradox 74604).";
			d["item.leftRight.ex.group"] = "na przykład wybierzesz Pas dla rowerów po prawej stronie małej dwupasmowej drogi i inne małe drogi też go tam dostaną, a duża droga nie.";
			d["item.leftRight.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tej samej pary wyborów stron, ale po przejściu do menu Elektryczność nie.";
			d["item.leftRight.ex.category"] = "droga sześciopasmowa też ma Pas dla rowerów po prawej, ale most i tory kolejowe nie.";
			d["item.general.label"] = "Ogólnie";
			d["item.general.desc"] = "Zapamiętuje przełączniki wiersza «Ogólnie» panelu Anarchy: Podłoże, Podwyższenie, Tunel, Stałe nachylenie, Szeroka mediana i Rozszerzony zasięg wysokości.\nWymaga Anarchy (mod Paradox 74604).";
			d["item.general.ex.group"] = "na przykład wybierzesz Tunel dla małej dwupasmowej drogi i inne małe drogi też zostaną zamienione w tunele, a duża droga nie.";
			d["item.general.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego zestawu przełączników wiersza Ogólnie, ale po przejściu do menu Elektryczność nie.";
			d["item.general.ex.category"] = "droga sześciopasmowa też daje Tunel, ale most i metro nie mają nic zaznaczonego.";
			d["item.underground.label"] = "Tryb podziemny";
			d["item.underground.desc"] = "Zapamiętuje przełącznik Tryb podziemny, który decyduje, czy budujesz pod ziemią, czy na niej.\nFabrycznie wyłączony, więc do czasu włączenia gra działa po swojemu.";
			d["item.underground.ex.group"] = "na przykład włączysz Tryb podziemny dla małej dwupasmowej drogi i inne małe drogi też zejdą pod ziemię, a duża droga nie.";
			d["item.underground.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest budowany pod ziemią, ale po przejściu do menu Elektryczność nie.";
			d["item.underground.ex.category"] = "droga sześciopasmowa też schodzi pod ziemię, ale most i tory kolejowe zostają na powierzchni.";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（pl-PL）====================
		d["domain.asset.label"] = "Pamiętaj narzędzia strony Zasoby";
		d["domain.asset.desc"] = "Włączony: ustawienia narzędzi z tej strony są zapisywane na bieżąco i wracają po wczytaniu zapisu.\nWyłączony: nic nie jest zapisywane ani przywracane, a zakresu współdzielenia nie da się ustawić.";
		d["domain.function.label"] = "Pamiętaj narzędzia strony Funkcje";
		d["domain.function.desc"] = "Włączony: ustawienia narzędzi z tej strony są zapisywane na bieżąco i wracają po wczytaniu zapisu.\nWyłączony: nic nie jest zapisywane ani przywracane, a zakresu współdzielenia nie da się ustawić.";
		d["domain.item.label"] = "Pamiętaj narzędzia strony Rekwizyty";
		d["domain.item.desc"] = "Włączony: ustawienia narzędzi z tej strony są zapisywane na bieżąco i wracają po wczytaniu zapisu.\nWyłączony: nic nie jest zapisywane ani przywracane, a zakresu współdzielenia nie da się ustawić.";
		d["group.domain"] = "Przełączniki stron";
		d["item.color.label"] = "Kolor";
		d["item.decoration.label"] = "Zachowaj wiek";
		d["item.decoration.desc"] = "Zapamiętuje przełącznik «Zachowaj wiek»: drzewa przestają rosnąć i umierać ze starości.\nWiersz pojawia się tylko na drzewach.";
			d["item.i.anarchy.label"] = "Anarchy";
			d["item.brushType.label"] = "Rodzaj pędzla";
			d["item.brushType.desc"] = "Zapamiętuje rodzaj pędzla: okrągły, kwadratowy, z miękką krawędzią.\nWiersz jest tylko u narzędzi, którymi można malować.";
			d["item.brushSize.label"] = "Rozmiar pędzla";
			d["item.brushSize.desc"] = "Zapamiętuje szerokość jednego pociągnięcia pędzla.\nWiersz jest tylko u narzędzi, którymi można malować.";
			d["item.brushStrength.label"] = "Nacisk pędzla";
			d["item.brushStrength.desc"] = "Zapamiętuje, jak mocno zmienia jedno pociągnięcie pędzla.\nWiersz jest tylko u narzędzi, którymi można malować.";
			d["item.brushHeight.label"] = "Docelowa wysokość";
			d["item.brushHeight.desc"] = "Zapamiętuje docelową wysokość, na jakiej kończy się teren po wyrównaniu albo uformowaniu spadku.\nWiersz mają tylko narzędzia wyrównywania terenu i spadku.";
			d["item.distance.label"] = "Odległość";
			d["item.distance.desc"] = "Zapamiętuje, jak daleko od siebie stoją obiekty stawiane wzdłuż linii albo krzywej.\nWiersz jest tylko w trybach Linia i Krzywa.";
			d["item.ageMask.label"] = "Wiek";
			d["item.ageMask.desc"] = "Zapamiętuje zaznaczone etapy wieku w wierszu «Wiek»: sadzonka, młode, dojrzałe i stare, kilka naraz.\nWiersz pojawia się tylko na drzewach.";
			d["item.anarchyObjectElevation.label"] = "Wzniesienie obiektu";
			d["item.anarchyObjectElevation.desc"] = "Zapamiętuje regulatory wysokości, które Anarchy dodaje do panelu: blokada przekształceń, wysokość, zmienność wysokości i krok podnoszenia/opuszczania.\nSą w jednym rzędzie, więc są zapamiętywane i współdzielone razem.";
		d["item.f.snap.desc"] = "Zapamiętuje przełączniki wiersza Przyciąganie w panelach funkcji, np. do kątów prostych, do siatki, do dróg czy linie konturów.\nKtóre przełączniki widać, zależy od narzędzia.";
		d["item.f.topography.desc"] = "Zapamiętuje, czy w panelach funkcji widać linie konturów terenu.";
		d["item.f.underground.desc"] = "Zapamiętuje przełącznik Tryb podziemny w panelach funkcji.";
		d["item.i.snap.desc"] = "Zapamiętuje przełączniki wiersza Przyciąganie na panelach rekwizytów, np. do powierzchni obiektu, do pobliskich kształtów czy Usuń tylko pasujący typ.\nKtóre widać, zależy od rekwizytu.";
		d["item.i.toolMode.desc"] = "Zapamiętuje tryb narzędzia na panelach rekwizytów: Umieść jeden, Umieść wiele, Linia, Krzywa, Stempel obiektu czy Pędzel.\nZestaw zależy od rekwizytu.";
		d["item.i.topography.desc"] = "Zapamiętuje, czy na panelach rekwizytów widać linie konturów terenu.";
		d["item.i.underground.desc"] = "Zapamiętuje, czy rekwizyty stawiasz pod ziemią, czy nad nią.";
		d["scope.category.function"] = "Ten sam typ";
		d["scope.category.item"] = "Ten sam typ rekwizytu";
		d["tab.assets"] = "Zasoby";
		d["tab.functions"] = "Funkcje";
		d["tab.items"] = "Rekwizyty";
		d["tab.master"] = "Główny przełącznik";
		d["enabled.label"] = "Włącz mod";
		d["enabled.desc"] = "Fabrycznie wyłączony. Włączony: ustawienia poniżej działają, a ustawienia narzędzi są zapisywane i wracają po wczytaniu zapisu.\nWyłączony: mod nic nie robi i wszystko działa jak w grze.";
			d["scope.line.category"] = "Ten sam typ zasobu: zasoby grupujemy według tego, komu służą, i jedna grupa używa jednej wartości. {0}";
			d["ex.net.group"] = "Na przykład wszystkie drogi z grupy Małe drogi używają jednej wartości, a Duże drogi to inna grupa.";
			d["ex.net.menu"] = "Na przykład wszystkie drogi w menu Drogi używają jednej wartości, a w menu Energia już nie.";
			d["ex.net.category"] = "Na przykład drogi o dowolnej liczbie pasów to jeden typ, a mosty i tory kolejowe to osobne typy.";
			d["ex.zone.group"] = "Na przykład zabudowa o niskiej, średniej i wysokiej gęstości w Strefy mieszkalne to jedna grupa, a Strefy komercyjne to inna grupa, choć obie są w tym samym menu.";
			d["ex.zone.menu"] = "Na przykład Strefy mieszkalne, Strefy komercyjne, Strefy przemysłowe i Strefy biurowe używają jednej wartości, a inne menu nie.";
			d["ex.route.group"] = "Na przykład linie grupuje się według środka transportu (droga, tramwaj, pociąg, metro itd.), a linia innego środka to inna grupa.";
			d["ex.route.menu"] = "Na przykład wszystkie linie w menu Transport używają jednej wartości, a inne menu nie.";
			d["ex.terrain.group"] = "Terraformowanie ma tylko jedną grupę, więc Ta sama grupa i To samo menu wybierają dokładnie te same narzędzia.";
			d["ex.terrain.menu"] = "Terraformowanie ma tylko jedną grupę, więc To samo menu i Ta sama grupa wybierają dokładnie te same narzędzia.";
			d["ex.prop.group"] = "Na przykład menu Kształtowanie krajobrazu dzieli obiekty na Natura, Park, mieszkalne, komercyjne, przemysłowe, gruz, naklejki i donice; jedna kategoria używa jednej wartości, a inna to inna grupa.";
			d["ex.prop.menu"] = "Wszystkie obiekty i drzewa w menu Kształtowanie krajobrazu używają jednej wartości.";
			d["ex.tree.group"] = "Wszystkie drzewa są w grupie Roślinność w menu Kształtowanie krajobrazu, więc Ta sama grupa i To samo menu wybierają te same drzewa.";
			d["ex.tree.menu"] = "Wszystkie drzewa są w grupie Roślinność w menu Kształtowanie krajobrazu, więc To samo menu i Ta sama grupa wybierają te same drzewa.";
			d["item.color.desc"] = "Zapamiętuje wiersz Kolor w narzędziach linii: kolor, którym rysowana jest linia.";
			d["item.f.snap.desc"] = "Zapamiętuje przełączniki wiersza Przyciąganie w narzędziach funkcji; dostępne opcje zależą od narzędzia.\nPrzycisk gry do włączania i wyłączania wszystkich przyciągnięć to przełącznik zbiorczy i nie jest zapamiętywany.";
			d["item.f.topography.desc"] = "Zapamiętuje wiersz Topografia w narzędziach stref, terraformowania i innych: czy widoczne są linie konturowe terenu.";
			d["item.f.underground.desc"] = "Zapamiętuje wiersz Tryb podziemny w narzędziach obszaru, linii, wyburzania i obiektów.";
			d["item.i.topography.desc"] = "Zapamiętuje wiersz Topografia w panelach obiektów.";
			d["scope.desc.function"] = "Jak szeroko współdzielona jest wartość tej pozycji.";
			d["scope.desc.item"] = "Jak szeroko współdzielona jest wartość tej pozycji.";
			d["scope.line.shared"] = "Globalnie: wszystkie budynki i usługi, które pokazują tę pozycję, używają jednego ustawienia.";
			d["scope.line.shared.function"] = "Globalnie: wszystkie funkcje, które pokazują tę pozycję, używają jednego ustawienia.";
			d["scope.line.shared.item"] = "Globalnie: wszystkie rekwizyty, które pokazują tę pozycję, używają jednego ustawienia.";
			d["scope.line.unique"] = "Bez współdzielenia: każdy budynek i każda usługa, które pokazują tę pozycję, są ustawiane osobno.";
			d["scope.line.unique.function"] = "Bez współdzielenia: każda funkcja, która pokazuje tę pozycję, jest ustawiana osobno.";
			d["scope.line.unique.item"] = "Bez współdzielenia: każdy rekwizyt, który pokazuje tę pozycję, jest ustawiany osobno.";
			d["scope.note"] = "Zakresy dla budynków i usług, funkcji oraz rekwizytów są niezależne.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Ten wiersz dotyczy tylko: {0}.";
			d["fam.net.desc"] = "Rodzina dróg: drogi, tory, rury i linie energetyczne liczą się wszystkie.";
			d["fam.net"] = "drogi";
			d["fam.assetObj"] = "budynki";
			d["fam.funcObj"] = "znaczniki";
			d["fam.zone"] = "strefy";
			d["fam.area"] = "narzędzia obszaru";
			d["item.toolMode.desc"] = "Zapamiętuje tryb wybrany w tych narzędziach.";
			d["item.f.toolMode.desc"] = "Zapamiętuje wiersz Tryb narzędzia w narzędziach stref: Wypełnianie, Zaznaczanie i Malowanie.";
			return d;
		}

		// ======================= pt-BR =======================

		private static Dictionary<string, string> Pt()
		{
			Dictionary<string, string> d = Frames("pt-BR");
			d["mod.name"] = "Memória das ferramentas";
			d["tab.mod"] = "Configurações do modo de ferramenta";
			d["tab.about"] = "Sobre";
			d["group.master"] = "Memória das ferramentas";
			d["group.official"] = "Configurações das ferramentas oficiais";
			d["group.anarchy"] = "Configurações das ferramentas do Anarchy";
			d["group.reset"] = "Gestão de memória";
			d["group.about"] = "Informações e links";

			d["scope.label"] = "Âmbito de partilha";
			d["scope.desc"] = "Até que ponto o valor deste item é partilhado; os parênteses mostram o que o próprio jogo faz.\nO âmbito só vale com o interruptor deste item ligado — desligado, ele fica como no jogo.";
			d["scope.line.group"] = "Mesmo grupo: as ferramentas do mesmo grupo da barra de menus partilham um valor. {0}";
			d["scope.line.menu"] = "Mesmo menu: todas as ferramentas de um menu partilham um valor. {0}";
			d[kScopeLineCategory] = "Mesmo tipo de ativo: agrupamos pelo que o ativo atende, então um estacionamento pequeno e um grande são o mesmo tipo, mas uma ponte é um tipo à parte: {0}";

			d[kScopeGroup] = "Mesmo grupo";
			d[kScopeMenu] = "Mesmo menu";
			d[kScopeCategory] = "Mesmo tipo de ativo";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Sem partilhar";
			d[kTagVanilla] = " (original)";
			d[kTagRecommended] = " (recomendado)";
			d[kTagRecVanilla] = " (recomendado, original)";

			d["reset.label"] = "Repor memória";
			d["reset.desc"] = "No jogo: apaga a memória deste jogo e devolve as ferramentas ao estado original do carregamento. No menu principal ou no Editor: apaga a memória de todos os jogos.";
			d["reset.warn"] = "Não pode ser desfeito.";
			d["reset.confirm"] = "Repor as definições de ferramenta memorizadas?";
			d["resetall.label"] = "Repor todas as definições";
			d["resetall.desc"] = "Devolve todas as definições deste mod aos valores de fábrica: o interruptor geral desliga, os interruptores das páginas ligam e cada item volta ao estado de fábrica.\nA memória já gravada dos jogos fica intacta.";
			d["resetall.warn"] = "Não pode ser desfeito.";
			d["resetall.confirm"] = "Repor todas as definições deste mod aos valores de fábrica?";
			d["folder.label"] = "Gerir os arquivos de memória de todos os jogos";
			d["folder.desc"] = "Abre a pasta com os arquivos de memória, um por jogo, com o nome igual ao do jogo na lista «Carregar jogo».\nUm arquivo só é usado quando o nome do jogo e o do arquivo coincidem, então renomear o arquivo copia a memória entre jogos.";

			d["about.version"] = "Versão do mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Apoie o autor no Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Abrir a conversa no fórum da Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Abrir o site da série Rainbow.";

			d["item.anarchy.desc"] = "Lembra o interruptor do mod Anarchy.\nSó é guardado nas ferramentas que têm realmente este interruptor.\nSem o mod Anarchy este item não aparece.";
			d["item.anarchy.ex.group"] = "você liga o Anarchy numa rua pequena e as outras ruas pequenas também ficam ligadas.";
			d["item.anarchy.ex.menu"] = "todo ativo que você clica no menu Vias tem o Anarchy ligado, mas ao mudar para o menu Eletricidade já não.";
			d["item.anarchy.ex.category"] = "mudar para uma via de 6 faixas mantém ligado, mas uma ponte e o metrô precisam do próprio ajuste.";
			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Lembra os temas marcados no filtro Tema da barra de ferramentas, o que limita a lista aos ativos que suportam esses temas.\nA linha só aparece na categoria que realmente tem temas.";
			d["item.themes.ex.group"] = "por exemplo, você marca um tema só em Vias/Ruas, então as vias grandes não filtram por ele.";
			d["item.themes.ex.menu"] = "um tema marcado no menu Vias deixa de valer no menu Eletricidade e volta quando você retorna.";
			d["item.themes.ex.category"] = "esta linha é o estado do próprio painel de filtros, então fica guardada pelo menu e grupo em que você está, e não pelo tipo de ativo.";
			d["item.packs.label"] = "Pacote";
			d["item.packs.desc"] = "Lembra os pacotes marcados no filtro Pacote da barra de ferramentas, o que limita a lista aos ativos desses pacotes.\nA linha só aparece na categoria que tem tais pacotes.";
			d["item.packs.ex.group"] = "por exemplo, você marca um pacote em Vias/Ruas, o original limpa essa marcação nas vias grandes, e ela volta quando você retorna.";
			d["item.packs.ex.menu"] = "os pacotes marcados no menu Vias e os marcados no menu Eletricidade são lembrados à parte.";
			d["item.packs.ex.category"] = "esta linha é o estado do painel de filtros, guardado pelo menu e grupo atuais.";
			d["item.toolMode.label"] = "Ferramentas";
			d["item.toolMode.ex.group"] = "você escolhe Curva simples numa rua pequena e as outras ruas pequenas também usam.";
			d["item.toolMode.ex.menu"] = "todo ativo que você clica no menu Vias está em Curva simples, mas ao mudar para o menu Eletricidade já não.";
			d["item.toolMode.ex.category"] = "por exemplo, mudar para uma via de 6 faixas ainda dá Curva simples, mas uma ponte ou um trilho de metrô voltam ao modo padrão.";
			d["item.elevation.label"] = "Elevação";
			d["item.elevation.desc"] = "Lembra a elevação em que a ferramenta ficou, junto com o passo de elevação, isto é, o quanto um toque desloca.\nA elevação de interseções não é lembrada; um ativo que não aceita o valor mantém a própria elevação.";
			d["item.elevation.ex.group"] = "por exemplo, você sobe uma rua de 2 faixas a 10 m e as outras ruas pequenas também vão a 10 m, enquanto uma via grande não está a 10 m.";
			d["item.elevation.ex.menu"] = "todo ativo que você clica no menu Vias está a 10 m, mas ao mudar para o menu Eletricidade não está a 10 m.";
			d["item.elevation.ex.category"] = "a via de 6 faixas também fica a 10 m, mas uma ponte ou um trilho volta a 0 m.";
			d["item.parallel.label"] = "Modo Paralelo";
			d["item.parallel.desc"] = "Lembra o interruptor Modo Paralelo e o contraste do painel Via paralela: uma segunda linha é criada ao lado da que você desenha.\nDesligado de fábrica, então até você ligá-lo o jogo segue o próprio comportamento.";
			d["item.parallel.ex.group"] = "você liga o Modo Paralelo com 5 m numa rua pequena e as outras ruas pequenas voltam iguais, mas a via grande não.";
			d["item.parallel.ex.menu"] = "todo ativo que você clica no menu Vias é paralelo com 5 m de contraste, mas ao mudar para o menu Eletricidade não.";
			d["item.parallel.ex.category"] = "a via de 6 faixas também fica com 5 m, mas uma ponte ou o metrô desligam o modo.";
			d["item.snap.label"] = "Aderir";
			d["item.snap.desc"] = "Lembra os interruptores da linha Aderir: à geometria existente, a ângulos de 90 graus, aos lados de uma via, a linhas guia e os demais.\nO conjunto depende da ferramenta; a linha Alternar modo Aderir do jogo não é lembrada.";
			d["item.snap.ex.group"] = "você desliga «Aderir aos lados de uma via» numa rua pequena e as outras ruas pequenas também ficam sem ela.";
			d["item.snap.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo conjunto de interruptores da linha Aderir, mas ao mudar para o menu Eletricidade não.";
			d["item.snap.ex.category"] = "a via de 6 faixas mantém o conjunto, mas uma ponte ou um caminho voltam ao padrão.";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Lembra a linha Topografia, ou seja, se as linhas de contorno do terreno são exibidas.";
			d["item.topography.ex.group"] = "por exemplo, você liga a Topografia em uma rua pequena de 2 faixas e as outras ruas pequenas também a têm ligada, enquanto uma via grande não.";
			d["item.topography.ex.menu"] = "todo ativo que você clica no menu Vias mostra as linhas de contorno, mas ao mudar para o menu Eletricidade não.";
			d["item.topography.ex.category"] = "mudar para uma via de 6 faixas ainda as mostra, mas uma ponte ou um cabo elétrico tem a linha desligada e precisa do próprio ajuste.";
			d["item.leftRight.label"] = "Esquerda e Direita";
			d["item.leftRight.desc"] = "Lembra as redes laterais escolhidas nas linhas «Esquerda» e «Direita» do painel do Anarchy, por exemplo Ciclovia, Grama, Quay ou Barreira de som.\nPrecisa do Anarchy (mod 74604 da Paradox).";
			d["item.leftRight.ex.group"] = "você escolhe Ciclovia à direita de uma rua pequena e as outras ruas pequenas também recebem.";
			d["item.leftRight.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo par de seleções laterais, mas ao mudar para o menu Eletricidade não.";
			d["item.leftRight.ex.category"] = "a via de 6 faixas também tem Ciclovia à direita, mas uma ponte ou um trilho duplo não.";
			d["item.general.label"] = "Geral";
			d["item.general.desc"] = "Lembra os interruptores da linha «Geral» do painel do Anarchy: Solo, Elevated, Tunnel, Inclinação constante, Wide Median e Expanded Elevation Range.\nPrecisa do Anarchy (mod 74604 da Paradox).";
			d["item.general.ex.group"] = "você escolhe Tunnel numa rua pequena e as outras ruas pequenas também viram túneis.";
			d["item.general.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo conjunto de interruptores da linha Geral, mas ao mudar para o menu Eletricidade não.";
			d["item.general.ex.category"] = "a via de 6 faixas também dá Tunnel, mas uma ponte ou o metrô ficam sem nada marcado.";
			d["item.underground.label"] = "Modo Subterrâneo";
			d["item.underground.desc"] = "Lembra o interruptor Modo Subterrâneo, que decide se você constrói abaixo ou acima do solo.\nDesligado de fábrica, então até você ligá-lo o jogo segue o próprio comportamento.";
			d["item.underground.ex.group"] = "você liga o Modo Subterrâneo numa rua pequena e as outras ruas pequenas também vão para o subsolo.";
			d["item.underground.ex.menu"] = "todo ativo que você clica no menu Vias é construído debaixo da terra, mas ao mudar para o menu Eletricidade não.";
			d["item.underground.ex.category"] = "a via de 6 faixas também vai para o subsolo, mas uma ponte ou um trilho duplo ficam na superfície.";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（pt-BR）====================
		d["enabled.label"] = "Ativar o mod";
		d["enabled.desc"] = "Desligado de fábrica. Ligado: as definições abaixo valem, e as das ferramentas são gravadas e voltam ao carregar um jogo.\nDesligado: o mod não faz nada e tudo funciona como no jogo.";
		d["domain.asset.desc"] = "Ligado: as definições das ferramentas desta página são gravadas em tempo real e voltam ao carregar um jogo.\nDesligado: nada é gravado nem restaurado, e o âmbito de partilha não pode ser definido.";
		d["domain.asset.label"] = "Lembrar as ferramentas da página Ativos";
		d["domain.function.desc"] = "Ligado: as definições das ferramentas desta página são gravadas em tempo real e voltam ao carregar um jogo.\nDesligado: nada é gravado nem restaurado, e o âmbito de partilha não pode ser definido.";
		d["domain.function.label"] = "Lembrar as ferramentas da página Funções";
		d["domain.item.desc"] = "Ligado: as definições das ferramentas desta página são gravadas em tempo real e voltam ao carregar um jogo.\nDesligado: nada é gravado nem restaurado, e o âmbito de partilha não pode ser definido.";
		d["domain.item.label"] = "Lembrar as ferramentas da página Acessórios";
		d["group.domain"] = "Interruptores por página";
		d["item.color.label"] = "Cor";
		d["item.decoration.desc"] = "Lembra o interruptor «Preservar idade»: as árvores param de crescer e de morrer de velhas.\nA linha só aparece nas árvores.";
			d["item.i.anarchy.label"] = "Anarchy";
			d["item.brushType.label"] = "Tipo de pincel";
			d["item.brushType.desc"] = "Lembra o tipo de pincel: circular, quadrado, com borda suave.\nA linha só existe nas ferramentas que pintam.";
			d["item.brushSize.label"] = "Tamanho do pincel";
			d["item.brushSize.desc"] = "Lembra até onde uma passada do pincel alcança.\nA linha só existe nas ferramentas que pintam.";
			d["item.brushStrength.label"] = "Força do pincel";
			d["item.brushStrength.desc"] = "Lembra o quanto uma passada do pincel muda.\nA linha só existe nas ferramentas que pintam.";
			d["item.brushHeight.label"] = "Altura do alvo";
			d["item.brushHeight.desc"] = "Lembra a altura do alvo, em que altura o terreno fica depois de nivelado ou transformado em declive.\nA linha só existe nas ferramentas de nivelamento e de declive.";
			d["item.distance.label"] = "Distância";
			d["item.distance.desc"] = "Lembra a que distância ficam os objetos colocados ao longo de uma linha ou de uma curva.\nA linha só existe nos modos Linha e Curva.";
			d["item.ageMask.label"] = "Idade";
			d["item.ageMask.desc"] = "Lembra os estágios de idade marcados na linha «Idade»: muda, jovem, madura e anciã, vários ao mesmo tempo.\nA linha só aparece nas árvores.";
			d["item.anarchyObjectElevation.label"] = "Elevação do objeto";
			d["item.anarchyObjectElevation.desc"] = "Guarda os controles de altura que o Anarchy acrescenta ao painel: bloqueio de transformação, altura, variação de altura e o passo para subir ou descer.\nFicam na mesma fila, por isso são guardados e partilhados em conjunto.";
		d["item.decoration.label"] = "Preservar idade";
		d["item.f.snap.desc"] = "Lembra os interruptores da linha Aderir nos painéis de funções, por exemplo a ângulos de 90 graus, à grade, a vias ou as linhas de contorno.\nQuais aparecem depende da ferramenta.";
		d["item.f.topography.desc"] = "Lembra se os painéis de funções exibem as linhas de contorno do terreno.";
		d["item.f.underground.desc"] = "Lembra o interruptor Modo Subterrâneo nos painéis de funções.";
		d["item.i.snap.desc"] = "Lembra os interruptores da linha Aderir nos painéis de acessórios, por exemplo à superfície de um objeto, à geometria próxima ou Remover apenas o tipo correspondente.\nQuais aparecem depende do acessório.";
		d["item.i.toolMode.desc"] = "Lembra o modo da ferramenta nos painéis de acessórios: Colocar um, Colocar vários, Linha, Curva, Carimbo de objetos ou Pintar.\nO conjunto depende do acessório.";
		d["item.i.topography.desc"] = "Lembra se os painéis de acessórios exibem as linhas de contorno do terreno.";
		d["item.i.underground.desc"] = "Lembra se você coloca os acessórios abaixo ou acima do solo.";
		d["scope.category.function"] = "Mesmo tipo";
		d["scope.category.item"] = "Mesmo tipo de acessório";
		d["tab.assets"] = "Ativos";
		d["tab.functions"] = "Funções";
		d["tab.items"] = "Acessórios";
		d["tab.master"] = "Interruptor geral";
			d["scope.line.category"] = "Mesmo tipo de ativo: agrupamos pelo que o ativo atende e cada tipo partilha um valor. {0}";
			d["ex.net.group"] = "Por exemplo, todas as vias do grupo Ruas partilham um valor; Vias grandes é outro grupo.";
			d["ex.net.menu"] = "Por exemplo, todas as vias do menu Vias partilham um valor; o menu Eletricidade não.";
			d["ex.net.category"] = "Por exemplo, vias com qualquer número de faixas são um único tipo; pontes e vias férreas são tipos à parte.";
			d["ex.zone.group"] = "Por exemplo, a habitação de baixa, média e alta densidade de Zonas residenciais forma um grupo; Zonas comerciais é outro grupo, embora ambos estejam no mesmo menu.";
			d["ex.zone.menu"] = "Por exemplo, Zonas residenciais, Zonas comerciais, Zonas industriais e Zonas de escritórios partilham um valor; outro menu não.";
			d["ex.route.group"] = "Por exemplo, as linhas são agrupadas por meio de transporte (estrada, elétrico, comboio, metro e assim por diante); uma linha de outro meio é outro grupo.";
			d["ex.route.menu"] = "Por exemplo, todas as linhas do menu Transporte partilham um valor; outro menu não.";
			d["ex.terrain.group"] = "A terraformação só tem um grupo, por isso Mesmo grupo e Mesmo menu escolhem exatamente as mesmas ferramentas.";
			d["ex.terrain.menu"] = "A terraformação só tem um grupo, por isso Mesmo menu e Mesmo grupo escolhem exatamente as mesmas ferramentas.";
			d["ex.prop.group"] = "Por exemplo, o menu Paisagismo divide os objetos em Natureza, Parque, residencial, comercial, industrial, detritos, decalques e vasos; uma categoria partilha um valor e outra é outro grupo.";
			d["ex.prop.menu"] = "Todos os objetos e árvores do menu Paisagismo partilham um valor.";
			d["ex.tree.group"] = "Todas as árvores estão no grupo Vegetação do menu Paisagismo, por isso Mesmo grupo e Mesmo menu escolhem as mesmas árvores.";
			d["ex.tree.menu"] = "Todas as árvores estão no grupo Vegetação do menu Paisagismo, por isso Mesmo menu e Mesmo grupo escolhem as mesmas árvores.";
			d["item.color.desc"] = "Guarda a fila Cor das ferramentas de linha: a cor com que a linha é desenhada.";
			d["item.f.snap.desc"] = "Guarda os interruptores da fila Aderir nas ferramentas de função; as opções dependem da ferramenta.\nO botão do jogo para ligar ou desligar todos os ajustes é um interruptor global e não é guardado.";
			d["item.f.topography.desc"] = "Guarda a fila Topografia nas ferramentas de zonas, terraformação e outras: se as curvas de nível são visíveis.";
			d["item.f.underground.desc"] = "Guarda a fila Modo Subterrâneo nas ferramentas de área, linha, demolição e objetos.";
			d["item.i.topography.desc"] = "Guarda a fila Topografia nos painéis de objetos.";
			d["scope.desc.function"] = "Até que ponto o valor deste item é partilhado.";
			d["scope.desc.item"] = "Até que ponto o valor deste item é partilhado.";
			d["scope.line.shared"] = "Global: todos os edifícios e serviços que mostram este item usam a mesma definição.";
			d["scope.line.shared.function"] = "Global: todas as ferramentas de função que mostram este item usam a mesma definição.";
			d["scope.line.shared.item"] = "Global: todos os acessórios que mostram este item usam a mesma definição.";
			d["scope.line.unique"] = "Sem partilhar: cada edifício e serviço que mostra este item é definido em separado.";
			d["scope.line.unique.function"] = "Sem partilhar: cada ferramenta de função que mostra este item é definida em separado.";
			d["scope.line.unique.item"] = "Sem partilhar: cada acessório que mostra este item é definido em separado.";
			d["scope.note"] = "Os âmbitos de edifícios e serviços, funções e acessórios são independentes.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Esta linha aplica-se apenas a {0}.";
			d["fam.net.desc"] = "A família das estradas: estradas, vias, tubagens e linhas elétricas contam todas.";
			d["fam.net"] = "estradas";
			d["fam.assetObj"] = "edifícios";
			d["fam.funcObj"] = "marcadores";
			d["fam.zone"] = "zonas";
			d["fam.area"] = "ferramentas de área";
			d["item.toolMode.desc"] = "Memoriza o modo escolhido nestas ferramentas.";
			d["item.f.toolMode.desc"] = "Memoriza a linha Modo da ferramenta nas ferramentas de zona: Preenchimento, Seleção e Pintura.";
			return d;
		}

		// ======================= ru-RU =======================

		private static Dictionary<string, string> Ru()
		{
			Dictionary<string, string> d = Frames("ru-RU");
			d["mod.name"] = "Память режима инструмента";
			d["tab.mod"] = "Настройки режима инструмента";
			d["tab.about"] = "О моде";
			d["group.master"] = "Память режима инструмента";
			d["group.official"] = "Настройки официальных инструментов";
			d["group.anarchy"] = "Настройки инструментов Anarchy";
			d["group.reset"] = "Управление памятью";
			d["group.about"] = "Сведения и ссылки";

			d["scope.label"] = "Область общего доступа";
			d["scope.desc"] = "Насколько широко разделяется значение этого пункта; в скобках видно, как поступает сама игра.\nОбласть действует только при включённом переключателе пункта — выключенный ведёт себя как в игре.";
			d["scope.line.group"] = "Та же группа: инструменты в одной группе панели меню используют одно значение. {0}";
			d["scope.line.menu"] = "То же меню: все инструменты меню используют одно значение. {0}";
			d[kScopeLineCategory] = "Тот же тип объекта: группируем по тому, кому служит объект, поэтому маленькая и большая парковка — один тип, а мост — отдельный: {0}";

			d[kScopeGroup] = "Та же группа";
			d[kScopeMenu] = "То же меню";
			d[kScopeCategory] = "Тот же тип объекта";
			d[kScopeGlobalShared] = "Глобально";
			d[kScopeGlobalUnique] = "Без разделения";
			d[kTagVanilla] = " (оригинал)";
			d[kTagRecommended] = " (рекомендуется)";
			d[kTagRecVanilla] = " (рекомендуется, оригинал)";

			d["reset.label"] = "Сбросить память";
			d["reset.desc"] = "В игре: стирает память этого сохранения и возвращает инструменты к исходному состоянию при загрузке. В главном меню или редакторе: стирает память всех сохранений.";
			d["reset.warn"] = "Отменить нельзя.";
			d["reset.confirm"] = "Сбросить запомненные настройки инструментов?";
			d["resetall.label"] = "Сбросить все настройки";
			d["resetall.desc"] = "Возвращает все настройки этого мода к заводским значениям: главный переключатель выключается, переключатели страниц включаются, а каждый пункт возвращается к заводскому состоянию.\nЗаписанная память сохранений не затрагивается.";
			d["resetall.warn"] = "Отменить нельзя.";
			d["resetall.confirm"] = "Сбросить все настройки этого мода к заводским значениям?";
			d["folder.label"] = "Управление файлами памяти всех сохранений";
			d["folder.desc"] = "Открывает папку с файлами памяти, по одному на сохранение, с названием как у сохранения в списке «Загрузить игру».\nФайл используется, только когда имена совпадают, поэтому переименование файла копирует память между сохранениями.";

			d["about.version"] = "Версия мода";
			d["about.author"] = "Автор";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Поддержать автора на Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Открыть тему на форуме Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Открыть сайт серии Rainbow.";

			d["item.anarchy.desc"] = "Запоминает переключатель мода Anarchy.\nОн запоминается только на инструментах, у которых этот переключатель действительно есть.\nБез мода Anarchy этот пункт не появляется.";
			d["item.anarchy.ex.group"] = "вы включаете Anarchy на небольшой дороге, и на других небольших дорогах он тоже включён.";
			d["item.anarchy.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», Anarchy включён, но в меню «Электричество» уже нет.";
			d["item.anarchy.ex.category"] = "переход на шестиполосную дорогу его сохраняет, но мост и метро требуют своей настройки.";
			d["item.themes.label"] = "Тема";
			d["item.themes.desc"] = "Запоминает отмеченные темы в фильтре «Тема» на панели инструментов: список ограничивается объектами с этими темами.\nСтрока появляется только в категории, где темы действительно есть.";
			d["item.themes.ex.group"] = "например, отметьте тему только в «Дороги/Небольшие дороги», и большие дороги по этой теме фильтроваться не будут.";
			d["item.themes.ex.menu"] = "тема, отмеченная в меню «Дороги», перестаёт действовать в меню «Электричество» и возвращается, когда вы снова открываете «Дороги».";
			d["item.themes.ex.category"] = "эта строка — состояние самой панели фильтров, поэтому она запоминается по текущему меню и группе, а не по типу объекта.";
			d["item.packs.label"] = "Набор";
			d["item.packs.desc"] = "Запоминает отмеченные наборы в фильтре «Набор» на панели инструментов: список ограничивается объектами из этих наборов.\nСтрока появляется только в категории, где такие наборы есть.";
			d["item.packs.ex.group"] = "например, отметьте набор в «Дороги/Небольшие дороги», оригинал снимет эту отметку на больших дорогах, а при возврате она вернётся.";
			d["item.packs.ex.menu"] = "наборы, отмеченные в меню «Дороги», и наборы, отмеченные в меню «Электричество», запоминаются раздельно.";
			d["item.packs.ex.category"] = "эта строка — состояние самой панели фильтров, она запоминается по текущему меню и группе; оригинал очищает её при каждой смене группы.";
			d["item.toolMode.label"] = "Режим инструмента";
			d["item.toolMode.ex.group"] = "вы выбираете простую кривую для небольшой дороги, и другие небольшие дороги тоже.";
			d["item.toolMode.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», стоит режим «Простая кривая», но в меню «Электричество» уже нет.";
			d["item.toolMode.ex.category"] = "например, переход на шестиполосную дорогу тоже даёт простую кривую, но мост или тоннель метро возвращаются к своему режиму по умолчанию.";
			d["item.elevation.label"] = "Эстакада";
			d["item.elevation.desc"] = "Запоминает высоту, на которой оставлен инструмент, вместе с шагом подъёма — насколько сдвигает одно нажатие.\nВысота перекрёстков не запоминается; актив, которому значение недоступно, сохраняет свою высоту.";
			d["item.elevation.ex.group"] = "например, поднимите двухполосную дорогу до 10 м, и другие небольшие дороги тоже поднимутся до 10 м, а большая дорога на 10 м не будет.";
			d["item.elevation.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», высота 10 м, но в меню «Электричество» уже не 10 м.";
			d["item.elevation.ex.category"] = "шестиполосная дорога тоже на 10 м, но мост и железнодорожные пути возвращаются к 0 м.";
			d["item.parallel.label"] = "Режим параллели";
			d["item.parallel.desc"] = "Запоминает переключатель «Режим параллели» и смещение на панели «Параллельная дорога»: вторая линия строится рядом с проводимой.\nПо умолчанию выключено, поэтому до включения игра работает по-своему.";
			d["item.parallel.ex.group"] = "вы включаете режим параллели со смещением 5 м на небольшой дороге, и другие небольшие дороги тоже.";
			d["item.parallel.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», параллель включена со смещением 5 м, но в меню «Электричество» уже нет.";
			d["item.parallel.ex.category"] = "шестиполосная дорога тоже со смещением 5 м, но мост и метро выключают режим.";
			d["item.snap.label"] = "Привязка";
			d["item.snap.desc"] = "Запоминает переключатели строки «Привязка»: к форме, к прямым углам, к обочине дороги, к направляющим и остальные.\nНабор зависит от инструмента; общая кнопка включения привязки из игры не запоминается.";
			d["item.snap.ex.group"] = "вы выключаете привязку к обочине дороги, и на других небольших дорогах она тоже выключена.";
			d["item.snap.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же набор переключателей привязки, но в меню «Электричество» уже нет.";
			d["item.snap.ex.category"] = "шестиполосная дорога сохраняет набор, но мост и дорожка возвращаются к значениям по умолчанию.";
			d["item.topography.label"] = "Топография";
			d["item.topography.desc"] = "Запоминает строку «Топография», то есть показываются ли контурные линии рельефа.";
			d["item.topography.ex.group"] = "например, включите топографию на небольшой двухполосной дороге, и на других небольших дорогах она тоже будет включена, а на большой дороге — нет.";
			d["item.topography.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», показаны контурные линии, но в меню «Электричество» уже нет.";
			d["item.topography.ex.category"] = "переход на шестиполосную дорогу тоже показывает их, но у моста или электрического кабеля строка выключена и требует собственной настройки.";
			d["item.leftRight.label"] = "Лево и Право";
			d["item.leftRight.desc"] = "Запоминает улучшения сети в строках «Лево» и «Право» панели Anarchy, например «Велосипедная полоса», «Трава», «Набережная» или «Шумозащитный экран».\nТребует Anarchy (мод Paradox 74604).";
			d["item.leftRight.ex.group"] = "вы выбираете велосипедную полосу справа, и другие небольшие дороги тоже её получают.";
			d["item.leftRight.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется та же пара боковых выборов, но в меню «Электричество» уже нет.";
			d["item.leftRight.ex.category"] = "шестиполосная дорога тоже с велосипедной полосой справа, но мост и двойные пути — нет.";
			d["item.general.label"] = "Общее";
			d["item.general.desc"] = "Запоминает переключатели строки «Общее» панели Anarchy: «Земля», «Эстакада», «Туннель», «Постоянный наклон», «Широкая медиана» и «Расширенный диапазон высот».\nТребует Anarchy (мод Paradox 74604).";
			d["item.general.ex.group"] = "вы выбираете «Туннель» для небольшой дороги, и другие небольшие дороги тоже уходят под землю.";
			d["item.general.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же набор переключателей строки «Общее», но в меню «Электричество» уже нет.";
			d["item.general.ex.category"] = "шестиполосная дорога тоже даёт «Туннель», но мост и метро остаются без отметок.";
			d["item.underground.label"] = "Режим тоннеля";
			d["item.underground.desc"] = "Запоминает переключатель «Режим тоннеля», который решает, строите вы под землёй или на ней.\nПо умолчанию выключено, поэтому до включения игра работает по-своему.";
			d["item.underground.ex.group"] = "вы включаете режим тоннеля на небольшой дороге, и другие небольшие дороги тоже уходят под землю.";
			d["item.underground.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», всё строится под землёй, но в меню «Электричество» уже нет.";
			d["item.underground.ex.category"] = "шестиполосная дорога тоже уходит под землю, но мост и двойные пути остаются на поверхности.";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行

		// ==================== v0.7.0 新增 / 重写（ru-RU）====================
		d["tab.master"] = "Главный переключатель";
		d["tab.assets"] = "Активы";
		d["tab.functions"] = "Функции";
		d["tab.items"] = "Объекты";
		d["group.domain"] = "Постраничные переключатели";
		d["domain.asset.label"] = "Запоминать инструменты страницы «Активы»";
		d["domain.asset.desc"] = "Включён: настройки инструментов этой страницы записываются сразу и возвращаются при загрузке сохранения.\nВыключен: ничего не записывается и не восстанавливается, а область общего доступа задать нельзя.";
		d["domain.function.label"] = "Запоминать инструменты страницы «Функции»";
		d["domain.function.desc"] = "Включён: настройки инструментов этой страницы записываются сразу и возвращаются при загрузке сохранения.\nВыключен: ничего не записывается и не восстанавливается, а область общего доступа задать нельзя.";
		d["domain.item.label"] = "Запоминать инструменты страницы «Объекты»";
		d["domain.item.desc"] = "Включён: настройки инструментов этой страницы записываются сразу и возвращаются при загрузке сохранения.\nВыключен: ничего не записывается и не восстанавливается, а область общего доступа задать нельзя.";
		d["item.color.label"] = "Цвет";
		d["item.decoration.label"] = "Заморозить возраст";
		d["item.decoration.desc"] = "Запоминает переключатель «Заморозить возраст»: деревья перестают расти и умирать от старости.\nСтрока появляется только у деревьев.";
			d["item.i.anarchy.label"] = "Anarchy";
			d["item.brushType.label"] = "Тип кисти";
			d["item.brushType.desc"] = "Запоминает тип кисти: круглая, квадратная, с мягким краем.\nСтрока есть только у инструментов, которыми можно закрашивать.";
			d["item.brushSize.label"] = "Размер кисти";
			d["item.brushSize.desc"] = "Запоминает, какую площадь охватывает одно движение кисти.\nСтрока есть только у инструментов, которыми можно закрашивать.";
			d["item.brushStrength.label"] = "Нажим кисти";
			d["item.brushStrength.desc"] = "Запоминает, насколько сильно одно движение кисти меняет результат.\nСтрока есть только у инструментов, которыми можно закрашивать.";
			d["item.brushHeight.label"] = "Целевая высота";
			d["item.brushHeight.desc"] = "Запоминает целевую высоту — на какой высоте окажется земля после выравнивания или склона.\nСтрока есть только у инструментов выравнивания и склона.";
			d["item.distance.label"] = "Расстояние";
			d["item.distance.desc"] = "Запоминает, на каком расстоянии друг от друга стоят объекты вдоль линии или кривой.\nСтрока есть только в режимах «Линия» и «Кривая».";
			d["item.ageMask.label"] = "Возраст";
			d["item.ageMask.desc"] = "Запоминает отмеченные стадии возраста в строке «Возраст»: саженец, молодое, зрелое и старое — можно несколько.\nСтрока появляется только у деревьев.";
			d["item.anarchyObjectElevation.label"] = "Высота объекта";
			d["item.anarchyObjectElevation.desc"] = "Запоминает регуляторы высоты, которые Anarchy добавляет на панель: блокировка трансформации, высота, разброс высоты и шаг подъёма/опускания.\nОни стоят в одном ряду, поэтому запоминаются и используются вместе.";
		d["item.f.snap.desc"] = "Запоминает переключатели строки «Привязка» на панелях функций: к прямым углам, к сетке, к дорогам, контурные линии и другие.\nКакие видны, зависит от инструмента.";
		d["item.f.topography.desc"] = "Запоминает, показывают ли панели функций контурные линии рельефа.";
		d["item.f.underground.desc"] = "Запоминает переключатель «Режим тоннеля» на панелях функций.";
		d["item.i.snap.desc"] = "Запоминает переключатели строки «Привязка» на панелях объектов: к поверхности объекта, к ближайшим формам, «Удалить только определенный тип» и другие.\nКакие видны, зависит от объекта.";
		d["item.i.toolMode.desc"] = "Запоминает режим инструмента на панелях объектов: «Разместить один», «Разместить несколько», «Линия», «Кривая», «Штамп объекта» или «Кисть».\nНабор зависит от объекта.";
		d["item.i.topography.desc"] = "Запоминает, показывают ли панели объектов контурные линии рельефа.";
		d["item.i.underground.desc"] = "Запоминает, размещаете вы объекты под землёй или над ней.";
		d["scope.category.function"] = "Тот же тип";
		d["scope.category.item"] = "Тот же тип объекта";
		d["enabled.label"] = "Включить мод";
		d["enabled.desc"] = "По умолчанию выключен. Включён: настройки ниже действуют, а настройки инструментов записываются и возвращаются при загрузке.\nВыключен: мод ничего не делает, всё работает как в игре.";
			d["scope.line.category"] = "Тот же тип объекта: объекты группируются по тому, кому служат, и одна группа использует одно значение. {0}";
			d["ex.net.group"] = "Например, все дороги группы Небольшие дороги используют одно значение; Большие дороги — это другая группа.";
			d["ex.net.menu"] = "Например, все дороги меню Дороги используют одно значение; в меню «Электричество» это не так.";
			d["ex.net.category"] = "Например, дороги с любым числом полос — один тип; мосты и железнодорожные пути — отдельные типы.";
			d["ex.zone.group"] = "Например, малоэтажная, среднеэтажная и высотная застройка в Жилые зоны — одна группа; Торговые зоны — другая группа, хотя обе в одном меню.";
			d["ex.zone.menu"] = "Например, Жилые зоны, Торговые зоны, Промышленные зоны и Офисные зоны используют одно значение; другое меню — нет.";
			d["ex.route.group"] = "Например, линии группируются по виду транспорта (дорога, трамвай, поезд, метро и так далее); линия другого вида — другая группа.";
			d["ex.route.menu"] = "Например, все линии в меню Общественный транспорт используют одно значение; другое меню — нет.";
			d["ex.terrain.group"] = "У терраформирования только одна группа, поэтому «Та же группа» и «То же меню» выбирают одни и те же инструменты.";
			d["ex.terrain.menu"] = "У терраформирования только одна группа, поэтому «То же меню» и «Та же группа» выбирают одни и те же инструменты.";
			d["ex.prop.group"] = "Например, меню Ландшафтный дизайн делит объекты на Природа, Парк, жилые, торговые, промышленные, обломки, наклейки и кашпо; одна категория использует одно значение, другая — другая группа.";
			d["ex.prop.menu"] = "Все объекты и деревья в меню Ландшафтный дизайн используют одно значение.";
			d["ex.tree.group"] = "Все деревья находятся в группе Флора меню Ландшафтный дизайн, поэтому «Та же группа» и «То же меню» выбирают одни и те же деревья.";
			d["ex.tree.menu"] = "Все деревья находятся в группе Флора меню Ландшафтный дизайн, поэтому «То же меню» и «Та же группа» выбирают одни и те же деревья.";
			d["item.color.desc"] = "Запоминает строку «Цвет» у инструментов линий: цвет, которым рисуется линия.";
			d["item.f.snap.desc"] = "Запоминает переключатели строки «Привязка» у функциональных инструментов; набор переключателей зависит от инструмента.\nКнопка игры «включить/выключить все привязки» — это общий переключатель, он не запоминается.";
			d["item.f.topography.desc"] = "Запоминает строку «Топография» у инструментов зон, терраформирования и других: видны ли контурные линии рельефа.";
			d["item.f.underground.desc"] = "Запоминает строку «Режим тоннеля» у инструментов областей, линий, сноса и объектов.";
			d["item.i.topography.desc"] = "Запоминает строку «Топография» на панелях объектов.";
			d["scope.desc.function"] = "Насколько широко разделяется значение этого пункта.";
			d["scope.desc.item"] = "Насколько широко разделяется значение этого пункта.";
			d["scope.line.shared"] = "Глобально: все здания и службы, которые показывают этот пункт, используют одну настройку.";
			d["scope.line.shared.function"] = "Глобально: все функции, которые показывают этот пункт, используют одну настройку.";
			d["scope.line.shared.item"] = "Глобально: все объекты, которые показывают этот пункт, используют одну настройку.";
			d["scope.line.unique"] = "Без разделения: каждое здание и каждая служба, которые показывают этот пункт, настраиваются отдельно.";
			d["scope.line.unique.function"] = "Без разделения: каждая функция, которая показывает этот пункт, настраивается отдельно.";
			d["scope.line.unique.item"] = "Без разделения: каждый объект, который показывает этот пункт, настраивается отдельно.";
			d["scope.note"] = "Области для зданий и служб, функций и объектов независимы.";
			d["label.family"] = "{0} ({1})";
			d["desc.family"] = "Эта строка относится только к: {0}.";
			d["fam.net.desc"] = "Семейство дорог: дороги, пути, трубы и линии электропередачи — всё это входит в него.";
			d["fam.net"] = "дороги";
			d["fam.assetObj"] = "здания";
			d["fam.funcObj"] = "маркеры";
			d["fam.zone"] = "зоны";
			d["fam.area"] = "инструменты области";
			d["item.toolMode.desc"] = "Запоминает режим, выбранный в этих инструментах.";
			d["item.f.toolMode.desc"] = "Запоминает строку «Режим инструмента» у инструментов зон: Заливка, Выделение и Кисть.";
			return d;
		}
	}
}
