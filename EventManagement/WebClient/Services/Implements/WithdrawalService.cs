using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly HttpClient _http;

        public WithdrawalService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            _http.BaseAddress = new Uri(config["Api:Url"]!);
        }

        public async Task<ReadWithdrawalDTO> Create(CreateWithdrawalDTO dto)
        {
            var response = await _http.PostAsJsonAsync(WithdrawalAPI.Base, dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to create withdrawal request.");
            }
            return (await response.Content.ReadFromJsonAsync<ReadWithdrawalDTO>())!;
        }

        public async Task<ReadWithdrawalDTO> Update(int id, UpdateWithdrawalDTO dto)
        {
            var response = await _http.PutAsJsonAsync(WithdrawalAPI.ById.WithId(id), dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to update withdrawal request.");
            }
            return (await response.Content.ReadFromJsonAsync<ReadWithdrawalDTO>())!;
        }

        public async Task<List<ReadWithdrawalDTO>> GetMine()
        {
            return (await _http.GetFromJsonAsync<List<ReadWithdrawalDTO>>(WithdrawalAPI.My))!;
        }

        public async Task<PaginatedResultDTO<ReadWithdrawalDTO>> SearchAndFilter(WithdrawalFilterDTO filter)
        {
            var qs = $"?searchTerm={filter.SearchTerm}&status={filter.Status}&pageNumber={filter.PageNumber}&pageSize={filter.PageSize}&sortBy={filter.SortBy}&sortOrder={filter.SortOrder}";
            return (await _http.GetFromJsonAsync<PaginatedResultDTO<ReadWithdrawalDTO>>(WithdrawalAPI.Base + qs))!;
        }

        public async Task<ReadWithdrawalDTO> GetById(int id)
        {
            return (await _http.GetFromJsonAsync<ReadWithdrawalDTO>(WithdrawalAPI.ById.WithId(id)))!;
        }

        public async Task<ReadWithdrawalDTO> Approve(int id)
        {
            var response = await _http.PatchAsJsonAsync(WithdrawalAPI.Approve.WithId(id), new { });
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to approve withdrawal request.");
            }
            return (await response.Content.ReadFromJsonAsync<ReadWithdrawalDTO>())!;
        }

        public async Task<ReadWithdrawalDTO> Reject(int id, RejectReasonDTO dto)
        {
            var response = await _http.PatchAsJsonAsync(WithdrawalAPI.Reject.WithId(id), dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                throw new Exception(error?.Message ?? "Failed to reject withdrawal request.");
            }
            return (await response.Content.ReadFromJsonAsync<ReadWithdrawalDTO>())!;
        }
    }
}
