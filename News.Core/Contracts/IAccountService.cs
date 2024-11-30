using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IAccountService
    {
        Task<(bool isSuccess, string message)> RegisterUserAsync(RegisterModel model);
        Task<(bool isSuccess, string token, string message)> LoginUserAsync(LoginModel model);
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> CheckAdminRoleAsync(ApplicationUser currentUser);


    }
}
