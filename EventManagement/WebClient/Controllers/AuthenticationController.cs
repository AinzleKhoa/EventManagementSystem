using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebClient.DTOs.Request;
using WebClient.Extensions;
using WebClient.Services;

namespace WebClient.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ToastMessage"] = "Vui lòng đăng nhập để tiếp tục đặt vé hoặc thao tác!";
                TempData["ToastType"] = "info";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var (data, error) = await _authService.Login(model);

            if (data == null)
            {
                TempData["ToastMessage"] = error ?? "Login failed. Please check your email or password.";
                TempData["ToastType"] = "danger";
                return View();
            }

            await HttpContext.SignInUserAsync(data);
            int roleId = data.User.Role?.RoleId ?? 3;
            return RedirectToRolePage(roleId);
        }

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO dto)
        {
            var (data, error) = await _authService.GoogleLogin(dto.IdToken);

            if (data == null)
                return Unauthorized(new { message = error });

            await HttpContext.SignInUserAsync(data);
            int roleId = data.User.Role?.RoleId ?? 3;
            return RedirectToRolePage(roleId);
        }

        private IActionResult RedirectToRolePage(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    return RedirectToAction("Admin", "Dashboard");
                case 2:
                    return RedirectToAction("Index", "Home");
                case 3:
                    return RedirectToAction("Organizer", "Dashboard");
                case 4:
                    return RedirectToAction("MyEvents", "TicketValidator");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var errorMessage = await _authService.Register(model);

            if (errorMessage != null)
            {
                TempData["ToastMessage"] = errorMessage;
                TempData["ToastType"] = "danger";
                return View(model);
            }

            TempData["ToastMessage"] = "Register successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var error = await _authService.ForgotPassword(model);

            if (error != null)
            {
                TempData["ToastMessage"] = error;
                TempData["ToastType"] = "danger";
                return View(model);
            }

            TempData["ToastMessage"] = "OTP has been sent to your email";
            TempData["ToastType"] = "success";
            return RedirectToAction("ResetPassword", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordDTO
            {
                Email = email
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var error = await _authService.ResetPassword(model);

            if (error != null)
            {
                Console.WriteLine("ResetPassword: err: " + error);
                TempData["ToastMessage"] = error;
                TempData["ToastType"] = "danger";

                if (error.Contains("Invalid or expired OTP"))
                {
                    return RedirectToAction("Login");
                }

                return View(model);
            }

            TempData["ToastMessage"] = "Password reset successfully!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}