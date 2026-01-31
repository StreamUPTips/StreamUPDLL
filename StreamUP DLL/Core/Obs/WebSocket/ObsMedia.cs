using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - MEDIA INPUTS
        // Methods for controlling media playback
        // ============================================================

        #region Media Status

        /// <summary>
        /// Gets the status of a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with mediaState, mediaDuration, mediaCursor, or null if failed</returns>
        public JObject ObsGetMediaInputStatus(string inputName, int connection = 0) =>
            ObsSendRequest("GetMediaInputStatus", new { inputName }, connection);

        /// <summary>
        /// Gets the current playback state of a media input.
        /// States: OBS_MEDIA_STATE_NONE, OBS_MEDIA_STATE_PLAYING, OBS_MEDIA_STATE_OPENING,
        /// /// OBS_MEDIA_STATE_BUFFERING, OBS_MEDIA_STATE_PAUSED, OBS_MEDIA_STATE_STOPPED,
        /// OBS_MEDIA_STATE_ENDED, OBS_MEDIA_STATE_ERROR
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Media state string, or null if failed</returns>
        public string ObsGetMediaState(string inputName, int connection = 0)
        {
            var status = ObsGetMediaInputStatus(inputName, connection);
            return status?["mediaState"]?.Value<string>();
        }

        /// <summary>
        /// Gets the duration of a media input in milliseconds.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Duration in ms, or null if failed</returns>
        public long? ObsGetMediaDuration(string inputName, int connection = 0)
        {
            var status = ObsGetMediaInputStatus(inputName, connection);
            return status?["mediaDuration"]?.Value<long>();
        }

        /// <summary>
        /// Gets the current cursor position of a media input in milliseconds.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Cursor position in ms, or null if failed</returns>
        public long? ObsGetMediaCursor(string inputName, int connection = 0)
        {
            var status = ObsGetMediaInputStatus(inputName, connection);
            return status?["mediaCursor"]?.Value<long>();
        }

        #endregion

        #region Media Cursor Control

        /// <summary>
        /// Sets the cursor position of a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="mediaCursor">Cursor position in milliseconds</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetMediaCursor(string inputName, long mediaCursor, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SetMediaInputCursor",
                new { inputName, mediaCursor },
                connection
            );

        /// <summary>
        /// Offsets the cursor position of a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="mediaCursorOffset">Offset in milliseconds (positive = forward, negative = backward)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOffsetMediaCursor(
            string inputName,
            long mediaCursorOffset,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "OffsetMediaInputCursor",
                new { inputName, mediaCursorOffset },
                connection
            );

        /// <summary>
        /// Seeks forward in a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="milliseconds">Milliseconds to seek forward</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSeekMediaForward(string inputName, long milliseconds, int connection = 0) =>
            ObsOffsetMediaCursor(inputName, milliseconds, connection);

        /// <summary>
        /// Seeks backward in a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="milliseconds">Milliseconds to seek backward</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSeekMediaBackward(string inputName, long milliseconds, int connection = 0) =>
            ObsOffsetMediaCursor(inputName, -milliseconds, connection);

        #endregion

        #region Media Actions

        /// <summary>
        /// Triggers a media input action.
        /// Actions: OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NONE, OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY,
        /// OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE, OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP,
        /// OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART, OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT,
        /// OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="mediaAction">Media action constant</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsTriggerMediaInputAction(
            string inputName,
            string mediaAction,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "TriggerMediaInputAction",
                new { inputName, mediaAction },
                connection
            );

        /// <summary>
        /// Plays a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsPlayMedia(string inputName, int connection = 0) =>
            ObsTriggerMediaInputAction(
                inputName,
                "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY",
                connection
            );

        /// <summary>
        /// Pauses a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsPauseMedia(string inputName, int connection = 0) =>
            ObsTriggerMediaInputAction(
                inputName,
                "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE",
                connection
            );

        /// <summary>
        /// Stops a media input.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsStopMedia(string inputName, int connection = 0) =>
            ObsTriggerMediaInputAction(
                inputName,
                "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP",
                connection
            );

        /// <summary>
        /// Restarts a media input from the beginning.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRestartMedia(string inputName, int connection = 0) =>
            ObsTriggerMediaInputAction(
                inputName,
                "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART",
                connection
            );

        /// <summary>
        /// Goes to the next item in a media playlist.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsNextMediaItem(string inputName, int connection = 0) =>
            ObsTriggerMediaInputAction(
                inputName,
                "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT",
                connection
            );

        /// <summary>
        /// Goes to the previous item in a media playlist.
        /// </summary>
        /// <param name="inputName">Name of the media input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsPreviousMediaItem(string inputName, int connection = 0) =>
            ObsTriggerMediaInputAction(
                inputName,
                "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS",
                connection
            );

        #endregion
    }
}
