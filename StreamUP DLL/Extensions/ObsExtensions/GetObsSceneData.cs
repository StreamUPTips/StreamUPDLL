using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // GET OBS SCENE DATA
        public bool GetObsSceneItemsArray(string sceneName, OBSSceneType sceneType, int obsConnection, out JArray sceneItemArray)
        {
            LogInfo($"Requesting scene item list for scene [{sceneType}] [{sceneName}]");

            string response;
            switch (sceneType)
            {
                case OBSSceneType.Scene:
                    response = _CPH.ObsSendRaw("GetSceneItemList", $"{{\"sceneName\":\"{sceneName}\"}}", obsConnection);
                    break;
                case OBSSceneType.Group:
                    response = _CPH.ObsSendRaw("GetGroupSceneItemList", $"{{\"sceneName\":\"{sceneName}\"}}", obsConnection);
                    break;
                default:
                    LogError("OBS sceneType is not set correctly");
                    sceneItemArray = null;
                    return false;
            }
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                sceneItemArray = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);
            if (responseObj["sceneItems"] == null)
            {
                LogError("sceneItems not found in the response");
                sceneItemArray = null;
                return false;
            }

            sceneItemArray = (JArray)responseObj["sceneItems"];
            LogInfo("Successfully retrieved scene items Array");
            return true;
        }

        public bool GetObsSceneItemsNamesList(string sceneName, OBSSceneType sceneType, int obsConnection, out List<string> sceneItemsNamesList)
        {
            sceneItemsNamesList = new List<string>();
            LogInfo($"Requesting scene item names list for [{sceneType}] [{sceneName}]");

            // Get the scene item array for the given scene or group
            if (!GetObsSceneItemsArray(sceneName, sceneType, obsConnection, out JArray sceneItemArray))
            {
                LogError("Unable to retrieve scene items array");
                return false;
            }

            // Process the items
            foreach (JObject item in sceneItemArray)
            {
                bool? isGroup = (bool?)item["isGroup"];
                string sourceName = (string)item["sourceName"];

                if (isGroup.HasValue && isGroup.Value)
                {
                    // If the item is a group, recursively fetch its scene items
                    LogInfo($"Source [{sourceName}] is a group, fetching its scene item names.");
                    if (!GetObsSceneItemsNamesList(sourceName, OBSSceneType.Group, obsConnection, out List<string> groupSceneItemsNames))
                    {
                        LogError($"Unable to retrieve scene items for group [{sourceName}]");
                        return false;
                    }

                    // Add all group items to the main list
                    sceneItemsNamesList.AddRange(groupSceneItemsNames);
                }
                else
                {
                    // If it's not a group, add the source name to the list
                    LogInfo($"Source [{sourceName}] is not a group, adding to scene item names list.");
                    sceneItemsNamesList.Add(sourceName);
                }
            }

            LogInfo($"Successfully retrieved all scene item names for [{sceneName}]");
            return true;
        }

        public bool GetObsCurrentDSKScene(string dskName, int obsConnection, out string sceneName) //! Requires Downstream Keyer OBS plugin
        {
            LogInfo($"Requesting OBS current DSK scene on DSK [{dskName}]");

            // Get current DSK scene
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"downstream-keyer\",\"requestType\":\"get_downstream_keyer\",\"requestData\":{\"dsk_name\":\"" + dskName + "\"}}", obsConnection);
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

        public bool GetObsSceneList(int obsConnection, out JObject sceneList)
        {
            LogInfo($"Requesting scene list from OBS");

            string response = _CPH.ObsSendRaw("GetSceneList", "{}", 0);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("Unable to retrieve sceneList");
                sceneList = null;
                return false;
            }

            sceneList = JObject.Parse(response);
            LogInfo($"Successfully retrieved scene list from OBS");
            return true;
        }

        public bool GetObsSceneExists(string sceneName, int obsConnection)
        {
            LogInfo($"Checking if scene [{sceneName}] exists in OBS");

            // Get the scene list from OBS
            if (!GetObsSceneList(obsConnection, out JObject sceneList))
            {
                LogError("Failed to retrieve the scene list from OBS.");
                return false;
            }

            // Check if the scene list contains the input scene name
            JArray scenes = (JArray)sceneList["scenes"];
            foreach (var scene in scenes)
            {
                string sceneNameInList = scene["sceneName"]?.ToString();
                if (sceneNameInList != null && sceneNameInList.Equals(sceneName))
                {
                    LogInfo($"Scene [{sceneName}] exists in OBS.");
                    return true;
                }
            }

            LogInfo($"Scene [{sceneName}] does not exist in OBS.");
            return false;
        }
    }
}
