using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MimeKit;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;
using News.Core.Settings;

namespace News.Service.Services
{
    public class UserService(IConfiguration _configuration , UserManager<ApplicationUser> _userManager) : IUserService
    {
        //public async Task<bool> SendFeedback(FeedbackModel feedbackModel)
        //{
        //    var smtpSettings = _configuration.GetSection("MailSettings");
        //    var smtpHost = smtpSettings["Host"];
        //    var smtpPort = int.Parse(smtpSettings["Port"]);
        //    var smtpUsername = smtpSettings["DisplayName"];
        //    var smtpPassword = smtpSettings["Password"];
        //    var smtpEmail = smtpSettings["Email"];

        //    var message = new MimeMessage
        //    {
        //        From = { new MailboxAddress(feedbackModel.FullName, feedbackModel.Email) },
        //        To = { new MailboxAddress(smtpUsername, "MariamShindyRoute@gmail.com") },
        //        Subject = "NewsAggregator Contact Us Form",
        //        Body = new TextPart("html")
        //        {
        //            Text = $@"
        //            <html>
        //            <body>
        //                <h2>Contact Us Form</h2>
        //                <p><strong>Subject:</strong> {feedbackModel.Subject}</p>
        //                <p><strong>Name:</strong> {feedbackModel.FullName}</p>
        //                <p><strong>Email:</strong> {feedbackModel.Email}</p>
        //                <p><strong>Message:</strong></p>
        //                <p>{feedbackModel.Message}</p>
        //            </body>
        //            </html>"
        //        }
        //    };

        //    using (var client = new SmtpClient())
        //    {
        //        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        //        await client.AuthenticateAsync(smtpEmail, smtpPassword);
        //        await client.SendAsync(message);
        //        await client.DisconnectAsync(true);
        //    }

        //    return true;
        //}

        //public async Task<ApplicationUser> GetCurrentUser(string userId)
        //{
        //    return await _userManager.FindByNameAsync(userId);
        //}

        //public async Task<IdentityResult> UpdateUser(string userId, EditUserModel model)
        //{
        //    var user = await GetCurrentUser(userId);
        //    if (user == null)
        //        return IdentityResult.Failed(); 

        //    user.FirstName = model.FirstName;
        //    user.LastName = model.LastName;
        //    user.ProfilePicUrl = model.ProfilePictureUrl;

        //    return await _userManager.UpdateAsync(user);
        //}
        public async Task<bool> SendFeedback(FeedbackDto feedbackDto)
        {
            // Map MailSettings to a strongly-typed configuration class
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
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ApplicationUser> GetCurrentUser(string userId)
        {
            return await _userManager.FindByNameAsync(userId);
        }

        public async Task<IdentityResult> UpdateUser(string userId, EditUserDto model)
        {
            var user = await GetCurrentUser(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfilePicUrl = model?.ProfilePicUrl??user.ProfilePicUrl;

            var result = await _userManager.UpdateAsync(user);
            return result;
        }

    }
}

