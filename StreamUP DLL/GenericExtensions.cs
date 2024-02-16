using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class GenericExtensions {

        public static string SUGetStreamerBotFolder(this IInlineInvokeProxy CPH) {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        
        public static void SUWriteLog(this IInlineInvokeProxy CPH, string logMessage, string productName = "General") {
            string sbFolder = CPH.SUGetStreamerBotFolder();
            string suFolder = Path.Combine(sbFolder, "StreamUP");
            string suLogFolder = Path.Combine(suFolder, "logs");

            if (!Directory.Exists(suFolder)) {
                Directory.CreateDirectory(suFolder);
            }

            if (!Directory.Exists(suLogFolder)) { 
                Directory.CreateDirectory(suLogFolder);
            }

            DateTime today = DateTime.Now;

            string todayFileName = $"{today.ToString("yyyyMMdd")} - StreamUP.log";

            string todayPath = Path.Combine(suLogFolder, todayFileName);

            if (!File.Exists(todayPath)) {
                using (FileStream fs = File.Create(todayPath)) {
                    byte[] info = new UTF8Encoding(true).GetBytes("Heyo duckies! New Day!");
                    fs.Write(info, 0, info.Length);
                }
            }

            using (StreamWriter file = new StreamWriter(todayPath, true)) {
                string formattedLogMessage = $"[{today.ToString("yyyy-MM-dd HH:mm:ss:fff")}] [{productName}] :: {logMessage}";
                file.Write($"\r\n{formattedLogMessage}");
            }
        }
   
        public static void SUSetProductObsVersion(this IInlineInvokeProxy CPH, string productName, int obsInstance, string inputName, string versionNumber)
        {
            // Load log string
            string logName = "GeneralExtensions-SUSetProductObsVersion";
            CPH.SUWriteLog("Method Started", logName);

            string inputSettings = $"\"product_version\": \"{versionNumber}\"";
            CPH.SUObsSetInputSettings("GeneralExtensions", obsInstance, inputName, inputSettings);
            CPH.SUWriteLog("Method complete", logName);
        }

        public static double SUGetObsCanvasScaleFactor(this IInlineInvokeProxy CPH, string productNumber, string productName, int obsInstance)
        {
            // Load log string
            string logName = "GeneralExtensions-SUGetObsCanvasScaleFactor";
            CPH.SUWriteLog("Method Started", logName);

            // Pull obs canvas width
            JObject videoSettings = CPH.SUObsGetVideoSettings(productName, obsInstance);
            double canvasWidth = (double)videoSettings["baseWidth"];
            CPH.SUWriteLog($"Pulled base canvas width from obs: canvasWidth=[{canvasWidth}]", logName);

            // Work out scale difference based on 1920x1080
            double canvasScaleFactor = (canvasWidth / 1920);
            CPH.SUWriteLog($"Worked out canvas scale factor: canvasScaleFactor=[{canvasScaleFactor}]", logName);

            // Save canvasScaleFactor to sb global var
            CPH.SetGlobalVar($"{productNumber}_CanvasScaleFactor", canvasScaleFactor); 
            CPH.SUWriteLog("Method completed", logName);
            return canvasScaleFactor;
        }
    }
}
