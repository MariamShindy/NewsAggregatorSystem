using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NewsController (INewsService _newsService) : ControllerBase
    {
        //GET: api/news/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews()
        {
            var newsResult = await _newsService.GetAllNews();

            if (newsResult.StartsWith("Error fetching news"))
                return StatusCode(500, newsResult);
            return Ok(newsResult);
        }
        // GET: api/news/{id}
        [HttpGet("{id}")]

        public async Task<IActionResult> GetNewsById(string id)
        {
            var articleResult = await _newsService.GetArticleById(id);

            if (articleResult.Contains("Article not found"))
                return NotFound(new { message = "Article not found" });

            if (articleResult.StartsWith("Error fetching news"))
                return StatusCode(500, articleResult);

            return Ok(articleResult);
        }
      
    }
}