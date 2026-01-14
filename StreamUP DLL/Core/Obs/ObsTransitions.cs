using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - TRANSITIONS
        // Methods for managing scene transitions
        // ============================================================

        #region Transition List

        /// <summary>
        /// Gets a list of all available transition kinds.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of transition kind strings, or null if failed</returns>
        public JArray ObsGetTransitionKindList(int connection = 0)
        {
            var response = ObsSendRequest("GetTransitionKindList", null, connection);
            return response?["transitionKinds"] as JArray;
        }

        /// <summary>
        /// Gets a list of all scene transitions.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with currentSceneTransitionName, currentSceneTransitionKind, and transitions array, or null if failed</returns>
        public JObject ObsGetSceneTransitionList(int connection = 0)
            => ObsSendRequest("GetSceneTransitionList", null, connection);

        #endregion

        #region Current Transition

        /// <summary>
        /// Gets information about the current scene transition.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with transition details, or null if failed</returns>
        public JObject ObsGetCurrentSceneTransition(int connection = 0)
            => ObsSendRequest("GetCurrentSceneTransition", null, connection);

        /// <summary>
        /// Gets the name of the current scene transition.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Transition name, or null if failed</returns>
        public string ObsGetCurrentSceneTransitionName(int connection = 0)
        {
            var response = ObsGetCurrentSceneTransition(connection);
            return response?["transitionName"]?.Value<string>();
        }

        /// <summary>
        /// Sets the current scene transition.
        /// </summary>
        /// <param name="transitionName">Name of the transition to use</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentSceneTransition(string transitionName, int connection = 0)
            => ObsSendRequestNoResponse("SetCurrentSceneTransition", new { transitionName }, connection);

        /// <summary>
        /// Gets the duration of the current scene transition.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Duration in milliseconds, or null if failed or transition has fixed duration</returns>
        public int? ObsGetCurrentSceneTransitionDuration(int connection = 0)
        {
            var response = ObsGetCurrentSceneTransition(connection);
            return response?["transitionDuration"]?.Value<int>();
        }

        /// <summary>
        /// Sets the duration of the current scene transition.
        /// </summary>
        /// <param name="duration">Duration in milliseconds (50-20000)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentSceneTransitionDuration(int duration, int connection = 0)
            => ObsSendRequestNoResponse("SetCurrentSceneTransitionDuration", new { transitionDuration = duration }, connection);

        /// <summary>
        /// Sets the settings of the current scene transition.
        /// </summary>
        /// <param name="transitionSettings">Settings object to apply</param>
        /// <param name="overlay">True to merge with existing settings, false to replace</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentSceneTransitionSettings(object transitionSettings, bool overlay = true, int connection = 0)
            => ObsSendRequestNoResponse("SetCurrentSceneTransitionSettings", new { transitionSettings, overlay }, connection);

        #endregion

        #region Transition Control

        /// <summary>
        /// Gets the cursor position of the current scene transition (0.0 to 1.0).
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Cursor position (1.0 when not transitioning), or null if failed</returns>
        public double? ObsGetCurrentSceneTransitionCursor(int connection = 0)
        {
            var response = ObsSendRequest("GetCurrentSceneTransitionCursor", null, connection);
            return response?["transitionCursor"]?.Value<double>();
        }

        /// <summary>
        /// Triggers the current scene transition (studio mode only).
        /// Same as clicking the Transition button in studio mode.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsTriggerStudioModeTransition(int connection = 0)
            => ObsSendRequestNoResponse("TriggerStudioModeTransition", null, connection);

        /// <summary>
        /// Sets the position of the TBar (studio mode only).
        /// </summary>
        /// <param name="position">Position (0.0 to 1.0)</param>
        /// <param name="release">True to release the TBar after setting position</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetTBarPosition(double position, bool release = true, int connection = 0)
            => ObsSendRequestNoResponse("SetTBarPosition", new { position, release }, connection);

        #endregion
    }
}
