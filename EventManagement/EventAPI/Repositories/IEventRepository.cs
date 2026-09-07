using EventAPI.Models;

namespace EventAPI.Repositories
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllAsync();

        Task<Event?> GetByIdAsync(int id);

        Task<Event> CreateAsync(Event eventModel);

        Task<Event?> UpdateAsync(int id, Event eventModel);

        Task<bool> DeleteAsync(int id);

        Task<List<Event>> SearchAsync(string? keyword, int[]? categoryIds, DateTime? fromDate, DateTime? toDate, decimal? minPrice, decimal? maxPrice);

        Task<List<Event>> GetByStatusAsync(string status);

        Task<IEnumerable<Event>> GetAssignedEventsByStaffIdAsync(int staffId);

       
        IQueryable<Event> GetAssignedEventsQuery(int staffId);
        Task<EventStaff> AddValidatorAsync(EventStaff eventStaff);
        Task<EventStaff?> GetValidatorByIdAsync(int eventStaffId);
        Task<List<EventStaff>> GetValidatorsByEventIdAsync(int eventId);
        Task<EventStaff?> GetValidatorByEmailAndEventAsync(int eventId, string email);
        Task<bool> RemoveValidatorAsync(int eventStaffId);
        Task<EventStaff?> UpdateValidatorAsync(int eventStaffId, EventStaff eventStaff);
        Task<List<Event>> GetEventByOrganizerId(int organizerId);

        Task<int> CountAllAsync();
        Task<int> CountActiveByStatusAsync(string status);
        Task<List<Event>> GetTopSellingEventsAsync(int count);
    }
}
