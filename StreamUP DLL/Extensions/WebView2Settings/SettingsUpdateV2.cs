using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Update a single setting and save to file and cache.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="settingKey">Setting key</param>
        /// <param name="value">New value</param>
        /// <returns>True if update successful</returns>
        public bool UpdateSettingV2(string productNumber, string settingKey, object value)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber) || string.IsNullOrEmpty(settingKey))
                {
                    LogError("Product number or setting key is null or empty");
                    return false;
                }

                // Load data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogError($"Failed to load data for product {productNumber}");
                    return false;
                }

                // Get or create settings section
                JObject settings = data["settings"] as JObject;
                if (settings == null)
                {
                    settings = new JObject();
                    data["settings"] = settings;
                }

                // Update setting
                settings[settingKey] = JToken.FromObject(value);
                LogDebug($"Setting changed in memory: {settingKey}");

                // Save to file
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"Setting updated and saved: {settingKey}");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error updating setting {settingKey}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update multiple settings at once and save to file and cache.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="settings">Dictionary of setting keys and values</param>
        /// <returns>True if update successful</returns>
        public bool UpdateSettingsV2(string productNumber, Dictionary<string, object> settings)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (settings == null || settings.Count == 0)
                {
                    LogDebug("No settings to update");
                    return true; // Nothing to update is not an error
                }

                // Load data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogError($"Failed to load data for product {productNumber}");
                    return false;
                }

                // Get or create settings section
                JObject settingsObj = data["settings"] as JObject;
                if (settingsObj == null)
                {
                    settingsObj = new JObject();
                    data["settings"] = settingsObj;
                }

                // Update all settings
                LogDebug($"Updating {settings.Count} settings");
                foreach (var kvp in settings)
                {
                    settingsObj[kvp.Key] = JToken.FromObject(kvp.Value);
                    LogDebug($"Setting changed in memory: {kvp.Key}");
                }

                // Save to file
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"Multiple settings updated and saved ({settings.Count} total)");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error updating settings: {ex.Message}");
                return false;
            }
        }
    }
}
