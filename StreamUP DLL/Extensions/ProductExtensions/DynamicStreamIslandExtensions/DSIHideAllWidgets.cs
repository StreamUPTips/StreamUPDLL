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
                        if (!_CPH.ObsIsSourceVisible("StreamUP Widgets • Dynamic Stream-Island", sourceName, obsConnection))
                        {
                            continue;
                        }

                        switch (sourceName)
                        {
                            case "DSI • Alerts • Group":
                                _CPH.ObsShowFilter("StreamUP Widgets • Dynamic Stream-Island", "DSI • Alerts • Group", "Opacity • OFF", obsConnection);
                                LogDebug("Hiding source: DSI • Alerts • Group");
                                break;
                            case "DSI • Goal Bar • Text FG Group":
                            case "DSI • Goal Bar • Text BG Group":
                                _CPH.ObsShowFilter(sceneName, "DSI • BG Filler", "[MV] Advanced Mask: Position X", obsConnection);
                                _CPH.ObsShowFilter(sceneName, "DSI • Goal Bar • Text Bottom FG", "[MV] Update Number", obsConnection);
                                _CPH.ObsShowFilter(sceneName, "DSI • Goal Bar • Text Bottom BG", "[MV] Update Number", obsConnection);
                                if (!GetObsMoveFilterDuration("DSI • BG Filler", "[MV] Advanced Mask: Position X", obsConnection, out int goalBarAnimationDuration))
                                {
                                    goalBarAnimationDuration = 1000;
                                }
                                _CPH.Wait(goalBarAnimationDuration);
                                _CPH.ObsHideSource(sceneName, "DSI • Goal Bar • Text FG Group", obsConnection);
                                _CPH.ObsHideSource(sceneName, "DSI • Goal Bar • Text BG Group", obsConnection);
                                _CPH.ObsHideSource(sceneName, "DSI • BG Filler • Group", obsConnection);
                                break;
                            case "DSI • BG Filler • Group":
                                break;
                            default:
                                _CPH.ObsHideSource("StreamUP Widgets • Dynamic Stream-Island", sourceName, obsConnection);
                                LogDebug($"Hiding source: {sourceName}");
                                break;
                        }
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

