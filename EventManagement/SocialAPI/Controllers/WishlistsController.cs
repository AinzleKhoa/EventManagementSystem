using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query; // THÊM USING NÀY
using Microsoft.AspNetCore.OData.Routing.Controllers; // KẾ THỪA TỪ ĐÂY
using SocialAPI.DTOs;
using SocialAPI.Services;
using System.Security.Claims;

namespace SocialAPI.Controllers
{
    [Authorize] 
    public class WishlistsController : ODataController 
    {
        private readonly ISocialService _socialService;

        public WishlistsController(ISocialService socialService)
        {
            _socialService = socialService;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_socialService.GetAllWishlistsQueryable());
        }

        // GET: api/Wishlists/my-wishlist
        [HttpGet("api/Wishlists/my-wishlist")]
        [EnableQuery] 
        public IActionResult GetMyWishlist()
        {
            var userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            return Ok(_socialService.GetUserWishlistsQueryable(userId));
        }

        // POST: api/Wishlists
        [HttpPost("api/Wishlists")]
        public async Task<IActionResult> AddToWishlist([FromBody] CreateWishlistDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == -1) return Unauthorized();

                dto.UserId = userId;

                var result = await _socialService.AddToWishlistAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Wishlists/{eventId}
        [HttpDelete("api/Wishlists/{eventId}")]
        public async Task<IActionResult> RemoveFromWishlist(int eventId)
        {
            var userId = GetCurrentUserId();
            if (userId == -1) return Unauthorized();

            var success = await _socialService.RemoveFromWishlistAsync(userId, eventId);

            if (!success) return NotFound(new { message = "Event not found in wishlist" });

            return NoContent();
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
    }
}