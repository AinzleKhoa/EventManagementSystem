using WebClient.DTOs;

namespace WebClient.Services
{
    /// <summary>
    /// Service to interact with ChatAPI
    /// Manages conversations and real-time messaging
    /// </summary>
    public interface IChatService
    {
        // Conversation operations
        Task<ConversationDTO> CreateConversationAsync(CreateConversationDTO dto);
        Task<ConversationDTO> GetConversationAsync(int conversationId);
        Task<List<ConversationDTO>> GetUserConversationsAsync(int userId);
        Task<List<ConversationDTO>> GetEventConversationsAsync(int eventId);
        Task<ConversationDTO> UpdateConversationAsync(int conversationId, UpdateConversationDTO dto);
        Task<bool> DeleteConversationAsync(int conversationId);
        
        // Participant operations
        Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId);
        Task<bool> RemoveParticipantAsync(int conversationId, int userId);
        Task<List<ConversationParticipantDTO>> GetParticipantsAsync(int conversationId);
        Task<bool> AssignRoleAsync(int conversationId, int userId, string role);
        
        // Message operations
        Task<MessageDTO> SendMessageAsync(CreateMessageDTO dto);
        Task<List<MessageDTO>> GetMessagesAsync(int conversationId, int pageNumber = 1, int pageSize = 50);
        Task<MessageDTO> EditMessageAsync(int messageId, UpdateMessageDTO dto);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<int> GetUnreadCountAsync(int conversationId, int userId);
        Task<bool> MarkAsReadAsync(int conversationId, int userId, int lastReadMessageId);
    }
}
