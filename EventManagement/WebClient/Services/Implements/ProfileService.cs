using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services.Implements
{
    public class ProfileService : IProfileService
    {
        private readonly HttpClient _http;

        public ProfileService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
        }

        public async Task<ProfileDTO?> GetProfile()
        {
            return await _http.GetFromJsonAsync<ProfileDTO>(AuthenticationAPI.Profile.Base);
        }

        public async Task<ProfileResponseDTO?> UpdateProfile(UpdateProfileDTO dto)
        {
            var response = await _http.PutAsJsonAsync(AuthenticationAPI.Profile.Base, dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProfileResponseDTO>();
            }
            return null;
        }

        public async Task<string?> ChangePassword(ChangePasswordDTO dto)
        {
            var response = await _http.PutAsJsonAsync(AuthenticationAPI.Profile.ChangePassword, dto);

            if (response.IsSuccessStatusCode) return null;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return "FORCE_LOGOUT";

            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return error?.Message ?? "Change password failed.";
        }

        public async Task<ProfileResponseDTO?> UploadAvatar(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(file.OpenReadStream());
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileStreamContent, "file", file.FileName);

            var response = await _http.PostAsync(AuthenticationAPI.Profile.Avatar, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProfileResponseDTO>();
            }
            return null;
        }

        public async Task<bool> IsGoogleAccount()
        {
            return await _http.GetFromJsonAsync<bool>(AuthenticationAPI.Profile.IsGoogleAccount);
        }

        public async Task<bool> Logout()
        {
            try
            {
                var response = await _http.PostAsync(AuthenticationAPI.Profile.Logout, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gọi API Logout: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestBecomeOrganizer()
        {
            try
            {
                // Gọi API Backend: POST /api/profile/become-organizer/request
                var response = await _http.PostAsync(AuthenticationAPI.Profile.RequestBecomeOrganizer, null);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LỖI API ORGANIZER] Status Code: {(int)response.StatusCode} - Chi tiết: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Lỗi khi gọi API Organizer: {ex.Message}");
                return false;
            }
        }

        public async Task<ProfileResponseDTO?> VerifyBecomeOrganizer(VerifyOrganizerOtpDTO dto)
        {
            try
            {
                // Gọi API Backend: POST /api/profile/become-organizer/verify
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.Profile.VerifyBecomeOrganizer, dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProfileResponseDTO>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xác nhận mã OTP Organizer: {ex.Message}");
                return null;
            }
        }
    }
}