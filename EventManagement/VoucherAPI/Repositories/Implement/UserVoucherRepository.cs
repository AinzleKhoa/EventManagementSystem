using Microsoft.EntityFrameworkCore;
using VoucherAPI.Models;

namespace VoucherAPI.Repositories.Implement
{
    public class UserVoucherRepository : IUserVoucherRepository
    {
        private readonly VoucherDbContext _context;

        public UserVoucherRepository(VoucherDbContext context)
        {
            _context = context;
        }

        public IQueryable<UserVoucher> GetAll()
          => _context.UserVouchers.AsQueryable();

        public async Task<UserVoucher?> GetByIdAsync(int id)
            => await _context.UserVouchers.FindAsync(id);

        public async Task AddAsync(UserVoucher voucher)
        {
            _context.UserVouchers.Add(voucher);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserVoucher voucher)
        {
            _context.UserVouchers.Update(voucher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(UserVoucher voucher)
        {
            _context.UserVouchers.Remove(voucher);
            await _context.SaveChangesAsync();
        }
    }
}
