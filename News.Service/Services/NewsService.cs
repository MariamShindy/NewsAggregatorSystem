using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using News.Core.Contracts;
using News.Core.Entities;
using Newtonsoft.Json;

namespace News.Service.Services
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NewsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["NewsAPI:ApiKey"];
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("News", "1.0"));
        }
        //Using newsAPI
        public async Task<string> GetAllNews(int? page = null , int? pageSize = null)
        {
            var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&page={page}&pageSize={pageSize}&apiKey={_apiKey}"; // 
            //var url = $" https://newsapi.org/v2/everything?q=bitcoin&apiKey={_apiKey}"; //not including the categories //total response 10330
            //var url = $" https://newsapi.org/v2/top-headlines/sources?apiKey={_apiKey}"; //include categories //include all sources

            Console.WriteLine($"Requesting URL: {url}");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error fetching news: {errorContent}";
            }
            return await response.Content.ReadAsStringAsync();
        }
        
        ////Using newsdata.io
        //public async Task<string> GetAllNews()
        //{
        //    var url = $"https://newsdata.io/api/1/latest?apikey=pub_6007775e8108a6bb924d771106d3d3a18e48a&q=social"; //include categories //total response 32031 
        //    Console.WriteLine($"Requesting URL: {url}");
        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        return $"Error fetching news: {errorContent}";
        //    }
        //    return await response.Content.ReadAsStringAsync();
        //}

        ////Using mediastack
        //public async Task<string> GetAllNews()
        //{
        //    var url = $"http://api.mediastack.com/v1/news?access_key=c3eca2376d67324d55ed5341e396a4fd"; //include categories //total response 10000
        //    Console.WriteLine($"Requesting URL: {url}");
        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        return $"Error fetching news: {errorContent}";
        //    }
        //    return await response.Content.ReadAsStringAsync();
        //}
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
