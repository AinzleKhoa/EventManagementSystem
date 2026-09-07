using CategoryAPI.DTOs;
using CategoryAPI.Models;

namespace CategoryAPI.Services
{
    public interface ICategoryService
    {
        Task Add(CreateCategoryDTO model);
        IQueryable<ReadCategoryDTO> GetAllQueryable();
        Task<ReadCategoryDTO> GetById(int id);
        Task Update(int id, UpdateCategoryDTO model);
        Task Delete(int id);
        Task<PaginatedResultDTO<ReadCategoryDTO>> SearchAndFilter(CategorySearchFilterDTO filter);
    }
}
