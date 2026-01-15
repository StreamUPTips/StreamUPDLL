namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS SCALE FACTOR HELPERS
        // Custom methods for calculating scale factors and canvas dimensions
        // ============================================================

        /// <summary>
        /// Gets the OBS canvas scale factor relative to 1920px reference width.
        /// This is useful for scaling elements proportionally based on canvas size.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scale factor (e.g., 1.0 for 1920px, 2.0 for 3840px, 0.667 for 1280px), or 1.0 if unable to retrieve</returns>
        public double GetObsScaleFactor(int connection = 0)
        {
            return GetObsScaleFactor(1920, connection);
        }

        /// <summary>
        /// Gets the OBS canvas scale factor relative to a custom reference width.
        /// </summary>
        /// <param name="referenceWidth">The reference width to calculate scale against (default: 1920)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scale factor, or 1.0 if unable to retrieve video settings</returns>
        public double GetObsScaleFactor(int referenceWidth, int connection = 0)
        {
            // Get video settings from OBS
            var videoSettings = ObsGetVideoSettings(connection);

            if (videoSettings == null)
            {
                LogError("Unable to retrieve OBS video settings for scale factor calculation");
                return 1.0;
            }

            // Get base (canvas) width
            int? baseWidth = videoSettings["baseWidth"]?.Value<int>();

            if (!baseWidth.HasValue || baseWidth.Value <= 0)
            {
                LogError("Invalid baseWidth in OBS video settings");
                return 1.0;
            }

            // Calculate scale factor
            double scaleFactor = (double)baseWidth.Value / referenceWidth;

            LogDebug($"OBS scale factor calculated: {scaleFactor} (baseWidth: {baseWidth.Value}, reference: {referenceWidth})");

            return scaleFactor;
        }

        /// <summary>
        /// Gets the OBS canvas dimensions.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <param name="baseWidth">Output: Canvas width in pixels</param>
        /// <param name="baseHeight">Output: Canvas height in pixels</param>
        /// <returns>True if successful, false if unable to retrieve</returns>
        public bool GetObsCanvasSize(int connection, out int baseWidth, out int baseHeight)
        {
            baseWidth = 0;
            baseHeight = 0;

            var videoSettings = ObsGetVideoSettings(connection);

            if (videoSettings == null)
            {
                LogError("Unable to retrieve OBS video settings for canvas size");
                return false;
            }

            int? width = videoSettings["baseWidth"]?.Value<int>();
            int? height = videoSettings["baseHeight"]?.Value<int>();

            if (!width.HasValue || !height.HasValue)
            {
                LogError("Invalid canvas dimensions in OBS video settings");
                return false;
            }

            baseWidth = width.Value;
            baseHeight = height.Value;

            LogDebug($"OBS canvas size: {baseWidth}x{baseHeight}");
            return true;
        }

        /// <summary>
        /// Gets the OBS output (scaled) resolution.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <param name="outputWidth">Output: Scaled output width in pixels</param>
        /// <param name="outputHeight">Output: Scaled output height in pixels</param>
        /// <returns>True if successful, false if unable to retrieve</returns>
        public bool GetObsOutputSize(int connection, out int outputWidth, out int outputHeight)
        {
            outputWidth = 0;
            outputHeight = 0;

            var videoSettings = ObsGetVideoSettings(connection);

            if (videoSettings == null)
            {
                LogError("Unable to retrieve OBS video settings for output size");
                return false;
            }

            int? width = videoSettings["outputWidth"]?.Value<int>();
            int? height = videoSettings["outputHeight"]?.Value<int>();

            if (!width.HasValue || !height.HasValue)
            {
                LogError("Invalid output dimensions in OBS video settings");
                return false;
            }

            outputWidth = width.Value;
            outputHeight = height.Value;

            LogDebug($"OBS output size: {outputWidth}x{outputHeight}");
            return true;
        }
    }
}
