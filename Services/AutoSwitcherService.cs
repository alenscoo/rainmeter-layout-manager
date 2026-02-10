using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Timers;

namespace RainmeterLayoutManager.Services
{
    public class AutoSwitcherService
    {
        private static readonly Lazy<AutoSwitcherService> lazy = new(() => new AutoSwitcherService());
        public static AutoSwitcherService Instance => lazy.Value;

        // service instances
        private readonly LayoutService layoutService = LayoutService.Instance;
        private readonly SettingsService settingsService = SettingsService.Instance;

        private readonly Timer debounceTimer;
        private string lastLoadedLayout = string.Empty;
        public event Action<string>? FingerprintDetected;

        private AutoSwitcherService()
        {
            // Timer to prevent spamming switches when display flickers during setup
            debounceTimer = new Timer(2000); // 2 seconds delay
            debounceTimer.Elapsed += OnDebounceTimerElapsed;
            debounceTimer.AutoReset = false;
        }

        public void Start()
        {
            Debug.WriteLine("AutoSwitcher started");
            // Subscribe to Windows Display Events
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

            // Check immediately on start
            CheckAndSwitch();
        }

        public void Stop()
        {
            Debug.WriteLine("AutoSwitcher stopped");
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            debounceTimer.Stop();
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine("Display Settings changed");
            // Reset the timer whenever an event fires. 
            // The code will only run 2 seconds AFTER the LAST event.
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private void OnDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            CheckAndSwitch();
        }

        private void CheckAndSwitch()
        {
            Debug.WriteLine("CheckAndSwitch started");
            try
            {
                string currentFingerprint = SettingsService.GetDisplayFingerprint();
                string? targetLayout = settingsService.GetLayoutForFingerprint(currentFingerprint);
                FingerprintDetected?.Invoke(currentFingerprint);

                Debug.WriteLine($"Fingerprint: {currentFingerprint}");
                Debug.WriteLine($"TargetLayout: {targetLayout}");
                Debug.WriteLine($"LastLayout: {lastLoadedLayout}");

                // Only switch if we found a layout AND it's different from what we last loaded
                if (!string.IsNullOrEmpty(targetLayout) && targetLayout != lastLoadedLayout)
                {
                    LayoutService.LoadLayout(targetLayout);
                    lastLoadedLayout = targetLayout;

                    // Apply variable overrides if any exist for this fingerprint
                    var config = settingsService.GetConfig();
                    if (config.Configurations.TryGetValue(currentFingerprint, out var fingerprintConfig) &&
                        fingerprintConfig.VariableOverrides != null)
                    {
                        layoutService.ApplyVariableOverrides(targetLayout, fingerprintConfig.VariableOverrides);
                        Debug.WriteLine($"Applied variable overrides for {fingerprintConfig.VariableOverrides.Count} skin(s)");
                    }

                    // Optional: Log this or show a toast notification
                    Debug.WriteLine($"Switched to layout: {targetLayout}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AutoSwitcher: {ex.Message}");
            }
        }
    }
}

