using ChatbotAPI.DTOs;
using ChatbotAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IGroqService _service;
        
        public ChatController(IGroqService service)
        {
            _service = service;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAi([FromBody] List<ChatMessageDTO> chatHistory)
        {
            if (chatHistory == null || !chatHistory.Any())
            {
                return BadRequest("Chat history is empty");
            }

            try
            {
                var reply = await _service.SendMessageAsync(chatHistory);
                return Ok(new { reply = reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
