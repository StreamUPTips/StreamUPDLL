using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - GENERAL
        // General OBS information and utility methods
        // ============================================================

        #region Version Info

        /// <summary>
        /// Gets version information about OBS and obs-websocket.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with obsVersion, obsWebSocketVersion, rpcVersion, platform, etc., or null if failed</returns>
        public JObject ObsGetVersion(int connection = 0) =>
            ObsSendRequest("GetVersion", null, connection);

        /// <summary>
        /// Gets the OBS Studio version string.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Version string (e.g., "30.2.2"), or null if failed</returns>
        public string ObsGetObsVersion(int connection = 0)
        {
            var version = ObsGetVersion(connection);
            return version?["obsVersion"]?.Value<string>();
        }

        /// <summary>
        /// Gets the obs-websocket version string.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Version string (e.g., "5.5.2"), or null if failed</returns>
        public string ObsGetWebSocketVersion(int connection = 0)
        {
            var version = ObsGetVersion(connection);
            return version?["obsWebSocketVersion"]?.Value<string>();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets OBS statistics (CPU, memory, FPS, etc.).
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with cpuUsage, memoryUsage, activeFps, averageFrameRenderTime, renderSkippedFrames, etc., or null if failed</returns>
        public JObject ObsGetStats(int connection = 0) =>
            ObsSendRequest("GetStats", null, connection);

        /// <summary>
        /// Gets the current CPU usage percentage.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>CPU usage percentage, or null if failed</returns>
        public double? ObsGetCpuUsage(int connection = 0)
        {
            var stats = ObsGetStats(connection);
            return stats?["cpuUsage"]?.Value<double>();
        }

        /// <summary>
        /// Gets the current memory usage in MB.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Memory usage in MB, or null if failed</returns>
        public double? ObsGetMemoryUsage(int connection = 0)
        {
            var stats = ObsGetStats(connection);
            return stats?["memoryUsage"]?.Value<double>();
        }

        /// <summary>
        /// Gets the current active FPS.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Active FPS, or null if failed</returns>
        public double? ObsGetActiveFps(int connection = 0)
        {
            var stats = ObsGetStats(connection);
            return stats?["activeFps"]?.Value<double>();
        }

        #endregion

        #region Hotkeys

        /// <summary>
        /// Gets a list of all hotkey names in OBS.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of hotkey name strings, or null if failed</returns>
        public JArray ObsGetHotkeyList(int connection = 0)
        {
            var response = ObsSendRequest("GetHotkeyList", null, connection);
            return response?["hotkeys"] as JArray;
        }

        /// <summary>
        /// Triggers a hotkey by its name.
        /// </summary>
        /// <param name="hotkeyName">Name of the hotkey</param>
        /// <param name="contextName">Optional context name</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsTriggerHotkeyByName(
            string hotkeyName,
            string contextName = null,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "TriggerHotkeyByName",
                new { hotkeyName, contextName },
                connection
            );

        /// <summary>
        /// Triggers a hotkey by key sequence.
        /// </summary>
        /// <param name="keyId">OBS key ID (see obs-hotkeys.h)</param>
        /// <param name="shift">Hold Shift</param>
        /// <param name="control">Hold Control</param>
        /// <param name="alt">Hold Alt</param>
        /// <param name="command">Hold Command (Mac)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsTriggerHotkeyByKeySequence(
            string keyId,
            bool shift = false,
            bool control = false,
            bool alt = false,
            bool command = false,
            int connection = 0
        )
        {
            var keyModifiers = new
            {
                shift,
                control,
                alt,
                command
            };
            return ObsSendRequestNoResponse(
                "TriggerHotkeyByKeySequence",
                new { keyId, keyModifiers },
                connection
            );
        }

        #endregion

        #region Custom Events

        /// <summary>
        /// Broadcasts a custom event to all WebSocket clients.
        /// </summary>
        /// <param name="eventData">Event data object to broadcast</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsBroadcastCustomEvent(object eventData, int connection = 0) =>
            ObsSendRequestNoResponse("BroadcastCustomEvent", new { eventData }, connection);

        #endregion

        #region Vendor Requests

        /// <summary>
        /// Calls a vendor-specific request (for plugins/scripts).
        /// </summary>
        /// <param name="vendorName">Name of the vendor</param>
        /// <param name="requestType">Vendor request type</param>
        /// <param name="requestData">Optional request data</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Response object with vendorName, requestType, responseData, or null if failed</returns>
        public JObject ObsCallVendorRequest(
            string vendorName,
            string requestType,
            object requestData = null,
            int connection = 0
        ) =>
            ObsSendRequest(
                "CallVendorRequest",
                new
                {
                    vendorName,
                    requestType,
                    requestData
                },
                connection
            );

        #endregion

        #region Sources

        /// <summary>
        /// Gets whether a source is active (visible in program or preview).
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with videoActive and videoShowing, or null if failed</returns>
        public JObject ObsGetSourceActive(string sourceName, int connection = 0) =>
            ObsSendRequest("GetSourceActive", new { sourceName }, connection);

        /// <summary>
        /// Checks if a source is currently visible in the program output.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if visible in program, false otherwise</returns>
        public bool ObsIsSourceActive(string sourceName, int connection = 0)
        {
            var active = ObsGetSourceActive(sourceName, connection);
            return active?["videoActive"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Takes a screenshot of a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="imageFormat">Image format (png, jpg, bmp, etc.)</param>
        /// <param name="imageWidth">Optional width in pixels</param>
        /// <param name="imageHeight">Optional height in pixels</param>
        /// <param name="imageCompressionQuality">Optional compression quality (1-100)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Base64-encoded image data, or null if failed</returns>
        public string ObsGetSourceScreenshot(
            string sourceName,
            string imageFormat = "png",
            int? imageWidth = null,
            int? imageHeight = null,
            int? imageCompressionQuality = null,
            int connection = 0
        )
        {
            var response = ObsSendRequest(
                "GetSourceScreenshot",
                new
                {
                    sourceName,
                    imageFormat,
                    imageWidth,
                    imageHeight,
                    imageCompressionQuality
                },
                connection
            );
            return response?["imageData"]?.Value<string>();
        }

        /// <summary>
        /// Saves a screenshot of a source to a file.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="imageFilePath">Path to save the image to</param>
        /// <param name="imageFormat">Image format (png, jpg, bmp, etc.)</param>
        /// <param name="imageWidth">Optional width in pixels</param>
        /// <param name="imageHeight">Optional height in pixels</param>
        /// <param name="imageCompressionQuality">Optional compression quality (1-100)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSaveSourceScreenshot(
            string sourceName,
            string imageFilePath,
            string imageFormat = "png",
            int? imageWidth = null,
            int? imageHeight = null,
            int? imageCompressionQuality = null,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SaveSourceScreenshot",
                new
                {
                    sourceName,
                    imageFormat,
                    imageFilePath,
                    imageWidth,
                    imageHeight,
                    imageCompressionQuality
                },
                connection
            );

        #endregion
    }
}
