using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public string EventTriggerVarReplacement(string message, TriggerData triggerData)
        {
            // Create a dictionary to map placeholders to their corresponding properties or values
            var replacements = new Dictionary<string, string>
            {
                { "watchStreak", triggerData.Amount.ToString() ?? string.Empty },
                { "amount", triggerData.Amount.ToString() ?? string.Empty },
                { "amountCurrency", triggerData.AmountCurrency?.ToString() ?? string.Empty }, 
                { "reason", triggerData.BanType ?? string.Empty },                          
                { "duration", triggerData.BanDuration.ToString() ?? string.Empty },
                { "user", triggerData.User ?? string.Empty },                               
                { "message", triggerData.Message ?? string.Empty },                         
                { "monthsStreak", triggerData.MonthsStreak.ToString() ?? string.Empty },
                { "monthsTotal", triggerData.MonthsTotal.ToString() ?? string.Empty },
                { "receiver", triggerData.Receiver ?? string.Empty },                       
                { "tier", triggerData.Tier ?? string.Empty },                               
                { "monthsGifted", triggerData.TotalAmount.ToString() ?? string.Empty },
                { "totalAmount", triggerData.TotalAmount.ToString() ?? string.Empty },
                { "monthDuration", triggerData.MonthDuration.ToString() ?? string.Empty },
                { "monthTenure", triggerData.MonthTenure.ToString() ?? string.Empty }
            };

            // Use a Regex to find all placeholders in the message (like %bits%, %user%, etc.)
            Regex regex = new Regex("%(.*?)%");

            // Replace each placeholder with the corresponding value from the dictionary
            string result = regex.Replace(message, match =>
            {
                string varName = match.Groups[1].Value; // Get the variable name inside % %

                // If the variable is found in the dictionary, replace it with the corresponding value
                if (replacements.ContainsKey(varName))
                {
                    return replacements[varName] ?? string.Empty; // Return the value or empty string if null
                }

                // If the variable is not found, leave it unchanged
                return match.Value;
            });

            return result;
        }


    }
}
