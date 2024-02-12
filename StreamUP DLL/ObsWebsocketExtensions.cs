using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {
    public static class ObsWebsocketExtensions {

        // GET VIDEO SETTINGS
        #region 
        public static JObject SUObsGetVideoSettings(this IInlineInvokeProxy CPH, int obsInstance) {
            string jsonResponse = CPH.ObsSendRaw("GetVideoSettings", "{}", obsInstance);
            JObject obsResponse = JObject.Parse(jsonResponse);
            return obsResponse;
        }
        #endregion

        // PULL SCENE ITEM TRANSFORM
        #region
        public static JObject SUObsPullSceneItemTransform(this IInlineInvokeProxy CPH, string productName, int obsInstance, int parentSourceType, string parentSource, string childSource) {
            string logName = $"{productName}-ObsPullSceneItemTransform";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemID
            CPH.SUWriteLog($"Pulling scene item ID for {parentSource}", logName);
            int sceneItemId = CPH.SUObsPullSceneItemId(productName, obsInstance, parentSourceType, parentSource, childSource);
            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found", logName);
                return null;
            }

            // Initialise a variable to store the JSON response
            string jsonResponse = "";
            // Extract the transformation data of the source
            CPH.SUWriteLog($"Sending request to get scene item transform for (sceneItemId: {sceneItemId}) on (parentSource: {parentSource})", logName);
            jsonResponse = CPH.ObsSendRaw("GetSceneItemTransform", "{\"sceneName\":\"" + parentSource + "\",\"sceneItemId\":" + sceneItemId + "}", obsInstance);
            // Parse the JSON response
            var json = JObject.Parse(jsonResponse);
            var transform = json["sceneItemTransform"] as JObject;
            // Check if the transform data is not null
            if (transform == null) {
                CPH.SUWriteLog($"No transform data found in jsonResponse", logName);
                return null;
            }
            CPH.SUWriteLog($"{transform.ToString()}", logName);
            return transform;
        }
        #endregion

        // PULL SCENE ITEM ID
        #region 
        public static int SUObsPullSceneItemId(this IInlineInvokeProxy CPH, string productName, int obsInstance, int parentSourceType, string parentSource, string childSource) {
            string logName = $"{productName}-ObsPullSceneItemId";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemLists (Group or Scene)
            string jsonResponse = "";
            switch (parentSourceType) {
                case 0:
                    jsonResponse = CPH.ObsSendRaw("GetSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsInstance);
                    break;
                case 1:
                    jsonResponse = CPH.ObsSendRaw("GetGroupSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsInstance);
                    break;
                default:
                    CPH.SUWriteLog($"parentSourceType is incorrectly set to '{parentSourceType}'. 0=Scene, 1=Group", logName);
                    return -1;
            }
            CPH.SUWriteLog("Received JSON response from OBS", logName);

            // Save and parse jsonResponse
            var json = JObject.Parse(jsonResponse);
            var sceneItems = json["sceneItems"] as JArray;
            if (sceneItems == null || sceneItems.Count == 0) {
                // Log if no scene items are found
                CPH.SUWriteLog("No scene items found in the response", logName);
            } else {
                CPH.SUWriteLog($"Found {sceneItems.Count} scene item(s) in jsonResponse", logName);
            }

            // Pull sceneItemId
            int sceneItemId = CPH.SUFindSceneItemId(sceneItems, childSource);
            CPH.SUWriteLog($"Returning sceneItemId for '{childSource}': {sceneItemId}", logName);

            return sceneItemId;
        }

        private static int SUFindSceneItemId(this IInlineInvokeProxy CPH, JArray sceneItems, string childSource) {
            foreach (var item in sceneItems) {
                string currentItemName = item["sourceName"].ToString();
                if (currentItemName == childSource) {
                    return int.Parse(item["sceneItemId"].ToString());
                }
            }
            return -1;
        }
        #endregion

        // SET SOURCE FILTER SETTINGS
        #region
        public static void SUObsSetSourceFilterSettings(this IInlineInvokeProxy CPH, int obsInstance, string sourceName, string filterName, string filterSettings) {
            CPH.ObsSendRaw("SetSourceFilterSettings", $$"""
{
    "sourceName": "{{sourceName}}",
    "filterName": "{{filterName}}",
    "filterSettings": {{{filterSettings}}},
    "overlay": true
}
""", obsInstance);
        }
        #endregion

        // SET SOURCE (INPUT) SETTINGS
        #region 
        public static void SUObsSetInputSettings(this IInlineInvokeProxy CPH, int obsInstance, string inputName, string inputSettings) {
            CPH.ObsSendRaw("SetInputSettings", $$"""
{
    "inputName": "{{inputName}}",
    "inputSettings": {{{inputSettings}}},
    "overlay": true
}
""", obsInstance);
        }
        #endregion

        // SET SCENE TRANSITION FOR SCENE
        #region 
        public static void SUObsSetSceneSceneTransitionOverride(this IInlineInvokeProxy CPH, int obsInstance, string sceneName, string transitionName, int transitionDuration) {
            CPH.ObsSendRaw("SetSceneSceneTransitionOverride", $$"""
{
    "sceneName": "{{sceneName}}",
    "transitionName": {{transitionName}},
    "transitionDuration": {{transitionDuration}}
}
""", obsInstance);
        }
        #endregion

        // GET SOURCE FILTER
        #region 
        public static JObject SUObsGetSourceFilter(this IInlineInvokeProxy CPH, string productName, int obsInstance, string sourceName, string filterName) {
            string logName = $"{productName}-ObsGetSourceFilter";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetSourceFilter", "{\"sourceName\":\"" + sourceName + "\",\"filterName\":\"" + filterName + "\"}", obsInstance);
            var obsResponse = JObject.Parse(jsonResponse);
            // Check if the data is null
            if (obsResponse == null) {
                CPH.SUWriteLog($"No data found in obsResponse", logName);
                return null;
            }
            return obsResponse;
        }
        #endregion
    }
}
