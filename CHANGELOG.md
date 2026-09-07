# Changelog

Running list of changes per version — kept separate from README.md (which only describes current
functionality) so in-progress work has a place to live before it's folded into the README in a
logical spot once a version is done.

## v2 (in progress)

- **Header title**: "FldrFltr" in the theme's accent color, with a smaller "Filter your
  folders"/"Filter je mappen" tagline next to it in the normal text color.
- **Collapsible results table**: the result table (after Test/Rename) starts collapsed; the
  status text next to its title stays visible either way, so a Rename run without a prior Test
  still shows its outcome. A dry run (Test, or a preset's automatic one) auto-expands the table.
  A checkbox next to "Saved presets" ("Show result when loading") controls only whether *loading*
  a preset auto-expands the table this way — the preset's dry run itself always still runs.
- **`release/ToCopy`**: every `release.ps1` run now clears and refills this folder with only the
  files that actually changed compared to the previous release (byte-compared, not by timestamp)
  — so updating a customer's remote PC only needs those files, not the whole install.

- **Variable overview**: two ways to see what each `{Variable}` in the name template means,
  without blocking the rest of the app — an always-on-top popup window ("Help" button, non-modal)
  and a collapsible side panel (toggle button), both showing the same grouped list with
  descriptions.
- **Target .NET Framework 4.8, not 4.8.1**: a customer machine refused to start the exe at all
  ("This application requires .NETFramework,Version=v4.8.1") and couldn't install the missing
  runtime. 4.8.1 (May 2022) is only preinstalled from Windows 11 22H2 onward; plain 4.8 (April
  2019) ships on far more machines (most Windows 10 installs since 2019, all Windows 11), so all
  three projects and `release.ps1` now target/publish `net48` instead of `net481`.

## v1

First release — see [README.md](README.md) for the full feature set (the three input fields,
testing/renaming, presets, localization/themes, window position).
