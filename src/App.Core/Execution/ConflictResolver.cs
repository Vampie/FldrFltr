using System;
using System.Collections.Generic;
using System.IO;
using App.Core.Model;

namespace App.Core.Execution
{
    /// <summary>Decides what happens when a computed new path is already taken — either by a file
    /// already on disk, or by an earlier file in the same batch that resolved to the same name
    /// (§1 "Veiligheid": skip/overwrite/auto-"(1)"-suffix, default AutoRename, never silently
    /// overwrite). usedPaths accumulates every path this batch has claimed so far — pass the same
    /// set across the whole run.</summary>
    public static class ConflictResolver
    {
        public static (string FinalPath, RenameStatus Status, string StatusDetail) Resolve(
            string desiredPath, string sourceFullPath, ConflictPolicy policy, HashSet<string> usedPaths)
        {
            bool sameAsSource = string.Equals(desiredPath, sourceFullPath, StringComparison.OrdinalIgnoreCase);
            if (sameAsSource)
            {
                usedPaths.Add(desiredPath);
                return (desiredPath, RenameStatus.NoChange, null);
            }

            bool taken = usedPaths.Contains(desiredPath) || File.Exists(desiredPath);
            if (!taken)
            {
                usedPaths.Add(desiredPath);
                return (desiredPath, RenameStatus.Ok, null);
            }

            switch (policy)
            {
                case ConflictPolicy.Overwrite:
                    usedPaths.Add(desiredPath);
                    return (desiredPath, RenameStatus.Ok, null);

                case ConflictPolicy.Skip:
                    return (desiredPath, RenameStatus.SkippedConflict, null);

                case ConflictPolicy.AutoRename:
                default:
                    string autoPath = FindFreeAutoRenamedPath(desiredPath, usedPaths);
                    usedPaths.Add(autoPath);
                    string suffix = Path.GetFileNameWithoutExtension(autoPath)
                        .Substring(Path.GetFileNameWithoutExtension(desiredPath).Length);
                    return (autoPath, RenameStatus.AutoRenamed, suffix);
            }
        }

        private static string FindFreeAutoRenamedPath(string desiredPath, HashSet<string> usedPaths)
        {
            string directory = Path.GetDirectoryName(desiredPath) ?? string.Empty;
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(desiredPath);
            string extension = Path.GetExtension(desiredPath);

            for (int suffix = 1; ; suffix++)
            {
                string candidate = Path.Combine(directory, $"{nameWithoutExtension} ({suffix}){extension}");
                if (!usedPaths.Contains(candidate) && !File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }
    }
}
