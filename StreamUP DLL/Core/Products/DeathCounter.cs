using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
         public void SetTriggersForDeathCounter()
        {
            string[] categories = { "StreamUP", "Death Counter" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Death Added", "deathAdded", categories),
                new("Death Removed", "deathRemoved", categories),
                new("Death Reset", "deathReset", categories),
                new("Death Set", "deathSet", categories),
                new("Death Count Show", "deathCountShow", categories),
                new("Fail/Error", "deathCounterFail", categories)
            };
            SetCustomTriggers(customTriggers);
        }
    }
}