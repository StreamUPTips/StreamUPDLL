using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void SetUserPointsById(string userId, Platform platform, long points, string varName = "points")
        {

            switch (platform)
            {
                case Platform.Twitch:
                    _CPH.SetTwitchUserVarById(userId, varName, points, true);
                    break;
                case Platform.YouTube:
                    _CPH.SetYouTubeUserVarById(userId, varName, points, true);
                    break;


            }
            long oldPoints = GetUserPointsById(userId, platform);
            _CPH.SetArgument("oldPoints", oldPoints);
            _CPH.SetArgument("newPoints", points);
            LogInfo($"[Currency Core] Set user points to {userId} => {platform} => {points}");
        }

        //Set Points by Name
        public void SetUserPointsByUser(string user, Platform platform, long points, string varName = "points")
        {


            switch (platform)
            {
                case Platform.Twitch:
                    _CPH.SetTwitchUserVar(user, varName, points, true);
                    break;
                case Platform.YouTube:
                    _CPH.SetYouTubeUserVar(user, varName, points, true);
                    break;


            }
            long oldPoints = GetUserPointsByUser(user, platform);
            _CPH.SetArgument("oldPoints", oldPoints);
            _CPH.SetArgument("newPoints", points);
            LogInfo($"[Currency Core] Set user points to {user} => {platform} => {points}");

        }

    }
}