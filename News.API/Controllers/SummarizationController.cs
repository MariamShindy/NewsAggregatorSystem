using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Service.Services;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummarizationController : ControllerBase
    {
        private readonly SummarizationService _summarizationService;

        public SummarizationController(SummarizationService summarizationService)
        {
            _summarizationService = summarizationService;
        }

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

    public class SummarizationRequest
    {
        public string? Text { get; set; }
    }
}
