using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Get YouTube User Data
        public string GetYouTubeProfilePicture(IDictionary<string, object> sbArgs)
        {
            LogInfo($"Requesting YouTube profile picture");
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

            LogInfo("Successfully retrieved user profile picture");
            return profilePictureUrl;
        }

    }
}
