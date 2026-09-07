using System.Threading.Tasks;

namespace ChatbotAPI.Services
{
    public interface IEventService
    {
        Task<string> GetAllEventsForAI();
    }
}