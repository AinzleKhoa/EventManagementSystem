using EventAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventAPI.Repositories.Implement
{
    public class EventRepository : IEventRepository
    {
        private readonly EventsDbContext _context;

        public EventRepository(EventsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Event>> GetAllAsync()
        {
            return await _context.Events
                                 .Include(e => e.TicketTypes)
                                 .OrderByDescending(e => e.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _context.Events
                                 .Include(e => e.TicketTypes)
                                 .FirstOrDefaultAsync(e => e.EventId == id);
        }

        public async Task<Event> CreateAsync(Event eventModel)
        {
            await _context.Events.AddAsync(eventModel);
            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task<Event?> UpdateAsync(int id, Event eventModel)
        {
            if (!_context.Events.Local.Any(e => e.EventId == id))
            {
                _context.Events.Attach(eventModel);
                _context.Entry(eventModel).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return false;

            var ticketTypes = _context.TicketTypes.Where(t => t.EventId == id);
            _context.TicketTypes.RemoveRange(ticketTypes);

            var staffs = _context.EventStaffs.Where(s => s.EventId == id);
            _context.EventStaffs.RemoveRange(staffs);

            _context.Events.Remove(eventModel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Event>> SearchAsync(string? keyword, int[]? categoryIds, DateTime? fromDate, DateTime? toDate, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Events.Include(e => e.TicketTypes).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(e => e.Title.Contains(keyword) || (e.Location != null && e.Location.Contains(keyword)));
            }

            if (categoryIds != null && categoryIds.Length > 0)
            {
                query = query.Where(e => categoryIds.Contains(e.CategoryId));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.StartDate <= toDate.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(e => e.TicketTypes.Any(t => t.Price >= minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(e => e.TicketTypes.Any(t => t.Price <= maxPrice.Value));
            }

            return await query.OrderBy(e => e.StartDate).ToListAsync();
        }

        public async Task<List<Event>> GetByStatusAsync(string status)
        {
            return await _context.Events
                                 .Include(e => e.TicketTypes)
                                 .Where(e => e.Status == status)
                                 .OrderBy(e => e.CreatedAt) 
                                 .ToListAsync();
        }


        public async Task<IEnumerable<Event>> GetAssignedEventsByStaffIdAsync(int staffId)
        {
            return await _context.Events
                .Where(e => e.EventStaffs.Any(es => es.StaffId == staffId && es.IsActive == true))
                .ToListAsync();
        }

        public IQueryable<Event> GetAssignedEventsQuery(int staffId)
        {
            return _context.Events
                .Where(e => e.EventStaffs.Any(es => es.StaffId == staffId && es.IsActive == true))
                .AsQueryable();
        }

        public async Task<EventStaff> AddValidatorAsync(EventStaff eventStaff)
        {
            await _context.EventStaffs.AddAsync(eventStaff);
            await _context.SaveChangesAsync();
            return eventStaff;
        }

        public async Task<EventStaff?> GetValidatorByIdAsync(int eventStaffId)
        {
            return await _context.EventStaffs
                                 .FirstOrDefaultAsync(es => es.EventStaffId == eventStaffId);
        }

        public async Task<List<EventStaff>> GetValidatorsByEventIdAsync(int eventId)
        {
            return await _context.EventStaffs
                                 .Where(es => es.EventId == eventId && es.IsActive == true)
                                 .ToListAsync();
        }

        public async Task<EventStaff?> GetValidatorByEmailAndEventAsync(int eventId, string email)
        {
            return await _context.EventStaffs
                                 .FirstOrDefaultAsync(es => es.EventId == eventId && es.StaffEmail == email);
        }

        public async Task<bool> RemoveValidatorAsync(int eventStaffId)
        {
            var validator = await _context.EventStaffs.FindAsync(eventStaffId);
            if (validator == null) return false;

            _context.EventStaffs.Remove(validator);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EventStaff?> UpdateValidatorAsync(int eventStaffId, EventStaff eventStaff)
        {
            var existing = await _context.EventStaffs.FindAsync(eventStaffId);
            if (existing == null) return null;

            existing.IsActive = eventStaff.IsActive;
            existing.ExpiredAt = eventStaff.ExpiredAt;
            existing.ValidatorRole = eventStaff.ValidatorRole;

            await _context.SaveChangesAsync();
            return existing;

        }
        public async Task<List<Event>> GetEventByOrganizerId(int organizerId) { 
            return await _context.Events
                                 .Include(e => e.TicketTypes)
                                 .Where(e => e.OrganizerId == organizerId)
                                 .OrderByDescending(e => e.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<int> CountAllAsync()
        {
            return await _context.Events.CountAsync();
        }

        public async Task<int> CountActiveByStatusAsync(string status)
        {
            var now = DateTime.Now;
            return await _context.Events
                .CountAsync(e => e.Status == status && e.EndDate >= now);
        }

        public async Task<List<Event>> GetTopSellingEventsAsync(int count)
        {
            return await _context.Events
                .Include(e => e.TicketTypes)
                .Where(e => e.Status == "Approved") 
                .OrderByDescending(e => e.TicketTypes.Sum(t => t.SoldQuantity))
                .Take(count)
                .ToListAsync();
        }
    }
}
