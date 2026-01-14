namespace StreamUP
{
    /// <summary>
    /// Types of modern dialogs available
    /// </summary>
    public enum DialogType
    {
        /// <summary>
        /// Error dialog - Red header, OK button
        /// </summary>
        Error,

        /// <summary>
        /// Warning dialog - Orange/amber header, OK button
        /// </summary>
        Warning,

        /// <summary>
        /// Prompt dialog - Blue header, Yes/No buttons
        /// </summary>
        Prompt,

        /// <summary>
        /// Success dialog - Green header, OK button
        /// </summary>
        Success,

        /// <summary>
        /// Info dialog - Blue header, OK button
        /// </summary>
        Info
    }

    /// <summary>
    /// Result from a modern dialog
    /// </summary>
    public class ModernDialogResult
    {
        /// <summary>
        /// True if user clicked Yes/OK/Accept
        /// </summary>
        public bool Accepted { get; set; }

        /// <summary>
        /// True if user clicked No/Cancel/X
        /// </summary>
        public bool Declined { get; set; }
    }
}
