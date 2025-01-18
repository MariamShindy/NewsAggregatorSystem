using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Entities
{
    public class NewsArticle
    {
        public string? Title { get; set; }                 
        public string? Author { get; set; }                
        public DateTime? Published_Date { get; set; }         
        public string? Published_Date_Precision { get; set; }
        public string? Link { get; set; }         
        public string? Clean_Url { get; set; }     
        public string? Excerpt { get; set; }      
        public string? Summary { get; set; }      
        public string? Rights { get; set; }       
        public int Rank { get; set; }            
        public string? Topic { get; set; }        
        public string? Country { get; set; }      
        public string? Language { get; set; }
        public object Authors { get; set; }
        public string? Media { get; set; }        
        public bool Is_Opinion { get; set; }      
        public string? Twitter_Account { get; set; } 
        public string? _Id { get; set; }
    }
}
