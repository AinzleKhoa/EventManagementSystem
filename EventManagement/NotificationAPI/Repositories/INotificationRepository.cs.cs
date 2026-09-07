using NotificationAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationAPI.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> DeleteAsync(int notificationId, int userId);
        Task DeleteAllByUserIdAsync(int userId);
    }
}
