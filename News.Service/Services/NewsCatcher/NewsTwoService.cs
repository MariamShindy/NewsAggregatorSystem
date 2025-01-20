using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using News.Core.Contracts.NewsCatcher;
using News.Core.Contracts.UnitOfWork;
using News.Core.Dtos;
using News.Core.Dtos.NewsCatcher;
using News.Core.Entities;
using News.Core.Entities.NewsCatcher;
using Newtonsoft.Json;

namespace News.Service.Services.NewsCatcher
{
    public class NewsTwoService(HttpClient _httpClient, IMapper _mapper, ILogger<NewsTwoService> _logger,
        IUnitOfWork _unitOfWork, IConfiguration _configuration) : INewsTwoService
    {
        private readonly string _apiKey = _configuration["NewsCatcher:ApiKey"]!;
        private readonly string _baseUrl = _configuration["NewsCatcher:BaseUrl"]!;
        private readonly List<string> _categories = new List<string>()
        {
            "gaming", "news", "sport", "tech",
            "world","finance", "politics","business", 
            "economics", "entertainment", "beauty",
            "travel","music","food","science","energy",
            "stockmarketinformationandanalysis","newsandcareerportal",
            "newsandmedia"
        };
       
        public async Task<List<NewsArticle>> GetAllNewsAsync(string language = "en", string country = "us")
        {
            //var requestUrl = $"{_baseUrl}?q={Uri.EscapeDataString("Google")}&lang={language}&sort_by=relevancy";
            int pageSize = 400;
            string from = "6 days ago", to = "5 days ago";
            var formattedFrom = Uri.EscapeDataString(from);
            var formattedTo = Uri.EscapeDataString(to);
            //var query = Uri.EscapeDataString("gaming OR news OR sport OR tech OR world OR finance OR politics OR business OR economics OR entertainment OR beauty OR travel OR music OR food OR science OR energy");
            var query = string.Join(" OR ", _categories.Select(Uri.EscapeDataString));

            var requestUrl = $"{_baseUrl}?q={query}&from={formattedFrom}&to={formattedTo}&page_size={pageSize}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            try
            {
                var response = await _httpClient.GetStringAsync(requestUrl);

                var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);
                if (newsResponse?.Articles != null)
                {
                    var groupedArticles = newsResponse.Articles.GroupBy(a => a.Topic).ToList();

                    var balancedArticles = groupedArticles.SelectMany(g => g.Take(10)).ToList();

                    newsResponse.Articles = balancedArticles;
                    newsResponse.TotalResults = newsResponse.Articles.Count;

                }
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
            //var newsResponse = await GetAllNewsAsync();
            var categories = _categories;
            await AddCategoriesToDatabaseAsync(categories);
            return categories ?? [];
        }
        private async Task AddCategoriesToDatabaseAsync(List<string?> Categories)
        {
            var existingCategories = await _unitOfWork.Repository<Category>().GetAllAsync();
            
            if (!existingCategories.Any() && Categories is not null)
            {
                foreach (var category in Categories)
                {
                    AddOrUpdateCategoryDto categoryDto = new AddOrUpdateCategoryDto { Name = category };
                    await AddCategoryAsync(categoryDto);
                    _logger.LogInformation($"Added {categoryDto.Name} category to database.");
                }
            }
        }
        public async Task<List<NewsArticle>> GetNewsByCategoryAsync(string category, string language = "en", string country = "us")
        {
            var newsResponse = await GetAllNewsAsync();

            var filteredNews = newsResponse.Where(article =>
            article.Topic != null &&
            article.Topic.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
            return filteredNews ?? [];
        }

        public async Task<NewsArticle> GetNewsByIdAsync(string id)
        {
            var newsResponse = await GetAllNewsAsync();
            var article = newsResponse.FirstOrDefault(a => a._Id == id);

            return article ;
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
        public async Task<IEnumerable<NewsArticleDto>> GetArticlesByCategoriesAsync(IEnumerable<CategoryDto> preferredCategories)
        {
            var categoryNames = preferredCategories.Select(c => c.Name).ToList();
            var allArticles = await GetAllNewsAsync();
            var articles = allArticles.ToList()
                .FindAll(a => categoryNames.Contains(a.Topic));
            var resArticles = _mapper.Map<IEnumerable<NewsArticleDto>>(articles);
            return resArticles;
        }
    }
}
