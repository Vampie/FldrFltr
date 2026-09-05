using System.Windows;
using App.Infrastructure.Configuration;

namespace FldrFltr
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private AppSettings _settings;
        private bool _isInitializing = true;

        public MainWindow()
        {
            InitializeComponent();

            _settings = _settingsService.LoadOrCreateDefault();

            LanguageComboBox.ItemsSource = Localization.GetAvailableLanguages();
            LanguageComboBox.SelectedItem = Localization.CurrentLanguage;

            ThemeComboBox.ItemsSource = ThemeProvider.AvailableThemes;
            ThemeComboBox.SelectedItem = _settings.Theme;

            PresetsListBox.ItemsSource = new[] { Localization.Get("Presets.Empty") };

            _isInitializing = false;
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string picked = ModernFolderPicker.PickFolder(Localization.Get("Fields.FolderBrowseDialogTitle"), FolderTextBox.Text);
            if (picked != null)
            {
                FolderTextBox.Text = picked;
            }
        }

        private void InsertVariableButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedYet();
        }

        private void TestDryRunButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedYet();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedYet();
        }

        private void SaveAsPresetButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedYet();
        }

        private void LanguageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isInitializing || !(LanguageComboBox.SelectedItem is string language))
            {
                return;
            }

            _settings.Language = language;
            _settingsService.Save(_settings);
            MessageBox.Show(this, Localization.Get("Settings.LanguageChangedRestartRequired"), Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isInitializing || !(ThemeComboBox.SelectedItem is string theme))
            {
                return;
            }

            _settings.Theme = theme;
            _settingsService.Save(_settings);
            ThemeProvider.ApplySetting(theme);
        }

        private void ShowNotImplementedYet()
        {
            MessageBox.Show(this, Localization.Get("NotImplementedYet"), Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
