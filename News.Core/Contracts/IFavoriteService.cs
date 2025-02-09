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
    }
}