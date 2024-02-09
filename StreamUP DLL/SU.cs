﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streamer.bot.Plugin.Interface;

namespace StreamUPDLL {

    public static class SU {

        public static string GetStreamerBotFolder () {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
       
        public static void WriteLog (string logMessage, string productName) {
            string sbFolder = GetStreamerBotFolder();
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
                string formattedLogMessage = $"[{today.ToString("yyyy-MM-dd HH:mm:ss:ff")}] [{productName}] :: {logMessage}";
                file.Write($"\r\n{formattedLogMessage}");
            }
        }
    }
}
