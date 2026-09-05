using System.Windows;
using App.Infrastructure.Configuration;

namespace FldrFltr
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                PortablePaths.EnsureBaseDirectoryIsWritable();
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "FldrFltr", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            AppSettings settings = new SettingsService().LoadOrCreateDefault();
            Localization.ApplyLanguage(settings.Language);
            ThemeProvider.ApplySetting(settings.Theme);
        }
    }
}
