namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Scenes Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets an array of all scenes in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getscenelist
        /// </summary>
        public JObject ObsRawGetSceneList() => BuildObsRequest("GetSceneList");

        /// <summary>
        /// Gets an array of all groups in OBS. Groups are treated as scenes in obs-websocket.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getgrouplist
        /// </summary>
        public JObject ObsRawGetGroupList() => BuildObsRequest("GetGroupList");

        /// <summary>
        /// Gets the current program scene.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getcurrentprogramscene
        /// </summary>
        public JObject ObsRawGetCurrentProgramScene() => BuildObsRequest("GetCurrentProgramScene");

        /// <summary>
        /// Sets the current program scene.
        /// <paramref name="sceneName">Scene name to set as the current program scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentprogramscene
        /// </summary>
        public JObject ObsRawSetCurrentProgramScene(string sceneName) => BuildObsRequest("SetCurrentProgramScene", new { sceneName });

        /// <summary>
        /// Gets the current preview scene. Only available when studio mode is enabled.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getcurrentpreviewscene
        /// </summary>
        public JObject ObsRawGetCurrentPreviewScene() => BuildObsRequest("GetCurrentPreviewScene");

        /// <summary>
        /// Sets the current preview scene. Only available when studio mode is enabled.
        /// <paramref name="sceneName">Scene name to set as the current preview scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentpreviewscene
        /// </summary>
        public JObject ObsRawSetCurrentPreviewScene(string sceneName) => BuildObsRequest("SetCurrentPreviewScene", new { sceneName });

        /// <summary>
        /// Creates a new scene in OBS.
        /// <paramref name="sceneName">Name for the new scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createscene
        /// </summary>
        public JObject ObsRawCreateScene(string sceneName) => BuildObsRequest("CreateScene", new { sceneName });

        /// <summary>
        /// Removes a scene from OBS.
        /// <paramref name="sceneName">Name of the scene to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#removescene
        /// </summary>
        public JObject ObsRawRemoveScene(string sceneName) => BuildObsRequest("RemoveScene", new { sceneName });

        /// <summary>
        /// Sets the name of a scene (rename).
        /// <paramref name="sceneName">Name of the scene to be renamed.</paramref>
        /// <paramref name="newSceneName">New name for the scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setscenename
        /// </summary>
        public JObject ObsRawSetSceneName(string sceneName, string newSceneName) => BuildObsRequest("SetSceneName", new { sceneName, newSceneName });

        /// <summary>
        /// Gets the scene transition overridden for a scene.
        /// <paramref name="sceneName">Name of the scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getscenescenetransitionoverride
        /// </summary>
        public JObject ObsRawGetSceneSceneTransitionOverride(string sceneName) => BuildObsRequest("GetSceneSceneTransitionOverride", new { sceneName });

        /// <summary>
        /// Sets the scene transition overridden for a scene.
        /// <paramref name="sceneName">Name of the scene.</paramref>
        /// <paramref name="transitionName">Name of the scene transition to use as override. Specify null to remove.</paramref>
        /// <paramref name="transitionDuration">Duration to use for any overridden transition. Specify null to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setscenescenetransitionoverride
        /// </summary>
        public JObject ObsRawSetSceneSceneTransitionOverride(string sceneName, string transitionName = null, int? transitionDuration = null) => BuildObsRequest("SetSceneSceneTransitionOverride", new { sceneName, transitionName, transitionDuration });
    }
}
