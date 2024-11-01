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

            // Exit early if a skip or undo event was triggered
            if (currentSplitData == null)
            {
                // Clear the previous pace difference to reset tracking after skip or undo
                _CPH.SetGlobalVar("theRunGgPreviousPaceDifference", 0, false);
                return;
            }

            // Proceed with event handling and tracking progress if no skip or undo event was triggered
            HandleEvents(runJson, currentSplitData);
            TrackProgress(currentSplitIndex, runJson);
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

            // Check for Undo Split event
            if (previousGlobalSplitIndex == currentSplitIndex + 1)
            {
                SetSegmentArgs(currentSplitIndex, runJson, true);
                HandleUndoSplitEvent(previousGlobalSplitIndex);
                return null;  // Exit early on undo
            }

            // Check for Skip Split event
            var currentSplitData = SetSegmentArgs(currentSplitIndex, runJson, true);
            if (previousSplitData != null && currentSplitIndex > 0 && previousSplitData["splitTime"]?.ToObject<double?>() == null)
            {
                HandleSkipSplitEvent(previousSplitData);
                return null;  // Exit early on skip
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


        public void TrackProgress(int currentSplitIndex, JObject runJson)
        {
            if (currentSplitIndex < 1)
            {
                LogDebug("Not enough data to calculate progress (first split or missing data).");
                return;
            }

            // Get the "current" split as the most recently completed one
            var previousSplitData = runJson["splits"]?[currentSplitIndex - 1] as JObject;
            var comparisonTime = previousSplitData?["comparisons"]?["Personal Best"]?.ToObject<double?>();

            if (previousSplitData != null && comparisonTime.HasValue)
            {
                // Check previous split pace against its comparison
                double currentPaceDifference = CalculatePace(previousSplitData, comparisonTime.Value, currentSplitIndex - 1);

                // Get the previous pace difference from a global variable
                double previousPaceDifference = _CPH.GetGlobalVar<double>("theRunGgPreviousPaceDifference", false);

                // Calculate and set whether time was saved or lost
                DetermineTimeSavedOrLost(currentPaceDifference, previousPaceDifference);

                // Update the global variable with the current pace difference for the next split comparison
                _CPH.SetGlobalVar("theRunGgPreviousPaceDifference", currentPaceDifference, false);
            }
        }

        private double CalculatePace(JObject previousSplitData, double comparisonTime, int splitIndex)
        {
            // Get the actual split time and convert to TimeSpan
            double actualSplitMilliseconds = previousSplitData["splitTime"]?.ToObject<double?>() ?? 0;
            TimeSpan actualSplitTime = ConvertToTimeSpan(actualSplitMilliseconds) ?? TimeSpan.Zero;
            TimeSpan comparisonSplitTime = ConvertToTimeSpan(comparisonTime) ?? TimeSpan.Zero;

            // Calculate the difference (positive if behind, negative if ahead)
            TimeSpan timeDifference = actualSplitTime - comparisonSplitTime;
            bool isAhead = timeDifference < TimeSpan.Zero;

            // Set arguments and trigger events based on pace
            _CPH.SetArgument("split.paceDifference", FormatTimeSpan(timeDifference));
            _CPH.SetArgument("split.paceStatus", isAhead ? "ahead" : "behind");
            _CPH.TriggerCodeEvent(isAhead ? "theRunGreenSplit" : "theRunRedSplit");

            // Return the time difference in total milliseconds for tracking pace
            return timeDifference.TotalMilliseconds;
        }

        private void DetermineTimeSavedOrLost(double currentPaceDifference, double previousPaceDifference)
        {
            // Check if previous pace difference is zero (or default value) to prevent triggering comparison
            if (previousPaceDifference == 0)
            {
                LogDebug("Skipping time comparison as previous pace difference is zero.");
                return;
            }

            // Calculate the difference in pace
            double paceDifferenceChange = currentPaceDifference - previousPaceDifference;

            bool timeSaved = paceDifferenceChange < 0;

            // Set arguments and trigger events if there’s a time difference
            _CPH.SetArgument("split.timeSavedOrLost", FormatTimeSpan(TimeSpan.FromMilliseconds(Math.Abs(paceDifferenceChange))));
            _CPH.SetArgument("split.timeStatus", timeSaved ? "saved" : "lost");
            _CPH.TriggerCodeEvent(timeSaved ? "theRunTimeSave" : "theRunTimeLoss");
        }
    }
}
