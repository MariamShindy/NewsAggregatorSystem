using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IAccountService
    {
        Task<(bool isSuccess, string message)> RegisterUser(RegisterModel model);
        Task<(bool isSuccess, string token, string message)> LoginUser(LoginModel model);
        Task<(bool Success, string Message)> ForgotPassword(string email);
        Task<(bool Success, string Message)> ResetPassword(string email, string token, string newPassword);
        Task<bool> CheckAdminRole(ApplicationUser currentUser);


    }
}
