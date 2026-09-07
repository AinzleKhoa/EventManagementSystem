using System;
using System.ComponentModel.DataAnnotations;
using VoucherAPI.Validations;

namespace VoucherAPI.DTOs
{
    public class VoucherCreateDTO
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
        [Range(1, 100000, ErrorMessage = "Quantity must be greater than 0")] // Không cho tạo nếu Quantity = 0
        public int Quantity { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Valid From date cannot be in the past.")]
        public DateTime? ValidFrom { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Valid To date cannot be in the past.")]
        [DateGreaterThan("ValidFrom", ErrorMessage = "End date must be on or after the start date.")]
        public DateTime? ValidTo { get; set; }
    }
}