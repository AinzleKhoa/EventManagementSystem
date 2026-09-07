using System.Text.Json;
using System.Text;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Api;
using WebClient.Helper;

namespace WebClient.Services.Implements
{
    public class EventService : IEventService
    {
        private readonly HttpClient _http;

        public EventService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient("ApiClient");
            var apiUrl = config["Api:Url"];
            _http.BaseAddress = new Uri(apiUrl);
        }

        public async Task Add(CreateEventDTO model)
        {
            await _http.PostAsJsonAsync(EventAPI.Base, model);
        }

        public async Task<List<ReadEventDTO>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<ReadEventDTO>>(EventAPI.Base);
        }

        public async Task<ReadEventDTO?> GetById(int id)
        {
            try
            {
                var result = await _http.GetFromJsonAsync<ReadEventDTO>(EventAPI.ById.WithId(id));
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"EventAPI request error for event {id}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading event {id}: {ex.Message}");
                throw;
            }
        }

        public async Task Update(int id, UpdateEventDTO model)
        {
            await _http.PutAsJsonAsync(EventAPI.ById.WithId(id), model);
        }

        public async Task<List<ReadEventDTO>> Search(string? query, List<int>? categoryIds, string? dateFilter, decimal? minPrice, decimal? maxPrice, bool freeOnly)
        {
            var url = $"{EventAPI.Base}/search?query={query}&dateFilter={dateFilter}&minPrice={minPrice}&maxPrice={maxPrice}&freeOnly={freeOnly}";

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var id in categoryIds)
                {
                    url += $"&categoryIds={id}";
                }
            }

            var result = await _http.GetFromJsonAsync<List<ReadEventDTO>>(url);
            return result ?? new List<ReadEventDTO>();
        }

        public async Task<List<ReadEventDTO>> GetAll(int? organizerId = null)
        {
            string url = EventAPI.Base;
            if (organizerId.HasValue)
            {
                url += $"?organizerId={organizerId}";
            }
            return await _http.GetFromJsonAsync<List<ReadEventDTO>>(url);
        }

        public async Task Delete(int id)
        {
            await _http.DeleteAsync(EventAPI.ById.WithId(id));
        }

        public async Task Submit(int id)
        {
            await _http.PostAsync(EventAPI.Submit.WithId(id), null);
        }

        public async Task Hide(int id)
        {
            await _http.PostAsync(EventAPI.Hide.WithId(id), null);
        }

        public async Task Resume(int id)
        {
            await _http.PostAsync(EventAPI.Resume.WithId(id), null);
        }
        public async Task<List<ReadEventDTO>> GetPendingEvents()
        {
            // Gọi API lấy danh sách pending (đã tạo ở EventAPI trước đó)
            return await _http.GetFromJsonAsync<List<ReadEventDTO>>($"{EventAPI.Base}/pending");
        }

        public async Task Approve(int id)
        {
            await _http.PostAsync($"event-gateway/events/{id}/approve", null);
        }

        public async Task Reject(int id, string reason)
        {
            // Gửi reason dưới dạng JSON body
            var content = new { Reason = reason };
            await _http.PostAsJsonAsync($"event-gateway/events/{id}/reject", content);
        }
        
        public async Task<ReadEventDTO?> GetEventByOrganizerId(int id)
        {
            try
            {
                
                var url = EventAPI.ByOrganizerId.Replace("{id}", id.ToString());
                return await _http.GetFromJsonAsync<ReadEventDTO>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading event by organizer {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<long> GetTotalEventsCount()
        {
            try
            {
                var count = await _http.GetFromJsonAsync<long>(EventAPI.TotalCount);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting total events count: {ex.Message}");
                return 0;
            }
        }

        public async Task<long> GetTotalPublicEventsCount()
        {
            try
            {
                var count = await _http.GetFromJsonAsync<long>(EventAPI.ApprovedActiveCount);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting approved active events count: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<ReadEventDTO>> GetTopSellingEvents(int count = 5)
        {
            try
            {
                var url = EventAPI.TopSelling.Replace("{count}", count.ToString());

                var result = await _http.GetFromJsonAsync<List<ReadEventDTO>>(url);
                return result ?? new List<ReadEventDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading top selling events: {ex.Message}");
                return new List<ReadEventDTO>();
            }
        }
    }
}
