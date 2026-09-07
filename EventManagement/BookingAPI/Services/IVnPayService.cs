namespace BookingAPI.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(long orderId, int amount, string ipAddress);
        bool ValidateSignature(IQueryCollection query, string rawQuery);
        Task<string> HandleVnPayReturn(IQueryCollection query, string rawQuery);
    }
}
