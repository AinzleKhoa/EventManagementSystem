using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IVoucherService
    {
        Task Add(CreateVoucherDTO model);
        Task<List<ReadVoucherDTO>> GetAll();
        Task<ReadVoucherDTO> GetById(int id);
        Task Update(int id, UpdateVoucherDTO model);
        Task Delete(int id);
        Task<PaginatedResultDTO<ReadVoucherDTO>> SearchAndFilter(VoucherSearchFilterDTO filter);
        Task DistributeByEmail(DistributeByEmailRequestDTO dto);
        Task DistributeToEvent(DistributeToEventRequestDTO dto);
    }
}
