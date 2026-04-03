using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void SetUserPointsById(string userId, Platform platform, long points, string varName = "points")
        {

            long oldPoints = GetUserPointsById(userId, platform, varName);
            _CPH.SetArgument("oldPoints", oldPoints);
            SetUserVariableById(userId, varName, points, platform, true);
            _CPH.SetArgument("newPoints", points);
            LogInfo($"[Currency Core] Set user points to {userId} => {platform} => {points}");
        }

        //Set Points by Name
        public void SetUserPointsByUser(string user, Platform platform, long points, string varName = "points")
        {

            long oldPoints = GetUserPointsByUser(user, platform, varName);
            _CPH.SetArgument("oldPoints", oldPoints);

            SetUserVariable(user, varName, points, platform, true);


            _CPH.SetArgument("newPoints", points);
            LogInfo($"[Currency Core] Set user points to {user} => {platform} => {points}");

        }

    }
}