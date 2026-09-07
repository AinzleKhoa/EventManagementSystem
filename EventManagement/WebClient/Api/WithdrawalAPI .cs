namespace WebClient.Api
{
    public static class WithdrawalAPI
    {
        public const string Base    = "withdrawal-gateway/withdrawals";
        public const string ById    = "withdrawal-gateway/withdrawals/{id}";
        public const string My      = "withdrawal-gateway/withdrawals/my";
        public const string Approve = "withdrawal-gateway/withdrawals/{id}/approve";
        public const string Reject  = "withdrawal-gateway/withdrawals/{id}/reject";
    }
}
