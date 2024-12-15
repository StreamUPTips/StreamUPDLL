using System;
using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // A method to set the state and perform state-specific logic
        public void DSISetState(string actionName, bool addToAlertQueue)
        {
            var dsiInfo = DSILoadInfo();

            if (addToAlertQueue)
            {
                DSIAlertAddToQueue(dsiInfo, actionName);
                return;
            }

            dsiInfo.LastActionRun = actionName;

            switch (dsiInfo.CurrentState)
            {
                case DSIInfo.DSIState.Default:
                    HandleRotator(dsiInfo, actionName);
                    break;
                case DSIInfo.DSIState.AlertStarting:
                    HandleAlertStarting(dsiInfo, actionName);
                    break;
                case DSIInfo.DSIState.AlertEnding:
                    HandleAlertEnding(dsiInfo, actionName);
                    break;
                case DSIInfo.DSIState.Locking:
                    HandleLockingState(dsiInfo, actionName);
                    break;
                case DSIInfo.DSIState.Locked:
                    HandleLockedState(dsiInfo, actionName);
                    break;
                case DSIInfo.DSIState.Static:
                    dsiInfo.ActiveWidget = actionName;
                    break;
            }

        }

        private void HandleLockedState(DSIInfo dsiInfo, string actionName)
        {
            if (dsiInfo.AlertQueue > 0)
            {
                LogDebug("Alert queue is not empty.");
                dsiInfo.CurrentState = DSIInfo.DSIState.AlertStarting;
            }
            else
            {
                LogDebug("Alert queue is empty.");
                dsiInfo.CurrentState = DSIInfo.DSIState.Default;
                _CPH.EnableTimerById("27f9dd60-c81c-434f-a11a-ffc5bd578785");
            }
            _CPH.ResumeActionQueue("StreamUP Widgets • DSI Widgets");

            DSISaveInfo(dsiInfo);
            DSISetState("", false);
        }

        private void HandleLockingState(DSIInfo dsiInfo, string actionName)
        {
            dsiInfo.CurrentState = DSIInfo.DSIState.Locked;
            DSISaveInfo(dsiInfo);
        }

        private void DSIAlertAddToQueue(DSIInfo dsiInfo, string actionName)
        {
            // Add alert to queue
            _CPH.DisableTimerById("27f9dd60-c81c-434f-a11a-ffc5bd578785");
            dsiInfo.AlertQueue += 1;
            DSISaveInfo(dsiInfo);

            while (dsiInfo.ActionInProgress)
            {
                LogDebug("Action in progress. Waiting...");
                _CPH.Wait(100);
                dsiInfo = DSILoadInfo();
            }

            _CPH.RunAction(actionName, false);
            if (dsiInfo.CurrentState == DSIInfo.DSIState.Default)
            {
                dsiInfo.CurrentState = DSIInfo.DSIState.AlertStarting;
                DSISaveInfo(dsiInfo);
                DSISetState("", false);
            }
            else
            {
                DSISaveInfo(dsiInfo);
                LogDebug("Alert added to queue. Something is already active.");
            }
        }

        private void HandleAlertEnding(DSIInfo dsiInfo, string actionName)
        {
            dsiInfo.AlertQueue -= 1;
            if (dsiInfo.AlertQueue <= 0)
            {
                LogDebug("Alert queue is empty.");
                dsiInfo.AlertEnded = true;
                dsiInfo.CurrentState = DSIInfo.DSIState.Default;
            }
            else
            {
                LogDebug("Alert queue is not empty.");
                dsiInfo.CurrentState = DSIInfo.DSIState.AlertStarting;
            }
            DSISaveInfo(dsiInfo);
            DSISetState(actionName, false);
        }

        private void HandleAlertStarting(DSIInfo dsiInfo, string actionName)
        {
            dsiInfo.CurrentState = DSIInfo.DSIState.AlertActive;
            DSISaveInfo(dsiInfo);
        }

        private void HandleRotator(DSIInfo dsiInfo, string actionName = "")
        {
            LogDebug($"Handling rotator. There are {dsiInfo.RotatorWidgets.Count} widgets in the rotator list.");
            if (dsiInfo.RotatorWidgets.Count <= 1)
            {
                if (dsiInfo.LastActionRun == "" || dsiInfo.LastActionRun != actionName || dsiInfo.AlertEnded == true)
                {
                    dsiInfo.ActiveWidget = dsiInfo.RotatorWidgets[0];
                    DSISaveInfo(dsiInfo);
                    _CPH.RunAction(dsiInfo.ActiveWidget, false);
                    _CPH.DisableTimerById("27f9dd60-c81c-434f-a11a-ffc5bd578785");
                }
                LogDebug($"There are {dsiInfo.RotatorWidgets.Count} in the rotator list. Skipping rotator.");

                return;
            }

            dsiInfo.RotatorIndex += 1;
            if (dsiInfo.RotatorIndex >= dsiInfo.RotatorWidgets.Count)
            {
                dsiInfo.RotatorIndex = 0;
            }
            dsiInfo.ActiveWidget = dsiInfo.RotatorWidgets[dsiInfo.RotatorIndex];
            DSISaveInfo(dsiInfo);
            _CPH.RunAction(dsiInfo.ActiveWidget, false);
            _CPH.EnableTimerById("27f9dd60-c81c-434f-a11a-ffc5bd578785");
        }

        public DSIInfo DSILoadInfo()
        {
            // Ensure StateInfo is not null
            string dsiInfoString = _CPH.GetGlobalVar<string>("sup069_DSIState", true);
            var dsiInfo = new DSIInfo();
            if (string.IsNullOrEmpty(dsiInfoString))
            {
                LogError("StateInfo is null. Initialising StateInfo.");
                string DSIInfoInit = JsonConvert.SerializeObject(dsiInfo);
                _CPH.SetGlobalVar("sup069_DSIState", DSIInfoInit, true);
            }

            dsiInfo = JsonConvert.DeserializeObject<DSIInfo>(_CPH.GetGlobalVar<string>("sup069_DSIState", true));

            LogDebug($"DSIInfo loaded: {dsiInfo}");
            return dsiInfo;
        }

        public void DSISaveInfo(DSIInfo dsiInfo)
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
            Locking,
            Locked,
            AlertStarting,
            AlertActive,
            AlertEnding,
            Static
        }

        public DSIState CurrentState { get; set; }
        public string ActiveWidget { get; set; }
        public List<string> InstalledWidgets { get; set; }
        public List<string> RotatorWidgets { get; set; }
        public int RotatorIndex { get; set; }
        public int AlertQueue { get; set; }
        public bool ActionInProgress { get; set; }
        public bool StateMachineRunning { get; set; }
        public string LastActionRun { get; set; }
        public bool AlertEnded { get; set; }

        public DSIInfo()
        {
            CurrentState = DSIState.Default;
            ActiveWidget = string.Empty;
            InstalledWidgets = new List<string>();
            RotatorWidgets = new List<string>();
            RotatorIndex = 0;
            AlertQueue = 0;
            ActionInProgress = false;
            StateMachineRunning = false;
            LastActionRun = string.Empty;
            AlertEnded = false;
        }
    }
}
