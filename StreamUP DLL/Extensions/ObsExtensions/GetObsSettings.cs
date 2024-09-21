using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // GET OBS SETTINGS
        public bool GetObsVideoSettings(int obsConnection, out JObject videoSettings)
        {
            LogInfo("Requesting OBS video settings");

            // Get OBS video settings
            string response = _CPH.ObsSendRaw("GetVideoSettings", "{}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                videoSettings = null;
                return false;
            }

            // Parse response
            videoSettings = JObject.Parse(response);

            LogInfo("Successfully retrieved OBS video settings");
            return true;
        }

        public bool GetObsCanvasSize(int obsConnection, out int baseWidth, out int baseHeight)
        {
            LogInfo("Getting OBS canvas size");
            baseWidth = 0;
            baseHeight = 0;

            // Get OBS video settings
            if (!GetObsVideoSettings(obsConnection, out JObject videoSettings))
            {
                LogError("Unable to retrieve OBS video settings");
                return false;
            }

            // Check baseWidth and baseHeight exist
            if (videoSettings["baseWidth"] == null)
            {
                LogError("baseWidth not found in the response");
                return false;
            }
            if (videoSettings["baseHeight"] == null)
            {
                LogError("baseHeight not found in the response");
                return false;
            }

            baseWidth = int.Parse((string)videoSettings["baseWidth"]);
            baseHeight = int.Parse((string)videoSettings["baseHeight"]);

            LogInfo($"Canvas size retrieved successfully. baseHeight [{baseHeight}], baseWidth [{baseWidth}]");
            return true;
        }

        public bool GetObsOutputFilePath(int obsConnection, out string filePath) //! Requires StreamUP OBS plugin
        {
            LogInfo($"Requesting OBS filepath");

            // Get OBS filepath
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getOutputFilePath\",\"requestData\":null}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                filePath = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);

            // Check if responseData and outputFilePath exist
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                filePath = null;
                return false;
            }

            if (responseObj["responseData"]["outputFilePath"] == null)
            {
                LogError("outputFilePath not found in the responseData");
                filePath = null;
                return false;
            }


            filePath = responseObj["responseData"]["outputFilePath"].ToString();
            LogInfo("Successfully retrieved output filepath");
            return true;
        }

        public bool GetObsCanvasScaleFactor(int obsConnection, out double scaleFactor)
        {
            LogInfo($"Requesting OBS canvas scale factor");

            // Get video settings
            if (!GetObsVideoSettings(obsConnection, out JObject videoSettings))
            {
                LogError("Unable to retrieve videoSettings");
                scaleFactor = 0.0;
                return false;
            }

            // Check if baseWidth exists
            if (videoSettings["baseWidth"] == null)
            {
                LogError("baseWidth not found in the response");
                scaleFactor = 0.0;
                return false;
            }

            // Extract canvas width
            double canvasWidth = (double)videoSettings["baseWidth"];

            // Work out scale difference based on 1920x1080
            scaleFactor = canvasWidth / 1920;
            LogInfo($"Successfully retrieved scale factor [{scaleFactor}]");
            return true;
        }

        //! NEEDS WORK
        public bool GetObsCurrentBitrate(int obsConnection, out string bitrate) //! Requires StreamUP OBS plugin
        {
            LogInfo("Requesting Current OBS bitrate");

            // Get current bitrate
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup\",\"requestType\":\"getBitrate\",\"requestData\":{}}", obsConnection);
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                LogError("No response from OBS");
                bitrate = null;
                return false;
            }

            // Parse as object
            JObject responseObj = JObject.Parse(response);

            // Check if responseData and outputFilePath exist
            if (responseObj["responseData"] == null)
            {
                LogError("responseData not found in the response");
                bitrate = null;
                return false;
            }
            if (responseObj["responseData"]["kbits-per-sec"] == null)
            {
                LogError("kbits-per-sec not found in responseData");
                bitrate = null;
                return false;
            }

            bitrate = responseObj["responseData"]["kbits-per-sec"].ToString();
            LogInfo("Successfully retrieved bitrate");
            return true;
        }

    }
}
