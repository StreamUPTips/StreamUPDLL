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

        //#region Split text based on width
        public string ObsSplitTextOnWidth(string productNumber, int obsConnection, OBSSceneType parentSourceType, string sceneName, string sourceName, string rawText, int maxWidth, int maxHeight)
        {
            // Check current text width by setting initial text
            _CPH.ObsSetGdiText(sceneName, sourceName, rawText, obsConnection);
            _CPH.Wait(20); // Allow OBS to update

            if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out JObject textTransform))
            {
                LogError("Unable to retrieve source transform");
                return null;
            }

            int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));

            string newMessage = "";

            // If text is too wide, split into lines
            if (textWidth > maxWidth)
            {
                List<string> lines = ObsSplitTextIntoLines(productNumber, obsConnection, maxWidth, rawText, parentSourceType, sceneName, sourceName);
                string previousMessage = ""; // Store the message from the previous iteration

                int currentHeight;
                foreach (var line in lines)
                {
                    // Build the test message by adding the new line
                    string testMessage = newMessage == "" ? line : newMessage + "\n" + line;
                    _CPH.ObsSetGdiText(sceneName, sourceName, testMessage, obsConnection);
                    _CPH.Wait(20); // Allow OBS to update

                    // Fetch and parse the new height
                    if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out textTransform))
                    {
                        LogError("Unable to retrieve source transform");
                        return null;
                    }

                    currentHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));

                    if (currentHeight > maxHeight)
                    {
                        // If the height exceeds the max height, use the previous message and append '...'
                        newMessage = previousMessage == "" ? "..." : previousMessage + "...";
                        break; // Exit the loop as we've reached the limit
                    }
                    else
                    {
                        // If the height is within bounds, update newMessage and keep this as previousMessage
                        newMessage = testMessage;
                        previousMessage = newMessage; // Update previousMessage to this newMessage for next iteration
                    }
                }

                if (newMessage.EndsWith("..."))
                {
                    bool fitsWithinBounds = false;
                    while (!fitsWithinBounds)
                    {
                        _CPH.ObsSetGdiText(sceneName, sourceName, newMessage, obsConnection);
                        _CPH.Wait(20); // Allow OBS to update

                        // Fetch and parse the current dimensions
                        if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out textTransform))
                        {
                            LogError("Unable to retrieve source transform");
                            return null;
                        }

                        currentHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));
                        int currentWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));

                        if (currentHeight > maxHeight || currentWidth > maxWidth)
                        {
                            // Remove the last word along with '...' then add '...' back
                            int lastSpaceIndex = newMessage.LastIndexOf(' ', newMessage.Length - 4); // -4 to ignore the '...'

                            if (lastSpaceIndex > 0) // Ensure there is a space to find, indicating another word exists
                            {
                                // Update newMessage by removing the last word
                                newMessage = newMessage.Substring(0, lastSpaceIndex) + "...";
                            }
                            else
                            {
                                // No more spaces found, which means no more words left to remove
                                fitsWithinBounds = true; // Exit the loop as we can't reduce the text further
                            }
                        }
                        else
                        {
                            // Text fits within the height and width, exit the loop
                            fitsWithinBounds = true;
                        }
                    }
                }
            }
            else
            {
                // If the text is within the width limit, use the original text
                newMessage = rawText;
            }


            // Set final text in OBS and log final dimensions
            _CPH.ObsSetGdiText(sceneName, sourceName, newMessage, obsConnection);
            _CPH.Wait(20); // Allow OBS to update for the last time

            if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out textTransform))
            {
                LogError("Unable to retrieve source transform");
                return null;
            }

            int finalWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));
            int finalHeight = (int)Math.Round(decimal.Parse(textTransform["height"].ToString()));

            if (finalWidth > maxWidth || finalHeight > maxHeight)
            {

                return null;
            }

            return newMessage;
        }

        private List<string> ObsSplitTextIntoLines(string productNumber, int obsConnection, int maxWidth, string message, OBSSceneType parentSourceType, string sceneName, string sourceName)
        {
            List<string> lines = new List<string>();
            string[] words = message.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = (currentLine.Length > 0 ? currentLine + " " : "") + word;
                _CPH.ObsSetGdiText(sceneName, sourceName, testLine, obsConnection);
                _CPH.Wait(20); // Allow OBS to update

                if (!GetObsSourceTransform(sceneName, sourceName, obsConnection, out JObject textTransform))
                {
                    LogError("Unable to retrieve source transform");
                    return null;
                }

                int textWidth = (int)Math.Round(decimal.Parse(textTransform["width"].ToString()));

                if (textWidth <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine); // Add the current line to the list of lines
                    }
                    currentLine = word; // Start a new line with the current word
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine); // Add the remaining line to the list
            }

            return lines;
        }        
        //#endregion

    }
}
