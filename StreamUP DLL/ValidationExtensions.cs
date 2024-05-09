using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class ValidationExtensions {
        public static bool SUValSBUpdateChecker(this IInlineInvokeProxy CPH, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUValSBUpdateChecker";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Check if user has already been prompted for the update checker this launch
            if (CPH.GetGlobalVar<bool>("sup000_UpdateCheckerPrompted", false))
            {
                CPH.SUWriteLog("Update checker has already been prompted this launch.", logName);
                CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
                return true;
            } 

            // Check if Update checker action exists
            if (!CPH.ActionExists("StreamUP Tools • Update Checker"))
            {
                string error1 = "The StreamUP Update Checker for Streamer.Bot is not installed";
                string error2 = "You can download it from the StreamUP website.";
                string error3 = "Would you like to open the link now?";
                CPH.SUWriteLog(error1, logName);
                DialogResult errorOutput = CPH.SUUIShowWarningYesNoMessage($"{error1}\n{error2}\n\n{error3}");           
                
                if (errorOutput == DialogResult.Yes) {
                    Process.Start("https://streamup.tips/product/update-checker");
                }      
                CPH.SetGlobalVar("sup000_UpdateCheckerPrompted", true, false);
            }  
            else
            {
                CPH.SUWriteLog("StreamUP update checker for Streamer.Bot is installed.", logName);
            } 
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }
        
        public static ProductInfo SUValProductInfoLoaded(this IInlineInvokeProxy CPH,string actionName, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUValProductInfoLoaded";
            CPH.SUWriteLog("METHOD STARTED!", logName);
            CPH.SUWriteLog("Checking ProductInfo is loaded.", logName);

            // Check ProductInfo is already loaded, if not, load it
            ProductInfo productInfo;
            string productInfoJson = CPH.GetGlobalVar<string>($"{productNumber}_ProductInfo", false);

            if (string.IsNullOrEmpty(productInfoJson))
            {
                if (!CPH.SULoadProductInfo(actionName))
                {
                    CPH.SUWriteLog("Couldn't load ProductInfo", logName);
                    CPH.SUWriteLog("METHOD FAILED!", logName);
                    return null;
                }
            }
            else
            {
                CPH.SUWriteLog("ProductInfo already loaded.", logName);
            }

            productInfo = JsonConvert.DeserializeObject<ProductInfo>(CPH.GetGlobalVar<string>($"{productNumber}_ProductInfo", false));
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return productInfo;
        }

        public static bool SUValProductSettingsLoaded(this IInlineInvokeProxy CPH, ProductInfo productInfo)
        {
            string logName = $"{productInfo.ProductNumber}::SUValProductSettingsLoaded";
            CPH.SUWriteLog("METHOD STARTED!", logName);
            CPH.SUWriteLog("Checking ProductSettings is loaded.", logName);

            string productSettingsJson = CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_ProductSettings", true);
            if (string.IsNullOrEmpty(productSettingsJson))
            {
                CPH.SUWriteLog($"Product settings for '{productInfo.ProductName}' are not loaded. Showing Dialog option for user to open settings menu", logName);
                var errorOutput = CPH.SUUIShowWarningYesNoMessage($"There are no product settings loaded for '{productInfo.ProductName}'\n\nWould you like to open the products settings menu now?");
                if (errorOutput == DialogResult.Yes)
                {
                    CPH.SUWriteLog($"User selected 'Yes'. Opening settings menu...", logName);
                    CPH.RunAction(productInfo.SettingsAction, false);
                }
                else
                {
                    CPH.SUWriteLog($"User selected 'No'.", logName);
                }
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;   
            }
            else
            {
                CPH.SUWriteLog("ProductSettings already loaded.", logName);
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static bool SUValLibraryVersion(this IInlineInvokeProxy CPH, Version minimumRequiredVersion, string productNumber = "DLL")
        {
            string logName = $"{productNumber}::SUValLibraryVersion";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            CPH.SUWriteLog($"Pulled library version. currentVersion=[{currentVersion}], minimumRequiredVersion=[{minimumRequiredVersion}]", logName);

            if (currentVersion < minimumRequiredVersion)
            {       
                CPH.SUWriteLog("StreamUP.dll file is not the required version. Prompting the user to download the latest.", logName);
                CPH.SUUIShowErrorOKMessage("You do not have the required version of 'StreamUP.dll' installed.\nPlease download it via the 'StreamUP_Library_Updater.exe' that was bundled in with this product.\n\nYou should have placed it in the root of your Streamer.Bot folder.");
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            CPH.SUWriteLog("The current version of the StreamUP.dll file is okay.", logName);          
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }
    
        public static bool SUValObsIsConnected(this IInlineInvokeProxy CPH, ProductInfo productInfo, int obsConnection)
        {
            string logName = $"{productInfo.ProductNumber}::SUValObsIsConnected";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Check obs connection is connected
            if (!CPH.ObsIsConnected(obsConnection))
            {
                var errorMessage = $"There is no OBS connection on connection number '{obsConnection}'.\n\n" +
                            "1. Check your OBS settings in the 'Stream Apps' tab.\n" +
                            $"2. Set the correct OBS number in the '{productInfo.SettingsAction}' Action.\n";
                CPH.SUWriteLog($"ERROR: There is no OBS connection on connection number '{obsConnection}'.", logName);
                CPH.SUUIShowErrorOKMessage(errorMessage);
                CPH.SUWriteLog("METHOD FAILED", logName);
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
                    CPH.SUWriteLog($"ERROR: There is no OBS Websocket v5.0.0 or above connection on connection number '{obsConnection}'.", logName);
                    CPH.SUUIShowErrorOKMessage(errorMessage);
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return false;
                }
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static bool SUValObsPlugins(this IInlineInvokeProxy CPH, ProductInfo productInfo, int obsConnection)
        {
            string logName = $"{productInfo.ProductNumber}::SUValObsPlugins";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string checkPlugins = CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"check_plugins\",\"requestData\":null}", obsConnection);
            if (checkPlugins.Trim() == "{}")
            {
                CPH.SUWriteLog("Cannot check OBS plugins. The StreamUP OBS plugin may not be installed or is out of date.", logName);
                DialogResult response = CPH.SUUIShowWarningYesNoMessage("Cannot check OBS plugins. The StreamUP OBS plugin may not be installed or is out of date.\n\nWould you like to go to the download page now?");       
                if (response == DialogResult.Yes)
                {
                    CPH.SUWriteLog("User selected 'Yes'. Opening StreamUP website in web browser.", logName);
                    Process.Start("https://streamup.tips/plugin");
                }

                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            bool? pluginReminder = CPH.GetGlobalVar<bool?>("sup000_ObsPluginReminder", false);
            if (pluginReminder == false)
            {
                CPH.SUWriteLog("User has requested not to be reminded about OBS plugin updates this session", logName);
                CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
                return true;
            }

            JObject checkPluginsObj = JObject.Parse(checkPlugins);
            bool isSuccess = checkPluginsObj["responseData"]?["success"]?.Value<bool>() ?? false;
            if (!isSuccess)
            {
                CPH.SUWriteLog($"OBS has plugins that are required that are out of date.", logName);   

                var (response, askAgain) = CPH.SUUIShowObsPluginsUpdateMessage();      

                if (response == DialogResult.Yes)
                {
                    CPH.SUWriteLog("User selected 'Yes'. Opening StreamUP website in web browser.", logName);
                    Process.Start("https://streamup.tips/product/plugin-installer");
                }

                CPH.SetGlobalVar("sup000_ObsPluginReminder", askAgain, false);

                if (askAgain)
                {
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return false;
                }
            }

            CPH.SUWriteLog("All plugins are loaded and up to date.", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static bool SUValStreamUPSceneExists(this IInlineInvokeProxy CPH, ProductInfo productInfo, int obsConnection)
        {
            string logName = $"{productInfo.ProductNumber}::SUValStreamUPSceneExists";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull Obs scene list and see if sceneName exists
            var sceneList = CPH.ObsSendRaw("GetSceneList", "{}", obsConnection);
            if (!sceneList.Contains($"{productInfo.SceneName}"))
            {
                string errorMessage = $"The scene '{productInfo.SceneName}' does not exist in OBS.\n\n" +
                                    "Please reinstall it into OBS using the products '.StreamUP' file.";
                CPH.SUWriteLog($"ERROR: The scene '{productInfo.SceneName}' does not exist in OBS.", logName);
                CPH.SUUIShowErrorOKMessage(errorMessage);

                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            CPH.SUWriteLog($"The scene '{productInfo.SceneName}' exists in Obs.", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }

        public static bool SUValProductObsVersion(this IInlineInvokeProxy CPH,  ProductInfo productInfo, int obsConnection)
        {
            string logName = $"{productInfo.ProductNumber}::SUValProductObsVersion";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Pull product version from source settings
            JObject inputSettings = CPH.SUObsGetInputSettings(productInfo.ProductNumber, obsConnection, productInfo.SourceNameVersionCheck);
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
                string error1 = $"No version number has been found in OBS for '{productInfo.ProductName}'.";
                string error2 = $"Please install the latest .StreamUP version for '{productInfo.ProductName}' into OBS.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                CPH.SUUIShowErrorOKMessage($"{error1}\n\n{error2}");

                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            // Check if installed version is up to date
            Version foundVer = new Version(foundVersion);
            Version targetVer = productInfo.SourceNameVersionNumber;

            if (foundVer < targetVer)
            {
                string error1 = $"Current version of '{productInfo.ProductName}' in OBS is out of date.";
                string error2 = $"Please make sure you have downloaded the latest version from https://my.streamup.tips";
                string error3 = $"Then install the latest .StreamUP version for '{productInfo.ProductName}' into OBS.";
                CPH.SUWriteLog($"ERROR: {error1}", logName);
                CPH.SUUIShowErrorOKMessage($"{error1}\n\n{error2}\n{error3}");
                
                CPH.SUWriteLog("METHOD FAILED", logName);
                return false;
            }

            CPH.SUWriteLog($"Current version {foundVersion} is up to date with target version {targetVer}.", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }
    
    public static bool SUValFontInstalled(this IInlineInvokeProxy CPH, List<(string fontName, string fontFile, string fontUrl)> requiredFonts, string productNumber = "DLL")
    {
        string logName = $"{productNumber}::SUValFontInstalled";
        CPH.SUWriteLog("METHOD STARTED!", logName);

        bool allFontsInstalled = true;

        foreach (var font in requiredFonts)
        {
            string fontName = font.fontName;
            string fontFile = font.fontFile;
            string fontUrl = font.fontUrl;

            // Get the list of installed font families
            FontFamily[] installedFonts = FontFamily.Families;

            if (!installedFonts.Any(ff => ff.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase)))
            {
                CPH.SUWriteLog($"WARNING: Unable to locate '{fontName}' in the font family list.", logName);
                allFontsInstalled = false;
                CPH.SUWriteLog($"WARNING: The font '{fontFile}' is not installed. The product may not function properly.", logName);
                DialogResult result = CPH.SUUIShowWarningYesNoMessage($"The font '{fontFile}' is not installed. The product may not function properly.\n\nWould you like to go to the fonts download page now? Once installed make sure to restart OBS.");
                if (result == DialogResult.Yes)
                {
                    Process.Start(fontUrl);
                }
                continue; // Skip further checks and move to the next font in the list
            }

            // Check for specific font file extension type
            string fontFileName = Path.GetFileNameWithoutExtension(fontFile);
            string searchPattern = $"{fontFileName}.*";

            // Check default font folder
            string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            CPH.SUWriteLog($"Searching for '{fontFile}' in: [{directoryPath}]", logName);
            List<string> baseDirMatchingFontFiles = GetAllMatchingFontFiles(CPH, directoryPath, searchPattern, productNumber);

            if (!CheckIfFontMatches(CPH, baseDirMatchingFontFiles, fontFile, productNumber))
            {
                // Check APPDATA font folder
                string appdataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                directoryPath = Path.Combine(appdataFolder, "Microsoft", "Windows", "Fonts");
                CPH.SUWriteLog($"Searching for '{fontFile}' in: [{directoryPath}]", logName);

                List<string> appdataMatchingFontFiles = GetAllMatchingFontFiles(CPH, directoryPath, searchPattern, productNumber);
                if (!CheckIfFontMatches(CPH, appdataMatchingFontFiles, fontFile, productNumber))
                {
                    allFontsInstalled = false;
                    CPH.SUWriteLog($"WARNING: The font '{fontFile}' is not installed. The product may not function properly.", logName);
                    DialogResult result = CPH.SUUIShowWarningYesNoMessage($"The font '{fontFile}' is not installed. The product may not function properly.\n\nWould you like to go to the fonts download page now? Once installed make sure to restart OBS.");
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(fontUrl);
                    }
                }
            }
        }

        if (allFontsInstalled)
        {
            CPH.SUWriteLog("All required fonts are installed.", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return true;
        }
        else
        {
            CPH.SUWriteLog("One or more required fonts are not installed.", logName);
            CPH.SUWriteLog("METHOD FAILED", logName);
            return false;
        }
    }

    private static List<string> GetAllMatchingFontFiles(this IInlineInvokeProxy CPH, string directoryPath, string searchPattern, string productNumber)
    {
        string logName = $"{productNumber}::GetAllMatchingFontFiles";
        CPH.SUWriteLog("METHOD STARTED!", logName);

        string[] filePaths = Directory.GetFiles(directoryPath, searchPattern);
        List<string> fileNames = new List<string>();
        foreach (var filePath in filePaths)
        {
            string fileName = Path.GetFileName(filePath);
            fileNames.Add(fileName);
            CPH.SUWriteLog($"Added '{fileName}' to list", logName);
        }
        CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        return fileNames;
    }

    private static bool CheckIfFontMatches(this IInlineInvokeProxy CPH, List<string> fontFiles, string fontName, string productNumber)
    {
        string logName = $"{productNumber}::CheckIfFontMatches";
        CPH.SUWriteLog("METHOD STARTED!", logName);

        foreach (var file in fontFiles)
        {
            if (file.Equals(fontName, StringComparison.OrdinalIgnoreCase))
            {
                CPH.SUWriteLog($"Font match found: {file}", logName);
                return true;
            }
        }
        CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        return false;
    }    
}


    [Serializable]
    public class ProductInfo
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductNumber { get; set; } = string.Empty;
        public Version RequiredLibraryVersion { get; set; } = new Version(0, 0, 0, 0);
        public string SceneName { get; set; } = string.Empty;
        public string SettingsAction { get; set; } = string.Empty;
        public string SourceNameVersionCheck { get; set; } = string.Empty;
        public Version SourceNameVersionNumber { get; set; } = new Version(0, 0, 0, 0);
    }
}
