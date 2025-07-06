using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Core helpers ---
        public JToken SendObsRaw(JObject request, int connection = 0)
        {
            string requestType = request["requestType"]?.ToString();
            string data = request["requestData"]?.ToString() ?? "{}";
            string result = _CPH.ObsSendRaw(requestType, data, connection);
            if (string.IsNullOrEmpty(result)) return null;
            return JToken.Parse(result);
        }

        /// <summary>
        /// Sends a batch of OBS requests as a raw JSON array. Returns true if the batch was accepted by the backend.
        /// Note: StreamerBot/OBS returns an empty object for batch requests, so responses are not available.
        /// </summary>
        public bool SendObsBatchRaw(List<JObject> requests, bool haltOnFailure = true, int executionType = 0, int connection = 0)
        {
            if (requests == null || requests.Count == 0) return false;
            string json = JArray.FromObject(requests).ToString(Formatting.None);
            string result = _CPH.ObsSendBatchRaw(json, haltOnFailure, executionType, connection);
            if (string.IsNullOrEmpty(result)) return false;
            if (result.TrimStart().StartsWith("["))
                return true;
            LogInfo("SendObsBatchRaw returned non-array: " + result);
            return false;
        }

        public JObject BuildObsRequest(string requestType, object data = null)
        {
            return new JObject
            {
                ["requestType"] = requestType,
                ["requestData"] = data != null ? JToken.FromObject(data) : new JObject()
            };
        }

        // --- General Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets data about the current plugin and RPC version.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getversion
        /// </summary>
        public JObject ObsRawGetVersion() => BuildObsRequest("GetVersion");

        /// <summary>
        /// Gets statistics about OBS, obs-websocket, and the current session.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstats
        /// </summary>
        public JObject ObsRawGetStats() => BuildObsRequest("GetStats");

        /// <summary>
        /// Broadcasts a CustomEvent to all WebSocket clients. Receivers are clients which are identified and subscribed.
        /// <paramref name="eventData">Data payload to emit to all receivers.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#broadcastcustomevent
        /// </summary>
        public JObject ObsRawBroadcastCustomEvent(object eventData) => BuildObsRequest("BroadcastCustomEvent", new { eventData });

        /// <summary>
        /// Call a request registered to a vendor (third-party plugin or script).
        /// <paramref name="vendorName">Name of the vendor to use.</paramref>
        /// <paramref name="requestType">The request type to call.</paramref>
        /// <paramref name="requestData">Object containing appropriate request data.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#callvendorrequest
        /// </summary>
        public JObject ObsRawCallVendorRequest(string vendorName, string requestType, object requestData = null) => BuildObsRequest("CallVendorRequest", new { vendorName, requestType, requestData });

        /// <summary>
        /// Gets an array of all hotkey names in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#gethotkeylist
        /// </summary>
        public JObject ObsRawGetHotkeyList() => BuildObsRequest("GetHotkeyList");

        /// <summary>
        /// Triggers a hotkey using its name. See GetHotkeyList.
        /// <paramref name="hotkeyName">Name of the hotkey to trigger.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggerhotkeybyname
        /// </summary>
        public JObject ObsRawTriggerHotkeyByName(string hotkeyName) => BuildObsRequest("TriggerHotkeyByName", new { hotkeyName });

        /// <summary>
        /// Triggers a hotkey using a sequence of keys.
        /// <paramref name="keyId">The OBS key ID to use.</paramref>
        /// <paramref name="keyModifiers">Object containing key modifiers to apply (shift, control, alt, command).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggerhotkeybykeysequence
        /// </summary>
        public JObject ObsRawTriggerHotkeyByKeySequence(string keyId, object keyModifiers = null) => BuildObsRequest("TriggerHotkeyByKeySequence", new { keyId, keyModifiers });

        /// <summary>
        /// Sleeps for a time duration or number of frames. Only available in request batches with types SERIAL_REALTIME or SERIAL_FRAME.
        /// <paramref name="sleepMs">Number of milliseconds to sleep for (if SERIAL_REALTIME mode).</paramref>
        /// <paramref name="sleepFrames">Number of frames to sleep for (if SERIAL_FRAME mode).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#sleep
        /// </summary>
        public JObject ObsRawSleep(int sleepMs = 0, int sleepFrames = 0) => BuildObsRequest("Sleep", new { sleepMs, sleepFrames });

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

        // --- Sources Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the active and show state of a source (input or scene).
        /// <paramref name="sourceName">Name of the source to get the active state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourceactive
        /// </summary>
        public JObject ObsRawGetSourceActive(string sourceName) => BuildObsRequest("GetSourceActive", new { sourceName });

        /// <summary>
        /// Gets a Base64-encoded screenshot of a source (input or scene).
        /// <paramref name="sourceName">Name of the source to take a screenshot of.</paramref>
        /// <paramref name="imageFormat">Image compression format to use.</paramref>
        /// <paramref name="imageWidth">Width to scale the screenshot to.</paramref>
        /// <paramref name="imageHeight">Height to scale the screenshot to.</paramref>
        /// <paramref name="imageCompressionQuality">Compression quality to use (0-100).</paramref>
        /// <paramref name="imageFilePath">Path to save the screenshot file to (optional).</paramref>
        /// <paramref name="imageEmbed">Whether to embed the image in the response (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcescreenshot
        /// </summary>
        public JObject ObsRawGetSourceScreenshot(string sourceName, string imageFormat, int imageWidth = 0, int imageHeight = 0, int imageCompressionQuality = 100, string imageFilePath = null, bool? imageEmbed = null) => BuildObsRequest("GetSourceScreenshot", new { sourceName, imageFormat, imageWidth, imageHeight, imageCompressionQuality, imageFilePath, imageEmbed });

        /// <summary>
        /// Saves a screenshot of a source to the filesystem.
        /// <paramref name="sourceName">Name of the source to take a screenshot of.</paramref>
        /// <paramref name="imageFormat">Image compression format to use.</paramref>
        /// <paramref name="imageFilePath">Path to save the screenshot file to.</paramref>
        /// <paramref name="imageWidth">Width to scale the screenshot to.</paramref>
        /// <paramref name="imageHeight">Height to scale the screenshot to.</paramref>
        /// <paramref name="imageCompressionQuality">Compression quality to use (0-100).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#savesourcescreenshot
        /// </summary>
        public JObject ObsRawSaveSourceScreenshot(string sourceName, string imageFormat, string imageFilePath, int imageWidth = 0, int imageHeight = 0, int imageCompressionQuality = 100) => BuildObsRequest("SaveSourceScreenshot", new { sourceName, imageFormat, imageFilePath, imageWidth, imageHeight, imageCompressionQuality });

        // --- Inputs Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets a list of all inputs (sources) in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputlist
        /// </summary>
        public JObject ObsRawGetInputList() => BuildObsRequest("GetInputList");

        /// <summary>
        /// Gets a list of all input kinds available in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputkindlist
        /// </summary>
        public JObject ObsRawGetInputKindList() => BuildObsRequest("GetInputKindList");

        /// <summary>
        /// Gets a list of special inputs used by OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getspecialinputs
        /// </summary>
        public JObject ObsRawGetSpecialInputs() => BuildObsRequest("GetSpecialInputs");

        /// <summary>
        /// Creates a new input (source) and adds it to a scene.
        /// <paramref name="sceneName">Name of the scene to add the input to.</paramref>
        /// <paramref name="inputName">Name for the new input.</paramref>
        /// <paramref name="inputKind">The kind of input to create.</paramref>
        /// <paramref name="inputSettings">Object containing settings for the input.</paramref>
        /// <paramref name="setVisible">Whether to set the input as visible.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createinput
        /// </summary>
        public JObject ObsRawCreateInput(string sceneName, string inputName, string inputKind, object inputSettings = null, bool setVisible = true) => BuildObsRequest("CreateInput", new { sceneName, inputName, inputKind, inputSettings, setVisible });

        /// <summary>
        /// Removes an input (source) from OBS.
        /// <paramref name="inputName">Name of the input to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#removeinput
        /// </summary>
        public JObject ObsRawRemoveInput(string inputName) => BuildObsRequest("RemoveInput", new { inputName });

        /// <summary>
        /// Renames an input (source).
        /// <paramref name="inputName">Name of the input to be renamed.</paramref>
        /// <paramref name="newInputName">New name for the input.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputname
        /// </summary>
        public JObject ObsRawSetInputName(string inputName, string newInputName) => BuildObsRequest("SetInputName", new { inputName, newInputName });

        /// <summary>
        /// Gets the default settings for an input kind.
        /// <paramref name="inputKind">The input kind to get the default settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputdefaultsettings
        /// </summary>
        public JObject ObsRawGetInputDefaultSettings(string inputKind) => BuildObsRequest("GetInputDefaultSettings", new { inputKind });

        /// <summary>
        /// Gets the settings for an input (source).
        /// <paramref name="inputName">Name of the input to get settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputsettings
        /// </summary>
        public JObject ObsRawGetInputSettings(string inputName) => BuildObsRequest("GetInputSettings", new { inputName });

        /// <summary>
        /// Sets the settings for an input (source).
        /// <paramref name="inputName">Name of the input to set settings for.</paramref>
        /// <paramref name="inputSettings">Object containing the new settings for the input.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputsettings
        /// </summary>
        public JObject ObsRawSetInputSettings(string inputName, object inputSettings, bool overlay = false) => BuildObsRequest("SetInputSettings", new { inputName, inputSettings, overlay });

        /// <summary>
        /// Gets the mute state of an input (source).
        /// <paramref name="inputName">Name of the input to get the mute state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputmute
        /// </summary>
        public JObject ObsRawGetInputMute(string inputName) => BuildObsRequest("GetInputMute", new { inputName });

        /// <summary>
        /// Sets the mute state of an input (source).
        /// <paramref name="inputName">Name of the input to set the mute state for.</paramref>
        /// <paramref name="inputMuted">Whether the input should be muted.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputmute
        /// </summary>
        public JObject ObsRawSetInputMute(string inputName, bool inputMuted) => BuildObsRequest("SetInputMute", new { inputName, inputMuted });

        /// <summary>
        /// Toggles the mute state of an input (source).
        /// <paramref name="inputName">Name of the input to toggle the mute state for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#toggleinputmute
        /// </summary>
        public JObject ObsRawToggleInputMute(string inputName) => BuildObsRequest("ToggleInputMute", new { inputName });

        /// <summary>
        /// Gets the volume level of an input (source).
        /// <paramref name="inputName">Name of the input to get the volume level of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputvolume
        /// </summary>
        public JObject ObsRawGetInputVolume(string inputName) => BuildObsRequest("GetInputVolume", new { inputName });

        /// <summary>
        /// Sets the volume level of an input (source).
        /// <paramref name="inputName">Name of the input to set the volume level for.</paramref>
        /// <paramref name="inputVolumeDb">Volume level in decibels.</paramref>
        /// <paramref name="inputVolumeMul">Volume level as a multiplier.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputvolume
        /// </summary>
        public JObject ObsRawSetInputVolume(string inputName, double inputVolumeDb, double inputVolumeMul) => BuildObsRequest("SetInputVolume", new { inputName, inputVolumeDb, inputVolumeMul });

        /// <summary>
        /// Gets the audio balance of an input (source).
        /// <paramref name="inputName">Name of the input to get the audio balance of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputaudiobalance
        /// </summary>
        public JObject ObsRawGetInputAudioBalance(string inputName) => BuildObsRequest("GetInputAudioBalance", new { inputName });

        /// <summary>
        /// Sets the audio balance of an input (source).
        /// <paramref name="inputName">Name of the input to set the audio balance for.</paramref>
        /// <paramref name="inputAudioBalance">Audio balance value.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputaudiobalance
        /// </summary>
        public JObject ObsRawSetInputAudioBalance(string inputName, double inputAudioBalance) => BuildObsRequest("SetInputAudioBalance", new { inputName, inputAudioBalance });

        /// <summary>
        /// Gets the audio sync offset of an input (source).
        /// <paramref name="inputName">Name of the input to get the audio sync offset of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputaudiosyncoffset
        /// </summary>
        public JObject ObsRawGetInputAudioSyncOffset(string inputName) => BuildObsRequest("GetInputAudioSyncOffset", new { inputName });

        /// <summary>
        /// Sets the audio sync offset of an input (source).
        /// <paramref name="inputName">Name of the input to set the audio sync offset for.</paramref>
        /// <paramref name="inputAudioSyncOffset">Audio sync offset in milliseconds.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputaudiosyncoffset
        /// </summary>
        public JObject ObsRawSetInputAudioSyncOffset(string inputName, int inputAudioSyncOffset) => BuildObsRequest("SetInputAudioSyncOffset", new { inputName, inputAudioSyncOffset });

        /// <summary>
        /// Gets the audio monitor type of an input (source).
        /// <paramref name="inputName">Name of the input to get the audio monitor type of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputaudiomonitortype
        /// </summary>
        public JObject ObsRawGetInputAudioMonitorType(string inputName) => BuildObsRequest("GetInputAudioMonitorType", new { inputName });

        /// <summary>
        /// Sets the audio monitor type of an input (source).
        /// <paramref name="inputName">Name of the input to set the audio monitor type for.</paramref>
        /// <paramref name="monitorType">Monitor type to set.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputaudiomonitortype
        /// </summary>
        public JObject ObsRawSetInputAudioMonitorType(string inputName, string monitorType) => BuildObsRequest("SetInputAudioMonitorType", new { inputName, monitorType });

        /// <summary>
        /// Gets the audio tracks of an input (source).
        /// <paramref name="inputName">Name of the input to get the audio tracks of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputaudiotracks
        /// </summary>
        public JObject ObsRawGetInputAudioTracks(string inputName) => BuildObsRequest("GetInputAudioTracks", new { inputName });

        /// <summary>
        /// Sets the audio tracks of an input (source).
        /// <paramref name="inputName">Name of the input to set the audio tracks for.</paramref>
        /// <paramref name="inputAudioTracks">Object containing the audio tracks to set.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputaudiotracks
        /// </summary>
        public JObject ObsRawSetInputAudioTracks(string inputName, object inputAudioTracks) => BuildObsRequest("SetInputAudioTracks", new { inputName, inputAudioTracks });

        /// <summary>
        /// Gets the list of property items for an input properties list.
        /// <paramref name="inputName">Name of the input to get the property items for.</paramref>
        /// <paramref name="propertyName">Name of the property to get the items for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputpropertieslistpropertyitems
        /// </summary>
        public JObject ObsRawGetInputPropertiesListPropertyItems(string inputName, string propertyName) => BuildObsRequest("GetInputPropertiesListPropertyItems", new { inputName, propertyName });

        /// <summary>
        /// Presses a button in the input properties dialog.
        /// <paramref name="inputName">Name of the input to press the button for.</paramref>
        /// <paramref name="propertyName">Name of the property the button belongs to.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#pressinputpropertiesbutton
        /// </summary>
        public JObject ObsRawPressInputPropertiesButton(string inputName, string propertyName) => BuildObsRequest("PressInputPropertiesButton", new { inputName, propertyName });

        // --- Transitions Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets a list of all transition kinds available in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#gettransitionkindlist
        /// </summary>
        public JObject ObsRawGetTransitionKindList() => BuildObsRequest("GetTransitionKindList");

        /// <summary>
        /// Gets a list of all scene transitions in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getscenetransitionlist
        /// </summary>
        public JObject ObsRawGetSceneTransitionList() => BuildObsRequest("GetSceneTransitionList");

        /// <summary>
        /// Gets the current scene transition.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getcurrentscenetransition
        /// </summary>
        public JObject ObsRawGetCurrentSceneTransition() => BuildObsRequest("GetCurrentSceneTransition");

        /// <summary>
        /// Sets the current scene transition.
        /// <paramref name="transitionName">Name of the transition to set as current.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenetransition
        /// </summary>
        public JObject ObsRawSetCurrentSceneTransition(string transitionName) => BuildObsRequest("SetCurrentSceneTransition", new { transitionName });

        /// <summary>
        /// Sets the duration of the current scene transition.
        /// <paramref name="transitionDuration">Duration of the transition in milliseconds.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenetransitionduration
        /// </summary>
        public JObject ObsRawSetCurrentSceneTransitionDuration(int transitionDuration) => BuildObsRequest("SetCurrentSceneTransitionDuration", new { transitionDuration });

        /// <summary>
        /// Sets the settings for the current scene transition.
        /// <paramref name="transitionSettings">Object containing the new transition settings.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setcurrentscenetransitionsettings
        /// </summary>
        public JObject ObsRawSetCurrentSceneTransitionSettings(object transitionSettings, bool overlay = false) => BuildObsRequest("SetCurrentSceneTransitionSettings", new { transitionSettings, overlay });

        /// <summary>
        /// Gets the current position of the scene transition cursor.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getcurrentscenetransitioncursor
        /// </summary>
        public JObject ObsRawGetCurrentSceneTransitionCursor() => BuildObsRequest("GetCurrentSceneTransitionCursor");

        /// <summary>
        /// Triggers the transition in studio mode.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#triggerstudiomodetransition
        /// </summary>
        public JObject ObsRawTriggerStudioModeTransition() => BuildObsRequest("TriggerStudioModeTransition");

        /// <summary>
        /// Sets the position of the T-Bar.
        /// <paramref name="position">Position of the T-Bar (0.0 - 1.0).</paramref>
        /// <paramref name="release">Whether to release the T-Bar after setting the position.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#settbarposition
        /// </summary>
        public JObject ObsRawSetTBarPosition(double position, bool release = false) => BuildObsRequest("SetTBarPosition", new { position, release });

        // --- Filters Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets a list of filters applied to a source.
        /// <paramref name="sourceName">Name of the source to get the filters for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilterlist
        /// </summary>
        public JObject ObsRawGetSourceFilterList(string sourceName) => BuildObsRequest("GetSourceFilterList", new { sourceName });

        /// <summary>
        /// Gets the default settings for a source filter kind.
        /// <paramref name="filterKind">The filter kind to get the default settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilterdefaultsettings
        /// </summary>
        public JObject ObsRawGetSourceFilterDefaultSettings(string filterKind) => BuildObsRequest("GetSourceFilterDefaultSettings", new { filterKind });

        /// <summary>
        /// Gets the settings for a source filter.
        /// <paramref name="sourceName">Name of the source to get the filter settings for.</paramref>
        /// <paramref name="filterName">Name of the filter to get the settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefiltersettings
        /// </summary>
        public JObject ObsRawGetSourceFilterSettings(string sourceName, string filterName) => BuildObsRequest("GetSourceFilterSettings", new { sourceName, filterName });

        /// <summary>
        /// Renames a source filter.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to be renamed.</paramref>
        /// <paramref name="newFilterName">New name for the filter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefiltername
        /// </summary>
        public JObject ObsRawSetSourceFilterName(string sourceName, string filterName, string newFilterName) => BuildObsRequest("SetSourceFilterName", new { sourceName, filterName, newFilterName });

        /// <summary>
        /// Sets the index of a source filter.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to set the index for.</paramref>
        /// <paramref name="filterIndex">New index for the filter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefilterindex
        /// </summary>
        public JObject ObsRawSetSourceFilterIndex(string sourceName, string filterName, int filterIndex) => BuildObsRequest("SetSourceFilterIndex", new { sourceName, filterName, filterIndex });

        /// <summary>
        /// Sets the settings for a source filter.
        /// <paramref name="sourceName">Name of the source to set the filter settings for.</paramref>
        /// <paramref name="filterName">Name of the filter to set the settings for.</paramref>
        /// <paramref name="filterSettings">Object containing the new filter settings.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefiltersettings
        /// </summary>
        public JObject ObsRawSetSourceFilterSettings(string sourceName, string filterName, object filterSettings, bool overlay = false) => BuildObsRequest("SetSourceFilterSettings", new { sourceName, filterName, filterSettings, overlay });

        /// <summary>
        /// Removes a source filter from a source.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=removesourcefilter
        /// </summary>
        public JObject ObsRawRemoveSourceFilter(string sourceName, string filterName) => BuildObsRequest("RemoveSourceFilter", new { sourceName, filterName });

        /// <summary>
        /// Creates a new source filter and adds it to a source.
        /// <paramref name="sourceName">Name of the source to add the filter to.</paramref>
        /// <paramref name="filterName">Name for the new filter.</paramref>
        /// <paramref name="filterKind">The kind of filter to create.</paramref>
        /// <paramref name="filterSettings">Object containing settings for the filter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=createsourcefilter
        /// </summary>
        public JObject ObsRawCreateSourceFilter(string sourceName, string filterName, string filterKind, object filterSettings = null) => BuildObsRequest("CreateSourceFilter", new { sourceName, filterName, filterKind, filterSettings });

        /// <summary>
        /// Gets a list of all source filter kinds available in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilterkindlist
        /// </summary>
        public JObject ObsRawGetSourceFilterKindList() => BuildObsRequest("GetSourceFilterKindList");

        // --- Scene Items Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets an array of all scene items in a scene.
        /// <paramref name="sceneName">Name of the scene to get the items from.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsceneitemlist
        /// </summary>
        public JObject ObsRawGetSceneItemList(string sceneName) => BuildObsRequest("GetSceneItemList", new { sceneName });

        /// <summary>
        /// Gets an array of all scene items in a group.
        /// <paramref name="sceneName">Name of the group to get the items from.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getgroupsceneitemlist
        /// </summary>
        public JObject ObsRawGetGroupSceneItemList(string sceneName) => BuildObsRequest("GetGroupSceneItemList", new { sceneName });

        /// <summary>
        /// Gets the ID of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sourceName">Name of the source (input or scene) the item refers to.</paramref>
        /// <paramref name="searchOffset">Optional offset for search (used for duplicate items).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsceneitemid
        /// </summary>
        public JObject ObsRawGetSceneItemId(string sceneName, string sourceName, int? searchOffset = null) => BuildObsRequest("GetSceneItemId", new { sceneName, sourceName, searchOffset });

        /// <summary>
        /// Creates a new scene item in a scene.
        /// <paramref name="sceneName">Name of the scene to add the item to.</paramref>
        /// <paramref name="sceneItem">Object containing the scene item data.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=createsceneitem
        /// </summary>
        public JObject ObsRawCreateSceneItem(string sceneName, object sceneItem) => BuildObsRequest("CreateSceneItem", new { sceneName, sceneItem });

        /// <summary>
        /// Removes a scene item from a scene.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=removesceneitem
        /// </summary>
        public JObject ObsRawRemoveSceneItem(string sceneName, int sceneItemId) => BuildObsRequest("RemoveSceneItem", new { sceneName, sceneItemId });

        /// <summary>
        /// Duplicates a scene item to another scene.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to duplicate.</paramref>
        /// <paramref name="destinationSceneName">Optional name of the destination scene. If not specified, duplicates within the same scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=duplicatesceneitem
        /// </summary>
        public JObject ObsRawDuplicateSceneItem(string sceneName, int sceneItemId, string destinationSceneName = null) => BuildObsRequest("DuplicateSceneItem", new { sceneName, sceneItemId, destinationSceneName });

        /// <summary>
        /// Gets the transform data of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the transform data for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemtransform
        /// </summary>
        public JObject ObsRawGetSceneItemTransform(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemTransform", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the transform data of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the transform data for.</paramref>
        /// <paramref name="sceneItemTransform">Object containing the new transform data.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemtransform
        /// </summary>
        public JObject ObsRawSetSceneItemTransform(string sceneName, int sceneItemId, object sceneItemTransform) => BuildObsRequest("SetSceneItemTransform", new { sceneName, sceneItemId, sceneItemTransform });

        /// <summary>
        /// Gets the enabled state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the enabled state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemenabled
        /// </summary>
        public JObject ObsRawGetSceneItemEnabled(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemEnabled", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the enabled state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the enabled state for.</paramref>
        /// <paramref name="sceneItemEnabled">Whether the scene item should be enabled.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemenabled
        /// </summary>
        public JObject ObsRawSetSceneItemEnabled(string sceneName, int sceneItemId, bool sceneItemEnabled) => BuildObsRequest("SetSceneItemEnabled", new { sceneName, sceneItemId, sceneItemEnabled });

        /// <summary>
        /// Gets the locked state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the locked state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemlocked
        /// </summary>
        public JObject ObsRawGetSceneItemLocked(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemLocked", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the locked state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the locked state for.</paramref>
        /// <paramref name="sceneItemLocked">Whether the scene item should be locked.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemlocked
        /// </summary>
        public JObject ObsRawSetSceneItemLocked(string sceneName, int sceneItemId, bool sceneItemLocked) => BuildObsRequest("SetSceneItemLocked", new { sceneName, sceneItemId, sceneItemLocked });

        /// <summary>
        /// Gets the index of a scene item in the scene item list.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the index of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemindex
        /// </summary>
        public JObject ObsRawGetSceneItemIndex(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemIndex", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the index of a scene item in the scene item list.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the index for.</paramref>
        /// <paramref name="sceneItemIndex">New index for the scene item.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemindex
        /// </summary>
        public JObject ObsRawSetSceneItemIndex(string sceneName, int sceneItemId, int sceneItemIndex) => BuildObsRequest("SetSceneItemIndex", new { sceneName, sceneItemId, sceneItemIndex });

        /// <summary>
        /// Gets the blend mode of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the blend mode of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemblendmode
        /// </summary>
        public JObject ObsRawGetSceneItemBlendMode(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemBlendMode", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the blend mode of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the blend mode for.</paramref>
        /// <paramref name="sceneItemBlendMode">New blend mode for the scene item.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemblendmode
        /// </summary>
        public JObject ObsRawSetSceneItemBlendMode(string sceneName, int sceneItemId, string sceneItemBlendMode) => BuildObsRequest("SetSceneItemBlendMode", new { sceneName, sceneItemId, sceneItemBlendMode });

        // --- Outputs Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the status of the virtual camera.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getvirtualcamstatus
        /// </summary>
        public JObject ObsRawGetVirtualCamStatus() => BuildObsRequest("GetVirtualCamStatus");

        /// <summary>
        /// Toggles the virtual camera on or off.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglevirtualcam
        /// </summary>
        public JObject ObsRawToggleVirtualCam() => BuildObsRequest("ToggleVirtualCam");

        /// <summary>
        /// Starts the virtual camera.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startvirtualcam
        /// </summary>
        public JObject ObsRawStartVirtualCam() => BuildObsRequest("StartVirtualCam");

        /// <summary>
        /// Stops the virtual camera.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopvirtualcam
        /// </summary>
        public JObject ObsRawStopVirtualCam() => BuildObsRequest("StopVirtualCam");

        /// <summary>
        /// Gets the status of the replay buffer.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getreplaybufferstatus
        /// </summary>
        public JObject ObsRawGetReplayBufferStatus() => BuildObsRequest("GetReplayBufferStatus");

        /// <summary>
        /// Toggles the replay buffer on or off.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglereplaybuffer
        /// </summary>
        public JObject ObsRawToggleReplayBuffer() => BuildObsRequest("ToggleReplayBuffer");

        /// <summary>
        /// Starts the replay buffer.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startreplaybuffer
        /// </summary>
        public JObject ObsRawStartReplayBuffer() => BuildObsRequest("StartReplayBuffer");

        /// <summary>
        /// Stops the replay buffer.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopreplaybuffer
        /// </summary>
        public JObject ObsRawStopReplayBuffer() => BuildObsRequest("StopReplayBuffer");

        /// <summary>
        /// Saves the current replay buffer to a file.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=savereplaybuffer
        /// </summary>
        public JObject ObsRawSaveReplayBuffer() => BuildObsRequest("SaveReplayBuffer");

        /// <summary>
        /// Gets the last replay buffer replay.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getlastreplaybufferreplay
        /// </summary>
        public JObject ObsRawGetLastReplayBufferReplay() => BuildObsRequest("GetLastReplayBufferReplay");

        /// <summary>
        /// Gets a list of all outputs in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getoutputlist
        /// </summary>
        public JObject ObsRawGetOutputList() => BuildObsRequest("GetOutputList");

        /// <summary>
        /// Gets the status of an output.
        /// <paramref name="outputName">Name of the output to get the status of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getoutputstatus
        /// </summary>
        public JObject ObsRawGetOutputStatus(string outputName) => BuildObsRequest("GetOutputStatus", new { outputName });

        /// <summary>
        /// Toggles an output on or off.
        /// <paramref name="outputName">Name of the output to toggle.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#toggleoutput
        /// </summary>
        public JObject ObsRawToggleOutput(string outputName) => BuildObsRequest("ToggleOutput", new { outputName });

        /// <summary>
        /// Starts an output.
        /// <paramref name="outputName">Name of the output to start.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startoutput
        /// </summary>
        public JObject ObsRawStartOutput(string outputName) => BuildObsRequest("StartOutput", new { outputName });

        /// <summary>
        /// Stops an output.
        /// <paramref name="outputName">Name of the output to stop.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopoutput
        /// </summary>
        public JObject ObsRawStopOutput(string outputName) => BuildObsRequest("StopOutput", new { outputName });

        /// <summary>
        /// Gets the settings for an output.
        /// <paramref name="outputName">Name of the output to get the settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getoutputsettings
        /// </summary>
        public JObject ObsRawGetOutputSettings(string outputName) => BuildObsRequest("GetOutputSettings", new { outputName });

        /// <summary>
        /// Sets the settings for an output.
        /// <paramref name="outputName">Name of the output to set the settings for.</paramref>
        /// <paramref name="outputSettings">Object containing the new settings for the output.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setoutputsettings
        /// </summary>
        public JObject ObsRawSetOutputSettings(string outputName, object outputSettings, bool overlay = false) => BuildObsRequest("SetOutputSettings", new { outputName, outputSettings, overlay });

        // --- Stream Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the current stream status.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstreamstatus
        /// </summary>
        public JObject ObsRawGetStreamStatus() => BuildObsRequest("GetStreamStatus");

        /// <summary>
        /// Toggles the stream on or off.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglestream
        /// </summary>
        public JObject ObsRawToggleStream() => BuildObsRequest("ToggleStream");

        /// <summary>
        /// Starts the stream.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startstream
        /// </summary>
        public JObject ObsRawStartStream() => BuildObsRequest("StartStream");

        /// <summary>
        /// Stops the stream.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stopstream
        /// </summary>
        public JObject ObsRawStopStream() => BuildObsRequest("StopStream");

        /// <summary>
        /// Sends a caption to the stream.
        /// <paramref name="captionText">Text of the caption to send.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#sendstreamcaption
        /// </summary>
        public JObject ObsRawSendStreamCaption(string captionText) => BuildObsRequest("SendStreamCaption", new { captionText });

        // --- Record Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the current record status.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getrecordstatus
        /// </summary>
        public JObject ObsRawGetRecordStatus() => BuildObsRequest("GetRecordStatus");

        /// <summary>
        /// Toggles the record on or off.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglerecord
        /// </summary>
        public JObject ObsRawToggleRecord() => BuildObsRequest("ToggleRecord");

        /// <summary>
        /// Starts recording.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#startrecord
        /// </summary>
        public JObject ObsRawStartRecord() => BuildObsRequest("StartRecord");

        /// <summary>
        /// Stops recording.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#stoprecord
        /// </summary>
        public JObject ObsRawStopRecord() => BuildObsRequest("StopRecord");

        /// <summary>
        /// Toggles the record pause state.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#togglerecordpause
        /// </summary>
        public JObject ObsRawToggleRecordPause() => BuildObsRequest("ToggleRecordPause");

        /// <summary>
        /// Pauses the recording.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#pauserecord
        /// </summary>
        public JObject ObsRawPauseRecord() => BuildObsRequest("PauseRecord");

        /// <summary>
        /// Resumes the recording.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#resumerecord
        /// </summary>
        public JObject ObsRawResumeRecord() => BuildObsRequest("ResumeRecord");

        /// <summary>
        /// Splits the current record file.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#splitrecordfile
        /// </summary>
        public JObject ObsRawSplitRecordFile() => BuildObsRequest("SplitRecordFile");

        /// <summary>
        /// Creates a new chapter in the record.
        /// <paramref name="chapterTitle">Optional title for the chapter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createrecordchapter
        /// </summary>
        public JObject ObsRawCreateRecordChapter(string chapterTitle = null) => BuildObsRequest("CreateRecordChapter", new { chapterTitle });

        // --- Media Inputs Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the status of a media input.
        /// <paramref name="inputName">Name of the media input to get the status of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getmediainputstatus
        /// </summary>
        public JObject ObsRawGetMediaInputStatus(string inputName) => BuildObsRequest("GetMediaInputStatus", new { inputName });

        /// <summary>
        /// Sets the cursor position of a media input.
        /// <paramref name="inputName">Name of the media input to set the cursor for.</paramref>
        /// <paramref name="mediaCursor">New cursor position in milliseconds.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setmediainputcursor
        /// </summary>
        public JObject ObsRawSetMediaInputCursor(string inputName, int mediaCursor) => BuildObsRequest("SetMediaInputCursor", new { inputName, mediaCursor });

        /// <summary>
        /// Offsets the cursor position of a media input.
        /// <paramref name="inputName">Name of the media input to offset the cursor for.</paramref>
        /// <paramref name="mediaCursorOffset">Offset value in milliseconds.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=offsetmediainputcursor
        /// </summary>
        public JObject ObsRawOffsetMediaInputCursor(string inputName, int mediaCursorOffset) => BuildObsRequest("OffsetMediaInputCursor", new { inputName, mediaCursorOffset });

        /// <summary>
        /// Triggers an action for a media input.
        /// <paramref name="inputName">Name of the media input to trigger the action for.</paramref>
        /// <paramref name="mediaAction">The action to trigger.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=triggermediainputaction
        /// </summary>
        public JObject ObsRawTriggerMediaInputAction(string inputName, string mediaAction) => BuildObsRequest("TriggerMediaInputAction", new { inputName, mediaAction });

        // --- Ui Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the enabled state of studio mode.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getstudiosmodeenabled
        /// </summary>
        public JObject ObsRawGetStudioModeEnabled() => BuildObsRequest("GetStudioModeEnabled");

        /// <summary>
        /// Sets the enabled state of studio mode.
        /// <paramref name="studioModeEnabled">Whether to enable studio mode.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setstudiosmodeenabled
        /// </summary>
        public JObject ObsRawSetStudioModeEnabled(bool studioModeEnabled) => BuildObsRequest("SetStudioModeEnabled", new { studioModeEnabled });

        /// <summary>
        /// Opens the input properties dialog for an input.
        /// <paramref name="inputName">Name of the input to open the properties dialog for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=openinputpropertiesdialog
        /// </summary>
        public JObject ObsRawOpenInputPropertiesDialog(string inputName) => BuildObsRequest("OpenInputPropertiesDialog", new { inputName });

        /// <summary>
        /// Opens the filters dialog for an input.
        /// <paramref name="inputName">Name of the input to open the filters dialog for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=openinputfiltersdialog
        /// </summary>
        public JObject ObsRawOpenInputFiltersDialog(string inputName) => BuildObsRequest("OpenInputFiltersDialog", new { inputName });

        /// <summary>
        /// Opens the interact dialog for an input.
        /// <paramref name="inputName">Name of the input to open the interact dialog for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=openinputinteractdialog
        /// </summary>
        public JObject ObsRawOpenInputInteractDialog(string inputName) => BuildObsRequest("OpenInputInteractDialog", new { inputName });

        /// <summary>
        /// Gets a list of all monitors.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getmonitorlist
        /// </summary>
        public JObject ObsRawGetMonitorList() => BuildObsRequest("GetMonitorList");

        /// <summary>
        /// Opens a video mix projector.
        /// <paramref name="videoMixType">Type of the video mix (e.g. "preview", "program").</paramref>
        /// <paramref name="monitorIndex">Index of the monitor to use for the projector.</paramref>
        /// <paramref name="projectorGeometryWidth">Width of the projector geometry.</paramref>
        /// <paramref name="projectorGeometryHeight">Height of the projector geometry.</paramref>
        /// <paramref name="projectorGeometryX">X position of the projector geometry.</paramref>
        /// <paramref name="projectorGeometryY">Y position of the projector geometry.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=openvideomixprojector
        /// </summary>
        public JObject ObsRawOpenVideoMixProjector(string videoMixType, int monitorIndex = 0, int projectorGeometryWidth = 0, int projectorGeometryHeight = 0, int projectorGeometryX = 0, int projectorGeometryY = 0) => BuildObsRequest("OpenVideoMixProjector", new { videoMixType, monitorIndex, projectorGeometryWidth, projectorGeometryHeight, projectorGeometryX, projectorGeometryY });

        /// <summary>
        /// Opens a source projector.
        /// <paramref name="sourceName">Name of the source to open the projector for.</paramref>
        /// <paramref name="monitorIndex">Index of the monitor to use for the projector.</paramref>
        /// <paramref name="projectorGeometryWidth">Width of the projector geometry.</paramref>
        /// <paramref name="projectorGeometryHeight">Height of the projector geometry.</paramref>
        /// <paramref name="projectorGeometryX">X position of the projector geometry.</paramref>
        /// <paramref name="projectorGeometryY">Y position of the projector geometry.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=opensourceprojector
        /// </summary>
        public JObject ObsRawOpenSourceProjector(string sourceName, int monitorIndex = 0, int projectorGeometryWidth = 0, int projectorGeometryHeight = 0, int projectorGeometryX = 0, int projectorGeometryY = 0) => BuildObsRequest("OpenSourceProjector", new { sourceName, monitorIndex, projectorGeometryWidth, projectorGeometryHeight, projectorGeometryX, projectorGeometryY });
    }
}