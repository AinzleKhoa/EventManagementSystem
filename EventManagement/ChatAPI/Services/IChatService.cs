using ChatAPI.DTOs;

namespace ChatAPI.Services
{
    public interface IConversationService
    {
        Task<ConversationDTO> CreateConversationAsync(CreateConversationDTO dto);
        Task<ConversationDTO> GetConversationByIdAsync(int id);
        Task<List<ConversationDTO>> GetConversationsByUserIdAsync(int userId);
        Task<List<ConversationDTO>> GetConversationsByEventIdAsync(int eventId);
        Task<ConversationDTO> UpdateConversationAsync(int id, UpdateConversationDTO dto);
        Task<bool> DeleteConversationAsync(int id);
        Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId);
        Task<bool> RemoveParticipantAsync(int conversationId, int userId);
        Task<List<ConversationParticipantDTO>> GetParticipantsAsync(int conversationId);
        Task<bool> AssignRoleAsync(int conversationId, int userId, string role);
    }

    public interface IMessageService
    {
        Task<MessageDTO> SendMessageAsync(CreateMessageDTO dto);
        Task<List<MessageDTO>> GetMessagesAsync(int conversationId, int pageNumber = 1, int pageSize = 50);
        Task<MessageDTO> UpdateMessageAsync(int messageId, UpdateMessageDTO dto);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<int> GetUnreadCountAsync(int conversationId, int userId);
        Task<bool> MarkAsReadAsync(int conversationId, int userId, int lastReadMessageId);
    }
}
