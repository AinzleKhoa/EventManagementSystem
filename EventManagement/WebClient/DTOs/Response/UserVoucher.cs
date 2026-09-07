using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Response
{
    public class ReadUserVoucherDTO
    {
        public int Id { get; set; }

        public int VoucherId { get; set; }

        public int UserId { get; set; }

        public int? Quantity { get; set; }

        public ReadVoucherDTO? Voucher { get; set; }
    }
}
