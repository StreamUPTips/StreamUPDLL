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
            CPH.SUWriteLog($"Method complete", logName);
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
                CPH.SUWriteLog($"Method complete", logName);          
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
                    CPH.SUWriteLog($"Method complete", logName);          
                    return false;
                }
            }
            CPH.SUWriteLog($"Method complete", logName);          
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
                CPH.SUWriteLog($"Method complete", logName);          
                return false;
            }
            CPH.SUWriteLog($"Method complete", logName);          
            return true;
        }
    
        public static bool SUGetProductObsVersion(this IInlineInvokeProxy CPH, string productNumber, string productName, string targetVersion, int obsInstance, string sceneName, string sourceName)
        {
            // Load log string
            string logName = $"{productName}-SUGetProductObsVersion";
            CPH.SUWriteLog("Method Started", logName);

            // Pull product version from source settings
            JObject filters = CPH.SUObsGetInputSettings(productName, obsInstance, sourceName);

            // Check if filter names contain the word 'Version'
            string foundVersion = null;
            JToken versionToken;
            if (filters.TryGetValue("product_version", out versionToken))
            {
                foundVersion = versionToken.ToString();
                CPH.SUWriteLog($"Found product version: {foundVersion}", logName);
            }          

            if (foundVersion == null)
            {
                string error1 = $"No version number has been found in OBS for'{productName}'.";
                string error2 = $"Please remove the scene '{sceneName}' and reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                var error = MessageBox.Show($"{error1}\n\n{error2}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
                CPH.SUWriteLog($"Method complete", logName);          
                return false;
            }

            // Check if installed version is up to date
            Version foundVer = new Version(foundVersion);
            Version targetVer = new Version(targetVersion);

            if (foundVer >= targetVer)
            {
                CPH.SUWriteLog($"Current version {foundVersion} is up to date with target version {targetVersion}.", logName);
                CPH.SUWriteLog($"Method complete", logName);          
                return true;
            }
            else
            {
                string error1 = $"Current version of '{productName}' in OBS is out of date.";
                string error2 = $"Please make sure you have downloaded the latest version from https://my.streamup.tips";
                string error3 = $"Then remove the scene '{sceneName}' and reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                var error = MessageBox.Show($"{error1}\n\n{error2}\n{error3}", $"StreamUP • {productName} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
                CPH.SUWriteLog($"Method complete", logName);          
                return false;
            }
        }
    }
}
