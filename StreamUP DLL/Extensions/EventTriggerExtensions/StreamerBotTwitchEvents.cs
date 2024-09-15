using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // StreamerBot Twitch Events

        
        public class TwitchChatMessageHandler : IEventHandler
        {
            public TriggerData HandleEvent(IDictionary<string, object> sbArgs)
            {
                var triggerData = new TriggerData();
                string userType = "userId";
                
                triggerData.Message = sbArgs["messageStripped"].ToString();
                triggerData.User = sbArgs["user"].ToString();
                triggerData.UserImage = GetTwitchProfilePicture(sbArgs, userType);
                return triggerData;
            }
        }



    }
}
