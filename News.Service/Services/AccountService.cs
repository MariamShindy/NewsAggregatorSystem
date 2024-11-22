using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using News.Core.Contracts;
using News.Core.Entities;
using News.Service.Helpers.EmailSettings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace News.Service.Services
{
    public class AccountService(UserManager<ApplicationUser> _userManager, IUrlHelperFactory _urlHelper, IHttpContextAccessor _httpContextAccessor, SignInManager<ApplicationUser> _signInManager, IConfiguration _configuration, IMailSettings _mailSettings) : IAccountService
    {
        public async Task<(bool isSuccess, string message)> RegisterUser(RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return (false, "User already exists!");

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfilePicUrl = model.ProfilePicUrl
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, result.Errors.FirstOrDefault()?.Description ?? "User creation failed");

            var role = !_userManager.Users.Any() ? "Admin" : "User";
            await _userManager.AddToRoleAsync(user, role);

            return (true, "User created successfully!");
        }
        public async Task<(bool isSuccess, string token, string message)> LoginUser(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null || await _userManager.IsLockedOutAsync(user))
            {
                return (false, null, "Account is locked or does not exist.");
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                return (true, token, "Login successful");
            }

            return (false, null, "Invalid credentials");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<(bool Success, string Message)> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.");

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
            return (true, "Password reset link has been sent to your email.");
        }

        //Should decode the token before use
        public async Task<(bool Success, string Message)> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found.");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(", ", resetPassResult.Errors.Select(e => e.Description));
                return (false, "Password reset failed.");
            }

            return (true, "Password reset successful.");
        }
    }
}

