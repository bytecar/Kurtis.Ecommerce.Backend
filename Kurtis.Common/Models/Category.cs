namespace Kurtis.Common.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Label { get; set; } = null!;
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
    }
}
