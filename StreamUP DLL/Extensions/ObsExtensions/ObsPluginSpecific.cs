using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        // Ghostscript PDF Plugin
        public bool SetObsGhostscriptPdfPage(string sourceName, int pageNumber, int obsConnection)
        {
            LogInfo($"Setting Ghostscript PDF page for source [{sourceName}]");

            // Prepare request data
            JObject settings = new JObject
            {
                ["page_number"] = pageNumber
            };

            SetObsSourceSettings(sourceName, settings, obsConnection);

            LogInfo($"Set Ghostscript PDF page successfully");
            return true;
        }

        public bool SetObsGhostscriptPdfFile(string sourceName, string filePath, int obsConnection)
        {
            LogInfo($"Setting Ghostscript PDF file for source [{sourceName}]");

            // Prepare request data
            JObject settings = new JObject
            {
                ["file_path"] = filePath
            };

            SetObsSourceSettings(sourceName, settings, obsConnection);

            LogInfo($"Set Ghostscript PDF page successfully");
            return true;
        }
    }
}

