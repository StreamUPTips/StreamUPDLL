using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Save full product data to file and update cache.
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
                    LogInfo($"Product data saved to file and cache: {productNumber}");
                }
                else
                {
                    LogInfo($"Product data saved to file: {productNumber}");
                }

                // Signal to other actions that settings have changed by setting a Non-Persisted global
                try
                {
                    string globalVarName = $"__StreamUP_SettingsChanged_{productNumber}";
                    _CPH.SetGlobalVar(globalVarName, true, false);
                    LogDebug($"Settings change signal set: {globalVarName}");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to set settings change signal for {productNumber}: {ex.Message}");
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
        /// Save product info section to file and cache (overwrites entire section).
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="productInfo">Product info JObject to save</param>
        /// <returns>True if save successful</returns>
        public bool SaveProductInfoV2(string productNumber, JObject productInfo)
        {
            try
            {
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
                LogDebug($"Product info updated in memory");

                // Save to file
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"Product info saved to file and cache");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error saving product info: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save settings section to file and cache (overwrites entire section).
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="settings">Settings JObject to save</param>
        /// <returns>True if save successful</returns>
        public bool SaveSettingsV2(string productNumber, JObject settings)
        {
            try
            {
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (settings == null)
                {
                    LogError("Settings is null");
                    return false;
                }

                // Load data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogError($"Failed to load data for product {productNumber}");
                    return false;
                }

                // Update settings section
                data["settings"] = settings;
                LogDebug($"Settings updated in memory");

                // Save to file
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"Settings saved to file and cache");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save OBS connection index to file and cache.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="obsConnection">OBS connection index</param>
        /// <returns>True if save successful</returns>
        public bool SaveObsConnectionV2(string productNumber, int obsConnection)
        {
            try
            {
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
                LogDebug($"OBS connection updated in memory");

                // Save to file
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"OBS connection saved to file and cache");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error saving OBS connection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save canvas scale factor to file and cache.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="scaleFactor">Canvas scale factor</param>
        /// <returns>True if save successful</returns>
        public bool SaveScaleFactorV2(string productNumber, double scaleFactor)
        {
            try
            {
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

                // Update scaleFactor
                data["scaleFactor"] = scaleFactor;
                LogDebug($"Scale factor updated in memory");

                // Save to file
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"Scale factor saved to file and cache");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error saving scale factor: {ex.Message}");
                return false;
            }
        }
    }
}
