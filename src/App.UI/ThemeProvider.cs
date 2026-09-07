using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Infrastructure.Configuration;
using ModernWpf;
using Newtonsoft.Json;

namespace FldrFltr
{
    /// <summary>
    /// Themes: three built-ins — "Systeem"/"Licht"/"Donker" (§3/§6 of the projectbrief: ModernWpfUI
    /// has no separate, explicitly-selectable "Hoog contrast" app theme, but Windows' own
    /// high-contrast mode keeps working automatically in the background via system colors when
    /// "Systeem" is selected) — plus any number of extra accent palettes, each a small JSON file
    /// under "Themes/&lt;Name&gt;.json" next to the exe (same portable, no-rebuild-needed
    /// philosophy as Languages). §5 phase 4 ships a batch of editor-inspired palettes this way;
    /// dropping in more/your own is just adding another file, no UI needed.
    /// Two sources: Base+AccentColor for Choco/DansLeRuSH-Dark/DarkModeDefault/Deep Black/
    /// Hello Kitty/HotFudgeSundae/Mono Industrial/Monokai/MossyLawn/Khaki come straight from the
    /// actual Notepad++ .xml theme files under C:\claude_code\Resources\themes (their
    /// GlobalStyles "Default Style" for Base, a distinctive syntax-highlight color for the
    /// accent). Dracula/Material/Lunar/Nord/Neon/ICLS/Bespin/"Slush &amp; Poppies"/Obsidian/
    /// "Nautical but Nice"/"Waher Style" come from the (color-light) descriptions on
    /// https://www.spec-india.com/blog/notepad-themes — canonical published palettes where one
    /// exists (Dracula, Nord), best-effort otherwise; Solarized was already covered by phase 4's
    /// own Solarized Dark/Light. Applied via ModernWpf's ThemeManager, which re-themes every open
    /// window live — no restart needed, unlike Language. (Adapted from FldrSrtr's
    /// App.UI/ThemeProvider.cs.)
    /// </summary>
    public static class ThemeProvider
    {
        public const string SystemThemeName = "Systeem";
        public const string LightThemeName = "Licht";
        public const string DarkThemeName = "Donker";

        private static readonly string[] BuiltInNames = { SystemThemeName, LightThemeName, DarkThemeName };

        public static string ThemesRootFolder => Path.Combine(PortablePaths.BaseDirectory, "Themes");

        public static void ApplySetting(string themeName)
        {
            (ApplicationTheme? baseTheme, string accentHex) = Resolve(themeName);
            ThemeManager.Current.ApplicationTheme = baseTheme;
            ThemeManager.Current.AccentColor = string.IsNullOrEmpty(accentHex)
                ? (System.Windows.Media.Color?)null
                : (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(accentHex);
        }

        private static (ApplicationTheme?, string) Resolve(string themeName)
        {
            if (string.Equals(themeName, LightThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return (ApplicationTheme.Light, null);
            }
            if (string.Equals(themeName, DarkThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return (ApplicationTheme.Dark, null);
            }
            if (string.Equals(themeName, SystemThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return (null, null);
            }

            ThemeFile file = LoadThemeFile(themeName);
            if (file == null)
            {
                return (null, null); // unknown/missing custom theme: fall back to following the OS
            }

            ApplicationTheme? based = string.Equals(file.Base, "Light", StringComparison.OrdinalIgnoreCase)
                ? ApplicationTheme.Light
                : string.Equals(file.Base, "Dark", StringComparison.OrdinalIgnoreCase)
                    ? ApplicationTheme.Dark
                    : (ApplicationTheme?)null;
            return (based, file.AccentColor);
        }

        private static ThemeFile LoadThemeFile(string themeName)
        {
            string path = Path.Combine(ThemesRootFolder, themeName + ".json");
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<ThemeFile>(File.ReadAllText(path));
            }
            catch (JsonException)
            {
                return null; // corrupt theme file: fall back rather than crash
            }
        }

        /// <summary>Built-in names first, then any palette found under Themes\ (alphabetical, and
        /// never duplicating a built-in name even if a stray file happens to match one).</summary>
        public static string[] GetAvailableThemes()
        {
            IEnumerable<string> custom = Directory.Exists(ThemesRootFolder)
                ? Directory.GetFiles(ThemesRootFolder, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(name => !BuiltInNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                : Enumerable.Empty<string>();

            return BuiltInNames.Concat(custom).ToArray();
        }

        private class ThemeFile
        {
            public string Base { get; set; }
            public string AccentColor { get; set; }
        }
    }
}
