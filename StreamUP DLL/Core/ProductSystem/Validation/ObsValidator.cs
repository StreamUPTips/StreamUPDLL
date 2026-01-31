using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Check if OBS is connected on the specified connection
        /// </summary>
        /// <param name="connectionIndex">OBS connection index (0-4)</param>
        /// <returns>True if connected</returns>
        public bool IsObsConnectedV2(int connectionIndex)
        {
            try
            {
                bool isConnected = _CPH.ObsIsConnected(connectionIndex);
                LogDebug($"OBS connection {connectionIndex} connected: {isConnected}");
                return isConnected;
            }
            catch (Exception ex)
            {
                LogError($"Failed to check OBS connection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a specific scene exists in OBS
        /// </summary>
        /// <param name="connectionIndex">OBS connection index (0-4)</param>
        /// <param name="sceneName">Name of the scene to check</param>
        /// <returns>True if scene exists</returns>
        public bool ObsSceneExistsV2(int connectionIndex, string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return true; // No scene to check
            }

            try
            {
                // Get scene list from OBS
                var sceneListResponse = _CPH.ObsSendRaw("GetSceneList", "{}", connectionIndex);

                if (string.IsNullOrWhiteSpace(sceneListResponse))
                {
                    LogError("Failed to retrieve scene list from OBS");
                    return false;
                }

                var jsonResponse = JObject.Parse(sceneListResponse);
                var scenes = jsonResponse["scenes"]?.ToObject<List<JObject>>();

                if (scenes == null)
                {
                    LogError("Scene list is empty or malformed");
                    return false;
                }

                bool sceneExists = scenes.Any(scene =>
                    scene["sceneName"]?.ToString().Equals(sceneName, StringComparison.OrdinalIgnoreCase) == true);

                LogDebug($"Scene '{sceneName}' exists: {sceneExists}");
                return sceneExists;
            }
            catch (Exception ex)
            {
                LogError($"Failed to check scene existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if the OBS scene version matches the expected version
        /// </summary>
        /// <param name="connectionIndex">OBS connection index (0-4)</param>
        /// <param name="sourceName">Name of the source to check version from</param>
        /// <param name="expectedVersion">Expected version string (e.g., "1.0.0.0")</param>
        /// <param name="installedVersion">Output: the version found in OBS</param>
        /// <returns>True if version is sufficient or no version check needed</returns>
        public bool ValidateObsSceneVersionV2(int connectionIndex, string sourceName, string expectedVersion, out Version installedVersion)
        {
            installedVersion = null;

            // No version check if no source name or expected version specified
            if (string.IsNullOrEmpty(sourceName) || string.IsNullOrEmpty(expectedVersion))
            {
                return true;
            }

            try
            {
                // Get source settings
                if (!GetObsSourceSettings(sourceName, connectionIndex, out JObject sourceSettings))
                {
                    LogError($"Unable to retrieve source settings for '{sourceName}'");
                    return false;
                }

                // Look for product_version in settings
                string foundVersionStr = null;
                if (sourceSettings.TryGetValue("product_version", out JToken versionToken))
                {
                    foundVersionStr = versionToken.ToString();
                }

                if (string.IsNullOrEmpty(foundVersionStr))
                {
                    LogInfo($"No version found in source '{sourceName}' - skipping version check");
                    return true; // Don't block if no version found
                }

                installedVersion = new Version(foundVersionStr);
                Version required = new Version(expectedVersion);

                bool isValid = installedVersion >= required;

                if (isValid)
                {
                    LogDebug($"Scene version check passed. Installed: {installedVersion}, Required: {required}");
                }
                else
                {
                    LogInfo($"Scene version check failed. Installed: {installedVersion}, Required: {required}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                LogError($"Failed to validate scene version: {ex.Message}");
                return true; // Don't block on errors
            }
        }
    }
}
