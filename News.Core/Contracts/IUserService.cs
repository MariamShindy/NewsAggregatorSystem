using Microsoft.AspNetCore.Identity;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();
        Task<bool> SendFeedbackAsync(FeedbackDto feedbackDto);
        public Task<bool> SendSurveyAsync(SurveyDto surveyDto);

        Task<IdentityResult> UpdateUserAsync(EditUserDto editUserDto);
        Task SetUserPreferredCategoriesAsync(ApplicationUser user, List<string> categoryNames);
        Task<IEnumerable<CategoryDto>> GetUserPreferredCategoriesAsync();
    }
}
