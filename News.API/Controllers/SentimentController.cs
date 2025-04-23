using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace News.API.Controllers
{
    [ApiController]
    [Route("api/sentiment-news")]
    public class SentimentController : ControllerBase
    {
        private readonly SentimentService _newsService;

        public SentimentController(SentimentService newsService)
        {
            _newsService = newsService;
        }

        [HttpGet("{sentiment}")]
        public async Task<IActionResult> GetSentimentNews(string sentiment)
        {
            sentiment = sentiment.ToLower();

            if (sentiment != "positive" && sentiment != "negative")
                return BadRequest("Sentiment must be either 'positive' or 'negative'");

            var articles = await _newsService.GetNewsBySentimentAsync(sentiment);
            return Ok(articles);
        }
    }
}
