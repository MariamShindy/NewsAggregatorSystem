using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Entities;
using News.Service.Helpers.EmailSettings;

namespace News.Service.Services
{
    public class NotificationService(ILogger<NotificationService> _logger, IMailSettings _mailSettings,
        IUserService _userService, INewsService _newsService, IMapper _mapper, 
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
                var notifications = articlesByCategories.Select(article => new NotificationDto
                {
                    ApplicationUserId = user.Id,
                    ArticleTitle = article.Title,
                    ArticleUrl = article.Url,
                    Category = article.Category,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                foreach (var notificationDto in notifications)
                {
                    var notification = _mapper.Map<Notification>(notificationDto);

                    await _unitOfWork.Repository<Notification>().AddAsync(notification);
                    await _unitOfWork.CompleteAsync();
                    var userEmail = (await _userManager.FindByIdAsync(notificationDto.ApplicationUserId))?.Email;
                    await _mailSettings.SendNotificationEmail(notificationDto, userEmail??"User@gmail.com");
                    _logger.LogInformation($"Notification sent in {notificationDto.CreatedAt} to uder with Email :{userEmail}");
                }

            }
            _logger.LogInformation("Finished sending notifications.");
        }
    }

}