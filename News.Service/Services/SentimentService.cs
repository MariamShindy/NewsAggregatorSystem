namespace News.Service.Services
{
    public class SentimentService (HttpClient _httpClient , IConfiguration _configuration): ISentimentService
	{
        public async Task<List<ArticleSentiment>> GetNewsBySentimentAsync(string sentiment)
        {
            var flaskApiUrl = _configuration["FlaskApi:BaseUrl"];
			var url = $"{flaskApiUrl}/articles?sentiment={sentiment}";
			//var url = $"http://localhost:8000/articles?sentiment={sentiment}";
			var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Flask API error: {response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<List<ArticleSentiment>>();
            return result ?? new List<ArticleSentiment>();
        }
    }

}
