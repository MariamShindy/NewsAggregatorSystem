using News.Core.Dtos;
using News.Core.Entities;

namespace News.Core.Contracts
{
	public interface IFavoriteService
	{
        Task AddToFavoritesAsync(string userId, string articleId);
        Task<IEnumerable<ArticleDto>> GetFavoritesByUserAsync(string userId);

        Task RemoveFromFavoritesAsync(string userId, string articleId);
        Task<bool> IsArticleFavoritedAsync(string userId, string articleId);
        Task<UserFavoriteArticle> GetFavoriteByIdAsync(int favoriteId);
        //Task Add(UserFavoriteArticle favorite);
        //Task<IEnumerable<UserFavoriteArticle>> GetFavoritesByUser(string userId);
        //Task Remove(int favoriteId);
        //public Task<bool> IsArticleFavorited(string userId, string newsId);
        //public Task<UserFavoriteArticle> GetFavoriteById(int favoriteId);
        //Task<IEnumerable<string>> GetFavoritesByUser(string userId);
    }
}