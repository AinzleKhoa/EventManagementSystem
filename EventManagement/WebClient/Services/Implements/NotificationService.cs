using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebClient.Api; // Nơi chứa NotificationAPI URL
using WebClient.DTOs.Response;

namespace WebClient.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Dùng IHttpClientFactory giống như bạn đã làm ở OrderService
        public NotificationService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _http = factory.CreateClient("ApiClient");
            _http.BaseAddress = new Uri(config["Api:Url"]!);
            _httpContextAccessor = httpContextAccessor;
        }

        // Hàm tự động nhét Token vào để Backend mở cửa
        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<NotificationResponseDTO>> GetMyNotificationsAsync()
        {
            try
            {
                AttachBearerToken();
                var response = await _http.GetFromJsonAsync<List<NotificationResponseDTO>>(NotificationAPI.NotificationBase);
                return response ?? new List<NotificationResponseDTO>();
            }
            catch
            {
                return new List<NotificationResponseDTO>();
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                AttachBearerToken();
                var response = await _http.GetFromJsonAsync<UnreadCountResponseDTO>(NotificationAPI.UnreadCount);
                return response?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                AttachBearerToken();
                var url = NotificationAPI.MarkAsRead.Replace("{id}", notificationId.ToString());
                var response = await _http.PutAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                AttachBearerToken();
                var response = await _http.PutAsync(NotificationAPI.MarkAllAsRead, null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                AttachBearerToken();
                var url = $"{NotificationAPI.NotificationBase}/{notificationId}";
                var response = await _http.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> DeleteAllNotificationsAsync()
        {
            try
            {
                AttachBearerToken();
                var url = $"{NotificationAPI.NotificationBase}/all";
                var response = await _http.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}