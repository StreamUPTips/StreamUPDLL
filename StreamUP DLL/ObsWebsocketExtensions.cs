using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
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
        public static JObject SUObsGetSceneItemTransform(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsGetSceneItemTransform";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemId
            CPH.SUWriteLog($"Pulling scene item ID for parentSource: [{parentSource}]", logName);
            int sceneItemId = CPH.SUObsGetSceneItemId(productName, obsConnection, parentSourceType, parentSource, childSource);
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
        public static int SUObsGetSceneItemId(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsGetSceneItemId";
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
            int sceneItemId = CPH.SUObsFindSceneItemId(productName, sceneItems, childSource);
            CPH.SUWriteLog($"Returning sceneItemId: childSource=[{childSource}], sceneItemId=[{sceneItemId}]", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return sceneItemId;
        }

        private static int SUObsFindSceneItemId(this IInlineInvokeProxy CPH, string productName, JArray sceneItems, string childSource) {
            // Load log string
            string logName = $"{productName}-SUObsFindSceneItemId";
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
        public static JArray SUObsGetGroupSceneItemList(this IInlineInvokeProxy CPH, string productName, int obsConnection, string groupName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetGroupSceneItemList";
            CPH.SUWriteLog("Method Started", logName);

            string jsonResponse = CPH.ObsSendRaw("GetGroupSceneItemList", $"{{\"sceneName\":\"{groupName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray sceneItems = (JArray)responseObj["sceneItems"];
            CPH.SUWriteLog($"Method complete", logName);
            return sceneItems;
        }

        // GET SCENE ITEM NAMES
        public static void SUObsGetSceneItemNames(this IInlineInvokeProxy CPH, string productName, int obsConnection, int sceneType, string sceneName, List<string> sceneItemNames)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetSceneItemNames";
            CPH.SUWriteLog("Method Started", logName);

            JArray sceneItems = new JArray();
            switch (sceneType)
            {
                case 0:
                    sceneItems = CPH.SUObsGetSceneItemList(productName, obsConnection, sceneName);
                    CPH.SUWriteLog($"Created sceneItemList of sceneName [{sceneName}]", logName);
                    break;
                case 1:
                    sceneItems = CPH.SUObsGetGroupSceneItemList(productName, obsConnection, sceneName);
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
                    CPH.SUObsGetSceneItemNames(productName, obsConnection, 1, sourceName, sceneItemNames);
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
        public static void SUObsAutosizeAdvancedMask(this IInlineInvokeProxy CPH, string productName, string productNumber, int obsConnection, string sourceName, string filterName, double sourceHeight, double sourceWidth, double padHeight, double padWidth)
        {
            // Load log string
            string logName = $"{productName}-SUObsAutosizeAdvancedMask";
            CPH.SUWriteLog("Method Started", logName);

            double scaleFactor = CPH.GetGlobalVar<double>($"{productNumber}_ScaleFactor", true);
            CPH.SUWriteLog($"Pulled product scaleFactor: scaleFactor=[{scaleFactor}]", logName);

            double newHeight = sourceHeight + (padHeight * scaleFactor);
            double newWidth = sourceWidth + (padWidth * scaleFactor);
            CPH.SUWriteLog($"Pulled size to change Advanced Mask: newHeight=[{newHeight}], newWidth=[{newWidth}]", logName);

            CPH.SUObsSetSourceFilterSettings(productName, obsConnection, sourceName, filterName, $"rectangle_width: {newWidth.ToString()}, rectangle_height: {newHeight}");
            
            CPH.SUWriteLog($"Method complete", logName);
        }

        public static void SUObsAutoposAdvancedMask(this IInlineInvokeProxy CPH, string productName, string productNumber, int obsConnection, string sourceName, string filterName, int padX, int padY)
        {
            // Load log string
            string logName = $"{productName}-SUObsAutoposAdvancedMask";
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
           
        public static int SUObsGetMoveFilterDuration(this IInlineInvokeProxy CPH, string productName, string productNumber, int obsConnection, string sourceName, string filterName)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetMoveFilterDuration";
            CPH.SUWriteLog("Method Started", logName);

            JObject filter = CPH.SUObsGetSourceFilter(productName, obsConnection, sourceName, filterName);
            int duration = (int)filter["filterSettings"]["duration"];

            CPH.SUWriteLog($"Method complete", logName);
            return duration;
        }
    
        // EFFICIENT GET OBS SCENE TRANSFORM
        public static JObject SUObsGetSceneItemTransformFast(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource)
        {
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
                    CPH.SUWriteLog($"parentSourceType is incorrectly set to '{parentSourceType}'. 0=Scene, 1=Group");
                    CPH.SUWriteLog($"Method complete");
                    return null;
            }

            // Save and parse jsonResponse
            var json = JObject.Parse(jsonResponse);
            var sceneItems = json["sceneItems"] as JArray;
            if (sceneItems == null || sceneItems.Count == 0) {
                CPH.SUWriteLog("No scene items found in the response");
                CPH.SUWriteLog($"Method complete");
                return null;
            }

            // Find sceneItemId from all sources on scene
            int sceneItemId = -1;
            foreach (var item in sceneItems) {
                string currentItemName = item["sourceName"].ToString();
                if (currentItemName == childSource) {
                    sceneItemId = int.Parse(item["sceneItemId"].ToString());
                    break;
                }
            }

            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found");
                CPH.SUWriteLog($"Method complete");
                return null;
            }

            jsonResponse = CPH.ObsSendRaw("GetSceneItemTransform", "{\"sceneName\":\"" + parentSource + "\",\"sceneItemId\":" + sceneItemId + "}", obsConnection);

            // Parse the JSON response
            var obsResponse = JObject.Parse(jsonResponse);
            // Check if the obsResponse data is not null
            if (obsResponse == null) {
                CPH.SUWriteLog($"No transform data found in jsonResponse");
                CPH.SUWriteLog($"Method complete");
                return null;
            }

            // Return sceneItemTransform from obsResponse
            JObject transform = obsResponse["sceneItemTransform"] as JObject;
            return transform;
        }
    
        // SPLIT TEXT ONTO MULTIPLE LINES FROM WIDTH
        public static string SUObsSplitTextOnWidth(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string sceneName, string sourceName, string rawText, int maxWidth, int maxHeight)
        {
            // Load log string
            string logName = $"{productName}-SUObsSplitTextOnWidth";
            CPH.SUWriteLog("Method Started", logName);

            // Remove URLs from the text
            string text = SURemoveUrls(rawText, "[URL Removed]");
            CPH.SUWriteLog($"Text after removing URLs: {text}", logName);

            // Check current text width by setting initial text
            CPH.ObsSetGdiText(sceneName, sourceName, text, obsConnection);
            CPH.SUWriteLog($"Text source updated: sourceName=[{sourceName}] text=[{text}]", logName);
            CPH.Wait(20); // Allow OBS to update

            JObject textTransform = CPH.SUObsGetSceneItemTransformFast(productName, obsConnection, parentSourceType, sceneName, sourceName);
            int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
            CPH.SUWriteLog($"Max widget width = [{maxWidth}], current text width=[{textWidth}]", logName);

            string newMessage = "";

            // If text is too wide, split into lines
            if (textWidth > maxWidth)
            {
                CPH.SUWriteLog($"Obs text source width is longer than maxWidth", logName);
                List<string> lines = SUObsSplitTextIntoLines(CPH, maxWidth, text, parentSourceType, sceneName, sourceName, obsConnection, productName);
                
                int currentHeight = 0;

                string previousMessage = ""; // Store the message from the previous iteration

                foreach (var line in lines)
                {
                    // Build the test message by adding the new line
                    string testMessage = (newMessage == "" ? line : newMessage + "\n" + line);
                    CPH.ObsSetGdiText(sceneName, sourceName, testMessage, obsConnection);
                    CPH.Wait(20); // Allow OBS to update

                    // Fetch and parse the new height
                    textTransform = CPH.SUObsGetSceneItemTransformFast(productName, obsConnection, parentSourceType, sceneName, sourceName);
                    currentHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));

                    if (currentHeight > maxHeight)
                    {
                        // If the height exceeds the max height, use the previous message and append '...'
                        newMessage = previousMessage == "" ? "..." : previousMessage + "...";
                        CPH.SUWriteLog($"Height exceeded after adding line. New message set: {newMessage}", logName);
                        break; // Exit the loop as we've reached the limit
                    }
                    else
                    {
                        // If the height is within bounds, update newMessage and keep this as previousMessage
                        newMessage = testMessage;
                        previousMessage = newMessage; // Update previousMessage to this newMessage for next iteration
                        CPH.SUWriteLog($"Message within height limit:\n{newMessage}", logName);
                    }
                }

                if (newMessage.EndsWith("..."))
                {
                    CPH.SUWriteLog("Starting final adjustment to fit within max width and height.", logName);
                    bool fitsWithinBounds = false;
                    while (!fitsWithinBounds)
                    {
                        CPH.ObsSetGdiText(sceneName, sourceName, newMessage, obsConnection);
                        CPH.Wait(20); // Allow OBS to update

                        // Fetch and parse the current dimensions
                        textTransform = CPH.SUObsGetSceneItemTransformFast(productName, obsConnection, parentSourceType, sceneName, sourceName);
                        currentHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));
                        int currentWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
                        CPH.SUWriteLog($"Current dimensions after adjustments: Width = {currentWidth}, Height = {currentHeight}", logName);

                        if (currentHeight > maxHeight || currentWidth > maxWidth)
                        {
                            // Remove the last word along with '...' then add '...' back
                            int lastSpaceIndex = newMessage.LastIndexOf(' ', newMessage.Length - 4); // -4 to ignore the '...'
                            
                            if (lastSpaceIndex > 0) // Ensure there is a space to find, indicating another word exists
                            {
                                // Update newMessage by removing the last word
                                newMessage = newMessage.Substring(0, lastSpaceIndex) + "...";
                                CPH.SUWriteLog($"Text adjusted to fit within constraints: {newMessage}", logName);
                            }
                            else
                            {
                                // No more spaces found, which means no more words left to remove
                                fitsWithinBounds = true; // Exit the loop as we can't reduce the text further
                                CPH.SUWriteLog("No more words left to remove, the text may still exceed maximum dimensions.", logName);
                            }
                        }
                        else
                        {
                            // Text fits within the height and width, exit the loop
                            fitsWithinBounds = true;
                            CPH.SUWriteLog("Text now fits within maximum width and height.", logName);
                        }
                    }
                }
            }
            else
            {
                // If the text is within the width limit, use the original text
                newMessage = text;
            }

            CPH.SUWriteLog($"Final message before checking dimensions: {newMessage}", logName);

            // Set final text in OBS and log final dimensions
            CPH.ObsSetGdiText(sceneName, sourceName, newMessage, obsConnection);
            CPH.Wait(20); // Allow OBS to update for the last time
            textTransform = CPH.SUObsGetSceneItemTransformFast(productName, obsConnection, parentSourceType, sceneName, sourceName);
            int finalWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
            int finalHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));
            CPH.SUWriteLog($"Final text dimensions: width=[{finalWidth}], height=[{finalHeight}]", logName);

            if (finalWidth > maxWidth || finalHeight > maxHeight)
            {
                CPH.SUWriteLog($"Exceeds constraints - Final dimensions: width={finalWidth}, height={finalHeight}", logName);
                return null;
            }

            CPH.SUWriteLog($"Within constraints - Final dimensions: width={finalWidth}, height={finalHeight}", logName);
            CPH.SUWriteLog("Method complete", logName);
            return newMessage;
        }

        private static List<string> SUObsSplitTextIntoLines(this IInlineInvokeProxy CPH, int maxWidth, string message, int parentSourceType, string sceneName, string sourceName, int obsConnection, string productName)
        {
            List<string> lines = new List<string>();
            string[] words = message.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = (currentLine.Length > 0 ? currentLine + " " : "") + word;
                CPH.ObsSetGdiText(sceneName, sourceName, testLine, obsConnection);
                CPH.Wait(20); // Allow OBS to update
                JObject textTransform = CPH.SUObsGetSceneItemTransformFast(productName, obsConnection, parentSourceType, sceneName, sourceName);
                int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));

                if (textWidth <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine); // Add the current line to the list of lines
                    }
                    currentLine = word; // Start a new line with the current word
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine); // Add the remaining line to the list
            }

            return lines;
        }

        public static string SURemoveUrls(string text, string replacementText)
        {
            // This pattern matches URLs starting with http://, https://, or ftp:// followed by any characters until a space is encountered
            string urlPattern = @"\b(http|https|ftp)://\S+";
            Regex urlRegex = new Regex(urlPattern, RegexOptions.IgnoreCase);

            // Replace URLs with replacementText
            return urlRegex.Replace(text, replacementText);
        }

        // SET SCENE ITEM TRANSFORM
        public static void SUObsSetSceneItemTransform(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource, string transformSettings) {
            // Load log string
            string logName = $"{productName}-SUObsSetSceneItemTransform";
            CPH.SUWriteLog("Method Started", logName);

            // Pull sceneItemId
            CPH.SUWriteLog($"Pulling scene item ID for parentSource: [{parentSource}]", logName);
            int sceneItemId = CPH.SUObsGetSceneItemId(productName, obsConnection, parentSourceType, parentSource, childSource);
            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog($"Method complete", logName);
                return;
            }

            // Set scene item transform using the provided JSON settings
            string jsonCommand = $$"""
            {
                "sceneName": "{{parentSource}}",
                "sceneItemId": {{sceneItemId}},
                "sceneItemTransform": {{{transformSettings}}}
            }
            """;
            CPH.ObsSendRaw("SetSceneItemTransform", jsonCommand, obsConnection);

            CPH.SUWriteLog($"Set scene item transform for (sceneItemId: [{sceneItemId}]) on (parentSource: [{parentSource}]) to (json: {transformSettings})", logName);
            CPH.SUWriteLog($"Method complete", logName);
            return;
        }
    
        // SET SCENE ITEM ENABLED
        public static void SUObsSetSceneItemEnabled(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource, bool visibilityState)
        {
            // Load log string
            string logName = $"{productName}-SUObsSetSceneItemEnabled";
            CPH.SUWriteLog("Method Started", logName);
                		
            // Pull scene Item ID
            int id = CPH.SUObsGetSceneItemId(productName, obsConnection, parentSourceType, parentSource, childSource);

            // Set scene item enabled/disabled
            CPH.SUWriteLog($"Setting scene item visiblity 'sourcename=[{childSource}]': parentSource=[{parentSource}], sceneItemId=[{id}], sceneItemEnabled=[{visibilityState.ToString().ToLower()}]", logName);
            CPH.ObsSendRaw("SetSceneItemEnabled", "{\"sceneName\":\""+parentSource+"\",\"sceneItemId\":"+id+",\"sceneItemEnabled\":"+visibilityState.ToString().ToLower()+"}", obsConnection);

            CPH.SUWriteLog($"Method complete", logName);
        }
    
        public static bool SUObsGetSceneItemEnabled(this IInlineInvokeProxy CPH, string productName, int obsConnection, int parentSourceType, string parentSource, string childSource)
        {
            // Load log string
            string logName = $"{productName}-SUObsGetSceneItemEnabled";
            CPH.SUWriteLog("Method Started", logName);
                		
            // Pull scene Item ID
            int id = CPH.SUObsGetSceneItemId(productName, obsConnection, parentSourceType, parentSource, childSource);

            // Set scene item enabled/disabled
            string visibilityState = CPH.ObsSendRaw("GetSceneItemEnabled", "{\"sceneName\":\""+parentSource+"\",\"sceneItemId\":"+id+"}", obsConnection);
            // Parse the JSON string
            var jsonObject = JObject.Parse(visibilityState);

            // Extract the 'sceneItemEnabled' value as a boolean
            bool sceneItemEnabled = jsonObject["sceneItemEnabled"].Value<bool>();
            CPH.SUWriteLog($"Got scene item visiblity 'sourcename=[{childSource}]': parentSource=[{parentSource}], sceneItemId=[{id}], sceneItemEnabled=[{sceneItemEnabled.ToString().ToLower()}]", logName);

            CPH.SUWriteLog($"Method complete", logName);
            return sceneItemEnabled;
        }
    
        // Get Canvas Scale Factor
        public static double SUGetObsCanvasScaleFactor(this IInlineInvokeProxy CPH, string productNumber, string productName, int obsConnection)
        {
            // Load log string
            string logName = "GeneralExtensions-SUGetObsCanvasScaleFactor";
            CPH.SUWriteLog("Method Started", logName);

            // Pull obs canvas width
            JObject videoSettings = CPH.SUObsGetVideoSettings(productName, obsConnection);
            double canvasWidth = (double)videoSettings["baseWidth"];
            CPH.SUWriteLog($"Pulled base canvas width from obs: canvasWidth=[{canvasWidth}]", logName);

            // Work out scale difference based on 1920x1080
            double canvasScaleFactor = (canvasWidth / 1920);
            CPH.SUWriteLog($"Worked out canvas scale factor: canvasScaleFactor=[{canvasScaleFactor}]", logName);

            // Save canvasScaleFactor to sb global var
            CPH.SetGlobalVar($"{productNumber}_ScaleFactor", canvasScaleFactor); 

            CPH.SUWriteLog("Method complete", logName);
            return canvasScaleFactor;
        }

    }
}
