using Microsoft.Extensions.Configuration;
using News.Core.Contracts;
using News.Core.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace News.Service.Services
{
    public class NewsTwoService : INewsServiceTwo
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NewsTwoService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["NewsCatcher:ApiKey"]!;
            _baseUrl = configuration["NewsCatcher:BaseUrl"]!;
        }
        public async Task<List<NewsArticle>> GetAllNewsAsync(string language = "en", string country = "us")
        {
            var requestUrl = $"{_baseUrl}?q={Uri.EscapeDataString("Google")}&lang={language}&sort_by=relevancy";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            try
            {
                var response = await _httpClient.GetStringAsync(requestUrl);

                var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);
                newsResponse.TotalResults = newsResponse.Articles.Count;
               
                foreach (var article in newsResponse?.Articles ?? new List<NewsArticle>())
                {
                    if (article.Authors is string author)
                    {
                        article.Authors = new List<string> { author };
                    }
                    else if (article.Authors == null)
                    {
                        article.Authors = new List<string>();
                    }
                }
                 Console.WriteLine($"Number of articles fetched ==> {newsResponse?.TotalResults}");
                return newsResponse?.Articles ?? new List<NewsArticle>();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request failed: {e.Message}");
            }
            return new List<NewsArticle>();
        }
    


        public async Task<List<string>> GetCategoriesAsync()
        {
            var newsResponse = await GetAllNewsAsync(); 

            var categories = newsResponse
                .Select(article => article.Topic)  
                .Distinct()                        
                .ToList();

            return categories ?? [];
        }

        public async Task<List<NewsArticle>> GetNewsByCategoryAsync(string category, string language = "en", string country = "us")
        {
            var url = $"{_baseUrl}?category={category}&language={language}&country={country}&apiKey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);
            return newsResponse?.Articles ?? new List<NewsArticle>();
        }

        public async Task<NewsArticle> GetNewsByIdAsync(string id)
        {
            var url = $"{_baseUrl}?q={id}&apiKey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);
            return newsResponse?.Articles?.FirstOrDefault() ?? new NewsArticle();
        }
    }
}
