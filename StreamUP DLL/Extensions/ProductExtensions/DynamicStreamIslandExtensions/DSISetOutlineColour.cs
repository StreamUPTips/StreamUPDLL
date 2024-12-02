using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Streamer.bot.Common.Events;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool DSISetOutlineColour(Dictionary<string, object> productSettings, string widgetPrefix, int obsConnection)
        {
            LogDebug("Setting outline colour filters...");
            string outlineColourMode = GetValueOrDefault<string>(productSettings, "OutlineColourMode", "None");
            LogDebug($"Outline colour mode is set to: {outlineColourMode}");
            bool alert = false;
            long outlineColour = 0;

            if (outlineColourMode != "None")
            {
                // Find out if the action was triggered by a timed action
                EventType eventType = _CPH.GetEventType();
                if (eventType == EventType.TimedAction)
                {
                    // If the action was triggered by a timed action, set the outline colour to the default colour
                    outlineColour = long.Parse(GetValueOrDefault<string>(productSettings, "DefaultOutlineColourOBS", "0"));
                    LogDebug($"Action triggered by a timed action. Changed accent colour to: [{outlineColour}]");
                }
                else
                {
                    // If the action was not triggered by a timed action, set the outline colour to the custom colour
                    alert = true;
                    outlineColour = long.Parse(GetValueOrDefault<string>(productSettings, $"{widgetPrefix}WidgetOutlineColourOBS", "0"));
                    LogDebug($"Action triggered by an alert. Changed accent colour to: [{outlineColour}]");
                }
            }

            var filterSettings = new JObject();

            switch (outlineColourMode)
            {
                // If accent mode is set to Off
                case "None":
                    filterSettings.Add("setting_color_alpha", 0);
                    SetObsSourceFilterSettings("DSI • BG", "Stroke Colour Set", filterSettings, obsConnection);
                    LogDebug("Outline setting is 'None'. Changed accent colour to '0'");
                    break;
                // If accent mode is set to Alert only
                case "Alerts Only":
                    if (alert)
                    {
                        filterSettings.Add("setting_color_alpha", outlineColour);
                        SetObsSourceFilterSettings("DSI • BG", "Stroke Colour Set", filterSettings, obsConnection);
                        LogDebug($"Outline setting is 'AlertsOnly'. Changed accent colour to [{outlineColour}]");
                    }
                    else
                    {
                        filterSettings.Add("setting_color_alpha", 0);
                        SetObsSourceFilterSettings("DSI • BG", "Stroke Colour Set", filterSettings, obsConnection);
                        LogDebug("Outline setting is 'AlertsOnly'. Changed accent colour to '0'");
                    }
                    break;
                // If accent mode is set to always on
                case "Always On":
                    filterSettings.Add("setting_color_alpha", outlineColour);
                    SetObsSourceFilterSettings("DSI • BG", "Stroke Colour Set", filterSettings, obsConnection);
                    LogDebug($"Outline setting is 'AlwaysOn'. Changed accent colour to: [{outlineColour}]");
                    break;
            }

            LogDebug("Outline colour filters set successfully.");
            return true;
        }
    }
}