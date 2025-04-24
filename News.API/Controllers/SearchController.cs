using News.Core.Dtos.NewsCatcher;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(SearchService _searchService) : ControllerBase
    {
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
}
