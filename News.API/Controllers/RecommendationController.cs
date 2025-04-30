using News.Core.Dtos.NewsCatcher;

namespace News.API.Controllers
{
	[Authorize]
	public class RecommendationController(IRecommendationService _recommendationService) : ApiController
    {
		// POST: api/recommendation/getRecommendations
		[HttpPost("getRecommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] RecommendationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Topic))
                return BadRequest(new { error = "Topic is required" });
            try
            {
                var recommendations = await _recommendationService.GetRecommendedArticlesAsync(request.Topic);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
