﻿namespace News.Service.Services.NewsCatcher
{
    public class NewsTwoService(HttpClient _httpClient, IMapper _mapper, ILogger<NewsTwoService> _logger,
        IUnitOfWork _unitOfWork, IConfiguration _configuration ,IRecommendationService _recommendationService
        , IUserService _userService) : INewsTwoService
    {
        private readonly string _apiKey = _configuration["NewsCatcher:ApiKey"]!;
        private readonly string _baseUrl = _configuration["NewsCatcher:BaseUrl"]!;
        private readonly List<string> _categories = new List<string>()
        {
            "Business","Economics","Entertainment","Finance","Health","Politics","Science","Sports","Tech",
            "Crime","Financial Crime","Lifestyle","Automotive","Travel","Weather","General"
        };
        #region Reading from API without pagination
        //public async Task<List<NewsArticle>> GetAllNewsAsync(string language = "en", string country = "us")
        //{
        //    //var requestUrl = $"{_baseUrl}?q={Uri.EscapeDataString("Google")}&lang={language}&sort_by=relevancy";
        //    int pageSize = 400;
        //    string from = "6 days ago", to = "5 days ago";
        //    var formattedFrom = Uri.EscapeDataString(from);
        //    var formattedTo = Uri.EscapeDataString(to);
        //    //var query = Uri.EscapeDataString("gaming OR news OR sport OR tech OR world OR finance OR politics OR business OR economics OR entertainment OR beauty OR travel OR music OR food OR science OR energy");
        //    var query = string.Join(" OR ", _categories.Select(Uri.EscapeDataString));

        //    var requestUrl = $"{_baseUrl}?q={query}&from={formattedFrom}&to={formattedTo}&page_size={pageSize}";

        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        //    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        //    try
        //    {
        //        var response = await _httpClient.GetStringAsync(requestUrl);

        //        var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);
        //        if (newsResponse?.Articles != null)
        //        {
        //            var groupedArticles = newsResponse.Articles.GroupBy(a => a.Topic).ToList();

        //            var balancedArticles = groupedArticles.SelectMany(g => g.Take(10)).ToList();

        //            newsResponse.Articles = balancedArticles;
        //            newsResponse.TotalResults = newsResponse.Articles.Count;

        //        }
        //        foreach (var article in newsResponse?.Articles ?? new List<NewsArticle>())
        //        {
        //            if (article.Authors is string author)
        //            {
        //                article.Authors = new List<string> { author };
        //            }
        //            else if (article.Authors == null)
        //            {
        //                article.Authors = new List<string>();
        //            }
        //        }
        //        Console.WriteLine($"Number of articles fetched ==> {newsResponse?.TotalResults}");
        //        return newsResponse?.Articles ?? new List<NewsArticle>();
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        Console.WriteLine($"Request failed: {e.Message}");
        //    }
        //    return new List<NewsArticle>();
        //}

        #endregion

        #region Reading from API with pagination
        //v3
        //public async Task<List<NewsArticle>> GetAllNewsAsync(List<string> categories, int pageNumber = 1, int? pageSize = null, string language = "en", string country = "us")
        //{
        //    int apiPageSize = 100;
        //    var currentDate = DateTime.UtcNow.Date;
        //    var to = currentDate.ToString("yyyy-MM-dd");
        //    var from = currentDate.AddDays(-10).ToString("yyyy-MM-dd");
        //    var countries = string.Join(",", new[] { country, "EG", "CA", "FR", "GB", "DE" });

        //    Console.WriteLine("From: " + from);
        //    Console.WriteLine("To: " + to);
        //    var query = string.Join(" OR ", categories);

        //    var requestUrl = $"{_baseUrl}?q={query}&from_={from}&to_={to}&lang={language}&countries={countries}&page_size={apiPageSize}&page={pageNumber}";

        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Add("x-api-token", _apiKey);
        //    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        //    try
        //    {
        //        var response = await _httpClient.GetStringAsync(requestUrl);
        //        var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);

        //        if (newsResponse?.Articles != null)
        //        {
        //            var articlesList = newsResponse.Articles.ToList();

        //            foreach (var article in articlesList)
        //            {
        //                if (string.IsNullOrEmpty(article.Author))
        //                    article.Author = "Unknown author";
        //                if (string.IsNullOrEmpty(article.Twitter_Account))
        //                    article.Twitter_Account = "Unknown account";
        //                if (article.Authors.Count == 0)
        //                    article.Authors.Add("Unknown authors");
        //            }
        //            _logger.LogInformation($"Returning articles count ==> {articlesList.Count}");
        //            return articlesList;
        //        }
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        _logger.LogError($"Request failed: {e.Message}");
        //    }

        //    return new List<NewsArticle>();
        //}

        //public async Task<List<NewsArticle>> GetAllNewsAsync(int pageNumber = 0, int? pageSize = null, string language = "en", string country = "us")
        //{
        //    int apiPageSize = 400;
        //    string from = "6 days ago", to = "5 days ago";
        //    var formattedFrom = Uri.EscapeDataString(from);
        //    var formattedTo = Uri.EscapeDataString(to);
        //    var query = string.Join(" OR ", _categories.Select(Uri.EscapeDataString));
        //    var requestUrl = $"{_baseUrl}?q={query}&from={formattedFrom}&to={formattedTo}&page_size={apiPageSize}";
        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        //    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        //    try
        //    {
        //        var response = await _httpClient.GetStringAsync(requestUrl);
        //        var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);

        //        if (newsResponse?.Articles != null)
        //        {
        //            var groupedArticles = newsResponse.Articles.GroupBy(a => a.Topic).ToList();
        //            var balancedArticles = groupedArticles.SelectMany(g => g.Take(10)).ToList();

        //            foreach (var article in balancedArticles)
        //            {
        //                if (article.Authors is JArray jArray)
        //                    article.Authors = jArray.ToObject<List<string>>();
        //                else if (article.Authors is string s && !string.IsNullOrWhiteSpace(s))
        //                    article.Authors = new List<string> { s };
        //                else
        //                    article.Authors = new List<string>() { "Unknown authors" };
        //                if (string.IsNullOrEmpty(article.Author))
        //                    article.Author = "Unknown author";
        //            }

        //            if (pageSize is null || pageNumber == 0)
        //            {
        //                _logger.LogInformation($"Returning all articles ==> {balancedArticles.Count}");
        //                return balancedArticles;
        //            }

        //            var paginatedArticles = balancedArticles
        //                .Skip((pageNumber - 1) * pageSize.Value)
        //                .Take(pageSize.Value)
        //                .ToList();

        //            _logger.LogInformation($"Number of articles fetched after pagination ==> {paginatedArticles.Count}");
        //            return paginatedArticles;
        //        }
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        _logger.LogError($"Request failed: {e.Message}");
        //    }

        //    return new List<NewsArticle>();
        //}

        #endregion

        #region Reading from json file with pagination
        public async Task<List<NewsArticle>> GetAllNewsAsync(List<string> categories,int pageNumber = 0, int? pageSize = null, string language = "en", string country = "us")
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(baseDirectory, "DataSeeding", "NewsData.json");

                var jsonData = await File.ReadAllTextAsync(filePath);
                var articles = JsonConvert.DeserializeObject<List<NewsArticle>>(jsonData);

                if (articles != null)
                {
                    var groupedArticles = articles.GroupBy(a => a.Topic).ToList();
                    var balancedArticles = groupedArticles.SelectMany(g => g.Take(10)).ToList();

                    // If pageSize is null,  return all articles
                    if (pageSize is null || pageNumber == 0)
                    {
                        _logger.LogInformation($"Returning all articles from json ==> {balancedArticles.Count}");
                        return balancedArticles;
                    }

                    //Else apply pagination
                    var paginatedArticles = balancedArticles
                        .Skip((pageNumber - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .ToList();
                    var articlesWithCategory = paginatedArticles
                         .FindAll(a => categories.Contains(a.Topic));
                    _logger.LogInformation($"Number of articles fetched from json ==> {paginatedArticles.Count}");
                    return articlesWithCategory;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading JSON file: {ex.Message}");
            }

            return new List<NewsArticle>();
        }

        #endregion

        public async Task<List<string>> GetCategoriesAsync()
        {
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
        private async Task<bool> AddCategoryAsync(AddOrUpdateCategoryDto categoryDto)
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
        private async Task<List<NewsArticle>> GetNewsByOneCategory(string category)
        {
            string language = "en";
            int apiPageSize = 250;
            int apiPageNum = 1;

            var currentDate = DateTime.UtcNow.Date;
            var to = currentDate.ToString("yyyy-MM-dd");
            var from = currentDate.AddDays(-10).ToString("yyyy-MM-dd");
            var countries = string.Join(",", new[] { "US", "EG", "CA", "FR", "GB", "DE" });

            Console.WriteLine("From: " + from);
            Console.WriteLine("To: " + to);
            var query = category;

            var requestUrl = $"{_baseUrl}?q={query}&from_={from}&to_={to}&lang={language}&countries={countries}&page_size={apiPageSize}&page={apiPageNum}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-token", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            try
            {
                var response = await _httpClient.GetStringAsync(requestUrl);
                var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(response);

                if (newsResponse?.Articles != null)
                {
                    var articlesList = newsResponse.Articles.ToList();

                    foreach (var article in articlesList)
                    {
                        if (string.IsNullOrEmpty(article.Author))
                            article.Author = "Unknown author";
                        if (string.IsNullOrEmpty(article.Twitter_Account))
                            article.Twitter_Account = "Unknown account";
                        if (article.Authors.Count == 0)
                            article.Authors.Add("Unknown authors");
                    }
                    _logger.LogInformation($"Returning articles count ==> {articlesList.Count}");
                    return articlesList;
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Request failed: {e.Message}");
            }

            return new List<NewsArticle>();
        }
        public async Task<List<NewsArticle>> GetNewsByCategoryAsync(string category, string language = "en", string country = "us")
        {
            //var newsResponse = await GetAllNewsAsync();
            var newsResponse = await GetNewsByOneCategory(category);
            var filteredNews = newsResponse.Where(article =>
            article.Topic != null &&
            article.Topic.Contains(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
            return filteredNews ?? [];
        }

        //public async Task<NewsArticle> GetNewsByIdAsync(string id)
        //{
        //    //var newsResponse = await GetAllNewsAsync();
        //    var user = await _userService.GetCurrentUserAsync();
        //    var newsResponse = await _recommendationService.GetLatestRecommendationsAsync(user.Id);
        //    var article = newsResponse.FirstOrDefault(a => a.Id == id);
        //    return article!;
        //}
        public async Task<NewsArticle> GetNewsByIdAsync(string id)
        {
            var user = await _userService.GetCurrentUserAsync();
            var newsResponse = await _recommendationService.GetLatestRecommendationsAsync(user.Id);
            var article = newsResponse.FirstOrDefault(a => a.Id == id);
            if (article == null)
            {
                article = await GetNewsFromApiByIdAsync(id);
                if (article == null)
                {
                    throw new Exception($"Article with ID '{id}' not found.");
                }
            }
            return article;
        }
        private async Task<NewsArticle?> GetNewsFromApiByIdAsync(string id)
        {
            string requestUrl = $"{_baseUrl}_by_link?ids={id}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("x-api-token", _apiKey);
            request.Headers.Add("Accept", "application/json");
            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(content);

                if (newsResponse?.Articles == null || !newsResponse.Articles.Any())
                {
                    _logger.LogWarning("No article found for ID: {Id}", id);
                    return null;
                }

                var article = newsResponse.Articles.First();
                article.Author ??= "Unknown author";
                article.Twitter_Account ??= "Unknown account";
                if (article.Authors == null || article.Authors.Count == 0)
                    article.Authors = new List<string> { "Unknown authors" };

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching article from API for ID: {Id}", id);
                return null;
            }
        }

        //private async Task<NewsArticle?> GetNewsFromApiAsync(string id)
        //{
        //    const string language = "en";
        //    const int pageSize = 250;
        //    const int pageNumber = 1;
        //    string[] countries = { "US", "EG", "CA", "FR", "GB", "DE" };

        //    var toDate = DateTime.UtcNow.Date;
        //    var fromDate = toDate.AddDays(-10);

        //    string requestUrl = $"{_baseUrl}?q={id}" +
        //                        $"&from_={fromDate:yyyy-MM-dd}" +
        //                        $"&to_={toDate:yyyy-MM-dd}" +
        //                        $"&lang={language}" +
        //                        $"&countries={string.Join(",", countries)}" +
        //                        $"&page_size={pageSize}&page={pageNumber}";

        //    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        //    request.Headers.Add("x-api-token", _apiKey);
        //    request.Headers.Add("Accept", "application/json");

        //    try
        //    {
        //        var response = await _httpClient.SendAsync(request);
        //        response.EnsureSuccessStatusCode();

        //        var content = await response.Content.ReadAsStringAsync();
        //        var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(content);

        //        if (newsResponse?.Articles == null || !newsResponse.Articles.Any())
        //        {
        //            _logger.LogWarning("No articles found for ID: {Id}", id);
        //            return null;
        //        }

        //        foreach (var article in newsResponse.Articles)
        //        {
        //            article.Author ??= "Unknown author";
        //            article.Twitter_Account ??= "Unknown account";
        //            if (article.Authors == null || article.Authors.Count == 0)
        //                article.Authors = new List<string> { "Unknown authors" };
        //        }

        //        var matchedArticle = newsResponse.Articles.FirstOrDefault(a => a.Id == id);
        //        if (matchedArticle == null)
        //        {
        //            _logger.LogWarning("No exact match found for ID: {Id} in API results.", id);
        //        }

        //        return matchedArticle;
        //    }

        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching article from API for ID: {Id}", id);
        //        return null;
        //    }
        //}

       
        #region Unused methods
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
        #endregion
        public async Task<IEnumerable<NewsArticleDto>> GetArticlesByCategoriesAsync(IEnumerable<CategoryDto> preferredCategories)
        {
            var categoryNames = preferredCategories.Select(c => c.Name).ToList();
            var allArticles = await GetAllNewsAsync(categoryNames);
            var articles = allArticles.ToList()
                .FindAll(a => categoryNames.Contains(a.Topic));
            var resArticles = _mapper.Map<IEnumerable<NewsArticleDto>>(articles);
            return resArticles;
        }

        public byte[] GenerateArticlePdf(NewsArticle article)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(memoryStream))
                {
                    using (var pdfDoc = new PdfDocument(writer))
                    {
                        var document = new Document(pdfDoc);
                        document.SetMargins(40, 40, 40, 40);

                        BuildArticlePdfBody(document, article); 

                        document.Close();
                    }
                }
                return memoryStream.ToArray();
            }
        }

        #region Helper methods 
        private void BuildArticlePdfBody(Document document, NewsArticle article)
        {
            document.Add(new Paragraph(article.Title ?? "Untitled")
                .SetFontSize(24)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.DARK_GRAY));

            document.Add(new LineSeparator(new SolidLine()));
            document.Add(new Paragraph("\n"));

            if (!string.IsNullOrEmpty(article.Media))
            {
                try
                {
                    var imageData = ImageDataFactory.Create(article.Media);
                    var image = new Image(imageData).SetAutoScale(true)
                                                     .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(image);
                }
                catch
                {
                    document.Add(new Paragraph("Image unavailable").SetFontColor(ColorConstants.RED));
                }
            }

            document.Add(new Paragraph("\n"));

            if (article.Authors is IEnumerable<string> authors && authors.Any())
            {
                document.Add(new Paragraph()
                        .Add(new Text("Authors: ").SetBold().SetFontSize(16))
                        .Add(new Text(string.Join(" , ", authors)).SetFontSize(16)));
            }
          
            if (!string.IsNullOrEmpty(article.Topic))
            {
                document.Add(new Paragraph()
                        .Add(new Text("Topic: ").SetBold().SetFontSize(16))
                        .Add(new Text(article.Topic).SetFontSize(16)));
            }
            if (!string.IsNullOrEmpty(article.Country))
            {
                document.Add(new Paragraph()
                        .Add(new Text("Country: ").SetBold().SetFontSize(16))
                        .Add(new Text(article.Country).SetFontSize(16)));
            }
            if (article.Published_Date != null)
            {
                document.Add(new Paragraph()
                        .Add(new Text("Published Date: ").SetBold().SetFontSize(16))
                        .Add(new Text(article.Published_Date.ToString()).SetFontSize(16)));
            }

            document.Add(new LineSeparator(new DottedLine()));

            AddSection(document, "Description", article.Description, 12, true);
            AddSection(document, "Content", article.Content, 12, true);

            document.Add(new LineSeparator(new DashedLine()));

            if (!string.IsNullOrEmpty(article.Link))
            {
                Link link = new Link("Read more", PdfAction.CreateURI(article.Link));
                document.Add(new Paragraph(link.SetFontSize(12).SetUnderline().SetFontColor(ColorConstants.BLUE)));
            }
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph($"Generated by News Aggregator System on {DateTime.Now:yyyy-MM-dd}")
                .SetFontSize(10)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER));

        }

        private void AddSection(Document document, string title, string content, float fontSize, bool italic = false)
        {
            if (!string.IsNullOrEmpty(content))
            {
                string cleanedContent = Regex.Replace(content.Trim(), @"(\n\s*)+", "\n");

                var paragraph = new Paragraph()
                    .Add(new Text($"{title}: ").SetBold())
                    .Add(new Text(cleanedContent))
                    .SetFontSize(fontSize);

                if (italic) paragraph.SetItalic();

                document.Add(paragraph);
            }
        }

        #endregion
    }
}