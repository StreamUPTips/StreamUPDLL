using System.Collections.Generic;

namespace StreamUP
{
    public class ObsConnectedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "message", "OBS Connected");

            return triggerData;
        }
    }
}
