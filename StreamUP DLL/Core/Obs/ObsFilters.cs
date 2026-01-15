using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - FILTERS
        // Methods for managing source filters
        // ============================================================

        #region Filter List

        /// <summary>
        /// Gets a list of all filters on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of filter objects, or null if failed</returns>
        public JArray ObsGetSourceFilterList(string sourceName, int connection = 0)
        {
            var response = ObsSendRequest("GetSourceFilterList", new { sourceName }, connection);
            return response?["filters"] as JArray;
        }

        /// <summary>
        /// Gets a list of all available filter kinds.
        /// </summary>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of filter kind strings, or null if failed</returns>
        public JArray ObsGetSourceFilterKindList(int connection = 0)
        {
            var response = ObsSendRequest("GetSourceFilterKindList", null, connection);
            return response?["sourceFilterKinds"] as JArray;
        }

        #endregion

        #region Filter Info

        /// <summary>
        /// Gets detailed info about a specific filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Object with filterEnabled, filterIndex, filterKind, filterSettings, or null if failed</returns>
        public JObject ObsGetSourceFilter(
            string sourceName,
            string filterName,
            int connection = 0
        ) => ObsSendRequest("GetSourceFilter", new { sourceName, filterName }, connection);

        /// <summary>
        /// Gets the default settings for a filter kind.
        /// </summary>
        /// <param name="filterKind">Filter kind to get defaults for</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Default settings object, or null if failed</returns>
        public JObject ObsGetSourceFilterDefaultSettings(string filterKind, int connection = 0)
        {
            var response = ObsSendRequest(
                "GetSourceFilterDefaultSettings",
                new { filterKind },
                connection
            );
            return response?["defaultFilterSettings"] as JObject;
        }

        #endregion

        #region Filter Enable/Disable

        /// <summary>
        /// Sets the enabled state of a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="enabled">True to enable, false to disable</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceFilterEnabled(
            string sourceName,
            string filterName,
            bool enabled,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetSourceFilterEnabled",
                new
                {
                    sourceName,
                    filterName,
                    filterEnabled = enabled
                },
                connection
            );

        /// <summary>
        /// Enables a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsEnableSourceFilter(
            string sourceName,
            string filterName,
            int connection = 0
        ) => ObsSetSourceFilterEnabled(sourceName, filterName, true, connection);

        /// <summary>
        /// Disables a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsDisableSourceFilter(
            string sourceName,
            string filterName,
            int connection = 0
        ) => ObsSetSourceFilterEnabled(sourceName, filterName, false, connection);

        /// <summary>
        /// Toggles the enabled state of a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New enabled state (true if now enabled), or null if failed</returns>
        public bool? ObsToggleSourceFilter(string sourceName, string filterName, int connection = 0)
        {
            var info = ObsGetSourceFilter(sourceName, filterName, connection);
            if (info == null)
                return null;
            bool currentState = info["filterEnabled"]?.Value<bool>() ?? false;
            bool newState = !currentState;
            if (ObsSetSourceFilterEnabled(sourceName, filterName, newState, connection))
                return newState;
            return null;
        }

        /// <summary>
        /// Checks if a filter on a source is enabled.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if enabled, false if disabled or not found</returns>
        public bool ObsIsSourceFilterEnabled(
            string sourceName,
            string filterName,
            int connection = 0
        )
        {
            var info = ObsGetSourceFilter(sourceName, filterName, connection);
            return info?["filterEnabled"]?.Value<bool>() ?? false;
        }

        #endregion

        #region Filter Settings

        /// <summary>
        /// Sets the settings of a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="filterSettings">Settings object to apply</param>
        /// <param name="overlay">True to merge with existing settings, false to replace</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceFilterSettings(
            string sourceName,
            string filterName,
            object filterSettings,
            bool overlay = true,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetSourceFilterSettings",
                new
                {
                    sourceName,
                    filterName,
                    filterSettings,
                    overlay
                },
                connection
            );

        #endregion

        #region Filter Management

        /// <summary>
        /// Creates a new filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source to add the filter to</param>
        /// <param name="filterName">Name for the new filter</param>
        /// <param name="filterKind">Kind of filter to create</param>
        /// <param name="filterSettings">Optional settings for the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsCreateSourceFilter(
            string sourceName,
            string filterName,
            string filterKind,
            object filterSettings = null,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "CreateSourceFilter",
                new
                {
                    sourceName,
                    filterName,
                    filterKind,
                    filterSettings
                },
                connection
            );

        /// <summary>
        /// Removes a filter from a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter to remove</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRemoveSourceFilter(
            string sourceName,
            string filterName,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "RemoveSourceFilter",
                new { sourceName, filterName },
                connection
            );

        /// <summary>
        /// Renames a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Current name of the filter</param>
        /// <param name="newFilterName">New name for the filter</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRenameSourceFilter(
            string sourceName,
            string filterName,
            string newFilterName,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetSourceFilterName",
                new
                {
                    sourceName,
                    filterName,
                    newFilterName
                },
                connection
            );

        /// <summary>
        /// Sets the index (order) of a filter on a source.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the filter</param>
        /// <param name="filterIndex">New index (0 is first)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceFilterIndex(
            string sourceName,
            string filterName,
            int filterIndex,
            int connection = 0
        ) =>
            ObsSendRequestNoResponse(
                "SetSourceFilterIndex",
                new
                {
                    sourceName,
                    filterName,
                    filterIndex
                },
                connection
            );

        #endregion

        #region Common Filter Shortcuts

        /// <summary>
        /// Sets the opacity of a Color Correction filter.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the Color Correction filter</param>
        /// <param name="opacity">Opacity value (0.0 to 1.0)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetFilterOpacity(
            string sourceName,
            string filterName,
            double opacity,
            int connection = 0
        ) => ObsSetSourceFilterSettings(sourceName, filterName, new { opacity }, true, connection);

        /// <summary>
        /// Sets the saturation of a Color Correction filter.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the Color Correction filter</param>
        /// <param name="saturation">Saturation value (-1.0 to 1.0, where 0 is normal)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetFilterSaturation(
            string sourceName,
            string filterName,
            double saturation,
            int connection = 0
        ) =>
            ObsSetSourceFilterSettings(
                sourceName,
                filterName,
                new { saturation },
                true,
                connection
            );

        /// <summary>
        /// Sets the brightness of a Color Correction filter.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the Color Correction filter</param>
        /// <param name="brightness">Brightness value (-1.0 to 1.0, where 0 is normal)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetFilterBrightness(
            string sourceName,
            string filterName,
            double brightness,
            int connection = 0
        ) =>
            ObsSetSourceFilterSettings(
                sourceName,
                filterName,
                new { brightness },
                true,
                connection
            );

        /// <summary>
        /// Sets the contrast of a Color Correction filter.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the Color Correction filter</param>
        /// <param name="contrast">Contrast value (-4.0 to 4.0, where 0 is normal)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetFilterContrast(
            string sourceName,
            string filterName,
            double contrast,
            int connection = 0
        ) => ObsSetSourceFilterSettings(sourceName, filterName, new { contrast }, true, connection);

        /// <summary>
        /// Sets the hue shift of a Color Correction filter.
        /// </summary>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="filterName">Name of the Color Correction filter</param>
        /// <param name="hueShift">Hue shift in degrees (-180 to 180)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetFilterHueShift(
            string sourceName,
            string filterName,
            double hueShift,
            int connection = 0
        ) =>
            ObsSetSourceFilterSettings(
                sourceName,
                filterName,
                new { hue_shift = hueShift },
                true,
                connection
            );

        #endregion
    }
}
