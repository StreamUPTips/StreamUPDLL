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
            LogDebug("TheRunTriggerHandler started");

            if (!_CPH.TryGetArg("message", out string returnedJsonStr))
            {
                LogDebug("Unable to retrieve message from the [message] arg");
                return;
            }

            LogDebug("Parsing JSON data");
            JObject json = JObject.Parse(returnedJsonStr);
            var runJson = json["run"] as JObject;

            if (runJson == null)
            {
                LogDebug("Run data not found.");
                return;
            }

            string user = json["user"]?.ToString() ?? "Unknown User";
            LogDebug($"User set to: {user}");

            SetGeneralArgs(runJson);
            LogDebug("General arguments set");

            int currentSplitIndex = runJson["currentSplitIndex"]?.ToObject<int>() ?? -1;
            LogDebug($"Current Split Index: {currentSplitIndex}");

            if (currentSplitIndex == -1)
            {
                SetSegmentArgs(-1, runJson, true);
                HandleResetEvent();
                LogDebug("Reset event handled");
                return;
            }

            int previousGlobalSplitIndex = _CPH.GetGlobalVar<int>("theRunGgCurrentSplitIndex", false);
            LogDebug($"Previous Global Split Index: {previousGlobalSplitIndex}");

            var currentSplitData = HandleSplitEvents(runJson, currentSplitIndex, previousGlobalSplitIndex);

            if (currentSplitData == null)
            {
                LogDebug("Skip or undo event triggered, exiting early");
                _CPH.SetGlobalVar("theRunGgPreviousPaceDifference", 0, false);
                return;
            }

            HandleEvents(runJson, currentSplitData);
            LogDebug("Events handled");

            TrackProgress(currentSplitIndex, runJson);
            LogDebug("Progress tracked");
        }

        private void SetGeneralArgs(JObject runJson)
        {
            LogDebug("Setting general arguments");
            var gameData = runJson["gameData"] as JObject;
            if (gameData != null)
            {
                SetGameDataArguments(gameData);
            }

            _CPH.SetArgument("data.currentComparison", runJson["currentComparison"]?.ToString());
            _CPH.SetArgument("data.currentSplitName", runJson["currentSplitName"]?.ToString());
            _CPH.SetArgument("data.currentSplitIndex", runJson["currentSplitIndex"]?.ToObject<int>());
            LogDebug("General time-related arguments set");

            SetTimeArgument("data.bestPossible", runJson["bestPossible"]);
            SetTimeArgument("data.currentPrediction", runJson["currentPrediction"]);
            SetTimeArgument("data.personalBest", gameData?["personalBest"]);
            SetTimeArgument("data.totalRunTime", gameData?["totalRunTime"]);
            SetTimeArgument("data.timeToSave", gameData?["timeToSave"]);
            SetTimeArgument("data.sumOfBests", gameData?["sumOfBests"]);
        }

        private void SetGameDataArguments(JObject gameData)
        {
            LogDebug("Setting game data arguments");
            _CPH.SetArgument("gameData.category", gameData["run"]?.ToString());
            _CPH.SetArgument("gameData.game", gameData["game"]?.ToString());
            _CPH.SetArgument("gameData.platform", gameData["platform"]?.ToString());
            _CPH.SetArgument("gameData.gameImage", gameData["gameImage"]?.ToString());

            var variables = gameData["variables"] as JObject;
            if (variables != null)
            {
                LogDebug("Setting game data variables");
                _CPH.SetArgument("gameData.layout", variables["Layout"]?.ToString());
                _CPH.SetArgument("gameData.playStyle", variables["Play Style"]?.ToString());
                _CPH.SetArgument("gameData.runType", variables["Run Type"]?.ToString());
                _CPH.SetArgument("gameData.version", variables["Version"]?.ToString());
            }
        }

        private JObject HandleSplitEvents(JObject runJson, int currentSplitIndex, int previousGlobalSplitIndex)
        {
            LogDebug("Handling split events");
            int previousSplitIndex = currentSplitIndex - 1;
            JObject previousSplitData = (previousSplitIndex >= 0) ? SetSegmentArgs(previousSplitIndex, runJson, false) : null;

            if (previousGlobalSplitIndex == currentSplitIndex + 1)
            {
                LogDebug("Undo split event detected");
                SetSegmentArgs(currentSplitIndex, runJson, true);
                HandleUndoSplitEvent(previousGlobalSplitIndex);
                return null;
            }

            var currentSplitData = SetSegmentArgs(currentSplitIndex, runJson, true);
            if (previousSplitData != null && currentSplitIndex > 0 && previousSplitData["splitTime"]?.ToObject<double?>() == null)
            {
                LogDebug("Skip split event detected");
                HandleSkipSplitEvent(previousSplitData);
                return null;
            }

            return currentSplitData;
        }

        private void HandleEvents(JObject runJson, JObject currentSplitData)
        {
            LogDebug("Handling run events");
            var events = runJson["events"] as JArray;
            if (events == null)
            {
                LogDebug("No events found in run data.");
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

                LogDebug($"Handling event of type: {eventType}");
                HandleEventWithType(eventData, eventType);
            }
        }

        private void HandleEventWithType(JObject eventData, string eventPrefix)
        {
            string eventPrefixLower = char.ToLower(eventPrefix[0]) + eventPrefix.Substring(1);
            LogDebug($"Handling event data for {eventPrefixLower}");

            if (eventData == null)
            {
                LogDebug($"No data available for event with prefix '{eventPrefixLower}'");
                return;
            }

            foreach (var property in eventData.Properties())
            {
                double? propertyValue = null;
                string argumentValue;

                if (property.Value.Type == JTokenType.Float || property.Value.Type == JTokenType.Integer)
                {
                    propertyValue = property.Value.ToObject<double>();
                    argumentValue = FormatTimeSpan(ConvertToTimeSpan(propertyValue.Value));
                }
                else if (property.Value.Type == JTokenType.String)
                {
                    if (double.TryParse(property.Value.ToString(), out double parsedValue))
                    {
                        propertyValue = parsedValue;
                        argumentValue = FormatTimeSpan(ConvertToTimeSpan(parsedValue));
                    }
                    else
                    {
                        argumentValue = property.Value.ToString();
                    }
                }
                else
                {
                    argumentValue = property.Value.ToString();
                }

                _CPH.SetArgument($"{eventPrefixLower}.{property.Name}", argumentValue);
            }

            LogDebug($"Triggering event: theRun{eventPrefix}");
            _CPH.TriggerCodeEvent($"theRun{eventPrefix}");
        }

        public JObject SetSegmentArgs(int splitIndex, JObject runJson, bool isCurrentSplit)
        {
            LogDebug($"Setting segment args for split index: {splitIndex}");
            if (splitIndex == -1)
            {
                _CPH.SetGlobalVar("theRunGgCurrentSplitIndex", -1, false);
                _CPH.SetGlobalVar("theRunGgCurrentSplitName", "Run Reset", false);
                return null;
            }

            JObject splitData = (JObject)runJson["splits"]?[splitIndex];
            if (splitData == null)
            {
                LogDebug($"Split data not found at index {splitIndex}");
                return null;
            }

            if (isCurrentSplit)
            {
                _CPH.SetGlobalVar("theRunGgCurrentSplitIndex", splitIndex, false);
                _CPH.SetGlobalVar("theRunGgCurrentSplitName", splitData["name"]?.ToString(), false);
            }

            string prefix = isCurrentSplit ? "currentSplit" : "previousSplit";
            _CPH.SetArgument($"{prefix}.splitName", splitData["name"]?.ToString());

            SetTimeArgument($"{prefix}.splitTime", splitData["splitTime"]);
            SetTimeArgument($"{prefix}.bestPossibleTime", splitData["bestPossibleTimeAtSplit"]);
            LogDebug("Segment arguments set");

            return splitData;
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
            return milliseconds.HasValue ? TimeSpan.FromMilliseconds(milliseconds.Value) : null;
        }

        private string FormatTimeSpan(TimeSpan? timeSpan)
        {
            return timeSpan.HasValue ? timeSpan.Value.ToString(@"hh\:mm\:ss\.fff") : null;
        }

        private void HandleResetEvent()
        {
            LogDebug("Handling reset event");
            _CPH.TriggerCodeEvent("theRunRunReset");
        }

        private void HandleSkipSplitEvent(JObject previousSplitData)
        {
            LogDebug("Handling skip split event");
            _CPH.SetArgument("skippedSplit.splitName", previousSplitData["name"]?.ToObject<string>());
            _CPH.TriggerCodeEvent("theRunSkipSplit");
        }

        private void HandleUndoSplitEvent(int splitIndex)
        {
            LogDebug("Handling undo split event");
            _CPH.SetArgument("undoneSplit.splitIndex", splitIndex - 1);
            _CPH.TriggerCodeEvent("theRunUndoSplit");
        }

        public void TrackProgress(int currentSplitIndex, JObject runJson)
        {
            LogDebug("Tracking progress");

            if (currentSplitIndex < 1)
            {
                LogDebug("Not enough data to calculate progress (first split or missing data).");
                return;
            }

            var previousSplitData = runJson["splits"]?[currentSplitIndex - 1] as JObject;
            var comparisonTime = previousSplitData?["comparisons"]?["Personal Best"]?.ToObject<double?>();

            if (previousSplitData != null && comparisonTime.HasValue)
            {
                double currentPaceDifference = CalculatePace(previousSplitData, comparisonTime.Value, currentSplitIndex - 1);
                double previousPaceDifference = _CPH.GetGlobalVar<double>("theRunGgPreviousPaceDifference", false);

                DetermineTimeSavedOrLost(currentPaceDifference, previousPaceDifference);
                _CPH.SetGlobalVar("theRunGgPreviousPaceDifference", currentPaceDifference, false);
            }
        }

        private double CalculatePace(JObject previousSplitData, double comparisonTime, int splitIndex)
        {
            double actualSplitMilliseconds = previousSplitData["splitTime"]?.ToObject<double?>() ?? 0;
            TimeSpan actualSplitTime = ConvertToTimeSpan(actualSplitMilliseconds) ?? TimeSpan.Zero;
            TimeSpan comparisonSplitTime = ConvertToTimeSpan(comparisonTime) ?? TimeSpan.Zero;
            TimeSpan timeDifference = actualSplitTime - comparisonSplitTime;

            bool isAhead = timeDifference < TimeSpan.Zero;
            _CPH.SetArgument("split.paceDifference", FormatTimeSpan(timeDifference));
            _CPH.SetArgument("split.paceStatus", isAhead ? "ahead" : "behind");
            _CPH.TriggerCodeEvent(isAhead ? "theRunGreenSplit" : "theRunRedSplit");

            return timeDifference.TotalMilliseconds;
        }

        private void DetermineTimeSavedOrLost(double currentPaceDifference, double previousPaceDifference)
        {
            LogDebug("Determining if time was saved or lost");
            if (previousPaceDifference == 0)
            {
                LogDebug("Skipping time comparison as previous pace difference is zero.");
                return;
            }

            double paceDifferenceChange = currentPaceDifference - previousPaceDifference;
            bool timeSaved = paceDifferenceChange < 0;

            _CPH.SetArgument("split.timeSavedOrLost", FormatTimeSpan(TimeSpan.FromMilliseconds(Math.Abs(paceDifferenceChange))));
            _CPH.SetArgument("split.timeStatus", timeSaved ? "saved" : "lost");
            _CPH.TriggerCodeEvent(timeSaved ? "theRunTimeSave" : "theRunTimeLoss");
        }
    }
}
