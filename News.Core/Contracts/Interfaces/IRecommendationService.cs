namespace News.Core.Contracts.Interfaces
{
	public interface IRecommendationService
	{
		Task<List<NewsArticle>> GetRecommendedArticlesAsync(List<string> topics);
	}
}
