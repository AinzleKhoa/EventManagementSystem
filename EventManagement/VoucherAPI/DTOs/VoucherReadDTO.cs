namespace VoucherAPI.DTOs
{
    public class VoucherReadDTO
    {
        public int VoucherId { get; set; }
        public string Code { get; set; } = null!;
        public string DiscountType { get; set; } = null!;
        public decimal DiscountValue { get; set; }
        public int Quantity { get; set; }
        public DateTime? ValidFrom { get; set; } 
        public DateTime? ValidTo { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
        public string EventTitle { get; set; }
        public int EventId { get; set; }
    }

}
