using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Load product settings and verify
        private bool LoadProductSettings(string productNumber, ProductInfo productInfo, out Dictionary<string, object> productSettings)
        {
            // Get productSettings from StreamerBot persisted global var
            LogInfo($"Loading productSettings from StreamerBot persisted global var [{productInfo.ProductNumber}_ProductSettings]");
            string settingsJson = _CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_ProductSettings", true);

            // Check if productSettings exists
            if (string.IsNullOrEmpty(settingsJson))
            {
                string errorMessage = $"No settings found for {productInfo.ProductName}";
                string actionMessage = "Please run the settings action for this product.\n\nWould you like to open the Settings now?";
                LogError(errorMessage);
                DialogResult result = MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    _CPH.RunAction(productInfo.SettingsAction, false);
                }
                productSettings = null;
                return false;
            }

            // Convert productSettings into Dictionary
            productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsJson);
            LogInfo("Successfully retrieved productSettings");
            return true;
        }
    }
}
