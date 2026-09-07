using Microsoft.EntityFrameworkCore;
using NotificationAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationAPI.Repositories.Implement
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context; 

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            // Lấy thông báo của User, ưu tiên cái mới nhất lên đầu
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
            if (notif == null) return false;

            if (notif != null && (notif.IsRead == false || notif.IsRead == null))
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadNotifs = await _context.Notifications
                .Where(n => n.UserId == userId && (n.IsRead == false || n.IsRead == null))
                .ToListAsync();

            if (unreadNotifs.Any())
            {
                foreach (var notif in unreadNotifs)
                {
                    notif.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && (n.IsRead == false || n.IsRead == null));
        }

        public async Task<bool> DeleteAsync(int notificationId, int userId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
            if (notif == null) return false;

            _context.Notifications.Remove(notif);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAllByUserIdAsync(int userId)
        {
            var notifs = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifs.Any())
            {
                _context.Notifications.RemoveRange(notifs);
                await _context.SaveChangesAsync();
            }
        }
    }
}
