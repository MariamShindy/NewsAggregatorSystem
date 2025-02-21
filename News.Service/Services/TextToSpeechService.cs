using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using System.Speech.Synthesis;

namespace News.Service.Services
{
    public class TextToSpeechService(ILogger<TextToSpeechService> _logger) : ITextToSpeechService
    {
        public byte[] ConvertTextToSpeech(string text)
        {
            _logger.LogInformation("TextToSpeechService --> ConvertTextToSpeech Started");
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.");

            using (var synth = new SpeechSynthesizer())
            using (var memoryStream = new MemoryStream())
            {
                synth.SetOutputToWaveStream(memoryStream);
                synth.Speak(text);

                return memoryStream.ToArray(); 
            }
        }
    }
}
