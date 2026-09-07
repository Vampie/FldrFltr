#Requires -Version 5.1
<#
.SYNOPSIS
    Bouwt FldrFltr in Release-configuratie en verpakt het resultaat als portable zip.
    Geen installer, geen snelkoppelingen, geen registry-writes — alles naast de exe.

.PARAMETER Version
    Major.Minor voor de release (bv. "1.0" of "1.0.0" — het patch-cijfer dat je hier meegeeft
    wordt genegeerd). Het patch-cijfer van de uiteindelijke versie is een doorlopende, in git
    bijgehouden bouw-teller (build/.build-counter) die bij elke build met 1 ophoogt, ongeacht
    welke Major.Minor je meegeeft. Voorbeeld: eerste build met -Version 1.0.0 -> 1.0.1, de tiende
    build (zelfde -Version) -> 1.0.10, en een daaropvolgende build met -Version 1.2 -> 1.2.11.

.EXAMPLE
    .\build\release.ps1 -Version 1.0
#>
param(
    [string]$Version = "0.0"
)

$ErrorActionPreference = "Stop"

$RepoRoot     = Split-Path -Parent $PSScriptRoot
$SlnPath      = Join-Path $RepoRoot "FldrFltr.slnx"
$IconPng      = Join-Path $RepoRoot "fldrfltr.png"
$PublishSrc   = Join-Path $RepoRoot "src\App.UI\bin\Release\net48"
$CounterPath  = Join-Path $PSScriptRoot ".build-counter"

$VersionParts = $Version.Split(".")
$Major = if ($VersionParts.Length -ge 1) { $VersionParts[0] } else { "0" }
$Minor = if ($VersionParts.Length -ge 2) { $VersionParts[1] } else { "0" }

$BuildNumber = 0
if (Test-Path $CounterPath) {
    $BuildNumber = [int](Get-Content $CounterPath -Raw).Trim()
}
$BuildNumber++
Set-Content -Path $CounterPath -Value $BuildNumber -NoNewline

$FullVersion = "$Major.$Minor.$BuildNumber"

$StagingDir = Join-Path $RepoRoot "release\FldrFltr-$FullVersion"

Write-Host "== FldrFltr release build v$FullVersion (build #$BuildNumber) ==" -ForegroundColor Cyan
Write-Host "Vergeet niet build/.build-counter mee te committen zodat de teller gedeeld blijft." -ForegroundColor DarkYellow

Write-Host "-- Bouwen (Release) --"
dotnet build $SlnPath -c Release "-p:Version=$FullVersion"
if ($LASTEXITCODE -ne 0) {
    throw "Build mislukt (exit code $LASTEXITCODE)."
}

if (-not (Test-Path $PublishSrc)) {
    throw "Build-output niet gevonden op $PublishSrc"
}

Write-Host "-- Verzamelen naar $StagingDir --"
if (Test-Path $StagingDir) {
    Remove-Item $StagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null

# Alles wat de app nodig heeft om te draaien: exe, .config, en alle NuGet-dependencies
# die de SDK-build al netjes naast de exe heeft gezet.
Get-ChildItem -Path $PublishSrc -File |
    Where-Object { $_.Extension -notin @(".pdb") } |
    Copy-Item -Destination $StagingDir -Force

# Subfolders next to the exe (Languages\<code>.json, ...) live as plain files, not embedded
# (zie Localization) — -File hierboven slaat ze over, dus kopieer elke submap generiek.
Get-ChildItem -Path $PublishSrc -Directory |
    ForEach-Object { Copy-Item -Path $_.FullName -Destination $StagingDir -Recurse -Force }

Copy-Item -Path $IconPng -Destination $StagingDir -Force

$ZipPath = Join-Path $RepoRoot "release\FldrFltr-$FullVersion.zip"
Write-Host "-- Zippen naar $ZipPath --"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}
Compress-Archive -Path (Join-Path $StagingDir "*") -DestinationPath $ZipPath

# Local scratch folder for quickly running the latest build by hand — not part of the release
# artifact itself, so files land flat (not under release\FldrFltr-<version>\), and existing
# settings.json/presets.json there (test data) are left alone since they aren't in the copy list.
$TestDir = Join-Path $RepoRoot "test_ACOT"
if (Test-Path $TestDir) {
    Write-Host "-- Kopieren naar $TestDir --"
    Get-ChildItem -Path $StagingDir -File | Copy-Item -Destination $TestDir -Force
    Get-ChildItem -Path $StagingDir -Directory |
        ForEach-Object { Copy-Item -Path $_.FullName -Destination $TestDir -Recurse -Force }
}

Write-Host ""
Write-Host "Klaar:" -ForegroundColor Green
Write-Host "  Map: $StagingDir"
if (Test-Path $TestDir) {
    Write-Host "  Test-map: $TestDir"
}
