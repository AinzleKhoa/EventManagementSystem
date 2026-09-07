namespace WebClient.Services
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrl(int orderId);
    }
}
