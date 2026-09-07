using VoucherAPI.Models;

namespace VoucherAPI.Repositories
{
    public interface IUserVoucherRepository
    {

        IQueryable<UserVoucher> GetAll();
        Task<UserVoucher?> GetByIdAsync(int id);
        Task AddAsync(UserVoucher voucher);
        Task UpdateAsync(UserVoucher voucher);
        Task DeleteAsync(UserVoucher voucher);
    }
}
