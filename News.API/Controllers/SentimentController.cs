namespace News.API.Controllers
{
    [ApiController]
    [Route("api/sentiment-news")]
	[Authorize]
	public class SentimentController(ISentimentService _newsService) : ControllerBase
    {
		// GET: api/sentiment/{sentiment}
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
