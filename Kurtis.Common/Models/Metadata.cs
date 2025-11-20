using System;
using System.Collections.Generic;
using System.Text;

namespace Kurtis.Common.Models
{
    public class Metadata
    {
        public int Id { get; set; }
        public string? Tags { get; set; } 
        public string? Colors { get; set; }
        public string? Sizes { get; set; }
        public List<Review>? Reviews { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Brand>? Brands { get; set; }
        public List<Product>? Products { get; set; }
    }
}

