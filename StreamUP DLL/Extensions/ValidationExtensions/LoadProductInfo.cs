using Newtonsoft.Json;

namespace StreamUP
{
    public partial class StreamUpLib
    {
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
    
            LogInfo("Retrieved productInfo successfully");
            return true;
        }

        private string FormatActionName(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                LogError("Action name cannot be null or empty");
                return string.Empty;
            }

            // Find the position of the first occurrence of '•' or '-'
            int delimiterPosition = actionName.IndexOfAny(new char[] { '•', '-' });

            string productName;

            if (delimiterPosition > -1)
            {
                // Extract everything before the delimiter
                productName = actionName.Substring(0, delimiterPosition).Trim();
            }
            else
            {
                // If no delimiter is found, use the full action name
                productName = actionName.Trim();
            }

            return productName;
        }

    }

}
