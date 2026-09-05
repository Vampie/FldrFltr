using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using App.Core.Model;

namespace App.Core.Execution
{
    /// <summary>
    /// Resolves the {Variables} listed in §2 of the projectbrief inside a naamsjabloon.
    /// {FileName}/{OriginalName} and {Extension}/{OriginalExtension} are aliases of each other —
    /// kept as separate tokens purely so a template that also adds new, literal name parts stays
    /// readable (see §2's table). {Year}/{Month}/... use the moment the rename runs (the "now"
    /// passed in, fixed once per batch so every file in the same run gets the same value);
    /// {CreatedYear}/{ModifiedYear}/... use the file's own timestamps instead.
    /// {Counter}/{Counter:100}/{Counter:100:5} is resolved via the counterResolver callback —
    /// RenameEngine supplies one that's fresh per run and increments once per distinct spec.
    /// {Guid}, {Random}/{Random:0000}, and {RandomString}/{RandomString:12} are generated fresh
    /// on every single resolve. (Adapted from FldrSrtr's App.Core/Execution/VariableResolver.cs.)
    /// </summary>
    public static class VariableResolver
    {
        private static readonly Regex TokenPattern = new Regex(@"\{([^{}]+)\}", RegexOptions.Compiled);
        private static readonly Random RandomGenerator = new Random();
        private const string RandomStringAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string Resolve(string template, FileEntry file, DateTime now, Func<string, int> counterResolver = null)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            Dictionary<string, string> tokens = BuildTokenMap(file, now);

            return TokenPattern.Replace(template, match =>
            {
                string spec = match.Groups[1].Value;

                if (IsSpec(spec, "Counter"))
                {
                    return counterResolver != null ? counterResolver(spec).ToString(CultureInfo.InvariantCulture) : match.Value;
                }
                if (spec.Equals("Guid", StringComparison.OrdinalIgnoreCase))
                {
                    return Guid.NewGuid().ToString("N");
                }
                if (IsSpec(spec, "RandomString"))
                {
                    return ResolveRandomString(spec);
                }
                if (IsSpec(spec, "Random"))
                {
                    return ResolveRandomNumber(spec);
                }

                return tokens.TryGetValue(spec, out string value) ? value : match.Value;
            });
        }

        private static bool IsSpec(string spec, string name) =>
            spec.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            spec.StartsWith(name + ":", StringComparison.OrdinalIgnoreCase);

        private static string ExtractParameter(string spec)
        {
            int colonIndex = spec.IndexOf(':');
            return colonIndex >= 0 && colonIndex < spec.Length - 1 ? spec.Substring(colonIndex + 1) : null;
        }

        /// <summary>{Random} defaults to a 6-digit zero-padded number; {Random:0000} uses the
        /// parameter as a .NET custom numeric format string.</summary>
        private static string ResolveRandomNumber(string spec)
        {
            string pattern = ExtractParameter(spec);
            if (string.IsNullOrEmpty(pattern) || !pattern.All(c => c == '0' || c == '#'))
            {
                pattern = "000000";
            }

            int digitCount = Math.Min(pattern.Length, 18);
            long max = (long)Math.Pow(10, digitCount);
            long value = (long)(RandomGenerator.NextDouble() * max);
            return value.ToString(pattern, CultureInfo.InvariantCulture);
        }

        /// <summary>{RandomString} defaults to 8 characters; {RandomString:12} sets the length.</summary>
        private static string ResolveRandomString(string spec)
        {
            string parameter = ExtractParameter(spec);
            int length = 8;
            if (!string.IsNullOrEmpty(parameter) &&
                int.TryParse(parameter, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedLength) &&
                parsedLength > 0)
            {
                length = Math.Min(parsedLength, 256);
            }

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = RandomStringAlphabet[RandomGenerator.Next(RandomStringAlphabet.Length)];
            }
            return new string(chars);
        }

        /// <summary>Parses "Counter", "Counter:100" or "Counter:100:5" into (start, step), defaulting to (1, 1).</summary>
        public static void ParseCounterSpec(string spec, out int start, out int step)
        {
            start = 1;
            step = 1;

            string[] parts = (spec ?? string.Empty).Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedStart))
            {
                start = parsedStart;
            }
            if (parts.Length >= 3 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedStep))
            {
                step = parsedStep;
            }
        }

        private static Dictionary<string, string> BuildTokenMap(FileEntry file, DateTime now)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FileName"] = Path.GetFileNameWithoutExtension(file.Name),
                ["OriginalName"] = Path.GetFileNameWithoutExtension(file.Name),
                ["Extension"] = file.Extension,
                ["OriginalExtension"] = file.Extension,
                ["FullPath"] = file.FullPath,
                ["Directory"] = file.Directory,
                ["FileSize"] = file.SizeBytes.ToString(CultureInfo.InvariantCulture)
            };

            AddDateTokens(tokens, string.Empty, now);
            AddDateTokens(tokens, "Created", file.CreatedUtc.ToLocalTime());
            AddDateTokens(tokens, "Modified", file.ModifiedUtc.ToLocalTime());

            return tokens;
        }

        private static void AddDateTokens(Dictionary<string, string> tokens, string prefix, DateTime moment)
        {
            tokens[$"{prefix}Year"] = moment.ToString("yyyy");
            tokens[$"{prefix}Month"] = moment.ToString("MM");
            tokens[$"{prefix}Day"] = moment.ToString("dd");
            tokens[$"{prefix}Hour"] = moment.ToString("HH");
            tokens[$"{prefix}Minute"] = moment.ToString("mm");
            tokens[$"{prefix}Second"] = moment.ToString("ss");
            tokens[$"{prefix}Date"] = moment.ToString("yyyy-MM-dd");
            tokens[$"{prefix}Time"] = moment.ToString("HH-mm-ss");
        }
    }
}
