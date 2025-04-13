namespace News.API.Controllers
{
    [Authorize]
    public class TranslationController(ITranslationService _translationService) : ApiController
    {
        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || string.IsNullOrWhiteSpace(request.Language))
                return BadRequest("Text and target language are required.");

            var translatedText = await _translationService.TranslateText(request.Text, request.Language);
                try
                {
                    return Ok(new { translation = translatedText });
                }
                catch (JsonReaderException ex)
                {
                    return BadRequest($"Invalid JSON response: {ex.Message}");
                }
        }

    }
}
