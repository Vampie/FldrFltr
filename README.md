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
- [x] **Bugfix: de 3 dropdowns (Taal/Thema/Bij naamconflict) en het "Variabele invoegen"-menu
      bleven altijd wit/zwart**, ongeacht het thema — onleesbaar bij donkere paletten met witte
      tekst. Oorzaak: geverifieerd dat ModernWpfUI's eigen ComboBox/ContextMenu/MenuItem-chrome op
      net481 een gewone `Background`/`Foreground`-`Setter` gewoon negeert (pixel-gemeten: bleef
      exact `#EAEAEA`, ongewijzigd, terwijl TextBox/Button/Border ernaast wél correct meekleurden
      met dezelfde sleutels). Fix: volledig eigen `ControlTemplate` voor `ComboBox`,
      `ComboBoxItem`, `ContextMenu` en `MenuItem` in `App.xaml`, opgebouwd met alleen
      `TemplateBinding` tegen properties die wij zelf zetten — dropdown-achtergrond = kaderkleur
      (`SystemControlBackgroundChromeMediumLowBrush`), tekst = paginatekstkleur
      (`PageForegroundBrush`), rand + gemarkeerd item = accentkleur. Geverifieerd met
      screenshots onder het Nord-thema: gesloten dropdown, open dropdown-lijst, en het
      "Variabele invoegen"-menu (incl. submenu "Bestand") tonen nu allemaal Nord's blauwgrijze
      achtergrond met witte tekst; Systeem-thema (licht) vertoont geen regressie.
- [x] **Twee opvolgpunten op de dropdown-fix hierboven**:
      - De ComboBox-template had een gedupliceerde `ToggleButton`-substructuur (eigen
        Border+pijltje bovenop de "echte" Border+pijltje) die het pijltje verkeerd positioneerde
        ("de dropdown is wat om zeep"). Vereenvoudigd naar één zichtbare laag
        (Border+tekst+pijltje in één Grid) met een volledig transparante `ToggleButton` erbovenop,
        enkel voor de klik-afhandeling.
      - De huidige/gehoverde regel in een dropdown-lijst (ComboBox én "Variabele invoegen")
        gebruikte een volle accentkleur-blok als markering — vervangen door de paginakleur
        (`PageBackgroundBrush`, een andere tint dan de kaderkleur van de lijst zelf), een subtielere
        aanduiding zonder de accentkleur, zoals gevraagd.
      - Geverifieerd met screenshots onder Nord: proper uitgelijnd pijltje, zachte
        achtergrond-highlight op de huidige regel; Systeem-thema (licht) vertoont geen regressie.
- [x] **Regressie uit de vorige stap hersteld + 3 kleine verbeteringen**:
      - De "één laag + transparante ToggleButton erbovenop"-vereenvoudiging van de ComboBox-template
        (vorige stap) bleek de dropdowns helemaal niet meer te laten openen. Teruggedraaid naar de
        structuur waarbij de `ToggleButton` zelf de zichtbare Border+tekst+pijltje draagt (exact de
        versie die eerder al met screenshots bevestigd werd te openen) — belangrijker dat hij
        opengaat dan een mogelijk ingebeeld pixel-verschil in het pijltje.
      - "Variabele invoegen" had geen "huidige regel"-aanduiding meer omdat `ContextMenu` en
        `MenuItem` in normale staat én in `IsHighlighted`-staat toevallig dezelfde
        `PageBackgroundBrush` gebruikten — geen zichtbaar verschil. Normale staat teruggezet naar de
        kaderkleur (`SystemControlBackgroundChromeMediumLowBrush`), zodat de highlight-kleur weer
        opvalt.
      - `Preset.DisplayText` toont nu ook het pad: "Naam → Map → Extensielijst → Sjabloon".
      - "Opslaan als preset..." vraagt nu eerst bevestiging wanneer de opgegeven naam al bestaat,
        in plaats van die preset stilzwijgend te overschrijven.
      - Laden/Verwijderen-knoppen in de presets-lijst staan nu vóór de tekst (vaste positie), zodat
        rijen met een verschillende tekstlengte toch mooi uitlijnen.
      - Kon deze ronde niet opnieuw met screenshots geverifieerd worden (de gebruiker was actief
        aan het werk op de machine — vensterfocus overnemen voor screenshots was niet veilig/gepast
        op dat moment); de ComboBox-fix is wel een letterlijke terugkeer naar reeds bevestigde
        werkende code.
- [x] **2 kleine layout-fixes op het vorige punt**:
      - Dropdowns waren weer heel klein/dun: de `ToggleButton` die de zichtbare Border+pijltje
        draagt heeft zelf geen echte content (enkel het pijltje), dus zonder een expliciete
        `MinHeight` kon de hele ComboBox terugvallen naar de hoogte van dat pijltje. `MinHeight="32"`
        toegevoegd aan de ComboBox-stijl.
      - De "Bewaarde presets"-kaart nam een vaste `*`-verhouding van de resterende ruimte in
        (voorheen `MinHeight="130"`), wat met weinig/geen presets veel witruimte gaf. Rij nu
        `Height="Auto"` (schaalt met de inhoud) met `MaxHeight="160"` op de `ListBox` zelf om te
        voorkomen dat de kaart bij veel presets ongelimiteerd groeit; de "Resultaat"-kaart eronder
        (`Height="*"`) krijgt daardoor de vrijgekomen ruimte.
      - Kon ook deze ronde niet met screenshots geverifieerd worden (gebruiker was actief in Visual
        Studio aan een ander project) — graag zelf even bevestigen.
