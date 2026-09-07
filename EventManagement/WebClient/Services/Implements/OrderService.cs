using System.Net;
using System.Net.Http.Headers;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor; // Thêm biến này để móc Token
        private readonly ILogger<OrderService> _logger;

        // Bổ sung IHttpContextAccessor vào Constructor
        public OrderService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor httpContextAccessor, ILogger<OrderService> logger)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task<HttpResponseMessage> Add(CreateOrderDTO model)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _http.PostAsJsonAsync(BookingAPI.OrderBase, model);
        }
        public async Task<ReadOrderDTO> GetById(int id)
        {
            return await _http.GetFromJsonAsync<ReadOrderDTO>(BookingAPI.OrderById.WithId(id));
        }

        public async Task<List<ReadOrderDTO>> GetOrderByCustomerId(int id)
        {
            return await _http.GetFromJsonAsync<List<ReadOrderDTO>>(
                BookingAPI.OrderBase + "?$filter=CustomerId eq " + id + "&$orderby=OrderDate desc"
            );
        }

        public async Task<long> GetTotalRevenueByEventId(int eventId)
        {
            var orders = await _http.GetFromJsonAsync<List<ReadOrderDTO>>(
                $"{BookingAPI.OrderBase}?$filter=EventId eq {eventId} and Status eq 'Paid'");
            return orders?.Sum(o => o.FinalAmount) ?? 0;
        }

        public async Task<bool> CheckUserHasPaidTicket(int eventId)
        {
            try
            {
                // 1. Lấy Token từ trình duyệt của User
                var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    // 2. Nhét Token vào Header để Backend OrderAPI cho phép đi qua
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // 3. Gọi API: "gateway/orders/check-paid-ticket/{eventId}"
                var url = $"{BookingAPI.OrderBase}/check-paid-ticket/{eventId}";
                var httpResponse = await _http.GetAsync(url);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var body = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Check paid ticket failed for Event {EventId}. Status={StatusCode}, Body={Body}",
                        eventId,
                        httpResponse.StatusCode,
                        body);
                    return false;
                }

                var response = await httpResponse.Content.ReadFromJsonAsync<CheckTicketResponseDTO>();

                return response?.HasPaid ?? false;
            }
            catch (Exception ex)
            {
                // Lỗi mạng hoặc Token hết hạn => Coi như chưa mua vé
                _logger.LogWarning(ex, "Check paid ticket threw an exception for Event {EventId}.", eventId);
                return false;
            }
        }
        public async Task<List<ReadOrderDTO>> GetOrdersByEventIdAsync(int eventId)
        {

            string url = BookingAPI.OrdersByEventId.Replace("{eventId}", eventId.ToString());

            var response = await _http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ReadOrderDTO>>();
            }
            return new List<ReadOrderDTO>();
        }
    }
}
