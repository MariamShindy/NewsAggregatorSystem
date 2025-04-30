namespace News.Core.Contracts.Interfaces
{
	public interface IRecommendationService
	{
		Task<List<RecommendationResult>> GetRecommendedArticlesAsync(string topic);
	}
}
