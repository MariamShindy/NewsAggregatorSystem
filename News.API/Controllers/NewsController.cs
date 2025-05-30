﻿namespace News.API.Controllers
{
    [Authorize/*(Roles ="User")*/]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class NewsController (INewsService _newsService) : ApiController
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
        //GET : api/news/generate-pdf/{id}
        [HttpGet("generate-pdf/{id}")]
        public IActionResult GeneratePdf(string id)
        {
            var article = _newsService.GetArticleByIdAsync(id);

            if (article == null)
                return NotFound("Article not found.");

            byte[] pdfBytes = _newsService.GenerateArticlePdf(article.Result);

            return File(pdfBytes, "application/pdf", $"{article.Result.Title}.pdf");
        }
    }
}