using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using App.Core.Execution;
using App.Core.Model;
using App.Infrastructure.Configuration;

namespace FldrFltr
{
    public partial class MainWindow : Window
    {
        /// <summary>Every {Variable} from §2 of the projectbrief, in the same order as its table —
        /// used to build the "Variabele invoegen" dropdown.</summary>
        private static readonly string[] VariableTokens =
        {
            "{FileName}", "{OriginalName}", "{Extension}", "{OriginalExtension}",
            "{FullPath}", "{Directory}", "{FileSize}",
            "{Year}", "{Month}", "{Day}", "{Hour}", "{Minute}", "{Second}", "{Date}", "{Time}",
            "{CreatedYear}", "{CreatedMonth}", "{CreatedDay}", "{CreatedHour}", "{CreatedMinute}", "{CreatedSecond}",
            "{ModifiedYear}", "{ModifiedMonth}", "{ModifiedDay}", "{ModifiedHour}", "{ModifiedMinute}", "{ModifiedSecond}",
            "{Counter}", "{Guid}", "{Random}", "{RandomString}"
        };

        private readonly SettingsService _settingsService = new SettingsService();
        private readonly PresetStore _presetStore = new PresetStore();
        private readonly AppSettings _settings;
        private readonly ObservableCollection<Preset> _presets;
        private bool _isInitializing = true;

        public MainWindow()
        {
            InitializeComponent();

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

        private void InsertVariableButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.ContextMenu == null)
            {
                button.ContextMenu = BuildVariableMenu();
            }
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }

        private ContextMenu BuildVariableMenu()
        {
            var menu = new ContextMenu();
            foreach (string token in VariableTokens)
            {
                var item = new MenuItem { Header = token };
                item.Click += (_, __) => InsertAtCaret(token);
                menu.Items.Add(item);
            }
            return menu;
        }

        private void InsertAtCaret(string token)
        {
            int caret = TemplateTextBox.CaretIndex;
            TemplateTextBox.Text = TemplateTextBox.Text.Insert(caret, token);
            TemplateTextBox.CaretIndex = caret + token.Length;
            TemplateTextBox.Focus();
        }

        private void TestDryRunButton_Click(object sender, RoutedEventArgs e) => RunPlan(execute: false);

        private void RenameButton_Click(object sender, RoutedEventArgs e) => RunPlan(execute: true);

        private void RunPlan(bool execute)
        {
            try
            {
                RenameOptions options = BuildOptionsFromFields();
                var plan = RenameEngine.BuildPlan(options);
                if (execute)
                {
                    RenameEngine.Execute(plan);
                }

                ResultsDataGrid.ItemsSource = plan.Select(p => new RenameResultRow(p)).ToList();
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(this, Localization.Get("Errors.FolderMissing"), Localization.Get("Errors.Title"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

            string name = Microsoft.VisualBasic.Interaction.InputBox(
                Localization.Get("Presets.SaveNamePrompt"), Localization.Get("Presets.SaveNameTitle"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return; // cancelled
            }

            // Saving under the name of an already-loaded/existing preset edits it in place
            // (§1 "bewerken") instead of creating a duplicate entry.
            Preset existing = _presets.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
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
        }

        private void PresetLoadButton_Click(object sender, RoutedEventArgs e)
        {
            var preset = (Preset)((Button)sender).Tag;

            FolderTextBox.Text = preset.Folder;
            ExtensionsTextBox.Text = preset.ExtensionFilter;
            TemplateTextBox.Text = preset.Template;

            preset.LastUsedUtc = DateTime.UtcNow;
            _presetStore.SaveAll(_presets.ToList());
        }

        private void PresetDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var preset = (Preset)((Button)sender).Tag;

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
