using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Entities;

namespace News.Service.Services
{
    public class FavoriteService(IUnitOfWork _unitOfWork) : IFavoriteService
	{

		public async Task Add(UserFavoriteArticle favorite)
		{  
			_unitOfWork.Repository<UserFavoriteArticle>().Add(favorite);
			await _unitOfWork.CompleteAsync();
		}

		public async Task<IEnumerable<UserFavoriteArticle>> GetFavoritesByUser(string userId)
		{
            var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();

            return favorites.Where(f => f.UserId == userId).ToList();
        }

		public async Task Remove(int favoriteId)
		{
			var favorite = await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
			if (favorite != null)
			{
				_unitOfWork.Repository<UserFavoriteArticle>().Delete(favorite);
                await _unitOfWork.CompleteAsync();
            }
        }
        public async Task<bool> IsArticleFavorited(string userId, string newsId)
        {
            var userFavorites = await GetFavoritesByUser(userId);
            return userFavorites.Any(f => f.ArticleId == newsId);
        }
        public async Task<UserFavoriteArticle> GetFavoriteById(int favoriteId)
        {
            return await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
        }

    }
}

