using EventAPI.Models;

namespace EventAPI.Repositories
{
    public interface ITicketTypeRepository
    {
        Task<List<TicketType>> GetByEventIdAsync(int eventId);
        Task<TicketType?> GetByIdAsync(int id);
        Task<TicketType> CreateAsync(TicketType ticketType);
        Task<TicketType> UpdateAsync(TicketType ticketType);
        Task<bool> DeleteAsync(int id);
       
    }
}
