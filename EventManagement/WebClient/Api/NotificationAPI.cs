namespace WebClient.Api
{
    public static class NotificationAPI
    {
        public const string NotificationBase = "notification-gateway/notifications";
        public const string UnreadCount = "notification-gateway/notifications/unread-count";
        public const string MarkAsRead = "notification-gateway/notifications/{id}/read";
        public const string MarkAllAsRead = "notification-gateway/notifications/read-all";
    }
}
