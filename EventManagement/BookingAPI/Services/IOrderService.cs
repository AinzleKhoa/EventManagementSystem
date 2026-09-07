using BookingAPI.DTOs.OrderDTO;
using BookingAPI.Models;

namespace BookingAPI.Services;

public interface IOrderService
{
    Task<int> Add(CreateOrderDTO model);
    IQueryable<ReadOrderDTO> GetAll();
    Task<ReadOrderDTO> GetById(int id);
    Task Update(int id, Order model);
    Task Delete(int id);
    Task<List<Ticket>> GenerateTickets(Order order);
    Task DeductQuantity(Order order);
    Task<bool> HasPaidTicketAsync(int customerId, int eventId);
    Task<string> HandleOrderReturn(Order order, string paymentMethod, string? transactionId);

    Task SendOrderPaidWebhookAsync(ReadOrderDTO order);
    Task SendEmailTickets(Order order);
    Task CancelOrderAsync(int orderId);

}
