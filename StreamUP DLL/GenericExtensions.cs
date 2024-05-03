using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;

namespace StreamUP {

    public static class GenericExtensions {

        public static bool SUInitialiseGeneralProduct(this IInlineInvokeProxy CPH, string actionName, string productNumber = "DLL", string settingsGlobalName = "ProductSettings")
        {
            string logName = $"{productNumber}::SUInitialiseGeneralProduct";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            if (CPH.GetGlobalVar<bool>($"{productNumber}_ProductInitialised", false))
            {
                CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
                return true;
            }

            // Check ProductInfo is loaded
            ProductInfo productInfo = CPH.SUValProductInfoLoaded(actionName, productNumber);
            if (productInfo == null)
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Check ProductSettings is loaded
            if (!CPH.SUValProductSettingsLoaded(productInfo))
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Deserialise productSettings into a Dictionary
            Dictionary<string, object> productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_{settingsGlobalName}"));

            // Mark product as initialised
            CPH.SetGlobalVar($"{productInfo.ProductNumber}_ProductInitialised", true, false);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static bool SUInitialiseObsProduct(this IInlineInvokeProxy CPH, string actionName, string productNumber = "DLL", string settingsGlobalName = "ProductSettings")
        {
            string logName = $"{productNumber}::SUInitialiseObsProduct";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            if (CPH.GetGlobalVar<bool>($"{productNumber}_ProductInitialised", false))
            {
                CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
                return true;
            }

            // Check ProductInfo is loaded
            ProductInfo productInfo = CPH.SUValProductInfoLoaded(actionName, productNumber);
            if (productInfo == null)
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Check ProductSettings is loaded
            if (!CPH.SUValProductSettingsLoaded(productInfo))
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Deserialise productSettings into a Dictionary
            Dictionary<string, object> productSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_{settingsGlobalName}"));
            int obsConnection = Convert.ToInt32(productSettings["ObsConnection"]);

            // Check Obs is connected
            if (!CPH.SUValObsIsConnected(productInfo, obsConnection))
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Check Obs plugins are installed and up to date
            if (!CPH.SUValObsPlugins(productInfo, obsConnection))
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Check StreamUP scene exists
            if (!CPH.SUValStreamUPSceneExists(productInfo, obsConnection))
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Check StreamUP scene/source version
            if (!CPH.SUValProductObsVersion(productInfo, obsConnection))
            {
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Mark product as initialised
            CPH.SetGlobalVar($"{productInfo.ProductNumber}_ProductInitialised", true, false);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static string SUGetStreamerBotFolder(this IInlineInvokeProxy CPH) {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        
        public static void SUWriteLog(this IInlineInvokeProxy CPH, string logMessage, string productName = "General") {
            string sbFolder = CPH.SUGetStreamerBotFolder();
            string suFolder = Path.Combine(sbFolder, "StreamUP");
            string suLogFolder = Path.Combine(suFolder, "logs");

            if (!Directory.Exists(suFolder)) {
                Directory.CreateDirectory(suFolder);
            }

            if (!Directory.Exists(suLogFolder)) { 
                Directory.CreateDirectory(suLogFolder);
            }

            DateTime today = DateTime.Now;

            string todayFileName = $"{today.ToString("yyyyMMdd")} - StreamUP.log";

            string todayPath = Path.Combine(suLogFolder, todayFileName);

            if (!File.Exists(todayPath)) {
                using (FileStream fs = File.Create(todayPath)) {
                    byte[] info = new UTF8Encoding(true).GetBytes("Heyo duckies! New Day!");
                    fs.Write(info, 0, info.Length);
                }
            }

            using (StreamWriter file = new StreamWriter(todayPath, true)) {
                string formattedLogMessage = $"[{today.ToString("yyyy-MM-dd HH:mm:ss:fff")}] [{productName}] :: {logMessage}";
                file.Write($"\r\n{formattedLogMessage}");
            }
        }
   
        public static bool SULoadProductInfo(this IInlineInvokeProxy CPH, string actionName, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SULoadProductInfo";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            CPH.SUWriteLog("Loading a products information...", logName);

            actionName = actionName.Replace("•", "-");

            // Split the triggeredAction string at the "-" character
            string[] parts = actionName.Split(new[] { '-' }, 2); // Limiting to 2 parts to ensure we only split at the first '-'
            if (parts.Length > 0)
            {
                string actionBeforeDash = parts[0].Trim();
                CPH.ExecuteMethod($"{actionBeforeDash} - Select Settings", "LoadProductInfo");
                CPH.SUWriteLog("Loaded products information.", logName);
            }
            else
            {
                CPH.SUWriteLog($"ERROR: Action that was run didn't have a '-' in the name. Make sure you use the Streamer.Bot action naming format 'productName - functionName'.", logName);
                CPH.SUWriteLog("METHOD FAILED!", logName);
                return false;
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static string SULoadProductSettings(this IInlineInvokeProxy CPH, ProductInfo productInfo)
        {
            string logName = $"{productInfo.ProductNumber}::SULoadProductInfo";
            CPH.SUWriteLog("METHOD STARTED!", logName);
            CPH.SUWriteLog("Loading a products settings...", logName);

            // Load ProductSettings
            string productSettings = CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_ProductSettings", true);        
            if (productSettings == null)
            {
                CPH.SUWriteLog($"Product settings have not been run");
                DialogResult runSettings = CPH.SUUIShowWarningYesNoMessage($"{productInfo.ProductName} has no settings.\n\nWould you like to run the settings selection now?");
                if (runSettings == DialogResult.Yes)
                {
                    CPH.RunAction(productInfo.SettingsAction, false);
                }
                CPH.SUWriteLog("METHOD FAILED!", logName);
                return null;
            }
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return productSettings;
        }

        public static bool SULoadSettingsMenu(this IInlineInvokeProxy CPH, Dictionary<string, object> sbArgs, ProductInfo productInfo, List<StreamUpSetting> supSettingsList, List<(string fontName, string fontFile, string fontUrl)> requiredFonts, string settingsGlobalName = "ProductSettings")
        {
            string logName = $"{productInfo.ProductNumber}::SULoadSettingsMenu";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Check if StreamUP.dll version is the required version or newer
            CPH.SUWriteLog("Checking if StreamUP.dll is the required version or newer...", logName);
            if (!CPH.SUValLibraryVersion(productInfo.RequiredLibraryVersion))
            {
                CPH.SUWriteLog("METHOD FAILED!", logName);
                return false;
            }

            // Check if there are any required fonts and that they are installed
            if (requiredFonts != null)
            {
                CPH.SUWriteLog("Checking for any required system fonts...", logName);
                if (requiredFonts.Count > 0)
                {
                    CPH.SUWriteLog("Required fonts found. Checking if user has them installed...", logName);
                    CPH.SUValFontInstalled(requiredFonts, productInfo.ProductNumber);
                }
            }

            // Load settings menu
            CPH.SUWriteLog("Launching settings menu...", logName);
            bool? settingsSaved = CPH.SUExecuteSettingsMenu(productInfo, supSettingsList, sbArgs, settingsGlobalName);
            if (!settingsSaved.HasValue || settingsSaved == false)
            {
                CPH.SUWriteLog("METHOD FAILED!", logName);
                return false;
            }

            // Check SB product update checker is installed
            CPH.SUWriteLog("Checking if the Streamer.Bot StreamUP update checker is installed...", logName);
            CPH.SUValSBUpdateChecker();

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static bool SUSetProductObsVersion(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string versionNumber, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SULoadSettingsMenu";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string inputSettings = $"\"product_version\": \"{versionNumber}\"";
            CPH.SUWriteLog($"Loaded version settings to set: inputSettings=[{inputSettings}]", logName);

            // Create sceneItem list
            List<string> sceneItemNames = new List<string>();
            CPH.SUObsGetSceneItemNames(productNumber, obsConnection, 0, sceneName, sceneItemNames);
            CPH.SUWriteLog($"Retrieved scene item list on scene [{sceneName}]: sceneItemNames=[{sceneItemNames.ToString()}]", logName);

            // Set the version number on each source in that scene
            foreach (string currentItemName in sceneItemNames)
            {
                CPH.SUObsSetInputSettings(productNumber, obsConnection, currentItemName, inputSettings);
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }
    
        public static string SUConvertCurrency(this IInlineInvokeProxy CPH, decimal amount, string fromCurrency, string toCurrency, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUConvertCurrency";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Get the exchange rate
            decimal exchangeRate = SUGetExchangeRate(CPH, fromCurrency.ToLower(), toCurrency.ToLower());
            CPH.SUWriteLog($"exchangeRate=[{exchangeRate}]", logName);

            // Convert the amount
            decimal convertedAmount = amount / exchangeRate;
            CPH.SUWriteLog($"convertedAmount=[{convertedAmount}]", logName);

            // Get the currency symbol
            string currencySymbol = CPH.SUGetCurrencySymbol(toCurrency);
            CPH.SUWriteLog($"currencySymbol=[{currencySymbol}]", logName);

            // Format the converted amount with the currency symbol
            string formattedAmount = $"{currencySymbol}{convertedAmount:N2}";
            CPH.SUWriteLog($"formattedAmount=[{formattedAmount}]", logName);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return formattedAmount;
        }

        private static decimal SUGetExchangeRate(this IInlineInvokeProxy CPH, string fromCurrency, string toCurrency, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUGetExchangeRate";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@2024.3.25/v1/currencies/{toCurrency}.json";

            string rawJson;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                rawJson = client.DownloadString(url);
            }

            JObject json = JObject.Parse(rawJson);

            decimal exRate = (decimal)json[toCurrency][fromCurrency];

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return exRate;
        }

        public static string SUGetCurrencySymbol(this IInlineInvokeProxy CPH, string currencyCode, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUGetCurrencySymbol";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            RegionInfo region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => new RegionInfo(ci.LCID))
                .FirstOrDefault(ri => ri.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return region != null ? region.CurrencySymbol : currencyCode;
        }      

        public static long SUGetContrastingColour(this IInlineInvokeProxy CPH, long inputColour, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUGetContrastingColour";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            long a = (inputColour >> 24) & 0xFF;
            long r = (inputColour >> 16) & 0xFF; 
            long g = (inputColour >> 8) & 0xFF;  
            long b = inputColour & 0xFF;         

            // Convert RGB to YIQ:
            double y = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            
            if (y >= 0.5)
            {
                CPH.SUWriteLog("Returning black as contrasting colour", logName);
                return 4278190080L;
            }
            else
            {
                CPH.SUWriteLog("Returning white as contrasting colour", logName);
                return 4294967295L;
            }
        }

        public static string SUGetRandomColour(this IInlineInvokeProxy CPH, string productNumber = "DLL")
        {
            Random random = new Random();            
            int red = random.Next(256);
            int green = random.Next(256);
            int blue = random.Next(256);

            // Convert RGB values to a hexadecimal string
            string hexColor = $"#{red:X2}{green:X2}{blue:X2}";

            return hexColor;
        }


    }
}
