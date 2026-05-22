using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchUserBannedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.createdById;
            var receiverType = StreamUpLib.TwitchUserType.userId;

            string reason = SUP.GetValueOrDefault<string>(sbArgs, "reason", string.Empty);
            triggerData.BanType = string.IsNullOrEmpty(reason) ? "No Reason" : reason;
            triggerData.Receiver = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.ReceiverImage = SUP.GetTwitchProfilePicture(sbArgs, receiverType);
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "createdByDisplayName", "No arg 'createdByDisplayName' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);
            return triggerData;
        }
    }
}
