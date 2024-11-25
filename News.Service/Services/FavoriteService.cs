using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Entities;

namespace News.Service.Services
{
    public class FavoriteService(ILogger<FavoriteService> _logger , IUnitOfWork _unitOfWork) : IFavoriteService
	{
		public async Task Add(UserFavoriteArticle favorite)
		{  
            _logger.LogInformation("FavoriteService --> Add called");
            _unitOfWork.Repository<UserFavoriteArticle>().Add(favorite);
			await _unitOfWork.CompleteAsync();
            _logger.LogInformation("FavoriteService --> Add succeeded");
        }

        public async Task<IEnumerable<UserFavoriteArticle>> GetFavoritesByUser(string userId)
		{
            _logger.LogInformation($"FavoriteService --> GetFavoritesByUser with userId : {userId} called");
            var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
            _logger.LogInformation($"FavoriteService --> GetFavoritesByUser with userId : {userId} succeeded");
            return favorites.Where(f => f.UserId == userId).ToList();
        }

		public async Task Remove(int favoriteId)
		{
            _logger.LogInformation($"FavoriteService --> Remove with favoriteId : {favoriteId} called");

            var favorite = await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
			if (favorite != null)
			{
				_unitOfWork.Repository<UserFavoriteArticle>().Delete(favorite);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation($"FavoriteService --> Remove with favoriteId : {favoriteId} succeeded");
            }

        }
        public async Task<bool> IsArticleFavorited(string userId, string newsId)
        {
            _logger.LogInformation($"FavoriteService --> IsArticleFavorited for userId : {userId} for newsId : {newsId} called");

            var userFavorites = await GetFavoritesByUser(userId);
            return userFavorites.Any(f => f.ArticleId == newsId);
        }
        public async Task<UserFavoriteArticle> GetFavoriteById(int favoriteId)
        {
            _logger.LogInformation($"FavoriteService --> GetFavoriteById with favoriteId : {favoriteId} called");
            return await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
        }

    }
}

