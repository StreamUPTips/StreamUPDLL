using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool AndiIsCameraOnline(string cameraName, string parentSource, int obsConnection, out bool isOnline)
        {
            LogDebug($"Checking if camera [{cameraName}] is online");

            GetObsSourceTransform(parentSource, cameraName, obsConnection, out JObject sourceTransform);

            int height = sourceTransform["height"].Value<int>();
            int width = sourceTransform["width"].Value<int>();

            if (height <= 0 && width <= 0)
            {
                LogDebug($"Camera [{cameraName}] is offline or not configured properly. Height: {height}, Width: {width}");
                isOnline = false;
                return false;
            }
            
            isOnline = true;

            LogDebug($"Camera [{cameraName}] is online: {isOnline}");
            return true;
        }
        
    }
}