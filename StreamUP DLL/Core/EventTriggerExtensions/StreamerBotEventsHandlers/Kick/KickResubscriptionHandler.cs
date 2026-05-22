using System.Collections.Generic;

namespace StreamUP
{
    public class KickResubscriptionHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "message", "No arg 'message' found");
            triggerData.MonthsTotal = SUP.GetValueOrDefault<int>(sbArgs, "monthsSubscribed", -1);
            triggerData.Tier = SUP.GetValueOrDefault<string>(sbArgs, "tier", "No arg 'tier' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetKickProfilePicture(sbArgs);
            return triggerData;
        }
    }
}
