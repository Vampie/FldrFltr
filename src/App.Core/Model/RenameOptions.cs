namespace App.Core.Model
{
    /// <summary>The three input fields plus the conflict policy — everything RenameEngine needs
    /// to build a plan.</summary>
    public class RenameOptions
    {
        public string Folder { get; set; }

        /// <summary>Raw text as typed, e.g. "xml", "xml;csv;txt", "*" or empty (both mean "all files").</summary>
        public string ExtensionFilter { get; set; }

        public string Template { get; set; }

        public ConflictPolicy ConflictPolicy { get; set; } = ConflictPolicy.AutoRename;
    }
}
