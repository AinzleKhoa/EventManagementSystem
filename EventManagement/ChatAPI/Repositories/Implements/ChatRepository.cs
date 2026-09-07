using Microsoft.EntityFrameworkCore;
using ChatAPI.Models;

namespace ChatAPI.Repositories.Implements
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ChatDbContext _context;

        public ConversationRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation> GetByIdAsync(int id)
        {
            return await _context.Conversations.FindAsync(id);
        }

        public async Task<List<Conversation>> GetByUserIdAsync(int userId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetByEventIdAsync(int eventId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.EventId == eventId)
                .ToListAsync();
        }

        public async Task<Conversation> CreateAsync(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation> UpdateAsync(Conversation conversation)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var conversation = await _context.Conversations.FindAsync(id);
            if (conversation == null)
                return false;

            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Conversation> GetWithParticipantsAndMessagesAsync(int id)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationId == id);
        }
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public MessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<Message> GetByIdAsync(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<List<Message>> GetByConversationIdAsync(int conversationId, int pageNumber = 1, int pageSize = 50)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Message> CreateAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return false;

            message.IsDeleted = true;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int conversationId, int userId, int? lastReadMessageId)
        {
            var query = _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted);

            if (lastReadMessageId.HasValue)
            {
                query = query.Where(m => m.MessageId > lastReadMessageId.Value);
            }

            return await query.CountAsync();
        }
    }

    public class ConversationParticipantRepository : IConversationParticipantRepository
    {
        private readonly ChatDbContext _context;

        public ConversationParticipantRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationParticipant> GetByIdAsync(int conversationId, int userId)
        {
            return await _context.ConversationParticipants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        }

        public async Task<List<ConversationParticipant>> GetByConversationIdAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .Where(p => p.ConversationId == conversationId)
                .ToListAsync();
        }

        public async Task<List<ConversationParticipant>> GetByUserIdAsync(int userId)
        {
            return await _context.ConversationParticipants
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> IsParticipantAsync(int conversationId, int userId)
        {
            return await _context.ConversationParticipants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        }

        public async Task<ConversationParticipant> AddAsync(ConversationParticipant participant)
        {
            _context.ConversationParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<ConversationParticipant> UpdateAsync(ConversationParticipant participant)
        {
            _context.ConversationParticipants.Update(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<bool> RemoveAsync(int conversationId, int userId)
        {
            var participant = await GetByIdAsync(conversationId, userId);
            if (participant == null)
                return false;

            _context.ConversationParticipants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
