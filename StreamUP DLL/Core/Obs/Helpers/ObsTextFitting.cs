using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS TEXT FITTING HELPERS
        // Methods for fitting and wrapping text within size constraints
        // ============================================================

        /// <summary>
        /// Fits text into a GDI text source, wrapping and truncating as needed to fit within the specified dimensions.
        /// Text that exceeds the bounds will be truncated with "..." appended.
        /// </summary>
        /// <param name="sceneName">The scene or group containing the text source</param>
        /// <param name="sourceName">The GDI text source name</param>
        /// <param name="text">The text to fit</param>
        /// <param name="maxWidth">Maximum width in pixels</param>
        /// <param name="maxHeight">Maximum height in pixels</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>The fitted text string, or null if unable to fit any text</returns>
        public string ObsFitTextToSize(string sceneName, string sourceName, string text, int maxWidth, int maxHeight, int connection = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                LogDebug("ObsFitTextToSize: Empty text provided");
                return text;
            }

            // Set initial text and check dimensions
            _CPH.ObsSetGdiText(sceneName, sourceName, text, connection);
            _CPH.Wait(20);

            var dimensions = GetTextDimensions(sceneName, sourceName, connection);
            if (dimensions == null)
            {
                LogError("ObsFitTextToSize: Unable to retrieve initial text dimensions");
                return null;
            }

            // If text already fits, return as-is
            if (dimensions.Value.width <= maxWidth && dimensions.Value.height <= maxHeight)
            {
                LogDebug($"ObsFitTextToSize: Text fits without wrapping ({dimensions.Value.width}x{dimensions.Value.height})");
                return text;
            }

            // Text is too wide - wrap into lines
            List<string> lines = WrapTextToWidth(sceneName, sourceName, text, maxWidth, connection);
            if (lines == null || lines.Count == 0)
            {
                LogError("ObsFitTextToSize: Failed to wrap text into lines");
                return null;
            }

            // Build text line by line, checking height
            string result = "";
            string previousResult = "";

            foreach (var line in lines)
            {
                string testText = string.IsNullOrEmpty(result) ? line : result + "\n" + line;

                _CPH.ObsSetGdiText(sceneName, sourceName, testText, connection);
                _CPH.Wait(20);

                dimensions = GetTextDimensions(sceneName, sourceName, connection);
                if (dimensions == null)
                {
                    LogError("ObsFitTextToSize: Unable to retrieve dimensions during line building");
                    return null;
                }

                if (dimensions.Value.height > maxHeight)
                {
                    // Height exceeded - use previous result with ellipsis
                    result = string.IsNullOrEmpty(previousResult) ? "..." : previousResult + "...";
                    break;
                }

                previousResult = testText;
                result = testText;
            }

            // If we added ellipsis, trim words until it fits
            if (result.EndsWith("..."))
            {
                result = TrimTextToFit(sceneName, sourceName, result, maxWidth, maxHeight, connection);
            }

            // Final verification
            _CPH.ObsSetGdiText(sceneName, sourceName, result, connection);
            _CPH.Wait(20);

            dimensions = GetTextDimensions(sceneName, sourceName, connection);
            if (dimensions == null || dimensions.Value.width > maxWidth || dimensions.Value.height > maxHeight)
            {
                LogError($"ObsFitTextToSize: Final text still exceeds bounds");
                return null;
            }

            LogDebug($"ObsFitTextToSize: Final dimensions {dimensions.Value.width}x{dimensions.Value.height}");
            return result;
        }

        /// <summary>
        /// Wraps text into lines that fit within the specified width.
        /// </summary>
        /// <param name="sceneName">The scene or group containing the text source</param>
        /// <param name="sourceName">The GDI text source name</param>
        /// <param name="text">The text to wrap</param>
        /// <param name="maxWidth">Maximum width in pixels</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>List of lines that fit within maxWidth, or null on error</returns>
        public List<string> ObsWrapTextToWidth(string sceneName, string sourceName, string text, int maxWidth, int connection = 0)
        {
            return WrapTextToWidth(sceneName, sourceName, text, maxWidth, connection);
        }

        // ============================================================
        // PRIVATE HELPER METHODS
        // ============================================================

        /// <summary>
        /// Gets the width and height of text currently set on a source.
        /// </summary>
        private (int width, int height)? GetTextDimensions(string sceneName, string sourceName, int connection)
        {
            JObject transform = ObsGetSourceTransform(sceneName, sourceName, connection);
            if (transform == null)
            {
                return null;
            }

            double? width = transform["width"]?.Value<double>();
            double? height = transform["height"]?.Value<double>();

            if (!width.HasValue || !height.HasValue)
            {
                return null;
            }

            return ((int)Math.Round(width.Value), (int)Math.Round(height.Value));
        }

        /// <summary>
        /// Wraps text into lines fitting within maxWidth.
        /// </summary>
        private List<string> WrapTextToWidth(string sceneName, string sourceName, string text, int maxWidth, int connection)
        {
            var lines = new List<string>();
            string[] words = text.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;

                _CPH.ObsSetGdiText(sceneName, sourceName, testLine, connection);
                _CPH.Wait(20);

                var dimensions = GetTextDimensions(sceneName, sourceName, connection);
                if (dimensions == null)
                {
                    LogError("WrapTextToWidth: Unable to retrieve dimensions");
                    return null;
                }

                if (dimensions.Value.width <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    // Word doesn't fit - save current line and start new one
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                    }
                    currentLine = word;
                }
            }

            // Add remaining line
            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }

        /// <summary>
        /// Trims words from the end of text until it fits within bounds.
        /// </summary>
        private string TrimTextToFit(string sceneName, string sourceName, string text, int maxWidth, int maxHeight, int connection)
        {
            string result = text;

            while (true)
            {
                _CPH.ObsSetGdiText(sceneName, sourceName, result, connection);
                _CPH.Wait(20);

                var dimensions = GetTextDimensions(sceneName, sourceName, connection);
                if (dimensions == null)
                {
                    LogError("TrimTextToFit: Unable to retrieve dimensions");
                    return result;
                }

                // Check if it fits
                if (dimensions.Value.width <= maxWidth && dimensions.Value.height <= maxHeight)
                {
                    return result;
                }

                // Find last space before ellipsis and remove word
                int lastSpace = result.LastIndexOf(' ', result.Length - 4); // -4 to skip "..."
                if (lastSpace <= 0)
                {
                    // No more words to remove
                    return result;
                }

                result = result.Substring(0, lastSpace) + "...";
            }
        }
    }
}
