# Dead Men Spell No Tales - dokumentacja projektu

## 1. Informacje ogolne

**Dead Men Spell No Tales** to jednoosobowa gra 2D top-down przygotowana jako projekt z przedmiotu Programowanie gier. Gracz wciela sie w pirata eksplorujacego archipelag objety klatwa cyklonu. Celem rozgrywki jest zdobywanie zlota, odkrywanie liter hasla i rozwiazanie zagadki przed uplywem czasu.

Projekt laczy elementy eksploracji, walki, ekonomii zasobow oraz minigry slownej inspirowanej wisielcem. Gra jest przygotowywana pod komputer PC / Windows.

## 2. Podstawowe zalozenia gry

- **Typ gry:** 2D top-down.
- **Tryb gry:** single-player.
- **Platforma docelowa:** PC / Windows.
- **Silnik:** Unity 6000.4.6f1.
- **Glowna petla rozgrywki:** eksploracja wysp, zdobywanie zlota, kupowanie liter, rozwiazanie hasla.
- **Warunek zwyciestwa:** poprawne odgadniecie hasla przed koncem timera.
- **Warunek porazki:** uplyw czasu lub niepowodzenie wynikajace z mechanik gry.

## 3. Uruchomienie projektu

### Uruchomienie w Unity

1. Otworzyc projekt w Unity Hub.
2. Wybrac wersje Unity **6000.4.6f1** lub kompatybilna wersje Unity 6.
3. Poczekac na zaimportowanie paczek i assetow.
4. Otworzyc scene:
   - `Assets/Scenes/MainMenu.unity` - menu glowne,
   - `Assets/Scenes/MainScene.unity` - glowna scena gry.
5. Uruchomic gre przyciskiem **Play** w edytorze.

### Build

Projekt ma skonfigurowane sceny `MainMenu` oraz `MainScene` w Build Settings. Docelowy build powinien byc wykonany dla platformy Windows i dolaczony do paczki oddawanej jako plik `.exe` wraz z wymaganymi plikami danych Unity.

## 4. Uzyte technologie i paczki

- **Unity 6000.4.6f1** - silnik gry.
- **C# / MonoBehaviour** - implementacja logiki gry.
- **Universal Render Pipeline 17.4.0** - pipeline renderowania 2D.
- **Renderer2D** - obsluga oprawy 2D.
- **Unity Input System 1.19.0** - wejscie gracza, ruch, interakcje i akcje.
- **Cinemachine 3.1.6** - obsluga kamer i przelaczanie widoku miedzy postacia i statkiem.
- **Unity Tilemap oraz 2D Tilemap Extras** - budowa wysp, archipelagu i nawigacji po kafelkach.
- **TextMeshPro oraz Unity UI / UGUI** - HUD, menu, ekran mapy i ekran wisielca.
- **ScriptableObject** - dane wysp i zakresy losowania skarbow.
- **Physics2D, Rigidbody2D, Collider2D** - ruch, walka, interakcje i pickupy.
- **Tiny Swords** - zewnetrzny asset graficzny wykorzystany w oprawie gry.
- **Wlasne grafiki i fonty** - m.in. mapy, ikony, elementy pirackie, PressStart2P i VT323.

## 5. Najwazniejsze systemy gry

### 5.1 GameManager i stan rozgrywki

`GameManager` przechowuje podstawowy stan gry:

- aktualna ilosc zlota,
- globalny timer rozgrywki,
- aktualnie wylosowane haslo,
- liste odkrytych liter,
- logike dodawania i wydawania zlota,
- kare czasowa za bledne odgadniecie hasla.

Hasla sa losowane z przygotowanej puli pirackich fraz. Timer domyslnie startuje od 15 minut.

### 5.2 Wisielec

System wisielca jest jedna z glownych mechanik projektu. Ekran zagadki:

- generuje pola liter na podstawie wylosowanego hasla,
- pokazuje zakupione litery,
- pozwala kupowac litery za zloto,
- pozwala wpisywac brakujace litery z klawiatury,
- obsluguje zatwierdzenie calego hasla,
- konczy gre zwyciestwem po poprawnej odpowiedzi,
- odejmuje czas po blednej probie.

### 5.3 Eksploracja i skarby

System skarbow sklada sie z klas `PlayerTreasureHunter`, `IslandData` i `ActiveTreasureMap`.

Gracz otrzymuje aktywne mapy skarbow. Kazda mapa wskazuje jedna z wysp oraz pozycje skarbu wygenerowana na poprawnym kafelku Tilemapy. Po dotarciu w odpowiednie miejsce gracz moze kopac, a odnalezienie skarbu nagradza go zlotem.

