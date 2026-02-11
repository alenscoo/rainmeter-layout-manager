using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RainmeterLayoutManager.Services;


namespace RainmeterLayoutManager
{
    public partial class MainWindow : Window
    {
        // service instances
        private readonly LayoutService layoutService = LayoutService.Instance;
        private readonly SettingsService settingsService = SettingsService.Instance;
        private readonly AutoSwitcherService autoSwitcherService = AutoSwitcherService.Instance;

        public ObservableCollection<LayoutMappingViewModel> LayoutMappings { get; set; }

        public static int MaxLayoutMappings => 1000;

        private System.Windows.Forms.NotifyIcon? notifyIcon;
        private bool isExplicitExit = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemTray();

            DataContext = this;
            StartWithWindowsCheckBox.IsChecked = settingsService.Config.StartWithWindows;
            LayoutMappings = [];

            // subscribe to fingerprint detected event
            autoSwitcherService.FingerprintDetected += AutoSwitcher_FingerprintDetected;

            // update fingerprint display
            UpdateFingerprintDisplay();

            // load mappings
            LoadMappings();
        }

        private static void SetStartup(bool isEnabled)
        {
            const string AppName = "RainmeterLayoutManager";
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return; // Registry key not accessible, exit gracefully

            if (isEnabled)
            {
                key.SetValue(AppName, $"\"{appPath}\" --minimized");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }

            key.Dispose();
        }



        private void LoadMappings()
        {
            LayoutMappings.Clear();
            var mappings = settingsService.GetFingerprintToLayoutMappings();

            // Sort by fingerprint (dictionary key) before adding to collection
            foreach (var kvp in mappings.OrderBy(m => m.Key))
            {
                LayoutMappings.Add(new LayoutMappingViewModel(kvp.Key, kvp.Value));
            }
        }

        private void UpdateFingerprintDisplay()
        {
            string fingerprint = SettingsService.GetDisplayFingerprint();
            FingerprintText.Text = fingerprint;
        }


        private static System.Drawing.Icon GetApplicationIcon()
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                // Extract icon from the executable
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                return icon ?? System.Drawing.SystemIcons.Application;
            }
            catch
            {
                // Fallback to system icon if extraction fails
                return System.Drawing.SystemIcons.Application;
            }
        }

        private void InitializeSystemTray()
        {
            // assign new instance if notifyIcon is null
            notifyIcon ??= new System.Windows.Forms.NotifyIcon
            {
                Icon = GetApplicationIcon(),
                Visible = true,
                Text = "Rainmeter Layout Manager"
            };

            notifyIcon.DoubleClick += (s, args) => ShowWindow();

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Open", null, (s, args) => ShowWindow());
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, (s, args) =>
            {
                isExplicitExit = true;
                Close();
            });
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (isExplicitExit)
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
                base.OnClosing(e);
            }
            else
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void StartWithWindowsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = StartWithWindowsCheckBox.IsChecked ?? false;
            settingsService.Config.StartWithWindows = isChecked;
            SetStartup(isChecked);
        }

        private void AutoSwitcher_FingerprintDetected(string fingerprint)
        {
            Dispatcher.Invoke(() =>
            {
                FingerprintText.Text = fingerprint;
            });
        }

        private void RefreshFingerprint_Click(object sender, RoutedEventArgs e)
        {
            autoSwitcherService.CheckAndSwitch();
        }

        private void AddLayoutBinding_Click(object sender, RoutedEventArgs e)
        {
            if (LayoutMappings.Count >= MaxLayoutMappings)
            {
                MessageBox.Show(
                    $"Maximum number of mappings reached ({MaxLayoutMappings}). Please remove some mappings before adding a new one.",
                    "Maximum Mappings Reached",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            string fingerprint = SettingsService.GetDisplayFingerprint();
            var dialog = new LayoutBindingDialog(fingerprint)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedLayoutName))
            {
                settingsService.SetLayoutForFingerprint(fingerprint, dialog.SelectedLayoutName);

                // Show non-intrusive toast notification via system tray
                notifyIcon?.ShowBalloonTip(
                    10000,
                    "Mapping Saved",
                    $"Layout '{dialog.SelectedLayoutName}' mapped to current display setup",
                    System.Windows.Forms.ToolTipIcon.Info
                );

                // Refresh the grid
                LoadMappings();
            }
        }

        private void MappingsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                TextBox? editedElement = e.EditingElement as TextBox;
                string newLayoutName = editedElement?.Text ?? "";

                if (e.Row.Item is LayoutMappingViewModel viewModel && viewModel.LayoutName != newLayoutName)
                {
                    // Update the underlying data and save
                    viewModel.LayoutName = newLayoutName;
                    settingsService.SetLayoutForFingerprint(viewModel.Fingerprint, newLayoutName);

                    MessageBox.Show($"Layout for fingerprint '{viewModel.Fingerprint[..15]}...' updated to '{newLayoutName}'.",
                                    "Mapping Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EditMapping_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.DataContext is LayoutMappingViewModel viewModel)
            {
                var dialog = new LayoutBindingDialog(viewModel.Fingerprint, viewModel.LayoutName)
                {
                    Owner = this
                };

                if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedLayoutName))
                {
                    // Update the mapping
                    viewModel.LayoutName = dialog.SelectedLayoutName;
                    settingsService.SetLayoutForFingerprint(viewModel.Fingerprint, dialog.SelectedLayoutName);

                    // Show notification
                    notifyIcon?.ShowBalloonTip(
                        5000,
                        "Mapping Updated",
                        $"Layout for display config updated to '{dialog.SelectedLayoutName}'",
                        System.Windows.Forms.ToolTipIcon.Info
                    );
                }
            }
        }

        private void RemoveMapping_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.DataContext is LayoutMappingViewModel viewModel)
            {
                var result = MessageBox.Show($"Are you sure you want to remove the mapping for fingerprint '{viewModel.Fingerprint}'?",
                                             "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    settingsService.RemoveFingerprintConfig(viewModel.Fingerprint);
                    LayoutMappings.Remove(viewModel);
                }
            }
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Clear DataGrid selection when clicking outside of it
            MappingsGrid.UnselectAll();
        }
    }

    public class LayoutMappingViewModel(string fingerprint, string layoutName) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string layoutName = layoutName;

        public string Fingerprint { get; set; } = fingerprint;
        public string LayoutName
        {
            get => layoutName;
            set
            {
                if (layoutName != value)
                {
                    layoutName = value;
                    OnPropertyChanged(nameof(LayoutName));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
