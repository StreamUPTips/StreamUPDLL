using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Static HttpClient for reuse (best practice)
        private static readonly HttpClient _updateHttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private const string PRODUCT_INFO_API_BASE = "https://api.streamup.tips/product/version/";

        /// <summary>
        /// Check if a product update is available.
        /// Non-blocking - always returns true to allow the product to continue running.
        /// Shows a user-friendly dialog if an update is available.
        /// </summary>
        /// <param name="productGuid">The product GUID for the API lookup</param>
        /// <param name="installedVersion">The currently installed version</param>
        /// <param name="productName">Product name for display in dialogs</param>
        /// <param name="productNumber">Product number for tracking dismissed versions</param>
        /// <returns>True always (non-blocking check)</returns>
        public bool CheckProductUpdate(string productGuid, string installedVersion, string productName, string productNumber)
        {
            // Skip if no GUID provided
            if (string.IsNullOrEmpty(productGuid))
            {
                LogDebug("[UpdateCheck] No productGuid provided, skipping update check");
                return true;
            }

            // Skip if no installed version
            if (string.IsNullOrEmpty(installedVersion))
            {
                LogDebug("[UpdateCheck] No installedVersion provided, skipping update check");
                return true;
            }

            // Check if already checked this session
            string sessionKey = $"sup000_UpdateChecked_{productNumber}";
            if (_CPH.GetGlobalVar<bool>(sessionKey, false))
            {
                LogDebug($"[UpdateCheck] Already checked this session for '{productName}'");
                return true;
            }

            // Mark as checked this session
            _CPH.SetGlobalVar(sessionKey, true, false);

            // Run the check asynchronously to not block
            Task.Run(async () =>
            {
                try
                {
                    await CheckProductUpdateAsync(productGuid, installedVersion, productName, productNumber);
                }
                catch (Exception ex)
                {
                    LogDebug($"[UpdateCheck] Error checking for updates: {ex.Message}");
                }
            });

            return true;
        }

        private async Task CheckProductUpdateAsync(string productGuid, string installedVersion, string productName, string productNumber)
        {
            try
            {
                // Fetch latest version from API
                string apiUrl = PRODUCT_INFO_API_BASE + productGuid;
                LogDebug($"[UpdateCheck] Fetching version from: {apiUrl}");

                string latestVersionStr = await _updateHttpClient.GetStringAsync(apiUrl);
                latestVersionStr = latestVersionStr?.Trim();

                if (string.IsNullOrEmpty(latestVersionStr))
                {
                    LogDebug("[UpdateCheck] API returned empty version");
                    return;
                }

                LogDebug($"[UpdateCheck] API returned version: {latestVersionStr}");

                // Parse versions
                if (!TryParseVersion(installedVersion, out Version installed))
                {
                    LogDebug($"[UpdateCheck] Could not parse installed version: {installedVersion}");
                    return;
                }

                if (!TryParseVersion(latestVersionStr, out Version latest))
                {
                    LogDebug($"[UpdateCheck] Could not parse latest version: {latestVersionStr}");
                    return;
                }

                // Compare versions
                if (installed >= latest)
                {
                    LogInfo($"[UpdateCheck] '{productName}' is up to date (v{installed})");
                    return;
                }

                // Update available - check if user dismissed this version
                string dismissedKey = $"sup000_DismissedVersion_{productNumber}";
                string dismissedVersion = _CPH.GetGlobalVar<string>(dismissedKey, true);

                if (!string.IsNullOrEmpty(dismissedVersion) && dismissedVersion == latestVersionStr)
                {
                    LogDebug($"[UpdateCheck] User dismissed v{latestVersionStr} for '{productName}'");
                    return;
                }

                // Show update dialog
                LogInfo($"[UpdateCheck] Update available for '{productName}': v{installed} → v{latest}");
                ShowUpdateAvailableDialog(productName, installed.ToString(), latestVersionStr, productNumber, dismissedKey);
            }
            catch (HttpRequestException ex)
            {
                LogDebug($"[UpdateCheck] Network error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                LogDebug("[UpdateCheck] Request timed out");
            }
            catch (Exception ex)
            {
                LogDebug($"[UpdateCheck] Unexpected error: {ex.Message}");
            }
        }

        private void ShowUpdateAvailableDialog(string productName, string installedVersion, string latestVersion, string productNumber, string dismissedKey)
        {
            // Use ModernDialog for consistent look
            string title = "Update Available";
            string message = $"A new version of {productName} is available!\n\n" +
                           $"Your version: v{installedVersion}\n" +
                           $"Latest version: v{latestVersion}\n\n" +
                           "Would you like to download the update?";

            var result = ModernDialog.ShowUpdatePrompt(title, message, productName);

            switch (result.Action)
            {
                case UpdateAction.Download:
                    LogInfo($"[UpdateCheck] User chose to download update for '{productName}'");
                    Process.Start("https://my.streamup.tips");
                    break;

                case UpdateAction.DontRemind:
                    LogInfo($"[UpdateCheck] User dismissed v{latestVersion} for '{productName}'");
                    _CPH.SetGlobalVar(dismissedKey, latestVersion, true);
                    break;

                case UpdateAction.Later:
                default:
                    LogDebug($"[UpdateCheck] User chose 'Later' for '{productName}'");
                    break;
            }
        }

        /// <summary>
        /// Try to parse a version string, handling various formats (1.0.0, 1.0.0.0, etc.)
        /// </summary>
        private bool TryParseVersion(string versionStr, out Version version)
        {
            version = null;

            if (string.IsNullOrEmpty(versionStr))
                return false;

            // Remove any leading 'v' or 'V'
            versionStr = versionStr.TrimStart('v', 'V');

            // Try direct parse first
            if (Version.TryParse(versionStr, out version))
                return true;

            // Handle cases like "1.0" by adding .0
            var parts = versionStr.Split('.');
            if (parts.Length == 2)
            {
                return Version.TryParse(versionStr + ".0", out version);
            }

            return false;
        }
    }

    /// <summary>
    /// Result of an update prompt dialog
    /// </summary>
    public enum UpdateAction
    {
        Later,
        Download,
        DontRemind
    }

    /// <summary>
    /// Result from the update dialog
    /// </summary>
    public class UpdatePromptResult
    {
        public UpdateAction Action { get; set; }
    }
}
