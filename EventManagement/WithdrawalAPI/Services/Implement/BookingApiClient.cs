using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WithdrawalAPI.Services.Implement
{
    public class BookingApiClient : IBookingApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BookingApiClient> _logger;

        public BookingApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<BookingApiClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            var bookingApiUrl = configuration["BookingAPI:Url"] ?? "https://localhost:7108";
            _httpClient.BaseAddress = new Uri(bookingApiUrl);
        }

        public async Task<List<BookingOrderDTO>> GetOrdersByEventIdAsync(int eventId)
        {
            try
            {
                _logger.LogInformation($"[WITHDRAWAL] Calling BookingAPI to get orders for EventId: {eventId}");
                
                var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Orders/event/{eventId}");
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    request.Headers.Add("Authorization", authorizationHeader);
                }

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[WITHDRAWAL] ❌ Failed to get orders for event {eventId} from BookingAPI. Status: {response.StatusCode}");
                    return new List<BookingOrderDTO>();
                }

                var orders = await response.Content.ReadFromJsonAsync<List<BookingOrderDTO>>();
                return orders ?? new List<BookingOrderDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[WITHDRAWAL] ❌ Exception when calling BookingAPI for EventId: {eventId}. Error: {ex.Message}");
                return new List<BookingOrderDTO>();
            }
        }
    }
}
