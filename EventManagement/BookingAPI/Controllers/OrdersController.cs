using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingAPI.Models;
using BookingAPI.Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.DTOs.OrderDetailsDTO;
using NuGet.Protocol;

namespace BookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private (int UserId, string Role) GetUserInfo()
        {
            var identity = HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
            var userIdClaim = identity?.FindFirst("sub")
                           ?? identity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                           ?? identity?.FindFirst("UserId");
            var roleClaim = identity?.FindFirst(System.Security.Claims.ClaimTypes.Role) 
                           ?? identity?.FindFirst("role");

            int.TryParse(userIdClaim?.Value, out int userId);
            return (userId, roleClaim?.Value ?? "");
        }

        // GET: api/Orders
        [HttpGet]
        [EnableQuery]
        public IQueryable<ReadOrderDTO> GetOrders()
        {
            return _orderService.GetAll();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadOrderDTO>> GetOrder(int id)
        {
            var order = await _orderService.GetById(id);

            if (order == null)
            {
                return NotFound();
            }

            var userInfo = GetUserInfo();
            if (userInfo.Role != "admin" && order.CustomerId != userInfo.UserId)
            {
                return Forbid();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            var existingOrder = await _orderService.GetById(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            var userInfo = GetUserInfo();
            if (userInfo.Role != "admin" && existingOrder.CustomerId != userInfo.UserId)
            {
                return Forbid();
            }

            order.CustomerId = existingOrder.CustomerId; // Prevent ownership transfer
            await _orderService.Update(id, order);


            if (order.Status == "Paid" || order.Status == "Completed")
            {
                try
                {
                    int eventId = order.EventId;
                    
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                    };

                    using (var httpClient = new HttpClient(handler))
                    {
                        string notificationApiUrl = $"https://localhost:7197/api/webhook/order-paid/{eventId}";

                        
                        var response = await httpClient.PostAsync(notificationApiUrl, null);

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"[Error WEBHOOK] Server return:: {response.StatusCode}");
                        }
                        else
                        {
                            Console.WriteLine("[Success] Sent Webhook to NotificationAPI!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EXCEPTION SIGNALR]: {ex.Message}");
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostOrder(CreateOrderDTO order, [FromHeader(Name = "Authorization")] string authorization)
        {
            var userInfo = GetUserInfo();
            if (userInfo.UserId <= 0)
            {
                return Unauthorized(new { message = "Invalid token or missing User ID." });
            }

            order.CustomerId = userInfo.UserId;

            if (!ModelState.IsValid)
            {
                return BadRequest(order);
            }
            string msg = "Add success";
            try
            {
                var orderId = await _orderService.Add(order);
                return Ok(new
                {
                    message = "Add success.",
                    orderId = orderId
                });
            }
            catch (Exception ex)
            {
                return Conflict(new { message = ex.Message });
            }


        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            var userInfo = GetUserInfo();
            if (userInfo.Role != "admin" && order.CustomerId != userInfo.UserId)
            {
                return Forbid();
            }

            await _orderService.Delete(id);
            return NoContent();
        }

        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<List<ReadOrderDTO>>> GetOrdersByEventId(int eventId)
        {
            
            var orders = await _orderService.GetAll()
                                            .Where(o => o.EventId == eventId)
                                            .ToListAsync();

            return Ok(orders);
        }


        [HttpGet("check-paid-ticket/{eventId}")]
        public async Task<IActionResult> CheckPaidTicket(int eventId, [FromQuery] int? userId = null)
        {
            int customerId = -1;


            if (userId.HasValue)
            {
                customerId = userId.Value;
            }

            else
            {
                var identity = HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
                var userClaim = identity?.FindFirst("sub")
                             ?? identity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                             ?? identity?.FindFirst("UserId");

                if (userClaim != null && int.TryParse(userClaim.Value, out int parsedId))
                {
                    customerId = parsedId;
                }
            }

            if (customerId == -1)
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }


            bool hasPaid = await _orderService.HasPaidTicketAsync(customerId, eventId);
            return Ok(new { hasPaid = hasPaid });
        }
    }
}