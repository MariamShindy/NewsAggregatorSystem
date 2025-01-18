using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Entities
{
    public class NewsApiResponse
    {
        public string? Status { get; set; }          
        public int TotalResults { get; set; }       
        public List<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
    }
}
