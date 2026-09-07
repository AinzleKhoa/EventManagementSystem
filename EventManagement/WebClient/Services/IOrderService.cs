using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IOrderService
    {
        Task<HttpResponseMessage> Add(CreateOrderDTO model);
        Task<List<ReadOrderDTO>> GetOrderByCustomerId(int id);
        Task<ReadOrderDTO> GetById(int id);
        Task<long> GetTotalRevenueByEventId(int eventId);
        Task<bool> CheckUserHasPaidTicket(int eventId);
        Task<List<ReadOrderDTO>> GetOrdersByEventIdAsync(int eventId);
    }
}
