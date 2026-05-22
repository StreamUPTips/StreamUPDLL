using System.Collections.Generic;

namespace StreamUP
{
    public class KickGiftSubscriptionHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.Anonymous = SUP.GetValueOrDefault<bool>(sbArgs, "anonymous", false);
            triggerData.MonthsGifted = SUP.GetValueOrDefault<int>(sbArgs, "monthsGifted", -1);
            triggerData.Receiver = SUP.GetValueOrDefault<string>(sbArgs, "recipientUser", "No arg 'recipientUser' found");
            triggerData.Tier = SUP.GetValueOrDefault<string>(sbArgs, "tier", "No arg 'tier' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetKickProfilePicture(sbArgs);
            return triggerData;
        }
    }
}
