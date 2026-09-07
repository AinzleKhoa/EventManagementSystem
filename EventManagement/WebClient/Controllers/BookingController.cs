using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Services;
using WebClient.ViewModel;

namespace WebClient.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly IUserVoucherService _userVoucherService;
        IMapper _mapper;

        public BookingController(IUserVoucherService userVoucherService, IEventService eventService, IUserService userService, IOrderService orderService, IMapper mapper, IVnPayService vnPayService, IMomoService momoService)
        {
            _eventService = eventService;
            _userService = userService;
            _orderService = orderService;
            _mapper = mapper;
            _vnPayService = vnPayService;
            _userVoucherService = userVoucherService;
            _momoService = momoService;
        }
        [HttpGet]
        public async Task<IActionResult> Checkout(int eventId, Dictionary<int, int> cart)
        {
            if (cart == null || !cart.Any(x => x.Value > 0))
            {
                TempData["ToastMessage"] = "Please select at least 1 ticket to book!";
                TempData["ToastType"] = "warning";
                return RedirectToAction("EventDetails", "Home", new { id = eventId });
            }

            var eventData = await _eventService.GetById(eventId);
            if (eventData == null) return NotFound();

            var selectedTickets = new List<TicketSelectionViewModel>();

            if (eventData.TicketTypes != null)
            {
                foreach (var item in cart.Where(x => x.Value > 0))
                {
                    var ticket = eventData.TicketTypes
                        .FirstOrDefault(t => t.TicketTypeId == item.Key);

                    if (ticket == null) continue;

                    selectedTickets.Add(new TicketSelectionViewModel
                    {
                        TicketTypeId = ticket.TicketTypeId,
                        TicketTypeName = ticket.TypeName,
                        Quantity = item.Value,
                        Price = ticket.Price
                    });
                }
            }

            if (!selectedTickets.Any())
                return RedirectToAction("EventDetails", "Home", new { id = eventId });

            var subTotal = selectedTickets.Sum(x => x.Quantity * x.Price);
            var total = selectedTickets.Sum(x => x.Quantity * x.Price);

            var userId = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Authentication");

            ReadUserDTO user = await _userService.GetById(int.Parse(userId));
            if (user == null || user.IsActive != true)
            {
                await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ToastMessage"] = "Your account has been locked by Administrator.";
                TempData["ToastType"] = "danger";
                return RedirectToAction("Login", "Authentication");
            }

            var vm = new CheckoutViewModel
            {
                EventId = eventId,
                CustomerId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.PhoneNumber,
                Event = eventData,
                Tickets = selectedTickets,
                SubTotal = subTotal,
                TotalAmount = total
            };
            ViewBag.UserVoucher = await _userVoucherService.GetByUserId(user.UserId, eventId);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var checkUser = await _userService.GetById(int.Parse(userId));
                if (checkUser == null || checkUser.IsActive != true)
                {
                    await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["ToastMessage"] = "Your account has been locked by Administrator.";
                    TempData["ToastType"] = "danger";
                    return RedirectToAction("Login", "Authentication");
                }
            }
            var order = _mapper.Map<CreateOrderDTO>(model);
            List<CreateOrderDetailsDTO> orderDetails = new List<CreateOrderDetailsDTO>();
            foreach (var orderDetail in model.Tickets)
            {
                CreateOrderDetailsDTO c = new CreateOrderDetailsDTO()
                {
                    Quantity = orderDetail.Quantity,
                    TicketTypeId = orderDetail.TicketTypeId,
                    UnitPrice = orderDetail.Price
                };
                orderDetails.Add(c);
            }
            order.OrderDetails = orderDetails;

            var response = await _orderService.Add(order);
            // Check if validate fail
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errors = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

                foreach (var error in errors.Errors)
                {
                    foreach (var msg in error.Value)
                    {
                        ModelState.AddModelError(error.Key, msg);
                    }
                }
                var eventData = await _eventService.GetById(model.EventId);
                var subTotal = model.Tickets.Sum(x => x.Quantity * x.Price);
                var total = model.Tickets.Sum(x => x.Quantity * x.Price);

                ViewBag.UserVoucher = await _userVoucherService.GetByUserId(model.CustomerId, model.EventId);

                model.Event = eventData;
                model.SubTotal = subTotal;
                model.TotalAmount = total;
                Console.WriteLine(model.ToJson());

                return View(model);
            }
            // Check if event is invalid or something
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorMsg = await response.Content.ReadFromJsonAsync<CreateOrderResponseDTO>();

                TempData["ToastMessage"] = errorMsg.Message;
                TempData["ToastType"] = "danger";

                return RedirectToAction("EventDetails", "Home", new { id = model.EventId });
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Authentication");
            }

            response.EnsureSuccessStatusCode();
            // if order create success, run code as below
            var result = await response.Content.ReadFromJsonAsync<CreateOrderResponseDTO>();

            var orderId = result.OrderId;
            var paymentUrl = "";

            if (model.PaymentMethod != null)
            {
                if (model.PaymentMethod.Equals("Momo"))
                {
                    paymentUrl = await _momoService.CreatePaymentUrl(orderId);
                }
                else
                {
                    paymentUrl = await _vnPayService.CreatePaymentUrl(orderId);
                }
                return Redirect(paymentUrl);
            }

            return RedirectToAction("OrderDetails", "Profile", new { id = orderId });

        }


        [HttpGet]
        public IActionResult PaymentCancel()
        {
            return View();
        }
    }
}
