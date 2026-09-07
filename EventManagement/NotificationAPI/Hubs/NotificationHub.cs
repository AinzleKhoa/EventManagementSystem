using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationAPI.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
           
            var userId = Context.User?.FindFirst("sub")?.Value
                      ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("UserId")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
             
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"🟢 User {userId} joined group user_{userId}");
            }
            else
            {
                Console.WriteLine("🔴 Không tìm thấy UserId trong Token.");
            }

            await base.OnConnectedAsync();
        }
    }
}