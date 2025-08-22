using Newtonsoft.Json.Linq;


namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Record Requests (from protocol) as build methods ---

        /// <summary>
        /// Gets the status of the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getrecordstatus
        /// </summary>
        public JObject ObsRawGetRecordStatus() => BuildObsRequest("GetRecordStatus");

        /// <summary>
        /// Toggles the status of the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglerecord
        /// </summary>
        public JObject ObsRawToggleRecord() => BuildObsRequest("ToggleRecord");

        /// <summary>
        /// Starts the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startrecord
        /// </summary>
        public JObject ObsRawStartRecord() => BuildObsRequest("StartRecord");

        /// <summary>
        /// Stops the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stoprecord
        /// </summary>
        public JObject ObsRawStopRecord() => BuildObsRequest("StopRecord");

        /// <summary>
        /// Toggles pause on the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglerecordpause
        /// </summary>
        public JObject ObsRawToggleRecordPause() => BuildObsRequest("ToggleRecordPause");

        /// <summary>
        /// Pauses the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#pauserecord
        /// </summary>
        public JObject ObsRawPauseRecord() => BuildObsRequest("PauseRecord");

        /// <summary>
        /// Resumes the record output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#resumerecord
        /// </summary>
        public JObject ObsRawResumeRecord() => BuildObsRequest("ResumeRecord");

        /// <summary>
        /// Splits the current file being recorded into a new file.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#splitrecordfile
        /// </summary>
        public JObject ObsRawSplitRecordFile() => BuildObsRequest("SplitRecordFile");

        /// <summary>
        /// Adds a new chapter marker to the file currently being recorded.
        /// <paramref name="chapterName">Name of the new chapter (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createrecordchapter
        /// </summary>
        public JObject ObsRawCreateRecordChapter(string chapterName = null) => BuildObsRequest("CreateRecordChapter", chapterName == null ? null : new { chapterName });
    }
}
