using System.Threading.Tasks;

namespace ChatbotAPI.Services
{
    public interface ICategoryService
    {
        Task<string> GetCategoriesForAI();
    }
}