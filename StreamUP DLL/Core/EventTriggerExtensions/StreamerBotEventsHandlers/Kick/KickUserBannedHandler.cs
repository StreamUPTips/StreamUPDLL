using System.Collections.Generic;

namespace StreamUP
{
    public class KickUserBannedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            string reason = SUP.GetValueOrDefault<string>(sbArgs, "reason", string.Empty);
            triggerData.BanType = string.IsNullOrEmpty(reason) ? "No Reason" : reason;
            triggerData.Receiver = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.ReceiverImage = SUP.GetKickProfilePicture(sbArgs);
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "createdByDisplayName", "No arg 'createdByDisplayName' found");
            return triggerData;
        }
    }
}
