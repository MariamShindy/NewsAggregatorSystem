namespace News.Core.Contracts.Interfaces
{
	public interface ISentimentService
	{
		Task<List<ArticleSentiment>> GetNewsBySentimentAsync(string sentiment);
	}
}
