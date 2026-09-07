using AuthenticationAPI.DTOs;
using AuthenticationAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationAPI.Controllers
{
    [Route("api/profile")] 
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ICloudinaryService _cloudinaryService;

        public ProfileController(IProfileService profileService, ICloudinaryService cloudinaryService)
        {
            _profileService = profileService;
            _cloudinaryService = cloudinaryService;
        }

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            // Trích xuất UserId từ Token
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var profile = await _profileService.GetProfile(userId);

            if (profile == null)
                return NotFound("User not found");

            return Ok(profile);
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            int userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
            var result = await _profileService.UpdateProfile(userId, dto);

            return Ok(result);
        }

        // PUT: api/profile/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null) return Unauthorized();

                int userId = int.Parse(userIdClaim.Value);

                await _profileService.ChangePassword(userId, dto);

                return Ok("Password changed successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/profile/avatar
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a valid image file.");
            }

            int userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            try
            {
                // 1. Tạo tên file duy nhất và cố định cho user này
                string fileName = $"avatar_user_{userId}";

                // 2. Truyền fileName vào tham số thứ 3 để Cloudinary ghi đè file cũ
                var avatarUrl = await _cloudinaryService.UploadImageAsync(file, "UserAvatars", fileName);

                if (string.IsNullOrEmpty(avatarUrl))
                {
                    return BadRequest("Error uploading image to Cloudinary.");
                }

                // 3. Cập nhật DB
                var result = await _profileService.UpdateAvatarUrl(userId, avatarUrl);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // In production, log the exception using ILogger
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Users/is-google-account/5
        [HttpGet("is-google-account")]
        public async Task<IActionResult> IsGoogleAccount()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var isGoogle = await _profileService.IsGoogleAccount(userId);
            return Ok(isGoogle);
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var jtiClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);
            var expClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp);

            if (jtiClaim != null && expClaim != null)
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime;
                var remainingTime = expTime - DateTime.UtcNow;

                if (remainingTime > TimeSpan.Zero)
                {
                    await _profileService.Logout(jtiClaim.Value, remainingTime);
                }

                return Ok(new { message = "Logged out from this device successfully" });
            }

            return BadRequest(new { message = "Invalid token claims" });
        }

        // POST: api/profile/become-organizer/request
        [HttpPost("become-organizer/request")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> RequestBecomeOrganizer()
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null) return Unauthorized();

                int userId = int.Parse(userIdClaim.Value);

                await _profileService.RequestBecomeOrganizer(userId);

                return Ok(new { message = "OTP has been sent to your email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/profile/become-organizer/verify
        [HttpPost("become-organizer/verify")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> VerifyBecomeOrganizer([FromBody] VerifyOrganizerOtpDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null) return Unauthorized();

                int userId = int.Parse(userIdClaim.Value);

                var result = await _profileService.VerifyBecomeOrganizer(userId, dto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}