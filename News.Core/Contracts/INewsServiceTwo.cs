using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface INewsServiceTwo
    {
        Task<List<NewsArticle>> GetAllNewsAsync(string language = "en", string country = "us");
        Task<List<NewsArticle>> GetNewsByCategoryAsync(string category, string language = "en", string country = "us");
        Task<NewsArticle> GetNewsByIdAsync(string id);
        Task<List<string>> GetCategoriesAsync();
    }
}
