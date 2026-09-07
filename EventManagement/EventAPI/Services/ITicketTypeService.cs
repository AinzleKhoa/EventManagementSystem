using EventAPI.DTOs;

namespace EventAPI.Services
{
    public interface ITicketTypeService
    {
        Task<List<ReadTicketTypeDTO>> GetTicketTypesByEventIdAsync(int eventId);
        Task<ReadTicketTypeDTO?> GetTicketTypeByIdAsync(int id);
        Task<ReadTicketTypeDTO> CreateTicketTypeAsync(CreateTicketTypeDTO createDto);
        Task<ReadTicketTypeDTO?> UpdateTicketTypeAsync(int id, UpdateTicketTypeDTO updateDto);
        Task UpdateTicketTypeQuantityAsync(int id, UpdateTicketTypeQuantityDTO updateQuanDTO);

        Task<bool> DeleteTicketTypeAsync(int id);
    }
}