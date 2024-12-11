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
        private readonly string _sourceBaseUrl = _configuration["NewsAPI:SourcesBaseUrl"]!;

        #region APIs URLs
        //NEWS API
        //var url = $"https://newsapi.org/v2/top-headlines?sources=bbc-news&page={page}&pageSize={pageSize}&apiKey={_apiKey}"; 
        //var url = $" https://newsapi.org/v2/everything?q=bitcoin&apiKey={_apiKey}"; //not including the categories //total response 10330
        //var url = $" https://newsapi.org/v2/top-headlines/sources?apiKey={_apiKey}"; //include categories //include all sources

        //NEWS DATA
        //var url = $"https://newsdata.io/api/1/latest?apikey=pub_6007775e8108a6bb924d771106d3d3a18e48a&q=social"; //include categories //total response 32031 

        //MEDIA STACK
        //var url = $"http://api.mediastack.com/v1/news?access_key=c3eca2376d67324d55ed5341e396a4fd"; //include categories //total response 10000 
        #endregion

        public async Task<Dictionary<string, string>> GetSourceCategoriesAsync()
        {
            _logger.LogInformation($"NewsService --> Fetching source categories from NewsAPI.");

            var url = $"{_sourceBaseUrl}?apiKey={_apiKey}";
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("News", "1.0"));

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"NewsService --> Failed to fetch source categories from NewsAPI. Status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error fetching source categories: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var sourcesResponse = JsonConvert.DeserializeObject<SourcesResponse>(jsonResponse);

            var sourceCategories = sourcesResponse?.Sources.ToDictionary(s => s.Id, s => s.Category) ?? new Dictionary<string, string>();

            _logger.LogInformation($"NewsService --> Fetched {sourceCategories.Count} source categories.");
            
            var existingCategories = await _unitOfWork.Repository<Category>().GetAllAsync();

            if (!existingCategories.Any())
            {
                foreach (var category in sourceCategories.Values.Distinct())
                {
                    AddOrUpdateCategoryDto categoryDto = new AddOrUpdateCategoryDto() { Name = category };
                    await AddCategoryAsync(categoryDto);
                    _logger.LogInformation($"NewsService --> GetSourceCategoriesAsync --> Add {categoryDto.Name} category to database .");
                }
            }
            return sourceCategories;
        }

        public async Task<IEnumerable<ArticleDto>> GetAllCategorizedArticlesAsync(int? page = 1, int? pageSize = 40)
        {
            _logger.LogInformation($"NewsService --> GetAllCategorizedArticles called with page: {page} and pageSize: {pageSize}");
            var sourceCategories = await GetSourceCategoriesAsync();
            var url = $"{_baseUrl}?sortBy=popularity&q=tesla&apiKey={_apiKey}";
            _logger.LogInformation($"NewsService --> Requesting URL: {url}");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"NewsService --> GetAllNews failed, Error fetching news");
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);

            var articles = newsData?.Articles
                .Where(article => !string.IsNullOrEmpty(article.UrlToImage))
                .Select(article =>
            {
                if (article.Source != null && !string.IsNullOrEmpty(article.Source.Id) && sourceCategories.ContainsKey(article.Source.Id))
                {
                    article.Category = sourceCategories[article.Source.Id];
                    article.Source.Category = sourceCategories[article.Source.Id];
                }
                else
                {
                    article.Category = "Uncategorized";
                    article.Source.Category = "Uncategorized";

                }
                article.Id = article.Url;
                return _mapper.Map<ArticleDto>(article);
            });
          
            return articles ?? new List<ArticleDto>();
        }
       
        public async Task<ArticleDto> GetArticleByIdAsync(string id)
        {
            _logger.LogInformation($"NewsService --> GetArticleById called with id : {id}");
            var response = await GetAllCategorizedArticlesAsync();
            if (response.Count() ==0)
                _logger.LogError($"NewsService --> GetArticleById failed with id : {id}");
            try
            {
                var article = response.FirstOrDefault(a => a.Url.Contains(id, StringComparison.OrdinalIgnoreCase));
                if (article == null)
                {
                    _logger.LogWarning($"NewsService --> Article with id : {id} not found in the response.");
                    return null;
                }
                _logger.LogInformation($"NewsService --> Article with id : {id} successfully fetched.");
                article.Id = id;
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
            var articles = await GetAllCategorizedArticlesAsync(1, 10);
            return articles.Any(a => a.Url.Contains(newsId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            _logger.LogInformation($"NewsService --> Fetching all categories from NewsAPI.");
            var allCategories =  await GetSourceCategoriesAsync();
            List<string> categories = allCategories.Values.Distinct().ToList(); ;   
            _logger.LogInformation($"NewsService --> Fetched {categories.Count} unique categories.");
            return categories;
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
