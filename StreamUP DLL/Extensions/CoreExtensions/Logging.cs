using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private void SetDebugLogModeOn()
        {
            _CPH.SetGlobalVar("sup000_DebugMode", true, true);
            LogInfo("Debug mode enabled.");
        }

        private void SetDebugLogModeOff()
        {
            _CPH.SetGlobalVar("sup000_DebugMode", false, true);
            LogInfo("Debug mode disabled.");
        }

        private void WriteToLog(string logMessage)
        {
            // Get log folder
            string logFolder = Path.Combine(GetStreamerBotFolder(), "StreamUP", "logs");
            Directory.CreateDirectory(logFolder);

            string todayFileName = $"{DateTime.Now:yyyyMMdd} - StreamUP.log";
            string todayPath = Path.Combine(logFolder, todayFileName);

            // Use FileStream with FileShare.ReadWrite to allow other processes to read the file while it's being written.
            using (
                var fileStream = new FileStream(
                    todayPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite
                )
            )
            using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                if (fileStream.Length == 0)
                {
                    writer.WriteLine("Ayoo duckies! New Day!");
                }

                writer.WriteLine(logMessage);
            }
        }

        public void LogDebug(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            if (!_DebugMode)
            {
                return;
            }

            // Format log message
            string formattedLogMessage =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} DBG] [{_ProductIdentifier}] {memberName}({lineNumber}) :: {message}";

            // Write to log
            WriteToLog(formattedLogMessage);
        }

        public void LogError(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            // Format log message
            string formattedLogMessage =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} ERR] [{_ProductIdentifier}] {memberName}({lineNumber}) :: {message}";

            // Write to log
            WriteToLog(formattedLogMessage);
        }

        public void LogInfo(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0
        )
        {
            // Format log message
            string formattedLogMessage =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} INF] [{_ProductIdentifier}] {memberName}({lineNumber}) :: {message}";

            // Write to log
            WriteToLog(formattedLogMessage);
        }
    }
}
