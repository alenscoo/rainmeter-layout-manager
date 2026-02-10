using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IniParser;
using IniParser.Model;

namespace RainmeterLayoutManager.Services
{
    public sealed class LayoutService
    {
        private static readonly Lazy<LayoutService> lazy = new(() => new LayoutService());
        public static LayoutService Instance => lazy.Value;


        private readonly string RainmeterLayoutsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rainmeter",
            "Layouts"
        );
        private readonly string RainmeterSkinsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Rainmeter",
            "Skins"
        );

        private LayoutService() { }

        /// <summary>
        /// Gets a list of available Rainmeter layouts by reading the directory names.
        /// </summary>
        /// <returns>A list of layout names.</returns>
        public List<string> GetRainmeterLayouts()
        {
            if (Directory.Exists(RainmeterLayoutsPath))
            {
                return [.. Directory.GetDirectories(RainmeterLayoutsPath)
                    .Select(System.IO.Path.GetFileName)
                    .OfType<string>()];
            }

            // return an empty list, if the directory doesnt't exist
            return [];
        }

        /// <summary>
        /// Gets a dictionary of skin names (active skins) and their file paths for a specific layout.
        /// </summary>
        /// <param name="layoutName">The name of the layout to get skins from.</param>
        /// <returns>A dictionary of skin names (active skins) and their (.ini) file paths (also from all subdirectories).</returns>
        public Dictionary<string, string[]> GetSkinsFromLayout(string layoutName)
        {
            string iniPath = System.IO.Path.Combine(RainmeterLayoutsPath, layoutName, "Rainmeter.ini");

            if (!File.Exists(iniPath))
            {
                return [];
            }

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(iniPath);

            string skinPath = data.Sections["Rainmeter"]["SkinPath"];

            // Get all sections except "Rainmeter"
            var allSkinSections = data.Sections
                .Select(section => section.SectionName)
                .Where(name => name != "Rainmeter")
                .ToList();

            var activeSkins = allSkinSections
                .Where(section => data.Sections[section].ContainsKey("Active") && data.Sections[section]["Active"] == "1")
                .ToList();

            var filesInDir = activeSkins
                .Select(section => new
                {
                    section,
                    path = System.IO.Path.Combine(skinPath, section)
                })
                .ToDictionary(
                    x => x.section,
                    x => Directory.Exists(x.path)
                        ? Directory.GetFiles(x.path, "*.ini", SearchOption.AllDirectories)
                        : []
                );

            return filesInDir;
        }

        /// <summary>
        /// Tells Rainmeter to load a specific layout.
        /// </summary>
        public static void LoadLayout(string layoutName)
        {
            RainmeterCLI.ApplicationBangs.LoadLayout(layoutName);
        }

        /// <summary>
        /// Sets a variable in a specific INI file and refreshes the config.
        /// Equivalent to your Python set_scale method.
        /// </summary>
        public static void SetScale(string configName, string iniFilePath, double scaleValue)
        {
            // 1. Write the variable
            RainmeterCLI.OptionAndVariableBangs.WriteKeyValue(iniFilePath, "Variables", "Scale", scaleValue.ToString());

            // 2. Refresh the skin to apply changes
            RainmeterCLI.SkinBangs.Refresh(configName);
        }

        /// <summary>
        /// Gets all variables from a skin's INI file.
        /// </summary>
        /// <param name="skinIniPath">Path to the skin's INI file.</param>
        /// <param name="filterIncludes">Whether to filter out @include directives.</param>
        /// <returns>Dictionary of variable names and their values.</returns>
        public Dictionary<string, string> GetSkinVariables(string skinIniPath, bool filterIncludes = true)
        {
            if (!File.Exists(skinIniPath))
            {
                return [];
            }

            try
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(skinIniPath);

                if (!data.Sections.ContainsSection("Variables"))
                {
                    return [];
                }
                var allVariables = data.Sections["Variables"]
                    .ToDictionary(keyData => keyData.KeyName, keyData => keyData.Value);

                var filteredVariables = allVariables
                    .Where(variable => !variable.Key.StartsWith("@include", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(variable => variable.Key, variable => variable.Value);

                return filterIncludes ? filteredVariables : allVariables;
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Sets a variable in a skin's INI file and refreshes the skin.
        /// </summary>
        /// <param name="skinIniPath">Path to the skin's INI file.</param>
        /// <param name="configName">Name of the Rainmeter config (for refresh).</param>
        /// <param name="variableName">Name of the variable to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetSkinVariable(string skinIniPath, string configName, string variableName, string value)
        {
            RainmeterCLI.OptionAndVariableBangs.WriteKeyValue(skinIniPath, "Variables", variableName, value);
            RainmeterCLI.SkinBangs.Refresh(configName);
        }

        /// <summary>
        /// Applies variable overrides to all skins in a layout.
        /// </summary>
        /// <param name="layoutName">Name of the layout.</param>
        /// <param name="overrides">Dictionary of skinName -> variableName -> value.</param>
        public void ApplyVariableOverrides(string layoutName, Dictionary<string, Dictionary<string, string>> overrides)
        {
            if (overrides == null || overrides.Count == 0)
            {
                return;
            }

            var skins = GetSkinsFromLayout(layoutName);

            foreach (var skinEntry in overrides)
            {
                string skinName = skinEntry.Key;
                var variables = skinEntry.Value;

                // Find the INI files for this skin
                if (skins.ContainsKey(skinName) && skins[skinName].Length > 0)
                {
                    // Apply variables to each INI file in this skin
                    foreach (var iniFile in skins[skinName])
                    {
                        foreach (var variable in variables)
                        {
                            try
                            {
                                SetSkinVariable(iniFile, skinName, variable.Key, variable.Value);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Failed to set variable {variable.Key} in {skinName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}
