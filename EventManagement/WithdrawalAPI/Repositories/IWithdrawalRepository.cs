using WithdrawalAPI.DTOs;
using WithdrawalAPI.Models;

namespace WithdrawalAPI.Repositories
{
    public interface IWithdrawalRepository
    {
        Task Add(WithdrawalRequest model);
        Task<WithdrawalRequest?> GetById(int id);
        Task<List<WithdrawalRequest>> GetByOrganizerId(int organizerId);
        Task Update(WithdrawalRequest model);
        Task<(List<WithdrawalRequest> Items, int TotalCount)> SearchAndFilter(WithdrawalFilterDTO filter);
        Task<bool> CanEdit(int id);
        IQueryable<WithdrawalRequest> GetQueryable();
    }
}
