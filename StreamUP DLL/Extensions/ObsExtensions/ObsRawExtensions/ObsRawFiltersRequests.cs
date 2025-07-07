namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Filters Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets a list of filters applied to a source.
        /// <paramref name="sourceName">Name of the source to get the filters for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilterlist
        /// </summary>
        public JObject ObsRawGetSourceFilterList(string sourceName) => BuildObsRequest("GetSourceFilterList", new { sourceName });

        /// <summary>
        /// Gets the default settings for a source filter kind.
        /// <paramref name="filterKind">The filter kind to get the default settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilterdefaultsettings
        /// </summary>
        public JObject ObsRawGetSourceFilterDefaultSettings(string filterKind) => BuildObsRequest("GetSourceFilterDefaultSettings", new { filterKind });

        /// <summary>
        /// Gets the settings for a source filter.
        /// <paramref name="sourceName">Name of the source to get the filter settings for.</paramref>
        /// <paramref name="filterName">Name of the filter to get the settings for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefiltersettings
        /// </summary>
        public JObject ObsRawGetSourceFilterSettings(string sourceName, string filterName) => BuildObsRequest("GetSourceFilterSettings", new { sourceName, filterName });

        /// <summary>
        /// Renames a source filter.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to be renamed.</paramref>
        /// <paramref name="newFilterName">New name for the filter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefiltername
        /// </summary>
        public JObject ObsRawSetSourceFilterName(string sourceName, string filterName, string newFilterName) => BuildObsRequest("SetSourceFilterName", new { sourceName, filterName, newFilterName });

        /// <summary>
        /// Sets the index of a source filter.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to set the index for.</paramref>
        /// <paramref name="filterIndex">New index for the filter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefilterindex
        /// </summary>
        public JObject ObsRawSetSourceFilterIndex(string sourceName, string filterName, int filterIndex) => BuildObsRequest("SetSourceFilterIndex", new { sourceName, filterName, filterIndex });

        /// <summary>
        /// Sets the settings for a source filter.
        /// <paramref name="sourceName">Name of the source to set the filter settings for.</paramref>
        /// <paramref name="filterName">Name of the filter to set the settings for.</paramref>
        /// <paramref name="filterSettings">Object containing the new filter settings.</paramref>
        /// <paramref name="overlay">Whether to apply the settings as an overlay.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefiltersettings
        /// </summary>
        public JObject ObsRawSetSourceFilterSettings(string sourceName, string filterName, object filterSettings, bool overlay = false) => BuildObsRequest("SetSourceFilterSettings", new { sourceName, filterName, filterSettings, overlay });

        /// <summary>
        /// Removes a source filter from a source.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#removesourcefilter
        /// </summary>
        public JObject ObsRawRemoveSourceFilter(string sourceName, string filterName) => BuildObsRequest("RemoveSourceFilter", new { sourceName, filterName });

        /// <summary>
        /// Creates a new source filter and adds it to a source.
        /// <paramref name="sourceName">Name of the source to add the filter to.</paramref>
        /// <paramref name="filterName">Name for the new filter.</paramref>
        /// <paramref name="filterKind">The kind of filter to create.</paramref>
        /// <paramref name="filterSettings">Object containing settings for the filter.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#createsourcefilter
        /// </summary>
        public JObject ObsRawCreateSourceFilter(string sourceName, string filterName, string filterKind, object filterSettings = null) => BuildObsRequest("CreateSourceFilter", new { sourceName, filterName, filterKind, filterSettings });

        /// <summary>
        /// Gets a list of all source filter kinds available in OBS.
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilterkindlist
        /// </summary>
        public JObject ObsRawGetSourceFilterKindList() => BuildObsRequest("GetSourceFilterKindList");

        /// <summary>
        /// Gets the settings for a source filter (alias for GetSourceFilterSettings in some protocol versions).
        /// <paramref name="sourceName">Name of the source to get the filter for.</paramref>
        /// <paramref name="filterName">Name of the filter to get.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcefilter
        /// </summary>
        public JObject ObsRawGetSourceFilter(string sourceName, string filterName) => BuildObsRequest("GetSourceFilter", new { sourceName, filterName });

        /// <summary>
        /// Sets the enabled state of a source filter.
        /// <paramref name="sourceName">Name of the source the filter belongs to.</paramref>
        /// <paramref name="filterName">Name of the filter to enable/disable.</paramref>
        /// <paramref name="filterEnabled">Whether the filter should be enabled.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#setsourcefilterenabled
        /// </summary>
        public JObject ObsRawSetSourceFilterEnabled(string sourceName, string filterName, bool filterEnabled) => BuildObsRequest("SetSourceFilterEnabled", new { sourceName, filterName, filterEnabled });
    }
}
