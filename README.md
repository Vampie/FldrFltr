# FldrFltr

Portable Windows-tool om bestanden in een map te selecteren op extensie en in bulk te hernoemen
via een naamsjabloon met variabelen (`{FileName}`, `{Year}`, `{Counter}`, ...). Zie
[`CLAUDE.md`](../CLAUDE.md) (in de bovenliggende map) voor de volledige projectbrief: functionele
eisen, de volledige variabelenlijst, de technologie-afweging en het architectuurvoorstel.

Zusterproject van [FldrSrtr](https://github.com/) — zelfde stack, zelfde portable filosofie
(geen installer, geen registry, alles naast de exe).

## Stack

- WPF + [ModernWpfUI](https://github.com/Kinnara/ModernWpf) op .NET Framework 4.8.1
- Portable: `settings.json` en `presets.json` naast de exe, geen `%AppData%`, geen registry
- Meertalig vanaf dag 1 (`Languages/*.json`), thema Licht/Donker/Systeem vanaf dag 1

## Bouwen

```powershell
dotnet build FldrFltr.slnx -c Debug
```

Portable release-zip (bouwnummer telt automatisch op, zie `build/release.ps1`):

```powershell
.\build\release.ps1 -Version 1.0
```

## Status van de implementatie

Bijgehouden per fase van [CLAUDE.md §5](../CLAUDE.md). Vink af zodra een fase werkt en getest is.

### Fase 1 — Skeleton + portable pipeline
- [x] Solution met 3 projecten: `App.Core` (leeg, klaar voor fase 2), `App.Infrastructure`
      (`PortablePaths`, `AppSettings`, `SettingsService`), `App.UI` (WPF-shell)
- [x] Eén venster met de drie invoervelden (Map, Extensielijst, Naamsjabloon) + knoppenrij
      (Testen/Hernoemen/Opslaan als preset — nog niet functioneel, zie fase 2) + lege
      presets-lijst en resultaat-tabel (frames, gevuld in fase 2/3)
- [x] "Bladeren..." werkt al (`ModernFolderPicker`, Ookii `VistaFolderBrowserDialog`)
- [x] Taal (`Localization`/`LocExtension`, `nl`/`en` in `Languages/`) en thema
      (`ThemeProvider`: Licht/Donker/Systeem via ModernWpf) overgenomen uit FldrSrtr en werkend
      vanaf de allereerste opstart, met een taal-/thema-kiezer rechtsboven in het venster
- [x] `build/release.ps1` (portable build, geen installer)
- [x] Applicatie-icoon: `assets/icon.ico` (gegenereerd uit
      `C:\claude_code\Resources\icons\fldrfltr.png`, geschaald naar 256×256 en als PNG-gecomprimeerd
      ICO opgeslagen — Windows Vista+ ondersteunt dat rechtstreeks), ingesteld via
      `App.UI.csproj`'s `ApplicationIcon`. `fldrfltr.png` zelf staat in de repo-root (zelfde
      patroon als FldrSrtr's `fldrsrtr.png`) en wordt door `release.ps1` mee in de release-map
      gekopieerd. Geverifieerd door het icoon terug uit de gebouwde exe te extraheren.

### Fase 2 — Kernfunctionaliteit
- [x] `FileMatcher`: Map + Extensielijst → bestanden opzoeken (niet-recursief, `*`/leeg = alles)
- [x] `VariableResolver`: naamsjabloon + alle variabelen uit §2 → nieuwe bestandsnamen
      (`{FileName}`/`{OriginalName}`, `{Extension}`/`{OriginalExtension}`, `{FullPath}`,
      `{Directory}`, `{FileSize}`, nu-tijdstip + `{Created*}`/`{Modified*}`, `{Counter}` met
      optionele start/stap, `{Guid}`, `{Random}`, `{RandomString}`) — inclusief het
      "Variabele invoegen ▾"-menu in de UI
- [x] Dry-run-weergave in de resultaat-tabel ("Testen (dry-run)"-knop)
- [x] Echt hernoemen ("Hernoemen"-knop), met `ConflictResolver` (Overslaan/Overschrijven/
      Automatisch hernoemen via een keuzelijst in de UI, standaard Automatisch hernoemen,
      nooit stilzwijgend overschrijven) en `RenameEngine`
- [x] Getest: dry-run, echt hernoemen, `{Counter:100:5}`, botsing → automatisch `(1)`-suffix,
      geen wijziging als het sjabloon op de huidige naam uitkomt, Overslaan-beleid

### Fase 3 — Presets-lijst
- [x] Preset-model (Naam, Map, Extensielijst, Naamsjabloon, LaatstGebruikt)
- [x] `PresetStore` (`presets.json` naast de exe, met timestamped backups, 10 bewaard)
- [x] Opslaan / laden / bewerken / verwijderen vanuit de UI — "Opslaan als preset..." onder een
      bestaande naam werkt als bewerken (overschrijft die preset in plaats van een duplicaat te
      maken), "Laden" vult de drie velden, "Verwijderen" vraagt eerst bevestiging

### Fase 4 — optioneel, later
- [x] Extra kleurthema's naast de drie basisthema's: `ThemeProvider` laadt elk bestand onder
      `Themes\<Naam>.json` (Base + AccentColor) — zelfde portable, geen-rebuild-principe als
      Languages. Standaard meegeleverd: Monokai, Solarized Dark, Solarized Light. Nieuwe
      paletten toevoegen = een JSON-bestand droppen, geen UI-editor nodig (dat blijft een
      latere uitbreiding, zie CLAUDE.md §1)

### Nafwerking (na fase 4)
- [x] **Thema-bug opgelost**: het venster miste `ui:WindowHelper.UseModernWindowStyle="True"` —
      daardoor bleven titelbalk en vensterachtergrond altijd licht, ongeacht het gekozen thema.
      Geverifieerd met een screenshot vóór/na: nu volledig donker bij "Donker".
- [x] **"Variabele invoegen ▾" onderverdeeld** in submenu's (`VariableMenuHelper`, zelfde opzet
      als FldrSrtr's `VariableMenuHelper`): Algemeen / Bestand / Datum (huidige datum/tijd), met
      scheidingslijnen tussen Bestand's aanmaak-/wijzigingsdatum-groepen
- [x] **Dropdowns breder gemaakt**: Bij naamconflict 140→220px, Thema 110→180px (past nu ook
      "Solarized Light"/"Solarized Dark"), Taal 90→110px
- [x] **Iconen toegevoegd** op de actieknoppen (Bladeren, Testen, Hernoemen, Opslaan als preset,
      Variabele invoegen, Laden, Verwijderen) — hergebruikt uit FldrSrtr's al-verwerkte
      `IconSets/Default` (bron-PNG's staan buiten de repo onder `C:\claude_code\Resources\icons`),
      via een vereenvoudigde `IconPathConverter` (één vaste set, geen pack-switching zoals in
      FldrSrtr — dat is hier niet gevraagd)
- [x] Layout-nasleep van de iconen: presetsrij paste niet meer volledig in de kaart — venster
      wat hoger gemaakt (620→680) en de presets-rij kreeg een `MinHeight`
- [x] **20 extra thema's** toegevoegd bovenop Monokai/Solarized Dark/Solarized Light, in twee
      groepen:
      - Exact overgenomen (Base + accentkleur) uit de echte Notepad++ `.xml`-themabestanden onder
        `C:\claude_code\Resources\themes`: Choco, DansLeRuSH-Dark, DarkModeDefault, Deep Black,
        Hello Kitty, HotFudgeSundae, Mono Industrial, MossyLawn, Khaki
      - Geïnspireerd op de thema's uit
        [spec-india.com/blog/notepad-themes](https://www.spec-india.com/blog/notepad-themes)
        (die pagina geeft geen exacte hexcodes, enkel kleurbeschrijvingen): Dracula, Material,
        Lunar, Nord, Neon, ICLS, Bespin, Slush & Poppies, Obsidian, Nautical but Nice, Waher Style
        — bij Dracula/Nord de daadwerkelijk gepubliceerde paletkleuren, de rest naar beste
        inschatting op basis van de beschrijving ("dark blue pastel, blue/red/white", "brown
        background", ...)
      - Thema-combobox verbreed naar 220px voor de langste namen ("Nautical but Nice")
      - Geverifieerd met screenshots: Dracula (donker), Khaki (licht) en Slush & Poppies (licht,
        bestandsnaam met "&") renderen allemaal correct
- [x] **Thema's tonen nu ook echt hun eigen kleur** (niet enkel licht/donker + een geaccentueerde
      knoprand): elk `Themes\*.json`-bestand kreeg een `Background` en `Foreground` naast Base en
      AccentColor. `ThemeProvider` zet die direct op het venster (`MainWindow`'s `Background`/
      `Foreground` binden op `PageBackgroundBrush`/`PageForegroundBrush`) én overschrijft
      ModernWpf's eigen kaartkleur-sleutel (`SystemControlBackgroundChromeMediumLowBrush`, dezelfde
      die `CardBorder` al gebruikte) met een variant die iets richting wit/zwart geduwd is, zodat
      kaarten nog steeds "verhoogd" aanvoelen boven de paginakleur. Voor de 3 basisthema's
      (Systeem/Licht/Donker) worden deze sleutels niet gezet — die blijven exact zoals voorheen
      (ModernWpf's eigen licht/donker-chrome), enkel de nieuwe paletten worden echt kleurrijk.
      Geverifieerd met screenshots: Choco (bruin), MossyLawn (olijfgroen), Nord (blauwgrijs) tonen
      nu allemaal hun eigen achtergrondkleur in plaats van generiek wit/zwart.
- [x] **Bugfix: Systeem/Licht/Donker toonden een blauwe achtergrond.** Oorzaak: `MainWindow`'s
      `Background`/`Foreground` bonden op `PageBackgroundBrush`/`PageForegroundBrush`, maar voor de
      3 basisthema's werden die sleutels helemaal niet gezet (bewust, om ModernWpf's eigen
      licht/donker-chrome niet te verstoren) — een `DynamicResource` die nergens naar kan
      resolven valt terug op een systeemkleur (op deze pc toevallig blauw), niet op gewoon wit/
      zwart. Fix: `ThemeProvider` zet deze twee sleutels nu altijd, ook voor de basisthema's —
      via `ThemeManager.Current.ActualApplicationTheme` (lost "Systeem" ook echt op naar de actuele
      OS-instelling) wordt een gewone wit/bijna-zwart kleur berekend wanneer er geen eigen
      Background in het paletbestand staat. Geverifieerd: Systeem is weer wit, Donker weer
      bijna-zwart.
- [x] **Rand van knoppen, dropdowns en de 3 kaders is nu de accentkleur**, voor alle thema's
      (ook Systeem/Licht/Donker, die de OS-accentkleur gebruiken bij gebrek aan een eigen
      AccentColor). Twee nieuwe implicte stijlen in `App.xaml` (`TargetType="Button"` /
      `"ComboBox"`, geen `x:Key`, dus automatisch overal van toepassing) zetten `BorderBrush` op
      `SystemControlForegroundAccentBrush`; `CardBorder`'s `BorderBrush` kreeg dezelfde sleutel.
      Geverifieerd: Neon-thema toont neongroene randen (zijn eigen accent) op knoppen, dropdowns
      én de 3 kaders, in plaats van het generieke Windows-accent.
