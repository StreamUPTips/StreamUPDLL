using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchAnnouncementHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "messageStripped", "No arg 'messageStripped' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);
            return triggerData;
        }
    }
}
