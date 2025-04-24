using News.Core.Dtos.NewsCatcher;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummarizationController(SummarizationService _summarizationService) : ControllerBase
    {
        [HttpPost("summarize")]
        public async Task<IActionResult> Summarize([FromBody] SummarizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { error = "No text provided" });
            }

            try
            {
                var summary = await _summarizationService.SummarizeTextAsync(request.Text);
                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
