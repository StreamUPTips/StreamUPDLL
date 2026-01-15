using System;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBSOLETE OBS SETTINGS METHODS
        // These methods are deprecated. Use Core\Obs\Helpers\ methods instead.
        // ============================================================

        [Obsolete(
            "Use ObsGetVideoSettings(int connection) from Core\\Obs\\WebSocket\\ObsConfig.cs instead"
        )]
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

        [Obsolete(
            "Use ObsGetCanvasSize(int connection, out int baseWidth, out int baseHeight) from Core\\Obs\\Helpers\\ instead"
        )]
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

            LogInfo(
                $"Canvas size retrieved successfully. baseHeight [{baseHeight}], baseWidth [{baseWidth}]"
            );
            return true;
        }

        [Obsolete("Use ObsGetScaleFactor(int connection) from Core\\Obs\\Helpers\\ instead")]
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
    }
}
