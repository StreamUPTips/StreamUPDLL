using System;
using System.Collections.Generic;
using System.Linq;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public List<UserVariableValue<string>> GetAllPointUsers(string varName = "points")
        {
            List<UserVariableValue<string>> userScores = new List<UserVariableValue<string>>();

            userScores.AddRange(_CPH.GetTwitchUsersVar<string>(varName, true));
            userScores.AddRange(_CPH.GetYouTubeUsersVar<string>(varName, true));

            return userScores;
        }

        public List<LeaderboardUser> GetLeaderboardUsers(List<UserVariableValue<long>> currentUsers, List<GroupUser> excludedUsers)
        {
            // Sort the currentUsers list by Points property in descending order
            currentUsers = currentUsers.OrderByDescending(u => u.Value).ToList();

            List<LeaderboardUser> users = new List<LeaderboardUser>();
            int position = 1;
            foreach (UserVariableValue<long> user in currentUsers)
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
                    LogInfo($"[Leaderboard - Points] Added {user.UserName} {user.UserType} => {user.Value}");
                    position++; // Increment position only when adding to leaderboard
                }
                else
                {
                    LogInfo($"[Leaderboard - Points] WRN {user.UserType}-{user.UserId} - {user.UserName} In Group Not Added!");
                }
            }

            return users;
        }

        public List<YouTubeUser> GetYouTubeUsers(string input, string varName = "points")
        {
            List<YouTubeUser> users = new List<YouTubeUser>();
            List<UserVariableValue<string>> userPointsList = _CPH.GetYouTubeUsersVar<string>(varName, true);
            foreach (UserVariableValue<string> user in userPointsList)
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
                    LogInfo($"[YouTubeUser Found] {user.UserId} => {user.UserName}");

                }
            }

            return users;
        }

        public bool AddTimedPoints(long pointsToAdd, string platformString, string varName = "points")
        {
            Enum.TryParse(platformString, true, out Platform platform);
            LogDebug($"[Points Admin] [Present Viewers] Starting Present Viewers from {platform}");
            _CPH.TryGetArg("users", out List<Dictionary<string, object>> users);
            string userId;
            for (int i = 0; i < users.Count; i++)
            {
                userId = users[i]["id"].ToString();
                AddUserPointsById(userId, platform, pointsToAdd, varName);
            }

            return true;
        }

        public void ResetGroupUsers(List<GroupUser> users, string varName = "points")
        {
            foreach (GroupUser user in users)
            {
                Enum.TryParse(user.Type, true, out Platform platform);
                SetUserPointsById(user.Id, platform, 0, varName);
                LogInfo($"[Group Excluded] {user.Username}-{user.Type} has been reset to 0");
            }

        }

        public int ResetAllUserPoints(string varName = "points")
        {
            List<UserVariableValue<string>> userScores = GetAllPointUsers(varName);
            _CPH.Wait(100);
            _CPH.UnsetAllUsersVar(varName, true);
            LogDebug("[Currency System Logging] Points have been reset");
            return userScores.Count;

        }

        public int AddPointsToAllUsers(long pointsToAdd, string varName = "points")
        {
            List<UserVariableValue<string>> userScores = GetAllPointUsers(varName);
            foreach (UserVariableValue<string> user in userScores)
            {
                Enum.TryParse(user.UserType, true, out Platform platform);
                AddUserPointsById(user.UserId, platform, pointsToAdd, varName);
            }
            return userScores.Count;
        }

        public int SetPointsToAllUsers(long pointsToSet, string varName = "points")
        {
            List<UserVariableValue<string>> userScores = GetAllPointUsers(varName);
            foreach (UserVariableValue<string> user in userScores)
            {
                Enum.TryParse(user.UserType, true, out Platform platform);
                SetUserPointsById(user.UserId, platform, pointsToSet,varName);
            }
            return userScores.Count;
        }





        public bool DefaultPointHandler(string userId, Platform platform, long points, out long newPoints, string varName = "points")
        {

            SetUserPointsById(userId, platform, points, varName);
            LogDebug($"User - {userId} fell below mark of points, rounding back up to {points}");
            newPoints = points;
            return true;
        }
    }
}