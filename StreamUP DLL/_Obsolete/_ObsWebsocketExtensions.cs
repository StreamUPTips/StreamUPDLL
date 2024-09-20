using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;
using static StreamUP.StreamUpLib;

namespace StreamUP
{
    public static class ObsWebsocketExtensions
    {
        // GET SCENE ITEM ID
        [Obsolete]
        public static int SUObsGetSceneItemId(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource)
        {
            string logName = $"{productNumber}::SUObsGetSceneItemId";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull sceneItemLists (Group or Scene)
            string jsonResponse = "";
            switch (parentSourceType)
            {
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
            if (sceneItems == null || sceneItems.Count == 0)
            {
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

        [Obsolete]
        private static int SUObsFindSceneItemId(this IInlineInvokeProxy CPH, string productNumber, JArray sceneItems, string childSource)
        {
            string logName = $"{productNumber}::SUObsFindSceneItemId";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Find sceneItemId from all sources on scene
            CPH.SUWriteLog($"Searching for sceneItemId: childSource=[{childSource}]", logName);
            foreach (var item in sceneItems)
            {
                string currentItemName = item["sourceName"].ToString();
                if (currentItemName == childSource)
                {
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

        // GET VIDEO SETTINGS
        [Obsolete]
        public static JObject SUObsGetVideoSettings(this IInlineInvokeProxy CPH, string productNumber, int obsConnection)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsVideoSettings(obsConnection, out JObject settings))
            {
                return null;
            }

            return settings;
        }

        // GET CURRENT SOURCE
        [Obsolete]
        public static string SUObsGetCurrentSource(this IInlineInvokeProxy CPH, int obsConnection)
        {
            StreamUpLib sup = new StreamUpLib(CPH);
            if (!sup.GetObsSelectedSource(obsConnection, out string source))
            {
                return null;
            }

            return source;
        }

        // GET SOURCE FILTER
        [Obsolete]
        public static JObject SUObsGetSourceFilter(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName, string filterName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSourceFilterData(sourceName, filterName, obsConnection, out JObject filterData))
            {
                return null;
            }

            return filterData;
        }

        // GET SOURCE FILTER LIST
        [Obsolete]
        public static JArray SUObsGetSourceFilterList(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSourceFilterList(sourceName, obsConnection, out JArray filterArray))
            {
                return null;
            }

            return filterArray;
        }

        // GET SCENE ITEM NAMES
        [Obsolete]
        public static void SUObsGetSceneItemNames(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType sceneType, string sceneName, List<string> sceneItemNames)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSceneItemsNamesList(sceneName, sceneType, obsConnection, out List<string> sceneItemsNamesList))
            {
                return;
            }

            sceneItemNames = sceneItemsNamesList;
        }

        // GET OUTPUT FILEPATH
        [Obsolete]
        public static string SUObsGetOutputFilePath(this IInlineInvokeProxy CPH, int obsConnection)
        {
            StreamUpLib sup = new StreamUpLib(CPH);
            if (!sup.GetObsOutputFilePath(obsConnection, out string filePath))
            {
                return null;
            }

            return filePath;
        }

        // GET SOURCE SETTINGS
        [Obsolete]
        public static JObject SUObsGetInputSettings(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSourceSettings(sourceName, obsConnection, out JObject sourceSettings))
            {
                return null;
            }

            return sourceSettings;
        }

        // GET SCENE ITEM LIST
        [Obsolete]
        public static JArray SUObsGetSceneItemList(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sceneName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSceneItemsArray(sceneName, OBSSceneType.Scene, obsConnection, out JArray sceneItemArray))
            {
                return null;
            }

