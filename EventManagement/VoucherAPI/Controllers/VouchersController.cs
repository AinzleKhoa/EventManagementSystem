using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoucherAPI.DTOs;
using VoucherAPI.Services;

namespace VoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _service;
        private readonly IMapper _mapper;

        public VouchersController(IVoucherService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetAll()
        {
            var query = _service.GetQueryable();
            var dtoQuery = query.ProjectTo<VoucherReadDTO>(_mapper.ConfigurationProvider);
            return Ok(dtoQuery);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var voucher = await _service.GetByIdAsync(id);
            if (voucher == null) return NotFound();
            return Ok(_mapper.Map<VoucherReadDTO>(voucher));
        }

        [HttpPost]
        public async Task<IActionResult> Create(VoucherCreateDTO dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(_mapper.Map<VoucherReadDTO>(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VoucherUpdateDTO dto)
        {
            try
            {
                var success = await _service.UpdateAsync(id, dto);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchAndFilter([FromQuery] VoucherSearchFilterDTO filter)
        {
            try
            {
                var result = await _service.SearchAndFilter(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}