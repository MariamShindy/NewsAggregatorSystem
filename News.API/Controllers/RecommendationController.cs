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
            if (request.Topics == null || request.Topics.Count == 0)
                return BadRequest(new { error = "At least one topic is required" }); 
            try
            {
                var recommendations = await _recommendationService.GetRecommendedArticlesAsync(request.Topics); 
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
