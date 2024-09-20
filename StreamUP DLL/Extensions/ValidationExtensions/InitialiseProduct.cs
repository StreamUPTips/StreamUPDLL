using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                LogError("Unable to retrieve OBS connection number from settings");
                return false;
            }

            if (!TryGetObsConnection(obsConnection))
            {
                LogError("Unable to connect to OBS");
                return false;
            }

            if (!CheckObsPlugins(productInfo, obsConnection))
            {
                LogError("OBS plugins are not up to date and user doesn't want to continue");
                return false;
            }

            if (!CheckProductSceneExists(productInfo, obsConnection))
            {
                LogError($"Product scene [{productInfo.SceneName}] has not been found in OBS");
                return false;
            }

            if (!CheckProductSceneVersion(productInfo, obsConnection))
            {
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

        // Load product information from settings
        private bool LoadProductInfo(string actionName, string productNumber, out ProductInfo productInfo)
        {
            LogInfo($"Loading productInfo");

            // Set settings method name to run
            actionName = FormatActionName(actionName);
            string methodName = $"{actionName} - Select Settings";

            // Run settings method to load productInfo
            LogInfo($"Executing settings method [{methodName}]");
            _CPH.ExecuteMethod(methodName, "LoadProductInfo");

            // Load productInfo from global StreamerBot var
            LogInfo($"Loading product info from StreamerBot non persisted Global [{productNumber}_ProductInfo]");
            string productInfoJson = _CPH.GetGlobalVar<string>($"{productNumber}_ProductInfo", false);
            if (string.IsNullOrEmpty(productInfoJson))
            {
                LogError($"Failed to load product info for {productNumber}");
                productInfo = null;
                return false;
            }

            // Convert productInfo string into ProductInfo class
            productInfo = JsonConvert.DeserializeObject<ProductInfo>(productInfoJson);
            LogInfo("Retrived productInfo successfully");
            return true;
        }

        private string FormatActionName(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                LogError("Action name cannot be null or empty");
                return string.Empty;
            }

            string formattedActionName = actionName.Replace("•", "-").Trim();

            return formattedActionName;
        }

        // Load product settings and verify
        private bool LoadProductSettings(string productNumber, ProductInfo productInfo, out Dictionary<string, object> productSettings)
        {
            // Get productSettings from StreamerBot persisted global var
            LogInfo($"Loading productSettings from StreamerBot persisted global var [{productInfo.ProductNumber}_ProductSettings]");
            string settingsJson = _CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_ProductSettings", true);

            // Check if productSettings exists
            if (string.IsNullOrEmpty(settingsJson))
            {
                LogError($"No settings found for {productInfo.ProductName}");
                productSettings = null;
                return false;
            }

            // Convert productsettings into Dictionary
            productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsJson);
            LogInfo("Successfully retrieved productSettings");
            return true;
        }

        // Check OBS connection and plugins
        public bool CheckObsConnectionSet(Dictionary<string, object> productSettings, out int obsConnection)
        {
            LogInfo("Getting obsConnection number from productSettings");
            // Retrieve ObsConnection from productSettings
            if (productSettings.TryGetValue("ObsConnection", out object obsConnectionObj))
            {
                obsConnection = Convert.ToInt32(obsConnectionObj);
                LogInfo($"Successfully retrieved the obsConnection number [{obsConnection}]");
                return true;
            }
            else
            {
                LogError("ObsConnection setting is missing in product settings.");
                obsConnection = -1;
                return false;
            }
        }

        private bool TryGetObsConnection(int obsConnection)
        {
            LogInfo("Attempting to connect to OBS");
            bool isConnected = _CPH.ObsIsConnected(obsConnection);

            if (!isConnected)
            {
                LogError($"OBS is not connected on connection [{obsConnection}]");
            }
            else
            {
                LogInfo("OBS is connected");
            }

            return isConnected;
        }

        private bool CheckObsPlugins(ProductInfo productInfo, int obsConnection)
        {
            LogInfo("Checking if OBS plugins are installed and are up to date");
            bool? pluginsUpToDate = GetObsPluginVersions(productInfo, obsConnection);

            if (pluginsUpToDate == false)
            {
                LogError("OBS plugins are not up to date.");
                return false; // Stop only if plugins are out of date and no preference to continue
            }

            return true;
        }

        // Retrieve plugin versions and handle missing or outdated plugins
        private bool GetObsPluginVersions(ProductInfo productInfo, int obsConnection)
        {
            LogInfo("Getting OBS plugin data via the StreamUP OBS plugin");

            // Send request to OBS to check plugin versions
            string pluginCheckResponse = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"check_plugins\",\"requestData\":null}", obsConnection);

            if (string.IsNullOrWhiteSpace(pluginCheckResponse) || pluginCheckResponse.Trim() == "{}")
            {
                LogInfo("StreamUP plugin is missing or is out of date");
                HandleMissingPlugin();
                return false; // Plugin is missing or out of date
            }

            if (UserRequestedNoPluginReminder())
            {
                LogInfo("User has requested to not be reminded about OBS plugins this session");
                return true; // User requested not to be reminded, continue without interrupting the process
            }

            // Check if plugins are up to date
            return ArePluginsUpToDate(pluginCheckResponse);
        }

        // Handle case where plugin is not installed or outdated
        private void HandleMissingPlugin()
        {
            string errorText = "Cannot check OBS plugins. The StreamUP OBS plugin may not be installed or is out of date.";
            LogError(errorText);

            DialogResult result = MessageBox.Show(errorText + "\n\nWould you like to go to the download page now?", "StreamUP Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                Process.Start("https://streamup.tips/plugin");
            }
        }

        // Check if the user has requested not to be reminded about plugin updates for this session
        private bool UserRequestedNoPluginReminder()
        {
            bool? pluginReminder = _CPH.GetGlobalVar<bool?>("sup000_ObsPluginReminder", false);

            if (pluginReminder == false)
            {
                LogInfo("User has requested not to be reminded about OBS plugins being out of date this session.");
                return true; // Continue without asking
            }

            return false;
        }

        // Parse the plugin check response and prompt the user if plugins are out of date
        private bool ArePluginsUpToDate(string pluginCheckResponse)
        {
            JObject checkPluginsObj = JObject.Parse(pluginCheckResponse);
            bool isSuccess = checkPluginsObj["responseData"]?["success"]?.Value<bool>() ?? false;

            if (!isSuccess)
            {
                LogError("OBS has plugins that are required but are out of date.");

                var (response, askAgain) = ShowObsPluginsUpdateMessage();

                if (response == DialogResult.Yes)
                {
                    Process.Start("https://streamup.tips/product/plugin-installer");
                }

                // Save the user's preference for not being reminded again
                _CPH.SetGlobalVar("sup000_ObsPluginReminder", askAgain, false);

                if (!askAgain)
                {
                    return false; // Stop the action if the user doesn't want to continue
                }
            }

            return true; // Continue if plugins are up to date or the user chooses to proceed
        }

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

            // Check if the product scene exists in the retrieved scene list
            if (!sceneListResponse.Contains(productInfo.SceneName))
            {
                string errorMessage = 
                @$"The scene '{productInfo.SceneName}' does not exist in OBS.
                Please reinstall it into OBS using the product's '.StreamUP' file.";
                LogError(errorMessage);
                MessageBox.Show(errorMessage, "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

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


        public enum ProductType
        {
            Obs,
            StreamerBot
        }


        // Show a form prompting the user to update OBS plugins
        public (DialogResult, bool) ShowObsPluginsUpdateMessage()
        {
            using (var form = new Form
            {
                Text = "StreamUP Warning",
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                Size = new Size(400, 250),
                FormBorderStyle = FormBorderStyle.FixedDialog
            })
            {
                var iconWarning = new PictureBox
                {
                    Image = SystemIcons.Warning.ToBitmap(),
                    Location = new Point(10, 20),
                    Size = new Size(32, 32),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                var labelMessage = new Label
                {
                    Text = "OBS has plugins that are required but are missing or out of date.\n" +
                        "You can use the StreamUP Pluginstaller to download them all in one go.\n\n" +
                        "Would you like to open the download page now?",
                    AutoSize = false,
                    Size = new Size(330, 100),
                    Location = new Point(50, 10),
                    Padding = new Padding(10)
                };

                var checkBoxReminder = new CheckBox
                {
                    Text = "Don't ask me again\nIf this is checked, the action will try to continue to run.",
                    Location = new Point(50, 110),
                    Size = new Size(300, 50)
                };

                var buttonYes = new Button
                {
                    Text = "Yes",
                    DialogResult = DialogResult.Yes,
                    Location = new Point(75, 160),
                    Size = new Size(100, 25)
                };

                var buttonNo = new Button
                {
                    Text = "No",
                    DialogResult = DialogResult.No,
                    Location = new Point(225, 160),
                    Size = new Size(100, 25)
                };

                form.Controls.Add(iconWarning);
                form.Controls.Add(labelMessage);
                form.Controls.Add(checkBoxReminder);
                form.Controls.Add(buttonYes);
                form.Controls.Add(buttonNo);

                form.AcceptButton = buttonYes;
                form.CancelButton = buttonNo;
                var dialogResult = form.ShowDialog();

                bool askAgain = !checkBoxReminder.Checked;
                return (dialogResult, askAgain);
            }
        }


        public bool TryGetSBProductUpdateChecker()
        {
            // Check if user has already been prompted for the update checker this launch
            if (_CPH.GetGlobalVar<bool>("sup000_UpdateCheckerPrompted", false))
            {
                return true;
            }

            // Check if Update checker action exists
            if (!_CPH.ActionExists("StreamUP Tools • Update Checker"))
            {
                string errorMessage = "The StreamUP Update Checker for Streamer.Bot is not installed";

                string actionMessage = 
                @"You can download it from the StreamUP website.
                Would you like to open the link now?";

                LogError(errorMessage);



                DialogResult result = MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Process.Start("https://streamup.tips/product/update-checker");
                }
                _CPH.SetGlobalVar("sup000_UpdateCheckerPrompted", true, false);
            }
            else
            {
                LogInfo("StreamUP update checker for Streamer.Bot is installed");
            }

            return true;
        }
    }

}
