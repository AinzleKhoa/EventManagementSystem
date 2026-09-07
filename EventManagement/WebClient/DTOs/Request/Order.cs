namespace WebClient.DTOs.Request
{
    public abstract class OrderValidationDTO
    {
        public int CustomerId { get; set; }
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public string? PosterUrl { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public long TotalAmount { get; set; }

        public long? DiscountAmount { get; set; }

        public long FinalAmount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public string? Status { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public int? VoucherId { get; set; }
        public int? UserVoucherId { get; set; }


    }
    public class CreateOrderDTO : OrderValidationDTO
    {
        public List<CreateOrderDetailsDTO>? OrderDetails { get; set; }
    }
    public class UpdateOrderDTO : OrderValidationDTO
    {
        public List<CreateOrderDetailsDTO>? OrderDetails { get; set; }
        public List<CreateTicketDTO>? Tickets { get; set; }

    }
}
