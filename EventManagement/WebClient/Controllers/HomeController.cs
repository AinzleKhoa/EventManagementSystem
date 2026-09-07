using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebClient.Models;
using WebClient.Services;
using WebClient.DTOs.Response;
using WebClient.ViewModel;
using WebClient.Services.Implements;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ICategoryService _categoryService;
        private readonly ISocialService _socialService;
        private readonly IAuthService _userService;
        private readonly IOrderService _orderService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IEventService eventService,
            ICategoryService categoryService,
            ISocialService socialService,
            IAuthService userService,
            IOrderService orderService,
            ILogger<HomeController> logger)
        {
            _eventService = eventService;
            _categoryService = categoryService;
            _socialService = socialService;
            _userService = userService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categoriesTask = _categoryService.GetAll();
            var eventsTask = _eventService.GetAll();

            await Task.WhenAll(categoriesTask, eventsTask);

            var allCategories = categoriesTask.Result;
            var events = eventsTask.Result;

            var approvedEvents = events
                .Where(e => e.Status == "Approved" && e.EndDate >= DateTime.Now)
                .OrderBy(e => e.StartDate)
                .ToList();

            var wishlistedIds = new List<int>();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    var myWishlist = await _socialService.GetMyWishlist();
                    if (myWishlist != null)
                    {
                        wishlistedIds = myWishlist.Select(w => w.EventId).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load wishlist in Index.");
                }
            }

            var model = new SearchViewModel
            {
                Events = approvedEvents,
                Categories = allCategories,
                WishlistedEventIds = wishlistedIds
            };

            return View(model);
        }

        public async Task<IActionResult> EventDetails(int id, string sortOrder = "newest")
        {
            var eventData = await _eventService.GetById(id);
            if (eventData == null || eventData.Status != "Approved")
            {
                return NotFound("This event has not been approved or does not exist.");
            }

            ReadUserDTO? organizerInfo = null;
            if (eventData.OrganizerId > 0)
            {
                organizerInfo = await _userService.GetUserById(eventData.OrganizerId);
            }

            string odataQuery = sortOrder switch
            {
                "oldest" => "?$orderby=CreatedAt asc",
                "highest" => "?$orderby=Rating desc, CreatedAt desc",
                "lowest" => "?$orderby=Rating asc, CreatedAt desc",
                _ => "?$orderby=CreatedAt desc"
            };

            var reviews = await _socialService.GetReviewsByEventId(id, odataQuery);

            if (reviews != null && reviews.Any())
            {
                var distinctUserIds = reviews.Select(r => r.UserId).Distinct().ToList();
                var userMap = new Dictionary<int, ReadUserDTO>();

                var userTasks = distinctUserIds.Select(async userId =>
                {
                    var userDto = await _userService.GetUserById(userId);
                    return new { UserId = userId, User = userDto };
                });

                var userResults = await Task.WhenAll(userTasks);

                foreach (var result in userResults)
                {
                    if (result.User != null) userMap[result.UserId] = result.User;
                }

                foreach (var review in reviews)
                {
                    if (userMap.TryGetValue(review.UserId, out var userInfo))
                    {
                        review.UserName = !string.IsNullOrEmpty(userInfo.FullName) ? userInfo.FullName : "User #" + review.UserId;
                        review.AvatarUrl = !string.IsNullOrEmpty(userInfo.AvatarUrl) ? userInfo.AvatarUrl : $"https://placehold.co/100x100?text={review.UserName[0]}";
                    }
                    else
                    {
                        review.UserName = "Anonymous";
                        review.AvatarUrl = "https://placehold.co/100x100?text=?";
                    }
                }
            }

            bool isWishlisted = false;
            bool hasBooked = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    var wishlistTask = _socialService.GetMyWishlist();
                    var bookingTask = _orderService.CheckUserHasPaidTicket(id);

                    await Task.WhenAll(wishlistTask, bookingTask);

                    var myWishlist = wishlistTask.Result;
                    if (myWishlist != null)
                    {
                        isWishlisted = myWishlist.Any(w => w.EventId == id);
                    }

                    hasBooked = bookingTask.Result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to load user state (wishlist/booking) for Event {id}.");
                }
            }

            ViewBag.IsWishlisted = isWishlisted;
            ViewBag.HasBooked = hasBooked;

            EventDetailViewModel model = new EventDetailViewModel
            {
                Event = eventData,
                Reviews = reviews,
                Organizer = organizerInfo
            };

            return View(model);
        }

        public async Task<IActionResult> Search(string? query, List<int>? categoryIds, string? dateFilter, decimal? minPrice, decimal? maxPrice, bool freeOnly = false, string? sortOption = null, string eventStatus = "available", int page = 1)
        {
            var model = await BuildSearchViewModel(query, categoryIds, dateFilter, minPrice, maxPrice, freeOnly, sortOption, eventStatus, page);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SearchResults(string? query, List<int>? categoryIds, string? dateFilter, decimal? minPrice, decimal? maxPrice, bool freeOnly = false, string? sortOption = null, string eventStatus = "available", int page = 1)
        {
            var model = await BuildSearchViewModel(query, categoryIds, dateFilter, minPrice, maxPrice, freeOnly, sortOption, eventStatus, page);
            return PartialView("_SearchResults", model);
        }

        private async Task<SearchViewModel> BuildSearchViewModel(string? query, List<int>? categoryIds, string? dateFilter, decimal? minPrice, decimal? maxPrice, bool freeOnly, string? sortOption, string eventStatus, int page)
        {
            var allCategories = await _categoryService.GetAll();
            var searchResults = await _eventService.Search(query, categoryIds, dateFilter, minPrice, maxPrice, freeOnly);

            var approvedResults = searchResults
                .Where(e => e.Status == "Approved")
                .ToList();

            eventStatus = eventStatus == "ended" ? "ended" : "available";

            approvedResults = eventStatus switch
            {
                "ended" => approvedResults.Where(e => e.EndDate < DateTime.Now).ToList(),
                _ => approvedResults.Where(e => e.EndDate >= DateTime.Now).ToList()
            };

            approvedResults = sortOption switch
            {
                "date_asc" => approvedResults.OrderBy(e => e.StartDate).ToList(),
                "price_asc" => approvedResults.OrderBy(e => e.TicketTypes?.Min(t => t.Price) ?? 0).ToList(),
                "price_desc" => approvedResults.OrderByDescending(e => e.TicketTypes?.Min(t => t.Price) ?? 0).ToList(),
                _ => approvedResults.OrderByDescending(e => e.StartDate).ToList()
            };

            int pageSize = 6;
            int totalItems = approvedResults.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedEvents = approvedResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var wishlistedIds = new List<int>();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    var myWishlist = await _socialService.GetMyWishlist();
                    if (myWishlist != null)
                    {
                        wishlistedIds = myWishlist.Select(w => w.EventId).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load wishlist in Search.");
                }
            }

            var model = new SearchViewModel
            {
                Events = pagedEvents,
                Categories = allCategories,
                Query = query,
                SelectedCategoryIds = categoryIds ?? new List<int>(),

                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,

                WishlistedEventIds = wishlistedIds
            };

            ViewBag.DateFilter = dateFilter;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.FreeOnly = freeOnly;
            ViewBag.SortOption = sortOption;
            ViewBag.EventStatus = eventStatus;

            return model;
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
