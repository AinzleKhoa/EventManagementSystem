using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize]
    public class TicketValidatorController : Controller
    {
        private readonly ITicketApiClient _ticketApi;
        private readonly IEventValidatorService _validatorService;

        public TicketValidatorController(ITicketApiClient ticketApi, IEventValidatorService validatorService)
        {
            _ticketApi = ticketApi;
            _validatorService = validatorService;
        }

        // UC45 - Scan QR / CheckIn page
        public IActionResult CheckIn()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        private string? GetValidatorEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                ?? User.FindFirst("unique_name")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }

        // UC44 - View ticket list of an event (card view of assigned events)
        public async Task<IActionResult> MyEvents()
        {
            var email = GetValidatorEmail();
            var events = new List<WebClient.DTOs.Response.ReadEventDTO>();
            if (!string.IsNullOrEmpty(email))
            {
                events = await _validatorService.GetEventsByValidatorEmailAsync(email);
            }
            return View(events);
        }

        // UC45 - Process QR/Manual Check-In (AJAX)
        [HttpPost]
        [Route("TicketValidator/ProcessCheckIn")]
        public async Task<IActionResult> ProcessCheckIn([FromBody] TicketCheckInRequestDTO request)
        {
            try
            {
                if (request == null || request.TicketCode.Equals(Guid.Empty))
                {
                    return Json(new { isSuccess = false, message = "Invalid ticket code." });
                }

                var email = GetValidatorEmail();
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { isSuccess = false, message = "Unable to identify the staff member." });
                }

                var assignedEvents = await _validatorService.GetEventsByValidatorEmailAsync(email);
                if (assignedEvents == null || !assignedEvents.Any())
                {
                    return Json(new { isSuccess = false, message = "You have not been assigned to validate any events." });
                }

                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                var token = authHeader?.Replace("Bearer ", "") ?? string.Empty;

                if (!Guid.TryParse(request.TicketCode, out Guid parsedTicketCode))
                {
                    return Json(new { isSuccess = false, message = "Invalid or incorrectly formatted ticket code." });
                }

                var ticketInfo = await _ticketApi.GetTicketByCode(parsedTicketCode, token);

                if (ticketInfo == null)
                {
                    return Json(new { isSuccess = false, message = "Ticket information not found." });
                }

                bool isAuthorizedForThisEvent = assignedEvents.Any(e => e.EventId == ticketInfo.EventId);

                if (!isAuthorizedForThisEvent)
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Access denied: This ticket belongs to an event you are not assigned to."
                    });
                }

                request.EventId = ticketInfo.EventId;
                var result = await _ticketApi.ScanTicketAsync(request, token);
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n>>> ERROR 500 AT WEBCLIENT: {ex.Message}\n");
                return Json(new { isSuccess = false, message = "System error: " + ex.Message });
            }
        }

        // UC44 - View ticket list of a specific event
        [HttpGet]
        public async Task<IActionResult> EventTickets(int eventId, string? searchTerm, string? status)
        {
            var email = User?.FindFirst("unique_name")?.Value
                      ?? User?.FindFirst(ClaimTypes.Name)?.Value;

            var assignedEvents = await _validatorService.GetEventsByValidatorEmailAsync(email ?? "");
            var targetEvent = assignedEvents?.FirstOrDefault(e => e.EventId == eventId);
            if (targetEvent == null)
            {
                TempData["Error"] = "You are not authorized to view tickets for this event.";
                return RedirectToAction("MyEvents");
            }

            var tickets = (await _ticketApi.GetTicketsByEventIdAsync(eventId))?.ToList()
                          ?? new List<ReadTicketDTO>();

            // UC47 - Search / Filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                tickets = tickets.Where(t =>
                    t.OwnerName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    t.TicketCode.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                tickets = tickets.Where(t => t.Status == status).ToList();
            }

            ViewBag.EventId = eventId;
            ViewBag.EventTitle = targetEvent.Title;
            ViewBag.EventStartDate = targetEvent.StartDate;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = status;

            return View(tickets);
        }

        // UC48 - View checked-in statistics
        [HttpGet]
        public async Task<IActionResult> Statistics(int eventId)
        {
            var email = User?.FindFirst("unique_name")?.Value
                      ?? User?.FindFirst(ClaimTypes.Name)?.Value;

            var assignedEvents = await _validatorService.GetEventsByValidatorEmailAsync(email ?? "");
            var targetEvent = assignedEvents?.FirstOrDefault(e => e.EventId == eventId);
            if (targetEvent == null)
            {
                TempData["Error"] = "Access denied.";
                return RedirectToAction("MyEvents");
            }

            var tickets = (await _ticketApi.GetTicketsByEventIdAsync(eventId))?.ToList()
                          ?? new List<ReadTicketDTO>();

            int totalTickets = tickets.Count;
            int checkedIn = tickets.Count(t => t.Status == "CheckedIn" || t.CheckedInAt.HasValue);
            int notCheckedIn = totalTickets - checkedIn;
            double checkInRate = totalTickets > 0 ? Math.Round((double)checkedIn / totalTickets * 100, 1) : 0;

            ViewBag.EventId = eventId;
            ViewBag.EventTitle = targetEvent.Title;
            ViewBag.EventStartDate = targetEvent.StartDate;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.CheckedIn = checkedIn;
            ViewBag.NotCheckedIn = notCheckedIn;
            ViewBag.CheckInRate = checkInRate;

            return View(tickets);
        }
    }
}
