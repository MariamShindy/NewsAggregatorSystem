namespace News.Core.Contracts
{
    public interface INewsService
    {
        public Task<string> GetAllNews();
        public Task<string> GetArticleById(string id);
        public Task<bool> CheckArticleExists(string newsId);

    }
}
