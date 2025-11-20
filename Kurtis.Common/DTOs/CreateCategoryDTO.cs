namespace Kurtis.Common.Models
{
    public class CreateCategoryDTO
    {
        public int Id { get; set; }
        public string Label { get; set; } = null!;
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}