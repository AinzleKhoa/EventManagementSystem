using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Request
{
    public class CreateWithdrawalDTO
    {
        public int? EventId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1,000.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Bank name is required.")]
        [MaxLength(100)]
        public string BankName { get; set; } = null!;

        [Required(ErrorMessage = "Bank account number is required.")]
        [MaxLength(50)]
        public string BankAccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Bank account name is required.")]
        [MaxLength(100)]
        public string BankAccountName { get; set; } = null!;
    }

    public class UpdateWithdrawalDTO
    {
        public int WithdrawalId { get; set; }
        public int? EventId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1,000.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Bank name is required.")]
        [MaxLength(100)]
        public string BankName { get; set; } = null!;

        [Required(ErrorMessage = "Bank account number is required.")]
        [MaxLength(50)]
        public string BankAccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Bank account name is required.")]
        [MaxLength(100)]
        public string BankAccountName { get; set; } = null!;
    }

    public class RejectReasonDTO
    {
        public int WithdrawalId { get; set; }

        [Required(ErrorMessage = "Reject reason is required.")]
        [MaxLength(500)]
        public string RejectReason { get; set; } = null!;
    }

    public class WithdrawalFilterDTO
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "RequestedAt";
        public string SortOrder { get; set; } = "desc";
    }
}
