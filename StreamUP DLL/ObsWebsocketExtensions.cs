using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {
    public static class ObsWebsocketExtensions {

        // GET VIDEO SETTINGS
        public static JObject SUObsGetVideoSettings(this IInlineInvokeProxy CPH, string productNumber, int obsConnection) {
            string logName = $"{productNumber}::SUObsGetVideoSettings";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull obs video settings
            string jsonResponse = CPH.ObsSendRaw("GetVideoSettings", "{}", obsConnection);
            if (jsonResponse == null) {
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
                return null;
            }

            // Parse as JObject and return
            JObject obsResponse = JObject.Parse(jsonResponse);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return obsResponse;
        }

        // PULL SCENE ITEM TRANSFORM
        public static JObject SUObsGetSceneItemTransform(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource) {
            string logName = $"{productNumber}::SUObsGetSceneItemTransform";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull sceneItemId
            CPH.SUWriteLog($"Pulling scene item ID for parentSource: [{parentSource}]", logName);
            int sceneItemId = CPH.SUObsGetSceneItemId(productNumber, obsConnection, parentSourceType, parentSource, childSource);
            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
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
                CPH.SUWriteLog("METHOD FAILED", logName);
                return null;
            }

            // Return sceneItemTransform from obsResponse
            JObject transform = obsResponse["sceneItemTransform"] as JObject;

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return transform;
        }

        // PULL SCENE ITEM ID
        public static int SUObsGetSceneItemId(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource) {
            string logName = $"{productNumber}::SUObsGetSceneItemId";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull sceneItemLists (Group or Scene)
            string jsonResponse = "";
            switch (parentSourceType) {
                case OBSSceneType.Scene:
                    jsonResponse = CPH.ObsSendRaw("GetSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsConnection);
                    break;
                case OBSSceneType.Group:
                    jsonResponse = CPH.ObsSendRaw("GetGroupSceneItemList", "{\"sceneName\":\"" + parentSource + "\"}", obsConnection);
                    break;
                default:
                    CPH.SUWriteLog($"parentSourceType is incorrectly set to '{parentSourceType}'. 0=Scene, 1=Group", logName);
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return -1;
            }
            CPH.SUWriteLog("Received JSON response from OBS", logName);

            // Save and parse jsonResponse
            var json = JObject.Parse(jsonResponse);
            var sceneItems = json["sceneItems"] as JArray;
            if (sceneItems == null || sceneItems.Count == 0) {
                // Log if no scene items are found
                CPH.SUWriteLog("No scene items found in the response", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
                return -1;
            } 
            CPH.SUWriteLog($"Found {sceneItems.Count} scene item(s) in jsonResponse", logName);

            // Pull sceneItemId and return
            int sceneItemId = CPH.SUObsFindSceneItemId(productNumber, sceneItems, childSource);
            CPH.SUWriteLog($"Returning sceneItemId: childSource=[{childSource}], sceneItemId=[{sceneItemId}]", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return sceneItemId;
        }

        private static int SUObsFindSceneItemId(this IInlineInvokeProxy CPH, string productNumber, JArray sceneItems, string childSource) 
        {
            string logName = $"{productNumber}::SUObsFindSceneItemId";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Find sceneItemId from all sources on scene
            CPH.SUWriteLog($"Searching for sceneItemId: childSource=[{childSource}]", logName);
            foreach (var item in sceneItems) {
                string currentItemName = item["sourceName"].ToString();
                if (currentItemName == childSource) {
                    int sceneItemId = int.Parse(item["sceneItemId"].ToString());
                    CPH.SUWriteLog($"Found sceneItemId: childSource=[{childSource}], sceneItemId=[{sceneItemId}]", logName);
                    CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
                    return sceneItemId;
                }
            }

            // If sceneItemId couldn't be found
            CPH.SUWriteLog($"Couldn't find sceneItemId for {childSource}. The source might not exist on the scene", logName);
            CPH.SUWriteLog("METHOD FAILED", logName);
            return -1;
        }

        // SET SOURCE FILTER SETTINGS
        public static void SUObsSetSourceFilterSettings(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName, string filterName, string filterSettings) 
        {
            string logName = $"{productNumber}::SUObsSetSourceFilterSettings";
            CPH.SUWriteLog("METHOD STARTED!", logName);

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
            CPH.Wait(50);
            CPH.SUWriteLog($"Set source filter settings: sourceName=[{sourceName}], filterName=[{filterName}], filterSettings=[{filterSettings}]", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        // SET INPUT (SOURCE) SETTINGS
        public static void SUObsSetInputSettings(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string inputName, string inputSettings) 
        {
            string logName = $"{productNumber}::SUObsSetInputSettings";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Set source (input) settings
            CPH.ObsSendRaw("SetInputSettings", $$"""
            {
                "inputName": "{{inputName}}",
                "inputSettings": {{{inputSettings}}},
                "overlay": true
            }
            """, obsConnection);

            // Log setting change
            CPH.Wait(50);
            CPH.SUWriteLog($"Set source (input) settings: inputName=[{inputName}], inputSettings=[{inputSettings}]", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        // SET SCENE TRANSITION FOR SCENE
        public static void SUObsSetSceneSceneTransitionOverride(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sceneName, string transitionName, int transitionDuration) 
        {
            string logName = $"{productNumber}::SUObsSetSceneSceneTransitionOverride";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Set scene transition override
            CPH.ObsSendRaw("SetSceneSceneTransitionOverride", $$"""
            {
                "sceneName": "{{sceneName}}",
                "transitionName": {{transitionName}},
                "transitionDuration": {{transitionDuration}}
            }
            """, obsConnection);

            // Log scene transition override change
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        // GET SOURCE FILTER
        public static JObject SUObsGetSourceFilter(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName, string filterName) 
        {
            string logName = $"{productNumber}::SUObsGetSourceFilter";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull source filter
            string jsonResponse = CPH.ObsSendRaw("GetSourceFilter", "{\"sourceName\":\"" + sourceName + "\",\"filterName\":\"" + filterName + "\"}", obsConnection);
            var obsResponse = JObject.Parse(jsonResponse);
            // Check if the data is null
            if (obsResponse == null) {
                CPH.SUWriteLog($"No data found in obsResponse", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
                return null;
            }

            // Return obsResponse
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return obsResponse;
        }
    
        // GET SOURCE FILTER LIST
        public static JArray SUObsGetSourceFilterList(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName)
        {
            string logName = $"{productNumber}::SUObsGetSourceFilterList";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string jsonResponse = CPH.ObsSendRaw("GetSourceFilterList", $"{{\"sourceName\":\"{sourceName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray filters = (JArray)responseObj["filters"];
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return filters;
        }
    
        // GET INPUT (SOURCE) SETTINGS
        public static JObject SUObsGetInputSettings(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName)
        {
            string logName = $"{productNumber}::SUObsGetInputSettings";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string jsonResponse = CPH.ObsSendRaw("GetInputSettings", "{\"inputName\":\""+sourceName+"\"}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JObject settings = (JObject)responseObj["inputSettings"];
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return settings;
        }

        // SET INPUT (SOURCE) VOLUME
        public static void SUObsSetInputVolume(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string inputName, VolumeType volumeType, double volumeLevel)
        {
            string logName = $"{productNumber}::SUObsSetInputVolume";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            switch (volumeType)
            {
                case VolumeType.Db:
                    CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\""+inputName+"\",\"inputVolumeDb\":"+volumeLevel.ToString(CultureInfo.InvariantCulture)+"}", obsConnection);
                    CPH.SUWriteLog($"Set obs input volume: inputName=[{inputName}], inputType=[Db], volumeLevel=[{volumeLevel.ToString(CultureInfo.InvariantCulture)}]", logName);
                    break;
                case VolumeType.Multiplier:
                    CPH.ObsSendRaw("SetInputVolume", "{\"inputName\":\""+inputName+"\",\"inputVolumeMul\":"+volumeLevel.ToString(CultureInfo.InvariantCulture)+"}", obsConnection);
                    CPH.SUWriteLog($"Set obs input volume: inputName=[{inputName}], inputType=[Multiplier], volumeLevel=[{volumeLevel.ToString(CultureInfo.InvariantCulture)}]", logName);
                    break;
                default:
                    CPH.SUWriteLog($"Invalid volume type [{volumeType}]. Please use Db or Multiplier.", logName);
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return;
            }
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        // GET SCENE ITEM LIST
        public static JArray SUObsGetSceneItemList(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sceneName)
        {
            string logName = $"{productNumber}::SUObsGetSceneItemList";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string jsonResponse = CPH.ObsSendRaw("GetSceneItemList", $"{{\"sceneName\":\"{sceneName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray sceneItems = (JArray)responseObj["sceneItems"];
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return sceneItems;
        }
        
        // GET GROUP SCENE ITEM LIST
        public static JArray SUObsGetGroupSceneItemList(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string groupName)
        {
            string logName = $"{productNumber}::SUObsGetGroupSceneItemList";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string jsonResponse = CPH.ObsSendRaw("GetGroupSceneItemList", $"{{\"sceneName\":\"{groupName}\"}}", obsConnection);
            JObject responseObj = JObject.Parse(jsonResponse);
            JArray sceneItems = (JArray)responseObj["sceneItems"];
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return sceneItems;
        }

        // GET SCENE ITEM NAMES
        public static void SUObsGetSceneItemNames(this IInlineInvokeProxy CPH, string productNumber, int obsConnection,  OBSSceneType sceneType, string sceneName, List<string> sceneItemNames)
        {
            string logName = $"{productNumber}::SUObsGetSceneItemNames";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            JArray sceneItems = new JArray();
            switch (sceneType)
            {
                case OBSSceneType.Scene:
                    CPH.SUWriteLog("Getting scene item list...", logName);
                    sceneItems = CPH.SUObsGetSceneItemList(productNumber, obsConnection, sceneName);
                    CPH.SUWriteLog($"Created sceneItemList of sceneName [{sceneName}]", logName);
                    break;
                case OBSSceneType.Group:
                    CPH.SUWriteLog("Getting group scene item list", logName);
                    sceneItems = CPH.SUObsGetGroupSceneItemList(productNumber, obsConnection, sceneName);
                    CPH.SUWriteLog($"Created sceneItemList of sceneName (group) [{sceneName}]", logName);
                    break;
                default:
                    CPH.SUWriteLog($"You have chosen sceneType=[{sceneType}]. Please set either [0]=Scene, [1]=Group", logName);
                    CPH.SUWriteLog("METHOD FAILED", logName);
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
                    CPH.SUWriteLog($"Source is a group, getting sceneItemNames for that group: sourceName=[{sourceName}]", logName);
                    CPH.SUObsGetSceneItemNames(productNumber, obsConnection, OBSSceneType.Group, sourceName, sceneItemNames);
                }
                else
                {
                    // If it's not a group or isGroup is null, add its source name to the list
                    CPH.SUWriteLog($"Source is not a group, adding sourceName to sceneItemNames list: sourceName=[{sourceName}]", logName);
                    sceneItemNames.Add(sourceName);
                }
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }    

        // AUTOSIZE ADVANCED MASK
        public static void SUObsAutosizeAdvancedMask(this IInlineInvokeProxy CPH, string productNumber, Dictionary<string, object> productSettings, string sourceName, string filterName, double sourceHeight, double sourceWidth, double padHeight, double padWidth)
        {
            string logName = $"{productNumber}::SUObsAutosizeAdvancedMask";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            double newHeight = sourceHeight + (padHeight * double.Parse(productSettings["ScaleFactor"].ToString()));
            double newWidth = sourceWidth + (padWidth * double.Parse(productSettings["ScaleFactor"].ToString()));
            CPH.SUWriteLog($"Pulled size to change Advanced Mask: newHeight=[{newHeight}], newWidth=[{newWidth}]", logName);

            CPH.SUWriteLog("Setting source filter settings...", logName);
            CPH.SUObsSetSourceFilterSettings(productNumber, int.Parse(productSettings["ObsConnection"].ToString()), sourceName, filterName, $"rectangle_width: {newWidth.ToString()}, rectangle_height: {newHeight}");
            
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        public static void SUObsAutoposAdvancedMask(this IInlineInvokeProxy CPH, string productNumber, Dictionary<string, object> productSettings, string sourceName, string filterName, int padX, int padY)
        {
            string logName = $"{productNumber}::SUObsAutoposAdvancedMask";
            CPH.SUWriteLog("METHOD STARTED!", logName);
    
            // Pull advanced mask
            CPH.SUWriteLog("Getting source filter...", logName);
            JObject amFilter = CPH.SUObsGetSourceFilter(productNumber, int.Parse(productSettings["ObsConnection"].ToString()), sourceName, filterName);
            JObject filterSettings = (JObject)amFilter["filterSettings"];
            double amHeight = (double)filterSettings["rectangle_height"];
            double amWidth = (double)filterSettings["rectangle_width"];
            CPH.SUWriteLog($"Pulled size of Advanced Mask: amHeight=[{amHeight}], amWidth=[{amWidth}]", logName);
                        
            double xPos = (amWidth / 2) + (padX * double.Parse(productSettings["ScaleFactor"].ToString()));
            double yPos = (amHeight / 2) + (padY * double.Parse(productSettings["ScaleFactor"].ToString()));
            CPH.SUWriteLog($"Worked out new positions for Advanced Mask: xPos=[{xPos}], yPos=[{yPos}]", logName);

            CPH.SUWriteLog("Setting source filter...", logName);
            CPH.SUObsSetSourceFilterSettings(productNumber, int.Parse(productSettings["ObsConnection"].ToString()), sourceName, filterName, $"position_x: {xPos.ToString(CultureInfo.InvariantCulture)}, position_y: {yPos.ToString(CultureInfo.InvariantCulture)}");

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }
           
        public static int SUObsGetMoveFilterDuration(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName, string filterName)
        {
            string logName = $"{productNumber}::SUObsGetMoveFilterDuration";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            CPH.SUWriteLog("Getting source filter...", logName);
            JObject filter = CPH.SUObsGetSourceFilter(productNumber, obsConnection, sourceName, filterName);
            int duration = (int)filter["filterSettings"]["duration"];

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return duration;
        }
    
        // SPLIT TEXT ONTO MULTIPLE LINES FROM WIDTH
        public static string SUObsSplitTextOnWidth(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string sceneName, string sourceName, string rawText, int maxWidth, int maxHeight)
        {
            string logName = $"{productNumber}::SUObsSplitTextOnWidth";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Remove URLs from the text
            CPH.SUWriteLog("Removing URLs from text...", logName);
            string text = SURemoveUrls(CPH, rawText, "[URL Removed]", productNumber);
            CPH.SUWriteLog($"Text after removing URLs: {text}", logName);

            // Check current text width by setting initial text
            CPH.ObsSetGdiText(sceneName, sourceName, text, obsConnection);
            CPH.SUWriteLog($"Text source updated: sourceName=[{sourceName}] text=[{text}]", logName);
            CPH.Wait(20); // Allow OBS to update

            CPH.SUWriteLog("Getting scene item transfrom...", logName);
            JObject textTransform = CPH.SUObsGetSceneItemTransform(productNumber, obsConnection, parentSourceType, sceneName, sourceName);
            int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
            CPH.SUWriteLog($"Max widget width = [{maxWidth}], current text width=[{textWidth}]", logName);

            string newMessage = "";

            // If text is too wide, split into lines
            if (textWidth > maxWidth)
            {
                CPH.SUWriteLog($"Obs text source width is longer than maxWidth", logName);
                List<string> lines = SUObsSplitTextIntoLines(CPH, productNumber, obsConnection, maxWidth, text, parentSourceType, sceneName, sourceName);
                
                int currentHeight = 0;

                string previousMessage = ""; // Store the message from the previous iteration

                foreach (var line in lines)
                {
                    // Build the test message by adding the new line
                    string testMessage = (newMessage == "" ? line : newMessage + "\n" + line);
                    CPH.ObsSetGdiText(sceneName, sourceName, testMessage, obsConnection);
                    CPH.Wait(20); // Allow OBS to update

                    // Fetch and parse the new height
                    CPH.SUWriteLog("Getting scene item transfrom...", logName);
                    textTransform = CPH.SUObsGetSceneItemTransform(productNumber, obsConnection, parentSourceType, sceneName, sourceName);
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
                        CPH.SUWriteLog("Getting scene item transform...", logName);
                        textTransform = CPH.SUObsGetSceneItemTransform(productNumber, obsConnection, parentSourceType, sceneName, sourceName);
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
            CPH.SUWriteLog("Getting scene item transfrom...", logName);
            textTransform = CPH.SUObsGetSceneItemTransform(productNumber, obsConnection, parentSourceType, sceneName, sourceName);
            int finalWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
            int finalHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));
            CPH.SUWriteLog($"Final text dimensions: width=[{finalWidth}], height=[{finalHeight}]", logName);

            if (finalWidth > maxWidth || finalHeight > maxHeight)
            {
                CPH.SUWriteLog($"Exceeds constraints - Final dimensions: width={finalWidth}, height={finalHeight}", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
                return null;
            }

            CPH.SUWriteLog($"Within constraints - Final dimensions: width={finalWidth}, height={finalHeight}", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return newMessage;
        }

        private static List<string> SUObsSplitTextIntoLines(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, int maxWidth, string message, OBSSceneType parentSourceType, string sceneName, string sourceName)
        {
            string logName = $"{productNumber}::SUObsSplitTextIntoLines";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            List<string> lines = new List<string>();
            string[] words = message.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = (currentLine.Length > 0 ? currentLine + " " : "") + word;
                CPH.ObsSetGdiText(sceneName, sourceName, testLine, obsConnection);
                CPH.Wait(20); // Allow OBS to update
                CPH.SUWriteLog("Getting scene item transform...", logName);
                JObject textTransform = CPH.SUObsGetSceneItemTransform(productNumber, obsConnection, parentSourceType, sceneName, sourceName);
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

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return lines;
        }

        public static string SURemoveUrls(this IInlineInvokeProxy CPH, string text, string replacementText, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SURemoveUrls";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // This pattern matches URLs starting with http://, https://, or ftp:// followed by any characters until a space is encountered
            string urlPattern = @"\b(http|https|ftp)://\S+";
            Regex urlRegex = new Regex(urlPattern, RegexOptions.IgnoreCase);

            // Replace URLs with replacementText
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return urlRegex.Replace(text, replacementText);
        }

        // SET SCENE ITEM TRANSFORM
        public static void SUObsSetSceneItemTransform(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource, string transformSettings) 
        {
            string logName = $"{productNumber}::SUObsSetSceneItemTransform";
            CPH.SUWriteLog("METHOD STARTED!", logName);
            
            // Pull sceneItemId
            CPH.SUWriteLog($"Pulling scene item ID for parentSource: [{parentSource}]", logName);
            CPH.SUWriteLog("Getting scene item id...", logName);
            int sceneItemId = CPH.SUObsGetSceneItemId(productNumber, obsConnection, parentSourceType, parentSource, childSource);
            if (sceneItemId == -1) {
                // Log an error if the Scene Item ID is not found
                CPH.SUWriteLog("Scene Item ID not found", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
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
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return;
        }
    
        // SET SCENE ITEM ENABLED
        public static void SUObsSetSceneItemEnabled(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource, bool visibilityState)
        {
            string logName = $"{productNumber}::SUObsSetSceneItemEnabled";
            CPH.SUWriteLog("METHOD STARTED!", logName);
                		
            // Pull scene Item ID
            CPH.SUWriteLog("Getting scene item id...", logName);
            int id = CPH.SUObsGetSceneItemId(productNumber, obsConnection, parentSourceType, parentSource, childSource);

            // Set scene item enabled/disabled
            CPH.SUWriteLog($"Setting scene item visiblity 'sourcename=[{childSource}]': parentSource=[{parentSource}], sceneItemId=[{id}], sceneItemEnabled=[{visibilityState.ToString().ToLower()}]", logName);
            CPH.ObsSendRaw("SetSceneItemEnabled", "{\"sceneName\":\""+parentSource+"\",\"sceneItemId\":"+id+",\"sceneItemEnabled\":"+visibilityState.ToString().ToLower()+"}", obsConnection);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }
    
        public static bool SUObsGetSceneItemEnabled(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource)
        {
            string logName = $"{productNumber}::SUObsGetSceneItemEnabled";
            CPH.SUWriteLog("METHOD STARTED!", logName);
                		
            // Pull scene Item ID
            CPH.SUWriteLog("Getting scene item id...", logName);
            int id = CPH.SUObsGetSceneItemId(productNumber, obsConnection, parentSourceType, parentSource, childSource);

            // Set scene item enabled/disabled
            string visibilityState = CPH.ObsSendRaw("GetSceneItemEnabled", "{\"sceneName\":\""+parentSource+"\",\"sceneItemId\":"+id+"}", obsConnection);
            // Parse the JSON string
            var jsonObject = JObject.Parse(visibilityState);

            // Extract the 'sceneItemEnabled' value as a boolean
            bool sceneItemEnabled = jsonObject["sceneItemEnabled"].Value<bool>();
            CPH.SUWriteLog($"Got scene item visiblity 'sourcename=[{childSource}]': parentSource=[{parentSource}], sceneItemId=[{id}], sceneItemEnabled=[{sceneItemEnabled.ToString().ToLower()}]", logName);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return sceneItemEnabled;
        }
    
        // Get Canvas Scale Factor
        public static double SUObsGetCanvasScaleFactor(this IInlineInvokeProxy CPH, string productNumber, int obsConnection)
        {
            string logName = $"{productNumber}::SUObsGetCanvasScaleFactor";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull obs canvas width
            CPH.SUWriteLog("Getting video settings...", logName);
            JObject videoSettings = CPH.SUObsGetVideoSettings(productNumber, obsConnection);
            double canvasWidth = (double)videoSettings["baseWidth"];
            CPH.SUWriteLog($"Pulled base canvas width from obs: canvasWidth=[{canvasWidth}]", logName);

            // Work out scale difference based on 1920x1080
            double canvasScaleFactor = (canvasWidth / 1920);
            CPH.SUWriteLog($"Worked out canvas scale factor: canvasScaleFactor=[{canvasScaleFactor}]", logName);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return canvasScaleFactor;
        }

        // Get Current DSK Scene
        public static string SUObsGetCurrentDSKScene(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string dskName)
        {
            string logName = $"{productNumber}::SUObsGetCurrentDSKScene";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Send the request and get the response
            string responseJson = CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"downstream-keyer\",\"requestType\":\"get_downstream_keyer\",\"requestData\":{\"dsk_name\":\""+dskName+"\"}}", obsConnection);
            
            // Log the response (optional)
            CPH.SUWriteLog($"Response: {responseJson}", logName);

            // Parse the response to extract the "scene" value
            string sceneName = "";
            try
            {
                JObject jsonResponse = JObject.Parse(responseJson);
                sceneName = jsonResponse["responseData"]?["scene"]?.ToString();

                if (string.IsNullOrEmpty(sceneName))
                {
                    CPH.SUWriteLog("Scene name not found in response.", logName);
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return null;
                }
            }
            catch (Exception ex)
            {
                CPH.SUWriteLog($"Error parsing response: {ex.Message}", logName);
                CPH.SUWriteLog("METHOD FAILED", logName);
                return null;
            }

            CPH.SUWriteLog($"Extracted scene: {sceneName}", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return sceneName;
        }

        // Set Current DSK Scene
        public static void SUObsSetCurrentDSKScene(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string dskName, string sceneName)
        {
            string logName = $"{productNumber}::SUObsSetCurrentDSKScene";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Set DSK scene
            CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"downstream-keyer\",\"requestType\":\"dsk_select_scene\",\"requestData\":{\"dsk_name\":\""+dskName+"\",\"scene\":\""+sceneName+"\"}}", obsConnection);
            
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        // Get Bitrate
        public static string SUObsGetBitrate(this IInlineInvokeProxy CPH, int obsConnection)
        {
            // Get Bitrate
            JObject bitrateJson = JObject.Parse(CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getBitrate\",\"requestData\":{}}", obsConnection));            
            string bitrate;
            JToken errorToken = bitrateJson.SelectToken("responseData.error");
            if (errorToken != null)
            {
                bitrate = null;
            }
            else
            {
                bitrate = bitrateJson["responseData"]["kbits-per-sec"].ToString();
            }        
            
            return bitrate;
        }

        // Get Source Show Transition
        public static JObject SUObsGetShowTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName)
        {
            JObject json = JObject.Parse(CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getShowTransition\",\"requestData\":{\"sceneName\":\""+sceneName+"\",\"sourceName\":\""+sourceName+"\"}}", obsConnection));
            JObject transition = (JObject)json["responseData"];

            return transition;
        }

        // Get Source Hide Transition
        public static JObject SUObsGetHideTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName)
        {
            JObject json = JObject.Parse(CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getHideTransition\",\"requestData\":{\"sceneName\":\""+sceneName+"\",\"sourceName\":\""+sourceName+"\"}}", obsConnection));
            JObject transition = (JObject)json["responseData"];

            return transition;
        }

        // Set Source Show Transition
        public static void SUObsSetShowTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName, string transitionType, string transitionSettings, int transitionDuration) 
        {
            JObject requestData = new JObject
            {
                ["sceneName"] = sceneName,
                ["sourceName"] = sourceName,
                ["transitionType"] = transitionType,
                ["transitionSettings"] = JObject.Parse(transitionSettings),
                ["transitionDuration"] = transitionDuration
            };

            JObject request = new JObject
            {
                ["vendorName"] = "streamup",
                ["requestType"] = "setShowTransition",
                ["requestData"] = requestData
            };

            // Send the request
            CPH.ObsSendRaw("CallVendorRequest", request.ToString(), obsConnection);
        }

        // Set Source Hide Transition
        public static void SUObsSetHideTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName, string transitionType, string transitionSettings, int transitionDuration) 
        {
            JObject requestData = new JObject
            {
                ["sceneName"] = sceneName,
                ["sourceName"] = sourceName,
                ["transitionType"] = transitionType,
                ["transitionSettings"] = JObject.Parse(transitionSettings),
                ["transitionDuration"] = transitionDuration
            };

            JObject request = new JObject
            {
                ["vendorName"] = "streamup",
                ["requestType"] = "setHideTransition",
                ["requestData"] = requestData
            };

            // Send the request
            CPH.ObsSendRaw("CallVendorRequest", request.ToString(), obsConnection);
        }

        // Get currect selected source
        public static string SUObsGetCurrentSource(this IInlineInvokeProxy CPH, int obsConnection) 
        {
            JObject response = JObject.Parse(CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getCurrentSource\",\"requestData\":null}", obsConnection));
            string sourceName = response["responseData"]["selectedSource"].ToString();
            return sourceName;
        }

        // Get Obs recording output filepath
        public static string SUObsGetOutputFilePath(this IInlineInvokeProxy CPH, int obsConnection) 
        {
            JObject response = JObject.Parse(CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getOutputFilePath\",\"requestData\":null}", obsConnection));
            string filePath = response["responseData"]["outputFilePath"].ToString();
            return filePath;
        }

        // Screenshot current selected source
        public static string SUObsScreenshotCurrentSource(this IInlineInvokeProxy CPH, int obsConnection, bool customFileName = false)
        {
            // Load vars
            string sourceName = CPH.SUObsGetCurrentSource(obsConnection);
            CPH.SUWriteLog($"sourceName=[{sourceName}]");
            string filePath;
            string fileName;

            string obsOutputFilePath = SUObsGetOutputFilePath(CPH, obsConnection).Replace("\\", "\\\\");
            CPH.SUWriteLog($"obsOutputFilePath=[{obsOutputFilePath}]");

            DateTime currentTime = DateTime.Now;
            string dateTimeString = currentTime.ToString("yyyyMMdd_HHmmss");
            CPH.SUWriteLog($"dateTimeString=[{dateTimeString}]");

            if (customFileName)
            {
                fileName = CPH.SUUIShowSaveScreenshotDialog(sourceName, dateTimeString); 
                filePath = $"{obsOutputFilePath}\\\\{fileName}.png";       
            }
            else
            {
                fileName = dateTimeString;
                filePath = $"{obsOutputFilePath}\\\\{sourceName}_{fileName}.png";       
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                string screenshotJson = $"{{\"sourceName\":\"{sourceName}\",\"imageFormat\":\"png\",\"imageFilePath\":\"{filePath}\"}}";
                CPH.SUWriteLog($"screenshotJson=[{screenshotJson}]");

                CPH.ObsSendRaw("SaveSourceScreenshot", screenshotJson, obsConnection);
                CPH.SUUIShowToastNotification("Screenshot Saved", "File saved to your OBS recording output folder.");
            }
            else
            {
                CPH.SUUIShowToastNotification("Screenshot Saving Error", "No name was selected for the screenshot. Cancelling.");
            }

            return filePath;
        }
    }

    public enum VolumeType
    {
        Db = 0,        // Decibels
        Multiplier = 1 // Multiplicative factor
    }

    public enum OBSSceneType
    {
        Scene = 0,
        Group = 1
    }
}
