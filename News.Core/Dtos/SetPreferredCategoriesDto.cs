namespace News.Core.Dtos
{
    public class SetPreferredCategoriesDto
    {
        public ICollection<string> CategoryNames { get; set; } = new List<string>(); 
    }
}
