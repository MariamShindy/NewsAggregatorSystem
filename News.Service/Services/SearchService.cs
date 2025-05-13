namespace News.Service.Services
{
    public class SearchService(HttpClient _httpClient , IConfiguration configuration) : ISearchService
    {
        private readonly string _flaskApiUrl = "http://127.0.0.1:5000/search";
		public async Task<SearchResponse> SearchArticlesAsync(string query)
        {
            var payload = new { query };
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_flaskApiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from Flask API: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var searchResponse = System.Text.Json.JsonSerializer.Deserialize<SearchResponse>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return searchResponse;
        }
    }
}

