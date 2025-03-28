namespace News.Service.Services.NewsCatcher
{
    public class NotificationTwoService(ILogger<NotificationTwoService> _logger, IMailSettings _mailSettings,
        IUserService _userService, INewsTwoService _newsService, IMapper _mapper, 
        UserManager<ApplicationUser> _userManager , IUnitOfWork _unitOfWork)
        : INotificationService
    {

        public async Task SendNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Start Sending notifications.");
                var users = await _userService.GetAllUsersAsync();

                foreach (var user in users)
                {
                    try
                    {
                        var preferredCategories = await _userService.GetUserPreferredCategoriesAsync(user.Id);
                        var articlesByCategories = await _newsService.GetArticlesByCategoriesAsync(preferredCategories);
                        var articleToSend = articlesByCategories.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                        if (articleToSend is not null)
                        {
                            var notificationDto = new NotificationDto
                            {
                                ApplicationUserId = user.Id,
                                ArticleTitle = articleToSend.Title ?? "No title available",
                                ArticleUrl = articleToSend.Clean_Url ?? "No url available",
                                Category = articleToSend.Topic ?? "No topic available",
                                CreatedAt = DateTime.UtcNow,
                                ArticleDescription = articleToSend.Excerpt ?? "No excerpt available",
                                ArticleId = articleToSend._Id ?? "No Id available"
                            };

                            var notification = _mapper.Map<Notification>(notificationDto);

                            await _unitOfWork.Repository<Notification>().AddAsync(notification);
                            await _unitOfWork.CompleteAsync();

                            var userEmail = (await _userManager.FindByIdAsync(notificationDto.ApplicationUserId))?.Email;
                            await _mailSettings.SendNotificationEmail(notificationDto, userEmail ?? "User@gmail.com");
                            _logger.LogInformation($"Notification sent at {notificationDto.CreatedAt} to user with Email: {userEmail}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending notification to user {user.Id}");
                    }
                }
                _logger.LogInformation("Finished sending notifications.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending notifications.");
            }
        }
    }
}
    