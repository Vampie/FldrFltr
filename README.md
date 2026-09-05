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
- [ ] `FileMatcher`: Map + Extensielijst → bestanden opzoeken
- [ ] `RenameTemplateEngine`: naamsjabloon + alle variabelen uit §2 → nieuwe bestandsnamen
- [ ] Dry-run-weergave in de resultaat-tabel
- [ ] Echt hernoemen, met `ConflictResolver` (skip/overwrite/auto-`(1)`-suffix, standaard
      auto-hernoemen, nooit stilzwijgend overschrijven)

### Fase 3 — Presets-lijst
- [ ] Preset-model (Naam, Map, Extensielijst, Naamsjabloon, LaatstGebruikt)
- [ ] `PresetStore` (`presets.json` naast de exe, met backup)
- [ ] Opslaan / laden / bewerken / verwijderen vanuit de UI

### Fase 4 — optioneel, later
- [ ] Extra kleurthema's naast de vier basisthema's (Monokai/Solarized-achtige paletten,
      zelfde idee als FldrSrtr's `ThemeEditorWindow`)
