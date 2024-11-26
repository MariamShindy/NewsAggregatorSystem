using News.Core.Dtos;
using News.Core.Entities;

namespace News.Core.Contracts
{
    public interface INewsService
    {
        Task<string> GetAllNews(int? page, int? pageSize);
        Task<string> GetArticleById(string id);
        Task<bool> CheckArticleExists(string newsId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<bool> AddCategoryAsync(AddOrUpdateCategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> UpdateCategoryAsync(int id, AddOrUpdateCategoryDto categoryDto);

    }
}
