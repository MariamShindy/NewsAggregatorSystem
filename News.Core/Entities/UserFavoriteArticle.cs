namespace News.Core.Entities
{
	public class UserFavoriteArticle
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }
		public string ArticleId { get; set; } 
		//public Article Article { get; set; }
		public DateTime AddedAt { get; set; } 
	}

}
