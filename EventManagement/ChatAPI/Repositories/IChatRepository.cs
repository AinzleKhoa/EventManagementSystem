using ChatAPI.Models;

namespace ChatAPI.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation> GetByIdAsync(int id);
        Task<List<Conversation>> GetByUserIdAsync(int userId);
        Task<List<Conversation>> GetByEventIdAsync(int eventId);
        Task<Conversation> CreateAsync(Conversation conversation);
        Task<Conversation> UpdateAsync(Conversation conversation);
        Task<bool> DeleteAsync(int id);
        Task<Conversation> GetWithParticipantsAndMessagesAsync(int id);
    }

    public interface IMessageRepository
    {
        Task<Message> GetByIdAsync(int id);
        Task<List<Message>> GetByConversationIdAsync(int conversationId, int pageNumber = 1, int pageSize = 50);
        Task<Message> CreateAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<bool> DeleteAsync(int id);
        Task<int> GetUnreadCountAsync(int conversationId, int userId, int? lastReadMessageId);
    }

    public interface IConversationParticipantRepository
    {
        Task<ConversationParticipant> GetByIdAsync(int conversationId, int userId);
        Task<List<ConversationParticipant>> GetByConversationIdAsync(int conversationId);
        Task<List<ConversationParticipant>> GetByUserIdAsync(int userId);
        Task<bool> IsParticipantAsync(int conversationId, int userId);
        Task<ConversationParticipant> AddAsync(ConversationParticipant participant);
        Task<ConversationParticipant> UpdateAsync(ConversationParticipant participant);
        Task<bool> RemoveAsync(int conversationId, int userId);
    }
}
