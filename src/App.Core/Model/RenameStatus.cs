namespace App.Core.Model
{
    /// <summary>Outcome kind of one planned/executed rename — kept as an enum rather than a plain
    /// string so the UI can localize the status text (see MainWindow's Loc keys) instead of Core
    /// hard-coding Dutch or English messages.</summary>
    public enum RenameStatus
    {
        /// <summary>Ready to rename (dry-run) / renamed successfully (after execution).</summary>
        Ok,

        /// <summary>The template resolved to the file's current name — nothing to do.</summary>
        NoChange,

        /// <summary>AutoRename policy kicked in — Message carries the "(1)"/"(2)" suffix that was added.</summary>
        AutoRenamed,

        /// <summary>Skip policy kicked in because the target name was already taken.</summary>
        SkippedConflict,

        /// <summary>File.Move (or a pre-delete for Overwrite) threw — Message carries the exception text.</summary>
        Error
    }
}
