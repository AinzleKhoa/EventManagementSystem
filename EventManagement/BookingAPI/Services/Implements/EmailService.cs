using BookingAPI.Services;
using System.Net;
using System.Net.Mail;

namespace BookingAPI.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, AlternateView htmlView)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]!);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];
            var displayName = _configuration["EmailSettings:DisplayName"];

            using var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!, displayName),
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            mailMessage.AlternateViews.Add(htmlView);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}