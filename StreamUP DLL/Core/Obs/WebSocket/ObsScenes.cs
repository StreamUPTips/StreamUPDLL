using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - SCENES
        // Methods for managing scenes
        // ============================================================

        #region Scene List

        /// <summary>
        /// Gets a list of all scenes in OBS.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of scene objects, or null if failed</returns>
        public JArray ObsGetSceneList(int connection = 0)
        {
            var response = ObsSendRequest("GetSceneList", null, connection);
            return response?["scenes"] as JArray;
        }

        /// <summary>
        /// Gets a list of all groups in OBS.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of group names, or null if failed</returns>
        public JArray ObsGetGroupList(int connection = 0)
        {
            var response = ObsSendRequest("GetGroupList", null, connection);
            return response?["groups"] as JArray;
        }

        #endregion

        #region Current Scene

        /// <summary>
        /// Gets the name of the current program (live) scene.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene name, or null if failed</returns>
        public string ObsGetCurrentScene(int connection = 0)
        {
            var response = ObsSendRequest("GetCurrentProgramScene", null, connection);
            return response?["sceneName"]?.Value<string>()
                ?? response?["currentProgramSceneName"]?.Value<string>();
        }

        /// <summary>
        /// Sets the current program (live) scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to switch to</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentScene(string sceneName, int connection = 0) =>
            ObsSendRequestNoResponse("SetCurrentProgramScene", new { sceneName }, connection);

        /// <summary>
        /// Gets the name of the current preview scene (studio mode only).
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene name, or null if failed or not in studio mode</returns>
        public string ObsGetCurrentPreviewScene(int connection = 0)
        {
            var response = ObsSendRequest("GetCurrentPreviewScene", null, connection);
            return response?["sceneName"]?.Value<string>()
                ?? response?["currentPreviewSceneName"]?.Value<string>();
        }

        /// <summary>
        /// Sets the current preview scene (studio mode only).
        /// </summary>
        /// <param name="sceneName">Name of the scene to preview</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentPreviewScene(string sceneName, int connection = 0) =>
            ObsSendRequestNoResponse("SetCurrentPreviewScene", new { sceneName }, connection);

        #endregion

        #region Scene Management

        /// <summary>
        /// Creates a new scene in OBS.
        /// </summary>
        /// <param name="sceneName">Name for the new scene</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>UUID of the created scene, or null if failed</returns>
        public string ObsCreateScene(string sceneName, int connection = 0)
        {
            var response = ObsSendRequest("CreateScene", new { sceneName }, connection);
            return response?["sceneUuid"]?.Value<string>();
        }

        /// <summary>
        /// Removes a scene from OBS.
        /// </summary>
        /// <param name="sceneName">Name of the scene to remove</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRemoveScene(string sceneName, int connection = 0) =>
            ObsSendRequestNoResponse("RemoveScene", new { sceneName }, connection);

        /// <summary>
        /// Renames a scene in OBS.
        /// </summary>
        /// <param name="sceneName">Current name of the scene</param>
        /// <param name="newSceneName">New name for the scene</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRenameScene(string sceneName, string newSceneName, int connection = 0) =>
            ObsSendRequestNoResponse("SetSceneName", new { sceneName, newSceneName }, connection);

        #endregion

        #region Scene Transition Override

        /// <summary>
        /// Gets the transition override for a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Response with transitionName and transitionDuration, or null if none set</returns>
        public JObject ObsGetSceneTransitionOverride(string sceneName, int connection = 0) =>
            ObsSendRequest("GetSceneSceneTransitionOverride", new { sceneName }, connection);

        /// <summary>
        /// Sets or clears the transition override for a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="transitionName">Name of the transition (null to clear)</param>
        /// <param name="transitionDuration">Duration in ms (null to use default)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSceneTransitionOverride(
            string sceneName,
            string transitionName,
            int? transitionDuration = null,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetSceneSceneTransitionOverride",
                new
                {
                    sceneName,
                    transitionName,
                    transitionDuration
                },
                connection
            );

        /// <summary>
        /// Clears the transition override for a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsClearSceneTransitionOverride(string sceneName, int connection = 0) =>
            ObsSetSceneTransitionOverride(sceneName, null, null, connection);

        #endregion
    }
}
