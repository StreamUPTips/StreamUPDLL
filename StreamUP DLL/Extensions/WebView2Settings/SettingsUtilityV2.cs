using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// High-level flow: Load product settings from file and apply via callback.
        /// Handles initialization, loading, section extraction, and callback invocation.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="applySettings">Callback that receives loaded settings and returns true on success</param>
        /// <returns>True if successful</returns>
        public bool LoadAndApplyProductSettingsV2(
            string productNumber,
            Func<JObject, bool> applySettings
        )
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
                int obsConnection = 0;
                if (settingsObj["ObsConnection"] == null)
                {
                    obsConnection = (int?)productData["obsConnection"] ?? 0;
                    settingsObj["ObsConnection"] = obsConnection;
                }

                LogInfo($"Loaded settings for product: {productName}");

                // 6. Call the product-specific callback to apply settings
                if (!applySettings(settingsObj))
                {
                    LogError("Failed to apply product settings");
                    return false;
                }

                // 7. Set Canvas Scaling
                if (GetObsCanvasScaleFactor(obsConnection, out double scaleFactor))
                {
                    // Save scale factor to product data
                    productData["scaleFactor"] = scaleFactor;
                    if (!SaveProductDataV2(productNumber, productData))
                    {
                        LogError("Failed to save scale factor to product data");
                        return false;
                    }
                }
                else
                {
                    LogError("Failed to retrieve canvas scale factor");
                    return false;
                }

                LogInfo("Successfully loaded and applied product settings");
                ShowToastNotification(
                    StreamUpLib.NotificationType.Success,
                    "StreamUP Settings Loaded",
                    $"{productName} settings were set successfully"
                );
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error loading and applying product settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate that a product's data structure is correct.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product data is valid</returns>
        public bool ValidateProductSettingsV2(string productNumber)
        {
            try
            {
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogDebug($"Product data is null or not found: {productNumber}");
                    return false;
                }

                // Validate required sections
                if (!data.ContainsKey("productInfo") || data["productInfo"] == null)
                {
                    LogError("Product missing 'productInfo' section");
                    return false;
                }

                if (!data.ContainsKey("obsConnection"))
                {
                    LogError("Product missing 'obsConnection' section");
                    return false;
                }

                if (!data.ContainsKey("settings") || data["settings"] == null)
                {
                    LogError("Product missing 'settings' section");
                    return false;
                }

                JObject productInfo = data["productInfo"] as JObject;
                if (productInfo == null || !productInfo.ContainsKey("productNumber"))
                {
                    LogError("ProductInfo missing 'productNumber' field");
                    return false;
                }

                LogInfo($"Product {productNumber} validation successful");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error validating product {productNumber}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all product files in the data directory.
        /// </summary>
        /// <returns>List of product numbers, or empty list if error</returns>
        public List<string> GetAllProductsV2()
        {
            try
            {
                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return new List<string>();
                }

                if (!Directory.Exists(directory))
                {
                    LogDebug("Product data directory does not exist");
                    return new List<string>();
                }

                var files = Directory.GetFiles(directory, "*_Data.json");
                var products = files
                    .Select(f => Path.GetFileName(f))
                    .Select(f => f.Replace("_Data.json", ""))
                    .OrderBy(p => p)
                    .ToList();

                LogInfo($"Found {products.Count} products");
                return products;
            }
            catch (Exception ex)
            {
                LogError($"Error getting all products: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get product name for all products.
        /// </summary>
        /// <returns>Dictionary of productNumber → productName</returns>
        public Dictionary<string, string> GetAllProductNamesV2()
        {
            try
            {
                var result = new Dictionary<string, string>();
                var products = GetAllProductsV2();

                foreach (var productNumber in products)
                {
                    var productInfo = LoadProductInfoV2(productNumber);
                    if (productInfo != null)
                    {
                        string productName = productInfo["productName"]?.ToString() ?? "Unknown";
                        result[productNumber] = productName;
                    }
                }

                LogDebug($"Retrieved names for {result.Count} products");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Error getting product names: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Load all products into memory.
        /// CAUTION: Can be memory intensive with many products.
        /// </summary>
        /// <param name="useCache">Use cache if available (default: true)</param>
        /// <returns>Dictionary of productNumber → product data</returns>
        public Dictionary<string, JObject> LoadAllProductsV2(bool useCache = true)
        {
            try
            {
                var result = new Dictionary<string, JObject>();
                var products = GetAllProductsV2();

                foreach (var productNumber in products)
                {
                    var data = LoadProductDataV2(productNumber, useCache);
                    if (data != null)
                    {
                        result[productNumber] = data;
                    }
                }

                LogInfo($"Loaded {result.Count} products into memory");
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Error loading all products: {ex.Message}");
                return new Dictionary<string, JObject>();
            }
        }

        /// <summary>
        /// Create a backup copy of a product's data file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="backupPath">Path where backup file should be saved</param>
        /// <returns>True if backup successful</returns>
        public bool BackupProductV2(string productNumber, string backupPath)
        {
            try
            {
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (string.IsNullOrEmpty(backupPath))
                {
                    LogError("Backup path is null or empty");
                    return false;
                }

                // Load product data
                JObject data = LoadProductDataV2(productNumber);
                if (data == null)
                {
                    LogError($"Failed to load product {productNumber} for backup");
                    return false;
                }

                // Ensure backup directory exists
                string backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Write backup file
                string jsonOutput = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(backupPath, jsonOutput);

                LogInfo($"Product {productNumber} backed up to {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error backing up product {productNumber}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Restore a product from a backup file.
        /// </summary>
        /// <param name="backupPath">Path to backup file</param>
        /// <param name="productNumber">Product identifier to restore to</param>
        /// <returns>True if restore successful</returns>
        public bool RestoreProductV2(string backupPath, string productNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(backupPath))
                {
                    LogError("Backup path is null or empty");
                    return false;
                }

                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (!File.Exists(backupPath))
                {
                    LogError($"Backup file not found: {backupPath}");
                    return false;
                }

                // Read backup file
                string jsonData = File.ReadAllText(backupPath);
                JObject data = JObject.Parse(jsonData);

                // Save to product
                bool saved = SaveProductDataV2(productNumber, data);
                if (saved)
                {
                    LogInfo($"Product {productNumber} restored from {backupPath}");
                }
                return saved;
            }
            catch (Exception ex)
            {
                LogError($"Error restoring product from {backupPath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Export product data to a JSON file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="filePath">Path where export file should be saved</param>
        /// <returns>True if export successful</returns>
        public bool ExportProductV2(string productNumber, string filePath)
        {
            return BackupProductV2(productNumber, filePath);
        }

        /// <summary>
        /// Import product data from a JSON file.
        /// </summary>
        /// <param name="filePath">Path to import file</param>
        /// <param name="productNumber">Product identifier to import as</param>
        /// <returns>True if import successful</returns>
        public bool ImportProductV2(string filePath, string productNumber)
        {
            return RestoreProductV2(filePath, productNumber);
        }

        /// <summary>
        /// Delete a product's data file completely.
        /// CAUTION: This is destructive and cannot be undone.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if deletion successful</returns>
        public bool DeleteProductV2(string productNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number is null or empty");
                    return false;
                }

                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return false;
                }

                string filePath = Path.Combine(directory, $"{productNumber}_Data.json");

                if (!File.Exists(filePath))
                {
                    LogDebug($"Product file does not exist: {filePath}");
                    return true; // Not an error if already deleted
                }

                File.Delete(filePath);
                InvalidateCacheV2(productNumber);

                LogInfo($"Product {productNumber} deleted permanently");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error deleting product {productNumber}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a product exists (file exists and is valid).
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product exists and is valid</returns>
        public bool ProductExistsV2(string productNumber)
        {
            try
            {
                return IsProductInitializedV2(productNumber);
            }
            catch (Exception ex)
            {
                LogError($"Error checking if product exists: {ex.Message}");
                return false;
            }
        }
    }
}
