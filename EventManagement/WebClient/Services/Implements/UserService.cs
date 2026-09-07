using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;
using WebClient.Services;

namespace WebClient.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly HttpClient _http;

        public UserService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
        }

        public async Task<List<ReadUserDTO>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<ReadUserDTO>>(AuthenticationAPI.User.Base) ?? new List<ReadUserDTO>();
        }

        public async Task<ReadUserDTO> GetById(int id)
        {
            var response = await _http.GetAsync(AuthenticationAPI.User.ById.WithId(id));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("User not found or API error.");
            }
            return await response.Content.ReadFromJsonAsync<ReadUserDTO>();
        }

        public async Task Create(CreateUserDTO model)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.FullName ?? ""), nameof(model.FullName));
            content.Add(new StringContent(model.Email), nameof(model.Email));
            content.Add(new StringContent(model.Password), nameof(model.Password));
            content.Add(new StringContent(model.ConfirmPassword), nameof(model.ConfirmPassword));
            content.Add(new StringContent(model.RoleId.ToString()), nameof(model.RoleId));

            if (!string.IsNullOrEmpty(model.PhoneNumber))
                content.Add(new StringContent(model.PhoneNumber), nameof(model.PhoneNumber));

            if (model.AvatarFile != null)
            {
                var fileContent = new StreamContent(model.AvatarFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.AvatarFile.ContentType);
                content.Add(fileContent, nameof(model.AvatarFile), model.AvatarFile.FileName);
            }

            var response = await _http.PostAsync(AuthenticationAPI.User.Base, content);
            await HandleResponseError(response);
        }

        public async Task Update(int id, UpdateUserDTO model)
        {
            using var content = new MultipartFormDataContent();

            if (model.FullName != null)
                content.Add(new StringContent(model.FullName), nameof(model.FullName));

            if (model.PhoneNumber != null)
                content.Add(new StringContent(model.PhoneNumber), nameof(model.PhoneNumber));

            if (model.RoleId.HasValue)
                content.Add(new StringContent(model.RoleId.Value.ToString()), nameof(model.RoleId));

            if (model.IsActive.HasValue)
                content.Add(new StringContent(model.IsActive.Value.ToString().ToLower()), nameof(model.IsActive));

            if (model.AvatarFile != null)
            {
                var fileContent = new StreamContent(model.AvatarFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.AvatarFile.ContentType);
                content.Add(fileContent, nameof(model.AvatarFile), model.AvatarFile.FileName);
            }

            var response = await _http.PutAsync(AuthenticationAPI.User.ById.WithId(id), content);
            await HandleResponseError(response);
        }

        public async Task<bool> IsExist(int id)
        {
            // BE trả về: return Ok(new { id, exists });
            var response = await _http.GetAsync($"{AuthenticationAPI.User.Base}/exists/{id}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserExistResponse>();
                return result?.Exists ?? false;
            }
            return false;
        }

        public async Task<PaginatedResultDTO<ReadUserDTO>> SearchAndFilter(UserSearchFilterDTO filter)
        {
            try
            {
                var queryParams = new Dictionary<string, string?>
                {
                    ["pageNumber"] = filter.PageNumber.ToString(),
                    ["pageSize"] = filter.PageSize.ToString(),
                    ["sortBy"] = filter.SortBy,
                    ["sortOrder"] = filter.SortOrder,
                    ["searchTerm"] = filter.SearchTerm,
                    ["isActive"] = filter.IsActive?.ToString().ToLower(),
                    ["roleId"] = (filter.RoleId >= 2 && filter.RoleId <= 4) ? filter.RoleId.ToString() : null
                };

                var cleanParams = queryParams.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value!);
                var url = QueryHelpers.AddQueryString($"{AuthenticationAPI.User.Base}/search", cleanParams);

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponseError(response);
                }

                return await response.Content.ReadFromJsonAsync<PaginatedResultDTO<ReadUserDTO>>()
                       ?? new PaginatedResultDTO<ReadUserDTO>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Cannot connect to the server. Please check your connection.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Helper method để tái sử dụng logic bắt lỗi
        private async Task HandleResponseError(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorRaw = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(errorRaw);
                    if (doc.RootElement.TryGetProperty("message", out var msgProp) && !string.IsNullOrEmpty(msgProp.GetString()))
                    {
                        throw new Exception(msgProp.GetString());
                    }
                }
                catch (JsonException) { }

                throw new Exception($"API Error: {response.StatusCode} - {errorRaw}");
            }
        }

        public async Task LockAndUnlockAccount(int id)
        {
            try
            {
                var url = AuthenticationAPI.User.LockAndUnlock.WithId(id);
                var response = await _http.PutAsync(url, null);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleResponseError(response);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<long> GetTotalCustomers()
        {
            try
            {
                var url = $"{AuthenticationAPI.UsersOData.Base}?$count=true&$filter=role/roleId eq 2";
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(jsonString);

                    if (doc.RootElement.TryGetProperty("@odata.count", out var countProperty))
                    {
                        long count = countProperty.GetInt64();
                        Console.WriteLine($"Thành công! Count: {count}");
                        return count;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard Debug Error: {ex.Message}");
                return 0;
            }
        }
    }
}