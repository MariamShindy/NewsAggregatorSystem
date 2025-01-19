using News.Core.Entities.NewsCatcher;

namespace News.Core.Contracts.NewsCatcher
{
    public interface INewsTwoService
    {
        Task<List<NewsArticle>> GetAllNewsAsync(string language = "en", string country = "us");
        Task<List<NewsArticle>> GetNewsByCategoryAsync(string category, string language = "en", string country = "us");
        Task<NewsArticle> GetNewsByIdAsync(string id);
        Task<List<string>> GetCategoriesAsync();
    }
}
