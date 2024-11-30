using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using News.Core.Contracts;
using News.Core.Entities;
using News.Service.Helpers.EmailSettings;
using News.Service.Helpers.ImageUploader;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace News.Service.Services
{
    public class AccountService(ILogger<AccountService> _logger ,ImageUploader _imageUploader, UserManager<ApplicationUser> _userManager, IUrlHelperFactory _urlHelper, IHttpContextAccessor _httpContextAccessor, SignInManager<ApplicationUser> _signInManager, IConfiguration _configuration, IMailSettings _mailSettings) : IAccountService
    {
        public async Task<(bool isSuccess, string message)> RegisterUserAsync(RegisterModel model)
        {
            _logger.LogInformation("AccountService --> RegisterUser called");

            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                _logger.LogWarning("AccountService --> RegisterUser --> User already exists! ");
                return (false, "User already exists!");
            }

            string profilePicUrl = null;
            if (model.ProfilePicUrl != null)
            {
                profilePicUrl = await _imageUploader.UploadProfileImageAsync(model.ProfilePicUrl);
                _logger.LogInformation("AccountService --> ImageUploader succeeded");
            }
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfilePicUrl = profilePicUrl
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, result.Errors.FirstOrDefault()?.Description ?? "User creation failed");

            var role = !_userManager.Users.Any() ? "Admin" : "User";
            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("AccountService --> RegisterUser succeeded");
            return (true, "User created successfully!");
        }
        public async Task<(bool isSuccess, string token, string message)> LoginUserAsync(LoginModel model)
        {
            _logger.LogInformation("AccountService --> LoginUser called");

            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null || await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("AccountService --> LoginUser --> Account is locked or does not exist");
                return (false, null, "Account is locked or does not exist.");
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                _logger.LogInformation("AccountService --> LoginUser succeeded");
                return (true, token, "Login successful");
            }
            _logger.LogWarning("AccountService --> LoginUser --> Invalid credentials");
            return (false, null, "Invalid credentials");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogInformation("AccountService --> GenerateJwtToken called");

            var authClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            _logger.LogInformation("AccountService --> GenerateJwtToken succeeded");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            _logger.LogInformation("AccountService --> ForgotPassword called");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("AccountService --> ForgotPassword --> user not found");
                return (false, "User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var actionContext = new ActionContext(_httpContextAccessor.HttpContext, _httpContextAccessor.HttpContext.GetRouteData(), new ActionDescriptor());
            var urlHelper = _urlHelper.GetUrlHelper(actionContext);

            var resetLink = urlHelper.Action("ResetPassword", "Account", new { token, email }, _httpContextAccessor.HttpContext.Request.Scheme);
            var emailBody = $"Please reset your password by clicking this link: {resetLink}";

            var emailToSend = new Email
            {
                To = email,
                Subject = "Reset password",
                Body = emailBody
            };

            await _mailSettings.SendEmail(emailToSend);
            _logger.LogInformation("AccountService --> ForgotPassword --> Password reset link has been sent to user email");

            return (true, "Password reset link has been sent to your email.");
        }

        //Should decode the token before use
        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            _logger.LogInformation("AccountService --> ResetPassword called");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("AccountService --> ResetPassword --> user not found");

                return (false, "User not found.");
            }
            var resetPassResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(", ", resetPassResult.Errors.Select(e => e.Description));
                _logger.LogWarning("AccountService --> ResetPassword failed");
                return (false, "Password reset failed.");
            }
            _logger.LogInformation("AccountService --> ResetPassword succeeded");
            return (true, "Password reset successful.");
        }
        public async Task<bool> CheckAdminRoleAsync(ApplicationUser currentUser)
        {
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (roles.Contains("Admin"))
                return true;
            else
                return false;
        }

    }
}

