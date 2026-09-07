using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebClient.DTOs.Request;
using WebClient.Services;
using WebClient.Extensions; // Đảm bảo có using này để nhận UpdateUserClaimsAsync

namespace WebClient.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProfileService _profileService;

        public ProfileController(IOrderService orderService, IProfileService profileService)
        {
            _orderService = orderService;
            _profileService = profileService;
        }

        private string GetToken()
        {
            return User.FindFirstValue("AccessToken") ?? string.Empty;
        }

        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            var profile = await _profileService.GetProfile();
            if (profile == null) return RedirectToAction("Login", "Auth");
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            var token = GetToken();
            var result = await _profileService.UpdateProfile(dto);

            if (result != null)
            {
                await HttpContext.UpdateUserClaimsAsync(result.NewToken, result.Profile);
                TempData["ToastMessage"] = "Profile updated successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Failed to update profile!";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var token = GetToken();
            var profile = await _profileService.GetProfile();

            if (profile == null) return RedirectToAction("Login", "Auth");

            var isGoogleAccount = await _profileService.IsGoogleAccount();
            if (isGoogleAccount == true)
            {
                TempData["ToastMessage"] = "This feature is not available for accounts using Google Login.";
                TempData["ToastType"] = "danger";

                return RedirectToAction(nameof(Index));
            }
            return View(new ChangePasswordDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            var token = GetToken();
            var result = await _profileService.ChangePassword(dto);

            if (result == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ToastMessage"] = "Password updated! Please log in again.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Login", "Authentication");
            }

            if (result == "FORCE_LOGOUT")
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ToastMessage"] = "Too many failed attempts. Account logged out.";
                TempData["ToastType"] = "danger";
                return RedirectToAction("Login", "Authentication");
            }

            TempData["ToastMessage"] = result;
            TempData["ToastType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ToastMessage"] = "Please select a valid image file.";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var token = GetToken();
            var result = await _profileService.UploadAvatar(avatarFile);

            if (result != null)
            {
                // Cập nhật Cookie để đồng bộ AvatarUrl và SecurityStamp mới nhất
                await HttpContext.UpdateUserClaimsAsync(result.NewToken, result.Profile);
                TempData["ToastMessage"] = "Avatar updated successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Failed to upload avatar.";
                TempData["ToastType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: api/profile/logout
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = User.FindFirstValue("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                await _profileService.Logout();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData.Clear();

            return RedirectToAction("Login", "Authentication");
        }


        public async Task<IActionResult> MyOrders()
        {
            var userId = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var list = await _orderService.GetOrderByCustomerId(int.Parse(userId));
            return View(list);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderService.GetById(id);
            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> RequestBecomeOrganizerOtp()
        {
            var success = await _profileService.RequestBecomeOrganizer();

            if (success)
            {
                TempData["ToastMessage"] = "An OTP has been sent to your email!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(VerifyBecomeOrganizer));
            }

            TempData["ToastMessage"] = "Failed to send the request. Please try again!";
            TempData["ToastType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "customer")]
        public IActionResult VerifyBecomeOrganizer()
        {
            return View(new VerifyOrganizerOtpDTO());
        }

        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> VerifyBecomeOrganizer(VerifyOrganizerOtpDTO dto)
        {
            if (string.IsNullOrEmpty(dto.OtpCode))
            {
                TempData["ToastMessage"] = "Please enter the OTP.";
                TempData["ToastType"] = "danger";
                return View(dto);
            }

            var result = await _profileService.VerifyBecomeOrganizer(dto);

            if (result != null)
            {
                await HttpContext.UpdateUserClaimsAsync(result.NewToken, result.Profile);

                TempData["ToastMessage"] = "Congratulations! You have successfully become an Organizer.";
                TempData["ToastType"] = "success";

                return RedirectToAction("Organizer", "Dashboard");
            }

            TempData["ToastMessage"] = "Invalid or expired OTP.";
            TempData["ToastType"] = "danger";

            return View(dto);
        }
    }
}