### 5.4 Mapa skarbu

`UIManager` odpowiada za panel mapy:

- otwieranie i zamykanie mapy,
- przelaczanie aktywnych map,
- wyswietlanie sprite'a wyspy,
- ustawianie znaku X na podstawie pozycji skarbu.

Mapa jest waznym elementem petli rozgrywki, bo prowadzi gracza do kolejnych celow eksploracji.

### 5.5 Ruch postaci

`PlayerController2D` odpowiada za ruch gracza w 2D:

- odczyt wejscia z Unity Input System,
- ruch przez `Rigidbody2D`,
- animacje chodzenia i stania,
- odwrocenie sprite'a zgodnie z kierunkiem ruchu,
- zapamietywanie ostatniego kierunku ruchu na potrzeby walki.

### 5.6 Statek

Projekt zawiera podstawowy system przelaczania miedzy postacia i statkiem. `DockingArea` pozwala wejsc na statek lub zejsc na lad, a klasa `Player` przelacza:

- aktywny kontroler,
- aktywna kamere Cinemachine,
- interakcje gracza,
- widocznosc postaci.

Pelna symulacja wiatru, zagli, kotwicy, zalania i naprawy statku zostala potraktowana jako zakres opcjonalny.

### 5.7 System interakcji

Interakcje sa oparte na klasie bazowej `Interactable` oraz komponencie `InteractionController`.

System:

- wykrywa obiekty interaktywne w zasiegu,
- wybiera najblizszy dostepny obiekt,
- pokazuje prompt na HUD,
- uruchamia akcje przypisana do danego obiektu.

Na tym systemie oparte sa m.in.:

- wejscie na statek,
- zejscie na lad,
- otwarcie wisielca,
- leczenie,
- reroll map skarbow.

### 5.8 Walka ladowa

`PlayerCombat` obsluguje walke w zwarciu:

- atak w kierunku ostatniego ruchu,
- cooldown ataku,
- opoznienie momentu trafienia,
- wykrywanie przeciwnikow przez `Physics2D.OverlapCircleAll`,
- animacje ataku,
- dzwieki trafienia i pudla.

Walka dystansowa pistoletem oraz walka morska armatami pozostaja elementami opcjonalnymi opisanymi w GDD.

### 5.9 AI przeciwnikow

`AIEntity` odpowiada za zachowanie przeciwnikow. System AI zawiera:

- stany `Idle`, `Patrol`, `Chase`, `Attack`, `Dead`,
- zdrowie przeciwnika,
- wykrywanie gracza,
- line of sight,
- patrolowanie,
- poscig,
- atak w zwarciu,
- hit feedback,
- stun po trafieniu,
- animacje,
- dzwieki ambientowe, ataku, obrazen i smierci.

### 5.10 Pathfinding po Tilemapie

`AITilemapPathfinder` pozwala przeciwnikom poruszac sie po kafelkach wysp. System:

- korzysta z Tilemap jako siatki nawigacyjnej,
- sprawdza, czy kafelek jest mozliwy do przejscia,
- uwzglednia przeszkody z Physics2D,
- wyszukuje sciezke do celu,
- szuka najblizszego poprawnego kafelka, jesli cel znajduje sie poza obszarem ruchu.

### 5.11 Spawner przeciwnikow

`EnemySpawner` tworzy przeciwnikow na wyspach. Spawner:

- wyszukuje Tilemapy oznaczone jako teren,
- losuje poprawny punkt spawnu na ladowym kafelku,
- kontroluje maksymalna liczbe aktywnych przeciwnikow,
- obsluguje spawn poczatkowy i okresowy.

### 5.12 Smierc i respawn

`GameFlowController` obsluguje:

- ekran wygranej,
- ekran przegranej,
- restart gry,
- powrot do menu,
- sekwencje smierci gracza,
- utrate zlota po smierci,
- przeniesienie gracza i statku do punktow respawnu,
- odnowienie zdrowia po respawnie.

### 5.13 Ekonomia i zloto

Zloto jest glownym zasobem progresji. Gracz zdobywa je:

- z wykopanych skarbow,
- z pickupow,
- z dropow po przeciwnikach.

Zloto jest wydawane na:

- kupowanie liter,
- leczenie,
- losowanie nowych map.

### 5.14 HUD i interfejs

Projekt zawiera kilka elementow UI:

- prompt interakcji,
- pasek zdrowia,
- licznik zlota,
- animacje przyrostu zlota,
- panel mapy skarbu,
- ekran wisielca,
- menu glowne,
- panele wygranej i przegranej,
- animacje wejscia paneli UI.

### 5.15 Audio

