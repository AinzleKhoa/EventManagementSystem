namespace VoucherAPI.DTOs
{
    public class UserVoucherReadDTO
    {
        public int Id { get; set; }

        public int VoucherId { get; set; }

        public int UserId { get; set; }

        public int? Quantity { get; set; }

        public VoucherReadDTO? Voucher { get; set; }
    }
}
