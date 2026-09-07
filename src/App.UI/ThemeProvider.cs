using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using App.Infrastructure.Configuration;
using ModernWpf;
using Newtonsoft.Json;

namespace FldrFltr
{
    /// <summary>
    /// Themes: three built-ins — "Systeem"/"Licht"/"Donker" (§3/§6 of the projectbrief: ModernWpfUI
    /// has no separate, explicitly-selectable "Hoog contrast" app theme, but Windows' own
    /// high-contrast mode keeps working automatically in the background via system colors when
    /// "Systeem" is selected) — plus any number of extra palettes, each a small JSON file under
    /// "Themes/&lt;Name&gt;.json" next to the exe (same portable, no-rebuild-needed philosophy as
    /// Languages). §5 phase 4 ships a batch of editor-inspired palettes this way; dropping in
    /// more/your own is just adding another file, no UI needed.
    ///
    /// A palette is Base (Light/Dark, decides which chrome ModernWpf's controls use — inputs,
    /// buttons, ...) + AccentColor (button/highlight tint) + Background/Foreground. The last two
    /// exist because AccentColor alone only tints a handful of control chrome elements — it does
    /// NOT make the app look distinctly "Monokai-colored" or "khaki-colored" the way the source
    /// editor theme does, it's still just generic light-gray-on-white or white-on-near-black with
    /// a colored button border. Background/Foreground are applied directly onto the window
    /// (MainWindow.xaml binds Background/Foreground to PageBackgroundBrush/PageForegroundBrush)
    /// and onto the same "SystemControlBackgroundChromeMediumLowBrush" key the CardBorder style
    /// already used before custom themes existed, so a palette's own background color actually
    /// shows through instead of only its accent. Removing the override (built-ins/System have no
    /// Background) lets DynamicResource fall back to ModernWpf's own definition of that key again.
    ///
    /// Two sources for the shipped palettes: Base+AccentColor+Background+Foreground for
    /// Choco/DansLeRuSH-Dark/DarkModeDefault/Deep Black/Hello Kitty/HotFudgeSundae/
    /// Mono Industrial/Monokai/MossyLawn/Khaki come straight from the actual Notepad++ .xml theme
    /// files under C:\claude_code\Resources\themes (their GlobalStyles "Default Style" fg/bg for
    /// Background/Foreground, a distinctive syntax-highlight color for the accent). Dracula/
    /// Material/Lunar/Nord/Neon/ICLS/Bespin/"Slush &amp; Poppies"/Obsidian/"Nautical but Nice"/
    /// "Waher Style"/Solarized come from https://www.spec-india.com/blog/notepad-themes — the
    /// actual published palette where one exists (Dracula, Nord, Solarized), best-effort otherwise.
    /// Applied via ModernWpf's ThemeManager, which re-themes every open window live — no restart
    /// needed, unlike Language. (Adapted from FldrSrtr's App.UI/ThemeProvider.cs.)
    /// </summary>
    public static class ThemeProvider
    {
        public const string SystemThemeName = "Systeem";
        public const string LightThemeName = "Licht";
        public const string DarkThemeName = "Donker";

        private const string CardBackgroundResourceKey = "SystemControlBackgroundChromeMediumLowBrush";
        private const string PageBackgroundResourceKey = "PageBackgroundBrush";
        private const string PageForegroundResourceKey = "PageForegroundBrush";

        private static readonly string[] BuiltInNames = { SystemThemeName, LightThemeName, DarkThemeName };

        public static string ThemesRootFolder => Path.Combine(PortablePaths.BaseDirectory, "Themes");

        public static void ApplySetting(string themeName)
        {
            (ApplicationTheme? baseTheme, string accentHex, string backgroundHex, string foregroundHex) = Resolve(themeName);

            ThemeManager.Current.ApplicationTheme = baseTheme;
            ThemeManager.Current.AccentColor = string.IsNullOrEmpty(accentHex)
                ? (Color?)null
                : (Color)ColorConverter.ConvertFromString(accentHex);

            ApplyPageColors(backgroundHex, foregroundHex);
        }

        private static (ApplicationTheme?, string accent, string background, string foreground) Resolve(string themeName)
        {
            if (string.Equals(themeName, LightThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return (ApplicationTheme.Light, null, null, null);
            }
            if (string.Equals(themeName, DarkThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return (ApplicationTheme.Dark, null, null, null);
            }
            if (string.Equals(themeName, SystemThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return (null, null, null, null);
            }

            ThemeFile file = LoadThemeFile(themeName);
            if (file == null)
            {
                return (null, null, null, null); // unknown/missing custom theme: fall back to following the OS
            }

            ApplicationTheme? based = string.Equals(file.Base, "Light", StringComparison.OrdinalIgnoreCase)
                ? ApplicationTheme.Light
                : string.Equals(file.Base, "Dark", StringComparison.OrdinalIgnoreCase)
                    ? ApplicationTheme.Dark
                    : (ApplicationTheme?)null;
            return (based, file.AccentColor, file.Background, file.Foreground);
        }

        /// <summary>Overrides the window's own background/foreground and the card surface color
        /// directly in Application.Resources — a dictionary's own entries take precedence over
        /// the same key defined inside its MergedDictionaries, so this shadows ModernWpf's default
        /// card brush without needing to touch App.xaml. Removing the keys (built-ins/System) lets
        /// that default show through again instead of leaving a stale color behind.</summary>
        private static void ApplyPageColors(string backgroundHex, string foregroundHex)
        {
            ResourceDictionary resources = Application.Current.Resources;

            if (string.IsNullOrEmpty(backgroundHex))
            {
                resources.Remove(PageBackgroundResourceKey);
                resources.Remove(PageForegroundResourceKey);
                resources.Remove(CardBackgroundResourceKey);
                return;
            }

            Color background = (Color)ColorConverter.ConvertFromString(backgroundHex);
            bool backgroundIsDark = IsDark(background);
            Color foreground = string.IsNullOrEmpty(foregroundHex)
                ? (backgroundIsDark ? Colors.White : Colors.Black)
                : (Color)ColorConverter.ConvertFromString(foregroundHex);
            // Card surface: same hue as the page, nudged toward white (dark bg) or black (light
            // bg) so panels stay visually "raised" instead of blending into the page.
            Color card = Blend(background, backgroundIsDark ? Colors.White : Colors.Black, backgroundIsDark ? 0.12 : 0.06);

            resources[PageBackgroundResourceKey] = new SolidColorBrush(background);
            resources[PageForegroundResourceKey] = new SolidColorBrush(foreground);
            resources[CardBackgroundResourceKey] = new SolidColorBrush(card);
        }

        private static bool IsDark(Color color) =>
            0.299 * color.R + 0.587 * color.G + 0.114 * color.B < 128;

        private static Color Blend(Color baseColor, Color towards, double amount) => Color.FromRgb(
            (byte)(baseColor.R + (towards.R - baseColor.R) * amount),
            (byte)(baseColor.G + (towards.G - baseColor.G) * amount),
            (byte)(baseColor.B + (towards.B - baseColor.B) * amount));

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
            public string Background { get; set; }
            public string Foreground { get; set; }
        }
    }
}
