using System.Collections.Generic;

namespace StreamUP
{
    public class KickPresentViewersHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            triggerData.Amount = SUP.GetValueOrDefault<int>(sbArgs, "viewerCount", -1);
            return triggerData;
        }
    }
}
