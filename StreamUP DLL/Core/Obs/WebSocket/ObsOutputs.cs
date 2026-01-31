using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - OUTPUTS (Stream, Record, Replay Buffer, Virtual Cam)
        // Methods for controlling OBS outputs
        // ============================================================

        #region Streaming

        /// <summary>
        /// Gets the status of the stream output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with outputActive, outputReconnecting, outputTimecode, outputDuration, outputCongestion, outputBytes, outputSkippedFrames, outputTotalFrames, or null if failed</returns>
        public JObject ObsGetStreamStatus(int connection = 0) =>
            ObsSendRequest("GetStreamStatus", null, connection);

        /// <summary>
        /// Checks if OBS is currently streaming.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if streaming, false otherwise</returns>
        public bool ObsIsStreaming(int connection = 0)
        {
            var status = ObsGetStreamStatus(connection);
            return status?["outputActive"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Toggles the stream output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New streaming state (true if now streaming), or null if failed</returns>
        public bool? ObsToggleStream(int connection = 0)
        {
            var response = ObsSendRequest("ToggleStream", null, connection);
            return response?["outputActive"]?.Value<bool>();
        }

        /// <summary>
        /// Starts the stream output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStartStream(int connection = 0) =>
            ObsSendRequestNoResponse("StartStream", null, connection);

        /// <summary>
        /// Stops the stream output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStopStream(int connection = 0) =>
            ObsSendRequestNoResponse("StopStream", null, connection);

        /// <summary>
        /// Sends a caption to the stream output.
        /// </summary>
        /// <param name="caption">Caption text to send</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSendStreamCaption(string caption, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SendStreamCaption",
                new { captionText = caption },
                connection
            );

        #endregion

        #region Recording

        /// <summary>
        /// Gets the status of the record output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with outputActive, outputPaused, outputTimecode, outputDuration, outputBytes, or null if failed</returns>
        public JObject ObsGetRecordStatus(int connection = 0) =>
            ObsSendRequest("GetRecordStatus", null, connection);

        /// <summary>
        /// Checks if OBS is currently recording.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if recording, false otherwise</returns>
        public bool ObsIsRecording(int connection = 0)
        {
            var status = ObsGetRecordStatus(connection);
            return status?["outputActive"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Checks if recording is paused.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if paused, false otherwise</returns>
        public bool ObsIsRecordingPaused(int connection = 0)
        {
            var status = ObsGetRecordStatus(connection);
            return status?["outputPaused"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Toggles the record output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New recording state (true if now recording), or null if failed</returns>
        public bool? ObsToggleRecord(int connection = 0)
        {
            var response = ObsSendRequest("ToggleRecord", null, connection);
            return response?["outputActive"]?.Value<bool>();
        }

        /// <summary>
        /// Starts the record output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStartRecord(int connection = 0) =>
            ObsSendRequestNoResponse("StartRecord", null, connection);

        /// <summary>
        /// Stops the record output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Output file path, or null if failed</returns>
        public string ObsStopRecord(int connection = 0)
        {
            var response = ObsSendRequest("StopRecord", null, connection);
            return response?["outputPath"]?.Value<string>();
        }

        /// <summary>
        /// Toggles the record pause state.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsToggleRecordPause(int connection = 0) =>
            ObsSendRequestNoResponse("ToggleRecordPause", null, connection);

        /// <summary>
        /// Pauses the record output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsPauseRecord(int connection = 0) =>
            ObsSendRequestNoResponse("PauseRecord", null, connection);

        /// <summary>
        /// Resumes the record output.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsResumeRecord(int connection = 0) =>
            ObsSendRequestNoResponse("ResumeRecord", null, connection);

        /// <summary>
        /// Splits the current recording file.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSplitRecordFile(int connection = 0) =>
            ObsSendRequestNoResponse("SplitRecordFile", null, connection);

        /// <summary>
        /// Creates a chapter marker in the recording.
        /// </summary>
        /// <param name="chapterName">Optional name for the chapter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsCreateRecordChapter(string chapterName = null, int connection = 0) =>
            ObsSendRequestNoResponse(
                "CreateRecordChapter",
                chapterName != null ? new { chapterName } : null,
                connection
            );

        #endregion

        #region Replay Buffer

        /// <summary>
        /// Gets the status of the replay buffer.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if replay buffer is active, false otherwise</returns>
        public bool ObsIsReplayBufferActive(int connection = 0)
        {
            var response = ObsSendRequest("GetReplayBufferStatus", null, connection);
            return response?["outputActive"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Toggles the replay buffer.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New state (true if now active), or null if failed</returns>
        public bool? ObsToggleReplayBuffer(int connection = 0)
        {
            var response = ObsSendRequest("ToggleReplayBuffer", null, connection);
            return response?["outputActive"]?.Value<bool>();
        }

        /// <summary>
        /// Starts the replay buffer.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStartReplayBuffer(int connection = 0) =>
            ObsSendRequestNoResponse("StartReplayBuffer", null, connection);

        /// <summary>
        /// Stops the replay buffer.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStopReplayBuffer(int connection = 0) =>
            ObsSendRequestNoResponse("StopReplayBuffer", null, connection);

        /// <summary>
        /// Saves the replay buffer to a file.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSaveReplayBuffer(int connection = 0) =>
            ObsSendRequestNoResponse("SaveReplayBuffer", null, connection);

        /// <summary>
        /// Gets the path of the last saved replay buffer file.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>File path, or null if failed</returns>
        public string ObsGetLastReplayBufferReplay(int connection = 0)
        {
            var response = ObsSendRequest("GetLastReplayBufferReplay", null, connection);
            return response?["savedReplayPath"]?.Value<string>();
        }

        #endregion

        #region Virtual Camera

        /// <summary>
        /// Checks if the virtual camera is active.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if virtual camera is active, false otherwise</returns>
        public bool ObsIsVirtualCamActive(int connection = 0)
        {
            var response = ObsSendRequest("GetVirtualCamStatus", null, connection);
            return response?["outputActive"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Toggles the virtual camera.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New state (true if now active), or null if failed</returns>
        public bool? ObsToggleVirtualCam(int connection = 0)
        {
            var response = ObsSendRequest("ToggleVirtualCam", null, connection);
            return response?["outputActive"]?.Value<bool>();
        }

        /// <summary>
        /// Starts the virtual camera.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStartVirtualCam(int connection = 0) =>
            ObsSendRequestNoResponse("StartVirtualCam", null, connection);

        /// <summary>
        /// Stops the virtual camera.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStopVirtualCam(int connection = 0) =>
            ObsSendRequestNoResponse("StopVirtualCam", null, connection);

        #endregion

        #region Generic Outputs

        /// <summary>
        /// Gets a list of all outputs.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of output objects, or null if failed</returns>
        public JArray ObsGetOutputList(int connection = 0)
        {
            var response = ObsSendRequest("GetOutputList", null, connection);
            return response?["outputs"] as JArray;
        }

        /// <summary>
        /// Gets the status of a specific output.
        /// </summary>
        /// <param name="outputName">Name of the output</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Output status object, or null if failed</returns>
        public JObject ObsGetOutputStatus(string outputName, int connection = 0) =>
            ObsSendRequest("GetOutputStatus", new { outputName }, connection);

        /// <summary>
        /// Toggles a specific output.
        /// </summary>
        /// <param name="outputName">Name of the output</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New state (true if now active), or null if failed</returns>
        public bool? ObsToggleOutput(string outputName, int connection = 0)
        {
            var response = ObsSendRequest("ToggleOutput", new { outputName }, connection);
            return response?["outputActive"]?.Value<bool>();
        }

        /// <summary>
        /// Starts a specific output.
        /// </summary>
        /// <param name="outputName">Name of the output</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStartOutput(string outputName, int connection = 0) =>
            ObsSendRequestNoResponse("StartOutput", new { outputName }, connection);

        /// <summary>
        /// Stops a specific output.
        /// </summary>
        /// <param name="outputName">Name of the output</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStopOutput(string outputName, int connection = 0) =>
            ObsSendRequestNoResponse("StopOutput", new { outputName }, connection);

        /// <summary>
        /// Gets the settings of a specific output.
        /// </summary>
        /// <param name="outputName">Name of the output</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Output settings object, or null if failed</returns>
        public JObject ObsGetOutputSettings(string outputName, int connection = 0)
        {
            var response = ObsSendRequest("GetOutputSettings", new { outputName }, connection);
            return response?["outputSettings"] as JObject;
        }

        /// <summary>
        /// Sets the settings of a specific output.
        /// </summary>
        /// <param name="outputName">Name of the output</param>
        /// <param name="outputSettings">Settings object to apply</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetOutputSettings(
            string outputName,
            object outputSettings,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetOutputSettings",
                new { outputName, outputSettings },
                connection
            );

        #endregion
    }
}
