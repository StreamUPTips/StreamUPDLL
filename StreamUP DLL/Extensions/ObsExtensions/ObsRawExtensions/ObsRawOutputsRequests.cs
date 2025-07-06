namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Outputs Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the status of the virtual camera.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getvirtualcamstatus
        /// </summary>
        public JObject ObsRawGetVirtualCamStatus() => BuildObsRequest("GetVirtualCamStatus");

        /// <summary>
        /// Toggles the virtual camera on or off.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglevirtualcam
        /// </summary>
        public JObject ObsRawToggleVirtualCam() => BuildObsRequest("ToggleVirtualCam");

        /// <summary>
        /// Starts the virtual camera.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startvirtualcam
        /// </summary>
        public JObject ObsRawStartVirtualCam() => BuildObsRequest("StartVirtualCam");

        /// <summary>
        /// Stops the virtual camera.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopvirtualcam
        /// </summary>
        public JObject ObsRawStopVirtualCam() => BuildObsRequest("StopVirtualCam");

        /// <summary>
        /// Gets the status of the replay buffer.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getreplaybufferstatus
        /// </summary>
        public JObject ObsRawGetReplayBufferStatus() => BuildObsRequest("GetReplayBufferStatus");

        /// <summary>
        /// Toggles the replay buffer on or off.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglereplaybuffer
        /// </summary>
        public JObject ObsRawToggleReplayBuffer() => BuildObsRequest("ToggleReplayBuffer");

        /// <summary>
        /// Starts the replay buffer.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startreplaybuffer
        /// </summary>
        public JObject ObsRawStartReplayBuffer() => BuildObsRequest("StartReplayBuffer");

        /// <summary>
        /// Stops the replay buffer.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopreplaybuffer
        /// </summary>
        public JObject ObsRawStopReplayBuffer() => BuildObsRequest("StopReplayBuffer");

        /// <summary>
        /// Saves the current replay buffer to a file.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=savereplaybuffer
        /// </summary>
        public JObject ObsRawSaveReplayBuffer() => BuildObsRequest("SaveReplayBuffer");

        /// <summary>
        /// Gets the last replay buffer replay.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getlastreplaybufferreplay
        /// </summary>
        public JObject ObsRawGetLastReplayBufferReplay() => BuildObsRequest("GetLastReplayBufferReplay");

        /// <summary>
        /// Gets a list of all outputs in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getoutputlist
        /// </summary>
        public JObject ObsRawGetOutputList() => BuildObsRequest("GetOutputList");

        /// <summary>
        /// Gets the status of an output.
        /// <paramref name="outputName">Name of the output to get the status for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getoutputstatus
        /// </summary>
        public JObject ObsRawGetOutputStatus(string outputName) => BuildObsRequest("GetOutputStatus", new { outputName });

        /// <summary>
        /// Toggles an output on or off.
        /// <paramref name="outputName">Name of the output to toggle.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#toggleoutput
        /// </summary>
        public JObject ObsRawToggleOutput(string outputName) => BuildObsRequest("ToggleOutput", new { outputName });

        /// <summary>
        /// Starts an output.
        /// <paramref name="outputName">Name of the output to start.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startoutput
        /// </summary>
        public JObject ObsRawStartOutput(string outputName) => BuildObsRequest("StartOutput", new { outputName });

        /// <summary>
        /// Stops an output.
        /// <paramref name="outputName">Name of the output to stop.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopoutput
        /// </summary>
        public JObject ObsRawStopOutput(string outputName) => BuildObsRequest("StopOutput", new { outputName });

        /// <summary>
        /// Gets the settings for an output.
        /// <paramref name="outputName">Name of the output to get settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getoutputsettings
        /// </summary>
        public JObject ObsRawGetOutputSettings(string outputName) => BuildObsRequest("GetOutputSettings", new { outputName });

        /// <summary>
        /// Sets the settings for an output.
        /// <paramref name="outputName">Name of the output to set settings for.</paramref>
        /// <paramref name="outputSettings">Object containing the new settings for the output.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setoutputsettings
        /// </summary>
        public JObject ObsRawSetOutputSettings(string outputName, object outputSettings, bool overlay = false) => BuildObsRequest("SetOutputSettings", new { outputName, outputSettings, overlay });


    }
}
