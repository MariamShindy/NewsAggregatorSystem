using System.Net.Http.Json;

namespace News.Service.Services
{
    public class SentimentService
    {
        private readonly HttpClient _httpClient;

        public SentimentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ArticleSentiment>> GetNewsBySentimentAsync(string sentiment)
        {
            var url = $"http://localhost:8000/articles?sentiment={sentiment}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Flask API error: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<List<ArticleSentiment>>();
            return result ?? new List<ArticleSentiment>();
        }
    }

}
