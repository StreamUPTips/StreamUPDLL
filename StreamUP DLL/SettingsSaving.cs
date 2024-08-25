using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            //CPH.LogInfo($"string({saveFile})2:" + CPH.GetGlobalVar<string>(saveFile, true));
            string json = CPH.GetGlobalVar<string>(saveFile, true);
            //CPH.LogInfo(json.ToString());
            return (Dictionary<string, object>)JsonConvert.DeserializeObject(json);
        }

        private static void StreamUpInternalSave(this IInlineInvokeProxy CPH)
        {
            _saveName = Path.GetFileNameWithoutExtension(_filePath);
            var json = JsonConvert.SerializeObject(_data);
            File.WriteAllText(_filePath, json);
            CPH.SetGlobalVar(_saveName, json, true);
        }

        public static void StreamUpInternalAdd(this IInlineInvokeProxy CPH, string key, object value)
        {
            _data[key] = value;
            CPH.StreamUpInternalSave();
        }

        public static void StreamUpInternalUpdate(this IInlineInvokeProxy CPH, string key, object newValue)
        {
            if (_data.ContainsKey(key))
            {
                _data[key] = newValue;
            }
            else
            {
                CPH.StreamUpInternalAdd(key, newValue);
            }

            CPH.StreamUpInternalSave();
        }

        public static T StreamUpInternalGet<T>(this IInlineInvokeProxy CPH, string key, T defaultValue)
        {
            CPH.LogInfo($"Trying to Get {key} from {_data} with default of {defaultValue} Type = {typeof(T)}");
            if (_data.TryGetValue(key, out var jsonValue))
            {
                try
                {
                    // Handle string, int, and decimal types
                    if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(decimal))
                    {
                        return (T)Convert.ChangeType(jsonValue, typeof(T));
                    }
                    // Handle Dictionary and List types
                    else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        return JsonConvert.DeserializeObject<T>(jsonValue.ToString());
                    }
                    else if (typeof(T) == typeof(List<string>))
                    {
                        var jArray = jsonValue as JArray;
                        return jArray != null ? jArray.ToObject<T>() : defaultValue;
                    }
                    else if (typeof(T) == typeof(JArray) || typeof(T) == typeof(JObject))
                    {
                        // If the expected type is JArray or JObject, return it directly
                        return (T)(object)jsonValue;
                    }
                }
                catch (Exception ex)
                {
                    // Handle or log the exception as needed
                    CPH.LogError($"Error deserializing value for key '{key}': {ex.Message}");
                }
            }

            return defaultValue;
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
    }
}
