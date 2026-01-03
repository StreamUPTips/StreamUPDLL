using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Load full product data from file with optional caching.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="useCache">Use cache if available (default: true)</param>
        /// <returns>Full product data as JObject, null if not found or error</returns>
        public JObject LoadProductDataV2(string productNumber, bool useCache = true)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return null;
                }

                // Check cache first (if useCache is true)
                if (useCache && SettingsCacheV2.TryGetCachedData(productNumber, out JObject cached))
                {
                    LogDebug($"[CACHE HIT] Loaded product data from cache: {productNumber}");
                    return cached;
                }

                // Log cache miss or cache disabled
                if (useCache)
                {
                    LogDebug(
                        $"[CACHE MISS] Product not in cache, loading from file: {productNumber}"
                    );
                }
                else
                {
                    LogDebug(
                        $"[CACHE DISABLED] Loading from file without caching: {productNumber}"
                    );
                }

                // Get directory
                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return null;
                }

                // Build file path
                string filePath = Path.Combine(directory, $"{productNumber}_Data.json");

                // Check file exists
                if (!File.Exists(filePath))
                {
                    LogError($"Product data file not found: {filePath}");
                    return null;
                }

                // Read and parse file
                string jsonData = File.ReadAllText(filePath);
                JObject data = JObject.Parse(jsonData);

                // Update cache with loaded data (if caching is enabled for this load)
                if (useCache)
                {
                    SettingsCacheV2.SetCachedData(productNumber, data);
                    LogDebug($"[FILE READ] Loaded from file and cached: {productNumber}");
                }
                else
                {
                    LogDebug($"[FILE READ] Loaded from file (not cached): {productNumber}");
                }

                return data;
            }
            catch (JsonException ex)
            {
                LogError($"JSON parsing error for product {productNumber}: {ex.Message}");
                SettingsCacheV2.ClearCache(productNumber);
                return null;
            }
            catch (IOException ex)
            {
                LogError($"File I/O error for product {productNumber}: {ex.Message}");
                SettingsCacheV2.ClearCache(productNumber);
                return null;
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error loading product data {productNumber}: {ex.Message}");
                SettingsCacheV2.ClearCache(productNumber);
                return null;
            }
        }

        /// <summary>
        /// Save full product data to file and optionally update cache.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="data">Product data to save</param>
        /// <param name="useCache">Update cache after save (default: true)</param>
        /// <returns>True if save successful</returns>
        public bool SaveProductDataV2(string productNumber, JObject data, bool useCache = true)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (data == null)
                {
                    LogError("Data is null");
                    return false;
                }

                // Get directory and ensure it exists
                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return false;
                }

                string filePath = Path.Combine(directory, $"{productNumber}_Data.json");

                // Ensure directory exists
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write to file with indented formatting
                string jsonOutput = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, jsonOutput);

                // Update cache atomically after successful write (if caching is enabled for this save)
                if (useCache)
                {
                    SettingsCacheV2.SetCachedData(productNumber, data);
                    LogDebug($"[SAVE] Saved to file and updated cache: {productNumber}");
                }
                else
                {
                    LogDebug($"[SAVE] Saved to file only (cache not updated): {productNumber}");
                }

                return true;
            }
            catch (IOException ex)
            {
                LogError($"File I/O error saving product {productNumber}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error saving product data {productNumber}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get a single setting value with type conversion (type-safe).
        /// </summary>
        /// <typeparam name="T">Type to convert setting to</typeparam>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="settingKey">Setting key</param>
        /// <param name="defaultValue">Value to return if setting not found</param>
        /// <returns>Setting value converted to type T, or defaultValue if not found</returns>
        public T LoadSettingV2<T>(string productNumber, string settingKey, T defaultValue)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber) || string.IsNullOrEmpty(settingKey))
                {
                    LogDebug(
                        $"Invalid input: productNumber='{productNumber}', settingKey='{settingKey}'"
                    );
                    return defaultValue;
                }

                // Load data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogDebug(
                        $"No data found for {productNumber}, returning default for {settingKey}"
                    );
                    return defaultValue;
                }

                // Get settings section
                JObject settings = data["settings"] as JObject;
                if (settings == null || !settings.TryGetValue(settingKey, out JToken value))
                {
                    LogDebug($"Setting not found: {settingKey}, returning default");
                    return defaultValue;
                }

                // Type conversion
                T result = value.ToObject<T>();
                LogDebug($"[GET] Retrieved setting {settingKey} = {result}");
                return result;
            }
            catch (JsonSerializationException ex)
            {
                LogError($"Type conversion error for setting {settingKey}: {ex.Message}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                LogError($"Error getting setting {settingKey}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Update a single setting and save to file.
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
                LogDebug($"[UPDATE] Setting changed {settingKey} = {value}");

                // Save to file (will show cache update status)
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogDebug($"[UPDATE] Setting {settingKey} saved successfully");
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
        /// Update multiple settings at once and save to file.
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
                    LogDebug("Settings dictionary is null or empty");
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
                LogDebug($"[UPDATE] Updating {settings.Count} settings for {productNumber}");
                foreach (var kvp in settings)
                {
                    settingsObj[kvp.Key] = JToken.FromObject(kvp.Value);
                    LogDebug($"[UPDATE] Setting changed {kvp.Key} = {kvp.Value}");
                }

                // Save to file (will show cache update status)
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogDebug($"[UPDATE] All {settings.Count} settings saved successfully");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error updating settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update the obsConnection setting and save to file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="obsConnection">OBS connection index</param>
        /// <returns>True if update successful</returns>
        public bool UpdateObsConnectionV2(string productNumber, int obsConnection)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                // Load data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogError($"Failed to load data for product {productNumber}");
                    return false;
                }

                // Update obsConnection
                data["obsConnection"] = obsConnection;
                LogDebug($"Updated obsConnection = {obsConnection}");

                // Save to file
                return SaveProductDataV2(productNumber, data);
            }
            catch (Exception ex)
            {
                LogError($"Error updating obsConnection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update the productInfo section and save to file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="productInfo">New productInfo object</param>
        /// <returns>True if update successful</returns>
        public bool UpdateProductInfoV2(string productNumber, JObject productInfo)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (productInfo == null)
                {
                    LogError("Product info is null");
                    return false;
                }

                // Load data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogError($"Failed to load data for product {productNumber}");
                    return false;
                }

                // Update productInfo section
                data["productInfo"] = productInfo;
                LogDebug("Updated productInfo section");

                // Save to file
                return SaveProductDataV2(productNumber, data);
            }
            catch (Exception ex)
            {
                LogError($"Error updating productInfo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clear cache for a specific product. Use this to force reload from file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public void InvalidateCacheV2(string productNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(productNumber))
                {
                    SettingsCacheV2.ClearCache(productNumber);
                    LogDebug($"Cleared cache for product {productNumber}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error clearing cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear entire cache. Use for application shutdown or full reset.
        /// </summary>
        public void InvalidateAllCacheV2()
        {
            try
            {
                SettingsCacheV2.ClearAllCache();
                LogDebug("Cleared all product cache");
            }
            catch (Exception ex)
            {
                LogError($"Error clearing all cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the file path for a product's data file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>Full file path, or empty string if unable to construct path</returns>
        private string GetProductDataFilePath(string productNumber)
        {
            try
            {
                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return string.Empty;
                }

                return Path.Combine(directory, $"{productNumber}_Data.json");
            }
            catch (Exception ex)
            {
                LogError($"Error constructing file path: {ex.Message}");
                return string.Empty;
            }
        }

        public bool GetStreamerBotDirectory(out string directory)
        {
            directory = string.Empty;

            try
            {
                string programDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Validate base directory
                if (string.IsNullOrEmpty(programDirectory))
                {
                    return false;
                }

                directory = Path.Combine(programDirectory, "StreamUP", "Data");

                // Verify the directory exists or can be created
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Verify we have write access
                string testFile = Path.Combine(directory, ".test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get a setting value from an already-loaded settings object with a default fallback.
        /// Use this for type-safe access to settings values from a JObject.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="settingsData">The JObject containing settings</param>
        /// <param name="key">The setting key</param>
        /// <param name="defaultValue">Default value if key not found or conversion fails</param>
        /// <returns>The setting value or default if not found/error</returns>
        public T GetSettingV2<T>(JObject settingsData, string key, T defaultValue)
        {
            try
            {
                if (settingsData == null || string.IsNullOrEmpty(key))
                    return defaultValue;

                JToken value = settingsData[key];
                if (value == null)
                    return defaultValue;

                return value.Value<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Set a setting value in an already-loaded settings object.
        /// Use this to update values in a JObject.
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <param name="settingsData">The JObject containing settings</param>
        /// <param name="key">The setting key</param>
        /// <param name="value">The value to set</param>
        public void SetSettingV2<T>(JObject settingsData, string key, T value)
        {
            try
            {
                if (settingsData != null && !string.IsNullOrEmpty(key))
                {
                    settingsData[key] = JToken.FromObject(value);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error setting {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply product settings with custom callback - handles all initialization and loading boilerplate.
        /// Products only need to implement the ApplySettings callback with their custom logic.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="applySettings">Callback that receives the loaded settings JObject and applies them</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool ApplyProductSettingsV2(string productNumber, Func<JObject, bool> applySettings)
        {
            try
            {
                // 1. Initialize product (verify settings file exists)
                if (!InitializeProductV2(productNumber, out string initError))
                {
                    LogError($"Product initialization failed: {initError}");
                    return false;
                }

                // 2. Load product data WITHOUT caching (one-time run when applying settings)
                JObject productData = LoadProductDataV2(productNumber, useCache: false);
                if (productData == null)
                {
                    LogError("Failed to load product data");
                    return false;
                }

                // 3. Extract productInfo from data
                JObject productInfoObj = productData["productInfo"] as JObject;
                if (productInfoObj == null)
                {
                    LogError("No productInfo found in settings data");
                    return false;
                }

                string productName = productInfoObj["productName"]?.ToString() ?? "Unknown Product";

                // 4. Extract settings
                JObject settingsObj = productData["settings"] as JObject;
                if (settingsObj == null)
                {
                    LogError("No settings found in settings data");
                    return false;
                }

                // 5. Ensure obsConnection is set (defaults to 0 if not in settings)
                if (settingsObj["ObsConnection"] == null)
                {
                    int obsConnection = (int?)productData["obsConnection"] ?? 0;
                    settingsObj["ObsConnection"] = obsConnection;
                }

                LogInfo($"Loaded settings for product: {productName}");

                // 6. Call the product-specific callback to apply settings
                if (!applySettings(settingsObj))
                {
                    LogError("Failed to apply product settings");
                    return false;
                }

                LogInfo("Successfully loaded and set product settings");
                ShowToastNotification(
                    StreamUpLib.NotificationType.Success,
                    "StreamUP Settings Loaded",
                    $"{productName} settings were set successfully"
                );
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ApplyProductSettingsV2: {ex.Message}");
                return false;
            }
        }
    }
}
