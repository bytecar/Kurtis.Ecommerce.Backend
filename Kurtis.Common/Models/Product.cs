using System;
using System.Collections.Generic;
namespace Kurtis.Common.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Gender { get; set; }
        public string? SizesJson { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public bool Featured { get; set; }
        public bool IsNew { get; set; }
        public string? ImageUrlsJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Inventory>? Inventories { get; set; }
        public ICollection<ProductCollection>? ProductCollections { get; set; }
    }
}
