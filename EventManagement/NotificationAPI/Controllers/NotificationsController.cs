using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationAPI.DTOs;
using NotificationAPI.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NotificationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaim = identity.FindFirst("sub")
                             ?? identity.FindFirst(ClaimTypes.NameIdentifier)
                             ?? identity.FindFirst("UserId");

                if (userClaim != null && int.TryParse(userClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return -1;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            int userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized(new { message = "Vui lòng đăng nhập." });

            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            try
            {
                int count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { count = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // 3. ĐÁNH DẤU 1 THÔNG BÁO LÀ "ĐÃ ĐỌC"
        // PUT: api/Notifications/{id}/read
        // ==========================================
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            try
            {
                var success = await _notificationService.MarkAsReadAsync(id, userId);
                if (!success) return NotFound(new { message = "Notification not found." });
                return Ok(new { success = true, message = "Đã đánh dấu đọc." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            try
            {
                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { success = true, message = "Đã đánh dấu đọc tất cả." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDTO dto)
        {
            try
            {
                var result = await _notificationService.CreateNotificationAsync(dto);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            try
            {
                var success = await _notificationService.DeleteNotificationAsync(id, userId);
                if (!success) return NotFound(new { message = "Notification not found." });
                return Ok(new { success = true, message = "Notification deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            int userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            try
            {
                await _notificationService.DeleteAllUserNotificationsAsync(userId);
                return Ok(new { success = true, message = "All notifications deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
