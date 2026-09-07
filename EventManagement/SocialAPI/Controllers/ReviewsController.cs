using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.SignalR;
using SocialAPI.DTOs;
using SocialAPI.Hubs;
using SocialAPI.Services;
using System.Security.Claims;

namespace SocialAPI.Controllers
{
    public class ReviewsController : ODataController
    {
        private readonly ISocialService _socialService;
        private readonly IHubContext<ReviewHub> _reviewHub;
        private readonly HttpClient _gatewayClient;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            ISocialService socialService,
            IHubContext<ReviewHub> reviewHub,
            IHttpClientFactory httpClientFactory,
            ILogger<ReviewsController> logger)
        {
            _socialService = socialService;
            _reviewHub = reviewHub;
            _gatewayClient = httpClientFactory.CreateClient("GatewayAPI");
            _logger = logger;
        }

        [HttpPost("api/Reviews")]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return BadRequest(new { message = firstError });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (userId == -1) return Unauthorized();

                dto.UserId = userId;

                var result = await _socialService.CreateReviewAsync(dto);
                await BroadcastReviewsChanged(result.EventId, "created", result.ReviewId);
                await NotifyOrganizerForNewReview(result.EventId, result.Rating);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("api/Reviews/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return BadRequest(new { message = firstError });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (userId == -1) return Unauthorized();

                var eventId = _socialService.GetAllReviewsQueryable()
                    .Where(r => r.ReviewId == id && r.UserId == userId)
                    .Select(r => r.EventId)
                    .FirstOrDefault();

                var success = await _socialService.UpdateReviewAsync(id, userId, dto);

                if (!success)
                {
                    return BadRequest(new { message = "Unable to update review (Review not found or you do not have permission)." });
                }

                if (eventId > 0)
                {
                    await BroadcastReviewsChanged(eventId, "updated", id);
                }

                return Ok(new { success = true, message = "Review updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("api/Reviews/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == -1) return Unauthorized();

                var eventId = _socialService.GetAllReviewsQueryable()
                    .Where(r => r.ReviewId == id && r.UserId == userId)
                    .Select(r => r.EventId)
                    .FirstOrDefault();

                var success = await _socialService.DeleteReviewAsync(id, userId);

                if (!success)
                {
                    return BadRequest(new { message = "Unable to delete review (Review not found or you do not have permission)." });
                }

                if (eventId > 0)
                {
                    await BroadcastReviewsChanged(eventId, "deleted", id);
                }

                return Ok(new { success = true, message = "Review deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_socialService.GetAllReviewsQueryable());
        }

        [HttpGet("api/Reviews/event/{eventId}")]
        [EnableQuery]
        public IActionResult GetReviewsByEvent(int eventId)
        {
            return Ok(_socialService.GetEventReviewsQueryable(eventId));
        }

        [HttpGet("api/Reviews/event/{eventId}/rating")]
        public async Task<IActionResult> GetEventRating(int eventId)
        {
            var rating = await _socialService.GetEventRatingAsync(eventId);
            return Ok(new { EventId = eventId, AverageRating = rating });
        }

        private int GetCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaim = identity.FindFirst(ClaimTypes.NameIdentifier)
                             ?? identity.FindFirst("sub")
                             ?? identity.FindFirst("UserId");

                if (userClaim != null && int.TryParse(userClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return -1;
        }

        private async Task BroadcastReviewsChanged(int eventId, string action, int reviewId)
        {
            var reviews = _socialService.GetEventReviewsQueryable(eventId).ToList();
            var averageRating = await _socialService.GetEventRatingAsync(eventId);

            await _reviewHub.Clients
                .Group(ReviewHub.GetEventGroupName(eventId))
                .SendAsync("ReviewsChanged", new
                {
                    eventId,
                    action,
                    reviewId,
                    averageRating,
                    totalReviews = reviews.Count
                });
        }

        private async Task NotifyOrganizerForNewReview(int eventId, int rating)
        {
            try
            {
                var eventInfo = await _gatewayClient.GetFromJsonAsync<EventNotificationInfo>($"event-gateway/events/{eventId}");
                if (eventInfo == null || eventInfo.OrganizerId <= 0)
                {
                    return;
                }

                var payload = new CreateNotificationPayload
                {
                    UserId = eventInfo.OrganizerId,
                    Title = "New event review",
                    Message = $"Your event \"{eventInfo.Title}\" received a new {rating}-star review.",
                    Type = "Review"
                };

                var response = await _gatewayClient.PostAsJsonAsync("notification-gateway/notifications", payload);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to create review notification for Event {EventId}. Status={StatusCode}", eventId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify organizer for new review on Event {EventId}.", eventId);
            }
        }

        private class EventNotificationInfo
        {
            public int OrganizerId { get; set; }
            public string? Title { get; set; }
        }

        private class CreateNotificationPayload
        {
            public int UserId { get; set; }
            public string? Title { get; set; }
            public string? Message { get; set; }
            public string? Type { get; set; }
        }
    }
}
