using System.Collections.Generic;

namespace StreamUP
{
    public class KickFollowHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetKickProfilePicture(sbArgs);
            return triggerData;
        }
    }
}
