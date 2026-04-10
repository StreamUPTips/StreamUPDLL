using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchCheerHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;

            triggerData.Amount = SUP.GetValueOrDefault<int>(sbArgs, "bits", -1);
            triggerData.Anonymous = SUP.GetValueOrDefault<bool>(sbArgs, "anonymous", false);
            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "messageCheermotesStripped", "No arg 'messageCheermotesStripped' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);    
            return triggerData;
        }
    }
}
