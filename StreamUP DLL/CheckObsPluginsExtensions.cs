using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class CheckObsPluginsExtensions {

        public static bool CheckWebsocketVersionCompatible(this IInlineInvokeProxy CPH, string versionNumberString)
        {
            // Load log string
            string logName = $"ValidationExtensions-CheckWebsocketVersionCompatible";
            CPH.SUWriteLog("Method Started", logName);

            CPH.SUWriteLog("Deconstructing versionNumberString", logName);
            string[] parts = versionNumberString.Split('.');
            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[0], out int major) &&
                    int.TryParse(parts[1], out int minor) &&
                    int.TryParse(parts[2], out int patch))
                {
                    if (major > 5 ||
                    (major == 5 && minor > 0) ||
                    (major == 5 && minor == 0 && patch >= 0))
                    {
                        CPH.SUWriteLog($"Websocket Version is compatible: major=[{major}], minor=[{minor}], patch=[{patch}]", logName);
                        CPH.SUWriteLog($"Method complete", logName);          
                        return true;
                    }
                }
            }
            CPH.SUWriteLog($"Websocket Version is incompatible: versionNumberString=[{versionNumberString}]", logName);
            CPH.SUWriteLog($"Method complete", logName);          
            return false;
        }
    
        public static (bool Success, string Message) FindOBSLogFile(this IInlineInvokeProxy CPH, int obsConnection)
        {
            // Load log string
            string logName = $"ValidationExtensions-FindOBSLogFile";
            CPH.SUWriteLog("Method Started", logName);

            CPH.SUWriteLog("Searching for portable obs instance", logName);
            string portableFolder = CPH.GetPortableOBS();

            if (!string.IsNullOrEmpty(portableFolder))
            {
                CPH.SUWriteLog($"Portable instance found: portableFolder[{portableFolder}]", logName);
                CPH.SUWriteLog($"Method complete", logName);          
                return CPH.OBSPluginVerification(portableFolder, obsConnection);
            }
            else
            {
                CPH.SUWriteLog($"Portable instance not found. Searching for main obs installed", logName);
                string appDataFolder = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData
                );
                string folderPath = Path.Combine(appDataFolder, "obs-studio", "logs");
                if (Directory.Exists(folderPath))
                {
                    CPH.SUWriteLog($"Obs install found: folderPath=[{folderPath}]", logName);
                    CPH.SUWriteLog($"Method complete", logName);          
                    return CPH.OBSPluginVerification(folderPath, obsConnection);
                }
                else
                {
                    string error1 = "Cannot find OBS logs directory.";
                    string error2 = "Make sure OBS is running then try again.";
                    CPH.SUWriteLog(error1, logName);          
                    CPH.SUWriteLog($"Method complete", logName);          
                    return (false, $"{error1}\n\n{error2}");
                }
            }
        }

        public static string GetPortableOBS(this IInlineInvokeProxy CPH)
        {
            // Load log string
            string logName = $"ValidationExtensions-GetPortableOBS";
            CPH.SUWriteLog("Method Started", logName);

            // Search for obs process running
            Process process = Process.GetProcessesByName("obs64").FirstOrDefault();
            if (process != null)
            {
                string error1;
                string error2;
                try
                {
                    string obsFilePath = process.MainModule.FileName;
                    string replacedPath = Path.Combine(
                        obsFilePath.Replace(@"\bin\64bit\obs64.exe", @"\config\obs-studio\logs")
                    );
                    if (File.Exists(replacedPath) || Directory.Exists(replacedPath))
                    {
                        CPH.SUWriteLog($"Method complete", logName);          
                        return replacedPath;
                    }
                    else
                    {
                        CPH.SUWriteLog($"Method complete", logName);          
                        return null;
                    }
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    error1 = "Permission denied accessing the OBS process";
                    error2 = "Please close Streamer.Bot and run it as Administrator.";
                    string error3 = e.Message;
                    CPH.SUWriteLog($"{error1} - {error3}", logName);
                }

                CPH.SUShowErrorMessage($"{error1}\n\n{error2}");
            }

            CPH.SUWriteLog($"Method complete", logName);          
            return null;
        }

        public static (bool Success, string Message) OBSPluginVerification(this IInlineInvokeProxy CPH, string folderPath, int obsConnection)
        {
            // Load log string
            string logName = $"ValidationExtensions-OBSPluginVerification";
            CPH.SUWriteLog("Method Started", logName);

            string[] files;
            try
            {
                files = Directory.GetFiles(folderPath);
            }
            catch (Exception e)
            {
                string e6 = "Permission denied accessing the OBS file directory";
                string e7 = "Please close Streamer.Bot and run it as Administrator.";
                CPH.SUWriteLog("Exception while accessing directory: " + e.Message, logName);
                CPH.SUWriteLog($"Method complete", logName);          
                return (false, $"{e6}\n\n{e7}");
            }

            string mostRecentFile = files
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();
            if (!File.Exists(mostRecentFile))
            {
                string e5 = "No log files found in the OBS logs directory.";
                CPH.SUWriteLog(e5, logName);
                CPH.SUWriteLog($"Method complete", logName);          
                return (false, e5);
            }

            string fileContent;
            try
            {
                using (
                    FileStream fs = new FileStream(
                        mostRecentFile,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite
                    )
                )
                using (StreamReader reader = new StreamReader(fs))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                string e3 = "Permission denied accessing the OBS log file";
                string e4 = "Please close Streamer.Bot and run it as Administrator.";
                CPH.SUWriteLog("Exception while reading file: " + e.Message, logName);          
                CPH.SUWriteLog($"Method complete", logName);          
                return (false, $"{e3}\n\n{e4}");
            }

            if (fileContent.Contains("[StreamUP] loaded version"))
            {
                string versionPattern = $@"{Regex.Escape("[StreamUP] loaded version")} (\d+\.\d+\.\d+)";
                Match match = Regex.Match(fileContent, versionPattern);
                if (match.Success)
                {
                    string pluginVersion = match.Groups[1].Value;
                    CPH.SUWriteLog($"StreamUP plugin loaded correctly (version {pluginVersion}).", logName);          
                    switch (pluginVersion)
                    {
                        case string v when ObsIsVersionLessThan(v, "0.0.9"):
                            string e1 = "StreamUP OBS plugin needs updating.";
                            string e2 = "Please download it with the StreamUP Pluginstaller: https://streamup.tips/product/plugin-installer";
                            CPH.SUWriteLog(e1, logName);          
                            CPH.SUWriteLog($"Method complete", logName);          
                            return (false, $"{e1}\n\n{e2}");

                        case string v when ObsIsVersionInRange(v, "0.0.9", "1.1.3"):
                            string error3 = "Please update your OBS plugins.";
                            string error4 = "Go to 'Tools\\StreamUP\\Check for OBS Plugin Updates' in OBS.";
                            CPH.SUWriteLog(error3, logName);          
                            CPH.SUWriteLog($"Method complete", logName);          
                            return (false, $"{error3}\n\n{error4}");

                        case string v when ObsIsVersionGreaterOrEqualTo(v, "1.1.4"):
                            var json = CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"check_plugins\",\"requestData\":null}", obsConnection);
                            if (!HandleJSON(json))
                            {
                                string error5 = "1 or more OBS plugins are missing or need updating.";
                                string error6 = "A window should appear in OBS showing you which need downloading.";
                                string error7 = "If it doesn't appear you can manually click 'Tools\\StreamUP\\Check Product Requirements'";
                                CPH.SUWriteLog(error5, logName);          
                                CPH.SUWriteLog($"Method complete", logName);          
                                return (false, $"{error5}\n\n{error6}\n\n{error7}");
                            }
                        CPH.SUWriteLog("All Obs plugins are up to date.", logName);          
                        CPH.SUWriteLog($"Method complete", logName);          
                        return (true, "Everything is up to date in OBS.");
                    }
                }
            }

            string error1 = "You are missing the StreamUP plugin in OBS.";
            string error2 = "Please download it with the StreamUP Pluginstaller: https://streamup.tips/product/plugin-installer";
            CPH.SUWriteLog(error1, logName);          
            CPH.SUWriteLog($"Method complete", logName);          
            return (false, $"{error1}\n\n{error2}");
        }

        public static bool ObsIsVersionLessThan(string version1, string version2)
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1 < v2;
        }

        public static bool ObsIsVersionInRange(string version, string minVersion, string maxVersion)
        {
            Version v = new Version(version);
            Version vMin = new Version(minVersion);
            Version vMax = new Version(maxVersion);
            return v >= vMin && v < vMax;
        }

        public static bool ObsIsVersionGreaterOrEqualTo(string version1, string version2)
        {
            Version v1 = new Version(version1);
            Version v2 = new Version(version2);
            return v1 >= v2;
        }
    
        public static bool HandleJSON(string json)
        {
            dynamic response = JsonConvert.DeserializeObject<dynamic>(json);
            if (response != null && response.requestType == "check_plugins")
            {
                return response.responseData?.success ?? false;
            }

            return false;
        }
    
    }
}
