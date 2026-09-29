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
			d["group.compat"] = "Zgodność";
			d["group.about"] = "Informacje i linki";

			d["enabled.label"] = "Włącz pamięć trybu narzędzia";
			d["enabled.desc"] = "Domyślnie włączone. Dopóki to włączone, każda wartość jest zapisywana na bieżąco, więc po powrocie do zapisu panele wyglądają tak, jak je zostawiłeś. Wyłączenie pojedynczej pozycji tylko ją wyłącza z przywracania - jej wartości nadal są zapisywane. Dopiero wyłączenie tego głównego przełącznika zatrzymuje zapisowanie i każde narzędzie wraca do zachowania z gry.";
			d["compat.label"] = "Zgodność z innymi modami";
			d["compat.desc"] = "Domyślnie włączone. Włączone: pamięć używa nazw menu i grup w postaci, w jakiej dostosowały je inne mody (Asset UI Manager, ExtraLib i podobne), więc przeniesiony zasób podąża za nowym miejscem. Wyłączone: zachowywana jest klasyfikacja widziana po raz pierwszy, co jest stabilniejsze. Uwaga: własny zasób, którego autor nie przypisał go do właściwego typu zasobu, nie może być zmieniany razem z tym typem. W obu przypadkach już zapisane wartości nie giną.";
			d["scope.label"] = "Zakres współdzielenia";
			d["scope.desc"] = "Jak szeroko współdzielona jest wartość tej pozycji. Nawiasy na liście rozwijanej pokazują, co robi gra oryginalna i co polecamy.";
			d["scope.line.group"] = "Ta sama grupa: {0}";
			d["scope.line.menu"] = "To samo menu: {0}";
			d["scope.line.category"] = "Ten sam typ zasobu: tylko zasoby lub funkcje z tej samej podkategorii współdzielą ustawienie, czyli {0}";
			d["scope.line.shared"] = "Globalnie: każdy zasób i każda funkcja obsługujące tę pozycję współdzielą jedną wartość, ale zasoby i funkcje nie współdzielą jej między sobą";
			d["scope.line.unique"] = "Bez współdzielenia: każdy zasób i każda funkcja obsługujące tę pozycję są ustawiane zupełnie niezależnie";
			d["scope.note"] = "Uwaga: przez funkcje rozumie się tu Strefy, Przestrzenie i obszary, Terraformowanie, Znacznik i prefaby obiektów, czyli narzędzia, które nie są zasobami";

			d[kScopeGroup] = "Ta sama grupa";
			d[kScopeMenu] = "To samo menu";
			d[kScopeCategory] = "Ten sam typ zasobu";
			d[kScopeGlobalShared] = "Globalnie";
			d[kScopeGlobalUnique] = "Bez współdzielenia";
			d[kTagVanilla] = " (oryginał)";
			d[kTagRecommended] = " (zalecane)";
			d[kTagRecVanilla] = " (zalecane, oryginał)";

			d["reset.label"] = "Resetuj pamięć";
			d["reset.desc"] = "W grze: kasuje pamięć tego zapisu i przywraca narzędzia do stanu oryginalnego z wczytania. W menu głównym: kasuje pamięć wszystkich zapisów.";
			d["reset.warn"] = "Nie można cofnąć.";
			d["reset.confirm"] = "Zresetować zapamiętane ustawienia narzędzi?";
			d["resetall.label"] = "Zresetuj wszystkie ustawienia";
			d["resetall.desc"] = "Przywraca każdą opcję tego moda do zalecanej wartości domyślnej: przełącznik główny i przełącznik zgodności znowu się włączają, a stan włączenia i zakres współdzielenia każdej pozycji też zostają przywrócone. Pamięć już zapisana dla zapisów gry pozostaje nienaruszona.";
			d["resetall.warn"] = "Nie można cofnąć.";
			d["resetall.confirm"] = "Zresetować wszystkie ustawienia tego moda do wartości zalecanych?";
			d["folder.label"] = "Zarządzaj plikami pamięci wszystkich zapisów";
			d["folder.desc"] = "Otwiera lokalny folder z plikami pamięci, jednym na zapis, nazwanym jak zapis.";

			d["about.version"] = "Wersja moda";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Wesprzyj autora na Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Otwórz wątek na forum Paradox.";
			d["about.rainbow"] = "RAINBOW官网";
			d["about.rainbow.desc"] = "Otwórz stronę serii Rainbow.";

			d["item.anarchy.desc"] = "Zapamiętuje stan przełącznika Anarchy dodawanego przez mod Anarchy (mod Paradox 74604): gdy jest włączony, stawianie obiektu nie wykonuje już sprawdzeń nakładania i podobnych ograniczeń. Ten przycisk pochodzi z tamtego moda, nie z gry; bez Anarchy ta pozycja nic nie robi.";
			d["item.anarchy.ex.group"] = "na przykład włączysz Anarchy dla małej dwupasmowej drogi i inne małe drogi też będą mieć Anarchy włączone, a duża droga nie";
			d["item.anarchy.ex.menu"] = "każdy zasób kliknięty w menu Drogi ma włączone Anarchy, ale po przejściu do menu Elektryczność już nie";
			d["item.anarchy.ex.category"] = "przełączenie na dużą drogę też zostaje z włączonym Anarchy, ale most wraca do wyłączonego i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.themes.label"] = "Motyw";
			d["item.themes.desc"] = "Zapamiętuje, które pozycje są zaznaczone w filtrze Motyw na pasku narzędzi. Ten wiersz pojawia się tylko wtedy, gdy bieżąca kategoria naprawdę zawiera zasoby używające motywów, a zaznaczenie jednego ogranicza listę na pasku narzędzi do zasobów obsługujących te motywy. Oryginał resetuje ten filtr do domyślnego motywu tylko przy wczytaniu zapisu, więc ta pozycja jest domyślnie wyłączona.";
			d["item.themes.ex.group"] = "na przykład zaznaczysz motyw tylko w Drogi/Małe drogi, więc duże drogi już według niego nie filtrują";
			d["item.themes.ex.menu"] = "motyw zaznaczony w menu Drogi przestaje działać w menu Elektryczność i wraca, gdy tam wrócisz";
			d["item.themes.ex.category"] = "uliczki i tory metra liczą się jako ten sam typ zasobu, więc zaznaczony motyw jest współdzielony między menu";
			d["item.packs.label"] = "Pakiet";
			d["item.packs.desc"] = "Zapamiętuje, które pozycje są zaznaczone w filtrze Pakiet na pasku narzędzi; zaznaczenie jednego ogranicza listę na pasku narzędzi do zasobów należących do tych pakietów. Oryginał czyści ten filtr przy każdej zmianie menu lub kategorii, więc zapamiętywanie go dla tego samego menu i kategorii odpowiada temu, co rzeczywiście widzisz, i ta pozycja też jest domyślnie wyłączona.";
			d["item.packs.ex.group"] = "na przykład zaznaczysz pakiet w Drogi/Małe drogi, oryginał wyczyści go przy przejściu na duże drogi, a po powrocie znów jest zaznaczony";
			d["item.packs.ex.menu"] = "pakiety zaznaczone w menu Drogi i pakiety zaznaczone w menu Elektryczność są zapamiętywane osobno";
			d["item.packs.ex.category"] = "uliczki są w menu Drogi i w menu Dzielnice pod tą samą nazwą, więc zaznaczone pakiety tworzą jedną współdzieloną listę";
			d["item.toolMode.label"] = "Narzędzia";
			d["item.toolMode.desc"] = "Zapamiętuje tryb wybrany dla każdego narzędzia, a opcje różnią się między nimi: drogi, tory i rury oferują Prosta, Jeden zakręt, Dwa zakręty, Ciągła, Siatka, Zastąp, Punkt; budynki, obiekty i drzewa oferują Umieść jeden, Umieść wiele, Linia, Krzywa, Stempel; strefy oferują Wypełnienie, Zaznaczanie obszarowe, Pędzel; obszary oferują Edytuj, Generuj siatkę mapy. Tryby zasobów i tryby funkcji zapamiętywane są osobno, bo ich opcje nie są takie same.";
			d["item.toolMode.ex.group"] = "na przykład ustawisz Jeden zakręt dla małej dwupasmowej drogi i inne małe drogi też będą mieć Jeden zakręt, a duża droga nie";
			d["item.toolMode.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest na trybie Jeden zakręt, ale po przejściu do menu Elektryczność już nie";
			d["item.toolMode.ex.category"] = "przełączenie na dużą drogę nadal daje Jeden zakręt, ale most wraca do swojego domyślnego trybu i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.elevation.label"] = "Wzniesienie";
			d["item.elevation.desc"] = "Zapamiętuje wysokość, na jakiej zostawiono narzędzie, wraz z wynikiem Zwiększ wzniesienie i Zmniejsz wzniesienie. Stopień wzniesienia, czyli o ile przesuwa jedno naciśnięcie Zwiększ wzniesienie albo Zmniejsz wzniesienie (w panelu moda Anarchy ten wiersz nazywa się tak samo), jest zapamiętywany razem z wysokością i używa tego samego zakresu współdzielenia. Wysokość skrzyżowań i węzłów nie jest objęta.";
			d["item.elevation.ex.group"] = "na przykład podniesiesz małą dwupasmową drogę do 10 m i inne małe drogi też pójdą na 10 m, a duża droga nie będzie na 10 m";
			d["item.elevation.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest na 10 m, ale po przejściu do menu Elektryczność nie jest na 10 m";
			d["item.elevation.ex.category"] = "przełączenie na dużą drogę nadal daje 10 m, ale most wraca do 0 m i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.parallel.label"] = "Tryb równoległy";
			d["item.parallel.desc"] = "Zapamiętuje przełącznik Tryb równoległy (Włącz/wyłącz tryb równoległy) razem z liczbą Równoległa droga i Równoległa kompensacja, które zmienia Zwiększ kompensację i Zmniejsz kompensację. Pozwala budować równoległe sieci.";
			d["item.parallel.ex.group"] = "na przykład włączysz Tryb równoległy dla małej dwupasmowej drogi i ustawisz liczbę na 3, inne małe drogi też zbudują 3, a duża droga nie";
			d["item.parallel.ex.menu"] = "każdy zasób kliknięty w menu Drogi buduje 3 równoległe linie, ale po przejściu do menu Elektryczność nie";
			d["item.parallel.ex.category"] = "przełączenie na dużą drogę nadal buduje 3, ale most wraca do trybu wyłączonego i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.snap.label"] = "Przyciąganie";
			d["item.snap.desc"] = "Zapamiętuje każdy przełącznik wiersza Przyciąganie: Przyciągaj do istniejących kształtów, Przyciągaj do wielkości pól w strefach, Przyciągaj do kątów prostych, Przyciągaj do stron drogi, Przyciągaj do dróg, Przyciągaj do strony właściciela, Przyciągaj do boków budynku, Przyciągaj do środka drogi, Przyciągaj do linii brzegowej, Przyciągaj do pobliskich kształtów, Przyciągaj do linii pomocniczych, Przyciągaj do siatki w strefie, Przyciągaj do węzłów, Przyciągaj do powierzchni obiektu, Przyciągaj do góry, Przyciągaj do siatki wysypiska, Wiąże nakładające się obiekty z budynkiem, Usuń tylko pasujący typ, Pokaż linie konturów, Przyciągaj do odległości. Wiersz Wł./wył. przyciąganie z gry to przełącznik zbiorczy i nie jest zapamiętywany.";
			d["item.snap.ex.group"] = "na przykład wyłączysz «Przyciągaj do stron drogi» dla małej dwupasmowej drogi i inne małe drogi też będą to mieć wyłączone, a duża droga nadal będzie mieć wszystko włączone";
			d["item.snap.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego zestawu przełączników przyciągania, ale po przejściu do menu Elektryczność nie";
			d["item.snap.ex.category"] = "przełączenie na dużą drogę zachowuje ten sam zestaw, ale most wraca do ustawień domyślnych i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Zapamiętuje wiersz Topografia, czyli czy Pokaż linie konturów jest włączone.";
			d["item.topography.ex.group"] = "na przykład włączysz Topografię dla małej dwupasmowej drogi i inne małe drogi też będą ją mieć włączoną, a duża droga nie";
			d["item.topography.ex.menu"] = "każdy zasób kliknięty w menu Drogi pokazuje linie konturów, ale po przejściu do menu Elektryczność nie";
			d["item.topography.ex.category"] = "przełączenie na dużą drogę nadal je pokazuje, ale most ma ten wiersz wyłączony i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.leftRight.label"] = "Po lewej i Po prawej";
			d["item.leftRight.desc"] = "Zapamiętuje, które ulepszenia sieci wybrano w wierszach «Po lewej» i «Po prawej» panelu sieci moda Anarchy, na przykład Pas dla rowerów, Drzewa, Parking, Nabrzeże, Ściana oporowa czy Bariera akustyczna. Wymaga Anarchy (mod Paradox 74604); bez niego ta pozycja nic nie robi.";
			d["item.leftRight.ex.group"] = "na przykład wybierzesz Pas dla rowerów po prawej stronie małej dwupasmowej drogi i inne małe drogi też go tam dostaną, a duża droga nie";
			d["item.leftRight.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tej samej pary wyborów stron, ale po przejściu do menu Elektryczność nie";
			d["item.leftRight.ex.category"] = "przełączenie na dużą drogę nadal daje Pas dla rowerów po prawej, ale most wraca do braku wyboru i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.general.label"] = "Ogólnie";
			d["item.general.desc"] = "Zapamiętuje przełączniki z wiersza «Ogólnie» panelu sieci moda Anarchy: Podłoże, Podwyższenie, Tunel, Stałe nachylenie, Szeroka mediana i Rozszerzony zasięg wysokości (ten ostatni znosi limit wysokości). Wymaga Anarchy (mod Paradox 74604); bez niego ta pozycja nic nie robi.";
			d["item.general.ex.group"] = "na przykład wybierzesz Tunel dla małej dwupasmowej drogi i inne małe drogi też zostaną zamienione w tunele, a duża droga nie";
			d["item.general.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego zestawu przełączników wiersza Ogólnie, ale po przejściu do menu Elektryczność nie";
			d["item.general.ex.category"] = "przełączenie na dużą drogę nadal daje Tunel, ale most nie ma nic zaznaczonego i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.underground.label"] = "Tryb podziemny";
			d["item.underground.desc"] = "Zapamiętuje przełącznik Tryb podziemny (Włącz/wyłącz tryb podziemny), który decyduje, czy stawiasz coś pod ziemią, czy nad nią.";
			d["item.underground.ex.group"] = "na przykład włączysz Tryb podziemny dla małej dwupasmowej drogi i inne małe drogi też zejdą pod ziemię, a duża droga nie";
			d["item.underground.ex.menu"] = "każdy zasób kliknięty w menu Drogi jest budowany pod ziemią, ale po przejściu do menu Elektryczność nie";
			d["item.underground.ex.category"] = "przełączenie na dużą drogę nadal schodzi pod ziemię, ale most zostaje na powierzchni i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
			d["item.other.label"] = "Inne";
			d["item.other.desc"] = "Zapamiętuje pozostałe wiersze, które panel narzędzia może zapisywać: kolory dróg z wiersza Kolor oraz Rozmiar pędzla i Nacisk pędzla.";
			d["item.other.ex.group"] = "na przykład ustawisz kolor małej dwupasmowej drogi na niebieski i inne małe drogi też zrobią się niebieskie, a duża droga nie";
			d["item.other.ex.menu"] = "każdy zasób kliknięty w menu Drogi używa tego samego koloru i ustawienia pędzla, ale po przejściu do menu Elektryczność nie";
			d["item.other.ex.category"] = "przełączenie na dużą drogę nadal jest niebieskie, ale most wraca do domyślnego koloru i wymaga własnego ustawienia, choć obie sieci są w menu Drogi";
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
			d["group.compat"] = "Compatibilidade";
			d["group.about"] = "Informações e links";

			d["enabled.label"] = "Ativar Memória do modo de ferramenta";
			d["enabled.desc"] = "Ligado por padrão. Enquanto estiver ligado, todos os valores ficam sendo gravados sem parar, então ao voltar a um jogo os painéis estão exatamente como você deixou. Desligar um item só impede que ele seja restaurado; os valores dele continuam sendo gravados. Só desligando este interruptor geral a gravação para e cada ferramenta volta ao comportamento original.";
			d["compat.label"] = "Compatível com outros mods";
			d["compat.desc"] = "Ligado por padrão. Ligado: a memória usa os nomes de menu e grupo conforme ajustados por outros mods (Asset UI Manager, ExtraLib e semelhantes), então um ativo que você moveu segue o novo lugar. Desligado: mantém a classificação que este mod viu primeiro, o que é mais estável. Note que um ativo personalizado cujo autor não o arquivou no tipo de ativo correto não pode ser ajustado junto com esse tipo. Nenhuma das duas escolhas apaga valores já gravados.";
			d["scope.label"] = "Âmbito de partilha";
			d["scope.desc"] = "Até que ponto o valor deste item é partilhado. Os parênteses na lista suspensa mostram o que o jogo original faz e o que recomendamos.";
			d["scope.line.group"] = "Mesmo grupo: {0}";
			d["scope.line.menu"] = "Mesmo menu: {0}";
			d["scope.line.category"] = "Mesmo tipo de ativo: só ativos ou funções da mesma subcategoria partilham a definição, ou seja, {0}";
			d["scope.line.shared"] = "Global: cada ativo e cada função que suportam este item partilham um valor, mas ativos e funções não partilham entre si";
			d["scope.line.unique"] = "Sem partilhar: cada ativo e cada função que suportam este item são definidos de forma totalmente independente";
			d["scope.note"] = "Nota: por funções entende-se Zonas, Espaços e Áreas, Terraformação, Marcador e objetos pré-fabricados, isto é, as ferramentas que não são ativos";

			d[kScopeGroup] = "Mesmo grupo";
			d[kScopeMenu] = "Mesmo menu";
			d[kScopeCategory] = "Mesmo tipo de ativo";
			d[kScopeGlobalShared] = "Global";
			d[kScopeGlobalUnique] = "Sem partilhar";
			d[kTagVanilla] = " (original)";
			d[kTagRecommended] = " (recomendado)";
			d[kTagRecVanilla] = " (recomendado, original)";

			d["reset.label"] = "Repor memória";
			d["reset.desc"] = "No jogo: apaga a memória deste jogo e devolve as ferramentas ao estado original do carregamento. No menu principal: apaga a memória de todos os jogos.";
			d["reset.warn"] = "Não pode ser desfeito.";
			d["reset.confirm"] = "Repor as definições de ferramenta memorizadas?";
			d["resetall.label"] = "Repor todas as definições";
			d["resetall.desc"] = "Devolve cada opção deste mod ao seu padrão recomendado: o interruptor geral e o de compatibilidade voltam a ligar, e o estado de ativação e o âmbito de partilha de cada item também são restaurados. A memória já gravada dos jogos não é afetada.";
			d["resetall.warn"] = "Não pode ser desfeito.";
			d["resetall.confirm"] = "Repor todas as definições deste mod aos valores recomendados?";
			d["folder.label"] = "Gerir os arquivos de memória de todos os jogos";
			d["folder.desc"] = "Abre a pasta local com os arquivos de memória, um por jogo, com o nome do jogo.";

			d["about.version"] = "Versão do mod";
			d["about.author"] = "Autor";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Apoie o autor no Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Abrir a conversa no fórum da Paradox.";
			d["about.rainbow"] = "RAINBOW官网";
			d["about.rainbow.desc"] = "Abrir o site da série Rainbow.";

			d["item.anarchy.desc"] = "Lembra o estado do botão Anarchy que o mod Anarchy (mod 74604 da Paradox) acrescenta: com ele ligado, colocar um objeto já não faz as verificações de sobreposição e semelhantes. O botão vem daquele mod, não do jogo; sem o Anarchy isto não faz nada.";
			d["item.anarchy.ex.group"] = "por exemplo, você liga o Anarchy em uma rua pequena de 2 faixas e as outras ruas pequenas também ficam com ele ligado, enquanto uma via grande não";
			d["item.anarchy.ex.menu"] = "todo ativo que você clica no menu Vias tem o Anarchy ligado, mas ao mudar para o menu Eletricidade já não";
			d["item.anarchy.ex.category"] = "mudar para uma via grande ainda o deixa ligado, mas uma ponte volta a desligada e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.themes.label"] = "Tema";
			d["item.themes.desc"] = "Lembra quais entradas estão marcadas no filtro Tema da barra de ferramentas. Essa linha só aparece quando a categoria atual realmente contém ativos que usam temas, e marcar um deles limita a lista da barra de ferramentas aos ativos que suportam esses temas. O original só devolve esse filtro ao tema padrão ao carregar um jogo, então este item já vem desligado.";
			d["item.themes.ex.group"] = "por exemplo, você marca um tema só em Vias/Ruas, então as vias grandes não filtram por ele";
			d["item.themes.ex.menu"] = "um tema marcado no menu Vias deixa de valer no menu Eletricidade e volta quando você retorna";
			d["item.themes.ex.category"] = "becos e trilhos do metrô contam como o mesmo tipo de ativo, então o tema marcado é partilhado entre os menus";
			d["item.packs.label"] = "Pacote";
			d["item.packs.desc"] = "Lembra quais entradas estão marcadas no filtro Pacote da barra de ferramentas; marcar um deles limita a lista da barra de ferramentas aos ativos que pertencem a esses pacotes. O original limpa essa seleção toda vez que você troca de menu ou de categoria, então lembrá-la por menu e categoria é o que corresponde ao que você vê, e este item também já vem desligado.";
			d["item.packs.ex.group"] = "por exemplo, você marca um pacote em Vias/Ruas, o original limpa essa marcação nas vias grandes, e ela volta quando você retorna";
			d["item.packs.ex.menu"] = "os pacotes marcados no menu Vias e os marcados no menu Eletricidade são lembrados à parte";
			d["item.packs.ex.category"] = "os becos aparecem nos menus Vias e Distritos com o mesmo nome, então os pacotes marcados formam um único conjunto partilhado";
			d["item.toolMode.label"] = "Ferramentas";
			d["item.toolMode.desc"] = "Lembra o modo escolhido para cada ferramenta, e as opções mudam conforme a ferramenta: estradas, trilhos e canos oferecem Reta, Curva simples, Curva complexa, Contínuo, Grade, Substituir, Ponto; prédios, acessórios e árvores oferecem Colocar um, Colocar vários, Linha, Curva, Ferramenta de carimbo de objetos; as zonas oferecem Preencher, Marcar, Pintar; as áreas oferecem Edição, Gerar grade de mapa. Os modos de ativos e os de funções são lembrados à parte porque as opções não são as mesmas.";
			d["item.toolMode.ex.group"] = "por exemplo, você escolhe Curva simples em uma rua pequena de 2 faixas e as outras ruas pequenas também usam Curva simples, enquanto uma via grande não";
			d["item.toolMode.ex.menu"] = "todo ativo que você clica no menu Vias está em Curva simples, mas ao mudar para o menu Eletricidade já não";
			d["item.toolMode.ex.category"] = "mudar para uma via grande ainda dá Curva simples, mas uma ponte volta ao seu modo padrão e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.elevation.label"] = "Elevação";
			d["item.elevation.desc"] = "Lembra a elevação em que a ferramenta ficou, incluindo o resultado de Aumentar elevação e Reduzir elevação. O Passo de elevação, o quanto se anda com um toque em Aumentar elevação ou Reduzir elevação (no painel do Anarchy essa linha tem o mesmo nome), é lembrado junto com a elevação e usa o mesmo âmbito de partilha. A elevação de cruzamentos e passagens não é coberta.";
			d["item.elevation.ex.group"] = "por exemplo, você sobe uma rua de 2 faixas a 10 m e as outras ruas pequenas também vão a 10 m, enquanto uma via grande não está a 10 m";
			d["item.elevation.ex.menu"] = "todo ativo que você clica no menu Vias está a 10 m, mas ao mudar para o menu Eletricidade não está a 10 m";
			d["item.elevation.ex.category"] = "mudar para uma via grande ainda dá 10 m, mas uma ponte volta a 0 m e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.parallel.label"] = "Modo Paralelo";
			d["item.parallel.desc"] = "Lembra o interruptor Modo Paralelo (Alternar modo paralelo) junto com a quantidade de Via paralela e o Contraste paralelo que Aumentar contraste e Reduzir contraste alteram. Permite construir redes paralelas.";
			d["item.parallel.ex.group"] = "por exemplo, você liga o Modo Paralelo em uma rua pequena de 2 faixas e põe a quantidade em 3, as outras ruas pequenas também constroem 3, enquanto uma via grande não";
			d["item.parallel.ex.menu"] = "todo ativo que você clica no menu Vias constrói 3 linhas paralelas, mas ao mudar para o menu Eletricidade não";
			d["item.parallel.ex.category"] = "mudar para uma via grande ainda constrói 3, mas uma ponte volta a desligada e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.snap.label"] = "Aderir";
			d["item.snap.desc"] = "Lembra cada interruptor da linha Aderir: Aderir à geometria existente, Aderir ao tamanho da célula da zona, Aderir a ângulos de 90 graus, Aderir aos lados de uma via, Aderir a vias, Aderir ao lado do proprietário, Aderir aos lados de uma estrutura, Aderir ao meio de uma via, Aderir à costa, Aderir a geometria próxima, Aderir a linhas guia, Aderir à grade da zona, Aderir aos nodos, Aderir à superfície de um objeto, Aderir verticalmente, Aderir à grade do lote, Vincula itens sobrepostos a uma estrutura, Remover apenas o tipo correspondente, Exibir linhas de contorno, Aderir à distância. A linha Alternar modo Aderir do jogo é um interruptor em lote e não é lembrada.";
			d["item.snap.ex.group"] = "por exemplo, você desliga «Aderir aos lados de uma via» em uma rua pequena de 2 faixas e as outras ruas pequenas também ficam sem ela, enquanto uma via grande ainda tem tudo ligado";
			d["item.snap.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo conjunto de interruptores da linha Aderir, mas ao mudar para o menu Eletricidade não";
			d["item.snap.ex.category"] = "mudar para uma via grande mantém o mesmo conjunto, mas uma ponte volta aos valores padrão e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.topography.label"] = "Topografia";
			d["item.topography.desc"] = "Lembra a linha Topografia, ou seja, se Exibir linhas de contorno está ligado.";
			d["item.topography.ex.group"] = "por exemplo, você liga a Topografia em uma rua pequena de 2 faixas e as outras ruas pequenas também a têm ligada, enquanto uma via grande não";
			d["item.topography.ex.menu"] = "todo ativo que você clica no menu Vias mostra as linhas de contorno, mas ao mudar para o menu Eletricidade não";
			d["item.topography.ex.category"] = "mudar para uma via grande ainda as mostra, mas uma ponte tem a linha desligada e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.leftRight.label"] = "Esquerda e Direita";
			d["item.leftRight.desc"] = "Lembra as redes laterais escolhidas nas linhas «Esquerda» e «Direita» do painel de redes do Anarchy, por exemplo Ciclovia, Árvores, Estacionamento, Cais, Muro de contenção e Barreira de som. Precisa do Anarchy (mod 74604 da Paradox); sem ele isto não faz nada.";
			d["item.leftRight.ex.group"] = "por exemplo, você escolhe Ciclovia no lado direito de uma rua pequena de 2 faixas e as outras ruas pequenas também a recebem à direita, enquanto uma via grande não";
			d["item.leftRight.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo par de seleções laterais, mas ao mudar para o menu Eletricidade não";
			d["item.leftRight.ex.category"] = "mudar para uma via grande ainda tem Ciclovia à direita, mas uma ponte volta sem nada escolhido e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.general.label"] = "Geral";
			d["item.general.desc"] = "Lembra os interruptores da linha «Geral» do painel de redes do Anarchy: Solo, Elevated, Tunnel, Inclinação constante, Wide Median e Expanded Elevation Range (este último tira o limite de altura). Precisa do Anarchy (mod 74604 da Paradox); sem ele isto não faz nada.";
			d["item.general.ex.group"] = "por exemplo, você escolhe Tunnel em uma rua pequena de 2 faixas e as outras ruas pequenas também são forçadas a virar túneis, enquanto uma via grande não";
			d["item.general.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo conjunto de interruptores da linha Geral, mas ao mudar para o menu Eletricidade não";
			d["item.general.ex.category"] = "mudar para uma via grande ainda dá Tunnel, mas uma ponte volta sem nada marcado e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.underground.label"] = "Modo Subterrâneo";
			d["item.underground.desc"] = "Lembra o interruptor Modo Subterrâneo (Alternar modo subterrâneo), que decide se o que você coloca fica abaixo ou acima do solo.";
			d["item.underground.ex.group"] = "por exemplo, você liga o Modo Subterrâneo em uma rua pequena de 2 faixas e as outras ruas pequenas também vão para baixo do solo, enquanto uma via grande não";
			d["item.underground.ex.menu"] = "todo ativo que você clica no menu Vias é construído debaixo da terra, mas ao mudar para o menu Eletricidade não";
			d["item.underground.ex.category"] = "mudar para uma via grande ainda vai para o subsolo, mas uma ponte fica na superfície e precisa do próprio ajuste, embora as duas estejam no menu Vias";
			d["item.other.label"] = "Outros";
			d["item.other.desc"] = "Lembra as demais linhas que um painel de ferramenta pode gravar: as cores de via da linha Cor, mais Tamanho do pincel e Força do pincel.";
			d["item.other.ex.group"] = "por exemplo, você põe a cor de uma rua pequena de 2 faixas em azul e as outras ruas pequenas também ficam azuis, enquanto uma via grande não";
			d["item.other.ex.menu"] = "todo ativo que você clica no menu Vias usa este mesmo ajuste de cor e pincel, mas ao mudar para o menu Eletricidade não";
			d["item.other.ex.category"] = "mudar para uma via grande ainda é azul, mas uma ponte volta à cor padrão e precisa do próprio ajuste, embora as duas estejam no menu Vias";
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
			d["group.compat"] = "Совместимость";
			d["group.about"] = "Сведения и ссылки";

			d["enabled.label"] = "Включить память режима инструмента";
			d["enabled.desc"] = "По умолчанию включено. Пока это включено, каждое значение записывается постоянно, и при возврате к сохранению панели будут такими, какими вы их оставили. Отключение одного пункта лишь убирает его восстановление, а значения всё равно записываются. Запись прекращается только при выключении этого главного переключателя, и каждый инструмент возвращается к обычному поведению.";
			d["compat.label"] = "Совместимость с другими модами";
			d["compat.desc"] = "По умолчанию включено. Включено: память раскладывается по названиям меню и групп в том виде, как их изменили другие моды (Asset UI Manager, ExtraLib и подобные), поэтому перемещённый объект следует за новым местом. Выключено: сохраняется классификация, которую мод увидел первой, это устойчивее. Учтите: пользовательский объект, который его автор отнёс не к тому типу объектов, нельзя изменить вместе с этим типом. В обоих случаях уже записанные значения не теряются.";
			d["scope.label"] = "Область общего доступа";
			d["scope.desc"] = "Насколько широко разделяется значение этого пункта. Скобки в раскрывающемся списке показывают, как поступает оригинал и что мы рекомендуем.";
			d["scope.line.group"] = "Та же группа: {0}";
			d["scope.line.menu"] = "То же меню: {0}";
			d["scope.line.category"] = "Тот же тип объекта: разделяют настройку только объекты или функции одной подкатегории, а именно {0}";
			d["scope.line.shared"] = "Глобально: каждый объект и каждая функция, поддерживающие этот пункт, делят одно значение, но объекты и функции не делят между собой";
			d["scope.line.unique"] = "Без разделения: каждый объект и каждая функция, поддерживающие этот пункт, настраиваются совершенно независимо";
			d["scope.note"] = "Примечание: под функциями имеются в виду Зоны, Пространства и области, Терраформирование, Маркер и префабы объектов, то есть инструменты, которые не являются объектами";

			d[kScopeGroup] = "Та же группа";
			d[kScopeMenu] = "То же меню";
			d[kScopeCategory] = "Тот же тип объекта";
			d[kScopeGlobalShared] = "Глобально";
			d[kScopeGlobalUnique] = "Без разделения";
			d[kTagVanilla] = " (оригинал)";
			d[kTagRecommended] = " (рекомендуется)";
			d[kTagRecVanilla] = " (рекомендуется, оригинал)";

			d["reset.label"] = "Сбросить память";
			d["reset.desc"] = "В игре: стирает память этого сохранения и возвращает инструменты к исходному состоянию при загрузке. В главном меню: стирает память всех сохранений.";
			d["reset.warn"] = "Отменить нельзя.";
			d["reset.confirm"] = "Сбросить запомненные настройки инструментов?";
			d["resetall.label"] = "Сбросить все настройки";
			d["resetall.desc"] = "Возвращает каждую настройку этого мода к рекомендованному значению: главный переключатель и переключатель совместимости снова включаются, а также восстанавливаются состояние включения и область общего доступа каждого пункта. Уже записанная память сохранений не затрагивается.";
			d["resetall.warn"] = "Отменить нельзя.";
			d["resetall.confirm"] = "Сбросить все настройки этого мода к рекомендованным значениям?";
			d["folder.label"] = "Управление файлами памяти всех сохранений";
			d["folder.desc"] = "Открывает локальную папку с файлами памяти, по одному на сохранение, названным по имени сохранения.";

			d["about.version"] = "Версия мода";
			d["about.author"] = "Автор";
			d["about.kofi"] = "Buy me a Coffee";
			d["about.kofi.desc"] = "Поддержать автора на Ko-fi.";
			d["about.forum"] = "Forum Page";
			d["about.forum.desc"] = "Открыть тему на форуме Paradox.";
			d["about.rainbow"] = "RAINBOW官网";
			d["about.rainbow.desc"] = "Открыть сайт серии Rainbow.";

			d["item.anarchy.desc"] = "Запоминает состояние кнопки Anarchy, которую добавляет мод Anarchy (мод Paradox 74604): когда она включена, при размещении объекта больше не выполняются проверки на пересечение и подобные нарушения. Эта кнопка относится к тому моду, а не к игре; без Anarchy этот пункт ничего не делает.";
			d["item.anarchy.ex.group"] = "например, включите Anarchy на небольшой двухполосной дороге, и на других небольших дорогах он тоже останется включённым, а на большой дороге — нет";
			d["item.anarchy.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», Anarchy включён, но в меню «Электричество» уже нет";
			d["item.anarchy.ex.category"] = "переход на большую дорогу тоже сохраняет его включённым, но мост возвращается к выключенному и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.themes.label"] = "Тема";
			d["item.themes.desc"] = "Запоминает, какие пункты отмечены в фильтре «Тема» на панели инструментов. Эта строка появляется, только когда текущая категория действительно содержит объекты, в которых используются темы, а отмеченная тема ограничивает список панели инструментов объектами, которые её поддерживают. В оригинальной игре этот фильтр возвращается к теме по умолчанию только при загрузке сохранения, поэтому данный пункт по умолчанию выключен.";
			d["item.themes.ex.group"] = "например, отметьте тему только в «Дороги/Небольшие дороги», и большие дороги по этой теме фильтроваться не будут";
			d["item.themes.ex.menu"] = "тема, отмеченная в меню «Дороги», перестаёт действовать в меню «Электричество» и возвращается, когда вы снова открываете «Дороги»";
			d["item.themes.ex.category"] = "переулки и пути метро считаются одной категорией, поэтому отмеченная тема разделяется между меню";
			d["item.packs.label"] = "Набор";
			d["item.packs.desc"] = "Запоминает, какие пункты отмечены в фильтре «Набор» на панели инструментов; отмеченный набор ограничивает список панели инструментов объектами из этого набора. Оригинальная игра очищает этот фильтр при каждой смене меню или категории, поэтому запоминание в пределах одного меню и категории совпадает с тем, что вы видите, и этот пункт тоже по умолчанию выключен.";
			d["item.packs.ex.group"] = "например, отметьте набор в «Дороги/Небольшие дороги», оригинал снимет эту отметку на больших дорогах, а при возврате она вернётся";
			d["item.packs.ex.menu"] = "наборы, отмеченные в меню «Дороги», и наборы, отмеченные в меню «Электричество», запоминаются раздельно";
			d["item.packs.ex.category"] = "переулки есть и в меню «Дороги», и в меню «Районы» под одним именем, поэтому отмеченные наборы общие для обоих меню";
			d["item.toolMode.label"] = "Режим инструмента";
			d["item.toolMode.desc"] = "Запоминает режим, выбранный для каждого инструмента, а параметры различаются: для дорог, путей и труб это Прямая, Простая кривая, Сложная кривая, Непрерывная, Сетка, Заменить, Точка; для зданий, объектов и деревьев — Разместить один, Разместить несколько, Линия, Кривая, Инструмент копирования объекта; для зон — Заливка, Рамка, Кисть; для областей — Изменить, Создать сетку карты. Режимы объектов и режимы функций запоминаются отдельно, потому что их наборы различаются.";
			d["item.toolMode.ex.group"] = "например, выберите для небольшой двухполосной дороги режим «Простая кривая», и другие небольшие дороги тоже перейдут в простую кривую, а большая дорога — нет";
			d["item.toolMode.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», стоит режим «Простая кривая», но в меню «Электричество» уже нет";
			d["item.toolMode.ex.category"] = "переход на большую дорогу тоже даёт простую кривую, но мост возвращается к своему режиму по умолчанию и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.elevation.label"] = "Эстакада";
			d["item.elevation.desc"] = "Запоминает высоту, на которой оставлен инструмент, включая результат от Увеличить подъем и Уменьшить подъем. Шаг подъема (насколько сдвигает одно нажатие Увеличить подъем или Уменьшить подъем; в панели мода Anarchy эта строка называется так же) запоминается вместе с высотой и делит с ней область общего доступа. Высота перекрёстков и развязок не запоминается.";
			d["item.elevation.ex.group"] = "например, поднимите двухполосную дорогу до 10 м, и другие небольшие дороги тоже поднимутся до 10 м, а большая дорога на 10 м не будет";
			d["item.elevation.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», высота 10 м, но в меню «Электричество» уже не 10 м";
			d["item.elevation.ex.category"] = "переход на большую дорогу тоже даёт 10 м, но мост возвращается к 0 м и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.parallel.label"] = "Режим параллели";
			d["item.parallel.desc"] = "Запоминает переключатель Режим параллели (Переключить режим параллели) вместе с количеством Параллельная дорога и Смещение параллели, которые меняют Увеличить смещение и Уменьшить смещение. Позволяет строить параллельные дороги.";
			d["item.parallel.ex.group"] = "например, включите режим параллели на небольшой двухполосной дороге и поставьте количество 3, другие небольшие дороги тоже построят по 3, а большая дорога — нет";
			d["item.parallel.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», строится по 3 параллельные линии, но в меню «Электричество» уже нет";
			d["item.parallel.ex.category"] = "переход на большую дорогу тоже строит 3, но мост возвращается к выключенному и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.snap.label"] = "Привязка";
			d["item.snap.desc"] = "Запоминает каждый переключатель в строке Привязка: Привязка к форме, Привязка к длине зоны, Привязка к прямым углам, Привязка к обочине дороги, Привязка к дорогам, Привязка к зданию-владельцу, Привязка к стенам здания, Привязка к середине дороги, Привязка к берегу, Привязка к ландшафту, Привязка к направляющим, Привязка к сетке зоны, Привязка к точкам, Привязка к поверхности объекта, Привязка по вертикали, Привязка к сетке участка, Привязывает перекрывающиеся элементы к зданию, Удалить только определенный тип, Показать контурные линии, Привязка к расстоянию. Строка Включить/выключить привязку — это пакетный переключатель, и он не запоминается.";
			d["item.snap.ex.group"] = "например, выключите «Привязку к обочине дороги» на небольшой двухполосной дороге, и на других небольших дорогах она тоже будет выключена, а на большой дороге всё ещё включено";
			d["item.snap.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же набор переключателей привязки, но в меню «Электричество» уже нет";
			d["item.snap.ex.category"] = "переход на большую дорогу сохраняет тот же набор, но мост возвращается к значениям по умолчанию и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.topography.label"] = "Топография";
			d["item.topography.desc"] = "Запоминает строку Топография, то есть включено ли Показать контурные линии.";
			d["item.topography.ex.group"] = "например, включите топографию на небольшой двухполосной дороге, и на других небольших дорогах она тоже будет включена, а на большой дороге — нет";
			d["item.topography.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», показаны контурные линии, но в меню «Электричество» уже нет";
			d["item.topography.ex.category"] = "переход на большую дорогу тоже показывает их, но у моста строка выключена и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.leftRight.label"] = "Лево и Право";
			d["item.leftRight.desc"] = "Запоминает улучшения сети, выбранные в строках «Лево» и «Право» сетевой панели мода Anarchy, например «Велосипедная полоса», «Деревья», «Парковка», «Набережная», «Подпорные стены», «Шумозащитный экран». Требует Anarchy (мод Paradox 74604); без него этот пункт ничего не делает.";
			d["item.leftRight.ex.group"] = "например, выберите велосипедную полосу с правой стороны небольшой двухполосной дороги, и другие небольшие дороги тоже получат её справа, а большая дорога — нет";
			d["item.leftRight.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется та же пара боковых выборов, но в меню «Электричество» уже нет";
			d["item.leftRight.ex.category"] = "переход на большую дорогу тоже оставляет велосипедную полосу справа, но у моста ничего не выбрано, и ему нужна собственная настройка, хотя оба находятся в меню «Дороги»";
			d["item.general.label"] = "Общее";
			d["item.general.desc"] = "Запоминает переключатели строки «Общее» в сетевой панели мода Anarchy: «Земля», «Эстакада», «Туннель», «Постоянный наклон», «Широкая медиана» и «Расширенный диапазон высот» (последний снимает ограничение высоты). Требует Anarchy (мод Paradox 74604); без него этот пункт ничего не делает.";
			d["item.general.ex.group"] = "например, выберите «Туннель» для небольшой двухполосной дороги, и другие небольшие дороги тоже обратятся в тоннели, а большая дорога — нет";
			d["item.general.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используется тот же набор переключателей строки «Общее», но в меню «Электричество» уже нет";
			d["item.general.ex.category"] = "переход на большую дорогу тоже даёт «Туннель», но у моста ничего не отмечено, и ему нужна собственная настройка, хотя оба находятся в меню «Дороги»";
			d["item.underground.label"] = "Режим тоннеля";
			d["item.underground.desc"] = "Запоминает переключатель Режим тоннеля (Переключить режим тоннеля), который решает, окажется ли размещаемое под землёй или над ней.";
			d["item.underground.ex.group"] = "например, включите режим тоннеля на небольшой двухполосной дороге, и другие небольшие дороги тоже уйдут под землю, а большая дорога — нет";
			d["item.underground.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», всё строится под землёй, но в меню «Электричество» уже нет";
			d["item.underground.ex.category"] = "переход на большую дорогу тоже уходит под землю, но мост остаётся на поверхности и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			d["item.other.label"] = "Прочее";
			d["item.other.desc"] = "Запоминает прочие строки, которые панель инструмента может записывать: цвета дорог из строки «Цвет», а также «Размер кисти» и «Нажим кисти».";
			d["item.other.ex.group"] = "например, задайте небольшой двухполосной дороге синий цвет, и другие небольшие дороги тоже станут синими, а большая дорога — нет";
			d["item.other.ex.menu"] = "у любого объекта, выбранного в меню «Дороги», используются тот же цвет и настройка кисти, но в меню «Электричество» уже нет";
			d["item.other.ex.category"] = "переход на большую дорогу тоже остаётся синим, но мост возвращается к цвету по умолчанию и требует собственной настройки, хотя оба находятся в меню «Дороги»";
			return d;
		}
	}
}
