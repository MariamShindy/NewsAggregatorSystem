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
            //var users = await _userService.GetAllUsersAsync();
            var users = (await _userService.GetAllUsersAsync())
           .Where(u => !u.IsPendingDeletion && !u.DeletionRequestedAt.HasValue) 
           .ToList();
            if (users.Any())
            {
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
                            ArticleUrl = articleToSend.Url,
                            Category = articleToSend.Category,
                            CreatedAt = DateTime.UtcNow
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

}