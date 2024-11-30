using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.Service.Services
{
    public class FavoriteService(ILogger<FavoriteService> _logger,INewsService _newsService, IMemoryCache _cache, IUnitOfWork _unitOfWork) : IFavoriteService
    {
        private const string CacheKeyPrefix = "FavoriteArticles_";

        #region Favorite before caching
        //public async Task Add(UserFavoriteArticle favorite)
        //{
        //    _logger.LogInformation("FavoriteService --> Add called");
        //    _unitOfWork.Repository<UserFavoriteArticle>().Add(favorite);
        //    await _unitOfWork.CompleteAsync();
        //    _logger.LogInformation("FavoriteService --> Add succeeded");
        //}

        //public async Task<IEnumerable<UserFavoriteArticle>> GetFavoritesByUser(string userId)
        //{
        //    _logger.LogInformation($"FavoriteService --> GetFavoritesByUser with userId : {userId} called");
        //    var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
        //    _logger.LogInformation($"FavoriteService --> GetFavoritesByUser with userId : {userId} succeeded");
        //    return favorites.Where(f => f.UserId == userId).ToList();
        //}

        //public async Task Remove(int favoriteId)
        //{
        //    _logger.LogInformation($"FavoriteService --> Remove with favoriteId : {favoriteId} called");

        //    var favorite = await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
        //    if (favorite != null)
        //    {
        //        _unitOfWork.Repository<UserFavoriteArticle>().Delete(favorite);
        //        await _unitOfWork.CompleteAsync();
        //        _logger.LogInformation($"FavoriteService --> Remove with favoriteId : {favoriteId} succeeded");
        //    }

        //}
        //public async Task<bool> IsArticleFavorited(string userId, string newsId)
        //{
        //    _logger.LogInformation($"FavoriteService --> IsArticleFavorited for userId : {userId} for newsId : {newsId} called");

        //    var userFavorites = await GetFavoritesByUser(userId);
        //    return userFavorites.Any(f => f.ArticleId == newsId);
        //}
        //public async Task<UserFavoriteArticle> GetFavoriteById(int favoriteId)
        //{
        //    _logger.LogInformation($"FavoriteService --> GetFavoriteById with favoriteId : {favoriteId} called");
        //    return await _unitOfWork.Repository<UserFavoriteArticle>().GetByIdAsync(favoriteId);
        //} 
        #endregion

        //public async Task AddToFavorites(string userId, string articleId)
        //{
        //    _logger.LogInformation($"FavoriteService --> AddToFavorites called for userId: {userId} and articleId: {articleId}");
        //    var favorite = new UserFavoriteArticle { UserId = userId, ArticleId = articleId , AddedAt = DateTime.UtcNow };
        //    _unitOfWork.Repository<UserFavoriteArticle>().Add(favorite);
        //    await _unitOfWork.CompleteAsync();

        //    var cacheKey = $"{CacheKeyPrefix}{userId}_{articleId}";
        //    if (!_cache.TryGetValue(cacheKey, out _))
        //    {
        //        var article = await _newsService.GetArticleById(articleId);
        //        if (article is not null)
        //        {
        //            //_cache.Set(cacheKey, article, TimeSpan.FromDays(2));
        //            _cache.Set(cacheKey, article, TimeSpan.FromMinutes(5));

        //            _logger.LogInformation($"Article cached for userId: {userId} and articleId: {articleId}");
        //        }
        //    }
        //}
        public async Task AddToFavoritesAsync(string userId, string articleId)
        {
            _logger.LogInformation($"FavoriteService --> AddToFavorites called for userId: {userId} and articleId: {articleId}");

            var favorite = new UserFavoriteArticle { UserId = userId, ArticleId = articleId, AddedAt = DateTime.UtcNow };
            await _unitOfWork.Repository<UserFavoriteArticle>().AddAsync(favorite);
            await _unitOfWork.CompleteAsync();

            var cacheKey = $"{CacheKeyPrefix}{articleId}";
            if (!_cache.TryGetValue(cacheKey, out _))
            {
                var article = await _newsService.GetArticleByIdAsync(articleId);
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
        //public async Task<IEnumerable<ArticleDto>> GetFavoritesByUser(string userId)
        //{
        //    _logger.LogInformation($"FavoriteService --> GetFavoritesByUser called for userId: {userId}");

        //    // Get all favorites for the user
        //    var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
        //    var userFavorites = favorites.Where(f => f.UserId == userId).Select(f => f.ArticleId);

        //    var favoriteArticles = new List<ArticleDto>();

        //    foreach (var articleId in userFavorites)
        //    {
        //        var cacheKey = $"{CacheKeyPrefix}{userId}_{articleId}";
        //        if (!_cache.TryGetValue(cacheKey, out ArticleDto article))
        //        {
        //            article = await _newsService.GetArticleById(articleId);
        //            if (article is not null)
        //            {
        //                // Cache the article JSON for 2 days
        //                _cache.Set(cacheKey, article, TimeSpan.FromDays(2));
        //                _logger.LogInformation($"Article {articleId} fetched and cached for userId: {userId}");
        //                favoriteArticles.Add(article);

        //            }
        //            else
        //            {
        //                _logger.LogWarning($"Article with ID {articleId} not found.");
        //            }
        //        }
        //    }

        //    return favoriteArticles;
        //}
        //public async Task<IEnumerable<ArticleDto>> GetFavoritesByUser(string userId)
        //{
        //    _logger.LogInformation($"FavoriteService --> GetFavoritesByUser called for userId: {userId}");

        //    var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
        //    _logger.LogInformation($"Total favorites found in database: {favorites.Count()}");
        //    var userFavorites = favorites.Where(f => f.UserId == userId).Select(f => f.ArticleId).ToList();

        //    _logger.LogInformation($"User {userId} favorites (ArticleIds): {string.Join(", ", userFavorites)}");

        //    var favoriteArticles = new List<ArticleDto>();

        //    foreach (var articleId in userFavorites)
        //    {
        //        var cacheKey = $"{CacheKeyPrefix}{userId}_{articleId}";
        //        _logger.LogInformation($"Checking cache for article with key: {cacheKey}");
        //        if (!_cache.TryGetValue(cacheKey, out ArticleDto article))
        //        {
        //            article = await _newsService.GetArticleById(articleId);
        //            if (article != null)
        //            {
        //                //_cache.Set(cacheKey, article, TimeSpan.FromDays(2)); // Cache for 2 days
        //                _cache.Set(cacheKey, article, TimeSpan.FromMinutes(5));
        //                _logger.LogInformation($"Article {articleId} fetched and cached for userId: {userId}");
        //                favoriteArticles.Add(article);
        //            }
        //            else
        //            {
        //                _logger.LogWarning($"Article with ID {articleId} not found for userId: {userId}");
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogInformation($"Article {articleId} found in cache for userId: {userId}");
        //            favoriteArticles.Add(article);
        //        }
        //    }
        //    _logger.LogInformation($"Total favorite articles fetched for user {userId}: {favoriteArticles.Count()}");
        //    return favoriteArticles;
        //}

        //public async Task<IEnumerable<string>> GetFavoritesByUser(string userId)
        //{
        //    _logger.LogInformation($"FavoriteService --> GetFavoritesByUser called for userId: {userId}");

        //    var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
        //    var userFavorites = favorites.Where(f => f.UserId == userId).Select(f => f.ArticleId);

        //    var favoriteArticles = new List<string>();

        //    foreach (var articleId in userFavorites)
        //    {
        //        var cacheKey = $"{CacheKeyPrefix}{userId}_{articleId}";
        //        if (!_cache.TryGetValue(cacheKey, out string article))
        //        {
        //            article = await _newsService.GetArticleById(articleId);
        //            if (!string.IsNullOrEmpty(article))
        //            {
        //                _cache.Set(cacheKey, article, TimeSpan.FromDays(2));
        //                _logger.LogInformation($"Article {articleId} fetched and cached for userId: {userId}");
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(article))
        //        {
        //            favoriteArticles.Add(article);
        //        }
        //    }

        //    return favoriteArticles;
        //}
        public async Task<IEnumerable<ArticleDto>> GetFavoritesByUserAsync(string userId)
        {
            _logger.LogInformation($"FavoriteService --> GetFavoritesByUser called for userId: {userId}");
            var favorites = await _unitOfWork.Repository<UserFavoriteArticle>().GetAllAsync();
            var userFavoriteIds = favorites.Where(f => f.UserId == userId).Select(f => f.ArticleId).ToList();
            var favoriteArticles = new List<ArticleDto>();
            foreach (var articleId in userFavoriteIds)
            {
                var cacheKey = $"{CacheKeyPrefix}{articleId}";

                if (!_cache.TryGetValue(cacheKey, out ArticleDto article))
                {
                    article = await _newsService.GetArticleByIdAsync(articleId);

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
                var article = await _newsService.GetArticleByIdAsync(articleId);
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
