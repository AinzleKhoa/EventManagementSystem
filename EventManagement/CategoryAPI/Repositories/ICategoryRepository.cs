using CategoryAPI.DTOs;
using CategoryAPI.Models;

namespace CategoryAPI.Repositories
{
    public interface ICategoryRepository
    {
        Task Add(Category model);
        IQueryable<Category> GetQueryable();
        Task<Category> GetById(int id);
        Task Update(Category model);
        Task Delete(int id);
        Task<(List<Category> Items, int TotalCount)> SearchAndFilter(CategorySearchFilterDTO filter);
        Task<bool> IsCategoryNameExists(string categoryName, int? excludeId = null);
        Task<bool> HasRelatedEvents(int categoryId);
    }
}
