using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Check OBS connection and plugins
        public bool CheckCanvasResolution(int baseWidth, int baseHeight)
        {
            // Check if the resolution is 16:9
            float aspectRatio = (float)baseWidth / baseHeight;
            const float expectedRatio = 16f / 9f;

            // Allow some tolerance for floating-point precision
            const float tolerance = 0.01f;
            
            if (Math.Abs(aspectRatio - expectedRatio) > tolerance)
            {
                LogError($"The canvas size is not 16:9. Current aspect ratio is [{aspectRatio:F2}]");
                return false;
            }       

            return true; 
        }
    }
}
