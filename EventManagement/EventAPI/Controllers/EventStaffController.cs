using EventAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace EventAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventStaffController : ControllerBase
    {
        private readonly IEventStaffService _service;

        public EventStaffController(IEventStaffService service)
        {
            _service = service;
        }

        [HttpGet("{staffId}")]
        [EnableQuery]
        public async Task<IActionResult> GetByStaff(int staffId)
        {
            var result = await _service.GetStaffEventsAsync(staffId);
            return Ok(result);
        }
    }
}
