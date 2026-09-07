using BookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPI.Repositories.Implements
{
    public class TicketRepository : ITicketRepository
    {
        private readonly BookingDbContext _context;
        public TicketRepository(BookingDbContext context)
        {
            _context = context;
        }
        public async Task Add(Ticket model)
        {
            try
            {
                _context.Tickets.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task Delete(int id)
        {
            try
            {
                var model = await _context.Tickets.FirstOrDefaultAsync(g => g.TicketId == id);
                if (model != null)
                {
                    _context.Tickets.Remove(model);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IQueryable<Ticket> GetAll()
        {
            try
            {
                return _context.Tickets.AsQueryable();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<Ticket> GetById(int id)
        {
            try
            {
                return await _context.Tickets.FirstOrDefaultAsync(g => g.TicketId == id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task Update(Ticket model)
        {
            try
            {
                _context.Tickets.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        public async Task<Ticket> GetTicketByCodeAsync(Guid ticketCode)
        {
            // Có thể Include thêm Order hoặc TicketType nếu cần thiết
            try
            {
                return await _context.Tickets.FirstOrDefaultAsync(g => g.TicketCode == ticketCode);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<bool> MarkTicketAsCheckedInAsync(Guid ticketCode, int staffId)
        {
            
            var rowsAffected = await _context.Tickets
                .Where(t => t.TicketCode == ticketCode && t.Status == "Active")
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Status, "CheckedIn")
                    .SetProperty(t => t.CheckedInAt, DateTime.Now)
                    .SetProperty(t => t.CheckedInBy, staffId));

            
            return rowsAffected > 0;
        }

        public IQueryable<Ticket> GetAll(int eventID)
        {
            try
            {
                return _context.Tickets.Where(e=>e.EventId == eventID).AsQueryable();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
