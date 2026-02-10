using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RainmeterLayoutManager.Models;
using RainmeterLayoutManager.Services;

namespace RainmeterLayoutManager
{
    public partial class LayoutBindingDialog : Window
    {
        private readonly LayoutService layoutService = LayoutService.Instance;
        private readonly SettingsService settingsService = SettingsService.Instance;
        private readonly string fingerprint;
        private readonly bool isEditMode;

        private readonly ObservableCollection<SkinViewModel> skinViewModels = [];

        public string? SelectedLayoutName { get; private set; }

        // Constructor for adding a new binding
        public LayoutBindingDialog(string displayFingerprint)
        {
            InitializeComponent();
            fingerprint = displayFingerprint;
            isEditMode = false;

            HeaderText.Text = "Add Layout Binding";
            FingerprintDisplay.Text = fingerprint;

            LoadAvailableLayouts();

            // Hook up layout selection changed event
            LayoutComboBox.SelectionChanged += LayoutComboBox_SelectionChanged;
        }

        // Constructor for editing an existing binding
        public LayoutBindingDialog(string displayFingerprint, string currentLayoutName) : this(displayFingerprint)
        {
            isEditMode = true;
            HeaderText.Text = "Edit Layout Binding";

            // Pre-select the current layout
            LayoutComboBox.SelectedItem = currentLayoutName;
        }

        private void LoadAvailableLayouts()
        {
            var layouts = layoutService.GetRainmeterLayouts();
            LayoutComboBox.ItemsSource = layouts;
        }

        private void LoadSkinsForLayout(string layoutName)
        {
            SelectedLayoutDisplay.Text = layoutName;
            SelectedLayoutDisplay.FontStyle = FontStyles.Normal;

            skinViewModels.Clear();

            // Get skins from the layout
            var skinsDict = layoutService.GetSkinsFromLayout(layoutName);

            // Load existing overrides if any
            var config = settingsService.GetConfig();
            Dictionary<string, Dictionary<string, string>>? existingOverrides = null;
            if (config.Configurations.TryGetValue(fingerprint, out var fingerprintConfig))
            {
                existingOverrides = fingerprintConfig.VariableOverrides;
            }

            foreach (var skinEntry in skinsDict)
            {
                string skinName = skinEntry.Key;
                string[] iniFiles = skinEntry.Value;

                if (iniFiles.Length == 0) continue;

                // Read variables from the first INI file for this skin
                var variables = layoutService.GetSkinVariables(iniFiles[0]);

                if (variables.Count == 0) continue;

                var skinViewModel = new SkinViewModel { SkinName = skinName };

                foreach (var variable in variables)
                {
                    // Get the default value for the variable (as defined in the INI file)
                    string defaultValue = variable.Value;
                    string variableValue = string.Empty;

                    // Check if there's an override for this variable
                    if (existingOverrides != null &&
                        existingOverrides.TryGetValue(skinName, out Dictionary<string, string>? overrideSkin) &&
                        overrideSkin.TryGetValue(variable.Key, out string? overrideValue))
                    {
                        variableValue = overrideValue;
                    }

                    skinViewModel.Variables.Add(new VariableViewModel
                    {
                        Key = variable.Key,
                        Value = variableValue,
                        DefaultValue = defaultValue
                    });
                }

                skinViewModels.Add(skinViewModel);
            }

            SkinsItemsControl.ItemsSource = skinViewModels;
        }

        private void SaveFingerprintConfiguration()
        {
            var config = settingsService.GetConfig();

            // Create the nested dictionary structure for this fingerprint
            var overrides = new Dictionary<string, Dictionary<string, string>>();

            foreach (var skinViewModel in skinViewModels)
            {
                var skinOverrides = new Dictionary<string, string>();

                foreach (var variable in skinViewModel.Variables)
                {
                    // Only save non-empty values (empty means "use default")
                    if (!string.IsNullOrWhiteSpace(variable.Value))
                    {
                        skinOverrides[variable.Key] = variable.Value;
                    }
                }

                if (skinOverrides.Count > 0)
                {
                    overrides[skinViewModel.SkinName] = skinOverrides;
                }
            }

            // Get or create the configuration for this fingerprint
            if (!config.Configurations.TryGetValue(fingerprint, out FingerprintConfig? value))
            {
                value = new FingerprintConfig();
                config.Configurations[fingerprint] = value;
            }

            value.VariableOverrides = overrides;

            // Save to file
            settingsService.UpdateConfig(config);
        }

        private void RefreshLayoutsButton_Click(object sender, RoutedEventArgs e)
        {
            string? currentSelection = LayoutComboBox.SelectedItem as string;
            LoadAvailableLayouts();

            // Try to restore the previous selection
            if (!string.IsNullOrEmpty(currentSelection))
            {
                LayoutComboBox.SelectedItem = currentSelection;
            }
        }

        private void LayoutComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LayoutComboBox.SelectedItem is string selectedLayout)
            {
                LoadSkinsForLayout(selectedLayout);
            }
            else
            {
                SelectedLayoutDisplay.Text = "(No layout selected)";
                skinViewModels.Clear();
                SkinsItemsControl.ItemsSource = null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LayoutComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a Rainmeter layout.",
                    "Selection Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            SelectedLayoutName = LayoutComboBox.SelectedItem.ToString();

            // Show warning, if fingerprint is already assigned to another layout (and not in edit mode)
            if (!isEditMode && settingsService.GetConfig().Configurations.ContainsKey(fingerprint))
            {
                MessageBoxResult result = MessageBox.Show(
                    "This fingerprint is already assigned to another layout. The previous assignment will be overwritten.",
                    "Warning",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                );

                // If user clicked Cancel, abort the save operation
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Save variable overrides
            SaveFingerprintConfiguration();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
