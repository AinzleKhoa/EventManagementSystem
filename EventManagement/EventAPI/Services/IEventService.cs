using EventAPI.DTOs;

namespace EventAPI.Services
{
    public interface IEventService
    {
        Task<List<ReadEventDTO>> GetAllAsync(int? organizerId);
        Task<ReadEventDTO?> GetEventByIdAsync(int id);
        Task<ReadEventDTO> CreateEventAsync(CreateEventDTO createDto);
        Task<ReadEventDTO?> UpdateEventAsync(int id, UpdateEventDTO updateDto);
        Task<bool> DeleteEventAsync(int id);
        Task<bool> SubmitForApprovalAsync(int id);
        Task<bool> HideEventAsync(int id);
        Task<bool> ResumeEventAsync(int id);
        Task<List<ReadEventDTO>> SearchEventsAsync(string? keyword, int[]? categoryIds, DateTime? fromDate, DateTime? toDate, decimal? minPrice, decimal? maxPrice);
        Task<List<ReadEventDTO>> GetPendingEventsAsync();
        Task<bool> ApproveEventAsync(int id);
        Task<bool> RejectEventAsync(int id, string reason);

        Task<IEnumerable<ReadEventDTO>> GetAssignedEventsAsync(int staffId);
        Task<ReadEventValidatorDTO> AddValidatorAsync(AddValidatorDTO addValidatorDto);
        Task<List<ReadEventValidatorDTO>> GetValidatorsByEventIdAsync(int eventId);
        Task<bool> RemoveValidatorAsync(int eventStaffId);
        Task<bool> DeactivateExpiredValidatorsAsync();

        Task<List<ReadEventDTO>> GetEventsByValidatorEmailAsync(string validatorEmail);
        Task<bool> IsValidatorOfEventAsync(int eventId, string validatorEmail);
        Task<List<ReadEventDTO>> GetEventByOrganizerId(int Organizerid);

        Task<int> GetTotalApprovedEventsAsync();
        Task<int> GetTotalEventsAsync();
        Task<List<ReadEventDTO>> GetTopSellingEventsAsync(int count = 5);
    }
}