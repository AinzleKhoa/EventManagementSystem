using System.Text.Json;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task<string> ExtractError(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return $"HTTP Error: {(int)response.StatusCode}";

            try
            {
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    return msg.GetString() ?? $"HTTP Error: {(int)response.StatusCode}";

                if (root.TryGetProperty("Message", out var msgUpper) && msgUpper.ValueKind == JsonValueKind.String)
                    return msgUpper.GetString() ?? $"HTTP Error: {(int)response.StatusCode}";

                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString() ?? $"HTTP Error: {(int)response.StatusCode}";
            }
            catch { }

            return $"An unexpected error occurred. (Status Code: {(int)response.StatusCode})";
        }

        public async Task<(AuthResponseDTO? Data, string? Error)> Login(LoginDTO loginDTO)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.Login, loginDTO);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
                    return (result, null);
                }

                return (null, await ExtractError(response));
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public async Task<(AuthResponseDTO? Data, string? Error)> GoogleLogin(string idToken)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.GoogleLogin, new { idToken });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
                    return (result, null);
                }

                return (null, await ExtractError(response));
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public async Task<ReadUserDTO?> GetUserById(int id)
        {
            try
            {
                var url = AuthenticationAPI.User.ById.Replace("{id}", id.ToString());
                return await _http.GetFromJsonAsync<ReadUserDTO>(url);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> Register(RegisterDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.Register, dto);

                if (response.IsSuccessStatusCode)
                    return null;

                return await ExtractError(response);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string?> ForgotPassword(ForgotPasswordDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.ForgotPassword, dto);

                if (response.IsSuccessStatusCode)
                    return null;

                return await ExtractError(response);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string?> ResetPassword(ResetPasswordDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.ResetPassword, dto);

                if (response.IsSuccessStatusCode)
                    return null;

                return await ExtractError(response);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<(AuthResponseDTO? Data, string? Error)> RefreshToken(string refreshToken)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(AuthenticationAPI.RefreshToken, new { refreshToken });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
                    return (result, null);
                }

                return (null, await ExtractError(response));
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}