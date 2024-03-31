using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;
using System.Globalization;
using System.Net;

namespace StreamUP {

    public static class GenericExtensions {

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
   
        public static void SUSetProductObsVersion(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sceneName, string versionNumber)
        {
            // Load log string
            string logName = "GeneralExtensions-SUSetProductObsVersion";
            CPH.SUWriteLog("Method Started", logName);

            string inputSettings = $"\"product_version\": \"{versionNumber}\"";
            CPH.SUWriteLog($"Loaded version settings to set: inputSettings=[{inputSettings}]", logName);

            // Create sceneItem list
            List<string> sceneItemNames = new List<string>();
            CPH.SUObsGetSceneItemNames(productName, obsConnection, 0, sceneName, sceneItemNames);
            CPH.SUWriteLog($"Retrieved scene item list on scene [{sceneName}]: sceneItemNames=[{sceneItemNames.ToString()}]", logName);

            // Set the version number on each source in that scene
            foreach (string currentItemName in sceneItemNames)
            {
                CPH.SUObsSetInputSettings("GeneralExtensions", obsConnection, currentItemName, inputSettings);
            }
            CPH.SUWriteLog("Method complete", logName);
        }
    
        public static string SUConvertCurrency(this IInlineInvokeProxy CPH, decimal amount, string fromCurrency, string toCurrency)
        {
            // Load log string
            string logName = "GeneralExtensions-SUConvertCurrency";
            CPH.SUWriteLog("Method Started", logName);

            // Get the exchange rate
            decimal exchangeRate = SUGetExchangeRate(CPH, fromCurrency.ToLower(), toCurrency.ToLower());
            CPH.SUWriteLog($"exchangeRate=[{exchangeRate}]");

            // Convert the amount
            decimal convertedAmount = amount / exchangeRate;
            CPH.SUWriteLog($"convertedAmount=[{convertedAmount}]");

            // Get the currency symbol
            string currencySymbol = CPH.SUGetCurrencySymbol(toCurrency);
            CPH.SUWriteLog($"currencySymbol=[{currencySymbol}]", logName);

            // Format the converted amount with the currency symbol
            string formattedAmount = $"{currencySymbol}{convertedAmount:N2}";
            CPH.SUWriteLog($"formattedAmount=[{formattedAmount}]", logName);

            CPH.SUWriteLog("Method completed", logName);
            return formattedAmount;
        }

        private static decimal SUGetExchangeRate(this IInlineInvokeProxy CPH, string fromCurrency, string toCurrency)
        {
            // Load log string
            string logName = "GeneralExtensions-SUGetExchangeRate";
            CPH.SUWriteLog("Method Started", logName);

            string url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@2024.3.25/v1/currencies/{toCurrency}.json";
            CPH.SUWriteLog(url, logName);

            string rawJson;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                rawJson = client.DownloadString(url);
                CPH.SUWriteLog(rawJson, logName);
            }

            JObject json = JObject.Parse(rawJson);
            CPH.SUWriteLog(json.ToString(), logName);

            decimal exRate = (decimal)json[toCurrency][fromCurrency];
            CPH.SUWriteLog($"exRate=[{exRate}]");

            CPH.SUWriteLog("Method completed", logName);
            return exRate;
        }

        public static string SUGetCurrencySymbol(this IInlineInvokeProxy CPH, string currencyCode)
        {
            RegionInfo region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => new RegionInfo(ci.LCID))
                .FirstOrDefault(ri => ri.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

            return region != null ? region.CurrencySymbol : currencyCode;
        }      

    }
}
