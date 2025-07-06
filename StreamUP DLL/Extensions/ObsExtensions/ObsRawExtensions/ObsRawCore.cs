namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Core helpers ---
        public JToken SendObsRaw(JObject request, int connection = 0)
        {
            string requestType = request["requestType"]?.ToString();
            string data = request["requestData"]?.ToString() ?? "{}";
            string result = _CPH.ObsSendRaw(requestType, data, connection);
            if (string.IsNullOrEmpty(result)) return null;
            return JToken.Parse(result);
        }

        /// <summary>
        /// Sends a batch of OBS requests as a raw JSON array. Returns true if the batch was accepted by the backend.
        /// Note: StreamerBot/OBS returns an empty object for batch requests, so responses are not available.
        /// </summary>
        public bool SendObsBatchRaw(List<JObject> requests, bool haltOnFailure = true, int executionType = 0, int connection = 0)
        {
            if (requests == null || requests.Count == 0) return false;
            string json = JArray.FromObject(requests).ToString(Formatting.None);
            string result = _CPH.ObsSendBatchRaw(json, haltOnFailure, executionType, connection);
            if (string.IsNullOrEmpty(result)) return false;
            if (result.TrimStart().StartsWith("["))
                return true;
            LogInfo("SendObsBatchRaw returned non-array: " + result);
            return false;
        }

        private JObject BuildObsRequest(string requestType, object data = null)
        {
            return new JObject
            {
                ["requestType"] = requestType,
                ["requestData"] = data != null ? JToken.FromObject(data) : new JObject()
            };
        }
    }
}
