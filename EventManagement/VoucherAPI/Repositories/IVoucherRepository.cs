using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Repositories
{
    public interface IVoucherRepository
    {
        Task<List<Voucher>> GetAllAsync();
        Task<Voucher?> GetByIdAsync(int id);
        Task AddAsync(Voucher voucher);
        Task UpdateAsync(Voucher voucher);
        Task DeleteAsync(Voucher voucher);
        Task<(List<Voucher> Items, int TotalCount)> SearchAndFilter(VoucherSearchFilterDTO filter);
        IQueryable<Voucher> GetQueryable();
    }

}
