using Newtonsoft.Json.Linq;
using System;

namespace StreamUP
{
    /// <summary>
    /// Result of product readiness check
    /// </summary>
    public enum ProductReadyStatus
    {
        /// <summary>Product is ready to use</summary>
        Ready,

        /// <summary>Product settings haven't been configured yet</summary>
        SettingsNotConfigured,

        /// <summary>StreamUP DLL needs to be updated</summary>
        LibraryOutdated,

        /// <summary>OBS is not connected</summary>
        ObsNotConnected,

        /// <summary>Required OBS scene is missing</summary>
        ObsSceneMissing,

        /// <summary>OBS scene is outdated</summary>
        ObsSceneOutdated,

        /// <summary>Product config not registered (run Settings action first)</summary>
        ConfigNotRegistered,

        /// <summary>User was already prompted this session (silent failure)</summary>
        AlreadyPrompted
    }

    public partial class StreamUpLib
    {
        #region Simplified Entry Points

        /// <summary>
        /// SIMPLIFIED: Check if product is ready for the Main Action.
        /// This is the ONE method you need to call in your Main Action.
        ///
        /// What it does:
        /// - Uses cached validation if available (instant return)
        /// - Loads product config (registered when Settings action ran)
        /// - Validates library version
        /// - Checks settings exist
        /// - For OBS products: validates OBS connection and scene
        /// - Caches successful result for future calls
        /// - Shows user-friendly dialogs if issues found
        ///
        /// Example usage in Main Action:
        /// <code>
        /// if (!sup.IsProductReady())
        ///     return;
        ///
        /// // Your main action code here...
        /// </code>
        /// </summary>
        /// <returns>True if product is ready to run</returns>
        public bool IsProductReady()
        {
            var result = CheckProductReady();
            return result == ProductReadyStatus.Ready;
        }

        /// <summary>
        /// SIMPLIFIED: Check if product is ready with detailed status.
        /// Same as IsProductReady() but returns the specific status.
        ///
        /// Example usage:
        /// <code>
        /// var status = sup.CheckProductReady();
        /// if (status != ProductReadyStatus.Ready)
        /// {
        ///     sup.LogInfo($"Product not ready: {status}");
        ///     return;
        /// }
        /// </code>
        /// </summary>
        /// <returns>ProductReadyStatus indicating the result</returns>
        public ProductReadyStatus CheckProductReady()
        {
            string productNumber = _ProductIdentifier;
            LogInfo($"[Simplified] Checking if product is ready: {productNumber}");

            // Try to get cached config first
            var config = ProductConfigRegistry.GetProductConfig(productNumber);
            if (config == null)
            {
                LogDebug($"[Simplified] Product config not found for: {productNumber}");
                LogDebug($"[Simplified] The Settings action needs to run first to register the product.");

                // Check if we can find the config from a previous version
                // For now, return ConfigNotRegistered
                return ProductReadyStatus.ConfigNotRegistered;
            }

            // Extract product info from config
            string productName = config["productName"]?.ToString() ?? productNumber;
            string productType = config["productType"]?.ToString() ?? "Streamer.Bot Only";
            bool isObsProduct = productType.Equals("OBS", StringComparison.OrdinalIgnoreCase);
            int obsConnection = GetProductObsConnection();

            // Check validation cache first (fast path)
            if (ProductValidationCache.HasValidCache(productNumber, obsConnection, isObsProduct))
            {
                LogDebug($"[Simplified] Using cached validation for: {productName}");
                return ProductReadyStatus.Ready;
            }

            // Run full validation
            var initResult = InitializeProductV2(config);

            // Map InitializationResult to ProductReadyStatus
            ProductReadyStatus status;
            switch (initResult)
            {
                case InitializationResult.Success:
                    status = ProductReadyStatus.Ready;
                    // Cache the successful validation
                    ProductValidationCache.CacheValidation(productNumber, obsConnection, isObsProduct);
                    break;

                case InitializationResult.SettingsNotConfigured:
                    status = ProductReadyStatus.SettingsNotConfigured;
                    break;

                case InitializationResult.LibraryOutOfDate:
                    status = ProductReadyStatus.LibraryOutdated;
                    break;

                case InitializationResult.ObsNotConnected:
                    status = ProductReadyStatus.ObsNotConnected;
                    break;

                case InitializationResult.ObsSceneNotFound:
                    status = ProductReadyStatus.ObsSceneMissing;
                    break;

                case InitializationResult.ObsSceneOutdated:
                    status = ProductReadyStatus.ObsSceneOutdated;
                    break;

                case InitializationResult.AlreadyPromptedThisSession:
                    status = ProductReadyStatus.AlreadyPrompted;
                    break;

                default:
                    status = ProductReadyStatus.SettingsNotConfigured;
                    break;
            }

            LogInfo($"[Simplified] Product ready check result: {status}");
            return status;
        }

