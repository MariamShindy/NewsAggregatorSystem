using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.NewsCatcher;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Dtos.NewsCatcher;
using News.Core.Entities;
using News.Core.Entities.NewsCatcher;

namespace News.Service.Services.NewsCatcher
{
    public class FavoriteTwoService(ILogger<FavoriteTwoService> _logger,
        INewsTwoService _newsService, IMemoryCache _cache,
        IUnitOfWork _unitOfWork) : IFavoriteTwoService
    {
        private const string CacheKeyPrefix = "FavoriteArticles_";

    
        public async Task AddToFavoritesAsync(string userId, string articleId)
        {
            _logger.LogInformation($"FavoriteService --> AddToFavorites called for userId: {userId} and articleId: {articleId}");

            var favorite = new UserFavoriteArticle { UserId = userId, ArticleId = articleId, AddedAt = DateTime.UtcNow };
            await _unitOfWork.Repository<UserFavoriteArticle>().AddAsync(favorite);
            await _unitOfWork.CompleteAsync();

            var cacheKey = $"{CacheKeyPrefix}{articleId}";
            if (!_cache.TryGetValue(cacheKey, out _))
            {
                var article = await _newsService.GetNewsByIdAsync(articleId);
                if (article is not null)
                {
                    _cache.Set(cacheKey, article, TimeSpan.FromDays(1)); 
                    _logger.LogInformation($"Article cached with key: {cacheKey}");

                    // Track the cache key in a list of keys
                    var keysCacheKey = "cachedArticleKeys";
                    var cachedKeys = _cache.Get<List<string>>(keysCacheKey) ?? new List<string>();
                    cachedKeys.Add(cacheKey);
                    _cache.Set(keysCacheKey, cachedKeys, TimeSpan.FromDays(1)); // Cache the keys list
                }
            }
        }
        
        public async Task<IEnumerable<NewsArticle>> GetFavoritesByUserAsync(string userId)
        {
            _logger.LogInformation($"FavoriteService --> GetFavoritesByUser called for userId: {userId}");
            var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
            var userFavoriteIds = favorites.Where(f => f.UserId == userId).Select(f => f.ArticleId).ToList();
            var favoriteArticles = new List<NewsArticle>();
            foreach (var articleId in userFavoriteIds)
            {
                var cacheKey = $"{CacheKeyPrefix}{articleId}";

                if (!_cache.TryGetValue(cacheKey, out NewsArticle article))
                {
                    article = await _newsService.GetNewsByIdAsync(articleId);

                    if (article != null)
                    {
                        _cache.Set(cacheKey, article, TimeSpan.FromDays(1)); 
                        _logger.LogInformation($"Article {articleId} fetched and cached.");
                    }
                    else
                    {
                        _logger.LogWarning($"Article {articleId} not found in external API.");
                    }
                }

                if (article != null)
                {
                    favoriteArticles.Add(article);
                }
            }

            _logger.LogInformation($"Total favorite articles fetched for user {userId}: {favoriteArticles.Count}");
            return favoriteArticles;
        }
        public async Task RemoveFromFavoritesAsync(string userId, string articleId)
        {
            _logger.LogInformation($"FavoriteService --> RemoveFromFavorites called for userId: {userId} and articleId: {articleId}");

            var favorite = await _unitOfWork.Repository<UserFavoriteArticle>()
                .FindAsync(f => f.UserId == userId && f.ArticleId == articleId);
            if (favorite != null)
            {
                await _unitOfWork.Repository<UserFavoriteArticle>().DeleteAsync(favorite.FirstOrDefault());
                await _unitOfWork.CompleteAsync();

                // Remove from cache
                var cacheKey = $"{CacheKeyPrefix}{userId}_{articleId}";
                _cache.Remove(cacheKey);

                _logger.LogInformation($"Article {articleId} removed from favorites and cache for userId: {userId}");
            }
        }
        public async Task<bool> IsArticleFavoritedAsync(string userId, string articleId)
        {
            _logger.LogInformation($"FavoriteService --> IsArticleFavorited called for userId: {userId} and articleId: {articleId}");

            // Check if the article is cached
            var cacheKey = $"{CacheKeyPrefix}{userId}_{articleId}";
            if (_cache.TryGetValue(cacheKey, out _))
            {
                _logger.LogInformation($"Article {articleId} found in cache for userId: {userId}");
                return true;
            }

            // Check the database for the favorite
            var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
            var isFavorited = favorites.Any(f => f.UserId == userId && f.ArticleId == articleId);

            if (isFavorited)
            {
                _logger.LogInformation($"Article {articleId} found in database for userId: {userId}");

                // Fetch the article from the API and cache it
                var article = await _newsService.GetNewsByIdAsync(articleId);
                if (article is not null)
                {
                    _cache.Set(cacheKey, article, TimeSpan.FromDays(2));
                    _logger.LogInformation($"Article {articleId} cached for userId: {userId}");
                }
            }

            return isFavorited;
        }
  
        public async Task<UserFavoriteArticle> GetFavoriteByIdAsync(int favoriteId)
        {
            _logger.LogInformation($"FavoriteService --> GetFavoriteById with favoriteId : {favoriteId} called");
            return await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
        }

    }
}
