using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatbotAPI.DTOs;
using Microsoft.Extensions.Configuration;
using ChatbotAPI.Services;

public class GroqService : IGroqService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly string _systemPrompt;
    private const string ModelName = "llama-3.3-70b-versatile";

    public GroqService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IEventService eventService, ICategoryService categoryService)
    {
        _httpClient = httpClientFactory.CreateClient("GroqClient");
        _configuration = configuration;
        _eventService = eventService;
        _categoryService = categoryService;
        var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "system_prompt.txt");
        _systemPrompt = File.ReadAllText(path);
    }

    // =========================
    // CHAT (CẬP NHẬT SYSTEM PROMPT & TOOL SCHEMA)
    // =========================
    public async Task<string> SendMessageAsync(List<ChatMessageDTO> chatHistory)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = _systemPrompt }
            };

            foreach (var msg in chatHistory)
            {
                messages.Add(new
                {
                    role = msg.Role == "model" ? "assistant" : "user",
                    content = msg.Text
                });
            }

            var tools = new object[]
            {
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "get_all_events",
                        description = "Lấy toàn bộ danh sách sự kiện hiện có trong hệ thống để phân tích.",
                        parameters = new { type = "object", properties = new { } } // Không cần tham số
                    }
                },
                new
                {
                    type = "function",
                    function = new
                    {
                        name = "get_categories",
                        description = "Tra cứu danh sách thể loại sự kiện (Music, Workshop, Art...) để lấy ID.",
                        parameters = new
                        {
                            type = "object",
                            properties = new { }
                        }
                    }
                }
            };

            var request = new
            {
                model = ModelName,
                messages = messages.ToArray(),
                tools = tools,
                tool_choice = "auto",
                temperature = 0.1 // Hạ nhiệt độ xuống thấp nhất để AI không bay bổng
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("========== GROQ RESPONSE 1 ==========");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine(json);
            Console.WriteLine("=====================================");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Groq API Error 1]: {json}");
                return "Lỗi kết nối AI (Lần 1).";
            }

            using var doc = JsonDocument.Parse(json);
            var messagePart = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

            if (!messagePart.TryGetProperty("tool_calls", out var toolCalls))
                return messagePart.GetProperty("content").GetString();

            var parsedToolCalls = new List<object>();
            foreach (var tc in toolCalls.EnumerateArray())
            {
                parsedToolCalls.Add(new
                {
                    id = tc.GetProperty("id").GetString(),
                    type = "function",
                    function = new
                    {
                        name = tc.GetProperty("function").GetProperty("name").GetString(),
                        arguments = tc.GetProperty("function").GetProperty("arguments").GetString()
                    }
                });
            }

            var messages2 = new List<object>(messages)
            {
                new
                {
                    role = "assistant",
                    content = messagePart.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() : "",
                    tool_calls = parsedToolCalls
                }
            };

            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var functionName = toolCall.GetProperty("function").GetProperty("name").GetString();
                var argsJson = toolCall.GetProperty("function").GetProperty("arguments").GetString();

                JsonElement args = default;
                if (!string.IsNullOrWhiteSpace(argsJson) && argsJson != "null")
                {
                    args = JsonDocument.Parse(argsJson).RootElement;
                }

                string result = "";

                if (functionName == "get_all_events")
                {
                    result = await _eventService.GetAllEventsForAI();
                }
                else if (functionName == "get_categories")
                {
                    result = await _categoryService.GetCategoriesForAI();
                }

                messages2.Add(new
                {
                    role = "tool",
                    tool_call_id = toolCall.GetProperty("id").GetString(),
                    name = functionName,
                    content = result
                });
            }

            var response2 = await _httpClient.PostAsJsonAsync("chat/completions", new
            {
                model = ModelName,
                messages = messages2.ToArray(),
                temperature = 0.1
            });

            var json2 = await response2.Content.ReadAsStringAsync();

            Console.WriteLine("========== GROQ RESPONSE 2 ==========");
            Console.WriteLine($"Status: {response2.StatusCode}");
            Console.WriteLine(json2);
            Console.WriteLine("=====================================");

            if (!response2.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Groq API Error 2]: {json2}");
                return "Lỗi kết nối AI (Lần 2).";
            }

            using var doc2 = JsonDocument.Parse(json2);

            return doc2.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            Console.WriteLine("========== GROQ SERVICE ERROR ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("========================================");
            return "Xin lỗi, hệ thống của HoviloFlow đang bận một chút. Bạn thử lại sau vài giây nhé!";
        }
    }
}