        /// <summary>
        /// SIMPLIFIED: Prepare for opening settings.
        /// Call this at the start of your Settings action BEFORE OpenSettingsMenu.
        ///
        /// What it does:
        /// - Validates library version
        /// - Shows user-friendly dialogs if issues found
        /// - Returns true if safe to proceed with OpenSettingsMenu
        ///
        /// Example usage in Settings Action:
        /// <code>
        /// if (!sup.IsSettingsReady(config))
        ///     return;
        ///
        /// sup.OpenSettingsMenu(config);
        /// </code>
        /// </summary>
        /// <param name="config">Product configuration from Settings Builder</param>
        /// <returns>True if OK to open settings</returns>
        public bool IsSettingsReady(JObject config)
        {
            if (config == null)
            {
                LogError("[Simplified] Config is null in IsSettingsReady");
                return false;
            }

            string productNumber = config["productNumber"]?.ToString() ?? _ProductIdentifier;
            string productName = config["productName"]?.ToString() ?? productNumber;
            string libraryVersion = config["libraryVersion"]?.ToString() ?? "0.0.0.0";

            LogInfo($"[Simplified] Checking if settings ready for: {productName}");

            // Only check library version for settings
            if (!ValidateLibraryVersionV2(libraryVersion))
            {
                // Check if already prompted
                if (PromptManager.HasBeenPrompted(productNumber, PromptManager.IssueKeys.LibraryOutdated))
                {
                    LogDebug($"[Simplified] Already prompted about library version");
                    return false;
                }

                Version currentVersion = GetCurrentLibraryVersion();

                string message = $"This product requires StreamUP Library v{libraryVersion} or higher.\n\n" +
                               $"Your current version: v{currentVersion}\n\n" +
                               "Please update the StreamUP DLL using the StreamUP Library Updater.";

                ModernDialog.ShowError("Update Required", message, productName);

                PromptManager.MarkAsPrompted(productNumber, PromptManager.IssueKeys.LibraryOutdated);
                return false;
            }

            LogInfo($"[Simplified] Settings ready for: {productName}");
            return true;
        }

        /// <summary>
        /// SIMPLIFIED: Overload accepting JSON string
        /// </summary>
        public bool IsSettingsReady(string configJson)
        {
            try
            {
                var config = JObject.Parse(configJson);
                return IsSettingsReady(config);
            }
            catch (Exception ex)
            {
                LogError($"[Simplified] Failed to parse config JSON: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// SIMPLIFIED: Open settings with automatic registration.
        /// This is the ONE method you need to call in your Settings Action.
        ///
        /// What it does:
        /// - Validates library version first
        /// - Registers the product config for future use
        /// - Opens the settings window
        /// - Invalidates validation cache (user might change settings)
        ///
        /// Example usage in Settings Action:
        /// <code>
        /// sup.OpenProductSettings(config);
        /// </code>
        /// </summary>
        /// <param name="config">Product configuration from Settings Builder</param>
        /// <returns>True if settings window opened successfully</returns>
        public bool OpenProductSettings(JObject config)
        {
            if (config == null)
            {
                LogError("[Simplified] Config is null in OpenProductSettings");
                return false;
            }

            string productNumber = config["productNumber"]?.ToString() ?? _ProductIdentifier;

            // Check if settings are ready (library version)
            if (!IsSettingsReady(config))
            {
                return false;
            }

            // Register the product config for future use by Main action
            ProductConfigRegistry.RegisterProduct(productNumber, config);

            // Invalidate validation cache since user might change settings
            ProductValidationCache.InvalidateCache(productNumber);

            // Open the settings menu
            return OpenSettingsMenu(config);
        }

        /// <summary>
        /// SIMPLIFIED: Overload accepting JSON string
        /// </summary>
        public bool OpenProductSettings(string configJson)
        {
            try
            {
                var config = JObject.Parse(configJson);
                return OpenProductSettings(config);
            }
            catch (Exception ex)
            {
                LogError($"[Simplified] Failed to parse config JSON: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Force re-validation on next IsProductReady() call.
        /// Call this if you know something changed (e.g., user reconnected OBS).
        /// </summary>
        public void InvalidateProductValidation()
        {
            ProductValidationCache.InvalidateCache(_ProductIdentifier);
            LogDebug($"[Simplified] Validation cache invalidated for: {_ProductIdentifier}");
        }

        /// <summary>
        /// Force re-validation for all OBS products.
        /// Call this when OBS connection state changes.
        /// </summary>
        public static void InvalidateAllObsValidations()
        {
            ProductValidationCache.InvalidateObsProductCaches();
        }

        /// <summary>
        /// Check if product has been configured (settings exist).
        /// This is a quick check that doesn't open any dialogs.
        /// </summary>
        /// <returns>True if product has saved settings</returns>
        public bool HasProductBeenConfigured()
        {
            return ProductSettingsExist();
        }

        /// <summary>
        /// Check if product config is registered.
        /// The config is registered when OpenProductSettings() is called.
        /// </summary>
        /// <returns>True if config is available</returns>
        public bool IsProductConfigRegistered()
        {
            return ProductConfigRegistry.IsProductRegistered(_ProductIdentifier);
        }

        /// <summary>
        /// Get product info from registered config.
        /// Returns null if config not registered.
        /// </summary>
        /// <param name="key">Config key (e.g., "productName", "productType")</param>
        /// <returns>Config value or null</returns>
        public string GetProductConfigInfo(string key)
        {
            return ProductConfigRegistry.GetConfigValue(_ProductIdentifier, key, (string)null);
        }

        #endregion
    }
}
