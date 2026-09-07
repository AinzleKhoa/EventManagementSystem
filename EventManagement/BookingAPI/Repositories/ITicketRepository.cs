using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface ITicketRepository
    {
        Task Add(Ticket model);
        IQueryable<Ticket> GetAll();
        Task<Ticket> GetById(int id);
        Task Update(Ticket model);
        Task Delete(int id);
        Task<Ticket> GetTicketByCodeAsync(Guid ticketCode);
        Task<bool> MarkTicketAsCheckedInAsync(Guid ticketCode, int staffId);
        IQueryable<Ticket> GetAll(int eventID);
    }
}
