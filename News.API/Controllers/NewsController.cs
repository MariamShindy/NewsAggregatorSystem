using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="User")]
    [ApiExplorerSettings(IgnoreApi = true)]

    public class NewsController (INewsService _newsService) : ControllerBase
    {
        // GET: api/news/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews([FromQuery] int? page = 1, [FromQuery] int? pageSize = 40)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be positive integers.");

            var newsResult = await _newsService.GetAllCategorizedArticlesAsync(page,pageSize);
             if (newsResult.Count() == 0)
                   return StatusCode(500, newsResult);

            return Ok(newsResult);
        }

        // GET: api/news/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            var article = await _newsService.GetArticleByIdAsync(id);

            if (article == null)
                return NotFound(new { message = "Article not found" });

            return Ok(article);  
        }

        //GET : api/news/all-categories
        [HttpGet("all-categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _newsService.GetAllCategoriesAsync();
            return Ok(categories); 
        }
    }
}