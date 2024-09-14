using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Set OBS Scene Data
        public bool SetObsCurrentDSKScene(string sceneName, string dskName, int obsConnection) //! Requires Downstream Keyer OBS plugin
        {
            LogInfo($"Setting OBS current DSK [{dskName}] to scene [{sceneName}]");

            _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"downstream-keyer\",\"requestType\":\"dsk_select_scene\",\"requestData\":{\"dsk_name\":\""+dskName+"\",\"scene\":\""+sceneName+"\"}}", obsConnection);

            LogInfo($"Successfully current DSK scene [{sceneName}]");
            return true;
        }

        public bool SetObsSceneTransitionOverride(string sceneName, string transitionName, int transitionDuration, int obsConnection)
        {
            LogInfo($"Setting scene transition override for scene [{sceneName}] with transition [{transitionName}]");

            // Prepare request data
            JObject requestData = new JObject
            {
                ["sceneName"] = sceneName,
                ["transitionName"] = transitionName,
                ["transitionDuration"] = transitionDuration
            };

            // Send the raw request to OBS
            _CPH.ObsSendRaw("SetSceneSceneTransitionOverride", requestData.ToString(), obsConnection);
            LogInfo($"Successfully set transition override for scene [{sceneName}] with transition [{transitionName}]");
            return true;
        }
    

    }
}

