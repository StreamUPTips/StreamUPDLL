using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool DSICentreOBSGroupSource(string groupName, double sourceWidth, int canvasWidth, double scaleFactor, int obsConnection)
        {
            LogDebug("Centering OBS group...");
            string sceneName = "StreamUP Widgets • Dynamic Stream-Island";

            // Calculate new X position to center the source
            double newXPosition = (canvasWidth / 2) - (sourceWidth / 2);
            double newYPosition = 511 * scaleFactor;
            LogDebug($"Calculated new position of: X=[{newXPosition}], Y=[{newYPosition}]");

            // Update Source Position in OBS
            var transformSettings = new JObject
            {
                ["positionX"] = newXPosition,
                ["positionY"] = newYPosition
            };
            SetObsSceneItemTransform(sceneName, groupName, transformSettings, obsConnection);
            _CPH.Wait(50);

            LogDebug("Centred group successfully.");
            return true;
        }

    }
}