﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Entities;
using News.Core.Settings;
using System.Security.Claims;

namespace News.Service.Services
{
    public class UserService(IHttpContextAccessor _httpContextAccessor ,IUnitOfWork _unitOfWork, ILogger<UserService> _logger ,IConfiguration _configuration , UserManager<ApplicationUser> _userManager) : IUserService
    {
        public async Task<bool> SendFeedback(FeedbackDto feedbackDto)
        {
            _logger.LogInformation("UserService --> SendFeedback called");
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
                _logger.LogInformation("UserService --> SendFeedback succeeded");
                return true;
            }
            catch (Exception)
            {
                _logger.LogError("UserService --> SendFeedback failed");
                return false;
            }
        }
        public async Task<ApplicationUser> GetCurrentUser()
        {
            _logger.LogInformation("UserService --> GetCurrentUser called");
            var currentUserName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserName))
                throw new UnauthorizedAccessException("No user is logged in.");
            //var currentUser = await _userManager.FindByNameAsync(currentUserName);
            var currentUser = await _userManager.Users
            .Include(u => u.Categories)  
            .FirstOrDefaultAsync(u => u.UserName == currentUserName);
            if (currentUser is null)
                 throw new InvalidOperationException("The user was not found.");
            _logger.LogInformation("UserService --> GetCurrentUser succeeded");
            return currentUser;
        }
        public async Task<IdentityResult> UpdateUser(EditUserDto model)
        {
            _logger.LogInformation("UserService --> UpdateUser called");
            var user = await GetCurrentUser();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfilePicUrl = model?.ProfilePicUrl??user.ProfilePicUrl;
            var result = await _userManager.UpdateAsync(user);
            _logger.LogInformation("UserService --> UpdateUser succeeded");
            return result;
        }
        public async Task SetUserPreferredCategories(ApplicationUser user, List<string> categoryNames)
        {
            var categories = await _unitOfWork.Repository<Category>()
                .Find(c => categoryNames.Contains(c.Name));

            if (categories.Count() != categoryNames.Count)
            {
                throw new ArgumentException("One or more category names are invalid.");
            }

            user.Categories.Clear();
            foreach (var category in categories)
            {
                user.Categories.Add(category);
            }

            await _unitOfWork.CompleteAsync();
        }
        public async Task<IEnumerable<CategoryDto>> GetUserPreferredCategories()
        {
            var user = await GetCurrentUser();
            if (user == null)
                throw new ArgumentException("User not found.");

            return user.Categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });
        }

    }
}

