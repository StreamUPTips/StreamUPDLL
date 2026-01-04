using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamUP
{
    /// <summary>
    /// Result of product initialization prompt check.
    /// </summary>
    public enum InitializationPromptResult
    {
        /// <summary>Product is already initialized</summary>
        AlreadyInitialized,

        /// <summary>Product not initialized, user clicked Yes to run settings</summary>
        NotInitializedUserAccepted,

        /// <summary>Product not initialized, user clicked No or already prompted this session</summary>
        NotInitializedUserDeclined
    }

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

        /// <summary>
        /// Check if product is initialized, and if not, prompt user to run settings.
        /// Shows MessageBox only once per session (until Streamer.bot restart).
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>InitializationPromptResult indicating the status and user response</returns>
        public InitializationPromptResult CheckAndPromptProductInitializationV2(string productNumber)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogError("Product number cannot be null or empty");
                    return InitializationPromptResult.NotInitializedUserDeclined;
                }

                // Check if already initialized
                if (IsProductInitializedV2(productNumber))
                {
                    LogDebug($"Product {productNumber} is already initialized");
                    return InitializationPromptResult.AlreadyInitialized;
                }

                // Check if already prompted this session
                if (!ProductInitializationTracker.ShouldShowInitializationPrompt(productNumber))
                {
                    LogDebug($"Product {productNumber} already prompted this session, skipping");
                    return InitializationPromptResult.NotInitializedUserDeclined;
                }

                // Mark as prompted (do this early to prevent double-prompting)
                ProductInitializationTracker.MarkInitializationPromptShown(productNumber);

                // Try to get product name for the prompt
                string productName = productNumber;
                JObject productInfo = LoadProductInfoV2(productNumber, useCache: false);
                if (productInfo != null)
                {
                    string loadedName = productInfo["productName"]?.ToString();
                    if (!string.IsNullOrEmpty(loadedName))
                    {
                        productName = loadedName;
                    }
                }

                LogInfo($"Product {productNumber} not initialized, showing user prompt");

                // Show MessageBox prompt
                string message = $"The {productName} settings have not been configured yet.\n\n" +
                                 "Would you like to open the settings menu now to configure this product?\n\n" +
                                 "Click 'Yes' to open settings, or 'No' to skip.";

                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    message,
                    $"StreamUP - {productName} Not Initialized",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question
                );

                // Handle user response
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    LogInfo($"User accepted settings prompt for {productNumber}");
                    return InitializationPromptResult.NotInitializedUserAccepted;
                }
                else
                {
                    LogInfo($"User declined settings prompt for {productNumber}");
                    return InitializationPromptResult.NotInitializedUserDeclined;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in CheckAndPromptProductInitializationV2: {ex.Message}");
                return InitializationPromptResult.NotInitializedUserDeclined;
            }
        }

        /// <summary>
        /// Validate OBS connection for an initialized product.
        /// Shows warning if OBS not connected (once per session).
        /// Does NOT block initialization - just warns user.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if OBS connected, false if not connected (but doesn't block)</returns>
        public bool ValidateObsConnectionV2(string productNumber)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogDebug("Product number cannot be null or empty for OBS validation");
                    return false;
                }

                // Load product data
                JObject productData = LoadProductDataV2(productNumber);
                if (productData == null)
                {
                    LogDebug($"Cannot validate OBS connection - product data not found for {productNumber}");
                    return false;
                }

                // Extract OBS connection number
                int? obsConnectionValue = (int?)productData["obsConnection"];
                if (obsConnectionValue == null || obsConnectionValue < 0)
                {
                    LogDebug($"Product {productNumber} does not use OBS connection (value: {obsConnectionValue})");
                    return true; // Not an error, just doesn't use OBS
                }

                int obsConnection = obsConnectionValue.Value;

                // Check if OBS is connected
                bool isConnected = _CPH.ObsIsConnected(obsConnection);

                if (isConnected)
                {
                    LogDebug($"OBS is connected on connection {obsConnection} for product {productNumber}");
                    return true;
                }

                // OBS not connected - show warning only once per session
                if (!ProductInitializationTracker.ShouldShowObsWarning(productNumber))
                {
                    LogDebug($"OBS warning already shown this session for {productNumber}");
                    return false;
                }

                // Mark as warned
                ProductInitializationTracker.MarkObsWarningShown(productNumber);

                LogInfo($"OBS not connected on connection {obsConnection} for product {productNumber} - showing warning");

                // Get product name for the warning
                string productName = productNumber;
                JObject productInfo = productData["productInfo"] as JObject;
                if (productInfo != null)
                {
                    string loadedName = productInfo["productName"]?.ToString();
                    if (!string.IsNullOrEmpty(loadedName))
                    {
                        productName = loadedName;
                    }
                }

                // Show warning MessageBox
                string message = $"{productName} requires OBS to be connected on connection #{obsConnection}.\n\n" +
                                 "Currently, OBS is not connected on this connection.\n\n" +
                                 "To resolve:\n" +
                                 "1. Open Streamer.bot Settings → Stream Apps tab\n" +
                                 "2. Ensure OBS is connected via WebSocket 5.0+\n" +
                                 "3. Verify it's using connection #{obsConnection}\n\n" +
                                 "The product will continue to load, but may not function correctly.";

                System.Windows.Forms.MessageBox.Show(
                    message,
                    "StreamUP - OBS Connection Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning
                );

                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error in ValidateObsConnectionV2: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate product versions: OBS scene version, scene name existence, and library version.
        /// Performs checks based on product type and shows warnings only once per session.
        /// For OBS products: checks if scene exists in OBS and if sceneName has correct version.
        /// For all products: checks libraryVersion against current StreamUP library version.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if all validations pass, false if any validation fails</returns>
        public bool ValidateProductVersionsV2(string productNumber)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(productNumber))
                {
                    LogDebug("Product number cannot be null or empty for version validation");
                    return false;
                }

                // Load product data
                JObject productData = LoadProductDataV2(productNumber);
                if (productData == null)
                {
                    LogDebug($"Cannot validate versions - product data not found for {productNumber}");
                    return false;
                }

                // Check if already warned this session
                if (!ProductInitializationTracker.ShouldShowVersionWarning(productNumber))
                {
                    LogDebug($"Version warning already shown this session for {productNumber}");
                    return true; // Already warned, don't warn again
                }

                string productName = productNumber;
                JObject productInfo = productData["productInfo"] as JObject;
                if (productInfo != null)
                {
                    string loadedName = productInfo["productName"]?.ToString();
                    if (!string.IsNullOrEmpty(loadedName))
                    {
                        productName = loadedName;
                    }
                }

                // Check library version first (all products)
                string libraryVersionStr = productInfo?["libraryVersion"]?.ToString();
                if (!string.IsNullOrEmpty(libraryVersionStr) && libraryVersionStr != "0.0.0.0")
                {
                    if (Version.TryParse(libraryVersionStr, out Version requiredLibVersion))
                    {
                        Version currentLibVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        if (currentLibVersion < requiredLibVersion)
                        {
                            ProductInitializationTracker.MarkVersionWarningShown(productNumber);

                            LogError($"LibraryVersion mismatch for {productNumber}: Required {requiredLibVersion}, Current {currentLibVersion}");

                            string message = $"The StreamUP library is out of date for {productName}.\n\n" +
                                           $"Required Version: {requiredLibVersion}\n" +
                                           $"Current Version: {currentLibVersion}\n\n" +
                                           $"Please update your StreamUP library using StreamUP_Library_Updater.exe";

                            System.Windows.Forms.MessageBox.Show(
                                message,
                                "StreamUP - Library Version Warning",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Warning
                            );

                            return false;
                        }

                        LogDebug($"Library version OK for {productNumber}: {currentLibVersion}");
                    }
                }

                // Extract OBS connection number for OBS-specific checks
                int? obsConnectionValue = (int?)productData["obsConnection"];
                string productType = productInfo?["productType"]?.ToString() ?? "Unknown";

                // OBS-specific checks
                if (productType.Equals("OBS", StringComparison.OrdinalIgnoreCase) && obsConnectionValue != null && obsConnectionValue >= 0)
                {
                    int obsConnection = obsConnectionValue.Value;

                    // Check if OBS is connected
                    if (!_CPH.ObsIsConnected(obsConnection))
                    {
                        LogDebug($"Cannot validate OBS scenes - OBS not connected for {productNumber}");
                        return true; // Don't fail validation if OBS not connected, just skip scene checks
                    }

                    // Check if scene exists
                    string sceneName = productInfo?["sceneName"]?.ToString();
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        if (!CheckObsSceneExists(sceneName, obsConnection))
                        {
                            ProductInitializationTracker.MarkVersionWarningShown(productNumber);

                            LogError($"OBS scene '{sceneName}' not found for {productNumber}");

                            string message = $"The OBS scene '{sceneName}' required by {productName} was not found.\n\n" +
                                           $"Please ensure the scene exists in OBS on connection #{obsConnection}.\n\n" +
                                           $"The product may not function correctly without this scene.";

                            System.Windows.Forms.MessageBox.Show(
                                message,
                                "StreamUP - OBS Scene Warning",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Warning
                            );

                            return false;
                        }

                        LogDebug($"OBS scene '{sceneName}' exists for {productNumber}");
                    }

                    // Check scene version
                    string sourceName = productInfo?["sourceName"]?.ToString();
                    string sceneVersionStr = productInfo?["sceneVersion"]?.ToString();

                    if (!string.IsNullOrEmpty(sourceName) && !string.IsNullOrEmpty(sceneVersionStr) && sceneVersionStr != "0.0.0.0")
                    {
                        if (Version.TryParse(sceneVersionStr, out Version requiredSceneVersion))
                        {
                            if (GetObsSourceVersion(sourceName, obsConnection, out Version currentSceneVersion))
                            {
                                if (currentSceneVersion < requiredSceneVersion)
                                {
                                    ProductInitializationTracker.MarkVersionWarningShown(productNumber);

                                    LogError($"Scene version mismatch for {productNumber}: Required {requiredSceneVersion}, Current {currentSceneVersion}");

                                    string message = $"The OBS scene '{sourceName}' version is out of date for {productName}.\n\n" +
                                                   $"Required Version: {requiredSceneVersion}\n" +
                                                   $"Current Version: {currentSceneVersion}\n\n" +
                                                   $"Please update the scene from https://my.streamup.tips";

                                    System.Windows.Forms.MessageBox.Show(
                                        message,
                                        "StreamUP - Scene Version Warning",
                                        System.Windows.Forms.MessageBoxButtons.OK,
                                        System.Windows.Forms.MessageBoxIcon.Warning
                                    );

                                    return false;
                                }

                                LogDebug($"Scene version OK for {productNumber}: {currentSceneVersion}");
                            }
                            else
                            {
                                LogDebug($"Could not determine scene version for source '{sourceName}' in {productNumber}");
                            }
                        }
                    }
                }

                // All checks passed
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ValidateProductVersionsV2: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if an OBS scene exists.
        /// </summary>
        /// <param name="sceneName">Name of the scene to check</param>
        /// <param name="obsConnection">OBS connection number</param>
        /// <returns>True if scene exists, false otherwise</returns>
        private bool CheckObsSceneExists(string sceneName, int obsConnection)
        {
            try
            {
                // Retrieve the list of scenes from OBS
                var sceneListResponse = _CPH.ObsSendRaw("GetSceneList", "{}", obsConnection);

                if (string.IsNullOrWhiteSpace(sceneListResponse))
                {
                    LogError("Failed to retrieve scene list from OBS");
                    return false;
                }

                // Parse the JSON response
                var jsonResponse = JObject.Parse(sceneListResponse);
                var scenes = jsonResponse["scenes"]?.ToObject<List<JObject>>();

                if (scenes == null)
                {
                    LogError("Scene list is empty or malformed");
                    return false;
                }

                // Check if the scene exists
                bool sceneExists = scenes.Any(scene => scene["sceneName"]?.ToString() == sceneName);

                if (!sceneExists)
                {
                    LogDebug($"Scene '{sceneName}' not found in OBS scene list");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error checking if scene exists: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the version of an OBS source from its 'sceneVersion' setting.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="obsConnection">OBS connection number</param>
        /// <param name="sourceVersion">Out parameter with the source version</param>
        /// <returns>True if version was found and parsed, false otherwise</returns>
        private bool GetObsSourceVersion(string sourceName, int obsConnection, out Version sourceVersion)
        {
            sourceVersion = null;

            try
            {
                // Get source settings
                if (!GetObsSourceSettings(sourceName, obsConnection, out JObject sourceSettings))
                {
                    LogDebug($"Failed to retrieve settings for source '{sourceName}'");
                    return false;
                }

                // Check if 'sceneVersion' exists in settings
                if (sourceSettings.TryGetValue("sceneVersion", out JToken versionToken))
                {
                    string versionStr = versionToken.ToString();
                    if (Version.TryParse(versionStr, out Version version))
                    {
                        sourceVersion = version;
                        LogDebug($"Found sceneVersion for source '{sourceName}': {version}");
                        return true;
                    }
                }

                LogDebug($"No 'sceneVersion' found in source settings for '{sourceName}'");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error getting source version: {ex.Message}");
                return false;
            }
        }
    }
}
