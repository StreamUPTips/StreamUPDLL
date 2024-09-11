using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool TryGetObsVideoSettings(int obsConnection, out JObject settings)
        {
            LogInfo("Requesting OBS video settings");

            try
            {
                // Pull OBS video settings
                string jsonResponse = _CPH.ObsSendRaw("GetVideoSettings", "{}", obsConnection);

                if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "{}")
                {
                    LogError("Video settings not found");
                    settings = null;
                    return false;
                }

                settings = JObject.Parse(jsonResponse);
                LogInfo("Successfully retrieved OBS video settings");
                return true;
            }
            catch (JsonReaderException ex)
            {
                LogError($"Error parsing OBS video settings: {ex.Message}");
                settings = null;
                return false;
            }
        }
    }
}
