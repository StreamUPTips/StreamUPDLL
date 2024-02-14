using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class ValidationExtensions {

        public static bool SUInitialiseProduct(this IInlineInvokeProxy CPH, string productNumber, string productName, int obsInstance, string sceneName)
        {
            // Load log string
            string logName = $"{productName}-SUInitialiseProduct";
            CPH.SUWriteLog("Method Started", logName);

            // Check if obs is connected
            if (!CPH.SUCheckObsIsConnected(productNumber, productName, obsInstance))
            {
                CPH.SUWriteLog("OBS is not connected. Initialization aborted.", logName);
                return false; // Stop execution since OBS is not connected
            }
            // Check if products scene exists
            if (!CPH.SUCheckStreamUPSceneExists(productNumber, productName, sceneName, obsInstance))
            {
                CPH.SUWriteLog($"Scene '{sceneName}' does not exist. Initialization aborted.", logName);
                return false; // Stop execution since the scene does not exist
            }

            CPH.SetGlobalVar($"{productNumber}_Initialised", true, false);
            return true;
        }

        public static bool SUCheckObsIsConnected(this IInlineInvokeProxy CPH, string productNumber, string productName, int obsInstance)
        {
            // Load log string
            string logName = $"{productName}-SUCheckObsIsConnected";
            CPH.SUWriteLog("Method Started", logName);

            // Check obs instance is connected
            if (!CPH.ObsIsConnected(obsInstance))
            {
                var errorMessage = $"There is no OBS connection on connection number '{obsInstance}'.\n\n" +
                            "1. Check your OBS settings in the 'Stream Apps' tab.\n" +
                            $"2. Set the correct OBS number in the '{productName} • Settings' Action.\n";
                CPH.SUWriteLog($"ERROR: {errorMessage}", logName);
                var error = MessageBox.Show($"{errorMessage}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);           
                return false;
            }
            else
            {
                // Pull Obs websocket version number
                string versionNumberString = null;
                string obsData = CPH.ObsSendRaw("GetVersion", "{}", obsInstance);
                JObject obsDataJson = JObject.Parse(obsData);
                if (obsDataJson.TryGetValue("obsWebSocketVersion", out var versionToken))
                {
                    versionNumberString = versionToken.ToString();
                }
                // If version isn't v5 and above
                if (versionNumberString == null)
                {
                    var errorMessage = $"There is no OBS Websocket v5.0.0 or above connection on connection number '{obsInstance}'.\n\n" +
                                "1. Check your OBS settings in the 'Stream Apps' tab.\n";
                    CPH.SUWriteLog($"ERROR: {errorMessage}", logName);
                    var error = MessageBox.Show($"{errorMessage}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);           
                    return false;
                }
            }
            return true;
        }
        
        public static bool SUCheckStreamUPSceneExists(this IInlineInvokeProxy CPH, string productNumber, string productName, string sceneName, int obsInstance)
        {
            // Load log string
            string logName = $"{productName}-SUCheckStreamUPSceneExists";
            CPH.SUWriteLog("Method Started", logName);

            // Pull Obs scene list and see if sceneName exists
            var sceneList = CPH.ObsSendRaw("GetSceneList", "{}", obsInstance);
            if (!sceneList.Contains($"{sceneName}"))
            {
                var errorMessage = $"The scene '{sceneName}' does not exist in OBS.\n\n" +
                                    "Please reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: {errorMessage}", logName);
                var error = MessageBox.Show($"{errorMessage}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);           		
                return false;
            }
            return true;
        }
    
        public static bool SUCheckProductObsVersion(this IInlineInvokeProxy CPH, string productNumber, string productName, string targetVersion, int obsInstance, string sceneName)
        {
            // Load log string
            string logName = $"{productName}-SUCheckProductObsVersion";
            CPH.SUWriteLog("Method Started", logName);

            // Pull scenes filters
            JArray filters = CPH.SUObsGetSourceFilterList(productName, obsInstance, sceneName);

            // Check if filter names contain the word 'Version'
            string foundVersion = null;
            int versionFilterCount = 0;
            foreach (var filter in filters)
            {
                string filterName = filter["filterName"].ToString();
                if (filterName.Contains("Version"))
                {
                    versionFilterCount++;
                    if (versionFilterCount == 1)
                    {
                        foundVersion = filterName.Replace("Version ", "").Trim();
                    }
                }
            }

            if (versionFilterCount > 1)
            {
                string error1 = $"Multiple version numbers have been found in OBS for '{productName}'.";
                string error2 = $"Please remove the scene '{sceneName}' and reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                var error = MessageBox.Show($"{error1}\n\n{error2}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
                return false;
            }
            else if (versionFilterCount == 0)
            {
                string error1 = $"No version number has been found in OBS for'{productName}'.";
                string error2 = $"Please remove the scene '{sceneName}' and reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                var error = MessageBox.Show($"{error1}\n\n{error2}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
                return false;
            }

            // Check if installed version is up to date
            Version foundVer = new Version(foundVersion);
            Version targetVer = new Version(targetVersion);

            if (foundVer >= targetVer)
            {
                CPH.SUWriteLog($"Current version {foundVersion} is up to date with target version {targetVersion}.", logName);
                return true;
            }
            else
            {
                string error1 = $"Current version of '{productName}' in OBS is out of date.";
                string error2 = $"Please make sure you have downloaded the latest version from https://my.streamup.tips";
                string error3 = $"Then remove the scene '{sceneName}' and reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                var error = MessageBox.Show($"{error1}\n\n{error2}\n{error3}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
                return false;
            }
        }
    }
}
