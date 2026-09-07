using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface ITicketTypeService
    {
        Task Delete(int id);
        Task<ReadTicketTypeDTO> Create(CreateTicketTypeDTO model);
        Task Update(int id, UpdateTicketTypeDTO model);

    }
}