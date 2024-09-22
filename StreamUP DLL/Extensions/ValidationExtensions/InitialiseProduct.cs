using System.Collections.Generic;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool InitialiseProduct(string actionName, string productNumber, ProductType productType)
        {
            // Check if already initialised
            if (IsProductInitialised(productNumber)) 
            {
                LogInfo("Product is already initialised. Skipping initialising step.");
                return true;
            }
            LogInfo("Product is not initialised. Starting initialising.");

            // Load productInfo
            if (!LoadProductInfo(actionName, productNumber, out var productInfo)) 
            {
                LogError("Unable to load productInfo");
                return false;
            }
            
            // Verify user has correct Library version
            if (!CheckStreamUpLibraryVersion(productInfo.RequiredLibraryVersion))
            {
                LogError("StreamUP library version is out of date");
                return false;
            }

            // Load productSettings
            if (!LoadProductSettings(productNumber, productInfo, out Dictionary<string, object> productSettings))
            {
                LogError("Unable to load productSettings");
                return false;
            }

            // If product is and OBS product, initialise OBS settings etc
            if (productType == ProductType.Obs)
            {
                if (!LoadObsProduct(productInfo, productSettings))
                {
                    LogError("Unable to load and confirm Obs data");
                    return false;
                }
            }

            // Mark product as initialised
            SetProductInitialised(productNumber, true);
            LogInfo($"Product [{productInfo.ProductName}] initialised successfully.");
            return true;
        }

        private bool LoadObsProduct(ProductInfo productInfo, Dictionary<string, object> productSettings)
        {
            if (!CheckObsConnectionSet(productSettings, out int obsConnection))
            {
                string errorMessage = "Unable to retrieve OBS connection number from settings.";
                MessageBox.Show(errorMessage, "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!TryGetObsConnection(obsConnection))
            {
                string errorMessage = "Unable to connect to OBS";
                string actionMessage = $"Check under the 'Stream Apps' tab in StreamerBot and make sure OBS is connected under connection number '{obsConnection}'";
                MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!CheckObsWebsocketVersion(obsConnection))
            {
                string errorMessage = $"There is no OBS Websocket v5.0.0 or above connection on connection number '{obsConnection}'.";
                string actionMessage = "Make sure OBS is connected via Websocket 5.0.0+ in the 'Stream Apps' tab in StreamerBot.";
                MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            GetObsCanvasSize(obsConnection, out int baseWidth, out int baseHeight);

            if (!CheckCanvasResolution(baseWidth, baseHeight))
            {
                LogError("Users canvas scale is not 16:9");
            }

            SetScaleFactor(baseWidth, productSettings, productInfo.ProductNumber);

            bool ignorePluginsOutOfDate = _CPH.GetGlobalVar<bool>("sup000_IgnoreObsPluginsOutOfDate", false);
            if (!ignorePluginsOutOfDate)
            {
                if (!CheckObsPlugins(productInfo, obsConnection))
                {
                    LogError("OBS plugins are not up to date and user doesn't want to continue");
                    return false;
                }
            }

            if (!CheckProductSceneExists(productInfo, obsConnection))
            {
                string errorMessage = $"The scene '{productInfo.SceneName}' was not found in OBS.";
                string actionMessage = "Please install the products '.StreamUP' file into OBS via the StreamUP OBS plugin";

                MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError($"Product scene [{productInfo.SceneName}] has not been found in OBS");
                return false;
            }

            if (!CheckProductSceneVersion(productInfo, obsConnection))
            {
                string errorMessage = $"The scene '{productInfo.SceneName}' in OBS is an older version.";
                string actionMessage = "Please install the products '.StreamUP' file into OBS via the StreamUP OBS plugin";

                MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError($"Product scene [{productInfo.SceneName}] is out of date in OBS");
                return false;
            }

            LogInfo("Products OBS has been verified and is compatible with this product");
            return true;
        }

        // Check if product is already initialised
        private bool IsProductInitialised(string productNumber)
        {
            return _CPH.GetGlobalVar<bool>($"{productNumber}_ProductInitialised", false);
        }

        // Set initialisation status
        private void SetProductInitialised(string productNumber, bool value)
        {
            LogInfo($"Setting product [{productNumber}] as initialised in StreamerBot global vars");
            _CPH.SetGlobalVar($"{productNumber}_ProductInitialised", value, false);
        }

    }

}
