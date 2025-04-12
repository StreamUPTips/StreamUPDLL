using System;
using System.Collections.Generic;
using System.Linq;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public T GetUserVariable<T>(string userName, string varName, Platform platform, bool persisted, T defaultValue)
        {
            if (platform == Platform.Twitch)
            {
                return _CPH.GetTwitchUserVar<T>(userName, varName, persisted) ?? defaultValue;
            }
            if (platform == Platform.YouTube)
            {
                return _CPH.GetYouTubeUserVar<T>(userName, varName, persisted) ?? defaultValue;
            }

            return default;
        }

        public T GetUserVariableById<T>(string userId, string varName, Platform platform, bool persisted, T defaultValue)
        {
            if (platform == Platform.Twitch)
            {
                return _CPH.GetTwitchUserVarById<T>(userId, varName, persisted) ?? defaultValue;
            }
            if (platform == Platform.YouTube)
            {
                return _CPH.GetYouTubeUserVarById<T>(userId, varName, persisted) ?? defaultValue;
            }

            return default;
        }

        public bool SetUserVariable(string userName, string varName, object value, Platform platform, bool persisted)
        {
            if (platform == Platform.Twitch)
            {
                _CPH.SetTwitchUserVar(userName, varName, value, persisted);
            }
            if (platform == Platform.YouTube)
            {
                _CPH.SetYouTubeUserVar(userName, varName, value, persisted);
            }
            return true;
        }

        public bool SetUserVariableById(string userId, string varName, object value, Platform platform, bool persisted)
        {
            if (platform == Platform.Twitch)
            {
                _CPH.SetTwitchUserVarById(userId, varName, value, persisted);
            }
            if (platform == Platform.YouTube)
            {
                _CPH.SetYouTubeUserVarById(userId, varName, value, persisted);
            }
            return true;
        }

       public bool UnsetUserVariable(string username, string varName, Platform platform, bool persisted)
       {

         if (platform == Platform.Twitch)
            {
                _CPH.UnsetTwitchUserVar(username, varName, persisted);
            }
            if (platform == Platform.YouTube)
            {
                _CPH.UnsetYouTubeUserVar(username, varName, persisted);
            }
        return true;
       }
       
        public bool UnsetUserVariableById(string userId, string varName, Platform platform, bool persisted)
       {

         if (platform == Platform.Twitch)
            {
                _CPH.UnsetTwitchUserVarById(userId, varName, persisted);
            }
            if (platform == Platform.YouTube)
            {
                _CPH.UnsetYouTubeUserVarById(userId, varName, persisted);
            }
        return true;
       }
       
       public T GetGlobal<T>(string varName, bool persisted, T defaultValue)
       {
        return _CPH.GetGlobalVar<T?>(varName,persisted) ?? defaultValue;
       }
       
       
       
       
       
       
        public T TryGetValueOrRandom<T>(IEnumerable<T> collection, int index = -1)
        {
            if(collection.Count() == 0)
            {
                LogError("Collection is Empty, Returning null");
                return default;

            }
            if (collection is IList<T> list)
            {
                // Handle lists or arrays
                if (index >= 0 && index < list.Count && list[index] != null)
                {
                    return list[index];  // Valid index, return the item
                }
                return list[random.Next(0, list.Count)];  // Return random item
            }

            if (collection is IDictionary<int, T> dictionary)
            {
                // Handle dictionaries (assuming integer keys)
                if (index >= 0 && dictionary.ContainsKey(index) && dictionary[index] != null)
                {
                    return dictionary[index];  // Valid key, return the value
                }

                // Get a random value from the dictionary
                var randomKey = random.Next(0, dictionary.Count);
                return dictionary.Values.ElementAt(randomKey);
            }

            throw new ArgumentException("Unsupported collection type");
        }

         public T TryGetArgOrDefault<T>(string key, T defaultValue = default)
        {
            if (_CPH.TryGetArg(key, out object value))
            {
                try
                {
                    if (value is T variable)
                    {
                        return variable;
                    }
                    else if (typeof(T) == typeof(string) && value is Guid guidValue)
                    {
                        return (T)(object)guidValue.ToString(); 
                    }
                    else if (value is IConvertible)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    else
                    {
                        LogError($"Key '{key}' contains a value of type '{value.GetType()}' which cannot be converted to type '{typeof(T)}'.", "Type Conversion Error");
                        return defaultValue;
                    }
                }
                catch (InvalidCastException ex)
                {
                    LogError($"Error converting key '{key}' to type '{typeof(T)}': {ex.Message}", $"{typeof(T).Name} Conversion Error");
                    return defaultValue;
                }
                catch (FormatException ex)
                {
                    LogError($"Error formatting key '{key}' to type '{typeof(T)}': {ex.Message}", $"{typeof(T).Name} Format Error");
                    return defaultValue;
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected error with key '{key}' to type '{typeof(T)}': {ex.Message}", $"{typeof(T).Name} Unexpected Error");
                    return defaultValue;
                }
            }
            else
            {
                LogError($"Key '{key}' not found in arguments.", "Key Not Found");
                return defaultValue;
            }
        }
    }
}