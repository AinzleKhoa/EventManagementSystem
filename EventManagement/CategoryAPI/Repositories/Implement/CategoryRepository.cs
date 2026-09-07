using CategoryAPI.DTOs;
using CategoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoryAPI.Repositories.Implement
{
    public class CategoryRepository : ICategoryRepository
    {

        private readonly CategoryDbContext _context;
        public CategoryRepository(CategoryDbContext context)
        {
            _context = context;
        }
        public async Task Add(Category model)
        {
            try
            {
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task Delete(int id)
        {
            try
            {
                var model = await _context.Categories.FirstOrDefaultAsync(g => g.CategoryId == id);
                if (model != null)
                {
                    _context.Categories.Remove(model);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IQueryable<Category> GetQueryable()
            => _context.Categories.AsQueryable();

        public async Task<Category> GetById(int id)
        {
            try
            {
                return await _context.Categories.FirstOrDefaultAsync(g => g.CategoryId == id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task Update(Category model)
        {
            try
            {
                _context.Categories.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task<(List<Category> Items, int TotalCount)> SearchAndFilter(CategorySearchFilterDTO filter)
        {
            try
            {
                var query = _context.Categories.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(c => c.CategoryName.ToLower().Contains(searchTerm) ||
                                           (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == filter.IsActive.Value);
                }

                var totalCount = await query.CountAsync();

                query = filter.SortBy?.ToLower() switch
                {
                    "description" => filter.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.Description) 
                        : query.OrderBy(c => c.Description),
                    "isactive" => filter.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.IsActive) 
                        : query.OrderBy(c => c.IsActive),
                    _ => filter.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.CategoryName) 
                        : query.OrderBy(c => c.CategoryName)
                };

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> IsCategoryNameExists(string categoryName, int? excludeId = null)
        {
            try
            {
                var query = _context.Categories.Where(c => c.CategoryName.ToLower() == categoryName.ToLower());
                
                if (excludeId.HasValue)
                {
                    query = query.Where(c => c.CategoryId != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> HasRelatedEvents(int categoryId)
        {
            // Cross-service check — always false until EventAPI integration is added
            return await Task.FromResult(false);
        }
    }

}
