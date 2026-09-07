namespace WebClient.Api
{
    public static class BookingAPI
    {
        public const string OrderBase = "booking-gateway/orders";
        public const string OrderById = "booking-gateway/orders/{id}";
        public const string CreateVNPayByOrderId = "booking-gateway/vnpay/create-payment/{id}";
        public const string CreateMomoByOrderId = "booking-gateway/momo/create-payment/{id}";
        public const string OrdersByEventId = "booking-gateway/orders/event/{eventId}";
    }
    public static class TicketAPI
    {
        public const string ByEventId = "booking-gateway/Tickets/by-event/{id}";
        public const string ByCode = "booking-gateway/Tickets/{code}";
        public const string Scan = "booking-gateway/Tickets/scan";
    }
}
