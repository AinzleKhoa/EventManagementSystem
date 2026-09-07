using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Services
{
    public interface IVoucherService
    {
        Task<List<Voucher>> GetAllAsync();
        Task<Voucher?> GetByIdAsync(int id);
        Task<Voucher> CreateAsync(VoucherCreateDTO voucherCreateDTO);
        Task<bool> UpdateAsync(int id, VoucherUpdateDTO voucherUpdateDTO);
        Task<bool> DeleteAsync(int id);
        Task<PaginatedResultDTO<VoucherReadDTO>> SearchAndFilter(VoucherSearchFilterDTO filter);
        IQueryable<Voucher> GetQueryable();

    }

}
