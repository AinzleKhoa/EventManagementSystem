using Microsoft.AspNetCore.Mvc;
using WebClient.DTOs.Request;
using WebClient.Services;
using WebClient.ViewModel;

namespace WebClient.Controllers
{

    public class WishlistController : Controller
    {
        private readonly ISocialService _socialService;
        private readonly IEventService _eventService; 

        public WishlistController(ISocialService socialService, IEventService eventService)
        {
            _socialService = socialService;
            _eventService = eventService;
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Authentication");
            }
           

            var myWishlist = await _socialService.GetMyWishlist();

            var model = new List<WishlistItemViewModel>();

            if (myWishlist != null && myWishlist.Any())
            {
                foreach (var item in myWishlist)
                {
                    var eventDetails = await _eventService.GetById(item.EventId);

                    if (eventDetails != null)
                    {
                        model.Add(new WishlistItemViewModel
                        {
                            WishlistId = item.WishlistId,
                            EventId = item.EventId,
                            AddedAt = item.AddedAt,
                            Event = eventDetails 
                        });
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWishlist([FromBody] ToggleWishlistRequest request)
        {

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "unauthorized" });
            }
            try
            {
                if (request.IsAdding)
                {
                    var dto = new CreateWishlistDTO { EventId = request.EventId };
                    await _socialService.AddToWishlist(dto);
                }
                else
                {
                    await _socialService.RemoveFromWishlist(request.EventId);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Toggle Wishlist: {ex.Message}");
                return Json(new { success = false, message = "error" });
            }
        }
    }

    public class ToggleWishlistRequest
    {
        public int EventId { get; set; }
        public bool IsAdding { get; set; }
    }
}