using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - UI
        // Methods for controlling OBS UI elements
        // ============================================================

        #region Studio Mode

        /// <summary>
        /// Checks if studio mode is enabled.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if studio mode is enabled, false otherwise</returns>
        public bool ObsIsStudioModeEnabled(int connection = 0)
        {
            var response = ObsSendRequest("GetStudioModeEnabled", null, connection);
            return response?["studioModeEnabled"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Enables or disables studio mode.
        /// </summary>
        /// <param name="enabled">True to enable, false to disable</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetStudioModeEnabled(bool enabled, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SetStudioModeEnabled",
                new { studioModeEnabled = enabled },
                connection
            );

        /// <summary>
        /// Enables studio mode.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsEnableStudioMode(int connection = 0) =>
            ObsSetStudioModeEnabled(true, connection);

        /// <summary>
        /// Disables studio mode.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsDisableStudioMode(int connection = 0) =>
            ObsSetStudioModeEnabled(false, connection);

        /// <summary>
        /// Toggles studio mode.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New state (true if now enabled), or null if failed</returns>
        public bool? ObsToggleStudioMode(int connection = 0)
        {
            bool current = ObsIsStudioModeEnabled(connection);
            if (ObsSetStudioModeEnabled(!current, connection))
                return !current;
            return null;
        }

        #endregion

        #region Dialog Boxes

        /// <summary>
        /// Opens the properties dialog for an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenInputPropertiesDialog(string inputName, int connection = 0) =>
            ObsSendRequestNoResponse("OpenInputPropertiesDialog", new { inputName }, connection);

        /// <summary>
        /// Opens the filters dialog for an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenInputFiltersDialog(string inputName, int connection = 0) =>
            ObsSendRequestNoResponse("OpenInputFiltersDialog", new { inputName }, connection);

        /// <summary>
        /// Opens the interact dialog for an input (e.g., browser source).
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenInputInteractDialog(string inputName, int connection = 0) =>
            ObsSendRequestNoResponse("OpenInputInteractDialog", new { inputName }, connection);

        #endregion

        #region Monitors

        /// <summary>
        /// Gets a list of all monitors/displays.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of monitor objects, or null if failed</returns>
        public JArray ObsGetMonitorList(int connection = 0)
        {
            var response = ObsSendRequest("GetMonitorList", null, connection);
            return response?["monitors"] as JArray;
        }

        #endregion

        #region Projectors

        /// <summary>
        /// Opens a video mix projector window.
        /// Video mix types: OBS_WEBSOCKET_VIDEO_MIX_TYPE_PREVIEW, OBS_WEBSOCKET_VIDEO_MIX_TYPE_PROGRAM,
        /// OBS_WEBSOCKET_VIDEO_MIX_TYPE_MULTIVIEW
        /// </summary>
        /// <param name="videoMixType">Type of video mix to display</param>
        /// <param name="monitorIndex">Monitor index to open on (-1 for windowed)</param>
        /// <param name="projectorGeometry">Optional Qt geometry string for windowed mode</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenVideoMixProjector(
            string videoMixType,
            int monitorIndex = -1,
            string projectorGeometry = null,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "OpenVideoMixProjector",
                new
                {
                    videoMixType,
                    monitorIndex,
                    projectorGeometry
                },
                connection
            );

        /// <summary>
        /// Opens a preview projector window.
        /// </summary>
        /// <param name="monitorIndex">Monitor index to open on (-1 for windowed)</param>
        /// <param name="projectorGeometry">Optional Qt geometry string for windowed mode</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenPreviewProjector(
            int monitorIndex = -1,
            string projectorGeometry = null,
            int connection = 0
        ) =>
            ObsOpenVideoMixProjector(
                "OBS_WEBSOCKET_VIDEO_MIX_TYPE_PREVIEW",
                monitorIndex,
                projectorGeometry,
                connection
            );

        /// <summary>
        /// Opens a program projector window.
        /// </summary>
        /// <param name="monitorIndex">Monitor index to open on (-1 for windowed)</param>
        /// <param name="projectorGeometry">Optional Qt geometry string for windowed mode</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenProgramProjector(
            int monitorIndex = -1,
            string projectorGeometry = null,
            int connection = 0
        ) =>
            ObsOpenVideoMixProjector(
                "OBS_WEBSOCKET_VIDEO_MIX_TYPE_PROGRAM",
                monitorIndex,
                projectorGeometry,
                connection
            );

        /// <summary>
        /// Opens a multiview projector window.
        /// </summary>
        /// <param name="monitorIndex">Monitor index to open on (-1 for windowed)</param>
        /// <param name="projectorGeometry">Optional Qt geometry string for windowed mode</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenMultiviewProjector(
            int monitorIndex = -1,
            string projectorGeometry = null,
            int connection = 0
        ) =>
            ObsOpenVideoMixProjector(
                "OBS_WEBSOCKET_VIDEO_MIX_TYPE_MULTIVIEW",
                monitorIndex,
                projectorGeometry,
                connection
            );

        /// <summary>
        /// Opens a source projector window.
        /// </summary>
        /// <param name="sourceName">Name of the source to project</param>
        /// <param name="monitorIndex">Monitor index to open on (-1 for windowed)</param>
        /// <param name="projectorGeometry">Optional Qt geometry string for windowed mode</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsOpenSourceProjector(
            string sourceName,
            int monitorIndex = -1,
            string projectorGeometry = null,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "OpenSourceProjector",
                new
                {
                    sourceName,
                    monitorIndex,
                    projectorGeometry
                },
                connection
            );

        #endregion
    }
}
