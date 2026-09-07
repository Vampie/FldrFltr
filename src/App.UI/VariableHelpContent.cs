using System.Collections.Generic;
using System.Linq;

namespace FldrFltr
{
    /// <summary>
    /// Shared content for the two "what does {Variable} mean" views (VariableHelpWindow's
    /// always-on-top popup and MainWindow's collapsible side panel) — same grouping as
    /// VariableMenuHelper's insert menu, plus a description key per entry, resolved through
    /// Localization at build time (v2, requested after the presets/theme work).
    /// </summary>
    public static class VariableHelpContent
    {
        public class Entry
        {
            public string Token { get; }
            public string DescriptionKey { get; }
            public Entry(string token, string descriptionKey) { Token = token; DescriptionKey = descriptionKey; }
        }

        public class Group
        {
            public string TitleKey { get; }
            public Entry[] Entries { get; }
            public Group(string titleKey, Entry[] entries) { TitleKey = titleKey; Entries = entries; }
        }

        private static readonly Group[] Groups =
        {
            new Group("VarHelp.Group.General", new[]
            {
                new Entry("{Counter} / {Counter:100} / {Counter:100:5}", "VarHelp.Counter"),
                new Entry("{Guid}", "VarHelp.Guid"),
                new Entry("{Random} / {Random:0000}", "VarHelp.Random"),
                new Entry("{RandomString} / {RandomString:12}", "VarHelp.RandomString"),
            }),
            new Group("VarHelp.Group.File", new[]
            {
                new Entry("{FileName}", "VarHelp.FileName"),
                new Entry("{OriginalName}", "VarHelp.OriginalName"),
                new Entry("{Extension}", "VarHelp.Extension"),
                new Entry("{OriginalExtension}", "VarHelp.OriginalExtension"),
                new Entry("{FullPath}", "VarHelp.FullPath"),
                new Entry("{Directory}", "VarHelp.Directory"),
                new Entry("{FileSize}", "VarHelp.FileSize"),
                new Entry("{CreatedYear} ... {CreatedSecond}, {CreatedDate}, {CreatedTime}", "VarHelp.CreatedParts"),
                new Entry("{ModifiedYear} ... {ModifiedSecond}, {ModifiedDate}, {ModifiedTime}", "VarHelp.ModifiedParts"),
            }),
            new Group("VarHelp.Group.Date", new[]
            {
                new Entry("{Year} {Month} {Day} {Hour} {Minute} {Second}", "VarHelp.DateParts"),
                new Entry("{Date} / {Time}", "VarHelp.DateTime"),
            }),
        };

        public class DisplayItem
        {
            public string Token { get; set; }
            public string Description { get; set; }
        }

        public class DisplayGroup
        {
            public string Title { get; set; }
            public List<DisplayItem> Items { get; set; }
        }

        /// <summary>Resolves every group/entry's Localization key into actual display text —
        /// called fresh by each view (window + panel) so both always show the current language.</summary>
        public static List<DisplayGroup> BuildLocalized() =>
            Groups.Select(g => new DisplayGroup
            {
                Title = Localization.Get(g.TitleKey),
                Items = g.Entries.Select(e => new DisplayItem { Token = e.Token, Description = Localization.Get(e.DescriptionKey) }).ToList()
            }).ToList();
    }
}
