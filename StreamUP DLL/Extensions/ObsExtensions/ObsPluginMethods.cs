using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS STREAMUP PLUGIN METHODS
        // These methods require the StreamUP OBS plugin to be installed
        // ============================================================

        /// <summary>
        /// Gets the OBS output file path. Requires StreamUP OBS plugin.
        /// </summary>
        /// <param name="obsConnection">OBS connection index (0-4)</param>
        /// <param name="filePath">Output: The output file path</param>
        /// <returns>True if successful, false if unable to retrieve</returns>
        public bool ObsGetOutputFilePath(int obsConnection, out string filePath)
        {
            LogInfo("Requesting OBS filepath");

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

        /// <summary>
        /// Gets the current OBS bitrate. Requires StreamUP OBS plugin.
        /// </summary>
        /// <param name="obsConnection">OBS connection index (0-4)</param>
        /// <param name="bitrate">Output: The current bitrate in kbits/sec</param>
        /// <returns>True if successful, false if unable to retrieve</returns>
        public bool ObsGetCurrentBitrate(int obsConnection, out string bitrate)
        {
            LogInfo("Requesting current OBS bitrate");

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

            // Check if responseData exists
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
