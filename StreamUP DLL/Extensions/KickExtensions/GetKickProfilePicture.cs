using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Get Kick User Data
        public string GetKickProfilePicture(IDictionary<string, object> sbArgs)
        {
            LogInfo($"Requesting Kick profile picture");
            string profilePictureUrl;

            // Try get profile picture
            if (!_CPH.TryGetArg("userProfileUrl", out profilePictureUrl))
            {
                if (!_CPH.TryGetArg("profileImageUrl", out profilePictureUrl))
                {
                    LogError("Unable to pull 'userProfileUrl' or 'profileImageUrl'. Setting image to StreamUP logo");
                    profilePictureUrl = "https://avatars.githubusercontent.com/u/86125158?v=4";
                }
            }

            LogInfo("Successfully retrieved Kick user profile picture");
            return profilePictureUrl;
        }
    }
}
