using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public class CustomTrigger
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public string[] TriggerCategory { get; set; }

        public CustomTrigger(string name, string code, string[] triggerCategory)
        {
            Name = name;
            Code = code;
            TriggerCategory = triggerCategory;
        }

    }
    public partial class StreamUpLib
    {

        public void SetCustomTriggers(List<CustomTrigger> customTriggers)
        {
            foreach (var customTrigger in customTriggers)
            {
                _CPH.RegisterCustomTrigger(customTrigger.Name, customTrigger.Code, customTrigger.TriggerCategory);
                LogInfo($"[Custom Triggers] {customTrigger.Name}, {customTrigger.Code}");
            }
        }
        /*
        public void SetTriggersForXXXXX()
        {
            string[] categories = { "StreamUP", "XXXX" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("NAME", "event", categories),
            };
            SetCustomTriggers(customTriggers);
        }
        */
    }
}