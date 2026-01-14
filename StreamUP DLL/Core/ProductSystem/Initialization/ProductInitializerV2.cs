using Newtonsoft.Json.Linq;
using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Initialize a product using the V2 WebView2-based settings system.
        /// This validates settings exist, library version, and OBS connection (for OBS products).
        /// Uses modern dialogs and prevents prompt spam.
        /// </summary>
        /// <param name="settingsConfig">The settings configuration JSON from Settings Builder export</param>
        /// <returns>InitializationResult indicating success or type of failure</returns>
        public InitializationResult InitializeProductV2(JObject settingsConfig)
        {
            string productNumber = _ProductIdentifier;

            LogInfo($"[V2] Initializing product: {productNumber}");

            // Extract product info from config
            string productName = settingsConfig["productName"]?.ToString() ?? productNumber;
            string productType = settingsConfig["productType"]?.ToString() ?? "Streamer.Bot Only";
            string libraryVersion = settingsConfig["libraryVersion"]?.ToString() ?? "0.0.0.0";
            string settingsAction = settingsConfig["settingsAction"]?.ToString();
            bool isObsProduct = productType.Equals("OBS", StringComparison.OrdinalIgnoreCase);

            // OBS-specific config
            string sceneName = settingsConfig["sceneName"]?.ToString();
            string sourceName = settingsConfig["sourceName"]?.ToString();
            string sceneVersion = settingsConfig["sceneVersion"]?.ToString();

            // STEP 1: Check library version
            if (!ValidateLibraryVersionV2(libraryVersion))
            {
                return HandleLibraryOutOfDate(productNumber, productName, libraryVersion);
            }

            // STEP 2: Check settings exist
            if (!ProductSettingsExist())
            {
                return HandleSettingsNotConfigured(productNumber, productName, settingsAction);
            }

            // STEP 3: For OBS products, validate OBS connection and scene
            if (isObsProduct)
            {
                // Get the OBS connection from saved settings
                int obsConnection = GetProductObsConnection();

                // Check OBS is connected
                if (!IsObsConnectedV2(obsConnection))
                {
                    return HandleObsNotConnected(productNumber, productName, obsConnection);
                }

                // Check scene exists (if specified)
                if (!string.IsNullOrEmpty(sceneName))
                {
                    if (!ObsSceneExistsV2(obsConnection, sceneName))
                    {
                        return HandleSceneNotFound(productNumber, productName, sceneName);
                    }

                    // Check scene version (if source name specified)
                    if (!string.IsNullOrEmpty(sourceName) && !string.IsNullOrEmpty(sceneVersion))
                    {
                        if (!ValidateObsSceneVersionV2(obsConnection, sourceName, sceneVersion, out Version installedVersion))
                        {
                            return HandleSceneOutdated(productNumber, productName, sceneName, sceneVersion, installedVersion);
                        }
                    }
                }
            }

            // All checks passed!
            LogInfo($"[V2] Product '{productName}' initialized successfully");

            // Clear any previous prompts for this product (they resolved their issues)
            PromptManager.ClearPrompts(productNumber);

            return InitializationResult.Success;
        }

        /// <summary>
        /// Initialize a product using the V2 system with a JSON string
        /// </summary>
        public InitializationResult InitializeProductV2(string settingsConfigJson)
        {
            try
            {
                var config = JObject.Parse(settingsConfigJson);
                return InitializeProductV2(config);
            }
            catch (Exception ex)
            {
                LogError($"[V2] Failed to parse settings config JSON: {ex.Message}");
                return InitializationResult.SettingsNotConfigured;
            }
        }

        #region Failure Handlers

        private InitializationResult HandleLibraryOutOfDate(string productNumber, string productName, string requiredVersion)
        {
            // Check if already prompted
            if (PromptManager.HasBeenPrompted(productNumber, PromptManager.IssueKeys.LibraryOutdated))
            {
                LogDebug($"[V2] Already prompted about library version for '{productName}'");
                return InitializationResult.AlreadyPromptedThisSession;
            }

            Version currentVersion = GetCurrentLibraryVersion();

            string message = $"This product requires StreamUP Library v{requiredVersion} or higher.\n\n" +
                           $"Your current version: v{currentVersion}\n\n" +
                           "Please update the StreamUP DLL using the StreamUP Library Updater.";

            ModernDialog.ShowError("Update Required", message, productName);

            PromptManager.MarkAsPrompted(productNumber, PromptManager.IssueKeys.LibraryOutdated);
            return InitializationResult.LibraryOutOfDate;
        }

        private InitializationResult HandleSettingsNotConfigured(string productNumber, string productName, string settingsAction)
        {
            // Check if already prompted
            if (PromptManager.HasBeenPrompted(productNumber, PromptManager.IssueKeys.SettingsMissing))
            {
                LogDebug($"[V2] Already prompted about settings for '{productName}'");
                return InitializationResult.AlreadyPromptedThisSession;
            }

            string message = "This product needs to be configured before first use.\n\n" +
                           "Would you like to open settings now?";

            var result = ModernDialog.ShowPrompt("Configuration Required", message, productName, "Yes, Open Settings", "No");

            if (result.Accepted && !string.IsNullOrEmpty(settingsAction))
            {
                LogInfo($"[V2] User accepted, running settings action: {settingsAction}");
                _CPH.RunAction(settingsAction, false);
            }

            PromptManager.MarkAsPrompted(productNumber, PromptManager.IssueKeys.SettingsMissing);
            return InitializationResult.SettingsNotConfigured;
        }

        private InitializationResult HandleObsNotConnected(string productNumber, string productName, int obsConnection)
        {
            // Check if already prompted
            if (PromptManager.HasBeenPrompted(productNumber, PromptManager.IssueKeys.ObsDisconnected))
            {
                LogDebug($"[V2] Already prompted about OBS connection for '{productName}'");
                return InitializationResult.AlreadyPromptedThisSession;
            }

            string message = $"OBS connection {obsConnection} is not connected.\n\n" +
                           "Please connect OBS in Streamer.bot:\n" +
                           $"Stream Apps → OBS → Connection {obsConnection}";

            ModernDialog.ShowWarning("OBS Not Connected", message, productName);

            PromptManager.MarkAsPrompted(productNumber, PromptManager.IssueKeys.ObsDisconnected);
            return InitializationResult.ObsNotConnected;
        }

        private InitializationResult HandleSceneNotFound(string productNumber, string productName, string sceneName)
        {
            // Check if already prompted
            if (PromptManager.HasBeenPrompted(productNumber, PromptManager.IssueKeys.ObsSceneMissing))
            {
                LogDebug($"[V2] Already prompted about missing scene for '{productName}'");
                return InitializationResult.AlreadyPromptedThisSession;
            }

            string message = $"The scene \"{sceneName}\" was not found in OBS.\n\n" +
                           "Please install the product's .StreamUP file into OBS via the StreamUP OBS plugin.";

            ModernDialog.ShowWarning("Scene Not Found", message, productName);

            PromptManager.MarkAsPrompted(productNumber, PromptManager.IssueKeys.ObsSceneMissing);
            return InitializationResult.ObsSceneNotFound;
        }

        private InitializationResult HandleSceneOutdated(string productNumber, string productName, string sceneName, string expectedVersion, Version installedVersion)
        {
            // Check if already prompted
            if (PromptManager.HasBeenPrompted(productNumber, PromptManager.IssueKeys.ObsSceneOutdated))
            {
                LogDebug($"[V2] Already prompted about outdated scene for '{productName}'");
                return InitializationResult.AlreadyPromptedThisSession;
            }

            string installedStr = installedVersion?.ToString() ?? "unknown";
            string message = $"The scene \"{sceneName}\" in OBS is outdated.\n\n" +
                           $"Installed version: v{installedStr}\n" +
                           $"Required version: v{expectedVersion}\n\n" +
                           "Please download and install the latest .StreamUP file from https://my.streamup.tips";

            ModernDialog.ShowWarning("Scene Outdated", message, productName);

            PromptManager.MarkAsPrompted(productNumber, PromptManager.IssueKeys.ObsSceneOutdated);
            return InitializationResult.ObsSceneOutdated;
        }

        #endregion
    }
}
