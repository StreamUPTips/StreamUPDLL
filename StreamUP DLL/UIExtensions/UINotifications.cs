namespace StreamUP
{
    public partial class StreamUpLib
    {
        // UI Notifications
        public bool ShowToastNotification(NotificationType type, string title, string message, string attribution = null)
        {
            // Determine the appropriate icon based on the notification type
            string icon = GetIconForNotificationType(type);

            // Show toast notification
            _CPH.ShowToastNotification(title, message, attribution, icon);

            return true;
        }

        private string GetIconForNotificationType(NotificationType type)
        {
            // Return the appropriate icon URL based on the notification type
            switch (type)
            {
                case NotificationType.Success:
                    return "https://api.iconify.design/emojione-v1:left-check-mark.svg";
                case NotificationType.Error:
                    return "https://api.iconify.design/emojione-v1:cross-mark.svg";
                case NotificationType.Warning:
                    return "https://api.iconify.design/emojione-v1:warning.svg";
                case NotificationType.Info:
                    return "https://api.iconify.design/emojione-v1:information.svg";
                default:
                    return null;
            }
        }

        public enum NotificationType
        {
            Success,
            Error,
            Warning,
            Info
        }
    }
}
