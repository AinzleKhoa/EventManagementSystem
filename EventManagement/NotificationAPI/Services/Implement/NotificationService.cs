using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using NotificationAPI.DTOs;
using NotificationAPI.Hubs;
using NotificationAPI.Models;
using NotificationAPI.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationAPI.Services.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository repository, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ReadNotificationDTO> CreateNotificationAsync(CreateNotificationDTO dto)
        {
            var notification = _mapper.Map<Notification>(dto);
            var createdNotification = await _repository.AddAsync(notification);
            var resultDto = _mapper.Map<ReadNotificationDTO>(createdNotification);

            await _hubContext.Clients.Group($"user_{dto.UserId}")
                             .SendAsync("ReceiveNewNotification", resultDto);

            return resultDto;
        }

        public async Task<List<ReadNotificationDTO>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ReadNotificationDTO>>(notifications);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            return await _repository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _repository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            return await _repository.DeleteAsync(notificationId, userId);
        }

        public async Task DeleteAllUserNotificationsAsync(int userId)
        {
            await _repository.DeleteAllByUserIdAsync(userId);
        }
    }
}
