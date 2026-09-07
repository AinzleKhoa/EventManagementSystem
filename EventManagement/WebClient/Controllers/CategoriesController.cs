using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebClient.DTOs.Request;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers
{
    [Authorize(Roles = "admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        IMapper _mapper;

        public CategoriesController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string searchTerm, bool? isActive, int pageNumber = 1, int pageSize = 10, string sortBy = "CategoryName", string sortOrder = "asc")
        {
            var filter = new CategorySearchFilterDTO
            {
                SearchTerm = searchTerm,
                IsActive = isActive,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _categoryService.SearchAndFilter(filter);
            ViewData["SearchTerm"] = searchTerm;
            ViewData["IsActive"] = isActive;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;
            return View(result);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                await _categoryService.Add(model);
                TempData["ToastMessage"] = "Create category successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return View(model);
            }
        }
        public async Task<IActionResult> Update(int id)
        {      
            return View(_mapper.Map<UpdateCategoryDTO>(await _categoryService.GetById(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCategoryDTO model)
        {
            try
            {
                await _categoryService.Update(model.CategoryId, model);
                TempData["ToastMessage"] = "Update category successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetById(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _categoryService.GetById(id);
                if (category != null)
                {
                    await _categoryService.Delete(id);
                }
                TempData["ToastMessage"] = "Delete category successfully!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
            }
            return RedirectToAction("Index");
        }

    }
}
