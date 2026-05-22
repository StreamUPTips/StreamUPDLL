using System.Collections.Generic;

namespace StreamUP
{
    public class YouTubeSuperStickerHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            // Get trigger data
            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "message", "No arg 'message' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetYouTubeProfilePicture(sbArgs);

            // Get superchat amount and currency code
            decimal inputAmount = SUP.GetValueOrDefault<decimal>(sbArgs, "microAmount", -1) / 1000000;
            string inputCurrencyCode = SUP.GetValueOrDefault<string>(sbArgs, "currencyCode", "No arg 'currencyCode' found");

            // Get local currency
            bool gotLocalCurrency = SUP.GetLocalCurrencyCode(out string localCurrencyCode);

            // Handle skipped conversion or conversion
            decimal finalAmount;
            string finalCurrencyCode;

            // If local currency is not found or is the same as input currency, skip the conversion
            if (!gotLocalCurrency || localCurrencyCode == inputCurrencyCode)
            {
                SUP.LogError("No local currency variable found or local currency is the same as input currency. Skipping conversion.");
                finalAmount = inputAmount;
                finalCurrencyCode = inputCurrencyCode;
            }
            else
            {
                // Try to convert input currency to local currency
                if (!SUP.ConvertCurrency(inputAmount, inputCurrencyCode, localCurrencyCode, out decimal convertedAmount))
                {
                    SUP.LogError($"Unable to convert input currency. Returning input currency amount.");
                    finalAmount = inputAmount;
                    finalCurrencyCode = inputCurrencyCode;
                }
                else
                {
                    finalAmount = convertedAmount;
                    finalCurrencyCode = localCurrencyCode;
                }
            }

            // Get the appropriate currency symbol and format the final amount
            triggerData.AmountCurrency = SUP.StringifyCurrencyWithSymbol(finalAmount, finalCurrencyCode);
            triggerData.AmountCurrencyDecimal = finalAmount;
            triggerData.AmountCurrencyDouble = (double)finalAmount;

            return triggerData;
        }
    }
}
