using News.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace News.Service.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskApiUrl = "http://127.0.0.1:8000/recommend";

        public RecommendationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RecommendationResult>> GetRecommendedArticlesAsync(string topic)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(new { topic }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_flaskApiUrl, requestContent);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from Recommendation API: {response.StatusCode}");
            }

            try
            {
                var recommendations = JsonSerializer.Deserialize<RecommendationResponse>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return recommendations?.Recommendations ?? new List<RecommendationResult>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing recommendation response: {ex.Message}", ex);
            }
        }
    }
    public class RecommendationResponse
    {
        public List<RecommendationResult> Recommendations { get; set; } = new();
    }

    public class RecommendationResult
    {
        public string Title { get; set; }
        public string Summary { get; set; }
    }
    //public class RecommendationResponse
    //{
    //    [JsonPropertyName("recommendations")]
    //    //public ArticleResult[] Recommendations { get; set; } = {};
    //    public List<ArticleDto>? Recommendations { get; set; }
    //}
}