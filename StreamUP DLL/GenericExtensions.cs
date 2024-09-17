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
using System.Drawing;
using System.Drawing.Imaging;
using static StreamUP.StreamUpLib;

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

        [Obsolete]
        public static string SUGetStreamerBotFolder(this IInlineInvokeProxy CPH) {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        
        [Obsolete]
        public static void SUWriteLog(this IInlineInvokeProxy CPH, string logMessage, string productName = "General") {
            StreamUpLib sup = new StreamUpLib(CPH, productName);
            sup.LogInfo(logMessage);
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

        [Obsolete]
        public static bool SUSetProductObsVersion(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string versionNumber, string productNumber = "DLL")
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);

            if (!sup.SetProductObsVersion(sceneName, versionNumber, obsConnection))
            {
                sup.LogError("Unable to set product version number");
            }

            return true;
        }
    
        [Obsolete]
        public static bool SUGetProductObsVersion(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string productNumber = "DLL")
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            sup.LogInfo($"Getting product version number for [{sceneName}]");

            // Create sceneItem list
            if (!sup.GetObsSceneItemsNamesList(sceneName, OBSSceneType.Scene, obsConnection, out List<string> sceneItemsNamesList))
            {
                sup.LogError("Unable to retrieve sceneItemsNameList");
                return false;
            }

            if (sceneItemsNamesList.Count > 0)
            {
                string firstItemName = sceneItemsNamesList[0];
                if (!sup.GetObsSourceSettings(firstItemName, obsConnection, out JObject inputSettings))
                {
                    sup.LogError($"Unable to retrieve source settings for [{firstItemName}]");
                    return false;
                }

                string versionNumber = inputSettings["product_version"].ToString();
                if (string.IsNullOrEmpty(versionNumber))
                {
                    sup.LogError($"No versionNumber found on source [{firstItemName}]");
                    return false;
                }

                sup.LogInfo($"Successfully retrieved product version number for [{versionNumber}]");
                return true;
            }
            else
            {
                sup.LogError($"No sources found on scene [{sceneName}]");
                return false;
            }
        }

        [Obsolete]
        public static string SUConvertCurrency(this IInlineInvokeProxy CPH, decimal amount, string fromCurrency, string toCurrency, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUConvertCurrency";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Get the exchange rate
            decimal exchangeRate = CPH.GetGlobalVar<decimal>("sup000_ExchangeRate", false);
            if (exchangeRate == 0)
            {
                CPH.SUWriteLog($"Getting exchange rate", logName);
                exchangeRate = SUGetExchangeRate(CPH, fromCurrency.ToLower(), toCurrency.ToLower());
                CPH.SetGlobalVar("sup000_ExchangeRate", exchangeRate, false);
            }
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

        [Obsolete]
        public static decimal SUGetExchangeRate(this IInlineInvokeProxy CPH, string fromCurrency, string toCurrency, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUGetExchangeRate";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string latestVersion = "";
            string versionUrl = "https://data.jsdelivr.com/v1/package/npm/@fawazahmed0/currency-api";
            
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                string rawJson = client.DownloadString(versionUrl);
                JObject json = JObject.Parse(rawJson);
                latestVersion = json["tags"]?["latest"]?.ToString();
            }

            if (string.IsNullOrEmpty(latestVersion))
            {
                CPH.SUWriteLog("Failed to fetch the latest version.", logName);
                return 0;
            }

            // Construct the URL using the latest version
            string url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@{latestVersion}/v1/currencies/{toCurrency.ToLower()}.json";

            string exchangeRateJson;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                exchangeRateJson = client.DownloadString(url);
            }

            JObject exchangeRateData = JObject.Parse(exchangeRateJson);
            decimal exchangeRate = (decimal)exchangeRateData[toCurrency][fromCurrency];

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return exchangeRate;
        }

        [Obsolete]
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

        public static void SUTrimPng(this IInlineInvokeProxy CPH, string filePath)
        {
            Image trimmedImage = TrimImage(filePath);
            trimmedImage.Save(filePath, ImageFormat.Png);
            trimmedImage.Dispose();
        }

        private static Image TrimImage(string imagePath)
        {
            Bitmap originalImage = new Bitmap(imagePath);
            Rectangle cropRect = GetImageBounds(originalImage);
            Bitmap trimmedImage = CropImage(originalImage, cropRect);
            originalImage.Dispose();
            return trimmedImage;
        }    

        private static Rectangle GetImageBounds(Bitmap img)
        {
            int x1 = img.Width, x2 = 0, y1 = img.Height, y2 = 0;
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                    if (pixel.A != 0)
                    {
                        if (x < x1)
                            x1 = x;
                        if (x > x2)
                            x2 = x;
                        if (y < y1)
                            y1 = y;
                        if (y > y2)
                            y2 = y;
                    }
                }
            }

            if (x1 > x2 || y1 > y2) 
                return new Rectangle(0, 0, img.Width, img.Height);
            return new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
        }

        private static Bitmap CropImage(Bitmap img, Rectangle cropArea)
        {
            return img.Clone(cropArea, img.PixelFormat);
        }
   
        public static T SUGetPropertyValue<T>(this IInlineInvokeProxy CPH, object obj, string propertyName)
        {
            return (T)obj.GetType().GetProperty(propertyName).GetValue(obj);
        }

        public static void SUSetPropertyValue<T>(this IInlineInvokeProxy CPH, object obj, string propertyName, T value)
        {
            obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        }   
   
   
   
    }
}
