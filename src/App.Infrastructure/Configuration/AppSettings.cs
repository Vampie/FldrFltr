namespace App.Infrastructure.Configuration
{
    /// <summary>The only app-wide, persisted settings: language and theme (§1 of the projectbrief).
    /// Presets are stored separately (PresetsFilePath) since they grow independently.</summary>
    public class AppSettings
    {
        public string Language { get; set; } = "nl";
        public string Theme { get; set; } = "Systeem";
    }
}
