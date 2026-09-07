using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClient.Services;
using WebClient.ViewModel;

namespace WebClient.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                int count = await _notificationService.GetUnreadCountAsync();
                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetMyNotificationsAsync();

                var viewModels = _mapper.Map<List<NotificationViewModel>>(notifications);

                return Json(new { success = true, data = viewModels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 3. ĐÁNH DẤU 1 THÔNG BÁO ĐÃ ĐỌC
        // Gọi bằng AJAX: POST /Notification/MarkAsRead?id=1
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(id);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 4. ĐÁNH DẤU TẤT CẢ ĐÃ ĐỌC
        // Gọi bằng AJAX: POST /Notification/MarkAllAsRead
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var success = await _notificationService.MarkAllAsReadAsync();
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 5. XÓA 1 THÔNG BÁO
        // Gọi bằng AJAX: DELETE /Notification/DeleteNotification?id=1
        // ==========================================
        [HttpDelete]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var success = await _notificationService.DeleteNotificationAsync(id);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 6. XÓA TẤT CẢ THÔNG BÁO CỦA USER
        // Gọi bằng AJAX: DELETE /Notification/DeleteAllNotifications
        // ==========================================
        [HttpDelete]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            try
            {
                var success = await _notificationService.DeleteAllNotificationsAsync();
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}