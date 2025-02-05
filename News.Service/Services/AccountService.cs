using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
    public class AccountService(ILogger<AccountService> _logger , IMemoryCache _memoryCache , ImageUploader _imageUploader, 
        UserManager<ApplicationUser> _userManager, IConfiguration _configuration,
        IMailSettings _mailSettings) : IAccountService
    {
        public async Task<(bool isSuccess, string message , string? token)> RegisterUserAsync(RegisterModel model)
        {
            _logger.LogInformation("AccountService --> RegisterUser called");
            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("AccountService --> RegisterUser --> Passwords do not match!");
                return (false, "Password and Confirm Password do not match!", null);
            }
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                _logger.LogWarning("AccountService --> RegisterUser --> User already exists! ");
                return (false, "User already exists!" ,null);
            }

            string? profilePicUrl = null;
            if (model.ProfilePicUrl is not null)
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
            var role = !_userManager.Users.Any() ? "Admin" : "User";
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, result.Errors.FirstOrDefault()?.Description ?? "User creation failed" , null);
            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("AccountService --> RegisterUser succeeded");
            var token = GenerateJwtToken(user);
            return (true, "User created successfully!" , token);
        }
        public async Task<(bool isSuccess, string token, string message)> LoginUserAsync(LoginModel model)
        {
            _logger.LogInformation("AccountService --> LoginUser called");

            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                _logger.LogWarning("AccountService --> LoginUser --> Account does not exist");
                return (false, null, "Account does not exist.")!;
            }
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("AccountService --> LoginUser --> Account locked");
                return (false, null, "Account locked.")!;
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                _logger.LogInformation("AccountService --> LoginUser succeeded");
                return (true, token, "Login successful");
            }
            _logger.LogWarning("AccountService --> LoginUser --> Invalid credentials");
            return (false, null, "Invalid credentials")!;
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

            var verificationCode = new Random().Next(100000, 999999).ToString(); 

            _memoryCache.Set(email, verificationCode, TimeSpan.FromMinutes(15)); 

            var emailBody = $"Your password reset verification code is: {verificationCode}";

            var emailToSend = new Email
            {
                To = email,
                Subject = "Password Reset Verification Code",
                Body = emailBody
            };

            await _mailSettings.SendEmail(emailToSend);
            _logger.LogInformation("AccountService --> ForgotPassword --> Verification code has been sent to user email");

            return (true, "Verification code sent to your email."); 
        }
        public async Task<(bool Success, string Message)> ValidateVerificationCodeAsync(string email, string verificationCode)
        {
            _logger.LogInformation("AccountService --> ValidateVerificationCode called");

            if (!_memoryCache.TryGetValue(email, out string storedCode) || storedCode != verificationCode)
            {
                _logger.LogWarning("AccountService --> ValidateVerificationCode --> Invalid or expired verification code");
                return (false, "Invalid or expired verification code.");
            }

            _logger.LogInformation("AccountService --> ValidateVerificationCode --> Verification code is valid");
            return (true, "Verification code is valid.");
        }

        public async Task<(bool Success, string Message , string? Token)> ResetPasswordAsync(string email, string verificationCode, string newPassword)
        {
            _logger.LogInformation("AccountService --> ResetPassword called");

            var validateResult = await ValidateVerificationCodeAsync(email, verificationCode);
            if (!validateResult.Success)
                return (validateResult.Success, validateResult.Message, null);  

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("AccountService --> ResetPassword --> user not found");
                return (false, "User not found.",null);
            }

            var resetPasswordtoken =  await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordtoken, newPassword);

            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(", ", resetPassResult.Errors.Select(e => e.Description));
                _logger.LogWarning("AccountService --> ResetPassword failed");
                return (false, "Password reset failed: " + errors,null);
            }
            var token = GenerateJwtToken(user);
            _logger.LogInformation("AccountService --> ResetPassword succeeded");
            return (true, "Password reset successful.",token);
        }
        private string GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogInformation("AccountService --> GenerateJwtToken called");

            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user?.UserName??""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var userRoles = _userManager.GetRolesAsync(user!).Result;
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

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
    }
}

