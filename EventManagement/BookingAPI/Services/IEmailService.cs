using System.Net.Mail;

namespace BookingAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, AlternateView htmlView);
    }
}
