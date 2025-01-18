using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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

      

        public T GetValueOrDefault<T>(IDictionary<string, object> dict, string key, T defaultValue = default)
        {
            if (dict == null)
            {
                LogError("Dictionary is null.");
                return defaultValue;
            }

            if (dict.TryGetValue(key, out var value))
            {
                LogDebug($"Key '{key}' found with value: {value} (Type: {value?.GetType()})");
                if (value is T typedValue)
                {
                    return typedValue;
                }

                // Attempt type conversion
                if (value is IConvertible)
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        LogError($"Conversion error for key '{key}' with value '{value}': {ex.Message}");
                    }
                }
                else
                {
                    LogError($"Key '{key}' value is not of type {typeof(T)} and cannot be converted.");
                }
            }
            else
            {
                LogDebug($"Key '{key}' not found in dictionary.");
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


        public string CreateGUID(int count = 12)
        {
            //Declare string for alphabet
            string randomIdString = "";
            //Loop through the ASCII characters 65 to 90
            for (int i = 1; i <= count; i++) //
            {
                int number = _CPH.Between(0, 35);
                if (number < 10)
                {
                    randomIdString += number.ToString();
                }
                else
                {
                    int letter = number + 55;
                    randomIdString += ((char)letter).ToString();
                }
            }
            return randomIdString;
        }

        


    }

    public static class NumericExtensions
    {
        public static string ToInvariantString(this double value)
        {
            return value.ToString("0.0#", CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this decimal value)
        {
            return value.ToString("0.0#", CultureInfo.InvariantCulture);
        }
    }

}
