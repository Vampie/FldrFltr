using System;
using ModernWpf;

namespace FldrFltr
{
    /// <summary>
    /// Themes: three built-ins — "Systeem"/"Licht"/"Donker" (§3/§6 of the projectbrief: ModernWpfUI
    /// has no separate, explicitly-selectable "Hoog contrast" app theme, but Windows' own
    /// high-contrast mode keeps working automatically in the background via system colors when
    /// "Systeem" is selected). Applied via ModernWpf's ThemeManager, which re-themes every open
    /// window live — no restart needed, unlike Language.
    /// (Adapted from FldrSrtr's App.UI/ThemeProvider.cs — custom/accent themes are not part of
    /// FldrFltr's phase 1 scope, see CLAUDE.md §5 point 4.)
    /// </summary>
    public static class ThemeProvider
    {
        public const string SystemThemeName = "Systeem";
        public const string LightThemeName = "Licht";
        public const string DarkThemeName = "Donker";

        public static readonly string[] AvailableThemes = { SystemThemeName, LightThemeName, DarkThemeName };

        public static void ApplySetting(string themeName)
        {
            ApplicationTheme? baseTheme = Resolve(themeName);
            ThemeManager.Current.ApplicationTheme = baseTheme;
        }

        private static ApplicationTheme? Resolve(string themeName)
        {
            if (string.Equals(themeName, LightThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return ApplicationTheme.Light;
            }
            if (string.Equals(themeName, DarkThemeName, StringComparison.OrdinalIgnoreCase))
            {
                return ApplicationTheme.Dark;
            }

            return null; // "Systeem" (or unknown): follow the OS
        }
    }
}
