using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool DSILoader(int obsConnection)
        {
            var dsiInfo = DSILoadInfo();

            if (!LoadExtensionsFromAPI())
            {
                LogError("Failed to load extensions from API.");
                return false;
            }

            if (!GetInstalledExtensions(dsiInfo))
            {
                LogError("Failed to get installed extensions.");
                return false;
            }

            SetBackgroundToLowestLayer(obsConnection);

            SetRotatorSettings(dsiInfo);

            ResetWidget(dsiInfo, obsConnection);

            return true;
        }

        public class DSIProductData
        {
            public bool TimeWidgetRotatorEnable { get; set; }
            public bool DateWidgetRotatorEnable { get; set; }
            public bool UptimeWidgetRotatorEnable { get; set; }
            public bool TwitchViewerCountWidgetRotatorEnable { get; set; }
        }

        private void SetRotatorSettings(DSIInfo dsiInfo)
        {
            // Get list of rotator widgets
            List<string> rotatorWidgets = dsiInfo.RotatorWidgets;

            DSIProductData productSettings = JsonConvert.DeserializeObject<DSIProductData>(
                _CPH.GetGlobalVar<string>("sup069_ProductSettings", true)
            );

            // Remove widgets from the list if they are disabled
            if (!productSettings.TimeWidgetRotatorEnable)
                rotatorWidgets.Remove("Dynamic Stream Island • TIME: Widget");
            if (!productSettings.DateWidgetRotatorEnable)
                rotatorWidgets.Remove("Dynamic Stream Island • DATE: Widget");
            if (!productSettings.UptimeWidgetRotatorEnable)
                rotatorWidgets.Remove("Dynamic Stream Island • STREAM UPTIME: Widget");
            if (!productSettings.TwitchViewerCountWidgetRotatorEnable)
                rotatorWidgets.Remove("Dynamic Stream Island • TWITCH VIEWER COUNT: Widget");

            // Save the updated list
            dsiInfo.RotatorWidgets = rotatorWidgets;
        }

        private void ResetWidget(DSIInfo dsiInfo, int obsConnection)
        {
            _CPH.ResumeActionQueue("StreamUP Widgets • DSI Widgets");
            _CPH.ResumeActionQueue("StreamUP Widgets • DSI Triggers");
            _CPH.ResumeActionQueue("StreamUP Widgets • DSI Non-Blocking");
            _CPH.EnableTimerById("27f9dd60-c81c-434f-a11a-ffc5bd578785");
            _CPH.ObsShowSource(
                "StreamUP Widgets • Dynamic Stream-Island",
                "DSI • Alerts • Group",
                obsConnection
            );

            dsiInfo.CurrentState = DSIInfo.DSIState.Default;
            dsiInfo.ActiveWidget = string.Empty;
            dsiInfo.RotatorIndex = 0;
            dsiInfo.AlertQueue = 0;
            dsiInfo.ActionInProgress = false;

            DSISaveInfo(dsiInfo);
        }

        private void SetBackgroundToLowestLayer(int obsConnection)
        {
            // Set the background to the lowest layer
            SetObsSceneItemIndex(
                "StreamUP Widgets • Dynamic Stream-Island",
                "DSI • BG",
                0,
                obsConnection
            );
        }

        private bool LoadExtensionsFromAPI()
        {
            string uri = "https://api.streamup.tips/product/dsi/extensions";

            try
            {
                // Prepare the HTTP request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

                // Send the request
                HttpResponseMessage result = _httpClient.SendAsync(request).Result;

                // Check if the response was successful
                if (!result.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Request failed with status code: {(int)result.StatusCode} - {result.ReasonPhrase}"
                    );
                }

                // Read the response content
                string content = result.Content.ReadAsStringAsync().Result;

                // Log the raw response (optional, for debugging)
                LogDebug($"API Response: {content}");

                // Deserialize the JSON into a list of Extension objects
                List<DSIExtensions> apiResponseList = JsonConvert.DeserializeObject<
                    List<DSIExtensions>
                >(content);

                // Log the deserialized data (optional, for debugging)
                foreach (var extension in apiResponseList)
                {
                    LogDebug($"Extension: {extension.ToString()}");
                }

                // Process the deserialized data if needed
                ProcessExtensionsFromAPI(apiResponseList);

                return true;
            }
            catch (HttpRequestException error)
            {
                // Handle HTTP request errors
                LogError($"HTTP Request Error: {error.Message}");
                return false;
            }
            catch (JsonSerializationException error)
            {
                // Handle JSON deserialization errors
                LogError($"JSON Deserialization Error: {error.Message}");
                return false;
            }
            catch (Exception error)
            {
                // Handle any other errors
                LogError($"Unexpected Error: {error.Message}");
                return false;
            }
        }

        private bool GetInstalledExtensions(DSIInfo dsiInfo)
        {
            dsiInfo.InstalledWidgets = new List<string>();
            dsiInfo.RotatorWidgets = new List<string>();

            foreach (var extension in dsiAvailableExtensions)
            {
                if (_CPH.ActionExists(extension.Name))
                {
                    dsiInfo.InstalledWidgets.Add(extension.Name);

                    if (extension.IsRotatorEnabled)
                    {
                        dsiInfo.RotatorWidgets.Add(extension.Name);
                    }

                    LogDebug(
                        $"Installed Extension: {extension.Name}, IsRotatorEnabled: {extension.IsRotatorEnabled}"
                    );
                }
                else
                {
                    LogDebug($"Extension not installed: {extension.Name}");
                }
            }

            return true;
        }

        private void ProcessExtensionsFromAPI(List<DSIExtensions> extensions)
        {
            dsiAvailableExtensions = new List<DSIExtensions>();

            foreach (var extension in extensions)
            {
                // Add to the list of all extensions
                dsiAvailableExtensions.Add(extension);

                LogDebug($"Loaded Extensions from API: {extension.ToString()}");
            }
        }

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public void DSIDisposeClient()
        {
            // Disposing our client if it exists
            _httpClient?.Dispose();
        }

        public List<DSIExtensions> dsiAvailableExtensions = new List<DSIExtensions>();

        public class DSIExtensions
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public bool IsRotatorEnabled { get; set; }

            public override string ToString()
            {
                return $"Name: {Name}, ActionID: {ID} IsRotatorEnabled: {IsRotatorEnabled}";
            }
        }
    }
}
