using System.Collections.Generic;
using Streamer.bot.Common.Events;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool DSIAnimateToNextWidget(string widgetName, string widgetPrefix, Dictionary<string, object> productSettings, int obsConnection)
        {
            LogDebug("Animating to next widget...");

            // Hide all widgets and wait for the animation to complete
            DSIHideAllWidgets(obsConnection);
            _CPH.Wait(300);

            // Start the animation for the background to adjust to the new widget size
            _CPH.ObsShowFilter("StreamUP Widgets • Dynamic Stream-Island", "DSI • BG", "New Size Bounce", obsConnection);
            _CPH.ObsShowFilter("StreamUP Widgets • Dynamic Stream-Island", "DSI • BG", "Stroke Colour Set", obsConnection);
            _CPH.Wait(500);

            // Play audio cue
            PlayAudioCue(widgetPrefix, productSettings);
            _CPH.Wait(250);

            // Show the new widget
            _CPH.ObsShowSource("StreamUP Widgets • Dynamic Stream-Island", widgetName, obsConnection);

            var dsiInfo = DSILoadInfo();

            // End alert if it is currently active
            if (dsiInfo.CurrentState == DSIInfo.DSIState.AlertActive)
            {
                int alertTime = GetValueOrDefault<int>(productSettings, "AlertTime", 5000);
                _CPH.Wait(alertTime);
                dsiInfo = DSILoadInfo();
                dsiInfo.CurrentState = DSIInfo.DSIState.AlertEnding;
                DSISaveInfo(dsiInfo);
                DSISetState("", false);
            }   

            LogDebug("Animated to next widget successfully.");
            return true;
        }

        private void PlayAudioCue(string widgetPrefix, Dictionary<string, object> productSettings)
        {
            EventType eventType = _CPH.GetEventType();
            if (eventType == EventType.TimedAction)
            {
                LogDebug("Action triggered by a timed action. Skipping sound effect.");
                return;
            }
            
            string soundFilePath = GetValueOrDefault<string>(productSettings, $"{widgetPrefix}WidgetSoundEffect", "");
            float soundVolumeSetting = float.Parse(GetValueOrDefault<string>(productSettings, $"{widgetPrefix}WidgetSoundEffectVolume", "50"));
            float soundVolume = soundVolumeSetting / 100;
            LogDebug($"Playing sound effect: {soundFilePath} at volume: {soundVolumeSetting}. Output volume: {soundVolume}");
            _CPH.PlaySound(soundFilePath, soundVolume, false);
        }
    }
}
