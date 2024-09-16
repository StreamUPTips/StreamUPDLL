using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool ConvertCurrency(decimal inputAmount, string fromCurrency, string toCurrency, out decimal convertedAmount)
        {
            LogInfo($"Converting {inputAmount} from {fromCurrency} to {toCurrency}");

            // Check in fromCurrency is the same as toCurrency
            if (fromCurrency == toCurrency)
            {
                LogInfo("Skipping converting currency. fromCurrency is the same as toCurrency");
                convertedAmount = inputAmount;
                return true;
            }

            // Get the exchange rate
            LogInfo($"Getting currency exchange rate");
            if (!TryGetCurrencyExchangeRate(fromCurrency.ToLower(), toCurrency.ToLower(), out decimal exchangeRate))
            {
                LogError("Unable to get exchange rate.");
                convertedAmount = inputAmount;
                return false;
            }

            // Convert the amount
            convertedAmount = inputAmount / exchangeRate;
            LogInfo($"Sucessfully retrieved converted amount");
            return true;
        }

        public bool TryGetCurrencyExchangeRate(string fromCurrency, string toCurrency, out decimal exchangeRate)
        {
            LogInfo("Getting exchange rate");

            exchangeRate = -1;  // Set default value for exchangeRate in case of failure

            try
            {
                // Fetch the latest version of the currency API
                string latestVersion = "";
                string versionUrl = "https://data.jsdelivr.com/v1/package/npm/@fawazahmed0/currency-api";

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    string rawJson = client.DownloadString(versionUrl);
                    JObject json = JObject.Parse(rawJson);
                    latestVersion = json["tags"]?["latest"]?.ToString();
                }

                // If the latest version is not found, log an error and return false
                if (string.IsNullOrEmpty(latestVersion))
                {
                    LogError("Failed to fetch the latest version.");
                    return false;
                }

                // Construct the URL using the latest version
                string url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@{latestVersion}/v1/currencies/{toCurrency.ToLower()}.json";

                string exchangeRateJson;

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    exchangeRateJson = client.DownloadString(url);
                }

                // Parse the JSON response for exchange rate
                JObject exchangeRateData = JObject.Parse(exchangeRateJson);

                // Ensure the necessary data exists in the JSON response
                if (exchangeRateData[toCurrency] == null || exchangeRateData[toCurrency][fromCurrency] == null)
                {
                    LogError($"Exchange rate for {fromCurrency} to {toCurrency} not found in the API response.");
                    return false;
                }

                // Get the exchange rate
                exchangeRate = (decimal)exchangeRateData[toCurrency][fromCurrency];
                LogInfo($"Successfully retrieved exchange rate: {fromCurrency} to {toCurrency} = {exchangeRate}");

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error occurred while fetching exchange rate: {ex.Message}");
                return false;
            }
        }

        public bool GetLocalCurrencyCode(out string localCurrencyCode)
        {
            LogInfo("Getting local currency code from Streamer.Bot globals");

            if (!GetStreamerBotGlobalVar("sup000_LocalCurrency", true, out localCurrencyCode))
            {
                LogError("Local currency variable not found.");
                localCurrencyCode = null;
                return false;
            }

            LogInfo("Successfully retrieved local currency code");
            return true;
        }

        public bool GetCurrencySymbol(string inputCurrencyCode, out string currencySymbol)
        {
            LogInfo($"Getting currency symbol for currency [{inputCurrencyCode}]");

            // Find the region info matching the currency code
            RegionInfo region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => new RegionInfo(ci.LCID))
                .FirstOrDefault(ri => ri.ISOCurrencySymbol.Equals(inputCurrencyCode, StringComparison.OrdinalIgnoreCase));

            // If region is found, assign the currency symbol, otherwise fallback to the inputCurrencyCode
            if (region != null)
            {
                currencySymbol = region.CurrencySymbol;
                LogInfo($"Successfully retrieved currency symbol: {currencySymbol}");
                return true;
            }
            else
            {
                currencySymbol = inputCurrencyCode; // Fallback to the currency code itself
                LogError($"Currency symbol for {inputCurrencyCode} not found, defaulting to currency code.");
                return false;
            }
        }

        public string StringifyCurrencyWithSymbol(decimal amount, string currencyCode)
        {
            LogInfo("Adding symbol to currency amount and outputting as string");
            if (!GetCurrencySymbol(currencyCode, out string currencySymbol))
            {
                LogError($"Unable to get currency symbol for {currencyCode}. Using currency code as symbol.");
                currencySymbol = currencyCode;
            }

            return $"{currencySymbol}{amount}";
        }
    }
}
