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
        /// <summary>
        /// Static cache for product settings - persists across all product executions while Streamer.bot runs
        /// </summary>
        private static Dictionary<string, CachedProductSettings> _productSettingsCache
            = new Dictionary<string, CachedProductSettings>();

        /// <summary>
        /// Cache entry for product settings
        /// </summary>
        private class CachedProductSettings
        {
            public JObject Data { get; set; }
            public DateTime FileTimestamp { get; set; }
        }

        /// <summary>
        /// Get the base data path for StreamUP data
        /// </summary>
        private string GetStreamUpDataPath()
        {
            var dataPath = Path.Combine(GetStreamerBotFolder(), "StreamUP", "Data");
            Directory.CreateDirectory(dataPath);
            return dataPath;
        }

        /// <summary>
        /// Get the file path for a product's settings
        /// </summary>
        /// <param name="productNumber">The product identifier</param>
        /// <returns>Full path to the settings JSON file</returns>
        public string GetProductSettingsFilePath(string productNumber)
        {
            return Path.Combine(GetStreamUpDataPath(), $"{productNumber}_Data.json");
        }

        /// <summary>
        /// Load product settings from cache or file (internal use)
        /// </summary>
        private JObject LoadProductSettingsInternal(string productNumber)
        {
            LogInfo($"Loading settings for product: {productNumber}");

            var filePath = GetProductSettingsFilePath(productNumber);

            // Check cache first
            if (_productSettingsCache.TryGetValue(productNumber, out var cached))
            {
                // Validate cache - check if file was modified externally
                if (File.Exists(filePath))
                {
                    var fileTime = File.GetLastWriteTimeUtc(filePath);
                    if (fileTime == cached.FileTimestamp)
                    {
                        LogDebug("Using cached settings (cache valid)");
                        return cached.Data?.DeepClone() as JObject;
                    }
                    LogDebug("Cache invalidated - file was modified externally");
                }
                else
                {
                    // File was deleted, remove from cache
                    _productSettingsCache.Remove(productNumber);
                    LogDebug("Cache invalidated - file was deleted");
                }
            }

            // Load from file
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath, Encoding.UTF8);
                    var data = JObject.Parse(json);

                    // Update cache
                    _productSettingsCache[productNumber] = new CachedProductSettings
                    {
                        Data = data.DeepClone() as JObject,
                        FileTimestamp = File.GetLastWriteTimeUtc(filePath)
                    };

                    LogInfo($"Settings loaded from file: {filePath}");
                    return data;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load settings file: {ex.Message}");
                    return null;
                }
            }

            LogInfo("No saved settings found - will use defaults");
            return null;
        }

        /// <summary>
        /// Save product settings to file and update cache
        /// </summary>
        private bool SaveProductSettingsInternal(string productNumber, JObject data)
        {
            LogInfo($"Saving settings for product: {productNumber}");

            var filePath = GetProductSettingsFilePath(productNumber);

            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Write to file
                var json = data.ToString(Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                // Update cache
                _productSettingsCache[productNumber] = new CachedProductSettings
                {
                    Data = data.DeepClone() as JObject,
                    FileTimestamp = File.GetLastWriteTimeUtc(filePath)
                };

                LogInfo($"Settings saved to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update the settings cache without saving to file
        /// </summary>
        private void UpdateSettingsCacheInternal(string productNumber, JObject data)
        {
            var filePath = GetProductSettingsFilePath(productNumber);

            _productSettingsCache[productNumber] = new CachedProductSettings
            {
                Data = data.DeepClone() as JObject,
                FileTimestamp = File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.UtcNow
            };
        }

        /// <summary>
        /// Clear the cache for a specific product
        /// </summary>
        private void ClearProductCacheInternal(string productNumber)
        {
            if (_productSettingsCache.ContainsKey(productNumber))
            {
                _productSettingsCache.Remove(productNumber);
                LogDebug($"Cache cleared for product: {productNumber}");
            }
        }

        /// <summary>
        /// Clear all cached settings
        /// </summary>
        public void ClearAllSettingsCache()
        {
            _productSettingsCache.Clear();
            LogInfo("All settings cache cleared");
        }

        /// <summary>
        /// Check if settings exist for a product
        /// </summary>
        private bool ProductSettingsExistInternal(string productNumber)
        {
            var filePath = GetProductSettingsFilePath(productNumber);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete settings file for a product
        /// </summary>
        private bool DeleteProductSettingsInternal(string productNumber)
        {
            LogInfo($"Deleting settings for product: {productNumber}");

            var filePath = GetProductSettingsFilePath(productNumber);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LogInfo($"Settings file deleted: {filePath}");
                }

                // Remove from cache
                ClearProductCacheInternal(productNumber);

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create default settings structure for a product
        /// </summary>
        private JObject CreateDefaultSettingsStructure(string productNumber, string productName = null)
        {
            return new JObject
            {
                ["meta"] = new JObject
                {
                    ["productNumber"] = productNumber,
                    ["productName"] = productName ?? productNumber,
                    ["savedAt"] = DateTime.UtcNow.ToString("O"),
                    ["viewerVersion"] = "1.0.0"
                },
                ["settings"] = new JObject(),
                ["productData"] = new JObject
                {
                    ["obsInstanceNumber"] = 0
                }
            };
        }
    }
}
