using BookingAPI.DTOs.ChatDTO;

namespace BookingAPI.Services
{
    /// <summary>
    /// Service to interact with ChatAPI
    /// Responsible for managing conversations and participants
    /// </summary>
    public interface IChatApiClient
    {
        Task<ConversationDTO> GetEventMainConversationAsync(int eventId);
        Task<ConversationDTO> GetOrCreateEventConversationAsync(int eventId);
        Task<List<ConversationDTO>> GetEventConversationsAsync(int eventId);
        Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId);
        Task<bool> RemoveParticipantAsync(int conversationId, int userId);
    }

    /// <summary>
    /// HTTP client for ChatAPI communication
    /// </summary>
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

        /// <summary>
        /// Get the main conversation for an event, or create it if it doesn't exist
        /// This ensures the first person to book an event automatically creates the chat group
        /// </summary>
        public async Task<ConversationDTO> GetOrCreateEventConversationAsync(int eventId)
        {
            try
            {
                // First, try to get existing conversation
                var conversation = await GetEventMainConversationAsync(eventId);

                if (conversation != null)
                {
                    _logger.LogInformation($"Event conversation found for event {eventId}: {conversation.ConversationId}");
                    return conversation;
                }

                // If not found, create new conversation for the event
                _logger.LogInformation($"Creating new conversation for event {eventId}");

                var createRequest = new
                {
                    title = $"Event {eventId} - Group Chat",
                    description = $"Main discussion group for event {eventId}",
                    conversationType = "Group",
                    eventId = eventId
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/api/conversations/create",
                    createRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create conversation for event {eventId}. Status: {response.StatusCode}, Response: {errorContent}");
                    return null;
                }

                var newConversation = await response.Content.ReadFromJsonAsync<ConversationDTO>();
                _logger.LogInformation($"Successfully created conversation {newConversation?.ConversationId} for event {eventId}");

                return newConversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting or creating conversation for event {eventId}");
                return null;
            }
        }

        /// <summary>
        /// Get the main conversation for an event
        /// </summary>
        public async Task<ConversationDTO> GetEventMainConversationAsync(int eventId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/conversations/event/{eventId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get event conversations for event {eventId}");
                    return null;
                }

                var conversations = await response.Content.ReadFromJsonAsync<List<ConversationDTO>>();

                // Return the first conversation (main group chat)
                return conversations?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting main conversation for event {eventId}");
                return null;
            }
        }

        /// <summary>
        /// Get all conversations for an event
        /// </summary>
        public async Task<List<ConversationDTO>> GetEventConversationsAsync(int eventId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/conversations/event/{eventId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get conversations for event {eventId}");
                    return new List<ConversationDTO>();
                }

                return await response.Content.ReadFromJsonAsync<List<ConversationDTO>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversations for event {eventId}");
                return new List<ConversationDTO>();
            }
        }

        /// <summary>
        /// Add a participant to a conversation (using internal endpoint without auth)
        /// </summary>
        public async Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId)
        {
            try
            {
                var request = new AddConversationParticipantDTO
                {
                    ConversationId = conversationId,
                    UserId = userId
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/conversations/{conversationId}/participants/internal",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to add participant {userId} to conversation {conversationId}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<ConversationDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding participant to conversation");
                return null;
            }
        }

        /// <summary>
        /// Remove a participant from a conversation
        /// </summary>
        public async Task<bool> RemoveParticipantAsync(int conversationId, int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"/api/conversations/{conversationId}/participants/{userId}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing participant from conversation");
                return false;
            }
        }
    }
}

