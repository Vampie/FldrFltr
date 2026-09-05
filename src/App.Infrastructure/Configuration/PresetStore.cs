using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Core.Model;
using Newtonsoft.Json;

namespace App.Infrastructure.Configuration
{
    /// <summary>Loads/saves presets.json next to the exe (§1/§4). Keeps a retained set of
    /// timestamped backups before every save, same principle as FldrSrtr's ConfigService.</summary>
    public class PresetStore
    {
        private const int BackupsToRetain = 10;

        public List<Preset> LoadAll()
        {
            if (!File.Exists(PortablePaths.PresetsFilePath))
            {
                return new List<Preset>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<Preset>>(File.ReadAllText(PortablePaths.PresetsFilePath))
                       ?? new List<Preset>();
            }
            catch (JsonException)
            {
                return new List<Preset>(); // corrupt presets.json: fall back rather than crash
            }
        }

        public void SaveAll(List<Preset> presets)
        {
            BackupExistingFile();
            File.WriteAllText(PortablePaths.PresetsFilePath, JsonConvert.SerializeObject(presets, Formatting.Indented));
        }

        private static void BackupExistingFile()
        {
            if (!File.Exists(PortablePaths.PresetsFilePath))
            {
                return;
            }

            string directory = Path.GetDirectoryName(PortablePaths.PresetsFilePath) ?? ".";
            string backupPath = Path.Combine(directory, $"presets.backup.{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");
            File.Copy(PortablePaths.PresetsFilePath, backupPath, overwrite: true);

            var oldBackups = Directory.GetFiles(directory, "presets.backup.*.json")
                .OrderByDescending(path => path)
                .Skip(BackupsToRetain);

            foreach (string oldBackup in oldBackups)
            {
                File.Delete(oldBackup);
            }
        }
    }
}
