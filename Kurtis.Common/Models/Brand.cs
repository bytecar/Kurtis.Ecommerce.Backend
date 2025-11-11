namespace Kurtis.Common.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Label { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool Active { get; set; } = true;
    }
}
