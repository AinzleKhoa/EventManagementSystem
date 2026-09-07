using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDTO>> GetMyNotificationsAsync();
        Task<int> GetUnreadCountAsync();
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync();
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<bool> DeleteAllNotificationsAsync();
    }
}