using Microsoft.Extensions.Configuration;
using News.Core.Contracts;
using System.Text;
using System.Text.Json;
using System.Web;

namespace News.Service.Services
{
    public class TranslationService(HttpClient _httpClient, IConfiguration _configuration) : ITranslationService
    {
        private readonly string API_URL = _configuration.GetSection("MyMemoryAPI")["BaseUrl"]!;
        private const int MaxCharactersPerRequest = 500;

        public async Task<string> TranslateText(string text, string targetLanguage)
        {
            if (text.Length <= MaxCharactersPerRequest)
                return await TranslateChunk(text, targetLanguage);

            var translatedText = new StringBuilder();
            var chunks = SplitTextIntoChunks(text, MaxCharactersPerRequest);

            foreach (var chunk in chunks)
            {
                string translatedChunk = await TranslateChunk(chunk, targetLanguage);
                translatedText.Append(translatedChunk).Append(" ");  
            }

            return translatedText.ToString().Trim();
        }

        private async Task<string> TranslateChunk(string text, string targetLanguage)
        {
            string encodedText = HttpUtility.UrlEncode(text); 
            var response = await _httpClient.GetStringAsync($"{API_URL}?q={encodedText}&langpair=en|{targetLanguage}");
            var result = JsonSerializer.Deserialize<JsonElement>(response);
            return result.GetProperty("responseData").GetProperty("translatedText").GetString()!;
        }

        private List<string> SplitTextIntoChunks(string text, int maxChunkSize)
        {
            var words = text.Split(' ');
            var chunks = new List<string>();
            var currentChunk = new StringBuilder();

            foreach (var word in words)
            {
                if (currentChunk.Length + word.Length + 1 > maxChunkSize)
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                }
                currentChunk.Append(word).Append(" ");
            }

            if (currentChunk.Length > 0)
                chunks.Add(currentChunk.ToString().Trim());

            return chunks;
        }
    }
}
