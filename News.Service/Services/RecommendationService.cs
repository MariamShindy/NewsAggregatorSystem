namespace News.Service.Services
{
    public class RecommendationService(HttpClient _httpClient , IConfiguration configuration) : IRecommendationService
    {
        private readonly string _flaskApiUrl = "http://127.0.0.1:6000/recommend";


        public async Task<List<NewsArticle>> GetRecommendedArticlesAsync(List<string> topics)
        {
            var requestBody = new { topics = topics };
            var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_flaskApiUrl, requestContent);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from Recommendation API: {response.StatusCode}");
            }

            try
            {
                var recommendations = System.Text.Json.JsonSerializer.Deserialize<RecommendationResponse>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return recommendations?.Recommendations ?? new List<NewsArticle>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing recommendation response: {ex.Message}", ex);
            }
        }
    }

}