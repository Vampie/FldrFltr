using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Core.Model;

namespace App.Core.Execution
{
    /// <summary>Map + Extensielijst → bestanden opzoeken (§1/§4). Non-recursive: only the files
    /// directly inside the given folder, matching the single "Map" field — no sub-folder scanning.</summary>
    public static class FileMatcher
    {
        /// <summary>Splits "xml;csv;txt" into ["xml", "csv", "txt"], lower-cased and without a
        /// leading dot even if the user typed one. "*", empty, or whitespace-only means "match
        /// everything" and resolves to an empty array — callers should treat that as "no filter".</summary>
        public static string[] ParseExtensions(string extensionFilter)
        {
            if (string.IsNullOrWhiteSpace(extensionFilter) || extensionFilter.Trim() == "*")
            {
                return Array.Empty<string>();
            }

            return extensionFilter
                .Split(';')
                .Select(part => part.Trim().TrimStart('.').ToLowerInvariant())
                .Where(part => part.Length > 0)
                .Distinct()
                .ToArray();
        }

        public static List<FileEntry> FindFiles(string folder, string extensionFilter)
        {
            string[] extensions = ParseExtensions(extensionFilter);

            IEnumerable<FileInfo> files = new DirectoryInfo(folder)
                .EnumerateFiles("*", SearchOption.TopDirectoryOnly);

            if (extensions.Length > 0)
            {
                files = files.Where(f => extensions.Contains(f.Extension.TrimStart('.').ToLowerInvariant()));
            }

            return files
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Select(ToFileEntry)
                .ToList();
        }

        private static FileEntry ToFileEntry(FileInfo file) => new FileEntry
        {
            FullPath = file.FullName,
            Directory = file.DirectoryName,
            Name = file.Name,
            Extension = file.Extension.TrimStart('.'),
            SizeBytes = file.Length,
            CreatedUtc = file.CreationTimeUtc,
            ModifiedUtc = file.LastWriteTimeUtc
        };
    }
}
