using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts.NewsCatcher;

namespace News.API.Controllers.NewsCatcher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class NewsTwoController : ControllerBase
    {
        private readonly INewsTwoService _newsService;

        public NewsTwoController(INewsTwoService newsService)
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
