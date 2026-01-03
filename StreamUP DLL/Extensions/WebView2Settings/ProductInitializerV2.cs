using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Initialize a product by verifying its settings file exists with proper structure.
        /// The settings file MUST be created by running the settings action first.
        /// </summary>
        /// <param name="productNumber">Product identifier (e.g., "sup001")</param>
        /// <param name="errorMessage">Out parameter with error description if initialization fails</param>
        /// <returns>True if product settings file exists and is valid</returns>
        public bool InitializeProductV2(string productNumber, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate input
            if (string.IsNullOrEmpty(productNumber))
            {
                errorMessage = "Product number cannot be null or empty";
                LogError(errorMessage);
                return false;
            }

            try
            {
                // Get StreamerBot directory
                if (!GetStreamerBotDirectory(out string directory))
                {
                    errorMessage = "Failed to access StreamerBot directory";
                    LogError(errorMessage);
                    return false;
                }

                string filePath = Path.Combine(directory, $"{productNumber}_Data.json");

                // Check if settings file exists
                if (!File.Exists(filePath))
                {
                    errorMessage =
                        $"Settings file not found for product {productNumber}. "
                        + $"Please run the settings action for this product and save to create the settings file.";
                    LogError(errorMessage);
                    return false;
                }

                // Verify file structure is valid
                JObject data = LoadProductDataV2(productNumber);
                if (data == null || !ValidateProductDataStructure(data))
                {
                    errorMessage = "Product settings file exists but has invalid structure";
                    LogError(errorMessage);
                    return false;
                }

                LogDebug(
                    $"Product {productNumber} initialized successfully with valid settings file"
                );
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Exception during product initialization: {ex.Message}";
                LogError(errorMessage);
                return false;
            }
        }

        /// <summary>
        /// Check if a product is already initialized (file exists with valid structure).
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product data file exists with valid structure</returns>
        public bool IsProductInitializedV2(string productNumber)
        {
            try
            {
                if (!GetStreamerBotDirectory(out string directory))
                {
                    return false;
                }

                string filePath = Path.Combine(directory, $"{productNumber}_Data.json");

                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Verify structure is valid
                JObject data = LoadProductDataV2(productNumber);
                return data != null && ValidateProductDataStructure(data);
            }
            catch (Exception ex)
            {
                LogError($"Error checking if product is initialized: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate that product data has required structure.
        /// </summary>
        /// <param name="data">Product data to validate</param>
        /// <returns>True if data has all required sections and valid structure</returns>
        private bool ValidateProductDataStructure(JObject data)
        {
            try
            {
                if (data == null)
                {
                    return false;
                }

                // Check required top-level sections
                if (!data.ContainsKey("productInfo") || data["productInfo"] == null)
                {
                    LogError("Product data missing 'productInfo' section");
                    return false;
                }

                if (!data.ContainsKey("obsConnection"))
                {
                    LogError("Product data missing 'obsConnection' section");
                    return false;
                }

                if (!data.ContainsKey("settings") || data["settings"] == null)
                {
                    LogError("Product data missing 'settings' section");
                    return false;
                }

                // Check productInfo has required fields
                JObject productInfo = data["productInfo"] as JObject;
                if (productInfo == null)
                {
                    LogError("'productInfo' is not a JSON object");
                    return false;
                }

                if (!productInfo.ContainsKey("productNumber"))
                {
                    LogError("'productInfo' missing 'productNumber' field");
                    return false;
                }

                // Check settings is an object
                JObject settings = data["settings"] as JObject;
                if (settings == null)
                {
                    LogError("'settings' is not a JSON object");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error validating product data structure: {ex.Message}");
                return false;
            }
        }
    }
}
