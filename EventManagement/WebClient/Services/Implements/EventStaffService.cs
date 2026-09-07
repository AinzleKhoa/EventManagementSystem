using WebClient.DTOs.Response;
using WebClient.Api;

namespace WebClient.Services.Implements
{
    public class EventStaffService : IEventStaffService
    {
        private readonly HttpClient _httpClient;

        public EventStaffService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<EventStaffResponseDto>> GetAssignedEventsAsync(int staffId)
        {
            // Kết quả: event-gateway/eventstaff/4
            var url = $"{EventStaffAPI.Base}/{staffId}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<EventStaffResponseDto>>()
                       ?? new List<EventStaffResponseDto>();
            }
            return new List<EventStaffResponseDto>();
        }

        
    }
}