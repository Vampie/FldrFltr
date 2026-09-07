# FldrFltr v1.0 — Release notes

Eerste release van FldrFltr: een portable Windows-tool om bestanden in een map te selecteren
op extensie en in bulk te hernoemen (of verplaatsen/herschikken) via een naamsjabloon met
variabelen.

## Functionaliteiten

**De drie invoervelden**
- **Map** — de map waarin gezocht wordt, met een "Bladeren..."-knop
- **Extensielijst** — `xml`, `xml;csv;txt`, of `*`/leeg voor alle bestanden
- **Naamsjabloon** — bepaalt de nieuwe naam én locatie van elk bestand, met alle variabelen uit
  CLAUDE.md §2 (`{FileName}`, `{Year}`, `{Counter}`, `{Guid}`, ...) en een menu om ze in te voegen
- `\` (of `/`) in het sjabloon verplaatst het bestand — incl. `..\` om een niveau omhoog te gaan —
  zodat je bestanden ook kan herschikken in (sub)mappen

**Testen / hernoemen**
- **Testen (dry-run)** toont het resultaat zonder iets te wijzigen
- **Hernoemen** voert de wijziging echt uit, met conflicthandling (Overslaan / Overschrijven /
  Automatisch hernoemen), standaard nooit stilzwijgend overschrijven
- Statusindicatie tijdens het werk, op een achtergrondthread

**Bewaarde presets**
- Map + Extensielijst + Naamsjabloon bewaren onder een naam, terugvinden, opnieuw uitvoeren
- Dubbelklikken laadt een preset en start meteen een dry-run
- Opslaan onthoudt de laatst gebruikte presetnaam en vraagt bevestiging bij overschrijven

**Meertaligheid & thema's**
- Nederlands/Engels, en een reeks kant-en-klare kleurthema's naast Systeem/Licht/Donker
- Valt terug op Engels/Systeem als een opgeslagen taal/thema niet meer bestaat

**Venstergrootte & -positie**
- Onthoudt positie, grootte en scherm; valt terug op een standaardgrootte als dat scherm er niet
  meer is, en klemt altijd binnen een aangesloten scherm

## Installatie

Uitpakken en `FldrFltr.exe` starten — geen installer, geen registry-writes. `settings.json` en
`presets.json` verschijnen naast de exe.
