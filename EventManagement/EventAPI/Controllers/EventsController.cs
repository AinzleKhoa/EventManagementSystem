using EventAPI.DTOs;
using EventAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace EventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        [AllowAnonymous]
        [EnableQuery]
        public async Task<ActionResult<List<ReadEventDTO>>> GetAll([FromQuery] int? organizerId)
        {
            var dtos = await _eventService.GetAllAsync(organizerId);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadEventDTO>> GetById(int id)
        {
            try
            {
                var eventDto = await _eventService.GetEventByIdAsync(id);
                if (eventDto == null) return NotFound();
                return Ok(eventDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting event {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = $"Error loading event: {ex.Message}" });
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ReadEventDTO>> Create([FromBody] CreateEventDTO createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdEvent = await _eventService.CreateEventAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = createdEvent.EventId }, createdEvent);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ReadEventDTO>> Update(int id, [FromBody] UpdateEventDTO updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updatedEvent = await _eventService.UpdateEventAsync(id, updateDto);
                if (updatedEvent == null) return NotFound();
                return Ok(updatedEvent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var removed = await _eventService.DeleteEventAsync(id);
                if (!removed) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var result = await _eventService.SubmitForApprovalAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [Authorize]
        [HttpPost("{id}/hide")]
        public async Task<IActionResult> HideEvent(int id)
        {
            var result = await _eventService.HideEventAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [Authorize]
        [HttpPost("{id}/resume")]
        public async Task<IActionResult> ResumeEvent(int id)
        {
            var result = await _eventService.ResumeEventAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? query,
            [FromQuery] int[]? categoryIds,
            [FromQuery] string? dateFilter,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool freeOnly = false)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(dateFilter))
            {
                var today = DateTime.Today;
                switch (dateFilter.ToLower())
                {
                    case "today":
                        fromDate = today;
                        toDate = today.AddDays(1).AddTicks(-1);
                        break;
                    case "tomorrow":
                        fromDate = today.AddDays(1);
                        toDate = today.AddDays(2).AddTicks(-1);
                        break;
                    case "weekend":
                        int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
                        fromDate = today.AddDays(daysUntilSaturday);
                        toDate = fromDate.Value.AddDays(2).AddTicks(-1);
                        break;
                }
            }

            if (freeOnly)
            {
                maxPrice = 0;
            }

            var result = await _eventService.SearchEventsAsync(query, categoryIds, fromDate, toDate, minPrice, maxPrice);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("pending")]
        public async Task<ActionResult<List<ReadEventDTO>>> GetPendingEvents()
        {
            var events = await _eventService.GetPendingEventsAsync();
            return Ok(events);
        }
        [Authorize]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var result = await _eventService.ApproveEventAsync(id);
            if (!result)
            {
                return BadRequest(new { message = "Event not found or event is not in Pending status." });
            }
            return NoContent();
        }
        [Authorize]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectEvent(int id, [FromBody] RejectEventDTO rejectDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _eventService.RejectEventAsync(id, rejectDto.Reason);
            if (!result)
            {
                return BadRequest(new { message = "Event not found or event is not in Pending status." });
            }
            return NoContent();
        }
        [Authorize]
        [HttpGet("organizer/{id}")]
        public async Task<ActionResult<ReadEventDTO>> GetEventByOrganizerId(int id)
        {
            try
            {
                var eventDto = await _eventService.GetEventByOrganizerId(id);
                if (eventDto == null) return NotFound();
                return Ok(eventDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting event {id}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = $"Error loading event: {ex.Message}" });
            }
        }

        [HttpGet("count-approved-active")]
        public async Task<ActionResult<int>> GetApprovedActiveCount()
        {
            var count = await _eventService.GetTotalApprovedEventsAsync();
            return Ok(count);
        }

        [HttpGet("count-total")]
        public async Task<ActionResult<int>> GetTotalCount()
        {
            var count = await _eventService.GetTotalEventsAsync();
            return Ok(count);
        }

        [HttpGet("top-selling")]
        [Authorize]
        public async Task<IActionResult> GetTopSellingEvents([FromQuery] int count = 5)
        {
            try
            {
                var topEvents = await _eventService.GetTopSellingEventsAsync(count);

                if (topEvents == null || !topEvents.Any())
                {
                    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(topEvents));
                    return NotFound(new { message = "No events found." });
                }

                return Ok(topEvents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during processing.", error = ex.Message });
            }
        }

    }
}