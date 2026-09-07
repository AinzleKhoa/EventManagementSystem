using EventAPI.Services;

namespace EventAPI.Services.Implement
{
    public class ChatApiClient : IChatApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatApiClient> _logger;

        public ChatApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ChatApiClient> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            var chatApiBaseUrl = configuration["ChatAPI:Url"] ?? "https://localhost:7148";
            _httpClient.BaseAddress = new Uri(chatApiBaseUrl);
        }

        public async Task<CreateEventConversationResponse> CreateEventConversationAsync(int eventId, string eventTitle, int organizerId)
        {
            try
            {
                _logger.LogInformation($"[CHAT] Creating conversation for event {eventId}: {eventTitle}, organizer: {organizerId}");

                var createRequest = new
                {
                    type = "Group",
                    title = eventTitle,
                    eventId = eventId,
                    participantIds = new List<int> { organizerId }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/api/conversations/create",
                    createRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[CHAT] ❌ Failed to create conversation for event {eventId}. Status: {response.StatusCode}, Response: {errorContent}");
                    return null;
                }

                var newConversation = await response.Content.ReadFromJsonAsync<CreateEventConversationResponse>();
                _logger.LogInformation($"[CHAT] ✅ Successfully created conversation {newConversation?.ConversationId} for event {eventId}");

                return newConversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CHAT] ❌ Exception: Failed to create conversation for event {eventId}. Error: {ex.Message}");
                return null;
            }
        }
    }
}
