using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private static string _cachedCurrencyApiVersion = null;
        private static DateTime _cachedCurrencyApiVersionExpiry = DateTime.MinValue;
        private static JObject _cachedExchangeRates = null;
        private static string _cachedExchangeRateCurrency = null;
        private static DateTime _cachedExchangeRatesExpiry = DateTime.MinValue;
        private static readonly TimeSpan _currencyCacheDuration = TimeSpan.FromHours(1);

        public bool ConvertCurrency(decimal inputAmount, string fromCurrency, string toCurrency, out decimal convertedAmount)
        {
            LogInfo($"Converting {inputAmount} from {fromCurrency} to {toCurrency}");

            convertedAmount = inputAmount;

            if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                LogInfo("Skipping converting currency. fromCurrency is the same as toCurrency");
                return true;
            }

            LogInfo("Getting currency exchange rate");
            if (!TryGetCurrencyExchangeRate(fromCurrency.ToLower(), toCurrency.ToLower(), out decimal exchangeRate))
            {
                LogError("Unable to get exchange rate.");
                return false;
            }

            convertedAmount = inputAmount / exchangeRate;
            LogInfo($"Successfully retrieved converted amount: {convertedAmount}");
            return true;
        }

        public bool TryGetCurrencyExchangeRate(string fromCurrency, string toCurrency, out decimal exchangeRate)
        {
            LogInfo($"Getting exchange rate for {fromCurrency} to {toCurrency}");
            exchangeRate = -1;

            try
            {
                // Use cached exchange rates if still valid and for the same toCurrency
                if (_cachedExchangeRates == null ||
                    _cachedExchangeRateCurrency != toCurrency ||
                    DateTime.UtcNow > _cachedExchangeRatesExpiry)
                {
                    LogInfo("Cache miss for exchange rates, fetching from API");

                    string apiVersion = GetCachedApiVersion();
                    if (string.IsNullOrEmpty(apiVersion))
                    {
                        LogError("Failed to fetch the latest currency API version.");
                        return false;
                    }

                    string url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@{apiVersion}/v1/currencies/{toCurrency}.json";
                    LogInfo($"Fetching exchange rates from: {url}");

                    string json = _httpClient.GetStringAsync(url).Result;
                    _cachedExchangeRates = JObject.Parse(json);
                    _cachedExchangeRateCurrency = toCurrency;
                    _cachedExchangeRatesExpiry = DateTime.UtcNow.Add(_currencyCacheDuration);

                    LogInfo($"Exchange rates cached for {toCurrency}, expires at {_cachedExchangeRatesExpiry}");
                }
                else
                {
                    LogDebug($"Using cached exchange rates for {toCurrency}");
                }

                if (_cachedExchangeRates[toCurrency]?[fromCurrency] == null)
                {
                    LogError($"Exchange rate for {fromCurrency} to {toCurrency} not found in the API response.");
                    return false;
                }

                exchangeRate = (decimal)_cachedExchangeRates[toCurrency][fromCurrency];
                LogInfo($"Successfully retrieved exchange rate: {fromCurrency} to {toCurrency} = {exchangeRate}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error occurred while fetching exchange rate: {ex.Message}");
                return false;
            }
        }

        private string GetCachedApiVersion()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cachedCurrencyApiVersion) &&
                    DateTime.UtcNow < _cachedCurrencyApiVersionExpiry)
                {
                    LogDebug($"Using cached API version: {_cachedCurrencyApiVersion}");
                    return _cachedCurrencyApiVersion;
                }

                LogInfo("Cache miss for API version, fetching from jsdelivr");
                string json = _httpClient.GetStringAsync("https://data.jsdelivr.com/v1/package/npm/@fawazahmed0/currency-api").Result;
                _cachedCurrencyApiVersion = JObject.Parse(json)["tags"]?["latest"]?.ToString();
                _cachedCurrencyApiVersionExpiry = DateTime.UtcNow.Add(_currencyCacheDuration);

                LogInfo($"API version cached: {_cachedCurrencyApiVersion}, expires at {_cachedCurrencyApiVersionExpiry}");
                return _cachedCurrencyApiVersion;
            }
            catch (Exception ex)
            {
                LogError($"Error fetching API version: {ex.Message}");
                return null;
            }
        }

        public bool GetLocalCurrencyCode(out string localCurrencyCode)
        {
            LogInfo("Getting local currency code from Streamer.Bot globals");

            localCurrencyCode = _CPH.GetGlobalVar<string>("sup000_LocalCurrency", true);
            if (string.IsNullOrEmpty(localCurrencyCode))
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

            RegionInfo region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(ci => new RegionInfo(ci.LCID))
                .FirstOrDefault(ri => ri.ISOCurrencySymbol.Equals(inputCurrencyCode, StringComparison.OrdinalIgnoreCase));

            if (region != null)
            {
                currencySymbol = region.CurrencySymbol;
                LogInfo($"Successfully retrieved currency symbol: {currencySymbol}");
                return true;
            }

            currencySymbol = inputCurrencyCode;
            LogError($"Currency symbol for {inputCurrencyCode} not found, defaulting to currency code.");
            return false;
        }

        public string StringifyCurrencyWithSymbol(decimal amount, string currencyCode)
        {
            LogInfo("Adding symbol to currency amount and outputting as string");

            if (!GetCurrencySymbol(currencyCode, out string currencySymbol))
            {
                LogError($"Unable to get currency symbol for {currencyCode}. Using currency code as symbol.");
                currencySymbol = currencyCode;
            }

            return $"{currencySymbol}{amount:F2}";
        }
    }
}
