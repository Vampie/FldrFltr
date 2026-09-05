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
- [ ] Applicatie-icoon (`assets/icon.ico`) — nog niet toegevoegd

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
