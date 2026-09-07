using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchTerm, bool? isActive, int? roleId, int pageNumber = 1, int pageSize = 10, string sortBy = "UserId", string sortOrder = "desc")
        {
            var filter = new UserSearchFilterDTO
            {
                SearchTerm = searchTerm,
                IsActive = isActive,
                RoleId = roleId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            try
            {
                var result = await _userService.SearchAndFilter(filter);

                ViewData["SearchTerm"] = searchTerm;
                ViewData["IsActive"] = isActive;
                ViewData["RoleId"] = roleId;
                ViewData["SortBy"] = sortBy;
                ViewData["SortOrder"] = sortOrder;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Could not load users: " + ex.Message;
                TempData["ToastType"] = "danger";

                var emptyResult = new PaginatedResultDTO<ReadUserDTO>
                {
                    Items = new List<ReadUserDTO>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return View(emptyResult);
            }
        }

        // GET: Users/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userService.Create(model);
                TempData["ToastMessage"] = "User created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return View(model);
            }
        }

        // GET: Users/Update/5
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var userDto = await _userService.GetById(id);
                if (userDto == null) return NotFound();

                var updateModel = _mapper.Map<UpdateUserDTO>(userDto);
                ViewData["CurrentAvatar"] = userDto.AvatarUrl;
                return View(updateModel);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // POST: Users/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateUserDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userService.Update(id, model);
                TempData["ToastMessage"] = "User updated successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            try
            {
                await _userService.LockAndUnlockAccount(id);
                return Json(new { success = true, message = "Status updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}