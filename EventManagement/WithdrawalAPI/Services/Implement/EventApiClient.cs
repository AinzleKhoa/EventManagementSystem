using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WithdrawalAPI.Services.Implement
{
    public class EventApiClient : IEventApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EventApiClient> _logger;

        public EventApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<EventApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var eventApiUrl = configuration["EventAPI:Url"] ?? "https://localhost:7289";
            _httpClient.BaseAddress = new Uri(eventApiUrl);
        }

        public async Task<EventDetailsDTO?> GetEventByIdAsync(int eventId)
        {
            try
            {
                _logger.LogInformation($"[WITHDRAWAL] Calling EventAPI to get event details for EventId: {eventId}");
                var response = await _httpClient.GetAsync($"/api/Events/{eventId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"[WITHDRAWAL] Event {eventId} not found or failed in EventAPI. Status: {response.StatusCode}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<EventDetailsDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[WITHDRAWAL] ❌ Exception when calling EventAPI for EventId: {eventId}. Error: {ex.Message}");
                return null;
            }
        }
    }
}
