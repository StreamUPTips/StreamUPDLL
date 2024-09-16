using System.Collections.Generic;

namespace StreamUP
{
    public class YouTubeNewSubscriberHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetYouTubeProfilePicture(sbArgs);
            return triggerData;
        }
    }
}
