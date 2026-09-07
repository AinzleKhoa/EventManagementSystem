using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClient.DTOs.Request;
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ISocialService _socialService;
        private readonly IEventService _eventService;
        private readonly IOrderService _orderService;
        private readonly IAuthService _userService;

        public ReviewController(ISocialService socialService, IEventService eventService, IOrderService orderService, IAuthService userService)
        {
            _socialService = socialService;
            _eventService = eventService;
            _orderService = orderService;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> EventSnapshot(int eventId, string sortOrder = "newest")
        {
            string odataQuery = sortOrder switch
            {
                "oldest" => "?$orderby=CreatedAt asc",
                "highest" => "?$orderby=Rating desc, CreatedAt desc",
                "lowest" => "?$orderby=Rating asc, CreatedAt desc",
                _ => "?$orderby=CreatedAt desc"
            };

            var reviews = await _socialService.GetReviewsByEventId(eventId, odataQuery);

            if (reviews.Any())
            {
                var userTasks = reviews
                    .Select(r => r.UserId)
                    .Distinct()
                    .Select(async userId => new { UserId = userId, User = await _userService.GetUserById(userId) });

                var users = await Task.WhenAll(userTasks);
                var userMap = users
                    .Where(u => u.User != null)
                    .ToDictionary(u => u.UserId, u => u.User!);

                foreach (var review in reviews)
                {
                    if (userMap.TryGetValue(review.UserId, out var userInfo))
                    {
                        review.UserName = !string.IsNullOrEmpty(userInfo.FullName) ? userInfo.FullName : $"User #{review.UserId}";
                        review.AvatarUrl = !string.IsNullOrEmpty(userInfo.AvatarUrl) ? userInfo.AvatarUrl : $"/images/default-avatar.png";
                    }
                    else
                    {
                        review.UserName = "Anonymous";
                        review.AvatarUrl = "/images/default-avatar.png";
                    }
                }
            }

            var currentUserId = GetCurrentUserId();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return Json(new
            {
                success = true,
                averageRating,
                totalReviews = reviews.Count,
                hasReviewed = currentUserId > 0 && reviews.Any(r => r.UserId == currentUserId),
                reviews
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDTO request)
        {
            if (request == null || request.Rating == 0)
            {
                return Json(new { success = false, message = "Invalid data. Please provide a valid rating." });
            }

            try
            {
                var eventData = await _eventService.GetById(request.EventId);

                if (eventData == null)
                {
                    return Json(new { success = false, message = "Event not found." });
                }

                if (DateTime.Now <= eventData.EndDate)
                {
                    return Json(new { success = false, message = "You can only review this event after it has ended." });
                }

                var hasPaidTicket = await _orderService.CheckUserHasPaidTicket(request.EventId);
                if (!hasPaidTicket)
                {
                    return Json(new { success = false, message = "You need to book a ticket for this event before leaving a review." });
                }

                await _socialService.CreateReview(request);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDTO request)
        {
            if (request == null || request.Rating == 0)
            {
                return Json(new { success = false, message = "Invalid data. Please provide a valid rating." });
            }

            try
            {
                var success = await _socialService.UpdateReview(id, request);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating review: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 3. DELETE REVIEW
        // URL: DELETE /Review/DeleteReview?id={id}
        // ==========================================
        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var success = await _socialService.DeleteReview(id);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting review: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var userClaim = identity?.FindFirst("sub")
                         ?? identity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                         ?? identity?.FindFirst("UserId");

            return userClaim != null && int.TryParse(userClaim.Value, out var userId) ? userId : -1;
        }
    }
}
