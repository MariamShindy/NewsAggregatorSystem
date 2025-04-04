using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace News.Service.Services
{
    public class SearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskApiUrl = "http://127.0.0.1:8000/search";

        public SearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SearchResponse> SearchArticlesAsync(string query, int page = 1)
        {
            var payload = new { query, page };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_flaskApiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from Flask API: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return searchResponse;
        }
    }

    public class SearchResponse
    {
        public ArticleResult[] Results { get; set; }
    }

    public class ArticleResult
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        //public double SimilarityScore { get; set; }
    }
}

