using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

public interface IEventService
{
    Task<List<ReadEventDTO>> GetAll(int? organizerId = null); 
    Task<ReadEventDTO?> GetById(int id);
    Task Add(CreateEventDTO model);
    Task Update(int id, UpdateEventDTO model);
    Task Delete(int id);
    Task Submit(int id);
    Task Hide(int id);
    Task Resume(int id);
    Task<List<ReadEventDTO>> Search(string? query, List<int>? categoryIds, string? dateFilter, decimal? minPrice, decimal? maxPrice, bool freeOnly);
    Task<List<ReadEventDTO>> GetPendingEvents();
    Task Approve(int id);
    Task Reject(int id, string reason);
    Task<ReadEventDTO?> GetEventByOrganizerId(int id);

    Task<long> GetTotalEventsCount();
    Task<long> GetTotalPublicEventsCount();
    Task<List<ReadEventDTO>> GetTopSellingEvents(int count = 5);
}