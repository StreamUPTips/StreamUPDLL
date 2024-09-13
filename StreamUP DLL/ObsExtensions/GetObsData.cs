using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool GetObsVideoSettings(int obsConnection, out JObject videoSettings)
        {
            LogInfo("Requesting OBS video settings");

            // Get OBS video settings
            string response = _CPH.ObsSendRaw("GetVideoSettings", "{}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                videoSettings = null;
                return false;
            }

            // Parse response
            videoSettings = JObject.Parse(response);

            LogInfo("Successfully retrieved OBS video settings");
            return true;
        }

        public bool GetObsCurrentSource(int obsConnection, out string sourceName)
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

        public bool GetObsOutputFilePath(int obsConnection, out string filePath)
        {
            LogInfo($"Requesting OBS filepath");

            // Get OBS filepath
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getOutputFilePath\",\"requestData\":null}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                filePath = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);

            // Check if responseData and outputFilePath exist
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                filePath = null;
                return false;
            }

            if (responseObj["responseData"]["outputFilePath"] == null)
            {
                LogError("outputFilePath not found in the responseData");
                filePath = null;
                return false;
            }


            filePath = responseObj["responseData"]["outputFilePath"].ToString();
            LogInfo("Successfully retrieved source filter list");
            return true;
        }

        public bool GetObsSourceSettings(string sourceName, int obsConnection, out JObject sourceSettings)
        {
            LogInfo($"Requesting input settings for source [{sourceName}]");

            string response = _CPH.ObsSendRaw("GetInputSettings", "{\"inputName\":\""+sourceName+"\"}", obsConnection);
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

        public bool GetObsSceneItemList(string sceneName, int obsConnection, out JArray sceneItemList)
        {
            LogInfo($"Requesting scene item list for scene [{sceneName}]");

            string response = _CPH.ObsSendRaw("GetSceneItemList", $"{{\"sceneName\":\"{sceneName}\"}}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sceneItemList = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["sceneItems"] == null)
            {
                LogError("sceneItems not found in the response");
                sceneItemList = null;
                return false;
            }

            sceneItemList = (JArray)responseObj["sceneItems"];
            LogInfo("Successfully retrieved scene item list");
            return true;
        }

        public bool GetObsSourceShowTransition(string sceneName, string sourceName, int obsConnection, out JObject showTransition)
        {
            LogInfo($"Requesting show transition for source [{sourceName}] on scene [{sceneName}]");

            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getShowTransition\",\"requestData\":{\"sceneName\":\""+sceneName+"\",\"sourceName\":\""+sourceName+"\"}}", obsConnection);
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

        public bool GetObsSourceHideTransition(string sceneName, string sourceName, int obsConnection, out JObject hideTransition)
        {
            LogInfo($"Requesting hide transition for source [{sourceName}] on scene [{sceneName}]");

            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getHideTransition\",\"requestData\":{\"sceneName\":\""+sceneName+"\",\"sourceName\":\""+sourceName+"\"}}", obsConnection);
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

        public bool GetObsCanvasScaleFactor(int obsConnection, out double scaleFactor)
        {
            LogInfo($"Requesting OBS canvas scale factor");

            // Get video settings
            if (!GetObsVideoSettings(obsConnection, out JObject videoSettings))
            {
                LogError("Unable to retrieve videoSettings");
                scaleFactor = 0.0;
                return false;
            }

            // Check if baseWidth exists
            if (videoSettings["baseWidth"] == null)
            {
                LogError("baseWidth not found in the response");
                scaleFactor = 0.0;
                return false;
            }

            // Extract canvas width
            double canvasWidth = (double)videoSettings["baseWidth"];

            // Work out scale difference based on 1920x1080
            scaleFactor = canvasWidth / 1920;
            LogInfo($"Successfully retrieved scale factor [{scaleFactor}]");
            return true;
        }

        public bool GetObsCurrentDSKScene(string dskName, int obsConnection, out string sceneName)
        {
            LogInfo($"Requesting OBS current DSK scene on DSK [{dskName}]");

            // Get current DSK scene
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"downstream-keyer\",\"requestType\":\"get_downstream_keyer\",\"requestData\":{\"dsk_name\":\""+dskName+"\"}}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sceneName = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                sceneName = null;
                return false;
            }

            if (responseObj["responseData"]["scene"] == null)
            {
                LogError("scene not found in responseData");
                sceneName = null;
                return false;
            }

            sceneName = responseObj["responseData"]["scene"].ToString();
            LogInfo($"Successfully current DSK scene [{sceneName}]");
            return true;
        }

        public bool GetObsSourceVisibility(string parentSource, OBSSceneType parentSourceType, string childSource, int obsConnection, out bool? sourceState)
        {
            LogInfo($"Requesting if source [{childSource}] on parent [{parentSourceType}] - [{parentSource}] is enabled");

            // Get scene item ID
            if (!GetObsSceneItemId(parentSource, parentSourceType, childSource, obsConnection, out int sceneItemId))
            {
                LogError("Unable to retrieve sceneItemId");
                sourceState = null;
                return false;
            }

            // Get visibility state
            string response = _CPH.ObsSendRaw("GetSceneItemEnabled", "{\"sceneName\":\""+parentSource+"\",\"sceneItemId\":"+sceneItemId+"}", obsConnection);
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

        public bool GetObsSceneItemId(string parentSource, OBSSceneType parentSourceType, string childSource, int obsConnection, out int sceneItemId)
        {
            LogInfo($"Requesting scene item id for source [{childSource}] on parent [{parentSourceType}] - [{parentSource}]");

            // Get sceneItemLists (Group or Scene)
            string response = "";
            switch (parentSourceType) {
                case OBSSceneType.Scene:
                    response = _CPH.ObsSendRaw("GetSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsConnection);
                    break;
                case OBSSceneType.Group:
                    response = _CPH.ObsSendRaw("GetGroupSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsConnection);
                    break;
                default:
                    LogError($"Unable to get sceneItemList, parentSourceType is incorrectly set [{parentSourceType}]");
                    sceneItemId = -1;
                    return false;
            }
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sceneItemId = -1;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["sceneItems"] == null)
            {
                LogError("sceneItems not found in the response");
                sceneItemId = -1;
                return false;
            }

            // Parse sceneItems array
            JArray sceneItems = (JArray)responseObj["sceneItems"];
            if (sceneItems == null || sceneItems.Count == 0) {
                LogError("No sceneItems found in the response");
                sceneItemId = -1;
                return false;
            } 

            // Search for sceneItemId
            if (!FindObsSceneItemIdBySourceName(sceneItems, childSource, out sceneItemId))
            {
                LogError("Unable to find sceneItemId");
                sceneItemId = -1;
                return false;
            }

            LogInfo($"Successfully retrieved source state");
            return true;
        }

        public bool FindObsSceneItemIdBySourceName(JArray sceneItems, string sourceToFind, out int sceneItemId)
        {
            LogInfo($"Requesting sceneItemId from list of sceneItems");

            foreach (var item in sceneItems) {
                string currentItemName = item["sourceName"].ToString();
                if (currentItemName == sourceToFind) {
                    sceneItemId = int.Parse(item["sceneItemId"].ToString());
                    LogInfo($"Successfully retrieved sceneItemId");
                    return true;
                }
            }

            LogError($"Unable to find sceneItemId from list of sceneItems");
            sceneItemId = -1;
            return false;
        }





    }
}
