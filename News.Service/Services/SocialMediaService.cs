using News.Core.Contracts;
using News.Core.Contracts.NewsCatcher;

namespace News.Service.Services
{
    public class SocialMediaService(INewsTwoService _newsService) : ISocialMediaService
    {
        //private const string ArticleUrlTemplate = "https://localhost:7291/newsTwo/{0}";
        public Dictionary<string, string> GenerateShareLinks(string newsId, string platform = null, string customMessage = null)
        {
            //var baseUrl = string.Format(ArticleUrlTemplate, newsId);
            var baseUrl = _newsService.GetNewsByIdAsync(newsId).Result.Link;
            var shareLinks = new Dictionary<string, string>();

            shareLinks["facebook"] = $"https://www.facebook.com/sharer/sharer.php?quote={Uri.EscapeDataString(customMessage ?? "Check this article out!")}&u={Uri.EscapeDataString(baseUrl)}";
            shareLinks["whatsapp"] = $"https://api.whatsapp.com/send?text={Uri.EscapeDataString(customMessage ?? "Check this article out!")}%20{Uri.EscapeDataString(baseUrl)}";
            shareLinks["twitter"] = $"https://twitter.com/intent/tweet?text={Uri.EscapeDataString(customMessage ?? "Check this article out!")}&url={Uri.EscapeDataString(baseUrl)}";

            if (platform != null && shareLinks.ContainsKey(platform.ToLower()))
            {
                return new Dictionary<string, string>
            {
                { platform.ToLower(), shareLinks[platform.ToLower()] }
            };
            }

            return shareLinks;
        }

    }
}
