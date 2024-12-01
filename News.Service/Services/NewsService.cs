using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using News.Core.Contracts;
using News.Core.Entities;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using AutoMapper;

namespace News.Service.Services
{
    public class NewsService(HttpClient _httpClient, IMapper _mapper, IUnitOfWork _unitOfWork,IConfiguration _configuration, ILogger<NewsService> _logger) : INewsService
    {
        private readonly string _apiKey = _configuration["NewsAPI:ApiKey"]!;
        private readonly string _baseUrl = _configuration["NewsAPI:BaseUrl"]!;

        //Using newsAPI
        public async Task<string> GetAllNewsAsync(int? page = 1 , int? pageSize = 10)
        {
            _logger.LogInformation($"NewsService --> GetAllNews called with page: {page} and pageSize: {pageSize}");
            //NEWS API
            //var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&page={page}&pageSize={pageSize}&apiKey={_apiKey}"; 
            //var url = $" https://newsapi.org/v2/everything?q=bitcoin&apiKey={_apiKey}"; //not including the categories //total response 10330
            //var url = $" https://newsapi.org/v2/top-headlines/sources?apiKey={_apiKey}"; //include categories //include all sources
            
            //NEWS DATA
            //var url = $"https://newsdata.io/api/1/latest?apikey=pub_6007775e8108a6bb924d771106d3d3a18e48a&q=social"; //include categories //total response 32031 
            
            //MEDIA STACK
            //var url = $"http://api.mediastack.com/v1/news?access_key=c3eca2376d67324d55ed5341e396a4fd"; //include categories //total response 10000

            var url = $"{_baseUrl}?sortBy=popularity&q=tesla&apiKey={_apiKey}";
            _logger.LogInformation($"NewsService --> Requesting URL: {url}");
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("News", "1.0"));
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"NewsService --> GetAllNews failed , Error fetching news");

                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error fetching news: {errorContent}";
            }
            return await response.Content.ReadAsStringAsync();
        }


        public async Task<ArticleDto> GetArticleByIdAsync(string id)
        {
            _logger.LogInformation($"NewsService --> GetArticleById called with id : {id}");

            var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&apiKey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"NewsService --> GetArticleById failed with id : {id}. Status Code: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error fetching news: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);

                // Find the specific article by ID
                var article = newsData?.Articles?.FirstOrDefault(a => a.Url.Contains(id, StringComparison.OrdinalIgnoreCase));

                if (article == null)
                {
                    _logger.LogWarning($"NewsService --> Article with id : {id} not found in the response.");
                    return null;
                }

                _logger.LogInformation($"NewsService --> Article with id : {id} successfully fetched.");

                return _mapper.Map<ArticleDto>(article); ;
            }
            catch (Exception ex)
            {
                _logger.LogError($"NewsService --> Error deserializing article with id : {id}. Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CheckArticleExistsAsync(string newsId)
        {
            _logger.LogInformation($"NewsService --> CheckArticleExists called with newsId : {newsId}");
            var jsonResponse = await GetAllNewsAsync();
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);
            return newsData?.Articles?.Any(a => a.Url.Contains(newsId, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            _logger.LogInformation($"NewsService --> GetAllCategoriesAsync called");
            return await _unitOfWork.Repository<Category>().GetAllAsync();
        }
        public async Task<bool> AddCategoryAsync(AddOrUpdateCategoryDto categoryDto)
        {
            _logger.LogInformation($"NewsService --> AddCategoryAsync called");

            if (string.IsNullOrWhiteSpace(categoryDto.Name))
                throw new ArgumentException("Category name cannot be null or empty.");

            var category = new Category
            {
                Name = categoryDto.Name
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            return await _unitOfWork.CompleteAsync() > 0;
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            _logger.LogInformation($"NewsService --> DeleteCategoryAsync called with id : {id}");

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return false;

            await _unitOfWork.Repository<Category>().DeleteAsync(category);
            return await _unitOfWork.CompleteAsync() > 0;
        }
        public async Task<bool> UpdateCategoryAsync(int id, AddOrUpdateCategoryDto categoryDto)
        {
            _logger.LogInformation($"NewsService --> UpdateCategoryAsync called with id : {id}");

            if (string.IsNullOrWhiteSpace(categoryDto.Name))
                throw new ArgumentException("Category name cannot be null or empty.");

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return false;

            category.Name = categoryDto.Name;

            await _unitOfWork.Repository<Category>().UpdateAsync(category);
            return await _unitOfWork.CompleteAsync() > 0;
        }

    }
}
