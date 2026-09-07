using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Infrastructure.Configuration;
using Newtonsoft.Json;

namespace FldrFltr
{
    /// <summary>
    /// UI text lives in plain JSON files under "Languages/&lt;code&gt;.json" next to the exe —
    /// portable, no-rebuild-needed: adding a language means dropping in a new file, not touching
    /// code. Each file is a flat {"Some.Key": "Vertaalde tekst"} map. Keys are dotted by area
    /// purely as a human-readable convention — Localization itself treats them as opaque strings.
    ///
    /// A missing key falls back to the Default language's value, and a key missing from every
    /// file falls back to the key itself — visibly wrong rather than silently blank, never a crash.
    ///
    /// Language is applied once at startup (App.xaml.cs, before any window's XAML is parsed, since
    /// {local:Loc} resolves at load time, not via a live binding) — changing it needs a restart.
    /// (Copied from FldrSrtr's App.UI/Localization.cs — see FldrFltr's CLAUDE.md §3/§6.)
    /// </summary>
    public static class Localization
    {
        public const string DefaultLanguage = "nl";

        /// <summary>Used when the requested/saved language code doesn't match any file under
        /// Languages\ anymore (deleted, renamed, or a stale settings.json from an older install)
        /// — falls back to English rather than silently keeping an invalid code, which used to
        /// leave the language ComboBox showing blank (its SelectedItem didn't match anything in
        /// GetAvailableLanguages()).</summary>
        public const string FallbackLanguage = "en";

        private static Dictionary<string, string> _active = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _default = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static string LanguagesFolder => Path.Combine(PortablePaths.BaseDirectory, "Languages");

        public static string CurrentLanguage { get; private set; } = DefaultLanguage;

        public static void ApplyLanguage(string languageCode)
        {
            string requested = string.IsNullOrWhiteSpace(languageCode) ? DefaultLanguage : languageCode;
            CurrentLanguage = ResolveLanguage(requested);

            _default = LoadFile(DefaultLanguage) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _active = string.Equals(CurrentLanguage, DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                ? _default
                : LoadFile(CurrentLanguage) ?? _default;
        }

        /// <summary>Picks the requested language if its file actually exists, else English if
        /// that exists, else whatever language file IS available, else the bare default code (at
        /// which point every Get() call falls back to raw keys anyway — no file means no crash,
        /// just visibly-untranslated text).</summary>
        private static string ResolveLanguage(string requested)
        {
            string[] available = GetAvailableLanguages();
            if (available.Contains(requested, StringComparer.OrdinalIgnoreCase))
            {
                return requested;
            }
            if (available.Contains(FallbackLanguage, StringComparer.OrdinalIgnoreCase))
            {
                return FallbackLanguage;
            }
            return available.Length > 0 ? available[0] : DefaultLanguage;
        }

        /// <summary>Every language file found under Languages\ — a plain directory listing, so a
        /// user-added translation appears without any code change. Falls back to just the Default
        /// language if the folder itself is missing, so the language picker never shows an empty list.</summary>
        public static string[] GetAvailableLanguages()
        {
            if (!Directory.Exists(LanguagesFolder))
            {
                return new[] { DefaultLanguage };
            }

            string[] codes = Directory.GetFiles(LanguagesFolder, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return codes.Length > 0 ? codes : new[] { DefaultLanguage };
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (_active.TryGetValue(key, out string value))
            {
                return value;
            }

            return _default.TryGetValue(key, out string fallback) ? fallback : key;
        }

        /// <summary>Same as Get, but formats the result with String.Format — for strings that
        /// carry runtime values (counts, paths, ...), e.g. Get("Files.MatchCount", 12).</summary>
        public static string Get(string key, params object[] args) =>
            string.Format(Get(key), args);

        private static Dictionary<string, string> LoadFile(string languageCode)
        {
            string path = Path.Combine(LanguagesFolder, languageCode + ".json");
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path))
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                return null; // corrupt language file: fall back rather than crash the whole app over UI text
            }
        }
    }
}
