using BookingAPI.DTOs.EventDTO;
using BookingAPI.DTOs.OrderDetailsDTO;
using BookingAPI.DTOs.TicketDTO;
using BookingAPI.DTOs.UserDTO;
using BookingAPI.Models;

namespace BookingAPI.DTOs.OrderDTO
{
    public class ReadOrderDTO
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public string? PosterUrl { get; set; }

        public DateTime? OrderDate { get; set; }

        public long TotalAmount { get; set; }

        public long? DiscountAmount { get; set; }

        public long FinalAmount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public string? Status { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public ICollection<ReadOrderDetailsDTO>? OrderDetails { get; set; }

        public ICollection<ReadTicketDTO>? Tickets { get; set; }
    }

    public class SendNotificationDTO
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // Ví dụ: "Order", "Event", "System"
    }


}
