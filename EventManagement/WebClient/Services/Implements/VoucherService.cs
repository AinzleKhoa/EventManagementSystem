using System.Text.Json;
using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class VoucherService : IVoucherService
    {
        private readonly HttpClient _http;

        public VoucherService(HttpClient http)
        {
            _http = http;
        }
        private async Task HandleErrorResponse(HttpResponseMessage response, string defaultMessage)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {errorContent}");

                try
                {
                    var errorObj = JsonSerializer.Deserialize<ErrorResponse>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    );
                    throw new Exception(errorObj?.Message ?? defaultMessage);
                }
                catch (JsonException)
                {
                    throw new Exception($"API returned {response.StatusCode}: {errorContent}");
                }
            }
        }

        public async Task Add(CreateVoucherDTO model)
        {
            var response = await _http.PostAsJsonAsync(VoucherAPI.Base, model);
            await HandleErrorResponse(response, "Failed to create voucher.");
        }

        public async Task Update(int id, UpdateVoucherDTO model)
        {
            var response = await _http.PutAsJsonAsync(VoucherAPI.ById.WithId(id), model);
            await HandleErrorResponse(response, "Failed to update voucher.");
        }

        public async Task Delete(int id)
        {
            var response = await _http.DeleteAsync(VoucherAPI.ById.WithId(id));
            await HandleErrorResponse(response, "Failed to delete voucher.");
        }

        public async Task<List<ReadVoucherDTO>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<ReadVoucherDTO>>(VoucherAPI.Base);
        }

        public async Task<ReadVoucherDTO> GetById(int id)
        {
            return await _http.GetFromJsonAsync<ReadVoucherDTO>(VoucherAPI.ById.WithId(id));
        }
        public async Task<PaginatedResultDTO<ReadVoucherDTO>> SearchAndFilter(VoucherSearchFilterDTO filter)
        {

            var queryParams = new List<string>
    {
        $"pageNumber={filter.PageNumber}",
        $"pageSize={filter.PageSize}"
    };

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");

            if (filter.IsActive.HasValue)
                queryParams.Add($"isActive={filter.IsActive.Value}");

            if (!string.IsNullOrEmpty(filter.SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");

            if (!string.IsNullOrEmpty(filter.SortOrder))
                queryParams.Add($"sortOrder={Uri.EscapeDataString(filter.SortOrder)}");



            var queryString = "?" + string.Join("&", queryParams);


            var response = await _http.GetAsync(VoucherAPI.Base + "/search" + queryString);

            if (!response.IsSuccessStatusCode)
            {
    
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi gọi API ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<PaginatedResultDTO<ReadVoucherDTO>>()
                   ?? new PaginatedResultDTO<ReadVoucherDTO>();
        }

        public async Task DistributeByEmail(DistributeByEmailRequestDTO dto)
        {
            var response = await _http.PostAsJsonAsync(VoucherAPI.DistributeByEmail, dto);
            await HandleErrorResponse(response, "Failed to distribute voucher by email.");
        }

        public async Task DistributeToEvent(DistributeToEventRequestDTO dto)
        {
            var response = await _http.PostAsJsonAsync(VoucherAPI.DistributeToEvent, dto);
            await HandleErrorResponse(response, "Failed to distribute voucher to event attendees.");
        }
    }
}
