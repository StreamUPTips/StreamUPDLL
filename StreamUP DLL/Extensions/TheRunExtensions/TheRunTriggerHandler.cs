using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void TheRunTriggerHandler()
        {
            LogInfo("Handling the trigger from TheRun.gg");

            if (!_CPH.TryGetArg("message", out string rawJson))
            {
                LogError("Unable to retrieve rawJson from the [message] arg");
                return;
            }

            // Parse JSON
            JObject json = JObject.Parse(rawJson);

            // Get user
            string user = json["user"]?.ToString() ?? "unknown";

            // Parse run data
            var runData = (JObject)json["run"];
            if (runData == null)
            {
                LogError("Run data not found.");
                return;
            }

            // Check for reset
            bool hasReset = runData["hasReset"]?.ToObject<bool>() ?? false;
            if (hasReset)
            {
                HandleResetEvent(runData);
                return; // Exit early to prevent processing other events
            }

            // Get splits and current split index
            JArray splits = runData["splits"] as JArray;
            int currentSplitIndex = runData["currentSplitIndex"]?.ToObject<int>() ?? 0;

            // Check if previous split exists and process it
            if (currentSplitIndex > 0 && splits != null)
            {
                JObject previousSplit = (JObject)splits[currentSplitIndex - 1];

                // Retrieve splitTime and pbSplitTime with null checks
                double? splitTime = previousSplit["splitTime"]?.ToObject<double>();
                double? pbSplitTime = previousSplit["pbSplitTime"]?.ToObject<double>();

                if (splitTime.HasValue && pbSplitTime.HasValue)
                {
                    // Calculate time difference
                    double timeDifference = splitTime.Value - pbSplitTime.Value;

                    // Process as green or red split, unless a gold split event exists
                    bool isGoldEventTriggered = CheckAndHandleGoldSplitEvent(runData["events"] as JArray);
                    if (!isGoldEventTriggered)
                    {
                        if (timeDifference < 0)
                        {
                            HandleGreenSplitEvent(previousSplit, timeDifference);
                        }
                        else
                        {
                            HandleRedSplitEvent(previousSplit, timeDifference);
                        }
                    }
                }
                else
                {
                    LogInfo("Previous split time or PB time is unavailable, skipping split comparison.");
                }
            }
            else
            {
                LogInfo("Previous split not available or splits array is null.");
            }

            // Handle other events in the run data
            HandleRunEvents(runData["events"] as JArray);
        }

        private bool CheckAndHandleGoldSplitEvent(JArray events)
        {
            if (events == null) return false;

            foreach (var eventToken in events)
            {
                var eventObj = (JObject)eventToken;
                if (eventObj["type"]?.ToString() == "gold_split_event")
                {
                    HandleGoldSplitEvent(eventObj);
                    return true;
                }
            }
            return false;
        }

        private void HandleRunEvents(JArray events)
        {
            if (events == null)
            {
                LogError("Events not found in run data.");
                return;
            }

            foreach (var eventToken in events)
            {
                var eventObj = (JObject)eventToken;
                string eventType = eventObj["type"]?.ToString() ?? "unknown";

                // Trigger the appropriate method based on event type
                switch (eventType)
                {
                    case "top_10_single_segment_event":
                        HandleTop10SingleSegmentEvent(eventObj);
                        break;
                    case "top_10_total_segment_event":
                        HandleTop10TotalSegmentEvent(eventObj);
                        break;
                    case "worst_10_single_segment_event":
                        HandleWorst10SingleSegmentEvent(eventObj);
                        break;
                    case "final_split_event":
                        HandleFinalSplitEvent(eventObj);
                        break;
                    case "run_ended_event":
                        HandleRunEndedEvent(eventObj);
                        break;
                    case "run_started_event":
                        HandleRunStartedEvent(eventObj);
                        break;
                    case "best_run_ever_event":
                        HandleBestRunEverEvent(eventObj);
                        break;
                    default:
                        LogError($"Unknown event type: {eventType}");
                        break;
                }
            }
        }

        // Individual event handlers

        private void HandleGoldSplitEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString();
            _CPH.TriggerCodeEvent("theRunGoldSplit");
            LogInfo("EVENT RUN");
        }

        private void HandleTop10SingleSegmentEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString();
            _CPH.TriggerCodeEvent("theRunTop10Single");
            LogInfo("EVENT RUN");
        }

        private void HandleTop10TotalSegmentEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString();
            _CPH.TriggerCodeEvent("theRunTop10Total");
            LogInfo("EVENT RUN");
        }

        private void HandleWorst10SingleSegmentEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString();
            _CPH.TriggerCodeEvent("theRunWorst10Single");
            LogInfo("EVENT RUN");
        }

        private void HandleFinalSplitEvent(JObject eventData)
        {
            string splitName = eventData["data"]?["splitName"]?.ToString();
            _CPH.TriggerCodeEvent("theRunFinalSplit");
            LogInfo("EVENT RUN");
        }

        private void HandleRunEndedEvent(JObject eventData)
        {
            string reason = eventData["data"]?["endReason"]?.ToString();
            _CPH.TriggerCodeEvent("theRunRunEnded");
            LogInfo("EVENT RUN");
        }

        private void HandleRunStartedEvent(JObject eventData)
        {
            string startTime = eventData["data"]?["startTime"]?.ToString();
            _CPH.TriggerCodeEvent("theRunRunStarted");
            LogInfo("EVENT RUN");
        }

        private void HandleBestRunEverEvent(JObject eventData)
        {
            string details = eventData["data"]?.ToString();
            _CPH.TriggerCodeEvent("theRunBestRun");
            LogInfo("EVENT RUN");
        }

        private void HandleGreenSplitEvent(JObject previousSplit, double timeDifference)
        {
            _CPH.TriggerCodeEvent("theRunGreenSplit");
            LogInfo("EVENT RUN");
        }

        private void HandleRedSplitEvent(JObject previousSplit, double timeDifference)
        {
            _CPH.TriggerCodeEvent("theRunRedSplit");
            LogInfo("EVENT RUN");
        }

        private void HandleResetEvent(JObject runData)
        {
            _CPH.TriggerCodeEvent("theRunReset");
            LogInfo("EVENT RUN");
        }
    }
}
