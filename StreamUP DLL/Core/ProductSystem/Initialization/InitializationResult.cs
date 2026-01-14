namespace StreamUP
{
    /// <summary>
    /// Result of product initialization attempt
    /// </summary>
    public enum InitializationResult
    {
        /// <summary>
        /// Product initialized successfully and is ready to use
        /// </summary>
        Success,

        /// <summary>
        /// Settings file doesn't exist - user was prompted to configure
        /// </summary>
        SettingsNotConfigured,

        /// <summary>
        /// StreamUP DLL version is too old for this product
        /// </summary>
        LibraryOutOfDate,

        /// <summary>
        /// OBS is not connected on the configured connection
        /// </summary>
        ObsNotConnected,

        /// <summary>
        /// Required OBS scene was not found
        /// </summary>
        ObsSceneNotFound,

        /// <summary>
        /// OBS scene version is outdated
        /// </summary>
        ObsSceneOutdated,

        /// <summary>
        /// User was already prompted about this issue this session - silently failing
        /// </summary>
        AlreadyPromptedThisSession
    }
}
