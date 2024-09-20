using System.Collections.Generic;
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
                LogError($"No settings found for {productInfo.ProductName}");
                productSettings = null;
                return false;
            }

            // Convert productsettings into Dictionary
            productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsJson);
            LogInfo("Successfully retrieved productSettings");
            return true;
        }
    }
}
