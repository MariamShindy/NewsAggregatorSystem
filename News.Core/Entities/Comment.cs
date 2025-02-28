using News.Core.Entities.NewsCatcher;

namespace News.Core.Entities
{
	public class Comment
	{
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
	    public DateTime CreatedAt {  get; set; } = DateTime.Now;
		public string ArticleId { get; set; } = string.Empty;
        //public Article Article { get; set; }  
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; }
        public bool ContainsBadWords { get; set; } 
        //public NewsArticle Article { get; set; }

    }
}
