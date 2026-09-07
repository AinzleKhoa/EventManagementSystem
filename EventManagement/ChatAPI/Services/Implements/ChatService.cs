using AutoMapper;
using ChatAPI.DTOs;
using ChatAPI.Models;
using ChatAPI.Repositories;

namespace ChatAPI.Services.Implements
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IConversationParticipantRepository _participantRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(
            IConversationRepository conversationRepository,
            IConversationParticipantRepository participantRepository,
            IMapper mapper,
            ILogger<ConversationService> logger)
        {
            _conversationRepository = conversationRepository;
            _participantRepository = participantRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ConversationDTO> CreateConversationAsync(CreateConversationDTO dto)
        {
            var conversation = new Conversation
            {
                Type = dto.Type,
                Title = dto.Title,
                ImageUrl = dto.ImageUrl,
                EventId = dto.EventId
            };

            var createdConversation = await _conversationRepository.CreateAsync(conversation);

            var isFirstParticipant = true;
            foreach (var userId in dto.ParticipantIds)
            {
                var participant = new ConversationParticipant
                {
                    ConversationId = createdConversation.ConversationId,
                    UserId = userId,
                    Role = isFirstParticipant ? "Admin" : "Member" 
                };
                await _participantRepository.AddAsync(participant);
                isFirstParticipant = false;
            }

            return _mapper.Map<ConversationDTO>(createdConversation);
        }

        public async Task<ConversationDTO> GetConversationByIdAsync(int id)
        {
            var conversation = await _conversationRepository.GetWithParticipantsAndMessagesAsync(id);
            if (conversation == null)
                throw new InvalidOperationException($"Conversation {id} not found");

            return _mapper.Map<ConversationDTO>(conversation);
        }

        public async Task<List<ConversationDTO>> GetConversationsByUserIdAsync(int userId)
        {
            var conversations = await _conversationRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ConversationDTO>>(conversations);
        }

        public async Task<List<ConversationDTO>> GetConversationsByEventIdAsync(int eventId)
        {
            var conversations = await _conversationRepository.GetByEventIdAsync(eventId);
            return _mapper.Map<List<ConversationDTO>>(conversations);
        }

        public async Task<ConversationDTO> UpdateConversationAsync(int id, UpdateConversationDTO dto)
        {
            var conversation = await _conversationRepository.GetByIdAsync(id);
            if (conversation == null)
                throw new InvalidOperationException($"Conversation {id} not found");

            if (!string.IsNullOrEmpty(dto.Title))
                conversation.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                conversation.ImageUrl = dto.ImageUrl;

            var updated = await _conversationRepository.UpdateAsync(conversation);
            return _mapper.Map<ConversationDTO>(updated);
        }

        public async Task<bool> DeleteConversationAsync(int id)
        {
            return await _conversationRepository.DeleteAsync(id);
        }

        public async Task<ConversationDTO> AddParticipantAsync(int conversationId, int userId)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
                throw new InvalidOperationException($"Conversation {conversationId} not found");

            var isParticipant = await _participantRepository.IsParticipantAsync(conversationId, userId);
            if (isParticipant)
                throw new InvalidOperationException($"User {userId} is already a participant");

            var participant = new ConversationParticipant
            {
                ConversationId = conversationId,
                UserId = userId,
                Role = "Member"
            };

            await _participantRepository.AddAsync(participant);
            return await GetConversationByIdAsync(conversationId);
        }

        public async Task<bool> RemoveParticipantAsync(int conversationId, int userId)
        {
            return await _participantRepository.RemoveAsync(conversationId, userId);
        }

        public async Task<List<ConversationParticipantDTO>> GetParticipantsAsync(int conversationId)
        {
            var participants = await _participantRepository.GetByConversationIdAsync(conversationId);
            return _mapper.Map<List<ConversationParticipantDTO>>(participants);
        }

        public async Task<bool> AssignRoleAsync(int conversationId, int userId, string role)
        {
            var participant = await _participantRepository.GetByIdAsync(conversationId, userId);
            if (participant == null)
                throw new InvalidOperationException($"Participant not found");

            participant.Role = role;
            await _participantRepository.UpdateAsync(participant);
            return true;
        }
    }

    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IConversationParticipantRepository _participantRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            IConversationParticipantRepository participantRepository,
            IMapper mapper,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _participantRepository = participantRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MessageDTO> SendMessageAsync(CreateMessageDTO dto)
        {
            var conversation = await _conversationRepository.GetByIdAsync(dto.ConversationId);
            if (conversation == null)
                throw new InvalidOperationException($"Conversation {dto.ConversationId} not found");

            var isParticipant = await _participantRepository.IsParticipantAsync(dto.ConversationId, dto.SenderId);
            if (!isParticipant)
                throw new InvalidOperationException($"Sender {dto.SenderId} is not a participant");

            var senderParticipant = await _participantRepository.GetByIdAsync(dto.ConversationId, dto.SenderId);

            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = dto.SenderId,
                Content = dto.Content,
                MessageType = dto.MessageType,
                AttachmentUrl = dto.AttachmentUrl
            };

            if (senderParticipant?.Role?.ToLower() == "admin")
            {
                message.SenderName = "Admin";
            }
            else if (!string.IsNullOrEmpty(dto.SenderName))
            {
                message.SenderName = dto.SenderName;
            }
            else
            {
                message.SenderName = $"User {dto.SenderId}";
            }

            var createdMessage = await _messageRepository.CreateAsync(message);

            conversation.LastMessageAt = DateTime.UtcNow;
            await _conversationRepository.UpdateAsync(conversation);

            return _mapper.Map<MessageDTO>(createdMessage);
        }

        public async Task<List<MessageDTO>> GetMessagesAsync(int conversationId, int pageNumber = 1, int pageSize = 50)
        {
            _logger.LogInformation($"[CHAT] GetMessagesAsync called - ConversationId: {conversationId}, Page: {pageNumber}, Size: {pageSize}");

            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                _logger.LogWarning($"[CHAT] Conversation {conversationId} not found");
                throw new InvalidOperationException($"Conversation {conversationId} not found");
            }

            _logger.LogInformation($"[CHAT] Found conversation {conversationId}");

            var messages = await _messageRepository.GetByConversationIdAsync(conversationId, pageNumber, pageSize);
            _logger.LogInformation($"[CHAT] Found {messages.Count} messages for conversation {conversationId}");

            try
            {
                var result = _mapper.Map<List<MessageDTO>>(messages);
                _logger.LogInformation($"[CHAT] Successfully mapped {result.Count} messages");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CHAT] Error mapping messages for conversation {conversationId}");
                throw;
            }
        }

        public async Task<MessageDTO> UpdateMessageAsync(int messageId, UpdateMessageDTO dto)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                throw new InvalidOperationException($"Message {messageId} not found");

            message.Content = dto.Content;
            var updated = await _messageRepository.UpdateAsync(message);
            return _mapper.Map<MessageDTO>(updated);
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            return await _messageRepository.DeleteAsync(messageId);
        }

        public async Task<int> GetUnreadCountAsync(int conversationId, int userId)
        {
            var participant = await _participantRepository.GetByIdAsync(conversationId, userId);
            if (participant == null)
                return 0;

            return await _messageRepository.GetUnreadCountAsync(conversationId, userId, participant.LastReadMessageId);
        }

        public async Task<bool> MarkAsReadAsync(int conversationId, int userId, int lastReadMessageId)
        {
            var participant = await _participantRepository.GetByIdAsync(conversationId, userId);
            if (participant == null)
                return false;

            participant.LastReadMessageId = lastReadMessageId;
            await _participantRepository.UpdateAsync(participant);
            return true;
        }
    }
}
