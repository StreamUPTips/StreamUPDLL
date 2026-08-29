using System.Collections.Generic;

namespace StreamUP
{
    public class ObsDisconnectedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "message", "OBS Disconnected");
       
            return triggerData;
        }
    }
}
