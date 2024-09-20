using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
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
                _CPH.SetGlobalVar("sup000_IgnoreObsPluginsOutOfDate", false);
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

    }
}
