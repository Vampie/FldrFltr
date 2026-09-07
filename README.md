# FldrFltr

Portable Windows tool to select files in a folder by extension and bulk-rename (or
move/reorganize) them via a name template with variables (`{FileName}`, `{Year}`, `{Counter}`,
...).

## Stack

- WPF + [ModernWpfUI](https://github.com/Kinnara/ModernWpf) on .NET Framework 4.8.1
- Portable: `settings.json` and `presets.json` next to the exe (with timestamped backups), no
  `%AppData%`, no registry
- Multilingual (`Languages/*.json`, nl/en), fully customizable themes (`Themes/*.json`) from day one

## Building

```powershell
dotnet build FldrFltr.slnx -c Debug
```

Portable release (build number auto-increments, see `build/release.ps1`):

```powershell
.\build\release.ps1 -Version 1.0
```

The version number also shows up in the title bar. If `test_ACOT/` exists (a local, uncommitted
scratch folder), the release script also copies the built files there automatically.

## Features

**The three input fields**
- **Folder** — the folder to search, with a "Browse..." button (Ookii `VistaFolderBrowserDialog`)
- **Extension list** — `xml`, `xml;csv;txt`, or `*`/empty for all files
- **Name template** — determines the new name *and* location of each file:
  - Every variable from CLAUDE.md §2: `{FileName}`/`{OriginalName}`, `{Extension}`/
    `{OriginalExtension}`, `{FullPath}`, `{Directory}`, `{FileSize}`, current moment
    (`{Year}`...`{Time}`), created/modified date (`{Created*}`/`{Modified*}`), `{Counter}` (with
    optional start/step), `{Guid}`, `{Random}`, `{RandomString}`
  - "Insert variable ▾" opens a menu with these variables, grouped into General/File/Date
  - `\` (or `/`) in the template moves the file — including `..\` to go up a level — so you can
    also reorganize files into (sub)folders, e.g. `{OriginalExtension}\{FileName}.{Counter:100}`
    sorts files into subfolders per extension

**Testing / renaming**
- **Test (dry run)** shows the result (from/to/status) without changing anything
- **Rename** performs the actual change, with conflict handling: Skip / Overwrite / Auto-rename
  (`(1)` suffix) — defaults to Auto-rename, never silently overwriting
- A status text next to "Result" shows "Testing/renaming..." while it runs (on a background
  thread, doesn't block the UI) and "Done — N file(s) ..." afterwards — no full progress bar,
  just a simple indicator

**Saved presets**
- A preset saves Folder + Extension list + Name template under a name; the list shows
  "Name → Folder → Extension list → Template"
- Double-clicking a preset loads the 3 fields and immediately starts a dry run
- "Save as preset..." suggests the last-used preset name, and asks for confirmation if you
  overwrite an existing name (edits that preset instead of creating a duplicate)
- "Delete" (icon, right-aligned) asks for confirmation first

**Localization & themes**
- Choose language (nl/en) and theme top-right, applied immediately (theme live, language after
  restart)
- Alongside System/Light/Dark, a range of ready-made color themes (Monokai, Solarized, taken from
  real Notepad++ theme files, and some inspired by well-known editor themes) — each theme also
  sets its own background/text color and an accent color on buttons/dropdowns/panels, not just
  light-or-dark
- Adding new themes/languages = drop a JSON file into `Themes/`/`Languages/`, no rebuild needed
- If `settings.json` points to a language/theme that no longer exists (deleted file, leftover from
  a previous install, ...), the language falls back to English and the theme to System — never a
  blank dropdown, and `settings.json` heals itself back to the valid value

**Window size & position**
- Remembers position, size and which screen at close; on the next start the window opens back in
  exactly the same place
- If that screen is no longer connected (or the saved position is invalid), it falls back to a
  default size — 35% of the screen's width, 80% of its height, centered — and is always clamped
  within a connected screen, so the app never opens mostly or fully off-screen
