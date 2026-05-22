using System.Collections.Generic;

namespace StreamUP
{
    public class YouTubeGiftMembershipReceivedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.Receiver = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.Tier = SUP.GetValueOrDefault<string>(sbArgs, "tier", "No arg 'tier' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "gifterUser", "No arg 'gifterUser' found");
            triggerData.UserImage = SUP.GetYouTubeProfilePicture(sbArgs);
            return triggerData;
        }
    }
}
