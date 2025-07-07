namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Transitions Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets a list of all transition kinds available in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#gettransitionkindlist
        /// </summary>
        public JObject ObsRawGetTransitionKindList() => BuildObsRequest("GetTransitionKindList");

        /// <summary>
        /// Gets a list of all scene transitions in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getscenetransitionlist
        /// </summary>
        public JObject ObsRawGetSceneTransitionList() => BuildObsRequest("GetSceneTransitionList");

        /// <summary>
        /// Gets the current scene transition.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getcurrentscenetransition
        /// </summary>
        public JObject ObsRawGetCurrentSceneTransition() => BuildObsRequest("GetCurrentSceneTransition");

        /// <summary>
        /// Sets the current scene transition.
        /// <paramref name="transitionName">Name of the transition to set as current.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenetransition
        /// </summary>
        public JObject ObsRawSetCurrentSceneTransition(string transitionName) => BuildObsRequest("SetCurrentSceneTransition", new { transitionName });

        /// <summary>
        /// Sets the duration of the current scene transition.
        /// <paramref name="transitionDuration">Duration of the transition in milliseconds.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenetransitionduration
        /// </summary>
        public JObject ObsRawSetCurrentSceneTransitionDuration(int transitionDuration) => BuildObsRequest("SetCurrentSceneTransitionDuration", new { transitionDuration });

        /// <summary>
        /// Sets the settings for the current scene transition.
        /// <paramref name="transitionSettings">Object containing the new transition settings.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenetransitionsettings
        /// </summary>
        public JObject ObsRawSetCurrentSceneTransitionSettings(object transitionSettings, bool overlay = false) => BuildObsRequest("SetCurrentSceneTransitionSettings", new { transitionSettings, overlay });

        /// <summary>
        /// Gets the current position of the scene transition cursor.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getcurrentscenetransitioncursor
        /// </summary>
        public JObject ObsRawGetCurrentSceneTransitionCursor() => BuildObsRequest("GetCurrentSceneTransitionCursor");

        /// <summary>
        /// Triggers the transition in studio mode.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggerstudiomodetransition
        /// </summary>
        public JObject ObsRawTriggerStudioModeTransition() => BuildObsRequest("TriggerStudioModeTransition");

        /// <summary>
        /// Sets the position of the T-Bar.
        /// <paramref name="position">Position of the T-Bar (0.0 - 1.0).</paramref>
        /// <paramref name="release">Whether to release the T-Bar after setting the position.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#settbarposition
        /// </summary>
        public JObject ObsRawSetTBarPosition(double position, bool release = false) => BuildObsRequest("SetTBarPosition", new { position, release });
    }
}
