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

        public bool SetUserVariableById(string userName, string varName, object value, Platform platform, bool persisted)
        {
            if (platform == Platform.Twitch)
            {
                _CPH.SetTwitchUserVarById(userName, varName, value, persisted);
            }
            if (platform == Platform.YouTube)
            {
                _CPH.SetYouTubeUserVarById(userName, varName, value, persisted);
            }
            return true;
        }

        public T TryGetValueOrRandom<T>(IEnumerable<T> collection, int index = -1)
        {
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
    }
}