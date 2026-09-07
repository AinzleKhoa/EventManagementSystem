using System.Net.Http.Headers;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Models;

namespace WebClient.Services.Implements
{
    public class TicketApiClient : ITicketApiClient
    {
        private readonly HttpClient _http;

        // Tiêm IHttpClientFactory và IConfiguration giống hệt OrderService
        public TicketApiClient(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl); // Lấy BaseAddress (chính là link API Gateway)
        }

        public async Task<TicketCheckInResponseDTO> ScanTicketAsync(TicketCheckInRequestDTO request, string token)
        {
            // 1. Đính kèm Token mà JS truyền lên
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 2. Gọi API (LƯU Ý: Vì đã có BaseAddress, ta chỉ cần gọi đường dẫn TƯƠNG ĐỐI)
            // Không gọi full link "https://localhost:7158/..." nữa
            var response = await _http.PostAsJsonAsync("booking-gateway/Tickets/scan", request);

            // 3. Kiểm tra kết quả
            if (!response.IsSuccessStatusCode)
            {
                var rawError = await response.Content.ReadAsStringAsync();
                return new TicketCheckInResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Lỗi Gateway ({(int)response.StatusCode}): {rawError}"
                };
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<TicketCheckInResponseDTO>();
            }
            catch (Exception ex)
            {
                return new TicketCheckInResponseDTO { IsSuccess = false, Message = "Lỗi đọc JSON: " + ex.Message };
            }
        }
        public async Task<IEnumerable<ReadTicketDTO>> GetTicketsByEventIdAsync(int eventId)
        {
            // Thay thế {id} bằng giá trị thực tế
            var url = TicketAPI.ByEventId.Replace("{id}", eventId.ToString());

            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<ReadTicketDTO>>()
                       ?? new List<ReadTicketDTO>();
            }
            return new List<ReadTicketDTO>();
        }

        // Thay đổi kiểu trả về thành ReadTicketDTO vì OData đang trả về DTO này
        public async Task<TicketValidationDTO> GetTicketByCode(Guid code, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            // Thay thế {code} bằng giá trị thực tế của Guid
            var url = TicketAPI.ByCode.Replace("{code}", code.ToString());

            try
            {
                // Gọi API
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TicketValidationDTO>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                var rawError = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GetTicketByCode Error] {(int)response.StatusCode}: {rawError}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] Lỗi hệ thống khi gọi GetTicketByCode: {ex.Message}");
                return null;
            }
        }
    }
    }
