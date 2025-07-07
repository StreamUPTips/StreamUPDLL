using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Media Inputs Requests (from protocol) as build methods ---

        /// <summary>
        /// Gets the status of a media input.
        /// <paramref name="inputName">Name of the media input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the media input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getmediainputstatus
        /// </summary>
        public JObject ObsRawGetMediaInputStatus(string inputName = null, string inputUuid = null) =>
            BuildObsRequest("GetMediaInputStatus", new { inputName, inputUuid });

        /// <summary>
        /// Sets the cursor position of a media input.
        /// <paramref name="mediaCursor">New cursor position to set (ms).</paramref>
        /// <paramref name="inputName">Name of the media input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the media input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setmediainputcursor
        /// </summary>
        public JObject ObsRawSetMediaInputCursor(long mediaCursor, string inputName = null, string inputUuid = null) =>
            BuildObsRequest("SetMediaInputCursor", new { inputName, inputUuid, mediaCursor });

        /// <summary>
        /// Offsets the current cursor position of a media input by the specified value.
        /// <paramref name="mediaCursorOffset">Value to offset the current cursor position by (ms).</paramref>
        /// <paramref name="inputName">Name of the media input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the media input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#offsetmediainputcursor
        /// </summary>
        public JObject ObsRawOffsetMediaInputCursor(long mediaCursorOffset, string inputName = null, string inputUuid = null) =>
            BuildObsRequest("OffsetMediaInputCursor", new { inputName, inputUuid, mediaCursorOffset });

        /// <summary>
        /// Triggers an action on a media input.
        /// <paramref name="mediaAction">Identifier of the ObsMediaInputAction enum.</paramref>
        /// <paramref name="inputName">Name of the media input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the media input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggermediainputaction
        /// </summary>
        public JObject ObsRawTriggerMediaInputAction(string mediaAction, string inputName = null, string inputUuid = null) =>
            BuildObsRequest("TriggerMediaInputAction", new { inputName, inputUuid, mediaAction });
    }
}
