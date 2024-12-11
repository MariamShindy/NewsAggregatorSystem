namespace News.Core.Entities
{
    public class NewsResponse
    {
        public string Status { get; set; } = string.Empty;
        public int TotalResults { get; set; }
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
