using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Security.Claims;
using WithdrawalAPI.DTOs;
using WithdrawalAPI.Services;

namespace WithdrawalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WithdrawalsController : ControllerBase
    {
        private readonly IWithdrawalService _service;

        public WithdrawalsController(IWithdrawalService service)
        {
            _service = service;
        }

        private int GetOrganizerIdFromToken()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out var id))
                throw new UnauthorizedAccessException("Invalid organizer identity.");
            return id;
        }

        // UC41 - Organizer: Create withdrawal request
        [HttpPost]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Create([FromBody] CreateWithdrawalDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var organizerId = GetOrganizerIdFromToken();
                var result = await _service.Create(organizerId, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.WithdrawalId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // UC42 - Organizer: Update withdrawal request (pending only)
        [HttpPut("{id}")]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWithdrawalDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var organizerId = GetOrganizerIdFromToken();
                var result = await _service.Update(id, organizerId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // UC43 - Organizer: View own withdrawal requests + status
        [HttpGet("my")]
        [Authorize(Roles = "organizer")]
        public async Task<IActionResult> GetMine()
        {
            try
            {
                var organizerId = GetOrganizerIdFromToken();
                var result = await _service.GetByOrganizer(organizerId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        // UC61 - Admin: View all withdrawal requests (paginated + filter)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll([FromQuery] WithdrawalFilterDTO filter)
        {
            var result = await _service.SearchAndFilter(filter);
            return Ok(result);
        }

        [HttpGet("odata")]
        [Authorize(Roles = "admin")]
        [EnableQuery]
        public IQueryable<ReadWithdrawalDTO> GetWithdrawalsQuery()
        {
            return _service.GetAllQueryable();
        }

        // Get by id (shared)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // UC62 - Admin: Approve withdrawal request
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var result = await _service.Approve(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // UC63 - Admin: Reject withdrawal request with reason
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectReasonDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.Reject(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
