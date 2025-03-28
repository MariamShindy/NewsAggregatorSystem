namespace News.Service.Services
{
    public class NewsService(HttpClient _httpClient, IMapper _mapper, 
        IUnitOfWork _unitOfWork,IConfiguration _configuration, 
        ILogger<NewsService> _logger) : INewsService
    {
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

        #region Old code
        //public async Task<Dictionary<string, string>> GetSourceCategoriesAsync()
        //{
        //    _logger.LogInformation($"NewsService --> Fetching source categories from NewsAPI.");

        //    var url = $"{_sourceBaseUrl}?apiKey={_apiKey}";
        //    _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("News", "1.0"));

        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        _logger.LogError($"NewsService --> Failed to fetch source categories from NewsAPI. Status: {response.StatusCode}");
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        throw new Exception($"Error fetching source categories: {errorContent}");
        //    }

        //    var jsonResponse = await response.Content.ReadAsStringAsync();
        //    var sourcesResponse = JsonConvert.DeserializeObject<SourcesResponse>(jsonResponse);

        //    var sourceCategories = sourcesResponse?.Sources.ToDictionary(s => s.Id, s => s.Category) ?? new Dictionary<string, string>();

        //    _logger.LogInformation($"NewsService --> Fetched {sourceCategories.Count} source categories.");

        //    var existingCategories = await _unitOfWork.Repository<Category>().GetAllAsync();

        //    if (!existingCategories.Any())
        //    {
        //        foreach (var category in sourceCategories.Values.Distinct())
        //        {
        //            AddOrUpdateCategoryDto categoryDto = new AddOrUpdateCategoryDto() { Name = category };
        //            await AddCategoryAsync(categoryDto);
        //            _logger.LogInformation($"NewsService --> GetSourceCategoriesAsync --> Add {categoryDto.Name} category to database .");
        //        }
        //    }
        //    return sourceCategories;
        //}

        //public async Task<IEnumerable<ArticleDto>> GetAllCategorizedArticlesAsync(int? page = 1, int? pageSize = 40)
        //{
        //    _logger.LogInformation($"NewsService --> GetAllCategorizedArticles called with page: {page} and pageSize: {pageSize}");
        //    var sourceCategories = await GetSourceCategoriesAsync();
        //    var url = $"{_baseUrl}?sortBy=popularity&q=tesla&apiKey={_apiKey}";
        //    _logger.LogInformation($"NewsService --> Requesting URL: {url}");
        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        _logger.LogError($"NewsService --> GetAllNews failed, Error fetching news");
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        return null!;
        //    }
        //    var jsonResponse = await response.Content.ReadAsStringAsync();
        //    var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);

        //    var articles = newsData?.Articles
        //        .Where(article => !string.IsNullOrEmpty(article.UrlToImage))
        //        .Select(article =>
        //    {
        //        if (article.Source != null && !string.IsNullOrEmpty(article.Source.Id) && sourceCategories.ContainsKey(article.Source.Id))
        //        {
        //            article.Category = sourceCategories[article.Source.Id];
        //            article.Source.Category = sourceCategories[article.Source.Id];
        //        }
        //        else
        //        {
        //            article.Category = "Uncategorized";
        //            article.Source!.Category = "Uncategorized";
        //        }
        //        article.Id = article.Url;
        //        return _mapper.Map<ArticleDto>(article);
        //    });

        //    return articles?.Where(a => a.Category != "Uncategorized") ?? new List<ArticleDto>();
        //} 
        #endregion

        private readonly string _apiKey = _configuration["NewsAPI:ApiKey"]!;
        private readonly string _baseUrl = _configuration["NewsAPI:BaseUrl"]!;
        private readonly string _sourceBaseUrl = _configuration["NewsAPI:SourcesBaseUrl"]!;


        /*
         * Fetches the source categories from the NewsAPI and updates the database
         * with any new categories found.
         * Returns a dictionary mapping source IDs to their categories.
         */
        public async Task<Dictionary<string, string>> GetSourceCategoriesAsync()
        {
            _logger.LogInformation($"NewsService --> Fetching source categories from NewsAPI.");

            var url = BuildSourcesUrl();
            var jsonResponse = await GetHttpResponseAsync(url);

            var sourceCategories = ParseSourceCategories(jsonResponse);
            await AddCategoriesToDatabaseAsync(sourceCategories);

            _logger.LogInformation($"NewsService --> Fetched {sourceCategories.Count} source categories.");
            return sourceCategories;
        }

        /*
         * Constructs the URL for fetching source categories from the NewsAPI.
         * Returns the URL for the sources endpoint with the API key.
         */
        private string BuildSourcesUrl() => $"{_sourceBaseUrl}?apiKey={_apiKey}";

        /*
         * Sends an HTTP GET request to the specified URL and returns the response
         * content as a string.
         * Throws an exception if the response status is not successful.
         */
        private async Task<string> GetHttpResponseAsync(string url)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("News", "1.0"));
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch data from URL: {url}, Status: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error fetching data: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        /*
         * Parses the JSON response from the NewsAPI to extract source categories.
         * Returns a dictionary mapping source IDs to their categories.
         */
        private Dictionary<string, string> ParseSourceCategories(string jsonResponse)
        {
            var sourcesResponse = JsonConvert.DeserializeObject<SourcesResponse>(jsonResponse);
            return sourcesResponse?.Sources.ToDictionary(s => s.Id, s => s.Category) ?? new Dictionary<string, string>();
        }

        /*
         * Adds any new categories from the source categories to the database.
         */
        private async Task AddCategoriesToDatabaseAsync(Dictionary<string, string> sourceCategories)
        {
            var existingCategories = await _unitOfWork.Repository<Category>().GetAllAsync();

            if (!existingCategories.Any())
            {
                foreach (var category in sourceCategories.Values.Distinct())
                {
                    AddOrUpdateCategoryDto categoryDto = new AddOrUpdateCategoryDto { Name = category };
                    await AddCategoryAsync(categoryDto);
                    _logger.LogInformation($"Added {categoryDto.Name} category to database.");
                }
            }
        }

        /*
         * Fetches all categorized articles from the NewsAPI and processes them into
         * ArticleDto objects.
         */
        public async Task<IEnumerable<ArticleDto>> GetAllCategorizedArticlesAsync(int? page = 1, int? pageSize = 40)
        {
            //_logger.LogInformation($"Fetching articles with page: {page}, pageSize: {pageSize}");

            //var sourceCategories = await GetSourceCategoriesAsync();
            //var url = BuildArticlesUrl();
            //var jsonResponse = await GetHttpResponseAsync(url);

            //return ProcessArticles(jsonResponse, sourceCategories);
            _logger.LogInformation($"Fetching articles for all sources with page: {page}, pageSize: {pageSize}");
            var sourceCategories = await GetSourceCategoriesAsync();
            var selectedSources = sourceCategories
                    .GroupBy(sc => sc.Value) 
                    .Select(g => g.First().Key) 
                    .ToList();
            Console.WriteLine();
            Console.WriteLine();

            foreach (var sor in selectedSources)
            {
                Console.WriteLine($"{sor}");
            }
            var allArticles = new List<ArticleDto>();

            foreach (var source in selectedSources)
            {
                var url = $"{_baseUrl}?sources={source}&apiKey={_apiKey}";

                var jsonResponse = await GetHttpResponseAsync(url);
                var articles = ProcessArticles(jsonResponse, sourceCategories);
                allArticles.AddRange(articles);
            }

            _logger.LogInformation($"Fetched {allArticles.Count} articles across all sources.");
            return allArticles;
        }

        /*
         * Constructs the URL for fetching articles from the NewsAPI.
         * Returns the URL for the articles endpoint with query parameters and API key.
         */
        // private string BuildArticlesUrl() => $"{_baseUrl}?sortBy=popularity&q=tesla&apiKey={_apiKey}";
        private string BuildArticlesUrl(string query = null!, int page = 1, int pageSize = 40)
        {
            var url = $"{_baseUrl}?pageSize={pageSize}&page={page}&sortBy=popularity&apiKey={_apiKey}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"&q={query}";
            }
            return url;
        }

        /*
         * Processes the JSON response from the NewsAPI, mapping it to a list of
         * ArticleDto objects.
         * Only includes articles with categories and excludes uncategorized ones.
         */
        private IEnumerable<ArticleDto> ProcessArticles(string jsonResponse, Dictionary<string, string> sourceCategories)
        {
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(jsonResponse);
            var articles = newsData?.Articles
                .Where(article => !string.IsNullOrEmpty(article.UrlToImage))
                .Select(article =>
                {
                    if (!string.IsNullOrEmpty(article.Source?.Id) && sourceCategories.ContainsKey(article.Source.Id))
                    {
                        article.Category = sourceCategories[article.Source.Id];
                        article.Source.Category = article.Category;
                    }
                    else
                    {
                        article.Category = "Uncategorized";
                        article.Source!.Category = "Uncategorized";
                    }
                    article.Id = article.Url;
                    return _mapper.Map<ArticleDto>(article);
                });

            return articles?.Where(a => a.Category != "Uncategorized") ?? new List<ArticleDto>();
        }

        /*
        * Get specific article using Id
        */
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
        /*
        * Check the article existence 
        */
        public async Task<bool> CheckArticleExistsAsync(string newsId)
        {
            _logger.LogInformation($"NewsService --> CheckArticleExists called with newsId : {newsId}");
            var articles = await GetAllCategorizedArticlesAsync(1, 40);
            return articles.Any(a => a.Url.Contains(newsId, StringComparison.OrdinalIgnoreCase));
        }
        /*
        * Get all categories
        */
        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            _logger.LogInformation($"NewsService --> Fetching all categories from NewsAPI.");
            var allCategories =  await GetSourceCategoriesAsync();
            List<string> categories = allCategories.Values.Distinct().ToList(); ;   
            _logger.LogInformation($"NewsService --> Fetched {categories.Count} unique categories.");
            return categories;
        }
        /*
        * Add new category
        */
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
        /*
        * Delete existing category
        */
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            _logger.LogInformation($"NewsService --> DeleteCategoryAsync called with id : {id}");

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return false;

            await _unitOfWork.Repository<Category>().DeleteAsync(category);
            return await _unitOfWork.CompleteAsync() > 0;
        }
        /*
        * Update old category
        */
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
        public async Task<IEnumerable<Article>> GetArticlesByCategoriesAsync(IEnumerable<CategoryDto> preferredCategories)
        {
            var categoryNames = preferredCategories.Select(c => c.Name).ToList();
            var allArticles = await GetAllCategorizedArticlesAsync();
            var articlesDtos = allArticles.ToList()
                .FindAll(a => categoryNames.Contains(a.Category));
            var articles = _mapper.Map<IEnumerable<Article>>(articlesDtos);
            return articles;
        }
        #region Old pdf
        //public byte[] GenerateArticlePdf(ArticleDto article)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        using (var writer = new PdfWriter(memoryStream))
        //        {
        //            using (var pdfDoc = new PdfDocument(writer))
        //            {
        //                var document = new Document(pdfDoc);

        //                document.Add(new Paragraph()
        //                    .Add(new Text(article.Title ?? "Untitled").SetBold().SetFontSize(30))
        //                    .SetTextAlignment(TextAlignment.CENTER));

        //                document.Add(new Paragraph("\n"));

        //                if (!string.IsNullOrEmpty(article.urlToImage))
        //                {
        //                    try
        //                    {
        //                        var imageData = ImageDataFactory.Create(article.urlToImage);
        //                        var image = new Image(imageData).SetWidth(500).SetHeight(500);
        //                        document.Add(image);
        //                        document.Add(new Paragraph("\n"));
        //                    }
        //                    catch
        //                    {
        //                        document.Add(new Paragraph("Media: Unable to render image (invalid URL or network issue)."));
        //                    }
        //                }

        //                if (article.Author != null)
        //                {
        //                    document.Add(new Paragraph()
        //                        .Add(new Text("Authors: ")).SetBold()
        //                        .Add(new Text(article.Author)));
        //                }
        //                if (article.Category != null)
        //                {
        //                    document.Add(new Paragraph()
        //                        .Add(new Text("Topic : ")).SetBold()
        //                        .Add(new Text(article.Category)));
        //                }
        //                if (article.PublishedAt != null)
        //                {
        //                    document.Add(new Paragraph()
        //                        .Add(new Text("Published Date: ")).SetBold()
        //                        .Add(new Text(article.PublishedAt.ToString())));
        //                }
        //                if (!string.IsNullOrEmpty(article.Content))
        //                {
        //                    document.Add(new Paragraph()
        //                        .Add(new Text("Summary: ")).SetBold().SetFontSize(15)
        //                        .Add(new Text(article.Content)));
        //                }

        //                if (!string.IsNullOrEmpty(article.Url))
        //                {
        //                    document.Add(new Paragraph()
        //                        .Add(new Text("Read More: ")).SetBold()
        //                        .Add(new Text(article.Url).SetUnderline()));
        //                    document.Add(new Paragraph("\n"));
        //                }
        //            }
        //        }

        //        return memoryStream.ToArray();
        //    }

        //} 
        #endregion

        public byte[] GenerateArticlePdf(ArticleDto article)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(memoryStream))
                {
                    using (var pdfDoc = new PdfDocument(writer))
                    {
                        var document = new Document(pdfDoc);

                        AddTitle(document, article.Title);  
                        AddImage(document, article.urlToImage); 
                        AddMetadata(document, article); 

                        document.Close();
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private void AddTitle(Document document, string title)
        {
            document.Add(new Paragraph()
                .Add(new Text(title ?? "Untitled").SetBold().SetFontSize(30))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.DARK_GRAY));
            
            document.Add(new LineSeparator(new SolidLine()));
            document.Add(new Paragraph("\n"));
        }

        private void AddImage(Document document, string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    var imageData = ImageDataFactory.Create(imageUrl);
                    var image = new Image(imageData).SetWidth(500).SetHeight(500);
                    document.Add(image);
                    document.Add(new Paragraph("\n"));
                }
                catch
                {
                    document.Add(new Paragraph("Media: Unable to render image (invalid URL or network issue)."));
                }
            }
        }

        private void AddMetadata(Document document, ArticleDto article)
        {
            if (article.Author != null)
            {
                document.Add(new Paragraph()
                    .Add(new Text("Authors: ")).SetBold()
                    .Add(new Text(article.Author)));
            }

            if (article.Category != null)
            {
                document.Add(new Paragraph()
                    .Add(new Text("Topic: ")).SetBold()
                    .Add(new Text(article.Category)));
            }

            if (article.PublishedAt != null)
            {
                document.Add(new Paragraph()
                    .Add(new Text("Published Date: ")).SetBold()
                    .Add(new Text(article.PublishedAt.ToString())));
            }

            if (!string.IsNullOrEmpty(article.Content))
            {
                document.Add(new Paragraph()
                    .Add(new Text("Summary: ")).SetBold().SetFontSize(15)
                    .Add(new Text(article.Content)));
            }

            if (!string.IsNullOrEmpty(article.Url))
            {
                document.Add(new Paragraph()
                    .Add(new Text("Read More: ")).SetBold()
                    .Add(new Text(article.Url).SetUnderline()));
                document.Add(new Paragraph("\n"));
            }
        }

    }
}
