using WebClient.DTOs.Response;

namespace WebClient.ViewModel
{
    public class CheckoutViewModel
    {
        public int EventId { get; set; }
        public int CustomerId { get; set; }

        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string PaymentMethod { get; set; }

        public List<TicketSelectionViewModel> Tickets { get; set; } = new();

        public long TotalAmount { get; set; }
        public long? DiscountAmount { get; set; }

        public ReadEventDTO Event { get; set; }
        public long SubTotal { get; set; }
        public int? VoucherId { get; set; }
        public int? UserVoucherId { get; set; }
    }
}
