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
        private readonly Random _random;
        // Kick stores booleans as "True"/"False" (C# default) instead of valid JSON "true"/"false",
        // causing Newtonsoft to throw when deserialising. This helper safely converts the raw result.
        private T SafeConvertUserVar<T>(object result, T defaultValue)
        {
            if (result is null)
                return defaultValue;

            try
            {
                // Handle Kick's "True"/"False" boolean quirk
                if (typeof(T) == typeof(bool) && result is string strResult)
                {
                    if (bool.TryParse(strResult, out bool boolValue))
                        return (T)(object)boolValue;
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                _CPH.LogError($"Failed to convert user variable value '{result}' to type {typeof(T)}: {ex.Message}. Returning default.");
                return defaultValue;
            }
        }

        public T GetUserVariable<T>(string userName, string varName, Platform platform, bool persisted, T defaultValue)
        {
            _CPH.LogDebug($"Getting User Variable: {varName} for UserId: {userName} on Platform: {platform} with Persisted: {persisted} And Default Value: {defaultValue} of Type: {typeof(T)}");
            object result = platform switch
            {
                Platform.Twitch => _CPH.GetTwitchUserVar<object>(userName, varName, persisted),
                Platform.YouTube => _CPH.GetYouTubeUserVar<object>(userName, varName, persisted),
                Platform.Kick => _CPH.GetKickUserVar<object>(userName, varName, persisted),
                _ => null
            };

            if (result is not null)
            {
                _CPH.LogDebug($"Retrieved User Variable: {result}");
                return SafeConvertUserVar<T>(result, defaultValue);
            }

            _CPH.LogError($"Could not retrieve or cast user variable for {platform}, returning default.");
            return defaultValue;
        }

        public T GetUserVariableById<T>(string userId, string varName, Platform platform, bool persisted, T defaultValue)
        {
            _CPH.LogDebug($"Getting User Variable: {varName} for UserId: {userId} on Platform: {platform} with Persisted: {persisted} And Default Value: {defaultValue} of Type: {typeof(T)}");
            object result = platform switch
            {
                Platform.Twitch => _CPH.GetTwitchUserVarById<object>(userId, varName, persisted),
                Platform.YouTube => _CPH.GetYouTubeUserVarById<object>(userId, varName, persisted),
                Platform.Kick => _CPH.GetKickUserVarById<object>(userId, varName, persisted),
                _ => null
            };

            if (result is not null)
            {
                _CPH.LogDebug($"Retrieved User Variable: {result}");
                return SafeConvertUserVar<T>(result, defaultValue);
            }

            _CPH.LogError($"Could not retrieve or cast user variable for {platform}, returning default.");
            return defaultValue;
        }



        public bool SetUserVariable(string userName, string varName, object value, Platform platform, bool persisted)
        {
            _CPH.LogDebug($"Setting User Variable: {varName} for UserId: {userName} on Platform: {platform} with Persisted: {persisted} And Value: {value}");

            if (platform == Platform.Twitch)
            {
                _CPH.SetTwitchUserVar(userName, varName, value, persisted);
            }
            if (platform == Platform.YouTube)
            {
                _CPH.SetYouTubeUserVar(userName, varName, value, persisted);
            }
            if (platform == Platform.Kick)
            {
                _CPH.SetKickUserVar(userName, varName, value, persisted);
            }

            _CPH.LogDebug($"Set User Variable: {varName} for UserId: {userName} on Platform: {platform} with Persisted: {persisted} And Value: {value}");
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


            if (platform == Platform.Kick)
            {
                _CPH.SetKickUserVarById(userId, varName, value, persisted);
            }

            _CPH.LogDebug($"Setting User Variable: {varName} for UserId: {userId} on Platform: {platform} with Persisted: {persisted} And Value: {value}");
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


            if (platform == Platform.Kick)
            {
                _CPH.UnsetKickUserVar(username, varName, persisted);
            }
            _CPH.LogDebug($"Unset User Variable: {varName} for UserId: {username} on Platform: {platform} with Persisted: {persisted}");
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


            if (platform == Platform.Kick)
            {
                _CPH.UnsetKickUserVarById(userId, varName, persisted);
            }
            _CPH.LogDebug($"Unset User Variable: {varName} for UserId: {userId} on Platform: {platform} with Persisted: {persisted}");
            return true;
        }


        public T GetGlobal<T>(string varName, bool persisted, T defaultValue)
        {
            object result = _CPH.GetGlobalVar<object>(varName, persisted);
            if (result is not null)
            {
                _CPH.LogDebug($"Retrieved Global Variable: {result} for VarName: {varName} with Persisted: {persisted} And Default Value: {defaultValue} of Type: {typeof(T)}");
                return (T)Convert.ChangeType(result, typeof(T));
            }

            _CPH.LogError($"Could not retrieve or cast global variable '{varName}', returning default value.");
            return defaultValue;


        }


        public TItem GetWeightedRandom<TItem>(Dictionary<TItem, double> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items collection is empty or null");

            double totalWeight = 0;
            foreach (var w in items.Values)
            {
                if (w < 0)
                    throw new ArgumentException("Weights cannot be negative");

                totalWeight += w;
            }

            if (totalWeight <= 0)
                throw new InvalidOperationException("Total weight must be greater than zero");

            double roll = _random.NextDouble() * totalWeight;

            double cumulative = 0;
            foreach (var kvp in items)
            {
                cumulative += kvp.Value;
                if (roll < cumulative)
                    return kvp.Key;
            }

            // fallback for floating point edge cases
            return items.Last().Key;
        }


        public T TryGetValueOrRandom<T>(IEnumerable<T> collection, int index = -1)
        {
            if (collection.Count() == 0)
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