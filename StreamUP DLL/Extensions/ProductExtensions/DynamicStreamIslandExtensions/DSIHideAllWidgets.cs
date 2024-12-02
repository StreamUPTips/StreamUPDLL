using System.Collections.Generic;
using Newtonsoft.Json;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool DSIHideAllWidgets(int obsConnection)
        {
            LogDebug("Hiding all DSI widgets...");

            string jsonResponse = _CPH.ObsSendRaw("GetSceneItemList", "{\"sceneName\":\"StreamUP Widgets • Dynamic Stream-Island\"}", obsConnection);

            // Parse the JSON response
            var sceneItems = JsonConvert.DeserializeObject<SceneItemList>(jsonResponse);

            if (sceneItems?.SceneItems != null)
            {
                foreach (var item in sceneItems.SceneItems)
                {
                    if (item.IsGroup == true)
                    {
                        string sourceName = item.SourceName;
                        _CPH.ObsHideSource("StreamUP Widgets • Dynamic Stream-Island", sourceName, obsConnection);
                        LogDebug($"Hiding source: {sourceName}");
                    }
                }
            }
            else
            {
                LogError("No scene items found in the response.");
            }

            LogDebug("All widgets hidden successfully.");
            return true;
        }
    }
}

// Classes for deserializing the JSON
public class SceneItemList
{
    [JsonProperty("sceneItems")]
    public List<SceneItem> SceneItems { get; set; }
}

public class SceneItem
{
    [JsonProperty("isGroup")]
    public bool? IsGroup { get; set; }

    [JsonProperty("sourceName")]
    public string SourceName { get; set; }
}

