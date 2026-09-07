using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebClient.DTOs.Response;
using WebClient.Services;
using WebClient.Services.Implements;

namespace WebClient.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public DashboardController(IEventService eventService, IOrderService orderService, IUserService userService)
        {
            _eventService = eventService;
            _orderService = orderService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Organizer(string searchTerm, string status, int pageNumber = 1, int pageSize = 10, string sortBy = "StartDate", string sortOrder = "desc")
        {
            var result = await GetEventDataAsync(searchTerm, status, pageNumber, pageSize, sortBy, sortOrder);
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Status"] = status;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            return View(result);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Admin()
        {
            var dashboard = new AdminDashBoardResponseDTO();

            try
            {
                dashboard.TotalCustomers = await _userService.GetTotalCustomers();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetTotalCustomers: " + ex.Message);
            }

            try
            {
                dashboard.TotalEvents = await _eventService.GetTotalEventsCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetTotalEventsCount: " + ex.Message);
            }

            try
            {
                dashboard.TotalPublicEvents = await _eventService.GetTotalPublicEventsCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetTotalPublicEventsCount: " + ex.Message);
            }

            try
            {
                dashboard.Top5SellingEvents = await _eventService.GetTopSellingEvents();
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(dashboard.Top5SellingEvents));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetTopSellingEvents: " + ex.Message);
            }

            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string status, int pageNumber = 1, int pageSize = 10, string sortBy = "StartDate", string sortOrder = "desc")
        {
            var result = await GetEventDataAsync(searchTerm, status, pageNumber, pageSize, sortBy, sortOrder);
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Status"] = status;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            return View(result);
        }


        private async Task<PaginatedResultDTO<ReadEventDTO>> GetEventDataAsync(string searchTerm, string status, int pageNumber, int pageSize, string sortBy, string sortOrder)
        {
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.TryParse(userIdStr, out int uid) ? uid : 1;

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
            else 
                listEvents = sortOrder == "asc" ? listEvents.OrderBy(e => e.StartDate).ToList() : listEvents.OrderByDescending(e => e.StartDate).ToList();


            long totalRevenue = 0;
            int totalTicketsSold = 0;

            var eventRevenues = new Dictionary<int, long>();
            var eventTickets = new Dictionary<int, int>();

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-5 + i).ToString("MMM"))
                .ToList();

            var monthlyRevenue = last6Months.ToDictionary(m => m, m => 0L);
            var monthlyTickets = last6Months.ToDictionary(m => m, m => 0);

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
    
                        totalRevenue += order.FinalAmount;

                        int ticketsInOrder = 0;
                        if (order.OrderDetails != null && order.OrderDetails.Any())
                        {
                            ticketsInOrder = order.OrderDetails.Sum(od => od.Quantity);
                        }
                        else if (order.Tickets != null)
                        {
                            ticketsInOrder = order.Tickets.Count;
                        }

                        totalTicketsSold += ticketsInOrder;

   
                        eventRevenues[e.EventId] += order.FinalAmount;
                        eventTickets[e.EventId] += ticketsInOrder;

                        if (order.OrderDate.HasValue)
                        {
                            var monthKey = order.OrderDate.Value.ToString("MMM");
                            if (monthlyRevenue.ContainsKey(monthKey))
                            {
                                monthlyRevenue[monthKey] += order.FinalAmount;
                                monthlyTickets[monthKey] += ticketsInOrder;
                            }
                        }
                    }
                }
            }

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalTicketsSold = totalTicketsSold;
            ViewBag.ActiveEvents = listEvents.Count(e => e.Status == "Approved");


            ViewBag.StatusLabels = new[] { "Approved", "Pending", "Draft", "Hidden" };
            ViewBag.StatusData = new[] {
                listEvents.Count(e => e.Status == "Approved"),
                listEvents.Count(e => e.Status == "Pending"),
                listEvents.Count(e => e.Status == "Draft"),
                listEvents.Count(e => e.Status == "Hidden")
            };

            ViewBag.RevenueMonthLabels = monthlyRevenue.Keys.ToArray();
            ViewBag.RevenueData = monthlyRevenue.Values.ToArray();
            ViewBag.TicketsData = monthlyTickets.Values.ToArray();

            ViewBag.EventRevenues = eventRevenues;
            ViewBag.EventTickets = eventTickets;


            var result = new PaginatedResultDTO<ReadEventDTO>
            {
                TotalCount = listEvents.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = listEvents.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };

            return result;
        }

        private IEventValidatorService GetValidatorService()
        {
            return HttpContext.RequestServices.GetRequiredService<IEventValidatorService>();
        }

        [HttpGet]
        [Authorize(Roles = "organizer,admin")]
        public async Task<IActionResult> EventOrders(int eventId)
        {

            var orders = await _orderService.GetOrdersByEventIdAsync(eventId);
            var eventDetails = await _eventService.GetById(eventId);

            if (eventDetails != null)
            {
                ViewBag.EventName = eventDetails.Title;
                ViewBag.PosterUrl = eventDetails.PosterUrl;
            }
            else
            {
                ViewBag.EventName = "Event Orders";
            }
            return View(orders);
        }


        [HttpGet]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> GetDashboardStats()
        {
            
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.TryParse(userIdStr, out int uid) ? uid : 1;

            
            var listEvents = await _eventService.GetAll(organizerId: userId);

            long totalRevenue = 0;
            int totalTicketsSold = 0;

          
            foreach (var e in listEvents)
            {
                var orders = await _orderService.GetOrdersByEventIdAsync(e.EventId);
                if (orders != null && orders.Any())
                {
                    var validOrders = orders.Where(o => o.Status == "Paid" || o.Status == "Completed");
                    foreach (var order in validOrders)
                    {
                        totalRevenue += order.FinalAmount;
                        int ticketsInOrder = order.OrderDetails?.Sum(od => od.Quantity) ?? order.Tickets?.Count ?? 0;
                        totalTicketsSold += ticketsInOrder;
                    }
                }
            }

            
            return Json(new
            {
                revenue = totalRevenue,
                tickets = totalTicketsSold,
                active = listEvents.Count(e => e.Status == "Approved")
            });
        }

        // UC40 - Export Attendee List as CSV
        [HttpGet]
        [Authorize(Roles = "organizer,admin")]
        public async Task<IActionResult> ExportAttendees(int eventId)
        {
            var orders = await _orderService.GetOrdersByEventIdAsync(eventId);
            var eventDetails = await _eventService.GetById(eventId);
            var eventName = eventDetails?.Title ?? $"Event_{eventId}";

            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"EVENT ATTENDEES & ORDERS REPORT");
            csv.AppendLine($"Event Name: \"{eventName.Replace("\"", "'")}\"");
            csv.AppendLine($"Exported Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
            csv.AppendLine();
            csv.AppendLine("Order ID,Customer Name,Email,Phone Number,Order Date,Tickets Quantity,Total Amount (VND),Payment Status");

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    int ticketCount = order.Tickets?.Count ?? order.OrderDetails?.Sum(od => od.Quantity) ?? 0;
                    string safeName = order.FullName?.Replace("\"", "'") ?? "N/A";
                    string safeEmail = order.Email?.Replace("\"", "'") ?? "N/A";
                    string safePhone = order.Phone?.Replace("\"", "'") ?? "N/A";
                    string formattedAmount = order.FinalAmount.ToString("N0");
                    csv.AppendLine($"{order.OrderId}," +
                                   $"\"{safeName}\"," +
                                   $"\"{safeEmail}\"," +
                                   $"\"{safePhone}\"," +
                                   $"\"{order.OrderDate?.ToString("dd/MM/yyyy HH:mm")}\"," +
                                   $"{ticketCount}," +
                                   $"\"{formattedAmount} VNĐ\"," +
                                   $"\"{order.Status}\"");
                }
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                        .Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString()))
                        .ToArray();

            var safeFileName = string.Join("_", eventName.Split(System.IO.Path.GetInvalidFileNameChars()));
            return File(bytes, "text/csv", $"Attendees_{safeFileName}_{DateTime.Now:yyyyMMdd}.csv");
        }

    }
}