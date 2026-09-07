using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Core.Model;

namespace App.Core.Execution
{
    /// <summary>Orchestrates FileMatcher + VariableResolver + ConflictResolver into the dry-run
    /// plan, and (separately) actually performs the rename (§1/§4 of the projectbrief). A template
    /// can contain "\" (or "/") to move a file into a subfolder, or "..\" to move it up into a
    /// parent folder — e.g. "{OriginalExtension}\{FileName}.{Counter:100}" sorts files into
    /// per-extension subfolders, "..\{OriginalExtension}\{FileName}.{Counter:100}" moves them up
    /// a level first. Every other segment of the template is still just a name, sanitized the
    /// same as before.</summary>
    public static class RenameEngine
    {
        public static List<RenamePlan> BuildPlan(RenameOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Folder) || !Directory.Exists(options.Folder))
            {
                throw new DirectoryNotFoundException(options.Folder);
            }

            List<FileEntry> files = FileMatcher.FindFiles(options.Folder, options.ExtensionFilter);
            DateTime now = DateTime.Now; // fixed once per batch — every file in this run shares the same {Year}/{Time}/...

            var counters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int NextCounterValue(string spec)
            {
                VariableResolver.ParseCounterSpec(spec, out int start, out int step);
                int value = counters.TryGetValue(spec, out int previous) ? previous + step : start;
                counters[spec] = value;
                return value;
            }

            var usedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var plan = new List<RenamePlan>(files.Count);

            foreach (FileEntry file in files)
            {
                string resolvedTemplate = VariableResolver.Resolve(options.Template, file, now, NextCounterValue);
                string relativePath = SanitizeRelativePath(resolvedTemplate);
                string desiredPath = Path.GetFullPath(Path.Combine(file.Directory, relativePath));

                (string finalPath, RenameStatus status, string detail) = ConflictResolver.Resolve(
                    desiredPath, file.FullPath, options.ConflictPolicy, usedPaths);

                plan.Add(new RenamePlan { Source = file, NewFullPath = finalPath, Status = status, StatusDetail = detail });
            }

            return plan;
        }

        /// <summary>Actually renames (and, if the template's path led it elsewhere, moves) every
        /// planned file that isn't Skipped/NoChange — creating any subfolder the template pointed
        /// at first. Continues past a per-file failure (permissions, file in use, ...) rather than
        /// aborting the whole batch — the plan's Status/StatusDetail is updated in place so the UI
        /// can show exactly which files succeeded and which didn't.</summary>
        public static void Execute(IEnumerable<RenamePlan> plan)
        {
            foreach (RenamePlan entry in plan)
            {
                if (entry.Status == RenameStatus.SkippedConflict || entry.Status == RenameStatus.NoChange)
                {
                    continue;
                }

                try
                {
                    string targetDirectory = Path.GetDirectoryName(entry.NewFullPath);
                    if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Only reachable for a ConflictPolicy.Overwrite match — AutoRename/no-conflict
                    // paths are already guaranteed free by ConflictResolver.
                    if (File.Exists(entry.NewFullPath) &&
                        !string.Equals(entry.NewFullPath, entry.Source.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(entry.NewFullPath);
                    }

                    File.Move(entry.Source.FullPath, entry.NewFullPath);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    entry.Status = RenameStatus.Error;
                    entry.StatusDetail = ex.Message;
                }
            }
        }

        /// <summary>Sanitizes a resolved naamsjabloon segment-by-segment, splitting on "\" and "/"
        /// first so those (and a literal ".."/"." navigation segment) survive as real path
        /// structure instead of being scrubbed out — only the characters actually invalid within
        /// a single file/folder name (e.g. from a variable like {FullPath} carrying stray
        /// characters) get replaced, and only within their own segment (§1 "Veiligheid": a bad
        /// template shouldn't crash the batch).</summary>
        private static string SanitizeRelativePath(string resolvedTemplate)
        {
            if (string.IsNullOrEmpty(resolvedTemplate))
            {
                return resolvedTemplate;
            }

            var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars().Except(new[] { '\\', '/' }));

            IEnumerable<string> segments = resolvedTemplate.Split('\\', '/')
                .Select(segment => segment == ".." || segment == "." || segment.Length == 0
                    ? segment
                    : new string(segment.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray()));

            return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
        }
    }
}
