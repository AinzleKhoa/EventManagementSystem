using WebClient.Api;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly HttpClient _http;

        public TicketTypeService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            _http.BaseAddress = new Uri(config["Api:Url"]);
        }

        public async Task<ReadTicketTypeDTO> Create(CreateTicketTypeDTO model)
        {
            var response = await _http.PostAsJsonAsync(TicketTypeAPI.Base, model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ReadTicketTypeDTO>();
        }

        public async Task Update(int id, UpdateTicketTypeDTO model)
        {
            await _http.PutAsJsonAsync(TicketTypeAPI.ById.WithId(id), model);
        }

        public async Task Delete(int id)
        {
            await _http.DeleteAsync(TicketTypeAPI.ById.WithId(id));
        }
    }
}