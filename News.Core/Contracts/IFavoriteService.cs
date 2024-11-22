using News.Core.Entities;

namespace News.Core.Contracts
{
	public interface IFavoriteService
	{
		Task Add(UserFavoriteArticle favorite);
		Task<IEnumerable<UserFavoriteArticle>> GetFavoritesByUser(string userId);
		Task Remove(int favoriteId);
		public Task<bool> IsArticleFavorited(string userId, string newsId);
		public Task<UserFavoriteArticle> GetFavoriteById(int favoriteId);

    }
}
