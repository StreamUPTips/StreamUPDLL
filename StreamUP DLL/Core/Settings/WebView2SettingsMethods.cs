using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Opens the WebView2 settings menu for a product.
        /// This method also auto-registers the product config for use by IsProductReady() in Main actions.
        /// </summary>
        /// <param name="settingsConfig">JObject containing the settings menu configuration (from product)</param>
        /// <returns>True if settings window opened successfully</returns>
        public bool OpenSettingsMenu(JObject settingsConfig)
        {
            LogInfo("OpenSettingsMenu called");

            try
            {
                // Validate config
                if (settingsConfig == null)
                {
                    LogError("Settings config is null");
                    return false;
                }

                // Check if another settings window is already open
                if (_settingsWindow != null && !_settingsWindow.IsDisposed)
                {
                    LogInfo("Settings window already open for another product");

                    // Bring existing window to front
                    try
                    {
                        _settingsWindow.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            _settingsWindow.WindowState = System.Windows.Forms.FormWindowState.Normal;
                            _settingsWindow.Activate();
                            _settingsWindow.BringToFront();
                        });
                    }
                    catch { }

                    return false;
                }

                // Store current instance for callbacks
                _currentInstance = this;

                // Extract product info
                _currentProductNumber = settingsConfig["productNumber"]?.ToString() ?? _ProductIdentifier;
                _currentConfig = settingsConfig;

                // AUTO-REGISTER: Store the product config for future use by IsProductReady()
                // This enables the simplified workflow where Main actions don't need the JSON
                ProductConfigRegistry.RegisterProduct(_currentProductNumber, settingsConfig);
                LogDebug($"Auto-registered product config for: {_currentProductNumber}");

                // INVALIDATE: Clear validation cache since user might change settings
                ProductValidationCache.InvalidateCache(_currentProductNumber);

                // Update window title with product name
                var productName = settingsConfig["productName"]?.ToString() ?? "Settings";
                var productVersion = settingsConfig["productVersion"]?.ToString();

                LogInfo($"Opening settings for: {productName} ({_currentProductNumber})");

                // Load saved settings from cache/file
                _currentSavedSettings = LoadProductSettingsInternal(_currentProductNumber);

                // Fetch live data from Streamer.bot and OBS
                _currentLiveData = FetchAllLiveData();

                // Create and show the window
                Task.Run(async () =>
                {
                    try
                    {
                        await CreateAndShowSettingsWindow();
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to open settings window: {ex.Message}");
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                LogError($"OpenSettingsMenu failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens the WebView2 settings menu using a JSON string
        /// </summary>
        /// <param name="settingsConfigJson">JSON string containing the settings menu configuration</param>
        /// <returns>True if settings window opened successfully</returns>
        public bool OpenSettingsMenu(string settingsConfigJson)
        {
            try
            {
                var config = JObject.Parse(settingsConfigJson);
                return OpenSettingsMenu(config);
            }
            catch (Exception ex)
            {
                LogError($"Failed to parse settings config JSON: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a settings window is currently open
        /// </summary>
        /// <returns>True if settings window is open</returns>
        public bool IsSettingsWindowOpen()
        {
            return _settingsWindow != null && !_settingsWindow.IsDisposed;
        }

        /// <summary>
        /// Close the settings window if open
        /// </summary>
        /// <param name="force">If true, close without prompting for unsaved changes</param>
        /// <returns>True if window was closed or wasn't open</returns>
        public bool CloseSettingsWindow(bool force = false)
        {
            if (_settingsWindow == null || _settingsWindow.IsDisposed)
            {
                return true;
            }

            try
            {
                if (force)
                {
                    _hasUnsavedChanges = false; // Skip the prompt
                }

                _settingsWindow.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    _settingsWindow.Close();
                });

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to close settings window: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update the window title
        /// </summary>
        private void UpdateWindowTitle(string productName, string productVersion)
        {
            if (_settingsWindow == null || _settingsWindow.IsDisposed)
            {
                return;
            }

            try
            {
                var title = !string.IsNullOrEmpty(productVersion)
                    ? $"{productName} v{productVersion} - Settings"
                    : $"{productName} - Settings";

                _settingsWindow.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    _settingsWindow.Text = title;
                });
            }
            catch (Exception ex)
            {
                LogDebug($"Failed to update window title: {ex.Message}");
            }
        }
    }
}
