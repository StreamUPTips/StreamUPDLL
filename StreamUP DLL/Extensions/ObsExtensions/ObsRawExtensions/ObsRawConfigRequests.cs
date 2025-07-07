using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Config Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the value of a "slot" from the selected persistent data realm.
        /// <paramref name="realm">The data realm to select. OBS_WEBSOCKET_DATA_REALM_GLOBAL or OBS_WEBSOCKET_DATA_REALM_PROFILE.</paramref>
        /// <paramref name="slotName">The name of the slot to retrieve data from.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getpersistentdata
        /// </summary>
        public JObject ObsRawGetPersistentData(string realm, string slotName) => BuildObsRequest("GetPersistentData", new { realm, slotName });

        /// <summary>
        /// Sets the value of a "slot" from the selected persistent data realm.
        /// <paramref name="realm">The data realm to select. OBS_WEBSOCKET_DATA_REALM_GLOBAL or OBS_WEBSOCKET_DATA_REALM_PROFILE.</paramref>
        /// <paramref name="slotName">The name of the slot to set data for.</paramref>
        /// <paramref name="slotValue">The value to apply to the slot.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setpersistentdata
        /// </summary>
        public JObject ObsRawSetPersistentData(string realm, string slotName, object slotValue) => BuildObsRequest("SetPersistentData", new { realm, slotName, slotValue });

        /// <summary>
        /// Gets an array of all scene collections.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getscenecollectionlist
        /// </summary>
        public JObject ObsRawGetSceneCollectionList() => BuildObsRequest("GetSceneCollectionList");

        /// <summary>
        /// Switches to a scene collection. Blocks until the collection has finished changing.
        /// <paramref name="sceneCollectionName">Name of the scene collection to switch to.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenecollection
        /// </summary>
        public JObject ObsRawSetCurrentSceneCollection(string sceneCollectionName) => BuildObsRequest("SetCurrentSceneCollection", new { sceneCollectionName });

        /// <summary>
        /// Creates a new scene collection, switching to it in the process.
        /// <paramref name="sceneCollectionName">Name for the new scene collection.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createscenecollection
        /// </summary>
        public JObject ObsRawCreateSceneCollection(string sceneCollectionName) => BuildObsRequest("CreateSceneCollection", new { sceneCollectionName });

        /// <summary>
        /// Gets an array of all profiles.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getprofilelist
        /// </summary>
        public JObject ObsRawGetProfileList() => BuildObsRequest("GetProfileList");

        /// <summary>
        /// Switches to a profile.
        /// <paramref name="profileName">Name of the profile to switch to.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentprofile
        /// </summary>
        public JObject ObsRawSetCurrentProfile(string profileName) => BuildObsRequest("SetCurrentProfile", new { profileName });

        /// <summary>
        /// Creates a new profile, switching to it in the process.
        /// <paramref name="profileName">Name for the new profile.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createprofile
        /// </summary>
        public JObject ObsRawCreateProfile(string profileName) => BuildObsRequest("CreateProfile", new { profileName });

        /// <summary>
        /// Removes a profile. If the current profile is chosen, it will change to a different profile first.
        /// <paramref name="profileName">Name of the profile to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#removeprofile
        /// </summary>
        public JObject ObsRawRemoveProfile(string profileName) => BuildObsRequest("RemoveProfile", new { profileName });

        /// <summary>
        /// Gets a parameter from the current profile's configuration.
        /// <paramref name="parameterCategory">Category of the parameter to get.</paramref>
        /// <paramref name="parameterName">Name of the parameter to get.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getprofileparameter
        /// </summary>
        public JObject ObsRawGetProfileParameter(string parameterCategory, string parameterName) => BuildObsRequest("GetProfileParameter", new { parameterCategory, parameterName });

        /// <summary>
        /// Sets the value of a parameter in the current profile's configuration.
        /// <paramref name="parameterCategory">Category of the parameter to set.</paramref>
        /// <paramref name="parameterName">Name of the parameter to set.</paramref>
        /// <paramref name="parameterValue">Value of the parameter to set. Use null to delete.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setprofileparameter
        /// </summary>
        public JObject ObsRawSetProfileParameter(string parameterCategory, string parameterName, object parameterValue) => BuildObsRequest("SetProfileParameter", new { parameterCategory, parameterName, parameterValue });

        /// <summary>
        /// Gets the current video settings.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getvideosettings
        /// </summary>
        public JObject ObsRawGetVideoSettings() => BuildObsRequest("GetVideoSettings");

        /// <summary>
        /// Sets the current video settings.
        /// <paramref name="videoSettings">Object containing the new video settings.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setvideosettings
        /// </summary>
        public JObject ObsRawSetVideoSettings(object videoSettings) => BuildObsRequest("SetVideoSettings", videoSettings);

        /// <summary>
        /// Gets the current stream service settings.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstreamservicesettings
        /// </summary>
        public JObject ObsRawGetStreamServiceSettings() => BuildObsRequest("GetStreamServiceSettings");

        /// <summary>
        /// Sets the current stream service settings.
        /// <paramref name="streamServiceSettings">Object containing the new stream service settings.</paramref>
        /// <paramref name="save">Whether to save the settings as the new default.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setstreamservicesettings
        /// </summary>
        public JObject ObsRawSetStreamServiceSettings(object streamServiceSettings, bool save = false) => BuildObsRequest("SetStreamServiceSettings", new { streamServiceSettings, save });

        /// <summary>
        /// Gets the directory used for recordings.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getrecorddirectory
        /// </summary>
        public JObject ObsRawGetRecordDirectory() => BuildObsRequest("GetRecordDirectory");

        /// <summary>
        /// Sets the directory used for recordings.
        /// <paramref name="recordDirectory">The new record directory.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setrecorddirectory
        /// </summary>
        public JObject ObsRawSetRecordDirectory(string recordDirectory) => BuildObsRequest("SetRecordDirectory", new { recordDirectory });
    }
}
