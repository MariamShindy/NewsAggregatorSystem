using News.Core.Entities;

namespace News.Core.Dtos
{
    public class UserPreferencesDto
    {
        public string UserId { get; set; } = string.Empty;
        public ICollection<string> PreferredCategories { get; set; } = new List<string>();
    }
}
