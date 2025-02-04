using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Entities;
using News.Service.Helpers.EmailSettings;
using News.Service.Helpers.ImageUploader;
using System.Security.Claims;

namespace News.Service.Services
{
    public class UserService(IHttpContextAccessor _httpContextAccessor,
        IMapper _mapper, ImageUploader _imageUploader, IUnitOfWork _unitOfWork,
        ILogger<UserService> _logger, IMailSettings _mailSettings,
        UserManager<ApplicationUser> _userManager) : IUserService
    {
        public async Task<List<UserPreferencesDto>> GetUsersPreferencesAsync()
        {
            var users = await _unitOfWork.Repository<ApplicationUser>().GetAllAsync();
            return users.Select(u => new UserPreferencesDto
            {
                UserId = u.Id,
                PreferredCategories = u.Categories.Select(c => c.Name).ToList()
            }).ToList();
        }
        public async Task<bool> SendFeedbackAsync(FeedbackDto feedbackDto)
        {
            _logger.LogInformation("UserService --> SendFeedback called");

            var email = new Email
            {
                To = "MariamShindyRoute@gmail.com",
                Subject = "NewsAggregator Contact Us Form",
                Body = BuildFeedbackEmailBody(feedbackDto)
            };
            try
            {
                await _mailSettings.SendEmail(email);
                _logger.LogInformation("UserService --> SendFeedback succeeded");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserService --> SendFeedback failed");
                return false;
            }
        }
        public async Task<bool> SendSurveyAsync(SurveyDto surveyDto)
        {
            _logger.LogInformation("UserService --> SendSurveyAsync called");

            var email = new Email
            {
                To = "MariamShindyRoute@gmail.com",
                Subject = "NewsAggregator Survey Form",
                Body = BuildSurveyEmailBody(surveyDto)
            };
            try
            {
                await _mailSettings.SendEmail(email);
                _logger.LogInformation("UserService --> SendSurveyAsync succeeded");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserService --> SendSurveyAsync failed");
                return false;
            }
        }
        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            _logger.LogInformation("UserService --> GetCurrentUser called");
            var currentUserName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserName))
                throw new UnauthorizedAccessException("No user is logged in.");
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
                if (editUserDto.Password != editUserDto.ConfirmPassword)
                {
                    _logger.LogWarning("Passwords do not match.");
                    return IdentityResult.Failed(new IdentityError { Description = "Password and ConfirmPassword do not match." });
                }

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
            user.FirstName = editUserDto.FirstName ?? user.FirstName;
            user.LastName = editUserDto.LastName ?? user.LastName;
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
        public async Task<IEnumerable<CategoryDto>> GetUserPreferredCategoriesAsync(string userId)
        {
            _logger.LogInformation($"UserService --> GetUserPreferredCategoriesAsync for User with id {userId} called");

            var user = await _userManager.Users.Include(u => u.Categories).FirstOrDefaultAsync(u => u.Id == userId);
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
                //var userDtos = _mapper.Map<List<UserDto>>(users);
                if (users is not null)
                {
                    var userDtos = users.Select(user => new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        ProfilePicUrl = user.ProfilePicUrl,
                        IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                    }).ToList();
                    return userDtos;
                }
                else
                    return new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while getting users: {ex.Message}");
                return new List<UserDto>();
            }
        }

        private string BuildFeedbackEmailBody(FeedbackDto feedbackDto)
        {
            var Body = $@"
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
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>Contact Us Form</h2>
                <div class='content'>
                    <p><strong>Subject:</strong> {feedbackDto.Subject}</p>
                    <p><strong>Name:</strong> {feedbackDto.FullName}</p>
                    <p><strong>Email:</strong> {feedbackDto.Email}</p>
                    <p><strong>Message:</strong></p>
                    <p>{feedbackDto.Message}</p>
                </div>
                <div class='footer'>
                    <p>This email was sent via the NewsAggregator Contact Us form.</p>
                </div>
            </div>
        </body>
        </html>";
            return Body;
        }

        private string BuildSurveyEmailBody(SurveyDto surveyDto)
        {
            var Body = $@"
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
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>Survey Form</h2>
                <div class='content'>
                    <p><strong>How did you find out about our news website?</strong><br>{surveyDto.SourceDiscovery}</p>
                    <p><strong>How often do you visit news websites?</strong><br>{surveyDto.VisitFrequency}</p>
                    <p><strong>Is the website loading speed satisfactory?</strong><br>{surveyDto.IsLoadingSpeedSatisfactory}</p>
                    <p><strong>How easy is it to navigate our website?</strong><br>{surveyDto.NavigationEaseRating}</p>
                </div>
                <div class='footer'>
                    <p>This email was sent via the NewsAggregator Survey Form.</p>
                </div>
            </div>
        </body>
        </html>";
            return Body;
        }
    }
}

