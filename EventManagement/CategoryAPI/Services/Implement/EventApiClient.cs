using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CategoryAPI.Services.Implement
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

        public async Task<bool> HasRelatedEventsAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation($"[CATEGORY] Checking related events for categoryId: {categoryId}");
                var response = await _httpClient.GetAsync($"/api/Events/search?categoryIds={categoryId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[CATEGORY] ❌ Failed to check related events for category {categoryId}. Status: {response.StatusCode}, Response: {error}");
                    return false;
                }

                var events = await response.Content.ReadFromJsonAsync<List<object>>();
                var hasEvents = events != null && events.Count > 0;
                
                _logger.LogInformation($"[CATEGORY] Result for categoryId {categoryId}: hasEvents = {hasEvents} (Count: {events?.Count ?? 0})");
                return hasEvents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CATEGORY] ❌ Error checking related events for category {categoryId}: {ex.Message}");
                return false;
            }
        }
    }
}
