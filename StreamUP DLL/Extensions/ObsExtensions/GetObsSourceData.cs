using System.Linq;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // GET OBS SOURCE DATA
        // Sources can also be referred to as Scene Items
        public bool GetObsSelectedSource(int obsConnection, out string sourceName) //! Requires StreamUP OBS plugin
        {
            LogInfo("Requesting current OBS source");

            // Get current source from OBS
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getCurrentSource\",\"requestData\":null}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sourceName = null;
                return false;
            }

            // Parse response
            JObject responseObj = JObject.Parse(response);

            // Check if responseData and selectedSource exist
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                sourceName = null;
                return false;
            }

            if (responseObj["responseData"]["selectedSource"] == null)
            {
                LogError("selectedSource not found in the responseData");
                sourceName = null;
                return false;
            }

            sourceName = responseObj["responseData"]["selectedSource"].ToString();
            LogInfo($"Successfully retrieved current source");
            return true;
        }

        public bool GetObsSourceVisibility(string parentSource, OBSSceneType parentSourceType, string childSource, int obsConnection, out bool? sourceState)
        {
            LogInfo($"Requesting if source [{childSource}] on parent [{parentSourceType}] - [{parentSource}] is enabled");

            // Get scene item ID
            if (!GetObsSceneItemId(parentSource, childSource, obsConnection, out int sceneItemId))
            {
                LogError("Unable to retrieve sceneItemId");
                sourceState = null;
                return false;
            }

            // Get visibility state
            string response = _CPH.ObsSendRaw("GetSceneItemEnabled", "{\"sceneName\":\"" + parentSource + "\",\"sceneItemId\":" + sceneItemId + "}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sourceState = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["sceneItemEnabled"] == null)
            {
                LogError("sceneItemEnabled not found in the response");
                sourceState = null;
                return false;
            }

            sourceState = (bool)responseObj["sceneItemEnabled"];
            LogInfo($"Successfully retrieved source state");
            return true;
        }

        public bool GetObsSceneItemId(string sceneName, string sourceName, int obsConnection, out int sceneItemId)
        {
            LogInfo($"Requesting scene item id for source [{sourceName}] on parent [{sceneName}]");

            // Send the request to OBS
            string response = _CPH.ObsSendRaw("GetSceneItemId", "{\"sceneName\":\"" + sceneName + "\",\"sourceName\":\"" + sourceName + "\",\"searchOffset\":0}", obsConnection);

            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sceneItemId = -1;
                return false;
            }

            // Parse the JSON response
            JObject responseObj = JObject.Parse(response);
            if (responseObj["sceneItemId"] == null)
            {
                LogError("sceneItemId not found in the response");
                sceneItemId = -1;
                return false;
            }           
            
            sceneItemId = responseObj.Value<int>("sceneItemId");
            LogInfo($"Successfully retrieved scene item ID: [{sceneItemId}]");
            return true;
        }

        public bool GetObsSourceSettings(string sourceName, int obsConnection, out JObject sourceSettings)
        {
            LogInfo($"Requesting input settings for source [{sourceName}]");

            string response = _CPH.ObsSendRaw("GetInputSettings", "{\"inputName\":\"" + sourceName + "\"}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sourceSettings = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["inputSettings"] == null)
            {
                LogError("sourceSettings not found in the response");
                sourceSettings = null;
                return false;
            }

            sourceSettings = (JObject)responseObj["inputSettings"];
            LogInfo("Successfully retrieved input settings");
            return true;
        }

        public bool GetObsSourceFilterList(string sourceName, int obsConnection, out JArray filterList)
        {
            LogInfo($"Requesting source filter list from source [{sourceName}]");

            // Get source filter list
            string response = _CPH.ObsSendRaw("GetSourceFilterList", $"{{\"sourceName\":\"{sourceName}\"}}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                filterList = null;
                return false;
            }

            // Convert to object
            JObject responseObj = JObject.Parse(response);

            // Parse as array
            filterList = (JArray)responseObj["filters"];
            LogInfo("Successfully retrieved source filter list");
            return true;
        }

        public bool GetObsSourceFilterData(string sourceName, string filterName, int obsConnection, out JObject filterData)
        {
            LogInfo($"Requesting source [{sourceName}] filter data for filter [{filterName}]");

            // Get source filter data
            string response = _CPH.ObsSendRaw("GetSourceFilter", "{\"sourceName\":\"" + sourceName + "\",\"filterName\":\"" + filterName + "\"}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                filterData = null;
                return false;
            }

            // Parse response
            filterData = JObject.Parse(response);
            LogInfo("Successfully retrieved source filter data");
            return true;
        }

        public bool GetObsSourceFilterSettings(string sourceName, string filterName, int obsConnection, out JObject filterSettings)
        {
            LogInfo($"Requesting source [{sourceName}] filter settings for filter [{filterName}]");

            // Get source filter data
            GetObsSourceFilterData(sourceName, filterName, obsConnection, out JObject filterData);

            // Parse response
            if (filterData["filterSettings"] == null)
            {
                LogError("filterSettings not found in the response");
                filterSettings = null;
                return false;
            }
            
            filterSettings = (JObject)filterData["filterSettings"];
            
            LogInfo("Successfully retrieved source filter settings");
            return true;
        }

        public bool GetObsSourceShowTransition(string sceneName, string sourceName, int obsConnection, out JObject showTransition) //! Requires StreamUP OBS plugin
        {
            LogInfo($"Requesting show transition for source [{sourceName}] on scene [{sceneName}]");

            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getShowTransition\",\"requestData\":{\"sceneName\":\"" + sceneName + "\",\"sourceName\":\"" + sourceName + "\"}}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                showTransition = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                showTransition = null;
                return false;
            }

            showTransition = (JObject)responseObj["responseData"];
            LogInfo("Successfully retrieved show transition");
            return true;
        }

        public bool GetObsSourceHideTransition(string sceneName, string sourceName, int obsConnection, out JObject hideTransition) //! Requires StreamUP OBS plugin
        {
            LogInfo($"Requesting hide transition for source [{sourceName}] on scene [{sceneName}]");

            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getHideTransition\",\"requestData\":{\"sceneName\":\"" + sceneName + "\",\"sourceName\":\"" + sourceName + "\"}}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                hideTransition = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                hideTransition = null;
                return false;
            }

            hideTransition = (JObject)responseObj["responseData"];
            LogInfo("Successfully retrieved hide transition");
            return true;
        }

        public bool GetObsSourceTransform(string parentSource, string childSource, int obsConnection, out JObject sourceTransform)
        {
            LogInfo($"Requesting source transform of [{childSource}] on scene/group [{parentSource}]");

            if (!GetObsSceneItemId(parentSource, childSource, obsConnection, out int sceneItemId))
            {
                LogError("Unable to retrieve sceneItemId");
                sourceTransform = null;
                return false;
            }

            // Get scene item transform
            string response = _CPH.ObsSendRaw("GetSceneItemTransform", "{\"sceneName\":\"" + parentSource + "\",\"sceneItemId\":" + sceneItemId + "}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sourceTransform = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["sceneItemTransform"] == null)
            {
                LogError("sceneItemTransform not found in the response");
                sourceTransform = null;
                return false;
            }

            // Ensure all numbers are integers
            var keys = responseObj.Properties().Select(p => p.Name).ToList();
            foreach (var key in keys) {
                if (responseObj[key].Type == JTokenType.Float) {
                    responseObj[key] = (int)responseObj[key];
                }
            }

            sourceTransform = (JObject)responseObj["sceneItemTransform"];
            LogInfo("Successfully retrieved source transform");
            return true;
        }

        public bool GetObsGdiTextSourceText(string sourceName, int obsConnection, out string outputText)
        {
            LogInfo($"Requesting text from source [{sourceName}]");

            if (!GetObsSourceSettings(sourceName, obsConnection, out JObject sourceSettings))
            {
                LogError("Unable to retrieve sourceSettings");
                outputText = null;
                return false;
            }

            if (sourceSettings["text"] == null)
            {
                LogError("text not found in the response");
                outputText = null;
                return false;
            }

            outputText = sourceSettings["text"].ToString();
            LogInfo("Successfully retrieved source transform");
            return true;
        }


    }
}
