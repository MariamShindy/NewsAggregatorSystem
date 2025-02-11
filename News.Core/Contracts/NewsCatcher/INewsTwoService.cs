using News.Core.Dtos.NewsCatcher;
using News.Core.Dtos;
using News.Core.Entities.NewsCatcher;
using News.Core.Entities;

namespace News.Core.Contracts.NewsCatcher
{
    public interface INewsTwoService
    {
        //use the json file
        Task<List<NewsArticle>> GetAllNewsAsync(int pageNumber = 1, int? pageSize = null, string language = "en", string country = "us");
        //Use tha api
        //Task<List<NewsArticle>> GetAllNewsAsync(string language = "en", string country = "us");
        Task<List<NewsArticle>> GetNewsByCategoryAsync(string category, string language = "en", string country = "us");
        Task<NewsArticle> GetNewsByIdAsync(string id);
        Task<List<string>> GetCategoriesAsync();
        Task<IEnumerable<NewsArticleDto>> GetArticlesByCategoriesAsync(IEnumerable<CategoryDto> preferredCategories);
        byte[] GenerateArticlePdf(NewsArticle article);

    }
}
