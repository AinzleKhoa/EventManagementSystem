namespace VoucherAPI.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int EventId { get; set; }
        public long TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
