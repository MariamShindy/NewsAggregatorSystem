using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using News.Core.Contracts;
using News.Core.Entities;
using Newtonsoft.Json;
using News.Core.Contracts.UnitOfWork;

namespace News.Service.Services
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _apiKey;
        public NewsService(HttpClient httpClient, IConfiguration configuration , IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _apiKey = configuration["GoogleNewsAPI:ApiKey"];
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("News", "1.0"));
        }
        //Using newsAPI
        public async Task<string> GetAllNews()
        {
            var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&apiKey={_apiKey}";
            Console.WriteLine($"Requesting URL: {url}");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error fetching news: {errorContent}";
            }
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetAllNewsSec()
        {
            var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&apiKey={_apiKey}";
            Console.WriteLine($"Requesting URL: {url}");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error fetching news: {errorContent}";
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetArticleById(string id)
        {
            var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&apiKey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error fetching news: {errorContent}";
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);
            var article = newsData?.Articles?.FirstOrDefault(a => a.Url.Contains(id, StringComparison.OrdinalIgnoreCase));
            return article != null ? JsonConvert.SerializeObject(article) : "Article not found";
        }
        public async Task<bool> CheckArticleExists(string newsId)
        {
            var jsonResponse = await GetAllNews();
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);
            return newsData?.Articles?.Any(a => a.Url.Contains(newsId, StringComparison.OrdinalIgnoreCase)) ?? false;
        }
    }
}
