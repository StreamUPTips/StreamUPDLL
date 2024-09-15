using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Utilities
        public string GetStreamerBotFolder() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public bool RemoveUrlFromString(string inputText, string replacementText, out string outputText)
        {
            LogInfo($"Replacing Url from [{inputText}] with [{replacementText}]");

            // This pattern matches URLs starting with http://, https://, or ftp:// followed by any characters until a space is encountered
            string urlPattern = @"\b(http|https|ftp)://\S+";
            Regex urlRegex = new Regex(urlPattern, RegexOptions.IgnoreCase);

            outputText = urlRegex.Replace(inputText, replacementText);
            LogInfo($"Successfully replaced Url. Output string: [{outputText}]");
            return true;
        }
    
        public T GetValueOrDefault<T>(IDictionary<string, object> dict, string key, T defaultValue = default)
        {
            if (dict != null && dict.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }


    }
}
