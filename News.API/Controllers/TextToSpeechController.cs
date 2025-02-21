using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Entities;

namespace News.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextToSpeechController : ControllerBase
    {
        private readonly ITextToSpeechService _textToSpeechService;

        public TextToSpeechController(ITextToSpeechService textToSpeechService)
        {
            _textToSpeechService = textToSpeechService;
        }
        //GET : api/TextToSpeech/speak
        [HttpPost("speak")]
        public IActionResult Speak([FromBody] TextToSpeechRequest request)
        {
            try
            {
                var audioBytes = _textToSpeechService.ConvertTextToSpeech(request.Text);
                return File(audioBytes, "audio/wav", "speech.wav");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
