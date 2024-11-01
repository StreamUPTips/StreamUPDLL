using System;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void TheRunTriggerHandler()
        {
            string user;

            if (!_CPH.TryGetArg("message", out string returnedJsonStr))
            {
                LogError("Unable to retrieve returnedJsonStr from the [message] arg");
                return;
            }

            // Parse message from TheRun.gg
            JObject json = JObject.Parse(returnedJsonStr);

            // Get username
            user = json["user"]?.ToString() ?? "Unknown User";

            // Get run json
            var runJson = (JObject)json["run"];
            if (runJson == null)
            {
                LogError("Run data not found.");
                return;
            }

            // Set general args
            SetGeneralArgs(runJson);

            // Check if current split is less than previous global split index
            int currentSplitIndex = (int)(runJson["currentSplitIndex"]?.ToObject<int>());

            // Check if run reset
            if (currentSplitIndex == -1)
            {
                HandleResetEvent();
                return;
            }

            int previousGlobalSplitIndex = _CPH.GetGlobalVar<int>("theRunGgCurrentSplitIndex", false);

            // Set split args
            int previousSplitIndex = currentSplitIndex - 1;
            var currentSplitData = new JObject();
            var previousSplitData = new JObject();

            // Check if split was undone
            if (previousGlobalSplitIndex == currentSplitIndex + 1)
            {
                // Set previous split args
                if (previousSplitIndex > -1)
                {
                    SetSegmentArgs(previousSplitIndex, runJson, false);
                }

                // Set current split args
                SetSegmentArgs(currentSplitIndex, runJson, true);

                HandleUndoSplitEvent(previousGlobalSplitIndex);
                return;
            }
            
            // Set previous split args
            if (previousSplitIndex > -1)
            {
                previousSplitData = SetSegmentArgs(previousSplitIndex, runJson, false);
            }

            // Set current split args
            currentSplitData = SetSegmentArgs(currentSplitIndex, runJson, true);

            // Check if split was skipped
            if (previousSplitData != null)
            {
                if (previousSplitData["splitTime"]?.ToObject<double?>() == null)
                {
                    HandleSkipSplitEvent(previousSplitData);
                    return;
                }
            }

            // Handle special events
            HandleEvents(runJson, previousSplitData, currentSplitData);

            HandleSplitDifference(runJson, previousSplitData);
        }

        private void SetGeneralArgs(JObject runJson)
        {
            // Set args for gameData with time conversions
            var gameData = runJson["gameData"] as JObject;
            if (gameData != null)
            {
                _CPH.SetArgument("gameData.category", gameData["run"]?.ToString());
                _CPH.SetArgument("gameData.game", gameData["game"]?.ToString());
                _CPH.SetArgument("gameData.platform", gameData["platform"]?.ToString());
                _CPH.SetArgument("gameData.gameImage", runJson["gameImage"]?.ToString());

                // Handle gameData.variables
                var variables = gameData["variables"] as JObject;
                if (variables != null)
                {
                    _CPH.SetArgument("gameData.layout", variables["Layout"]?.ToString());
                    _CPH.SetArgument("gameData.playStyle", variables["Play Style"]?.ToString());
                    _CPH.SetArgument("gameData.runType", variables["Run Type"]?.ToString());
                    _CPH.SetArgument("gameData.version", variables["Version"]?.ToString());
                }
            }

            // String and non-time arguments
            _CPH.SetArgument("data.currentComparison", runJson["currentComparison"]?.ToString());
            _CPH.SetArgument("data.currentSplitName", runJson["currentSplitName"]?.ToString());
            _CPH.SetArgument("data.currentSplitIndex", runJson["currentSplitIndex"]?.ToObject<int>());

            // Root-level run arguments with converted times
            _CPH.SetArgument("data.bestPossible", RoundDouble(runJson["bestPossible"]?.ToObject<double>()));
            _CPH.SetArgument("data.currentPrediction", RoundDouble(runJson["currentPrediction"]?.ToObject<double>()));
            _CPH.SetArgument("data.personalBest", RoundDouble(gameData["personalBest"]?.ToObject<double>()));
            _CPH.SetArgument("data.personalBestDate", gameData["personalBestTime"]?.ToObject<DateTime>());
            _CPH.SetArgument("data.finishedAttemptCount", gameData["finishedAttemptCount"]?.ToObject<int>());
            _CPH.SetArgument("data.totalRunTime", RoundDouble(gameData["totalRunTime"]?.ToObject<double>()));
            _CPH.SetArgument("data.timeToSave", RoundDouble(gameData["timeToSave"]?.ToObject<double>()));
            _CPH.SetArgument("data.sumOfBests", RoundDouble(gameData["sumOfBests"]?.ToObject<double>()));
        }

        public JObject SetSegmentArgs(int splitIndex, JObject runJson, bool isCurrentSplit)
        {
            // Retrieve split object at the specified index
            JObject splitData = (JObject)runJson["splits"][splitIndex];
            if (splitData == null)
            {
                LogError($"Split data not found at index {splitIndex}");
                return null;
            }

            // Set current split globals
            if (isCurrentSplit)
            {
                _CPH.SetGlobalVar("theRunGgCurrentSplitIndex", splitIndex, false);
                _CPH.SetGlobalVar("theRunGgCurrentSplitName", splitData["name"]?.ToObject<string>(), false);
            }
            
            // Define prefix based on whether it's the current or previous split
            string split = isCurrentSplit ? "currentSplit" : "previousSplit";

            // Set general split args
            _CPH.SetArgument($"{split}.splitName", splitData["name"]?.ToObject<string>());
            _CPH.SetArgument($"{split}.splitTime", RoundDouble(splitData["splitTime"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.bestPossibleTime", RoundDouble(splitData["bestPossibleTimeAtSplit"]?.ToObject<double?>()));

            // Set segment args
            _CPH.SetArgument($"{split}.segment.averageTime", RoundDouble(splitData["average"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.segment.bestPossible", RoundDouble(splitData["single"]["bestPossible"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.segment.comparisonTime", RoundDouble(splitData["single"]["time"]?.ToObject<double?>()));

            // Set run args
            _CPH.SetArgument($"{split}.run.splitTime", RoundDouble(splitData["pbSplitTime"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.run.predictedTotalTime", RoundDouble(splitData["predictedTotalTime"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.run.bestAchievedTime", RoundDouble(splitData["total"]["bestAchievedTime"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.run.bestPossibleTime", RoundDouble(splitData["total"]["bestPossibleTime"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.run.averageTime", RoundDouble(splitData["total"]["averageTime"]?.ToObject<double?>()));
            _CPH.SetArgument($"{split}.run.comparisonTime", RoundDouble(splitData["total"]["time"]?.ToObject<double?>()));

            return splitData;
        }

        private double? RoundDouble(double? value)
        {
            return value.HasValue ? Math.Round(value.Value, 0) : null;
        }

        private void HandleEvents(JObject runJson, JObject previousSplitData, JObject currentSplitData)
        {
            // Extract events array
            JArray events = runJson["events"] as JArray;
            if (events == null)
            {
                LogInfo("No events found in run data.");
                return;
            }        
        
            // Iterate through each event
            foreach (JObject eventObj in events)
            {
                string eventType = eventObj["type"]?.ToString();
                JObject eventData = eventObj["data"] as JObject;

                // Check the event type and process accordingly
                switch (eventType)
                {
                    case "best_run_ever_event":
                        HandleBestRunEverEvent(eventData);
                        break;

                    case "final_split_event":
                        HandleFinalSplitEvent(eventData);
                        break;

                    case "run_ended_event":
                        HandleRunEndedEvent(eventData);
                        break;

                    case "run_started_event":
                        HandleRunStartedEvent(eventData);
                        break;

                    case "top_10_single_segment_event":
                        HandleTop10SingleSegmentEvent(eventData);
                        break;

                    case "top_10_total_segment_event":
                        HandleTop10TotalSegmentEvent(eventData);
                        break;

                    case "worst_10_single_segment_event":
                        HandleWorst10SingleSegmentEvent(eventData);
                        break;
                
                    case "gold_split_event":
                        HandleGoldSplitEvent(eventData);
                        break;

                    default:
                        LogDebug($"Unknown event type: {eventType}");
                        break;
                }        
            }
        }

        private void HandleGoldSplitEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString() ?? "Unknown Split Name";

            double? achievedTime = RoundDouble(eventData["data"]?["newGold"]?.ToObject<double?>());
            double? targetTime = RoundDouble(eventData["data"]?["previousGold"]?.ToObject<double?>());
            double? timeDifference = RoundDouble(eventData["data"]?["delta"]?.ToObject<double?>());

            // Set arguments for event
            _CPH.SetArgument("goldSegment.splitName", splitName);
            _CPH.SetArgument("goldSegment.newGold", achievedTime);
            _CPH.SetArgument("goldSegment.previousGold", targetTime);
            _CPH.SetArgument("goldSegment.timeDifference", timeDifference);

            _CPH.TriggerCodeEvent("theRunGoldSplit");
        }

        private void HandleTop10SingleSegmentEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString() ?? "Unknown Split Name";

            double? achievedTime = RoundDouble(eventData["data"]?["achievedTime"]?.ToObject<double?>());
            double? targetTime = RoundDouble(eventData["data"]?["targetTime"]?.ToObject<double?>());

            // Calculate and format the time saved
            double? timeSaved = targetTime - achievedTime;

            // Set arguments for event
            _CPH.SetArgument("top10.segment.splitName", splitName);
            _CPH.SetArgument("top10.segment.achievedTime", achievedTime);
            _CPH.SetArgument("top10.segment.targetTime", targetTime);
            _CPH.SetArgument("top10.segment.timeDifference", timeSaved);

            _CPH.TriggerCodeEvent("theRunTop10Single");
        }

        private void HandleTop10TotalSegmentEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString() ?? "Unknown Split Name";

            double? achievedTime = RoundDouble(eventData["data"]?["achievedTime"]?.ToObject<double?>());
            double? targetTime = RoundDouble(eventData["data"]?["targetTime"]?.ToObject<double?>());

            // Calculate and format the time saved
            double? timeSaved = targetTime - achievedTime;

            // Set arguments for event
            _CPH.SetArgument("top10.total.splitName", splitName);
            _CPH.SetArgument("top10.total.achievedTime", achievedTime);
            _CPH.SetArgument("top10.total.targetTime", targetTime);
            _CPH.SetArgument("top10.total.timeDifference", timeSaved);

            _CPH.TriggerCodeEvent("theRunTop10Total");
        }

        private void HandleWorst10SingleSegmentEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString() ?? "Unknown Split Name";

            double? achievedTime = RoundDouble(eventData["data"]?["achievedTime"]?.ToObject<double?>());
            double? targetTime = RoundDouble(eventData["data"]?["targetTime"]?.ToObject<double?>());

            // Calculate and format the time lost
            double? timeLost = achievedTime - targetTime;

            // Set arguments for event
            _CPH.SetArgument("worst10.segment.splitName", splitName);
            _CPH.SetArgument("worst10.segment.achievedTime", achievedTime);
            _CPH.SetArgument("worst10.segment.targetTime", targetTime);
            _CPH.SetArgument("worst10.segment.timeDifference", timeLost);

            _CPH.TriggerCodeEvent("theRunWorst10Single");
        }

        private void HandleFinalSplitEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString() ?? "Unknown Split Name";

            double? pbSplitTime = RoundDouble(eventData["data"]?["pbSplitTime"]?.ToObject<double?>());
            double? expectedSplitTime = RoundDouble(eventData["data"]?["expectedSplitTime"]?.ToObject<double?>());

            // Log formatted results or set arguments
            _CPH.SetArgument("finalSplit.splitName", splitName);
            _CPH.SetArgument("finalSplit.bestSegmentTime", pbSplitTime);
            _CPH.SetArgument("finalSplit.expectedSegmentTime", expectedSplitTime);

            _CPH.TriggerCodeEvent("theRunFinalSplit");
        }

        private void HandleRunEndedEvent(JObject eventData)
        {
            double? finalTime = RoundDouble(eventData["data"]?["finalTime"]?.ToObject<double?>());
            double? predictedTime = RoundDouble(eventData["data"]?["predictedTime"]?.ToObject<double?>());
            double? personalBest = RoundDouble(eventData["data"]?["personalBest"]?.ToObject<double?>());
            double? deltaToPredictedTime = RoundDouble(eventData["data"]?["deltaToPredictedTime"]?.ToObject<double>());
            double? deltaToPersonalBest = RoundDouble(eventData["data"]?["deltaToPersonalBest"]?.ToObject<double>());

            // Set arguments for the run ended event
            _CPH.SetArgument("runEnded.finalTime", finalTime);
            _CPH.SetArgument("runEnded.predictedTime", predictedTime);
            _CPH.SetArgument("runEnded.personalBest", personalBest);
            _CPH.SetArgument("runEnded.deltaToPredicted", deltaToPredictedTime);
            _CPH.SetArgument("runEnded.deltaToPersonalBest", deltaToPersonalBest);

            _CPH.TriggerCodeEvent("theRunRunEnded");
        }

        private void HandleRunStartedEvent(JObject eventData)
        {
            double? personalBest = RoundDouble(eventData["data"]?["personalBest"]?.ToObject<double?>());
            double? expectedEndTime = RoundDouble(eventData["data"]?["expectedEndTime"]?.ToObject<double?>());

            // Set arguments for event
            _CPH.SetArgument("runStarted.personalBest", personalBest);
            _CPH.SetArgument("runStarted.expectedEndTime", expectedEndTime);

            _CPH.TriggerCodeEvent("theRunRunStarted");
        }

        private void HandleBestRunEverEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString() ?? "Unknown Split Name";

            double? achievedTime = RoundDouble(eventData["data"]?["achievedTime"]?.ToObject<double?>());
            double? targetTime = RoundDouble(eventData["data"]?["targetTime"]?.ToObject<double?>());

            // Calculate the time difference and format with appropriate sign
            double? timeDifference = achievedTime - targetTime;

            // Set arguments for the event
            _CPH.SetArgument("bestRunEver.splitName", splitName);
            _CPH.SetArgument("bestRunEver.achievedTime", achievedTime);
            _CPH.SetArgument("bestRunEver.targetTime", targetTime);
            _CPH.SetArgument("bestRunEver.timeDifference", timeDifference);

            _CPH.TriggerCodeEvent("theRunBestRun");
        }

        private void HandleSkipSplitEvent(JObject previousSplitData)
        {
            // Set arguments for the skip split event
            _CPH.SetArgument("skippedSplit.splitName", previousSplitData["name"]?.ToObject<string>());

            _CPH.TriggerCodeEvent("theRunSkipSplit");
        }

        private void HandleUndoSplitEvent(int splitIndex)
        {     
            // Set split args
            _CPH.SetArgument("undoneSplit.splitIndex", splitIndex - 1);
            //_CPH.SetArgument("undoneSplit.splitName", splitName);

            _CPH.SetGlobalVar("theRunGgCurrentSplitIndex", splitIndex - 1, false);

            _CPH.TriggerCodeEvent("theRunUndoSplit");
        }

        private void HandleResetEvent()
        {
            _CPH.TriggerCodeEvent("theRunRunReset");
        }

        private void HandleSplitDifference(JObject runJson, JObject previousSplitData)
        {
            double? segmentBestPossibleTime = RoundDouble(previousSplitData["single"]["bestPossible"]?.ToObject<double?>());
            double? runSplitTime = RoundDouble(previousSplitData["splitTime"]?.ToObject<double?>());
            double? runTargetTime = RoundDouble(previousSplitData["total"]["time"]?.ToObject<double?>());
            double? oldTimeDifference = _CPH.GetGlobalVar<double?>("theRunGgCurrentTimeDifference", true);
            double? newTimeDifference;
            double? timeSaveLossAmount;
            bool goldSegment = false;

            // Check if gold event
            // Extract events array
            JArray events = runJson["events"] as JArray;
            if (events == null)
            {
                LogInfo("No events found in run data.");
                return;
            }        
        
            // Iterate through each event
            foreach (JObject eventObj in events)
            {
                string eventType = eventObj["type"]?.ToString();
                JObject eventData = eventObj["data"] as JObject;

                // Check the event type and process accordingly
                if (eventType == "gold_split_event")
                {
                    goldSegment = true;
                }        
            }
            _CPH.SetArgument("goldSegment", goldSegment);
            
            // Check if run is red or green
            if (runSplitTime > runTargetTime)
            {
                newTimeDifference = runSplitTime - runTargetTime;
                HandleRedSplitEvent(runSplitTime, runTargetTime, newTimeDifference);
            }
            else
            {
                newTimeDifference = runTargetTime - runSplitTime;
                HandleGreenSplitEvent(runSplitTime, runTargetTime, newTimeDifference);            
            }
            _CPH.SetGlobalVar("theRunGgCurrentTimeDifference", newTimeDifference, true);

            // Check if there is a time save
            if (oldTimeDifference > newTimeDifference)
            {
                timeSaveLossAmount = oldTimeDifference - newTimeDifference;
                HandleTimeSaveEvent(timeSaveLossAmount);
            }
            else
            {
                timeSaveLossAmount = newTimeDifference - oldTimeDifference;
                HandleTimeLossEvent(timeSaveLossAmount);
            }
        }

        private void HandleTimeLossEvent(double? timeLossAmount)
        {
            _CPH.SetArgument("segmentComparison.timeDifference", timeLossAmount);

            _CPH.TriggerCodeEvent("theRunTimeLoss");
        }

        private void HandleTimeSaveEvent(double? timeSaveAmount)
        {
            _CPH.SetArgument("segmentComparison.timeDifference", timeSaveAmount);

            _CPH.TriggerCodeEvent("theRunTimeSave");
        }

        private void HandleGreenSplitEvent(double? splitTime, double? targetTime, double? timeDifference)
        {
            // Set arguments for the green split event
            _CPH.SetArgument("segmentComparison.splitTime", splitTime);
            _CPH.SetArgument("segmentComparison.targetTime", targetTime);
            _CPH.SetArgument("segmentComparison.timeDifference", timeDifference);

            _CPH.TriggerCodeEvent("theRunGreenSplit");
        }

        private void HandleRedSplitEvent(double? splitTime, double? targetTime, double? timeDifference)
        {
            // Set arguments for the red split event
            _CPH.SetArgument("segmentComparison.splitTime", splitTime);
            _CPH.SetArgument("segmentComparison.targetTime", targetTime);
            _CPH.SetArgument("segmentComparison.timeDifference", timeDifference);

            _CPH.TriggerCodeEvent("theRunRedSplit");
        }


    }
}
