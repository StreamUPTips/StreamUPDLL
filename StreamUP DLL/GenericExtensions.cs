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

    }
}
