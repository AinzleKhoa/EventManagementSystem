using EventAPI.DTOs;
using EventAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace EventAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypesController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;

        public TicketTypesController(ITicketTypeService ticketTypeService)
        {
            _ticketTypeService = ticketTypeService;
        }

        [HttpGet("by-event/{eventId}")]
        [EnableQuery]
        public async Task<ActionResult<List<ReadTicketTypeDTO>>> GetByEventId(int eventId)
        {
            var ticketTypes = await _ticketTypeService.GetTicketTypesByEventIdAsync(eventId);
            return Ok(ticketTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadTicketTypeDTO>> GetById(int id)
        {
            var ticketType = await _ticketTypeService.GetTicketTypeByIdAsync(id);
            if (ticketType == null) return NotFound();
            return Ok(ticketType);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ReadTicketTypeDTO>> Create([FromBody] CreateTicketTypeDTO createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdTicket = await _ticketTypeService.CreateTicketTypeAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = createdTicket.TicketTypeId }, createdTicket);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ReadTicketTypeDTO>> Update(int id, [FromBody] UpdateTicketTypeDTO updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updatedTicket = await _ticketTypeService.UpdateTicketTypeAsync(id, updateDto);
                if (updatedTicket == null) return NotFound();
                return Ok(updatedTicket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _ticketTypeService.DeleteTicketTypeAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-quantity/{id}")]
        public async Task<ActionResult> UpdateQuantity(int id, [FromBody] UpdateTicketTypeQuantityDTO update)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _ticketTypeService.UpdateTicketTypeQuantityAsync(id, update);

            return Ok();
        }
    }
}