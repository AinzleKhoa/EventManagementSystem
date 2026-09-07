using Microsoft.AspNetCore.SignalR;

namespace SocialAPI.Hubs
{
    public class ReviewHub : Hub
    {
        public async Task JoinEventGroup(int eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetEventGroupName(eventId));
        }

        public async Task LeaveEventGroup(int eventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetEventGroupName(eventId));
        }

        public static string GetEventGroupName(int eventId) => $"event-{eventId}";
    }
}
