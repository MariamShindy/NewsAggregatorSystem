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
        public async Task SendNotificationEmail(NotificationDto notificationDto, string userEmail)
        {
            var email = new Email
            {
                To = userEmail,
                Subject = $"New Article in {notificationDto.Category}",
                Body = BuildNotificationEmailBody(notificationDto, userEmail)
            };
            await SendEmail(email);
        }
        private string BuildNotificationEmailBody(NotificationDto notificationDto, string userEmail)
        {
            var body = $@"
               <html>
               <head>
                   <style>
                       body {{
                           font-family: Arial, sans-serif;
                           line-height: 1.6;
                           color: #333333;
                       }}
                       h2 {{
                           color: #007BFF;
                       }}
                       .container {{
                           border: 1px solid #dddddd;
                           padding: 20px;
                           border-radius: 10px;
                           background-color: #f9f9f9;
                           max-width: 600px;
                           margin: 20px auto;
                       }}
                       .content {{
                           margin-bottom: 15px;
                       }}
                       .footer {{
                           font-size: 0.9em;
                           color: #555555;
                           margin-top: 20px;
                           border-top: 1px solid #dddddd;
                           padding-top: 10px;
                           text-align: center;
                       }}
                       .link {{
                           display: inline-block;
                           background-color: #007BFF;
                           color: white;
                           text-decoration: none;
                           padding: 10px 20px;
                           border-radius: 5px;
                           margin-top: 10px;
                       }}
                       .link:hover {{
                           background-color: #0056b3;
                           color: white;
                       }}
                      
                   </style>
               </head>
               <body>
                   <div class='container'>
                       <h2>New Article Alert: {notificationDto.Category}</h2>
                       <div class='content'>
                           <p>Dear User,</p>
                           <p>We have a new article in your preferred category <strong>{notificationDto.Category}</strong>:</p>
                           <p><strong>{notificationDto.ArticleTitle}</strong></p>
                           <a href='{notificationDto.ArticleUrl}' class='link'>Read More</a>
                       </div>
                       <div class='footer'>
                           <p>This email was sent to notify you about updates in your preferred category.</p>
                           <p>Thank you for using our service!</p>
                       </div>
                   </div>
               </body>
               </html>";
            return body;
        }
    }
}
