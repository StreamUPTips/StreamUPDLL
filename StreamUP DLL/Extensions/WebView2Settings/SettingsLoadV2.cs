using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Load full product data from file with optional caching.
        /// NOTE: Cache is automatically populated on first load if useCache=true.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="useCache">Use cache if available (default: true). On cache miss, file is loaded AND cached.</param>
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

                // Check if settings have changed (Non-Persisted global signal)
                string settingsChangedVar = $"__StreamUP_SettingsChanged_{productNumber}";
                try
                {
                    if (_CPH.GetGlobalVar<bool>(settingsChangedVar, false))
                    {
                        // Clear the signal for next time
                        _CPH.UnsetGlobalVar(settingsChangedVar);
                        LogDebug(
                            $"Settings change detected - forcing reload from disk: {productNumber}"
                        );
                        // Force cache miss to reload from disk
                        SettingsCacheV2.ClearCache(productNumber);
                    }
                }
                catch (Exception ex)
                {
                    LogDebug($"Error checking settings change signal: {ex.Message}");
                }

                // Check cache first (if useCache is true and no settings change detected)
                if (useCache && SettingsCacheV2.TryGetCachedData(productNumber, out JObject cached))
                {
                    LogDebug($"Cache hit - Loaded product data from cache: {productNumber}");
                    return cached;
                }

                // Log cache miss or cache disabled
                if (useCache)
                {
                    LogDebug($"Cache miss - Loading product from file: {productNumber}");
                }
                else
                {
                    LogDebug(
                        $"Cache disabled - Loading product from file without caching: {productNumber}"
                    );
                }

                // Get directory
                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return null;
                }

                // Build file path
                string filePath = System.IO.Path.Combine(directory, $"{productNumber}_Data.json");

                // Check file exists
                if (!System.IO.File.Exists(filePath))
                {
                    LogError($"Product data file not found: {filePath}");
                    return null;
                }

                // Read and parse file
                string jsonData = System.IO.File.ReadAllText(filePath);
                JObject data = JObject.Parse(jsonData);

                // Update cache with loaded data (if caching is enabled for this load)
                if (useCache)
                {
                    SettingsCacheV2.SetCachedData(productNumber, data);
                    LogDebug($"Product loaded from file and cached: {productNumber}");
                }
                else
                {
                    LogDebug($"Product loaded from file (not cached): {productNumber}");
                }

                return data;
            }
            catch (JsonException ex)
            {
                LogError($"JSON parsing error for product {productNumber}: {ex.Message}");
                SettingsCacheV2.ClearCache(productNumber);
                return null;
            }
            catch (System.IO.IOException ex)
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
        /// Load product info section with optional caching.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="useCache">Use cache if available (default: true)</param>
        /// <returns>productInfo as JObject, null if not found</returns>
        public JObject LoadProductInfoV2(string productNumber, bool useCache = true)
        {
            try
            {
                JObject data = LoadProductDataV2(productNumber, useCache);
                if (data == null)
                {
                    LogDebug($"No product data found for {productNumber}");
                    return null;
                }

                JObject productInfo = data["productInfo"] as JObject;
                if (productInfo == null)
                {
                    LogDebug($"No productInfo section found for {productNumber}");
                    return null;
                }

                LogDebug($"Loaded productInfo for {productNumber}");
                return productInfo;
            }
            catch (Exception ex)
            {
                LogError($"Error loading productInfo for {productNumber}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load settings section with optional caching.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="useCache">Use cache if available (default: true)</param>
        /// <returns>settings as JObject, null if not found</returns>
        public JObject LoadSettingsV2(string productNumber, bool useCache = true)
        {
            try
            {
                JObject data = LoadProductDataV2(productNumber, useCache);
                if (data == null)
                {
                    LogDebug($"No product data found for {productNumber}");
                    return null;
                }

                JObject settings = data["settings"] as JObject;
                if (settings == null)
                {
                    LogDebug($"No settings section found for {productNumber}");
                    return null;
                }

                LogDebug($"Loaded settings for {productNumber}");
                return settings;
            }
            catch (Exception ex)
            {
                LogError($"Error loading settings for {productNumber}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load OBS connection index with optional caching.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="useCache">Use cache if available (default: true)</param>
        /// <returns>OBS connection index, or 0 if not found</returns>
        public int LoadObsConnectionV2(string productNumber, bool useCache = true)
        {
            try
            {
                JObject data = LoadProductDataV2(productNumber, useCache);
                if (data == null)
                {
                    LogDebug($"No product data found for {productNumber}");
                    return 0;
                }

                int obsConnection = (int?)data["obsConnection"] ?? 0;
                LogDebug($"Loaded obsConnection for {productNumber}");
                return obsConnection;
            }
            catch (Exception ex)
            {
                LogError($"Error loading obsConnection for {productNumber}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Load canvas scale factor with optional caching.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="useCache">Use cache if available (default: true)</param>
        /// <returns>Canvas scale factor, or 1.0 if not found</returns>
        public double LoadScaleFactorV2(string productNumber, bool useCache = true)
        {
            try
            {
                JObject data = LoadProductDataV2(productNumber, useCache);
                if (data == null)
                {
                    LogDebug($"No product data found for {productNumber}");
                    return 1.0;
                }

                double scaleFactor = (double?)data["scaleFactor"] ?? 1.0;
                LogDebug($"Loaded scaleFactor for {productNumber}");
                return scaleFactor;
            }
            catch (Exception ex)
            {
                LogError($"Error loading scaleFactor for {productNumber}: {ex.Message}");
                return 1.0;
            }
        }

        /// <summary>
        /// Load a single setting value from file with type conversion (type-safe).
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
                LogDebug($"Retrieved setting from file: {settingKey}");
                return result;
            }
            catch (JsonSerializationException ex)
            {
                LogError($"Type conversion error for setting {settingKey}: {ex.Message}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                LogError($"Error loading setting {settingKey}: {ex.Message}");
                return defaultValue;
            }
        }
    }
}
