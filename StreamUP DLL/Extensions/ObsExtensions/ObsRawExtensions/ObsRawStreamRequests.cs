using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Stream Requests (from protocol) as build methods ---

        /// <summary>
        /// Gets the status of the stream output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstreamstatus
        /// </summary>
        public JObject ObsRawGetStreamStatus() => BuildObsRequest("GetStreamStatus");


        /// <summary>
        /// Toggles the status of the stream output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglestream
        /// </summy>
        public JObject ObsRawToggleStream() => BuildObsRequest("ToggleStream");


        /// <summary>
        /// Starts the stream output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startstream
    /// </summar/// y>
        public JObject ObsRawStartStream() => BuildObsRequest("StartStream");


        /// <summary>
        /// Stops the stream output.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopstream
        /// </summary>

        public JObject ObsRawStopStream() => BuildObsRequest("StopStream");


        /// <summary>
        /// Sends CEA-608 caption text over the stream output.
        /// <paramref name="captionText">Caption text to send.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#sendstreamcaption
        /// </summary>

        public JObject ObsRawSendStreamCaption(string captionText) => BuildObsRequest("SendStreamCaption", new { captionText });

    }
}
