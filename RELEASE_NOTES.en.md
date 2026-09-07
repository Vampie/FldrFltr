# FldrFltr v1.0 — Release notes

First release of FldrFltr: a portable Windows tool to select files in a folder by extension and
bulk-rename (or move/reorganize) them via a name template with variables.

## Features

**The three input fields**
- **Folder** — the folder to search, with a "Browse..." button
- **Extension list** — `xml`, `xml;csv;txt`, or `*`/empty for all files
- **Name template** — determines each file's new name *and* location, with every variable from
  CLAUDE.md §2 (`{FileName}`, `{Year}`, `{Counter}`, `{Guid}`, ...) and a menu to insert them
- `\` (or `/`) in the template moves the file — including `..\` to go up a level — so you can also
  reorganize files into (sub)folders

**Testing / renaming**
- **Test (dry run)** shows the result without changing anything
- **Rename** performs the actual change, with conflict handling (Skip / Overwrite / Auto-rename),
  defaulting to never silently overwriting
- A status indicator during the work, running on a background thread

**Saved presets**
- Save Folder + Extension list + Name template under a name, find it again, re-run it
- Double-clicking loads a preset and immediately starts a dry run
- Saving remembers the last-used preset name and asks for confirmation before overwriting

**Localization & themes**
- Dutch/English, plus a range of ready-made color themes alongside System/Light/Dark
- Falls back to English/System if a saved language/theme no longer exists

**Window size & position**
- Remembers position, size and screen; falls back to a default size if that screen is no longer
  connected, and always clamps within a connected screen

## Installation

Unzip and run `FldrFltr.exe` — no installer, no registry writes. `settings.json` and
`presets.json` appear next to the exe.
