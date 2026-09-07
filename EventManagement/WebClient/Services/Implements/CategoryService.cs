using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _http;
        public CategoryService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
        }

        public async Task Add(CreateCategoryDTO model)
        {
            var response = await _http.PostAsJsonAsync(CategoryAPI.Base, model);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to create category.");
            }
        }

        public async Task Delete(int id)
        {
            var response = await _http.DeleteAsync(CategoryAPI.ById.WithId(id));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to delete category.");
            }
        }

        public async Task<List<ReadCategoryDTO>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<ReadCategoryDTO>>(CategoryAPI.Base);
        }

        public async Task<ReadCategoryDTO> GetById(int id)
        {
            return await _http.GetFromJsonAsync<ReadCategoryDTO>(CategoryAPI.ById.WithId(id));
        }

        public async Task Update(int id, UpdateCategoryDTO model)
        {
            var response = await _http.PutAsJsonAsync(CategoryAPI.ById.WithId(id), model);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to update category.");
            }
        }

        public async Task<PaginatedResultDTO<ReadCategoryDTO>> SearchAndFilter(CategorySearchFilterDTO filter)
        {
            var queryString = $"?searchTerm={filter.SearchTerm}&isActive={filter.IsActive}&pageNumber={filter.PageNumber}&pageSize={filter.PageSize}&sortBy={filter.SortBy}&sortOrder={filter.SortOrder}";
            return await _http.GetFromJsonAsync<PaginatedResultDTO<ReadCategoryDTO>>(CategoryAPI.Base + "/search" + queryString);
        }

        public async Task<PaginatedResultDTO<ReadUserDTO>> SearchAndFilter(UserSearchFilterDTO filter)
        {
            // Xây dựng query string tương tự như CategoryService
            var queryString = $"?searchTerm={filter.SearchTerm}&isActive={filter.IsActive}&pageNumber={filter.PageNumber}&pageSize={filter.PageSize}&sortBy={filter.SortBy}&sortOrder={filter.SortOrder}";

            // Giả định bạn có endpoint /search trong UsersController (giống Category)
            // Nếu bạn muốn dùng OData của Get(), logic sẽ cần thay đổi một chút
            return await _http.GetFromJsonAsync<PaginatedResultDTO<ReadUserDTO>>(AuthenticationAPI.User.Base + "/search" + queryString);
        }
    }
}
