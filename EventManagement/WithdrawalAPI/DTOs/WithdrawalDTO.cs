using System.ComponentModel.DataAnnotations;

namespace WithdrawalAPI.DTOs
{
    public class ReadWithdrawalDTO
    {
        public int WithdrawalId { get; set; }
        public int OrganizerId { get; set; }
        public int? EventId { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; } = null!;
        public string BankAccountNumber { get; set; } = null!;
        public string BankAccountName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? RejectReason { get; set; }
    }

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
        [Required(ErrorMessage = "Reject reason is required.")]
        [MaxLength(500)]
        public string RejectReason { get; set; } = null!;
    }
}
