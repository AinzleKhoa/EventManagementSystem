using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IUserVoucherService
    {
        Task<List<ReadUserVoucherDTO>?> GetByUserId(int userId, int eventId);

    }
}
