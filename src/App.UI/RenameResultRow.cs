using App.Core.Model;

namespace FldrFltr
{
    /// <summary>Adapts a RenamePlan (Core, no localization) into a bindable row with a localized
    /// status text for the results DataGrid.</summary>
    public class RenameResultRow
    {
        public string From { get; }
        public string To { get; }
        public string StatusText { get; }

        public RenameResultRow(RenamePlan plan)
        {
            From = plan.From;
            To = plan.To;
            StatusText = plan.Status switch
            {
                RenameStatus.Ok => Localization.Get("Status.Ok"),
                RenameStatus.NoChange => Localization.Get("Status.NoChange"),
                RenameStatus.AutoRenamed => Localization.Get("Status.AutoRenamed", plan.StatusDetail),
                RenameStatus.SkippedConflict => Localization.Get("Status.SkippedConflict"),
                RenameStatus.Error => Localization.Get("Status.Error", plan.StatusDetail),
                _ => plan.Status.ToString()
            };
        }
    }
}
