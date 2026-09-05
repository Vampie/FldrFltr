using System;

namespace App.Core.Model
{
    /// <summary>A saved Map + Extensielijst + Naamsjabloon combination (§1/§4 of the projectbrief).</summary>
    public class Preset
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; }
        public string Folder { get; set; }
        public string ExtensionFilter { get; set; }
        public string Template { get; set; }
        public DateTime LastUsedUtc { get; set; }

        /// <summary>Human-readable one-liner for the presets list, e.g.
        /// "Facturen → xml → {FileName}_{Year}.{Extension}" (matches the §1 mockup).</summary>
        public string DisplayText => $"{Name} → {(string.IsNullOrWhiteSpace(ExtensionFilter) ? "*" : ExtensionFilter)} → {Template}";
    }
}
