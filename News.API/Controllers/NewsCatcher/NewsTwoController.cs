using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts.NewsCatcher;

namespace News.API.Controllers.NewsCatcher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize/*(Roles = "User")*/]
    public class NewsTwoController(INewsTwoService _newsService) : ControllerBase
    {
        ////GET : api/newsTwo/all
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllNews([FromQuery] string language = "en", [FromQuery] string country = "us")
        //{
        //    var news = await _newsService.GetAllNewsAsync(language, country);
        //    return Ok(news);
        //}

        //Apply pagination
        //GET : api/newsTwo/all?pageNumber=1&pageSize=35
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNews([FromQuery] int pageNumber = 0 , [FromQuery] int? pageSize = null, [FromQuery] string language = "en", [FromQuery] string country = "us")
        {
            var news = await _newsService.GetAllNewsAsync(pageNumber, pageSize, language, country);
            return Ok(news);
        }

        //GET : api/newsTwo/category/{category}
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetNewsByCategory(string category, [FromQuery] string language = "en", [FromQuery] string country = "us")
        {
            var news = await _newsService.GetNewsByCategoryAsync(category, language, country);
            return Ok(news);
        }

        //GET : api/newsTwo/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                //return NotFound();
                return NoContent();
            }
            return Ok(news);
        }

        //GET : api/newsTwo/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _newsService.GetCategoriesAsync();
            return Ok(categories);
        }

        //GET : api/newsTwo/generate-pdf/{id}
        [HttpGet("generate-pdf/{id}")]
        public IActionResult GeneratePdf(string id)
        {
            var article = _newsService.GetNewsByIdAsync(id); 

            if (article == null)
                return NoContent();
            //return NotFound("Article not found.");

            byte[] pdfBytes = _newsService.GenerateArticlePdf(article.Result); 

            return File(pdfBytes, "application/pdf", $"{article.Result.Title}.pdf");
        }
    }
}
