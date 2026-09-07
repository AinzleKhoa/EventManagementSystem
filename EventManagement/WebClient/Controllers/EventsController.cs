using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Services;
using WebClient.Services.Implements;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebClient.Controllers
{


    [Authorize(Roles = "organizer,ticket_validator,admin")]

    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ITicketTypeService _ticketTypeService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env; 
        private readonly IPhotoService _photoService;
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        public IActionResult Validators()
        {
            // Truyền model nếu cần
            return View();
        }
        public EventsController(IEventService eventService,
                                ITicketTypeService ticketTypeService,
                                ICategoryService categoryService,
                                IPhotoService photoService,
                                IConfiguration configuration,
                                IOrderService orderService)
        {
            _eventService = eventService;
            _ticketTypeService = ticketTypeService;
            _categoryService = categoryService;
            _photoService = photoService;
            _configuration = configuration;
            _orderService = orderService;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdStr, out int uid) ? uid : 3;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string status, int pageNumber = 1, int pageSize = 10, string sortBy = "EventId", string sortOrder = "desc")
        {
            int userId = GetCurrentUserId();


            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            List<ReadEventDTO> listEvents = new();

            if (userRole?.ToLower() != "organizer" && userRole?.ToLower() != "admin")
            {

                var validatorEmail = User.FindFirst("unique_name")?.Value
                                   ?? User.FindFirst(ClaimTypes.Name)?.Value
                                   ?? User.FindFirst(ClaimTypes.Email)?.Value 
                                   ?? User.FindFirst("email")?.Value
                                   ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                if (!string.IsNullOrWhiteSpace(validatorEmail))
                {
                    var validatorService = GetValidatorService();
                    listEvents = await validatorService.GetEventsByValidatorEmailAsync(validatorEmail);
                }
            }
            else
            {

                listEvents = await _eventService.GetAll(organizerId: userId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                listEvents = listEvents.Where(e => (e.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                                                || (e.Location?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                listEvents = listEvents.Where(e => e.Status == status).ToList();
            }


            if (sortBy == "Title")
                listEvents = sortOrder == "asc" ? listEvents.OrderBy(e => e.Title).ToList() : listEvents.OrderByDescending(e => e.Title).ToList();
            else if (sortBy == "Status")
                listEvents = sortOrder == "asc" ? listEvents.OrderBy(e => e.Status).ToList() : listEvents.OrderByDescending(e => e.Status).ToList();
            else if (sortBy == "StartDate")
                listEvents = sortOrder == "asc" ? listEvents.OrderBy(e => e.StartDate).ToList() : listEvents.OrderByDescending(e => e.StartDate).ToList();
            else 
                listEvents = sortOrder == "asc" ? listEvents.OrderBy(e => e.EventId).ToList() : listEvents.OrderByDescending(e => e.EventId).ToList();

            // Get tickets and revenue data
            var eventRevenues = new Dictionary<int, long>();
            var eventTickets = new Dictionary<int, int>();

            foreach (var e in listEvents)
            {
                eventRevenues[e.EventId] = 0;
                eventTickets[e.EventId] = 0;

                var orders = await _orderService.GetOrdersByEventIdAsync(e.EventId);
                if (orders != null && orders.Any())
                {
                    var validOrders = orders.Where(o => o.Status == "Paid" || o.Status == "Completed");
                    foreach (var order in validOrders)
                    {
                        eventRevenues[e.EventId] += order.FinalAmount;
                        int ticketsInOrder = 0;
                        if (order.OrderDetails != null && order.OrderDetails.Any())
                        {
                            ticketsInOrder = order.OrderDetails.Sum(od => od.Quantity);
                        }
                        else if (order.Tickets != null)
                        {
                            ticketsInOrder = order.Tickets.Count;
                        }
                        eventTickets[e.EventId] += ticketsInOrder;
                    }
                }
            }

            ViewBag.EventRevenues = eventRevenues;
            ViewBag.EventTickets = eventTickets;

            var result = new PaginatedResultDTO<ReadEventDTO>
            {
                TotalCount = listEvents.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = listEvents.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };

            ViewData["SearchTerm"] = searchTerm;
            ViewData["Status"] = status;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            return View(result);
        }

        [Authorize(Roles = "organizer")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAll();
            return View(new CreateEventViewModel());
        }

        [Authorize(Roles = "organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateEventViewModel model)
        {

            model.OrganizerId = GetCurrentUserId();


            if (model.PosterImage != null)
            {
                try
                {
                    model.PosterUrl = await _photoService.AddPhotoAsync(model.PosterImage);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi upload ảnh: " + ex.Message);
                }
            }

            if (!string.IsNullOrEmpty(model.PosterUrl))
            {
                ModelState.Remove("PosterUrl");
            }


            if (model.StartDate <= DateTime.Now)
                ModelState.AddModelError("StartDate", "Event Start Date & Time must be in the future.");

            if (model.EndDate <= model.StartDate)
                ModelState.AddModelError("EndDate", "Event End Date & Time must be after Start Date & Time.");

            if (model.TicketTypes == null || !model.TicketTypes.Any())
            {
                ModelState.AddModelError("", "At least one Ticket Type is required for the event.");
            }
            else
            {
                for (int i = 0; i < model.TicketTypes.Count; i++)
                {
                    var t = model.TicketTypes[i];
                    if (t.SaleStartDate.HasValue && t.SaleStartDate.Value >= model.EndDate)
                    {
                        ModelState.AddModelError($"TicketTypes[{i}].SaleStartDate", $"Ticket #{i + 1}: Sale Start Date must be before Event End Date.");
                    }
                    if (t.SaleStartDate.HasValue && t.SaleEndDate.HasValue && t.SaleEndDate.Value <= t.SaleStartDate.Value)
                    {
                        ModelState.AddModelError($"TicketTypes[{i}].SaleEndDate", $"Ticket #{i + 1}: Sale End Date must be after Sale Start Date.");
                    }
                    if (t.SaleEndDate.HasValue && t.SaleEndDate.Value > model.EndDate)
                    {
                        ModelState.AddModelError($"TicketTypes[{i}].SaleEndDate", $"Ticket #{i + 1}: Sale End Date cannot be after Event End Date.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAll();
                return View(model);
            }

            try
            {
                var createDto = new CreateEventDTO
                {
                    OrganizerId = model.OrganizerId,
                    CategoryId = model.CategoryId,
                    Title = model.Title,
                    Description = model.Description,
                    Location = model.Location,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    PosterUrl = model.PosterUrl,
                    TicketTypes = model.TicketTypes
                };

                await _eventService.Add(createDto);
                TempData["ToastMessage"] = "Event created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Categories = await _categoryService.GetAll();
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var eventData = await _eventService.GetById(id);
            if (eventData == null) return NotFound();

            // Block editing if event has been approved
            if (eventData.Status == "Approved" || eventData.Status == "Published")
            {
                TempData["ToastMessage"] = "Cannot edit this event because it has been approved.";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UpdateEventViewModel
            {
                EventId = eventData.EventId,
                OrganizerId = eventData.OrganizerId,
                CategoryId = eventData.CategoryId,
                Title = eventData.Title,
                Description = eventData.Description,
                Location = eventData.Location,
                StartDate = eventData.StartDate,
                EndDate = eventData.EndDate,
                PosterUrl = eventData.PosterUrl,
                TicketTypes = eventData.TicketTypes?.Select(t => new UpdateTicketTypeDTO
                {
                    TicketTypeId = t.TicketTypeId,
                    TypeName = t.TypeName,
                    Price = t.Price,
                    Quantity = t.Quantity,
                    ColorCode = t.ColorCode,
                    SaleStartDate = t.SaleStartDate,
                    SaleEndDate = t.SaleEndDate
                }).ToList() ?? new List<UpdateTicketTypeDTO>()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateEventViewModel model)
        {
            // Block editing if event has been approved
            var eventData = await _eventService.GetById(model.EventId);
            if (eventData != null && (eventData.Status == "Approved" || eventData.Status == "Published"))
            {
                TempData["ToastMessage"] = "Cannot edit this event because it has been approved.";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            // 1. Nếu có chọn ảnh mới -> Upload và lấy URL mới
            if (model.PosterImage != null)
            {
                try
                {
                    model.PosterUrl = await _photoService.AddPhotoAsync(model.PosterImage);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi upload ảnh mới: " + ex.Message);
                }
            }

            // 2. Xóa các lỗi validation liên quan đến ảnh (vì ta dùng PosterUrl cũ hoặc đã có URL mới)
            ModelState.Remove("PosterUrl");
            ModelState.Remove("PosterImage");

            if (model.StartDate <= DateTime.Now)
                ModelState.AddModelError("StartDate", "Event Start Date & Time must be in the future.");

            if (model.EndDate <= model.StartDate)
                ModelState.AddModelError("EndDate", "Event End Date & Time must be after Start Date & Time.");

            if (model.TicketTypes == null || !model.TicketTypes.Any())
            {
                ModelState.AddModelError("", "At least one Ticket Type is required for the event.");
            }
            else
            {
                for (int i = 0; i < model.TicketTypes.Count; i++)
                {
                    var t = model.TicketTypes[i];
                    if (t.SaleStartDate.HasValue && t.SaleStartDate.Value >= model.EndDate)
                    {
                        ModelState.AddModelError($"TicketTypes[{i}].SaleStartDate", $"Ticket #{i + 1}: Sale Start Date must be before Event End Date.");
                    }
                    if (t.SaleStartDate.HasValue && t.SaleEndDate.HasValue && t.SaleEndDate.Value <= t.SaleStartDate.Value)
                    {
                        ModelState.AddModelError($"TicketTypes[{i}].SaleEndDate", $"Ticket #{i + 1}: Sale End Date must be after Sale Start Date.");
                    }
                    if (t.SaleEndDate.HasValue && t.SaleEndDate.Value > model.EndDate)
                    {
                        ModelState.AddModelError($"TicketTypes[{i}].SaleEndDate", $"Ticket #{i + 1}: Sale End Date cannot be after Event End Date.");
                    }
                }
            }

            if (!ModelState.IsValid) return View(model);

            try
            {
                var updateDto = new UpdateEventDTO
                {
                    EventId = model.EventId,
                    OrganizerId = model.OrganizerId,
                    CategoryId = model.CategoryId,
                    Title = model.Title,
                    Description = model.Description,
                    Location = model.Location,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    PosterUrl = model.PosterUrl,
                    TicketTypes = model.TicketTypes
                };

                await _eventService.Update(model.EventId, updateDto);

                if (model.TicketTypes != null)
                {
                    foreach (var ticket in model.TicketTypes)
                    {
                        await _ticketTypeService.Update(ticket.TicketTypeId, ticket);
                    }
                }

                TempData["ToastMessage"] = "Updated successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var data = await _eventService.GetById(id);
            if (data == null) return NotFound();

            // Fetch Category Name
            try
            {
                var category = await _categoryService.GetById(data.CategoryId);
                if (category != null)
                {
                    data.CategoryName = category.CategoryName;
                }
            }
            catch { }

            // Check if user is a validator for this event
            // Validators (staff) can only view their assigned events
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            if (userRole?.ToLower() == "validator" || userRole?.ToLower() == "staff")
            {
                var validatorEmail = User.FindFirst("unique_name")?.Value
                                   ?? User.FindFirst(ClaimTypes.Name)?.Value
                                   ?? User.FindFirst(ClaimTypes.Email)?.Value 
                                   ?? User.FindFirst("email")?.Value
                                   ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                if (!string.IsNullOrWhiteSpace(validatorEmail))
                {
                    // Call API to check if this validator is assigned to this event
                    var isValidator = await GetValidatorService().IsValidatorOfEventAsync(id, validatorEmail);
                    if (!isValidator)
                    {
                        return Forbid("❌ Bạn không có quyền xem sự kiện này.");
                    }
                }
                else
                {
                    return Forbid("❌ Không tìm thấy email của bạn. Vui lòng đăng nhập lại.");
                }
            }

            // Fetch actual orders to get real-time ticket sold and revenue data
            if (data.TicketTypes != null && data.TicketTypes.Count > 0)
            {
                var orders = await _orderService.GetOrdersByEventIdAsync(id);
                if (orders != null && orders.Any())
                {
                    var validOrders = orders.Where(o => o.Status == "Paid" || o.Status == "Completed").ToList();

                    // Reset SoldQuantity for each ticket type and recalculate from orders
                    foreach (var ticketType in data.TicketTypes)
                    {
                        ticketType.SoldQuantity = 0;
                    }

                    // Update SoldQuantity based on actual orders
                    foreach (var order in validOrders)
                    {
                        if (order.OrderDetails != null && order.OrderDetails.Any())
                        {
                            foreach (var orderDetail in order.OrderDetails)
                            {
                                var ticketType = data.TicketTypes.FirstOrDefault(t => t.TicketTypeId == orderDetail.TicketTypeId);
                                if (ticketType != null)
                                {
                                    ticketType.SoldQuantity += orderDetail.Quantity;
                                }
                            }
                        }
                        else if (order.Tickets != null && order.Tickets.Any())
                        {
                            // Group tickets by TicketTypeId and count
                            var ticketsByType = order.Tickets.GroupBy(t => t.TicketTypeId);
                            foreach (var group in ticketsByType)
                            {
                                var ticketType = data.TicketTypes.FirstOrDefault(t => t.TicketTypeId == group.Key);
                                if (ticketType != null)
                                {
                                    ticketType.SoldQuantity += group.Count();
                                }
                            }
                        }
                    }
                }
            }

            return View(data);
        }

        private IEventValidatorService GetValidatorService()
        {
            return HttpContext.RequestServices.GetRequiredService<IEventValidatorService>();
        }

        // Actions: Submit Hide, Resume, Delete
        public async Task<IActionResult> Submit(int id)
        {
            await _eventService.Submit(id);
            TempData["ToastMessage"] = "Event submitted for approval.";
            TempData["ToastType"] = "info";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Hide(int id)
        {
            await _eventService.Hide(id);
            TempData["ToastMessage"] = "Event hidden from public.";
            TempData["ToastType"] = "warning";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Resume(int id)
        {
            await _eventService.Resume(id);
            TempData["ToastMessage"] = "Event resumed sales.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            // Block deleting if event has been approved
            var eventData = await _eventService.GetById(id);
            if (eventData != null && (eventData.Status == "Approved" || eventData.Status == "Published"))
            {
                TempData["ToastMessage"] = "Cannot delete this event because it has been approved.";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _eventService.Delete(id);
                TempData["ToastMessage"] = "Event deleted permanently.";
                TempData["ToastType"] = "success";
            }
            catch
            {
                TempData["ToastMessage"] = "Cannot delete event (tickets sold or active).";
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper: Save Image to wwwroot
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string folder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/uploads/" + fileName;
        }

        [HttpPost]
        public async Task<IActionResult> AjaxCreateTicket([FromBody] CreateTicketTypeDTO model)
        {
            try
            {
                await _ticketTypeService.Create(model);
                return Json(new { success = true });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> AjaxDeleteTicket(int id)
        {
            try
            {
                await _ticketTypeService.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // Geocoding endpoint - converts address to coordinates
        [HttpGet]
        [AllowAnonymous]
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

    // ViewModels để hỗ trợ IFormFile (Kế thừa DTO gốc)
    public class CreateEventViewModel : CreateEventDTO
    {
        public IFormFile? PosterImage { get; set; }
    }

    public class UpdateEventViewModel : UpdateEventDTO
    {
        public IFormFile? PosterImage { get; set; }
    }

}