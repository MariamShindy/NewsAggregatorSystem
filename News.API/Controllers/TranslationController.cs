using News.Core.Dtos.NewsCatcher;

namespace News.API.Controllers
{
    [Authorize]
    public class TranslationController(TranslationService _translationService) : ApiController
    {
		// POST: api/translation/translate
		[HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || string.IsNullOrWhiteSpace(request.SourceLang) || string.IsNullOrWhiteSpace(request.TargetLang))
                return BadRequest("Missing required fields.");

            try
            {
                var result = await _translationService.TranslateTextAsync(request);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
