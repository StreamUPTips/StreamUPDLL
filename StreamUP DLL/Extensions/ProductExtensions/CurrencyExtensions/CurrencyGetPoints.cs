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
            long points;
            try
            {
                var pointsVarValue = GetUserVariableById<long?>(userId,varName,platform,true,0);
                points = pointsVarValue ?? 0;
            }
            catch (Exception e)
            {
                LogError($"[Currency Core] Error getting points value for {platform} user ({userId}) -- {e}");
                SetUserVariableById(userId,varName,0,platform,true);
                LogDebug($"[Currency Core] Error Pulling Points as 0, and Resetting user back to 0");
                _CPH.SetArgument("points", 0);
                return 0;
            }
            _CPH.SetArgument("points", points);
            LogInfo($"[Currency Core] Getting User Points from {platform}, {userId} has {points} points");
            return points;
        }

        //Get Points by Name

        public long GetUserPointsByUser(string user, Platform platform, string varName = "points")
        {
            long points; // Default value

            try
            {
                var pointsVarValue = GetUserVariable<long?>(user,varName,platform,true,0);
                points = pointsVarValue ?? 0;
               
            }
            catch (Exception e)
            {
                LogError($"[Currency Core] Error getting points value for {platform} user ({user}) -- {e}");
                SetUserVariableById(user,varName,0,platform,true);
                _CPH.SetArgument("points", 0);
                LogDebug($"[Currency Core] Error Pulling Points as 0");
                return 0;
            }
            _CPH.SetArgument("points", points);
            LogInfo($"[Currency Core] Getting User Points from {platform}, {user} has {points} points");
            return points;
        }


    }
}