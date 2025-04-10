﻿namespace News.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechController(ISpeechService _textToSpeechService) : ControllerBase
    {
        //GET : api/Speech/text-to-speech
        [HttpPost("text-to-speech")]
        public IActionResult Speak([FromBody] TextToSpeechRequest request)
        {
            try
            {
                var audioBytes = _textToSpeechService.ConvertTextToSpeech(request.Text,request.Language);
                return File(audioBytes, "audio/wav", "speech.wav");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }       
    }
}
