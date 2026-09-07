using System.Collections.Generic;
using System.Threading.Tasks;

namespace WithdrawalAPI.Services
{
    public interface IBookingApiClient
    {
        Task<List<BookingOrderDTO>> GetOrdersByEventIdAsync(int eventId);
    }

    public class BookingOrderDTO
    {
        public int OrderId { get; set; }
        public int EventId { get; set; }
        public long FinalAmount { get; set; }
        public string? Status { get; set; }
    }
}
