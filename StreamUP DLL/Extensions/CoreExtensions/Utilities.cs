using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Utilities
        public string GetStreamerBotFolder()
        {
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

        public bool GetProductObsVersion(string sceneName, string sourceName, int obsConnection, out string productVersion)
        {
            LogInfo($"Getting product version number for scene [{sceneName}]");

            // Check scene exists
            if (!GetObsSceneExists(sceneName, obsConnection))
            {
                LogError($"Scene [{sceneName}] doesn't exist in OBS");
                productVersion = null;
                return false;
            }

            // Get settings for source
            if (!GetObsSourceSettings(sourceName, obsConnection, out JObject sourceSettings))
            {
                LogError($"Unable to pull source settings for source [{sourceName}]");
                productVersion = null;
                return false;
            }

            // Check sourceSettings contains product_version
            string versionNumber = sourceSettings["product_version"].ToString();
            if (string.IsNullOrEmpty(versionNumber))
            {
                LogError("No product_version setting found");
                productVersion = null;
                return false;
            }

            productVersion = versionNumber;
            LogInfo($"Successfully retrieved product [{sceneName}] version number [{sourceName}] - [{versionNumber}]");
            return true;
        }

        public bool SetProductObsVersion(string sourceName, string versionNumber, int obsConnection)
        {
            LogInfo($"Setting product OBS version [{versionNumber}] to source [{sourceName}]");

            // Create a JObject for product_version
            JObject productVersionJson = new JObject
            {
                { "product_version", versionNumber }
            };

            SetObsSourceSettings(sourceName, productVersionJson, obsConnection);
            

            LogInfo($"Successfully set product OBS version");
            return true;
        }

    }
}
