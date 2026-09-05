using System;
using System.IO;
using Newtonsoft.Json;

namespace App.Infrastructure.Configuration
{
    /// <summary>Loads/saves settings.json next to the exe. A missing or corrupt file falls back
    /// to defaults rather than crashing the app over UI preferences.</summary>
    public class SettingsService
    {
        public AppSettings LoadOrCreateDefault()
        {
            if (!File.Exists(PortablePaths.SettingsFilePath))
            {
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }

            try
            {
                return JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(PortablePaths.SettingsFilePath))
                       ?? new AppSettings();
            }
            catch (JsonException)
            {
                return new AppSettings(); // corrupt settings.json: fall back rather than crash
            }
        }

        public void Save(AppSettings settings)
        {
            File.WriteAllText(PortablePaths.SettingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}
