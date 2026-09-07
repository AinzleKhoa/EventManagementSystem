using NotificationAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationAPI.Services
{
    public interface INotificationService
    {
        Task<ReadNotificationDTO> CreateNotificationAsync(CreateNotificationDTO dto);
        Task<List<ReadNotificationDTO>> GetUserNotificationsAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
        Task DeleteAllUserNotificationsAsync(int userId);
    }
}
