using System.Threading.Tasks;

namespace WithdrawalAPI.Services
{
    public interface IEventApiClient
    {
        Task<EventDetailsDTO?> GetEventByIdAsync(int eventId);
    }

    public class EventDetailsDTO
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public string Title { get; set; } = null!;
    }
}
