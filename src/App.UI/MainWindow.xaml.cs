using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using App.Core.Execution;
using App.Core.Model;
using App.Infrastructure.Configuration;

namespace FldrFltr
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private readonly PresetStore _presetStore = new PresetStore();
        private readonly AppSettings _settings;
        private readonly ObservableCollection<Preset> _presets;
        private bool _isInitializing = true;

        /// <summary>Name of the preset the user last loaded (double-clicked), so "Opslaan als
        /// preset..." can pre-fill it — saving under the same name then just edits that preset
        /// instead of asking for a fresh name every time. Null until a preset is loaded or saved
        /// this session.</summary>
        private string _lastLoadedPresetName;

        public MainWindow()
        {
            InitializeComponent();

            Title = $"{Title} v{GetAppVersion()}";

            _settings = _settingsService.LoadOrCreateDefault();

            LanguageComboBox.ItemsSource = Localization.GetAvailableLanguages();
            LanguageComboBox.SelectedItem = Localization.CurrentLanguage;

            ThemeComboBox.ItemsSource = ThemeProvider.GetAvailableThemes();
            ThemeComboBox.SelectedItem = _settings.Theme;

            ConflictPolicyComboBox.ItemsSource = new[]
            {
                new ConflictPolicyOption(ConflictPolicy.AutoRename, Localization.Get("ConflictPolicy.AutoRename")),
                new ConflictPolicyOption(ConflictPolicy.Skip, Localization.Get("ConflictPolicy.Skip")),
                new ConflictPolicyOption(ConflictPolicy.Overwrite, Localization.Get("ConflictPolicy.Overwrite"))
            };
            ConflictPolicyComboBox.SelectedIndex = 0;

            _presets = new ObservableCollection<Preset>(_presetStore.LoadAll());
            PresetsListBox.ItemsSource = _presets;
            _presets.CollectionChanged += (_, __) => UpdatePresetsEmptyState();
            UpdatePresetsEmptyState();

            _isInitializing = false;
        }

        private void UpdatePresetsEmptyState()
        {
            PresetsEmptyTextBlock.Visibility = _presets.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string picked = ModernFolderPicker.PickFolder(Localization.Get("Fields.FolderBrowseDialogTitle"), FolderTextBox.Text);
            if (picked != null)
            {
                FolderTextBox.Text = picked;
            }
        }

        private void InsertVariableButton_Click(object sender, RoutedEventArgs e) =>
            VariableMenuHelper.ShowVariableMenu((Button)sender, TemplateTextBox);

        private async void TestDryRunButton_Click(object sender, RoutedEventArgs e) => await RunPlanAsync(execute: false);

        private async void RenameButton_Click(object sender, RoutedEventArgs e) => await RunPlanAsync(execute: true);

        /// <summary>Runs FileMatcher/rename on a background thread so the UI stays responsive,
        /// with a simple status text (no real progress bar needed — see StatusTextBlock in
        /// MainWindow.xaml) showing "busy" while it runs and a result summary once it's done.</summary>
        private async Task RunPlanAsync(bool execute)
        {
            SetBusy(Localization.Get(execute ? "Status.Busy.Rename" : "Status.Busy.Test"));
            try
            {
                RenameOptions options = BuildOptionsFromFields();
                List<RenamePlan> plan = await Task.Run(() =>
                {
                    List<RenamePlan> builtPlan = RenameEngine.BuildPlan(options);
                    if (execute)
                    {
                        RenameEngine.Execute(builtPlan);
                    }
                    return builtPlan;
                });

                ResultsDataGrid.ItemsSource = plan.Select(p => new RenameResultRow(p)).ToList();
                StatusTextBlock.Text = Localization.Get(execute ? "Status.Done.Rename" : "Status.Done.Test", plan.Count);
            }
            catch (DirectoryNotFoundException)
            {
                StatusTextBlock.Text = string.Empty;
                MessageBox.Show(this, Localization.Get("Errors.FolderMissing"), Localization.Get("Errors.Title"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                SetBusy(null);
            }
        }

        /// <summary>Toggles the "busy" state: a status message plus disabling the buttons that
        /// would let the user start a second run (or edit the fields/preset) while one is already
        /// in flight. Pass null to clear the busy state (the caller sets StatusTextBlock's final
        /// text separately once the result is known).</summary>
        private void SetBusy(string busyText)
        {
            bool isBusy = busyText != null;
            if (isBusy)
            {
                StatusTextBlock.Text = busyText;
            }

            TestDryRunButton.IsEnabled = !isBusy;
            RenameButton.IsEnabled = !isBusy;
            SaveAsPresetButton.IsEnabled = !isBusy;
        }

        private RenameOptions BuildOptionsFromFields() => new RenameOptions
        {
            Folder = FolderTextBox.Text.Trim(),
            ExtensionFilter = ExtensionsTextBox.Text,
            Template = TemplateTextBox.Text,
            ConflictPolicy = ((ConflictPolicyOption)ConflictPolicyComboBox.SelectedItem).Value
        };

        private void SaveAsPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FolderTextBox.Text) || string.IsNullOrWhiteSpace(TemplateTextBox.Text))
            {
                MessageBox.Show(this, Localization.Get("Presets.NoFieldsToSave"), Localization.Get("Errors.Title"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pre-filled with the last preset the user loaded (or saved) this session, so
            // re-saving over the same preset is just "confirm" instead of retyping its name.
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                Localization.Get("Presets.SaveNamePrompt"), Localization.Get("Presets.SaveNameTitle"), _lastLoadedPresetName);
            if (string.IsNullOrWhiteSpace(name))
            {
                return; // cancelled
            }

            // Saving under the name of an already-loaded/existing preset edits it in place
            // (§1 "bewerken") instead of creating a duplicate entry — but confirm first, since
            // this silently overwrites whatever that preset held before.
            Preset existing = _presets.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                MessageBoxResult overwrite = MessageBox.Show(this, Localization.Get("Presets.ConfirmOverwrite", name),
                    Localization.Get("Errors.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (overwrite != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            Preset preset = existing ?? new Preset { Name = name };
            preset.Folder = FolderTextBox.Text.Trim();
            preset.ExtensionFilter = ExtensionsTextBox.Text;
            preset.Template = TemplateTextBox.Text;
            preset.LastUsedUtc = DateTime.UtcNow;

            if (existing == null)
            {
                _presets.Add(preset);
            }
            else
            {
                PresetsListBox.Items.Refresh(); // Preset isn't INotifyPropertyChanged — force the DisplayText update
            }
            _presetStore.SaveAll(_presets.ToList());
            _lastLoadedPresetName = preset.Name;
        }

        private async void PresetsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-clicking the Delete button itself would also bubble up as a ListBox
            // double-click — but by then the preset is already removed from _presets, so
            // SelectedItem is null and there's nothing to load.
            if (PresetsListBox.SelectedItem is Preset preset)
            {
                LoadPreset(preset);
                await RunPlanAsync(execute: false); // dry-run straight away so results show what the preset would do
            }
        }

        private void LoadPreset(Preset preset)
        {
            FolderTextBox.Text = preset.Folder;
            ExtensionsTextBox.Text = preset.ExtensionFilter;
            TemplateTextBox.Text = preset.Template;

            preset.LastUsedUtc = DateTime.UtcNow;
            _presetStore.SaveAll(_presets.ToList());
            _lastLoadedPresetName = preset.Name;
        }

        private void PresetDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var preset = (Preset)((Button)sender).DataContext;

            MessageBoxResult result = MessageBox.Show(this, Localization.Get("Presets.ConfirmDelete", preset.Name),
                Localization.Get("Errors.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _presets.Remove(preset);
            _presetStore.SaveAll(_presets.ToList());
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || !(LanguageComboBox.SelectedItem is string language))
            {
                return;
            }

            _settings.Language = language;
            _settingsService.Save(_settings);
            MessageBox.Show(this, Localization.Get("Settings.LanguageChangedRestartRequired"), Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || !(ThemeComboBox.SelectedItem is string theme))
            {
                return;
            }

            _settings.Theme = theme;
            _settingsService.Save(_settings);
            ThemeProvider.ApplySetting(theme);
        }

        /// <summary>Reads the informational version (e.g. "1.0.12") that release.ps1 stamps onto
        /// the assembly via -p:Version. Falls back to the plain assembly version for local/dev
        /// builds that were never packaged through the release script. (Same approach as
        /// FldrSrtr's MainWindow.GetAppVersion.)</summary>
        private static string GetAppVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string informational = assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion;

            return !string.IsNullOrWhiteSpace(informational) ? informational : assembly.GetName().Version.ToString();
        }

        /// <summary>ComboBox item wrapper: ConflictPolicy has no display text of its own (Core
        /// stays free of Localization), so pair each value with its already-localized label.</summary>
        private class ConflictPolicyOption
        {
            public ConflictPolicy Value { get; }
            private readonly string _display;

            public ConflictPolicyOption(ConflictPolicy value, string display)
            {
                Value = value;
                _display = display;
            }

            public override string ToString() => _display;
        }
    }
}
