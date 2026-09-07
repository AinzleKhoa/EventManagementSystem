using AuthenticationAPI.DTOs;
using AuthenticationAPI.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace AuthenticationAPI.Controllers
{
    [Route("odata/UsersOData")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UsersODataController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersODataController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<ReadUserDTO> Get()
        {
            return _userService.GetAll();
        }
    }
}
