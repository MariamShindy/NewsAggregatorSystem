namespace News.Service.Services
{
    public class SocialMediaService(INewsTwoService _newsService) : ISocialMediaService
    {
        //private const string ArticleUrlTemplate = "https://localhost:7291/newsTwo/{0}";
        public Dictionary<string, string> GenerateShareLinks(string newsId, string platform = null!)
        {
            //var baseUrl = string.Format(ArticleUrlTemplate, newsId);
            var baseUrl = _newsService.GetNewsByIdAsync(newsId).Result.Link;
            var shareLinks = new Dictionary<string, string>();

            shareLinks["facebook"] = $"https://www.facebook.com/sharer/sharer.php?u={Uri.EscapeDataString(baseUrl)}";
            shareLinks["whatsapp"] = $"https://api.whatsapp.com/send?text={Uri.EscapeDataString(baseUrl)}";
            shareLinks["twitter"] = $"https://twitter.com/intent/tweet?url={Uri.EscapeDataString(baseUrl)}";

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
