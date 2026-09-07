# FldrFltr

Portable Windows-tool om bestanden in een map te selecteren op extensie en in bulk te hernoemen
(of verplaatsen/herschikken) via een naamsjabloon met variabelen (`{FileName}`, `{Year}`,
`{Counter}`, ...). Zie [`CLAUDE.md`](../CLAUDE.md) (in de bovenliggende map) voor de volledige
projectbrief: functionele eisen, de volledige variabelenlijst, de technologie-afweging en het
architectuurvoorstel.

Zusterproject van [FldrSrtr](https://github.com/) — zelfde stack, zelfde portable filosofie
(geen installer, geen registry, alles naast de exe).

## Stack

- WPF + [ModernWpfUI](https://github.com/Kinnara/ModernWpf) op .NET Framework 4.8.1
- Portable: `settings.json` en `presets.json` naast de exe (met timestamped backups), geen
  `%AppData%`, geen registry
- Meertalig (`Languages/*.json`, nl/en), volledig aanpasbare thema's (`Themes/*.json`) vanaf dag 1

## Bouwen

```powershell
dotnet build FldrFltr.slnx -c Debug
```

Portable release (bouwnummer telt automatisch op, zie `build/release.ps1`):

```powershell
.\build\release.ps1 -Version 1.0
```

Het versienummer verschijnt ook in de titelbalk. Als `test_ACOT/` bestaat (lokale, niet-ingecheckte
scratch-map), kopieert het releasescript de gebouwde bestanden er ook automatisch naartoe.

## Functionaliteiten

**De drie invoervelden**
- **Map** — de map waarin gezocht wordt, met een "Bladeren..."-knop (Ookii `VistaFolderBrowserDialog`)
- **Extensielijst** — `xml`, `xml;csv;txt`, of `*`/leeg voor alle bestanden
- **Naamsjabloon** — bepaalt de nieuwe naam én locatie van elk bestand:
  - Alle variabelen uit CLAUDE.md §2: `{FileName}`/`{OriginalName}`, `{Extension}`/
    `{OriginalExtension}`, `{FullPath}`, `{Directory}`, `{FileSize}`, huidig moment
    (`{Year}`...`{Time}`), aanmaak-/wijzigingsdatum (`{Created*}`/`{Modified*}`), `{Counter}`
    (met optionele start/stap), `{Guid}`, `{Random}`, `{RandomString}`
  - "Variabele invoegen ▾" opent een menu met deze variabelen, onderverdeeld in Algemeen/
    Bestand/Datum
  - `\` (of `/`) in het sjabloon verplaatst het bestand — incl. `..\` om een niveau omhoog te
    gaan — zodat je bestanden ook kan herschikken in (sub)mappen,
    bv. `{OriginalExtension}\{FileName}.{Counter:100}` sorteert per extensie in submappen

**Testen / hernoemen**
- **Testen (dry-run)** toont het resultaat (van/naar/status) zonder iets te wijzigen
- **Hernoemen** voert de wijziging echt uit, met conflicthandling: Overslaan / Overschrijven /
  Automatisch hernoemen (`(1)`-suffix) — standaard Automatisch hernoemen, nooit stilzwijgend
  overschrijven
- Een statustekst naast "Resultaat" toont "Bezig met testen/hernoemen..." terwijl het loopt (op
  een achtergrondthread, blokkeert de UI niet) en "Klaar — N bestand(en) ..." erna — geen
  volwaardige voortgangsbalk, enkel een eenvoudige indicatie

**Bewaarde presets**
- Een preset bewaart Map + Extensielijst + Naamsjabloon onder een naam; de lijst toont
  "Naam → Map → Extensielijst → Sjabloon"
- Dubbelklikken op een preset laadt de 3 velden én start meteen een dry-run
- "Opslaan als preset..." onthoudt de laatst gebruikte presetnaam als voorstel, en vraagt
  bevestiging als je een bestaande naam overschrijft (bewerkt die preset dan in plaats van een
  duplicaat te maken)
- "Verwijderen" (icoon, rechts uitgelijnd) vraagt eerst bevestiging

**Meertaligheid & thema's**
- Taal (nl/en) en thema kiezen rechtsboven, meteen actief (thema live, taal na herstart)
- Naast Systeem/Licht/Donker een reeks kant-en-klare kleurthema's (Monokai, Solarized,
  overgenomen uit echte Notepad++-themabestanden, en enkele geïnspireerd op bekende
  editor-thema's) — elk thema zet ook een eigen achtergrond-/tekstkleur en een accentkleur op
  knoppen/dropdowns/kaders, niet enkel licht-of-donker
- Nieuwe thema's/talen toevoegen = een JSON-bestand droppen in `Themes/`/`Languages/`, geen
  rebuild nodig
- Verwijst `settings.json` naar een taal/thema die niet meer bestaat (verwijderd bestand, oud
  bestand van een vorige installatie, ...), dan valt de taal terug op Engels en het thema op
  Systeem — nooit een lege dropdown, en `settings.json` herstelt zichzelf naar de geldige waarde

**Venstergrootte & -positie**
- Onthoudt positie, grootte én welk scherm bij het sluiten; bij een volgende start staat het
  venster weer exact daar
- Staat dat scherm niet meer aangesloten (of is de opgeslagen positie ongeldig), dan valt het
  terug op een standaardgrootte — 35% van de breedte, 80% van de hoogte van het beeldscherm,
  gecentreerd — en wordt hoe dan ook geklemd binnen een aangesloten scherm, zodat de app nooit
  goeddeels of volledig buiten beeld opent
