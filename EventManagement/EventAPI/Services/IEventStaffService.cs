using EventAPI.DTOs;

namespace EventAPI.Services
{
    public interface IEventStaffService
    {
        Task<IEnumerable<EventStaffResponseDto>> GetStaffEventsAsync(int staffId);
    }
}
