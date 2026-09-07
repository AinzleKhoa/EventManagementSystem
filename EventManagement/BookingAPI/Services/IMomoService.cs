namespace BookingAPI.Services
{
    public interface IMomoService
    {
        Task<string> CreatePaymentUrl(long orderId, int amount);
        bool ValidateSignature(IQueryCollection query);
        Task<string> HandleMomoReturn(IQueryCollection query);
    }
}
