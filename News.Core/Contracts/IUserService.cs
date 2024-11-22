using Microsoft.AspNetCore.Identity;
using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IUserService
    {
        Task<bool> SendFeedback(FeedbackModel feedbackModel);
        Task<ApplicationUser> GetCurrentUser(string userId);
        Task<IdentityResult> UpdateUser(string userId, EditUserModel model);
    }
}
