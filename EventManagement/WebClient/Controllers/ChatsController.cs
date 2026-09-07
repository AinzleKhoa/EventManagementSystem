using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebClient.DTOs;
using WebClient.Services;

namespace WebClient.Controllers
{
    /// <summary>
    /// Controller for managing conversations and messages
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class ChatsController : Controller
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatsController> _logger;

        public ChatsController(IChatService chatService, ILogger<ChatsController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// Display chat interface
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        #region Conversation Operations

        /// <summary>
        /// Create a new conversation
        /// </summary>
        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDTO dto)
        {
            try
            {
                var result = await _chatService.CreateConversationAsync(dto);
                if (result == null)
                    return BadRequest(new { message = "Failed to create conversation" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get conversation by ID
        /// </summary>
        [HttpGet("conversations/{id}")]
        public async Task<IActionResult> GetConversation(int id)
        {
            try
            {
                var result = await _chatService.GetConversationAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all conversations for current user
        /// </summary>
        [HttpGet("conversations/user/{userId}")]
        public async Task<IActionResult> GetUserConversations(int userId)
        {
            try
            {
                var result = await _chatService.GetUserConversationsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user conversations");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all conversations for an event
        /// </summary>
        [HttpGet("conversations/event/{eventId}")]
        public async Task<IActionResult> GetEventConversations(int eventId)
        {
            try
            {
                var result = await _chatService.GetEventConversationsAsync(eventId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event conversations");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update conversation details
        /// </summary>
        [HttpPut("conversations/{id}")]
        public async Task<IActionResult> UpdateConversation(int id, [FromBody] UpdateConversationDTO dto)
        {
            try
            {
                var result = await _chatService.UpdateConversationAsync(id, dto);
                if (result == null)
                    return BadRequest(new { message = "Failed to update conversation" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete conversation
        /// </summary>
        [HttpDelete("conversations/{id}")]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            try
            {
                var result = await _chatService.DeleteConversationAsync(id);
                if (!result)
                    return BadRequest(new { message = "Failed to delete conversation" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Message Operations

        /// <summary>
        /// Send a new message
        /// </summary>
        [HttpPost("messages")]
        [HttpPost("messages/send")]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDTO dto)
        {
            try
            {
                var result = await _chatService.SendMessageAsync(dto);
                if (result == null)
                    return BadRequest(new { message = "Failed to send message" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get messages from conversation
        /// </summary>
        [HttpGet("messages/{conversationId}")]
        [HttpGet("messages/conversation/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _chatService.GetMessagesAsync(conversationId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Edit a message
        /// </summary>
        [HttpPut("messages/{id}")]
        public async Task<IActionResult> EditMessage(int id, [FromBody] UpdateMessageDTO dto)
        {
            try
            {
                var result = await _chatService.EditMessageAsync(id, dto);
                if (result == null)
                    return BadRequest(new { message = "Failed to edit message" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var result = await _chatService.DeleteMessageAsync(id);
                if (!result)
                    return BadRequest(new { message = "Failed to delete message" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get unread message count
        /// </summary>
        [HttpGet("messages/unread/{conversationId}/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int conversationId, int userId)
        {
            try
            {
                var count = await _chatService.GetUnreadCountAsync(conversationId, userId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPut("messages/mark-read/{conversationId}")]
        public async Task<IActionResult> MarkAsRead(int conversationId, [FromQuery] int userId, [FromQuery] int lastReadMessageId)
        {
            try
            {
                var result = await _chatService.MarkAsReadAsync(conversationId, userId, lastReadMessageId);
                if (!result)
                    return BadRequest(new { message = "Failed to mark as read" });

                return Ok(new { message = "Marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking as read");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}
