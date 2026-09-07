using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Models;

namespace WebClient.Services
{
    public interface ITicketApiClient
    {
        Task<TicketCheckInResponseDTO> ScanTicketAsync(TicketCheckInRequestDTO request, string token);
        Task<IEnumerable<ReadTicketDTO>> GetTicketsByEventIdAsync(int eventId);
        Task<TicketValidationDTO> GetTicketByCode(Guid code, string token);
    }
}
