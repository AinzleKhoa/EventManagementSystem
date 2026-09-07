namespace WebClient.DTOs.Response
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
        public string? OrganizerName { get; set; }
    }
}
