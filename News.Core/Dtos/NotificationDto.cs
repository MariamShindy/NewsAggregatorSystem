using News.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = String.Empty;
        public string Category { get; set; } = String.Empty;
        public string ArticleTitle { get; set; } = string.Empty;
        public string ArticleDescription { get; set; } = string.Empty;
        public string ArticleUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        //public ApplicationUser User { get; set; }
    }
}
