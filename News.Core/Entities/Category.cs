﻿namespace News.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        public ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();
    }
}
