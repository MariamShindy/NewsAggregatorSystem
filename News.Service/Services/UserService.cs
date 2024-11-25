using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;
using News.Core.Settings;
using System.Security.Claims;

namespace News.Service.Services
{
    public class UserService(IHttpContextAccessor _httpContextAccessor, IConfiguration _configuration , UserManager<ApplicationUser> _userManager) : IUserService
    {
        public async Task<bool> SendFeedback(FeedbackDto feedbackDto)
        {
            var smtpSettings = _configuration.GetSection("MailSettings").Get<MailSettings>();
            if (smtpSettings == null)
                return false;

            var message = new MimeMessage
            {
                From = { new MailboxAddress(feedbackDto.FullName, feedbackDto.Email) },
                To = { new MailboxAddress(smtpSettings.DisplayName, "MariamShindyRoute@gmail.com") },
                Subject = "NewsAggregator Contact Us Form",
                Body = new TextPart("html")
                {
                    Text = $@"
                    <html>
                    <body>
                        <h2>Contact Us Form</h2>
                        <p><strong>Subject:</strong> {feedbackDto.Subject}</p>
                        <p><strong>Name:</strong> {feedbackDto.FullName}</p>
                        <p><strong>Email:</strong> {feedbackDto.Email}</p>
                        <p><strong>Message:</strong></p>
                        <p>{feedbackDto.Message}</p>
                    </body>
                    </html>"
                }
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpSettings.Host, smtpSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpSettings.Email, smtpSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<ApplicationUser> GetCurrentUser()
        {
            var currentUserName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserName))
                throw new UnauthorizedAccessException("No user is logged in.");
            var currentUser = await _userManager.FindByNameAsync(currentUserName);
            if (currentUser is null)
                 throw new InvalidOperationException("The user was not found.");
            return currentUser;
        }
        public async Task<IdentityResult> UpdateUser(EditUserDto model)
        {
            var user = await GetCurrentUser();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfilePicUrl = model?.ProfilePicUrl??user.ProfilePicUrl;
            var result = await _userManager.UpdateAsync(user);
            return result;
        }
    }
}

