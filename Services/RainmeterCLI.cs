using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RainmeterLayoutManager.Services
{
    internal static class RainmeterCLI
    {
        private const string RainmeterPath = @"C:\Program Files\Rainmeter\Rainmeter.exe";

        private static void RunRainmeterBang(string args)
        {
            if (File.Exists(RainmeterPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = RainmeterPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
            }
            else
            {
                // In a real app, you might want to show a MessageBox here
                Debug.WriteLine("Rainmeter executable not found.");
            }
        }

        internal static class OperationSystemBangs
        {
            internal enum Position
            {
                Center,
                Tile,
                Stretch,

                // Win 7 specific
                Fit,
                Fill,

                // Win 10 specific
                Span
            }

            /// <summary>
            /// Copies the specified string to the Windows clipboard.
            ///
            /// Example: !SetClip "This is copied to the clipboard!"
            /// </summary>
            /// 
            /// <param name="text">
            /// (required) The string to be copied to the clipboard.
            /// </param>
            public static void SetClip(string text)
            {
                RunRainmeterBang($"!SetClip \"{text}\"");
            }

            /// <summary>
            /// Sets the Windows desktop background to the specified image file.
            /// 
            /// Example: !SetWallpaper "Some Image.png" Center
            /// </summary>
            /// 
            /// <param name="file">
            /// (required) The file to be set as the desktop background.
            /// The following formats are supported: .bmp, .jpg, .png, .gif, and .tiff.
            /// If the filename contains spaces, you must enclose the filename in quotes.
            /// </param>
            /// 
            /// <param name="position">
            /// (optional) Sets the position of the wallpaper.
            /// Valid values are: 'Center', 'Tile', 'Stretch'.
            /// Windows 7 (and above) also have the values: 'Fit' and 'Fill'.
            /// Windows 10 also has the value: 'Span'.
            /// </param>
            public static void SetWallpaper(string file, Position? position = null)
            {
                var command = new StringBuilder($"!SetWallpaper \"{file}\"");

                if (position is not null)
                {
                    command.Append($" {position}");
                }

                RunRainmeterBang(command.ToString());
            }
        }

        internal static class ApplicationBangs
        {
            internal enum TabNameAbout
            {
                Log,
                Skins,
                Plugins,
                Version
            }

            internal enum TabNameManage
            {
                Skins,
                Layouts,
                GameMode,
                Settings
            }

            internal enum ErrorType
            {
                Notice,
                Error,
                Warning,
                Debug
            }

            /// <summary>
            /// Opens the About window.
            /// 
            /// Example: !About Skins
            /// </summary>
            /// 
            /// <param name="tabName">
            /// (optional) Name of the tab to open. Valid values are: Log (default), Skins, Plugins, and Version.
            /// </param>
            public static void About(TabNameAbout tabName = TabNameAbout.Log)
            {
                RunRainmeterBang($"!About {tabName}");
            }


            /// <summary>
            /// Opens the Manage window.
            /// 
            /// Example: !Manage Skins "illustro\Clock" "Clock.ini"
            /// </summary>
            /// 
            /// <param name="config">
            /// (optional) Config name. If specified the list on the left will jump to and select the named config.
            /// If 'config' is specified, then 'tabName' is required (will discard the value otherwise).
            /// </param>
            /// 
            /// <param name="tabName">
            /// (optional) Name of the tab to open. Valid values are: Skins (default), Layouts, GameMode, and Settings.
            /// </param>
            /// 
            /// <param name="file">
            /// (optional) A skin .ini file in a named config.
            /// If specified the list on the left will jump to and select the named skin .ini file.
            /// If 'file' is specified, then 'tabName and 'config' are required (will discard the value otherwise).
            /// </param>
            public static void Manage(string config, TabNameManage tabName = TabNameManage.Skins, string? file = null)
            {
                var command = new StringBuilder($"!Manage {tabName}");

                if (!string.IsNullOrEmpty(config))
                {
                    command.Append($" \"{config}\"");

                    if (!string.IsNullOrEmpty(file))
                    {
                        command.Append($" \"{file}\"");
                    }
                }

                RunRainmeterBang(command.ToString());
            }

            /// <summary>
            /// Opens the Rainmeter context menu.
            /// 
            /// Example: !TrayMenu
            /// </summary>
            public static void TrayMenu()
            {
                RunRainmeterBang("!TrayMenu");
            }

            /// <summary>
            /// Writes a string to the Rainmeter log.
            /// 
            /// Example: !Log "There was an error!" Error
            /// </summary>
            /// 
            /// <param name="text">
            /// (required) The string to be written to the log.
            /// If the string contains spaces, it will be automatically enclosed in quotes.
            /// </param>
            /// 
            /// <param name="errorType">
            /// (optional) Specifies the type of error. Valid values are: Notice (default), Error, Warning, and Debug.
            /// </param>
            public static void Log(string text, ErrorType errorType = ErrorType.Notice)
            {
                RunRainmeterBang($"!Log \"{text}\" {errorType}");
            }

            /// <summary>
            /// Resets network and other statistics.
            /// 
            /// Example: !ResetStats
            /// </summary>
            public static void ResetStats()
            {
                RunRainmeterBang("!ResetStats");
            }

            /// <summary>
            /// Loads a saved Rainmeter layout.
            /// 
            /// Example: !LoadLayout "My Saved Layout"
            /// </summary>
            /// 
            /// <param name="layoutName">
            /// (required) Name of the layout to load.
            /// </param>
            public static void LoadLayout(string layoutName)
            {
                RunRainmeterBang($"!LoadLayout \"{layoutName}\"");
            }

            /// <summary>
            /// Does a full refresh of all skins and reloads the list of configs and Rainmeter.ini settings.
            /// This is the same as "Refresh All" from the system tray context menu.
            /// The main difference from !Refresh * is that the skins folder is rescanned.
            /// Example: !RefreshApp
            /// </summary>
            public static void RefreshApp()
            {
                RunRainmeterBang("!RefreshApp");
            }

            /// <summary>
            /// Quits Rainmeter completely.
            /// 
            /// Note: This becomes the only bang that can be executed from the command line
            /// when 'Game Mode' and 'Unload all skins' is turned on.
            /// 
            /// Example: !Quit
            /// </summary>
            public static void Quit()
            {
                RunRainmeterBang("!Quit");
            }

            /// <summary>
            /// Plays a .wav sound file.
            /// 
            /// While listed in this section for simplicity, the Play commands are not bangs
            /// and do not require or allow the ! character at the beginning.
            /// Rainmeter uses a single-threaded call to embedded Windows functionality to play the sound,
            /// and only one sound at a time can be managed, Rainmeter-wide.
            /// Playing a sound in any skin will stop any other currently playing sounds.
            /// 
            /// Example: !Play "SomeFile.wav"
            /// </summary>
            /// 
            /// <param name="soundFile">
            /// (required) Path and file to be played. Must be a .wav file.
            /// </param>
            public static void Play(string soundFile)
            {
                RunRainmeterBang($"Play \"{soundFile}\"");
            }

            /// <summary>
            /// Plays a .wav sound file in a loop.
            /// 
            /// Example: !PlayLoop "SomeFile.wav"
            /// </summary>
            /// 
            /// <param name="soundFile">
            /// (required) File to be played. Must be a .wav file.
            /// </param>
            public static void PlayLoop(string soundFile)
            {
                RunRainmeterBang($"PlayLoop \"{soundFile}\"");
            }

            /// <summary>
            /// Stops playing sound.
            /// 
            /// Example: PlayStop
            /// </summary>
            public static void PlayStop()
            {
                RunRainmeterBang("PlayStop");
            }
        }

        internal static class OptionAndVariableBangs
        {
            /// <summary>
            /// Sets an option for a meter or measure.
            /// 
            /// Example: !SetOption SomeStringMeter Text "New Text"
            /// </summary>
            /// 
            /// <param name="config">
            /// (optional) Config name.
            /// </param>
            /// 
            /// <param name="meterOrMeasure">
            /// (required) Name of the meter or measure section.
            /// </param>
            /// 
            /// <param name="option">
            /// (required) Name of the option to be changed.
            /// </param>
            /// 
            /// <param name="value">
            /// (required) New value to be set.
            /// </param>
            public static void SetOption(string config, string meterOrMeasure, string option, string value)
            {
                RunRainmeterBang($"!SetOption {meterOrMeasure} {option} \"{value}\" \"{config}\"");
            }

            /// <summary>
            /// Sets a variable value.
            /// 
            /// Example: !SetVariable SomeVariable "New value!"
            /// </summary>
            /// 
            /// <param name="config">
            /// (required) Config name.
            /// </param>
            /// 
            /// <param name="variable">
            /// (required) Name of the variable.
            /// </param>
            /// 
            /// <param name="value">
            /// (required) New value to be set.
            /// </param>
            public static void SetVariable(string config, string variable, string value)
            {
                RunRainmeterBang($"!SetVariable {variable} \"{value}\" \"{config}\"");
            }

            /// <summary>
            /// Writes a key=value pair to an ini file.
            /// 
            /// Example: !WriteKeyValue Variables MyFontName Arial "#@#Variables.inc"
            /// </summary>
            /// 
            /// <param name="filePath">
            /// (required) The file must exist and must be located under either #SKINSPATH# or #SETTINGSPATH#.
            /// </param>
            /// 
            /// <param name="section">
            /// (required) If the section does not exist in the file, a new section will be written at the end of the file.
            /// </param>
            /// 
            /// <param name="key">
            /// (required) If the key does not exist under the section, a new key will be written at the end of the section.
            /// </param>
            /// 
            /// <param name="value">
            /// (required) Value to be written. Any previous value will be overwritten.
            /// </param>
            public static void WriteKeyValue(string filePath, string section, string key, string value)
            {
                RunRainmeterBang($"!WriteKeyValue {section} {key} \"{value}\" \"{filePath}\"");
            }

            /// <summary>
            /// Sets an option for all meters/measures in a group.
            /// 
            /// Example: !SetOptionGroup StringGroup Text "New text!"
            /// </summary>
            /// 
            /// <param name="config">
            /// (required) Config name.
            /// </param>
            /// 
            /// <param name="group">
            /// (required) Name of the group.
            /// </param>
            /// 
            /// <param name="option">
            /// (required) Name of the option to be changed.
            /// </param>
            /// 
            /// <param name="value">
            /// (required) New value to be set.
            /// </param>
            public static void SetOptionGroup(string config, string group, string option, string value)
            {
                RunRainmeterBang($"!SetOptionGroup {group} {option} \"{value}\" \"{config}\"");
            }

            /// <summary>
            /// Sets a variable for all configs in a group.
            /// 
            /// Example: !SetVariableGroup MyFontName "Arial" ConfigGroup
            /// </summary>
            /// 
            /// <param name="variable">
            /// (required) Name of the variable.
            /// </param>
            /// 
            /// <param name="value">
            /// (required) New value to be set.
            /// </param>
            /// 
            /// <param name="group">
            /// (required) Name of the group.
            /// </param>
            public static void SetVariableGroup(string variable, string value, string group)
            {
                RunRainmeterBang($"!SetVariableGroup {variable} \"{value}\" {group}");
            }
        }

        internal static class SkinBangs
        {
            /// <summary>Shows a skin. Example: !Show "illustro\Clock"</summary>
            /// <param name="config">(required) Config to show.</param>
            public static void Show(string config)
            {
                RunRainmeterBang($"!Show \"{config}\"");
            }

            /// <summary>Hides a skin. Example: !Hide "illustro\Clock"</summary>
            /// <param name="config">(required) Config to hide.</param>
            public static void Hide(string config)
            {
                RunRainmeterBang($"!Hide \"{config}\"");
            }

            /// <summary>Toggles a skin between shown and hidden. Example: !Toggle "illustro\Clock"</summary>
            /// <param name="config">(required) Config to toggle.</param>
            public static void Toggle(string config)
            {
                RunRainmeterBang($"!Toggle \"{config}\"");
            }

            /// <summary>Shows a skin with fade effect.</summary>
            /// <param name="config">(required) Config to show.</param>
            public static void ShowFade(string config)
            {
                RunRainmeterBang($"!ShowFade \"{config}\"");
            }

            /// <summary>Hides a skin with fade effect.</summary>
            /// <param name="config">(required) Config to hide.</param>
            public static void HideFade(string config)
            {
                RunRainmeterBang($"!HideFade \"{config}\"");
            }

            /// <summary>Toggles a skin between shown and hidden with fade effect.</summary>
            /// <param name="config">(required) Config to toggle.</param>
            public static void ToggleFade(string config)
            {
                RunRainmeterBang($"!ToggleFade \"{config}\"");
            }

            /// <summary>Sets fade duration in milliseconds.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="milliseconds">(required) Number of milliseconds for fade transition.</param>
            public static void FadeDuration(string config, int milliseconds)
            {
                RunRainmeterBang($"!FadeDuration {milliseconds} \"{config}\"");
            }

            /// <summary>Shows blur effect on a skin.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void ShowBlur(string config)
            {
                RunRainmeterBang($"!ShowBlur \"{config}\"");
            }

            /// <summary>Hides blur effect on a skin.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void HideBlur(string config)
            {
                RunRainmeterBang($"!HideBlur \"{config}\"");
            }

            /// <summary>Toggles blur effect on a skin.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void ToggleBlur(string config)
            {
                RunRainmeterBang($"!ToggleBlur \"{config}\"");
            }

            /// <summary>Adds blur effect to a region.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="region">(required) Region to add blur effect.</param>
            public static void AddBlur(string config, string region)
            {
                RunRainmeterBang($"!AddBlur \"{region}\" \"{config}\"");
            }

            /// <summary>Removes blur effect from a region.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="region">(required) Region to remove blur effect.</param>
            public static void RemoveBlur(string config, string region)
            {
                RunRainmeterBang($"!RemoveBlur \"{region}\" \"{config}\"");
            }

            /// <summary>Moves a skin to a new position. Example: !Move "100" "100"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="x">(required) New X position.</param>
            /// <param name="y">(required) New Y position.</param>
            public static void Move(string config, string x, string y)
            {
                RunRainmeterBang($"!Move \"{x}\" \"{y}\" \"{config}\"");
            }

            /// <summary>Sets window position and optionally anchor. Example: !SetWindowPosition "100" "100" "10" "50"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="windowX">(required) New WindowX position.</param>
            /// <param name="windowY">(required) New WindowY position.</param>
            /// <param name="anchorX">(optional) New AnchorX position. If specified, anchorY is required. Otherwise, none of them are used.</param>
            /// <param name="anchorY">(optional) New AnchorY position. Required if anchorX is specified. Otherwise, none of them are used.</param>
            public static void SetWindowPosition(string config, string windowX, string windowY, string? anchorX = null, string? anchorY = null)
            {
                var command = new StringBuilder($"!SetWindowPosition \"{windowX}\" \"{windowY}\"");
                if (!string.IsNullOrEmpty(anchorX) && !string.IsNullOrEmpty(anchorY))
                {
                    command.Append($" \"{anchorX}\" \"{anchorY}\"");
                }
                command.Append($" \"{config}\"");
                RunRainmeterBang(command.ToString());
            }

            /// <summary>Sets anchor position. Example: !SetAnchor "100" "100"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="anchorX">(required) New AnchorX position.</param>
            /// <param name="anchorY">(required) New AnchorY position.</param>
            public static void SetAnchor(string config, string anchorX, string anchorY)
            {
                RunRainmeterBang($"!SetAnchor \"{anchorX}\" \"{anchorY}\" \"{config}\"");
            }

            /// <summary>Activates a config. Example: !ActivateConfig "illustro\Clock" "Clock.ini"</summary>
            /// <param name="config">(required) The config to be activated.</param>
            /// <param name="file">(optional) The .ini file to be activated.</param>
            public static void ActivateConfig(string config, string? file = null)
            {
                var command = new StringBuilder($"!ActivateConfig \"{config}\"");
                if (!string.IsNullOrEmpty(file)) command.Append($" \"{file}\"");
                RunRainmeterBang(command.ToString());
            }

            /// <summary>Deactivates a config. Example: !DeactivateConfig "illustro\Clock"</summary>
            /// <param name="config">(required) The config to be deactivated.</param>
            public static void DeactivateConfig(string config)
            {
                RunRainmeterBang($"!DeactivateConfig \"{config}\"");
            }

            /// <summary>Toggles a config. Example: !ToggleConfig "illustro\Clock" "Clock.ini"</summary>
            /// <param name="config">(required) The config to be activated or deactivated.</param>
            /// <param name="file">(optional) The .ini file to be activated or deactivated.</param>
            public static void ToggleConfig(string config, string? file = null)
            {
                var command = new StringBuilder($"!ToggleConfig \"{config}\"");
                if (!string.IsNullOrEmpty(file)) command.Append($" \"{file}\"");
                RunRainmeterBang(command.ToString());
            }

            /// <summary>
            /// Overrides the setting of the Update option in [Rainmeter], immediately updates all measures and meters, and redraws the skin.
            /// This does not override any UpdateDivider options on measures or meters.
            /// </summary>
            /// <param name="config">(required) Config name.</param>
            public static void Update(string config)
            {
                RunRainmeterBang($"!Update \"{config}\"");
            }

            /// <summary>Immediately redraws all visible elements of the entire skin, independent of the normal skin update cycle.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void Redraw(string config)
            {
                RunRainmeterBang($"!Redraw \"{config}\"");
            }

            /// <summary>Refreshes a skin file and recreates the skin.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void Refresh(string config)
            {
                RunRainmeterBang($"!Refresh \"{config}\"");
            }

            /// <summary>Opens the skin context menu.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void SkinMenu(string config)
            {
                RunRainmeterBang($"!SkinMenu \"{config}\"");
            }

            /// <summary>Opens the custom skin context menu.</summary>
            /// <param name="config">(required) Config name.</param>
            public static void SkinCustomMenu(string config)
            {
                RunRainmeterBang($"!SkinCustomMenu \"{config}\"");
            }

            /// <summary>Sets transparency. Example: !SetTransparency "128" "illustro\Clock"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="alpha">(required) From 0 (invisible) to 255 (opaque).</param>
            public static void SetTransparency(string config, int alpha)
            {
                RunRainmeterBang($"!SetTransparency \"{alpha}\" \"{config}\"");
            }

            /// <summary>Sets Z-position. Example: !ZPos "2" "illustro\Clock"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="position">(required) -2 for On desktop, -1 for Bottom, 0 for Normal, 1 for On top, or 2 for Always on top.</param>
            public static void ZPos(string config, int position)
            {
                RunRainmeterBang($"!ZPos \"{position}\" \"{config}\"");
            }

            /// <summary>Sets draggable state.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void Draggable(string config, int setting)
            {
                RunRainmeterBang($"!Draggable {setting} \"{config}\"");
            }

            /// <summary>Sets keep on screen state.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void KeepOnScreen(string config, int setting)
            {
                RunRainmeterBang($"!KeepOnScreen {setting} \"{config}\"");
            }

            /// <summary>Sets click through state.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void ClickThrough(string config, int setting)
            {
                RunRainmeterBang($"!ClickThrough {setting} \"{config}\"");
            }

            /// <summary>Sets snap edges state.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void SnapEdges(string config, int setting)
            {
                RunRainmeterBang($"!SnapEdges {setting} \"{config}\"");
            }

            /// <summary>Sets auto select screen state.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void AutoSelectScreen(string config, int setting)
            {
                RunRainmeterBang($"!AutoSelectScreen {setting} \"{config}\"");
            }

            /// <summary>Opens a skin file in the editor. Example: !EditSkin "illustro\Clock" "Clock.ini"</summary>
            /// <param name="config">(required) The config containing the skin.</param>
            /// <param name="file">(optional) A specific .ini skin file to edit.</param>
            public static void EditSkin(string config, string? file = null)
            {
                var command = new StringBuilder($"!EditSkin \"{config}\"");
                if (!string.IsNullOrEmpty(file)) command.Append($" \"{file}\"");
                RunRainmeterBang(command.ToString());
            }

            // Skin Group Bangs

            /// <summary>Shows all skins in a group. Example: !ShowGroup "SomeGroup"</summary>
            /// <param name="group">(required) Group to show.</param>
            public static void ShowGroup(string group)
            {
                RunRainmeterBang($"!ShowGroup \"{group}\"");
            }

            /// <summary>Hides all skins in a group.</summary>
            /// <param name="group">(required) Group to hide.</param>
            public static void HideGroup(string group)
            {
                RunRainmeterBang($"!HideGroup \"{group}\"");
            }

            /// <summary>Toggles all skins in a group.</summary>
            /// <param name="group">(required) Group to toggle.</param>
            public static void ToggleGroup(string group)
            {
                RunRainmeterBang($"!ToggleGroup \"{group}\"");
            }

            /// <summary>Shows all skins in a group with fade effect.</summary>
            /// <param name="group">(required) Group to show.</param>
            public static void ShowFadeGroup(string group)
            {
                RunRainmeterBang($"!ShowFadeGroup \"{group}\"");
            }

            /// <summary>Hides all skins in a group with fade effect.</summary>
            /// <param name="group">(required) Group to hide.</param>
            public static void HideFadeGroup(string group)
            {
                RunRainmeterBang($"!HideFadeGroup \"{group}\"");
            }

            /// <summary>Toggles all skins in a group with fade effect.</summary>
            /// <param name="group">(required) Group to toggle.</param>
            public static void ToggleFadeGroup(string group)
            {
                RunRainmeterBang($"!ToggleFadeGroup \"{group}\"");
            }

            /// <summary>Sets fade duration for all skins in a group.</summary>
            /// <param name="group">(required) Group to set the value for.</param>
            /// <param name="milliseconds">(required) Number of milliseconds for fade transition.</param>
            public static void FadeDurationGroup(string group, int milliseconds)
            {
                RunRainmeterBang($"!FadeDurationGroup {milliseconds} \"{group}\"");
            }

            /// <summary>Deactivates all configs in a group.</summary>
            /// <param name="group">(required) Group to deactivate.</param>
            public static void DeactivateConfigGroup(string group)
            {
                RunRainmeterBang($"!DeactivateConfigGroup \"{group}\"");
            }

            /// <summary>Updates all skins in a group.</summary>
            /// <param name="group">(required) Group to update.</param>
            public static void UpdateGroup(string group)
            {
                RunRainmeterBang($"!UpdateGroup \"{group}\"");
            }

            /// <summary>Redraws all skins in a group.</summary>
            /// <param name="group">(required) Group to redraw.</param>
            public static void RedrawGroup(string group)
            {
                RunRainmeterBang($"!RedrawGroup \"{group}\"");
            }

            /// <summary>Refreshes all skins in a group.</summary>
            /// <param name="group">(required) Group to refresh.</param>
            public static void RefreshGroup(string group)
            {
                RunRainmeterBang($"!RefreshGroup \"{group}\"");
            }

            /// <summary>Sets transparency for all skins in a group. Example: !SetTransparencyGroup "128" "SuiteName"</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="alpha">(required) From 0 (invisible) to 255 (opaque).</param>
            public static void SetTransparencyGroup(string group, int alpha)
            {
                RunRainmeterBang($"!SetTransparencyGroup \"{alpha}\" \"{group}\"");
            }

            /// <summary>Sets draggable state for all skins in a group.</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void DraggableGroup(string group, int setting)
            {
                RunRainmeterBang($"!DraggableGroup {setting} \"{group}\"");
            }

            /// <summary>Sets Z-position for all skins in a group.</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="position">(required) -2 for On desktop, -1 for Bottom, 0 for Normal, 1 for On top, or 2 for Always on top.</param>
            public static void ZPosGroup(string group, int position)
            {
                RunRainmeterBang($"!ZPosGroup \"{position}\" \"{group}\"");
            }

            /// <summary>Sets keep on screen state for all skins in a group.</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void KeepOnScreenGroup(string group, int setting)
            {
                RunRainmeterBang($"!KeepOnScreenGroup {setting} \"{group}\"");
            }

            /// <summary>Sets click through state for all skins in a group.</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void ClickThroughGroup(string group, int setting)
            {
                RunRainmeterBang($"!ClickThroughGroup {setting} \"{group}\"");
            }

            /// <summary>Sets snap edges state for all skins in a group.</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void SnapEdgesGroup(string group, int setting)
            {
                RunRainmeterBang($"!SnapEdgesGroup {setting} \"{group}\"");
            }

            /// <summary>Sets auto select screen state for all skins in a group.</summary>
            /// <param name="group">(required) Name of the group.</param>
            /// <param name="setting">(required) 0 to disable, 1 to enable or -1 to toggle.</param>
            public static void AutoSelectScreenGroup(string group, int setting)
            {
                RunRainmeterBang($"!AutoSelectScreenGroup {setting} \"{group}\"");
            }
        }

        internal static class MeterBangs
        {
            /// <summary>Shows a meter. Example: !ShowMeter "MyMeter"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="meter">(required) Name of the meter to show.</param>
            public static void ShowMeter(string config, string meter)
            {
                RunRainmeterBang($"!ShowMeter {meter} \"{config}\"");
            }

            /// <summary>Hides a meter. Example: !HideMeter "MyMeter"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="meter">(required) Name of the meter to hide.</param>
            public static void HideMeter(string config, string meter)
            {
                RunRainmeterBang($"!HideMeter {meter} \"{config}\"");
            }

            /// <summary>Toggles a meter between shown and hidden. Example: !ToggleMeter "MyMeter"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="meter">(required) Name of the meter to toggle.</param>
            public static void ToggleMeter(string config, string meter)
            {
                RunRainmeterBang($"!ToggleMeter {meter} \"{config}\"");
            }

            /// <summary>Updates a meter. Example: !UpdateMeter "MyMeter"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="meter">(required) Name of the meter to update. Use * to update all meters.</param>
            public static void UpdateMeter(string config, string meter)
            {
                RunRainmeterBang($"!UpdateMeter {meter} \"{config}\"");
            }

            /// <summary>Moves a meter to a new position. Example: !MoveMeter 15 10 "MyMeter"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="x">(required) New X position.</param>
            /// <param name="y">(required) New Y position.</param>
            /// <param name="meter">(required) Name of the meter to move.</param>
            public static void MoveMeter(string config, string x, string y, string meter)
            {
                RunRainmeterBang($"!MoveMeter {x} {y} {meter} \"{config}\"");
            }

            // Meter Group Bangs

            /// <summary>Shows all meters in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Group to show.</param>
            public static void ShowMeterGroup(string config, string group)
            {
                RunRainmeterBang($"!ShowMeterGroup {group} \"{config}\"");
            }

            /// <summary>Hides all meters in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Group to hide.</param>
            public static void HideMeterGroup(string config, string group)
            {
                RunRainmeterBang($"!HideMeterGroup {group} \"{config}\"");
            }

            /// <summary>Toggles all meters in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Group to toggle.</param>
            public static void ToggleMeterGroup(string config, string group)
            {
                RunRainmeterBang($"!ToggleMeterGroup {group} \"{config}\"");
            }

            /// <summary>Updates all meters in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the meter group to update.</param>
            public static void UpdateMeterGroup(string config, string group)
            {
                RunRainmeterBang($"!UpdateMeterGroup {group} \"{config}\"");
            }
        }

        internal static class MeasureBangs
        {
            /// <summary>Enables a measure. Example: !EnableMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            public static void EnableMeasure(string config, string measure)
            {
                RunRainmeterBang($"!EnableMeasure {measure} \"{config}\"");
            }

            /// <summary>Disables a measure. Example: !DisableMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            public static void DisableMeasure(string config, string measure)
            {
                RunRainmeterBang($"!DisableMeasure {measure} \"{config}\"");
            }

            /// <summary>Toggles a measure between enabled and disabled. Example: !ToggleMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            public static void ToggleMeasure(string config, string measure)
            {
                RunRainmeterBang($"!ToggleMeasure {measure} \"{config}\"");
            }

            /// <summary>Pauses a measure. Example: !PauseMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            public static void PauseMeasure(string config, string measure)
            {
                RunRainmeterBang($"!PauseMeasure {measure} \"{config}\"");
            }

            /// <summary>Unpauses a measure. Example: !UnpauseMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            public static void UnpauseMeasure(string config, string measure)
            {
                RunRainmeterBang($"!UnpauseMeasure {measure} \"{config}\"");
            }

            /// <summary>Toggles a measure between paused and unpaused. Example: !TogglePauseMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            public static void TogglePauseMeasure(string config, string measure)
            {
                RunRainmeterBang($"!TogglePauseMeasure {measure} \"{config}\"");
            }

            /// <summary>Updates a measure. Example: !UpdateMeasure "CPUMeasure"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure. Use * to update all measures.</param>
            public static void UpdateMeasure(string config, string measure)
            {
                RunRainmeterBang($"!UpdateMeasure {measure} \"{config}\"");
            }

            /// <summary>Sends arguments to a measure. Example: !CommandMeasure "NowPlayingParent" "Previous"</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="measure">(required) Name of the measure.</param>
            /// <param name="arguments">(required) Arguments to send to the measure.</param>
            public static void CommandMeasure(string config, string measure, string arguments)
            {
                RunRainmeterBang($"!CommandMeasure {measure} \"{arguments}\" \"{config}\"");
            }

            // Measure Group Bangs

            /// <summary>Enables all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void EnableMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!EnableMeasureGroup {group} \"{config}\"");
            }

            /// <summary>Disables all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void DisableMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!DisableMeasureGroup {group} \"{config}\"");
            }

            /// <summary>Toggles all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void ToggleMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!ToggleMeasureGroup {group} \"{config}\"");
            }

            /// <summary>Pauses all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void PauseMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!PauseMeasureGroup {group} \"{config}\"");
            }

            /// <summary>Unpauses all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void UnpauseMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!UnpauseMeasureGroup {group} \"{config}\"");
            }

            /// <summary>Toggles pause state for all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void TogglePauseMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!TogglePauseMeasureGroup {group} \"{config}\"");
            }

            /// <summary>Updates all measures in a group.</summary>
            /// <param name="config">(required) Config name.</param>
            /// <param name="group">(required) Name of the group.</param>
            public static void UpdateMeasureGroup(string config, string group)
            {
                RunRainmeterBang($"!UpdateMeasureGroup {group} \"{config}\"");
            }
        }

        /// <summary>
        /// These bangs set the state of mouse detection / action on one or more meters or the skin background.
        /// 
        /// The states involved are:
        /// 
        /// Enabled: The actions defined on the meter or skin background are detected and executed normally.
        /// Disabled: The actions defined on the meter or skin background are detected, but cause no change to the cursor, and take no action. This can be used to "block" mouse actions on meters or the skin behind, causing them to be ignored, while not taking any action itself.
        /// This is similar to using !SetOption to set the value of a mouse action to[] (an empty action).
        /// Cleared: The actions defined on the meter or skin background are in effect "removed", and are not detected.This will allow mouse actions on meters or the skin background behind the meter to be detected and executed.
        /// This is similar to using !SetOption to set the value of a mouse action to "" (an empty string).
        /// 
        /// When action(s) are either Disabled or Cleared, simply returning the state to Enabled, either explicitly or with a toggle, will return the actions to their original defined state, without having to re-define them in a !SetOption.
        /// 
        /// Mouse actions on meters or the skin background can be set to an initial state of Disabled or Cleared by using the OnRefreshAction action in the [Rainmeter] section of the skin.
        /// </summary>
        internal static class MouseActionStateBangs
        {
            internal enum MouseAction
            {
                All,
                LeftMouseUpAction,
                LeftMouseDownAction,
                LeftMouseDoubleClickAction,
                RightMouseUpAction,
                RightMouseDownAction,
                RightMouseDoubleClickAction,
                MiddleMouseUpAction,
                MiddleMouseDownAction,
                MiddleMouseDoubleClickAction,
                X1MouseUpAction,
                X1MouseDownAction,
                X1MouseDoubleClickAction,
                X2MouseUpAction,
                X2MouseDownAction,
                X2MouseDoubleClickAction,
                MouseScrollUpAction,
                MouseScrollDownAction,
                MouseScrollLeftAction,
                MouseScrollRightAction,
                MouseOverAction,
                MouseLeaveAction
            }

            /// <summary>
            /// Converts MouseAction enum values to the string format expected by Rainmeter.
            /// </summary>
            private static string ConvertMouseActionsToString(params MouseAction[] mouseActions)
            {
                if (mouseActions == null || mouseActions.Length == 0)
                    return "*";

                if (mouseActions.Length == 1 && mouseActions[0] == MouseAction.All)
                    return "*";

                var actionStrings = mouseActions.Select(a => a == MouseAction.All ? "*" : a.ToString());
                return string.Join("|", actionStrings);
            }

            /// <summary>
            /// Disables the mouse action(s) defined in the mouseActions parameter.
            /// 
            /// Example: DisableMouseAction("MyConfig", "MyMeter", MouseAction.LeftMouseUpAction)
            /// Example: DisableMouseAction("MyConfig", "MyMeter", MouseAction.MouseOverAction, MouseAction.MouseLeaveAction)
            /// Example: DisableMouseAction("MyConfig", "Rainmeter", MouseAction.All)
            /// </summary>
            /// 
            /// <param name="config">
            /// (required) Config name.
            /// </param>
            /// 
            /// <param name="meter">
            /// (required) Name of a meter or * (all) to apply the change to.
            /// Can also be the [Rainmeter] section.
            /// </param>
            /// 
            /// <param name="mouseActions">
            /// (required) One or more mouse actions to disable. Use MouseAction.All for all actions.
            /// </param>
            public static void DisableMouseAction(string config, string meter, params MouseAction[] mouseActions)
            {
                var actionsString = ConvertMouseActionsToString(mouseActions);
                RunRainmeterBang($"!DisableMouseAction {meter} \"{actionsString}\" \"{config}\"");
            }

            /// <summary>
            /// Clears the mouse action(s) defined in the mouseActions parameter.
            /// 
            /// Example: ClearMouseAction("MyConfig", "MyMeter", MouseAction.MouseOverAction, MouseAction.MouseLeaveAction)
            /// </summary>
            /// 
            /// <param name="config">
            /// (required) Config name.
            /// </param>
            /// 
            /// <param name="meter">
            /// (required) Name of a meter or * (all) to apply the change to.
            /// Can also be the [Rainmeter] section.
            /// </param>
            /// 
            /// <param name="mouseActions">
            /// (required) One or more mouse actions to clear. Use MouseAction.All for all actions.
            /// </param>
            public static void ClearMouseAction(string config, string meter, params MouseAction[] mouseActions)
            {
                var actionsString = ConvertMouseActionsToString(mouseActions);
                RunRainmeterBang($"!ClearMouseAction {meter} \"{actionsString}\" \"{config}\"");
            }

            /// <summary>
            /// Enables the original mouse action(s) defined in the mouseActions parameter.
            /// 
            /// Example: EnableMouseAction("MyConfig", "MyMeter", MouseAction.MouseOverAction, MouseAction.MouseLeaveAction)
            /// </summary>
            /// 
            /// <param name="config">
            /// (required) Config name.
            /// </param>
            /// 
            /// <param name="meter">
            /// (required) Name of a meter or * (all) to apply the change to.
            /// Can also be the [Rainmeter] section.
            /// </param>
            /// 
            /// <param name="mouseActions">
            /// (required) One or more mouse actions to enable. Use MouseAction.All for all actions.
            /// </param>
            public static void EnableMouseAction(string config, string meter, params MouseAction[] mouseActions)
            {
                var actionsString = ConvertMouseActionsToString(mouseActions);
                RunRainmeterBang($"!EnableMouseAction {meter} \"{actionsString}\" \"{config}\"");
            }

            /// <summary>
            /// Toggles the mouse action(s) defined in the mouseActions parameter between the enabled and disabled/cleared state.
            /// Disabled is the default toggle state, but will change to the Cleared state if the mouse action was cleared beforehand.
            /// 
            /// Example: ToggleMouseAction("MyConfig", "MyMeter", MouseAction.All)
            /// </summary>
            /// 
            /// <param name="config">
            /// (required) Config name.
            /// </param>
            /// 
            /// <param name="meter">
            /// (required) Name of a meter or * (all) to apply the change to.
            /// Can also be the [Rainmeter] section.
            /// </param>
            /// 
            /// <param name="mouseActions">
            /// (required) One or more mouse actions to toggle. Use MouseAction.All for all actions.
            /// </param>
            public static void ToggleMouseAction(string config, string meter, params MouseAction[] mouseActions)
            {
                var actionsString = ConvertMouseActionsToString(mouseActions);
                RunRainmeterBang($"!ToggleMouseAction {meter} \"{actionsString}\" \"{config}\"");
            }
        }
    }
}
