namespace News.Core.Contracts.Interfaces
{
	public interface ISearchService
	{
		 Task<SearchResponse> SearchArticlesAsync(string query, int page = 1);
	}
}
