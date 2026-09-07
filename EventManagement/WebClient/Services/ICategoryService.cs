using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.Models;

namespace WebClient.Services
{
    public interface ICategoryService
    {
        Task Add(CreateCategoryDTO model);
        Task<List<ReadCategoryDTO>> GetAll();
        Task<ReadCategoryDTO> GetById(int id);
        Task Update(int id, UpdateCategoryDTO model);
        Task Delete(int id);
        Task<PaginatedResultDTO<ReadCategoryDTO>> SearchAndFilter(CategorySearchFilterDTO filter);
    }
}
