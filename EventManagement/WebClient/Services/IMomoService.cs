namespace WebClient.Services
{
    public interface IMomoService
    {
        Task<string> CreatePaymentUrl(int orderId);
    }
}
