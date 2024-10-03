using System.Globalization;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool SetObsTextGDIFont(string sourceName, string face, int size, string style, int obsConnection)
        {
            _CPH.ObsSendRaw("SetInputSettings", "{\"inputName\":\"" + sourceName + "\",\"inputSettings\":{\"font\":{\"face\":\"" + face + "\",\"size\":" + size + ",\"style\":\"" + style + "\"}},\"overlay\":true}", obsConnection);
            return true;
        }

        public bool SetObsTextGDIColour(string sourceName, long colour, int obsConnection)
        {
            JObject changeOfColour = new()
        {
            {
                "color",
                colour
            }
        };
            SetObsSourceSettings(sourceName, changeOfColour, obsConnection);
            return true;
        }

        public bool SetObsStrokeColour(string sourceName, string filterName, long strokeColour, int obsConnection)
        {

            JObject changeStrokeColour = new()
        {
            {
                "stroke_fill_color",
                strokeColour
            }
        };
            SetObsSourceFilterSettings(sourceName, filterName, changeStrokeColour, 0);
            return true;
        }
    }
}