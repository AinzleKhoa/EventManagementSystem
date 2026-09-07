namespace BookingAPI.DTOs.VoucherDTO
{
    public class ReadVoucherDTO
    {
        public int VoucherId { get; set; }
        public string Code { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public decimal DiscountValue { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int Quantity { get; set; }
        public int UsedCount { get; set; }
        public string EventTitle { get; set; }
        public bool IsActive { get; set; }
        public int EventId { get; set; }
    }
}
