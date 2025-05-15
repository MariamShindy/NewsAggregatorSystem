namespace News.Service.Services
{
    public class SentimentService (HttpClient _httpClient , IConfiguration _configuration): ISentimentService
	{
        public async Task<List<NewsArticle>> GetNewsBySentimentAsync(string sentiment, int userId)
        {
			var url = $"http://127.0.0.1:7000/articles?sentiment={sentiment}&user_id={userId}";
			var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Flask API error: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<List<NewsArticle>>();
            return result ?? new List<NewsArticle>();
        }
    }
}
