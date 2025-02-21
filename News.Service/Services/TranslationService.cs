using Microsoft.Extensions.Configuration;
using News.Core.Contracts;
using System.Text.Json;

namespace News.Service.Services
{
    public class TranslationService(HttpClient _httpClient ,IConfiguration _configuration) : ITranslationService
    {
        private readonly string API_URL = _configuration.GetSection("MyMemoryAPI")["BaseUrl"]!;
        public async Task<string> TranslateText(string text, string targetLanguage)
        {
            var response = await _httpClient.GetStringAsync($"{API_URL}?q={text}&langpair=en|{targetLanguage}");
            var result = JsonSerializer.Deserialize<JsonElement>(response);
            return result.GetProperty("responseData").GetProperty("translatedText").GetString()!;
        }
    }
}
