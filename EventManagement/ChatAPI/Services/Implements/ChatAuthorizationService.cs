using ChatAPI.DTOs;
using ChatAPI.Models;
using ChatAPI.Repositories;

namespace ChatAPI.Services.Implements
{
    public class ChatAuthorizationService : IChatAuthorizationService
    {
        private readonly IConversationParticipantRepository _participantRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<ChatAuthorizationService> _logger;

        public ChatAuthorizationService(
            IConversationParticipantRepository participantRepository,
            IMessageRepository messageRepository,
            ILogger<ChatAuthorizationService> logger)
        {
            _participantRepository = participantRepository;
            _messageRepository = messageRepository;
            _logger = logger;
        }

        public async Task<bool> IsParticipantAsync(int conversationId, int userId)
        {
            return await _participantRepository.IsParticipantAsync(conversationId, userId);
        }

        public async Task<bool> IsAdminAsync(int conversationId, int userId)
        {
            var participant = await _participantRepository.GetByIdAsync(conversationId, userId);
            return participant?.Role == "Admin";
        }

        public async Task<bool> CanDeleteMessageAsync(int messageId, int userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning($"Message {messageId} not found");
                return false;
            }

            if (message.SenderId == userId)
                return true;

            return await IsAdminAsync(message.ConversationId, userId);
        }

        public async Task<bool> CanEditMessageAsync(int messageId, int userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning($"Message {messageId} not found");
                return false;
            }

            return message.SenderId == userId;
        }
    }
}
