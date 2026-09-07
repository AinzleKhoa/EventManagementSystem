using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAPI.DTOs;
using VoucherAPI.Models;
using VoucherAPI.Services;

namespace VoucherAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserVouchersController : ControllerBase
    {
        private readonly IUserVoucherService _service;
        private readonly IVoucherService _voucherService;
        private readonly IMapper _mapper;

        public UserVouchersController(IUserVoucherService service, IMapper mapper, IVoucherService voucherService)
        {
            _service = service;
            _mapper = mapper;
            _voucherService = voucherService;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<UserVoucherReadDTO> Get()
        {
            return _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserVoucherReadDTO>> GetUserVoucher(int id)
        {
            var userVoucher = await _service.GetByIdAsync(id);

            if (userVoucher == null)
            {
                return NotFound();
            }

            return userVoucher;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserVoucher(int id, UserVoucherUpdateDTO userVoucher)
        {
            if (id != userVoucher.Id)
            {
                return BadRequest();
            }
            await _service.UpdateAsync(userVoucher);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<UserVoucher>> PostUserVoucher(UserVoucherAddDTO userVoucher)
        {
            await _service.AddAsync(userVoucher);

            return CreatedAtAction("GetUserVoucher", new { id = userVoucher.Id }, userVoucher);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserVoucher(int id)
        {
            await _service.DeleteAsync(id);

            return NoContent();
        }

        [HttpPut("use/{id}")]
        public async Task<IActionResult> UseVoucher(int id)
        {
            var userVoucher = await _service.GetByIdAsync(id);
            if (userVoucher == null)
            {
                return NotFound();
            }
            if (userVoucher.Quantity <= 0)
            {
                return BadRequest("The voucher has no remaining uses.");
            }

            var voucher = await _voucherService.GetByIdAsync(userVoucher.VoucherId);
            if (voucher == null)
            {
                return NotFound();
            }


            if (voucher.UsedCount >= voucher.Quantity)
            {
                return BadRequest("The voucher has run out of quantity.");
            }

            var updateDto = new UserVoucherUpdateDTO
            {
                Id = userVoucher.Id,
                VoucherId = userVoucher.VoucherId,
                UserId = userVoucher.UserId,
                Quantity = userVoucher.Quantity - 1
            };
            await _service.UpdateAsync(updateDto);

            voucher.UsedCount += 1;

            var voucherUpdateDto = _mapper.Map<VoucherUpdateDTO>(voucher);
            await _voucherService.UpdateAsync(voucher.VoucherId, voucherUpdateDto);

            return NoContent();
        }

        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreVoucher(int id)
        {
            var userVoucher = await _service.GetByIdAsync(id);
            if (userVoucher == null)
            {
                return NotFound();
            }

            var voucher = await _voucherService.GetByIdAsync(userVoucher.VoucherId);
            if (voucher == null)
            {
                return NotFound();
            }

            // Restore user voucher quantity
            var updateDto = new UserVoucherUpdateDTO
            {
                Id = userVoucher.Id,
                VoucherId = userVoucher.VoucherId,
                UserId = userVoucher.UserId,
                Quantity = userVoucher.Quantity + 1
            };
            await _service.UpdateAsync(updateDto);

            // Restore total used count
            if (voucher.UsedCount > 0)
            {
                voucher.UsedCount -= 1;
                var voucherUpdateDto = _mapper.Map<VoucherUpdateDTO>(voucher);
                await _voucherService.UpdateAsync(voucher.VoucherId, voucherUpdateDto);
            }

            return NoContent();
        }
    }
}