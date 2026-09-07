using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClient.Services;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebClient.Controllers
{
    // [Authorize(Roles = "Admin")] // Bật lên khi có authen
    public class AdminEventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;

        public AdminEventsController(IEventService eventService, IUserService userService, ICategoryService categoryService)
        {
            _eventService = eventService;
            _userService = userService;
            _categoryService = categoryService;
        }

        private async Task<IActionResult> FilterEvents(string tab, string? searchTerm, string sortBy, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            List<WebClient.DTOs.Response.ReadEventDTO> events;

            if (tab == "Pending")
            {
                events = await _eventService.GetPendingEvents();
            }
            else
            {
                var all = await _eventService.GetAll();
                events = tab switch
                {
                    "Approved" => all.Where(e => e.Status == "Approved" || e.Status == "Published").ToList(),
                    "Rejected" => all.Where(e => e.Status == "Rejected").ToList(),
                    _ => all
                };
            }

            // ✅ THÊM CODE FETCH ORGANIZER & CATEGORY NAME
            try
            {
                var categories = await _categoryService.GetAll();
                var categoryDict = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);
                
                foreach (var evt in events)
                {
                    if (string.IsNullOrEmpty(evt.OrganizerName) || evt.OrganizerName == "Unknown")
                    {
                        evt.OrganizerName = await GetOrganizerNameAsync(evt.OrganizerId);
                    }
                    if (categoryDict.TryGetValue(evt.CategoryId, out var catName))
                    {
                        evt.CategoryName = catName;
                    }
                }
            }
            catch
            {
                foreach (var evt in events)
                {
                    if (string.IsNullOrEmpty(evt.OrganizerName) || evt.OrganizerName == "Unknown")
                    {
                        evt.OrganizerName = await GetOrganizerNameAsync(evt.OrganizerId);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                events = events
                    .Where(e => (e.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (e.Location?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                (e.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            events = sortBy switch
            {
                "Title" => sortOrder == "asc"
                    ? events.OrderBy(e => e.Title).ToList()
                    : events.OrderByDescending(e => e.Title).ToList(),
                "StartDate" => sortOrder == "asc"
                    ? events.OrderBy(e => e.StartDate).ToList()
                    : events.OrderByDescending(e => e.StartDate).ToList(),
                _ => sortOrder == "asc"
                    ? events.OrderBy(e => e.EventId).ToList()
                    : events.OrderByDescending(e => e.EventId).ToList()
            };

            int totalCount = events.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;

            var pagedEvents = events.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewData["SearchTerm"] = searchTerm;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;
            ViewData["TabActive"] = tab;
            ViewData["PageNumber"] = pageNumber;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalCount"] = totalCount;
            ViewData["TotalPages"] = totalPages;

            return View("PendingOrApproved", pagedEvents);
        }

        // ✅ THÊM HÀM FETCH ORGANIZER NAME
        private async Task<string> GetOrganizerNameAsync(int organizerId)
        {
            try
            {
                var user = await _userService.GetById(organizerId);
                if (user != null && !string.IsNullOrEmpty(user.FullName))
                {
                    return user.FullName;
                }
            }
            catch { }
            return $"User #{organizerId}";
        }

        public async Task<IActionResult> Pending(string? searchTerm, string sortBy = "DateSubmitted", string sortOrder = "desc", int pageNumber = 1, int pageSize = 10)
            => await FilterEvents("Pending", searchTerm, sortBy, sortOrder, pageNumber, pageSize);

        public async Task<IActionResult> Approved(string? searchTerm, string sortBy = "DateSubmitted", string sortOrder = "desc", int pageNumber = 1, int pageSize = 10)
            => await FilterEvents("Approved", searchTerm, sortBy, sortOrder, pageNumber, pageSize);

        public async Task<IActionResult> Rejected(string? searchTerm, string sortBy = "DateSubmitted", string sortOrder = "desc", int pageNumber = 1, int pageSize = 10)
            => await FilterEvents("Rejected", searchTerm, sortBy, sortOrder, pageNumber, pageSize);

        public async Task<IActionResult> All(string? searchTerm, string sortBy = "DateSubmitted", string sortOrder = "desc", int pageNumber = 1, int pageSize = 10)
            => await FilterEvents("All", searchTerm, sortBy, sortOrder, pageNumber, pageSize);

        // Xem chi tiết để duyệt (Tận dụng lại view chi tiết nhưng ở chế độ Admin)
        public async Task<IActionResult> Details(int id)
        {
            var eventDetail = await _eventService.GetById(id);
            if (eventDetail == null) return NotFound();
 
            // ✅ THÊM FETCH ORGANIZER NAME
            if (string.IsNullOrEmpty(eventDetail.OrganizerName) || eventDetail.OrganizerName == "Unknown")
            {
                eventDetail.OrganizerName = await GetOrganizerNameAsync(eventDetail.OrganizerId);
            }
 
            // Fetch Category Name
            try
            {
                var category = await _categoryService.GetById(eventDetail.CategoryId);
                if (category != null)
                {
                    eventDetail.CategoryName = category.CategoryName;
                }
            }
            catch { }
 
            return View(eventDetail);
        }

        // UC54: Approve an event
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _eventService.Approve(id);
                TempData["ToastMessage"] = "Event has been approved successfully!";
                TempData["ToastType"] = "success";
            }
            catch
            {
                TempData["ToastMessage"] = "Failed to approve event.";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Pending");
        }

        // UC55: Reject an event with reason
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ToastMessage"] = "Rejection reason is required.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                await _eventService.Reject(id, reason);
                TempData["ToastMessage"] = "Event has been rejected.";
                TempData["ToastType"] = "success";
            }
            catch
            {
                TempData["ToastMessage"] = "Failed to reject event.";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Pending");
        }

        // Geocoding endpoint - converts address to coordinates
        [HttpGet]
        public async Task<IActionResult> Geocode(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return Json(new { success = false, message = "Address is empty" });

            try
            {
                using var client = new HttpClient();
                var apiKey = "AIzaSyDLIYsDqBmH4AYMmhUnYHJx5RHE3rymOhk";
                var encodedAddress = Uri.EscapeDataString(address.Trim());
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={apiKey}";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Geocoding request for: {address}");
                System.Diagnostics.Debug.WriteLine($"Google response: {content}");

                var json = JsonSerializer.Deserialize<JsonElement>(content);

                // Check the status first
                if (json.TryGetProperty("status", out var status))
                {
                    var statusValue = status.GetString();
                    if (statusValue != "OK")
                    {
                        return Json(new { success = false, message = $"Geocoding status: {statusValue}" });
                    }
                }

                if (json.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    if (firstResult.TryGetProperty("geometry", out var geometry) &&
                        geometry.TryGetProperty("location", out var location))
                    {
                        if (location.TryGetProperty("lat", out var lat) && 
                            location.TryGetProperty("lng", out var lng))
                        {
                            return Json(new { 
                                success = true, 
                                lat = lat.GetDouble(), 
                                lng = lng.GetDouble() 
                            });
                        }
                    }
                }

                return Json(new { success = false, message = "No results found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}