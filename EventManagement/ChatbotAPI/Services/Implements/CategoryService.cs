using ChatbotAPI.Services;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatbotAPI.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        
        public CategoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayAPI");
        }

        public async Task<string> GetCategoriesForAI()
        {
            try
            {
                var response = await _httpClient.GetAsync("category-gateway/categories");
                var json = await response.Content.ReadAsStringAsync();

                var formattedData = FormatCategoriesForAI(json);
                return $"[START_DATA]\n{formattedData}\n[END_DATA]";
            }
            catch
            {
                return "Hiện chưa có dữ liệu thể loại.";
            }
        }

        private string FormatCategoriesForAI(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var sb = new StringBuilder();
                sb.AppendLine("Danh sách thể loại:");
                foreach (var c in doc.RootElement.EnumerateArray())
                {
                    var id = c.GetProperty("categoryId").GetInt32();
                    var name = c.GetProperty("categoryName").GetString();
                    sb.AppendLine($"ID: {id} - {name}");
                }
                return sb.ToString();
            }
            catch
            {
                return "Không đọc được danh sách thể loại.";
            }
        }
    }
}