namespace News.Core.Contracts
{
    public interface INewsService
    {
        public Task<string> GetAllNews(int? page, int? pageSize);
        public Task<string> GetArticleById(string id);
        public Task<bool> CheckArticleExists(string newsId);

    }
}
