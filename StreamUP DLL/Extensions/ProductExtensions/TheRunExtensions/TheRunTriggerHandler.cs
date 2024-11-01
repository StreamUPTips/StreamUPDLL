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
            if (!_CPH.TryGetArg("message", out string returnedJsonStr))
            {
                LogError("Unable to retrieve message from the [message] arg");
                return;
            }

            // Parse JSON data
            JObject json = JObject.Parse(returnedJsonStr);
            var runJson = json["run"] as JObject;
            if (runJson == null)
            {
                LogError("Run data not found.");
                return;
            }

            // Set up user and general arguments
            string user = json["user"]?.ToString() ?? "Unknown User";
            SetGeneralArgs(runJson);

            int currentSplitIndex = runJson["currentSplitIndex"]?.ToObject<int>() ?? -1;
            if (currentSplitIndex == -1)
            {
                HandleResetEvent();
                return;
            }

            // Handle split events
            int previousGlobalSplitIndex = _CPH.GetGlobalVar<int>("theRunGgCurrentSplitIndex", false);
            var currentSplitData = HandleSplitEvents(runJson, currentSplitIndex, previousGlobalSplitIndex);
            HandleEvents(runJson, currentSplitData);
        }

        private void SetGeneralArgs(JObject runJson)
        {
            var gameData = runJson["gameData"] as JObject;
            if (gameData != null)
            {
                SetGameDataArguments(gameData);
            }

            _CPH.SetArgument("data.currentComparison", runJson["currentComparison"]?.ToString());
            _CPH.SetArgument("data.currentSplitName", runJson["currentSplitName"]?.ToString());
            _CPH.SetArgument("data.currentSplitIndex", runJson["currentSplitIndex"]?.ToObject<int>());

            // Set time-related arguments
            SetTimeArgument("data.bestPossible", runJson["bestPossible"]);
            SetTimeArgument("data.currentPrediction", runJson["currentPrediction"]);
            SetTimeArgument("data.personalBest", gameData?["personalBest"]);
            SetTimeArgument("data.totalRunTime", gameData?["totalRunTime"]);
            SetTimeArgument("data.timeToSave", gameData?["timeToSave"]);
            SetTimeArgument("data.sumOfBests", gameData?["sumOfBests"]);
        }

        private void SetGameDataArguments(JObject gameData)
        {
            _CPH.SetArgument("gameData.category", gameData["run"]?.ToString());
            _CPH.SetArgument("gameData.game", gameData["game"]?.ToString());
            _CPH.SetArgument("gameData.platform", gameData["platform"]?.ToString());
            _CPH.SetArgument("gameData.gameImage", gameData["gameImage"]?.ToString());

            var variables = gameData["variables"] as JObject;
            if (variables != null)
            {
                _CPH.SetArgument("gameData.layout", variables["Layout"]?.ToString());
                _CPH.SetArgument("gameData.playStyle", variables["Play Style"]?.ToString());
                _CPH.SetArgument("gameData.runType", variables["Run Type"]?.ToString());
                _CPH.SetArgument("gameData.version", variables["Version"]?.ToString());
            }
        }

        private JObject HandleSplitEvents(JObject runJson, int currentSplitIndex, int previousGlobalSplitIndex)
        {
            int previousSplitIndex = currentSplitIndex - 1;
            JObject previousSplitData = (previousSplitIndex >= 0)
                                        ? SetSegmentArgs(previousSplitIndex, runJson, false)
                                        : null;

            // Check for Undo Split or Skip Split scenarios
            if (previousGlobalSplitIndex == currentSplitIndex + 1)
            {
                SetSegmentArgs(currentSplitIndex, runJson, true);
                HandleUndoSplitEvent(previousGlobalSplitIndex);
                return null;
            }

            var currentSplitData = SetSegmentArgs(currentSplitIndex, runJson, true);
            if (previousSplitData != null && currentSplitIndex > 0 && previousSplitData["splitTime"]?.ToObject<double?>() == null)
            {
                HandleSkipSplitEvent(previousSplitData);
                return null;
            }

            return currentSplitData;
        }

        private void HandleEvents(JObject runJson, JObject currentSplitData)
        {
            var events = runJson["events"] as JArray;
            if (events == null)
            {
                LogInfo("No events found in run data.");
                return;
            }

            foreach (JObject eventObj in events)
            {
                string eventType = eventObj["type"]?.ToString();
                if (string.IsNullOrEmpty(eventType))
                {
                    LogDebug("Encountered an event without a type. Skipping.");
                    continue;
                }

                JObject eventData = eventObj["data"] as JObject;
                if (eventData == null)
                {
                    LogDebug($"Event '{eventType}' has no data payload. Skipping.");
                    continue;
                }

                switch (eventType.ToLower())
                {
                    case "best_run_ever_event":
                        HandleEventWithType(eventData, "BestRun");
                        break;
                    case "final_split_event":
                        HandleEventWithType(eventData, "FinalSplit");
                        break;
                    case "run_ended_event":
                        HandleEventWithType(eventData, "RunEnded");
                        break;
                    case "run_started_event":
                        HandleEventWithType(eventData, "RunStarted");
                        break;
                    case "top_10_single_segment_event":
                        HandleEventWithType(eventData, "Top10Single");
                        break;
                    case "top_10_total_segment_event":
                        HandleEventWithType(eventData, "Top10Total");
                        break;
                    case "worst_10_single_segment_event":
                        HandleEventWithType(eventData, "Worst10Single");
                        break;
                    case "gold_split_event":
                        HandleEventWithType(eventData, "GoldSplit");
                        break;
                    default:
                        LogDebug($"Unknown event type: {eventType}");
                        break;
                }
            }
        }

        private void HandleEventWithType(JObject eventData, string eventPrefix)
        {
            string eventPrefixLower = eventPrefix.Length > 0 
                ? char.ToLower(eventPrefix[0]) + eventPrefix.Substring(1) 
                : eventPrefix;

            if (eventData == null)
            {
                LogDebug($"No data available for event with prefix '{eventPrefixLower}'");
                return;
            }

            foreach (var property in eventData.Properties())
            {
                double? propertyValue = null;
                string argumentValue;

                // Check if the property is numeric
                if (property.Value.Type == JTokenType.Float || property.Value.Type == JTokenType.Integer)
                {
                    propertyValue = property.Value.ToObject<double>();
                    // Convert numeric property to TimeSpan and format
                    argumentValue = FormatTimeSpan(ConvertToTimeSpan(propertyValue.Value));
                }
                else if (property.Value.Type == JTokenType.String)
                {
                    // Check if the string can be parsed as a double
                    if (double.TryParse(property.Value.ToString(), out double parsedValue))
                    {
                        propertyValue = parsedValue;
                        // Convert parsable string to TimeSpan and format
                        argumentValue = FormatTimeSpan(ConvertToTimeSpan(parsedValue));
                    }
                    else
                    {
                        // Treat as a regular string
                        argumentValue = property.Value.ToString();
                    }
                }
                else
                {
                    // Treat other non-numeric types as strings
                    argumentValue = property.Value.ToString();
                }

                // Set the argument using either the formatted TimeSpan or the string value
                _CPH.SetArgument($"{eventPrefixLower}.{property.Name}", argumentValue);
            }

            // Trigger the appropriate event in Streamer.Bot
            _CPH.TriggerCodeEvent($"theRun{eventPrefix}");
        }

        public JObject SetSegmentArgs(int splitIndex, JObject runJson, bool isCurrentSplit)
        {
            // Retrieve split data from the JSON based on the splitIndex
            JObject splitData = (JObject)runJson["splits"]?[splitIndex];
            if (splitData == null)
            {
                LogError($"Split data not found at index {splitIndex}");
                return null;
            }

            // Set global variables if this is the current split
            if (isCurrentSplit)
            {
                _CPH.SetGlobalVar("theRunGgCurrentSplitIndex", splitIndex, false);
                _CPH.SetGlobalVar("theRunGgCurrentSplitName", splitData["name"]?.ToString(), false);
            }

            string prefix = isCurrentSplit ? "currentSplit" : "previousSplit";

            // Set the split name
            _CPH.SetArgument($"{prefix}.splitName", splitData["name"]?.ToString());

            // Set primary time-related arguments with robust null handling
            SetTimeArgument($"{prefix}.splitTime", splitData["splitTime"]);
            SetTimeArgument($"{prefix}.bestPossibleTime", splitData["bestPossibleTimeAtSplit"]);

            // Handle 'segment' sub-properties
            JObject singleSegmentData = splitData["single"] as JObject;
            if (singleSegmentData != null)
            {
                SetTimeArgument($"{prefix}.segment.averageTime", singleSegmentData["averageTime"]);
                SetTimeArgument($"{prefix}.segment.bestPossible", singleSegmentData["bestPossibleTime"]);
                SetTimeArgument($"{prefix}.segment.comparisonTime", singleSegmentData["time"]);
            }
            else
            {
                LogDebug($"No 'single' segment data found for {prefix} at index {splitIndex}");
            }

            // Handle 'run' sub-properties under 'total' if available
            JObject totalRunData = splitData["total"] as JObject;
            if (totalRunData != null)
            {
                SetTimeArgument($"{prefix}.run.splitTime", totalRunData["pbSplitTime"]);
                SetTimeArgument($"{prefix}.run.predictedTotalTime", totalRunData["predictedTotalTime"]);
                SetTimeArgument($"{prefix}.run.bestAchievedTime", totalRunData["bestAchievedTime"]);
                SetTimeArgument($"{prefix}.run.bestPossibleTime", totalRunData["bestPossibleTime"]);
                SetTimeArgument($"{prefix}.run.averageTime", totalRunData["averageTime"]);
                SetTimeArgument($"{prefix}.run.comparisonTime", totalRunData["time"]);
            }
            else
            {
                LogDebug($"No 'total' run data found for {prefix} at index {splitIndex}");
            }

            return splitData;
        }

        private void SetSplitTimeArguments(JObject splitData, string splitPrefix)
        {
            SetTimeArgument($"{splitPrefix}.splitTime", splitData["splitTime"]);
            SetTimeArgument($"{splitPrefix}.bestPossibleTime", splitData["bestPossibleTimeAtSplit"]);

            JObject segment = splitData["segment"] as JObject;
            if (segment != null)
            {
                SetTimeArgument($"{splitPrefix}.segment.averageTime", segment["average"]);
                SetTimeArgument($"{splitPrefix}.segment.bestPossible", segment["bestPossible"]);
                SetTimeArgument($"{splitPrefix}.segment.comparisonTime", segment["comparisonTime"]);
            }

            JObject run = splitData["run"] as JObject;
            if (run != null)
            {
                SetTimeArgument($"{splitPrefix}.run.splitTime", run["pbSplitTime"]);
                SetTimeArgument($"{splitPrefix}.run.predictedTotalTime", run["predictedTotalTime"]);
                SetTimeArgument($"{splitPrefix}.run.bestAchievedTime", run["bestAchievedTime"]);
                SetTimeArgument($"{splitPrefix}.run.bestPossibleTime", run["bestPossibleTime"]);
                SetTimeArgument($"{splitPrefix}.run.averageTime", run["averageTime"]);
                SetTimeArgument($"{splitPrefix}.run.comparisonTime", run["comparisonTime"]);
            }
        }

        private void SetTimeArgument(string argumentName, JToken timeToken)
        {
            if (timeToken != null)
            {
                TimeSpan? timeSpan = ConvertToTimeSpan(timeToken.ToObject<double?>());
                _CPH.SetArgument(argumentName, FormatTimeSpan(timeSpan));
            }
            else
            {
                _CPH.SetArgument(argumentName, null);
            }
        }

        private TimeSpan? ConvertToTimeSpan(double? milliseconds)
        {
            return milliseconds.HasValue
                    ? TimeSpan.FromMilliseconds(milliseconds.Value)
                    : null;
        }

        private string FormatTimeSpan(TimeSpan? timeSpan)
        {
            return timeSpan.HasValue
                    ? timeSpan.Value.ToString(@"hh\:mm\:ss\.fff")
                    : null;
        }

        private void HandleResetEvent()
        {
            _CPH.TriggerCodeEvent("theRunRunReset");
        }

        private void HandleSkipSplitEvent(JObject previousSplitData)
        {
            _CPH.SetArgument("skippedSplit.splitName", previousSplitData["name"]?.ToObject<string>());
            _CPH.TriggerCodeEvent("theRunSkipSplit");
        }

        private void HandleUndoSplitEvent(int splitIndex)
        {
            _CPH.SetArgument("undoneSplit.splitIndex", splitIndex - 1);
            _CPH.TriggerCodeEvent("theRunUndoSplit");
        }







        /*
                private void HandleSplitDifference(JObject runJson, JObject previousSplitData)
                {
                    double? runSplitTime = RoundDouble(previousSplitData["splitTime"]?.ToObject<double?>());
                    double? runTargetTime = RoundDouble(previousSplitData["total"]["time"]?.ToObject<double?>());
                    double? newTimeDifference = runTargetTime - runSplitTime;
                    LogDebug($"runSplitTime={runSplitTime}, runTargetTime={runTargetTime}, newTimeDifference={newTimeDifference}");

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
                    bool ahead;
                    newTimeDifference = runSplitTime - runTargetTime;
                    if (runSplitTime > runTargetTime)
                    {
                        HandleRedSplitEvent(runSplitTime, runTargetTime);
                        ahead = false;
                    }
                    else
                    {
                        HandleGreenSplitEvent(runSplitTime, runTargetTime);
                        ahead = true;
                    }

                    // Check if there is a time save
                    //HandleTimeSaveLossEvent(ahead, oldTimeDifference, newTimeDifference);

                    _CPH.SetGlobalVar("theRunGgCurrentTimeDifference", newTimeDifference, false);
                }





                private void HandleTimeSaveLossEvent(bool ahead, double? oldTimeDifference, double? newTimeDifference)
                {
                    double? timeSaveLossAmount =  newTimeDifference - oldTimeDifference;
                    LogDebug($"timeSaveLossAmount=[{timeSaveLossAmount}], --> (newTimeDifference=[{newTimeDifference}] - oldTimeDifference=[{oldTimeDifference}])");

                    if (timeSaveLossAmount < 0)
                    {
                        HandleTimeSaveEvent(timeSaveLossAmount);
                    }
                    else
                    {
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

                private void HandleGreenSplitEvent(double? splitTime, double? targetTime)
                {
                    // Set arguments for the green split event
                    _CPH.SetArgument("segmentComparison.splitTime", splitTime);
                    _CPH.SetArgument("segmentComparison.targetTime", targetTime);

                    _CPH.TriggerCodeEvent("theRunGreenSplit");
                }

                private void HandleRedSplitEvent(double? splitTime, double? targetTime)
                {
                    // Set arguments for the red split event
                    _CPH.SetArgument("segmentComparison.splitTime", splitTime);
                    _CPH.SetArgument("segmentComparison.targetTime", targetTime);

                    _CPH.TriggerCodeEvent("theRunRedSplit");
                }
        */

    }
}
