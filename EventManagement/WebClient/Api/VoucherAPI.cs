namespace WebClient.Api
{
    public static class VoucherAPI
    {
        public const string Base = "/voucher-gateway/vouchers";
        public const string ById = "/voucher-gateway/vouchers/{id}";
        public const string UserVoucherBase = "/voucher-gateway/uservouchers";
        public const string DistributeByEmail = "/voucher-gateway/voucherdistribution/email";
        public const string DistributeToEvent = "/voucher-gateway/voucherdistribution/event";
    }
}
