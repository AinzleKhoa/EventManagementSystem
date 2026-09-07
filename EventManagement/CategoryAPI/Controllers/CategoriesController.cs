using CategoryAPI.DTOs;
using CategoryAPI.Models;
using CategoryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace CategoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Categories/search
        [HttpGet("search")]
        public async Task<ActionResult<PaginatedResultDTO<ReadCategoryDTO>>> SearchCategories([FromQuery] CategorySearchFilterDTO filter)
        {
            try
            {
                var result = await _categoryService.SearchAndFilter(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching categories.", error = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [EnableQuery(MaxTop = 100, AllowedQueryOptions =
            AllowedQueryOptions.Select |
            AllowedQueryOptions.Filter |
            AllowedQueryOptions.OrderBy |
            AllowedQueryOptions.Top |
            AllowedQueryOptions.Skip |
            AllowedQueryOptions.Count)]
        public IQueryable<ReadCategoryDTO> GetCategories()
        {
            return _categoryService.GetAllQueryable();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReadCategoryDTO>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetById(id);

                if (category == null)
                {
                    return NotFound(new { message = $"Category with ID {id} not found." });
                }

                return category;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the category.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, UpdateCategoryDTO category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest(new { message = "Category ID mismatch." });
            }

            try
            {
                await _categoryService.Update(id, category);
                return Ok(new { message = "Category updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the category.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(CreateCategoryDTO category)
        {
            try
            {
                await _categoryService.Add(category);
                return Ok(new { message = "Category created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the category.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.Delete(id);
                return Ok(new { message = "Category deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the category.", error = ex.Message });
            }
        }
    }
}
