using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.Core.Dtos
{
    public class SurveyResponseDto
    {
        public string? SourceDiscovery { get; set; }
        public string? VisitFrequency { get; set; }
        public string? IsLoadingSpeedSatisfactory { get; set; }
        public string? NavigationEaseRating { get; set; }

        public string? ApplicationUserId { get; set; }
        public string? ApplicationUserName { get; set; }
        public string? ApplicationUserEmail { get; set; }
    }
}
