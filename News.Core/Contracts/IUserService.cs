using Microsoft.AspNetCore.Identity;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUser();
        Task<bool> SendFeedback(FeedbackDto feedbackModel);
        Task<IdentityResult> UpdateUser(EditUserDto model);
    }

}
