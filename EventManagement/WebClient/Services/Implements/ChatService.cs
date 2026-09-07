using WebClient.Api;
using WebClient.DTOs;

namespace WebClient.Services.Implements
{
    /// <summary>
    /// Chat service implementation for communicating with ChatAPI
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatService> _logger;

        public ChatService(HttpClient httpClient, ILogger<ChatService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        #region Conversation Operations

        /// <summary>
        /// Create a new conversation (Private or Group)
        /// </summary>
        public async Task<ConversationDTO> CreateConversationAsync(CreateConversationDTO dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ChatAPI.CreateConversation, dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ConversationDTO>();
                }

                _logger.LogError($"Failed to create conversation. Status: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return null;
            }
        }

        /// <summary>
        /// Get a conversation by ID
        /// </summary>
        public async Task<ConversationDTO> GetConversationAsync(int conversationId)
        {
            try
            {
                var response = await _httpClient.GetAsync(ChatAPI.GetConversation.Replace("{id}", conversationId.ToString()));

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ConversationDTO>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation {conversationId}");
                return null;
            }
        }

        /// <summary>
        /// Get all conversations for a user
        /// </summary>
        public async Task<List<ConversationDTO>> GetUserConversationsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync(ChatAPI.GetUserConversations.Replace("{userId}", userId.ToString()));

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ConversationDTO>>();
                }

                return new List<ConversationDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversations for user {userId}");
                return new List<ConversationDTO>();
            }
        }

        /// <summary>
        /// Get all conversations for an event
        /// </summary>
        public async Task<List<ConversationDTO>> GetEventConversationsAsync(int eventId)
        {
            try
            {
                var response = await _httpClient.GetAsync(ChatAPI.GetEventConversations.Replace("{eventId}", eventId.ToString()));

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ConversationDTO>>();
                }

                return new List<ConversationDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversations for event {eventId}");
                return new List<ConversationDTO>();
            }
        }

        /// <summary>
        /// Update a conversation
        /// </summary>
        public async Task<ConversationDTO> UpdateConversationAsync(int conversationId, UpdateConversationDTO dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    ChatAPI.UpdateConversation.Replace("{id}", conversationId.ToString()), 
                    dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ConversationDTO>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating conversation {conversationId}");
                return null;
            }
        }

        /// <summary>
        /// Delete a conversation
        /// </summary>
        public async Task<bool> DeleteConversationAsync(int conversationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(ChatAPI.DeleteConversation.Replace("{id}", conversationId.ToString()));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting conversation {conversationId}");
                return false;
            }
        }

        #endregion

        #region Participant Operations

        /// <summary>
        /// Add a participant to a conversation
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
                    ChatAPI.AddParticipant.Replace("{id}", conversationId.ToString()),
                    request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ConversationDTO>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding participant to conversation {conversationId}");
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
                    ChatAPI.RemoveParticipant
                        .Replace("{id}", conversationId.ToString())
                        .Replace("{userId}", userId.ToString()));

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing participant from conversation {conversationId}");
                return false;
            }
        }

        /// <summary>
        /// Get all participants of a conversation
        /// </summary>
        public async Task<List<ConversationParticipantDTO>> GetParticipantsAsync(int conversationId)
        {
            try
            {
                var response = await _httpClient.GetAsync(ChatAPI.GetParticipants.Replace("{id}", conversationId.ToString()));

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ConversationParticipantDTO>>();
                }

                return new List<ConversationParticipantDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting participants for conversation {conversationId}");
                return new List<ConversationParticipantDTO>();
            }
        }

        /// <summary>
        /// Assign role to a participant
        /// </summary>
        public async Task<bool> AssignRoleAsync(int conversationId, int userId, string role)
        {
            try
            {
                var request = new AssignRoleDTO
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    Role = role
                };

                var response = await _httpClient.PutAsJsonAsync(
                    ChatAPI.AssignRole.Replace("{id}", conversationId.ToString()),
                    request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role");
                return false;
            }
        }

        #endregion

        #region Message Operations

        /// <summary>
        /// Send a new message
        /// </summary>
        public async Task<MessageDTO> SendMessageAsync(CreateMessageDTO dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ChatAPI.SendMessage, dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MessageDTO>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return null;
            }
        }

        /// <summary>
        /// Get messages from a conversation
        /// </summary>
        public async Task<List<MessageDTO>> GetMessagesAsync(int conversationId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var url = $"{ChatAPI.GetMessages.Replace("{id}", conversationId.ToString())}?pageNumber={pageNumber}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MessageDTO>>();
                }

                return new List<MessageDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for conversation {conversationId}");
                return new List<MessageDTO>();
            }
        }

        /// <summary>
        /// Edit a message
        /// </summary>
        public async Task<MessageDTO> EditMessageAsync(int messageId, UpdateMessageDTO dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    ChatAPI.EditMessage.Replace("{id}", messageId.ToString()),
                    dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MessageDTO>();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error editing message {messageId}");
                return null;
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(ChatAPI.DeleteMessage.Replace("{id}", messageId.ToString()));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId}");
                return false;
            }
        }

        /// <summary>
        /// Get unread message count
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int conversationId, int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    ChatAPI.GetUnreadCount
                        .Replace("{conversationId}", conversationId.ToString())
                        .Replace("{userId}", userId.ToString()));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                    return result?["unreadCount"] ?? 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread count");
                return 0;
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int conversationId, int userId, int lastReadMessageId)
        {
            try
            {
                var url = $"{ChatAPI.MarkAsRead.Replace("{conversationId}", conversationId.ToString())}?userId={userId}&lastReadMessageId={lastReadMessageId}";
                var response = await _httpClient.PutAsync(url, null);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking as read");
                return false;
            }
        }

        #endregion
    }
}

