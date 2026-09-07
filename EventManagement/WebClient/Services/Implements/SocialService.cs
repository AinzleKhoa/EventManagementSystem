using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class SocialService : ISocialService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SocialService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
            _httpContextAccessor = httpContextAccessor;
        }

        private void AttachBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // --- REVIEW SECTIONS ---

        public async Task<List<ReadReviewDTO>> GetReviewsByEventId(int eventId, string odataQuery = "")
        {
            var url = SocialAPI.ReviewByEvent.Replace("{id}", eventId.ToString());

            if (!string.IsNullOrWhiteSpace(odataQuery))
            {
                // 🔥 THÊM DÒNG NÀY: Mã hóa khoảng trắng thành %20 để Gateway không bị nghẹn
                odataQuery = odataQuery.Replace(" ", "%20");

                url += odataQuery.StartsWith("?") ? odataQuery : $"?{odataQuery}";
            }

            var result = await _http.GetFromJsonAsync<List<ReadReviewDTO>>(url);
            return result ?? new List<ReadReviewDTO>();
        }

        public async Task CreateReview(CreateReviewDTO model)
        {
            AttachBearerToken();
            var response = await _http.PostAsJsonAsync(SocialAPI.ReviewBase, model);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await ExtractErrorMessage(response);
                throw new Exception(errorMessage);
            }
        }

        // 2. SỬA HÀM UPDATE REVIEW
        public async Task<bool> UpdateReview(int reviewId, UpdateReviewDTO model)
        {
            AttachBearerToken();
            var url = SocialAPI.ReviewUpdate.Replace("{id}", reviewId.ToString());

            var response = await _http.PutAsJsonAsync(url, model);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await ExtractErrorMessage(response);
                throw new Exception(errorMessage);
            }
            return true;
        }

        public async Task<bool> DeleteReview(int reviewId)
        {

            AttachBearerToken();
            var url = SocialAPI.ReviewDelete.Replace("{id}", reviewId.ToString());

            var response = await _http.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<double> GetEventRating(int eventId)
        {
            try
            {
                var url = SocialAPI.ReviewRating.Replace("{id}", eventId.ToString());

                var response = await _http.GetFromJsonAsync<System.Text.Json.JsonElement>(url);

                if (response.TryGetProperty("averageRating", out var rating))
                {
                    return rating.GetDouble();
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // --- WISHLIST SECTIONS ---
        public async Task<List<ReadWishlistDTO>> GetMyWishlist(string odataQuery = "")
        {
            AttachBearerToken();
            var url = SocialAPI.WishlistMine;

            if (!string.IsNullOrWhiteSpace(odataQuery))
            {
                odataQuery = odataQuery.Replace(" ", "%20");

                url += odataQuery.StartsWith("?") ? odataQuery : $"?{odataQuery}";
            }

            var result = await _http.GetFromJsonAsync<List<ReadWishlistDTO>>(url);
            return result ?? new List<ReadWishlistDTO>();
        }

        public async Task AddToWishlist(CreateWishlistDTO model)
        {
            AttachBearerToken();
            var response = await _http.PostAsJsonAsync(SocialAPI.WishlistBase, model);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await ExtractErrorMessage(response);
                throw new Exception(errorMessage);
            }
        }

        public async Task RemoveFromWishlist(int eventId)
        {
            AttachBearerToken();
            var url = SocialAPI.WishlistRemove.Replace("{id}", eventId.ToString());
            var response = await _http.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await ExtractErrorMessage(response);
                throw new Exception(errorMessage);
            }
        }

        private async Task<string> ExtractErrorMessage(HttpResponseMessage response)
        {
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<JsonElement>();

                if (errorResult.TryGetProperty("errors", out var errors))
                {
                    var firstErrorProperty = errors.EnumerateObject().FirstOrDefault();
                    if (firstErrorProperty.Value.ValueKind == JsonValueKind.Array)
                    {
                        return firstErrorProperty.Value[0].GetString() ?? "Invalid data.";
                    }
                }

                // Trường hợp 2: Lỗi BadRequest bình thường tự ném ra (có chứa chữ "message")
                if (errorResult.TryGetProperty("message", out var msg))
                {
                    return msg.GetString() ?? "Processing error.";
                }
            }
            catch
            {
                // Lỡ không ép kiểu được JSON thì thôi, báo lỗi chung chung
            }

            return "A server error occurred while processing your request.";
        }
    }
}
