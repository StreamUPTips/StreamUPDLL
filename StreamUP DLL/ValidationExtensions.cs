using System;
using System.Diagnostics;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class ValidationExtensions {

        public static bool SUInitialiseProduct(this IInlineInvokeProxy CPH, string productNumber, string productName, string sceneName, string settingsAction)
        {
            // Load log string
            string logName = $"{productName}-SUInitialiseProduct";
            CPH.SUWriteLog("Method Started", logName);

            if (CPH.GetGlobalVar<bool>($"{productNumber}_Initialised", false))
            {
                CPH.SUWriteLog($"{productName} is already initialised. Skipping checks.", logName);
                CPH.SUWriteLog("Method complete", logName);
                return true;
            }

            // Check product settings have been run
            if (!CPH.SUCheckProductSettingsLoaded(productNumber, productName, settingsAction))
            {
                CPH.SUWriteLog("Settings action has not been run. Initialisation aborted.", logName);
                return false; // Stop execution since OBS is not connected
            }     

            // Check if obs is connected
            int obsConnection = CPH.GetGlobalVar<int>($"{productNumber}_ObsConnection", true);
            if (!CPH.SUCheckObsIsConnected(productNumber, productName, obsConnection))
            {
                CPH.SUWriteLog("OBS is not connected. Initialisation aborted.", logName);
                return false; // Stop execution since OBS is not connected
            }
            // Check if products scene exists
            if (!CPH.SUCheckStreamUPSceneExists(productNumber, productName, sceneName, obsConnection))
            {
                CPH.SUWriteLog($"Scene '{sceneName}' does not exist. Initialisation aborted.", logName);
                return false; // Stop execution since the scene does not exist
            }

            CPH.SetGlobalVar($"{productNumber}_Initialised", true, false);
            CPH.SUWriteLog($"Method complete", logName);
            return true;
        }

        public static bool SUCheckProductSettingsLoaded(this IInlineInvokeProxy CPH, string productNumber, string productName, string settingsAction)
        {
            // Load log string
            string logName = $"{productName}-SUCheckProductSettingsLoaded";
            CPH.SUWriteLog("Method Started", logName);
            if (CPH.GetGlobalVar<string>($"{productNumber}_ObsConnection", true) == null)
            {
                string error1 = $"There are no {productName} settings found.";
                string error2 = $"Please run the '{productName} • Settings' Action first.";
                string error3 = "Press 'Yes' to open these settings automatically.";

                CPH.SUWriteLog(error1, logName);
                var errorOutput = MessageBox.Show($"{error1}\n\n{error2}\n\n{error3}", $"StreamUP • {productName} Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (errorOutput == DialogResult.Yes)
                {
                    CPH.RunAction(settingsAction, false);
                }
                CPH.SUWriteLog($"Method complete", logName);
                return false;    
            }
            CPH.SUWriteLog($"Method complete", logName);
            return true;
        }
        
        public static bool SUCheckObsIsConnected(this IInlineInvokeProxy CPH, string productNumber, string productName, int obsConnection)
        {
            // Load log string
            string logName = $"{productName}-SUCheckObsIsConnected";
            CPH.SUWriteLog("Method Started", logName);

            // Check obs connection is connected
            if (!CPH.ObsIsConnected(obsConnection))
            {
                var errorMessage = $"There is no OBS connection on connection number '{obsConnection}'.\n\n" +
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
                string obsData = CPH.ObsSendRaw("GetVersion", "{}", obsConnection);
                JObject obsDataJson = JObject.Parse(obsData);
                if (obsDataJson.TryGetValue("obsWebSocketVersion", out var versionToken))
                {
                    versionNumberString = versionToken.ToString();
                }
                // If version isn't v5 and above
                if (versionNumberString == null)
                {
                    var errorMessage = $"There is no OBS Websocket v5.0.0 or above connection on connection number '{obsConnection}'.\n\n" +
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
        
        public static bool SUCheckStreamUPSceneExists(this IInlineInvokeProxy CPH, string productNumber, string productName, string sceneName, int obsConnection)
        {
            // Load log string
            string logName = $"{productName}-SUCheckStreamUPSceneExists";
            CPH.SUWriteLog("Method Started", logName);

            // Pull Obs scene list and see if sceneName exists
            var sceneList = CPH.ObsSendRaw("GetSceneList", "{}", obsConnection);
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
    
        public static bool SUGetProductObsVersion(this IInlineInvokeProxy CPH, string productNumber, string productName, string targetVersion, int obsConnection, string sceneName, string sourceName)
        {
            // Load log string
            string logName = $"{productName}-SUGetProductObsVersion";
            CPH.SUWriteLog("Method Started", logName);

            // Pull product version from source settings
            JObject inputSettings = CPH.SUObsGetInputSettings(productName, obsConnection, sourceName);
            CPH.SUWriteLog($"Pulled inputSettings: inputSettings=[{inputSettings.ToString()}]", logName);

            // Check if filter names contain the word 'Version'
            string foundVersion = null;
            JToken versionToken;
            if (inputSettings.TryGetValue("product_version", out versionToken))
            {
                foundVersion = versionToken.ToString();
                CPH.SUWriteLog($"Found product version: {foundVersion}", logName);
            }          

            if (foundVersion == null)
            {
                string error1 = $"No version number has been found in OBS for '{productName}'.";
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
    
        public static bool SUCheckObsPlugins(this IInlineInvokeProxy CPH)
        {
            // Load log string
            string logName = $"ValidationExtensions-SUCheckObsPlugins";
            CPH.SUWriteLog("Method Started", logName);

            // Search for obs connection
            int obsConnection = 0;
	        string versionNumberString = null;
            while (obsConnection <= 20)
            {
                if (CPH.ObsIsConnected(obsConnection))
                {
                    CPH.SUWriteLog($"Obs connection found connected on connection [{obsConnection}]", logName);

                    // Pull obs version data
                    string obsData = CPH.ObsSendRaw("GetVersion", "{}", obsConnection);
                    JObject obsDataJson = JsonConvert.DeserializeObject<JObject>(obsData);                
                    CPH.SUWriteLog($"Pulled Obs GetVersion data: GetVersion=[{obsDataJson.ToString()}]", logName);

                    // Check websocket version 4.0 or lower, 5.0+
                    if (obsDataJson.TryGetValue("obsWebSocketVersion", out var versionToken) ||
                        obsDataJson.TryGetValue("obs-websocket-version", out versionToken))
                    {
                        CPH.SUWriteLog($"Pulled websocket version string: versionNumberString=[{versionToken.ToString()}]", logName);
                        versionNumberString = versionToken.ToString();
                    }                
                    if (CPH.CheckWebsocketVersionCompatible(versionNumberString))
                    {
                        break;
                    }
                }
                // Increase obs connection number
                obsConnection++;
            }

            if (!CPH.ObsIsConnected(obsConnection))
            {
                if (versionNumberString == null)
                {
                    string error1 = "OBS is not connected to Streamer.Bot";
                    string error2 = "Visit the 'Stream Apps' tab in Streamer.Bot and connect OBS via Websocket 5.0.0 or above.";
                    CPH.SUWriteLog(error1, logName);
                    CPH.SUShowErrorMessage($"{error1}\n\n{error2}");
                }
                else
                {
                    string error1 = $"Your OBS is connected to Streamer.Bot via websocket {versionNumberString}";
                    string error2 = "Visit the 'Stream Apps' tab in Streamer.Bot and connect OBS via Websocket 5.0.0 or above.";
                    CPH.SUWriteLog(error1, logName);
                    CPH.SUShowErrorMessage($"{error1}\n\n{error2}");
                }
                CPH.SUWriteLog($"Method complete", logName);          
                return false;
            }

            // Search for obs log file
            CPH.SUWriteLog("Beginning search for OBS log file", logName);
            var obsPluginResult = CPH.FindOBSLogFile(obsConnection);
            if (!obsPluginResult.Success)
            {
                CPH.SUShowErrorMessage(obsPluginResult.Message);
                CPH.SUWriteLog($"Method complete", logName);          
                return false;
            }
            string error3 = "StreamUP plugin is installed and loaded correctly";   
            string error4 = "Initiating StreamUP product settings menu..."; 
            CPH.SUWriteLog($"{error3} {error4}", logName);

            // Check if StreamerBot product update checker is installed
            CPH.SUCheckForSBUpdateChecker();
            CPH.SUWriteLog($"Method complete", logName);          
            return true;
        }

        public static bool SUCheckForSBUpdateChecker(this IInlineInvokeProxy CPH)
        {
            // Load log string
            string logName = $"ValidationExtensions-SUCheckForSBUpdateChecker";
            CPH.SUWriteLog("Method Started", logName);

            // Check if Update checker action exists
            if (!CPH.ActionExists("StreamUP Tools • Update Checker"))
            {
                string error1 = "The StreamUP Update Checker for Streamer.Bot is notinstalled";
                string error2 = "You can download it from the StreamUP website.";
                string error3 = "Would you like to open the link now?";
                CPH.SUWriteLog(error1, logName);
                DialogResult errorOutput = CPH.SUShowYesNoWarningMessage($"{error1}\n{error2}\n\n{error3}");           
                
                if (errorOutput == DialogResult.Yes) {
                    Process.Start("https://streamup.tips/product/update-checker");
                }      
            }   
            CPH.SUWriteLog($"Method complete", logName);          
            return true;
        }
    }

}
