using AuthenticationAPI.DTOs;
using AuthenticationAPI.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;

        // Inject IUserService thay vì AuthDbContext
        public UsersController(IUserService userService, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _userService = userService;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        // GET: api/Users
        [HttpGet]
        [EnableQuery] // Kích hoạt OData
        public IQueryable<ReadUserDTO> Get()
        {
            return _userService.GetAll();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReadUserDTO>> GetUser(int id)
        {
            var user = await _userService.GetById(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> PostUser([FromForm] CreateUserDTO dto)
        {
            try
            {
                if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
                {
                    string fileName = $"avatar_temp_{Guid.NewGuid()}";
                    var uploadedUrl = await _cloudinaryService.UploadImageAsync(dto.AvatarFile, "UserAvatars", fileName);

                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        dto.AvatarUrl = uploadedUrl;
                    }
                }

                await _userService.Create(dto);

                return StatusCode(201, new { message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDTO dto)
        {
            try
            {
                var targetUser = await _userService.GetById(id);
                if (targetUser != null && targetUser.Role.RoleId == 1)
                {
                    return Forbid("You cannot modify an Administrator account.");
                }
                if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
                {
                    string fileName = $"avatar_user_{id}";
                    var uploadedUrl = await _cloudinaryService.UploadImageAsync(dto.AvatarFile, "UserAvatars", fileName);

                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        dto.AvatarUrl = uploadedUrl;
                    }
                }

                await _userService.Update(id, dto);

                return Ok(new { message = "Update user successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Users/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUser(int id)
        //{
        //    try
        //    {
        //        await _userService.Delete(id);
        //        return Ok(new { message = "User delete successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message == "User not found")
        //        {
        //            return NotFound(new { message = ex.Message });
        //        }
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpGet("exists/{id}")]
        public async Task<IActionResult> CheckExists(int id)
        {
            var exists = await _userService.IsExist(id);
            return Ok(new { id, exists });
        }


        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] UserSearchFilterDTO filter)
        {
            filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var result = await _userService.SearchAndFilter(filter);
            return Ok(result);
        }

        // PUT: api/Users/lock-unlock/5
        [HttpPut("lock-unlock/{id}")]
        public async Task<IActionResult> LockAndUnlockAccount(int id)
        {
            try
            {
                var targetUser = await _userService.GetById(id);
                if (targetUser != null && targetUser.Role.RoleId == 1)
                {
                    return Forbid("You cannot modify an Administrator account.");
                }
                await _userService.LockAndUnlockAccount(id);
                return Ok(new { message = "Account status updated successfully" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found")
                    return NotFound(new { message = ex.Message });

                return BadRequest(new { message = ex.Message });
            }
        }
    }
}