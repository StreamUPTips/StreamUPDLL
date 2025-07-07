namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Sources Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets the active and show state of a source (input or scene).
        /// <paramref name="sourceName">Name of the source to get the active state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourceactive
        /// </summary>
        public JObject ObsRawGetSourceActive(string sourceName) => BuildObsRequest("GetSourceActive", new { sourceName });

        /// <summary>
        /// Gets a Base64-encoded screenshot of a source (input or scene).
        /// <paramref name="sourceName">Name of the source to take a screenshot of.</paramref>
        /// <paramref name="imageFormat">Image compression format to use.</paramref>
        /// <paramref name="imageWidth">Width to scale the screenshot to.</paramref>
        /// <paramref name="imageHeight">Height to scale the screenshot to.</paramref>
        /// <paramref name="imageCompressionQuality">Compression quality to use (0-100).</paramref>
        /// <paramref name="imageFilePath">Path to save the screenshot file to (optional).</paramref>
        /// <paramref name="imageEmbed">Whether to embed the image in the response (optional).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsourcescreenshot
        /// </summary>
        public JObject ObsRawGetSourceScreenshot(string sourceName, string imageFormat, int imageWidth = 0, int imageHeight = 0, int imageCompressionQuality = 100, string imageFilePath = null, bool? imageEmbed = null) => BuildObsRequest("GetSourceScreenshot", new { sourceName, imageFormat, imageWidth, imageHeight, imageCompressionQuality, imageFilePath, imageEmbed });

        /// <summary>
        /// Saves a screenshot of a source to the filesystem.
        /// <paramref name="sourceName">Name of the source to take a screenshot of.</paramref>
        /// <paramref name="imageFormat">Image compression format to use.</paramref>
        /// <paramref name="imageFilePath">Path to save the screenshot file to.</paramref>
        /// <paramref name="imageWidth">Width to scale the screenshot to.</paramref>
        /// <paramref name="imageHeight">Height to scale the screenshot to.</paramref>
        /// <paramref name="imageCompressionQuality">Compression quality to use (0-100).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#savesourcescreenshot
        /// </summary>
        public JObject ObsRawSaveSourceScreenshot(string sourceName, string imageFormat, string imageFilePath, int imageWidth = 0, int imageHeight = 0, int imageCompressionQuality = 100) => BuildObsRequest("SaveSourceScreenshot", new { sourceName, imageFormat, imageFilePath, imageWidth, imageHeight, imageCompressionQuality });
    }
}
