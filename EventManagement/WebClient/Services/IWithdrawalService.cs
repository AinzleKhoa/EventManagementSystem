using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IWithdrawalService
    {
        Task<ReadWithdrawalDTO> Create(CreateWithdrawalDTO dto);
        Task<ReadWithdrawalDTO> Update(int id, UpdateWithdrawalDTO dto);
        Task<List<ReadWithdrawalDTO>> GetMine();
        Task<PaginatedResultDTO<ReadWithdrawalDTO>> SearchAndFilter(WithdrawalFilterDTO filter);
        Task<ReadWithdrawalDTO> GetById(int id);
        Task<ReadWithdrawalDTO> Approve(int id);
        Task<ReadWithdrawalDTO> Reject(int id, RejectReasonDTO dto);
    }
}
