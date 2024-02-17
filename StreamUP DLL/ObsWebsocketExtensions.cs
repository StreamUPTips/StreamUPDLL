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
        public static JObject SUObsGetVideoSettings(this IInlineInvokeProxy CPH, string productName, int obsInstance) {
            // Load log string
            string logName = $"{productName}-SUObsGetVideoSettings";
            CPH.SUWriteLog("Method Started", logName);

            // Pull obs video settings
            string jsonResponse = CPH.ObsSendRaw("GetVideoSettings", "{}", obsInstance);
            if (jsonResponse == null) {
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog($"Method complete", logName);
                return null;
            }

            // Parse as JObject and return
            JObject obsResponse = JObject.Parse(jsonResponse);
            CPH.SUWriteLog($"Returning obsResponse: {obsResponse.ToString()}", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return obsResponse;
        }

        // PULL SCENE ITEM TRANSFORM
        public static JObject SUObsPullSceneItemTransform(this IInlineInvokeProxy CPH, string productName, int obsInstance, int parentSourceType, string parentSource, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsPullSceneItemTransform";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemId
            CPH.SUWriteLog($"Pulling scene item ID for parentSource: [{parentSource}]", logName);
            int sceneItemId = CPH.SUObsPullSceneItemId(productName, obsInstance, parentSourceType, parentSource, childSource);
            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog($"Method complete", logName);
                return null;
            }

            // Extract the transformation data of the source
            CPH.SUWriteLog($"Sending request to get scene item transform for (sceneItemId: [{sceneItemId}]) on (parentSource: [{parentSource}])", logName);
            string jsonResponse = CPH.ObsSendRaw("GetSceneItemTransform", "{\"sceneName\":\"" + parentSource + "\",\"sceneItemId\":" + sceneItemId + "}", obsInstance);

            // Parse the JSON response
            var obsResponse = JObject.Parse(jsonResponse);
            // Check if the obsResponse data is not null
            if (obsResponse == null) {
                CPH.SUWriteLog($"No transform data found in jsonResponse", logName);
                CPH.SUWriteLog($"Method complete", logName);
                return null;
            }

            // Return sceneItemTransform from obsResponse
            JObject transform = obsResponse["sceneItemTransform"] as JObject;
            CPH.SUWriteLog($"Returning obsResponse: {transform.ToString()}", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return transform;
        }

        // PULL SCENE ITEM ID
        public static int SUObsPullSceneItemId(this IInlineInvokeProxy CPH, string productName, int obsInstance, int parentSourceType, string parentSource, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsPullSceneItemId";
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
                    CPH.SUWriteLog($"Method complete", logName);
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

            // Pull sceneItemId and return
            int sceneItemId = CPH.SUFindSceneItemId(productName, sceneItems, childSource);
            CPH.SUWriteLog($"Returning sceneItemId: childSource=[{childSource}], sceneItemId=[{sceneItemId}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return sceneItemId;
        }

        private static int SUFindSceneItemId(this IInlineInvokeProxy CPH, string productName, JArray sceneItems, string childSource) {
            // Load log string
            string logName = $"{productName}-SUFindSceneItemId";
            CPH.SUWriteLog("Method Started", logName);

            // Find sceneItemId from all sources on scene
            CPH.SUWriteLog($"Searching for sceneItemId: childSource=[{childSource}]", logName);
            foreach (var item in sceneItems) {
                string currentItemName = item["sourceName"].ToString();
                if (currentItemName == childSource) {
                    int sceneItemId = int.Parse(item["sceneItemId"].ToString());
                    CPH.SUWriteLog($"Found sceneItemId: childSource=[{childSource}], sceneItemId=[{sceneItemId}]", logName);
                    CPH.SUWriteLog($"Method complete", logName);
                    return sceneItemId;
                }
            }
            // If sceneItemId couldn't be found
            CPH.SUWriteLog($"Couldn't find sceneItemId for {childSource}. The source might not exist on the scene", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return -1;
        }

        // SET SOURCE FILTER SETTINGS
        public static void SUObsSetSourceFilterSettings(this IInlineInvokeProxy CPH, string productName, int obsInstance, string sourceName, string filterName, string filterSettings) {
            // Load log string
            string logName = $"{productName}-SUObsSetSourceFilterSettings";
            CPH.SUWriteLog("Method Started", logName);

            // Set source filter settings
            CPH.ObsSendRaw("SetSourceFilterSettings", $$"""
            {
                "sourceName": "{{sourceName}}",
                "filterName": "{{filterName}}",
                "filterSettings": {{{filterSettings}}},
                "overlay": true
            }
            """, obsInstance);

            // Log setting change
            CPH.SUWriteLog($"Set source filter settings: sourceName=[{sourceName}], filterName=[{filterName}], filterSettings=[{filterSettings}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
        }

        // SET INPUT (SOURCE) SETTINGS
        public static void SUObsSetInputSettings(this IInlineInvokeProxy CPH, string productName, int obsInstance, string inputName, string inputSettings) {
            // Load log string
            string logName = $"{productName}-SUObsSetInputSettings";
            CPH.SUWriteLog("Method Started", logName);

            // Set source (input) settings
            CPH.ObsSendRaw("SetInputSettings", $$"""
            {
                "inputName": "{{inputName}}",
                "inputSettings": {{{inputSettings}}},
                "overlay": true
            }
            """, obsInstance);

            // Log setting change
            CPH.SUWriteLog($"Set source (input) settings: inputName=[{inputName}], inputSettings=[{inputSettings}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
        }

        // SET SCENE TRANSITION FOR SCENE
        public static void SUObsSetSceneSceneTransitionOverride(this IInlineInvokeProxy CPH, string productName, int obsInstance, string sceneName, string transitionName, int transitionDuration) {
            // Load log string
            string logName = $"{productName}-SUObsSetSceneSceneTransitionOverride";
            CPH.SUWriteLog("Method Started", logName);

            // Set scene transition override
            CPH.ObsSendRaw("SetSceneSceneTransitionOverride", $$"""
            {
                "sceneName": "{{sceneName}}",
                "transitionName": {{transitionName}},
                "transitionDuration": {{transitionDuration}}
            }
            """, obsInstance);

            // Log scene transition override change
            CPH.SUWriteLog($"Set scene transition override: sceneName=[{sceneName}], transitionName=[{transitionName}], transitionDuration=[{transitionDuration}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
        }

        // GET SOURCE FILTER
        public static JObject SUObsGetSourceFilter(this IInlineInvokeProxy CPH, string productName, int obsInstance, string sourceName, string filterName) {
            // Load log string
            string logName = $"{productName}-ObsGetSourceFilter";
            CPH.SUWriteLog("Method Started", logName);

            // Pull source filter
            string jsonResponse = CPH.ObsSendRaw("GetSourceFilter", "{\"sourceName\":\"" + sourceName + "\",\"filterName\":\"" + filterName + "\"}", obsInstance);
            var obsResponse = JObject.Parse(jsonResponse);
            // Check if the data is null
            if (obsResponse == null) {
                CPH.SUWriteLog($"No data found in obsResponse", logName);
                CPH.SUWriteLog($"Method complete", logName);
                return null;
            }

            // Return obsResponse
            CPH.SUWriteLog($"Returning obsResponse: {obsResponse.ToString()}", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return obsResponse;
        }
    
        // GET SOURCE FILTER LIST
        public static JArray SUObsGetSourceFilterList(this IInlineInvokeProxy CPH, string productName, int obsInstance, string sourceName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetSourceFilterList";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetSourceFilterList", $"{{\"sourceName\":\"{sourceName}\"}}", obsInstance);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray filters = (JArray)responseObj["filters"];
            CPH.SUWriteLog($"Method complete", logName);
            return filters;
        }
    
        // GET INPUT (SOURCE) SETTINGS
        public static JObject SUObsGetInputSettings(this IInlineInvokeProxy CPH, string productName, int obsInstance, string sourceName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetInputSettings";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetInputSettings", "{\"inputName\":\""+sourceName+"\"}", obsInstance);
            JObject responseObj = JObject.Parse(jsonResponse);
            JObject settings = (JObject)responseObj["inputSettings"];
            CPH.SUWriteLog($"Method complete", logName);
            return settings;
        }

        // SET INPUT (SOURCE) VOLUME
        public static void SUObsSetInputVolume(this IInlineInvokeProxy CPH, string productName, string inputName, int volumeType, double volumeLevel, int obsInstance)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetInputSettings";
            CPH.SUWriteLog("Method Started", logName);

            switch (volumeType)
            {
                case 0:
                    CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\""+inputName+"\",\"inputVolumeDb\":"+volumeLevel+"}", obsInstance); 
                    CPH.SUWriteLog($"Set obs input volume: inputName=[{inputName}, inputType=[0 (Db)], volumeLevel=[{volumeLevel}]", logName);
                    break;
                case 1:
                    CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\""+inputName+"\",\"inputVolumeMul\":"+volumeLevel+"}", obsInstance); 
                    CPH.SUWriteLog($"Set obs input volume: inputName=[{inputName}, inputType=[1 (Multiplier)], volumeLevel=[{volumeLevel}]", logName);
                    break;
                default:
                    CPH.SUWriteLog($"Cannot set obs inputVolume. Please make sure the type is either [0](Db) or [1](Multiplier). You set this to [{volumeType}]", logName);
                    break;
            }
            CPH.SUWriteLog("Method complete", logName);
        }
    }
}
