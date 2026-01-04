using Newtonsoft.Json.Linq;
using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Get a setting value from an already-loaded JObject (in-memory access, no file I/O).
        /// Use this for type-safe access to settings values.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="settingsData">The JObject containing settings</param>
        /// <param name="key">The setting key</param>
        /// <param name="defaultValue">Default value if key not found or conversion fails</param>
        /// <returns>The setting value or default if not found/error</returns>
        public T GetSettingFromObjectV2<T>(JObject settingsData, string key, T defaultValue)
        {
            try
            {
                if (settingsData == null || string.IsNullOrEmpty(key))
                    return defaultValue;

                JToken value = settingsData[key];
                if (value == null)
                    return defaultValue;

                LogDebug($"Retrieved setting from object: {key}");
                return value.Value<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Set a setting value in an already-loaded JObject (in-memory only, no file I/O).
        /// Use this to update values in a loaded object before saving.
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <param name="settingsData">The JObject containing settings</param>
        /// <param name="key">The setting key</param>
        /// <param name="value">The value to set</param>
        public void SetSettingInObjectV2<T>(JObject settingsData, string key, T value)
        {
            try
            {
                if (settingsData != null && !string.IsNullOrEmpty(key))
                {
                    settingsData[key] = JToken.FromObject(value);
                    LogDebug($"Set setting in object: {key}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error setting {key}: {ex.Message}");
            }
        }
    }
}
