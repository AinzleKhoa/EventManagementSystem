using BookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPI.Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BookingDbContext _context;
        public OrderRepository(BookingDbContext context)
        {
            _context = context;
        }
        public async Task<Order> Add(Order model)
        {
            try
            {
                await _context.Orders.AddAsync(model);
                await _context.SaveChangesAsync();
                return model;
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
                var model = await _context.Orders.FirstOrDefaultAsync(g => g.OrderId == id);
                if (model != null)
                {
                    _context.Orders.Remove(model);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IQueryable<Order> GetAll()
        {
            try
            {
                return _context.Orders
                    .Include(p => p.OrderDetails)
                    .AsQueryable();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<Order> GetById(int id)
        {
            try
            {
                return await _context.Orders
                    .Include(p => p.OrderDetails)
                    .Include(p => p.Tickets)
                    .FirstOrDefaultAsync(g => g.OrderId == id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task Update(Order model)
        {
            try
            {
                _context.Orders.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
}
