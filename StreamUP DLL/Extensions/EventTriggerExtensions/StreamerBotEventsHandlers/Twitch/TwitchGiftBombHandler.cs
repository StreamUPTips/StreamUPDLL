using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchGiftBombHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;

            triggerData.Amount = SUP.GetValueOrDefault<int>(sbArgs, "gifts", -1);
            triggerData.Anonymous = SUP.GetValueOrDefault<bool>(sbArgs, "anonymous", false);
            triggerData.Tier = SUP.GetValueOrDefault<string>(sbArgs, "tier", "No arg 'tier' found");
            triggerData.TotalAmount = SUP.GetValueOrDefault<int>(sbArgs, "totalGifts", -1);
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);
            return triggerData;
        }
    }
}
