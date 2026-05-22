using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchGiftSubHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;
            var receiverType = StreamUpLib.TwitchUserType.recipientId;

            triggerData.Anonymous = SUP.GetValueOrDefault<bool>(sbArgs, "anonymous", false);
            triggerData.MonthsTotal = SUP.GetValueOrDefault<int>(sbArgs, "cumulativeMonths", -1);
            triggerData.MonthsGifted = SUP.GetValueOrDefault<int>(sbArgs, "monthsGifted", -1);
            triggerData.Receiver = SUP.GetValueOrDefault<string>(sbArgs, "recipientUser", "No arg 'recipientUser' found");
            triggerData.ReceiverImage = SUP.GetTwitchProfilePicture(sbArgs, receiverType);
            triggerData.Tier = SUP.GetValueOrDefault<string>(sbArgs, "tier", "No arg 'tier' found");
            triggerData.TotalAmount = SUP.GetValueOrDefault<int>(sbArgs, "totalSubsGifted", -1);
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);
            
            triggerData.MonthDuration =   SUP.GetValueOrDefault<int>(sbArgs, "monthsGifted", 1);
            triggerData.IsMultiMonth = SUP.GetValueOrDefault<int>(sbArgs, "monthsGifted", 1) == 1;
            return triggerData;
        }
    }
}
