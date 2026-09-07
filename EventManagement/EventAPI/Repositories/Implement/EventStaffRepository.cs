using EventAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventAPI.Repositories.Implement
{
    public class EventStaffRepository : IEventStaffRepository
    {
        private readonly EventsDbContext _context;

        public EventStaffRepository(EventsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetEventsByStaffIdAsync(int staffId)
        {
            return await _context.EventStaffs
                .Where(es => es.StaffId == staffId && es.IsActive == true)
                .Include(es => es.Event) 
                .Select(es => es.Event)
                .ToListAsync();
        }
    }
}
