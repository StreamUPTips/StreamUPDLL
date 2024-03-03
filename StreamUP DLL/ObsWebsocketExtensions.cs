using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {
    public static class ObsWebsocketExtensions {

        // GET VIDEO SETTINGS
        public static JObject SUObsGetVideoSettings(this IInlineInvokeProxy CPH, string productName, int obsConnection) {
            // Load log string
            string logName = $"{productName}-SUObsGetVideoSettings";
            CPH.SUWriteLog("Method Started", logName);

            // Pull obs video settings
            string jsonResponse = CPH.ObsSendRaw("GetVideoSettings", "{}", obsConnection);
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
        public static JObject SUObsPullSceneItemTransform(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsPullSceneItemTransform";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemId
            CPH.SUWriteLog($"Pulling scene item ID for parentSource: [{parentSource}]", logName);
            int sceneItemId = CPH.SUObsPullSceneItemId(productName, obsConnection, parentSourceType, parentSource, childSource);
            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog($"Method complete", logName);
                return null;
            }

            // Extract the transformation data of the source
            CPH.SUWriteLog($"Sending request to get scene item transform for (sceneItemId: [{sceneItemId}]) on (parentSource: [{parentSource}])", logName);
            string jsonResponse = CPH.ObsSendRaw("GetSceneItemTransform", "{\"sceneName\":\"" + parentSource + "\",\"sceneItemId\":" + sceneItemId + "}", obsConnection);

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
        public static int SUObsPullSceneItemId(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsPullSceneItemId";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemLists (Group or Scene)
            string jsonResponse = "";
            switch (parentSourceType) {
                case 0:
                    jsonResponse = CPH.ObsSendRaw("GetSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsConnection);
                    break;
                case 1:
                    jsonResponse = CPH.ObsSendRaw("GetGroupSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsConnection);
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
        public static void SUObsSetSourceFilterSettings(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sourceName, string filterName, string filterSettings) {
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
            """, obsConnection);

            // Log setting change
            CPH.SUWriteLog($"Set source filter settings: sourceName=[{sourceName}], filterName=[{filterName}], filterSettings=[{filterSettings}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
        }

        // SET INPUT (SOURCE) SETTINGS
        public static void SUObsSetInputSettings(this IInlineInvokeProxy CPH, string productName, int obsConnection, string inputName, string inputSettings) {
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
            """, obsConnection);

            // Log setting change
            CPH.SUWriteLog($"Set source (input) settings: inputName=[{inputName}], inputSettings=[{inputSettings}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
        }

        // SET SCENE TRANSITION FOR SCENE
        public static void SUObsSetSceneSceneTransitionOverride(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sceneName, string transitionName, int transitionDuration) {
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
            """, obsConnection);

            // Log scene transition override change
            CPH.SUWriteLog($"Set scene transition override: sceneName=[{sceneName}], transitionName=[{transitionName}], transitionDuration=[{transitionDuration}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
        }

        // GET SOURCE FILTER
        public static JObject SUObsGetSourceFilter(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sourceName, string filterName) {
            // Load log string
            string logName = $"{productName}-ObsGetSourceFilter";
            CPH.SUWriteLog("Method Started", logName);

            // Pull source filter
            string jsonResponse = CPH.ObsSendRaw("GetSourceFilter", "{\"sourceName\":\"" + sourceName + "\",\"filterName\":\"" + filterName + "\"}", obsConnection);
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
        public static JArray SUObsGetSourceFilterList(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sourceName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetSourceFilterList";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetSourceFilterList", $"{{\"sourceName\":\"{sourceName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray filters = (JArray)responseObj["filters"];
            CPH.SUWriteLog($"Method complete", logName);
            return filters;
        }
    
        // GET INPUT (SOURCE) SETTINGS
        public static JObject SUObsGetInputSettings(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sourceName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetInputSettings";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetInputSettings", "{\"inputName\":\""+sourceName+"\"}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JObject settings = (JObject)responseObj["inputSettings"];
            CPH.SUWriteLog($"Method complete", logName);
            return settings;
        }

        // SET INPUT (SOURCE) VOLUME
        public static void SUObsSetInputVolume(this IInlineInvokeProxy CPH, string productName, string inputName, int volumeType, double volumeLevel, int obsConnection)
        {
            // Load log string
            string logName = $"{productName}-SUObsSetInputVolume";
            CPH.SUWriteLog("Method Started", logName);

            switch (volumeType)
            {
                case 0:
                    CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\""+inputName+"\",\"inputVolumeDb\":"+volumeLevel.ToString(CultureInfo.InvariantCulture)+"}", obsConnection); 
                    CPH.SUWriteLog($"Set obs input volume: inputName=[{inputName}, inputType=[0 (Db)], volumeLevel=[{volumeLevel.ToString(CultureInfo.InvariantCulture)}]", logName);
                    break;
                case 1:
                    CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\""+inputName+"\",\"inputVolumeMul\":"+volumeLevel.ToString(CultureInfo.InvariantCulture)+"}", obsConnection); 
                    CPH.SUWriteLog($"Set obs input volume: inputName=[{inputName}, inputType=[1 (Multiplier)], volumeLevel=[{volumeLevel.ToString(CultureInfo.InvariantCulture)}]", logName);
                    break;
                default:
                    CPH.SUWriteLog($"Cannot set obs inputVolume. Please make sure the type is either [0](Db) or [1](Multiplier). You set this to [{volumeType}]", logName);
                    break;
            }
            CPH.SUWriteLog("Method complete", logName);
        }
    
        // GET SCENE ITEM LIST
        public static JArray SUObsGetSceneItemList(this IInlineInvokeProxy CPH, string productName, int obsConnection, string sceneName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetSceneItemList";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetSceneItemList", $"{{\"sceneName\":\"{sceneName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray sceneItems = (JArray)responseObj["sceneItems"];
            CPH.SUWriteLog($"Method complete", logName);
            return sceneItems;
        }
        
        // GET GROUP SCENE ITEM LIST
        public static JArray SUGetGroupSceneItemList(this IInlineInvokeProxy CPH, string productName, int obsConnection, string groupName)
        {
            // Load log string
            string logName = $"{productName}-SUGetGroupSceneItemList";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetGroupSceneItemList", $"{{\"sceneName\":\"{groupName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray sceneItems = (JArray)responseObj["sceneItems"];
            CPH.SUWriteLog($"Method complete", logName);
            return sceneItems;
        }

        // GET SCENE ITEM NAMES
        public static void SUGetSceneItemNames(this IInlineInvokeProxy CPH, string productName, int obsConnection, int sceneType, string sceneName, List<string> sceneItemNames)
        {
            // Load log string
            string logName = $"{productName}-SUGetSceneItemNames";
            CPH.SUWriteLog("Method Started", logName);

            JArray sceneItems = new JArray();
            switch (sceneType)
            {
                case 0:
                    sceneItems = CPH.SUObsGetSceneItemList(productName, obsConnection, sceneName);
                    CPH.SUWriteLog($"Created sceneItemList of sceneName [{sceneName}]", logName);
                    break;
                case 1:
                    sceneItems = CPH.SUGetGroupSceneItemList(productName, obsConnection, sceneName);
                    CPH.SUWriteLog($"Created sceneItemList of sceneName (group) [{sceneName}]", logName);
                    break;
                default:
                    CPH.SUWriteLog($"You have chosen sceneType=[{sceneType}]. Please set either [0]=Scene, [1]=Group", logName);
                    break;
            }
            CPH.SUWriteLog($"sceneItems=[{sceneItems.ToString()}]", logName);
            foreach (JObject item in sceneItems)
            {
                bool? isGroup = (bool?)item["isGroup"];
                string sourceName = (string)item["sourceName"];

                // Check if isGroup has a value and is true
                if (isGroup.HasValue && isGroup.Value)
                {
                    // If the item is a group, recursively fetch its scene items
                    CPH.SUWriteLog($"Source is a group, running sceneItemNames for that group: sourceName=[{sourceName}]", logName);
                    CPH.SUGetSceneItemNames(productName, obsConnection, 1, sourceName, sceneItemNames);
                }
                else
                {
                    // If it's not a group or isGroup is null, add its source name to the list
                    CPH.SUWriteLog($"Source is not a group, adding sourceName to sceneItemNames list: sourceName=[{sourceName}]", logName);
                    sceneItemNames.Add(sourceName);
                }
            }
            
        }    

        // AUTOSIZE ADVANCED MASK
        public static void SUAutosizeAdvancedMask(this IInlineInvokeProxy CPH, string productName, string productNumber, int obsConnection, string sourceName, string filterName, double sourceHeight, double sourceWidth, double padHeight, double padWidth)
        {
            // Load log string
            string logName = $"{productName}-SUAutosizeAdvancedMask";
            CPH.SUWriteLog("Method Started", logName);

            double scaleFactor = CPH.GetGlobalVar<double>($"{productNumber}_ScaleFactor", true);
            CPH.SUWriteLog($"Pulled product scaleFactor: scaleFactor=[{scaleFactor}]", logName);

            double newHeight = sourceHeight + (padHeight * scaleFactor);
            double newWidth = sourceWidth + (padWidth * scaleFactor);
            CPH.SUWriteLog($"Pulled size to change Advanced Mask: newHeight=[{newHeight}], newWidth=[{newWidth}]", logName);

            CPH.SUObsSetSourceFilterSettings(productName, obsConnection, sourceName, filterName, $"rectangle_width: {newWidth.ToString()}, rectangle_height: {newHeight}");
            
            CPH.SUWriteLog($"Method complete", logName);
        }

        public static void SUAutoposAdvancedMask(this IInlineInvokeProxy CPH, string productName, string productNumber, int obsConnection, string sourceName, string filterName, int padX, int padY)
        {
            // Load log string
            string logName = $"{productName}-SUAutoposAdvancedMask";
            CPH.SUWriteLog("Method Started", logName);
    
            // Pull advanced mask
            JObject amFilter = CPH.SUObsGetSourceFilter(productName, obsConnection, sourceName, filterName);
            JObject filterSettings = (JObject)amFilter["filterSettings"];
            double amHeight = (double)filterSettings["rectangle_height"];
            double amWidth = (double)filterSettings["rectangle_width"];
            CPH.SUWriteLog($"Pulled size of Advanced Mask: amHeight=[{amHeight}], amWidth=[{amWidth}]", logName);
            
            // Pull canvas scaleFactor
            double scaleFactor = CPH.GetGlobalVar<double>($"{productNumber}_ScaleFactor", true);
            CPH.SUWriteLog($"Pulled product scaleFactor: scaleFactor=[{scaleFactor}]", logName);
            
            double xPos = (amWidth / 2) + (padX * scaleFactor);
            double yPos = (amHeight / 2) + (padY * scaleFactor);
            CPH.SUWriteLog($"Worked out new positions for Advanced Mask: xPos=[{xPos}], yPos=[{yPos}]", logName);

            CPH.SUObsSetSourceFilterSettings(productName, obsConnection, sourceName, filterName, $"position_x: {xPos.ToString(CultureInfo.InvariantCulture)}, position_y: {yPos.ToString(CultureInfo.InvariantCulture)}");

            CPH.SUWriteLog($"Method complete", logName);
        }
           
        public static int SUGetMoveFilterDuration(this IInlineInvokeProxy CPH, string productName, string productNumber, int obsConnection, string sourceName, string filterName)
        {
            // Load log string
            string logName = $"{productName}-SUGetMoveFilterDuration";
            CPH.SUWriteLog("Method Started", logName);

            JObject filter = CPH.SUObsGetSourceFilter(productName, obsConnection, sourceName, filterName);
            int duration = (int)filter["filterSettings"]["duration"];

            CPH.SUWriteLog($"Method complete", logName);
            return duration;
        }
    
    }
}
