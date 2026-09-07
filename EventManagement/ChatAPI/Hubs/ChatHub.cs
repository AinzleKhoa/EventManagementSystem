using Microsoft.AspNetCore.SignalR;
using ChatAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ChatAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IMessageService messageService, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            if (exception != null)
            {
                _logger.LogError(exception, "Disconnection error");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinConversation(int conversationId)
        {
            var groupName = $"Conversation-{conversationId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation($"Client {Context.ConnectionId} joined conversation: {groupName}");

            await Clients.Group(groupName).SendAsync("UserJoined", new
            {
                message = "A user has joined the conversation",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task JoinAllConversations(int[] conversationIds)
        {
            foreach (var conversationId in conversationIds)
            {
                var groupName = $"Conversation-{conversationId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }

            _logger.LogInformation($"Client {Context.ConnectionId} joined {conversationIds.Length} conversation groups");
        }

        public async Task LeaveConversation(int conversationId)
        {
            var groupName = $"Conversation-{conversationId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation($"Client {Context.ConnectionId} left conversation: {groupName}");

            await Clients.Group(groupName).SendAsync("UserLeft", new
            {
                message = "A user has left the conversation",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendMessage(int conversationId, int senderId, string content, string messageType = "Text", string? attachmentUrl = null, string? senderName = null)
        {
            try
            {
                var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? Context.User?.FindFirst("sub")?.Value
                    ?? Context.User?.FindFirst("UserId")?.Value;
                
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int secureUserId))
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Unauthorized: Invalid token claims." });
                    return;
                }

                var dto = new ChatAPI.DTOs.CreateMessageDTO
                {
                    ConversationId = conversationId,
                    SenderId = secureUserId,
                    SenderName = senderName,
                    Content = content,
                    MessageType = messageType,
                    AttachmentUrl = attachmentUrl
                };

                var savedMessage = await _messageService.SendMessageAsync(dto);

                var groupName = $"Conversation-{conversationId}";
                await Clients.Group(groupName).SendAsync("MessageReceived", new
                {
                    messageId = savedMessage.MessageId,
                    conversationId = savedMessage.ConversationId,
                    senderId = savedMessage.SenderId,
                    senderName = savedMessage.SenderName,
                    content = savedMessage.Content,
                    messageType = savedMessage.MessageType,
                    attachmentUrl = savedMessage.AttachmentUrl,
                    sentAt = savedMessage.SentAt,
                    isDeleted = savedMessage.IsDeleted
                });

                _logger.LogInformation($"Message {savedMessage.MessageId} sent to conversation {conversationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", new { message = "Failed to send message" });
            }
        }

        public async Task EditMessage(int messageId, int conversationId, string newContent)
        {
            try
            {
                var dto = new ChatAPI.DTOs.UpdateMessageDTO { Content = newContent };
                var updatedMessage = await _messageService.UpdateMessageAsync(messageId, dto);

                var groupName = $"Conversation-{conversationId}";
                await Clients.Group(groupName).SendAsync("MessageEdited", new
                {
                    messageId = updatedMessage.MessageId,
                    content = updatedMessage.Content,
                    sentAt = updatedMessage.SentAt
                });

                _logger.LogInformation($"Message {messageId} edited");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message");
                await Clients.Caller.SendAsync("Error", new { message = "Failed to edit message" });
            }
        }

        public async Task DeleteMessage(int messageId, int conversationId)
        {
            try
            {
                await _messageService.DeleteMessageAsync(messageId);

                var groupName = $"Conversation-{conversationId}";
                await Clients.Group(groupName).SendAsync("MessageDeleted", new
                {
                    messageId = messageId
                });

                _logger.LogInformation($"Message {messageId} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                await Clients.Caller.SendAsync("Error", new { message = "Failed to delete message" });
            }
        }

        public async Task SendTypingIndicator(int conversationId, int userId)
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("UserId")?.Value;
                
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int secureUserId))
                return;

            var groupName = $"Conversation-{conversationId}";
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserTyping", new
            {
                userId = secureUserId,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task StopTypingIndicator(int conversationId, int userId)
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("UserId")?.Value;
                
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int secureUserId))
                return;

            var groupName = $"Conversation-{conversationId}";
            await Clients.Group(groupName).SendAsync("UserStoppedTyping", new
            {
                userId = secureUserId
            });
        }

        public async Task MarkAsRead(int conversationId, int userId, int lastReadMessageId)
        {
            try
            {
                var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? Context.User?.FindFirst("sub")?.Value
                    ?? Context.User?.FindFirst("UserId")?.Value;
                    
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int secureUserId))
                {
                    await Clients.Caller.SendAsync("Error", new { message = "Unauthorized." });
                    return;
                }

                await _messageService.MarkAsReadAsync(conversationId, secureUserId, lastReadMessageId);

                var groupName = $"Conversation-{conversationId}";
                await Clients.Group(groupName).SendAsync("MessageMarkedAsRead", new
                {
                    userId = secureUserId,
                    lastReadMessageId = lastReadMessageId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking as read");
                await Clients.Caller.SendAsync("Error", new { message = "Failed to mark as read" });
            }
        }
    }
}
