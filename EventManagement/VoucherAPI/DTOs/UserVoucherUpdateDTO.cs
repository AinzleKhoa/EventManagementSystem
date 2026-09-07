using System.ComponentModel.DataAnnotations;

namespace VoucherAPI.DTOs
{
    public class UserVoucherUpdateDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "VoucherId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "VoucherId must be greater than 0")]
        public int VoucherId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
        public int? Quantity { get; set; }
    }
}
