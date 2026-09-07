using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> Add(Order model);
        IQueryable<Order> GetAll();
        Task<Order> GetById(int id);
        Task Update(Order model);
        Task Delete(int id);
    }
}
