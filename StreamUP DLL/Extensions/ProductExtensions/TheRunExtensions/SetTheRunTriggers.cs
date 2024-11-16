using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void SetTheRunTriggers()
        {
            LogInfo("Setting up TheRun.gg triggers into Streamer.Bot");

            // Set debug mode on
            _CPH.SetGlobalVar("sup_debugMode", true, true);

            string[] splits = { "StreamUP", "TheRun.gg", "Splits" };
            string[] general = { "StreamUP", "TheRun.gg", "General" };
            string[] performance = { "StreamUP", "TheRun.gg", "Performance" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                

                // Splits
                new("Green Split", "theRunGreenSplit", splits),
                new("Red Split", "theRunRedSplit", splits),
                new("Gold Split", "theRunGoldSplit", splits),
                new("Final Split", "theRunFinalSplit", splits),
                new("Skip Split", "theRunSkipSplit", splits),
                new("Undo Split", "theRunUndoSplit", splits),
                new("Time Loss on Split", "theRunTimeLoss", splits),
                new("Time Save on Split", "theRunTimeSave", splits),
                
                // General
                new("Run Started", "theRunRunStarted", general),
                new("Run Ended", "theRunRunEnded", general),
                new("Run Reset", "theRunRunReset", general),

                // Performance
                new("Best Pace", "theRunBestRun", performance),
                new("Top 10% Pace", "theRunTop10Total", performance),
                new("Top 10% Split", "theRunTop10Single", performance),
                new("Worst 10% Split", "theRunWorst10Single", performance),
            };
            SetCustomTriggers(customTriggers);

            LogInfo("Successfully set up TheRun.gg triggers into Streamer.Bot");
        }
    }
}
