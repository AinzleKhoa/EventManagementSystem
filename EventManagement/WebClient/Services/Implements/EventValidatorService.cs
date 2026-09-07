using System.Text.Json;
using WebClient.DTOs.Response;

namespace WebClient.Services.Implements
{
    public class EventValidatorService : IEventValidatorService
    {
        private readonly HttpClient _http;

        public EventValidatorService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
        }

        public async Task<EventValidatorDTO> AddValidatorAsync(int eventId, string validatorEmail)
        {
            var request = new { eventId, validatorEmail };

            // Log the request
            var json = System.Text.Json.JsonSerializer.Serialize(request);
            Console.WriteLine($"Sending request to event-gateway/eventvalidators/add: {json}");

            var response = await _http.PostAsJsonAsync("event-gateway/eventvalidators/add", request);

            Console.WriteLine($"Response status: {response.StatusCode}");
            var responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseJson}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode} - {responseJson}");

                // Extract error message with better handling
                string errorMessage = "Lỗi khi thêm validator. Vui lòng thử lại.";
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    using (var doc = JsonDocument.Parse(responseJson))
                    {
                        if (doc.RootElement.TryGetProperty("message", out var messageProp))
                        {
                            var msg = messageProp.GetString();
                            // Nếu lỗi từ API, sử dụng trực tiếp
                            if (!string.IsNullOrEmpty(msg))
                            {
                                errorMessage = msg;
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Nếu response không phải JSON, sử dụng nội dung trực tiếp
                    if (!string.IsNullOrEmpty(responseJson))
                    {
                        errorMessage = responseJson;
                    }
                }

                throw new InvalidOperationException(errorMessage);
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using (var doc = JsonDocument.Parse(responseJson))
            {
                if (doc.RootElement.TryGetProperty("data", out var dataProp))
                {
                    var result = JsonSerializer.Deserialize<EventValidatorDTO>(dataProp.GetRawText(), jsonOptions);
                    Console.WriteLine($"Deserialized result: {result?.StaffEmail}");
                    return result;
                }
            }

            // Fallback: try to deserialize directly
            var directResult = JsonSerializer.Deserialize<EventValidatorDTO>(responseJson, jsonOptions);
            Console.WriteLine($"Deserialized result (direct): {directResult?.StaffEmail}");
            return directResult;
        }

        public async Task<List<EventValidatorDTO>> GetValidatorsByEventIdAsync(int eventId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<EventValidatorDTO>>($"event-gateway/eventvalidators/{eventId}") 
                    ?? new List<EventValidatorDTO>();
            }
            catch
            {
                return new List<EventValidatorDTO>();
            }
        }

        public async Task<bool> RemoveValidatorAsync(int eventStaffId)
        {
            try
            {
                var response = await _http.DeleteAsync($"event-gateway/eventvalidators/{eventStaffId}");
                var responseJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Remove validator response status: {response.StatusCode}");
                Console.WriteLine($"Remove validator response content: {responseJson}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode} - {responseJson}");
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        using (var doc = JsonDocument.Parse(responseJson))
                        {
                            if (doc.RootElement.TryGetProperty("message", out var messageProp))
                            {
                                var msg = messageProp.GetString();
                                throw new InvalidOperationException(msg ?? "Không thể xóa validator");
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        throw new InvalidOperationException(responseJson);
                    }
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Remove validator error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReadEventDTO>> GetEventsByValidatorEmailAsync(string validatorEmail)
        {
            try
            {
                var response = await _http.GetAsync($"event-gateway/eventvalidators/by-validator/{validatorEmail}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GetEventsByValidatorEmail error: {response.StatusCode}");
                    return new List<ReadEventDTO>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<List<ReadEventDTO>>(responseJson, options);

                return result ?? new List<ReadEventDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetEventsByValidatorEmail error: {ex.Message}");
                return new List<ReadEventDTO>();
            }
        }

        public async Task<bool> IsValidatorOfEventAsync(int eventId, string validatorEmail)
        {
            try
            {
                var response = await _http.GetAsync($"event-gateway/eventvalidators/check/{eventId}/{validatorEmail}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"IsValidatorOfEvent error: {response.StatusCode}");
                    return false;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using (var doc = JsonDocument.Parse(responseJson))
                {
                    if (doc.RootElement.TryGetProperty("data", out var dataProp))
                    {
                        return dataProp.GetBoolean();
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsValidatorOfEvent error: {ex.Message}");
                return false;
            }
        }
    }
}
