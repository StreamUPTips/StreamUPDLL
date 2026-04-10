using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchShoutoutCreatedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;
            var receiverType = StreamUpLib.TwitchUserType.targetUserId;

            triggerData.Receiver = SUP.GetValueOrDefault<string>(sbArgs, "targetUserDisplayName", "No arg 'targetUserDisplayName' found");
            triggerData.ReceiverImage = SUP.GetTwitchProfilePicture(sbArgs, receiverType);
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);
            return triggerData;
        }
    }
}
