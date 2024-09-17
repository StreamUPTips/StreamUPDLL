using System.Collections.Generic;

namespace StreamUP
{
    public class YouTubeUserBannedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.BanDuration = SUP.GetValueOrDefault<int>(sbArgs, "banDuration", -1);
            triggerData.BanType = SUP.GetValueOrDefault<string>(sbArgs, "banType", "No arg 'banType' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetYouTubeProfilePicture(sbArgs);
            return triggerData;
        }
    }
}
