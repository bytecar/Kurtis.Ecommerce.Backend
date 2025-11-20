using System;
using System.Collections.Generic;
using System.Text;

namespace Kurtis.Common.DTOs
{
    public class UpdateProductDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public string? Gender { get; set; }
        public string[]? Sizes { get; set; }
        public string[]? ImageUrls { get; set; }
        public bool? Featured { get; set; }
    }
}
