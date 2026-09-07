using WithdrawalAPI.DTOs;

namespace WithdrawalAPI.Services
{
    public interface IWithdrawalService
    {
        Task<ReadWithdrawalDTO> Create(int organizerId, CreateWithdrawalDTO model);
        Task<ReadWithdrawalDTO> Update(int id, int organizerId, UpdateWithdrawalDTO model);
        Task<List<ReadWithdrawalDTO>> GetByOrganizer(int organizerId);
        Task<PaginatedResultDTO<ReadWithdrawalDTO>> SearchAndFilter(WithdrawalFilterDTO filter);
        Task<ReadWithdrawalDTO> Approve(int id);
        Task<ReadWithdrawalDTO> Reject(int id, RejectReasonDTO dto);
        Task<ReadWithdrawalDTO> GetById(int id);
        IQueryable<ReadWithdrawalDTO> GetAllQueryable();
    }
}
