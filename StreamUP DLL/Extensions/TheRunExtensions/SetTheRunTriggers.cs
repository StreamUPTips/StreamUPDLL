using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void SetTheRunTriggers()
        {
            LogInfo("Setting up TheRun.gg triggers into Streamer.Bot");

            string[] categories = { "TheRun.gg" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                // Splits
                new("Green Split", "theRunGreenSplit", categories),
                new("Red Split", "theRunRedSplit", categories),
                new("Gold Split", "theRunGoldSplit", categories),
                new("Final Split", "theRunFinalSplit", categories),

                // General
                new("Run Started", "theRunRunStarted", categories),
                new("Run Ended", "theRunRunEnded", categories),
                new("Run Reset", "theRunRunReset", categories),

                // Performance
                new("Best Pace", "theRunBestRun", categories),
                new("Top 10% Pace", "theRunTop10Total", categories),
                new("Top 10% Split", "theRunTop10Single", categories),
                new("Worst 10% Split", "theRunWorst10Single", categories),
            };
            SetCustomTriggers(customTriggers);

            LogInfo("Successfully set up TheRun.gg triggers into Streamer.Bot");
        }
    }
}
