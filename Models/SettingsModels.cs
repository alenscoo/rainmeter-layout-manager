using System.Collections.Generic;

namespace RainmeterLayoutManager.Models
{
    /// <summary>
    /// Configuration for a specific display fingerprint.
    /// </summary>
    public class FingerprintConfig
    {
        /// <summary>
        /// The Rainmeter layout name to load for this fingerprint.
        /// </summary>
        public string LayoutName { get; set; } = string.Empty;

        /// <summary>
        /// Variable overrides for skins in this layout.
        /// Structure: skinName -> variableName -> value
        /// </summary>
        public Dictionary<string, Dictionary<string, string>>? VariableOverrides { get; set; }
    }

    public class AppConfig
    {
        /// <summary>
        /// Stores configurations for each display fingerprint.
        /// Structure: fingerprint -> FingerprintConfig
        /// </summary>
        public Dictionary<string, FingerprintConfig> Configurations { get; set; } = [];

        /// <summary>
        /// Indicates whether the application should start with Windows.
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        public string RainmeterLayoutsPath { get; set; } = string.Empty;

        public string RainmeterSkinsPath { get; set; } = string.Empty;
    }
}