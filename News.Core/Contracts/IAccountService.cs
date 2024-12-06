﻿using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IAccountService
    {
        Task<(bool isSuccess, string message, string? token)> RegisterUserAsync(RegisterModel model);
        Task<(bool isSuccess, string token, string message)> LoginUserAsync(LoginModel model);
        Task<(bool Success, string Message)> ValidateVerificationCodeAsync(string email, string verificationCode);
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
        Task<(bool Success, string Message, string? Token)> ResetPasswordAsync(string email, string verificationCode, string newPassword);
        Task<bool> CheckAdminRoleAsync(ApplicationUser currentUser);
    }
}
