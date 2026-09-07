using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebClient.DTOs.Request;
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize(Roles = "organizer")]
    public class WithdrawalsController : Controller
    {
        private readonly IWithdrawalService _withdrawalService;
        private readonly IEventService _eventService;
        private readonly IOrderService _orderService;

        public WithdrawalsController(IWithdrawalService withdrawalService,
                                     IEventService eventService,
                                     IOrderService orderService)
        {
            _withdrawalService = withdrawalService;
            _eventService = eventService;
            _orderService = orderService;
        }

        // UC43 - View own withdrawal requests + status
        public async Task<IActionResult> Index(string? status)
        {
            var all = await _withdrawalService.GetMine();
            if (!string.IsNullOrEmpty(status))
                all = all.Where(w => w.Status == status).ToList();
            ViewData["Status"] = status;
            return View(all);
        }

        // Details - Organizer: View full details of any own withdrawal request
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var item = await _withdrawalService.GetById(id);
                return View(item);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // UC41 - Create withdrawal request
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.TryParse(userIdStr, out int uid) ? uid : 0;

            var myEvents = await _eventService.GetAll(organizerId: userId);
            var myWithdrawals = await _withdrawalService.GetMine();
            var blockedEventIds = myWithdrawals
                .Where(w => w.EventId.HasValue && (w.Status == "Pending" || w.Status == "Approved"))
                .Select(w => w.EventId!.Value)
                .ToHashSet();

            ViewBag.EligibleEvents = myEvents
                .Where(e => !blockedEventIds.Contains(e.EventId))
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWithdrawalDTO model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEligibleEvents();
                return View(model);
            }
            try
            {
                await _withdrawalService.Create(model);
                TempData["ToastMessage"] = "Withdrawal request created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                await PopulateEligibleEvents();
                return View(model);
            }
        }

        // AJAX: Get net revenue for an event (after 10% platform fee)
        [HttpGet]
        public async Task<IActionResult> GetEventRevenue(int eventId)
        {
            try
            {
                var revenue = await _orderService.GetTotalRevenueByEventId(eventId);
                var netAmount = (decimal)revenue * 0.9m;
                return Json(new { amount = (long)netAmount });
            }
            catch
            {
                return Json(new { amount = 0 });
            }
        }

        private async Task PopulateEligibleEvents()
        {
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.TryParse(userIdStr, out int uid) ? uid : 0;

            var myEvents = await _eventService.GetAll(organizerId: userId);
            var myWithdrawals = await _withdrawalService.GetMine();
            var blockedEventIds = myWithdrawals
                .Where(w => w.EventId.HasValue && (w.Status == "Pending" || w.Status == "Approved"))
                .Select(w => w.EventId!.Value)
                .ToHashSet();

            ViewBag.EligibleEvents = myEvents
                .Where(e => !blockedEventIds.Contains(e.EventId))
                .ToList();
        }
    }
}
