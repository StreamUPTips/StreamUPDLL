using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
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

        /// <summary>
        /// Gets the deinterlace mode of an input (output).
        /// <paramref name="inputName">Name of the input to get the deinterlace mode for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputdeinterlacemode
        /// </summary>
        public JObject ObsRawGetInputDeinterlaceMode(string inputName) => BuildObsRequest("GetInputDeinterlaceMode", new { inputName });

        /// <summary>
        /// Sets the deinterlace mode of an input (output).
        /// <paramref name="inputName">Name of the input to set the deinterlace mode for.</paramref>
        /// <paramref name="deinterlaceMode">The deinterlace mode to set.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputdeinterlacemode
        /// </summary>
        public JObject ObsRawSetInputDeinterlaceMode(string inputName, string deinterlaceMode) => BuildObsRequest("SetInputDeinterlaceMode", new { inputName, deinterlaceMode });

        /// <summary>
        /// Gets the deinterlace field order of an input (output).
        /// <paramref name="inputName">Name of the input to get the deinterlace field order for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getinputdeinterlacefieldorder
        /// </summary>
        public JObject ObsRawGetInputDeinterlaceFieldOrder(string inputName) => BuildObsRequest("GetInputDeinterlaceFieldOrder", new { inputName });

        /// <summary>
        /// Sets the deinterlace field order of an input (output).
        /// <paramref name="inputName">Name of the input to set the deinterlace field order for.</paramref>
        /// <paramref name="fieldOrder">The field order to set.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setinputdeinterlacefieldorder
        /// </summary>
        public JObject ObsRawSetInputDeinterlaceFieldOrder(string inputName, string fieldOrder) => BuildObsRequest("SetInputDeinterlaceFieldOrder", new { inputName, fieldOrder });
    }
}
