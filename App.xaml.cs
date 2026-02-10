using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using RainmeterLayoutManager.Services;

namespace RainmeterLayoutManager
{
    public partial class App : Application
    {
        // service instances
        private readonly AutoSwitcherService autoSwitcherService = AutoSwitcherService.Instance;
        private readonly SettingsService settingsService = SettingsService.Instance;
        private readonly LayoutService layoutService = LayoutService.Instance;

        protected override void OnStartup(StartupEventArgs e)
        {
            Debug.WriteLine("App OnStartup fired");

            // AutoSwitcherService will immediately start monitoring the display setup
            // and apply the appropriate layout if needed (when display setup changes)
            // Will also immediately check the display setup once and apply the appropriate layout if needed
            autoSwitcherService.Start();

            var mainWindow = new MainWindow();

            // if started with --minimized, don't show the main window
            if (e.Args.Contains("--minimized"))
            {
                // when the main window is not shown, the application will exit immediately
                // so we need to prevent that
                // we do this by setting the ShutdownMode to OnExplicitShutdown
                // and then we can close the application when the user clicks the exit button in the tray icon menu
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            else
            {
                mainWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Debug.WriteLine("App OnExit fired");
            autoSwitcherService.Stop();
            base.OnExit(e);
        }
    }
}