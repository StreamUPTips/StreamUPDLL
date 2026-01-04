using System;
using System.IO;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Get the file path for a product's data file.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>Full file path, or empty string if unable to construct path</returns>
        private string GetProductDataFilePath(string productNumber)
        {
            try
            {
                if (!GetStreamerBotDirectory(out string directory))
                {
                    LogError("Failed to get StreamerBot directory");
                    return string.Empty;
                }

                return Path.Combine(directory, $"{productNumber}_Data.json");
            }
            catch (Exception ex)
            {
                LogError($"Error constructing file path: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the StreamerBot data directory, creating it if necessary.
        /// Verifies write access to the directory.
        /// </summary>
        /// <param name="directory">Out parameter containing the directory path</param>
        /// <returns>True if directory is accessible and writable</returns>
        public bool GetStreamerBotDirectory(out string directory)
        {
            directory = string.Empty;

            try
            {
                string programDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Validate base directory
                if (string.IsNullOrEmpty(programDirectory))
                {
                    return false;
                }

                directory = Path.Combine(programDirectory, "StreamUP", "Data");

                // Verify the directory exists or can be created
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Verify we have write access
                string testFile = Path.Combine(directory, ".test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
