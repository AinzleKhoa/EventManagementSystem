using EventAPI.Models;

namespace EventAPI.Repositories
{
    public interface IEventStaffRepository
    {
        Task<IEnumerable<Event>> GetEventsByStaffIdAsync(int staffId);
    }
}