Warstwa audio sklada sie z kilku komponentow:

- `AudioCue` - losowanie klipu, glosnosc, pitch range, one-shoty, dzwiek przestrzenny,
- `SoundCue` - serializowalny zestaw dzwiekow uzywany przez inne skrypty,
- `ProximityAudioEmitter` - ambient i losowe dzwieki zalezne od odleglosci,
- `UIButtonAudio` - dzwieki klikniecia i najechania na przyciski.

## 6. Sterowanie

Sterowanie korzysta z Unity Input System. W aktualnej wersji projektu najwazniejsze akcje to:

- **Ruch postaci:** akcja ruchu z Input System.
- **Interakcja:** akcja interakcji na najblizszym obiekcie.
- **Mapa skarbu:** `M`.
- **Zmiana aktywnej mapy:** `Q` / `E`.
- **Kopanie:** `Spacja`.
- **Atak:** akcja ataku przypisana w Input System.
- **Wisielec:** wpisywanie liter z klawiatury.
- **Zatwierdzenie hasla:** `Enter`.
- **Zamkniecie ekranu wisielca:** `Escape`.

## 7. Struktura katalogow

Najwazniejsze katalogi projektu:

- `Assets/Scenes` - sceny Unity, m.in. `MainMenu` i `MainScene`.
- `Assets/Scripts/GameManager` - stan gry, timer, hasla, menu, wygrana, przegrana i respawn.
- `Assets/Scripts/Player` - ruch gracza, walka, interakcje, statek i skarby.
- `Assets/Scripts/AI` - przeciwnicy, pathfinding, spawner i oznaczenia tilemap.
- `Assets/Scripts/TreasureSystem` - dane wysp i aktywne mapy skarbow.
- `Assets/Scripts/HUD` - UI, mapa, licznik zlota i animacje paneli.
- `Assets/Scripts/Buildings` - interaktywne obiekty w hubie.
- `Assets/Scripts/Rendering` - sortowanie sprite'ow.
- `Assets/Art` - wlasne assety graficzne projektu.
- `Assets/Tiny Swords` - zewnetrzny asset graficzny.
- `Packages` - manifest paczek Unity.
- `ProjectSettings` - ustawienia projektu Unity.

## 8. Zakres wersji oddawanej

Wersja oddawana skupia sie na najwazniejszej petli rozgrywki:

1. Start gry z menu.
2. Eksploracja mapy.
3. Korzystanie z map skarbow.
4. Wykopywanie skarbow.
5. Zdobywanie zlota.
6. Walka z przeciwnikami.
7. Kupowanie liter w wisielcu.
8. Rozwiazanie hasla przed koncem czasu.

Zrealizowane zostaly kluczowe elementy MVP:

- ruch postaci,
- system interakcji,
- eksploracja skarbow,
- mapa skarbu,
- ekonomia zlota,
- wisielec,
- timer,
- walka ladowa,
- AI przeciwnikow,
- pathfinding po Tilemapie,
- smierc i respawn,
- HUD,
- menu,
- audio,
- podstawowy stan statku.

Elementy opisane w GDD jako bardziej rozbudowane lub opcjonalne pozostaja poza podstawowym zakresem wersji oddawanej:

- pelna symulacja wiatru i zagli,
- kotwica,
- zalanie i naprawa statku,
- walka morska armatami,
- pistolet i proch,
- dynamiczna sciana cyklonu,
- wrogie statki,
- zaawansowane wskazowki oparte o landmarki.

## 9. Odniesienie do wymagan projektu

Projekt spelnia glowne wymagania z regulaminu:

- posiada Game Design Document,
- jest gra 2D przygotowana pod komputer,
- posiada scenariusz i motyw przewodni,
- jest utrzymywany w repozytorium,
- korzysta z dodatkowych paczek i assetow,
- zawiera kilka zaawansowanych mechanik,
- ma podzial kodu na obszary odpowiedzialnosci,
- posiada dokumentacje projektu,
- moze zostac zbudowany jako aplikacja Windows

## 10. Podsumowanie

**Dead Men Spell No Tales** realizuje koncepcje gry pirackiej, w ktorej eksploracja i walka sa polaczone z presja czasu oraz zagadka slowa. Najwazniejsza wartoscia projektu jest polaczenie kilku systemow w jedna petle rozgrywki: skarbow, zlota, wisielca, AI przeciwnikow, UI i audio.

Projekt moze byc dalej rozwijany o pelniejsza zegluge, walke morska i bardziej rozbudowany system zasobow, ale aktualna wersja zawiera dzialajacy rdzen rozgrywki oraz dokumentacje potrzebna do oddania projektu.
