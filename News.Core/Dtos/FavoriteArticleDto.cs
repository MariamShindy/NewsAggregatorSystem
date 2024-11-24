using News.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Dtos
{
    public class FavoriteArticleDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } 
        public string ArticleId { get; set; } = string.Empty;
        //public Article Article { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
