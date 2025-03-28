namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController(IMemoryCache _cache) : ControllerBase
    {
        // GET: api/cache/chaced-articles
        [HttpGet("cached-articles")]
        public IActionResult GetAllCachedArticles()
        {
            var keysCacheKey = "cachedArticleKeys";
            var cachedKeys = _cache.Get<List<string>>(keysCacheKey);

            if (cachedKeys == null || !cachedKeys.Any())
                return NoContent();

            var cachedArticles = new List<object>();
            foreach (var key in cachedKeys)
            {
                if (_cache.TryGetValue(key, out var article))
                    cachedArticles.Add(new { Key = key, Article = article });
            }
            return Ok(cachedArticles);
        }
    }
}
