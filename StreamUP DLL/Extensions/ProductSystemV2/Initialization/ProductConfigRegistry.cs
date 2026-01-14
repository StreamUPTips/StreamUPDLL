using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StreamUP
{
    /// <summary>
    /// Manages product configuration storage and retrieval.
    /// Allows products to register their config once (in Settings action),
    /// and then retrieve it later (in Main action) without needing to pass it again.
    ///
    /// Configs are persisted to disk so they survive Streamer.bot restarts.
    /// </summary>
    public static class ProductConfigRegistry
    {
        // In-memory cache of registered configs
        private static readonly Dictionary<string, RegisteredProductConfig> _configCache
            = new Dictionary<string, RegisteredProductConfig>(StringComparer.OrdinalIgnoreCase);

        // Lock for thread safety
        private static readonly object _lock = new object();

        // Base folder for config storage
        private static string _dataFolder;

        /// <summary>
        /// Internal class to track config metadata
        /// </summary>
        private class RegisteredProductConfig
        {
            public JObject Config { get; set; }
            public DateTime RegisteredAt { get; set; }
            public DateTime FileTimestamp { get; set; }
        }

        /// <summary>
        /// Initialize the registry with the Streamer.bot data folder path
        /// </summary>
        /// <param name="streamerBotFolder">Path to Streamer.bot folder</param>
        public static void Initialize(string streamerBotFolder)
        {
            _dataFolder = Path.Combine(streamerBotFolder, "StreamUP", "ProductConfigs");
            Directory.CreateDirectory(_dataFolder);
        }

        /// <summary>
        /// Register a product's configuration for later use.
        /// Call this from your Settings action when OpenSettingsMenu is called.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="config">The product configuration JSON from Settings Builder</param>
        /// <returns>True if registered successfully</returns>
        public static bool RegisterProduct(string productNumber, JObject config)
        {
            if (string.IsNullOrEmpty(productNumber) || config == null)
            {
                return false;
            }

            lock (_lock)
            {
                try
                {
                    // Store in memory cache
                    _configCache[productNumber] = new RegisteredProductConfig
                    {
                        Config = config.DeepClone() as JObject,
                        RegisteredAt = DateTime.UtcNow,
                        FileTimestamp = DateTime.UtcNow
                    };

                    // Persist to disk
                    if (!string.IsNullOrEmpty(_dataFolder))
                    {
                        var filePath = GetConfigFilePath(productNumber);
                        var json = config.ToString(Formatting.Indented);
                        File.WriteAllText(filePath, json, Encoding.UTF8);
                        _configCache[productNumber].FileTimestamp = File.GetLastWriteTimeUtc(filePath);
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if a product has been registered (config is available)
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product config is available</returns>
        public static bool IsProductRegistered(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
            {
                return false;
            }

            lock (_lock)
            {
                // Check memory first
                if (_configCache.ContainsKey(productNumber))
                {
                    return true;
                }

                // Check disk
                if (!string.IsNullOrEmpty(_dataFolder))
                {
                    var filePath = GetConfigFilePath(productNumber);
                    return File.Exists(filePath);
                }

                return false;
            }
        }

        /// <summary>
        /// Get a product's registered configuration
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>The product config JObject, or null if not registered</returns>
        public static JObject GetProductConfig(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
            {
                return null;
            }

            lock (_lock)
            {
                // Check memory cache first
                if (_configCache.TryGetValue(productNumber, out var cached))
                {
                    // Validate cache against file timestamp if possible
                    if (!string.IsNullOrEmpty(_dataFolder))
                    {
                        var filePath = GetConfigFilePath(productNumber);
                        if (File.Exists(filePath))
                        {
                            var fileTime = File.GetLastWriteTimeUtc(filePath);
                            if (fileTime == cached.FileTimestamp)
                            {
                                return cached.Config?.DeepClone() as JObject;
                            }
                            // File was modified externally, reload
                        }
                    }
                    else
                    {
                        return cached.Config?.DeepClone() as JObject;
                    }
                }

                // Load from disk
                if (!string.IsNullOrEmpty(_dataFolder))
                {
                    var filePath = GetConfigFilePath(productNumber);
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            var json = File.ReadAllText(filePath, Encoding.UTF8);
                            var config = JObject.Parse(json);

                            // Update memory cache
                            _configCache[productNumber] = new RegisteredProductConfig
                            {
                                Config = config.DeepClone() as JObject,
                                RegisteredAt = DateTime.UtcNow,
                                FileTimestamp = File.GetLastWriteTimeUtc(filePath)
                            };

                            return config;
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Unregister a product (remove from cache and disk)
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void UnregisterProduct(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
            {
                return;
            }

            lock (_lock)
            {
                _configCache.Remove(productNumber);

                if (!string.IsNullOrEmpty(_dataFolder))
                {
                    var filePath = GetConfigFilePath(productNumber);
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Clear all registered products from memory cache
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _configCache.Clear();
            }
        }

        /// <summary>
        /// Get specific config value from a registered product
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="key">Config key</param>
        /// <param name="defaultValue">Default if not found</param>
        /// <returns>The config value or default</returns>
        public static T GetConfigValue<T>(string productNumber, string key, T defaultValue)
        {
            var config = GetProductConfig(productNumber);
            if (config == null || !config.ContainsKey(key))
            {
                return defaultValue;
            }

            try
            {
                return config[key].ToObject<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get the file path for a product's config file
        /// </summary>
        private static string GetConfigFilePath(string productNumber)
        {
            return Path.Combine(_dataFolder, $"{productNumber}_Config.json");
        }
    }
}
