using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IEventValidatorService
    {
        Task<EventValidatorDTO> AddValidatorAsync(int eventId, string validatorEmail);
        Task<List<EventValidatorDTO>> GetValidatorsByEventIdAsync(int eventId);
        Task<bool> RemoveValidatorAsync(int eventStaffId);
        Task<List<ReadEventDTO>> GetEventsByValidatorEmailAsync(string validatorEmail);
        Task<bool> IsValidatorOfEventAsync(int eventId, string validatorEmail);
    }
}
