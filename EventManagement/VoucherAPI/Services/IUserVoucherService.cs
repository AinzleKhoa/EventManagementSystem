using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Services
{
    public interface IUserVoucherService
    {

        IQueryable<UserVoucherReadDTO> GetAll();
        Task<UserVoucherReadDTO?> GetByIdAsync(int id);
        Task AddAsync(UserVoucherAddDTO voucher);
        Task UpdateAsync(UserVoucherUpdateDTO voucher);
        Task DeleteAsync(int id);
    }
}
