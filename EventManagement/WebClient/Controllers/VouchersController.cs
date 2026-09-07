using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response; 
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize(Roles = "organizer,admin")]
    public class VouchersController : Controller
    {
        private readonly IVoucherService _voucherService;
        private readonly IEventService _eventService;
        IMapper _mapper;
        public VouchersController(IVoucherService voucherService, IEventService eventService,IMapper mapper)
        {
            _voucherService = voucherService;
            _eventService = eventService;
            _mapper = mapper;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdStr, out int uid) ? uid : 0;
        }

        // GET: /Vouchers
        public async Task<IActionResult> Index(string searchTerm, bool? isActive, string sortBy = "Code", string sortOrder = "asc", int pageNumber = 1)
        {
            int userId = GetCurrentUserId();

            var filter = new VoucherSearchFilterDTO
            {
                SearchTerm = searchTerm,
                IsActive = isActive,
                PageNumber = pageNumber,
                PageSize = 10,
                SortBy = sortBy,
                SortOrder = sortOrder
            };
            var result = await _voucherService.SearchAndFilter(filter) ?? new PaginatedResultDTO<ReadVoucherDTO>();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            var myEvents = (userRole?.ToLower() == "admin")
                ? await _eventService.GetAll()
                : await _eventService.GetAll(organizerId: userId);
            myEvents ??= new List<ReadEventDTO>();
            var myEventIds = myEvents.Select(e => e.EventId).ToList();
            if (userRole?.ToLower() != "admin")
            {
                result.Items = result.Items.Where(v => myEventIds.Contains(v.EventId)).ToList();
            }

            foreach (var voucher in result.Items)
            {
                var matchedEvent = myEvents.FirstOrDefault(e => e.EventId == voucher.EventId);
                voucher.EventTitle = matchedEvent?.Title ?? $"Unknown (ID: {voucher.EventId})";
            }

            ViewData["SearchTerm"] = searchTerm;
            ViewData["IsActive"] = isActive?.ToString();
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;

            return View(result);
        }
        // GET: /Vouchers/Create
        public async Task<IActionResult> CreateAsync()
        {
            int userId = GetCurrentUserId(); 
            var listEvents = await _eventService.GetAll(organizerId: userId);
           

            ViewBag.EventList = new SelectList(listEvents, "EventId", "Title");
            return View();
        }

        // POST: /Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVoucherDTO dto)
        {

            if (!ModelState.IsValid)
            {
                int userId = GetCurrentUserId();
                var events = await _eventService.GetAll(userId);
                ViewBag.EventList = new SelectList(events, "EventId", "Title", dto.EventId);

                return View(dto);
            }

            try
            {
                await _voucherService.Add(dto);


                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Voucher created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = ex.Message;


                int userId = GetCurrentUserId();
                var events = await _eventService.GetAll(userId);
                ViewBag.EventList = new SelectList(events, "EventId", "Title", dto.EventId);


                return View(dto);
            }
        }

        // GET: /Vouchers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _voucherService.GetById(id);
            if (voucher == null) return NotFound();
            var eventItem = await _eventService.GetById(voucher.EventId); 
            var eventName = eventItem?.Title ?? "Unknown Event";

            ViewBag.EventName = eventName;

            return View(_mapper.Map<UpdateVoucherDTO>(voucher));
        }

        // POST: /Vouchers/Edit/5  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateVoucherDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var eventItem = await _eventService.GetById(dto.EventId);
                ViewBag.EventName = eventItem?.Title ?? "Unknown Event";
                return View(dto);
            }

            try
            {
                await _voucherService.Update(id, dto);
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Voucher updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = ex.Message;

                var eventItem = await _eventService.GetById(dto.EventId);
                ViewBag.EventName = eventItem?.Title ?? "Unknown Event";
                return View(dto);
            }
        }

        // GET: /Vouchers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _voucherService.GetById(id);
            if (voucher == null) return NotFound();

            return View(voucher);
        }

        // POST: /Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _voucherService.Delete(id);
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Voucher deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Vouchers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var voucher = await _voucherService.GetById(id);
            if (voucher == null) return NotFound();
            var eventItem = await _eventService.GetById(voucher.EventId);
            ViewBag.EventName = eventItem?.Title ?? $"Unknown (ID: {voucher.EventId})";

                        return View(_mapper.Map<ReadVoucherDTO>(voucher));

                    }

            

                    [HttpPost]

                    public async Task<IActionResult> DistributeByEmail([FromBody] DistributeByEmailRequestDTO dto)

                    {

                        try

                        {

                            await _voucherService.DistributeByEmail(dto);

                            return Json(new { success = true });

                        }

                        catch (Exception ex)

                        {

                            return Json(new { success = false, message = ex.Message });

                        }

                    }

            

                    [HttpPost]

                    public async Task<IActionResult> DistributeToEvent([FromBody] DistributeToEventRequestDTO dto)

                    {

                        try

                        {

                            await _voucherService.DistributeToEvent(dto);

                            return Json(new { success = true });

                        }

                        catch (Exception ex)

                        {

                            return Json(new { success = false, message = ex.Message });

                        }

                    }

                }

            }

            