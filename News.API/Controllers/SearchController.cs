using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Service.Services;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost("searchResults")]
        public async Task<IActionResult> SearchArticles([FromBody] SearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "No query provided" });
            }

            try
            {
                var response = await _searchService.SearchArticlesAsync(request.Query, request.Page);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class SearchRequest
    {
        public string Query { get; set; }
        public int Page { get; set; } = 1;
    }
}
