using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsTwoController : ControllerBase
    {
        private readonly INewsServiceTwo _newsService;

        public NewsTwoController(INewsServiceTwo newsService)
        {
            _newsService = newsService;
        }

        // Get all news articles
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews([FromQuery] string language = "en", [FromQuery] string country = "us")
        {
            var news = await _newsService.GetAllNewsAsync(language, country);
            return Ok(news);
        }

        // Get news articles by category
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetNewsByCategory(string category, [FromQuery] string language = "en", [FromQuery] string country = "us")
        {
            var news = await _newsService.GetNewsByCategoryAsync(category, language, country);
            return Ok(news);
        }

        // Get news article by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return Ok(news);
        }

        // Get all categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _newsService.GetCategoriesAsync();
            return Ok(categories);
        }
    }
}
