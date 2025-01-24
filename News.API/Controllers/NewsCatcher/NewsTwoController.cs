using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts.NewsCatcher;

namespace News.API.Controllers.NewsCatcher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class NewsTwoController(INewsTwoService _newsService) : ControllerBase
    {
        // Get all news articles
        //GET : api/newsTwo/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews([FromQuery] string language = "en", [FromQuery] string country = "us")
        {
            var news = await _newsService.GetAllNewsAsync(language, country);
            return Ok(news);
        }

        // Get news articles by category
        //GET : api/newsTwo/category/{category}
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetNewsByCategory(string category, [FromQuery] string language = "en", [FromQuery] string country = "us")
        {
            var news = await _newsService.GetNewsByCategoryAsync(category, language, country);
            return Ok(news);
        }

        // Get news article by ID
        //GET : api/newsTwo/{id}
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
        //GET : api/newsTwo/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _newsService.GetCategoriesAsync();
            return Ok(categories);
        }

        // Dowanload article as pdf
        //GET : api/newsTwo/generate-pdf/{id}
        [HttpGet("generate-pdf/{id}")]
        public IActionResult GeneratePdf(string id)
        {
            var article = _newsService.GetNewsByIdAsync(id); 

            if (article == null)
                return NotFound("Article not found.");

            byte[] pdfBytes = _newsService.GenerateArticlePdf(article.Result); 

            return File(pdfBytes, "application/pdf", $"{article.Result.Title}.pdf");
        }
    }
}
