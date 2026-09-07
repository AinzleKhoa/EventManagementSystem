using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VoucherAPI.DTOs;
using VoucherAPI.Services;

namespace VoucherAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VoucherDistributionController : ControllerBase
    {
        private readonly IVoucherDistributionService _voucherDistributionService;

        public VoucherDistributionController(IVoucherDistributionService voucherDistributionService)
        {
            _voucherDistributionService = voucherDistributionService;
        }

        [HttpPost("email")]
        public async Task<IActionResult> DistributeToUserByEmail(VoucherDistributeByEmailDTO dto)
        {
            try
            {
                await _voucherDistributionService.DistributeToUserByEmail(dto);
                return Ok(new { success = true });
            }
            catch (InvalidOperationException ex)
            {

                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = "A system error occurred while distributing vouchers: " + ex.Message });
            }
        }
    }
}