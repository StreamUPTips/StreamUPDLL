using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool InitProductObsSettings(string actionName, string productNumber)
        {
            // // Get ObsConnection setting
            // Dictionary<string, object> productSettingsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(productSettings);
            // int obsConnection = Convert.ToInt32(productSettingsDict["ObsConnection"]);

            // // Check OBS is connected
            // if (!TryGetObsConnection(obsConnection))
            // {
            //     LogError($"OBS is not connected on connection [{obsConnection}]");
            //     return false;
            // }

            // // Check OBS plugins are installed and up to date
            // bool? obsPluginsUpToDate = GetObsPluginVersions(obsConnection);
            // if ((bool)!obsPluginsUpToDate)
            // {
            //     LogError("OBS plugins are not up to date, user has opted to not continue");
            //     return false;
            // }

            // // Check products scene exists in OBS
            // if (!TryGetObsProductScene(productInfo, obsConnection))
            // {
            //     LogError($"Product scene [{productInfo.SceneName}] does not exist in OBS");
            //     return false;
            // }

            // // Check product scene version number
            // if (!GetObsProductVersion(productInfo, obsConnection, out Version versionInstalled))
            // {
            //     LogError($"Obs product is out of date. This version requires [{productInfo.SourceNameVersionNumber}]. The user has [{versionInstalled}]");
            //     return false;
            // }

            // // Mark product as initialised
            // _CPH.SetGlobalVar($"{productNumber}_ProductInitialised", true, false);
            // LogInfo($"Product [{productInfo.ProductName}] has been initialised");
            return true;
        }

    }
}
