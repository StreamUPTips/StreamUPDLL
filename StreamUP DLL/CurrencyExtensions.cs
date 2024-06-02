using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Enums;
using System;
using Streamer.bot.Plugin.Interface.Model;
using System.Collections.Generic;
using System.Linq;
using Streamer.bot.Common.Events;
using System.Text.RegularExpressions;

namespace StreamUP
{

    public class LeaderboardUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public long Points { get; set; }
        public int Position { get; set; }
    }

    public class YouTubeUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

    }


    public static class Currency
    {
        //Add Points by Id
        public static long AddUserPointsById(this IInlineInvokeProxy CPH, string userId, Platform platform, long pointsToAdd)
        {

            long points = CPH.GetUserPointsById(userId, platform);

            long newPoints = points + pointsToAdd;
            if (newPoints <= -1)
            {
                CPH.SUWriteLog($"[Currency Add] Points Under 0 Error Resetting to 0");
                newPoints = 0;
            }
            CPH.SUWriteLog($"[Currency Add] {points} + {pointsToAdd} = {newPoints}");

            CPH.SetUserPointsById(userId, platform, newPoints);

            CPH.SetArgument("oldPoints", points);
            CPH.SetArgument("newPoints", newPoints);

            return newPoints;

        }

        //Add Points by Usernames
        public static long AddUserPointsByUser(this IInlineInvokeProxy CPH, string user, Platform platform, long pointsToAdd)
        {
            long points = CPH.GetUserPointsByUser(user, platform);

            long newPoints = points + pointsToAdd;
            CPH.SUWriteLog($"[Currency Add] {points} + {pointsToAdd} = {newPoints}");
            CPH.SetUserPointsByUser(user, platform, newPoints);


            CPH.SetArgument("oldPoints", points);
            CPH.SetArgument("newPoints", newPoints);
            return newPoints;


        }

        //Points Reset
        public static int ResetAllUserPoints(this IInlineInvokeProxy CPH)
        {
            List<UserVariableValue<long>> userScores = CPH.GetAllPointUsers();
            CPH.Wait(100);
            CPH.UnsetAllUsersVar("points", true);
            CPH.SUWriteLog("[Currency System Logging] Points have been reset");
            return userScores.Count;

        }

        //Get Users Points by Id
        public static long GetUserPointsById(this IInlineInvokeProxy CPH, string userId, Platform platform)
        {
            long points = 0; // Default value

            try
            {
                switch (platform)
                {
                    case Platform.Twitch:
                        var twitchPoints = CPH.GetTwitchUserVarById<long?>(userId, "points", true);
                        points = twitchPoints ?? 0;

                        break;
                    case Platform.YouTube:
                        var youtubePoints = CPH.GetYouTubeUserVarById<long?>(userId, "points", true);
                        points = youtubePoints ?? 0;
                        break;
                    case Platform.Trovo:
                        var trovoPoints = CPH.GetTrovoUserVarById<long?>(userId, "points", true);
                        points = trovoPoints ?? 0;
                        break;

                }
            }
            catch (Exception e)
            {
                CPH.SUWriteLog($"[Currency Core] Error getting points value for {platform} user ({userId}) -- {e}");
                CPH.SUWriteLog($"[Currency Core] Error Pulling Points as 0");
                return 0;
            }
            CPH.SetArgument("points", points);
            CPH.SUWriteLog($"[Currency Get] {platform}/{userId} = {points}");
            return points;
        }

        //Get Points by Name

        public static long GetUserPointsByUser(this IInlineInvokeProxy CPH, string user, Platform platform)
        {
            long points = 0; // Default value

            try
            {
                switch (platform)
                {
                    case Platform.Twitch:
                        var twitchPoints = CPH.GetTwitchUserVar<long?>(user, "points", true);
                        points = twitchPoints ?? 0;

                        break;
                    case Platform.YouTube:
                        var youtubePoints = CPH.GetYouTubeUserVar<long?>(user, "points", true);
                        points = youtubePoints ?? 0;
                        break;
                    case Platform.Trovo:
                        var trovoPoints = CPH.GetTrovoUserVar<long?>(user, "points", true);
                        points = trovoPoints ?? 0;
                        break;

                }
            }
            catch (Exception e)
            {
                CPH.SUWriteLog($"[Currency Core] Error getting points value for {platform} user ({user}) -- {e}");
                CPH.SUWriteLog($"[Currency Core] Error Pulling Points as 0");
            }
            CPH.SetArgument("points", points);
            CPH.SUWriteLog($"[Currency Get] {platform}/{user} = {points}");
            return points;
        }

        //Set Points By ID
        public static void SetUserPointsById(this IInlineInvokeProxy CPH, string userId, Platform platform, long points)
        {

            long oldPoints = CPH.GetUserPointsById(userId, platform);
            switch (platform)
            {
                case Platform.Twitch:
                    CPH.SetTwitchUserVarById(userId, "points", points, true);
                    break;
                case Platform.YouTube:
                    CPH.SetYouTubeUserVarById(userId, "points", points, true);
                    break;
                case Platform.Trovo:
                    CPH.SetTrovoUserVarById(userId, "points", points, true);
                    break;

            }
            CPH.SetArgument("oldPoints", oldPoints);
            CPH.SetArgument("newPoints", points);
            CPH.SUWriteLog($"[Currency Core] Set user points to {userId} => {platform} => {points}");
        }

        //Set Points by Name
        public static void SetUserPointsByUser(this IInlineInvokeProxy CPH, string user, Platform platform, long points)
        {

            long oldPoints = CPH.GetUserPointsByUser(user, platform);

            switch (platform)
            {
                case Platform.Twitch:
                    CPH.SetTwitchUserVar(user, "points", points, true);
                    break;
                case Platform.YouTube:
                    CPH.SetYouTubeUserVar(user, "points", points, true);
                    break;
                case Platform.Trovo:
                    CPH.SetTrovoUserVar(user, "points", points, true);
                    break;

            }
            CPH.SetArgument("oldPoints", oldPoints);
            CPH.SetArgument("newPoints", points);
            CPH.SUWriteLog($"[Currency Core] Set user points to {user} => {platform} => {points}");

        }

        //Reset Certain Users
        public static void ResetGroupUsers(this IInlineInvokeProxy CPH, List<GroupUser> users)
        {
            foreach (GroupUser user in users)
            {
                Enum.TryParse(user.Type, true, out Platform platform);
                CPH.SetUserPointsById(user.Id, platform, 0);
                CPH.SUWriteLog($"[Group Excluded] {user.Username}-{user.Type} has been reset to 0");
            }

        }

        public static bool AddTimedPoints(this IInlineInvokeProxy CPH, long pointsToAdd, string platformString)
        {
            Enum.TryParse(platformString, true, out Platform platform);
            CPH.SUWriteLog($"[Points Admin] [Present Viewers] Starting Present Viewers from {platform}");
            CPH.TryGetArg("users", out List<Dictionary<string, object>> users);
            string userId;
            for (int i = 0; i < users.Count; i++)
            {
                userId = users[i]["id"].ToString();
                CPH.AddUserPointsById(userId, platform, pointsToAdd);
            }

            return true;
        }

        public static List<LeaderboardUser> GetLeaderboardUsers(this IInlineInvokeProxy CPH, List<UserVariableValue<object>> currentUsers, List<GroupUser> excludedUsers)
        {
            // Sort the currentUsers list by Points property in descending order
            currentUsers = currentUsers.OrderByDescending(u => u.Value).ToList();

            List<LeaderboardUser> users = new List<LeaderboardUser>();
            int position = 1;
            foreach (UserVariableValue<object> user in currentUsers)
            {
                if (!excludedUsers.Any(u => u.Type == user.UserType && u.Id == user.UserId))
                {
                    LeaderboardUser leaderboardUser = new LeaderboardUser
                    {
                        UserId = user.UserId,
                        UserName = user.UserName, // Assuming you have UserName property in UserVariableValue<long>
                        UserType = user.UserType,
                        Points = (long)user.Value, // Assuming the points are stored in the Value property of UserVariableValue<long>
                        Position = position // Assign the current position
                    };

                    users.Add(leaderboardUser);
                    CPH.SUWriteLog($"[Leaderboard - Points] Added {user.UserName} {user.UserType} => {user.Value}");
                    position++; // Increment position only when adding to leaderboard
                }
                else
                {
                    CPH.SUWriteLog($"[Leaderboard - Points] WRN {user.UserType}-{user.UserId} - {user.UserName} In Group Not Added!");
                }
            }

            return users;
        }

        public static List<YouTubeUser> GetYouTubeUsers(this IInlineInvokeProxy CPH, string input)
        {
            List<YouTubeUser> users = new List<YouTubeUser>();
            List<UserVariableValue<long>> userPointsList = CPH.GetYouTubeUsersVar<long>("points", true);
            foreach (UserVariableValue<long> user in userPointsList)
            {
                string username = user.UserLogin;
                if (input.ToLower() == username.ToLower())
                {
                    YouTubeUser youtubeUser = new YouTubeUser
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                    };
                    users.Add(youtubeUser);
                    CPH.SUWriteLog($"[YouTubeUser Found] {user.UserId} => {user.UserName}");

                }
            }

            return users;
        }           

        public static bool TwitchEventsAddPoints(this IInlineInvokeProxy CPH)
        {

            CPH.TryGetArg("userId", out string userId);
            CPH.TryGetArg("user", out string user);
            Platform platform = Platform.Twitch;
            EventType eventName = CPH.GetEventType();
            CPH.SUWriteLog($"[Twitch Events] - [1] '{user}'/'{userId}' '{eventName}' Started");
            switch (eventName)
            {
                case EventType.TwitchCheer:
                    {
                        CPH.SUWriteLog($"[Twitch Events] - [2] '{user}' Cheer Ran");
                        CPH.TryGetArg("pointsPerBit", out long pointsPerBit);
                        CPH.SUWriteLog($"{pointsPerBit}");
                        CPH.TryGetArg("bits", out int bits);
                        long pointsToAdd = pointsPerBit * bits;
                        CPH.SUWriteLog($"{pointsPerBit} * {bits} = {pointsToAdd}");
                        CPH.AddUserPointsById(userId, platform, pointsToAdd);
                        break;
                    }

                case EventType.TwitchSub:
                    {
                        CPH.SUWriteLog($"[Twitch Events] - [2] '{user}' Sub Ran");
                        CPH.TryGetArg("tier", out string tier);
                        CPH.TryGetArg("pointsForSubTier1", out long subBaseReward);

                        if (tier == "tier 2")
                        {
                            CPH.TryGetArg("pointsForSubTier2", out subBaseReward);
                        }

                        if (tier == "tier 3")
                        {
                            CPH.TryGetArg("pointsForSubTier3", out subBaseReward);
                        }


                        CPH.TryGetArg("pointsPerMonthSubbed", out long monthReward);
                        long pointsToAdd = subBaseReward + monthReward;
                        CPH.AddUserPointsById(userId.ToString(), platform, pointsToAdd);
                        break;
                    }
                    /*
                                case EventType.TwitchReSub:
                                {
                                    CPH.PointsLog($"[Twitch Events] - [2] '{user}' ReSub Ran");
                                    int subBaseReward = int.Parse(args["pointsForSub"].ToString());
                                    int monthReward = int.Parse(args["pointsPerMonthSubbed"].ToString());
                                    int monthsSubbed = int.Parse(args["cumulative"].ToString());
                                    int pointsToAdd = subBaseReward + (monthReward * monthsSubbed);
                                    CPH.AddUserPointsById(userId, platform, pointsToAdd);
                                    break;
                                }

                                case EventType.TwitchGiftSub:
                                {
                                    CPH.PointsLog($"[Twitch Events] - [2] '{user}' GiftSub Ran");
                                    int pointsToAdd = int.Parse(args["pointsPerGiftSub"].ToString());
                                    CPH.AddUserPointsById(userId, platform, pointsToAdd);
                                    break;
                                }

                                case EventType.TwitchGiftBomb:
                                {
                                    CPH.PointsLog($"[Twitch Events] - [2] '{user}' GiftBomb Ran");
                                    int giftSubBase = int.Parse(args["pointsPerGiftSub"].ToString());
                                    int giftSubAmount = int.Parse(args["gifts"].ToString());
                                    int pointsToAdd = giftSubBase * giftSubAmount;
                                    CPH.AddUserPointsById(userId, platform, pointsToAdd);
                                    break;
                                }

                                case EventType.TwitchFollow:
                                {
                                    CPH.PointsLog($"[Twitch Events] - [2] '{user}' Follow Ran");
                                    int pointsToAdd = int.Parse(args["pointsForFollow"].ToString());
                                    CPH.AddUserPointsById(userId, platform, pointsToAdd);
                                    break;
                                }

                                case EventType.TwitchRaid:
                                {
                                    CPH.PointsLog($"[Twitch Events] - [2] '{user}' Raid Ran");
                                    int raidBaseReward = int.Parse(args["pointsForRaidBase"].ToString());
                                    int viewerBonus = int.Parse(args["pointsPerViewerInRaid"].ToString());
                                    int viewers = int.Parse(args["viewers"].ToString());
                                    int pointsToAdd = raidBaseReward + (viewers * viewerBonus);
                                    CPH.AddUserPointsById(userId, platform, pointsToAdd);
                                    break;
                                }
                                */
            }



            return true;

        }

        public static int AddPointsToAllUsers(this IInlineInvokeProxy CPH, long pointsToAdd)
        {
            List<UserVariableValue<long>> userScores = CPH.GetAllPointUsers();
            foreach (UserVariableValue<long> user in userScores)
            {
                Enum.TryParse(user.UserType, true, out Platform platform);
                CPH.AddUserPointsById(user.UserId, platform, pointsToAdd);
            }
            return userScores.Count;
        }

        public static int SetPointsToAllUsers(this IInlineInvokeProxy CPH, long pointsToSet)
        {
            List<UserVariableValue<long>> userScores = CPH.GetAllPointUsers();
            foreach (UserVariableValue<long> user in userScores)
            {
                Enum.TryParse(user.UserType, true, out Platform platform);
                CPH.SetUserPointsById(user.UserId, platform, pointsToSet);
            }
            return userScores.Count;
        }

        public static List<UserVariableValue<long>> GetAllPointUsers(this IInlineInvokeProxy CPH)
        {

            List<UserVariableValue<long>> userScores = new List<UserVariableValue<long>>();
                    
                userScores.AddRange(CPH.GetTwitchUsersVar<long>("points", true));
                userScores.AddRange(CPH.GetYouTubeUsersVar<long>("points", true));
                //.. CPH.GetTrovoUsersVar<long>("points", true),
            
            return userScores;
        }
       
        public static bool CurrencyError(this IInlineInvokeProxy CPH, int code, string message, string game = "Points Games")
        {

            CPH.SetArgument("failReason", message);
            CPH.SetArgument("failCode", code);
            CPH.SUWriteLog($"[{game}] Error - {code} - {message}");
            CPH.TriggerCodeEvent("currencyFail");
           
            return true;
        }


        public static long GetBetSize(this IInlineInvokeProxy CPH, string input, long currentPoints, long minBet, long maxBet, long defaultPoints, long defaultBet)
        {
           
            if (maxBet < 1)
            {
                maxBet = long.MaxValue;
            }

            long betSize = 0;

            if (input.ToLower() == "all")
            {
                betSize = currentPoints;
            }

            else if (long.TryParse(input, out long num))
            {
                betSize = Math.Abs(num); //Turns Positive if negative
            }

            else if (Regex.IsMatch(input, @"\d+%"))
            {
                int percentage = Convert.ToInt32(input.Replace("%", string.Empty));
                betSize = (long)((double)currentPoints / 100 * percentage);
            }

            else if (Regex.IsMatch(input, @"\d+[mM]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("m", string.Empty));
                betSize = value * 1000000;
            }

            else if (Regex.IsMatch(input, @"\d+[kK]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("k", string.Empty));
                betSize = value * 1000;
            }
            else
            {
                //Default Bet  if nothing entered
            }

            if (betSize == 0)
            {

                 betSize = defaultBet;
            }

            if (betSize > currentPoints)
            {
                betSize = currentPoints;
            }

            //num
            if (betSize > maxBet)
            {
                betSize = maxBet;
            }

            if (betSize < minBet)
            {
                betSize = minBet;
            }

            if (betSize > currentPoints)
            {
                betSize = 0;
            }

            if (betSize == 0)
            {
                betSize = defaultPoints;
            }


            return betSize;
        }

         public static long GetPrizeSize(this IInlineInvokeProxy CPH, string input, long minPrize, long maxPrize, long defaultPrize)
        {
            if (maxPrize < 1)
            {
                maxPrize = long.MaxValue;
            }

            long betPrize = 0;

           if (long.TryParse(input, out long num))
            {
                betPrize = Math.Abs(num); //Turns Positive if negative
            }

            else if (Regex.IsMatch(input, @"\d+[mM]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("m", string.Empty));
                betPrize = value * 1000000;
            }

            else if (Regex.IsMatch(input, @"\d+[kK]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("k", string.Empty));
                betPrize = value * 1000;
            }
            else
            {
                //Default Bet  if nothing entered
            }

            //num
            if (betPrize > maxPrize)
            {
                betPrize = maxPrize;
            }

            if (betPrize < minPrize)
            {
                betPrize = minPrize;
            }

            if (betPrize == 0)
            {
                betPrize = defaultPrize;
            }


            return betPrize;
        }


        public static bool CanUserAffordById(this IInlineInvokeProxy CPH, string userId, long cost, Platform platform)
        {
            long currentPoints = CPH.GetUserPointsById(userId, platform);
            if (currentPoints < cost)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool CanUserAffordByName(this IInlineInvokeProxy CPH, string user, long cost, Platform platform)
        {
            long currentPoints = CPH.GetUserPointsByUser(user, platform);
            if (currentPoints < cost)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        
        
    }
}

