using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - CONFIG
        // Methods for OBS configuration (profiles, scene collections, video settings)
        // ============================================================

        #region Scene Collections

        /// <summary>
        /// Gets a list of all scene collections.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with currentSceneCollectionName and sceneCollections array, or null if failed</returns>
        public JObject ObsGetSceneCollectionList(int connection = 0)
            => ObsSendRequest("GetSceneCollectionList", null, connection);

        /// <summary>
        /// Gets the name of the current scene collection.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene collection name, or null if failed</returns>
        public string ObsGetCurrentSceneCollection(int connection = 0)
        {
            var list = ObsGetSceneCollectionList(connection);
            return list?["currentSceneCollectionName"]?.Value<string>();
        }

        /// <summary>
        /// Switches to a different scene collection.
        /// Note: This will block until the collection has finished changing.
        /// </summary>
        /// <param name="sceneCollectionName">Name of the scene collection to switch to</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentSceneCollection(string sceneCollectionName, int connection = 0)
            => ObsSendRequestNoResponse("SetCurrentSceneCollection", new { sceneCollectionName }, connection);

        /// <summary>
        /// Creates a new scene collection, switching to it in the process.
        /// Note: This will block until the collection has finished changing.
        /// </summary>
        /// <param name="sceneCollectionName">Name for the new scene collection</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsCreateSceneCollection(string sceneCollectionName, int connection = 0)
            => ObsSendRequestNoResponse("CreateSceneCollection", new { sceneCollectionName }, connection);

        #endregion

        #region Profiles

        /// <summary>
        /// Gets a list of all profiles.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with currentProfileName and profiles array, or null if failed</returns>
        public JObject ObsGetProfileList(int connection = 0)
            => ObsSendRequest("GetProfileList", null, connection);

        /// <summary>
        /// Gets the name of the current profile.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Profile name, or null if failed</returns>
        public string ObsGetCurrentProfile(int connection = 0)
        {
            var list = ObsGetProfileList(connection);
            return list?["currentProfileName"]?.Value<string>();
        }

        /// <summary>
        /// Switches to a different profile.
        /// </summary>
        /// <param name="profileName">Name of the profile to switch to</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetCurrentProfile(string profileName, int connection = 0)
            => ObsSendRequestNoResponse("SetCurrentProfile", new { profileName }, connection);

        /// <summary>
        /// Creates a new profile, switching to it in the process.
        /// </summary>
        /// <param name="profileName">Name for the new profile</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsCreateProfile(string profileName, int connection = 0)
            => ObsSendRequestNoResponse("CreateProfile", new { profileName }, connection);

        /// <summary>
        /// Removes a profile.
        /// </summary>
        /// <param name="profileName">Name of the profile to remove</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRemoveProfile(string profileName, int connection = 0)
            => ObsSendRequestNoResponse("RemoveProfile", new { profileName }, connection);

        /// <summary>
        /// Gets a parameter from the current profile's configuration.
        /// </summary>
        /// <param name="parameterCategory">Category of the parameter</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with parameterValue and defaultParameterValue, or null if failed</returns>
        public JObject ObsGetProfileParameter(string parameterCategory, string parameterName, int connection = 0)
            => ObsSendRequest("GetProfileParameter", new { parameterCategory, parameterName }, connection);

        /// <summary>
        /// Sets a parameter in the current profile's configuration.
        /// </summary>
        /// <param name="parameterCategory">Category of the parameter</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="parameterValue">Value to set</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetProfileParameter(string parameterCategory, string parameterName, string parameterValue, int connection = 0)
            => ObsSendRequestNoResponse("SetProfileParameter", new { parameterCategory, parameterName, parameterValue }, connection);

        #endregion

        #region Video Settings

        /// <summary>
        /// Gets the current video settings.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with fpsNumerator, fpsDenominator, baseWidth, baseHeight, outputWidth, outputHeight, or null if failed</returns>
        public JObject ObsGetVideoSettings(int connection = 0)
            => ObsSendRequest("GetVideoSettings", null, connection);

        /// <summary>
        /// Sets video settings.
        /// Note: Requires OBS restart to take effect.
        /// </summary>
        /// <param name="fpsNumerator">FPS numerator (e.g., 60 for 60fps)</param>
        /// <param name="fpsDenominator">FPS denominator (e.g., 1 for 60fps)</param>
        /// <param name="baseWidth">Base (canvas) width</param>
        /// <param name="baseHeight">Base (canvas) height</param>
        /// <param name="outputWidth">Output (scaled) width</param>
        /// <param name="outputHeight">Output (scaled) height</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetVideoSettings(int? fpsNumerator = null, int? fpsDenominator = null, int? baseWidth = null, int? baseHeight = null, int? outputWidth = null, int? outputHeight = null, int connection = 0)
            => ObsSendRequestNoResponse("SetVideoSettings", new { fpsNumerator, fpsDenominator, baseWidth, baseHeight, outputWidth, outputHeight }, connection);

        #endregion

        #region Stream Service Settings

        /// <summary>
        /// Gets the current stream service settings.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with streamServiceType and streamServiceSettings, or null if failed</returns>
        public JObject ObsGetStreamServiceSettings(int connection = 0)
            => ObsSendRequest("GetStreamServiceSettings", null, connection);

        /// <summary>
        /// Sets the stream service settings.
        /// </summary>
        /// <param name="streamServiceType">Stream service type</param>
        /// <param name="streamServiceSettings">Service settings object</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetStreamServiceSettings(string streamServiceType, object streamServiceSettings, int connection = 0)
            => ObsSendRequestNoResponse("SetStreamServiceSettings", new { streamServiceType, streamServiceSettings }, connection);

        #endregion

        #region Record Directory

        /// <summary>
        /// Gets the current recording directory.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Recording directory path, or null if failed</returns>
        public string ObsGetRecordDirectory(int connection = 0)
        {
            var response = ObsSendRequest("GetRecordDirectory", null, connection);
            return response?["recordDirectory"]?.Value<string>();
        }

        /// <summary>
        /// Sets the recording directory.
        /// </summary>
        /// <param name="recordDirectory">Path to the recording directory</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetRecordDirectory(string recordDirectory, int connection = 0)
            => ObsSendRequestNoResponse("SetRecordDirectory", new { recordDirectory }, connection);

        #endregion

        #region Persistent Data

        /// <summary>
        /// Gets a value from the persistent data store.
        /// </summary>
        /// <param name="realm">Data realm (OBS_WEBSOCKET_DATA_REALM_GLOBAL or OBS_WEBSOCKET_DATA_REALM_PROFILE)</param>
        /// <param name="slotName">Name of the data slot</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>The stored value, or null if not found</returns>
        public JToken ObsGetPersistentData(string realm, string slotName, int connection = 0)
        {
            var response = ObsSendRequest("GetPersistentData", new { realm, slotName }, connection);
            return response?["slotValue"];
        }

        /// <summary>
        /// Sets a value in the persistent data store.
        /// </summary>
        /// <param name="realm">Data realm (OBS_WEBSOCKET_DATA_REALM_GLOBAL or OBS_WEBSOCKET_DATA_REALM_PROFILE)</param>
        /// <param name="slotName">Name of the data slot</param>
        /// <param name="slotValue">Value to store</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetPersistentData(string realm, string slotName, object slotValue, int connection = 0)
            => ObsSendRequestNoResponse("SetPersistentData", new { realm, slotName, slotValue }, connection);

        /// <summary>
        /// Gets a global persistent data value.
        /// </summary>
        /// <param name="slotName">Name of the data slot</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>The stored value, or null if not found</returns>
        public JToken ObsGetGlobalData(string slotName, int connection = 0)
            => ObsGetPersistentData("OBS_WEBSOCKET_DATA_REALM_GLOBAL", slotName, connection);

        /// <summary>
        /// Sets a global persistent data value.
        /// </summary>
        /// <param name="slotName">Name of the data slot</param>
        /// <param name="slotValue">Value to store</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetGlobalData(string slotName, object slotValue, int connection = 0)
            => ObsSetPersistentData("OBS_WEBSOCKET_DATA_REALM_GLOBAL", slotName, slotValue, connection);

        /// <summary>
        /// Gets a profile-specific persistent data value.
        /// </summary>
        /// <param name="slotName">Name of the data slot</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>The stored value, or null if not found</returns>
        public JToken ObsGetProfileData(string slotName, int connection = 0)
            => ObsGetPersistentData("OBS_WEBSOCKET_DATA_REALM_PROFILE", slotName, connection);

        /// <summary>
        /// Sets a profile-specific persistent data value.
        /// </summary>
        /// <param name="slotName">Name of the data slot</param>
        /// <param name="slotValue">Value to store</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetProfileData(string slotName, object slotValue, int connection = 0)
            => ObsSetPersistentData("OBS_WEBSOCKET_DATA_REALM_PROFILE", slotName, slotValue, connection);

        #endregion
    }
}
