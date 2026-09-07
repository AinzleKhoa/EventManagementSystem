using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebClient.DTOs.Response;
using WebClient.Services;

namespace WebClient.Controllers
{
    public class EventStaffController : Controller
    {
        private readonly IEventStaffService _staffService;
        private readonly IEventValidatorService _validatorService;

        public EventStaffController(IEventStaffService staffService, IEventValidatorService validatorService)
        {
            _staffService = staffService;
            _validatorService = validatorService;
        }

        public async Task<IActionResult> Index()
        {
            var email = User?.FindFirst("unique_name")?.Value
                      ?? User?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return View(Enumerable.Empty<EventStaffResponseDto>());
            }

            var events = await _validatorService.GetEventsByValidatorEmailAsync(email);
            var result = events.Select(e => new EventStaffResponseDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                PosterUrl = e.PosterUrl,
                Location = e.Location,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Status = e.Status
            });

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> CheckIsValidator()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Json(new { isValidator = false });
            }

            var email = User.FindFirst("unique_name")?.Value
                      ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { isValidator = false });
            }

            try
            {
                var events = await _validatorService.GetEventsByValidatorEmailAsync(email);
                return Json(new { isValidator = events != null && events.Count > 0 });
            }
            catch
            {
                return Json(new { isValidator = false });
            }
        }
    }
}
