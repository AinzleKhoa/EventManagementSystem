using Microsoft.AspNetCore.SignalR;

namespace NotificationAPI.Hubs
{
    public class DashboardHub : Hub
    {
        // Nhóm các user theo EventId thay vì OrganizerId
        public async Task JoinEventGroup(int eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Event_{eventId}");
        }
    }
}