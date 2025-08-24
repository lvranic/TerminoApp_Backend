using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TerminoApp_NewBackend.Services
{
    public class EmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;
        private readonly string? _displayName;

        public EmailService(string host, int port, string username, string password, string fromEmail, string? displayName = null)
        {
            _fromEmail = fromEmail;
            _displayName = displayName;

            _smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _displayName ?? _fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(to);

                await _smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri slanju emaila: {ex.Message}");
                throw;
            }
        }

        public async Task SendReservationNotificationAsync(string toEmail, string subject, string message)
        {
            await SendEmailAsync(toEmail, subject, message);
        }
    }
}