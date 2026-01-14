using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        #region Public Helper Methods for Products

        /// <summary>
        /// Get a single setting value for the current product
        /// </summary>
        /// <typeparam name="T">The type to return (string, int, bool, double, List, Dictionary, etc.)</typeparam>
        /// <param name="key">The setting key (backendName from settings config)</param>
        /// <param name="defaultValue">Default value if setting doesn't exist</param>
        /// <returns>The setting value or default</returns>
        public T GetProductSetting<T>(string key, T defaultValue)
        {
            LogDebug($"Getting product setting: {key}");

            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                if (settings == null)
                {
                    LogDebug($"No settings found, returning default for: {key}");
                    return defaultValue;
                }

                var settingsObj = settings["settings"] as JObject;
                if (settingsObj == null || !settingsObj.ContainsKey(key))
                {
                    LogDebug($"Key not found, returning default for: {key}");
                    return defaultValue;
                }

                var value = settingsObj[key];

                // Handle null
                if (value == null || value.Type == JTokenType.Null)
                {
                    return defaultValue;
                }

                // Convert to requested type
                return value.ToObject<T>();
            }
            catch (Exception ex)
            {
                LogError($"Failed to get setting '{key}': {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Set a single setting value for the current product
        /// </summary>
        /// <param name="key">The setting key (backendName from settings config)</param>
        /// <param name="value">The value to save</param>
        /// <returns>True if successful</returns>
        public bool SetProductSetting(string key, object value)
        {
            LogInfo($"Setting product setting: {key}");

            try
            {
                // Load existing settings or create new
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                if (settings == null)
                {
                    settings = CreateDefaultSettingsStructure(_ProductIdentifier);
                }

                // Ensure settings object exists
                if (settings["settings"] == null)
                {
                    settings["settings"] = new JObject();
                }

                var settingsObj = settings["settings"] as JObject;

                // Set the value
                settingsObj[key] = value != null ? JToken.FromObject(value) : JValue.CreateNull();

                // Update meta
                if (settings["meta"] == null)
                {
                    settings["meta"] = new JObject();
                }
                settings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

                // Save
                return SaveProductSettingsInternal(_ProductIdentifier, settings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to set setting '{key}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set multiple settings at once
        /// </summary>
        /// <param name="settingsToUpdate">Dictionary of key-value pairs to set</param>
        /// <returns>True if successful</returns>
        public bool SetProductSettings(Dictionary<string, object> settingsToUpdate)
        {
            LogInfo($"Setting {settingsToUpdate.Count} product settings");

            try
            {
                // Load existing settings or create new
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                if (settings == null)
                {
                    settings = CreateDefaultSettingsStructure(_ProductIdentifier);
                }

                // Ensure settings object exists
                if (settings["settings"] == null)
                {
                    settings["settings"] = new JObject();
                }

                var settingsObj = settings["settings"] as JObject;

                // Set all values
                foreach (var kvp in settingsToUpdate)
                {
                    settingsObj[kvp.Key] = kvp.Value != null ? JToken.FromObject(kvp.Value) : JValue.CreateNull();
                }

                // Update meta
                if (settings["meta"] == null)
                {
                    settings["meta"] = new JObject();
                }
                settings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

                // Save
                return SaveProductSettingsInternal(_ProductIdentifier, settings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to set multiple settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all settings for the current product
        /// </summary>
        /// <returns>JObject containing all settings, or empty JObject</returns>
        public JObject GetAllProductSettings()
        {
            LogDebug("Getting all product settings");

            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                return settings?["settings"] as JObject ?? new JObject();
            }
            catch (Exception ex)
            {
                LogError($"Failed to get all settings: {ex.Message}");
                return new JObject();
            }
        }

        /// <summary>
        /// Get the full settings data including meta and productData
        /// </summary>
        /// <returns>Full settings JObject or null</returns>
        public JObject GetFullProductData()
        {
            LogDebug("Getting full product data");

            try
            {
                return LoadProductSettingsInternal(_ProductIdentifier);
            }
            catch (Exception ex)
            {
                LogError($"Failed to get full product data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if settings exist for the current product
        /// </summary>
        /// <returns>True if settings file exists</returns>
        public bool ProductSettingsExist()
        {
            return ProductSettingsExistInternal(_ProductIdentifier);
        }

        /// <summary>
        /// Delete all settings for the current product
        /// </summary>
        /// <returns>True if deleted successfully</returns>
        public bool DeleteProductSettings()
        {
            return DeleteProductSettingsInternal(_ProductIdentifier);
        }

        /// <summary>
        /// Get the OBS connection index selected for the current product
        /// </summary>
        /// <returns>OBS connection index (0-4), defaults to 0</returns>
        public int GetProductObsConnection()
        {
            LogDebug("Getting product OBS connection");

            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                return settings?["productData"]?["obsInstanceNumber"]?.Value<int>() ?? 0;
            }
            catch (Exception ex)
            {
                LogError($"Failed to get OBS connection: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Set the OBS connection index for the current product
        /// </summary>
        /// <param name="connectionIndex">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool SetProductObsConnection(int connectionIndex)
        {
            LogInfo($"Setting product OBS connection to: {connectionIndex}");

            try
            {
                // Validate connection index
                if (connectionIndex < 0 || connectionIndex >= MAX_OBS_CONNECTIONS)
                {
                    LogError($"Invalid OBS connection index: {connectionIndex}");
                    return false;
                }

                // Load existing settings or create new
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                if (settings == null)
                {
                    settings = CreateDefaultSettingsStructure(_ProductIdentifier);
                }

                // Ensure productData exists
                if (settings["productData"] == null)
                {
                    settings["productData"] = new JObject();
                }

                settings["productData"]["obsInstanceNumber"] = connectionIndex;

                // Update meta
                if (settings["meta"] == null)
                {
                    settings["meta"] = new JObject();
                }
                settings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

                // Save
                return SaveProductSettingsInternal(_ProductIdentifier, settings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to set OBS connection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a specific setting key exists
        /// </summary>
        /// <param name="key">The setting key to check</param>
        /// <returns>True if the key exists in settings</returns>
        public bool HasProductSetting(string key)
        {
            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                var settingsObj = settings?["settings"] as JObject;
                return settingsObj != null && settingsObj.ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove a specific setting key
        /// </summary>
        /// <param name="key">The setting key to remove</param>
        /// <returns>True if successful</returns>
        public bool RemoveProductSetting(string key)
        {
            LogInfo($"Removing product setting: {key}");

            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                if (settings == null)
                {
                    return true; // Nothing to remove
                }

                var settingsObj = settings["settings"] as JObject;
                if (settingsObj == null || !settingsObj.ContainsKey(key))
                {
                    return true; // Key doesn't exist
                }

                settingsObj.Remove(key);

                // Update meta
                settings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

                return SaveProductSettingsInternal(_ProductIdentifier, settings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to remove setting '{key}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all setting keys for the current product
        /// </summary>
        /// <returns>List of setting keys</returns>
        public List<string> GetProductSettingKeys()
        {
            var keys = new List<string>();

            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                var settingsObj = settings?["settings"] as JObject;

                if (settingsObj != null)
                {
                    foreach (var prop in settingsObj.Properties())
                    {
                        keys.Add(prop.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to get setting keys: {ex.Message}");
            }

            return keys;
        }

        /// <summary>
        /// Get all settings deserialized into a typed class.
        /// Use this with the generated settings class from the Settings Builder.
        /// </summary>
        /// <typeparam name="T">The typed settings class (generated by Settings Builder)</typeparam>
        /// <returns>Instance of T with all settings populated, or new instance with defaults if no settings exist</returns>
        /// <example>
        /// <code>
        /// // Using generated typed class from Settings Builder:
        /// var settings = sup.GetTypedSettings&lt;MyProductSettings&gt;();
        /// string userName = settings.UserName;  // IntelliSense + type safety!
        /// int volume = settings.MasterVolume;
        /// </code>
        /// </example>
        public T GetTypedSettings<T>() where T : class, new()
        {
            LogDebug($"Getting typed settings as {typeof(T).Name}");

            try
            {
                var settingsObj = GetAllProductSettings();

                if (settingsObj == null || !settingsObj.HasValues)
                {
                    LogDebug("No settings found, returning default instance");
                    return new T();
                }

                // Use Replace for object creation to prevent array merging with defaults
                var serializer = new JsonSerializer
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                return settingsObj.ToObject<T>(serializer) ?? new T();
            }
            catch (Exception ex)
            {
                LogError($"Failed to deserialize settings to {typeof(T).Name}: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// Save all settings from a typed class.
        /// Use this with the generated settings class from the Settings Builder.
        /// </summary>
        /// <typeparam name="T">The typed settings class (generated by Settings Builder)</typeparam>
        /// <param name="settings">The settings instance to save</param>
        /// <returns>True if successful</returns>
        /// <example>
        /// <code>
        /// var settings = sup.GetTypedSettings&lt;MyProductSettings&gt;();
        /// settings.UserName = "NewUser";
        /// settings.MasterVolume = 75;
        /// sup.SaveTypedSettings(settings);
        /// </code>
        /// </example>
        public bool SaveTypedSettings<T>(T settings) where T : class
        {
            LogInfo($"Saving typed settings from {typeof(T).Name}");

            try
            {
                if (settings == null)
                {
                    LogError("Cannot save null settings");
                    return false;
                }

                // Convert typed object to JObject
                var settingsObj = JObject.FromObject(settings);

                // Load existing data or create new
                var fullData = LoadProductSettingsInternal(_ProductIdentifier);
                if (fullData == null)
                {
                    fullData = CreateDefaultSettingsStructure(_ProductIdentifier);
                }

                // Replace settings with the typed object
                fullData["settings"] = settingsObj;

                // Update meta
                if (fullData["meta"] == null)
                {
                    fullData["meta"] = new JObject();
                }
                fullData["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

                return SaveProductSettingsInternal(_ProductIdentifier, fullData);
            }
            catch (Exception ex)
            {
                LogError($"Failed to save typed settings: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Settings Export/Import

        /// <summary>
        /// Export current product settings to a JSON string
        /// </summary>
        /// <returns>JSON string of settings</returns>
        public string ExportProductSettings()
        {
            try
            {
                var settings = LoadProductSettingsInternal(_ProductIdentifier);
                return settings?.ToString(Formatting.Indented) ?? "{}";
            }
            catch (Exception ex)
            {
                LogError($"Failed to export settings: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Import settings from a JSON string
        /// </summary>
        /// <param name="json">JSON string containing settings</param>
        /// <param name="mergeWithExisting">If true, merge with existing settings. If false, replace all.</param>
        /// <returns>True if successful</returns>
        public bool ImportProductSettings(string json, bool mergeWithExisting = false)
        {
            LogInfo($"Importing product settings (merge: {mergeWithExisting})");

            try
            {
                var importedSettings = JObject.Parse(json);

                if (mergeWithExisting)
                {
                    var existingSettings = LoadProductSettingsInternal(_ProductIdentifier);
                    if (existingSettings != null)
                    {
                        // Merge settings
                        var existingSettingsObj = existingSettings["settings"] as JObject ?? new JObject();
                        var importedSettingsObj = importedSettings["settings"] as JObject ?? new JObject();

                        foreach (var prop in importedSettingsObj.Properties())
                        {
                            existingSettingsObj[prop.Name] = prop.Value;
                        }

                        existingSettings["settings"] = existingSettingsObj;
                        existingSettings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");

                        return SaveProductSettingsInternal(_ProductIdentifier, existingSettings);
                    }
                }

                // Replace all
                importedSettings["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");
                importedSettings["meta"]["productNumber"] = _ProductIdentifier;

                return SaveProductSettingsInternal(_ProductIdentifier, importedSettings);
            }
            catch (Exception ex)
            {
                LogError($"Failed to import settings: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
