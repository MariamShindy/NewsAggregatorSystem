using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Dtos.NewsCatcher
{
    public class RecommendationResponse
    {
        public List<NewsArticle> Recommendations { get; set; } = new();

    }
}
