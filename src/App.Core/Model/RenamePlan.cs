namespace App.Core.Model
{
    /// <summary>One row of the result table (§1: "Resultaat (na Testen/Hernoemen): tabel met
    /// van/naar/status"). Built by RenameEngine.BuildPlan for the dry-run, then mutated in place
    /// by RenameEngine.Execute to reflect what actually happened.</summary>
    public class RenamePlan
    {
        public FileEntry Source { get; set; }

        /// <summary>The full path the file will get (or got) — equals Source.FullPath's directory
        /// plus the resolved, conflict-adjusted file name.</summary>
        public string NewFullPath { get; set; }

        public RenameStatus Status { get; set; }

        /// <summary>Extra detail for Error (exception message) or AutoRenamed (the suffix that was
        /// added) — the UI formats this into the localized status text.</summary>
        public string StatusDetail { get; set; }

        public string From => Source.FullPath;
        public string To => NewFullPath;
    }
}
