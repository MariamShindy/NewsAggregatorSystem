using News.Core.Entities;

namespace News.Core.Dtos
{
    public class ArticleDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string? Summary { get; set; } = string.Empty;
        public bool IsTrending { get; set; }
        public string Language { get; set; } = string.Empty;
        public string? Sentiment { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public ICollection<Comment>? Comments { get; set; }
        public Category Category { get; set; }
    }
}
