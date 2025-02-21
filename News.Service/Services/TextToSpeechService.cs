using Microsoft.Extensions.Logging;
using News.Core.Contracts;
using System.Globalization;
using System.Speech.Synthesis;

namespace News.Service.Services
{
    public class TextToSpeechService(ILogger<TextToSpeechService> _logger) : ITextToSpeechService
    {
        public byte[] ConvertTextToSpeech(string text, string language = "en-US")
        {
            _logger.LogInformation("TextToSpeechService --> ConvertTextToSpeech Started");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be empty.");

            using (var synth = new SpeechSynthesizer())
            using (var memoryStream = new MemoryStream())
            {
                SetVoice(synth, language);
                synth.SetOutputToWaveStream(memoryStream);
                synth.Speak(text);

                return memoryStream.ToArray();
            }
        }

        private void SetVoice(SpeechSynthesizer synth, string language)
        {
            var availableVoices = synth.GetInstalledVoices()
                                       .Where(v => v.VoiceInfo.Culture.Name.StartsWith(language))
                                       .ToList();
            foreach (var voice in synth.GetInstalledVoices())
            {
                _logger.LogInformation($"Voice: {voice.VoiceInfo.Name}, Language: {voice.VoiceInfo.Culture}");
            }

            if (availableVoices.Any())
            {
                synth.SelectVoice(availableVoices.First().VoiceInfo.Name);
            }
            //else if (language == "ar-SA")
            //{
            //    synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult, 0, new CultureInfo("ar-SA"));
            //}
            else
            {
                throw new InvalidOperationException($"No installed voices found for language: {language}");
            }
        }
    }
}
