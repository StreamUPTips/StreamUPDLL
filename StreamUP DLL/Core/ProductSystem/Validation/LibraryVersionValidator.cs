using System;
using System.Reflection;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Get the current StreamUP library version
        /// </summary>
        public Version GetCurrentLibraryVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        /// <summary>
        /// Check if the current library version meets the minimum requirement
        /// </summary>
        /// <param name="minimumRequiredVersion">Minimum version required (as string, e.g., "1.2.0.0")</param>
        /// <returns>True if current version is sufficient</returns>
        public bool ValidateLibraryVersionV2(string minimumRequiredVersion)
        {
            if (string.IsNullOrEmpty(minimumRequiredVersion) || minimumRequiredVersion == "0.0.0.0")
            {
                // No version requirement specified
                return true;
            }

            try
            {
                Version currentVersion = GetCurrentLibraryVersion();
                Version requiredVersion = new Version(minimumRequiredVersion);

                bool isValid = currentVersion >= requiredVersion;

                if (isValid)
                {
                    LogDebug($"Library version check passed. Current: {currentVersion}, Required: {requiredVersion}");
                }
                else
                {
                    LogInfo($"Library version check failed. Current: {currentVersion}, Required: {requiredVersion}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                LogError($"Failed to parse library version '{minimumRequiredVersion}': {ex.Message}");
                return true; // Don't block on parse errors
            }
        }

        /// <summary>
        /// Check if the current library version meets the minimum requirement
        /// </summary>
        /// <param name="minimumRequiredVersion">Minimum version required</param>
        /// <returns>True if current version is sufficient</returns>
        public bool ValidateLibraryVersionV2(Version minimumRequiredVersion)
        {
            if (minimumRequiredVersion == null)
            {
                return true;
            }

            Version currentVersion = GetCurrentLibraryVersion();
            bool isValid = currentVersion >= minimumRequiredVersion;

            if (isValid)
            {
                LogDebug($"Library version check passed. Current: {currentVersion}, Required: {minimumRequiredVersion}");
            }
            else
            {
                LogInfo($"Library version check failed. Current: {currentVersion}, Required: {minimumRequiredVersion}");
            }

            return isValid;
        }
    }
}
