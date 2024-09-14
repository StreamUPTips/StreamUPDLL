using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

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

        public bool ObsAutoSizeAdvancedMask(string sourceName, string filterName, double sourceHeight, double sourceWidth, double padHeight, double padWidth, double scaleFactor, int obsConnection)
        {
            LogInfo($"Starting autosize advanced mask for source [{sourceName}] and filter [{filterName}]");

            // Calculate new height and width
            double newHeight = sourceHeight + (padHeight * scaleFactor);
            double newWidth = sourceWidth + (padWidth * scaleFactor);
            LogInfo($"Calculated new dimensions: Height=[{newHeight}], Width=[{newWidth}]");

            // Prepare filter settings as a JSON object
            JObject filterSettingsObj = new JObject
            {
                ["rectangle_width"] = newWidth,
                ["rectangle_height"] = newHeight
            };

            // Set the source filter settings
            SetObsSourceFilterSettings(sourceName, filterName, filterSettingsObj, obsConnection);
            LogInfo("Autosize advanced mask completed successfully.");
            return true;
        }

        public bool ObsAutoPositionAdvancedMask(string sourceName, string filterName, double padHeight, double padWidth, double scaleFactor, int obsConnection)
        {
            LogInfo($"Starting auto position advanced mask for source [{sourceName}] and filter [{filterName}]");

            // Retrieve the filter data
            if (!GetObsSourceFilterData(sourceName, filterName, obsConnection, out JObject filterData))
            {
                LogError($"Unable to retrieve filter data for source [{sourceName}] and filter [{filterName}]");
                return false;
            }

            // Check if filterSettings exists
            if (filterData["filterSettings"] == null)
            {
                LogError("filterSettings not found in the response");
                return false;
            }

            // Check if rectangle_height exists
            if (filterData["filterSettings"]["rectangle_height"] == null)
            {
                LogError("rectangle_height not found in filterSettings");
                return false;
            }

            // Check if rectangle_width exists
            if (filterData["filterSettings"]["rectangle_width"] == null)
            {
                LogError("rectangle_width not found in filterSettings");
                return false;
            }

            // Get current height and width
            double currentHeight = (double)filterData["filterSettings"]["rectangle_height"];
            double currentWidth = (double)filterData["filterSettings"]["rectangle_width"];

            // Calculate new positions
            double xPos = (currentWidth / 2) + (padWidth * scaleFactor);
            double yPos = (currentHeight / 2) + (padHeight * scaleFactor);

            // Prepare filter settings as a JSON object
            JObject newFilterSettings = new JObject
            {
                ["position_x"] = xPos.ToString(CultureInfo.InvariantCulture),
                ["position_y"] = yPos.ToString(CultureInfo.InvariantCulture)
            };

            // Set the source filter settings
            SetObsSourceFilterSettings(sourceName, filterName, newFilterSettings, obsConnection);
            LogInfo("Auto position advanced mask completed successfully.");
            return true;
        }

        public bool SUObsSplitTextOnWidth(string sceneName, string sourceName, string inputText, int maxWidth, int maxHeight, int obsConnection, out string outputText)
        {
            _CPH.ObsSetGdiText(sceneName, sourceName, inputText, obsConnection);
            _CPH.Wait(20); // Allow OBS to update
            //!Do we need to add a delay!?

            // Get initial text dimensions
            if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out JObject textTransform))
            {
                LogError("Unable to get source transform");
                outputText = null;
                return false;
            }

            // Check if width exists
            if (textTransform["width"] == null)
            {
                LogError("width not found in the response");
                outputText = null;
                return false;
            }

            int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));

            if (textWidth <= maxWidth)
            {
                outputText = inputText;
                return true;
            }

            // Split text into lines if needed
            if (!ObsSplitTextIntoLines(sceneName, sourceName, inputText, maxWidth, obsConnection, out List<string> textLines))
            {
                LogError("Unable to split text into separate lines");
                outputText = null;
                return false;
            }

            // Initialise message vars
            var newMessage = new StringBuilder();
            string previousMessage = "";

            // Add each line to check if it exceeds height
            foreach (var line in textLines)
            {
                // Test by appending new lines
                string testMessage = newMessage.Length == 0 ? line : $"{newMessage}\n{line}";
                _CPH.ObsSetGdiText(sceneName, sourceName, testMessage, obsConnection);
                _CPH.Wait(20);
                //!Do we need to add a delay!?

                // Get the height of the updated text
                if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out textTransform))
                {
                    outputText = null;
                    LogError("Unable to get source transform");
                    return false;
                }

                // Check if height exists
                if (textTransform["height"] == null)
                {
                    LogError("height not found in the response");
                    outputText = null;
                    return false;
                }

                int currentHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));

                if (currentHeight > maxHeight)
                {
                    // Exceeded height, add ellipsis
                    newMessage.Clear().Append(previousMessage).Append("...");
                    break;
                }

                previousMessage = newMessage.ToString();
                newMessage.Append(line);
            }

            // Final adjustment to ensure it fits within the width and height
            while (newMessage.ToString().EndsWith("..."))
            {
                // Call DoesTextExceedBounds and handle the null case
                bool? exceedsBounds = DoesTextExceedBounds(sceneName, sourceName, newMessage.ToString(), maxWidth, maxHeight, obsConnection);

                if (exceedsBounds == null)
                {
                    // Error case, break the loop or handle accordingly
                    LogError("Error while checking bounds. Exiting loop.");
                    break;
                }
                else if (exceedsBounds == true)
                {
                    // Trim the message to fit the bounds
                    TrimMessageToFitBounds(ref newMessage);
                }
                else
                {
                    // Message fits within bounds, exit the loop
                    break;
                }
            }

            // Set final text source
            _CPH.ObsSetGdiText(sceneName, sourceName, newMessage.ToString(), obsConnection);
            _CPH.Wait(20);
            //!Do we need to add a delay!?

            outputText = newMessage.ToString();
            LogInfo("Text split into lines successfully");
            return true;
        }

        private bool ObsSplitTextIntoLines(string sceneName, string sourceName, string message, int maxWidth, int obsConnection, out List<string> textLines)
        {
            // Initialise vars
            textLines = new List<string>();
            var currentLine = new StringBuilder();
            string[] words = message.Split(' ');

            // Check length after each word is added and split the string into lines
            foreach (string word in words)
            {
                // Build the test line by adding the new word
                string testLine = (currentLine.Length > 0 ? currentLine + " " : "") + word;

                // Set text in OBS and measure the width
                _CPH.ObsSetGdiText(sceneName, sourceName, testLine, obsConnection);
                _CPH.Wait(20); // Allow OBS to update
                //!Do we need to add a delay!?

                // Get the width of the updated text
                if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out JObject textTransform))
                {
                    textLines = null;
                    LogError("Unable to get source transform");
                    return false;
                }

                // Check if width exists
                if (textTransform["width"] == null)
                {
                    LogError("width not found in the response");
                    textLines = null;
                    return false;
                }

                // Load text width as var
                int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));

                if (textWidth <= maxWidth)
                {
                    // If the width is within bounds, keep adding words to the current line
                    currentLine.Clear().Append(testLine);
                }
                else
                {
                    // Add the current line to the list of lines and start a new line with the current word
                    if (!string.IsNullOrEmpty(currentLine.ToString()))
                    {
                        textLines.Add(currentLine.ToString());
                    }
                    currentLine.Clear().Append(word);
                }
            }

            // Add the last remaining line
            if (!string.IsNullOrEmpty(currentLine.ToString()))
            {
                textLines.Add(currentLine.ToString());
            }

            LogInfo("Text lines split successfully");
            return true;
        }

        // Helper to check bounds
        private bool? DoesTextExceedBounds(string sceneName, string sourceName, string text, int maxWidth, int maxHeight, int obsConnection)
        {
            _CPH.ObsSetGdiText(sceneName, sourceName, text, obsConnection);
            _CPH.Wait(20);
            //!Do we need to add a delay!?

            // Get the height of the updated text
            if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out JObject textTransform))
            {
                LogError("Unable to get source transform");
                return null;
            }

            // Check if width exists
            if (textTransform["width"] == null)
            {
                LogError("width not found in the response");
                return null;
            }

            // Check if height exists
            if (textTransform["height"] == null)
            {
                LogError("height not found in the response");
                return null;
            }

            // Set current width and height
            int currentWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
            int currentHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));

            return currentWidth > maxWidth || currentHeight > maxHeight;
        }

        // Helper to trim the message for fitting within bounds
        private void TrimMessageToFitBounds(ref StringBuilder message)
        {
            int lastSpaceIndex = message.ToString().LastIndexOf(' ', message.Length - 4); // -4 to ignore the '...'
            if (lastSpaceIndex > 0)
            {
                message.Remove(lastSpaceIndex, message.Length - lastSpaceIndex).Append("...");
            }
        }

    }
}
