using News.Core.Entities;

namespace News.Service.Helpers.EmailSettings
{
    public interface IMailSettings
    {
        public Task SendEmail(Email email);
    }
}
