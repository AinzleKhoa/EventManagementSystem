using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.OData.Query;
using ChatAPI.Services;
using ChatAPI.DTOs;
using ChatAPI.Hubs;
using ChatAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IChatAuthorizationService _authService;
        private readonly ILogger<MessagesController> _logger;
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly HttpClient _gatewayClient;

        public MessagesController(
            IMessageService messageService,
            IChatAuthorizationService authService,
            ILogger<MessagesController> logger,
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IHubContext<ChatHub> hubContext,
            IHttpClientFactory httpClientFactory)
        {
            _messageService = messageService;
            _authService = authService;
            _logger = logger;
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _hubContext = hubContext;
            _gatewayClient = httpClientFactory.CreateClient("GatewayAPI");
        }

        private (int UserId, string UserName) GetUserInfo()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("UserId")?.Value;
                
            var userName = User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("FullName")?.Value
                ?? User.FindFirst("name")?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value;

            int.TryParse(userIdStr, out int userId);
            
            if (string.IsNullOrEmpty(userName))
                userName = $"User {userId}";
                
            return (userId, userName);
        }

        [HttpGet("test/{conversationId}")]
        public async Task<ActionResult> TestConversation(int conversationId)
        {
            try
            {
                _logger.LogInformation($"[TEST] Testing conversation {conversationId}");

                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    return Ok(new { status = "error", message = $"Conversation {conversationId} not found in database" });
                }

                return Ok(new 
                { 
                    status = "ok", 
                    conversationId = conversation.ConversationId,
                    title = conversation.Title,
                    messageCount = "check query next",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TEST] Error testing conversation");
                return BadRequest(new { status = "error", message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("send")]
        public async Task<ActionResult<MessageDTO>> SendMessage([FromBody] CreateMessageDTO dto)
        {
            try
            {
                var userInfo = GetUserInfo();
                if (userInfo.UserId <= 0) return Unauthorized(new { message = "Invalid token claims." });
                
                // Securely override SenderId and SenderName
                dto.SenderId = userInfo.UserId;
                dto.SenderName = userInfo.UserName;

                var isParticipant = await _authService.IsParticipantAsync(dto.ConversationId, dto.SenderId);
                if (!isParticipant)
                {
                    return BadRequest(new { message = "Sender is not a participant of this conversation" });
                }

                var result = await _messageService.SendMessageAsync(dto);

                var groupName = $"Conversation-{result.ConversationId}";
                await _hubContext.Clients.Group(groupName).SendAsync("MessageReceived", new
                {
                    messageId = result.MessageId,
                    conversationId = result.ConversationId,
                    senderId = result.SenderId,
                    senderName = result.SenderName,
                    content = result.Content,
                    messageType = result.MessageType,
                    attachmentUrl = result.AttachmentUrl,
                    sentAt = result.SentAt,
                    isDeleted = result.IsDeleted
                });

                await NotifyConversationParticipants(result);

                return CreatedAtAction(nameof(GetMessagesByConversation), new { conversationId = result.ConversationId }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error sending message");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("conversation/{conversationId}")]
        [EnableQuery]
        public async Task<ActionResult<List<MessageDTO>>> GetMessagesByConversation(int conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                _logger.LogInformation($"[MESSAGES] GetMessagesByConversation called - ConversationId: {conversationId}, Page: {pageNumber}, Size: {pageSize}");
                var result = await _messageService.GetMessagesAsync(conversationId, pageNumber, pageSize);
                _logger.LogInformation($"[MESSAGES] Successfully returned {result.Count} messages");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Conversation {conversationId} not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages");
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{messageId}")]
        public async Task<ActionResult<MessageDTO>> EditMessage(int messageId, [FromBody] UpdateMessageDTO dto, [FromQuery] int userId = 0)
        {
            try
            {
                var userInfo = GetUserInfo();
                if (userInfo.UserId <= 0) return Unauthorized(new { message = "Invalid token claims." });

                var secureUserId = userInfo.UserId;
                var canEdit = await _authService.CanEditMessageAsync(messageId, secureUserId);
                if (!canEdit)
                {
                    return Forbid("You can only edit your own messages");
                }

                var result = await _messageService.UpdateMessageAsync(messageId, dto);

                var groupName = $"Conversation-{result.ConversationId}";
                await _hubContext.Clients.Group(groupName).SendAsync("MessageEdited", new
                {
                    messageId = result.MessageId,
                    conversationId = result.ConversationId,
                    content = result.Content,
                    sentAt = result.SentAt
                });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Message {messageId} not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(int messageId, [FromQuery] int userId = 0)
        {
            try
            {
                var userInfo = GetUserInfo();
                if (userInfo.UserId <= 0) return Unauthorized(new { message = "Invalid token claims." });

                var secureUserId = userInfo.UserId;
                var canDelete = await _authService.CanDeleteMessageAsync(messageId, secureUserId);
                if (!canDelete)
                {
                    return Forbid("You can only delete your own messages");
                }

                var message = await _messageRepository.GetByIdAsync(messageId);
                var conversationId = message?.ConversationId ?? 0;

                var result = await _messageService.DeleteMessageAsync(messageId);
                if (result)
                {
                    if (conversationId > 0)
                    {
                        var groupName = $"Conversation-{conversationId}";
                        await _hubContext.Clients.Group(groupName).SendAsync("MessageDeleted", messageId);
                    }
                    return NoContent();
                }
                return NotFound(new { message = "Message not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("unread/{conversationId}/{userId}")]
        public async Task<ActionResult<int>> GetUnreadCount(int conversationId, int userId = 0)
        {
            try
            {
                var userInfo = GetUserInfo();
                if (userInfo.UserId <= 0) return Unauthorized();

                var secureUserId = userInfo.UserId;
                var count = await _messageService.GetUnreadCountAsync(conversationId, secureUserId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("mark-read/{conversationId}")]
        public async Task<ActionResult> MarkAsRead(int conversationId, [FromQuery] int userId = 0, [FromQuery] int lastReadMessageId = 0)
        {
            try
            {
                var userInfo = GetUserInfo();
                if (userInfo.UserId <= 0) return Unauthorized();

                var secureUserId = userInfo.UserId;
                var result = await _messageService.MarkAsReadAsync(conversationId, secureUserId, lastReadMessageId);
                if (result)
                    return Ok(new { message = "Marked as read" });
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking as read");
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task NotifyConversationParticipants(MessageDTO message)
        {
            try
            {
                var participants = await _conversationRepository.GetWithParticipantsAndMessagesAsync(message.ConversationId);
                if (participants?.Participants == null)
                {
                    return;
                }

                var contentPreview = string.IsNullOrWhiteSpace(message.Content)
                    ? "sent an attachment"
                    : message.Content.Length > 80 ? message.Content.Substring(0, 80) + "..." : message.Content;

                foreach (var participant in participants.Participants.Where(p => p.UserId != message.SenderId && !p.IsMuted))
                {
                    var payload = new CreateNotificationPayload
                    {
                        UserId = participant.UserId,
                        Title = "New chat message",
                        Message = $"{message.SenderName}: {contentPreview}",
                        Type = "Chat"
                    };

                    var response = await _gatewayClient.PostAsJsonAsync("notification-gateway/notifications", payload);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "Failed to create chat notification. Conversation={ConversationId}, Receiver={UserId}, Status={StatusCode}",
                            message.ConversationId,
                            participant.UserId,
                            response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify participants for Conversation {ConversationId}.", message.ConversationId);
            }
        }

        private class CreateNotificationPayload
        {
            public int UserId { get; set; }
            public string? Title { get; set; }
            public string? Message { get; set; }
            public string? Type { get; set; }
        }
    }
}
