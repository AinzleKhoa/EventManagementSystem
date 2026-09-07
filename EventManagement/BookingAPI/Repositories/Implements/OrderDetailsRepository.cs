using BookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPI.Repositories.Implements
{
    public class OrderDetailsRepository : IOrderDetailsRepository
    {
        private readonly BookingDbContext _context;
        public OrderDetailsRepository(BookingDbContext context)
        {
            _context = context;
        }
        public async Task Add(OrderDetail model)
        {
            try
            {
                _context.OrderDetails.Add(model);
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
                var model = await _context.OrderDetails.FirstOrDefaultAsync(g => g.OrderDetailId == id);
                if (model != null)
                {
                    _context.OrderDetails.Remove(model);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IQueryable<OrderDetail> GetAll()
        {
            try
            {
                return _context.OrderDetails.AsQueryable();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<OrderDetail> GetById(int id)
        {
            try
            {
                return await _context.OrderDetails.FirstOrDefaultAsync(g => g.OrderDetailId == id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task Update(OrderDetail model)
        {
            try
            {
                _context.OrderDetails.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
}
