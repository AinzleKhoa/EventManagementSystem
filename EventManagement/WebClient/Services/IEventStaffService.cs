using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IEventStaffService
    {
        Task<IEnumerable<EventStaffResponseDto>> GetAssignedEventsAsync(int staffId);
    }
}
