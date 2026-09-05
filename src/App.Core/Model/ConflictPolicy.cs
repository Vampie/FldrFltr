namespace App.Core.Model
{
    /// <summary>What to do when a computed new name collides with an existing file, or with
    /// another file's computed new name in the same batch (§1 "Veiligheid" of the projectbrief).
    /// Default is AutoRename — never silently overwrite.</summary>
    public enum ConflictPolicy
    {
        Skip,
        Overwrite,
        AutoRename
    }
}
