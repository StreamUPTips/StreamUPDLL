using System.Collections.Generic;

namespace StreamUP
{
    public class DonorDriveDonationHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            triggerData.Donation = true;

            // Get donation amount and currency code
            decimal inputAmount = SUP.GetValueOrDefault<decimal>(sbArgs, "amount", -1);
            string inputCurrencyCode = SUP.GetValueOrDefault<string>(sbArgs, "currency", "No arg 'currency' found");
            if (string.IsNullOrEmpty(inputCurrencyCode))
            {
                inputCurrencyCode = "USD";
            }

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
            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "donorMessage", "No arg 'donorMessage' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "donorName", "No arg 'donorName' found");

            triggerData.AmountRaw = (double)inputAmount;
            triggerData.FromCode = inputCurrencyCode;

            string donorAvatar = SUP.GetValueOrDefault<string>(sbArgs, "donorAvatarUrl", null);
            if (string.IsNullOrEmpty(donorAvatar))
            {
                donorAvatar = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRkit_tzoCTq4oI9WfM2tvtpiArtg4fc5jbag&s";
            }
            triggerData.UserImage = donorAvatar;
            return triggerData;
        }
    }
}
