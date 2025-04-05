using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace News.Service.Services
{
    public class SummarizationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flaskApiUrl = "http://127.0.0.1:8000/summarize";

        public SummarizationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SummarizeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text should not be empty", nameof(text));

            var jsonContent = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_flaskApiUrl, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from Flask API: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var summaryResponse = JsonSerializer.Deserialize<SummarizationResponse>(result);
            return summaryResponse?.Summary ?? "Error in summarization";
        }
    }

    public class SummarizationResponse
    {
        public string? Summary { get; set; }
    }
}