using ChatbotAPI.DTOs;
using ChatbotAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatbotAPI.Services.Implements
{
    public class EventService : IEventService
    {
        private readonly HttpClient _httpClient;

        public EventService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayAPI");
        }

        public async Task<string> GetAllEventsForAI()
        {
            try
            {
                string url = "event-gateway/Events";
                Console.WriteLine($"[Event API] {url}");

                var events = await _httpClient.GetFromJsonAsync<List<ReadEventDTO>>(url);
                return FormatEventsForAI(events);
            }
            catch
            {
                return "SYSTEM_MESSAGE: Lỗi kết nối đến cơ sở dữ liệu sự kiện. Hãy báo cho người dùng biết hệ thống đang bảo trì.";
            }
        }

        private string FormatEventsForAI(List<ReadEventDTO> events)
        {
            var approvedEvents = events?.Where(e => "Approved".Equals(e.Status, StringComparison.OrdinalIgnoreCase)).ToList();

            if (approvedEvents == null || !approvedEvents.Any())
                return "SYSTEM_MESSAGE: [START_DATA] Danh sách sự kiện hiện tại đang RỖNG (0 sự kiện). [END_DATA]";

            var sb = new StringBuilder();
            sb.AppendLine("[START_DATA] Dữ liệu sự kiện thực tế từ hệ thống:");

            foreach (var e in approvedEvents)
            {
                string priceInfo = "Chưa cập nhật";
                if (e.TicketTypes != null && e.TicketTypes.Any())
                {
                    var prices = e.TicketTypes.Select(t => t.Price).ToList();
                    if (prices.Any())
                    {
                        long min = prices.Min();
                        long max = prices.Max();
                        priceInfo = min == max ? $"{min:N0} đ" : $"{min:N0} - {max:N0} đ";
                    }
                }

                sb.AppendLine($"- ID: {e.EventId}");
                sb.AppendLine($"- Tên: {e.Title}");
                sb.AppendLine($"  Địa điểm: {e.Location}");
                sb.AppendLine($"  Thời gian: {e.StartDate:dd/MM/yyyy HH:mm}");
                sb.AppendLine($"  Giá vé: {priceInfo}");
                sb.AppendLine();
            }

            sb.AppendLine("[END_DATA]");
            return sb.ToString();
        }
    }
}