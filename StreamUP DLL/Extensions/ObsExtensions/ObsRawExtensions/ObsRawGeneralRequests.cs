namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- General Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets data about the current plugin and RPC version.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getversion
        /// </summary>
        public JObject ObsRawGetVersion() => BuildObsRequest("GetVersion");

        /// <summary>
        /// Gets statistics about OBS, obs-websocket, and the current session.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstats
        /// </summary>
        public JObject ObsRawGetStats() => BuildObsRequest("GetStats");

        /// <summary>
        /// Broadcasts a CustomEvent to all WebSocket clients. Receivers are clients which are identified and subscribed.
        /// <paramref name="eventData">Data payload to emit to all receivers.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#broadcastcustomevent
        /// </summary>
        public JObject ObsRawBroadcastCustomEvent(object eventData) => BuildObsRequest("BroadcastCustomEvent", new { eventData });

        /// <summary>
        /// Call a request registered to a vendor (third-party plugin or script).
        /// <paramref name="vendorName">Name of the vendor to use.</paramref>
        /// <paramref name="requestType">The request type to call.</paramref>
        /// <paramref name="requestData">Object containing appropriate request data.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#callvendorrequest
        /// </summary>
        public JObject ObsRawCallVendorRequest(string vendorName, string requestType, object requestData = null) => BuildObsRequest("CallVendorRequest", new { vendorName, requestType, requestData });

        /// <summary>
        /// Gets an array of all hotkey names in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#gethotkeylist
        /// </summary>
        public JObject ObsRawGetHotkeyList() => BuildObsRequest("GetHotkeyList");

        /// <summary>
        /// Triggers a hotkey using its name. See GetHotkeyList.
        /// <paramref name="hotkeyName">Name of the hotkey to trigger.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggerhotkeybyname
        /// </summary>
        public JObject ObsRawTriggerHotkeyByName(string hotkeyName) => BuildObsRequest("TriggerHotkeyByName", new { hotkeyName });

        /// <summary>
        /// Triggers a hotkey using a sequence of keys.
        /// <paramref name="keyId">The OBS key ID to use.</paramref>
        /// <paramref name="keyModifiers">Object containing key modifiers to apply (shift, control, alt, command).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggerhotkeybykeysequence
        /// </summary>
        public JObject ObsRawTriggerHotkeyByKeySequence(string keyId, object keyModifiers = null) => BuildObsRequest("TriggerHotkeyByKeySequence", new { keyId, keyModifiers });

        /// <summary>
        /// Sleeps for a time duration or number of frames. Only available in request batches with types SERIAL_REALTIME or SERIAL_FRAME.
        /// <paramref name="sleepMs">Number of milliseconds to sleep for (if SERIAL_REALTIME mode).</paramref>
        /// <paramref name="sleepFrames">Number of frames to sleep for (if SERIAL_FRAME mode).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#sleep
        /// </summary>
        public JObject ObsRawSleep(int sleepMs = 0, int sleepFrames = 0) => BuildObsRequest("Sleep", new { sleepMs, sleepFrames });
    }
}
