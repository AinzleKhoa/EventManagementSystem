using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NotificationAPI.Hubs;

namespace NotificationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<DashboardHub> _hubContext;

        public WebhookController(IHubContext<DashboardHub> hubContext)
        {
            _hubContext = hubContext;
        }
        [HttpPost("order-paid/{eventId}")]
        public async Task<IActionResult> NotifyOrderPaid(int eventId)
        {
            await _hubContext.Clients.Group($"Event_{eventId}").SendAsync("UpdateDashboard");
            return Ok();
        }
    }
}