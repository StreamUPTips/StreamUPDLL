using System;
using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Check OBS connection and plugins
        public bool CheckObsConnectionSet(Dictionary<string, object> productSettings, out int obsConnection)
        {
            LogInfo("Getting obsConnection number from productSettings");
            // Retrieve ObsConnection from productSettings
            if (productSettings.TryGetValue("ObsConnection", out object obsConnectionObj) || productSettings.TryGetValue("obsConnection", out obsConnectionObj))
            {
                obsConnection = Convert.ToInt32(obsConnectionObj);
                LogInfo($"Successfully retrieved the obsConnection number [{obsConnection}]");
                return true;
            }
            else
            {
                LogError("ObsConnection setting is missing in product settings.");
                obsConnection = -1;
                return false;
            }
        }

        private bool TryGetObsConnection(int obsConnection)
        {
            LogInfo("Attempting to connect to OBS");
            bool isConnected = _CPH.ObsIsConnected(obsConnection);

            if (!isConnected)
            {
                LogError($"OBS is not connected on connection [{obsConnection}]");
            }
            else
            {
                LogInfo("OBS is connected");
            }

            return isConnected;
        }
    }
}
