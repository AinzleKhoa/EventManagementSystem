using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface IOrderDetailsRepository
    {
        Task Add(OrderDetail model);
        IQueryable<OrderDetail> GetAll();
        Task<OrderDetail> GetById(int id);
        Task Update(OrderDetail model);
        Task Delete(int id);
    }
}
