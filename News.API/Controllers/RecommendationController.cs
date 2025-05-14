using News.Core.Dtos.NewsCatcher;

namespace News.API.Controllers
{
    [Authorize]
    public class RecommendationController(IRecommendationService _recommendationService, IUserService _userService) : ApiController
    {
        // POST: api/recommendation/getRecommendations
        [HttpPost("getRecommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] RecommendationRequest request)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (request.Topics == null || request.Topics.Count == 0)
                return BadRequest(new { error = "At least one topic is required" });
            try
            {
                var recommendations = await _recommendationService.GetRecommendedArticlesAsync(request.Topics, user.Id);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("get-latest")]
        public async Task<IActionResult> GetLatestRecommendations()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                var recommendations = await _recommendationService.GetLatestRecommendationsAsync(user.Id);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}