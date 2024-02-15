using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class CheckObsPlugins {

        public static bool CheckWebsocketVersionCompatible(this IInlineInvokeProxy CPH, string versionNumberString)
        {
            // Load log string
            string logName = $"ValidationExtensions-CheckWebsocketVersionCompatible";
            CPH.SUWriteLog("Method Started", logName);

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
                        return true;
                    }
                }
            }
            return false;
        }
    
        public static (bool Success, string Message) FindOBSLogFile(this IInlineInvokeProxy CPH, int obsInstance)
        {
            // Load log string
            string logName = $"ValidationExtensions-FindOBSLogFile";
            CPH.SUWriteLog("Method Started", logName);

            string portableFolder = CPH.GetPortableOBS();
            if (!string.IsNullOrEmpty(portableFolder))
            {
                return CPH.OBSPluginVerification(portableFolder, obsInstance);
            }
            else
            {
                string appDataFolder = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData
                );
                string folderPath = Path.Combine(appDataFolder, "obs-studio", "logs");
                if (Directory.Exists(folderPath))
                {
                    return CPH.OBSPluginVerification(folderPath, obsInstance);
                }
                else
                {
                    return (
                        false,
                        "Cannot find OBS logs directory.\n\nMake sure OBS is running then try again."
                    );
                }
            }
        }

        public static string GetPortableOBS(this IInlineInvokeProxy CPH)
        {
            // Load log string
            string logName = $"ValidationExtensions-GetPortableOBS";
            CPH.SUWriteLog("Method Started", logName);

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
                        return replacedPath;
                    }
                    else
                    {
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

            return null;
        }

        public static (bool Success, string Message) OBSPluginVerification(this IInlineInvokeProxy CPH, string folderPath, int obsInstance)
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
                string error1 = "Permission denied accessing the OBS file directory";
                string error2 = "Please close Streamer.Bot and run it as Administrator.";
                CPH.SUWriteLog("Exception while accessing directory: " + e.Message, logName);
                return (false, $"{error1}\n\n{error2}");
            }

            string mostRecentFile = files
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();
            if (!File.Exists(mostRecentFile))
            {
                string error1 = "No log files found in the OBS logs directory.";
                CPH.SUWriteLog(error1, logName);
                return (false, error1);
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
                string error1 = "Permission denied accessing the OBS log file";
                string error2 = "Please close Streamer.Bot and run it as Administrator.";
                CPH.LogWarn("Exception while reading file: " + e.Message);
                return (false, $"{error1}\n\n{error2}");
            }

            if (fileContent.Contains("[StreamUP] loaded version"))
            {
                string versionPattern = $@"{Regex.Escape("[StreamUP] loaded version")} (\d+\.\d+\.\d+)";
                Match match = Regex.Match(fileContent, versionPattern);
                if (match.Success)
                {
                    string pluginVersion = match.Groups[1].Value;
                    CPH.LogInfo($"StreamUP plugin loaded correctly (version {pluginVersion}).");
                    switch (pluginVersion)
                    {
                        case string v when ObsIsVersionLessThan(v, "0.0.9"):
                            return (
                                false,
                                "Your StreamUP OBS plugin needs updating. Please download it with the StreamUP Pluginstaller: https://streamup.tips/product/plugin-installer"
                            );
                        case string v when ObsIsVersionInRange(v, "0.0.9", "1.1.3"):
                            return (
                                false,
                                "Please update your OBS plugins by going to 'Tools\\StreamUP\\Check for OBS Plugin Updates' in OBS."
                            );
                        case string v when ObsIsVersionGreaterOrEqualTo(v, "1.1.4"):
                            var json = CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"check_plugins\",\"requestData\":null}", obsInstance);
                            if (!HandleJSON(json))
                            {
                                return (
                                    false,
                                    "1 or more OBS plugins are missing or need updating.\n\nA window should appear in OBS showing you which need downloading.\n\nIf it doesn't appear you can manually click 'Tools\\StreamUP\\Check Product Requirements'"
                                );
                            }

                            return (true, "Everything is up to date in OBS.");
                    }
                }
            }

            return (false, "You are missing the StreamUP plugin in OBS. Please download it with the StreamUP Pluginstaller: https://streamup.tips/product/plugin-installer");
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
            dynamic response = (dynamic)json;
            if (response != null && response.requestType == "check_plugins")
            {
                return response.responseData?.success ?? false;
            }

            return false;
        }
    
    

    

    }
}
