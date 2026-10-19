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
			d["scope.desc"] = "Jak szeroko współdzielona jest wartość tej pozycji. Nawiasy na liście rozwijanej pokazują, co robi gra oryginalna i co polecamy.";
			d["scope.line.group"] = "Ta sama grupa: {0}";
			d["scope.line.menu"] = "To samo menu: {0}";
			d[kScopeLineCategory] = "Ten sam typ zasobu: zasoby grupujemy według tego, komu faktycznie służą, więc liczba pasów, szerokość, poziom, rozmiar i kategoria na pasku narzędzi nie mają znaczenia (mały parking i duży parking to ten sam typ), za to mosty, nabrzeża i węzły drogowe tworzą osobne typy, inne niż drogi, czyli {0}";
			d["scope.line.shared"] = "Globalnie: każdy zasób i każda funkcja obsługujące tę pozycję współdzielą jedną wartość, ale zasoby i funkcje nie współdzielą jej między sobą";
			d["scope.line.unique"] = "Bez współdzielenia: każdy zasób i każda funkcja obsługujące tę pozycję są ustawiane zupełnie niezależnie";

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
			d["resetall.desc"] = "Przywraca każdą opcję tego moda do zalecanej wartości domyślnej: przełącznik główny znowu się włącza, a stan włączenia i zakres współdzielenia każdej pozycji też zostają przywrócone. Pamięć już zapisana dla zapisów gry pozostaje nienaruszona.";
			d["resetall.warn"] = "Nie można cofnąć.";
			d["resetall.confirm"] = "Zresetować wszystkie ustawienia tego moda do wartości zalecanych?";
			d["folder.label"] = "Zarządzaj plikami pamięci wszystkich zapisów";
			d["folder.desc"] = "Otwiera lokalny folder z plikami pamięci, jednym na zapis, a nazwa pliku jest dokładnie taka jak nazwa zapisu widziana na liście «Wczytaj grę». Pamięć jest zawsze odczytywana po nazwie zapisu: plik jest używany tylko wtedy, gdy nazwa zapisu i nazwa pliku json są takie same, więc zmiana nazwy pliku to sposób na skopiowanie pamięci z jednego zapisu do drugiego. Po wczytaniu automatycznego zapisu jego pamięć nadal trafia do pliku pierwotnego zapisu i go nadpisuje, ponieważ automatyczny zapis za każdym razem dostaje nową nazwę i to nie ona może być nazwą pliku. Tymczasowo pod nazwą miasta pamięć trzyma się tylko wtedy, gdy nie da się rozpoznać pierwotnego zapisu; po ręcznym zapisaniu gry plik sam przyjmie właściwą nazwę zapisu.";

			d["about.version"] = "Wersja moda";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Wesprzyj autora na Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Otwórz wątek na forum Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Otwórz stronę serii Rainbow.";

			d["item.anarchy.desc"] = "Zapamiętuje stan przełącznika Anarchy dodawanego przez mod Anarchy. Anarchy jest współdzielony w całej grze, więc ta pozycja jest globalna; po włączeniu możesz zmienić zakres współdzielenia. Bez moda Anarchy ta pozycja nie jest wyświetlana.";
			d["item.anarchy.ex.group"] = "na przykład włączysz Anarchy dla małej dwupasmowej drogi i inne małe drogi też będą mieć Anarchy włączone, a duża droga nie";
			d["item.anarchy.ex.menu"] = "każdy zasób kliknięty w menu Drogi ma włączone Anarchy, ale po przejściu do menu Elektryczność już nie";
			d["item.anarchy.ex.category"] = "przełączenie na drogę sześciopasmową nadal pozostawia go włączonym, ale most albo podwójne tory kolejki metra wyłączają go i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.themes.label"] = "Motyw";
			d["item.themes.desc"] = "Zapamiętuje, które pozycje są zaznaczone w filtrze Motyw na pasku narzędzi. Ten wiersz pojawia się tylko wtedy, gdy bieżąca kategoria naprawdę zawiera zasoby używające motywów, a zaznaczenie jednego ogranicza listę na pasku narzędzi do zasobów obsługujących te motywy. Oryginał resetuje ten filtr do domyślnego motywu tylko przy wczytaniu zapisu, więc zalecamy, aby ta pozycja pozostała wyłączona.";
			d["item.themes.ex.group"] = "na przykład zaznaczysz motyw tylko w Drogi/Małe drogi, więc duże drogi już według niego nie filtrują";
			d["item.themes.ex.menu"] = "motyw zaznaczony w menu Drogi przestaje działać w menu Elektryczność i wraca, gdy tam wrócisz";
			d["item.themes.ex.category"] = "ten wiersz to stan samego panelu filtrowania, więc jest zapamiętywany pod aktualnym menu i grupą, a nie według typu zasobu";
			d["item.packs.label"] = "Pakiet";
			d["item.packs.desc"] = "Zapamiętuje, które pozycje są zaznaczone w filtrze Pakiet na pasku narzędzi; zaznaczenie jednego ogranicza listę na pasku narzędzi do zasobów należących do tych pakietów. Oryginał czyści ten filtr przy każdej zmianie menu lub kategorii, więc zapamiętywanie go dla tego samego menu i kategorii odpowiada temu, co rzeczywiście widzisz, i tę pozycję też zalecamy zostawić wyłączoną.";
			d["item.packs.ex.group"] = "na przykład zaznaczysz pakiet w Drogi/Małe drogi, oryginał wyczyści go przy przejściu na duże drogi, a po powrocie znów jest zaznaczony";
			d["item.packs.ex.menu"] = "pakiety zaznaczone w menu Drogi i pakiety zaznaczone w menu Elektryczność są zapamiętywane osobno";
			d["item.packs.ex.category"] = "ten wiersz to stan samego panelu filtrowania, zapamiętywany pod aktualnym menu i grupą; oryginał czyści go przy każdej zmianie grupy";
			d["item.toolMode.label"] = "Narzędzia";
			d["item.toolMode.desc"] = "Zapamiętuje tryb wybrany dla każdego narzędzia, a opcje różnią się między nimi: drogi, tory i rury oferują Prosta, Jeden zakręt, Dwa zakręty, Ciągła, Siatka, Zastąp, Punkt; budynki, obiekty i drzewa oferują Umieść jeden, Umieść wiele, Linia, Krzywa, Stempel; strefy oferują Wypełnienie, Zaznaczanie obszarowe, Pędzel; obszary oferują Edytuj, Generuj siatkę mapy. Tryby zasobów i tryby funkcji zapamiętywane są osobno, bo ich opcje nie są takie same.";
			d["item.toolMode.ex.group"] = "na przykład ustawisz Jeden zakręt dla małej dwupasmowej drogi i inne małe drogi też będą mieć Jeden zakręt, a duża droga nie";
			d["item.toolMode.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest na trybie Jeden zakręt, ale po przejściu do menu Elektryczność już nie";
			d["item.toolMode.ex.category"] = "przełączenie na drogę sześciopasmową nadal daje Jeden zakręt, ale most albo podwójne tory kolejki metra wracają do swojego trybu domyślnego i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.elevation.label"] = "Wzniesienie";
			d["item.elevation.desc"] = "Zapamiętuje wysokość, na jakiej zostawiono narzędzie, wraz z wynikiem Zwiększ wzniesienie i Zmniejsz wzniesienie. Stopień wzniesienia, czyli o ile przesuwa jedno naciśnięcie Zwiększ wzniesienie albo Zmniejsz wzniesienie (w panelu moda Anarchy ten wiersz nazywa się tak samo), jest zapamiętywany razem z wysokością i używa tego samego zakresu współdzielenia. Wysokość skrzyżowań nie jest objęta. Część zasobów ma własny dopuszczalny zakres wzniesienia: jeśli współdzielona wartość wykracza poza to, na co pozwala wybrany zasób, zasób ten zachowuje wzniesienie możliwe dla niego, a zapamiętana wartość współdzielona pozostaje bez zmian.";
			d["item.elevation.ex.group"] = "na przykład podniesiesz małą dwupasmową drogę do 10 m i inne małe drogi też pójdą na 10 m, a duża droga nie będzie na 10 m";
			d["item.elevation.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest na 10 m, ale po przejściu do menu Elektryczność nie jest na 10 m";
			d["item.elevation.ex.category"] = "przełączenie na drogę sześciopasmową nadal daje 10 m, ale most albo podwójne tory kolejowe wracają do 0 m i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.parallel.label"] = "Tryb równoległy";
			d["item.parallel.desc"] = "Zapamiętuje dwie rzeczy z panelu Równoległa droga: przełącznik Tryb równoległy (Włącz/wyłącz tryb równoległy) oraz odstęp z wiersza «Równoległa kompensacja». Tryb równoległy to zawsze dwie linie: po włączeniu druga linia powstaje obok tej, którą rysujesz, a ustawić możesz tylko to, jak daleko są od siebie, na przykład 1 m czy 2 m. Po wyłączeniu zostaje pojedyncza linia.";
			d["item.parallel.ex.group"] = "na przykład włączysz Tryb równoległy dla małej dwupasmowej drogi i ustawisz kompensację na 5 m, inne małe drogi też wrócą z włączonym trybem i tym samym 5 m, a duża droga nie";
			d["item.parallel.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest równoległy z kompensacją 5 m, ale po przejściu do menu Elektryczność już nie";
			d["item.parallel.ex.category"] = "przełączenie na drogę sześciopasmową nadal pozostawia włączony tryb z kompensacją 5 m, ale most albo podwójne tory kolejki metra wyłączają go i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.snap.label"] = "Przyciąganie";
			d["item.snap.desc"] = "Zapamiętuje każdy przełącznik wiersza Przyciąganie: Przyciągaj do istniejących kształtów, Przyciągaj do wielkości pól w strefach, Przyciągaj do kątów prostych, Przyciągaj do stron drogi, Przyciągaj do dróg, Przyciągaj do strony właściciela, Przyciągaj do boków budynku, Przyciągaj do środka drogi, Przyciągaj do linii brzegowej, Przyciągaj do pobliskich kształtów, Przyciągaj do linii pomocniczych, Przyciągaj do siatki w strefie, Przyciągaj do węzłów, Przyciągaj do powierzchni obiektu, Przyciągaj do góry, Przyciągaj do siatki wysypiska, Wiąże nakładające się obiekty z budynkiem, Usuń tylko pasujący typ, Pokaż linie konturów, Przyciągaj do odległości. Wiersz Wł./wył. przyciąganie z gry to przełącznik zbiorczy i nie jest zapamiętywany.";
			d["item.snap.ex.group"] = "na przykład wyłączysz «Przyciągaj do stron drogi» dla małej dwupasmowej drogi i inne małe drogi też będą to mieć wyłączone, a duża droga nadal będzie mieć wszystko włączone";
			d["item.snap.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego zestawu przełączników przyciągania, ale po przejściu do menu Elektryczność nie";
			d["item.snap.ex.category"] = "przełączenie na drogę sześciopasmową zachowuje ten sam zestaw, ale most albo ścieżka wracają do ustawień domyślnych i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Zapamiętuje wiersz Topografia, czyli czy Pokaż linie konturów jest włączone.";
			d["item.topography.ex.group"] = "na przykład włączysz Topografię dla małej dwupasmowej drogi i inne małe drogi też będą ją mieć włączoną, a duża droga nie";
			d["item.topography.ex.menu"] = "każdy zasób kliknięty w menu Drogi pokazuje linie konturów, ale po przejściu do menu Elektryczność nie";
			d["item.topography.ex.category"] = "przełączenie na drogę sześciopasmową nadal je pokazuje, ale most albo przewód elektryczny mają ten wiersz wyłączony i wymagają własnego ustawienia";
			d["item.leftRight.label"] = "Po lewej i Po prawej";
			d["item.leftRight.desc"] = "Zapamiętuje, które ulepszenia sieci wybrano w wierszach «Po lewej» i «Po prawej» panelu sieci moda Anarchy, na przykład Pas dla rowerów, Drzewa, Trawnik, Nabrzeże, Ściana oporowa czy Bariera akustyczna. Wymaga Anarchy (mod Paradox 74604); bez niego ta pozycja nie jest wyświetlana.";
			d["item.leftRight.ex.group"] = "na przykład wybierzesz Pas dla rowerów po prawej stronie małej dwupasmowej drogi i inne małe drogi też go tam dostaną, a duża droga nie";
			d["item.leftRight.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tej samej pary wyborów stron, ale po przejściu do menu Elektryczność nie";
			d["item.leftRight.ex.category"] = "przełączenie na drogę sześciopasmową nadal daje Pas dla rowerów po prawej, ale most albo podwójne tory kolejowe wracają do braku wyboru i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.general.label"] = "Ogólnie";
			d["item.general.desc"] = "Zapamiętuje przełączniki z wiersza «Ogólnie» panelu sieci moda Anarchy: Podłoże, Podwyższenie, Tunel, Stałe nachylenie, Szeroka mediana i Rozszerzony zasięg wysokości (ten ostatni znosi limit wysokości). Wymaga Anarchy (mod Paradox 74604); bez niego ta pozycja nie jest wyświetlana.";
			d["item.general.ex.group"] = "na przykład wybierzesz Tunel dla małej dwupasmowej drogi i inne małe drogi też zostaną zamienione w tunele, a duża droga nie";
			d["item.general.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego zestawu przełączników wiersza Ogólnie, ale po przejściu do menu Elektryczność nie";
			d["item.general.ex.category"] = "przełączenie na drogę sześciopasmową nadal daje Tunel, ale most albo podwójne tory kolejki metra nie mają nic zaznaczonego i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.underground.label"] = "Tryb podziemny";
			d["item.underground.desc"] = "Zapamiętuje przełącznik Tryb podziemny (Włącz/wyłącz tryb podziemny), który decyduje, czy stawiasz coś pod ziemią, czy nad nią.";
			d["item.underground.ex.group"] = "na przykład włączysz Tryb podziemny dla małej dwupasmowej drogi i inne małe drogi też zejdą pod ziemię, a duża droga nie";
			d["item.underground.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest budowany pod ziemią, ale po przejściu do menu Elektryczność nie";
			d["item.underground.ex.category"] = "przełączenie na drogę sześciopasmową nadal schodzi pod ziemię, ale most albo podwójne tory kolejowe zostają na powierzchni i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";
			d["item.other.label"] = "Inne";
			d["item.other.desc"] = "Zapamiętuje pozostałe wiersze, które panel narzędzia może zapisywać: kolory dróg z wiersza Kolor.";
			d["item.other.ex.group"] = "na przykład ustawisz kolor małej dwupasmowej drogi na niebieski i inne małe drogi też zrobią się niebieskie, a duża droga nie";
			d["item.other.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego koloru, ale po przejściu do menu Elektryczność nie";
			d["item.other.ex.category"] = "przełączenie na drogę sześciopasmową nadal jest niebieskie, ale most albo podwójne tory kolejowe mają własny zapis koloru i wymagają własnego ustawienia, choć wszystkie są w menu Drogi";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行
			d["item.brush.label"] = "Pędzel";

		// ==================== v0.7.0 新增 / 重写（pl-PL）====================
		d["domain.asset.label"] = "Włącz pamięć pozycji narzędzi zasobów";
		d["domain.asset.desc"] = "Obejmuje wszystkie pozycje na stronie Zasoby: zapisywanie i przywracanie przy wczytaniu to jeden przełącznik. Po wyłączeniu cała strona Zasoby jest wyszarzona, narzędzia zachowują się jak w grze oryginalnej, a już zapisane wartości pozostają nienaruszone w pliku, więc ponowne włączenie je przywraca.";
		d["domain.function.label"] = "Włącz pamięć pozycji narzędzi funkcji";
		d["domain.function.desc"] = "Obejmuje wszystkie pozycje na stronie Funkcje, czyli sześć grup funkcji: Strefy, narzędzia obszarów, wszystkie narzędzia tras, Terraformowanie, Przestrzenie i obszary oraz Znacznik i prefaby obiektów. Po wyłączeniu cała strona Funkcje jest wyszarzona, narzędzia zachowują się jak w grze oryginalnej, a już zapisane wartości pozostają nienaruszone w pliku, więc ponowne włączenie je przywraca.";
		d["domain.item.label"] = "Włącz pamięć pozycji narzędzi rekwizytów";
		d["domain.item.desc"] = "Obejmuje wszystkie pozycje na stronie Rekwizyty, czyli osiem kategorii rekwizytów menu Kształtowanie krajobrazu: Natura, Park, Rekwizyty (strefa mieszkalna), Rekwizyty (strefa komercyjna), Rekwizyty (strefa przemysłowa), Gruz, Naklejki i Rośliny doniczkowe. Po wyłączeniu cała strona Rekwizyty jest wyszarzona, narzędzia zachowują się jak w grze oryginalnej, a już zapisane wartości pozostają nienaruszone w pliku, więc ponowne włączenie je przywraca.";
		d["group.domain"] = "Przełączniki stron";
		d["item.color.label"] = "Kolor";
		d["item.decoration.label"] = "Zachowaj wiek";
		d["item.decoration.desc"] = "Zapamiętuje przełącznik «Zachowaj wiek»: gdy jest włączony, drzewa, które zamalujesz pędzlem albo postawisz, przestają rosnąć i przestają umierać ze starości, czyli ich wiek zostaje zamrożony na obecnym poziomie. Wiersz pojawia się tylko na zasobach przenoszących dane drzew (malowanie drzew z menu Roślinność); zasoby, których panel nie ma takiego wiersza, nie są ani zapisywane, ani zmieniane.";
		d["item.decoration.ex.category"] = "przełączenie na inny rodzaj drzewa nadal daje włączone «Zachowaj wiek», ale ten wiersz istnieje tylko na zasobach z danymi drzew, więc malowanie naklejki nie ma wcale takiego wiersza (wymaga własnego ustawienia)";
		d["item.decoration.ex.group"] = "na przykład włączysz «Zachowaj wiek» przy malowaniu drzew i inne sposoby malowania roślinności w tej samej grupie też są włączone, a inna grupa nie";
		d["item.decoration.ex.menu"] = "każdy zasób w menu Roślinność ma włączone «Zachowaj wiek», ale po przejściu do innego menu już nie";
		d["item.f.brush.desc"] = "Zapamiętuje wiersze «Rozmiar pędzla» i «Nacisk pędzla» w panelach funkcji: rozmiar określa, jaki obszar obejmuje jedno pociągnięcie, a nacisk, ile zmienia każde pociągnięcie. Działają tylko te funkcje, których panel naprawdę ma te wiersze, czyli dziewięć narzędzi terraformowania (Narzędzie do podnoszenia lub obniżania terenu, Narzędzie do wygładzania terenu, Narzędzie do wyrównania terenu, Narzędzie do tworzenia spadków terenu, Narzędzie do malowania tekstury 01-04 i Gumka tekstury) oraz malowanie stref; funkcje bez tych wierszy nie są ani zapisywane, ani zmieniane.";
		d["item.f.brush.ex.category"] = "przełączenie na inną grupę funkcji używa własnego pędzla tej grupy (wymaga własnego ustawienia), nawet gdy oba panele mają te dwa wiersze";
		d["item.f.brush.ex.group"] = "na przykład ustawisz rozmiar pędzla na 30 m na jednym narzędziu terraformowania i inne narzędzia terraformowania w tej samej grupie też mają 30 m, a inna grupa nie";
		d["item.f.brush.ex.menu"] = "każde narzędzie w grupie Terraformowanie używa tego samego rozmiaru i nacisku pędzla, a inna grupa funkcji nie";
		d["item.f.snap.desc"] = "Zapamiętuje każdy przełącznik wiersza «Przyciąganie» w panelach funkcji: Przyciągaj do kątów prostych, Przyciągaj do siatki w strefie, Przyciągaj do dróg, Przyciągaj do strony właściciela, Pokaż linie konturów i tak dalej — to, które przełączniki się pojawią, zależy od narzędzia. Pochodzący z gry przełącznik «Wł./wył. przyciąganie» to przełącznik zbiorczy i nie jest zapamiętywany. Wiersz przyciągania na stronie Zasoby i na stronie Rekwizyty jest zapamiętywany osobno i żaden nie wpływa na pozostałe.";
		d["item.f.snap.ex.category"] = "przełączenie na inną grupę funkcji wraca do ustawień domyślnych tej grupy (wymaga własnego ustawienia), nawet gdy obie są dostępne z tego samego menu paska narzędzi";
		d["item.f.snap.ex.group"] = "na przykład wyłączysz «Przyciągaj do siatki w strefie» dla strefy mieszkalnej i strefy komercyjne, przemysłowe i biurowe też mają to wyłączone, a narzędzia obszarów nie";
		d["item.f.snap.ex.menu"] = "każde narzędzie wewnątrz jednej grupy funkcji używa tego samego zestawu przełączników, a inna grupa funkcji nie";
		d["item.f.toolMode.desc"] = "Zapamiętuje wybrany tryb narzędzia w panelach funkcji. Opcje różnią się między narzędziami: Strefy oferują Wypełnienie, Zaznaczanie obszarowe i Pędzel; narzędzia obszarów (Narzędzie do tworzenia dzielnicy, Narzędzie do wyznaczania terenu pod przemysł specjalistyczny, Narzędzie do tworzenia powierzchni) oferują Edytuj i Generuj siatkę mapy; każde narzędzie terraformowania ma własne tryby; buldożer oferuje Główne elementy, Elementy podrzędne i Wszystko. Tryb narzędzia na stronie Zasoby i na stronie Rekwizyty jest zapamiętywany osobno i żaden nie wpływa na pozostałe.";
		d["item.f.toolMode.ex.category"] = "przełączenie na inną grupę funkcji wraca do własnego trybu tej grupy (wymaga własnego ustawienia), nawet gdy obie są dostępne z tego samego menu paska narzędzi";
		d["item.f.toolMode.ex.group"] = "na przykład wybierzesz Pędzel dla strefy mieszkalnej i strefy komercyjne, przemysłowe i biurowe też są na Pędzelu, a narzędzia obszarów nie";
		d["item.f.toolMode.ex.menu"] = "każde narzędzie wewnątrz jednej grupy funkcji używa tego samego trybu, a inna grupa funkcji nie";
		d["item.f.topography.desc"] = "Zapamiętuje stan wiersza «Linie konturów» w panelach funkcji, czyli czy podczas rysowania stref widać kontury terenu. Ten wiersz jest odczytywany i zapisywany osobno dla każdej strony: strona Funkcje trzyma swoją kopię dla stref, strona Zasoby ma własną i jedna nigdy nie nadpisuje drugiej.";
		d["item.f.topography.ex.category"] = "przełączenie na inną grupę funkcji używa własnej kopii tej grupy (wymaga własnego ustawienia); wiersz «Linie konturów» na stronie Zasoby to osobna kopia i nie jest nią objęty";
		d["item.f.topography.ex.group"] = "na przykład zaznaczysz linie konturów dla strefy mieszkalnej i strefy komercyjne, przemysłowe i biurowe też mają je zaznaczone";
		d["item.f.topography.ex.menu"] = "każde narzędzie wewnątrz jednej grupy funkcji utrzymuje je zaznaczone, a inna grupa funkcji nie";
		d["item.f.underground.desc"] = "Zapamiętuje stan wiersza «Tryb podziemny» w panelach funkcji. Ten wiersz jest odczytywany i zapisywany osobno dla każdej strony: strona Funkcje trzyma swoją kopię dla stref, a strony Zasoby i Rekwizyty mają każdą własną i żadna nie jest współdzielona.";
		d["item.f.underground.ex.category"] = "przełączenie na inną grupę funkcji używa własnej kopii tej grupy (wymaga własnego ustawienia); wiersz «Tryb podziemny» na stronie Zasoby to osobna kopia i nie jest nią objęty";
		d["item.f.underground.ex.group"] = "na przykład włączysz tryb podziemny dla strefy mieszkalnej i strefy komercyjne, przemysłowe i biurowe też są pod ziemią";
		d["item.f.underground.ex.menu"] = "każde narzędzie wewnątrz jednej grupy funkcji pozostaje w trybie podziemnym, a inna grupa funkcji nie";
		d["item.i.brush.desc"] = "Zapamiętuje wiersze «Rozmiar pędzla» i «Nacisk pędzla» w panelach rekwizytów, czyli jak szeroko sięga jedno pociągnięcie pędzla i ile rekwizytów jedno pociągnięcie stawia. Działają tylko te rekwizyty, których panel naprawdę ma te wiersze; rekwizyty bez nich nie są ani zapisywane, ani zmieniane.";
		d["item.i.brush.ex.category"] = "przełączenie na inną kategorię rekwizytów używa własnego pędzla tej kategorii (wymaga własnego ustawienia), choć obie są w menu Kształtowanie krajobrazu";
		d["item.i.brush.ex.group"] = "na przykład ustawisz rozmiar pędzla na 20 m i inne rekwizyty tej samej grupy nadające się do malowania też mają 20 m, a inna grupa nie";
		d["item.i.brush.ex.menu"] = "każdy rekwizyt nadający się do malowania w menu Kształtowanie krajobrazu używa tego samego pędzla, ale po przejściu do innego menu już nie";
		d["item.i.snap.desc"] = "Zapamiętuje każdy przełącznik wiersza «Przyciąganie» w panelach rekwizytów: Przyciągaj do powierzchni obiektu, Przyciągaj do pobliskich kształtów, Przyciągaj do dróg, Usuń tylko pasujący typ i tak dalej, w zależności od rekwizytu. Wiersz przyciągania na stronie Zasoby i na stronie Funkcje jest zapamiętywany osobno i żaden nie wpływa na pozostałe.";
		d["item.i.snap.ex.category"] = "przełączenie na inną kategorię rekwizytów wraca do ustawień domyślnych tej kategorii (wymaga własnego ustawienia), choć obie są w menu Kształtowanie krajobrazu";
		d["item.i.snap.ex.group"] = "na przykład wyłączysz «Przyciągaj do powierzchni obiektu» dla rekwizytu z kategorii Park i inne rekwizyty w tej samej grupie też mają to wyłączone, a inna grupa nie";
		d["item.i.snap.ex.menu"] = "każdy rekwizyt w menu Kształtowanie krajobrazu używa tego samego zestawu przełączników, ale po przejściu do innego menu już nie";
		d["item.i.toolMode.desc"] = "Zapamiętuje wybór wiersza «Tryb narzędzia» w panelach rekwizytów: Umieść jeden, Umieść wiele, Linia, Krzywa, Stempel obiektu, Pędzel i tak dalej, w zależności od narzędzia. Tryb narzędzia na stronie Zasoby i na stronie Funkcje jest zapamiętywany osobno i żaden nie wpływa na pozostałe.";
		d["item.i.toolMode.ex.category"] = "przełączenie na inną kategorię rekwizytów, na przykład z Natura na Naklejki, wraca do własnego trybu tej kategorii (wymaga własnego ustawienia), choć obie są w menu Kształtowanie krajobrazu";
		d["item.i.toolMode.ex.group"] = "na przykład wybierzesz Umieść wiele dla rekwizytu z kategorii Natura i inne rekwizyty w tej samej grupie też używają Umieść wiele, a inna grupa nie";
		d["item.i.toolMode.ex.menu"] = "każdy rekwizyt w menu Kształtowanie krajobrazu jest na tym trybie, ale po przejściu do innego menu nie jest";
		d["item.i.topography.desc"] = "Zapamiętuje stan wiersza «Linie konturów» w panelach rekwizytów. Ten wiersz jest odczytywany i zapisywany osobno dla każdej strony: strona Rekwizyty trzyma własną kopię, a strony Zasoby i Funkcje mają każdą swoją i żadna nie jest współdzielona.";
		d["item.i.topography.ex.category"] = "przełączenie na inną kategorię rekwizytów używa własnej kopii tej kategorii (wymaga własnego ustawienia); wiersze «Linie konturów» na stronach Zasoby i Funkcje to osobne kopie i nie są nimi objęte";
		d["item.i.topography.ex.group"] = "na przykład zaznaczysz linie konturów podczas stawiania rekwizytów i inne rekwizyty w tej samej grupie też mają je zaznaczone";
		d["item.i.topography.ex.menu"] = "każdy rekwizyt w menu Kształtowanie krajobrazu utrzymuje je zaznaczone, ale po przejściu do innego menu już nie";
		d["item.i.underground.desc"] = "Zapamiętuje stan wiersza «Tryb podziemny» w panelach rekwizytów, czyli czy rekwizyty stawiasz pod ziemią, czy nad nią. Ten wiersz jest odczytywany i zapisywany osobno dla każdej strony: strona Rekwizyty trzyma własną kopię, a strony Zasoby i Funkcje mają każdą swoją i żadna nie jest współdzielona.";
		d["item.i.underground.ex.category"] = "przełączenie na inną kategorię rekwizytów używa własnej kopii tej kategorii (wymaga własnego ustawienia); wiersze «Tryb podziemny» na stronach Zasoby i Funkcje to osobne kopie i nie są nimi objęte";
		d["item.i.underground.ex.group"] = "na przykład włączysz tryb podziemny dla jakiegoś rekwizytu i inne rekwizyty w tej samej grupie też schodzą pod ziemię";
		d["item.i.underground.ex.menu"] = "każdy rekwizyt w menu Kształtowanie krajobrazu schodzi pod ziemię, ale po przejściu do innego menu nie";
		d["scope.category.function"] = "Ten sam typ";
		d["scope.category.item"] = "Ten sam typ rekwizytu";
		d["scope.line.category.function"] = "Ten sam typ: grupujemy według jednej z sześciu grup funkcji, do której należy narzędzie — Strefy (mieszkalne, komercyjne, przemysłowe, biurowe), narzędzia obszarów (Narzędzie do tworzenia dzielnicy, Narzędzie do wyznaczania terenu pod przemysł specjalistyczny, Narzędzie do tworzenia powierzchni), narzędzia tras (każda trasa w menu Transport), dziewięć narzędzi terraformowania (Narzędzie do podnoszenia lub obniżania terenu, Narzędzie do wygładzania terenu, Narzędzie do wyrównania terenu, Narzędzie do tworzenia spadków terenu, Narzędzie do malowania tekstury 01-04, Gumka tekstury), Przestrzenie i obszary (Teren parkowy, Teren spacerowy, Teren rekreacyjny, Czysty teren, Prześwietlony teren) oraz Znacznik i prefaby obiektów (narzędzie do umieszczania zwierząt i im podobne): {0}";
		d["scope.line.category.item"] = "Ten sam typ rekwizytu: grupujemy według jednej z ośmiu kategorii rekwizytów menu Kształtowanie krajobrazu, do której należy element (Natura, Park, Rekwizyty (strefa mieszkalna), Rekwizyty (strefa komercyjna), Rekwizyty (strefa przemysłowa), Gruz, Naklejki, Rośliny doniczkowe); inny model albo inny rozmiar wewnątrz jednej kategorii nie tworzy innego typu: {0}";
		d["tab.assets"] = "Zasoby";
		d["tab.functions"] = "Funkcje";
		d["tab.items"] = "Rekwizyty";
		d["tab.master"] = "Główny przełącznik";
		d["enabled.label"] = "Włącz mod";
		d["enabled.desc"] = "Domyślnie włączone. Dopóki jest włączone, każde ustawienie poniżej działa, a wartość każdej pozycji jest zapisywana na bieżąco, więc powrót do zapisu stawia panele dokładnie tak, jak je zostawiłeś; wyłączenie pojedynczej pozycji tylko powstrzymuje jej przywracanie — jej wartości nadal są zapisywane. Wyłączenie tego głównego przełącznika cofa wszystko do oryginału: zapisywanie się zatrzymuje, nic nie jest przywracane, a wszystkie ustawienia na stronach Zasoby, Funkcje i Rekwizyty są od razu wyszarzane, więc każde narzędzie zachowuje się tak, jak robi to gra sama. Przełącznik ten wyłącza się sam, gdy wszystkie trzy przełączniki stron poniżej są wyłączone, i włącza się z powrotem, gdy tylko jeden z nich jest włączony.";
		d["scope.line.sharedSingle"] = "Współdzielone globalnie: każde narzędzie tej strony obsługujące tę pozycję używa jednej wartości";
		d["scope.note"] = "Uwaga: strony Zasoby, Funkcje i Rekwizyty mają każdą własną pamięć, nawet gdy wiersz nazywa się tak samo na kilku stronach; «Współdzielone globalnie» i «Bez współdzielenia» obowiązują tylko na stronie, którą aktualnie oglądasz.";
		d["item.brush.desc"] = "Zapamiętuje wiersze «Rozmiar pędzla» i «Nacisk pędzla»: rozmiar określa, jaki obszar obejmuje jedno pociągnięcie, a nacisk, ile zmienia każde pociągnięcie. Ta pozycja obejmuje tylko malowanie na stronie Zasoby, czyli stawianie zasobów takich jak drzewa z menu Roślinność w trybie pędzla; dziewięć narzędzi terraformowania ma własną pozycję pędzla na stronie Funkcje, a kategorie rekwizytów własną na stronie Rekwizyty i trzy strony nigdy nie współdzielą. Narzędzia, których panel nie ma takich wierszy, nie są ani zapisywane, ani zmieniane.";
		d["item.brush.ex.group"] = "na przykład ustawisz rozmiar pędzla na 30 m przy malowaniu drzew i inne zasoby nadające się do malowania w tej samej grupie też mają 30 m, a narzędzie z innej grupy nie";
		d["item.brush.ex.menu"] = "każdy zasób malowany w menu Roślinność używa tego samego rozmiaru i nacisku pędzla, ale po przejściu do innego menu już nie";
		d["item.brush.ex.category"] = "przełączenie na inny rodzaj drzewa zachowuje ten sam pędzel, ale ten poziom grupuje zasoby według tego, komu służą, a narzędzia terraformowania i rekwizyty trzymają każdą własną kopię na innej stronie (wymagają własnego ustawienia)";
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
			d["scope.desc"] = "Até que ponto o valor deste item é partilhado. Os parênteses na lista suspensa mostram o que o jogo original faz e o que recomendamos.";
			d["scope.line.group"] = "Mesmo grupo: {0}";
			d["scope.line.menu"] = "Mesmo menu: {0}";
			d[kScopeLineCategory] = "Mesmo tipo de ativo: agrupado por aquilo que o ativo realmente atende, então número de faixas, largura, nível, tamanho e a categoria da barra de ferramentas não fazem diferença (um estacionamento pequeno e um estacionamento grande são o mesmo tipo), mas pontes, cais e interseções são tipos próprios, separados das vias, ou seja, {0}";
			d["scope.line.shared"] = "Global: cada ativo e cada função que suportam este item partilham um valor, mas ativos e funções não partilham entre si";
			d["scope.line.unique"] = "Sem partilhar: cada ativo e cada função que suportam este item são definidos de forma totalmente independente";

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
			d["resetall.desc"] = "Devolve cada opção deste mod ao seu padrão recomendado: o interruptor geral volta a ligar, e o estado de ativação e o âmbito de partilha de cada item também são restaurados. A memória já gravada dos jogos não é afetada.";
			d["resetall.warn"] = "Não pode ser desfeito.";
			d["resetall.confirm"] = "Repor todas as definições deste mod aos valores recomendados?";
			d["folder.label"] = "Gerir os arquivos de memória de todos os jogos";
			d["folder.desc"] = "Abre a pasta local com os arquivos de memória, um por jogo, com o nome exatamente igual ao do jogo que você vê na lista «Carregar jogo». A memória é sempre lida pelo nome do jogo: um arquivo só é usado quando o nome do jogo e o nome do arquivo json coincidem, então renomear o arquivo é como você copia a memória de um jogo para outro. Ao carregar um autossalvamento, a memória dele continua indo para o arquivo do jogo original e sobrescrevendo-o, porque um autossalvamento recebe um nome novo toda vez e esse nome não serve como nome de arquivo. A memória de um autossalvamento só fica temporariamente sob o nome da cidade quando não dá para descobrir qual era o jogo original, e o arquivo é renomeado assim que você salva manualmente.";

			d["about.version"] = "Versão do mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Apoie o autor no Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Abrir a conversa no fórum da Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Abrir o site da série Rainbow.";

			d["item.anarchy.desc"] = "Lembra o estado do botão Anarchy que o mod Anarchy acrescenta. O Anarchy é partilhado em todo o jogo, por isso este item é global; com ele ligado, você pode mudar o âmbito de partilha. Sem o mod Anarchy isto não aparece.";
			d["item.anarchy.ex.group"] = "por exemplo, você liga o Anarchy em uma rua pequena de 2 faixas e as outras ruas pequenas também ficam com ele ligado, enquanto uma via grande não";
			d["item.anarchy.ex.menu"] = "todo ativo que você clica no menu Vias tem o Anarchy ligado, mas ao mudar para o menu Eletricidade já não";
			d["item.anarchy.ex.category"] = "mudar para uma via de 6 faixas ainda o deixa ligado, mas uma ponte ou um trilho duplo de metrô o desliga e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Lembra quais entradas estão marcadas no filtro Tema da barra de ferramentas. Essa linha só aparece quando a categoria atual realmente contém ativos que usam temas, e marcar um deles limita a lista da barra de ferramentas aos ativos que suportam esses temas. O original só devolve esse filtro ao tema padrão ao carregar um jogo, então recomendamos deixar este item desligado.";
			d["item.themes.ex.group"] = "por exemplo, você marca um tema só em Vias/Ruas, então as vias grandes não filtram por ele";
			d["item.themes.ex.menu"] = "um tema marcado no menu Vias deixa de valer no menu Eletricidade e volta quando você retorna";
			d["item.themes.ex.category"] = "esta linha é o estado do próprio painel de filtros, então fica guardada pelo menu e grupo em que você está, e não pelo tipo de ativo";
			d["item.packs.label"] = "Pacote";
			d["item.packs.desc"] = "Lembra quais entradas estão marcadas no filtro Pacote da barra de ferramentas; marcar um deles limita a lista da barra de ferramentas aos ativos que pertencem a esses pacotes. O original limpa essa seleção toda vez que você troca de menu ou de categoria, então lembrá-la por menu e categoria é o que corresponde ao que você vê, e também recomendamos deixar este item desligado.";
			d["item.packs.ex.group"] = "por exemplo, você marca um pacote em Vias/Ruas, o original limpa essa marcação nas vias grandes, e ela volta quando você retorna";
			d["item.packs.ex.menu"] = "os pacotes marcados no menu Vias e os marcados no menu Eletricidade são lembrados à parte";
			d["item.packs.ex.category"] = "esta linha é o estado do próprio painel de filtros, guardada pelo menu e grupo em que você está; o original limpa essa seleção toda vez que o grupo muda";
			d["item.toolMode.label"] = "Ferramentas";
			d["item.toolMode.desc"] = "Lembra o modo escolhido para cada ferramenta, e as opções mudam conforme a ferramenta: estradas, trilhos e canos oferecem Reta, Curva simples, Curva complexa, Contínuo, Grade, Substituir, Ponto; prédios, acessórios e árvores oferecem Colocar um, Colocar vários, Linha, Curva, Ferramenta de carimbo de objetos; as zonas oferecem Preencher, Marcar, Pintar; as áreas oferecem Edição, Gerar grade de mapa. Os modos de ativos e os de funções são lembrados à parte porque as opções não são as mesmas.";
			d["item.toolMode.ex.group"] = "por exemplo, você escolhe Curva simples em uma rua pequena de 2 faixas e as outras ruas pequenas também usam Curva simples, enquanto uma via grande não";
			d["item.toolMode.ex.menu"] = "todo ativo que você clica no menu Vias está em Curva simples, mas ao mudar para o menu Eletricidade já não";
			d["item.toolMode.ex.category"] = "mudar para uma via de 6 faixas ainda dá Curva simples, mas uma ponte ou um trilho duplo de metrô volta ao seu modo padrão e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.elevation.label"] = "Elevação";
			d["item.elevation.desc"] = "Lembra a elevação em que a ferramenta ficou, incluindo o resultado de Aumentar elevação e Reduzir elevação. O Passo de elevação, o quanto se anda com um toque em Aumentar elevação ou Reduzir elevação (no painel do Anarchy essa linha tem o mesmo nome), é lembrado junto com a elevação e usa o mesmo âmbito de partilha. A elevação de cruzamentos não é coberta. Alguns ativos têm a sua própria faixa de elevação permitida: se o valor compartilhado estiver fora daquilo que o ativo selecionado permite, este mantém uma elevação que lhe é possível e o valor compartilhado memorizado permanece o mesmo.";
			d["item.elevation.ex.group"] = "por exemplo, você sobe uma rua de 2 faixas a 10 m e as outras ruas pequenas também vão a 10 m, enquanto uma via grande não está a 10 m";
			d["item.elevation.ex.menu"] = "todo ativo que você clica no menu Vias está a 10 m, mas ao mudar para o menu Eletricidade não está a 10 m";
			d["item.elevation.ex.category"] = "mudar para uma via de 6 faixas ainda dá 10 m, mas uma ponte ou um trilho duplo volta a 0 m e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.parallel.label"] = "Modo Paralelo";
			d["item.parallel.desc"] = "Lembra as duas coisas do painel Via paralela: o interruptor Modo Paralelo (Alternar modo paralelo) e a distância do Contraste paralelo. No modo paralelo são sempre duas linhas: ligado, uma segunda linha é construída ao lado da que você desenha, e o que você ajusta é a distância entre as duas, por exemplo 1 m ou 2 m. Desligado, fica só a linha única.";
			d["item.parallel.ex.group"] = "por exemplo, você liga o Modo Paralelo em uma rua pequena de 2 faixas e põe o contraste em 5 m, as outras ruas pequenas voltam ligadas com os mesmos 5 m, enquanto uma via grande não";
			d["item.parallel.ex.menu"] = "todo ativo que você clica no menu Vias é paralelo com 5 m de contraste, mas ao mudar para o menu Eletricidade não";
			d["item.parallel.ex.category"] = "mudar para uma via de 6 faixas continua ligado com os mesmos 5 m de contraste, mas uma ponte ou um trilho duplo de metrô o desliga e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.snap.label"] = "Aderir";
			d["item.snap.desc"] = "Lembra cada interruptor da linha Aderir: Aderir à geometria existente, Aderir ao tamanho da célula da zona, Aderir a ângulos de 90 graus, Aderir aos lados de uma via, Aderir a vias, Aderir ao lado do proprietário, Aderir aos lados de uma estrutura, Aderir ao meio de uma via, Aderir à costa, Aderir a geometria próxima, Aderir a linhas guia, Aderir à grade da zona, Aderir aos nodos, Aderir à superfície de um objeto, Aderir verticalmente, Aderir à grade do lote, Vincula itens sobrepostos a uma estrutura, Remover apenas o tipo correspondente, Exibir linhas de contorno, Aderir à distância. A linha Alternar modo Aderir do jogo é um interruptor em lote e não é lembrada.";
			d["item.snap.ex.group"] = "por exemplo, você desliga «Aderir aos lados de uma via» em uma rua pequena de 2 faixas e as outras ruas pequenas também ficam sem ela, enquanto uma via grande ainda tem tudo ligado";
			d["item.snap.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo conjunto de interruptores da linha Aderir, mas ao mudar para o menu Eletricidade não";
			d["item.snap.ex.category"] = "mudar para uma via de 6 faixas mantém o mesmo conjunto, mas uma ponte ou um caminho volta aos valores padrão e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Lembra a linha Topografia, ou seja, se Exibir linhas de contorno está ligado.";
			d["item.topography.ex.group"] = "por exemplo, você liga a Topografia em uma rua pequena de 2 faixas e as outras ruas pequenas também a têm ligada, enquanto uma via grande não";
			d["item.topography.ex.menu"] = "todo ativo que você clica no menu Vias mostra as linhas de contorno, mas ao mudar para o menu Eletricidade não";
			d["item.topography.ex.category"] = "mudar para uma via de 6 faixas ainda as mostra, mas uma ponte ou um cabo elétrico tem a linha desligada e precisa do próprio ajuste";
			d["item.leftRight.label"] = "Esquerda e Direita";
			d["item.leftRight.desc"] = "Lembra as redes laterais escolhidas nas linhas «Esquerda» e «Direita» do painel de redes do Anarchy, por exemplo Ciclovia, Árvores, Grama, Quay, Retaining Wall e Barreira de som. Precisa do Anarchy (mod 74604 da Paradox); sem ele isto não aparece.";
			d["item.leftRight.ex.group"] = "por exemplo, você escolhe Ciclovia no lado direito de uma rua pequena de 2 faixas e as outras ruas pequenas também a recebem à direita, enquanto uma via grande não";
			d["item.leftRight.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo par de seleções laterais, mas ao mudar para o menu Eletricidade não";
			d["item.leftRight.ex.category"] = "mudar para uma via de 6 faixas ainda tem Ciclovia à direita, mas uma ponte ou um trilho duplo volta sem nada escolhido e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.general.label"] = "Geral";
			d["item.general.desc"] = "Lembra os interruptores da linha «Geral» do painel de redes do Anarchy: Solo, Elevated, Tunnel, Inclinação constante, Wide Median e Expanded Elevation Range (este último tira o limite de altura). Precisa do Anarchy (mod 74604 da Paradox); sem ele isto não aparece.";
			d["item.general.ex.group"] = "por exemplo, você escolhe Tunnel em uma rua pequena de 2 faixas e as outras ruas pequenas também são forçadas a virar túneis, enquanto uma via grande não";
			d["item.general.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo conjunto de interruptores da linha Geral, mas ao mudar para o menu Eletricidade não";
			d["item.general.ex.category"] = "mudar para uma via de 6 faixas ainda dá Tunnel, mas uma ponte ou um trilho duplo de metrô volta sem nada marcado e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.underground.label"] = "Modo Subterrâneo";
			d["item.underground.desc"] = "Lembra o interruptor Modo Subterrâneo (Alternar modo subterrâneo), que decide se o que você coloca fica abaixo ou acima do solo.";
			d["item.underground.ex.group"] = "por exemplo, você liga o Modo Subterrâneo em uma rua pequena de 2 faixas e as outras ruas pequenas também vão para baixo do solo, enquanto uma via grande não";
			d["item.underground.ex.menu"] = "todo ativo que você clica no menu Vias é construído debaixo da terra, mas ao mudar para o menu Eletricidade não";
			d["item.underground.ex.category"] = "mudar para uma via de 6 faixas ainda vai para o subsolo, mas uma ponte ou um trilho duplo fica na superfície e precisa do próprio ajuste, embora todos estejam no menu Vias";
			d["item.other.label"] = "Outros";
			d["item.other.desc"] = "Lembra as demais linhas que um painel de ferramenta pode gravar: as cores de via da linha Cor.";
			d["item.other.ex.group"] = "por exemplo, você põe a cor de uma rua pequena de 2 faixas em azul e as outras ruas pequenas também ficam azuis, enquanto uma via grande não";
			d["item.other.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo ajuste de cor, mas ao mudar para o menu Eletricidade não";
			d["item.other.ex.category"] = "mudar para uma via de 6 faixas ainda é azul, mas uma ponte ou um trilho duplo guarda a sua própria cópia e precisa do próprio ajuste, embora todos estejam no menu Vias";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行
			d["item.brush.label"] = "Pincel";

		// ==================== v0.7.0 新增 / 重写（pt-BR）====================
		d["enabled.label"] = "Ativar o mod";
		d["enabled.desc"] = "Ligado por padrão. Enquanto ele estiver ligado, todas as configurações abaixo têm efeito e o valor de cada item é gravado enquanto você trabalha, então, ao voltar para um jogo salvo, os painéis estão exatamente como você os deixou; desligar um item apenas impede que ele seja restaurado, pois os valores dele continuam sendo gravados. Desligar este interruptor geral devolve tudo ao jogo original: a gravação para, nada é restaurado e todas as configurações das páginas Ativos, Funções e Acessórios ficam acinzentadas ao mesmo tempo, e cada ferramenta se comporta como o jogo faz por conta própria. Ele se desliga sozinho quando os três interruptores por página aqui embaixo estiverem todos desligados, e se religa assim que qualquer um deles for ligado.";
		d["scope.line.sharedSingle"] = "Global: todas as ferramentas desta página que suportam este item usam um único valor";
		d["scope.note"] = "Nota: as páginas Ativos, Funções e Acessórios guardam cada uma a sua própria memória, mesmo quando uma linha tem o mesmo nome em mais de uma página; «Global» e «Sem partilhar» só valem dentro da página que você está vendo.";
		d["item.brush.desc"] = "Lembra as linhas «Tamanho do pincel» e «Força do pincel»: o tamanho define até onde uma passada alcança e a força define quanto cada passada muda. Este item só cobre a pintura feita na página Ativos, ou seja, colocar com o pincel ativos como as árvores do menu Vegetação; as nove ferramentas de Terraformação têm o próprio item de pincel na página Funções e as oito categorias de acessórios têm o delas na página Acessórios, e as três páginas nunca dividem valores entre si. As ferramentas cujo painel não tem essas duas linhas não são gravadas nem alteradas.";
		d["item.brush.ex.group"] = "por exemplo, você põe o tamanho do pincel em 30 m ao pintar árvores e os outros ativos pintáveis do mesmo grupo também ficam em 30 m, mas um ativo de outro grupo não";
		d["item.brush.ex.menu"] = "todo ativo que você pinta no menu Vegetação usa este mesmo tamanho e força de pincel, mas ao mudar para outro menu não";
		d["item.brush.ex.category"] = "ao mudar para outra espécie de árvore este mesmo pincel continua valendo, mas as ferramentas de Terraformação e os acessórios guardam cada um a sua própria cópia em outra página (precisa do próprio ajuste)";
		d["domain.asset.desc"] = "Cobre todos os itens da página Ativos: a gravação e a restauração ao carregar se ligam e se desligam juntas. Enquanto ele estiver desligado, a página Ativos inteira fica acinzentada e sem efeito, as ferramentas se comportam como no jogo original e os valores já gravados continuam no arquivo intactos, então religá-lo restaura tudo como antes.";
		d["domain.asset.label"] = "Lembrar os itens de ferramenta dos ativos";
		d["domain.function.desc"] = "Cobre todos os itens da página Funções, ou seja, os seis grupos de funções: os tipos de zona, as ferramentas de área, todas as ferramentas de rota, as ferramentas de Terraformação, os espaços e áreas, e os marcadores e objetos pré-fabricados. Enquanto ele estiver desligado, a página Funções inteira fica acinzentada e sem efeito, as ferramentas se comportam como no jogo original e os valores já gravados continuam no arquivo intactos, então religá-lo restaura tudo como antes.";
		d["domain.function.label"] = "Lembrar os itens de ferramenta das funções";
		d["domain.item.desc"] = "Cobre todos os itens da página Acessórios, ou seja, as oito categorias de acessórios do menu Paisagismo: Natureza, Parque, Acessórios residenciais, Acessórios comerciais, Acessórios industriais, Detritos, Decalques e Plantas envasadas. Enquanto ele estiver desligado, a página Acessórios inteira fica acinzentada e sem efeito, as ferramentas se comportam como no jogo original e os valores já gravados continuam no arquivo intactos, então religá-lo restaura tudo como antes.";
		d["domain.item.label"] = "Lembrar os itens de ferramenta dos acessórios";
		d["group.domain"] = "Interruptores por página";
		d["item.color.label"] = "Cor";
		d["item.decoration.desc"] = "Lembra o interruptor «Preservar idade»: com ele ligado, as árvores que você pinta ou coloca param de crescer e param de morrer de velhas, o que congela a idade daquele conjunto de árvores exatamente onde está. Essa linha só aparece nos ativos que carregam dados de árvore (ao pintar árvores no menu Vegetação); os ativos cujo painel não tem essa linha não são gravados nem alterados.";
		d["item.decoration.ex.category"] = "ao mudar para outra espécie de árvore ele continua ligado, mas essa linha só existe nos ativos com dados de árvore, então pintar um decalque não tem essa linha nenhuma (precisa do próprio ajuste)";
		d["item.decoration.ex.group"] = "por exemplo, você liga «Preservar idade» ao pintar árvores e as outras formas de pintar vegetação do mesmo grupo também ficam ligadas, mas as de outro grupo não";
		d["item.decoration.ex.menu"] = "todos os ativos do menu Vegetação mantêm essa linha ligada, mas ao mudar para outro menu não";
		d["item.decoration.label"] = "Preservar idade";
		d["item.f.brush.desc"] = "Lembra as linhas «Tamanho do pincel» e «Força do pincel» nos painéis das funções: o tamanho define até onde uma passada alcança e a força define quanto cada passada muda. Só valem para as funções cujo painel realmente tem essas duas linhas, ou seja, as nove ferramentas de Terraformação (Ferramenta de modificação de terreno, Ferramenta de suavização de terreno, Ferramenta de nivelamento de terreno, Ferramenta de declive, Ferramenta de pintura de textura 01 a 04 e Borracha de textura) e a pintura de zonas; as funções sem essas duas linhas não são gravadas nem alteradas.";
		d["item.f.brush.ex.category"] = "ao mudar para outro grupo de funções vale o pincel daquele próprio grupo (precisa do próprio ajuste), mesmo que os dois painéis tenham essas duas linhas";
		d["item.f.brush.ex.group"] = "por exemplo, você põe o tamanho do pincel da Ferramenta de modificação de terreno em 30 m e as outras ferramentas de Terraformação do mesmo grupo também ficam em 30 m, mas as de outro grupo não";
		d["item.f.brush.ex.menu"] = "todas as ferramentas do grupo Terraformação usam este mesmo tamanho e força de pincel, mas as de outro grupo de funções não";
		d["item.f.snap.desc"] = "Lembra cada interruptor da linha «Aderir» nos painéis das funções: aderir a ângulos de 90 graus, aderir à grade, aderir a vias, aderir ao lado do proprietário, exibir linhas de contorno e assim por diante; quais interruptores aparecem dependem da ferramenta. O próprio botão do jogo que ativa ou desativa todas as aderências de uma vez é um interruptor em lote e não é lembrado. A linha «Aderir» da página Ativos e a da página Acessórios são lembradas à parte e nenhuma interfere nas outras.";
		d["item.f.snap.ex.category"] = "ao mudar para outro grupo de funções voltam os interruptores padrão daquele grupo (precisa do próprio ajuste), mesmo que os dois possam ser abertos pelo mesmo menu da barra de ferramentas";
		d["item.f.snap.ex.group"] = "por exemplo, você desliga a aderência à grade na zona residencial e as zonas comercial, industrial e de escritórios também ficam sem ela, mas as ferramentas de área não";
		d["item.f.snap.ex.menu"] = "todas as ferramentas de um mesmo grupo de funções usam este mesmo conjunto de interruptores, mas as de outro grupo de funções não";
		d["item.f.toolMode.desc"] = "Lembra a escolha feita na linha «Modo da ferramenta» dos painéis das funções. As opções mudam conforme a ferramenta: os tipos de zona oferecem Preencher, Marcar e Pintar; as ferramentas de área (Ferramenta de criação de distrito, Ferramenta de área de especialização industrial e Ferramenta de área de superfície) oferecem Editar e Gerar grade de mapa; cada ferramenta de Terraformação tem os próprios modos; a ferramenta de demolição oferece Elementos principais, Subelementos e Tudo. O modo da ferramenta da página Ativos e o da página Acessórios são lembrados à parte e nenhum interfere nos outros.";
		d["item.f.toolMode.ex.category"] = "ao mudar para outro grupo de funções vale de novo o modo daquele próprio grupo (precisa do próprio ajuste), mesmo que os dois possam ser abertos pelo mesmo menu da barra de ferramentas";
		d["item.f.toolMode.ex.group"] = "por exemplo, você escolhe Pintar na zona residencial e as zonas comercial, industrial e de escritórios também ficam em Pintar, mas as ferramentas de área não";
		d["item.f.toolMode.ex.menu"] = "todas as ferramentas de um mesmo grupo de funções usam este mesmo modo, mas as de outro grupo de funções não";
		d["item.f.topography.desc"] = "Lembra se a linha «Linhas de contorno» dos painéis das funções está ligada ou desligada, isto é, se você vê as curvas de nível do terreno enquanto desenha zonas. Essa linha é lida e gravada página por página: a página Funções guarda a cópia das zonas, a página Ativos guarda a sua, e uma nunca sobrescreve a outra.";
		d["item.f.topography.ex.category"] = "ao mudar para outro grupo de funções vale a cópia daquele próprio grupo (precisa do próprio ajuste); a linha «Linhas de contorno» da página Ativos é uma cópia à parte e não é afetada";
		d["item.f.topography.ex.group"] = "por exemplo, você marca as linhas de contorno na zona residencial e as zonas comercial, industrial e de escritórios também ficam marcadas";
		d["item.f.topography.ex.menu"] = "todas as ferramentas de um mesmo grupo de funções ficam com a marcação ativa, mas as de outro grupo de funções não";
		d["item.f.underground.desc"] = "Lembra se a linha «Modo Subterrâneo» dos painéis das funções está ligada ou desligada. Essa linha é lida e gravada página por página: a página Funções guarda a cópia das zonas, e as páginas Ativos e Acessórios guardam cada uma a sua, sem nada em comum.";
		d["item.f.underground.ex.category"] = "ao mudar para outro grupo de funções vale a cópia daquele próprio grupo (precisa do próprio ajuste); a linha «Modo Subterrâneo» da página Ativos é uma cópia à parte e não é afetada";
		d["item.f.underground.ex.group"] = "por exemplo, você liga o modo subterrâneo na zona residencial e as zonas comercial, industrial e de escritórios também vão para debaixo da terra";
		d["item.f.underground.ex.menu"] = "todas as ferramentas de um mesmo grupo de funções ficam no modo subterrâneo, mas as de outro grupo de funções não";
		d["item.i.brush.desc"] = "Lembra as linhas «Tamanho do pincel» e «Força do pincel» nos painéis dos acessórios, ou seja, até onde uma passada do pincel alcança e quantos acessórios cada passada coloca. Só valem para os acessórios cujo painel realmente tem essas duas linhas; os acessórios sem essas linhas não são gravados nem alterados.";
		d["item.i.brush.ex.category"] = "ao mudar para outra categoria de acessórios vale o pincel daquela própria categoria (precisa do próprio ajuste), mesmo que as duas estejam no menu Paisagismo";
		d["item.i.brush.ex.group"] = "por exemplo, você põe o tamanho do pincel em 20 m e os outros acessórios pintáveis do mesmo grupo também ficam em 20 m, mas os de outro grupo não";
		d["item.i.brush.ex.menu"] = "todos os acessórios pintáveis do menu Paisagismo usam este mesmo pincel, mas ao mudar para outro menu não";
		d["item.i.snap.desc"] = "Lembra cada interruptor da linha «Aderir» nos painéis dos acessórios: aderir à superfície de um objeto, aderir a geometria próxima, aderir a vias, remover apenas o tipo correspondente e assim por diante, conforme o acessório. A linha «Aderir» da página Ativos e a da página Funções são lembradas à parte e nenhuma interfere nas outras.";
		d["item.i.snap.ex.category"] = "ao mudar para outra categoria de acessórios voltam os interruptores padrão daquela categoria (precisa do próprio ajuste), mesmo que as duas estejam no menu Paisagismo";
		d["item.i.snap.ex.group"] = "por exemplo, você desliga a aderência à superfície de um objeto em um acessório de Parque e os outros acessórios do mesmo grupo também ficam sem ela, mas os de outro grupo não";
		d["item.i.snap.ex.menu"] = "todos os acessórios do menu Paisagismo usam este mesmo conjunto de interruptores, mas ao mudar para outro menu não";
		d["item.i.toolMode.desc"] = "Lembra a escolha feita na linha «Modo da ferramenta» dos painéis dos acessórios: Colocar um, Colocar vários, Linha, Curva, Ferramenta de carimbo de objetos, Pintar e assim por diante, conforme a ferramenta. O modo da ferramenta da página Ativos e o da página Funções são lembrados à parte e nenhum interfere nos outros.";
		d["item.i.toolMode.ex.category"] = "ao mudar para outra categoria de acessórios, por exemplo de Natureza para Decalques, vale de novo o modo daquela própria categoria (precisa do próprio ajuste), mesmo que as duas estejam no menu Paisagismo";
		d["item.i.toolMode.ex.group"] = "por exemplo, você escolhe Colocar vários em um acessório de Natureza e os outros acessórios do mesmo grupo também usam Colocar vários, mas os de outro grupo não";
		d["item.i.toolMode.ex.menu"] = "todos os acessórios do menu Paisagismo estão neste modo, mas ao mudar para outro menu não";
		d["item.i.topography.desc"] = "Lembra se a linha «Linhas de contorno» dos painéis dos acessórios está ligada ou desligada. Essa linha é lida e gravada página por página: a página Acessórios guarda a cópia dos acessórios, e as páginas Ativos e Funções guardam cada uma a sua, sem nada em comum.";
		d["item.i.topography.ex.category"] = "ao mudar para outra categoria de acessórios vale a cópia daquela própria categoria (precisa do próprio ajuste); as linhas «Linhas de contorno» das páginas Ativos e Funções são cópias à parte e não são afetadas";
		d["item.i.topography.ex.group"] = "por exemplo, você marca as linhas de contorno ao colocar acessórios e os outros acessórios do mesmo grupo também ficam marcados";
		d["item.i.topography.ex.menu"] = "todos os acessórios do menu Paisagismo ficam com a marcação ativa, mas ao mudar para outro menu não";
		d["item.i.underground.desc"] = "Lembra se a linha «Modo Subterrâneo» dos painéis dos acessórios está ligada ou desligada, isto é, se você coloca os acessórios abaixo ou acima do solo. Essa linha é lida e gravada página por página: a página Acessórios guarda a cópia dos acessórios, e as páginas Ativos e Funções guardam cada uma a sua, sem nada em comum.";
		d["item.i.underground.ex.category"] = "ao mudar para outra categoria de acessórios vale a cópia daquela própria categoria (precisa do próprio ajuste); as linhas «Modo Subterrâneo» das páginas Ativos e Funções são cópias à parte e não são afetadas";
		d["item.i.underground.ex.group"] = "por exemplo, você liga o modo subterrâneo em um acessório e os outros acessórios do mesmo grupo também vão para debaixo da terra";
		d["item.i.underground.ex.menu"] = "todos os acessórios do menu Paisagismo vão para debaixo da terra, mas ao mudar para outro menu não";
		d["scope.category.function"] = "Mesmo tipo";
		d["scope.category.item"] = "Mesmo tipo de acessório";
		d["scope.line.category.function"] = "Mesmo tipo: agrupa por um dos seis grupos de funções a que a ferramenta pertence - os tipos de zona (residencial, comercial, industrial e de escritórios), as ferramentas de área (Ferramenta de criação de distrito, Ferramenta de área de especialização industrial e Ferramenta de área de superfície), as ferramentas de rota (todas as rotas do menu Transporte), as nove ferramentas de Terraformação (Ferramenta de modificação de terreno, Ferramenta de suavização de terreno, Ferramenta de nivelamento de terreno, Ferramenta de declive, Ferramenta de pintura de textura 01 a 04, Borracha de textura), os espaços e áreas (Área de parque, Área de pedestre, Área de convivência, Área vazia e Superfície de transparência) e os marcadores e objetos pré-fabricados (a ferramenta de geração de animais e outras parecidas): {0}";
		d["scope.line.category.item"] = "Mesmo tipo de acessório: agrupa por uma das oito categorias de acessórios do menu Paisagismo a que o item pertence (Natureza, Parque, Acessórios residenciais, Acessórios comerciais, Acessórios industriais, Detritos, Decalques e Plantas envasadas); um modelo diferente ou um tamanho diferente dentro da mesma categoria não é outro tipo: {0}";
		d["tab.assets"] = "Ativos";
		d["tab.functions"] = "Funções";
		d["tab.items"] = "Acessórios";
		d["tab.master"] = "Interruptor geral";
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
			d["scope.desc"] = "Насколько широко разделяется значение этого пункта. Скобки в раскрывающемся списке показывают, как поступает оригинал и что мы рекомендуем.";
			d["scope.line.group"] = "Та же группа: {0}";
			d["scope.line.menu"] = "То же меню: {0}";
			d[kScopeLineCategory] = "Тот же тип объекта: объекты группируются по тому, кому они реально служат, поэтому число полос, ширина, уровень и размер, как и категория на панели инструментов, различий не создают (маленькая парковка и большая парковка — один тип), но мосты, набережные и развязки образуют каждый свой тип, отдельный от дорог, а именно {0}";
			d["scope.line.shared"] = "Глобально: каждый объект и каждая функция, поддерживающие этот пункт, делят одно значение, но объекты и функции не делят между собой";
			d["scope.line.unique"] = "Без разделения: каждый объект и каждая функция, поддерживающие этот пункт, настраиваются совершенно независимо";

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
			d["resetall.desc"] = "Возвращает каждую настройку этого мода к рекомендованному значению: главный переключатель снова включается, а также восстанавливаются состояние включения и область общего доступа каждого пункта. Уже записанная память сохранений не затрагивается.";
			d["resetall.warn"] = "Отменить нельзя.";
			d["resetall.confirm"] = "Сбросить все настройки этого мода к рекомендованным значениям?";
			d["folder.label"] = "Управление файлами памяти всех сохранений";
			d["folder.desc"] = "Открывает локальную папку с файлами памяти, по одному на сохранение, и название файла точно такое же, как имя сохранения в списке «Загрузить игру». Память всегда считывается по имени сохранения: файл используется только тогда, когда имя сохранения и название файла json совпадают, поэтому переименование файла и есть способ скопировать память с одного сохранения на другое. При загрузке автосохранения его память по-прежнему пишется в файл исходного сохранения и перезаписывает его, поскольку автосохранение каждый раз получает новое имя и его нельзя использовать как имя файла. Под названием города память автосохранения временно хранится только тогда, когда исходное сохранение не удаётся распознать; как только вы сохраните игру вручную, файл сам получит настоящее имя сохранения.";

			d["about.version"] = "Версия мода";
			d["about.author"] = "Автор";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Поддержать автора на Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Открыть тему на форуме Paradox.";
			d["about.rainbow"] = "RAINBOW Site";
			d["about.rainbow.desc"] = "Открыть сайт серии Rainbow.";

			d["item.anarchy.desc"] = "Запоминает состояние кнопки Anarchy, которую добавляет мод Anarchy. Anarchy разделяется во всей игре, так что этот пункт глобальный; когда он включён, можно изменить область общего доступа. Без мода Anarchy этот пункт не отображается.";
			d["item.anarchy.ex.group"] = "например, включите Anarchy на небольшой двухполосной дороге, и на других небольших дорогах он тоже останется включённым, а на большой дороге — нет";
			d["item.anarchy.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», Anarchy включён, но в меню «Электричество» уже нет";
			d["item.anarchy.ex.category"] = "переход на шестиполосную дорогу тоже сохраняет его включённым, но мост или двухпутный тоннель метро выключает его и требует собственной настройки, хотя все они находятся в меню «Дороги»";
			d["item.themes.label"] = "Тема";
			d["item.themes.desc"] = "Запоминает, какие пункты отмечены в фильтре «Тема» на панели инструментов. Эта строка появляется, только когда текущая категория действительно содержит объекты, в которых используются темы, а отмеченная тема ограничивает список панели инструментов объектами, которые её поддерживают. В оригинальной игре этот фильтр возвращается к теме по умолчанию только при загрузке сохранения, поэтому мы рекомендуем оставить этот пункт выключенным.";
			d["item.themes.ex.group"] = "например, отметьте тему только в «Дороги/Небольшие дороги», и большие дороги по этой теме фильтроваться не будут";
			d["item.themes.ex.menu"] = "тема, отмеченная в меню «Дороги», перестаёт действовать в меню «Электричество» и возвращается, когда вы снова открываете «Дороги»";
			d["item.themes.ex.category"] = "эта строка — состояние самой панели фильтров, поэтому она запоминается по текущему меню и группе, а не по типу объекта";
			d["item.packs.label"] = "Набор";
			d["item.packs.desc"] = "Запоминает, какие пункты отмечены в фильтре «Набор» на панели инструментов; отмеченный набор ограничивает список панели инструментов объектами из этого набора. Оригинальная игра очищает этот фильтр при каждой смене меню или категории, поэтому запоминание в пределах одного меню и категории совпадает с тем, что вы видите, и этот пункт мы тоже рекомендуем оставить выключенным.";
			d["item.packs.ex.group"] = "например, отметьте набор в «Дороги/Небольшие дороги», оригинал снимет эту отметку на больших дорогах, а при возврате она вернётся";
			d["item.packs.ex.menu"] = "наборы, отмеченные в меню «Дороги», и наборы, отмеченные в меню «Электричество», запоминаются раздельно";
			d["item.packs.ex.category"] = "эта строка — состояние самой панели фильтров, она запоминается по текущему меню и группе; оригинал очищает её при каждой смене группы";
			d["item.toolMode.label"] = "Режим инструмента";
			d["item.toolMode.desc"] = "Запоминает режим, выбранный для каждого инструмента, а параметры различаются: для дорог, путей и труб это Прямая, Простая кривая, Сложная кривая, Непрерывная, Сетка, Заменить, Точка; для зданий, объектов и деревьев — Разместить один, Разместить несколько, Линия, Кривая, Инструмент копирования объекта; для зон — Заливка, Рамка, Кисть; для областей — Изменить, Создать сетку карты. Режимы объектов и режимы функций запоминаются отдельно, потому что их наборы различаются.";
			d["item.toolMode.ex.group"] = "например, выберите для небольшой двухполосной дороги режим «Простая кривая», и другие небольшие дороги тоже перейдут в простую кривую, а большая дорога — нет";
			d["item.toolMode.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», стоит режим «Простая кривая», но в меню «Электричество» уже нет";
			d["item.toolMode.ex.category"] = "переход на шестиполосную дорогу тоже даёт простую кривую, но мост или двухпутный тоннель метро возвращается к своему режиму по умолчанию и требует собственной настройки, хотя все они находятся в меню «Дороги»";
			d["item.elevation.label"] = "Эстакада";
			d["item.elevation.desc"] = "Запоминает высоту, на которой оставлен инструмент, включая результат от Увеличить подъем и Уменьшить подъем. Шаг подъема (насколько сдвигает одно нажатие Увеличить подъем или Уменьшить подъем; в панели мода Anarchy эта строка называется так же) запоминается вместе с высотой и делит с ней область общего доступа. Высота перекрёстков не запоминается. У отдельных объектов есть свой допустимый диапазон высот: если общее значение выходит за пределы, доступные выбранному объекту, объект сохраняет высоту, которая ему доступна, а запомненное общее значение остаётся прежним.";
			d["item.elevation.ex.group"] = "например, поднимите двухполосную дорогу до 10 м, и другие небольшие дороги тоже поднимутся до 10 м, а большая дорога на 10 м не будет";
			d["item.elevation.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», высота 10 м, но в меню «Электричество» уже не 10 м";
			d["item.elevation.ex.category"] = "переход на шестиполосную дорогу тоже даёт 10 м, но мост или двойные железнодорожные пути возвращаются к 0 м и требуют собственной настройки, хотя все они находятся в меню «Дороги»";
			d["item.parallel.label"] = "Режим параллели";
			d["item.parallel.desc"] = "Запоминает две вещи на панели «Параллельная дорога»: переключатель «Режим параллели» (Переключить режим параллели) и расстояние «Смещение параллели». Режим параллели — это всегда две линии: пока он включён, вторая линия строится рядом с той, которую вы проводите, а настроить можно только то, как далеко они стоят друг от друга, например на 1 м или 2 м. Когда он выключен, остаётся одна линия.";
			d["item.parallel.ex.group"] = "например, включите режим параллели на небольшой двухполосной дороге и поставьте смещение 5 м, другие небольшие дороги тоже вернутся включёнными с теми же 5 м, а большая дорога — нет";
			d["item.parallel.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», параллель включена со смещением 5 м, но в меню «Электричество» уже нет";
			d["item.parallel.ex.category"] = "переход на шестиполосную дорогу тоже оставляет режим включённым с теми же 5 м, но мост или двухпутный тоннель метро выключает его и требует собственной настройки, хотя все они находятся в меню «Дороги»";
			d["item.snap.label"] = "Привязка";
			d["item.snap.desc"] = "Запоминает каждый переключатель в строке Привязка: Привязка к форме, Привязка к длине зоны, Привязка к прямым углам, Привязка к обочине дороги, Привязка к дорогам, Привязка к зданию-владельцу, Привязка к стенам здания, Привязка к середине дороги, Привязка к берегу, Привязка к ландшафту, Привязка к направляющим, Привязка к сетке зоны, Привязка к точкам, Привязка к поверхности объекта, Привязка по вертикали, Привязка к сетке участка, Привязывает перекрывающиеся элементы к зданию, Удалить только определенный тип, Показать контурные линии, Привязка к расстоянию. Строка Включить/выключить привязку — это пакетный переключатель, и он не запоминается.";
			d["item.snap.ex.group"] = "например, выключите «Привязку к обочине дороги» на небольшой двухполосной дороге, и на других небольших дорогах она тоже будет выключена, а на большой дороге всё ещё включено";
			d["item.snap.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же набор переключателей привязки, но в меню «Электричество» уже нет";
			d["item.snap.ex.category"] = "переход на шестиполосную дорогу сохраняет тот же набор, но мост или дорожка возвращаются к значениям по умолчанию и требуют собственной настройки, хотя все они находятся в меню «Дороги»";
			d["item.topography.label"] = "Топография";
			d["item.topography.desc"] = "Запоминает строку Топография, то есть включено ли Показать контурные линии.";
			d["item.topography.ex.group"] = "например, включите топографию на небольшой двухполосной дороге, и на других небольших дорогах она тоже будет включена, а на большой дороге — нет";
			d["item.topography.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», показаны контурные линии, но в меню «Электричество» уже нет";
			d["item.topography.ex.category"] = "переход на шестиполосную дорогу тоже показывает их, но у моста или электрического кабеля строка выключена и требует собственной настройки";
			d["item.leftRight.label"] = "Лево и Право";
			d["item.leftRight.desc"] = "Запоминает улучшения сети, выбранные в строках «Лево» и «Право» сетевой панели мода Anarchy, например «Велосипедная полоса», «Деревья», «Трава», «Набережная», «Подпорные стены», «Шумозащитный экран». Требует Anarchy (мод Paradox 74604); без него этот пункт не отображается.";
			d["item.leftRight.ex.group"] = "например, выберите велосипедную полосу с правой стороны небольшой двухполосной дороги, и другие небольшие дороги тоже получат её справа, а большая дорога — нет";
			d["item.leftRight.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется та же пара боковых выборов, но в меню «Электричество» уже нет";
			d["item.leftRight.ex.category"] = "переход на шестиполосную дорогу тоже оставляет велосипедную полосу справа, но на мосту или двойных железнодорожных путях ничего не выбрано, и им нужна собственная настройка, хотя все они находятся в меню «Дороги»";
			d["item.general.label"] = "Общее";
			d["item.general.desc"] = "Запоминает переключатели строки «Общее» в сетевой панели мода Anarchy: «Земля», «Эстакада», «Туннель», «Постоянный наклон», «Широкая медиана» и «Расширенный диапазон высот» (последний снимает ограничение высоты). Требует Anarchy (мод Paradox 74604); без него этот пункт не отображается.";
			d["item.general.ex.group"] = "например, выберите «Туннель» для небольшой двухполосной дороги, и другие небольшие дороги тоже обратятся в тоннели, а большая дорога — нет";
			d["item.general.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же набор переключателей строки «Общее», но в меню «Электричество» уже нет";
			d["item.general.ex.category"] = "переход на шестиполосную дорогу тоже даёт «Туннель», но на мосту или двухпутном тоннеле метро ничего не отмечено, и им нужна собственная настройка, хотя все они находятся в меню «Дороги»";
			d["item.underground.label"] = "Режим тоннеля";
			d["item.underground.desc"] = "Запоминает переключатель Режим тоннеля (Переключить режим тоннеля), который решает, окажется ли размещаемое под землёй или над ней.";
			d["item.underground.ex.group"] = "например, включите режим тоннеля на небольшой двухполосной дороге, и другие небольшие дороги тоже уйдут под землю, а большая дорога — нет";
			d["item.underground.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», всё строится под землёй, но в меню «Электричество» уже нет";
			d["item.underground.ex.category"] = "переход на шестиполосную дорогу тоже уходит под землю, но мост или двойные железнодорожные пути остаются на поверхности и требуют собственной настройки, хотя все они находятся в меню «Дороги»";
			d["item.other.label"] = "Прочее";
			d["item.other.desc"] = "Запоминает прочие строки, которые панель инструмента может записывать: цвета дорог из строки «Цвет».";
			d["item.other.ex.group"] = "например, задайте небольшой двухполосной дороге синий цвет, и другие небольшие дороги тоже станут синими, а большая дорога — нет";
			d["item.other.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же цвет, но в меню «Электричество» уже нет";
			d["item.other.ex.category"] = "переход на шестиполосную дорогу тоже остаётся синим, но мост или двойные железнодорожные пути хранят собственный вариант цвета и требуют собственной настройки, хотя все они находятся в меню «Дороги»";

			// v0.6.2 新增：笔刷（笔刷粗细 / 笔刷强度）从「其它」独立成行
			d["item.brush.label"] = "Кисть";

		// ==================== v0.7.0 新增 / 重写（ru-RU）====================
		d["tab.master"] = "Главный переключатель";
		d["tab.assets"] = "Активы";
		d["tab.functions"] = "Функции";
		d["tab.items"] = "Объекты";
		d["group.domain"] = "Постраничные переключатели";
		d["domain.asset.label"] = "Запоминать настройки инструментов на странице активов";
		d["domain.asset.desc"] = "Охватывает все инструменты на странице «Активы»: запись и восстановление при загрузке переключаются вместе. Пока выключено, все настройки страницы «Активы» становятся недоступными и не действуют, инструменты ведут себя как в оригинале, а уже записанные значения остаются в файле нетронутыми и при повторном включении восстанавливаются.";
		d["domain.function.label"] = "Запоминать настройки инструментов на странице функций";
		d["domain.function.desc"] = "Охватывает все инструменты на странице «Функции» — шесть функциональных групп: типы зон, инструменты областей, инструменты маршрутов, инструменты терраформирования, пространства и области, а также маркеры и префабы объектов. Пока выключено, все настройки страницы «Функции» становятся недоступными и не действуют, инструменты ведут себя как в оригинале, а уже записанные значения остаются в файле нетронутыми и при повторном включении восстанавливаются.";
		d["domain.item.label"] = "Запоминать настройки инструментов на странице объектов";
		d["domain.item.desc"] = "Охватывает все инструменты на странице «Объекты» — восемь категорий объектов меню «Ландшафтный дизайн»: «Природа», «Парк», «Объекты жилой зоны», «Объекты торговой зоны», «Объекты промышленной зоны», «Обломки», «Надписи» и «Растения в горшках». Пока выключено, все настройки страницы «Объекты» становятся недоступными и не действуют, инструменты ведут себя как в оригинале, а уже записанные значения остаются в файле нетронутыми и при повторном включении восстанавливаются.";
		d["item.color.label"] = "Цвет";
		d["item.decoration.label"] = "Заморозить возраст";
		d["item.decoration.desc"] = "Запоминает переключатель «Заморозить возраст»: когда он включён, деревья, которые вы закрашиваете или размещаете, перестают расти и умирать от старости — возраст этой группы деревьев словно замораживается на текущем значении. Эта строка появляется только на активах с данными деревьев (при закрашивании деревьев из меню «Флора»); активы, на панелях которых такой строки нет, не записываются и не меняются.";
		d["item.decoration.ex.category"] = "при переходе к другому виду деревьев он остаётся включённым, но эта строка есть только у активов с данными деревьев, поэтому при закрашивании надписи её нет вовсе (нужна отдельная настройка)";
		d["item.decoration.ex.group"] = "например, включите «Заморозить возраст» при закрашивании деревьев, и другие способы размещения растительности в той же группе тоже останутся включёнными, а в другой группе — нет";
		d["item.decoration.ex.menu"] = "все активы в меню «Флора» остаются с включённым «Заморозить возраст», но в других меню — нет";
		d["item.f.brush.desc"] = "Запоминает строки «Размер кисти» и «Нажим кисти» на функциональных панелях: «Размер кисти» задаёт, какую площадь охватывает одно движение, а «Нажим кисти» — насколько каждое движение меняет результат. Действует только на те инструменты, у которых на панели есть обе строки: девять инструментов терраформирования («Высота рельефа», «Сглаживание рельефа», «Выравнивание рельефа», «Склон», «Инструмент рисования текстуры 01–04» и «Ластик текстуры») и закрашивание зон; функции, у которых этих строк нет, не записываются и не меняются.";
		d["item.f.brush.ex.category"] = "при переходе к другой функциональной группе используется её собственная кисть (нужна отдельная настройка), даже если на обеих панелях есть эти две строки";
		d["item.f.brush.ex.group"] = "например, задайте размер кисти 30 м для одного из инструментов рельефа, и другие инструменты терраформирования в той же группе тоже станут 30 м, а в другой группе — нет";
		d["item.f.brush.ex.menu"] = "все инструменты в группе терраформирования используют одни и те же размер и нажим кисти, а в другой функциональной группе — нет";
		d["item.f.snap.desc"] = "Запоминает каждый переключатель в строке «Привязка» на функциональных панелях: «Привязка к прямым углам», «Привязка к сетке», «Привязка к дорогам», «Привязка к зданию-владельцу», «Показать контурные линии» и так далее — какие переключатели показаны, зависит от инструмента. Собственная кнопка игры «Включить/выключить привязку» является пакетной и не запоминается. Строка «Привязка» на страницах «Активы» и «Объекты» запоминается отдельно, и одна на другую не влияет.";
		d["item.f.snap.ex.category"] = "при переходе к другой функциональной группе возвращаются её собственные переключатели по умолчанию (нужна отдельная настройка), даже если обе группы открываются из одного меню панели инструментов";
		d["item.f.snap.ex.group"] = "например, выключите «Привязку к сетке» для жилой зоны, и в коммерческих, промышленных и офисных зонах она тоже будет выключена, а у инструментов областей — нет";
		d["item.f.snap.ex.menu"] = "все инструменты в одной функциональной группе используют один и тот же набор переключателей, а в другой функциональной группе — нет";
		d["item.f.toolMode.desc"] = "Запоминает выбор в строке «Режим инструмента» на функциональных панелях. Варианты зависят от инструмента: типы зон предлагают «Заливка», «Рамка» и «Кисть»; инструменты областей («Создание района», «Инструмент района с промышленной специализацией», «Инструмент поверхности») — «Изменить» и «Создать сетку карты»; у инструментов терраформирования свои режимы для каждого инструмента; инструмент сноса предлагает «Только основные элементы», «Вместе с дополнительными» и «Всё». Режим инструмента на страницах «Активы» и «Объекты» запоминается отдельно, и один на другой не влияет.";
		d["item.f.toolMode.ex.category"] = "при переходе к другой функциональной группе возвращается её собственный режим (нужна отдельная настройка), даже если обе группы открываются из одного меню панели инструментов";
		d["item.f.toolMode.ex.group"] = "например, выберите «Кисть» для жилой зоны, и коммерческие, промышленные и офисные зоны тоже перейдут в режим «Кисть», а инструменты областей — нет";
		d["item.f.toolMode.ex.menu"] = "все инструменты в одной функциональной группе используют один и тот же режим, а в другой функциональной группе — нет";
		d["item.f.topography.desc"] = "Запоминает строку «Топография» (контурные линии) на функциональных панелях, то есть видны ли контуры рельефа, пока вы рисуете зоны. Эта строка считывается и записывается по страницам: страница «Функции» хранит свою копию для функциональных зон, а страница «Активы» — отдельную, и одна никогда не перезаписывает другую.";
		d["item.f.topography.ex.category"] = "при переходе к другой функциональной группе используется её собственная копия (нужна отдельная настройка); строка «Топография» на странице «Активы» — отдельная копия, и она от этой не зависит";
		d["item.f.topography.ex.group"] = "например, включите контурные линии для жилой зоны, и в коммерческих, промышленных и офисных зонах они тоже будут включены";
		d["item.f.topography.ex.menu"] = "во всех инструментах одной функциональной группы контурные линии включены, а в другой функциональной группе — нет";
		d["item.f.underground.desc"] = "Запоминает строку «Режим тоннеля» на функциональных панелях. Эта строка считывается и записывается по страницам: страница «Функции» хранит свою копию для функциональных зон, а страницы «Активы» и «Объекты» — каждая свою, и они не делятся между собой.";
		d["item.f.underground.ex.category"] = "при переходе к другой функциональной группе используется её собственная копия (нужна отдельная настройка); строка «Режим тоннеля» на странице «Активы» — отдельная копия, и она от этой не зависит";
		d["item.f.underground.ex.group"] = "например, включите режим тоннеля для жилой зоны, и коммерческие, промышленные и офисные зоны тоже уйдут под землю";
		d["item.f.underground.ex.menu"] = "все инструменты в одной функциональной группе находятся в режиме тоннеля, а в другой функциональной группе — нет";
		d["item.i.brush.desc"] = "Запоминает строки «Размер кисти» и «Нажим кисти» на панелях объектов, то есть какую площадь охватывает одно движение кисти и сколько объектов размещается за один проход. Действует только на те объекты, у которых на панели есть обе строки; объекты без этих строк не записываются и не меняются.";
		d["item.i.brush.ex.category"] = "при переходе к другой категории объектов используется её собственная кисть (нужна отдельная настройка), даже если обе находятся в меню «Ландшафтный дизайн»";
		d["item.i.brush.ex.group"] = "например, задайте размер кисти 20 м, и другие размещаемые кистью объекты в той же группе тоже станут 20 м, а в другой группе — нет";
		d["item.i.brush.ex.menu"] = "все размещаемые кистью объекты в меню «Ландшафтный дизайн» используют одну и ту же кисть, а в другом меню — нет";
		d["item.i.snap.desc"] = "Запоминает каждый переключатель в строке «Привязка» на панелях объектов: «Привязка к поверхности объекта», «Привязка к ближайшим формам», «Привязка к дорогам», «Удалить только определенный тип» и так далее — какие переключатели показаны, зависит от объекта. Строка «Привязка» на страницах «Активы» и «Функции» запоминается отдельно, и одна на другую не влияет.";
		d["item.i.snap.ex.category"] = "при переходе к другой категории объектов возвращаются её собственные переключатели по умолчанию (нужна отдельная настройка), даже если обе находятся в меню «Ландшафтный дизайн»";
		d["item.i.snap.ex.group"] = "например, выключите «Привязку к поверхности объекта» для объекта категории «Парк», и у других объектов в той же группе она тоже будет выключена, а в другой группе — нет";
		d["item.i.snap.ex.menu"] = "все объекты в меню «Ландшафтный дизайн» используют один и тот же набор переключателей, а в другом меню — нет";
		d["item.i.toolMode.desc"] = "Запоминает выбор в строке «Режим инструмента» на панелях объектов: «Разместить один», «Разместить несколько», «Линия», «Кривая», «Инструмент копирования объекта», «Кисть» и так далее — в зависимости от объекта. Режим инструмента на страницах «Активы» и «Функции» запоминается отдельно, и один на другой не влияет.";
		d["item.i.toolMode.ex.category"] = "при переходе к другой категории объектов, скажем от «Природы» к «Надписям», возвращается её собственный режим (нужна отдельная настройка), даже если обе находятся в меню «Ландшафтный дизайн»";
		d["item.i.toolMode.ex.group"] = "например, выберите «Разместить несколько» для объекта категории «Природа», и другие объекты в той же группе тоже будут в режиме «Разместить несколько», а в другой группе — нет";
		d["item.i.toolMode.ex.menu"] = "все объекты в меню «Ландшафтный дизайн» находятся в одном и том же режиме, а в другом меню — нет";
		d["item.i.topography.desc"] = "Запоминает строку «Топография» (контурные линии) на панелях объектов. Эта строка считывается и записывается по страницам: страница «Объекты» хранит свою копию для объектов, а страницы «Активы» и «Функции» — каждая свою, и они не делятся между собой.";
		d["item.i.topography.ex.category"] = "при переходе к другой категории объектов используется её собственная копия (нужна отдельная настройка); строки «Топография» на страницах «Активы» и «Функции» — отдельные копии, и они от этой не зависят";
		d["item.i.topography.ex.group"] = "например, включите контурные линии при размещении объектов, и у других объектов в той же группе они тоже будут включены";
		d["item.i.topography.ex.menu"] = "все объекты в меню «Ландшафтный дизайн» остаются с включёнными контурными линиями, а в другом меню — нет";
		d["item.i.underground.desc"] = "Запоминает строку «Режим тоннеля» на панелях объектов, то есть размещаются ли объекты под землёй или над ней. Эта строка считывается и записывается по страницам: страница «Объекты» хранит свою копию для объектов, а страницы «Активы» и «Функции» — каждая свою, и они не делятся между собой.";
		d["item.i.underground.ex.category"] = "при переходе к другой категории объектов используется её собственная копия (нужна отдельная настройка); строки «Режим тоннеля» на страницах «Активы» и «Функции» — отдельные копии, и они от этой не зависят";
		d["item.i.underground.ex.group"] = "например, включите режим тоннеля для одного объекта, и другие объекты в той же группе тоже уйдут под землю";
		d["item.i.underground.ex.menu"] = "все объекты в меню «Ландшафтный дизайн» уходят под землю, а в другом меню — нет";
		d["scope.category.function"] = "Тот же тип";
		d["scope.category.item"] = "Тот же тип объекта";
		d["scope.line.category.function"] = "Тот же тип: группировка по одной из шести функциональных групп, к которым относится инструмент, — типы зон (жилые, коммерческие, промышленные, офисные), инструменты областей («Создание района», «Инструмент района с промышленной специализацией», «Инструмент поверхности»), инструменты маршрутов (все маршруты в меню «Транспорт»), девять инструментов терраформирования («Высота рельефа», «Сглаживание рельефа», «Выравнивание рельефа», «Склон», «Инструмент рисования текстуры 01–04», «Ластик текстуры»), пространства и области («Парковка», «Пешеходная зона», «Область ожидания», «Свободная область», «Обрезка поверхности») и маркеры с префабами объектов (инструмент появления животных и подобные): {0}";
		d["scope.line.category.item"] = "Тот же тип объекта: группировка по одной из восьми категорий объектов меню «Ландшафтный дизайн», к которой принадлежит элемент («Природа», «Парк», «Объекты жилой зоны», «Объекты торговой зоны», «Объекты промышленной зоны», «Обломки», «Надписи», «Растения в горшках»); другая модель или другой размер внутри одной категории не образуют другой тип: {0}";
		d["enabled.label"] = "Включить мод";
		d["enabled.desc"] = "По умолчанию включено. Пока это включено, все расположенные ниже настройки действуют, а значения всех инструментов записываются по мере работы, поэтому при возврате к сохранению панели оказываются точно такими, какими вы их оставили; отключение одного пункта лишь прекращает его восстановление — его значения продолжают записываться. Выключение этого главного переключателя возвращает всё к оригиналу: запись прекращается, восстановление не выполняется, а все настройки на страницах «Активы», «Функции» и «Объекты» одновременно отключаются (становятся серыми), и каждый инструмент ведёт себя так, как задумано в игре. Он выключается сам, когда все три расположенных ниже переключателя страниц выключены, и снова включается, как только включается любой из них.";
		d["scope.line.sharedSingle"] = "Глобально: все инструменты на этой странице, поддерживающие данный пункт, используют одно значение";
		d["scope.note"] = "Примечание: страницы «Активы», «Функции» и «Объекты» хранят память каждую отдельно, даже если строка называется одинаково на нескольких страницах; «Глобально» и «Без разделения» действуют только в пределах страницы, которую вы просматриваете.";
		d["item.brush.desc"] = "Запоминает строки «Размер кисти» и «Нажим кисти»: размер определяет, какую площадь охватывает одно движение, а нажим — насколько каждое движение меняет результат. Этот пункт касается только закрашивания на странице «Активы», то есть размещения кистью таких активов, как деревья из меню «Флора»; у девяти инструментов терраформирования есть собственный пункт кисти на странице «Функции», у восьми категорий объектов — свой на странице «Объекты», и три страницы никогда не делят значения. Инструменты, на панелях которых нет этих строк, не записываются и не меняются.";
		d["item.brush.ex.group"] = "например, задайте размер кисти 30 м при закрашивании деревьев, и другие размещаемые кистью активы в той же группе тоже станут 30 м, а актив из другой группы — нет";
		d["item.brush.ex.menu"] = "все активы, закрашиваемые в меню «Флора», используют одни и те же размер и нажим кисти, а в другом меню — нет";
		d["item.brush.ex.category"] = "при переходе к другому виду деревьев используется та же кисть, но этот уровень группирует активы по тому, кому они служат, поэтому инструменты терраформирования и объекты хранят каждый свою копию на двух других страницах (нужна отдельная настройка)";
			return d;
		}
	}
}
