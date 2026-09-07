using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using ChatAPI.Services;
using ChatAPI.DTOs;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IChatAuthorizationService _authService;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(
            IConversationService conversationService,
            IChatAuthorizationService authService,
            ILogger<ConversationsController> logger)
        {
            _conversationService = conversationService;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<ActionResult<ConversationDTO>> CreateConversation([FromBody] CreateConversationDTO dto)
        {
            try
            {
                var result = await _conversationService.CreateConversationAsync(dto);
                return CreatedAtAction(nameof(GetConversationById), new { id = result.ConversationId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDTO>> GetConversationById(int id)
        {
            try
            {
                var result = await _conversationService.GetConversationByIdAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Conversation {id} not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [EnableQuery]
        public async Task<ActionResult<List<ConversationDTO>>> GetConversationsByUserId(int userId)
        {
            try
            {
                var result = await _conversationService.GetConversationsByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user conversations");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("event/{eventId}")]
        [EnableQuery]
        public async Task<ActionResult<List<ConversationDTO>>> GetConversationsByEventId(int eventId)
        {
            try
            {
                var result = await _conversationService.GetConversationsByEventIdAsync(eventId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event conversations");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ConversationDTO>> UpdateConversation(int id, [FromBody] UpdateConversationDTO dto, [FromQuery] int userId)
        {
            try
            {
                var isAdmin = await _authService.IsAdminAsync(id, userId);
                if (!isAdmin)
                {
                    return Forbid("Only admin can update this conversation");
                }

                var result = await _conversationService.UpdateConversationAsync(id, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Conversation {id} not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteConversation(int id, [FromQuery] int userId)
        {
            try
            {
                var isAdmin = await _authService.IsAdminAsync(id, userId);
                if (!isAdmin)
                {
                    return Forbid("Only admin can delete this conversation");
                }

                var result = await _conversationService.DeleteConversationAsync(id);
                if (result)
                    return NoContent();
                return NotFound(new { message = "Conversation not found" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Conversation {id} not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{conversationId}/participants")]
        public async Task<ActionResult<ConversationDTO>> AddParticipant(int conversationId, [FromBody] AddConversationParticipantDTO dto, [FromQuery] int userId)
        {
            try
            {
                var isAdmin = await _authService.IsAdminAsync(conversationId, userId);
                if (!isAdmin)
                {
                    return Forbid("Only admin can add participants");
                }

                var result = await _conversationService.AddParticipantAsync(conversationId, dto.UserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Error adding participant to conversation {conversationId}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{conversationId}/participants/{participantUserId}")]
        public async Task<ActionResult> RemoveParticipant(int conversationId, int participantUserId, [FromQuery] int userId)
        {
            try
            {
                var isAdmin = await _authService.IsAdminAsync(conversationId, userId);
                if (!isAdmin)
                {
                    return Forbid("Only admin can remove participants");
                }

                var result = await _conversationService.RemoveParticipantAsync(conversationId, participantUserId);
                if (result)
                    return NoContent();
                return NotFound(new { message = "Participant not found" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Error removing participant from conversation {conversationId}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing participant");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{conversationId}/participants")]
        [EnableQuery]
        public async Task<ActionResult<List<ConversationParticipantDTO>>> GetParticipants(int conversationId)
        {
            try
            {
                var result = await _conversationService.GetParticipantsAsync(conversationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting participants");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{conversationId}/participants/role")]
        public async Task<ActionResult> AssignRole(int conversationId, [FromBody] AssignRoleDTO dto, [FromQuery] int userId)
        {
            try
            {
                var isAdmin = await _authService.IsAdminAsync(conversationId, userId);
                if (!isAdmin)
                {
                    return Forbid("Only admin can assign roles");
                }

                await _conversationService.AssignRoleAsync(conversationId, dto.UserId, dto.Role);
                return Ok(new { message = "Role assigned successfully" });
             }
             catch (InvalidOperationException ex)
             {
                _logger.LogWarning(ex, $"Error assigning role in conversation {conversationId}");
                return BadRequest(new { message = ex.Message });
             }
             catch (Exception ex)
             {
                _logger.LogError(ex, "Error assigning role");
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpPost("{conversationId}/participants/internal")]
        [AllowAnonymous]
        public async Task<ActionResult<ConversationDTO>> AddParticipantInternal(int conversationId, [FromBody] AddConversationParticipantDTO dto)
        {
            try
            {
                _logger.LogInformation($"[INTERNAL] Adding participant {dto.UserId} to conversation {conversationId}");
                var result = await _conversationService.AddParticipantAsync(conversationId, dto.UserId);
                _logger.LogInformation($"[INTERNAL] ✅ Successfully added participant {dto.UserId} to conversation {conversationId}");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Error adding participant to conversation {conversationId}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding participant");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
