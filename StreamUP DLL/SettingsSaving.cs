using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP
{
    public static class SimpleDatabase
    {
        private static string _filePath;
        private static string _saveName;
        private static Dictionary<string, object> _data = new Dictionary<string, object>();

        // Static method to initialize the database
        public static void Initialize(IInlineInvokeProxy CPH, string filePath)
        {
            _filePath = filePath;
            _saveName = Path.GetFileNameWithoutExtension(_filePath);
            _data = CPH.StreamUpInternalLoad(_saveName);
        }

        private static Dictionary<string, object> StreamUpInternalLoad(this IInlineInvokeProxy CPH, string saveFile)
        {
            CPH.LogInfo($"string({saveFile})2:" + CPH.GetGlobalVar<string>(saveFile, true));

            // Initialize the dictionary safely
            Dictionary<string, object> json = CPH.GetGlobalVar<Dictionary<string, object>?>(saveFile, true) ?? new Dictionary<string, object>();

            CPH.LogInfo($"Loaded JSON: {json}");
            return json;
        }


        private static void StreamUpInternalSave(this IInlineInvokeProxy CPH)
        {
            _saveName = Path.GetFileNameWithoutExtension(_filePath);
            var json = JsonConvert.SerializeObject(_data);
            CPH.SetGlobalVar(_saveName, json, true);


            //string jsonString = JsonConvert.SerializeObject(_data, Formatting.Indented);
            var cleanedDict = DeserializeDictionary(json);
string jsonString = ConvertDictionaryToJsonString(cleanedDict);
            File.WriteAllText(_filePath, jsonString, Encoding.UTF8);
        }

        public static void StreamUpInternalUpdate(this IInlineInvokeProxy CPH, string key, object newValue)
        {
            // Convert JArray to a serializable List<string> before storing
            if (newValue is JArray jArray)
            {
                // Convert to a List<string>
                List<string> serializedList = jArray.ToObject<List<string>>();
                _data[key] = serializedList;
            }
            // Convert JObject to a Dictionary<string, object> if needed
            else if (newValue is JObject jObject)
            {
                // Convert to a Dictionary<string, object>
                Dictionary<string, object> serializedDict = jObject.ToObject<Dictionary<string, object>>();
                _data[key] = serializedDict;
            }
            // If it's a complex type, consider storing it as a JSON string
            else if (newValue is not string && !newValue.GetType().IsPrimitive)
            {
                string jsonString = JsonConvert.SerializeObject(newValue);
                _data[key] = jsonString;
            }
            else
            {
                // Store primitive types or strings directly
                _data[key] = newValue;
            }

            // Save the updated _data dictionary
            CPH.StreamUpInternalSave();
        }
        public static T StreamUpInternalGet<T>(this IInlineInvokeProxy CPH, string key, T defaultValue)
        {
            CPH.LogInfo($"Trying to Get {key} with default of {defaultValue}, Type = {typeof(T)}");

            if (_data.TryGetValue(key, out var jsonValue))
            {
                try
                {
                    if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(decimal))
                    {
                        return (T)Convert.ChangeType(jsonValue, typeof(T));
                    }
                    else if (typeof(T) == typeof(Dictionary<string, bool>) ||
                             typeof(T) == typeof(Dictionary<string, string>) ||
                             typeof(T) == typeof(Dictionary<string, int>))
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
                        CPH.LogInfo($"Unsupported type: {typeof(T)}. Returning default value.");
                        return defaultValue;
                    }
                }
                catch (Exception ex)
                {
                    CPH.LogError($"Error during conversion or deserialization: {ex.Message}");
                    return defaultValue;
                }
            }
            else
            {
                CPH.LogInfo($"Key '{key}' not found. Returning default value.");
                return defaultValue;
            }
        }


        public static void StreamUpInternalDelete(this IInlineInvokeProxy CPH, string key)
        {
            if (_data.Remove(key))
            {
                CPH.StreamUpInternalSave();
            }
        }

        public static IEnumerable<string> StreamUpInternalGetAllKeys(this IInlineInvokeProxy CPH)
        {
            return _data.Keys;
        }


        public static Dictionary<string, object> DeserializeDictionary(string jsonString)
        {
            // Deserialize the outer dictionary
            var outerDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            // Deserialize nested JSON strings within the dictionary
            var result = new Dictionary<string, object>();
            foreach (var kvp in outerDict)
            {
                if (kvp.Value is string strValue && IsJson(strValue))
                {
                    // Deserialize the nested JSON string
                    var nestedObject = JsonConvert.DeserializeObject<object>(strValue);
                    result[kvp.Key] = nestedObject;
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        // Helper function to check if a string is JSON
        private static bool IsJson(string value)
        {
            // Basic check for JSON format
            return value.StartsWith("{") || value.StartsWith("[");
        }

        public static string ConvertDictionaryToJsonString(Dictionary<string, object> dictionary)
        {
            return JsonConvert.SerializeObject(dictionary, Formatting.Indented);
        }

    }
}