            return sceneItemArray;
        }

        [Obsolete]
        // GET GROUP SCENE ITEM LIST
        public static JArray SUObsGetGroupSceneItemList(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string groupName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSceneItemsArray(groupName, OBSSceneType.Group, obsConnection, out JArray sceneItemArray))
            {
                return null;
            }

            return sceneItemArray;
        }

        // GET SOURCE SHOW TRANSITION
        [Obsolete]
        public static JObject SUObsGetShowTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName)
        {
            StreamUpLib sup = new StreamUpLib(CPH);
            if (!sup.GetObsSourceShowTransition(sceneName, sourceName, obsConnection, out JObject showTransition))
            {
                return null;
            }

            return showTransition;
        }

        // GET SOURCE HIDE TRANSITION
        [Obsolete]
        public static JObject SUObsGetHideTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName)
        {
            StreamUpLib sup = new StreamUpLib(CPH);
            if (!sup.GetObsSourceHideTransition(sceneName, sourceName, obsConnection, out JObject hideTransition))
            {
                return null;
            }

            return hideTransition;
        }

        // GET CANVAS SCALE FACTOR
        [Obsolete]
        public static double SUObsGetCanvasScaleFactor(this IInlineInvokeProxy CPH, string productNumber, int obsConnection)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsCanvasScaleFactor(obsConnection, out double scaleFactor))
            {
                return 0.0;
            }

            return scaleFactor;
        }

        // GET CURRENT DSK SCENE
        [Obsolete]
        public static string SUObsGetCurrentDSKScene(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string dskName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsCurrentDSKScene(dskName, obsConnection, out string sceneName))
            {
                return null;
            }

            return sceneName;
        }

        // GET SCENE ITEM ENABLED
        [Obsolete]
        public static bool? SUObsGetSceneItemEnabled(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSourceVisibility(parentSource, parentSourceType, childSource, obsConnection, out bool? sourceState))
            {
                return null;
            }

            return sourceState;
        }

        // GET MOVE FILTER DURATION
        [Obsolete]
        public static int SUObsGetMoveFilterDuration(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sourceName, string filterName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsMoveFilterDuration(sourceName, filterName, obsConnection, out int moveDuration))
            {
                return -1;
            }

            return moveDuration;
        }

        // GET SCENE ITEM TRANSFORM
        [Obsolete]
        public static JObject SUObsGetSceneItemTransform(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.GetObsSourceTransform(parentSource, childSource, obsConnection, out JObject sourceTransform))
            {
                return null;
            }

            return sourceTransform;
        }

        // SET SOURCE SHOW TRANSITION
        [Obsolete]
        public static void SUObsSetShowTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName, string transitionType, string transitionSettings, int transitionDuration)
        {
            StreamUpLib sup = new StreamUpLib(CPH);

            // Ensure transitionSettings is a valid JSON object, even if it's an empty string
            JObject transitionSettingsObj;
            if (string.IsNullOrWhiteSpace(transitionSettings))
            {
                transitionSettingsObj = new JObject();
            }
            else
            {
                try // Try and parse the JSON Object
                {
                    transitionSettingsObj = JObject.Parse(transitionSettings);
                }
                catch (Exception ex)
                {
                    sup.LogError($"Failed to parse transitionSettings: {ex.Message}");
                    return;
                }
            }

            if (!sup.SetObsSourceShowTransition(sceneName, sourceName, transitionType, transitionDuration, transitionSettingsObj, obsConnection))
            {
                sup.LogError("Unable to set show transition");
            }
        }

        // SET SOURCE HIDE TRANSITION
        [Obsolete]
        public static void SUObsSetHideTransition(this IInlineInvokeProxy CPH, int obsConnection, string sceneName, string sourceName, string transitionType, string transitionSettings, int transitionDuration)
        {
            StreamUpLib sup = new StreamUpLib(CPH);

            // Ensure transitionSettings is a valid JSON object, even if it's an empty string
            JObject transitionSettingsObj;
            if (string.IsNullOrWhiteSpace(transitionSettings))
            {
                transitionSettingsObj = new JObject();
            }
            else
            {
                try // Try and parse the JSON Object
                {
                    transitionSettingsObj = JObject.Parse(transitionSettings);
                }
                catch (Exception ex)
                {
                    sup.LogError($"Failed to parse transitionSettings: {ex.Message}");
                    return;
                }
            }

            if (!sup.SetObsSourceHideTransition(sceneName, sourceName, transitionType, transitionDuration, transitionSettingsObj, obsConnection))
            {
                sup.LogError("Unable to set hide transition");
            }
        }

        // SET CURRENT DSK SCENE
        [Obsolete]
        public static void SUObsSetCurrentDSKScene(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string dskName, string sceneName)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            sup.SetObsCurrentDSKScene(sceneName, dskName, obsConnection);
        }

        // SET SCENE ITEM ENABLED
        [Obsolete]
        public static void SUObsSetSceneItemEnabled(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource, bool visibilityState)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.SetObsSourceEnabled(parentSource, parentSourceType, childSource, visibilityState, obsConnection))
            {
                sup.LogError("Unable to set scene item visibility");
            }
        }

        // SET INPUT (SOURCE) VOLUME
        [Obsolete]
        public static void SUObsSetInputVolume(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string inputName, VolumeType volumeType, double volumeLevel)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.SetObsSourceVolume(inputName, volumeType, volumeLevel, obsConnection))
            {
                sup.LogError("Unable to set source volume");
            }
        }

        // SET SOURCE FILTER SETTINGS
        [Obsolete]
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
        [Obsolete]
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
        [Obsolete]
        public static void SUObsSetSceneSceneTransitionOverride(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, string sceneName, string transitionName, int transitionDuration)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            sup.SetObsSceneTransitionOverride(sceneName, transitionName, transitionDuration, obsConnection);
        }

        // SET SCENE ITEM TRANSFORM
        [Obsolete]
        public static void SUObsSetSceneItemTransform(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string parentSource, string childSource, string transformSettings)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);

            // Ensure transformSettings is a valid JSON object
            JObject transformSettingsObj;
            if (string.IsNullOrWhiteSpace(transformSettings))
            {
                transformSettingsObj = new JObject();
            }
            else
            {
                try
                {
                    transformSettingsObj = JObject.Parse(transformSettings);
                }
                catch (Exception ex)
                {
                    sup.LogError($"Failed to parse transformSettings: {ex.Message}");
                    return;
                }
            }

            if (!sup.SetObsSceneItemTransform(parentSource, childSource, transformSettingsObj, obsConnection))
            {
                sup.LogError("Unable to set scene item transform");
            }
            return;
        }

        // AUTOSIZE ADVANCED MASK
        [Obsolete]
        public static void SUObsAutosizeAdvancedMask(this IInlineInvokeProxy CPH, string productNumber, Dictionary<string, object> productSettings, string sourceName, string filterName, double sourceHeight, double sourceWidth, double padHeight, double padWidth)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);

            // Get scale factor
            if (!productSettings.TryGetValue("ScaleFactor", out object scaleFactorInput))
            {
                sup.LogError("Unable to retrieve ScaleFactor arg from productSettings dictionary");
                return;
            }
            double scaleFactor = double.Parse(scaleFactorInput.ToString(), CultureInfo.InvariantCulture);

            // Get OBS connection
            if (!productSettings.TryGetValue("ScaleFactor", out object obsConnectionInput))
            {
                sup.LogError("Unable to retrieve ScaleFactor arg from productSettings dictionary");
                return;
            }
            int obsConnection = int.Parse(obsConnectionInput.ToString());

            // Autosize advanced mask
            if (!sup.ObsAutoSizeAdvancedMask(sourceName, filterName, sourceHeight, sourceWidth, padHeight, padWidth, scaleFactor, obsConnection))
            {
                sup.LogError("Unable to autosize advanced mask");
                return;
            }

            return;
        }

        // AUTO POSITION ADVANCED MASK
        [Obsolete]
        public static void SUObsAutoposAdvancedMask(this IInlineInvokeProxy CPH, string productNumber, Dictionary<string, object> productSettings, string sourceName, string filterName, int padX, int padY)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);

            // Get scale factor
            if (!productSettings.TryGetValue("ScaleFactor", out object scaleFactorInput))
            {
                sup.LogError("Unable to retrieve ScaleFactor arg from productSettings dictionary");
                return;
            }
            double scaleFactor = double.Parse(scaleFactorInput.ToString(), CultureInfo.InvariantCulture);

            // Get OBS connection
            if (!productSettings.TryGetValue("ScaleFactor", out object obsConnectionInput))
            {
                sup.LogError("Unable to retrieve ScaleFactor arg from productSettings dictionary");
                return;
            }
            int obsConnection = int.Parse(obsConnectionInput.ToString());

            // Auto position advanced mask
            if (!sup.ObsAutoPositionAdvancedMask(sourceName, filterName, padY, padX, scaleFactor, obsConnection))
            {
                sup.LogError("Unable to autosize advanced mask");
                return;
            }

            return;
        }

        // REMOVE URL FROM STRING
        [Obsolete]
        public static string SURemoveUrls(this IInlineInvokeProxy CPH, string text, string replacementText, string productNumber = "DLL")
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            if (!sup.RemoveUrlFromString(text, replacementText, out string outputText))
            {
                sup.LogError("Unable to remove Url from string");
                return null;
            }

            return outputText;
        }

        // SPLIT TEXT ONTO MULTIPLE LINES FROM WIDTH
        [Obsolete]
        public static string SUObsSplitTextOnWidth(this IInlineInvokeProxy CPH, string productNumber, int obsConnection, OBSSceneType parentSourceType, string sceneName, string sourceName, string rawText, int maxWidth, int maxHeight)
        {
            StreamUpLib sup = new StreamUpLib(CPH, productNumber);
            string outputText = sup.ObsSplitTextOnWidth(productNumber, obsConnection, parentSourceType, sceneName, sourceName, rawText, maxWidth, maxHeight);


            return outputText;
        }
    }
}
