using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
       public T GetAlternateSetting<T>(string settingLocation, string key, T defaultValue)
        {
            //LogInfo($"Trying to Get {key} with default of {defaultValue}, Type = {typeof(T)}");
            Dictionary<string, object> json = _CPH.GetGlobalVar<Dictionary<string, object>?>(settingLocation, true) ?? new Dictionary<string, object>();

            if (json.TryGetValue(key, out var jsonValue))
            {
                try
                {
                    if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(double) || typeof(T) == typeof(bool) || typeof(T) == typeof(long))
                    {
                        return (T)Convert.ChangeType(jsonValue, typeof(T));
                    }
                    else if (typeof(T) == typeof(Dictionary<string, bool>) ||
                             typeof(T) == typeof(Dictionary<string, string>) ||
                             typeof(T) == typeof(Dictionary<string, int>) ||
                             typeof(T) == typeof(List<(string Emote, int Payout, int Percentage)>)
                             )
                    {
                        return JsonConvert.DeserializeObject<T>(jsonValue.ToString());
                    }
                    else if (typeof(T) == typeof(List<string>))
                    {
                        // Deserialize JSON to List<string>
                        JArray jArray = JArray.Parse(jsonValue.ToString());
                        List<string> returnedList = jArray.ToObject<List<string>>();
                        return (T)(object)returnedList;
                    }
                    else
                    {
                        LogInfo($"Unsupported type: {typeof(T)}. Returning default value.");
                        return defaultValue;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error during conversion or deserialization: {ex.Message}");
                    return defaultValue;
                }
            }
            else
            {
                LogInfo($"Key '{key}' not found. Returning default value.");
                return defaultValue;
            }
        }
    }
}