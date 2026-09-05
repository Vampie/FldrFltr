using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Core.Model;

namespace App.Core.Execution
{
    /// <summary>Orchestrates FileMatcher + VariableResolver + ConflictResolver into the dry-run
    /// plan, and (separately) actually performs the rename (§1/§4 of the projectbrief). Hernoemen
    /// ≠ verplaatsen: files stay in the same folder, only the name changes.</summary>
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
                string resolvedName = SanitizeFileName(VariableResolver.Resolve(options.Template, file, now, NextCounterValue));
                string desiredPath = Path.Combine(file.Directory, resolvedName);

                (string finalPath, RenameStatus status, string detail) = ConflictResolver.Resolve(
                    desiredPath, file.FullPath, options.ConflictPolicy, usedPaths);

                plan.Add(new RenamePlan { Source = file, NewFullPath = finalPath, Status = status, StatusDetail = detail });
            }

            return plan;
        }

        /// <summary>Actually renames every planned file that isn't Skipped/NoChange. Continues
        /// past a per-file failure (permissions, file in use, ...) rather than aborting the whole
        /// batch — the plan's Status/StatusDetail is updated in place so the UI can show exactly
        /// which files succeeded and which didn't.</summary>
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

        /// <summary>Strips characters that can't appear in a Windows file name — a template can
        /// combine literal text with variables like {FullPath} that carry path separators, and a
        /// bad template shouldn't crash the batch (§1 "Veiligheid").</summary>
        private static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.IsNullOrEmpty(fileName)
                ? fileName
                : new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        }
    }
}
