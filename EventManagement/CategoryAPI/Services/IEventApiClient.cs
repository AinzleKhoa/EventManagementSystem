using System.Threading.Tasks;

namespace CategoryAPI.Services
{
    public interface IEventApiClient
    {
        Task<bool> HasRelatedEventsAsync(int categoryId);
    }
}
