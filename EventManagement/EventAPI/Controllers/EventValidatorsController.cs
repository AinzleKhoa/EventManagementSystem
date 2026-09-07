using EventAPI.DTOs;
using EventAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace EventAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventValidatorsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventValidatorsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddValidator([FromBody] AddValidatorDTO addValidatorDto)
        {
            try
            {
                if (addValidatorDto == null || string.IsNullOrWhiteSpace(addValidatorDto.ValidatorEmail))
                {
                    return BadRequest(new { success = false, message = "Validator email cannot be empty." });
                }

                var result = await _eventService.AddValidatorAsync(addValidatorDto);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddValidator error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("{eventId}")]
        [EnableQuery]
        public async Task<ActionResult<List<ReadEventValidatorDTO>>> GetValidatorsByEventId(int eventId)
        {
            try
            {
                if (eventId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid event ID." });
                }

                var result = await _eventService.GetValidatorsByEventIdAsync(eventId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetValidatorsByEventId error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpDelete("{eventStaffId}")]
        public async Task<IActionResult> RemoveValidator(int eventStaffId)
        {
            try
            {
                if (eventStaffId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid validator ID." });
                }

                var result = await _eventService.RemoveValidatorAsync(eventStaffId);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Validator does not exist." });
                }
                return Ok(new { success = true, message = "Validator removed successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemoveValidator error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("deactivate-expired")]
        public async Task<ActionResult<bool>> DeactivateExpiredValidators()
        {
            try
            {
                var result = await _eventService.DeactivateExpiredValidatorsAsync();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeactivateExpiredValidators error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("by-validator/{validatorEmail}")]
        [EnableQuery]
        public async Task<ActionResult<List<ReadEventDTO>>> GetEventsByValidatorEmail(string validatorEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(validatorEmail))
                {
                    return BadRequest(new { success = false, message = "Validator email cannot be empty." });
                }

                var result = await _eventService.GetEventsByValidatorEmailAsync(validatorEmail);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetEventsByValidatorEmail error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("check/{eventId}/{validatorEmail}")]
        public async Task<ActionResult<bool>> IsValidatorOfEvent(int eventId, string validatorEmail)
        {
            try
            {
                if (eventId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid event ID." });
                }

                if (string.IsNullOrWhiteSpace(validatorEmail))
                {
                    return BadRequest(new { success = false, message = "Validator email cannot be empty." });
                }

                var result = await _eventService.IsValidatorOfEventAsync(eventId, validatorEmail);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsValidatorOfEvent error: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
