using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public long AddUserPointsById(string userId, Platform platform, long pointsToAdd, string varName = "points")
        {

            long points = GetUserPointsById(userId, platform, varName);

            long newPoints = points + pointsToAdd;
            if (newPoints <= -1)
            {
                LogError($"[Currency Add] Points Under 0 Error Resetting to 0");
                newPoints = 0;
            }
            LogInfo($"[Currency Core] Adding {userId} Points => {points} + {pointsToAdd} = {newPoints}");

            SetUserPointsById(userId, platform, newPoints, varName);

            _CPH.SetArgument("oldPoints", points);
            _CPH.SetArgument("newPoints", newPoints);

            return newPoints;

        }

        //Add Points by Usernames
        public long AddUserPointsByUser(string user, Platform platform, long pointsToAdd, string varName = "points")
        {
            long points = GetUserPointsByUser(user, platform, varName);
            long newPoints = points + pointsToAdd;
            if (newPoints <= -1)
            {
                LogError($"[Currency Add] Points Under 0 Error Resetting to 0");
                newPoints = 0;
            }
            LogInfo($"[Currency Core] Adding {user} Points =>  {points} + {pointsToAdd} = {newPoints}");
            SetUserPointsByUser(user, platform, newPoints, varName);
            _CPH.SetArgument("oldPoints", points);
            _CPH.SetArgument("newPoints", newPoints);
            return newPoints;
        }


    }
}