using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public long GetUserPointsById(string userId, Platform platform, string varName = "points")
        {
            long points = 0; // Default value

            try
            {
                switch (platform)
                {
                    case Platform.Twitch:
                        var twitchPoints = _CPH.GetTwitchUserVarById<long?>(userId, varName, true);
                        points = twitchPoints ?? 0;

                        break;
                    case Platform.YouTube:
                        var youtubePoints = _CPH.GetYouTubeUserVarById<long?>(userId, varName, true);
                        points = youtubePoints ?? 0;
                        break;


                }
            }
            catch (Exception e)
            {
                LogError($"[Currency Core] Error getting points value for {platform} user ({userId}) -- {e}");
                SetUserPointsById(userId,platform,0,varName);
                LogDebug($"[Currency Core] Error Pulling Points as 0, and Resetting user back to 0");
                return 0;
            }
            _CPH.SetArgument("points", points);
            LogInfo($"[Currency Core] Getting User Points from {platform}, {userId} has {points} points");
            return points;
        }

        //Get Points by Name

        public long GetUserPointsByUser(string user, Platform platform, string varName = "points")
        {
            long points = 0; // Default value

            try
            {
                switch (platform)
                {
                    case Platform.Twitch:
                        var twitchPoints = _CPH.GetTwitchUserVar<long?>(user, varName, true);
                        points = twitchPoints ?? 0;

                        break;
                    case Platform.YouTube:
                        var youtubePoints = _CPH.GetYouTubeUserVar<long?>(user, varName, true);
                        points = youtubePoints ?? 0;
                        break;
               
                }
            }
            catch (Exception e)
            {
                LogError($"[Currency Core] Error getting points value for {platform} user ({user}) -- {e}");
                SetUserPointsByUser(user,platform,0,varName);
                LogDebug($"[Currency Core] Error Pulling Points as 0");
            }
            _CPH.SetArgument("points", points);
            LogInfo($"[Currency Core] Getting User Points from {platform}, {user} has {points} points");
            return points;
        }


    }
}