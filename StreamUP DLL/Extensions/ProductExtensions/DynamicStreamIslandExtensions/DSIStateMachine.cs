using System.Collections.Generic;
using Newtonsoft.Json;
using Streamer.bot.Common.Events;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public DSIInfo dsiInfo { get; set; }

        public StreamUpLib()
        {
            dsiInfo = new DSIInfo();
        }

        // A method to set the state and perform state-specific logic
        public void DSISetState(string actionName, DSIInfo.DSIState state)
        {
            DSILoadInfo();

            EventType eventType = _CPH.GetEventType();
            if (eventType == EventType.TimedAction)
            {
                LogDebug("DSISetState called from a Timed Action.");
                dsiInfo.CurrentState = DSIInfo.DSIState.Default;
            }

            // Update the current state and active widget
            dsiInfo.CurrentState = state;
            dsiInfo.PreviousRotatorWidget = dsiInfo.ActiveWidget;

            switch (state)
            {
                case DSIInfo.DSIState.Default:
                    dsiInfo.RotatorIndex += 1;
                    if (dsiInfo.RotatorIndex >= dsiInfo.RotatorWidgets.Count)
                    {
                        dsiInfo.RotatorIndex = 0;
                    }
                    dsiInfo.ActiveWidget = dsiInfo.RotatorWidgets[dsiInfo.RotatorIndex];
                    _CPH.RunAction(dsiInfo.ActiveWidget, false);
                    break;
                case DSIInfo.DSIState.Locked:
                    dsiInfo.ActiveWidget = actionName;
                    break;
                case DSIInfo.DSIState.Alert:
                    break;
                case DSIInfo.DSIState.Static:
                    dsiInfo.ActiveWidget = actionName;
                    break;
            }

            // Save the updated state info
            DSISaveInfo();

            LogDebug($"State changed to: {dsiInfo.CurrentState}, Active Widget: {dsiInfo.ActiveWidget}");
        }

        public DSIInfo DSILoadInfo()
        {
            // Ensure StateInfo is not null
            string dsiInfoString = _CPH.GetGlobalVar<string>("sup069_DSIState", true);
            if (dsiInfo == null || string.IsNullOrEmpty(dsiInfoString))
            {
                LogError("StateInfo is null. Initialising StateInfo.");
                dsiInfo = new DSIInfo();
                string DSIInfoInit = JsonConvert.SerializeObject(dsiInfo);
                _CPH.SetGlobalVar("sup069_DSIState", DSIInfoInit, true);
            }

            dsiInfo = JsonConvert.DeserializeObject<DSIInfo>(_CPH.GetGlobalVar<string>("sup069_DSIState", true));

            LogDebug($"DSIInfo loaded: {dsiInfo}");
            return dsiInfo;
        }
    
        public void DSISaveInfo()
        {
            string dsiInfoString = JsonConvert.SerializeObject(dsiInfo);
            _CPH.SetGlobalVar("sup069_DSIState", dsiInfoString, true);
            LogDebug($"DSIInfo saved: {dsiInfoString}");
        }
    }

    public class DSIInfo
    {
        public enum DSIState
        {
            Default,
            Locked,
            Alert,
            Static
        }


        public DSIState CurrentState { get; set; }
        public string ActiveWidget { get; set; }
        public string PreviousRotatorWidget { get; set; }
        public List<string> InstalledWidgets { get; set; }
        public List<string> RotatorWidgets { get; set; }
        public int RotatorIndex { get; set; }

        public DSIInfo()
        {
            CurrentState = DSIState.Default;
            ActiveWidget = string.Empty;
            PreviousRotatorWidget = string.Empty;
            InstalledWidgets = new List<string>();
            RotatorWidgets = new List<string>();
            RotatorIndex = 0;
        }
    }
}
