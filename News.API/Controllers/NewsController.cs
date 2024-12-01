﻿using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> GetAllNews([FromQuery] int? page = 1, [FromQuery] int? pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be positive integers.");

            var newsResult = await _newsService.GetAllNewsAsync(page, pageSize);
            if (newsResult.StartsWith("Error fetching news"))
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
        //BEFORE CACHING
        //// GET: api/news/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetNewsById(string id)
        //{
        //    var articleResult = await _newsService.GetArticleById(id);
        //    if (articleResult.Contains("Article not found"))
        //        return NotFound(new { message = "Article not found" });
        //    if (articleResult.StartsWith("Error fetching news"))
        //        return StatusCode(500, articleResult);
        //    return Ok(articleResult);
        //}
    }
}