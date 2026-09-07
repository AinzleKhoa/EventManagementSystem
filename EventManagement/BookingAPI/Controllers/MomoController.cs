using AutoMapper;
using BookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MomoController : ControllerBase
    {
        private readonly IMomoService _momoService;
        private readonly IOrderService _orderService;
        IMapper _mapper;

        public MomoController(IMomoService momoService, IOrderService orderService, IMapper mapper)
        {
            _momoService = momoService;
            _orderService = orderService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("create-payment/{orderId}")]
        public async Task<ActionResult<string>> CreateMomoPayment(int orderId)
        {
            var order = await _orderService.GetById(orderId);

            if (order == null) return NotFound("Order not found");
            if (order.Status != "Pending") return BadRequest("Order is not pending");
            if (order.FinalAmount <= 0) return BadRequest("Invalid amount");

            var paymentUrl = await _momoService.CreatePaymentUrl(orderId, (int)order.FinalAmount);
            return paymentUrl;
        }

        [HttpGet("momo-return")]
        public async Task<IActionResult> MomoReturn()
        {
            var query = Request.Query;
            var orderIdStr = query["extraData"].ToString();
            var result = await _momoService.HandleMomoReturn(query);
            var frontendBaseUrl = "https://localhost:7104";

            if (result == "Invalid signature" || result == "Invalid order id" ||
                result == "Order not found" || result == "Cancelled" || result == "Payment failed")
            {
                return Redirect($"{frontendBaseUrl}/Booking/PaymentCancel");
            }


            if (int.TryParse(orderIdStr, out int orderId))
            {
                try
                {
                    var order = await _orderService.GetById(orderId);

                    if (order != null)
                    {
                        await _orderService.SendOrderPaidWebhookAsync(order);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Exception SIGNALR MOMO]: {ex.Message}");
                }
            }

            return Redirect($"{frontendBaseUrl}/Profile/OrderDetails/{orderIdStr}?payment=success");
        }
    }
}