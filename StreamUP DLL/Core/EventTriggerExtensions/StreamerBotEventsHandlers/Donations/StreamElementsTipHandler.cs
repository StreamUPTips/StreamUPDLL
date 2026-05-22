using System.Collections.Generic;

namespace StreamUP
{
    public class StreamElementsTipHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            triggerData.Donation = true;

            // Get donation amount and currency code
            decimal inputAmount = SUP.GetValueOrDefault<decimal>(sbArgs, "tipAmount", -1);
            string inputCurrencyCode = SUP.GetValueOrDefault<string>(sbArgs, "tipCurrency", "No arg 'tipCurrency' found");

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

            triggerData.AmountRaw = (double)inputAmount;
            triggerData.FromCode = inputCurrencyCode;

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "tipMessage", "No arg 'tipMessage' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "tipUsername", "No arg 'tipUsername' found");
            triggerData.UserImage = "https://streamer.bot/img/integrations/streamelements.png";
            return triggerData;
        }
    }
}
