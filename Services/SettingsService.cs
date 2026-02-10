using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using RainmeterLayoutManager.Models;

namespace RainmeterLayoutManager.Services
{
    public sealed class SettingsService
    {
        private static readonly Lazy<SettingsService> lazy = new(() => new SettingsService());
        public static SettingsService Instance => lazy.Value;

        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public AppConfig Config { get; private set; } = new AppConfig();
        private readonly string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly string folder;
        private readonly string configPath;

        private SettingsService()
        {
            // save settings in AppData
            folder = Path.Combine(appData, "RainmeterLayoutManager");
            Directory.CreateDirectory(folder);

            configPath = Path.Combine(folder, "settings.json");

            LoadConfig();
        }

        /// <summary>
        /// Loads the configuration from the .json file.
        /// </summary>
        private void LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                // file does not exist, create a new config
                Config = GetDefaultConfig();
                return;
            }

            try
            {
                var json = File.ReadAllText(configPath);
                Config = JsonSerializer.Deserialize<AppConfig>(json) ?? GetDefaultConfig();
            }
            catch
            {
                // If file is corrupted, start fresh
                Config = GetDefaultConfig();
            }
        }

        private static AppConfig GetDefaultConfig()
        {
            return new AppConfig
            {
                RainmeterLayoutsPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Rainmeter",
                    "Layouts"
                ),
                RainmeterSkinsPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Rainmeter",
                    "Skins"
                )
            };
        }

        /// <summary>
        /// Saves the configuration to the .json file.
        /// </summary>
        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(Config, jsonOptions);
            File.WriteAllText(configPath, json);
        }


        /// <summary>
        /// Returns the current configuration.
        /// </summary>
        public AppConfig GetConfig()
        {
            return Config;
        }

        /// <summary>
        /// Updates the configuration and saves it.
        /// </summary>
        public void UpdateConfig(AppConfig config)
        {
            Config = config;
            SaveConfig();
        }

        /// <summary>
        /// Removes the configuration for the given fingerprint.
        /// </summary>
        /// <param name="fingerprint">The fingerprint to remove.</param>
        public void RemoveFingerprintConfig(string fingerprint)
        {
            Config.Configurations.Remove(fingerprint);
            SaveConfig();
        }

        /// <summary>
        /// Updates or creates a FingerprintConfig of the given fingerprint with the given layout name.
        /// </summary>
        /// <param name="fingerprint">The fingerprint to save the layout for.</param>
        /// <param name="layoutName">The layout name to save.</param>
        public void SetLayoutForFingerprint(string fingerprint, string layoutName)
        {
            // Get or create the configuration for this fingerprint
            if (!Config.Configurations.TryGetValue(fingerprint, out FingerprintConfig? config))
            {
                config = new FingerprintConfig();
                Config.Configurations[fingerprint] = config;
            }

            config.LayoutName = layoutName;
            SaveConfig();
        }

        /// <summary>
        /// Returns the layout name for the given fingerprint, or null if no mapping exists.
        /// </summary>
        /// <param name="fingerprint">The fingerprint to look up.</param>
        /// <returns>The layout name, or null if no mapping exists.</returns>
        public string? GetLayoutForFingerprint(string fingerprint)
        {
            if (Config.Configurations.TryGetValue(fingerprint, out FingerprintConfig? config))
            {
                return config.LayoutName;
            }
            return null;
        }

        /// <summary>
        /// Sets the variable overrides for the given fingerprint.
        /// </summary>
        /// <param name="fingerprint">The fingerprint to set the variable overrides for.</param>
        /// <param name="variableOverrides">The variable overrides to set.</param>
        /// <exception cref="InvalidOperationException">Thrown if no configuration exists for the given fingerprint.</exception>
        public void SetVariableOverridesForFingerprint(string fingerprint, Dictionary<string, Dictionary<string, string>>? variableOverrides = null)
        {
            if (Config.Configurations.TryGetValue(fingerprint, out FingerprintConfig? config))
            {
                config.VariableOverrides = variableOverrides;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Cannot set variable overrides: No configuration exists for fingerprint '{fingerprint}'. Create a layout mapping first."
                );
            }

            SaveConfig();
        }

        /// <summary>
        /// Returns the variable overrides for the given fingerprint.
        /// </summary>
        /// <param name="fingerprint">The fingerprint to look up.</param>
        /// <returns>The variable overrides, or an empty dictionary if no mapping exists.</returns>
        public Dictionary<string, Dictionary<string, string>> GetVariableObverridesForFingerprint(string fingerprint)
        {
            if (Config.Configurations.TryGetValue(fingerprint, out FingerprintConfig? config))
            {
                return config.VariableOverrides ?? [];
            }
            return [];
        }

        /// <summary>
        /// Returns all "Fingerprint: LayoutName"-mappings.
        /// </summary>
        /// <returns>A dictionary of all "Fingerprint: LayoutName"-mappings.</returns>
        public Dictionary<string, string> GetFingerprintToLayoutMappings()
        {
            return Config.Configurations.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.LayoutName
            );
        }

        /// <summary>
        /// Generates a unique string ID for the current monitor setup.
        /// Format: "[Width]x[Height]@[XBound],[YBound]|...", e.g. "1920x1080@0,0|2560x1440@1920,0"
        /// </summary>
        public static string GetDisplayFingerprint()
        {
            // Get all screens and sort them by X position, then Y position
            // This ensures the fingerprint is consistent even if Windows re-indexes IDs
            var screens = Screen.AllScreens
                .OrderBy(s => s.Bounds.X)
                .ThenBy(s => s.Bounds.Y);

            // Create a string like: "1920x1080@0,0|2560x1440@1920,0"
            var fingerprint = string.Join("|", screens.Select(s =>
                $"{s.Bounds.Width}x{s.Bounds.Height}@{s.Bounds.X},{s.Bounds.Y}"));

            return fingerprint;
        }

        public string GetRainmeterLayoutsPath()
        {
            return Config.RainmeterLayoutsPath;
        }

        public void SetRainmeterLayoutsPath(string pathToLayoutsFolder)
        {
            Config.RainmeterLayoutsPath = pathToLayoutsFolder;
            SaveConfig();
        }

        public string GetRainmeterSkinsPath()
        {
            return Config.RainmeterSkinsPath;
        }

        public void SetRainmeterSkinsPath(string pathToSkinsFolder)
        {
            Config.RainmeterSkinsPath = pathToSkinsFolder;
            SaveConfig();
        }
    }
}
