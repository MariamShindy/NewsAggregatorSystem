using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.NewsCatcher;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Entities;
using News.Service.Helpers.EmailSettings;

namespace News.Service.Services.NewsCatcher
{
    public class NotificationTwoService(ILogger<NotificationTwoService> _logger, IMailSettings _mailSettings,
        IUserService _userService, INewsTwoService _newsService, IMapper _mapper, 
        UserManager<ApplicationUser> _userManager , IUnitOfWork _unitOfWork)
        : INotificationService
    {

        public async Task SendNotificationsAsync()
        {
            _logger.LogInformation($"Start Sending notifications.");
            var users = await _userService.GetAllUsersAsync();

            foreach (var user in users)
            {
                var preferredCategories = await _userService.GetUserPreferredCategoriesAsync(user.Id);
                var articlesByCategories = await _newsService.GetArticlesByCategoriesAsync(preferredCategories);
                var articleToSend = articlesByCategories.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (articleToSend != null)
                {
                    var notificationDto = new NotificationDto
                    {
                        ApplicationUserId = user.Id,
                        ArticleTitle = articleToSend.Title,
                        ArticleUrl = articleToSend.Clean_Url,
                        Category = articleToSend.Topic,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var notification = _mapper.Map<Notification>(notificationDto);

                    await _unitOfWork.Repository<Notification>().AddAsync(notification);
                    await _unitOfWork.CompleteAsync();
                    var userEmail = (await _userManager.FindByIdAsync(notificationDto.ApplicationUserId))?.Email;
                    await _mailSettings.SendNotificationEmail(notificationDto, userEmail ?? "User@gmail.com");
                    _logger.LogInformation($"Notification sent in {notificationDto.CreatedAt} to user with Email :{userEmail}");
                }
            }
            _logger.LogInformation("Finished sending notifications.");
        }
    }

}