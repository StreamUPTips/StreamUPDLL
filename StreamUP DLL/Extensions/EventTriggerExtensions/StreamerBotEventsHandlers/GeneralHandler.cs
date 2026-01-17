using System.Collections.Generic;

namespace StreamUP
{
    // THIS IS EXPERIMENTAL FOR TRIGGERING ACTIONS BASED ON CHAT MESSAGES WINDOW IN STREAMER.BOT
    public class GeneralHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;

            // Get message
            triggerData.Message = SUP.GetValueOrDefault<string>(
                sbArgs,
                "message",
                "No arg 'message' found"
            );

            // Set username
            triggerData.User = SUP.GetValueOrDefault<string>(
                sbArgs,
                "displayName",
                "No arg 'displayName' found"
            );

            // Get profile picture
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);

            return triggerData;
        }
    }
}
