using System;
using System.Collections.Generic;
using System.IO;

namespace StreamUP
{
    /// <summary>
    /// Caches successful validation results so subsequent Main action runs are instant.
    /// The cache is keyed by product number and includes validation context (OBS connection, etc.)
    /// to ensure we re-validate when conditions change.
    ///
    /// Cache is session-scoped (clears on Streamer.bot restart) to ensure fresh validation
    /// after OBS/SB restarts.
    /// </summary>
    public static class ProductValidationCache
    {
        /// <summary>
        /// Represents a cached validation result
        /// </summary>
        private class ValidationCacheEntry
        {
            public bool IsValid { get; set; }
            public DateTime ValidatedAt { get; set; }
            public string SettingsFileHash { get; set; }
            public int ObsConnection { get; set; }
            public bool WasObsProduct { get; set; }
        }

        // In-memory cache
        private static readonly Dictionary<string, ValidationCacheEntry> _validationCache
            = new Dictionary<string, ValidationCacheEntry>(StringComparer.OrdinalIgnoreCase);

        // Lock for thread safety
        private static readonly object _lock = new object();

        // Data folder for settings files (to check if settings changed)
        private static string _dataFolder;

        /// <summary>
        /// Initialize the validation cache with the data folder path
        /// </summary>
        /// <param name="streamerBotFolder">Path to Streamer.bot folder</param>
        public static void Initialize(string streamerBotFolder)
        {
            _dataFolder = Path.Combine(streamerBotFolder, "StreamUP", "Data");
        }

        /// <summary>
        /// Check if a product has a valid cached validation result.
        /// Returns true if the product was previously validated successfully AND
        /// no conditions have changed (settings file, OBS connection, etc.)
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="obsConnection">Current OBS connection (for OBS products)</param>
        /// <param name="isObsProduct">Whether this is an OBS product</param>
        /// <returns>True if cached validation is still valid</returns>
        public static bool HasValidCache(string productNumber, int obsConnection = 0, bool isObsProduct = false)
        {
            if (string.IsNullOrEmpty(productNumber))
            {
                return false;
            }

            lock (_lock)
            {
                if (!_validationCache.TryGetValue(productNumber, out var entry))
                {
                    return false;
                }

                // Check if the entry is valid
                if (!entry.IsValid)
                {
                    return false;
                }

                // Check if OBS product status changed
                if (entry.WasObsProduct != isObsProduct)
                {
                    return false;
                }

                // For OBS products, check if connection changed
                if (isObsProduct && entry.ObsConnection != obsConnection)
                {
                    return false;
                }

                // Check if settings file was modified
                if (!string.IsNullOrEmpty(_dataFolder))
                {
                    var settingsFilePath = Path.Combine(_dataFolder, $"{productNumber}_Data.json");
                    var currentHash = GetFileHash(settingsFilePath);

                    if (currentHash != entry.SettingsFileHash)
                    {
                        // Settings changed, invalidate cache
                        _validationCache.Remove(productNumber);
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Cache a successful validation result for a product.
        /// Call this after all validation checks pass.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="obsConnection">OBS connection used (for OBS products)</param>
        /// <param name="isObsProduct">Whether this is an OBS product</param>
        public static void CacheValidation(string productNumber, int obsConnection = 0, bool isObsProduct = false)
        {
            if (string.IsNullOrEmpty(productNumber))
            {
                return;
            }

            lock (_lock)
            {
                string settingsHash = null;
                if (!string.IsNullOrEmpty(_dataFolder))
                {
                    var settingsFilePath = Path.Combine(_dataFolder, $"{productNumber}_Data.json");
                    settingsHash = GetFileHash(settingsFilePath);
                }

                _validationCache[productNumber] = new ValidationCacheEntry
                {
                    IsValid = true,
                    ValidatedAt = DateTime.UtcNow,
                    SettingsFileHash = settingsHash,
                    ObsConnection = obsConnection,
                    WasObsProduct = isObsProduct
                };
            }
        }

        /// <summary>
        /// Invalidate the cached validation for a product.
        /// Call this when settings change or user opens settings window.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void InvalidateCache(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
            {
                return;
            }

            lock (_lock)
            {
                _validationCache.Remove(productNumber);
            }
        }

        /// <summary>
        /// Clear all cached validations.
        /// Call this when OBS reconnects or major state changes.
        /// </summary>
        public static void ClearAllCache()
        {
            lock (_lock)
            {
                _validationCache.Clear();
            }
        }

        /// <summary>
        /// Invalidate all OBS product caches.
        /// Call this when OBS connection state changes.
        /// </summary>
        public static void InvalidateObsProductCaches()
        {
            lock (_lock)
            {
                var toRemove = new List<string>();
                foreach (var kvp in _validationCache)
                {
                    if (kvp.Value.WasObsProduct)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in toRemove)
                {
                    _validationCache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Get a simple hash of a file to detect changes.
        /// Uses file size + last write time as a fast hash.
        /// </summary>
        private static string GetFileHash(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var info = new FileInfo(filePath);
                return $"{info.Length}_{info.LastWriteTimeUtc.Ticks}";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if a product has been validated this session (regardless of current validity)
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product was validated at some point this session</returns>
        public static bool WasValidatedThisSession(string productNumber)
        {
            lock (_lock)
            {
                return _validationCache.ContainsKey(productNumber);
            }
        }

        /// <summary>
        /// Get the time when a product was last validated
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>DateTime of last validation, or DateTime.MinValue if never validated</returns>
        public static DateTime GetLastValidationTime(string productNumber)
        {
            lock (_lock)
            {
                if (_validationCache.TryGetValue(productNumber, out var entry))
                {
                    return entry.ValidatedAt;
                }
                return DateTime.MinValue;
            }
        }
    }
}
