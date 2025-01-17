using News.Core.Dtos;
using News.Core.Entities;

namespace News.Service.Helpers.EmailSettings
{
    public interface IMailSettings
    {
        Task SendEmail(Email email);
        Task SendNotificationEmail(NotificationDto notificationDto , string userEmail);
    }
}
