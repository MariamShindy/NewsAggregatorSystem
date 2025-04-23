namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;

        public RecommendationController(RecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost("getRecommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] RecommendationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest(new { error = "Topic is required" });
            }

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

    public class RecommendationRequest
    {
        public string Topic { get; set; }
    }
}
