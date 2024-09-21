using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool CheckObsWebsocketVersion(int obsConnection)
        {
            // Pull Obs websocket version number
            string response = _CPH.ObsSendRaw("GetVersion", "{}", obsConnection);
            JObject responseJson = JObject.Parse(response);

            // Check if response contains obsWebSocketVersion
            if (responseJson["obsWebSocketVersion"] == null)
            {
                LogError("obsWebSocketVersion not found in the response.");
                return false;
            }

            LogInfo($"Websocket version verified [{responseJson["obsWebSocketVersion"].ToString()}]");
            return true;

        }

    }
}
