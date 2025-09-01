using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FitPick_EXE201.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_settings.From));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    _settings.UseSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send email", ex);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
