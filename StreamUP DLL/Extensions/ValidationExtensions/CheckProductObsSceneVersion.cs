using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Validate OBS product scene
        private bool CheckProductSceneExists(ProductInfo productInfo, int obsConnection)
        {
            if (!TryGetObsProductScene(productInfo, obsConnection))
            {
                LogError($"Product scene [{productInfo.SceneName}] does not exist in OBS.");
                return false;
            }
            return true;
        }

        private bool TryGetObsProductScene(ProductInfo productInfo, int obsConnection)
        {
            // Retrieve the list of scenes from OBS
            var sceneListResponse = _CPH.ObsSendRaw("GetSceneList", "{}", obsConnection);

            // Ensure scene list is not empty or malformed
            if (string.IsNullOrWhiteSpace(sceneListResponse))
            {
                string errorMessage = "Failed to retrieve scene list from OBS. Please check your OBS connection and try again.";
                LogError(errorMessage);
                MessageBox.Show(errorMessage, "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Parse the JSON response to extract the scenes list
            var jsonResponse = JObject.Parse(sceneListResponse); // Parse the response as JSON
            var scenes = jsonResponse["scenes"]?.ToObject<List<JObject>>(); // Extract the "scenes" array

            
            if (scenes == null)
            {
                string errorMessage = "Scene list is empty or malformed.";
                LogError(errorMessage);
                MessageBox.Show(errorMessage, "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Check if the exact scene name exists in the scene list
            bool sceneExists = scenes.Any(scene => scene["sceneName"]?.ToString() == productInfo.SceneName);

            if (!sceneExists)
            {
                return false;
            }

            return true;
        }


        // Check product version
        private bool CheckObsProductSceneVersion(ProductInfo productInfo, int obsConnection)
        {
            if (!GetObsProductVersion(productInfo, obsConnection, out Version versionInstalled))
            {
                LogError($"OBS product out of date. Expected: {productInfo.SourceNameVersionNumber}, Found: {versionInstalled}");
                return false;
            }
            return true;
        }

        public bool GetObsProductVersion(ProductInfo productInfo, int obsConnection, out Version versionInstalled)
        {
            // Pull product version from source settings
            if (!GetObsSourceSettings(productInfo.SourceNameVersionCheck, obsConnection, out JObject sourceSettings))
            {
                LogError("Unable to retrieve source settings");
                versionInstalled = null;
                return false;
            }

            // Check if 'product_version' is found in the settings
            string foundVersion = null;
            if (sourceSettings.TryGetValue("product_version", out JToken versionToken))
            {
                foundVersion = versionToken.ToString();
                LogInfo($"Found product version: {foundVersion}");
            }

            // Handle case where version is not found
            if (string.IsNullOrEmpty(foundVersion))
            {
                string warningMessage =
                $"No version number found for '{productInfo.ProductName}' in OBS.";

                string actionMessage =
                @$"This may indicate the scene is not up-to-date, or another OBS plugin such as 'Source Defaults' could be causing a conflict.
                Please check your OBS setup or reinstall the latest .StreamUP version for '{productInfo.ProductName}'.";

                LogError(warningMessage);
                MessageBox.Show($"{warningMessage}\n\n{actionMessage}", "StreamUP Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                versionInstalled = null;
                return true;
            }

            // Check if the found version is up-to-date
            Version foundVer = new Version(foundVersion);
            Version targetVer = productInfo.SourceNameVersionNumber;

            if (foundVer < targetVer)
            {
                string errorMessage =
                @$"Current version of '{productInfo.ProductName}' in OBS is out of date.";

                string actionMessage =
                @$"Please make sure you have downloaded the latest version from https://my.streamup.tips.
                Then install the latest .StreamUP version for '{productInfo.ProductName}' into OBS.";

                LogError(errorMessage);
                MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                versionInstalled = foundVer;
                return false;
            }

            versionInstalled = foundVer;
            return true;
        }

        // Check product version
        private bool CheckProductSceneVersion(ProductInfo productInfo, int obsConnection)
        {
            if (!GetObsProductVersion(productInfo, obsConnection, out Version versionInstalled))
            {
                LogError($"OBS product out of date. Expected: {productInfo.SourceNameVersionNumber}, Found: {versionInstalled}");
                return false;
            }
            return true;
        }

    }
}
