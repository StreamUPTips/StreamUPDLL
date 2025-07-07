namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Ui Requests (from protocol) as build methods ---

        /// <summary>
        /// Gets whether studio mode is enabled.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstudiomodeenabled
        /// </summary>
        public JObject ObsRawGetStudioModeEnabled() => BuildObsRequest("GetStudioModeEnabled");

        /// <summary>
        /// Enables or disables studio mode.
        /// <paramref name="studioModeEnabled">True to enable, false to disable.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setstudiomodeenabled
        /// </summary>
        public JObject ObsRawSetStudioModeEnabled(bool studioModeEnabled) => BuildObsRequest("SetStudioModeEnabled", new { studioModeEnabled });

        /// <summary>
        /// Opens the properties dialog of an input.
        /// <paramref name="inputName">Name of the input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#openinputpropertiesdialog
        /// </summary>
        public JObject ObsRawOpenInputPropertiesDialog(string inputName = null, string inputUuid = null) => BuildObsRequest("OpenInputPropertiesDialog", new { inputName, inputUuid });

        /// <summary>
        /// Opens the filters dialog of an input.
        /// <paramref name="inputName">Name of the input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#openinputfiltersdialog
        /// </summary>
        public JObject ObsRawOpenInputFiltersDialog(string inputName = null, string inputUuid = null) => BuildObsRequest("OpenInputFiltersDialog", new { inputName, inputUuid });

        /// <summary>
        /// Opens the interact dialog of an input.
        /// <paramref name="inputName">Name of the input (optional).</paramref>
        /// <paramref name="inputUuid">UUID of the input (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#openinputinteractdialog
        /// </summary>
        public JObject ObsRawOpenInputInteractDialog(string inputName = null, string inputUuid = null) => BuildObsRequest("OpenInputInteractDialog", new { inputName, inputUuid });

        /// <summary>
        /// Gets a list of connected monitors and information about them.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getmonitorlist
        /// </summary>
        public JObject ObsRawGetMonitorList() => BuildObsRequest("GetMonitorList");

        /// <summary>
        /// Opens a projector for a specific output video mix.
        /// <paramref name="videoMixType">Type of mix to open.</paramref>
        /// <paramref name="monitorIndex">Monitor index (optional).</paramref>
        /// <paramref name="projectorGeometry">Size/Position data for a windowed projector (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#openvideomixprojector
        /// </summary>
        public JObject ObsRawOpenVideoMixProjector(string videoMixType, int? monitorIndex = null, string projectorGeometry = null) =>
            BuildObsRequest("OpenVideoMixProjector", new { videoMixType, monitorIndex, projectorGeometry });

        /// <summary>
        /// Opens a projector for a source.
        /// <paramref name="sourceName">Name of the source (optional).</paramref>
        /// <paramref name="sourceUuid">UUID of the source (optional).</paramref>
        /// <paramref name="monitorIndex">Monitor index (optional).</paramref>
        /// <paramref name="projectorGeometry">Size/Position data for a windowed projector (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#opensourceprojector
        /// </summary>
        public JObject ObsRawOpenSourceProjector(string sourceName = null, string sourceUuid = null, int? monitorIndex = null, string projectorGeometry = null) =>
            BuildObsRequest("OpenSourceProjector", new { sourceName, sourceUuid, monitorIndex, projectorGeometry });
    }
}
