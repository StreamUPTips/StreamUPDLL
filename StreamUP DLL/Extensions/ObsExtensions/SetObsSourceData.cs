using System.Globalization;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Set OBS Source Data
        public bool SetObsSourceShowTransition(string sceneName, string sourceName, string transitionType, int transitionDuration, JObject transitionSettings, int obsConnection) //! Requires StreamUP OBS plugin
        {
            LogInfo($"Setting show transition for source [{sourceName}] on scene [{sceneName}]");

            // Prepare request data
            JObject requestData = new JObject
            {
                ["sceneName"] = sceneName,
                ["sourceName"] = sourceName,
                ["transitionType"] = transitionType,
                ["transitionSettings"] = transitionSettings,
                ["transitionDuration"] = transitionDuration
            };

            JObject request = new JObject
            {
                ["vendorName"] = "streamup",
                ["requestType"] = "setShowTransition",
                ["requestData"] = requestData
            };

            _CPH.ObsSendRaw("CallVendorRequest", requestData.ToString(), obsConnection);
            LogInfo($"Set show transition successfully");
            return true;
        }

        public bool SetObsSourceHideTransition(string sceneName, string sourceName, string transitionType, int transitionDuration, JObject transitionSettings, int obsConnection) //! Requires StreamUP OBS plugin
        {
            LogInfo($"Setting hide transition for source [{sourceName}] on scene [{sceneName}]");

            // Prepare request data
            JObject requestData = new JObject
            {
                ["sceneName"] = sceneName,
                ["sourceName"] = sourceName,
                ["transitionType"] = transitionType,
                ["transitionSettings"] = transitionSettings,
                ["transitionDuration"] = transitionDuration
            };

            JObject request = new JObject
            {
                ["vendorName"] = "streamup",
                ["requestType"] = "setHideTransition",
                ["requestData"] = requestData
            };

            _CPH.ObsSendRaw("CallVendorRequest", requestData.ToString(), obsConnection);
            LogInfo($"Set hide transition successfully");
            return true;
        }

        public bool SetObsSourceEnabled(string sceneName, OBSSceneType sceneType, string sourceName, bool visibilityState, int obsConnection)
        {
            LogInfo($"Setting visibility of source [{sourceName}] on [{sceneType}] [{sceneName}] to [{visibilityState}]");

            // Get scene item id
            if (!GetObsSceneItemId(sceneName, sourceName, obsConnection, out int sceneItemId))
            {
                LogError("Unable to retrieve sceneItemId");
                return false;
            }

            _CPH.ObsSendRaw("SetSceneItemEnabled", "{\"sceneName\":\"" + sceneName + "\",\"sceneItemId\":" + sceneItemId + ",\"sceneItemEnabled\":" + visibilityState.ToString().ToLower() + "}", obsConnection);
            LogInfo($"Set visibility successfully");
            return true;
        }

        public bool SetObsSourceVolume(string sourceName, VolumeType volumeType, double volumeLevel, int obsConnection)
        {
            LogInfo($"Setting source volume of [{sourceName}] to [{volumeType}] [{volumeLevel}]");

            switch (volumeType)
            {
                case VolumeType.Db:
                    _CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\"" + sourceName + "\",\"inputVolumeDb\":" + volumeLevel.ToString(CultureInfo.InvariantCulture) + "}", obsConnection);
                    break;
                case VolumeType.Multiplier:
                    _CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\"" + sourceName + "\",\"inputVolumeMul\":" + volumeLevel.ToString(CultureInfo.InvariantCulture) + "}", obsConnection);
                    break;
                default:
                    LogError("Invalid volume type used");
                    return false;
            }

            LogInfo($"Set source volume successfully");
            return true;
        }

        public bool SetObsSourceFilterSettings(string sourceName, string filterName, JObject filterSettings, int obsConnection)
        {
            LogInfo($"Setting filter settings for source [{sourceName}] and filter [{filterName}]");

            // Prepare request data
            JObject requestData = new JObject
            {
                ["sourceName"] = sourceName,
                ["filterName"] = filterName,
                ["filterSettings"] = filterSettings,
                ["overlay"] = true
            };

            // Send the raw request to OBS
            _CPH.ObsSendRaw("SetSourceFilterSettings", requestData.ToString(), obsConnection);
            LogInfo($"Successfully set filter settings for source [{sourceName}] and filter [{filterName}]");
            return true;
        }

        public bool SetObsSourceSettings(string inputName, JObject inputSettings, int obsConnection)
        {
            LogInfo($"Setting input settings for input [{inputName}]");

            // Prepare request data
            JObject requestData = new JObject
            {
                ["inputName"] = inputName,
                ["inputSettings"] = inputSettings,
                ["overlay"] = true
            };

            // Send the raw request to OBS
            _CPH.ObsSendRaw("SetInputSettings", requestData.ToString(), obsConnection);
            LogInfo($"Successfully set input settings for input [{inputName}]");
            return true;
        }

        public bool SetObsSceneItemTransform(string sceneName, string sourceName, JObject transformSettings, int obsConnection)
        {
            LogInfo($"Starting scene item transform for parent source [{sceneName}] and child source [{sourceName}]");

            // Pull sceneItemId
            if (!GetObsSceneItemId(sceneName, sourceName, obsConnection, out int sceneItemId))
            {
                LogError($"Scene item ID for source [{sourceName}] in scene [{sceneName}] not found.");
                return false;
            }

            // Prepare the request data
            JObject requestData = new JObject
            {
                ["sceneName"] = sceneName,
                ["sceneItemId"] = sceneItemId,
                ["sceneItemTransform"] = transformSettings
            };

            // Send the raw request to OBS
            _CPH.ObsSendRaw("SetSceneItemTransform", requestData.ToString(), obsConnection);
            LogInfo($"Successfully set scene item transform for scene item ID [{sceneItemId}] in scene [{sceneName}]");
            return true;
        }    
    

    }
}

