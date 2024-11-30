using News.Core.Dtos;
using News.Core.Entities;

namespace News.Core.Contracts
{
	public interface IFavoriteService
	{
        //Task Add(UserFavoriteArticle favorite);
        //Task<IEnumerable<UserFavoriteArticle>> GetFavoritesByUser(string userId);
        //Task Remove(int favoriteId);
        //public Task<bool> IsArticleFavorited(string userId, string newsId);
        //public Task<UserFavoriteArticle> GetFavoriteById(int favoriteId);

        Task AddToFavorites(string userId, string articleId);
        //Task<IEnumerable<string>> GetFavoritesByUser(string userId);
        Task<IEnumerable<ArticleDto>> GetFavoritesByUser(string userId);

        Task RemoveFromFavorites(string userId, string articleId);
        Task<bool> IsArticleFavorited(string userId, string articleId);
        Task<UserFavoriteArticle> GetFavoriteById(int favoriteId);

    }
}
