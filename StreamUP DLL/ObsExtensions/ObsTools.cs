using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // OBS Tools
        public bool ObsScreenshotSelectedSource(int obsConnection)
        {
            LogInfo("Getting info to screenshot current OBS source");

            // Get current selected source
            if (!GetObsSelectedSource(obsConnection, out string selectedSource))
            {
                LogError("Unable to retrieve current selected source");
                ShowToastNotification(NotificationType.Error, "Screenshot Failed", "Unable to retieve current seleced source");
                return false;
            }

            // Get obs output filepath
            if (!GetObsOutputFilePath(obsConnection, out string obsOutputFilePath))
            {
                LogError("Unable to retrieve OBS output filepath");
                ShowToastNotification(NotificationType.Error, "Screenshot Failed", "Unable to retrieve OBS output filepath");
                return false;
            }

            // Get current DateTime and save it as 
            DateTime currentTime = DateTime.Now;
            string currentTimeStr = currentTime.ToString("yyyyMMdd_HHmmss");

            // Create full file path name
            string fileName = $"{selectedSource}_{currentTimeStr}.png";
            string filePath = $"{obsOutputFilePath}\\{fileName}";       
            
            // Escape backslashes
            string jsonFilePath = filePath.Replace("\\", "\\\\");

            // Screenshot source
            _CPH.ObsSendRaw("SaveSourceScreenshot", $"{{\"sourceName\":\"{selectedSource}\",\"imageFormat\":\"png\",\"imageFilePath\":\"{jsonFilePath}\"}}", obsConnection);

            // Send toast confirmation
            LogInfo($"Screenshot of source [{selectedSource}] has been saved to [{filePath}]");
            ShowToastNotification(NotificationType.Success, "Screenshot Saved", $"'{selectedSource}' was screenshot successfully", $"Saved to: {filePath}");
            return true;
        }

    }
}
