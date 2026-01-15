using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - INPUTS (Sources)
        // Methods for managing inputs/sources
        // ============================================================

        #region Input List

        /// <summary>
        /// Gets a list of all inputs (sources) in OBS.
        /// </summary>
        /// <param name="inputKind">Optional filter by input kind (e.g., "browser_source")</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of input objects, or null if failed</returns>
        public JArray ObsGetInputList(string inputKind = null, int connection = 0)
        {
            var requestData = inputKind != null ? new { inputKind } : null;
            var response = ObsSendRequest("GetInputList", requestData, connection);
            return response?["inputs"] as JArray;
        }

        /// <summary>
        /// Gets a list of all available input kinds in OBS.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of input kind strings, or null if failed</returns>
        public JArray ObsGetInputKindList(int connection = 0)
        {
            var response = ObsSendRequest("GetInputKindList", null, connection);
            return response?["inputKinds"] as JArray;
        }

        /// <summary>
        /// Gets the special inputs (Desktop Audio, Mic, etc.).
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with desktop1, desktop2, mic1-4 properties, or null if failed</returns>
        public JObject ObsGetSpecialInputs(int connection = 0) =>
            ObsSendRequest("GetSpecialInputs", null, connection);

        #endregion

        #region Input Management

        /// <summary>
        /// Creates a new input and adds it to a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to add the input to</param>
        /// <param name="inputName">Name for the new input</param>
        /// <param name="inputKind">Kind of input to create (e.g., "browser_source", "text_gdiplus_v2")</param>
        /// <param name="inputSettings">Optional settings object for the input</param>
        /// <param name="enabled">Whether the input should be visible initially</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with inputUuid and sceneItemId, or null if failed</returns>
        public JObject ObsCreateInput(
            string sceneName,
            string inputName,
            string inputKind,
            object inputSettings = null,
            bool enabled = true,
            int connection = 0
        ) =>
            ObsSendRequest(
                "CreateInput",
                new
                {
                    sceneName,
                    inputName,
                    inputKind,
                    inputSettings,
                    sceneItemEnabled = enabled
                },
                connection
            );

        /// <summary>
        /// Removes an input from OBS entirely (removes all scene items using this input).
        /// </summary>
        /// <param name="inputName">Name of the input to remove</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRemoveInput(string inputName, int connection = 0) =>
            ObsSendRequestNoResponse("RemoveInput", new { inputName }, connection);

        /// <summary>
        /// Renames an input.
        /// </summary>
        /// <param name="inputName">Current name of the input</param>
        /// <param name="newInputName">New name for the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRenameInput(string inputName, string newInputName, int connection = 0) =>
            ObsSendRequestNoResponse("SetInputName", new { inputName, newInputName }, connection);

        #endregion

        #region Input Settings

        /// <summary>
        /// Gets the settings of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with inputSettings and inputKind, or null if failed</returns>
        public JObject ObsGetInputSettings(string inputName, int connection = 0) =>
            ObsSendRequest("GetInputSettings", new { inputName }, connection);

        /// <summary>
        /// Sets the settings of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="inputSettings">Settings object to apply</param>
        /// <param name="overlay">True to merge with existing settings, false to replace</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputSettings(
            string inputName,
            object inputSettings,
            bool overlay = true,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetInputSettings",
                new
                {
                    inputName,
                    inputSettings,
                    overlay
                },
                connection
            );

        /// <summary>
        /// Gets the default settings for an input kind.
        /// </summary>
        /// <param name="inputKind">Input kind to get defaults for</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Default settings object, or null if failed</returns>
        public JObject ObsGetInputDefaultSettings(string inputKind, int connection = 0)
        {
            var response = ObsSendRequest("GetInputDefaultSettings", new { inputKind }, connection);
            return response?["defaultInputSettings"] as JObject;
        }

        #endregion

        #region Mute

        /// <summary>
        /// Gets the mute state of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if muted, false if unmuted or not found</returns>
        public bool ObsIsInputMuted(string inputName, int connection = 0)
        {
            var response = ObsSendRequest("GetInputMute", new { inputName }, connection);
            return response?["inputMuted"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Gets the mute state of an input (alias for ObsIsInputMuted).
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if muted, false if unmuted or not found</returns>
        public bool ObsGetInputMute(string inputName, int connection = 0) =>
            ObsIsInputMuted(inputName, connection);

        /// <summary>
        /// Sets the mute state of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="muted">True to mute, false to unmute</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputMute(string inputName, bool muted, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SetInputMute",
                new { inputName, inputMuted = muted },
                connection
            );

        /// <summary>
        /// Mutes an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsMuteInput(string inputName, int connection = 0) =>
            ObsSetInputMute(inputName, true, connection);

        /// <summary>
        /// Unmutes an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsUnmuteInput(string inputName, int connection = 0) =>
            ObsSetInputMute(inputName, false, connection);

        /// <summary>
        /// Toggles the mute state of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New mute state (true if now muted), or null if failed</returns>
        public bool? ObsToggleInputMute(string inputName, int connection = 0)
        {
            var response = ObsSendRequest("ToggleInputMute", new { inputName }, connection);
            return response?["inputMuted"]?.Value<bool>();
        }

        #endregion

        #region Volume

        /// <summary>
        /// Gets the volume of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with inputVolumeMul and inputVolumeDb, or null if failed</returns>
        public JObject ObsGetInputVolume(string inputName, int connection = 0) =>
            ObsSendRequest("GetInputVolume", new { inputName }, connection);

        /// <summary>
        /// Gets the volume of an input in decibels.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Volume in dB, or null if failed</returns>
        public double? ObsGetInputVolumeDb(string inputName, int connection = 0)
        {
            var response = ObsGetInputVolume(inputName, connection);
            return response?["inputVolumeDb"]?.Value<double>();
        }

        /// <summary>
        /// Sets the volume of an input in decibels.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="volumeDb">Volume in dB (-100 to 26)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputVolumeDb(string inputName, double volumeDb, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SetInputVolume",
                new { inputName, inputVolumeDb = volumeDb },
                connection
            );

        /// <summary>
        /// Sets the volume of an input as a multiplier.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="volumeMul">Volume multiplier (0 to 20, where 1 = 100%)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputVolumeMul(string inputName, double volumeMul, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SetInputVolume",
                new { inputName, inputVolumeMul = volumeMul },
                connection
            );

        #endregion

        #region Audio Settings

        /// <summary>
        /// Gets the audio balance of an input (0.0 = full left, 1.0 = full right).
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Balance value (0.0-1.0), or null if failed</returns>
        public double? ObsGetInputAudioBalance(string inputName, int connection = 0)
        {
            var response = ObsSendRequest("GetInputAudioBalance", new { inputName }, connection);
            return response?["inputAudioBalance"]?.Value<double>();
        }

        /// <summary>
        /// Sets the audio balance of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="balance">Balance value (0.0 = full left, 0.5 = center, 1.0 = full right)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputAudioBalance(string inputName, double balance, int connection = 0) =>
            ObsSendRequestNoResponse(
                "SetInputAudioBalance",
                new { inputName, inputAudioBalance = balance },
                connection
            );

        /// <summary>
        /// Gets the audio sync offset of an input in milliseconds.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Sync offset in ms, or null if failed</returns>
        public int? ObsGetInputAudioSyncOffset(string inputName, int connection = 0)
        {
            var response = ObsSendRequest("GetInputAudioSyncOffset", new { inputName }, connection);
            return response?["inputAudioSyncOffset"]?.Value<int>();
        }

        /// <summary>
        /// Sets the audio sync offset of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="syncOffset">Sync offset in milliseconds</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputAudioSyncOffset(
            string inputName,
            int syncOffset,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetInputAudioSyncOffset",
                new { inputName, inputAudioSyncOffset = syncOffset },
                connection
            );

        /// <summary>
        /// Gets the audio monitor type of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Monitor type string, or null if failed</returns>
        public string ObsGetInputAudioMonitorType(string inputName, int connection = 0)
        {
            var response = ObsSendRequest(
                "GetInputAudioMonitorType",
                new { inputName },
                connection
            );
            return response?["monitorType"]?.Value<string>();
        }

        /// <summary>
        /// Sets the audio monitor type of an input.
        /// Types: OBS_MONITORING_TYPE_NONE, OBS_MONITORING_TYPE_MONITOR_ONLY, OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="monitorType">Monitor type constant</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputAudioMonitorType(
            string inputName,
            string monitorType,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetInputAudioMonitorType",
                new { inputName, monitorType },
                connection
            );

        /// <summary>
        /// Gets the audio tracks configuration of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with track configuration, or null if failed</returns>
        public JObject ObsGetInputAudioTracks(string inputName, int connection = 0)
        {
            var response = ObsSendRequest("GetInputAudioTracks", new { inputName }, connection);
            return response?["inputAudioTracks"] as JObject;
        }

        /// <summary>
        /// Sets the audio tracks configuration of an input.
        /// </summary>
        /// <param name="inputName">Name of the input</param>
        /// <param name="audioTracks">Track configuration object</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetInputAudioTracks(
            string inputName,
            object audioTracks,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetInputAudioTracks",
                new { inputName, inputAudioTracks = audioTracks },
                connection
            );

        #endregion

        #region Text Sources (GDI+/FreeType2)

        /// <summary>
        /// Sets the text of a text source (GDI+ or FreeType2).
        /// </summary>
        /// <param name="sourceName">Name of the text source</param>
        /// <param name="text">Text to display</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetText(string sourceName, string text, int connection = 0) =>
            ObsSetInputSettings(sourceName, new { text }, true, connection);

        /// <summary>
        /// Gets the text of a text source.
        /// </summary>
        /// <param name="sourceName">Name of the text source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Text content, or null if failed</returns>
        public string ObsGetText(string sourceName, int connection = 0)
        {
            var settings = ObsGetInputSettings(sourceName, connection);
            return settings?["inputSettings"]?["text"]?.Value<string>();
        }

        #endregion

        #region Browser Sources

        /// <summary>
        /// Sets the URL of a browser source.
        /// </summary>
        /// <param name="sourceName">Name of the browser source</param>
        /// <param name="url">URL to load</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetBrowserUrl(string sourceName, string url, int connection = 0) =>
            ObsSetInputSettings(sourceName, new { url }, true, connection);

        /// <summary>
        /// Refreshes a browser source.
        /// </summary>
        /// <param name="sourceName">Name of the browser source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRefreshBrowser(string sourceName, int connection = 0) =>
            ObsSendRequestNoResponse(
                "PressInputPropertiesButton",
                new { inputName = sourceName, propertyName = "refreshnocache" },
                connection
            );

        #endregion

        #region Image Sources

        /// <summary>
        /// Sets the file path of an image source.
        /// </summary>
        /// <param name="sourceName">Name of the image source</param>
        /// <param name="filePath">Path to the image file</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetImageFile(string sourceName, string filePath, int connection = 0) =>
            ObsSetInputSettings(sourceName, new { file = filePath }, true, connection);

        #endregion

        #region Media Sources

        /// <summary>
        /// Sets the file path of a media source.
        /// </summary>
        /// <param name="sourceName">Name of the media source</param>
        /// <param name="filePath">Path to the media file</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetMediaFile(string sourceName, string filePath, int connection = 0) =>
            ObsSetInputSettings(sourceName, new { local_file = filePath }, true, connection);

        #endregion
    }
}
