using BookingAPI.DTOs.TicketDTO;
using BookingAPI.Models;
using BookingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // GET: api/Tickets
        [HttpGet]
        [EnableQuery]
        public IQueryable<ReadTicketDTO> GetTickets()
        {
            return _ticketService.GetAll();
        }

        
        [HttpGet("by-event/{id}")]
        [EnableQuery]
        public IActionResult GetTicketsByEventID(int id)
        {
            var tickets = _ticketService.GetAll(id);

            if (tickets == null)
            {
                return NotFound();
            }

            return Ok(tickets);
        }

        // GET: api/Tickets/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadTicketDTO>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetById(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        // PUT: api/Tickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, UpdateTicketDTO ticket)
        {

            await _ticketService.Update(id, ticket);

            return NoContent();
        }

        // POST: api/Tickets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostTicket(CreateTicketDTO ticket)
        {
            await _ticketService.Add(ticket);

            return Ok(new { message = "Add success" });
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _ticketService.GetById(id);
            if (ticket == null)
            {
                return NotFound();
            }

            await _ticketService.Delete(id);
            return NoContent();
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanTicket([FromBody] TicketCheckInRequestDTO request)
        {
            // Lấy StaffId từ JWT Token của người đang đăng nhập (Claim "sub" hoặc "id")
            var staffIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            int staffId = int.Parse(staffIdClaim ?? "0");

            var result = await _ticketService.CheckInAsync(request, staffId);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Trả về 400 kèm câu thông báo lỗi
            }

            return Ok(result); // Trả về 200 kèm thông tin vé thành công
        }

        [HttpGet("{code:guid}")]
        public async Task<ActionResult<TicketValidationDTO>> GetTicketByCode(Guid code)
        {
            var ticket = await _ticketService.GetTicketByCode(code);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

    }
}
