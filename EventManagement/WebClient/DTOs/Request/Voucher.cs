using System.ComponentModel.DataAnnotations;
using WebClient.Validations;

namespace WebClient.DTOs.Request
{
    public class CreateVoucherDTO
    {
        [Required(ErrorMessage = "EventId is required")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must be uppercase alphanumeric without spaces")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "DiscountType is required")]
        [RegularExpression(@"^(Percent|Amount)$", ErrorMessage = "DiscountType must be strictly 'Percent' or 'Amount'")]
        public string DiscountType { get; set; } = null!;

        [Required]
        [ValidDiscountValue("DiscountType")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100000, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Valid From date cannot be in the past.")]
        public DateTime? ValidFrom { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Valid To date cannot be in the past.")]
        [DateGreaterThan("ValidFrom", ErrorMessage = "End date must be on or after the start date.")]
        public DateTime? ValidTo { get; set; }
    }

    public class UpdateVoucherDTO
    {
        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Code must be uppercase alphanumeric without spaces")]
        public string? Code { get; set; }

        public int EventId { get; set; }
        public int UsedCount { get; set; }

        [Required(ErrorMessage = "DiscountType is required")]
        [RegularExpression(@"^(Percent|Amount)$", ErrorMessage = "DiscountType must be strictly 'Percent' or 'Amount'")]
        public string DiscountType { get; set; } = null!;

        [Required]
        [ValidDiscountValue("DiscountType", ErrorMessage = "Percentage discount cannot exceed 100%.")]
        public decimal DiscountValue { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [GreaterThanOrEqualTo("UsedCount", ErrorMessage = "Quantity cannot be less than Used Count.")]
        public int Quantity { get; set; }

        [Required]
        public DateTime? ValidFrom { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "End date cannot be set in the past.")]
        [DateGreaterThan("ValidFrom", ErrorMessage = "End date must be on or after the start date.")]
        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; }
    }
}