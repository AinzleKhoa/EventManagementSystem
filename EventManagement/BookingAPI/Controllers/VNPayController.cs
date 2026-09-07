using AutoMapper;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.Models;
using BookingAPI.Repositories;
using BookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderService _orderService;
        IMapper _mapper;

        public VNPayController(IVnPayService vnPayService, IOrderService orderService, IMapper mapper)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("create-payment/{orderId}")]
        public async Task<ActionResult<string>> CreatePayment(int orderId)
        {
            var order = await _orderService.GetById(orderId);

            if (order == null) return NotFound("Order not found");
            if (order.Status != "Pending") return BadRequest("Order is not pending");
            if (order.FinalAmount <= 0) return BadRequest("Invalid amount");

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var paymentUrl = _vnPayService.CreatePaymentUrl(order.OrderId, (int)order.FinalAmount, ipAddress);

            return paymentUrl;
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var query = Request.Query;
            string rawQuery = Request.QueryString.Value;
            var orderIdStr = query["vnp_TxnRef"].ToString();

            var result = await _vnPayService.HandleVnPayReturn(query, rawQuery);
            var frontendBaseUrl = "https://localhost:7104";

            if (result == "Invalid signature" || result == "Invalid order id" ||
                result == "Order not found" || result == "Cancelled")
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
                    Console.WriteLine($"[Exception SIGNALR VNPAY]: {ex.Message}");
                }
            }

            return Redirect($"{frontendBaseUrl}/Profile/OrderDetails/{orderIdStr}?payment=success");
        }

    }
}