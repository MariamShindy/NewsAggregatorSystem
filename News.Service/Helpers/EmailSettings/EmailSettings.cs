using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using News.Core.Settings;
using News.Core.Entities;
using News.Core.Dtos;
using Microsoft.AspNetCore.Identity;
namespace News.Service.Helpers.EmailSettings
{
    public class EmailSettings(IOptions<MailSettings> _options) : IMailSettings
    {
        public async Task SendEmail(Email email)
        {
            var mail = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_options.Value.Email),
                Subject = email.Subject
            };
            mail.To.Add(MailboxAddress.Parse(email.To));
            mail.From.Add(new MailboxAddress(_options.Value.DisplayName, _options.Value.Email));
            var builder = new BodyBuilder
            {
                HtmlBody = email.Body 
            };
            mail.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_options.Value.Host, _options.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_options.Value.Email, _options.Value.Password);
            smtp.Send(mail);
            smtp.Disconnect(true);
        }
        public async Task SendNotificationEmail(NotificationDto notificationDto , string userEmail)
        {
            var email = new Email
            {
                To = userEmail, 
                Subject = $"New Article in {notificationDto.Category}",
                Body = $@"
            <html>
            <body>
                <h2>{notificationDto.ArticleTitle}</h2>
                <p>Check out this new article in your preferred category <strong>{notificationDto.Category}</strong>:</p>
                <a href='{notificationDto.ArticleUrl}'>Read more</a>
            </body>
            </html>"
            };

            await SendEmail(email);
        }
    }
}
