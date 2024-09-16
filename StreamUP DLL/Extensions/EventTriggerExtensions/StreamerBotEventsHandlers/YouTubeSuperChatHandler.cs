using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StreamUP
{
    public class YouTubeSuperChatHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            // Get superchat amount and currency code
            decimal inputAmount = SUP.GetValueOrDefault<decimal>(sbArgs, "microAmount", -1) / 1000000;
            string inputCurrencyCode = SUP.GetValueOrDefault<string>(sbArgs, "currencyCode", "No arg 'currencyCode' found");

            // Get local currency
            string localCurrencyCode = SUP.GetLocalCurrencyCode();

            // Convert the amount to local currency
            if (!SUP.TryConvertCurrency(inputAmount, inputCurrencyCode, localCurrencyCode, out string convertedAmount))
            {
                SUP.LogError("Currency conversion failed. Proceeding with fallback value.");
            }

            // Parse and clean the converted amount for further calculations
            if (SUP.TryParseCurrencyAsDouble(convertedAmount, out double amountCurrencyDouble))
            {
                triggerData.AmountCurrencyDouble = amountCurrencyDouble;
            }

            // Retrieve other common information
            triggerData.AmountCurrency = convertedAmount;
            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "message", "No arg 'message' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetYouTubeProfilePicture(sbArgs);

            return triggerData;
        }
    }
}
