using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Get Twitch User Data
        public string GetTwitchProfilePicture(IDictionary<string, object> sbArgs, TwitchUserType userType)
        {
            LogInfo($"Requesting Twitch profile picture for userType [{userType}]");
            string profilePictureUrl;
            
            // Get userId from userType
            if (!_CPH.TryGetArg($"{userType}", out string userId))
            {
                LogError("Unable to pull userId from userType. Setting image to StreamUP logo");
                profilePictureUrl = "https://streamup.tips/assets/StreamUp-3color-midnightcyanpink-button.png";
                return profilePictureUrl;
            }

            // Load extended user data
            TwitchUserInfoEx userInfo;
            userInfo = _CPH.TwitchGetExtendedUserInfoById(userId);
            
            // Get profile picture
            profilePictureUrl = userInfo.ProfileImageUrl;

            LogInfo("Successfully retrieved user profile picture");
            return profilePictureUrl;
        }

        public enum TwitchUserType
        {
            userId = 0,
            recipientId = 1,
            createdById = 2,
            targetUserId = 3,
            broadcastUserId = 4
        }



    }
}
