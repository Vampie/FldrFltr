namespace App.Infrastructure.Configuration
{
    /// <summary>The only app-wide, persisted settings: language, theme (§1 of the projectbrief)
    /// and the window's last position/size. Presets are stored separately (PresetsFilePath)
    /// since they grow independently.</summary>
    public class AppSettings
    {
        public string Language { get; set; } = "nl";
        public string Theme { get; set; } = "Systeem";

        /// <summary>Null until the window has been closed at least once — MainWindow then falls
        /// back to a sensible default size/position (see MainWindow.ApplyWindowPlacement) instead
        /// of forcing every field to a sentinel value here.</summary>
        public double? WindowLeft { get; set; }
        public double? WindowTop { get; set; }
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }
    }
}
