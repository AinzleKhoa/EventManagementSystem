using EventAPI.Models;
using EventAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventAPI.Repositories.Implement
{
    public class TicketTypeRepository : ITicketTypeRepository
    {
        private readonly EventsDbContext _context;

        public TicketTypeRepository(EventsDbContext context)
        {
            _context = context;
        }

        public async Task<List<TicketType>> GetByEventIdAsync(int eventId)
        {
            return await _context.TicketTypes
                                 .Where(t => t.EventId == eventId)
                                 .ToListAsync();
        }

        public async Task<TicketType?> GetByIdAsync(int id)
        {
            return await _context.TicketTypes.FindAsync(id);
        }

        public async Task<TicketType> CreateAsync(TicketType ticketType)
        {
            await _context.TicketTypes.AddAsync(ticketType);
            await _context.SaveChangesAsync();
            return ticketType;
        }

        public async Task<TicketType> UpdateAsync(TicketType ticketType)
        {
            _context.TicketTypes.Update(ticketType);
            await _context.SaveChangesAsync();
            return ticketType;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticketType = await _context.TicketTypes.FindAsync(id);
            if (ticketType == null) return false;

            _context.TicketTypes.Remove(ticketType);
            await _context.SaveChangesAsync();
            return true;
        }
       
    }
}
