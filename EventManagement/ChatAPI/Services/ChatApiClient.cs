using System.Net.Http.Json;
using ChatAPI.DTOs;

namespace ChatAPI.Services
{
    public interface IChatApiClient
    {
        Task<ConversationDTO> CreateConversationAsync(CreateConversationDTO dto);
        Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId);
        Task<bool> RemoveParticipantAsync(int conversationId, int userId);
        Task<List<ConversationDTO>> GetEventConversationsAsync(int eventId);
        Task<List<ConversationDTO>> GetUserConversationsAsync(int userId);
    }

    public class ChatApiClient : IChatApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatApiClient> _logger;

        public ChatApiClient(HttpClient httpClient, ILogger<ChatApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ConversationDTO> CreateConversationAsync(CreateConversationDTO dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/conversations/create", dto);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ConversationDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                throw new InvalidOperationException("Failed to create conversation", ex);
            }
        }

        public async Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId)
        {
            try
            {
                var request = new AddConversationParticipantDTO { ConversationId = conversationId, UserId = userId };
                var response = await _httpClient.PostAsJsonAsync($"/api/conversations/{conversationId}/participants", request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ConversationDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error adding participant {userId} to conversation {conversationId}");
                throw new InvalidOperationException($"Failed to add participant to conversation", ex);
            }
        }

        public async Task<bool> RemoveParticipantAsync(int conversationId, int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/conversations/{conversationId}/participants/{userId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error removing participant {userId} from conversation {conversationId}");
                throw new InvalidOperationException($"Failed to remove participant from conversation", ex);
            }
        }

        public async Task<List<ConversationDTO>> GetEventConversationsAsync(int eventId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/conversations/event/{eventId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ConversationDTO>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error getting conversations for event {eventId}");
                throw new InvalidOperationException("Failed to get event conversations", ex);
            }
        }

        public async Task<List<ConversationDTO>> GetUserConversationsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/conversations/user/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ConversationDTO>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error getting conversations for user {userId}");
                throw new InvalidOperationException("Failed to get user conversations", ex);
            }
        }
    }
}
