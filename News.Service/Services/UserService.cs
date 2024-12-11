using AutoMapper;
using MailKit.Net.Smtp;
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
using News.Service.Helpers.ImageUploader;
using System.Security.Claims;

namespace News.Service.Services
{
    public class UserService(IHttpContextAccessor _httpContextAccessor,IMapper _mapper ,ImageUploader _imageUploader,IUnitOfWork _unitOfWork, ILogger<UserService> _logger ,IConfiguration _configuration , UserManager<ApplicationUser> _userManager) : IUserService
    {
        public async Task<bool> SendFeedbackAsync(FeedbackDto feedbackDto)
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
        public async Task<bool> SendSurveyAsync(SurveyDto surveyDto)
        {
            _logger.LogInformation("UserService --> SendSurveyAsync called");
            var smtpSettings = _configuration.GetSection("MailSettings").Get<MailSettings>();
            if (smtpSettings == null)
                return false;
            var currentUser = await GetCurrentUserAsync();
            var message = new MimeMessage
            {
                From = { new MailboxAddress(currentUser.UserName, currentUser.Email) },
                To = { new MailboxAddress(smtpSettings.DisplayName, "MariamShindyRoute@gmail.com") },
                Subject = "NewsAggregator Survey Form",
                Body = new TextPart("html")
                {
                    Text = $@"
                    <html>
                    <body>
                        <h2>Survey Form</h2>
                        <p><strong>How did you find out about our news website?</strong><br> {surveyDto.SourceDiscovery}</p>
                        <p><strong>How often do you visit news websites?</strong><br> {surveyDto.VisitFrequency}</p>
                        <p><strong>Is the website loading speed satisfactory?</strong><br> {surveyDto.IsLoadingSpeedSatisfactory}</p>
                        <p><strong>How easy is it to navigate our website?</strong><br>{surveyDto.NavigationEaseRating}</p>
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
                _logger.LogInformation("UserService --> SendSurveyAsync succeeded");
                return true;
            }
            catch (Exception)
            {
                _logger.LogError("UserService --> SendSurveyAsync failed");
                return false;
            }
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
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
        public async Task<IdentityResult> UpdateUserAsync(EditUserDto editUserDto)
        {
            _logger.LogInformation("UserService --> UpdateUser called");
            var user = await GetCurrentUserAsync();
            if (!string.IsNullOrWhiteSpace(editUserDto.Username) && editUserDto.Username != user.UserName)
            {
                var existingUserWithUsername = await _userManager.FindByNameAsync(editUserDto.Username);
                if (existingUserWithUsername != null)
                {
                    _logger.LogWarning("Username already exists.");
                    return IdentityResult.Failed(new IdentityError { Description = "Username already exists." });
                }
                user.UserName = editUserDto.Username;
            }
            if (!string.IsNullOrWhiteSpace(editUserDto.Email) && editUserDto.Email != user.Email)
            {
                var existingUserWithEmail = await _userManager.FindByEmailAsync(editUserDto.Email);
                if (existingUserWithEmail != null)
                {
                    _logger.LogWarning("Email already exists.");
                    return IdentityResult.Failed(new IdentityError { Description = "Email already exists." });
                }
                user.Email = editUserDto.Email;
            }
            if (!string.IsNullOrWhiteSpace(editUserDto.Password))
            {
                var passwordValidator = new PasswordValidator<ApplicationUser>();
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, editUserDto.Password);
                if (!passwordValidationResult.Succeeded)
                {
                    _logger.LogWarning("Password validation failed.");
                    return passwordValidationResult;
                }
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, editUserDto.Password);
            }
            if (editUserDto.ProfilePicUrl != null && editUserDto.ProfilePicUrl.Length > 0)
            {
                var imagePath = await _imageUploader.UploadProfileImageAsync(editUserDto.ProfilePicUrl);
                user.ProfilePicUrl = imagePath;
            }
            user.FirstName = editUserDto.FirstName??user.FirstName;
            user.LastName = editUserDto.LastName??user.LastName;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                _logger.LogInformation("UserService --> UpdateUser succeeded");
            else
                _logger.LogWarning("UserService --> UpdateUser failed");
            return result;
        }
        public async Task SetUserPreferredCategoriesAsync(ApplicationUser user, ICollection<string> categoryNames)
        {
            _logger.LogInformation("UserService --> SetUserPreferredCategoriesAsync called");
            var categories = await _unitOfWork.Repository<Category>()
                .FindAsync(c => categoryNames.Contains(c.Name));

            if (categories.Count() != categoryNames.Count)
                throw new ArgumentException("One or more category names are invalid.");

            user.Categories.Clear();
            foreach (var category in categories)
                user.Categories.Add(category);

            await _unitOfWork.CompleteAsync();
        }
        public async Task<IEnumerable<CategoryDto>> GetUserPreferredCategoriesAsync()
        {
            _logger.LogInformation("UserService --> GetUserPreferredCategoriesAsync called");

            var user = await GetCurrentUserAsync();
            if (user == null)
                throw new ArgumentException("User not found.");

            return user.Categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("UserService --> GetAllUsersAsync called");

            try
            {
                var users = await _userManager.GetUsersInRoleAsync("User");
                var userDtos = _mapper.Map<List<UserDto>>(users); 
                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting users: {ex.Message}");
                return new List<UserDto>();
            }
        }
    }
}

