using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - CORE HELPERS
        // ============================================================

        /// <summary>
        /// Sends a raw OBS WebSocket request and returns the response.
        /// </summary>
        /// <param name="requestType">The OBS WebSocket request type (e.g., "GetSceneList")</param>
        /// <param name="requestData">Optional request data object</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Response as JObject, or null if failed</returns>
        public JObject ObsSendRequest(string requestType, object requestData = null, int connection = 0)
        {
            string data = requestData != null ? JObject.FromObject(requestData).ToString(Formatting.None) : "{}";
            string result = _CPH.ObsSendRaw(requestType, data, connection);
            if (string.IsNullOrEmpty(result)) return null;
            try
            {
                return JObject.Parse(result);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a raw OBS WebSocket request with no response expected.
        /// </summary>
        /// <param name="requestType">The OBS WebSocket request type</param>
        /// <param name="requestData">Optional request data object</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if the request was sent successfully</returns>
        public bool ObsSendRequestNoResponse(string requestType, object requestData = null, int connection = 0)
        {
            string data = requestData != null ? JObject.FromObject(requestData).ToString(Formatting.None) : "{}";
            string result = _CPH.ObsSendRaw(requestType, data, connection);
            return !string.IsNullOrEmpty(result);
        }

        /// <summary>
        /// Sends a batch of OBS WebSocket requests.
        /// </summary>
        /// <param name="requests">List of request objects (each with requestType and optional requestData)</param>
        /// <param name="haltOnFailure">Stop processing on first failure</param>
        /// <param name="executionType">0=SerialRealtime, 1=SerialFrame, 2=Parallel</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if batch was accepted</returns>
        public bool ObsSendBatch(List<ObsBatchRequest> requests, bool haltOnFailure = false, int executionType = 0, int connection = 0)
        {
            if (requests == null || requests.Count == 0) return false;

            var batchArray = new JArray();
            foreach (var req in requests)
            {
                var reqObj = new JObject { ["requestType"] = req.RequestType };
                if (req.RequestData != null)
                    reqObj["requestData"] = JToken.FromObject(req.RequestData);
                batchArray.Add(reqObj);
            }

            string json = batchArray.ToString(Formatting.None);
            string result = _CPH.ObsSendBatchRaw(json, haltOnFailure, executionType, connection);
            return !string.IsNullOrEmpty(result);
        }

        /// <summary>
        /// Gets the scene item ID for a source in a scene. Required for many scene item operations.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene item ID, or -1 if not found</returns>
        public int ObsGetSceneItemId(string sceneName, string sourceName, int connection = 0)
        {
            var response = ObsSendRequest("GetSceneItemId", new { sceneName, sourceName }, connection);
            if (response == null) return -1;
            return response["sceneItemId"]?.Value<int>() ?? -1;
        }
    }

    /// <summary>
    /// Represents a single request in an OBS batch operation.
    /// </summary>
    public class ObsBatchRequest
    {
        /// <summary>The OBS WebSocket request type</summary>
        public string RequestType { get; set; }

        /// <summary>Optional request data</summary>
        public object RequestData { get; set; }

        public ObsBatchRequest(string requestType, object requestData = null)
        {
            RequestType = requestType;
            RequestData = requestData;
        }
    }
}
