using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using PGSA_Licence3.Data;

namespace PGSA_Licence3.Services.Notifications
{
    public class MailService
    {
        private readonly EmailSettings _settings;
        private readonly ApplicationDbContext _db;

        public MailService(
            IOptions<EmailSettings> settings,
            ApplicationDbContext db)
        {
            _settings = settings.Value;
            _db = db;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_settings.From));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlBody
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, false);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
