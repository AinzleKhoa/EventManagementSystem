using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClient.DTOs.Request;
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminWithdrawalsController : Controller
    {
        private readonly IWithdrawalService _withdrawalService;
        private readonly IUserService _userService;

        public AdminWithdrawalsController(IWithdrawalService withdrawalService, IUserService userService)
        {
            _withdrawalService = withdrawalService;
            _userService = userService;
        }

        private async Task PopulateOrganizerNames(IEnumerable<WebClient.DTOs.Response.ReadWithdrawalDTO> items)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                try
                {
                    var user = await _userService.GetById(item.OrganizerId);
                    item.OrganizerName = user?.FullName ?? $"Organizer #{item.OrganizerId}";
                }
                catch
                {
                    item.OrganizerName = $"Organizer #{item.OrganizerId}";
                }
            }
        }

        // UC61 - Admin: View all withdrawal requests (paginated + filter)
        public async Task<IActionResult> Index(string? searchTerm, string? status,
            int pageNumber = 1, int pageSize = 10,
            string sortBy = "RequestedAt", string sortOrder = "desc")
        {
            var filter = new WithdrawalFilterDTO
            {
                SearchTerm = searchTerm,
                Status     = status,
                PageNumber = pageNumber,
                PageSize   = pageSize,
                SortBy     = sortBy,
                SortOrder  = sortOrder
            };
            var result = await _withdrawalService.SearchAndFilter(filter);
            if (result?.Items != null)
            {
                await PopulateOrganizerNames(result.Items);
            }
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Status"]     = status;
            ViewData["SortBy"]     = sortBy;
            ViewData["SortOrder"]  = sortOrder;
            return View(result);
        }

        // UC62 - Admin: Approve withdrawal request
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _withdrawalService.Approve(id);
                TempData["ToastMessage"] = "Withdrawal request approved successfully!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Index");
        }

        // Details - Admin: View full details of any withdrawal request
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

        // UC63 - Admin: Reject withdrawal request with reason (GET: show form)
        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var item = await _withdrawalService.GetById(id);
                var dto = new RejectReasonDTO { WithdrawalId = id };
                ViewBag.Withdrawal = item;
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // UC63 - Admin: Reject withdrawal request with reason (POST: submit)
        [HttpPost]
        [ActionName("Reject")]
        public async Task<IActionResult> RejectConfirmed(RejectReasonDTO model)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    var item = await _withdrawalService.GetById(model.WithdrawalId);
                    ViewBag.Withdrawal = item;
                }
                catch { }
                return View(model);
            }
            try
            {
                await _withdrawalService.Reject(model.WithdrawalId, model);
                TempData["ToastMessage"] = "Withdrawal request rejected.";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Index");
        }
    }
}
