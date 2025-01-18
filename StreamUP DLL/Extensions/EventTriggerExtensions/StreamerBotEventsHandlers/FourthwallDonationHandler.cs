using System.Collections.Generic;

namespace StreamUP
{
    public class FourthwallDonationHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            triggerData.Donation = true;

            // Get donation amount and currency code
            decimal inputAmount = SUP.GetValueOrDefault<decimal>(sbArgs, "fw.amount", -1);

            string inputCurrencyCode = SUP.GetValueOrDefault<string>(sbArgs, "fw.currency", "No arg 'fw.currency' found");

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

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "fw.message", "No arg 'fw.message' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "fw.username", "No arg 'fw.username' found");
            triggerData.UserImage = "https://fourthwall.com/homepage/static/logo-aae6bab7310025c5a3da5ed8acd67a8d.png";
            return triggerData;
        }
    }
}
