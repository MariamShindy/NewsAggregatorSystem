using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Entities
{
    public class Survey
    {
        public int Id { get; set; }
        public string? SourceDiscovery { get; set; } // For "How did you find out about our news website?"
        public string? VisitFrequency { get; set; }  // For "How often do you visit news websites?"
        public string? IsLoadingSpeedSatisfactory { get; set; } // For "Is the website loading speed satisfactory?"
        public string? NavigationEaseRating { get; set; } // For "How easy is it to navigate
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
