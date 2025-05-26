using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using News.Core.Contracts.Interfaces;
using News.Core.Dtos.NewsCatcher;

namespace News.Service.Services
{
    public class RecommendationService(HttpClient _httpClient, IConfiguration configuration) : IRecommendationService
    {
        private readonly string _recommendApiUrl = "http://127.0.0.1:8070/recommend";
        private readonly string _cachedRecommendationsUrl = "http://127.0.0.1:8070/get-latest-recommendations";


        public async Task<List<NewsArticle>> GetRecommendedArticlesAsync(List<string> topics, string userId)
        {
            var requestBody = new { topics = topics, user_id = userId };
            var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_recommendApiUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from Recommendation API: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();

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

        public async Task<List<NewsArticle>> GetLatestRecommendationsAsync(string userId, int pageNumber = 0, int? pageSize = null)
        {
            var response = await _httpClient.GetAsync($"{_cachedRecommendationsUrl}?user_id={userId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching cached recommendations: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();

            try
            {
                var recommendations = System.Text.Json.JsonSerializer.Deserialize<RecommendationResponse>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var allRecommendations = recommendations?.Recommendations ?? new List<NewsArticle>();

                if (!pageSize.HasValue || pageNumber <= 0)
                    return allRecommendations;

                return allRecommendations
                    .Skip((pageNumber - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToList();

            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing cached recommendations: {ex.Message}", ex);
            }
        }
    }
}