using System.Collections.Generic;

namespace StreamUP
{
    public class YouTubeStatisticsUpdatedHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            return triggerData;
        }
    }
}
