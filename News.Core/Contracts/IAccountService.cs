using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IAccountService
    {
        public Task<(bool isSuccess, string message)> RegisterUser(RegisterModel model);
        public Task<(bool isSuccess, string token, string message)> LoginUser(LoginModel model);
        public Task<(bool Success, string Message)> ForgotPassword(string email);
        public Task<(bool Success, string Message)> ResetPassword(string email, string token, string newPassword);


    }
}
