using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationAPI.Providers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("sub")?.Value
                ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("UserId")?.Value;
        }
    }
}