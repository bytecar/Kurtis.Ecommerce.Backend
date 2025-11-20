using System;

namespace Kurtis.Common.DTOs
{
    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class CreateProductDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public string? Gender { get; set; }
        public string[]? Sizes { get; set; }
        public string[]? ImageUrls { get; set; }
        public bool Featured { get; set; } = false;
        public bool IsNew { get; set; } = false;
    }
}
